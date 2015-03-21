using System.Drawing;
using System.IO;
using Rhino.PlugIns;
#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

#if RDK_CHECKED

namespace Rhino.Render
{
  /// <summary>
  /// Contains the custom user interfaces that may be provided
  /// </summary>
  public enum RenderPanelType
  {
    /// <summary>
    /// A custom control panel added to the render output window.
    /// </summary>
    RenderWindow = UnsafeNativeMethods.RhRdkCustomUiType.RenderWindowCustomDlgInterface,
  }

  class RenderTabData
  {
    public Type PanelType { get; set; }
    public Guid PlugInId { get; set; }
    public RenderPanelType RenderPanelType { get; set; }
    public Icon Icon { get; set; }
    public Dictionary<Guid, IWin32Window> Tabs = new Dictionary<Guid, IWin32Window>();
  }

  public sealed class RenderTabs
  {
    internal RenderTabs() { }
    /// <summary>
    /// Get the instance of a render tab associated with a specific render
    /// session, this is useful when it is necessary to update a control from a
    /// <see cref="RenderPipeline"/>
    /// </summary>
    /// <param name="plugIn">
    /// The plug-in that registered the custom user interface
    /// </param>
    /// <param name="tabType">
    /// The type of tab to return
    /// </param>
    /// <param name="renderSessionId">
    /// The <see cref="RenderPipeline.RenderSessionId"/> of a specific render
    /// session.
    /// </param>
    /// <returns>
    /// Returns the custom tab object if found; otherwise null is returned.
    /// </returns>
    public static object FromRenderSessionId(PlugIn plugIn, Type tabType, Guid renderSessionId)
    {
      if (plugIn == null) throw new ArgumentNullException("plugIn");
      if (tabType == null) throw new ArgumentNullException("tabType");
      var attr = tabType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
      if (attr.Length != 1)
        return null;
      var data = FindExistingTabData(plugIn.Id, tabType.GUID);
      if (data == null)
        return null;
      IWin32Window tab;
      data.Tabs.TryGetValue(renderSessionId, out tab);
      return tab;
    }
    /// <summary>
    /// Get the session Id that created the specified tab object.
    /// </summary>
    /// <param name="tab"></param>
    /// <returns></returns>
    public static Guid SessionIdFromTab(object tab)
    {
      if (tab == null) return Guid.Empty;
      var tab_type = tab.GetType();
      var attr = tab_type.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
      if (attr.Length != 1) return Guid.Empty;
      var id = tab_type.GUID;
      foreach (var item in g_existing_dockbar_tabs)
      {
        if (!item.PanelType.GUID.Equals(id))
          continue;
        foreach (var kvp in item.Tabs)
          if (tab.Equals(kvp.Value)) return kvp.Key;
        return Guid.Empty;
      }
      return Guid.Empty;
    }
    /// <summary>
    /// Register custom render user interface with Rhino.  This should only be
    /// done in <see cref="RenderPlugIn.RegisterRenderPanels"/>.  Panels
    /// registered after <see cref="RenderPlugIn.RegisterRenderPanels"/> is called
    /// will be ignored.
    /// </summary>
    /// <param name="plugin">
    /// The plug-in providing the custom user interface
    /// </param>
    /// <param name="tabType">
    /// The type of object to be created and added to the render container.
    /// </param>
    /// <param name="caption">
    /// The caption for the custom user interface.
    /// </param>
    /// <param name="icon">
    /// </param>
    public void RegisterTab(PlugIn plugin, Type tabType, string caption, Icon icon)
    {
      if (!typeof(IWin32Window).IsAssignableFrom(tabType))
        throw new ArgumentException("tabType must implement IWin32Window interface", "tabType");
      var constructor = tabType.GetConstructor(Type.EmptyTypes);
      if (!tabType.IsPublic || constructor == null)
        throw new ArgumentException("tabType must be a public class and have a parameterless constructor", "tabType");
      var attr = tabType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
      if (attr.Length != 1)
        throw new ArgumentException("tabType must have a GuidAttribute", "tabType");

      if (g_existing_dockbar_tabs == null)
        g_existing_dockbar_tabs = new List<RenderTabData>();
      // make sure the type is not already registered
      for (var i = 0; i < g_existing_dockbar_tabs.Count; i++)
      {
        var pd = g_existing_dockbar_tabs[i];
        if (pd != null && pd.PlugInId == plugin.Id && pd.PanelType == tabType)
          return;
      }

      var panel_data = new RenderTabData() { PlugInId = plugin.Id, PanelType = tabType, Icon = icon };
      g_existing_dockbar_tabs.Add(panel_data);

      var render_panel_type = RenderPanelTypeToRhRdkCustomUiType(panel_data.RenderPanelType);

      g_create_dockbar_tab_callback = OnCreateDockBarTabCallback;
      g_visible_dockbar_tab_callback = OnVisibleDockBarTabCallback;
      g_destroy_dockbar_tab_callback = OnDestroyDockBarTabCallback;

      UnsafeNativeMethods.CRhCmnRdkRenderPlugIn_RegisterCustomDockBarTab(
        render_panel_type,
        caption,
        tabType.GUID,
        plugin.Id,
        icon == null ? IntPtr.Zero : icon.Handle,
        g_create_dockbar_tab_callback,
        g_visible_dockbar_tab_callback,
        g_destroy_dockbar_tab_callback
        );
    }

    UnsafeNativeMethods.RhRdkCustomUiType RenderPanelTypeToRhRdkCustomUiType(RenderPanelType type)
    {
      switch (type)
      {
        case RenderPanelType.RenderWindow:
          return UnsafeNativeMethods.RhRdkCustomUiType.RenderWindowCustomDlgInterface;
      }
      throw new Exception("Unknown RenderPanelTypeToRhRdkCustomUiType");
    }

    private static RenderPanels.CreatePanelCallback g_create_dockbar_tab_callback;
    private static RenderPanels.VisiblePanelCallback g_visible_dockbar_tab_callback;
    private static RenderPanels.DestroyPanelCallback g_destroy_dockbar_tab_callback;
    private static List<RenderTabData> g_existing_dockbar_tabs;

    private static IntPtr OnCreateDockBarTabCallback(Guid pluginId, Guid tabId, Guid sessionId, IntPtr hParent)
    {
      var tab = FindOrCreateTab(pluginId, tabId, sessionId);
      if (tab == null) return IntPtr.Zero;
      var type = tab.GetType();
      var property = type.GetProperty("Handle");
      return (property == null ? IntPtr.Zero : (IntPtr) property.GetValue(tab, new object[0]));
    }

    private static int OnVisibleDockBarTabCallback(Guid pluginId, Guid tabId, Guid sessionId, uint state)
    {
      var data = FindExistingTabData(pluginId, tabId);
      if (data == null)
        return 0;
      IWin32Window window;
      data.Tabs.TryGetValue(sessionId, out window);
      var panel = window as Control;
      if (panel == null)
        return 0;
      panel.Visible = (state != 0);
      return 1;
    }
    private static void OnDestroyDockBarTabCallback(Guid pluginId, Guid tabId, Guid sessionId)
    {
      var data = FindExistingTabData(pluginId, tabId);
      if (data == null)
        return;
      if (data.Tabs.ContainsKey(sessionId))
        data.Tabs.Remove(sessionId);
    }
    private static RenderTabData FindExistingTabData(Guid pluginId, Guid tabId)
    {
      if (g_existing_dockbar_tabs == null) return null;
      for (var i = 0; i < g_existing_dockbar_tabs.Count; i++)
      {
        var pd = g_existing_dockbar_tabs[i];
        if (pd != null && pd.PlugInId == pluginId && pd.PanelType.GUID == tabId)
          return pd;
      }
      return null;
    }

    private static object FindOrCreateTab(Guid pluginId, Guid tabId, Guid sessionId)
    {
      if (sessionId == Guid.Empty) throw new InvalidDataException("sessionId can't be Guid.Empty");
      var data = FindExistingTabData(pluginId, tabId);
      if (data == null)
        return null;
      IWin32Window window;
      data.Tabs.TryGetValue(sessionId, out window);
      if (window != null) return window;
      window = Activator.CreateInstance(data.PanelType) as IWin32Window;
      data.Tabs.Add(sessionId, window);
      return window;
    }
  }

  class RenderPanelData
  {
    public Type PanelType { get; set; }
    public Guid PlugInId { get; set; }
    public RenderPanelType RenderPanelType { get; set; }
    public Dictionary<Guid, IWin32Window> Panels = new Dictionary<Guid, IWin32Window>();
  }

  /// <summary>
  /// This class is used to extend the standard Render user interface
  /// </summary>
  public sealed class RenderPanels
  {
    internal RenderPanels() {}
    /// <summary>
    /// Get the instance of a render panel associated with a specific render
    /// session, this is useful when it is necessary to update a control from a
    /// <see cref="RenderPipeline"/>
    /// </summary>
    /// <param name="plugIn">
    /// The plug-in that registered the custom user interface
    /// </param>
    /// <param name="panelType">
    /// The type of panel to return
    /// </param>
    /// <param name="renderSessionId">
    /// The <see cref="RenderPipeline.RenderSessionId"/> of a specific render
    /// session.
    /// </param>
    /// <returns>
    /// Returns the custom panel object if found; otherwise null is returned.
    /// </returns>
    public static object FromRenderSessionId(PlugIn plugIn, Type panelType, Guid renderSessionId)
    {
      if (plugIn == null) throw new ArgumentNullException("plugIn");
      if (panelType == null) throw new ArgumentNullException("panelType");
      var attr = panelType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
      if (attr.Length != 1)
        return null;
      var data = FindExistingPanelData(plugIn.Id, panelType.GUID);
      if (data == null)
        return null;
      IWin32Window panel;
      data.Panels.TryGetValue(renderSessionId, out panel);
      return panel;
    }
    /// <summary>
    /// Register custom render user interface with Rhino.  This should only be
    /// done in <see cref="RenderPlugIn.RegisterRenderPanels"/>.  Panels
    /// registered after <see cref="RenderPlugIn.RegisterRenderPanels"/> is called
    /// will be ignored.
    /// </summary>
    /// <param name="plugin">
    /// The plug-in providing the custom user interface
    /// </param>
    /// <param name="renderPanelType">
    /// See <see cref="RenderPanelType"/> for supported user interface types.
    /// </param>
    /// <param name="panelType">
    /// The type of object to be created and added to the render container.
    /// </param>
    /// <param name="caption">
    /// The caption for the custom user interface.
    /// </param>
    /// <param name="alwaysShow">
    /// If true the custom user interface will always be visible, if false then
    /// it may be hidden or shown as requested by the user.
    /// </param>
    /// <param name="initialShow">
    /// Initial visibility state of the custom user interface control.
    /// </param>
    public void RegisterPanel(PlugIn plugin, RenderPanelType renderPanelType, Type panelType, string caption, bool alwaysShow, bool initialShow)
    {
      if (!typeof(IWin32Window).IsAssignableFrom(panelType))
        throw new ArgumentException("panelType must implement IWin32Window interface", "panelType");
      var constructor = panelType.GetConstructor(System.Type.EmptyTypes);
      if (!panelType.IsPublic || constructor == null)
        throw new ArgumentException("panelType must be a public class and have a parameterless constructor", "panelType");
      var attr = panelType.GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
      if (attr.Length != 1)
        throw new ArgumentException("panelType must have a GuidAttribute", "panelType");

      if (g_existing_panels == null)
        g_existing_panels = new List<RenderPanelData>();
      // make sure the type is not already registered
      for (var i = 0; i < g_existing_panels.Count; i++)
      {
        var pd = g_existing_panels[i];
        if (pd != null && pd.PlugInId == plugin.Id && pd.PanelType == panelType)
          return;
      }
      var panel_data = new RenderPanelData { PlugInId = plugin.Id, PanelType = panelType, RenderPanelType = renderPanelType };
      g_existing_panels.Add(panel_data);


      g_create_panel_callback = OnCreatePanelCallback;
      g_visible_panel_callback = OnVisiblePanelCallback;
      g_destroy_panel_callback = OnDestroyPanelCallback;

      var render_panel_type = RenderPanelTypeToRhRdkCustomUiType(renderPanelType);

      UnsafeNativeMethods.CRhCmnRdkRenderPlugIn_RegisterCustomPlugInUi(
        render_panel_type,
        caption,
        panelType.GUID, 
        plugin.Id,
        alwaysShow,
        initialShow,
        g_create_panel_callback,
        g_visible_panel_callback,
        g_destroy_panel_callback);
    }

    UnsafeNativeMethods.RhRdkCustomUiType RenderPanelTypeToRhRdkCustomUiType(RenderPanelType type)
    {
      switch (type)
      {
        case RenderPanelType.RenderWindow:
          return UnsafeNativeMethods.RhRdkCustomUiType.RenderWindowCustomDlgInterface;
      }
      throw new Exception("Unknown RenderPanelTypeToRhRdkCustomUiType");
    }

    internal delegate IntPtr CreatePanelCallback(Guid pluginId, Guid tabId, Guid sessionId, IntPtr hParent);
    internal delegate int VisiblePanelCallback(Guid pluginId, Guid tabId, Guid sessionId, uint state);
    internal delegate void DestroyPanelCallback(Guid pluginId, Guid tabId, Guid sessionId);

    private static CreatePanelCallback g_create_panel_callback;
    private static VisiblePanelCallback g_visible_panel_callback;
    private static DestroyPanelCallback g_destroy_panel_callback;
    private static List<RenderPanelData> g_existing_panels;

    private static IntPtr OnCreatePanelCallback(Guid pluginId, Guid tabId, Guid sessionId, IntPtr hParent)
    {
      var panel = FindOrCreatePanel(pluginId, tabId, sessionId);
      return (panel != null ? panel.Handle : IntPtr.Zero);
    }
    private static int OnVisiblePanelCallback(Guid pluginId, Guid tabId, Guid sessionId, uint state)
    {
      var data = FindExistingPanelData(pluginId, tabId);
      if (data == null)
        return 0;
      IWin32Window window;
      data.Panels.TryGetValue(sessionId, out window);
      var panel = window as Control;
      if (panel == null)
        return 0;
      panel.Visible = (state != 0);
      return 1;
    }
    private static void OnDestroyPanelCallback(Guid pluginId, Guid tabId, Guid sessionId)
    {
      var data = FindExistingPanelData(pluginId, tabId);
      if (data == null)
        return;
      if (data.Panels.ContainsKey(sessionId))
        data.Panels.Remove(sessionId);
    }
    private static RenderPanelData FindExistingPanelData(Guid pluginId, Guid tabId)
    {
      if (g_existing_panels == null) return null;
      for (var i = 0; i < g_existing_panels.Count; i++)
      {
        var pd = g_existing_panels[i];
        if (pd != null && pd.PlugInId == pluginId && pd.PanelType.GUID == tabId)
          return pd;
      }
      return null;
    }
    private static IWin32Window FindOrCreatePanel(Guid pluginId, Guid tabId, Guid sessionId)
    {
      var data = FindExistingPanelData(pluginId, tabId);
      if (data == null)
        return null;
      IWin32Window window;
      data.Panels.TryGetValue(sessionId, out window);
      if (window != null) return window;
      window = Activator.CreateInstance(data.PanelType) as IWin32Window;
      data.Panels.Add(sessionId, window);
      return window;
    }
  }

  /// <summary>
  /// Provides facilities to a render plug-in for integrating with the standard
  /// Rhino render window. Also adds helper functions for processing a render
  /// scene. This is the suggested class to use when integrating a renderer with
  /// Rhino and maintaining a "standard" user interface that users will expect.
  /// </summary>
  public abstract class RenderPipeline : IDisposable
  {
    private IntPtr m_pSdkRender;
    private Rhino.PlugIns.PlugIn m_plugin;
    private int m_serial_number;
    private System.Drawing.Size m_size;
    private Rhino.Render.RenderWindow.StandardChannels m_channels;
    private readonly Guid m_session_id = Guid.Empty;



    private static int m_current_serial_number = 1;
    private static readonly Dictionary<int, RenderPipeline> m_all_render_pipelines = new Dictionary<int, RenderPipeline>();

    /// <summary>
    /// Constructs a subclass of this object on the stack in your Rhino plug-in's Render() or RenderWindow() implementation.
    /// </summary>
    /// <param name="doc">A Rhino document.</param>
    /// <param name="mode">A command running mode, such as scripted or interactive.</param>
    /// <param name="plugin">A plug-in.</param>
    /// <param name="sizeRendering">The width and height of the rendering.</param>
    /// <param name="caption">The caption to display in the frame window.</param>
    /// <param name="channels">The color channel or channels.</param>
    /// <param name="reuseRenderWindow">true if the rendering window should be reused; otherwise, a new one will be instanciated.</param>
    /// <param name="clearLastRendering">true if the last rendering should be removed.</param>
    protected RenderPipeline(RhinoDoc doc,
                          Rhino.Commands.RunMode mode,
                          Rhino.PlugIns.PlugIn plugin,
                          System.Drawing.Size sizeRendering,
                          string caption,
                          Rhino.Render.RenderWindow.StandardChannels channels,
                          bool reuseRenderWindow,
                          bool clearLastRendering
                          )
    {
      Debug.Assert(Rhino.PlugIns.RenderPlugIn.RenderCommandContextPointer != IntPtr.Zero);

      m_serial_number = m_current_serial_number++;
      m_all_render_pipelines.Add(m_serial_number, this);
      m_size = sizeRendering;
      m_channels = channels;

      m_pSdkRender = UnsafeNativeMethods.Rdk_SdkRender_New(m_serial_number, Rhino.PlugIns.RenderPlugIn.RenderCommandContextPointer, plugin.NonConstPointer(), caption, reuseRenderWindow, clearLastRendering);
      m_session_id = UnsafeNativeMethods.CRhRdkSdkRender_RenderSessionId(m_pSdkRender);
      m_plugin = plugin;

      UnsafeNativeMethods.Rdk_RenderWindow_Initialize(m_pSdkRender, (int)channels, m_size.Width, m_size.Height);
    }

    internal static RenderPipeline FromSerialNumber(int serial_number)
    {
      RenderPipeline rc;
      m_all_render_pipelines.TryGetValue(serial_number, out rc);
      return rc;
    }

    public enum RenderReturnCode : int
    {
      Ok = 0,
      EmptyScene,
      Cancel,
      NoActiveView,
      OnPreCreateWindow,
      NoFrameWndPointer,
      ErrorCreatingWindow,
      ErrorStartingRender,
      EnterModalLoop,
      ExitModalLoop,
      ExitRhino,
      InternalError
    };

    public Rhino.Commands.Result CommandResult()
    {
      return CommandResultFromReturnCode(m_ReturnCode);
    }

    /// <summary>
    /// Convert RenderReturnCode to 
    /// </summary>
    /// <param name="code"></param>
    /// <returns></returns>
    static Rhino.Commands.Result CommandResultFromReturnCode(RenderReturnCode code)
    {
      if (code == RenderReturnCode.Ok)
        return Commands.Result.Success;
      if (code == RenderReturnCode.Cancel)
        return Commands.Result.Cancel;
      if (code == RenderReturnCode.EmptyScene)
        return Commands.Result.Nothing;
      if (code == RenderReturnCode.ExitRhino)
        return Commands.Result.ExitRhino;
      return Commands.Result.Failure;
    }

    RenderReturnCode m_ReturnCode = RenderReturnCode.EmptyScene;

    /// <summary>
    /// Call this function to render the scene normally. The function returns when rendering is complete (or cancelled).
    /// </summary>
    /// <returns>A code that explains how rendering completed.</returns>
    public RenderReturnCode Render()
    {
      m_ReturnCode = RenderReturnCode.InternalError;
      if (IntPtr.Zero != m_pSdkRender)
      {
        m_ReturnCode = (RenderReturnCode)UnsafeNativeMethods.Rdk_SdkRender_Render(m_pSdkRender, m_size.Height, m_size.Width);
      }
      return m_ReturnCode;
    }
    /// <summary>
    /// Get the Id associated with this render session, this is useful when
    /// looking up Rhino.Render.RenderPanels.
    /// </summary>
    public Guid RenderSessionId
    {
      get
      {
        if (m_render_session_id != Guid.Empty) return m_render_session_id;
        var pointer = ConstPointer();
        m_render_session_id = UnsafeNativeMethods.CRhRdkSdkRender_RenderSessionId(pointer);
        return m_render_session_id;
      }
    }
    private Guid m_render_session_id = Guid.Empty;
    /// <summary>
    /// Call this function to render the scene in a view window. The function returns when rendering is complete (or cancelled).
    /// </summary>
    /// <param name="view">the view that the user selected a rectangle in.</param>
    /// <param name="rect">rectangle that the user selected.</param>
    /// <param name="inWindow">true to render directly into the view window.</param>
    /// <returns>A code that explains how rendering completed.</returns>
    /// //TODO - ViewInfo is wrong here
    public RenderReturnCode RenderWindow(Rhino.Display.RhinoView view, System.Drawing.Rectangle rect, bool inWindow)
    {
      m_ReturnCode = RenderReturnCode.InternalError;
      if (m_pSdkRender != IntPtr.Zero)
      {
        m_ReturnCode = (RenderReturnCode)UnsafeNativeMethods.Rdk_SdkRender_RenderWindow(m_pSdkRender, view.ConstPointer(), rect.Top, rect.Left, rect.Bottom, rect.Right, inWindow);
      }
      return m_ReturnCode;
    }

    /// <summary>
    /// Gets the render size as specified in the ON_3dmRenderSettings. Will automatically return the correct size based on the ActiveView or custom settings.
    /// </summary>
    /// <returns>The render size.</returns>
    public static System.Drawing.Size RenderSize()
    {
      int width = 0; int height = 0;

      UnsafeNativeMethods.Rdk_SdkRender_RenderSize(ref width, ref height);

      System.Drawing.Size size = new System.Drawing.Size(width, height);
      return size;

    }

    public RenderWindow GetRenderWindow()
    {
      //IntPtr pRW = UnsafeNativeMethods.Rdk_SdkRender_GetRenderWindow(ConstPointer());
      // The above call attempts to get the render frame associated with this pipeline
      // then get the render frame associated with the pipeline then get the render
      // window from the frame.  The problem is that the underlying unmanaged object
      // attached to this pipeline gets destroyed after the rendering is completed.
      // The render frame and window exist until the user closes the render frame so
      // the above call will fail when trying to access the render window for post
      // processing or tone operator adjustments after a render is completed. The
      // method bellow will get the render window using the render session Id associated
      // with this render instance and work as long as the render frame is available.
      var pointer = UnsafeNativeMethods.IRhRdkRenderWindow_Find(m_session_id);
      if (pointer == IntPtr.Zero)
        return null;
      var value = new RenderWindow(m_session_id);
      return value;
    }

    /// <summary>
    /// Sets the number of seconds that need to elapse during rendering before the user is asked if the rendered image should be saved.
    /// </summary>
    public int ConfirmationSeconds
    {
      set
      {
        UnsafeNativeMethods.Rdk_SdkRender_SetConfirmationSeconds(ConstPointer(), value);
      }
    }

    #region Abstract methods
    /// <summary>
    /// Called by the framework when it is time to start rendering, the render window will be created at this point and it is safe to start 
    /// </summary>
    /// <returns></returns>
    protected abstract bool OnRenderBegin();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="view"></param>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    protected abstract bool OnRenderWindowBegin(Rhino.Display.RhinoView view, System.Drawing.Rectangle rectangle);

    public Rhino.PlugIns.PlugIn PlugIn
    {
      get
      {
        return m_plugin;
      }
    }

    /// <summary>
    /// Called by the framework when the user closes the render window or clicks
    /// on the stop button in the render window.
    /// </summary>
    /// <param name="e"></param>
    protected abstract void OnRenderEnd(RenderEndEventArgs e);
    /// <summary>
    /// Frequently called during a rendering by the frame work in order to
    /// determine if the rendering should continue.
    /// </summary>
    /// <returns>Returns true if the rendering should continue.</returns>
    protected abstract bool ContinueModal();
    #endregion Abstract methods

    protected virtual bool NeedToProcessGeometryTable()
    {
      return true;
    }
    protected virtual bool NeedToProcessLightTable()
    {
      return true;
    }
    protected virtual bool RenderSceneWithNoMeshes()
    {
      return true;
    }
    protected virtual bool RenderPreCreateWindow()
    {
      return true;
    }
    protected virtual bool RenderEnterModalLoop()
    {
      return true;
    }
    protected virtual bool RenderExitModalLoop()
    {
      return true;
    }

    protected virtual bool IgnoreRhinoObject(Rhino.DocObjects.RhinoObject obj)
    {
      return true;
    }
    protected virtual bool AddRenderMeshToScene(Rhino.DocObjects.RhinoObject obj, Rhino.DocObjects.Material material, Rhino.Geometry.Mesh mesh)
    {
      return true;
    }
    protected virtual bool AddLightToScene(Rhino.DocObjects.LightObject light)
    {
      return true;
    }

    #region virtual function implementation
    enum VirtualFunctions : int
    {
      StartRendering = 0,
      StartRenderingInWindow = 1,
      StopRendering = 2,              
      NeedToProcessGeometryTable = 3, 
      NeedToProcessLightTable = 4,    
      RenderSceneWithNoMeshes = 5,    
      RenderPreCreateWindow = 6,      
      RenderEnterModalLoop = 7,       
      RenderExitModalLoop = 8,        
      RenderContinueModal = 9,        
      IgnoreRhinoObject = 10,          
      AddRenderMeshToScene = 11,
      AddLightToScene = 12,
    }

    internal delegate int ReturnBoolGeneralCallback(int serial_number, int iVirtualFunction, IntPtr pObj, IntPtr pMat, IntPtr pMesh, IntPtr pView, int rectLeft, int rectTop, int rectRight, int rectBottom);
    internal static ReturnBoolGeneralCallback m_ReturnBoolGeneralCallback = OnReturnBoolGeneralCallback;
    static int OnReturnBoolGeneralCallback(int serial_number, int iVirtualFunction, IntPtr pObj, IntPtr pMat, IntPtr pMesh, IntPtr pView, int rectLeft, int rectTop, int rectRight, int rectBottom)
    {
      try
      {
        RenderPipeline pipe = RenderPipeline.FromSerialNumber(serial_number);
        if (pipe != null)
        {
          switch ((VirtualFunctions)iVirtualFunction)
          {
            case VirtualFunctions.StartRendering:
              return pipe.OnRenderBegin() ? 1 : 0;
            case VirtualFunctions.StartRenderingInWindow:
              {
                Rhino.Display.RhinoView view = Rhino.Display.RhinoView.FromIntPtr(pView);
                System.Drawing.Rectangle rect = System.Drawing.Rectangle.FromLTRB(rectLeft, rectTop, rectRight, rectBottom);
                return pipe.OnRenderWindowBegin(view, rect) ? 1 : 0;
              }
            case VirtualFunctions.StopRendering:
              pipe.OnRenderEnd(new RenderEndEventArgs());
              return 1;
            case VirtualFunctions.NeedToProcessGeometryTable:
              return pipe.NeedToProcessGeometryTable() ? 1 : 0;
            case VirtualFunctions.NeedToProcessLightTable:
              return pipe.NeedToProcessLightTable() ? 1 : 0;
            case VirtualFunctions.RenderSceneWithNoMeshes:
              return pipe.RenderSceneWithNoMeshes() ? 1 : 0;
            case VirtualFunctions.RenderPreCreateWindow:
              return pipe.RenderPreCreateWindow() ? 1:0;
            case VirtualFunctions.RenderEnterModalLoop:
              return pipe.RenderEnterModalLoop() ? 1 : 0;
            case VirtualFunctions.RenderExitModalLoop:
              return pipe.RenderExitModalLoop() ? 1 : 0;
            case VirtualFunctions.RenderContinueModal:
              return pipe.ContinueModal() ? 1 : 0;

            case VirtualFunctions.IgnoreRhinoObject:
              {
                if (pObj != IntPtr.Zero)
                {
                  Rhino.DocObjects.RhinoObject obj = Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pObj);
                  if (obj != null)
                    return pipe.IgnoreRhinoObject(obj) ? 1 : 0;
                }
              }
              return 0;
            case VirtualFunctions.AddLightToScene:
              {
                if (pObj != IntPtr.Zero)
                {
                  Rhino.DocObjects.LightObject obj = Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pObj) as Rhino.DocObjects.LightObject;
                  if (obj != null)
                    return pipe.AddLightToScene(obj) ? 1 : 0;
                }
              }
              return 0;
            case VirtualFunctions.AddRenderMeshToScene:
              {
                if (pObj != IntPtr.Zero && pMat != IntPtr.Zero && pMesh != IntPtr.Zero)
                {
                  Rhino.DocObjects.RhinoObject obj = Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pObj);
                  Rhino.DocObjects.Material mat = Rhino.DocObjects.Material.NewTemporaryMaterial(pMat);

                  //Steve....you need to look at this
                  Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh(pMesh, obj);

                  return pipe.AddRenderMeshToScene(obj, mat, mesh) ? 1:0;
                }
              }
              return 0;
          }          
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      Debug.Assert(false);
      return 0;
    }

    #endregion

    #region internals
    internal IntPtr ConstPointer()
    {
      return m_pSdkRender;
    }
    #endregion


    #region IDisposable Members

    ~RenderPipeline()
    {
      Dispose(false);
      GC.SuppressFinalize(this);
    }
    public void Dispose()
    {
      Dispose(true);
    }

    protected virtual void Dispose(bool isDisposing)
    {
      UnsafeNativeMethods.Rdk_SdkRender_Delete(m_pSdkRender);
      m_pSdkRender = IntPtr.Zero;
      m_plugin = null;
      m_all_render_pipelines.Remove(m_serial_number);
      m_serial_number = -1;
    }

    #endregion
  }

  /// <summary>
  /// Contains information about why OnRenderEnd was called
  /// </summary>
  public class RenderEndEventArgs : EventArgs
  {
  }
}

#endif