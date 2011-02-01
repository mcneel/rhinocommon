using System;
using Rhino.Geometry;
using System.Runtime.InteropServices;
//
// good marshalling webpage
// http://msdn.microsoft.com/en-us/library/aa288468(VS.71).aspx
//
//

class Import
{
#if BUILDING_MONO
  public const string lib = "__Internal";
  public const string librdk = "__Internal";
#else
  // DO NOT add the ".dll, .dynlib, .so, ..." extension.
  // Each platform should be smart enough to figure out how
  // to append an extension to find the dynamic library
  public const string lib = "rhcommon_c";
  public const string librdk = "rhcommonrdk_c";
#endif
  private Import() { }
}

// 19 Dec. 2010 S. Baer
// Giulio saw a significant performance increase by marking this class with the
// SuppressUnmanagedCodeSecurity attribute. See MSDN for details
[System.Security.SuppressUnmanagedCodeSecurity]
internal partial class UnsafeNativeMethods
{
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetPythonEvaluateCallback(Rhino.Runtime.HostUtils.EvaluateExpressionCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetGetNowProc(Rhino.Runtime.HostUtils.GetNowCallback callback, Rhino.Runtime.HostUtils.GetFormattedTimeCallback formattedTimCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks(Rhino.PlugIns.PlugIn.OnLoadDelegate onloadCallback,
    Rhino.PlugIns.PlugIn.OnShutdownDelegate shutdownCallback,
    Rhino.PlugIns.PlugIn.OnGetPlugInObjectDelegate getpluginobjectCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks2(Rhino.PlugIns.PlugIn.CallWriteDocumentDelegate callwriteCallback,
    Rhino.PlugIns.PlugIn.WriteDocumentDelegate writedocumentCallback,
    Rhino.PlugIns.PlugIn.ReadDocumentDelegate readdocumentCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoPlugIn_SetCallbacks3(Rhino.PlugIns.PlugIn.OnAddPagesToOptionsDelegate addoptionpagesCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileImportPlugIn_SetCallbacks(Rhino.PlugIns.FileImportPlugIn.AddFileType addfiletype,
    Rhino.PlugIns.FileImportPlugIn.ReadFileFunc readfile);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileExportPlugIn_SetCallbacks(Rhino.PlugIns.FileExportPlugIn.AddFileType addfiletype,
    Rhino.PlugIns.FileExportPlugIn.WriteFileFunc writefile);

#if !BUILDING_MONO
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDigitizerPlugIn_SetCallbacks(Rhino.PlugIns.DigitizerPlugIn.EnableDigitizerFunc enablefunc,
    Rhino.PlugIns.DigitizerPlugIn.UnitSystemFunc unitsystemfunc,
    Rhino.PlugIns.DigitizerPlugIn.PointToleranceFunc pointtolfunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern IntPtr CRhinoSkin_New(Rhino.Runtime.Skin.ShowSplashCallback cb);
#endif

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoCommand_SetRunCommandCallback(Rhino.Commands.Command.RunCommandCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern bool ON_SpaceMorph_MorphGeometry(IntPtr pConstGeometry, double tolerance, [MarshalAs(UnmanagedType.U1)]bool quickpreview, [MarshalAs(UnmanagedType.U1)]bool preserveStructure, SpaceMorph.MorphPointCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDisplayConduit_SetCallback(int which, Rhino.Display.DisplayPipeline.ConduitCallback cb, Rhino.Runtime.HostUtils.ReportCallback reportcb);

  //In RhinoApp
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetEscapeKeyCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetInitAppCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCloseAppCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetAppSettingsChangeCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  //In Command
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginCommandCallback(Rhino.Commands.Command.CommandCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndCommandCallback(Rhino.Commands.Command.CommandCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetUndoEventCallback(Rhino.Commands.Command.UndoCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  //In RhinoDoc
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCloseDocumentCallback(Rhino.RhinoDoc.DocumentCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetNewDocumentCallback(Rhino.RhinoDoc.DocumentCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDocPropChangeCallback(Rhino.RhinoDoc.DocumentCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginOpenDocumentCallback(Rhino.RhinoDoc.DocumentIoCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndOpenDocumentCallback(Rhino.RhinoDoc.DocumentIoCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetBeginSaveDocumentCallback(Rhino.RhinoDoc.DocumentIoCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetEndSaveDocumentCallback(Rhino.RhinoDoc.DocumentIoCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetAddObjectCallback(Rhino.RhinoDoc.RhinoObjectCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDeleteObjectCallback(Rhino.RhinoDoc.RhinoObjectCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetReplaceObjectCallback(Rhino.RhinoDoc.RhinoObjectCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetUnDeleteObjectCallback(Rhino.RhinoDoc.RhinoObjectCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetPurgeObjectCallback(Rhino.RhinoDoc.RhinoObjectCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  //In RhinoView
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetCreateViewCallback(Rhino.Display.RhinoView.ViewCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDestroyViewCallback(Rhino.Display.RhinoView.ViewCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetActiveViewCallback(Rhino.Display.RhinoView.ViewCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetRenameViewCallback(Rhino.Display.RhinoView.ViewCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetObject_GetObjects(IntPtr ptr, int min, int max, Rhino.Input.Custom.GetObject.GeometryFilterCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetPoint_GetPoint(IntPtr ptr,
                                                      [MarshalAs(UnmanagedType.U1)]bool onMouseUp,
                                                      Rhino.Input.Custom.GetPoint.MouseCallback mouseCB,
                                                      Rhino.Input.Custom.GetPoint.DrawCallback drawCB);

  //bool ON_Arc_Copy(ON_Arc* pRdnArc, ON_Arc* pRhCmnArc, bool rdn_to_rhc)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_Arc_Copy(IntPtr pRdnArc, ref Arc pRhCmnArc, [MarshalAs(UnmanagedType.U1)]bool rdn_to_rhc);

  //void CRhinoMouseCallback_Enable(bool on, RHMOUSECALLBACK_FUNC mouse_cb)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoMouseCallback_Enable([MarshalAs(UnmanagedType.U1)]bool on, Rhino.UI.MouseCallback.MouseCallbackFromCPP callback_func);

#if USING_RDK
  // RDK Functions
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureEvaluatorCallbacks(Rhino.Render.TextureEvaluator.GetColorCallback callback_func,
    Rhino.Render.TextureEvaluator.OnDeleteThisCallback ondeletethis_callback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureCallback(Rhino.Render.RenderTexture.NewTextureCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentDeleteThisCallback(Rhino.Render.RenderContent.RenderContentDeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetContentStringCallback(Rhino.Render.RenderTexture.GetRenderContentStringCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureEvaluatorCallback(Rhino.Render.RenderTexture.GetNewTextureEvaluatorCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateTextureCallback(Rhino.Render.RenderTexture.GetSimulateTextureCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetAddUISectionsCallback(Rhino.Render.RenderTexture.GetAddUISectionsCallback callback_func);
#endif
}
