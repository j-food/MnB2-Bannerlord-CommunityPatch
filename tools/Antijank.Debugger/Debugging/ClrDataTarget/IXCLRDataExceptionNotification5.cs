﻿using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;


namespace Antijank.Debugging {

  [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
  [Guid("E77A39EA-3548-44D9-B171-8569ED1A9423")]
  [SuppressUnmanagedCodeSecurity]
  [ComImport]
  
  public interface IXCLRDataExceptionNotification5 : IXCLRDataExceptionNotification4 {

    /*
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnCodeGenerated([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataMethodInstance method);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnCodeDiscarded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataMethodInstance method);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnProcessExecution([In] uint state);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnTaskExecution([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataTask task, [In] uint state);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnModuleLoaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataModule mod);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnModuleUnloaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataModule mod);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnTypeLoaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataTypeInstance typeInst);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnTypeUnloaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataTypeInstance typeInst);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnAppDomainLoaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataAppDomain domain);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnAppDomainUnloaded([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataAppDomain domain);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnException([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataExceptionState exception);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnGcEvent([In] GcEvtArgs GcEvtArgs);
    [MethodImpl(MethodImplOptions.InternalCall)]
    void ExceptionCatcherEnter([MarshalAs(UnmanagedType.Interface)] [In] IXCLRDataMethodInstance catchingMethod, uint catcherNativeOffset);
    */
    [MethodImpl(MethodImplOptions.InternalCall)]
    void OnCodeGenerated2([MarshalAs(UnmanagedType.Interface)] [In]
      IXCLRDataMethodInstance method, [In] ulong nativeCodeLocation);

  }

}