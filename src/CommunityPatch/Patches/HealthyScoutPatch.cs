using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.SandBox.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using static CommunityPatch.HarmonyHelpers;

namespace CommunityPatch.Patches {

  internal class HealthyScoutPatch : PatchBase<HealthyScoutPatch> {

    public override bool Applied { get; protected set; }

    private static readonly MethodInfo TargetMethodInfo = typeof(DefaultCharacterStatsModel)
      .GetMethod(nameof(DefaultCharacterStatsModel.MaxHitpoints), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

    private static readonly MethodInfo PatchMethodInfo = typeof(HealthyScoutPatch).GetMethod(nameof(Postfix), BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

    public override IEnumerable<MethodBase> GetMethodsChecked() {
      yield return TargetMethodInfo;
    }

    private PerkObject _perk;

    private static readonly byte[][] Hashes = {
      new byte[] {
        0xb4, 0x8e, 0x91, 0x0e, 0x9f, 0x71, 0xc0, 0xf8,
        0x15, 0xa7, 0x63, 0xb0, 0x0b, 0x56, 0x76, 0xd2,
        0x02, 0xb5, 0xa7, 0x61, 0x3a, 0x52, 0x58, 0x23,
        0x5e, 0xbf, 0x3f, 0xb6, 0xe4, 0x93, 0xee, 0xa1
      },
      new byte[] {
        // e1.1.0
        0xD5, 0x4F, 0x75, 0x22, 0xAC, 0xD2, 0x2E, 0xA7,
        0xA8, 0x4D, 0x56, 0x4D, 0x3F, 0x92, 0x44, 0xFF,
        0x33, 0x01, 0xA6, 0x65, 0xFB, 0x0F, 0x53, 0x13,
        0xDD, 0xEA, 0x5E, 0x56, 0xF8, 0x92, 0xE7, 0x03
      }
    };

    public override void Reset()
      => _perk = PerkObject.FindFirst(x => x.Name.GetID() == "dDKOoD3e");

    public override bool IsApplicable(Game game)
      // ReSharper disable once CompareOfFloatsByEqualityOperator
    {
      if (!(_perk.PrimaryRole == SkillEffect.PerkRole.PartyMember
        && _perk.PrimaryBonus == 0.15f))
        return false;

      var patchInfo = Harmony.GetPatchInfo(TargetMethodInfo);
      if (AlreadyPatchedByOthers(patchInfo))
        return false;

      var hash = TargetMethodInfo.MakeCilSignatureSha256();
      return hash.MatchesAnySha256(Hashes);
    }

    public override void Apply(Game game) {
      // Dear TaleWorlds; Value should probably be publicly exposed, maybe by a method
      // and maybe marked [Obsolete] if you want to avoid your developers doing dirty deeds
      var textObjStrings = TextObject.ConvertToStringList(
        new List<TextObject> {
          _perk.Name,
          _perk.Description
        }
      );

      // most of the properties of skills have private setters, yet Initialize is public
      _perk.Initialize(
        textObjStrings[0],
        textObjStrings[1],
        _perk.Skill,
        (int) _perk.RequiredSkillValue,
        _perk.AlternativePerk,
        SkillEffect.PerkRole.Personal, 8f,
        _perk.SecondaryRole, _perk.SecondaryBonus,
        _perk.IncrementType
      );

      if (Applied) return;

      CommunityPatchSubModule.Harmony.Patch(TargetMethodInfo,
        postfix: new HarmonyMethod(PatchMethodInfo));

      Applied = true;
    }

    // ReSharper disable once InconsistentNaming
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Postfix(ref int __result, CharacterObject character, StatExplainer explanation) {
      var result = __result;

      var explainedNumber = new ExplainedNumber(result, explanation);

      var perk = ActivePatch._perk;

      PerkHelper.AddPerkBonusForCharacter(perk, character, ref explainedNumber);

      __result = MBMath.Round(explainedNumber.ResultNumber);
    }

  }

}