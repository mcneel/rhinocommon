using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rhino.Geometry;

#if USING_RDK

namespace Rhino.Render
{
  public abstract class RenderPipeline : IDisposable
  {
    private IntPtr m_pSdkRender;
    private Rhino.PlugIns.PlugIn m_plugin;
    private int m_serial_number;
    private System.Drawing.Size m_size;
    private Rhino.Render.RenderWindow.StandardChannels m_channels;

    private static int m_current_serial_number = 1;
    private static readonly Dictionary<int, RenderPipeline> m_all_render_pipelines = new Dictionary<int, RenderPipeline>();

    /// <summary>
    /// Construct a subclass of this object on the stack in your Rhino plug-in's Render() or RenderWindow() implementation.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="mode"></param>
    /// <param name="plugin"></param>
    /// <param name="sizeRendering"></param>
    /// <param name="caption">The caption to display in the frame window.</param>
    /// <param name="channels"></param>
    /// <param name="reuseRenderWindow"></param>
    /// <param name="clearLastRendering"></param>
    public RenderPipeline(RhinoDoc doc,
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
      m_plugin = plugin;

      UnsafeNativeMethods.Rdk_RenderWindow_Initialize(m_pSdkRender, (int)channels, m_size.Width, m_size.Height);
    }

    internal static RenderPipeline FromSerialNumber(int serial_number)
    {
      RenderPipeline rc = null;
      m_all_render_pipelines.TryGetValue(serial_number, out rc);
      return rc;
    }

    public enum RenderReturnCodes : int
    {
      OK = 0,
      EmptyScene,
      Cancel,
      NoActiveView,
      OnPreCreateWindow,
      NoFrameWndPointer,
      ErrorCreatingWindow,
      ErrorStartingRender,
      EnterModalLoop,
      ExitModalLoop,
      WMQuit,
      InternalError
    };

    /// <summary>
    /// Call this function to render the scene normally. The function returns when rendering is complete (or cancelled).
    /// </summary>
    /// <returns></returns>
    public RenderReturnCodes Render()
    {
      if (IntPtr.Zero != m_pSdkRender)
      {
        return (RenderReturnCodes)UnsafeNativeMethods.Rdk_SdkRender_Render(m_pSdkRender, m_size.Height, m_size.Width);
      }
      return RenderReturnCodes.InternalError;
    }

    /// <summary>
    /// Call this function to render the scene in a view window. The function returns when rendering is complete (or cancelled).
    /// </summary>
    /// <param name="view">the view that the user selected a rectangle in.</param>
    /// <param name="rect">rectangle that the user selected.</param>
    /// <param name="inWindow">true to render directly into the view window. */</param>
    /// <returns></returns>
    /// //TODO - ViewInfo is wrong here
    public RenderReturnCodes RenderWindow(Rhino.Display.RhinoView view, System.Drawing.Rectangle rect, bool inWindow)
    {
      if (m_pSdkRender != IntPtr.Zero)
      {
        return (RenderReturnCodes)UnsafeNativeMethods.Rdk_SdkRender_RenderWindow(m_pSdkRender, view.ConstPointer(), rect.Top, rect.Left, rect.Bottom, rect.Right, inWindow);
      }
      return RenderReturnCodes.InternalError;
    }

    /// <summary>
    /// Get the render size as specified in the ON_3dmRenderSettings. Will automatically return the correct size based on the ActiveView or custom settings.
    /// </summary>
    /// <returns></returns>
    public static System.Drawing.Size RenderSize()
    {
      int width = 0; int height = 0;

      UnsafeNativeMethods.Rdk_SdkRender_RenderSize(ref width, ref height);

      System.Drawing.Size size = new System.Drawing.Size(width, height);
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
    /// Set the number of seconds that need to elapse during rendering before the user is asked if the rendered image should be saved.
    /// </summary>
    public int ConfirmationSeconds
    {
      set
      {
        UnsafeNativeMethods.Rdk_SdkRender_SetConfirmationSeconds(ConstPointer(), value);
      }
    }

    /// <summary>
    /// A new render mesh iterator. The caller shall delete the iterator. Any meshes created by the iterator persist in memory for the lifetime of the iterator.
    /// </summary>
    /// <param name="forceTriMesh"></param>
    /// <param name="vp">The rendering view camera</param>
    /// <returns></returns>
    /// //TODO - ON_Viewport
    public RenderMeshIterator NewRenderMeshIterator(Rhino.DocObjects.ViewportInfo vp, bool forceTriMesh)
    {
      IntPtr pIterator = UnsafeNativeMethods.Rdk_SdkRender_NewRenderMeshIterator(ConstPointer(), vp.ConstPointer(), forceTriMesh);
      if (pIterator != IntPtr.Zero)
      {
        return new RenderMeshIterator(pIterator);
      }
      return null;
    }

    /// <summary>
    /// Implement this method to start your rendering thread.
    /// </summary>
    protected abstract bool StartRendering();
    protected abstract bool StartRenderingInWindow(Rhino.Display.RhinoView view, System.Drawing.Rectangle rectangle);

    public Rhino.PlugIns.PlugIn PlugIn
    {
      get
      {
        return m_plugin;
      }
    }

    protected virtual void StopRendering()
    {
    }
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
    protected virtual bool RenderContinueModal()
    {
      return false;
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

    internal delegate int ReturnBoolGeneralCallback(int serial_number, int iVirtualFunction, IntPtr pObj, IntPtr pMat, IntPtr pMesh);
    internal static ReturnBoolGeneralCallback m_ReturnBoolGeneralCallback = OnReturnBoolGeneralCallback;
    static int OnReturnBoolGeneralCallback(int serial_number, int iVirtualFunction, IntPtr pObj, IntPtr pMat, IntPtr pMesh)
    {
      try
      {
        RenderPipeline pipe = RenderPipeline.FromSerialNumber(serial_number);
        if (pipe != null)
        {
          switch ((VirtualFunctions)iVirtualFunction)
          {
            case VirtualFunctions.StartRendering:
              return pipe.StartRendering() ? 1 : 0;
            case VirtualFunctions.StopRendering:
              pipe.StopRendering();
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
              return pipe.RenderContinueModal() ? 1 : 0;

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
                  Rhino.DocObjects.LightObject obj = Rhino.DocObjects.LightObject.CreateRhinoObjectHelper(pObj) as Rhino.DocObjects.LightObject;
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
}

#endif