#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;

#if RDK_CHECKED

namespace Rhino.Render
{
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
    private Rhino.Drawing.Size m_size;
    private Rhino.Render.RenderWindow.StandardChannels m_channels;

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
                          Rhino.Drawing.Size sizeRendering,
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
    /// Call this function to render the scene in a view window. The function returns when rendering is complete (or cancelled).
    /// </summary>
    /// <param name="view">the view that the user selected a rectangle in.</param>
    /// <param name="rect">rectangle that the user selected.</param>
    /// <param name="inWindow">true to render directly into the view window.</param>
    /// <returns>A code that explains how rendering completed.</returns>
    /// //TODO - ViewInfo is wrong here
    public RenderReturnCode RenderWindow(Rhino.Display.RhinoView view, Rhino.Drawing.Rectangle rect, bool inWindow)
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
    public static Rhino.Drawing.Size RenderSize()
    {
      int width = 0; int height = 0;

      UnsafeNativeMethods.Rdk_SdkRender_RenderSize(ref width, ref height);

      Rhino.Drawing.Size size = new Rhino.Drawing.Size(width, height);
      return size;

    }

    public RenderWindow GetRenderWindow()
    {
      IntPtr pRW = UnsafeNativeMethods.Rdk_SdkRender_GetRenderWindow(ConstPointer());
      if (pRW != IntPtr.Zero)
      {
        RenderWindow rw = new RenderWindow(this);
        return rw;
      }
      return null;
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
    protected abstract bool OnRenderWindowBegin(Rhino.Display.RhinoView view, Rhino.Drawing.Rectangle rectangle);

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
                Rhino.Drawing.Rectangle rect = Rhino.Drawing.Rectangle.FromLTRB(rectLeft, rectTop, rectRight, rectBottom);
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