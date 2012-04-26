#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.Input.Custom
{
  /// <summary>
  /// Provides picking values that describe common CAD picking behavior.
  /// </summary>
  public enum PickStyle : int
  {
    None = 0,
    PointPick = 1,
    WindowPick = 2,
    CrossingPick = 3
  }

  /// <summary>
  /// Picking can happen in wireframe or shaded display mode
  /// </summary>
  public enum PickMode : int
  {
    Wireframe = 1,
    Shaded = 2
  }

  /// <summary>
  /// Provides storage for picking operations.
  /// </summary>
  public class PickContext : IDisposable
  {
    public PickContext()
    {
      m_pRhinoPickContext = UnsafeNativeMethods.CRhinoPickContext_New();
      m_is_const = false;
    }

    internal PickContext(IntPtr pConstRhinoPickContext)
    {
      m_is_const = true;
      m_pRhinoPickContext = pConstRhinoPickContext;
      GC.SuppressFinalize(this);
    }

    #region IDisposable/Pointer handling
    readonly bool m_is_const;
    IntPtr m_pRhinoPickContext; // CRhinoPickContext*
    internal IntPtr ConstPointer() { return m_pRhinoPickContext; }
    internal IntPtr NonConstPointer()
    {
      if (m_is_const)
        return IntPtr.Zero;
      return m_pRhinoPickContext;
    }

    ~PickContext()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_pRhinoPickContext != IntPtr.Zero && !m_is_const)
      {
        UnsafeNativeMethods.CRhinoPickContext_Delete(m_pRhinoPickContext);
      }
      m_pRhinoPickContext = IntPtr.Zero;
    }
    #endregion

    /// <summary>
    /// This view can be a model view or a page view. When view is a page view,
    /// then you need to distingish between the viewports MainViewport() and
    /// ActiveViewport().  When m_view is a model view, both MainViewport() and
    /// ActiveViewport() return the world view's viewport.
    /// </summary>
    public Rhino.Display.RhinoView View
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pView = UnsafeNativeMethods.CRhinoPickContext_GetView(pConstThis);
        return Rhino.Display.RhinoView.FromIntPtr(pView);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        IntPtr pView = value.NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetView(pThis, pView);
      }
    }

    /// <summary>
    /// pick chord starts on near clipping plane and ends on far clipping plane.
    /// </summary>
    public Rhino.Geometry.Line PickLine
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Rhino.Geometry.Line rc = new Geometry.Line();
        UnsafeNativeMethods.CRhinoPickContext_PickLine(pConstThis, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickLine(pThis, ref value);
      }
    }


    public PickStyle PickStyle
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (PickStyle)UnsafeNativeMethods.CRhinoPickContext_PickStyle(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickStyle(pThis, (int)value);
      }
    }

    public PickMode PickMode
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (PickMode)UnsafeNativeMethods.CRhinoPickContext_PickMode(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickMode(pThis, (int)value);
      }
    }

    /// <summary>
    /// Thue if GroupObjects should be added to the pick list
    /// </summary>
    public bool PickGroupsEnabled
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoPickContext_GetPickGroups(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickGroups(pThis, value);
      }
    }

    /// <summary>
    /// True if the user had activated subobject selection
    /// </summary>
    public bool SubObjectSelectionEnabled
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoPickContext_GetSubSelect(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetSubSelect(pThis, value);
      }
    }

    Rhino.Input.Custom.GetObject m_cached_get_object;
    public Rhino.Input.Custom.GetObject GetObjectUsed
    {
      get
      {
        if (m_cached_get_object == null)
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pConstRhinoGetObject = UnsafeNativeMethods.CRhinoPickContext_GetObject(pConstThis);
          if (pConstRhinoGetObject == IntPtr.Zero)
            return null;
          var active = Rhino.Input.Custom.GetObject.ActiveGetObject;
          if (active != null && active.ConstPointer() == pConstRhinoGetObject)
            m_cached_get_object = active;
          else
            m_cached_get_object = new GetObject(pConstRhinoGetObject);
        }
        return m_cached_get_object;
      }
    }

    public void SetPickTransform(Rhino.Geometry.Transform transform)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoPickContext_SetPickTransform(pThis, ref transform);
    }

    /// <summary>
    /// Updates the clipping plane information in pick region. The
    /// SetClippingPlanes and View fields must be called before calling
    /// UpdateClippingPlanes().
    /// </summary>
    public void UpdateClippingPlanes()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoPickContext_UpdateClippingPlanes(pThis);
    }

    /// <summary>
    /// Fast test to check if a bounding box intersects a pick frustum.
    /// </summary>
    /// <param name="box"></param>
    /// <param name="boxCompletelyInFrustum">
    /// Set to true if the box is completely contained in the pick frustum.
    /// When doing a window or crossing pick, you can immediately return a
    /// hit if the object's bounding box is completely inside of the pick frustum.
    /// </param>
    /// <returns>
    /// False if bbox is invalid or box does not intersect the pick frustum
    /// </returns>
    public bool PickFrustumTest(Rhino.Geometry.BoundingBox box, out bool boxCompletelyInFrustum)
    {
      boxCompletelyInFrustum = false;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickBox(pConstThis, ref box, ref boxCompletelyInFrustum);
    }

    /// <summary>Utility for picking 3d point</summary>
    /// <param name="point"></param>
    /// <param name="depth">
    /// depth returned here for point picks.
    /// LARGER values are NEARER to the camera.
    /// SMALLER values are FARTHER from the camera.
    /// </param>
    /// <param name="distance">
    /// planar distance returned here for point picks.
    /// SMALLER values are CLOSER to the pick point
    /// </param>
    /// <returns>true if there is a hit</returns>
    public bool PickFrustumTest(Rhino.Geometry.Point3d point, out double depth, out double distance)
    {
      depth = -1;
      distance = -1;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickPoint(pConstThis, point, ref depth, ref distance);
    }

    public bool PickFrustumTest(Rhino.Geometry.Point3d[] points, out int pointIndex, out double depth, out double distance)
    {
      pointIndex = -1;
      depth = -1;
      distance = -1;
      if (points == null || points.Length < 1)
        return false;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickPointCloud(pConstThis, points.Length, points, ref pointIndex, ref depth, ref distance);
    }

    public bool PickFrustumTest(Rhino.Geometry.PointCloud cloud, out int pointIndex, out double depth, out double distance)
    {
      pointIndex = -1;
      depth = -1;
      distance = -1;
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstCloud = cloud.ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickPointCloud2(pConstThis, pConstCloud, ref pointIndex, ref depth, ref distance);
    }

    public bool PickFrustumTest(Rhino.Geometry.Line line, out double t, out double depth, out double distance)
    {
      t = -1;
      depth = -1;
      distance = -1;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickLine2(pConstThis, line.From, line.To, ref t, ref depth, ref distance);
    }

    public bool PickFrustumTest(Rhino.Geometry.BezierCurve bezier, out double t, out double depth, out double distance)
    {
      t = -1;
      depth = -1;
      distance = -1;
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstBezier = bezier.ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickBezier(pConstThis, pConstBezier, ref t, ref depth, ref distance);
    }

    public bool PickFrustumTest(Rhino.Geometry.NurbsCurve curve, out double t, out double depth, out double distance)
    {
      t = -1;
      depth = -1;
      distance = -1;
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstCurve = curve.ConstPointer();
      return UnsafeNativeMethods.CRhinoPickContext_PickNurbsCurve(pConstThis, pConstCurve, ref t, ref depth, ref distance);
    }

    /// <summary>Utility for picking meshes</summary>
    /// <param name="mesh">mesh to test</param>
    /// <param name="pickStyle">mode used for pick test</param>
    /// <param name="hitPoint">location returned here for point picks</param>
    /// <param name="hitSurfaceUV">
    /// If the mesh has surface parameters, set to the surface parameters of the hit point
    /// </param>
    /// <param name="hitTextureCoordinate">
    /// If the mesh has texture coordinates, set to the texture coordinate of the hit
    /// point.  Note that the texture coodinates can be set in many different ways
    /// and this information is useless unless you know how the texture coordinates
    /// are set on this particular mesh.
    /// </param>
    /// <param name="depth">
    /// depth returned here for point picks
    /// LARGER values are NEARER to the camera.
    /// SMALLER values are FARTHER from the camera.
    /// </param>
    /// <param name="distance">
    /// planar distance returned here for point picks.
    /// SMALLER values are CLOSER to the pick point
    /// </param>
    /// <param name="hitFlag">
    /// For point picks, How to interpret the hitIndex (vertex hit, edge hit, or face hit)
    /// </param>
    /// <param name="hitIndex">
    /// index of vertex/edge/face that was hit. Use hitFlag to determine what this index
    /// corresponds to
    /// </param>
    /// <returns></returns>
    public bool PickFrustumTest(Rhino.Geometry.Mesh mesh, MeshPickStyle pickStyle, out Rhino.Geometry.Point3d hitPoint, out Rhino.Geometry.Point2d hitSurfaceUV, out Rhino.Geometry.Point2d hitTextureCoordinate,
      out double depth, out double distance, out MeshHitFlag hitFlag, out int hitIndex)
    {
      hitPoint = Rhino.Geometry.Point3d.Unset;
      hitSurfaceUV = Rhino.Geometry.Point2d.Unset;
      hitTextureCoordinate = Rhino.Geometry.Point2d.Unset;
      depth = -1;
      distance = -1;
      hitFlag = MeshHitFlag.Invalid;
      hitIndex = -1;
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstMesh = mesh.ConstPointer();
      int vef_flag = -1;
      bool rc = UnsafeNativeMethods.CRhinoPickContext_PickMesh(pConstThis, pConstMesh, (int)pickStyle, ref hitPoint, ref hitSurfaceUV, ref hitTextureCoordinate, ref depth, ref distance, ref vef_flag, ref hitIndex);
      hitFlag = (MeshHitFlag)vef_flag;
      return rc;
    }

    /// <summary>Utility for picking meshes</summary>
    /// <param name="mesh">mesh to test</param>
    /// <param name="pickStyle">mode used for pick test</param>
    /// <param name="hitPoint">location returned here for point picks</param>
    /// <param name="depth">
    /// depth returned here for point picks
    /// LARGER values are NEARER to the camera.
    /// SMALLER values are FARTHER from the camera.
    /// </param>
    /// <param name="distance">
    /// planar distance returned here for point picks.
    /// SMALLER values are CLOSER to the pick point
    /// </param>
    /// <param name="hitFlag">
    /// For point picks, How to interpret the hitIndex (vertex hit, edge hit, or face hit)
    /// </param>
    /// <param name="hitIndex">
    /// index of vertex/edge/face that was hit. Use hitFlag to determine what this index
    /// corresponds to
    /// </param>
    /// <returns></returns>
    public bool PickFrustumTest(Rhino.Geometry.Mesh mesh, MeshPickStyle pickStyle, out Rhino.Geometry.Point3d hitPoint, out double depth, out double distance, out MeshHitFlag hitFlag, out int hitIndex)
    {
      hitPoint = Rhino.Geometry.Point3d.Unset;
      depth = -1;
      distance = -1;
      hitFlag = MeshHitFlag.Invalid;
      hitIndex = -1;
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstMesh = mesh.ConstPointer();
      int vef_flag = -1;
      bool rc = UnsafeNativeMethods.CRhinoPickContext_PickMesh2(pConstThis, pConstMesh, (int)pickStyle, ref hitPoint, ref depth, ref distance, ref vef_flag, ref hitIndex);
      hitFlag = (MeshHitFlag)vef_flag;
      return rc;
    }

    /// <summary>
    /// Utility for picking mesh vertices
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns>indices of mesh topology vertices that were picked</returns>
    public int[] PickMeshTopologyVertices(Rhino.Geometry.Mesh mesh)
    {
      using (var indices = new Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pConstMesh = mesh.ConstPointer();
        IntPtr pIndices = indices.NonConstPointer();
        int rc = UnsafeNativeMethods.CRhinoPickContext_PickMeshTopologyVertices(pConstThis, pConstMesh, pIndices);
        if (rc < 1)
          return new int[0];
        return indices.ToArray();
      }
    }

    public enum MeshPickStyle : int
    {
      /// <summary>Checks for vertex and edge hits</summary>
      WireframePicking = 0,
      /// <summary>Checks for face hits</summary>
      ShadedModePicking = 1,
      /// <summary>Returns false if no vertices are hit</summary>
      VertexOnlyPicking = 2
    }

    public enum MeshHitFlag : int
    {
      Invalid = -1,
      Vertex = 0,
      Edge = 1,
      Face = 2
    }
  }
}
#endif