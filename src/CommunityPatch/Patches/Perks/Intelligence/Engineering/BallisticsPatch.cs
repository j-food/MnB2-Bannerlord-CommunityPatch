using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using HarmonyLib;
using Helpers;
using TaleWorlds.Core.ViewModelCollection;
using static System.Reflection.BindingFlags;
using static CommunityPatch.HarmonyHelpers;

namespace CommunityPatch.Patches.Perks.Intelligence.Engineering {

  public sealed class BallisticsPatch : PerkPatchBase<BallisticsPatch> {

    public override bool Applied { get; protected set; }

    private static readonly MethodInfo TooltipTargetMethodInfo = SiegeTooltipHelper.TargetMethodInfo;

    private static readonly MethodInfo BombardTargetMethodInfo = typeof(SiegeEvent).GetMethod("BombardHitEngine", NonPublic | Instance | DeclaredOnly);

    private static readonly MethodInfo TooltipPatchMethodInfo = typeof(BallisticsPatch).GetMethod(nameof(TooltipPostfix), Public | NonPublic | Static | DeclaredOnly);

    private static readonly MethodInfo BombardPatchMethodInfo = typeof(BallisticsPatch).GetMethod(nameof(BombardPrefix), Public | NonPublic | Static | DeclaredOnly);

    public override IEnumerable<MethodBase> GetMethodsChecked() {
      yield return TooltipTargetMethodInfo;
      yield return BombardTargetMethodInfo;
    }

    public static byte[][] TooltipHashes => SiegeTooltipHelper.TooltipHashes;

    public static readonly byte[][] BombardHashes = {
      new byte[] {
        // e1.1.0.225190
        0x97, 0xF2, 0xEB, 0x6F, 0xD0, 0x02, 0x95, 0x39,
        0x50, 0xEF, 0x10, 0x9B, 0x78, 0x8C, 0xEF, 0xDC,
        0x42, 0x30, 0x5E, 0x08, 0x02, 0xCE, 0x7E, 0x56,
        0x53, 0x60, 0x27, 0xA9, 0x84, 0x1C, 0xC3, 0xF2
      },
      new byte[] {
        // e1.4.0.228531
        0xA4, 0x93, 0x88, 0xD0, 0x7F, 0x04, 0x72, 0x3D,
        0x40, 0xD6, 0xB3, 0x14, 0xED, 0xC7, 0x36, 0xDF,
        0xBF, 0x97, 0x0C, 0x56, 0x7C, 0x2D, 0x23, 0x01,
        0x7E, 0x1A, 0xBC, 0xBD, 0x3F, 0xB5, 0xF5, 0xA5
      }
    };

    public BallisticsPatch() : base("LyVZYGkN") {
    }

    public override bool? IsApplicable(Game game)
      // ReSharper disable once CompareOfFloatsByEqualityOperator
    {
      if (Perk == null) return false;
      if (Perk.PrimaryBonus != 0.3f) return false;
      if (TooltipTargetMethodInfo == null) return false;
      if (BombardTargetMethodInfo == null) return false;

      var tooltipPatchInfo = Harmony.GetPatchInfo(TooltipTargetMethodInfo);
      if (AlreadyPatchedByOthers(tooltipPatchInfo)) return false;

      var bombardPatchInfo = Harmony.GetPatchInfo(BombardTargetMethodInfo);
      if (AlreadyPatchedByOthers(bombardPatchInfo)) return false;

      var tooltipHash = TooltipTargetMethodInfo.MakeCilSignatureSha256();
      var bombardHash = BombardTargetMethodInfo.MakeCilSignatureSha256();
      if (!(tooltipHash.MatchesAnySha256(TooltipHashes) && bombardHash.MatchesAnySha256(BombardHashes)))
        return false;

      return base.IsApplicable(game);
    }

    public override void Apply(Game game) {
      Perk.SetPrimary(SkillEffect.PerkRole.PartyLeader, 30f);
      if (Applied) return;

      CommunityPatchSubModule.Harmony.Patch(TooltipTargetMethodInfo, postfix: new HarmonyMethod(TooltipPatchMethodInfo));
      CommunityPatchSubModule.Harmony.Patch(BombardTargetMethodInfo, new HarmonyMethod(BombardPatchMethodInfo));
      Applied = true;
    }

    // ReSharper disable once InconsistentNaming

    public static void TooltipPostfix(ref List<TooltipProperty> __result, SiegeEvent.SiegeEngineConstructionProgress engineInProgress = null) {
      var siegeEventSide = SiegeTooltipHelper.GetConstructionSiegeEventSide(engineInProgress);
      if (siegeEventSide == null) return;

      if (!IsCatapult(engineInProgress.SiegeEngine)) return;

      CalculateBonusDamageAndRates(engineInProgress.SiegeEngine, siegeEventSide, out var bonusRate, out var bonusDamage);
      SiegeTooltipHelper.AddPerkTooltip(__result, ActivePatch.Perk, bonusRate);
      SiegeTooltipHelper.UpdateRangedDamageToWallsTooltip(__result, bonusDamage);
      SiegeTooltipHelper.UpdateRangedEngineDamageTooltip(__result, bonusDamage);
    }

    // ReSharper disable once InconsistentNaming

    public static void BombardPrefix(ISiegeEventSide siegeEventSide, SiegeEngineType attackerEngineType, SiegeEvent.SiegeEngineConstructionProgress damagedEngine) {
      if (!IsCatapult(attackerEngineType)) return;

      CalculateBonusDamageAndRates(attackerEngineType, siegeEventSide, out _, out var bonusDamage);
      damagedEngine.SetHitpoints(damagedEngine.Hitpoints - bonusDamage);
    }

    private static bool IsCatapult(SiegeEngineType engine)
      => engine == DefaultSiegeEngineTypes.Catapult || engine == DefaultSiegeEngineTypes.FireCatapult;

    private static void CalculateBonusDamageAndRates(
      SiegeEngineType siegeEngine,
      ISiegeEventSide siegeEventSide, out float bonusRateOnly, out float bonusDamageOnly) {
      var perk = ActivePatch.Perk;
      var baseDamage = siegeEngine.Damage;
      var partyMemberDamage = new ExplainedNumber(baseDamage);
      var partyMemberRate = new ExplainedNumber(100f);
      var parties = siegeEventSide.SiegeParties.Where(x => x.MobileParty != null);

      foreach (var party in parties) {
        PerkHelper.AddPerkBonusForParty(perk, party.MobileParty, ref partyMemberRate);
        PerkHelper.AddPerkBonusForParty(perk, party.MobileParty, ref partyMemberDamage);
      }

      bonusRateOnly = partyMemberRate.ResultNumber - 100;
      bonusDamageOnly = partyMemberDamage.ResultNumber - baseDamage;
    }

  }

}