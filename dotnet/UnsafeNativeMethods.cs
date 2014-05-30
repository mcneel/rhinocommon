using System;
using Rhino.Geometry;
using System.Runtime.InteropServices;
//
// good marshalling webpage
// http://msdn.microsoft.com/en-us/library/aa288468(VS.71).aspx
//
//


// 19 Dec. 2010 S. Baer
// Giulio saw a significant performance increase by marking this class with the
// SuppressUnmanagedCodeSecurity attribute. See MSDN for details
//[System.Security.SuppressUnmanagedCodeSecurity]
[System.Security.SecurityCritical]
internal partial class UnsafeNativeMethods
{
  [StructLayout(LayoutKind.Sequential)]
  public struct Point
  {
    public int X;
    public int Y;

    public Point(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }
  }

#if OPENNURBS_SDK_ANYCPU
  static UnsafeNativeMethods()
  {
    Init();
  }

  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern IntPtr LoadLibrary(string libname);

  private static IntPtr g_rh3dm_native_handle = IntPtr.Zero;
  public static void Init()
  {
    if (g_rh3dm_native_handle == IntPtr.Zero)
    {
      var assembly_name = System.Reflection.Assembly.GetExecutingAssembly().Location;
      var assembly_path = assembly_name.Substring(0, assembly_name.LastIndexOf('\\'));
      var sub_directory = Environment.Is64BitProcess ? "x64" : "x86";
      var native_dll_name = System.IO.Path.Combine(assembly_path, sub_directory, "rhino3dmio_native");
      Console.WriteLine(string.Format("Rhino3dmIO native: {0}", native_dll_name));
      g_rh3dm_native_handle = LoadLibrary(native_dll_name);
      if (g_rh3dm_native_handle  == IntPtr.Zero)
      {
        int error_code = Marshal.GetLastWin32Error();
        throw new Exception(string.Format("Failed to load library (ErrorCode: {0})", error_code));
      }
    }
  }
#endif
  
  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool GetCursorPos(out Point lpPoint);

  [DllImport("user32.dll")]
  internal extern static bool DestroyIcon(IntPtr handle);

  [DllImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

#if RHINO_SDK
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern bool ON_SpaceMorph_MorphGeometry(IntPtr pConstGeometry, double tolerance, [MarshalAs(UnmanagedType.U1)]bool quickpreview, [MarshalAs(UnmanagedType.U1)]bool preserveStructure, SpaceMorph.MorphPointCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetPythonEvaluateCallback(Rhino.Runtime.HostUtils.EvaluateExpressionCallback callback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetGetNowProc(Rhino.Runtime.HostUtils.GetNowCallback callback, Rhino.Runtime.HostUtils.GetFormattedTimeCallback formattedTimCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetCmnUtilitiesCallbacks(Rhino.PlugIns.PlugIn.GetPlugInSettingsFolderDelegate getPlugInSettingsHook,
                                                           Rhino.PlugIns.PlugIn.GetPlugInRuiFileNameDelegate getPlugInRuiFileNameHook,
                                                           Rhino.PlugIns.PlugIn.ValidateRegisteredPlugInRuiFileNameDelegate validateRegisteredRuiFileName
                                                          );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_SetLicenseManagerCallbacks(Rhino.Runtime.LicenseManager.InitializeCallback initLicenseManagerProc,
                                                             Rhino.Runtime.LicenseManager.EchoCallback echoProc,
                                                             Rhino.Runtime.LicenseManager.ShowValidationUiCallback showLicenseValidationProc,
                                                             Rhino.Runtime.LicenseManager.UuidCallback licenseUuidProc,
                                                             Rhino.Runtime.LicenseManager.GetLicenseCallback getLicense,
                                                             Rhino.Runtime.LicenseManager.GetCustomLicenseCallback getCustomLicense,
                                                             Rhino.Runtime.LicenseManager.AskUserForLicenseCallback askUserForLicense
    );

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  [return: MarshalAs(UnmanagedType.U1)]
  internal static extern bool CRhMainFrame_Invoke(Rhino.RhinoWindow.InvokeAction invoke_proc);

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
  internal static extern void CRhinoPlugIn_SetCallbacks3(Rhino.PlugIns.PlugIn.OnAddPagesToOptionsDelegate addoptionpagesCallback,
                                                         Rhino.PlugIns.PlugIn.OnAddPagesToObjectPropertiesDelegate addobjectpropertiespagesCallback);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileImportPlugIn_SetCallbacks(Rhino.PlugIns.FileImportPlugIn.AddFileType addfiletype,
    Rhino.PlugIns.FileImportPlugIn.ReadFileFunc readfile);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoFileExportPlugIn_SetCallbacks(Rhino.PlugIns.FileExportPlugIn.AddFileType addfiletype,
    Rhino.PlugIns.FileExportPlugIn.WriteFileFunc writefile);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetCallbacks(Rhino.PlugIns.RenderPlugIn.RenderFunc render, Rhino.PlugIns.RenderPlugIn.RenderWindowFunc renderwindow);

#if RDK_CHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoRenderPlugIn_SetRdkCallbacks(Rhino.PlugIns.RenderPlugIn.SupportsFeatureCallback supportsFeatureCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.AbortRenderCallback abortRenderCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.AllowChooseContentCallback allowChooseContentCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreateDefaultContentCallback createDefaultContentCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.OutputTypesCallback outputTypesCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreateTexturePreviewCallback texturePreviewCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.CreatePreviewCallback previewCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.DecalCallback decalCallback,
                                                                 Rhino.PlugIns.RenderPlugIn.RegisterContentIoCallback registerContentIoCallback
                                                                 );
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
  internal static extern void RHC_SetKeyboardCallback(Rhino.RhinoApp.KeyboardHookEvent cb);

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
  internal static extern void CRhinoEventWatcher_SetTextureMappingEventCallback(Rhino.RhinoDoc.TextureMappingEventCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetIdefTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetLightTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetMaterialTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRhinoEventWatcher_SetGroupTableEventCallback(Rhino.RhinoDoc.RhinoTableCallback cb, Rhino.Runtime.HostUtils.ReportCallback report);

  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern uint CRhinoDoc_AddCustomUndoEvent(int doc_id, [MarshalAs(UnmanagedType.LPWStr)]string description,
                                                           Rhino.RhinoDoc.RhinoUndoEventHandlerCallback undo_cb,
                                                           Rhino.RhinoDoc.RhinoDeleteUndoEventHandlerCallback delete_cb);

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
  internal static extern void CRhinoEventWatcher_SetOnIdleCallback(Rhino.RhinoApp.RhCmnEmptyCallback cb);

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
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectSelectionCallback selection_change,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectTransformCallback transform,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectSpaceMorphCallback morph,
                                                        Rhino.DocObjects.RhinoObject.RhinoObjectDeletedCallback deleted);

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

  // Docking Tabs in rh_utilities.cpp
  [DllImport(Import.lib, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RHC_RegisterTabbedDockBar([MarshalAs(UnmanagedType.LPWStr)]string caption, Guid tab_id, Guid plugin_id, IntPtr icon,
    Rhino.UI.Panels.CreatePanelCallback create_proc,
    Rhino.UI.Panels.VisiblePanelCallback visible_proc);

#endif

  #region RDK Functions
#if RDK_CHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureEvaluatorCallbacks(Rhino.Render.TextureEvaluator.GetColorCallback callback_func,
    Rhino.Render.TextureEvaluator.OnDeleteThisCallback ondeletethis_callback);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureCallback(Rhino.Render.RenderTexture.NewRenderContentCallbackEvent callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewMaterialCallback(Rhino.Render.RenderMaterial.NewRenderContentCallbackEvent callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewEnvironmentCallback(Rhino.Render.RenderEnvironment.NewRenderContentCallbackEvent callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetNewTextureEvaluatorCallback(Rhino.Render.RenderTexture.GetNewTextureEvaluatorCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateTextureCallback(Rhino.Render.RenderTexture.SimulateTextureCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureGetVirtualIntCallback(Rhino.Render.RenderTexture.GetVirtualIntCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureSetVirtualIntCallback(Rhino.Render.RenderTexture.SetVirtualIntCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureGetVirtualVector3dCallback(Rhino.Render.RenderTexture.GetVirtual3DVectorCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void RdkSetTextureSetVirtualVector3dCallback(Rhino.Render.RenderTexture.SetVirtual3DVectorCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateMaterialCallback(Rhino.Render.RenderMaterial.SimulateMaterialCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSimulateEnvironmentCallback(Rhino.Render.RenderEnvironment.SimulateEnvironmentCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoDeleteThisCallback(Rhino.Render.RenderContentSerializer.DeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoLoadCallback(Rhino.Render.RenderContentSerializer.LoadCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoSaveCallback(Rhino.Render.RenderContentSerializer.SaveCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentIoStringCallback(Rhino.Render.RenderContentSerializer.GetRenderContentIoStringCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetTextureChildSlotNameCallback(Rhino.Render.RenderMaterial.TextureChildSlotNameCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_DeleteThis(Rhino.Render.CustomRenderMeshProvider.CrmProviderDeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_WillBuild(Rhino.Render.CustomRenderMeshProvider.CrmProviderWillBuildCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_BBox(Rhino.Render.CustomRenderMeshProvider.CrmProviderBBoxCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetCallback_CRMProvider_Build(Rhino.Render.CustomRenderMeshProvider.CrmProviderBuildCallback callback_func);
#endif

#if RDK_UNCHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetDocumentSettingsChangedEventCallback(Rhino.RhinoDoc.RdkDocumentSettingsChangedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);
#endif

#if RDK_CHECKED
  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearingEventCallback(Rhino.Render.RenderContentTable.ContentListClearingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListClearedEventCallback(Rhino.Render.RenderContentTable.ContentListClearedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetContentListLoadedEventCallback(Rhino.Render.RenderContentTable.ContentListLoadedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentDeleteThisCallback(Rhino.Render.RenderContent.RenderContentDeleteThisCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetRenderContentBitFlagsCallback(Rhino.Render.RenderContent.RenderContentBitFlagsCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetContentStringCallback(Rhino.Render.RenderContent.GetRenderContentStringCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetAddUISectionsCallback(Rhino.Render.RenderContent.AddUiSectionsCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetDefaultsFromUserCallback(Rhino.Render.RenderContent.GetDefaultsFromUserCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetIsContentTypeAcceptableAsChildCallback(Rhino.Render.RenderContent.IsContentTypeAcceptableAsChildCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSetParameterCallback(Rhino.Render.RenderContent.SetParameterCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetParameterCallback(Rhino.Render.RenderContent.GetParameterCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetExtraRequirementParameterCallback(Rhino.Render.RenderContent.GetExtraRequirementParameterCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetSetExtraRequirementParameterCallback(Rhino.Render.RenderContent.SetExtraRequirementParameterCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetContentIconCallback(Rhino.Render.RenderContent.SetContentIconCallback callbackFunction);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetHarvestDataCallback(Rhino.Render.RenderContent.HarvestDataCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_SetGetShaderCallback(Rhino.Render.RenderContent.GetShaderCallback callback_func);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void Rdk_ContentUiSectionSetCallbacks(Rhino.Render.UI.UserInterfaceSection.SerialNumberCallback deleteThisCallback,
                                                               Rhino.Render.UI.UserInterfaceSection.SerialNumberCallback displayDataCallback,
                                                               Rhino.Render.UI.UserInterfaceSection.SerialNumberBoolCallback onExpandCallback
                                                              );

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
  internal static extern void CRdkCmnEventWatcher_SetFactoryAddedEventCallback(Rhino.Render.RenderContent.ContentTypeAddedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletingEventCallback(Rhino.Render.RenderContent.ContentTypeDeletingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetFactoryDeletedEventCallback(Rhino.Render.RenderContent.ContentTypeDeletedCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

  [DllImport(Import.librdk, CallingConvention = CallingConvention.Cdecl)]
  internal static extern void CRdkCmnEventWatcher_SetClientPlugInUnloadingEventCallback(Rhino.RhinoApp.ClientPlugInUnloadingCallback cb, Rhino.Runtime.HostUtils.RdkReportCallback report_cb);

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
