using System;
using Rhino.Geometry;


namespace Rhino.Display
{
  public enum DefinedViewportProjection : int
  {
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 3,
    Right = 4,
    Front = 5,
    Back = 6,
    Perspective = 7
  }

  public class RhinoViewport : IDisposable
  {
    readonly Rhino.DocObjects.DetailViewObject m_parent_detail;
    RhinoView m_parent_view;
    IntPtr m_ptr;
    bool m_bDeletePtr; // = false; initialized by runtime
    internal RhinoViewport(RhinoView parent_view, IntPtr ptr)
    {
      m_ptr = ptr;
      m_parent_view = parent_view;
    }

    internal RhinoViewport(Rhino.DocObjects.DetailViewObject detail)
    {
      m_parent_detail = detail;
    }

    internal void OnDetailCommit()
    {
      if (IntPtr.Zero != m_ptr && m_bDeletePtr)
      {
        UnsafeNativeMethods.CRhinoViewport_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
        m_bDeletePtr = false;
      }
    }

    ~RhinoViewport()
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
      if (IntPtr.Zero != m_ptr && m_bDeletePtr )
      {
        UnsafeNativeMethods.CRhinoViewport_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    internal IntPtr NonConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_parent_detail != null)
      {
        IntPtr pDetail = m_parent_detail.ConstPointer();
        m_ptr = UnsafeNativeMethods.CRhinoDetailViewObject_DuplicateViewport(pDetail);
        m_bDeletePtr = true;
      }
      return m_ptr;
    }

    internal IntPtr ConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_parent_detail != null)
      {
        IntPtr pDetail = m_parent_detail.ConstPointer();
        return UnsafeNativeMethods.CRhinoDetailViewObject_GetViewport(pDetail);
      }
      return m_ptr;
    }

    #region Wrappers for ON_Viewport

    // from ON_Geometry
    /// <summary>
    /// Rotates about the specified axis. A positive rotation angle results
    /// in a counter-clockwise rotation about the axis (right hand rule).
    /// </summary>
    /// <param name="angleRadians">angle of rotation in radians</param>
    /// <param name="rotationAxis">direction of the axis of rotation</param>
    /// <param name="rotationCenter">point on the axis of rotation</param>
    /// <returns>true if geometry successfully rotated</returns>
    public bool Rotate(double angleRadians, Vector3d rotationAxis, Point3d rotationCenter)
    {
      IntPtr pConstThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_Rotate(pConstThis, angleRadians, rotationAxis, rotationCenter);
    }


    const int idxIsValidCamera = 0;
    const int idxIsValidFrustum = 1;
    const int idxIsPerspectiveProjection = 2;
    const int idxIsTwoPointPerspectiveProjection = 3;
    const int idxIsParallelProjection = 4;
    public bool IsValidCamera
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsValidCamera);
      }
    }
    public bool IsValidFrustum
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsValidFrustum);
      }
    }

    public bool IsPerspectiveProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsPerspectiveProjection);
      }
    }
#if USING_V5_SDK
    public bool IsTwoPointPerspectiveProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsTwoPointPerspectiveProjection);
      }
    }
#endif
    public bool IsParallelProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsParallelProjection);
      }
    }

#if USING_V5_SDK
    /// <summary>
    /// Use this function to change projections of valid viewports from persective to parallel.
    /// It will make common additional adjustments to the frustum so the resulting views are
    /// similar. The camera location and direction will not be changed.
    /// </summary>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <returns>
    /// If the current projection is parallel and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToParallelProjection(bool symmetricFrustum)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_ChangeToParallelProjection(pThis, symmetricFrustum);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports from parallel to perspective.
    /// It will make common additional adjustments to the frustum and camera location so the
    /// resulting views are similar. The camera direction and target point are not be changed.
    /// </summary>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <param name="lensLength">
    /// (pass 50.0 when in doubt) 35 mm lens length to use when changing from parallel to perspective
    /// projections. If the current projection is perspective or lens_length is &lt;= 0.0, then
    /// this parameter is ignored.
    /// </param>
    /// <returns>
    /// If the current projection is perspective and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToPerspectiveProjection(bool symmetricFrustum, double lensLength)
    {
      return ChangeToPerspectiveProjection(RhinoMath.UnsetValue, symmetricFrustum, lensLength);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports from parallel to perspective.
    /// It will make common additional adjustments to the frustum and camera location so the
    /// resulting views are similar. The camera direction and target point are not be changed.
    /// </summary>
    /// <param name="targetDistance">
    /// If RhinoMath.UnsetValue this parameter is ignored. Otherwise it must be > 0 and indicates
    /// which plane in the current view frustum should be perserved.
    /// </param>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <param name="lensLength">
    /// (pass 50.0 when in doubt) 35 mm lens length to use when changing from parallel to perspective
    /// projections. If the current projection is perspective or lens_length is &lt;= 0.0, then
    /// this parameter is ignored.
    /// </param>
    /// <returns>
    /// If the current projection is perspective and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToPerspectiveProjection(double targetDistance, bool symmetricFrustum, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_ChangeToPerspectiveProjection(pThis, targetDistance, symmetricFrustum, lensLength);
    }
#endif
    const int idxCameraLocation = 0;
    const int idxCameraDirection = 1;
    const int idxCameraUp = 2;
    const int idxCameraX = 3;
    const int idxCameraY = 4;
    const int idxCameraZ = 5;

    public Point3d CameraLocation
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraLocation, ref v);
        return new Point3d(v);
      }
    }
    public Vector3d CameraDirection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraDirection, ref v);
        return v;
      }
    }
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public Vector3d CameraUp
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraUp, ref v);
        return v;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_VP_SetVector(pThis, idxCameraUp, value);
      }
    }

    /// <summary>
    /// Returns true if current camera orientation is valid
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool GetCameraFrame(out Plane frame)
    {
      frame = new Plane();
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetCameraFrame(pConstThis, ref frame);
    }

    /// <summary>unit to right vector</summary>
    public Vector3d CameraX
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraX, ref v);
        return v;
      }
    }
    /// <summary>unit up vector</summary>
    public Vector3d CameraY
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraY, ref v);
        return v;
      }
    }
    /// <summary>unit vector in CameraDirection</summary>
    public Vector3d CameraZ
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraZ, ref v);
        return v;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left">left &lt; right</param>
    /// <param name="right">left &lt; right</param>
    /// <param name="bottom">bottom &lt; top</param>
    /// <param name="top">bottom &lt; top</param>
    /// <param name="nearDistance">0 &lt; nearDistance &lt; farDistance</param>
    /// <param name="farDistance">0 &lt; nearDistance &lt; farDistance</param>
    /// <returns></returns>
    public bool GetFrustum(out double left, out double right, out double bottom, out double top, out double nearDistance, out double farDistance)
    {
      double[] items = new double[6];
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoViewport_VP_GetFrustum(ptr, ref items[0]);
      left = items[0];
      right = items[1];
      bottom = items[2];
      top = items[3];
      nearDistance = items[4];
      farDistance = items[5];
      return rc;
    }

    const int idxFrustumAspect = 0;
    const int idxScreenPortAspect = 1;
    const int idxCamera35mmLensLength = 2;

    /// <summary>frustum's width/height</summary>
    public double FrustumAspect
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxFrustumAspect);
      }
    }

    /// <summary>
    /// Returns world coordinates of frustum's center
    /// </summary>
    /// <param name="center"></param>
    /// <returns>true if the center was successfully computed</returns>
    public bool GetFrustumCenter(out Point3d center)
    {
      center = new Point3d();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetFrustumCenter(ptr, ref center);
    }
    
    /// <summary>Get clipping distance of a point</summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns>
    /// True if the point is ing the view frustum and near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(Point3d point, out double distance)
    {
      IntPtr ptr = ConstPointer();
      distance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth1(ptr, point, ref distance);
    }
    /// <summary>
    /// Get near and far clipping distances of a bounding box
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="nearDistance"></param>
    /// <param name="farDistance"></param>
    /// <returns>
    /// True if the bounding box intersects the view frustum and near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(BoundingBox bbox, out double nearDistance, out double farDistance)
    {
      IntPtr ptr = ConstPointer();
      nearDistance = farDistance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth2(ptr, bbox.m_min, bbox.m_max, ref nearDistance, ref farDistance);
    }
    /// <summary>
    /// Get near and far clipping distances of a sphere
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="nearDistance"></param>
    /// <param name="farDistance"></param>
    /// <returns>
    /// True if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// False if the sphere does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(Sphere sphere, out double nearDistance, out double farDistance)
    {
      IntPtr ptr = ConstPointer();
      nearDistance = farDistance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth3(ptr, ref sphere, ref nearDistance, ref farDistance);
    }

    const int idxGetFrustumNearPlane = 0;
    const int idxGetFrustumFarPlane = 1;
    const int idxGetFrustumLeftPlane = 2;
    const int idxGetFrustumRightPlane = 3;
    const int idxGetFrustumBottomPlane = 4;
    const int idxGetFrustumTopPlane = 5;

    /// <summary>Get near clipping plane</summary>
    /// <param name="plane">
    /// near clipping plane if camera and frustum are valid. The plane's
    /// frame is the same as the camera's frame. The origin is located at
    /// the intersection of the camera direction ray and the near clipping
    /// plane.
    /// </param>
    /// <returns>
    /// true if camera and frustum are valid
    /// </returns>
    public bool GetFrustumNearPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumNearPlane, ref plane);
    }
    /// <summary>Get far clipping plane</summary>
    /// <param name="plane">
    /// far clipping plane if camera and frustum are valid. The plane's
    /// frame is the same as the camera's frame. The origin is located at
    /// the intersection of the camera direction ray and the far clipping
    /// plane.
    /// </param>
    /// <returns>
    /// true if camera and frustum are valid
    /// </returns>
    public bool GetFrustumFarPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumFarPlane, ref plane);
    }
    /// <summary>Get left world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum left side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumLeftPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumLeftPlane, ref plane);
    }
    /// <summary>Get right world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum right side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumRightPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumRightPlane, ref plane);
    }
    /// <summary>Get bottom world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum bottom side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumBottomPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumBottomPlane, ref plane);
    }
    /// <summary>Get top world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum top side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumTopPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumTopPlane, ref plane);
    }

    /// <summary>Get corners of near clipping plane rectangle</summary>
    /// <returns>
    /// [left_bottom, right_bottom, left_top, right_top] points on success
    /// null on failure
    /// </returns>
    public Point3d[] GetNearRect()
    {
      Point3d[] rc = new Point3d[4];
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetRect(ptr, true, rc))
        rc = null;
      return rc;
    }
    /// <summary>Get corners of far clipping plane rectangle</summary>
    /// <returns>
    /// [left_bottom, right_bottom, left_top, right_top] points on success
    /// null on failure
    /// </returns>
    public Point3d[] GetFarRect()
    {
      Point3d[] rc = new Point3d[4];
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetRect(ptr, false, rc))
        rc = null;
      return rc;
    }

    /// <summary>
    /// Location of viewport in pixels.  These are provided so you can set the port you are using
    /// and get the appropriate transformations to and from screen space.
    /// </summary>
    /// <param name="portLeft">portLeft != portRight</param>
    /// <param name="portRight">portLeft != portRight</param>
    /// <param name="portBottom">portTop != portBottom</param>
    /// <param name="portTop">portTop != portBottom</param>
    /// <param name="portNear"></param>
    /// <param name="portFar"></param>
    /// <returns></returns>
    public bool GetScreenPort(out int portLeft, out int portRight, out int portBottom, out int portTop, out int portNear, out int portFar)
    {
      IntPtr ptr = ConstPointer();
      int[] items = new int[6];
      bool rc = UnsafeNativeMethods.CRhinoViewport_VP_GetScreenPort(ptr, ref items[0]);
      portLeft = items[0];
      portRight = items[1];
      portBottom = items[2];
      portTop = items[3];
      portNear = items[4];
      portFar = items[5];
      return rc;
    }

    /// <summary>
    /// screen port's width/height
    /// </summary>
    public double ScreenPortAspect
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxScreenPortAspect);
      }
    }

    public bool GetCameraAngle(out double halfDiagonalAngle, out double halfVerticalAngle, out double halfHorizontalAngle)
    {
      halfDiagonalAngle = halfVerticalAngle = halfHorizontalAngle = 0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetCameraAngle(ptr, ref halfDiagonalAngle, ref halfVerticalAngle, ref halfHorizontalAngle);
    }

    public double Camera35mmLensLength
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxCamera35mmLensLength);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_VP_SetCamera35mmLensLength(pThis, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceSystem"></param>
    /// <param name="destinationSystem"></param>
    /// <returns>
    /// 4x4 transformation matrix (acts on the left)
    /// Identity matrix is returned if this function fails
    /// </returns>
    public Transform GetTransform(Rhino.DocObjects.CoordinateSystem sourceSystem, Rhino.DocObjects.CoordinateSystem destinationSystem)
    {
      Transform matrix = new Transform();
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetXform(ptr, (int)sourceSystem, (int)destinationSystem, ref matrix))
        return Transform.Identity;
      return matrix;
    }

    /// <summary>
    /// Get the world coordinate line in the view frustum that projects to a point on the screen.
    /// </summary>
    /// <param name="screenX">(screenx,screeny) = screen location</param>
    /// <param name="screenY">(screenx,screeny) = screen location</param>
    /// <param name="worldLine">
    /// 3d world coordinate line segment starting on the near clipping
    /// plane and ending on the far clipping plane.
    /// </param>
    /// <returns>
    /// true if successful.
    /// false if view projection or frustum is invalid.
    /// </returns>
    public bool GetFrustumLine(double screenX, double screenY, out Line worldLine)
    {
      worldLine = new Line();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetFrustumLine(ptr, screenX, screenY, ref worldLine);
    }

    /// <summary>
    /// </summary>
    /// <param name="pointInFrustum"></param>
    /// <param name="pixelsPerUnit">
    /// scale = number of pixels per world unit at the 3d point
    /// </param>
    /// <returns></returns>
    public bool GetWorldToScreenScale(Point3d pointInFrustum, out double pixelsPerUnit)
    {
      pixelsPerUnit = 0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetWorldToScreenScale(ptr, pointInFrustum, ref pixelsPerUnit);
    }
    
    #endregion

    #region Wrappers for ON_3dmView
    const int idxConstructionGridVisible = 0;
    const int idxConstructionAxesVisible = 1;
    const int idxWorldAxesVisible = 2;

    public bool ConstructionGridVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxConstructionGridVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxConstructionGridVisible, value);
      }
    }
    public bool ConstructionAxesVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxConstructionAxesVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxConstructionAxesVisible, value);
      }
    }
    public bool WorldAxesVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxWorldAxesVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxWorldAxesVisible, value);
      }
    }
    #endregion

    //[skipping]
    // public RhinoViewport()

    /// <summary>
    /// Gets the parent view, if there is one
    /// 
    /// Every CRhinoView has an associated CRhinoViewport that does all the 3d display work.
    /// Those associated viewports return the CRhinoView as their parent view. However,
    /// CRhinoViewports are used in other image creating contexts that do not have a parent
    /// CRhinoView.  If you call ParentView, you MUST check for NULL return values.
    /// </summary>
    public RhinoView ParentView
    {
      get
      {
        if (null == m_parent_view)
        {
          IntPtr ptr = ConstPointer();
          IntPtr pParentView = UnsafeNativeMethods.CRhinoViewport_ParentView(ptr);
          m_parent_view = RhinoView.FromIntPtr(pParentView);
        }
        return m_parent_view;
      }
    }

    /// <summary>Unique id for this viewport</summary>
    public Guid Id
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_ViewportId(ptr);
      }
    }

    /// <summary>
    /// The value of change counter is incremented every time the view projection
    /// or construction plane changes. The user can the mouse and nestable view 
    /// manipulation commands to change a view at any time. The value value of change
    /// counter can be used to detect these changes in code that is sensitive to
    /// the view projection.
    /// </summary>
    [CLSCompliant(false)]
    public uint ChangeCounter
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_ChangeCounter(ptr);
      }
    }



    /// <summary>
    /// returns true if some portion world coordinate bounding box is
    /// potentially visible in the viewing frustum.
    /// </summary>
    /// <param name="bbox"></param>
    /// <returns></returns>
    public bool IsVisible(BoundingBox bbox)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_IsVisible(ptr, bbox.Min, bbox.Max, true);
    }
    /// <summary>
    /// returns true if some portion world coordinate point is
    /// potentially visible in the viewing frustum.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsVisible(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_IsVisible(ptr, point, Point3d.Unset, false);
    }

    /// <summary>name associated with this viewport</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public string Name
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        IntPtr rc = UnsafeNativeMethods.CRhinoViewport_GetSetName(ptr, false, null);
        if (rc == IntPtr.Zero)
          return null;
        return System.Runtime.InteropServices.Marshal.PtrToStringUni(rc);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_GetSetName(ptr, true, value);
      }
    }


    public Point3d CameraTarget
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d target = new Point3d();
        UnsafeNativeMethods.CRhinoViewport_Target(pConstThis, ref target);
        return target;
      }
    }

    const int idxSetCameraTarget = 0;
    const int idxSetCameraDirection = 1;
    const int idxSetCameraLocation = 2;

    /// <summary>
    /// Set viewport target point. By default the camera location
    /// is translated so that the camera direction vector is parallel
    /// to the vector from the camera location to the target location.
    /// </summary>
    /// <param name="targetLocation">new target location</param>
    /// <param name="updateCameraLocation">
    /// if true, the camera location is translated so that the camera direction
    /// vector is parallel to the vector from the camera location to the target
    /// location.
    /// If false, the camera location is not changed.
    /// </param>
    /// <remarks>
    /// In general, Rhino users expect to have the camera direction vector and
    /// the vector from the camera location to the target location to be parallel.
    /// If you use the RhinoViewport functions to set the camera location, camera
    /// direction, and target point, then the relationship between these three
    /// points and vectors is automatically maintained.  If you directly manipulate
    /// the camera properties, then you should carefully set the target by calling
    /// SetTarget() with updateCameraLocation=false.
    /// </remarks>
    public void SetCameraTarget(Point3d targetLocation, bool updateCameraLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, targetLocation, updateCameraLocation, idxSetCameraTarget);
    }

    /// <summary>
    /// Set viewport camera location and target location. The camera direction vector is
    /// changed so that it is parallel to the vector from the camera location to
    /// the target location.
    /// </summary>
    /// <param name="targetLocation">new target location</param>
    /// <param name="cameraLocation">new camera location</param>
    public void SetCameraLocations(Point3d targetLocation, Point3d cameraLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraLocations(pThis, targetLocation, cameraLocation);
    }

    /// <summary>
    /// Set viewport camera direction. By default the target location is changed so that
    /// the vector from the camera location to the target is parallel to the camera direction.
    /// </summary>
    /// <param name="cameraDirection">new camera direction</param>
    /// <param name="updateTargetLocation">
    /// if true, the target location is changed so that the vector from the camera
    /// location to the target is parallel to the camera direction.
    /// If false, the target location is not changed.
    /// See the remarks section of RhinoViewport.SetTarget for important details.
    /// </param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void SetCameraDirection(Vector3d cameraDirection, bool updateTargetLocation)
    {
      IntPtr pThis = NonConstPointer();
      Point3d dirAsPoint = new Point3d(cameraDirection);
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, dirAsPoint, updateTargetLocation, idxSetCameraDirection);
    }


    /// <summary>
    ///  Set viewport camera location. By default the target location is changed so that
    ///  the vector from the camera location to the target is parallel to the camera direction
    ///  vector.
    /// </summary>
    /// <param name="cameraLocation">new camera location</param>
    /// <param name="updateTargetLocation">
    /// if true, the target location is changed so that the vector from the camera
    /// location to the target is parallel to the camera direction vector.  
    /// If false, the target location is not changed. See the remarks section of
    /// RhinoViewport.SetTarget for important details.
    /// </param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void SetCameraLocation(Point3d cameraLocation, bool updateTargetLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, cameraLocation, updateTargetLocation, idxSetCameraLocation);
    }


    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public Plane ConstructionPlane()
    {
      IntPtr ptr = ConstPointer();
      Plane plane = new Plane();
      UnsafeNativeMethods.CRhinoViewport_ConstructionPlane(ptr, ref plane, false);
      return plane;
    }

    public void SetConstructionPlane(Plane plane)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_ConstructionPlane(ptr, ref plane, true);
    }

    //  // Returns:
    //  //   Viewport construction plane
    //  const ON_3dmConstructionPlane& ConstructionPlane() const;

    /// <summary>
    /// Sets the construction plane to cplane.
    /// </summary>
    /// <param name="cplane"></param>
    public void SetConstructionPlane(DocObjects.ConstructionPlane cplane)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pCPlane = cplane.CopyToNative();
      if (pCPlane != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoViewport_SetConstructionPlane(pThis, pCPlane, false);
        UnsafeNativeMethods.ON_3dmConstructionPlane_Delete(pCPlane);
      }
    }

    /// <summary>
    /// Pushes the current construction plane on the viewport's
    /// construction plane stack and sets the construction plane
    /// to cplane.
    /// </summary>
    /// <param name="cplane"></param>
    public void PushConstructionPlane(DocObjects.ConstructionPlane cplane)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pCPlane = cplane.CopyToNative();
      if (pCPlane != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoViewport_SetConstructionPlane(pThis, pCPlane, true);
        UnsafeNativeMethods.ON_3dmConstructionPlane_Delete(pCPlane);
      }
    }

    const int idxPopConstructionPlane = 0;
    const int idxNextConstructionPlane = 1;
    const int idxPrevConstructionPlane = 2;

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to PushConstructionPlane.
    /// </summary>
    /// <returns>true if a construction plane was popped</returns>
    public bool PopConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxPopConstructionPlane);
    }

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to PreviousConstructionPlane.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool NextConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxNextConstructionPlane);
    }

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to NextConstructionPlane
    /// or SetConstructionPlane.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool PreviousConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxPrevConstructionPlane);
    }


    /// <summary>
    /// Remove trace image (background bitmap) for this viewport if one exists
    /// </summary>
    public void ClearTraceImage()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_ClearTraceImage(ptr);
    }

    /// <summary>
    /// Set trace image (background bitmap) for this viewport
    /// </summary>
    /// <param name="bitmapFileName"></param>
    /// <param name="plane"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="grayscale"></param>
    /// <param name="filtered">true if image should be filtered (bilinear) before displayed</param>
    /// <returns>true if successful</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public bool SetTraceImage(string bitmapFileName, Plane plane, double width, double height, bool grayscale, bool filtered)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_SetTraceImage(ptr, bitmapFileName, ref plane, width, height, grayscale, filtered);
    }

    /// <summary>
    /// Appends the current view projection and target to the viewport's view stack.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void PushViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_PushViewProjection(pThis);
    }

    /// <summary>
    /// Sets the view projection and target to the settings at the top of
    /// the view stack and removes those settings from the view stack.
    /// </summary>
    /// <returns>true if there were settings that could be popped from the stack</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public bool PopViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_PopViewProjection(pThis);
    }

    public bool PushViewInfo(DocObjects.ViewInfo viewinfo, bool includeTraceImage)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstView = viewinfo.ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_PushViewInfo(pThis, pConstView, includeTraceImage);
    }

    public bool KeyboardRotate(bool leftRight, double angleRadians)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_KeyboardRotate(pThis, leftRight, angleRadians);
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains all of the
    /// selected document objects that can be seen in view. If the projection is
    /// perspective, the camera angle is not changed.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool ZoomExtents()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoDollyExtents(pThis, false);
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains all of the
    /// selected document objects that can be seen in view. If the projection is
    /// perspective, the camera angle is not changed.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool ZoomExtentsSelected()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoDollyExtents(pThis, true);
    }

    /// <summary>
    /// Zooms the viewport to the given bounding box
    /// </summary>
    /// <param name="box"></param>
    /// <returns>true if successful</returns>
    public bool ZoomBoundingBox(BoundingBox box)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhZoomExtentsHelper(pThis, box.m_min, box.m_max);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="magnificationFactor"></param>
    /// <param name="mode">
    /// 0 = perform a "dolly" magnification by moving the camera towards/away from
    /// the target so that the amount of the screen subtended by an object changes.
    /// 1 = perform a "zoom" magnification by adjusting the "lens" angle           
    /// </param>
    /// <returns></returns>
    public bool Magnify(double magnificationFactor, int mode)
    {
      System.Drawing.Point pt = new System.Drawing.Point(-1, -1);
      return Magnify(magnificationFactor, mode, pt);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="magnificationFactor"></param>
    /// <param name="mode">
    /// 0 = perform a "dolly" magnification by moving the camera towards/away from
    /// the target so that the amount of the screen subtended by an object changes.
    /// 1 = perform a "zoom" magnification by adjusting the "lens" angle           
    /// </param>
    /// <param name="fixedScreenPoint"></param>
    /// <returns></returns>
    public bool Magnify(double magnificationFactor, int mode, System.Drawing.Point fixedScreenPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_Magnify(pThis, magnificationFactor, mode, fixedScreenPoint.X, fixedScreenPoint.Y);
    }

    public ViewportType ViewportType
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.CRhinoViewport_ViewportType(pConstThis);
        return (ViewportType)rc;
      }
    }
  }


  public enum ViewportType : int
  {
    StandardModelingViewport = 0,
    PageViewMainViewport = 1,
    DetailViewport = 2
  }
}


#region drawing functions
//[skipping]
//  ON::DisplayMode DisplayMode() const;
//  bool SetDisplayMode(const ON_UUID&);
//  bool DisplayModeIsShaded(void) const;
//  void EnableFlatShade( bool bFlatShade = true );
//  void EnableXrayShade( bool bXrayShade = true );
//  void EnableGhostedShade( bool bGhostedShade = true );
//  void EnableSelectedShade( bool bSelectedShade = true );
//  bool FlatShade() const;
//  bool XrayShade() const;
//  bool GhostedShade() const;
//  bool SelectedShade() const;
//  int SetZBiasWire(int zbias);
//  int ZBiasWire() const;
//  void SetZBufferWireDepth( int wire_depth );
//  int ZBufferWireDepth() const;
//  bool  GetZBuffer(CRhinoZBuffer&) const;
//  void  Flush(void);
//  ON_Color SetDrawColor(COLORREF color);
//  ON_Color DrawColor() const;
//  ON_Color SetShadeColor(COLORREF);
//  ON_Color ShadeColor() const;
//  int GetLightingModel( ON_ClassArray<ON_Light>& lights );
//  CRhinoFont* SetDrawFont( CRhinoFont* );
//  CRhinoFont* DrawFont() const;
//  bool InterruptDrawing() const;
//  ///////////////////////////////////////////////////////////////////
//  //
//  // Drawing controls
//  //
//  void SetScreenSize(int,int);

//  // set optimal clipping planes to view objects in a world coordinate 3d bounding box
//  void SetClippingPlanes(const ON_BoundingBox&);

//  void Clear( COLORREF );

//  enum DRAW_STEP
//  {
//    draw_idle = 0,     // not in CRhinoView::OnDraw
//    draw_begin,        // preparing to draw
//    draw_background,   // drawing background (grid, bitmaps, etc.)
//    draw_middleground, // drawing middlground (objects)
//    draw_foreground,   // drawing forground (world axes, etc.)
//    draw_finish_backbuffer, // drawing forground (world axes, etc.)
//    draw_decorations,  // drawing decorations
//    draw_finish_frontbuffer // finishing 
//  };

//  /*
//  Returns:
//    Current drawing step.
//  */
//  DRAW_STEP DrawStep() const;

//  enum point_style
//  {
//    ps_standard       = 0,
//    ps_single_dot     = 1,
//    ps_double_dot     = 2,
//    ps_triple_dot     = 3,
//  };

//  enum point_mode
//  {
//    pm_normal      = 0,
//    pm_shaded      = 1,
//    pm_highlighted = 2,
//  };

//  point_style   SetPointStyle(point_style  ps, point_mode=pm_normal);
//  point_style   GetPointStyle(void);
//  point_style   GetPointStyle(point_mode&);

//  static void          SetPointCloudStyle(point_style  ps);
//  static point_style   GetPointCloudStyle(void);

//  // OBSOLETE - DO NOT USE 
//  //__declspec(deprecated) void DrawDocument( CRhinoObjectIterator& iterator, 
//  //                                          bool bIgnoreHighlights = false,
//  //                                          bool display_hint = false);

//  ///////////////////////////////////////////////////////////////////
//  //
//  // 3d world coordinate drawing tools
//  //

//  /*
//  Description:
//    Draw a construction plane grid.
//  Parameters:
//    bShowConstructionGrid - [in] if true, the construction grid is drawn
//    bShowConstructionAxes - [in] if true, the construction grid m_x and m_y axes are drawn
//    thin_line_color - [in] color used to draw thin grid lines.
//       The default is RhinoApp().AppSettings().GridThinLineColor().
//       If thin_line_color != ON_UNSET_COLOR, then this specified 
//       color is used.
//    thick_line_color - [in] color used to draw thick grid lines.
//       The default is RhinoApp().AppSettings().GridThickLineColor().
//       If thin_line_color != ON_UNSET_COLOR, then this specified 
//       color is used.
//    grid_x_axis_color - [in] color used to draw the m_x axis.
//       The default is RhinoApp().AppSettings().GridXAxisColor().
//       If thin_line_color != ON_UNSET_COLOR, then this specified 
//       color is used.
//    grid_y_axis_color - [in] color used to draw the m_y axis.
//       The default is RhinoApp().AppSettings().GridYAxisColor().
//       If thin_line_color != ON_UNSET_COLOR, then this specified 
//       color is used.
//  */
//  void DrawConstructionPlane( 
//          const ON_3dmConstructionPlane& cplane,
//          BOOL bShowConstructionGrid,
//          BOOL bShowConstructionAxes,
//          COLORREF thin_line_color = ON_UNSET_COLOR,
//          COLORREF thick_line_color = ON_UNSET_COLOR,
//          COLORREF grid_x_axis_color = ON_UNSET_COLOR,
//          COLORREF grid_y_axis_color = ON_UNSET_COLOR
//          );

//  /*
//  Description:
//    Draw cross hairs across the entire viewport that
//    run along the plane's x and y axes.
//  Parameters:
//    plane - [in] the cross hairs run along the plane's
//                 x and y axes.
//    color - [in]
//  */
//  void DrawCrossHairs( 
//        const ON_Plane& plane, 
//        COLORREF color 
//        );

//  /*
//  Description:
//    Do not use this function.  It is a debugging tool.
//    See rhino3ViewDraw.cpp source code for details.
//  */
//  bool DrawPixelTest( 
//          ON::coordinate_system point_cs,
//          ON_3dPoint point,
//          COLORREF color,
//          ON_TextLog* text_dump = 0
//          );


//  void DrawPoint( const ON_3dPoint&, const ON_3dVector* = NULL );
//  void DrawLine(const ON_3dPoint&,const ON_3dPoint&);
//  void DrawDottedLine(const ON_3dPoint&,const ON_3dPoint&);

//  /*
//  Description:
//    Draw inference point used in gesture based snapping
//  Parameters:
//    P - [in] world 3d
//    color - [in] if = ON_UNSET_COLOR, then the system inference color
//                 is used.
//  */
//  void DrawInferencePoint( 
//          const ON_3dPoint& P, 
//          COLORREF color=ON_UNSET_COLOR 
//          );

//  /*
//  Description:
//    Draw inference line used in gesture based snapping
//  Parameters:
//    P - [in] world 3d start
//    Q - [in] world 3d end
//    color - [in] if = ON_UNSET_COLOR, then the system inference color
//                 is used.
//    type - [in] 
//             0 = chord, 
//             1 = ray from P through Q
//             2 = infinite line
//  */
//  void DrawInferenceLine( 
//          const ON_3dPoint& P,
//          const ON_3dPoint& Q, 
//          COLORREF color=ON_UNSET_COLOR,
//          int type=0
//          );

//  void DrawArc(const ON_Arc&);
//  void DrawCircle(const ON_Circle&);

//  void DrawPointCloud( int point_count, const ON_3dPoint* points, const ON_3dPoint& origin );

//  // Description:
//  //   Draw a wireframe sphere.
//  // Parameters:
//  //   sphere - [in]
//  void DrawSphere(const ON_Sphere& sphere);

//  // Description:
//  //   Draw a wireframe sphere.
//  // Parameters:
//  //   sphere - [in]
//  void DrawTorus(const ON_Torus& torus);

//  // Description:
//  //   Draw a wireframe cylinder.
//  // Parameters:
//  //   cylinder - [in]
//  void DrawCylinder(const ON_Cylinder& cylinder);

//  // Description:
//  //   Draw a wireframe cone.
//  // Parameters:
//  //   cone - [in]
//  void DrawCone(const ON_Cone& cone);
  
//  // Description:
//  //   Draw a wireframe box.
//  // Parameters:
//  //   box_corners - [in] array of eight box corners
//  //
//  //            7______________6
//  //            |\             |\
//  //            | \            | \
//  //            |  \ _____________\ 
//  //            |   4          |   5
//  //            |   |          |   |
//  //            |   |          |   |
//  //            3---|----------2   |
//  //            \   |          \   |
//  //             \  |           \  |
//  //              \ |            \ |
//  //               \0_____________\1
//  //
//  void DrawBox( const ON_3dPoint* box_corners );

//  /*
//  Description:
//    Draws a wireframe icon of a light.
//  Parameters:
//    light - [in]
//    wireframe_color - [in] color to use for wireframe
//  */
//  void DrawLight( const ON_Light& light, COLORREF wireframe_color );

//  /*
//  Description:
//    Draws a curve in a viewport window.
//  Parameters:
//    curve - [in]
//  Remarks:
//    This funtion can be slow and should not be used in drawing code
//    that is called every time the mouse moves.
//  See Also:
//    CRhinoViewport::DrawNurbsCurve
//    CRhinoViewport::DrawBezier
//    CRhinoViewport::DrawCurve
//  */
//  void DrawCurve( 
//        const ON_Curve& curve
//        );

//  /*
//  Description:
//    Draws the curve's curvature hair graph in a viewport window.
//  Parameters:
//    curve - [in]
//    hair_settings - [in]  If NULL, the settings
//    from RhinoApp().AppSettings().CurvatureGraphSettings() are
//    used.
//  Remarks:
//    This funtion can be slow and should not be used in drawing code
//    that is called every time the mouse moves.
//  See Also:
//    CRhinoViewport::DrawCurve
//  */
//  void DrawCurvatureGraph( 
//        const ON_Curve& curve, 
//        const CRhinoCurvatureGraphSettings* hair_settings = 0 
//        );


//  /*
//  Description:
//    Draw the surface's normal curvature along surface iso parametric curves.
//  Parameters:
//    surface - [in]
//    wire_density - [in]
//      Density of iso-curves
//    hair_settings - [in]
//      If NULL, RhinoApp().AppSettings().CurvatureGraphSettings()
//      are used.
//  */
//  void DrawNormalCurvatureGraph( 
//        const ON_Surface& surface, 
//        int wire_density,
//        const CRhinoCurvatureGraphSettings* hair_settings = 0 
//        );

//  /*
//  Description:
//    Draws a NURBS curve in a viewport window.
//  Parameters:
//    nurbs_curve - [in]
//  See Also:
//    CRhinoViewport::DrawNurbsCurve
//    CRhinoViewport::DrawBezier
//    CRhinoViewport::DrawCurve
//  */
//  void DrawNurbsCurve(
//    const ON_NurbsCurve& nurbs_curve
//    );

//  void DrawWireframeMesh(
//         const ON_Mesh*,
//         BOOL bCullCW // = true to cull clockwise triangles
//         );
//  void DrawShadedMesh(
//         const ON_Mesh*,
//         bool bCullCW,     // bCullCW = true to cull clockwise triangles
//         bool bFlatShade,  // true for flat shading
//         bool bHighlighted // true if mesh is highlighed
//         );
//  void DrawRenderPreviewMesh(
//         const ON_Mesh* mesh,
//         const CRhinoMaterial& render_material,
//         BOOL bCullCW,     // true to cull clockwise triangles
//         BOOL bHighlighted // true if mesh is highlighed
//         );

//  /*
//  Description:
//    Draws a zebra stripe mesh.
//  Parameters:
//    mesh - [in] mesh with vertex normals
//    zebra_info - [in] zebra stripe settings
//    mesh_color - [in] color for non-stripe areas
//    bCullCW - [in] true to cull clockwise triangles
//    edge_color - [in] if not ON_UNSET_COLOR, then
//                      mesh wireframe is drawn in this color.
//  */
//  void DrawZebraMesh(
//         const ON_Mesh* mesh,
//         const CRhinoZebraAnalysisSettings& zebra_info,
//         COLORREF mesh_color,
//         bool bCullCW,
//         COLORREF edge_color = ON_UNSET_COLOR
//         );

//  /*
//  Description:
//    Draws environment mapped mesh.
//  Parameters:
//    mesh - [in] mesh with vertex normals
//    emap_info - [in] environment map settings
//    mesh_color - [in] diffuse color
//    bCullCW - [in] true to cull clockwise triangles
//    edge_color - [in] if not ON_UNSET_COLOR, then
//                      mesh wireframe is drawn in this color.
//  */
//  void DrawEmapMesh(
//         const ON_Mesh* mesh,
//         const CRhinoEmapAnalysisSettings& emap_info,
//         COLORREF mesh_color,
//         bool bCullCW,
//         COLORREF edge_color = ON_UNSET_COLOR
//         );

//  /*
//  Description:
//    Draws false color mesh.
//  Parameters:
//    mesh - [in] mesh with vertex colors
//    bCullCW - [in] true to cull clockwise triangles
//    edge_color - [in] if not ON_UNSET_COLOR, then
//                      mesh wireframe is drawn in this color.
//  */
//  void DrawFalseColorMesh(
//         const ON_Mesh* mesh,
//         bool bCullCW,
//         COLORREF edge_color = ON_UNSET_COLOR
//         );

//  void DrawActivePoint( const ON_3dPoint& );
//  void DrawBoundingBox( const ON_BoundingBox& );

//  /*
//  Description:
//    Draws a bezier curve in a viewport window.
//  Parameters:
//    nurbs_curve - [in]
//  See Also:
//    CRhinoViewport::DrawNurbsCurve
//    CRhinoViewport::DrawBezier
//    CRhinoViewport::DrawCurve
//  */
//  void DrawBezier(
//         const ON_BezierCurve& bezier_curve
//         );

//  void DrawBezier(
//         int order,
//         const ON_2dPoint* cv // 2d world coordinate CVs 
//         );
//  void DrawBezier(
//         int orer,
//         const ON_3dPoint* cv // 3d world coordinate CVs 
//         );
//  void DrawBezier(
//         int order,
//         const ON_4dPoint* cv // 4d homogeneous world coordinate CVs 
//         );
//  void DrawBezier(
//         int order,
//         const ON_2fPoint* cv // 2d world coordinate CVs 
//         );
//  void DrawBezier(
//         int order,
//         const ON_3fPoint* cv // 3d world coordinate CVs 
//         );
//  void DrawBezier(
//         int order,
//         const ON_4fPoint* cv // 4d homogeneous world coordinate CVs 
//         );
//  void DrawBezier(
//         int dim,
//         BOOL bRational,
//         int order,
//         int cv_stride,  // number of doubles between CVs (>=4)
//         const double* cv4d // 4d homogeneous world coordinate CVs 
//         );
//  void DrawBezier(
//         int dim,
//         BOOL bRational,
//         int order,
//         int cv_stride,  // number of floats between CVs (>=4)
//         const float* cv4d // 4d homogeneous world coordinate CVs 
//         );

//  /*
//  Description:
//    Draws a wireframe surface.
//  Parameters:
//    surface - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  Remarks:
//    This funtion can be slow and should not be used in drawing code
//    that is called every time the mouse moves.
//  See Also:
//    CRhinoViewport::DrawNurbsSurface
//    CRhinoViewport::DrawBezierSurface
//    CRhinoViewport::DrawSurface
//    CRhinoViewport::DrawBrep
//  */
//  void DrawSurface( 
//        const ON_Surface& surface, 
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe NURBS surface.
//  Parameters:
//    nurbs_surface - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  See Also:
//    CRhinoViewport::DrawNurbsSurface
//    CRhinoViewport::DrawBezierSurface
//    CRhinoViewport::DrawSurface
//  */
//  void DrawNurbsSurface( 
//        const ON_NurbsSurface& nurbs_surface, 
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe bezier surface.
//  Parameters:
//    bezier_surface - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  */
//  void DrawBezierSurface( 
//        const ON_BezierSurface& bezier_surface,
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe NURBS cage.
//  Parameters:
//    nurbs_cage - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  */
//  void DrawNurbsCage( 
//        const ON_NurbsCage& nurbs_cage, 
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe bezier cage.
//  Parameters:
//    bezier_cage - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  */
//  void DrawBezierCage( 
//        const ON_BezierSurface& bezier_cage,
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe brep.
//  Parameters:
//    brep - [in]
//    display_density - [in] ON_3dmObjectAttributes.m_wire_density value
//  Remarks:
//    This funtion can be slow and should not be used in drawing code
//    that is called every time the mouse moves.
//  See Also:
//    CRhinoViewport::DrawSurface
//    CRhBrepDisplay::Draw
//  */
//  void DrawBrep( 
//        const ON_Brep& brep,
//        int display_density = 1
//        );

//  /*
//  Description:
//    Draws a wireframe beam.
//  Parameters:
//    beam - [in]
//  Remarks:
//    This funtion is intended to draw dynamic display in commands that
//    create and edit beams.
//  */
//  void DrawBeam(
//        const ON_Beam& beam
//        );

//  /*
//  Description:
//    Draws a transformed instance definition
//  Parameters:
//    object - [in]
//    xform - [in] The object is drawn as if this transformtion
//      had been applied to its geometry. The object itself is
//      not modified.
//    color - [in] By default, the object is drawn in
//      the color returned by CRhinoObject::ObjectDrawColor(true).
//      If color is not ON_UNSET_COLOR, then the objects
//      are drawn in the specified color.
//  Remarks:
//    This funtion can be slow and should not be used in drawing code
//    that is called every time the mouse moves.
//  See Also:
//    CRhinoViewport::DrawInstanceDefinition
//  */
//  void DrawRhinoObject( 
//    const CRhinoObject* object, 
//    ON_Xform xform,
//    ON_Color color = ON_UNSET_COLOR
//    );

//  /*
//  Description:
//    Draws a round circle with the text in the circle.
//    This tool is used for things like the "A", "B", "C", "D"
//    in the NetworkSrf dialog and the the "1", "2", ... in
//    the PerspectiveMatch command.
//  Parameters:
//    screen_x - [in] 0 = screen left
//    screen_y - [in] 0 = screen top
//    text - [in] NULL for an empty dot or a short
//               (generally 1 character or number) string.
//    dot_color - [in] color of dot
//    text_color - [in] text color. If text_color is ON_unset_color,
//               then white is used if the dot_color is dark and
//               black is used if the dot_color is light.
//  Returns:
//    True if successful.
//  */
//  void DrawDot(
//         int screen_x,
//         int screen_y,
//         const wchar_t* text = NULL,
//         COLORREF dot_color = RGB(0,0,0),
//         COLORREF text_color = ON_UNSET_COLOR
//         );

//  /*
//  Description:
//    Draws a round circle with the text in the circle.
//    This tool is used for things like the "A", "B", "C", "D"
//    in the NetworkSrf dialog and the the "1", "2", ... in
//    the PerspectiveMatch command.
//  Parameters:
//    point - [in] 3d world location of the dot
//    text - [in] NULL for an empty dot or a short
//               (generally 1 character or number) string.
//    dot_color - [in] color of dot
//    text_color - [in] text color. If text_color is ON_unset_color,
//               then white is used if the dot_color is dark and
//               black is used if the dot_color is light.
//  Returns:
//    True if successful.
//  Remarks:
//    Default transforms the point to screen coordinates
//    and calls the DrawDot(screen_x,screen_y,text).
//  */
//  void DrawDot(
//         ON_3dPoint point,
//         const wchar_t* text = NULL,
//         COLORREF dot_color = RGB(0,0,0),
//         COLORREF text_color = ON_UNSET_COLOR
//         );

//  //////////
//  // Draws text with specified color, font and height
//  void DrawString( 
//    const wchar_t* string,    // string to draw, can return a modified string
//    int slength,              // length of the string
//    const ON_3dPoint& point,  // definition point, either lower-left or middle
//    int bMiddle = false,      // true: middle justified, false: lower-left justified
//    int rotation = 0,         // Text rotation in 1/10 degrees
//    int height = 12,          // height in pixels
//    const wchar_t* fontface = L"Arial"
//    );


//  //////////
//  // Draws text on an arbitrary plane
//  void DrawString(
//    const ON_Plane&,    // 3d world coordinates
//    ON_2dPoint,   // point in plane
//    ON_2dVector, // rotation in plane (sin(angle),cosine(angle))
//    const CRhinoText&        // string
//    );


//  //////////
//  // Draws an arrow symobl.  The plane is in world coordinates (WCS)
//  // and provides a 2d coordinate plane (ECS) for the arrow.
//  void DrawArrow(
//    const ON_Plane& plane, // in wcs points - arrow points are in plane (ECS) coordinates
//    const CRhinoArrowhead& arrow);  // arrow symbol to draw

//  //////////
//  // Draws a triangle from 3 3d points
//  void DrawTriangle( const ON_3dPoint corners[3]);
//  void DrawTriangle( const ON_3dPoint& p0, const ON_3dPoint& p1, const ON_3dPoint& p2);

//  // viewport decorations
//  void DrawWorldAxes();

//  /*
//  Description:
//    Draws a direction arrow decoration.
//  Parameters:
//    tail_point - [in] start of the arrow.
//    direction_vector - [in] direction of arrow.
//       A vector with a length of 1 unit draws the standard
//       arrow dectoration.
//    head_point - [out] if not NULL, the location of the
//        arrow tip is returned here.
//  Remarks:
//    These are the arrows used to indicate curve
//    direction and surface normals.
//  See Also:
//    CRhinoViewport::SetDrawColor,
//    CRhinoViewport::DrawTangentBar, 
//    ON_Viewport::GetWorldToScreenScale, 
//  */
//  void DrawDirectionArrow(
//    const ON_3dPoint& tail_point,
//    const ON_3dVector& direction_vector,
//    ON_3dPoint* head_point = NULL
//    );

//  /*
//  Description:
//    Draws a tangent line segment decoration.
//  Parameters:
//    mid_point - [in] midpoint of line segment
//    direction_vector - [in] unit vector direction of line segment.
//    start_point -[out] if not NULL, the start of the tangent bar
//       is returned here.
//    end_point -[out] if not NULL, the end of the tangent bar
//       is returned here.
//  Remarks:
//    These are the line segments used to indicate curve
//    tangents.  Generally, direction_vector should have length = 1.
//    For special situations when a shorter/longer tangent bar
//    is desired, 
//  See Also:
//    CRhinoViewport::SetDrawColor,
//    CRhinoViewport::DrawDirectionArrow
//    ON_Viewport::GetWorldToScreenScale, 
//  */
//  void DrawTangentBar(
//    const ON_3dPoint& mid_point,
//    const ON_3dVector& direction_vector,
//    ON_3dPoint* start_point = NULL,
//    ON_3dPoint* end_point = NULL
//    );
#endregion drawing functions

//  /*
//  Description:
//    Input viewport projection information.
//  Returns:
//    viewport projection information as an ON_Viewport.
//  See Also:
//    CRhinoViewport::SetVP
//  */
//  const ON_Viewport& VP() const;

//  /*
//  Description:
//    Set viewport camera projection.
//  Parameters:
//    camera_location - [in] new target location
//    bUpdateTargetLocation - [in] if true, the target
//        location is changed so that the vector from the camera
//        location to the target is parallel to the camera direction
//        vector.  
//        If false, the target location is not changed.
//        See the remarks section of CRhinoViewport::SetTarget
//        for important details.
//  Remarks:
//    See the remarks section of CRhinoViewport::SetTarget for
//    important details.
//  See Also:
//    CRhinoViewport::Target
//    CRhinoViewport::SetTarget
//    CRhinoViewport::SetTargetAndCameraLocation
//    CRhinoViewport::SetCameraLocation
//    CRhinoViewport::SetCameraDirection
//    CRhinoViewport::SetVP
//    CRhinoViewport::SetView
//  */
//  bool SetVP( const ON_Viewport&, BOOL bUpdateTargetLocation );

//  /*
//  Description:
//    Sets the wallpaper bitmap.
//  Parameters:
//    wallpaper - [in]
//  Returns:
//    True if successful.
//  */
//  bool SetWallpaperImage( 
//         const ON_3dmWallpaperImage& wallpaper
//         );

//  /*
//  Description:
//    Verifies the existance of a wallpaper.
//  Parameters:
//    None.
//  Returns:
//    True if a wallpaper exists.
//    False otherwise
//  */
//  bool IsWallpaperImage();

//  /*
//  Description:
//    Sets the background bitmap.
//  Parameters:
//    traceimage - [in]
//  Returns:
//    True if successful.
//  */
//  bool SetTraceImage( 
//         const ON_3dmViewTraceImage& traceimage
//         );

//  /*
//  Description:
//    Verifies the existance of background bitmap.
//  Parameters:
//    None.
//  Returns:
//    True if a background bitmap exists.
//    False otherwise
//  */
//  bool IsTraceImage();
         
//  /*
//  Description:
//    Input complete information about the viewport projection, target 
//    location, name, display mode, window location in the Rhino
//    mainframe, construction plane, background bitmap and wallpaper 
//    bitmap.
//  Returns:
//    view informtion as an ON_3dmView.
//  See Also:
//    CRhinoViewport::SetView
//  */
//  const ON_3dmView& View() const;


//  /*
//  Description:
//    Expert user function to set viewport projection, target 
//    location, name, display mode, window location in the Rhino
//    mainframe, construction plane, background bitmap and wallpaper 
//    bitmap.
//  Parameters:
//    view - [in]
//  Remarks:
//    This function is primarily used to save and restore view settings.
//    If you want to modify specific viewport settings, use the functions
//    listed in the see also section.
//  See Also:
//    CRhinoViewport::Target
//    CRhinoViewport::SetTarget
//    CRhinoViewport::SetTargetAndCameraLocation
//    CRhinoViewport::SetCameraLocation
//    CRhinoViewport::SetCameraDirection
//    CRhinoViewport::SetVP
//    CRhinoViewport::SetView
//  */
//  void SetView( const ON_3dmView& view );

//  // controls display of construction plane grid
//  bool ShowConstructionGrid() const;
//  void SetShowConstructionGrid(BOOL);

//  // controls display of construction plane axes
//  bool ShowConstructionAxes() const;
//  void SetShowConstructionAxes(BOOL);

//  // controls display of world axes icon
//  bool ShowWorldAxes() const;
//  void SetShowWorldAxes(BOOL);

//  // viewports are either x-up, y-up, or z-up
//  bool IsXUp() const;
//  bool IsYUp() const;
//  bool IsZUp() const;

//  // Description:
//  //   Calculate elevator mode point
//  // Parameters:
//  //     world_line - [in] line from near to far clipping plane that
//  //         projects to mouse point.
//  //     elevator_basepoint - [in]
//  //     elevator_axis - [in] unit vector
//  //     bGridSnap - [in] true if grid snap is enabled.  When in
//  //        doubt, use value of RhinoApp().AppSettings().GridSnap().
//  //     grid_snap_spacing - [in] grid snap distance.  When in
//  //        doubt, use value of ConstructionPlane().m_snap_spacing
//  //     elevator_height -[out] elevator height returned here.
//  // Returns:
//  //   true if point is successfully calculated.  The 3d location
//  //   is elevator_basepoint + elevator_height*elevator_axis.
//  bool GetElevatorHeight( 
//    const ON_Line& world_line,
//    const ON_3dPoint& elevator_basepoint, 
//    const ON_3dVector& elevator_axis,
//    BOOL bGridSnap,
//    double grid_snap_spacing,
//    double* elevator_height
//    ) const;

//  //////////
//  // Default views
//  bool SetToPlanView( 
//          const ON_3dPoint&,  // plane origin
//          const ON_3dVector&, // plane xaxis
//          const ON_3dVector&, // plane yaxis
//          BOOL = false        // bSetConstructionPlane
//          );

//  // Description:
//  //   Set view to parallel top view projection.
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToTopView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to parallel projection with world m_y 
//  //   pointing down and world m_x pointing to the right.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToBottomView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to parallel projection with world m_z 
//  //   pointing up and world m_y pointing to the left.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToLeftView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to parallel projection with world m_z 
//  //   pointing up and world m_y pointing to the right.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToRightView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to parallel projection with world m_z 
//  //   pointing up and world m_x pointing to the right.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToFrontView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to parallel projection with world m_z 
//  //   pointing up and world m_x pointing to the left.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the view plane.
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToBackView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Set view to bird's eye perspective projection.
//  // Parameters:
//  //   sViewName - [in] if not NULL, the view name is set
//  // Parameters:
//  //   sViewName - [in] if not NULL, then view name is also set
//  //   bUpdateConstructionPlane - [in] if true, then the
//  //       construction plane is set to the world XY plane
//  // Returns:
//  //   true if successful
//  // See Also
//  //  CRhinoViewport::SetConstructionPlane, CRhinoViewport::SetName,
//  //  CRhinoViewport::IsXUp, CRhinoViewport::IsYUp, CRhinoViewport::IsZUp
//  bool SetToPerspectiveView(
//    const wchar_t* sViewName = NULL,
//    BOOL bUpdateConstructionPlane = true
//    );


//  /*
//  Description:
//    Set view to the Rhino default two point perspective projection.
//  Parameters:
//    sViewName - [in] if not NULL, the view name is set
//    bUpdateConstructionPlane - [in]
//      Set construction plane to be parallel to ground plane.
//  Returns:
//    true if successful
//  */
//  bool SetToTwoPointPerspectiveView(
//    const wchar_t* sViewName,
//    bool bUpdateConstructionPlane = true
//    );

//  // Description:
//  //   Appends the current view projection and target to the
//  //   viewport's view stack.
//  void PushViewProjection();

//  // Description:
//  //   Sets the view projection and target to the settings
//  //   at the top of the view stack and removes those settings
//  //   from the view stack.
//  bool PopViewProjection();

//  // Description:
//  //   Sets the view projection and target to the settings that
//  //   were active before the last call to PrevView.
//  // Returns:
//  //   true if the view stack was popped.
//  bool NextViewProjection();

//  // Description:
//  //   Sets the view projection and target to the settings that
//  //   were active before the last call to NextViewProjection.
//  // Returns:
//  //   true if the view stack was popped.
//  bool PrevViewProjection();


//  /*
//  Description:
//    Clears saved view projections and cplanes.
//  Parameters:
//    bClearProjections - [in] if true, then saved view projections
//                             are cleared.
//    bClearCPlanes - [in] if true, then saved construction planes
//                         are cleared.
//  Remarks:
//    This function should be used only in special circumstances,
//    like when a new file is read.  Calling this function destroys
//    the information needed in commands likeUndoView and 
//    CPlaneNext/CPlanePrevious.
//  */
//  void ClearUndoInformation( 
//    bool bClearProjections = true, 
//    bool bClearCPlanes = true 
//    );

//  //////////
//  // returns true if construction plane z axis is parallel
//  // to camera direction.
//  bool IsPlanView() const;

//  /*
//  Description:
//    Dolly the camera (change its location) so that the 
//    bbox is centered in the viewport and fills the 
//    central region of the viewport.  The "target" point
//    is updated to be the center of the bounding box.
//  Paramters:
//    bbox - [in]
//    cs - [in] coordinates system of the bounding box.
//      NOTE WELL:
//        If cs is anything besides ON::world_cs, the
//        the coodinates are with respect to the current
//        projection returned by this->VP().  Note
//        that the call to DollyExtents will change the 
//        projection so that any non-world coordinate
//        bbox will not be valid after the function 
//        returns.  If this is confusing, then restrict
//        yourself to using world coordinate bounding boxes
//        and cs = ON::world_cs.
//  Remarks:
//    The Rhino "ZoomExtents" command uses DollyExtents
//    to calculate the view projection.  Technically,
//    "zoom" would leave the camera location fixed and
//    modify the camera angle.
//  */
//  bool DollyExtents( 
//    ON_BoundingBox bbox,
//    ON::coordinate_system cs
//    );

//  // keyboard arrow key interaction tools
//  bool LeftRightDolly( double );  // left < 0 < right
//  bool DownUpDolly( double );     // down < 0 < up
//  bool InOutDolly( double );     // out < 0 < in


//  /*
//  Description:
//    Set the model transformation that is applied to geometry before
//    it is drawn.
//  Parameters:
//    model_xform - [in]
//  Remarks:
//    The default model transformation is the identity.  The
//    model transformation is intended to be used for dynamic
//    drawing of objects.  The camera and projection transformations
//    are handled in the m_v settings.
//  See Also:
//    CRhinoViewport::SetDisplayXform
//    CRhinoViewport::GetModelXform
//  */
//  void SetModelXform( const ON_Xform& model_xform );

//  /*
//  Description:
//    Input the model transformation that is applied to geometry before
//    it is drawn.
//  Parameters:
//    model_xform - [out]
//  Remarks:
//    The default model transformation is the identity.  The
//    model transformation is intended to be used for dynamic
//    drawing of objects.  The camera and projection transformations
//    are handled in the m_v settings.
//  See Also:
//    CRhinoViewport::SetModelXform
//  */
//  void GetModelXform( ON_Xform& model_xform ) const;

//  /*
//  Description:
//    Set the display transformation that is applied to the projected
//    geometry immediately before it is drawn.  
//  Parameters:
//    display_xform - [in]
//  Remarks:
//    The default display transformation is the identity.  The
//    display transformation is intended to be used in printing
//    applications.  The camera and projection transformations
//    are handled in the m_v settings.
//  See Also:
//    CRhinoViewport::SetModelXform
//    CRhinoViewport::GetDisplayXform
//  */
//  void SetDisplayXform( const ON_Xform& display_xform );

//  /*
//  Description:
//    Input the display transformation that is applied to the projected
//    geometry immediately before it is drawn.  
//  Parameters:
//    display_xform - [out]
//  Remarks:
//    The default display transformation is the identity.  The
//    display transformation is intended to be used in printing
//    applications.  The camera and projection transformations
//    are handled in the m_v settings.
//  See Also:
//    CRhinoViewport::SetDisplayXform
//  */
//  void GetDisplayXform( ON_Xform& display_xform ) const;

//  /*
//  Description:
//    Set the display transformation that is applied to marked objects.
//  Parameters:
//    mark_value - [in] if not zero, then objects with this mark value
//            are displayed using the marked_object_xform.
//    marked_object_xform - [in]
//  Remarks:
//    The default marked object transformation is the identity.
//    The marked object transformation is intended to be used when
//    inserting models.
//  See Also:
//    CRhinoViewport::SetModelXform
//    CRhinoViewport::GetMarkedObjectXform
//  */
//  void SetMarkedObjectXform( 
//          int mark_value,
//          const ON_Xform& marked_object_xform 
//          );

//  /*
//  Description:
//    Input the display transformation that is applied to marked objects.
//  Parameters:
//    mark_value - [out]
//    marked_object_xform - [out]
//  See Also:
//    CRhinoViewport::SetMarkedObjectXform
//  */
//  void GetMarkedObjectXform( 
//        int* mark_value, 
//        ON_Xform& marked_object_xform 
//        ) const;

//  // picking

//  //////////
//  // GetPickXform takes a rectangle in screen coordinates and returns 
//  // a transformation that maps the 3d frustum defined by the rectangle
//  // to a -1/+1 clipping coordinate box.
//  bool GetPickXform( 
//         int, int, // screen coordinates of a mouse click
//         ON_Xform& 
//         ) const;
//  bool GetPickXform( 
//    const RECT&, // screen coordinates of a rectangle defining the picking frustum
//    ON_Xform& 
//    ) const;

//  CRhinoDisplayPipeline*        DisplayPipeline(void) const;


//  //Description:
//  //  Convert a point in parent CRhinoView client window coordinates to the ON_Viewport screen port
//  //  client coordinates. The screen port of a CRhinoViewport may not match the client area of
//  //  the parent CRhinoView. This occurs in cases when the CRhinoViewport is a nested child viewport
//  //Parameters:
//  //  pt [in/out]: point in client coordinates of parent CRhinoView window as input. This is
//  //               converted to the screen port client coordinates of the ON_Viewport as output
//  //Returns:
//  //  true if the point is inside of the CRhinoViewport's screen port rectangle
//  bool ClientToScreenPort(CPoint& pt) const;

//  // Description:
//  //   Determines if this viewport is the main viewport for a page view. Page views
//  //   should be able to be panned and zoomed, but not rotated or changed to a
//  //   different camera view vector. This is used by functions that attempt to set the
//  //   viewport's projection. If the projection is not compatible with a page view and
//  //   IsPageViewMainViewport() == true, then the setting of the projection is not allowed
//  // Return:
//  //   true if thie viewport is the main viewport for a CRhinoPageView
//  bool IsPageViewMainViewport() const;

//  // Description:
//  //   Similar to an operator= except this will not modify the viewport id unless told to.
//  //   Items not copied are: m_draw_step, m_vp_iteration, m_xform_iteration, m_dp
//  //                         m_view_stack, m_view_stack_index, m_cplane_stack, m_cplane_stack_index
//  // Parameters:
//  //   src [in]: CRhinoViewport ot copy information from
//  //   copy_id [in]: If true, this viewport's id will be changed to match that of src. This is
//  //                 typically NOT something that you want to do. This set to true for operations
//  //                 like printing in order to work with a temporary CRhinoViewport that is an
//  //                 exact duplicate of teh src viewport
//  void CopyFrom(const CRhinoViewport& src, bool copy_id = false);

//  // Description:
//  //   Determines if printing or "print display" is active for this viewport
//  // Parameters:
//  //   thickness_scale [out] - optional. Scale applied to an object's plot thickness for
//  //                           displaying on the screen or printing
//  // Returns:
//  //   true if printing or print display is active for this viewport
//  bool IsThickDisplayActive( double* thickness_scale = NULL ) const;

//  // Pipeline adds...
//  mutable CRhinoDisplayPipeline*    m_dp;

//  // 21 Sep 2006 Dale Lear:
//  //   "m_bSpecialView" is a silly name.  What this really means is
//  //   m_bSpecialView = true -> view is a page view
//  //   m_bSpecialView = false -> view is a model view.
//  //   This will be changed in the next few weeks so all code
//  //   looks at m_v.m_view_type to determine what space is
//  //   in the view and m_bSpecialView will be deleted.
//  bool m_bSpecialView;

//  ON_3dmView    m_v; // viewport settings
//};