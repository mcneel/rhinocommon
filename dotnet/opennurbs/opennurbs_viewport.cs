using System;

namespace Rhino.DocObjects
{
  /// <summary>
  /// Represents a viewing frustum
  /// </summary>
  public sealed class ViewportInfo : IDisposable
  {
    object m_parent = null;
    IntPtr m_pViewportPointer = IntPtr.Zero;

    internal IntPtr ConstPointer()
    {
      if( m_pViewportPointer!=IntPtr.Zero )
        return m_pViewportPointer;

      ViewInfo vi = m_parent as ViewInfo;
      if (vi != null)
      {
        return vi.ConstViewportPointer();
      }
      throw new Rhino.Runtime.DocumentCollectedException();
    }

    internal IntPtr NonConstPointer()
    {
      if (m_pViewportPointer == IntPtr.Zero)
      {
        ViewInfo vi = m_parent as ViewInfo;
        if (vi != null)
        {
          return vi.NonConstViewportPointer();
        }
        throw new Rhino.Runtime.DocumentCollectedException();
      }
      return m_pViewportPointer;
    }

    public ViewportInfo()
    {
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(IntPtr.Zero);
    }

    public ViewportInfo(ViewportInfo other)
    {
      IntPtr pOther = other.ConstPointer();
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(pOther);
    }

    /// <summary>
    /// Copies all of the ViewportInfo data from an existing RhinoViewport
    /// </summary>
    /// <param name="rhinoViewport"></param>
    public ViewportInfo(Rhino.Display.RhinoViewport rhinoViewport)
    {
      IntPtr pRhinoViewport = rhinoViewport.ConstPointer();
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New2(pRhinoViewport);
    }

    internal ViewportInfo(IntPtr pONViewport)
    {
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(pONViewport);
    }

    internal ViewportInfo(ViewInfo parent)
    {
      m_parent = parent;
    }

    const int idxIsValidCamera = 0;
    const int idxIsValidFrustum = 1;
    const int idxIsValid = 2;
    const int idxIsPerspectiveProjection = 3;
    const int idxIsParallelProjection = 4;
    const int idxIsCameraLocationLocked = 5;
    const int idxIsCameraDirectionLocked = 6;
    const int idxIsCameraUpLocked = 7;
    const int idxIsFrustumLeftRightSymmetric = 8;
    const int idxIsFrustumTopBottomSymmetric = 9;

    bool GetBool(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Viewport_GetBool(pConstThis, which);
    }
    

    public bool IsValidCamera
    {
      get { return GetBool(idxIsValidCamera); }
    }

    public bool IsValidFrustum
    {
      get { return GetBool(idxIsValidFrustum); }
    }

    public bool IsValid
    {
      get { return GetBool(idxIsValid); }
    }

    public bool IsPerspectiveProjection
    {
      get { return GetBool(idxIsPerspectiveProjection); }
    }

    public bool IsParallelProjection
    {
      get { return GetBool(idxIsParallelProjection); }
    }

    public bool IsTwoPointPerspectiveProjection
    {
      get { return IsPerspectiveProjection && IsCameraUpLocked && IsFrustumLeftRightSymmetric && !IsFrustumTopBottomSymmetric; }
    }

    /// <summary>
    /// Use this function to change projections of valid viewports
    /// from parallel to perspective.  It will make common additional
    /// adjustments to the frustum and camera location so the resulting
    /// views are similar.  The camera direction and target point are
    /// not be changed.
    /// If the current projection is parallel and symmetricFrustum,
    /// FrustumIsLeftRightSymmetric() and FrustumIsTopBottomSymmetric()
    /// are all equal, then no changes are made and true is returned.
    /// </summary>
    /// <param name="symmetricFrustum">true if you want the resulting frustum to be symmetric.</param>
    /// <returns></returns>
    public bool ChangeToParallelProjection(bool symmetricFrustum)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToParallelProjection(pThis, symmetricFrustum);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports
    /// from parallel to perspective.  It will make common additional
    /// adjustments to the frustum and camera location so the resulting
    /// views are similar.  The camera direction and target point are
    /// not changed.
    /// If the current projection is perspective and symmetricFrustum,
    /// IsFrustumIsLeftRightSymmetric, and IsFrustumIsTopBottomSymmetric
    /// are all equal, then no changes are made and true is returned.
    /// </summary>
    /// <param name="targetDistance">
    /// If RhinoMath.UnsetValue this parameter is ignored.
    /// Otherwise it must be > 0 and indicates which plane in the current view frustum should be perserved.
    /// </param>
    /// <param name="symmetricFrustum">
    /// True if you want the resulting frustum to be symmetric.
    /// </param>
    /// <param name="lensLength">(pass 50.0 when in doubt)
    /// 35 mm lens length to use when changing from parallel
    /// to perspective projections. If the current projection
    /// is perspective or lens_length is &lt;= 0.0,
    /// then this parameter is ignored.</param>
    /// <returns></returns>
    public bool ChangeToPerspectiveProjection( double targetDistance, bool symmetricFrustum, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToPerspectiveProjection(pThis, targetDistance, symmetricFrustum, lensLength);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports
    /// to a two point perspective.  It will make common additional
    /// adjustments to the frustum and camera location and direction
    /// so the resulting views are similar.
    /// If the current projection is perspective and
    /// IsFrustumIsLeftRightSymmetric is true and
    /// IsFrustumIsTopBottomSymmetric is false, then no changes are
    /// made and true is returned.
    /// </summary>
    /// <param name="targetDistance">
    /// If RhinoMath.UnsetValue this parameter is ignored.  Otherwise
    /// it must be > 0 and indicates which plane in the current 
    /// view frustum should be perserved.
    /// </param>
    /// <param name="up">
    /// The locked up direction. Pass Vector3d.Zero if you want to use the world
    /// axis direction that is closest to the current up direction.
    /// Pass CameraY() if you want to preserve the current up direction.
    /// </param>
    /// <param name="lensLength">
    /// (pass 50.0 when in doubt)
    /// 35 mm lens length to use when changing from parallel
    /// to perspective projections. If the current projection
    /// is perspective or lens_length is &lt;= 0.0,
    /// then this parameter is ignored.
    /// </param>
    /// <returns></returns>
    public bool ChangeToTwoPointPerspectiveProjection(double targetDistance, Rhino.Geometry.Vector3d up, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToTwoPointPerspectiveProjection(pThis, targetDistance, up, lensLength);
    }

    public Rhino.Geometry.Point3d CameraLocation
    {
      get
      {
        Rhino.Geometry.Point3d loc = new Rhino.Geometry.Point3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraLocation(pConstThis, ref loc);
        return loc;
      }
    }

    public bool SetCameraLocation(Rhino.Geometry.Point3d location)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetCameraLocation(pThis, location);
    }

    public Rhino.Geometry.Vector3d CameraDirection
    {
      get
      {
        Rhino.Geometry.Vector3d loc = new Rhino.Geometry.Vector3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraDirection(pConstThis, ref loc);
        return loc;
      }
    }

    public bool SetCameraDirection(Rhino.Geometry.Vector3d direction)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetCameraDirection(pThis, direction);
    }


    public Rhino.Geometry.Vector3d CameraUp
    {
      get
      {
        Rhino.Geometry.Vector3d loc = new Rhino.Geometry.Vector3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraUp(pConstThis, ref loc);
        return loc;
      }
    }

    public bool SetCameraUp(Rhino.Geometry.Vector3d up)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetCameraUp(pThis, up);
    }

    const int idxCameraLocationLock = 0;
    const int idxCameraDirectionLock = 1;
    const int idxCameraUpLock = 2;
    void SetCameraLock(int which, bool val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Viewport_SetLocked(pThis, which, val);
    }

    public bool IsCameraLocationLocked
    {
      get { return GetBool(idxIsCameraLocationLocked); }
      set { SetCameraLock(idxCameraLocationLock, value); }
    }

    public bool IsCameraDirectionLocked
    {
      get { return GetBool(idxIsCameraDirectionLocked); }
      set { SetCameraLock(idxCameraDirectionLock, value); }
    }

    public bool IsCameraUpLocked
    {
      get { return GetBool(idxIsCameraUpLocked); }
      set { SetCameraLock(idxCameraUpLock, value); }
    }

    public bool IsFrustumLeftRightSymmetric
    {
      get { return GetBool(idxIsFrustumLeftRightSymmetric); }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetIsFrustumSymmetry(pThis, true, value);
      }
    }

    public bool IsFrustumTopBottomSymmetric
    {
      get { return GetBool(idxIsFrustumTopBottomSymmetric); }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetIsFrustumSymmetry(pThis, false, value);
      }
    }
    
    public void UnlockCamera()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Viewport_Unlock(pThis, true);
    }

    public void UnlockFrustumSymmetry()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Viewport_Unlock(pThis, false);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="cameraX"></param>
    /// <param name="cameraY"></param>
    /// <param name="cameraZ"></param>
    /// <returns>returns true if current camera orientation is valid</returns>
    public bool GetCameraFrame(out Rhino.Geometry.Point3d location,  out Rhino.Geometry.Vector3d cameraX, out Rhino.Geometry.Vector3d cameraY, out Rhino.Geometry.Vector3d cameraZ)
    {
      location = new Rhino.Geometry.Point3d(0, 0, 0);
      cameraX = new Rhino.Geometry.Vector3d(0, 0, 0);
      cameraY = new Rhino.Geometry.Vector3d(0, 0, 0);
      cameraZ = new Rhino.Geometry.Vector3d(0, 0, 0);
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Viewport_GetCameraFrame(pConstThis, ref location, ref cameraX, ref cameraY, ref cameraZ);
    }

    /// <summary>
    /// unit to right vector
    /// </summary>
    public Rhino.Geometry.Vector3d CameraX
    {
      get 
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraAxis(pConstThis, 0, ref v);
        return v;
      }
    }

    /// <summary>
    /// unit up vector
    /// </summary>
    public Rhino.Geometry.Vector3d CameraY
    {
      get
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraAxis(pConstThis, 1, ref v);
        return v;
      }
    }

    /// <summary>
    /// unit vector in -CameraDirection
    /// </summary>
    public Rhino.Geometry.Vector3d CameraZ
    {
      get
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_CameraAxis(pConstThis, 2, ref v);
        return v;
      }
    }

    //TODO
    //bool IsCameraFrameWorldPlan( 
    //bool GetCameraExtents( 

    /// <summary>
    /// Set the view frustum.  If FrustumSymmetryIsLocked() is true
    /// and left != -right or bottom != -top, then they will be
    /// adjusted so the resulting frustum is symmetric.
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    /// <param name="top"></param>
    /// <param name="nearDistance"></param>
    /// <param name="farDistance"></param>
    /// <returns></returns>
    public bool SetFrustum( double left, double right, double bottom, double top, double nearDistance, double farDistance )
    {
      return UnsafeNativeMethods.ON_Viewport_SetFrustum(NonConstPointer(), left, right, bottom, top, nearDistance, farDistance);
    }

    public bool GetFrustum( out double left, out double right, out double bottom, out double top, out double nearDistance, out double farDistance )
    {
      left = 0;
      right = 0;
      bottom = 0;
      top = 0;
      nearDistance = 0;
      farDistance = 0;
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Viewport_GetFrustum(pConstThis, ref left, ref right, ref bottom, ref top, ref nearDistance, ref farDistance);
    }

    /// <summary>
    /// Setting FrustumAspect changes the larger of the frustum's width/height
    /// so that the resulting value of width/height matches the requested
    /// aspect.  The camera angle is not changed.  If you change the shape
    /// of the view port with a call SetScreenPort(), then you generally 
    /// want to call SetFrustumAspect() with the value returned by 
    /// GetScreenPortAspect().
    /// </summary>
    public double FrustumAspect
    {
      get
      {
        double dAspect = 0.0;
        IntPtr pConstThis = ConstPointer();
        if (!UnsafeNativeMethods.ON_Viewport_GetFrustrumAspect(pConstThis, ref dAspect))
          dAspect = 0;
        return dAspect;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetFrustumAspect(pThis, value);
      }
    }

    public Rhino.Geometry.Point3d FrustumCenter
    {
      get
      {
        Rhino.Geometry.Point3d cen = new Rhino.Geometry.Point3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_GetFrustumCenter(pConstThis, ref cen);
        return cen;
      }
    }

    const int idxFrustumLeft = 0;
    const int idxFrustumRight = 1;
    const int idxFrustumBottom = 2;
    const int idxFrustumTop = 3;
    const int idxFrustumNear = 4;
    const int idxFrustumFar = 5;
    const int idxFrustumMinimumDiameter = 6;
    const int idxFrustumMaximumDiameter = 7;
    double GetDouble(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Viewport_GetDouble(pConstThis, which);
    }

    public double FrustumLeft { get { return GetDouble(idxFrustumLeft); } }
    public double FrustumRight { get { return GetDouble(idxFrustumRight); } }
    public double FrustumBottom { get { return GetDouble(idxFrustumBottom); } }
    public double FrustumTop { get { return GetDouble(idxFrustumTop); } }
    public double FrustumNear { get { return GetDouble(idxFrustumNear); } }
    public double FrustumFar { get { return GetDouble(idxFrustumFar); } }

    public double FrustumWidth    { get { return FrustumRight - FrustumLeft; } }
    public double FrustumHeight   { get { return FrustumTop - FrustumBottom; } }

    public double FrustumMinimumDiameter { get { return GetDouble(idxFrustumMinimumDiameter); } }
    public double FrustumMaximumDiameter { get { return GetDouble(idxFrustumMaximumDiameter); } }

    public bool SetFrustumNearFar(Rhino.Geometry.BoundingBox boundingBox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarBoundingBox(pThis, boundingBox.Min, boundingBox.Max);
    }
    public bool SetFrustumNearFar(Rhino.Geometry.Point3d center, double radius)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarSphere(pThis, center, radius);
    }
    public bool SetFrustumNearFar(double nearDistance, double farDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFar(pThis, nearDistance, farDistance);
    }

    /// <summary>
    /// If needed, adjust the current frustum so it has the 
    /// specified symmetries and adjust the camera location
    /// so the target plane remains visible.
    /// </summary>
    /// <param name="isLeftRightSymmetric">If true, the frustum will be adjusted so left = -right.</param>
    /// <param name="isTopBottomSymmetric">If true, the frustum will be adjusted so top = -bottom.</param>
    /// <param name="targetDistance">
    /// If projection is not perspective or target_distance is RhinoMath.UnsetValue,
    /// then this parameter is ignored. If the projection is perspective and targetDistance
    /// is not RhinoMath.UnsetValue, then it must be > 0.0 and it is used to determine
    /// which plane in the old frustum will appear unchanged in the new frustum.
    /// </param>
    /// <returns>
    /// Returns true if the returned viewport has a frustum with the specified symmetries.
    /// </returns>
    public bool ChangeToSymmetricFrustum(bool isLeftRightSymmetric, bool isTopBottomSymmetric, double targetDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToSymmetricFrustum(pThis, isLeftRightSymmetric, isTopBottomSymmetric, targetDistance);
    }

    /// <summary>
    /// Get clipping distance of a point. This function ignores the
    /// current value of the viewport's near and far settings. If
    /// the viewport is a perspective projection, then it intersects
    /// the semi infinite frustum volume with the bounding box and
    /// returns the near and far distances of the intersection.
    /// If the viewport is a parallel projection, it instersects the
    /// infinte view region with the bounding box and returns the
    /// near and far distances of the projection.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="distance">distance of the point (can be &lt; 0)</param>
    /// <returns>True if the bounding box intersects the view frustum and
    /// near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.</returns>
    public bool GetPointDepth(Rhino.Geometry.Point3d point, out double distance)
    {
      IntPtr pConstThis = ConstPointer();
      double farDistance = 0;
      distance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetPointDepth(pConstThis, point, ref distance, ref farDistance, false);
    }

    /// <summary>
    /// Get near and far clipping distances of a bounding box.
    /// This function ignores the current value of the viewport's 
    /// near and far settings. If the viewport is a perspective
    /// projection, the it intersects the semi infinite frustum
    /// volume with the bounding box and returns the near and far
    /// distances of the intersection.  If the viewport is a parallel
    /// projection, it instersects the infinte view region with the
    /// bounding box and returns the near and far distances of the
    /// projection.
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="nearDistance">Near distance of the box. This value can be zero or 
    /// negative when the camera location is inside bbox.</param>
    /// <param name="farDistance">Far distance of the box. This value can be equal to 
    /// near_dist, zero or negative when the camera location is in front of the bounding box.</param>
    /// <returns>True if the bounding box intersects the view frustum and near_dist/far_dist were set. 
    /// False if the bounding box does not intesect the view frustum.</returns>
    public bool GetBoundingBoxDepth(Rhino.Geometry.BoundingBox bbox, out double nearDistance, out double farDistance)
    {
      IntPtr pConstThis = ConstPointer();
      nearDistance = 0;
      farDistance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetBoundingBoxDepth(pConstThis, bbox.Min, bbox.Max, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Get near and far clipping distances of a bounding sphere.
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="nearDistance">Near distance of the sphere (can be &lt; 0)</param>
    /// <param name="farDistance">Far distance of the sphere (can be equal to near_dist)</param>
    /// <returns>True if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// False if the sphere does not intesect the view frustum.</returns>
    public bool GetSphereDepth(Rhino.Geometry.Sphere sphere, out double nearDistance, out double farDistance)
    {
      IntPtr pConstThis = ConstPointer();
      nearDistance = 0;
      farDistance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetSphereDepth(pConstThis, sphere.Center, sphere.Radius, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Set near and far clipping distance subject to constraints.
    /// </summary>
    /// <param name="nearDistance">(>0) desired near clipping distance</param>
    /// <param name="farDistance">(>near_dist) desired near clipping distance</param>
    /// <param name="minNearDistance">
    /// If min_near_dist &lt;= 0.0, it is ignored.
    /// If min_near_dist &gt; 0 and near_dist &lt; min_near_dist, then the frustum's near_dist will be increased to min_near_dist.
    /// </param>
    /// <param name="minNearOverFar">
    /// If min_near_over_far &lt;= 0.0, it is ignored.
    /// If near_dist &lt; far_dist*min_near_over_far, then
    /// near_dist is increased and/or far_dist is decreased
    /// so that near_dist = far_dist*min_near_over_far.
    /// If near_dist &lt; target_dist &lt; far_dist, then near_dist
    /// near_dist is increased and far_dist is decreased so that
    /// projection precision will be good at target_dist.
    /// Otherwise, near_dist is simply set to 
    /// far_dist*min_near_over_far.
    /// </param>
    /// <param name="targetDistance">If target_dist &lt;= 0.0, it is ignored.
    /// If target_dist &gt; 0, it is used as described in the
    /// description of the min_near_over_far parameter.</param>
    /// <returns></returns>
    public bool SetFrustumNearFar(double nearDistance, double farDistance, double minNearDistance, double minNearOverFar, double targetDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustrumNearFar(pThis, nearDistance, farDistance, minNearDistance, minNearOverFar, targetDistance);
    }

    const int idxNearPlane = 0;
    const int idxFarPlane = 1;
    const int idxLeftPlane = 2;
    const int idxRightPlane = 3;
    const int idxBottomPlane = 4;
    const int idxTopPlane = 5;
    Rhino.Geometry.Plane GetPlane(int which)
    {
      IntPtr pConstThis = ConstPointer();
      Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
      if (!UnsafeNativeMethods.ON_Viewport_GetPlane(pConstThis, which, ref plane))
        plane = Rhino.Geometry.Plane.Unset;
      return plane;
    }

    /// <summary>
    /// Get near clipping plane if camera and frustum
    /// are valid.  The plane's frame is the same as the camera's
    /// frame.  The origin is located at the intersection of the
    /// camera direction ray and the near clipping plane. The plane's
    /// normal points out of the frustum towards the camera
    /// location.
    /// </summary>
    public Rhino.Geometry.Plane FrustumNearPlane
    {
      get { return GetPlane(idxNearPlane); }
    }

    /// <summary>
    /// Get far clipping plane if camera and frustum
    /// are valid.  The plane's frame is the same as the camera's
    /// frame.  The origin is located at the intersection of the
    /// camera direction ray and the far clipping plane. The plane's
    /// normal points into the frustum towards the camera location.
    /// </summary>
    public Rhino.Geometry.Plane FrustumFarPlane
    {
      get { return GetPlane(idxFarPlane); }
    }
    public Rhino.Geometry.Plane FrustumLeftPlane
    {
      get { return GetPlane(idxLeftPlane); }
    }
    public Rhino.Geometry.Plane FrustumRightPlane
    {
      get { return GetPlane(idxRightPlane); }
    }
    public Rhino.Geometry.Plane FrustumBottomPlane
    {
      get { return GetPlane(idxBottomPlane); }
    }
    public Rhino.Geometry.Plane FrustumTopPlane
    {
      get { return GetPlane(idxTopPlane); }
    }
        
    /// <summary>
    /// Get corners of near clipping plane rectangle.
    /// 4 points are returned in the order of bottom left, bottom right,
    /// top left, top right
    /// </summary>
    /// <returns>
    /// Four corner points on success.
    /// Empty array if viewport is not valid.
    /// </returns>
    public Rhino.Geometry.Point3d[] GetNearPlaneCorners()
    {
      Rhino.Geometry.Point3d leftBottom = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d rightBottom = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d leftTop = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d rightTop = new Rhino.Geometry.Point3d();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetNearFarRect(pConstThis, true, ref leftBottom, ref rightBottom, ref leftTop, ref rightTop))
        return new Rhino.Geometry.Point3d[0];
      return new Rhino.Geometry.Point3d[] { leftBottom, rightBottom, leftTop, rightTop };
    }

    /// <summary>
    /// Get corners of far clipping plane rectangle.
    /// 4 points are returned in the order of bottom left, bottom right,
    /// top left, top right
    /// </summary>
    /// <returns>
    /// Four corner points on success.
    /// Empty array if viewport is not valid.
    /// </returns>
    public Rhino.Geometry.Point3d[] GetFarPlaneCorners()
    {
      Rhino.Geometry.Point3d leftBottom = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d rightBottom = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d leftTop = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Point3d rightTop = new Rhino.Geometry.Point3d();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetNearFarRect(pConstThis, false, ref leftBottom, ref rightBottom, ref leftTop, ref rightTop))
        return new Rhino.Geometry.Point3d[0];
      return new Rhino.Geometry.Point3d[] { leftBottom, rightBottom, leftTop, rightTop };
    }

    /// <summary>
    /// Location of viewport in pixels.
    /// These are provided so you can set the port you are using
    /// and get the appropriate transformations to and from
    /// screen space.
    /// // For a Windows window
    /// /      int width = width of window client area in pixels;
    /// /      int height = height of window client area in pixels;
    /// /      port_left = 0;
    /// /      port_right = width;
    /// /      port_top = 0;
    /// /      port_bottom = height;
    /// /      port_near = 0;
    /// /      port_far = 1;
    /// /      SetScreenPort( port_left, port_right, 
    /// /                     port_bottom, port_top, 
    /// /                     port_near, port_far );
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right">(port_left != port_right)</param>
    /// <param name="bottom"></param>
    /// <param name="top">(port_top != port_bottom)</param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns>true if input is valid.</returns>
    public bool SetScreenPort( int left, int right, int bottom, int top, int near, int far)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetScreenPort(pThis, left, right, bottom, top, near, far);
    }

    public bool SetScreenPort(System.Drawing.Rectangle windowRectangle, int near, int far)
    {
      return SetScreenPort(windowRectangle.Left, windowRectangle.Right, windowRectangle.Bottom, windowRectangle.Top, near, far);
    }
    public bool SetScreenPort(System.Drawing.Rectangle windowRectangle)
    {
      return SetScreenPort(windowRectangle, 0, 0);
    }

    /// <summary>
    /// See documentation for SetScreenPort
    /// </summary>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns>empty rectangle on error</returns>
    public System.Drawing.Rectangle GetScreenPort(out int near, out int far)
    {
      int left = 0;
      int right = 0;
      int bottom = 0;
      int top = 0;
      near = 0;
      far = 0;
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetScreenPort(pConstThis, ref left, ref right, ref bottom, ref top, ref near, ref far))
        return System.Drawing.Rectangle.Empty;
      return System.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
    }

    public System.Drawing.Rectangle GetScreenPort()
    {
      int near = 0;
      int far = 0;
      return GetScreenPort(out near, out far);
    }

    public double ScreenPortAspect
    {
      get
      {
        double dAspect = 0.0;
        IntPtr pConstThis = ConstPointer();
        if (UnsafeNativeMethods.ON_Viewport_GetScreenPortAspect(pConstThis, ref dAspect))
          dAspect = 0;
        return dAspect;
      }
    }

    /// <summary>
    /// Field of view
    /// </summary>
    /// <param name="halfDiagonalAngleRadians">1/2 of diagonal subtended angle</param>
    /// <param name="halfVerticalAngleRadians">1/2 of vertical subtended angle</param>
    /// <param name="halfHorizontalAngleRadians">1/2 of horizontal subtended angle</param>
    /// <returns></returns>
    public bool GetCameraAngles( out double halfDiagonalAngleRadians, out double halfVerticalAngleRadians, out double halfHorizontalAngleRadians)
    {
      IntPtr pConstThis = ConstPointer();
      halfDiagonalAngleRadians = 0;
      halfHorizontalAngleRadians = 0;
      halfVerticalAngleRadians = 0;
      return UnsafeNativeMethods.ON_Viewport_GetCameraAngle2(pConstThis, ref halfDiagonalAngleRadians, ref halfVerticalAngleRadians, ref halfHorizontalAngleRadians);
    }

    /// <summary>
    /// Half smallest angle - use GetCameraAngle for more information
    /// </summary>
    public double CameraAngle
    {
      get
      {
        double d = 0.0;
        IntPtr pConstThis = ConstPointer();
        if (!UnsafeNativeMethods.ON_Viewport_GetCameraAngle(pConstThis, ref d))
          d = 0;
        return d;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetCameraAngle(pThis, value);
      }
    }

    /// <summary>
    /// This property assumes the camera is horizontal and crop the
    /// film rather than the image when the aspect of the frustum
    /// is not 36/24.  (35mm film is 36mm wide and 24mm high.)
    /// Setting preserves camera location,
    /// changes the frustum, but maintains the frsutrum's aspect.
    /// </summary>
    public double Camera35mmLensLength
    {
      get
      {
        double d = 0.0;
        IntPtr pConstThis = ConstPointer();
        if (!UnsafeNativeMethods.ON_Viewport_GetCamera35mmLensLength(pConstThis, ref d))
          d = 0;
        return d;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetCamera35mmLensLength(pThis, value);
      }
    }

    /// <summary>
    /// </summary>
    /// <param name="sourceSystem"></param>
    /// <param name="destinationSystem"></param>
    /// <returns>4x4 transformation matrix (acts on the left)</returns>
    public Rhino.Geometry.Transform GetXform(Rhino.DocObjects.CoordinateSystem sourceSystem, Rhino.DocObjects.CoordinateSystem destinationSystem)
    {
      Rhino.Geometry.Transform matrix = new Rhino.Geometry.Transform();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetXform(pConstThis, (int)sourceSystem, (int)destinationSystem, ref matrix))
        matrix = Rhino.Geometry.Transform.Unset;
      return matrix;
    }

    /// <summary>
    /// Get the world coordinate line in the view frustum
    /// that projects to a point on the screen.
    /// </summary>
    /// <param name="screenX">(screenx,screeny) = screen location</param>
    /// <param name="screenY">(screenx,screeny) = screen location</param>
    /// <returns>3d world coordinate line segment starting on the near clipping plane and ending on the far clipping plane</returns>
    Rhino.Geometry.Line GetFrustumLine( double screenX, double screenY)
    {
      Rhino.Geometry.Line line = new Rhino.Geometry.Line();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetFrustumLine(pConstThis, screenX, screenY, ref line))
        line = Rhino.Geometry.Line.Unset;
      return line;
    }

    Rhino.Geometry.Line GetFrustumLine(System.Drawing.Point screenPoint)
    {
      return GetFrustumLine(screenPoint.X, screenPoint.Y);
    }
    Rhino.Geometry.Line GetFrustumLine(System.Drawing.PointF screenPoint)
    {
      return GetFrustumLine(screenPoint.X, screenPoint.Y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointInFrustum">point in viewing frustum.</param>
    /// <returns>number of pixels per world unit at the 3d point</returns>
    double GetWorldToScreenScale( Rhino.Geometry.Point3d pointInFrustum)
    {
      double d = 0.0;
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetWorldToScreenScale(pConstThis, pointInFrustum, ref d))
        d = 0;
      return d;
    }

    //TODO bool GetCoordinateSprite(

    /// <summary>
    /// Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.
    /// </summary>
    /// <param name="halfViewAngleRadians"></param>
    /// <param name="bbox"></param>
    /// <returns></returns>
    public bool Extents( double halfViewAngleRadians, Rhino.Geometry.BoundingBox bbox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ExtentsBBox(pThis, halfViewAngleRadians, bbox.Min, bbox.Max);
    }

    /// <summary>
    /// Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.
    /// </summary>
    /// <param name="halfViewAngleRadians"></param>
    /// <param name="sphere"></param>
    /// <returns></returns>
    public bool Extents( double halfViewAngleRadians, Rhino.Geometry.Sphere sphere)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ExtentsSphere(pThis, halfViewAngleRadians, sphere.Center, sphere.Radius);
    }

    /// <summary>
    /// View changing from screen input points.  Handy for
    /// using a mouse to manipulate a view.
    /// ZoomToScreenRect() may change camera and frustum settings
    /// </summary>
    /// <param name="left">Screen coord</param>
    /// <param name="top">Screen coord</param>
    /// <param name="right">Screen coord</param>
    /// <param name="bottom">Screen coord</param>
    /// <returns></returns>
    public bool ZoomToScreenRect( int left, int top, int right, int bottom)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ZoomToScreenRect(pThis, left, top, right, bottom);
    }

    public bool ZoomToScreenRect(System.Drawing.Rectangle windowRectangle)
    {
      return ZoomToScreenRect(windowRectangle.Left, windowRectangle.Top, windowRectangle.Right, windowRectangle.Bottom);
    }

    /// <summary>
    /// DollyCamera() does not update the frustum's clipping planes.
    /// To update the frustum's clipping planes call DollyFrustum(d)
    /// with d = dollyVector o cameraFrameZ.  To convert screen locations
    /// into a dolly vector, use GetDollyCameraVector().
    /// Does not update frustum.  To update frustum use DollyFrustum(d) with d = dollyVector o cameraFrameZ
    /// </summary>
    /// <param name="dollyVector">dolly vector in world coordinates</param>
    /// <returns></returns>
    public bool DollyCamera(Rhino.Geometry.Vector3d dollyVector)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_DollyCamera(pThis, dollyVector);
    }

    /// <summary>
    /// Gets a world coordinate dolly vector that can be passed to DollyCamera().
    /// </summary>
    /// <param name="screenX0">screen coords of start point</param>
    /// <param name="screenY0">screen coords of start point</param>
    /// <param name="screenX1">screen coords of end point</param>
    /// <param name="screenY1">screen coords of end point</param>
    /// <param name="projectionPlaneDistance">distance of projection plane from camera. When in doubt, use 0.5*(frus_near+frus_far).</param>
    /// <returns>world coordinate dolly vector</returns>
    public Rhino.Geometry.Vector3d GetDollyCameraVector( int screenX0, int screenY0, int screenX1, int screenY1, double projectionPlaneDistance)
    {
      Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
      IntPtr pConstThis = ConstPointer();
      if (UnsafeNativeMethods.ON_Viewport_GetDollyCameraVector(pConstThis, screenX0, screenY0, screenX1, screenY1, projectionPlaneDistance, ref v))
        v = Rhino.Geometry.Vector3d.Unset;
      return v;
    }

    public Rhino.Geometry.Vector3d GetDollyCameraVector(System.Drawing.Point screen0, System.Drawing.Point screen1, double projectionPlaneDistance)
    {
      return GetDollyCameraVector(screen0.X, screen0.Y, screen1.X, screen1.Y, projectionPlaneDistance);
    }

    /// <summary>
    /// Moves frustum's clipping planes
    /// </summary>
    /// <param name="dollyDistance">distance to move in camera direction</param>
    /// <returns></returns>
    public bool DollyFrustum( double dollyDistance )
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_DollyFrustum(pThis, dollyDistance);
    }

    /// <summary>
    /// Apply scaling factors to parallel projection clipping coordinates
    /// by setting the m_clip_mod transformation. 
    /// If you want to compress the view projection across the viewing
    /// plane, then set x = 0.5, y = 1.0, and z = 1.0.
    /// </summary>
    public System.Drawing.SizeF ViewScale
    {
      get
      {
        double w = 0.0;
        double h = 0.0;
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_GetViewScale(pConstThis, ref w, ref h);
        return new System.Drawing.SizeF((float)w, (float)h);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetViewScale(pThis, (double)value.Width, (double)value.Height);
      }
    }
    
    /* Don't wrap until someone asks for this.
    /// <summary>
    /// Gets the m_clip_mod transformation
    /// </summary>
    public Rhino.Geometry.Transform ClipModTransform
    {
      get
      {
        Rhino.Geometry.Transform matrix = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.ON_Viewport_ClipModXform(ConstPointer(), ref matrix);
        return matrix;
      }
    }

    /// <summary>
    /// Gets the m_clip_mod inverse transformation
    /// </summary>
    public Rhino.Geometry.Transform ClipModInverseTransform
    {
      get
      {
        Rhino.Geometry.Transform matrix = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.ON_Viewport_ClipModInverseXform(ConstPointer(), ref matrix);
        return matrix;
      }
    }

    public bool IsClipModTransformIdentity
    {
      get
      {
        return 1==UnsafeNativeMethods.ON_Viewport_ClipModXformIsIdentity(ConstPointer());
      }
    }
    */

    /// <summary>
    /// Return a point on the central axis of the view frustum.
    /// This point is a good choice for a general purpose target point.
    /// </summary>
    /// <param name="targetDistance">If targetDistance > 0.0, then the distance from the returned
    /// point to the camera plane will be targetDistance. Note that
    /// if the frustum is not symmetric, the distance from the
    /// returned point to the camera location will be larger than
    /// targetDistance.
    /// If targetDistance == ON_UNSET_VALUE and the frustum
    /// is valid with near > 0.0, then 0.5*(near + far) will be used
    /// as the targetDistance.</param>
    /// <returns>A point on the frustum's central axis.  If the viewport or input
    /// is not valid, then ON_3dPoint::UnsetPoint is returned</returns>
    public Rhino.Geometry.Point3d FrustumCenterPoint( double targetDistance ) 
    {
      Rhino.Geometry.Point3d point = Rhino.Geometry.Point3d.Unset;
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.ON_Viewport_FrustumCenterPoint(pConstThis, targetDistance, ref point);
      return point;
    }

    /// <summary>
    /// The current value of the target point.  This point does not play
    /// a role in the view projection calculations.  It can be used as a 
    /// fixed point when changing the camera so the visible regions of the
    /// before and after frustums both contain the region of interest.
    /// The default constructor sets this point on ON_3dPoint::UnsetPoint.
    /// You must explicitly call one SetTargetPoint() functions to set
    /// the target point.
    /// </summary>
    public Rhino.Geometry.Point3d TargetPoint
    {
      get
      {
        Rhino.Geometry.Point3d point = new Rhino.Geometry.Point3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Viewport_TargetPoint(pConstThis, ref point);
        return point;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetTargetPoint(pThis, value);
      }
    }

    /// <summary>
    /// Get the distance from the target point to the camera plane.
    /// Note that if the frustum is not symmetric, then this distance
    /// is shorter than the distance from the target to the camera location.
    /// </summary>
    /// <param name="useFrustumCenterFallback">If bUseFrustumCenterFallback is false and the target point is
    /// not valid, then ON_UNSET_VALUE is returned.
    /// If bUseFrustumCenterFallback is true and the frustum is valid
    /// and current target point is not valid or is behind the camera,
    /// then 0.5*(near + far) is returned.</param>
    /// <returns>Shortest signed distance from camera plane to target point.
    /// If the target point is on the visible side of the camera,
    /// a positive value is returned.  ON_UNSET_VALUE is returned
    /// when the input of view is not valid.</returns>
    public double TargetDistance( bool useFrustumCenterFallback )
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Viewport_TargetDistance(pConstThis, useFrustumCenterFallback);
    }

    /*
    /// <summary>
    /// Get suggested values for setting the perspective minimum
    /// near distance and minimum near/far ratio.
    /// </summary>
    /// <param name="cameraLocation"></param>
    /// <param name="depthBufferBitDepth">Typically 32, 34, 16 or 8, but any value can be passed in.</param>
    /// <param name="minNearDist">Suggest value for passing to SetPerspectiveMinNearDist().  </param>
    /// <param name="minNearOverFar">Suggest value for passing to SetPerspectiveMinNearOverFar().    </param>
    public static void GetPerspectiveClippingPlaneConstraints( Rhino.Geometry.Point3d cameraLocation, 
                                                               int depthBufferBitDepth, 
                                                               ref double minNearDist, 
                                                               ref double minNearOverFar)
    {
      UnsafeNativeMethods.ON_Viewport_GetPerspectiveClippingPlaneConstraints(cameraLocation, depthBufferBitDepth, ref minNearDist, ref minNearOverFar);
    }

    /// <summary>
    /// Set suggested the perspective minimum near distance and
    /// minimum near/far ratio to the suggested values returned
    /// by GetPerspectiveClippingPlaneConstraints().
    /// </summary>
    /// <param name="depthBufferBitDepth">Typically 32, 34, 16 or 8, but any value can be passed in.</param>
    public void SetPerspectiveClippingPlaneConstraints( int depthBufferBitDepth)
    {
      UnsafeNativeMethods.ON_Viewport_SetPerspectiveClippingPlaneConstraints(NonConstPointer(), depthBufferBitDepth);
    }

    /// <summary>
    /// Expert user function to control the minimum
    /// ratio of near/far when perspective projections
    /// are begin used.
    /// </summary>
    public double PerspectiveMinNearOverFar
    {
      get
      {
        return UnsafeNativeMethods.ON_Viewport_GetPerspectiveMinNearOverFar(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetPerspectiveMinNearOverFar(NonConstPointer(), value);
      }
    }

    /// <summary>
    /// Expert user function to control the minimum
    /// value of near when perspective projections
    /// are begin used.
    /// </summary>
    public double PerspectiveMinNearDist
    {
      get
      {
        return UnsafeNativeMethods.ON_Viewport_GetPerspectiveMinNearDist(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetPerspectiveMinNearDist(NonConstPointer(), value);
      }
    }
    */

    /// <summary>
    /// Sets the viewport's id to the value used to 
    /// uniquely identify this viewport.
    /// There is no approved way to change the viewport 
    /// id once it is set in order to maintain consistency
    /// across multiple viewports and those routines that 
    /// manage them.
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Viewport_GetViewportId(pConstThis);
      }
      //set
      //{
      //  IntPtr pThis = NonConstPointer();
      //  UnsafeNativeMethods.ON_Viewport_SetViewportId(pThis, value);
      //}
    }

    /*
    public static double DefaultNearDistance        { get { return 0.005; } }
    public static double DefaultFarDistance         { get { return 1000.0; } } 
    public static double DefaultMinNearDistance     { get { return 0.0001; } }
    public static double DefaultMinNearOverFar      { get { return 0.0001; } } 
    */

    #region IDisposable implementation

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~ViewportInfo()
    {
      Dispose(false);
    }

    private void Dispose(bool isDisposing)
    {
      if (IntPtr.Zero!=m_pViewportPointer)
      {
        UnsafeNativeMethods.ON_Viewport_Delete(m_pViewportPointer);
      }
      m_pViewportPointer = IntPtr.Zero;
    }

    #endregion
  }
}



