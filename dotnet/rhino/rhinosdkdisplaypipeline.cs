#pragma warning disable 1591
using System;
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.Display
{
  public enum DepthMode : int
  {
    Neutral = 0,
    AlwaysInFront = 1,
    AlwaysInBack = 2
  }

  public enum ZBiasMode : int
  {
    Neutral = 0,
    TowardsCamera = 1,
    AwayFromCamera = 2
  }

  public enum CullFaceMode : int
  {
    DrawFrontAndBack = 0,
    DrawFrontFaces = 1,
    DrawBackFaces = 2
  }

  public enum PointStyle : int
  {
    Simple = 0,
    ControlPoint = 1,
    /// <summary>
    /// Like a control point but includes vertical/horizontal crosshair lines.
    /// </summary>
    ActivePoint = 2,
    X = 3,
  }

  /// <summary>
  /// <para>
  /// The display pipeline calls events during specific phases of drawing
  /// During the drawing of a single frame the events are called in the following order.
  /// </para>
  /// [Begin Drawing of a Frame]
  /// <list type="bullet">
  /// <item><description>CalculateBoundingBox</description></item>
  /// <item><description>CalculateClippingPanes</description></item>
  /// <item><description>SetupFrustum</description></item>
  /// <item><description>SetupLighting</description></item>
  /// <item><description>InitializeFrameBuffer</description></item>
  /// <item><description>DrawBackground</description></item>
  /// <item><description>If this is a layout and detail objects exist the channels are called in the
  ///   same order for each detail object (drawn as a nested viewport)</description></item>
  /// <item><description>PreDrawObjects</description></item>
  ///
  /// <item><description>For Each Visible Non Highlighted Object</description>
  /// <list type="bullet">
  /// <item><description>SetupObjectDisplayAttributes</description></item>
  /// <item><description>PreDrawObject</description></item>
  /// <item><description>DrawObject</description></item>
  /// <item><description>PostDrawObject</description></item>
  /// </list></item>
  /// <item><description>PostDrawObjects - depth writing/testing on</description></item>
  /// <item><description>DrawForeGround - depth writing/testing off</description></item>
  ///
  /// <item><description>For Each Visible Highlighted Object</description>
  /// <list type="bullet">
  /// <item><description>SetupObjectDisplayAttributes</description></item>
  /// <item><description>PreDrawObject</description></item>
  /// <item><description>DrawObject</description></item>
  /// <item><description>PostDrawObject</description></item>
  /// </list></item>
  ///
  /// <item><description>PostProcessFrameBuffer (If a delegate exists that requires this)</description></item>
  /// <item><description>DrawOverlay (if Rhino is in a feedback mode)</description></item>
  /// </list>
  /// [End of Drawing of a Frame]
  ///
  /// <para>NOTE: There may be multiple DrawObject calls for a single object. An example of when this could
  ///       happen would be with a shaded sphere. The shaded mesh is first drawn and these channels would
  ///       be processed; then at a later time the isocurves for the sphere would be drawn.</para>
  /// </summary>
  public sealed class DisplayPipeline
  {
    #region members
    private readonly IntPtr m_ptr;
    internal IntPtr NonConstPointer() { return m_ptr; }
    #endregion

    #region constants
    const int idxIsPrinting = 0;
    const int idxInStereoMode = 1;
    const int idxInterruptDrawing = 2;
    const int idxViewInDynamicDisplay = 3;
    const int idxDrawingWires = 4;
    const int idxDrawingGrips = 5;
    const int idxDrawingSurfaces = 6;
    const int idxShadingRequired = 7;
    const int idxSupportsShading = 8;
    const int idxModelTransformIsIdentity = 9;

    // get/set integers
    const int idxActiveStereoProjection = 0;
    const int idxRenderPass = 1;
    const int idxNestLevel = 2;
    const int idxDepthMode = 3;
    const int idxZBiasMode = 4;


    const int idxModelTransform = 0;
    const int idxDepthTesting = 1;
    const int idxDepthWriting = 2;
    const int idxColorWriting = 3;
    const int idxLighting = 4;
    const int idxClippingPlanes = 5;
    const int idxClipTesting = 6;
    const int idxCullFaceMode = 7;
    #endregion

    #region constructors
    // users should not be able to create instances of this class
    internal DisplayPipeline(IntPtr pPipeline)
    {
      m_ptr = pPipeline;
    }
    #endregion

    #region conduit events
    const int idxCalcBoundingBox = 0;
    const int idxPreDrawObjects = 1;
    const int idxPostDrawObjects = 2;
    const int idxDrawForeground = 3;
    const int idxDrawOverlay = 4;
    const int idxCalcBoundingBoxZoomExtents = 5;
    const int idxDrawObject = 6;
    const int idxObjectCulling = 7;

    static void ConduitReport(int which)
    {
      string title = null;
      Delegate cb = null;
      switch (which)
      {
        case idxCalcBoundingBox:
          title = "CalcBBox";
          cb = m_calcbbox;
          break;
        case idxPreDrawObjects:
          title = "PreDrawObjects";
          cb = m_predrawobjects;
          break;
        case idxPostDrawObjects:
          title = "PostDrawObjects";
          cb = m_postdrawobjects;
          break;
        case idxDrawForeground:
          title = "DrawForeground";
          cb = m_drawforeground;
          break;
        case idxDrawObject:
          title = "DrawObject";
          cb = m_drawobject;
          break;
        case idxObjectCulling:
          title = "ObjectCulling";
          cb = m_objectCulling;
          break;
      }
      if (!string.IsNullOrEmpty(title) && cb != null)
      {
        UnsafeNativeMethods.CRhinoDisplayConduit_LogState(title);
        Delegate[] list = cb.GetInvocationList();
        if (list != null && list.Length > 0)
        {
          for (int i = 0; i < list.Length; i++)
          {
            Delegate subD = list[i];
            Type t = subD.Target.GetType();
            string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, "- Plug-In = {0}\n", t.Assembly.GetName().Name);
            UnsafeNativeMethods.CRhinoDisplayConduit_LogState(msg);
          }
        }
      }
    }

    private static readonly Runtime.HostUtils.ReportCallback m_report = ConduitReport;

    // Callback used by C++ conduit to call into .NET
    internal delegate void ConduitCallback(IntPtr pPipeline, IntPtr pConduit);
    private static ConduitCallback m_ObjectCullingCallback;
    private static ConduitCallback m_CalcBoundingBoxCallback;
    private static ConduitCallback m_CalcBoundingBoxZoomExtentsCallback;
    private static ConduitCallback m_PreDrawObjectsCallback;
    private static ConduitCallback m_DrawObjectCallback;
    private static ConduitCallback m_PostDrawObjectsCallback;
    private static ConduitCallback m_DrawForegroundCallback;
    private static ConduitCallback m_DrawOverlayCallback;

    private static EventHandler<CullObjectEventArgs> m_objectCulling;
    private static EventHandler<CalculateBoundingBoxEventArgs> m_calcbbox;
    private static EventHandler<CalculateBoundingBoxEventArgs> m_calcbbox_zoomextents;
    private static EventHandler<DrawEventArgs> m_predrawobjects;
    private static EventHandler<DrawObjectEventArgs> m_drawobject;
    private static EventHandler<DrawEventArgs> m_postdrawobjects;
    private static EventHandler<DrawEventArgs> m_drawforeground;
    private static EventHandler<DrawEventArgs> m_drawoverlay;

    private static void OnObjectCulling(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_objectCulling != null)
      {
        try
        {
          m_objectCulling(null, new CullObjectEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    private static void OnCalcBoundingBox(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_calcbbox != null)
      {
        try
        {
          m_calcbbox(null, new CalculateBoundingBoxEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnCalcBoundingBoxZoomExtents(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_calcbbox_zoomextents != null)
      {
        try
        {
          m_calcbbox_zoomextents(null, new CalculateBoundingBoxEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    private static void OnPreDrawObjects(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_predrawobjects != null)
      {
        try
        {
          m_predrawobjects(null, new DrawEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnDrawObject(IntPtr pPipeline, IntPtr pConduit)
    {
      if( m_drawobject != null )
      {
        try
        {
          m_drawobject(null, new DrawObjectEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnPostDrawObjects(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_postdrawobjects != null)
      {
        try
        {
          m_postdrawobjects(null, new DrawEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnDrawForeground(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_drawforeground != null)
      {
        try
        {
          m_drawforeground(null, new DrawForegroundEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnDrawOverlay(IntPtr pPipeline, IntPtr pConduit)
    {
      if (m_drawoverlay != null)
      {
        try
        {
          m_drawoverlay(null, new DrawEventArgs(pPipeline, pConduit));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }


    public static event EventHandler<CullObjectEventArgs> ObjectCulling
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_objectCulling, value))
          return;

        if (null == m_objectCulling)
        {
          m_ObjectCullingCallback = OnObjectCulling;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxObjectCulling, m_ObjectCullingCallback, m_report);
        }

        m_objectCulling -= value;
        m_objectCulling += value;
      }
      remove
      {
        m_objectCulling -= value;
        if (m_objectCulling == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxObjectCulling, null, m_report);
          m_ObjectCullingCallback = null;
        }
      }
    }


    public static event EventHandler<CalculateBoundingBoxEventArgs> CalculateBoundingBox
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_calcbbox, value))
          return;

        if (null == m_calcbbox)
        {
          m_CalcBoundingBoxCallback = OnCalcBoundingBox;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxCalcBoundingBox, m_CalcBoundingBoxCallback, m_report);
        }

        m_calcbbox -= value;
        m_calcbbox += value;
      }
      remove
      {
        m_calcbbox -= value;
        if (m_calcbbox == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxCalcBoundingBox, null, m_report);
          m_CalcBoundingBoxCallback = null;
        }
      }
    }

    /// <summary>
    /// Calculate a bounding to include in the Zoom Extents command.
    /// </summary>
    public static event EventHandler<CalculateBoundingBoxEventArgs> CalculateBoundingBoxZoomExtents
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_calcbbox_zoomextents, value))
          return;

        if (null == m_calcbbox_zoomextents)
        {
          m_CalcBoundingBoxZoomExtentsCallback = OnCalcBoundingBoxZoomExtents;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxCalcBoundingBoxZoomExtents, m_CalcBoundingBoxZoomExtentsCallback, m_report);
        }

        m_calcbbox_zoomextents -= value;
        m_calcbbox_zoomextents += value;
      }
      remove
      {
        m_calcbbox_zoomextents -= value;
        if (m_calcbbox_zoomextents == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxCalcBoundingBoxZoomExtents, null, m_report);
          m_CalcBoundingBoxZoomExtentsCallback = null;
        }
      }
    }

    /// <summary>
    /// Called before objects are been drawn. Depth writing and testing are on.
    /// </summary>
    public static event EventHandler<DrawEventArgs> PreDrawObjects
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_predrawobjects, value))
          return;

        if (null == m_predrawobjects)
        {
          m_PreDrawObjectsCallback = OnPreDrawObjects;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxPreDrawObjects, m_PreDrawObjectsCallback, m_report);
        }
        m_predrawobjects -= value;
        m_predrawobjects += value;
      }
      remove
      {
        m_predrawobjects -= value;
        if (m_predrawobjects == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxPreDrawObjects, null, m_report);
          m_PreDrawObjectsCallback = null;
        }
      }
    }

    /// <summary>
    /// Called right before an individual object is being drawn. NOTE: Do not use this event
    /// unless you absolutely need to.  It is called for every object in the document and can
    /// slow disply down if a large number of objects exist in the document
    /// </summary>
    public static event EventHandler<DrawObjectEventArgs> PreDrawObject
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_drawobject, value))
          return;

        if (null == m_drawobject)
        {
          m_DrawObjectCallback = OnDrawObject;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawObject, m_DrawObjectCallback, m_report);
        }
        m_drawobject -= value;
        m_drawobject += value;
      }
      remove
      {
        m_drawobject -= value;
        if (m_drawobject == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawObject, null, m_report);
          m_DrawObjectCallback = null;
        }
      }
    }

    /// <summary>
    /// Called after all non-highlighted objects have been drawn. Depth writing and testing are
    /// still turned on. If you want to draw without depth writing/testing, see DrawForeground.
    /// </summary>
    public static event EventHandler<DrawEventArgs> PostDrawObjects
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_postdrawobjects, value))
          return;

        if (null == m_postdrawobjects)
        {
          m_PostDrawObjectsCallback = OnPostDrawObjects;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxPostDrawObjects, m_PostDrawObjectsCallback, m_report);
        }
        m_postdrawobjects -= value;
        m_postdrawobjects += value;
      }
      remove
      {
        m_postdrawobjects -= value;
        if (m_postdrawobjects == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxPostDrawObjects, null, m_report);
          m_PostDrawObjectsCallback = null;
        }
      }
    }

    /// <summary>
    /// Called after all non-highlighted objects have been drawn and PostDrawObjects has been called.
    /// Depth writing and testing are turned OFF. If you want to draw with depth writing/testing,
    /// see PostDrawObjects.
    /// </summary>
    /// <remarks>
    /// This event is actually passed a DrawForegroundEventArgs, but we could not change
    /// the event declaration without breaking the SDK. Cast to a DrawForegroundEventArgs
    /// if you need it.
    /// </remarks>
    public static event EventHandler<DrawEventArgs> DrawForeground
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_drawforeground, value))
          return;

        if (null == m_drawforeground)
        {
          m_DrawForegroundCallback = OnDrawForeground;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawForeground, m_DrawForegroundCallback, m_report);
        }
        m_drawforeground -= value;
        m_drawforeground += value;
      }
      remove
      {
        m_drawforeground -= value;
        if (m_drawforeground == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawForeground, null, m_report);
          m_DrawForegroundCallback = null;
        }
      }
    }

    /// <summary>
    /// If Rhino is in a feedback mode, the draw overlay call allows for temporary geometry to be drawn on top of
    /// everything in the scene. This is similar to the dynamic draw routine that occurs with custom get point.
    /// </summary>
    public static event EventHandler<DrawEventArgs> DrawOverlay
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_drawoverlay, value))
          return;

        if (null == m_drawoverlay)
        {
          m_DrawOverlayCallback = OnDrawOverlay;
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawOverlay, m_DrawOverlayCallback, m_report);
        }
        m_drawoverlay -= value;
        m_drawoverlay += value;
      }
      remove
      {
        m_drawoverlay -= value;
        if (m_drawoverlay == null)
        {
          UnsafeNativeMethods.CRhinoDisplayConduit_SetCallback(idxDrawOverlay, null, m_report);
          m_DrawOverlayCallback = null;
        }
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the size of the framebuffer that this pipeline is drawing to.
    /// </summary>
    public Rhino.Drawing.Size FrameSize
    {
      get
      {
        int width = 0, height = 0;
        UnsafeNativeMethods.CRhinoDisplayPipeline_FrameSize(m_ptr, ref width, ref height);
        return new Rhino.Drawing.Size(width, height);
      }
    }

    /// <summary>
    /// Gets the curve thickness as defined by the current display mode. 
    /// Note: this only applies to curve objects, Brep and Mesh wires may have different settings.
    /// </summary>
    public int DefaultCurveThickness
    {
      get 
      {
        int width = UnsafeNativeMethods.CRhinoDisplayPipeline_DefaultCurveThickness(m_ptr);
        return width;
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not this pipeline is drawing into an OpenGL context.
    /// </summary>
    public bool IsOpenGL
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_UsesOpenGL(m_ptr); }
    }
    /// <summary>
    /// Gets a value that indicates whether this pipeline is currently using an 
    /// engine that is performing stereo style drawing. Stereo drawing is for 
    /// providing an "enhanced 3-D" effect through stereo viewing devices.
    /// </summary>
    public bool IsStereoMode
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxInStereoMode); }
    }
    /// <summary>
    /// Gets a value that indicates whether this pipeline 
    /// is currently drawing for printing purposes.
    /// </summary>
    public bool IsPrinting
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxIsPrinting); }
    }
    /// <summary>
    /// Gets a value that indicates whether the viewport is in Dynamic Display state. 
    /// Dynamic display is the state a viewport is in when it is rapidly redrawing because of
    /// an operation like panning or rotating. The pipeline will drop some level of detail
    /// while inside a dynamic display state to keep the frame rate as high as possible.
    /// </summary>
    public bool IsDynamicDisplay
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxViewInDynamicDisplay); }
    }

    // SupportsShading is called in grasshopper a whole lot. Cache value since it never
    // changes for a given pipeline
    int m_supports_shading; //0=uninitialized, 1=no, 2=yes
    /// <summary>
    /// Gets whether or not this pipeline supports shaded meshes.
    /// </summary>
    public bool SupportsShading
    {
      get
      {
        if (2 == m_supports_shading)
          return true;
        if (0 == m_supports_shading)
        {
          bool support = UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxSupportsShading);
          m_supports_shading = support ? 2 : 1;
        }

        return (2 == m_supports_shading);
      }
    }

    /// <summary>
    /// Gets the current stereo projection if stereo mode is on.
    /// <para>0 = left</para>
    /// <para>1 = right</para>
    /// If stereo mode is not enables, this property always returns 0.
    /// </summary>
    public int StereoProjection
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetInt(m_ptr, idxActiveStereoProjection); }
    }

    //David: only applied to vertices?
    /// <summary>
    /// Gets or sets the current model transformation that is applied to vertices when drawing.
    /// </summary>
    public Transform ModelTransform
    {
      get
      {
        Transform xf = new Transform();
        UnsafeNativeMethods.CRhinoDisplayPipeline_GetSetModelTransform(m_ptr, false, ref xf);
        return xf;
      }
      set
      {
        UnsafeNativeMethods.CRhinoDisplayPipeline_GetSetModelTransform(m_ptr, true, ref value);
      }
    }
    /// <summary>
    /// Gets a value that indicates whether the Model Transform is an Identity transformation.
    /// </summary>
    public bool ModelTransformIsIdentity
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxModelTransformIsIdentity); }
    }

    /// <summary>
    /// Gets the current pass that the pipeline is in for drawing a frame. 
    /// Typically drawing a frame requires a single pass through the DrawFrameBuffer 
    /// function, but some special display effects can be achieved through 
    /// drawing with multiple passes.
    /// </summary>
    public int RenderPass
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetInt(m_ptr, idxRenderPass); }
    }
    //David: are 0 and 1 the only possible return values? If so, it should be an enum.
    /// <summary>
    /// Gets the current nested viewport drawing level. 
    /// This is used to know if you are currently inside the drawing of a nested viewport (detail object in Rhino). 
    /// <para>Nest level = 0 Drawing is occuring in a standard Rhino viewport or on the page viewport.</para>
    /// <para>Nest level = 1 Drawing is occuring inside a detail view object.</para>
    /// </summary>
    public int NestLevel
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetInt(m_ptr, idxNestLevel); }
    }

    /// <summary>
    /// Gets a value that indicates whether the pipeline is currently in a curve
    /// drawing operation. This is useful when inside of a draw event or display
    /// conduit to check and see if the geometry is about to be drawn is going to
    /// be drawing the wire representation of the geometry (mesh, extrusion, or
    /// brep).  See DrawingSurfaces to check and see if the shaded mesh representation
    /// of the geometry is going to be drawn.
    /// </summary>
    public bool DrawingWires
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxDrawingWires); }
    }
    /// <summary>
    /// Gets a value that indicates whether the pipeline is currently in a grip drawing operation.
    /// </summary>
    public bool DrawingGrips
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxDrawingGrips); }
    }
    /// <summary>
    /// Gets a value that indicates whether the pipeline is currently in a surface
    /// drawing operation.  Surface drawing means draw the shaded triangles of a mesh
    /// representing the surface (mesh, extrusion, or brep).  This is useful when
    /// inside of a draw event or display conduit to check and see if the geometry is
    /// about to be drawn as a shaded set of triangles representing the geometry.
    /// See DrawingWires to check and see if the wireframe representation of the
    /// geometry is going to be drawn.
    /// </summary>
    public bool DrawingSurfaces
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxDrawingSurfaces); }
    }

    /// <summary>
    /// Gets or sets the "ShadingRequired" flag. This flag gets set inside the pipeline when a request is 
    /// made to draw a shaded mesh but the current render engine doesn't support shaded 
    /// mesh drawing...at this point the redraw mechanism will make sure everything will 
    /// work the next time around.
    /// </summary>
    public bool ShadingRequired
    {
      get { return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxShadingRequired); }
      set { UnsafeNativeMethods.CRhinoDisplayPipeline_SetShadingRequired(m_ptr, value); }
    }

    RhinoViewport m_viewport;
    public RhinoViewport Viewport
    {
      get
      {
        if (null == m_viewport)
        {
          IntPtr pDisplayPipeline = NonConstPointer();
          IntPtr pViewport = UnsafeNativeMethods.CRhinoDisplayPipeline_RhinoViewport(pDisplayPipeline);
          if (IntPtr.Zero != pViewport)
            m_viewport = new RhinoViewport(null, pViewport);
        }
        return m_viewport;
      }
    }

    DisplayPipelineAttributes m_display_attrs;
    public DisplayPipelineAttributes DisplayPipelineAttributes
    {
      get { return m_display_attrs ?? (m_display_attrs = new DisplayPipelineAttributes(this)); }
    }

    internal IntPtr DisplayAttributeConstPointer()
    {
      IntPtr pConstThis = NonConstPointer(); // There is no const version of display pipeline accessible in RhinoCommon
      return UnsafeNativeMethods.CRhinoDisplayPipeline_DisplayAttrs(pConstThis);
    }

    #endregion

    #region pipeline settings
    /// <summary>
    /// Push a model transformation on the engine's model transform stack.
    /// </summary>
    /// <param name="xform">Transformation to push.</param>
    public void PushModelTransform(Transform xform)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_PushModelTransform(m_ptr, ref xform);
    }

    /// <summary>
    /// Pop a model transformation off the engine's model transform stack.
    /// </summary>
    public void PopModelTransform()
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Pop(m_ptr, idxModelTransform);
    }

    /// <summary>
    /// Enable or disable the DepthTesting behaviour of the engine. 
    /// When DepthTesting is disabled, objects in front will no 
    /// longer occlude objects behind them.
    /// </summary>
    /// <param name="enable">true to enable DepthTesting, false to disable.</param>
    public void EnableDepthTesting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Enable(m_ptr, enable, idxDepthTesting);
    }

    /// <summary>
    /// Enable or disable the DepthWriting behaviour of the engine. 
    /// When DepthWriting is disabled, drawn geometry does not affect the Z-Buffer.
    /// </summary>
    /// <param name="enable">true to enable DepthWriting, false to disable.</param>
    public void EnableDepthWriting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Enable(m_ptr, enable, idxDepthWriting);
    }

    /// <summary>
    /// Enable or disable the ColorWriting behaviour of the engine. 
    /// </summary>
    /// <param name="enable">true to enable ColorWriting, false to disable.</param>
    public void EnableColorWriting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Enable(m_ptr, enable, idxColorWriting);
    }

    /// <summary>
    /// Enable or disable the Lighting logic of the engine. 
    /// </summary>
    /// <param name="enable">true to enable Lighting, false to disable.</param>
    public void EnableLighting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Enable(m_ptr, enable, idxLighting);
    }

    /// <summary>
    /// Enable or disable the Clipping Plane logic of the engine. 
    /// </summary>
    /// <param name="enable">true to enable Clipping Planes, false to disable.</param>
    public void EnableClippingPlanes(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Enable(m_ptr, enable, idxClippingPlanes);
    }

    /// <summary>
    /// Push a DepthTesting flag on the engine's stack.
    /// </summary>
    /// <param name="enable">DepthTesting flag.</param>
    public void PushDepthTesting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Push(m_ptr, enable, idxDepthTesting);
    }

    /// <summary>
    /// Pop a DepthTesting flag off the engine's stack.
    /// </summary>
    public void PopDepthTesting()
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Pop(m_ptr, idxDepthTesting);
    }

    /// <summary>
    /// Push a DepthWriting flag on the engine's stack.
    /// </summary>
    /// <param name="enable">DepthWriting flag.</param>
    public void PushDepthWriting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Push(m_ptr, enable, idxDepthWriting);
    }

    /// <summary>
    /// Pop a DepthWriting flag off the engine's stack.
    /// </summary>
    public void PopDepthWriting()
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Pop(m_ptr, idxDepthWriting);
    }

    /// <summary>
    /// Push a ClipTesting flag on the engine's stack.
    /// </summary>
    /// <param name="enable">ClipTesting flag.</param>
    public void PushClipTesting(bool enable)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Push(m_ptr, enable, idxClipTesting);
    }

    /// <summary>
    /// Pop a ClipTesting flag off the engine's stack.
    /// </summary>
    public void PopClipTesting()
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Pop(m_ptr, idxClipTesting);
    }

    /// <summary>
    /// Push a FaceCull flag on the engine's stack.
    /// </summary>
    /// <param name="mode">FaceCull flag.</param>
    public void PushCullFaceMode(CullFaceMode mode)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_PushCullFaceMode(m_ptr, (int)mode);
    }

    /// <summary>
    /// Pop a FaceCull flag off the engine's stack.
    /// </summary>
    public void PopCullFaceMode()
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_Pop(m_ptr, idxCullFaceMode);
    }

    public DepthMode DepthMode
    {
      get { return (DepthMode)UnsafeNativeMethods.CRhinoDisplayPipeline_GetInt(m_ptr, idxDepthMode); }
      set { UnsafeNativeMethods.CRhinoDisplayPipeline_SetInt(m_ptr, idxDepthMode, (int)value); }
    }

    public ZBiasMode ZBiasMode
    {
      get { return (ZBiasMode)UnsafeNativeMethods.CRhinoDisplayPipeline_GetInt(m_ptr, idxZBiasMode); }
      set { UnsafeNativeMethods.CRhinoDisplayPipeline_SetInt(m_ptr, idxZBiasMode, (int)value); }
    }
    /*
    public DepthMode DepthMode
    {
      get{}
      set{}
    }
    public ZBiasMode ZBiasMode
    {
      get { }
      set { }
    }
    //public AntiAliasMode AntiAliasMode
    //{
    //  get { }
    //  set { }
    //}

    //public void EnableDrawGrayScale(bool enable)
    //{
    //}

    DocObjects.RhinoObject GetObjectAt(Rhino.Drawing.Point screenPoint)
    {
    }
    */
    #endregion

    #region methods
    /// <summary>
    /// Returns a value indicating if only points on the side of the surface that
    /// face the camera are displayed.
    /// </summary>
    /// <returns>true if backfaces of surface and mesh control polygons are culled.
    /// This value is determined by the _CullControlPolygon command.</returns>
    public static bool CullControlPolygon()
    {
      return UnsafeNativeMethods.RHC_RhinoCullControlPolygon();
    }

    /// <summary>
    /// Test a given 3d world coordinate point for visibility inside the view 
    /// frustum under the current viewport and model transformation settings.
    /// </summary>
    /// <param name="worldCoordinate">Point to test for visibility.</param>
    /// <returns>true if the point is visible, false if it is not.</returns>
    public bool IsVisible(Point3d worldCoordinate)
    {
      return UnsafeNativeMethods.CRhinoDisplayPipeline_IsVisible1(m_ptr, worldCoordinate);
    }

    /// <summary>
    /// Test a given object for visibility inside the view frustum under the current viewport and model 
    /// transformation settings. This function calls a virtual IsVisibleFinal function that 
    /// subclassed pipelines can add extra tests to. In the base class, this test only tests 
    /// visibility based on the objects world coordinates location and does not pay attention 
    /// to the object's attributes.
    /// 
    /// NOTE: Use CRhinoDisplayPipeline::IsVisible() to perform "visibility" 
    ///       tests based on location (is some part of the object in the view frustum). 
    ///       Use CRhinoDisplayPipeline::IsActive() to perform "visibility" 
    ///       tests based on object type.
    /// </summary>
    /// <param name="rhinoObject">Object to test.</param>
    /// <returns>true if the object is visible, false if not.</returns>
    public bool IsVisible(DocObjects.RhinoObject rhinoObject)
    {
      IntPtr pRhinoObject = rhinoObject.ConstPointer();
      return UnsafeNativeMethods.CRhinoDisplayPipeline_IsVisibleOrActive(m_ptr, pRhinoObject, true);
    }

    /// <summary>
    /// Test a given box for visibility inside the view frustum under the current 
    /// viewport and model transformation settings.
    /// </summary>
    /// <param name="bbox">Box to test for visibility.</param>
    /// <returns>true if at least some portion of the box is visible, false if not.</returns>
    public bool IsVisible(BoundingBox bbox)
    {
      return UnsafeNativeMethods.CRhinoDisplayPipeline_IsVisible2(m_ptr, ref bbox);
    }

    /// <summary>
    /// Determines if an object can be visible in this viewport based on it's object type and display attributes. 
    /// This test does not check for visibility based on location of the object. 
    /// NOTE: Use CRhinoDisplayPipeline::IsVisible() to perform "visibility" 
    ///       tests based on location (is some part of the object in the view frustum). 
    ///       Use CRhinoDisplayPipeline::IsActive() to perform "visibility" 
    ///       tests based on object type.
    /// </summary>
    /// <param name="rhinoObject">Object to test.</param>
    /// <returns>
    /// true if this object can be drawn in the pipeline's viewport based on it's object type and display attributes.
    /// </returns>
    public bool IsActive(DocObjects.RhinoObject rhinoObject)
    {
      IntPtr pRhinoObject = rhinoObject.ConstPointer();
      return UnsafeNativeMethods.CRhinoDisplayPipeline_IsVisibleOrActive(m_ptr, pRhinoObject, false);
    }

    /// <summary>
    /// Tests to see if the pipeline should stop drawing more geometry and just show what it has so far. 
    /// If a drawing operation is taking a long time, this function will return true and tell Rhino it should just 
    /// finish up and show the frame buffer. This is used in dynamic drawing operations. 
    /// </summary>
    /// <returns>
    /// true if the pipeline should stop attempting to draw more geometry and just show the frame buffer.
    /// </returns>
    public bool InterruptDrawing()
    {
      return UnsafeNativeMethods.CRhinoDisplayPipeline_GetBool(m_ptr, idxInterruptDrawing);
    }

    /// <summary>
    /// Draw a given viewport to an off-screen bitmap.
    /// </summary>
    /// <param name="viewport">Viewport to draw.</param>
    /// <param name="width">Width of target image.</param>
    /// <param name="height">Height of target image.</param>
    /// <returns>A bitmap containing the given view, or null on error.</returns>
    public static Rhino.Drawing.Bitmap DrawToBitmap(RhinoViewport viewport, int width, int height)
    {
      if (null == viewport)
        return null;
      IntPtr pViewport = viewport.ConstPointer();
      IntPtr hBitmap = UnsafeNativeMethods.CRhinoDisplayPipeline_DrawToBitmap(pViewport, width, height);
      if (IntPtr.Zero == hBitmap)
        return null;
      return Rhino.Drawing.Image.FromHbitmap(hBitmap);
    }
    #endregion

    #region CDisplayPipeline draw functions
    /// <summary>
    /// Draws all the wires in a given mesh.
    /// </summary>
    /// <param name="mesh">Mesh for wire drawing.</param>
    /// <param name="color">Color of mesh wires.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_meshdrawing.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_meshdrawing.cs' lang='cs'/>
    /// <code source='examples\py\ex_meshdrawing.py' lang='py'/>
    /// </example>
    public void DrawMeshWires(Mesh mesh, Rhino.Drawing.Color color)
    {
      DrawMeshWires(mesh, color, 1);
    }
    /// <summary>
    /// Draws all the wires in a given mesh.
    /// </summary>
    /// <param name="mesh">Mesh for wire drawing.</param>
    /// <param name="color">Color of mesh wires.</param>
    /// <param name="thickness">Thickness (in pixels) of mesh wires.</param>
    public void DrawMeshWires(Mesh mesh, Rhino.Drawing.Color color, int thickness)
    {
      if (thickness > 0)
      {
        int argb = color.ToArgb();
        IntPtr pMesh = mesh.ConstPointer();
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawMeshWires(m_ptr, pMesh, argb, thickness);
      }
    }
    /// <summary>
    /// Draws all the vertices in a given mesh.
    /// </summary>
    /// <param name="mesh">Mesh for vertex drawing.</param>
    /// <param name="color">Color of mesh vertices.</param>
    public void DrawMeshVertices(Mesh mesh, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      IntPtr pMesh = mesh.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawMeshVertices(m_ptr, pMesh, argb);
    }

    /// <summary>
    /// Draws the shaded faces of a given mesh.
    /// </summary>
    /// <param name="mesh">Mesh to draw.</param>
    /// <param name="material">Material to draw faces with.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_meshdrawing.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_meshdrawing.cs' lang='cs'/>
    /// <code source='examples\py\ex_meshdrawing.py' lang='py'/>
    /// </example>
    public void DrawMeshShaded(Mesh mesh, DisplayMaterial material)
    {
      IntPtr pMesh = mesh.ConstPointer();
      IntPtr pMaterial = IntPtr.Zero;
      if (null != material)
        pMaterial = material.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawShadedMesh(m_ptr, pMesh, pMaterial);
    }

    /// <summary>
    /// Draws the shaded faces of a given mesh.
    /// </summary>
    /// <param name="mesh">Mesh to draw.</param>
    /// <param name="material">Material to draw faces with.</param>
    /// <param name="faceIndices">Indices of specific faces to draw</param>
    public void DrawMeshShaded(Mesh mesh, DisplayMaterial material, int[] faceIndices)
    {
      IntPtr pMesh = mesh.ConstPointer();
      IntPtr pMaterial = IntPtr.Zero;
      if (null != material)
        pMaterial = material.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawShadedMesh2(m_ptr, pMesh, pMaterial, faceIndices.Length, faceIndices);
    }

    /// <summary>
    /// Draws the mesh faces as false color patches. 
    /// The mesh must have Vertex Colors defined for this to work.
    /// </summary>
    /// <param name="mesh">Mesh to draw.</param>
    public void DrawMeshFalseColors(Mesh mesh)
    {
      IntPtr pMesh = mesh.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawMeshFalseColors(m_ptr, pMesh, false);
    }

    /// <summary>
    /// Draws a shaded mesh representation of a brep.
    /// </summary>
    /// <param name="brep">Brep to draw.</param>
    /// <param name="material">Material to draw faces with.</param>
    public void DrawBrepShaded(Brep brep, DisplayMaterial material)
    {
      IntPtr pBrep = brep.ConstPointer();
      IntPtr pMaterial = IntPtr.Zero;
      if (null != material)
        pMaterial = material.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawShadedBrep(m_ptr, pBrep, pMaterial);
    }

    /// <summary>
    /// Draws all the wireframe curves of a brep object.
    /// </summary>
    /// <param name="brep">Brep to draw.</param>
    /// <param name="color">Color of Wireframe curves.</param>
    public void DrawBrepWires(Brep brep, Rhino.Drawing.Color color)
    {
      DrawBrepWires(brep, color, 1);
    }
    /// <summary>
    /// Draws all the wireframe curves of a brep object.
    /// </summary>
    /// <param name="brep">Brep to draw.</param>
    /// <param name="color">Color of Wireframe curves.</param>
    /// <param name="wireDensity">
    /// "Density" of wireframe curves.
    /// <para>-1 = no internal wires.</para>
    /// <para> 0 = default internal wires.</para>
    /// <para>>0 = custom high density.</para>
    /// </param>
    public void DrawBrepWires(Brep brep, Rhino.Drawing.Color color, int wireDensity)
    {
      int argb = color.ToArgb();
      IntPtr pBrep = brep.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBrep(m_ptr, pBrep, argb, wireDensity);
    }

    /// <summary>Draws a point with a given radius, style and color.</summary>
    /// <param name="point">Location of point in world coordinates.</param>
    /// <param name="color">Color of point.</param>
    public void DrawPoint(Point3d point, Rhino.Drawing.Color color)
    {
      DrawPoint(point, PointStyle.ControlPoint, 3, color);
    }
    /// <summary>Draws a point with a given radius, style and color.</summary>
    /// <param name="point">Location of point in world coordinates.</param>
    /// <param name="style">Point display style.</param>
    /// <param name="radius">Point size in pixels.</param>
    /// <param name="color">
    /// Color of point. If style is ControlPoint, this will be the border color.
    /// </param>
    public void DrawPoint(Point3d point, PointStyle style, int radius, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      Point3d[] pts = new Point3d[] { point };
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawPoints(m_ptr, 1, pts, (int)style, radius, argb);
    }
    /// <summary>Draw a set of points with a given radius, style and color.</summary>
    /// <param name="points">Location of points in world coordinates.</param>
    /// <param name="style">Point display style.</param>
    /// <param name="radius">Point size in pixels.</param>
    /// <param name="color">
    /// Color of points. If style is ControlPoint, this will be the border color.
    /// </param>
    public void DrawPoints(System.Collections.Generic.IEnumerable<Point3d> points, PointStyle style, int radius, Rhino.Drawing.Color color)
    {
      int count;
      Point3d[] ptArray = Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 1)
        return;
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawPoints(m_ptr, count, ptArray, (int)style, radius, argb);
    }

    /// <summary>
    /// Draws a point cloud.
    /// </summary>
    /// <param name="cloud">Point cloud to draw, if the cloud has a color array, it will be used, otherwise the points will be black.</param>
    /// <param name="size">Size of points.</param>
    public void DrawPointCloud(PointCloud cloud, int size)
    {
      DrawPointCloud(cloud, size, Rhino.Drawing.Color.Black);
    }
    /// <summary>
    /// Draws a point cloud.
    /// </summary>
    /// <param name="cloud">Point cloud to draw.</param>
    /// <param name="size">Size of points.</param>
    /// <param name="color">Color of points in the cloud, if the cloud has a color array this setting is ignored.</param>
    public void DrawPointCloud(PointCloud cloud, int size, Rhino.Drawing.Color color)
    {
      IntPtr pCloud = cloud.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawPointCloud(m_ptr, pCloud, size, color.ToArgb());
    }

    public void DrawDirectionArrow(Point3d location, Vector3d direction, Rhino.Drawing.Color color)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDirectionArrow(m_ptr, location, direction, color.ToArgb());
    }

    /// <summary>
    /// Draws a single arrow object. An arrow consists of a Shaft and an Arrow head at the end of the shaft.
    /// </summary>
    /// <param name="line">Arrow shaft.</param>
    /// <param name="color">Color of arrow.</param>
    public void DrawArrow(Line line, Rhino.Drawing.Color color)
    {
      Line[] lines = new Line[] { line };
      DrawArrows(lines, color);
    }
    /// <summary>
    /// Draws a single arrow object. 
    /// An arrow consists of a Shaft and an Arrow head at the end of the shaft.
    /// </summary>
    /// <param name="line">Arrow shaft.</param>
    /// <param name="color">Color of arrow.</param>
    /// <param name="screenSize">If screenSize != 0.0 then the size (in screen pixels) of the arrow head will be equal to screenSize.</param>
    /// <param name="relativeSize">If relativeSize != 0.0 and screensize == 0.0 the size of the arrow head will be proportional to the arrow shaft length.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_conduitarrowheads.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_conduitarrowheads.cs' lang='cs'/>
    /// <code source='examples\py\ex_conduitarrowheads.py' lang='py'/>
    /// </example>
    public void DrawArrow(Line line, Rhino.Drawing.Color color, double screenSize, double relativeSize)
    {
      Line[] lines = new Line[] { line };
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrows2(m_ptr, 1, lines, color.ToArgb(), screenSize, relativeSize);
    }
    /// <summary>
    /// Draws a collection of arrow objects. An arrow consists of a Shaft and an Arrow head at the end of the shaft.
    /// </summary>
    /// <param name="lines">Arrow shafts.</param>
    /// <param name="color">Color of arrows.</param>
    public void DrawArrows(Line[] lines, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrows(m_ptr, lines.Length, lines, argb);
    }
    /// <summary>
    /// Draws a collection of arrow objects. An arrow consists of a Shaft and an Arrow head at the end of the shaft. 
    /// </summary>
    /// <param name="lines">Arrow shafts.</param>
    /// <param name="color">Color of arrows.</param>
    public void DrawArrows(System.Collections.Generic.IEnumerable<Line> lines, Rhino.Drawing.Color color)
    {
      if (lines == null) { return; }

      int argb = color.ToArgb();

      // Cast Line array
      Line[] line_array = lines as Line[];
      if (line_array != null)
      {
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrows(m_ptr, line_array.Length, line_array, argb);
        return;
      }

      // Cast RhinoList<Line>
      Rhino.Collections.RhinoList<Line> line_list = lines as Rhino.Collections.RhinoList<Line>;
      if (line_list != null)
      {
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrows(m_ptr, line_list.Count, line_list.m_items, argb);
        return;
      }

      // Iterate over the enumeration
      Rhino.Collections.RhinoList<Line> line_copy = new Rhino.Collections.RhinoList<Line>();
      line_copy.AddRange(lines);
      if (line_copy.Count > 0)
      {
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrows(m_ptr, line_copy.Count, line_copy.m_items, argb);
      }
    }

    /// <summary>
    /// Draws a single arrow head.
    /// </summary>
    /// <param name="tip">Point of arrow head tip.</param>
    /// <param name="direction">Direction in which arrow head is pointing.</param>
    /// <param name="color">Color of arrow head.</param>
    /// <param name="screenSize">If screenSize != 0.0, then the size (in screen pixels) of the arrow head will be equal to the screenSize.</param>
    /// <param name="worldSize">If worldSize != 0.0 and screensize == 0.0 the size of the arrow head will be equal to the number of units in worldSize.</param>
    public void DrawArrowHead(Point3d tip, Vector3d direction, Rhino.Drawing.Color color, double screenSize, double worldSize)
    {
      if (screenSize != 0.0)
      {
        Line line = new Line(tip, tip - direction);
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrowHead(m_ptr, ref line, color.ToArgb(), screenSize, 0.0);
      }
      else if (worldSize != 0.0)
      {
        if (!direction.Unitize()) { return; }
        Line line = new Line(tip, tip - direction * worldSize);
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArrowHead(m_ptr, ref line, color.ToArgb(), 0.0, 1.0);
      }
    }
    /// <summary>
    /// Draws an arrow made up of three line segments.
    /// </summary>
    /// <param name="line">Base line for arrow.</param>
    /// <param name="color">Color of arrow.</param>
    /// <param name="thickness">Thickness (in pixels) of the arrow line segments.</param>
    /// <param name="size">Size (in world units) of the arrow tip lines.</param>
    public void DrawLineArrow(Line line, Rhino.Drawing.Color color, int thickness, double size)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawLineArrow(m_ptr, ref line, color.ToArgb(), thickness, size);
    }

    /// <summary>
    /// Draws a single line object.
    /// </summary>
    /// <param name="line">Line to draw.</param>
    /// <param name="color">Color to draw line in.</param>
    public void DrawLine(Line line, Rhino.Drawing.Color color)
    {
      DrawLine(line.From, line.To, color, 1);
    }
    /// <summary>
    /// Draws a single line object.
    /// </summary>
    /// <param name="line">Line to draw.</param>
    /// <param name="color">Color to draw line in.</param>
    /// <param name="thickness">Thickness (in pixels) of line.</param>
    public void DrawLine(Line line, Rhino.Drawing.Color color, int thickness)
    {
      DrawLine(line.From, line.To, color, thickness);
    }
    /// <summary>
    /// Draws a single line object.
    /// </summary>
    /// <param name="from">Line from point.</param>
    /// <param name="to">Line to point.</param>
    /// <param name="color">Color to draw line in.</param>
    public void DrawLine(Point3d from, Point3d to, Rhino.Drawing.Color color)
    {
      DrawLine(from, to, color, 1);
    }
    /// <summary>
    /// Draws a single line object.
    /// </summary>
    /// <param name="from">Line from point.</param>
    /// <param name="to">Line to point.</param>
    /// <param name="color">Color to draw line in.</param>
    /// <param name="thickness">Thickness (in pixels) of line.</param>
    public void DrawLine(Point3d from, Point3d to, Rhino.Drawing.Color color, int thickness)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawLine(m_ptr, from, to, color.ToArgb(), thickness);
    }

    /// <summary>
    /// Draws a single dotted line.
    /// </summary>
    /// <param name="line">Line to draw.</param>
    /// <param name="color">Color of line.</param>
    public void DrawDottedLine(Line line, Rhino.Drawing.Color color)
    {
      DrawDottedLine(line.From, line.To, color);
    }
    /// <summary>
    /// Draws a single dotted line.
    /// </summary>
    /// <param name="from">Line start point.</param>
    /// <param name="to">Line end point.</param>
    /// <param name="color">Color of line.</param>
    public void DrawDottedLine(Point3d from, Point3d to, Rhino.Drawing.Color color)
    {
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDottedLine(m_ptr, from, to, color.ToArgb());
    }

    /// <summary>
    /// Draws a set of connected lines (polyline) in a dotted pattern.
    /// </summary>
    /// <param name="points">End points of each line segment.</param>
    /// <param name="color">Color of polyline.</param>
    /// <param name="close">Draw a line between the first and last points.</param>
    public void DrawDottedPolyline(System.Collections.Generic.IEnumerable<Point3d> points, Rhino.Drawing.Color color, bool close)
    {
      Point3d first_point = Point3d.Unset;
      Point3d previous = Point3d.Unset;
      foreach(Point3d point in points)
      {
        if( previous.IsValid )
        {
          DrawDottedLine(previous, point, color);
        }
        previous = point;
        if (close && !first_point.IsValid)
          first_point = point;
      }
      if (close && previous.IsValid && first_point.IsValid && previous != first_point)
        DrawDottedLine(previous, first_point, color);
    }

    /// <summary>
    /// Draws a set of lines with a given color and thickness. If you want the fastest possible set of lines
    /// to be drawn, pass a Line[] for lines.
    /// </summary>
    /// <param name="lines">Lines to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawLines(System.Collections.Generic.IEnumerable<Line> lines, Rhino.Drawing.Color color)
    {
      DrawLines(lines, color, 1);
    }
    /// <summary>
    /// Draws a set of lines with a given color and thickness. If you want the fastest possible set of lines
    /// to be drawn, pass a Line[] for lines.
    /// </summary>
    /// <param name="lines">Lines to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of lines.</param>
    public void DrawLines(System.Collections.Generic.IEnumerable<Line> lines, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      Line[] linesArray = lines as Line[];
      if (linesArray != null)
      {
        int count = linesArray.Length;
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawLines(m_ptr, count, linesArray, argb, thickness);
      }
      else
      {
        UnsafeNativeMethods.CRhinoDisplayPipeline_SetMultiLineAttributes(argb, thickness);
        foreach (Line line in lines)
        {
          UnsafeNativeMethods.CRhinoDisplayPipeline_MultiLineDraw(m_ptr, line.From, line.To);
        }
      }
    }

    /// <summary>
    /// Draws a single Polyline object.
    /// </summary>
    /// <param name="polyline">Polyline to draw.</param>
    /// <param name="color">Color to draw in.</param>
    public void DrawPolyline(System.Collections.Generic.IEnumerable<Point3d> polyline, Rhino.Drawing.Color color)
    {
      DrawPolyline(polyline, color, 1);
    }
    /// <summary>
    /// Draws a single Polyline object.
    /// </summary>
    /// <param name="polyline">Polyline to draw.</param>
    /// <param name="color">Color to draw in.</param>
    /// <param name="thickness">Thickness (in pixels) of the Polyline.</param>
    public void DrawPolyline(System.Collections.Generic.IEnumerable<Point3d> polyline, Rhino.Drawing.Color color, int thickness)
    {
      int count;
      Point3d[] points = Collections.Point3dList.GetConstPointArray(polyline, out count);
      if (null != points && count > 1)
      {
        int argb = color.ToArgb();
        UnsafeNativeMethods.CRhinoDisplayPipeline_DrawPolyline(m_ptr, count, points, argb, thickness);
      }
    }

    /// <summary>
    /// Draws a filled polygon.
    /// </summary>
    /// <param name="points">
    /// Collection of world coordinate points that are connected by lines to form a closed shape. 
    /// Collection must contain at least 3 points.
    /// </param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="filled">
    /// true if the closed area should be filled with color. 
    /// false if you want to draw just the border of the closed shape.
    /// </param>
    public void DrawPolygon(System.Collections.Generic.IEnumerable<Point3d> points, Rhino.Drawing.Color color, bool filled)
    {
      int count;
      Point3d[] ptArray = Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 3)
        return;

      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeLine_DrawPolygon(m_ptr, count, ptArray, argb, filled);
    }

    /// <summary>
    /// Draws a text dot in screen coordinates.
    /// </summary>
    /// <param name="screenX">X coordinate (in pixels) of dot center.</param>
    /// <param name="screenY">Y coordinate (in pixels) of dot center.</param>
    /// <param name="text">Text content of dot.</param>
    /// <param name="dotColor">Dot background color.</param>
    /// <param name="textColor">Dot foreground color.</param>
    public void DrawDot(int screenX, int screenY, string text, Rhino.Drawing.Color dotColor, Rhino.Drawing.Color textColor)
    {
      int argbDot = dotColor.ToArgb();
      int argbText = textColor.ToArgb();
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDot3(pThis, screenX, screenY, text, argbDot, argbText);
    }
    /// <summary>
    /// Draws a text dot in screen coordinates.
    /// </summary>
    /// <param name="screenX">X coordinate (in pixels) of dot center.</param>
    /// <param name="screenY">Y coordinate (in pixels) of dot center.</param>
    /// <param name="text">Text content of dot.</param>
    public void DrawDot(int screenX, int screenY, string text)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDot4(pThis, screenX, screenY, text);
    }
    /// <summary>
    /// Draw a text dot in world coordinates.
    /// </summary>
    /// <param name="worldPosition">Location of dot in world coordinates.</param>
    /// <param name="text">Text content of dot.</param>
    /// <param name="dotColor">Dot background color.</param>
    /// <param name="textColor">Dot foreground color.</param>
    public void DrawDot(Point3d worldPosition, string text, Rhino.Drawing.Color dotColor, Rhino.Drawing.Color textColor)
    {
      int argbDot = dotColor.ToArgb();
      int argbText = textColor.ToArgb();
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDot(pThis, worldPosition, text, argbDot, argbText);
    }
    /// <summary>
    /// Draws a text dot in world coordinates.
    /// </summary>
    /// <param name="worldPosition">Location of dot in world coordinates.</param>
    /// <param name="text">Text content of dot.</param>
    public void DrawDot(Point3d worldPosition, string text)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawDot2(pThis, worldPosition, text);
    }

    /// <summary>
    /// Draws the edges of a BoundingBox.
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw in.</param>
    public void DrawBox(BoundingBox box, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBox(m_ptr, box.m_min, box.m_max, argb, 1);
    }
    /// <summary>
    /// Draws the edges of a BoundingBox.
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw in.</param>
    /// <param name="thickness">Thickness (in pixels) of box edges.</param>
    public void DrawBox(BoundingBox box, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBox(m_ptr, box.m_min, box.m_max, argb, thickness);
    }
    /// <summary>
    /// Draws the edges of a Box object.
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw in.</param>
    public void DrawBox(Box box, Rhino.Drawing.Color color)
    {
      DrawBox(box, color, 1);
    }
    /// <summary>
    /// Draws the edges of a Box object.
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw in.</param>
    /// <param name="thickness">Thickness (in pixels) of box edges.</param>
    public void DrawBox(Box box, Rhino.Drawing.Color color, int thickness)
    {
      if (!box.IsValid) { return; }

      bool dx = box.X.IsSingleton;
      bool dy = box.Y.IsSingleton;
      bool dz = box.Z.IsSingleton;

      // If degenerate in all directions, then there's nothing to draw.
      if (dx && dy && dz) { return; }

      Point3d[] C = box.GetCorners();

      // If degenerate in two directions, we can draw a single line.
      if (dx && dy)
      {
        DrawLine(C[0], C[4], color, thickness);
        return;
      }
      if (dx && dz)
      {
        DrawLine(C[0], C[3], color, thickness);
        return;
      }
      if (dy && dz)
      {
        DrawLine(C[0], C[1], color, thickness);
        return;
      }

      // If degenerate in one direction, we can draw rectangles.
      if (dx)
      {
        DrawLine(C[0], C[3], color, thickness);
        DrawLine(C[3], C[7], color, thickness);
        DrawLine(C[7], C[4], color, thickness);
        DrawLine(C[4], C[0], color, thickness);
        return;
      }
      if (dy)
      {
        DrawLine(C[0], C[1], color, thickness);
        DrawLine(C[1], C[5], color, thickness);
        DrawLine(C[5], C[4], color, thickness);
        DrawLine(C[4], C[0], color, thickness);
        return;
      }
      if (dz)
      {
        DrawLine(C[0], C[1], color, thickness);
        DrawLine(C[1], C[2], color, thickness);
        DrawLine(C[2], C[3], color, thickness);
        DrawLine(C[3], C[0], color, thickness);
        return;
      }

      // Draw all 12 edges
      DrawLine(C[0], C[1], color, thickness);
      DrawLine(C[1], C[2], color, thickness);
      DrawLine(C[2], C[3], color, thickness);
      DrawLine(C[3], C[0], color, thickness);

      DrawLine(C[0], C[4], color, thickness);
      DrawLine(C[1], C[5], color, thickness);
      DrawLine(C[2], C[6], color, thickness);
      DrawLine(C[3], C[7], color, thickness);

      DrawLine(C[4], C[5], color, thickness);
      DrawLine(C[5], C[6], color, thickness);
      DrawLine(C[6], C[7], color, thickness);
      DrawLine(C[7], C[4], color, thickness);
    }

    /// <summary>
    /// Draws corner widgets of a world aligned boundingbox. 
    /// Widget size will be 5% of the Box diagonal.
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawBoxCorners(BoundingBox box, Rhino.Drawing.Color color)
    {
      double diag = box.m_min.DistanceTo(box.m_max);
      DrawBoxCorners(box, color, 0.05 * diag, 1);
    }
    /// <summary>
    /// Draws corner widgets of a world aligned boundingbox. 
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="size">Size (in model units) of the corner widgets.</param>
    public void DrawBoxCorners(BoundingBox box, Rhino.Drawing.Color color, double size)
    {
      DrawBoxCorners(box, color, size, 1);
    }
    /// <summary>
    /// Draws corner widgets of a world aligned boundingbox. 
    /// </summary>
    /// <param name="box">Box to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="size">Size (in model units) of the corner widgets.</param>
    /// <param name="thickness">Thickness (in pixels) of the corner widgets.</param>
    public void DrawBoxCorners(BoundingBox box, Rhino.Drawing.Color color, double size, int thickness)
    {
      if (!box.IsValid) { return; }

      // Size of box in all directions.
      double dx = box.m_max.m_x - box.m_min.m_x;
      double dy = box.m_max.m_y - box.m_min.m_y;
      double dz = box.m_max.m_z - box.m_min.m_z;

      // Singleton flags for all directions.
      bool fx = dx < 1e-6;
      bool fy = dy < 1e-6;
      bool fz = dz < 1e-6;

      // Singular box, don't draw.
      if (fx && fy && fz) { return; }

      // Linear box, don't draw.
      if (fx && fy) { return; }
      if (fx && fz) { return; }
      if (fy && fz) { return; }

      Point3d[] c = box.GetCorners();

      // Draw edges parallel to world Xaxis.
      if (dx > 1e-6)
      {
        if ((2.0 * size) >= dx)
        {
          // Draw single connecting wires.
          DrawLine(c[0], c[1], color, thickness);
          DrawLine(c[3], c[2], color, thickness);
          DrawLine(c[4], c[5], color, thickness);
          DrawLine(c[7], c[6], color, thickness);
        }
        else
        {
          // Draw corner widgets.
          DrawLine(c[0], new Point3d(c[0].m_x + size, c[0].m_y, c[0].m_z), color, thickness);
          DrawLine(c[3], new Point3d(c[3].m_x + size, c[3].m_y, c[3].m_z), color, thickness);
          DrawLine(c[4], new Point3d(c[4].m_x + size, c[4].m_y, c[4].m_z), color, thickness);
          DrawLine(c[7], new Point3d(c[7].m_x + size, c[7].m_y, c[7].m_z), color, thickness);
          DrawLine(c[1], new Point3d(c[1].m_x - size, c[1].m_y, c[1].m_z), color, thickness);
          DrawLine(c[2], new Point3d(c[2].m_x - size, c[2].m_y, c[2].m_z), color, thickness);
          DrawLine(c[5], new Point3d(c[5].m_x - size, c[5].m_y, c[5].m_z), color, thickness);
          DrawLine(c[6], new Point3d(c[6].m_x - size, c[6].m_y, c[6].m_z), color, thickness);
        }
      }

      // Draw edges parallel to world Yaxis.
      if (dy > 1e-6)
      {
        if ((2.0 * size) >= dy)
        {
          // Draw single connecting wires.
          DrawLine(c[0], c[3], color, thickness);
          DrawLine(c[1], c[2], color, thickness);
          DrawLine(c[4], c[7], color, thickness);
          DrawLine(c[5], c[6], color, thickness);
        }
        else
        {
          // Draw corner widgets.
          DrawLine(c[0], new Point3d(c[0].m_x, c[0].m_y + size, c[0].m_z), color, thickness);
          DrawLine(c[1], new Point3d(c[1].m_x, c[1].m_y + size, c[1].m_z), color, thickness);
          DrawLine(c[4], new Point3d(c[4].m_x, c[4].m_y + size, c[4].m_z), color, thickness);
          DrawLine(c[5], new Point3d(c[5].m_x, c[5].m_y + size, c[5].m_z), color, thickness);
          DrawLine(c[2], new Point3d(c[2].m_x, c[2].m_y - size, c[2].m_z), color, thickness);
          DrawLine(c[3], new Point3d(c[3].m_x, c[3].m_y - size, c[3].m_z), color, thickness);
          DrawLine(c[6], new Point3d(c[6].m_x, c[6].m_y - size, c[6].m_z), color, thickness);
          DrawLine(c[7], new Point3d(c[7].m_x, c[7].m_y - size, c[7].m_z), color, thickness);
        }
      }

      // Draw edges parallel to world Zaxis.
      if (dz > 1e-6)
      {
        if ((2.0 * size) >= dz)
        {
          // Draw single connecting wires.
          DrawLine(c[0], c[4], color, thickness);
          DrawLine(c[1], c[5], color, thickness);
          DrawLine(c[2], c[6], color, thickness);
          DrawLine(c[3], c[7], color, thickness);
        }
        else
        {
          // Draw corner widgets.
          DrawLine(c[0], new Point3d(c[0].m_x, c[0].m_y, c[0].m_z + size), color, thickness);
          DrawLine(c[1], new Point3d(c[1].m_x, c[1].m_y, c[1].m_z + size), color, thickness);
          DrawLine(c[2], new Point3d(c[2].m_x, c[2].m_y, c[2].m_z + size), color, thickness);
          DrawLine(c[3], new Point3d(c[3].m_x, c[3].m_y, c[3].m_z + size), color, thickness);
          DrawLine(c[4], new Point3d(c[4].m_x, c[4].m_y, c[4].m_z - size), color, thickness);
          DrawLine(c[5], new Point3d(c[5].m_x, c[5].m_y, c[5].m_z - size), color, thickness);
          DrawLine(c[6], new Point3d(c[6].m_x, c[6].m_y, c[6].m_z - size), color, thickness);
          DrawLine(c[7], new Point3d(c[7].m_x, c[7].m_y, c[7].m_z - size), color, thickness);
        }
      }
    }

    /// <summary>
    /// Draws an arrow marker as a view-aligned widget.
    /// </summary>
    /// <param name="tip">Location of arrow tip point.</param>
    /// <param name="direction">Direction of arrow.</param>
    /// <param name="color">Color of arrow widget.</param>
    public void DrawMarker(Point3d tip, Vector3d direction, Rhino.Drawing.Color color)
    {
      DrawMarker(tip, direction, color, 3, 16, 0.0);
    }
    /// <summary>
    /// Draws an arrow marker as a view-aligned widget.
    /// </summary>
    /// <param name="tip">Location of arrow tip point.</param>
    /// <param name="direction">Direction of arrow.</param>
    /// <param name="color">Color of arrow widget.</param>
    /// <param name="thickness">Thickness of arrow widget lines.</param>
    public void DrawMarker(Point3d tip, Vector3d direction, Rhino.Drawing.Color color, int thickness)
    {
      DrawMarker(tip, direction, color, thickness, 16.0, 0.0);
    }
    /// <summary>
    /// Draws an arrow marker as a view-aligned widget.
    /// </summary>
    /// <param name="tip">Location of arrow tip point.</param>
    /// <param name="direction">Direction of arrow.</param>
    /// <param name="color">Color of arrow widget.</param>
    /// <param name="thickness">Thickness of arrow widget lines.</param>
    /// <param name="size">Size (in pixels) of the arrow shaft.</param>
    public void DrawMarker(Point3d tip, Vector3d direction, Rhino.Drawing.Color color, int thickness, double size)
    {
      DrawMarker(tip, direction, color, thickness, size, 0.0);
    }
    /// <summary>
    /// Draws an arrow marker as a view-aligned widget.
    /// </summary>
    /// <param name="tip">Location of arrow tip point.</param>
    /// <param name="direction">Direction of arrow.</param>
    /// <param name="color">Color of arrow widget.</param>
    /// <param name="thickness">Thickness of arrow widget lines.</param>
    /// <param name="size">Size (in pixels) of the arrow shaft.</param>
    /// <param name="rotation">Rotational angle adjustment (in radians, counter-clockwise of direction.</param>
    public void DrawMarker(Point3d tip, Vector3d direction, Rhino.Drawing.Color color, int thickness, double size, double rotation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawMarker(pThis, tip, direction, color.ToArgb(), thickness, size, rotation);
    }

    /*
      public void DrawConstructionPlane( bool depthBuffered, int transparency )
      {
      }
    */
    public void DrawConstructionPlane(Rhino.DocObjects.ConstructionPlane constructionPlane)
    {
      int[] colors = constructionPlane.ArgbColors();
      int boolFlags = 0;
      if (constructionPlane.m_bDepthBuffered)
        boolFlags = 1;
      if (constructionPlane.m_bShowGrid)
        boolFlags |= 2;
      if (constructionPlane.m_bShowAxes)
        boolFlags |= 4;
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawConstructionPlane(m_ptr, ref constructionPlane.m_plane,
        constructionPlane.m_grid_spacing, constructionPlane.m_grid_line_count, constructionPlane.m_grid_thick_frequency,
        boolFlags, ref colors[0]);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">the string to draw.</param>
    /// <param name="color">text color.</param>
    /// <param name="screenCoordinate">definition point in screen coordinates (0,0 is top-left corner)</param>
    /// <param name="middleJustified">if true text is centered around the definition point, otherwise it is lower-left justified.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_drawstring.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_drawstring.cs' lang='cs'/>
    /// <code source='examples\py\ex_drawstring.py' lang='py'/>
    /// </example>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point2d screenCoordinate, bool middleJustified)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText(pThis, text.Length, text, color.ToArgb(), screenCoordinate, middleJustified, 12, null);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">the string to draw.</param>
    /// <param name="color">text color.</param>
    /// <param name="screenCoordinate">definition point in screen coordinates (0,0 is top-left corner)</param>
    /// <param name="middleJustified">if true text is centered around the definition point, otherwise it is lower-left justified.</param>
    /// <param name="height">height in pixels (good default is 12)</param>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point2d screenCoordinate, bool middleJustified, int height)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText(pThis, text.Length, text, color.ToArgb(), screenCoordinate, middleJustified, height, null);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">the string to draw.</param>
    /// <param name="color">text color.</param>
    /// <param name="screenCoordinate">definition point in screen coordinates (0,0 is top-left corner)</param>
    /// <param name="middleJustified">if true text is centered around the definition point, otherwise it is lower-left justified.</param>
    /// <param name="height">height in pixels (good default is 12)</param>
    /// <param name="fontface">font name (good default is "Arial")</param>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point2d screenCoordinate, bool middleJustified, int height, string fontface)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText(pThis, text.Length, text, color.ToArgb(), screenCoordinate, middleJustified, height, fontface);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">the string to draw.</param>
    /// <param name="color">text color.</param>
    /// <param name="worldCoordinate">definition point in world coordinates.</param>
    /// <param name="middleJustified">if true text is centered around the definition point, otherwise it is lower-left justified.</param>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point3d worldCoordinate, bool middleJustified)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText2(pThis, text.Length, text, color.ToArgb(), worldCoordinate, middleJustified, 12, null);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">the string to draw.</param>
    /// <param name="color">text color.</param>
    /// <param name="worldCoordinate">definition point in world coordinates.</param>
    /// <param name="middleJustified">if true text is centered around the definition point, otherwise it is lower-left justified.</param>
    /// <param name="height">height in pixels (good default is 12)</param>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point3d worldCoordinate, bool middleJustified, int height)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText2(pThis, text.Length, text, color.ToArgb(), worldCoordinate, middleJustified, height, null);
    }

    /// <summary>
    /// Draws 2D text on the viewport.
    /// </summary>
    /// <param name="text">The string to draw.</param>
    /// <param name="color">Text color.</param>
    /// <param name="worldCoordinate">Definition point in world coordinates.</param>
    /// <param name="middleJustified">If true text is centered around the definition point, otherwise it is lower-left justified.</param>
    /// <param name="height">Height in pixels (good default is 12).</param>
    /// <param name="fontface">Font name (good default is "Arial").</param>
    public void Draw2dText(string text, Rhino.Drawing.Color color, Point3d worldCoordinate, bool middleJustified, int height, string fontface)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw2dText2(pThis, text.Length, text, color.ToArgb(), worldCoordinate, middleJustified, height, fontface);
    }

    public void Draw3dText(string text, Rhino.Drawing.Color color, Plane textPlane, double height, string fontface)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw3dText(pThis, text, color.ToArgb(), ref textPlane, height, fontface);
    }

    public void Draw3dText(Text3d text, Rhino.Drawing.Color color)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pAnnotationText = text.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw3dText2(pThis, pAnnotationText, text.FontFace, color.ToArgb(), text.Bold, text.Italic);
    }

    /// <summary>
    /// Draws 3d text with a different plane than what is defined in the Text3d class.
    /// </summary>
    /// <param name="text">The string to draw.</param>
    /// <param name="color">Text color.</param>
    /// <param name="textPlane">The plane for the text object.</param>
    public void Draw3dText(Text3d text, Rhino.Drawing.Color color, Plane textPlane)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pAnnotationText = text.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw3dText3(pThis, pAnnotationText, text.FontFace, color.ToArgb(), text.Bold, text.Italic, ref textPlane);
    }

    /// <summary>
    /// Draws 3d text using the Text3d plane with an adjusted origin.
    /// </summary>
    /// <param name="text">The string to draw.</param>
    /// <param name="color">Text color.</param>
    /// <param name="textPlaneOrigin">The origin of the plane to draw.</param>
    public void Draw3dText(Text3d text, Rhino.Drawing.Color color, Point3d textPlaneOrigin)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pAnnotationText = text.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_Draw3dText4(pThis, pAnnotationText, text.FontFace, color.ToArgb(), text.Bold, text.Italic, textPlaneOrigin);
    }

    /*

    /// <summary>
    /// Determines screen rectangle that would be drawn to using the DrawString(..) function
    /// with the same parameters.
    /// </summary>
    /// <param name="measuredRectangle">rectangle in the viewport's screen coordinates on success.</param>
    /// <param name="text">text to measure.</param>
    /// <param name="definitionPoint">either lower-left or middle of text.</param>
    /// <param name="middleJustified">true=middle justified. false=lower-left justified.</param>
    /// <param name="rotation">text rotation in 1/10 degrees.</param>
    /// <param name="height">height in pixels (good default is 12)</param>
    /// <param name="fontface">font name (good default is "Arial")</param>
    /// <returns>true on success, false on failure.</returns>
    public bool MeasureString( out Rhino.Drawing.Rectangle measuredRectangle, string text, ON_2dPoint definitionPoint, bool middleJustified, int rotation, int height, string fontFace )
    {
    }

    public void DrawTriangle( ON_3dPoint p0, ON_3dPoint p1, ON_3dPoint p2, Rhino.Drawing.Color color )
    {
    }

    */

    public void DrawObject(DocObjects.RhinoObject rhinoObject)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pRhinoObject = rhinoObject.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawObject(pThis, pRhinoObject); 
    }

    /// <summary>
    /// Draws a <see cref="DocObjects.RhinoObject">RhinoObject</see> with an applied transformation.
    /// </summary>
    /// <param name="rhinoObject">The Rhino object.</param>
    /// <param name="xform">The transformation.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_arraybydistance.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_arraybydistance.cs' lang='cs'/>
    /// <code source='examples\py\ex_arraybydistance.py' lang='py'/>
    /// </example>
    public void DrawObject(DocObjects.RhinoObject rhinoObject, Transform xform)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pRhinoObject = rhinoObject.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawObject2(pThis, pRhinoObject, ref xform);
    }

    /*
    public void DrawObjects(System.Collections.Generic.IEnumerable<DocObjects.RhinoObject> rhinoObjects)
    {
    }

    public void DrawObjects(System.Collections.Generic.IEnumerable<DocObjects.RhinoObject> rhinoObjects, Transform xform)
    {
    }

    public void DrawSubObject(DocObjects.RhinoObject rhinoObject, ON_ComponentIndex componentIndex)
    {
    }
    public void DrawSubObject(DocObjects.RhinoObject rhinoObject, ON_ComponentIndex componentIndex, ON_Xform xform)
    {
    }
    */
    #endregion

    #region CRhinoViewport draw functions
    /// <summary>
    /// Draw a single arc object.
    /// </summary>
    /// <param name="arc">Arc to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawArc(Arc arc, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArc(m_ptr, ref arc, argb, 1);
    }
    /// <summary>
    /// Draw a single arc object.
    /// </summary>
    /// <param name="arc">Arc to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of arc.</param>
    public void DrawArc(Arc arc, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawArc(m_ptr, ref arc, argb, thickness);
    }
    /// <summary>
    /// Draw a single circle object.
    /// </summary>
    /// <param name="circle">Circle to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_getpointdynamicdraw.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_getpointdynamicdraw.cs' lang='cs'/>
    /// <code source='examples\py\ex_getpointdynamicdraw.py' lang='py'/>
    /// </example>
    public void DrawCircle(Circle circle, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCircle(m_ptr, ref circle, argb, 1);
    }
    /// <summary>
    /// Draw a single circle object.
    /// </summary>
    /// <param name="circle">Circle to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of circle.</param>
    public void DrawCircle(Circle circle, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCircle(m_ptr, ref circle, argb, thickness);
    }
    /// <summary>
    /// Draw a wireframe sphere.
    /// </summary>
    /// <param name="sphere">Sphere to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawSphere(Sphere sphere, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawSphere(m_ptr, ref sphere, argb, 1);
    }
    /// <summary>
    /// Draw a wireframe sphere.
    /// </summary>
    /// <param name="sphere">Sphere to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of Sphere wires.</param>
    public void DrawSphere(Sphere sphere, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawSphere(m_ptr, ref sphere, argb, thickness);
    }
    /// <summary>
    /// Draw a wireframe torus.
    /// </summary>
    /// <param name="torus">Torus to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawTorus(Torus torus, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawTorus(m_ptr, ref torus, argb, 1);
    }
    /// <summary>
    /// Draw a wireframe torus.
    /// </summary>
    /// <param name="torus">Torus to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of torus wires.</param>
    public void DrawTorus(Torus torus, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawTorus(m_ptr, ref torus, argb, thickness);
    }
    /// <summary>
    /// Draw a wireframe cylinder.
    /// </summary>
    /// <param name="cylinder">Cylinder to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawCylinder(Cylinder cylinder, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCylinder(m_ptr, ref cylinder, argb, 1);
    }
    /// <summary>
    /// Draw a wireframe cylinder.
    /// </summary>
    /// <param name="cylinder">Cylinder to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of cylinder wires.</param>
    public void DrawCylinder(Cylinder cylinder, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCylinder(m_ptr, ref cylinder, argb, thickness);
    }
    /// <summary>
    /// Draw a wireframe cone.
    /// </summary>
    /// <param name="cone">Cone to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawCone(Cone cone, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCone(m_ptr, ref cone, argb, 1);
    }
    /// <summary>
    /// Draw a wireframe cone.
    /// </summary>
    /// <param name="cone">Cone to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of Cone wires.</param>
    public void DrawCone(Cone cone, Rhino.Drawing.Color color, int thickness)
    {
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCone(m_ptr, ref cone, argb, thickness);
    }
    /// <summary>
    /// Draw a single Curve object.
    /// </summary>
    /// <param name="curve">Curve to draw.</param>
    /// <param name="color">Color to draw with.</param>
    public void DrawCurve(Geometry.Curve curve, Rhino.Drawing.Color color)
    {
      curve.Draw(this, color, 1);
    }
    /// <summary>
    /// Draw a single Curve object.
    /// </summary>
    /// <param name="curve">Curve to draw.</param>
    /// <param name="color">Color to draw with.</param>
    /// <param name="thickness">Thickness (in pixels) of curve.</param>
    public void DrawCurve(Geometry.Curve curve, Rhino.Drawing.Color color, int thickness)
    {
      curve.Draw(this, color, thickness);
    }

    /// <summary>
    /// Draw a typical Rhino Curvature Graph.
    /// </summary>
    /// <param name="curve">Base curve for curvature graph.</param>
    /// <param name="color">Color of curvature graph.</param>
    public void DrawCurvatureGraph(Geometry.Curve curve, Rhino.Drawing.Color color)
    {
      int argb = color.ToArgb();
      IntPtr pCurve = curve.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCurvatureGraph(m_ptr, pCurve, argb, 100, 1, 2);
    }
    /// <summary>
    /// Draw a typical Rhino Curvature Graph.
    /// </summary>
    /// <param name="curve">Base curve for curvature graph.</param>
    /// <param name="color">Color of curvature graph.</param>
    /// <param name="hairScale">100 = true length, &gt; 100 magnified, &lt; 100 shortened.</param>
    public void DrawCurvatureGraph(Geometry.Curve curve, Rhino.Drawing.Color color, int hairScale)
    {
      int argb = color.ToArgb();
      IntPtr pCurve = curve.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCurvatureGraph(m_ptr, pCurve, argb, hairScale, 1, 2);
    }
    /// <summary>
    /// Draw a typical Rhino Curvature Graph.
    /// </summary>
    /// <param name="curve">Base curve for curvature graph.</param>
    /// <param name="color">Color of curvature graph.</param>
    /// <param name="hairScale">100 = true length, &gt; 100 magnified, &lt; 100 shortened.</param>
    /// <param name="hairDensity">&gt;= 0 larger numbers = more hairs (good default is 1).</param>
    /// <param name="sampleDensity">Between 1 and 10. Higher numbers draw smoother outer curves. (good default is 2).</param>
    public void DrawCurvatureGraph(Geometry.Curve curve, Rhino.Drawing.Color color, int hairScale, int hairDensity, int sampleDensity)
    {
      int argb = color.ToArgb();
      IntPtr pCurve = curve.ConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawCurvatureGraph(m_ptr, pCurve, argb, hairScale, hairDensity, sampleDensity);
    }

    /// <summary>
    /// Draw wireframe display for a single surface.
    /// </summary>
    /// <param name="surface">Surface to draw.</param>
    /// <param name="wireColor">Color to draw with.</param>
    /// <param name="wireDensity">Thickness (in pixels) or wires to draw.</param>
    public void DrawSurface(Geometry.Surface surface, Rhino.Drawing.Color wireColor, int wireDensity)
    {
      surface.Draw(this, wireColor, wireDensity);
    }

    #endregion

    public void DrawSprite(DisplayBitmap bitmap, Point3d worldLocation, float size, bool sizeInWorldSpace)
    {
      DrawSprite(bitmap, worldLocation, size, Rhino.Drawing.Color.White, sizeInWorldSpace);
    }

    public void DrawSprite(DisplayBitmap bitmap, Point3d worldLocation, float size, Rhino.Drawing.Color blendColor, bool sizeInWorldSpace)
    {
      IntPtr pBitmap = bitmap.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBitmap(m_ptr, pBitmap, worldLocation, size, blendColor.ToArgb(), sizeInWorldSpace);
    }

    public void DrawSprite(DisplayBitmap bitmap, Point2d screenLocation, float size)
    {
      DrawSprite(bitmap, screenLocation, size, Rhino.Drawing.Color.White);
    }
    public void DrawSprite(DisplayBitmap bitmap, Point2d screenLocation, float size, Rhino.Drawing.Color blendColor)
    {
      IntPtr pBitmap = bitmap.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBitmap2(m_ptr, pBitmap, screenLocation, size, blendColor.ToArgb());
    }

    public void DrawSprites(DisplayBitmap bitmap, DisplayBitmapDrawList items, float size, bool sizeInWorldSpace)
    {
      DrawSprites(bitmap, items, size, Vector3d.Zero, sizeInWorldSpace);
    }

    public void DrawSprites(DisplayBitmap bitmap, DisplayBitmapDrawList items, float size, Vector3d translation, bool sizeInWorldSpace)
    {
      Vector3d camera_direction = new Vector3d();
      UnsafeNativeMethods.CRhinoDisplayPipeline_GetCameraDirection(m_ptr, ref camera_direction);
      int[] indices = items.Sort(camera_direction);
      IntPtr pBitmap = bitmap.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawBitmaps(m_ptr, pBitmap, items.m_points.Length, items.m_points, items.m_colors_argb.Length, items.m_colors_argb, indices, size, translation, sizeInWorldSpace);
    }

    public void DrawParticles(Rhino.Geometry.ParticleSystem particles)
    {
      particles.UpdateDrawCache();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawParticles1(m_ptr, IntPtr.Zero, particles.m_points.Length, particles.m_points, particles.m_sizes, particles.m_colors_argb, particles.DisplaySizesInWorldUnits);
    }

    public void DrawParticles(Rhino.Geometry.ParticleSystem particles, DisplayBitmap bitmap)
    {
      particles.UpdateDrawCache();
      IntPtr pBitmap = bitmap.NonConstPointer();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawParticles1(m_ptr, pBitmap, particles.m_points.Length, particles.m_points, particles.m_sizes, particles.m_colors_argb, particles.DisplaySizesInWorldUnits);
    }

    public void DrawParticles(Rhino.Geometry.ParticleSystem particles, DisplayBitmap[] bitmaps)
    {
      particles.UpdateDrawCache();
      uint[] ids = new uint[bitmaps.Length];
      for (int i = 0; i < bitmaps.Length; i++)
        ids[i] = UnsafeNativeMethods.CRhCmnDisplayBitmap_TextureId(m_ptr, bitmaps[i].NonConstPointer());

      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawParticles2(m_ptr, ids.Length, ids, particles.m_points.Length, particles.m_points, particles.m_sizes, particles.m_colors_argb, particles.m_display_bitmap_ids, particles.DisplaySizesInWorldUnits);
    }

    /*
    //public void Draw2dRectangle( Rhino.Drawing.Rectangle rectangle, HPEN pen, bool=true);
    //public void Draw2dLine(const CPoint&, const CPoint&, HPEN, bool=true);
  
    public void FillSolidRect( Rhino.Drawing.Rectangle screenRectangle, Rhino.Drawing.Color color, int transparency)
    {
    }
    */
  }

  public class DrawEventArgs : System.EventArgs
  {
    internal IntPtr m_pDisplayPipeline;
    internal readonly IntPtr m_pDisplayConduit;
    internal RhinoViewport m_viewport;
    DisplayPipeline m_dp;
    internal DrawEventArgs(IntPtr pDisplayPipeline, IntPtr pDisplayConduit)
    {
      m_pDisplayPipeline = pDisplayPipeline;
      m_pDisplayConduit = pDisplayConduit;
    }
    internal DrawEventArgs() { }

    public RhinoViewport Viewport
    {
      get
      {
        if (null == m_viewport)
        {
          IntPtr pViewport = UnsafeNativeMethods.CRhinoDisplayPipeline_RhinoViewport(m_pDisplayPipeline);
          if (IntPtr.Zero != pViewport)
            m_viewport = new RhinoViewport(null, pViewport);
        }
        return m_viewport;
      }
    }

    public DisplayPipeline Display
    {
      get { return m_dp ?? (m_dp=new DisplayPipeline(m_pDisplayPipeline)); }
    }

    RhinoDoc m_doc;
    public RhinoDoc RhinoDoc
    {
      get
      {
        if (m_doc == null)
        {
          IntPtr pRhinoDoc = UnsafeNativeMethods.CRhinoDisplayPipeline_RhinoDoc(m_pDisplayPipeline);
          m_doc = RhinoDoc.FromIntPtr(pRhinoDoc);
        }
        return m_doc;
      }
    }


    internal const int idxDrawObject = 0;
    internal const int idxWorldAxesDrawn = 1;
    internal const int idxDrawWorldAxes = 2;

    internal bool GetChannelAttributeBool(int which)
    {
      return UnsafeNativeMethods.CChannelAttributes_GetBool(m_pDisplayConduit, which);
    }
    internal void SetChannelAttributeBool(int which, bool value)
    {
      UnsafeNativeMethods.CChannelAttributes_SetBool(m_pDisplayConduit, which, value);
    }
  }

  public class DrawForegroundEventArgs : DrawEventArgs
  {
    internal DrawForegroundEventArgs(IntPtr pDisplayPipeline, IntPtr pDisplayConduit)
      : base(pDisplayPipeline, pDisplayConduit)
    {
    }
    public bool WorldAxesDrawn
    {
      get { return GetChannelAttributeBool(idxWorldAxesDrawn); }
      set { SetChannelAttributeBool(idxWorldAxesDrawn, value); }
    }
    public bool DrawWorldAxes
    {
      get { return GetChannelAttributeBool(idxDrawWorldAxes); }
      set { SetChannelAttributeBool(idxDrawWorldAxes, value); }
    }
  }

  public class CullObjectEventArgs : DrawEventArgs
  {
    internal CullObjectEventArgs(IntPtr pDisplayPipeline, IntPtr pDisplayConduit)
      : base(pDisplayPipeline, pDisplayConduit)
    {
    }

    Rhino.DocObjects.RhinoObject m_rhino_object;
    public Rhino.DocObjects.RhinoObject RhinoObject
    {
      get
      {
        if (m_rhino_object == null)
        {
          IntPtr pRhinoObject = UnsafeNativeMethods.CChannelAttributes_RhinoObject(m_pDisplayConduit);
          m_rhino_object = Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
        }
        return m_rhino_object;
      }
    }

    public bool CullObject
    {
      get { return !GetChannelAttributeBool(idxDrawObject); }
      set { SetChannelAttributeBool(idxDrawObject, !value); }
    }
  }


  public class DrawObjectEventArgs : DrawEventArgs
  {
    internal DrawObjectEventArgs(IntPtr pDisplayPipeline, IntPtr pDisplayConduit)
      : base(pDisplayPipeline, pDisplayConduit)
    {
    }

    Rhino.DocObjects.RhinoObject m_rhino_object;
    public Rhino.DocObjects.RhinoObject RhinoObject
    {
      get
      {
        if (m_rhino_object == null)
        {
          IntPtr pRhinoObject = UnsafeNativeMethods.CChannelAttributes_RhinoObject(m_pDisplayConduit);
          m_rhino_object = Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
        }
        return m_rhino_object;
      }
    }

    public bool DrawObject
    {
      get { return GetChannelAttributeBool(idxDrawObject); }
      set { SetChannelAttributeBool(idxDrawObject, value); }
    }
  }

  public class CalculateBoundingBoxEventArgs : DrawEventArgs
  {
    private BoundingBox m_bbox;

    internal CalculateBoundingBoxEventArgs(IntPtr pDisplayPipeline, IntPtr pDisplayConduit)
      : base(pDisplayPipeline, pDisplayConduit)
    {
      UnsafeNativeMethods.CChannelAttr_GetSetBBox(m_pDisplayConduit, false, ref m_bbox);
    }

    /// <summary>
    /// Gets the current bounding box.
    /// </summary>
    public BoundingBox BoundingBox
    {
      get { return m_bbox; }
    }

    /// <summary>
    /// Unites a bounding box with the current display bounding box in order to ensure
    /// dynamic objects in "box" are drawn.
    /// </summary>
    /// <param name="box">The box to unite.</param>
    public void IncludeBoundingBox(BoundingBox box)
    {
      m_bbox.Union(box);
      UnsafeNativeMethods.CChannelAttr_GetSetBBox(m_pDisplayConduit, true, ref m_bbox);
    }
  }

  /// <summary>
  /// Provides functionality for getting the zbuffer values from a viewport
  /// and a given display mode
  /// </summary>
  public class ZBufferCapture : IDisposable
  {
    IntPtr m_ptr; //CRhinoZBuffer*
    public ZBufferCapture(RhinoViewport viewport)
    {
      IntPtr pViewport = IntPtr.Zero;
      if( viewport!=null )
        pViewport = viewport.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoZBuffer_New(pViewport);
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~ZBufferCapture() { Dispose(false); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.CRhinoZBuffer_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }

    IntPtr NonConstPointer(bool destroyCache)
    {
      if (destroyCache && m_bitmap != null)
        m_bitmap = null;

      return m_ptr;
    }

    public void SetDisplayMode(Guid modeId)
    {
      IntPtr pThis = NonConstPointer(true);
      UnsafeNativeMethods.CRhinoZBuffer_SetDisplayMode(pThis, modeId);
    }

    void SetBool(int which, bool on)
    {
      IntPtr pThis = NonConstPointer(true);
      UnsafeNativeMethods.CRhinoZBuffer_SetBool(pThis, which, on);
    }

    const int IDX_SHOW_ISOCURVES = 0;
    const int IDX_SHOW_MESH_WIRES = 1;
    const int IDX_SHOW_CURVES = 2;
    const int IDX_SHOW_POINTS = 3;
    const int IDX_SHOW_TEXT = 4;
    const int IDX_SHOW_ANNOTATIONS = 5;
    const int IDX_SHOW_LIGHTS = 6;

    public void ShowIsocurves(bool on) { SetBool(IDX_SHOW_ISOCURVES, on); }
    public void ShowMeshWires(bool on) { SetBool(IDX_SHOW_MESH_WIRES, on); }
    public void ShowCurves(bool on) { SetBool(IDX_SHOW_CURVES, on); }
    public void ShowPoints(bool on) { SetBool(IDX_SHOW_POINTS, on); }
    public void ShowText(bool on) { SetBool(IDX_SHOW_TEXT, on); }
    public void ShowAnnotations(bool on) { SetBool(IDX_SHOW_ANNOTATIONS, on); }
    public void ShowLights(bool on) { SetBool(IDX_SHOW_LIGHTS, on); }

    public int HitCount()
    {
      IntPtr pThis = NonConstPointer(false);
      return UnsafeNativeMethods.CRhinoZBuffer_HitCount(pThis);
    }
    public float MaxZ()
    {
      IntPtr pThis = NonConstPointer(false);
      return UnsafeNativeMethods.CRhinoZBuffer_MaxZ(pThis);
    }
    public float MinZ()
    {
      IntPtr pThis = NonConstPointer(false);
      return UnsafeNativeMethods.CRhinoZBuffer_MinZ(pThis);
    }
    public float ZValueAt(int x, int y)
    {
      IntPtr pThis = NonConstPointer(false);
      return UnsafeNativeMethods.CRhinoZBuffer_ZValue(pThis, x, y);
    }
    public Point3d WorldPointAt(int x, int y)
    {
      IntPtr pThis = NonConstPointer(false);
      Point3d rc = new Point3d();
      UnsafeNativeMethods.CRhinoZBuffer_WorldPoint(pThis, x, y, ref rc);
      return rc;
    }

    Rhino.Drawing.Bitmap m_bitmap;
    public Rhino.Drawing.Bitmap GrayscaleDib()
    {
      if (m_bitmap == null)
      {
        IntPtr pThis = NonConstPointer(false);
        IntPtr hBitmap = UnsafeNativeMethods.CRhinoZBuffer_GrayscaleDib(pThis);
        m_bitmap = Rhino.Drawing.Image.FromHbitmap(hBitmap);
      }
      return m_bitmap;
    }

  }
}
#endif