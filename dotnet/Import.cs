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
#if MONO_BUILD
  public const string lib = "__Internal";
  public const string librdk = "__Internal";
#elif USING_OPENNURBS
  public const string lib = "rhcommon_opennurbs";
  public const string librdk = "rhcommon_opennurbs";
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
  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern bool ON_SpaceMorph_MorphGeometry(IntPtr pConstGeometry, double tolerance, [MarshalAs(UnmanagedType.U1)]bool quickpreview, [MarshalAs(UnmanagedType.U1)]bool preserveStructure, SpaceMorph.MorphPointCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetPythonEvaluateCallback(Rhino.Runtime.HostUtils.EvaluateExpressionCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetGetNowProc(Rhino.Runtime.HostUtils.GetNowCallback callback, Rhino.Runtime.HostUtils.GetFormattedTimeCallback formattedTimCallback);

#if RHINO_SDK
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoVisualAnalysisMode_SetCallbacks(Rhino.Display.VisualAnalysisMode.ANALYSISMODEENABLEUIPROC enableui_proc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODEOBJECTSUPPORTSPROC objectSupportProc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODESHOWISOCURVESPROC showIsoCurvesProc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODESETDISPLAYATTRIBUTESPROC displayAttributesProc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODEUPDATEVERTEXCOLORSPROC updateVertexColorsProc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODEDRAWRHINOOBJECTPROC drawRhinoObjectProc,
    Rhino.Display.VisualAnalysisMode.ANALYSISMODEDRAWGEOMETRYPROC drawGeometryProc);

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

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetCallbacks(Rhino.PlugIns.RenderPlugIn.RenderFunc render, Rhino.PlugIns.RenderPlugIn.RenderWindowFunc renderwindow);

#if RDK_UNCHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetRdkCallbacks(Rhino.PlugIns.RenderPlugIn.SupportsFeatureCallback supportsFeatureCallback, 
                                                                 Rhino.PlugIns.RenderPlugIn.AbortRenderCallback abortRenderCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.AllowChooseContentCallback allowChooseContentCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreateDefaultContentCallback createDefaultContentCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.OutputTypesCallback outputTypesCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreateTexturePreviewCallback texturePreviewCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreatePreviewCallback previewCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.DecalCallback decalCallback);
#endif
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDigitizerPlugIn_SetCallbacks(Rhino.PlugIns.DigitizerPlugIn.EnableDigitizerFunc enablefunc,
    Rhino.PlugIns.DigitizerPlugIn.UnitSystemFunc unitsystemfunc,
    Rhino.PlugIns.DigitizerPlugIn.PointToleranceFunc pointtolfunc);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern IntPtr CRhinoSkin_New(Rhino.Runtime.Skin.ShowSplashCallback cb, [MarshalAs(UnmanagedType.LPWStr)]string name, IntPtr hicon);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoCommand_SetRunCommandCallbacks(Rhino.Commands.Command.RunCommandCallback cb,
                                                                   Rhino.Commands.Command.DoHelpCallback dohelp_cb,
                                                                   Rhino.Commands.Command.ContextHelpCallback contexthelp_cb,
                                                                   Rhino.Commands.Command.ReplayHistoryCallback replayhistory_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoCommand_SetSelCommandCallback(Rhino.Commands.SelCommand.SelFilterCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoDisplayConduit_SetCallback(int which, Rhino.Display.DisplayPipeline.ConduitCallback cb, Rhino.Runtime.HostUtils.ReportCallback reportcb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetReplaceColorDialogCallback(Rhino.UI.Dialogs.ColorDialogCallback cb);

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

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetSelectObjectCallback(Rhino.RhinoDoc.RhinoObjectSelectionCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);
  
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetDeselectAllObjectsCallback(Rhino.RhinoDoc.RhinoDeselectAllObjectsCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetModifyObjectAttributesCallback(Rhino.RhinoDoc.RhinoModifyObjectAttributesCallback cb, Rhino.Runtime.HostUtils.ReportCallback report_cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetLayerTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetMaterialTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetGroupTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

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
  internal static extern void CRhinoEventWatcher_SetDetailEventCallback(Rhino.Display.RhinoPageView.PageViewCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetObject_GetObjects(IntPtr ptr, int min, int max, Rhino.Input.Custom.GetObject.GeometryFilterCallback cb);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetPoint_GetPoint(IntPtr ptr,
                                                      [MarshalAs(UnmanagedType.U1)]bool onMouseUp,
                                                      Rhino.Input.Custom.GetPoint.MouseCallback mouseCB,
                                                      Rhino.Input.Custom.GetPoint.DrawCallback drawCB,
                                                      Rhino.Display.DisplayPipeline.ConduitCallback postDrawCB,
                                                      Rhino.Input.Custom.GetTransform.CalculateXformCallack calcXformCB);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoGetXform_GetXform(IntPtr ptr, Rhino.Input.Custom.GetPoint.MouseCallback mouseCB,
                                                      Rhino.Input.Custom.GetPoint.DrawCallback drawCB,
                                                      Rhino.Display.DisplayPipeline.ConduitCallback postDrawCB,
                                                      Rhino.Input.Custom.GetTransform.CalculateXformCallack calcXformCB);

  //In RhinoObject
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObject_SetCallbacks(Rhino.DocObjects.RhinoObject.RhinoObjectDuplicateCallback duplicate,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectDrawCallback draw,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectDocNotifyCallback doc_notify,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectActiveInViewportCallback active_in_viewport,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectSelectionCallback selection_change);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObject_SetPickCallbacks(Rhino.DocObjects.RhinoObject.RhinoObjectPickCallback pick,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectPickedCallback picked_callback);

  //void CRhinoMouseCallback_Enable(bool on, RHMOUSECALLBACK_FUNC mouse_cb)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoMouseCallback_Enable([MarshalAs(UnmanagedType.U1)]bool on, Rhino.UI.MouseCallback.MouseCallbackFromCPP callback_func);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoApp_RegisterGripsEnabler(Guid key, Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoGripsEnablerCallback turnon_func);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoObjectGrips_SetCallbacks(Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsResetCallback reset_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsResetCallback resetmesh_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsUpdateMeshCallback updatemesh_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNewGeometryCallback newgeom_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsDrawCallback draw_dunc,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNeighborGripCallback neighborgrip_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNurbsSurfaceGripCallback nurbssurfacegrip_func,
    Rhino.DocObjects.Custom.CustomObjectGrips.CRhinoObjectGripsNurbsSurfaceCallback nurbssurface_func);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnGripObject_SetCallbacks(Rhino.DocObjects.Custom.CustomGripObject.CRhinoObjectDestructorCallback destructor_func,
    Rhino.DocObjects.Custom.CustomGripObject.CRhinoGripObjectWeightCallback getweight_func,
    Rhino.DocObjects.Custom.CustomGripObject.CRhinoGripObjectSetWeightCallback setweight_func);
#endif

  #region RDK Functions
#if RDK_UNCHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureEvaluatorCallbacks(Rhino.Render.TextureEvaluator.GetColorCallback callback_func,
    Rhino.Render.TextureEvaluator.OnDeleteThisCallback ondeletethis_callback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureCallback(Rhino.Render.RenderTexture.NewTextureCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewMaterialCallback(Rhino.Render.RenderMaterial.NewMaterialCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewEnvironmentCallback(Rhino.Render.RenderEnvironment.NewEnvironmentCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentDeleteThisCallback(Rhino.Render.RenderContent.RenderContentDeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetContentStringCallback(Rhino.Render.RenderContent.GetRenderContentStringCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureEvaluatorCallback(Rhino.Render.RenderTexture.GetNewTextureEvaluatorCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateTextureCallback(Rhino.Render.RenderTexture.SimulateTextureCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetAddUISectionsCallback(Rhino.Render.RenderContent.AddUISectionsCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetIsContentTypeAcceptableAsChildCallback(Rhino.Render.RenderContent.IsContentTypeAcceptableAsChildCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetHarvestDataCallback(Rhino.Render.RenderContent.HarvestDataCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetShaderCallback(Rhino.Render.RenderContent.GetShaderCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateMaterialCallback(Rhino.Render.RenderMaterial.SimulateMaterialCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateEnvironmentCallback(Rhino.Render.RenderEnvironment.SimulateEnvironmentCallback callback_func);


  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureChildSlotNameCallback(Rhino.Render.RenderMaterial.TextureChildSlotNameCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_DeleteThis(Rhino.Render.CustomRenderMesh.Provider.CRMProviderDeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_WillBuild(Rhino.Render.CustomRenderMesh.Provider.CRMProviderWillBuildCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_BBox(Rhino.Render.CustomRenderMesh.Provider.CRMProviderBBoxCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_Build(Rhino.Render.CustomRenderMesh.Provider.CRMProviderBuildCallback callback_func);


  //Events
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentAddedEventCallback(Rhino.Render.RenderContent.ContentAddedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentRenamedEventCallback(Rhino.Render.RenderContent.ContentRenamedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);
  
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentDeletingEventCallback(Rhino.Render.RenderContent.ContentDeletingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentReplacingEventCallback(Rhino.Render.RenderContent.ContentReplacingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentReplacedEventCallback(Rhino.Render.RenderContent.ContentReplacedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentChangedEventCallback(Rhino.Render.RenderContent.ContentChangedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentUpdatePreviewEventCallback(Rhino.Render.RenderContent.ContentUpdatePreviewCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentCurrencyChangedEventCallback(Rhino.Render.RenderContent.CurrentContentChangedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);



  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearingEventCallback(Rhino.Render.ContentList.ContentListClearingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearedEventCallback(Rhino.Render.ContentList.ContentListClearedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListLoadedEventCallback(Rhino.Render.ContentList.ContentListLoadedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);


  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetNewRdkDocumentEventCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetGlobalSettingsChangedEventCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetUpdateAllPreviewsEventCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetCacheImageChangedEventCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetRendererChangedEventCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetCustomRenderMeshesChangedEventCallback(Rhino.Render.CustomRenderMesh.Manager.CRMManagerEmptyCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);



  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryAddedEventCallback(Rhino.Render.RenderContent.ContentTypeAddedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletingEventCallback(Rhino.Render.RenderContent.ContentTypeDeletingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletedEventCallback(Rhino.Render.RenderContent.ContentTypeDeletedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);



  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetClientPlugInUnloadingEventCallback(Rhino.RhinoApp.ClientPlugInUnloadingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);


  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(Rhino.RhinoDoc.RdkDocumentSettingsChangedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

#endif
#if RDK_CHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSdkRenderCallback(Rhino.Render.RenderPipeline.ReturnBoolGeneralCallback callback_func);
#endif
  #endregion

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhCmnUserData_SetCallbacks(Rhino.DocObjects.Custom.UserData.TransformUserDataCallback xform_func,
    Rhino.DocObjects.Custom.UserData.ArchiveUserDataCallback archive_func,
    Rhino.DocObjects.Custom.UserData.ReadWriteUserDataCallback readwrite_func,
    Rhino.DocObjects.Custom.UserData.DuplicateUserDataCallback duplicate_func,
    Rhino.DocObjects.Custom.UserData.CreateUserDataCallback create_func,
    Rhino.DocObjects.Custom.UserData.DeleteUserDataCallback delete_func);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_Search(IntPtr pConstRtree, Point3d pt0, Point3d pt1, int serial_number, RTree.SearchCallback searchCB);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_SearchSphere(IntPtr pConstRtree, Point3d center, double radius, int serial_number, RTree.SearchCallback searchCB);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_RTree_Search2(IntPtr pConstRtreeA, IntPtr pConstRtreeB, double tolerance, int serial_number, RTree.SearchCallback searchCB);

  //bool ON_Arc_Copy(ON_Arc* pRdnArc, ON_Arc* pRhCmnArc, bool rdn_to_rhc)
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool ON_Arc_Copy(IntPtr pRdnArc, ref Arc pRhCmnArc, [MarshalAs(UnmanagedType.U1)]bool rdn_to_rhc);
}
