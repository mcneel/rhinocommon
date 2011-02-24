using System;
using System.Diagnostics;
using System.Collections;

#if USING_RDK

/*
namespace Rhino.Display
{
  /// <summary>
  /// Wraps ON_Viewport
  /// </summary>
  public sealed class Viewport : IDisposable
  {
    private IntPtr m_pViewportPointer = IntPtr.Zero;
    private bool m_bAutoDelete = false;

    public Viewport(IntPtr pViewport, bool bAutoDelete)
    {
      m_pViewportPointer = pViewport;
      m_bAutoDelete = bAutoDelete;
    }

    public Viewport()
    {
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New();
      m_bAutoDelete = true;
    }
    

    public bool IsValidCamera
    {
      get { return UnsafeNativeMethods.ON_Viewport_IsValidCamera(ConstPointer()); }
    }

    public bool IsValidFrustum
    {
      get { return UnsafeNativeMethods.ON_Viewport_IsValidFrustum(ConstPointer()); }
    }

    public bool IsValid
    {
      get { return UnsafeNativeMethods.ON_Viewport_IsValid(ConstPointer()); }
    }

    /// <summary>
    /// The x/y/z_2pt_perspective_view projections are ordinary perspective projection. Using these values insures the ON_Viewport member
    /// fuctions properly constrain the camera up and camera direction vectors to preserve the specified perspective vantage.
    /// </summary>
    public enum ViewProjections : int
    { 
      None          = 0,
      Parallel      = 1,
      Perspective   = 2
    };

    ViewProjections Projection
    {
      get { return (ViewProjections)UnsafeNativeMethods.ON_Viewport_Projection(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetProjection((int)value); }
    }

    public bool IsPerspectiveProjection
    {
      get { return Projection == ViewProjections.Perspective; }
    }

    public bool IsParallelProjection
    {
      get { return Projection == ViewProjections.Parallel; }
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
    /// If the current projection is parallel and bSymmetricFrustum,
    /// FrustumIsLeftRightSymmetric() and FrustumIsTopBottomSymmetric()
    /// are all equal, then no changes are made and true is returned.
    /// </summary>
    /// <param name="symmetricFrustum">true if you want the resulting frustum to be symmetric.</param>
    /// <returns></returns>
    public bool ChangeToParallelProjection(bool symmetricFrustum)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_ChangeToParallelProjection(NonConstPointer(), symmetricFrustum);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports
    /// from parallel to perspective.  It will make common additional
    /// adjustments to the frustum and camera location so the resulting
    /// views are similar.  The camera direction and target point are
    /// not be changed.
    /// If the current projection is perspective and bSymmetricFrustum,
    /// FrustumIsLeftRightSymmetric() and FrustumIsTopBottomSymmetric()
    /// are all equal, then no changes are made and true is returned.
    /// </summary>
    /// <param name="targetDistance">If ON_UNSET_VALUE this parameter is ignored.  Otherwise it must be > 0 and indicates which plane in the current view frustum should be perserved.</param>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <param name="lensLength">(pass 50.0 when in doubt)
    /// 35 mm lens length to use when changing from parallel
    /// to perspective projections. If the current projection
    /// is perspective or lens_length is &lt;= 0.0,
    /// then this parameter is ignored.</param>
    /// <returns></returns>
    public bool ChangeToPerspectiveProjection( double targetDistance, bool symmetricFrustum, double lensLength)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_ChangeToPerspectiveProjection(NonConstPointer(), targetDistance, symmetricFrustum, lensLength);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports
    /// to a two point perspective.  It will make common additional
    /// adjustments to the frustum and camera location and direction
    /// so the resulting views are similar.
    /// If the current projection is perspective and 
    /// FrustumIsLeftRightSymmetric() is true and
    /// FrustumIsTopBottomSymmetric() is false, then no changes are
    /// made and true is returned.
    /// </summary>
    /// <param name="targetDistance">If ON_UNSET_VALUE this parameter is ignored.  Otherwise
    /// it must be > 0 and indicates which plane in the current 
    /// view frustum should be perserved.</param>
    /// <param name="up">This direction will be the locked up direction.  Pass 
    /// ON_3dVector::ZeroVector if you want to use the world axis
    /// direction that is closest to the current up direction.
    /// Pass CameraY() if you want to preserve the current up direction.</param>
    /// <param name="lensLength">(pass 50.0 when in doubt)
    /// 35 mm lens length to use when changing from parallel
    /// to perspective projections. If the current projection
    /// is perspective or lens_length is &lt;= 0.0,
    /// then this parameter is ignored.</param>
    /// <returns></returns>
    public bool ChangeToTwoPointPerspectiveProjection(double targetDistance, Rhino.Geometry.Vector3d up, double lensLength)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_ChangeToTwoPointPerspectiveProjection(NonConstPointer(), targetDistance, up, lensLength);
    }

    public Rhino.Geometry.Point3d CameraLocation
    {
      get
      {
        Rhino.Geometry.Point3d loc = new Rhino.Geometry.Point3d();
        UnsafeNativeMethods.ON_Viewport_CameraLocation(ConstPointer(), ref loc);
        return loc;
      }
      set
      {
        if (1!=UnsafeNativeMethods.ON_Viewport_SetCameraLocation(NonConstPointer(), value))
        {
          throw new InvalidOperationException("Cannot get camera location");
        }
      }
    }

    public Rhino.Geometry.Vector3d CameraDirection
    {
      get
      {
        Rhino.Geometry.Vector3d loc = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.ON_Viewport_CameraDirection(ConstPointer(), ref loc);
        return loc;
      }
      set
      {
        if (1!=UnsafeNativeMethods.ON_Viewport_SetCameraDirection(NonConstPointer(), value))
        {
          throw new InvalidOperationException("Cannot get camera direction");
        }
      }
    }

    public Rhino.Geometry.Vector3d CameraUp
    {
      get
      {
        Rhino.Geometry.Vector3d loc = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.ON_Viewport_CameraUp(ConstPointer(), ref loc);
        return loc;
      }
      set
      {
        if (1!=UnsafeNativeMethods.ON_Viewport_SetCameraUp(NonConstPointer(), value))
        {
          throw new InvalidOperationException("Cannot get camera up vector");
        }
      }
    }

    public bool IsCameraLocationLocked
    {
      get { return 1==UnsafeNativeMethods.ON_Viewport_CameraLocationLocked(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetCameraLocationLocked(NonConstPointer(), value); }
    }

    public bool IsCameraDirectionLocked
    {
      get { return 1==UnsafeNativeMethods.ON_Viewport_CameraDirectionLocked(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetCameraDirectionLocked(NonConstPointer(), value); }
    }

    public bool IsCameraUpLocked
    {
      get { return 1==UnsafeNativeMethods.ON_Viewport_CameraUpLocked(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetCameraUpLocked(NonConstPointer(), value); }
    }

    public bool IsFrustumLeftRightSymmetric
    {
      get { return 1==UnsafeNativeMethods.ON_Viewport_IsFrustumLeftRightSymmetric(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetIsFrustumLeftRightSymmetric(NonConstPointer(), value); }
    }

    public bool IsFrustumTopBottomSymmetric
    {
      get { return 1==UnsafeNativeMethods.ON_Viewport_IsFrustumTopBottomSymmetric(ConstPointer()); }
      set { UnsafeNativeMethods.ON_Viewport_SetIsFrustumTopBottomSymmetric(NonConstPointer(), value); }
    }

    public void UnlockCamera()
    {
      UnsafeNativeMethods.ON_Viewport_UnlockCamera(NonConstPointer());
    }

    public void UnlockFrustumSymmetry()
    {
      UnsafeNativeMethods.ON_Viewport_UnlockFrustumSymmetry(NonConstPointer());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="location"></param>
    /// <param name="cameraX"></param>
    /// <param name="cameraY"></param>
    /// <param name="cameraZ"></param>
    /// <returns>returns true if current camera orientation is valid</returns>
    public bool GetCameraFrame(ref Rhino.Geometry.Point3d location, ref Rhino.Geometry.Vector3d cameraX, ref Rhino.Geometry.Vector3d cameraY, ref Rhino.Geometry.Vector3d cameraZ)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetCameraFrame(ConstPointer(), location, cameraX, cameraY, cameraZ);
    }

    /// <summary>
    /// unit to right vector
    /// </summary>
    public Rhino.Geometry.Vector3d CameraX
    {
      get 
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.ON_Viewport_CameraAxis(ConstPointer(), 0, ref v);
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
        UnsafeNativeMethods.ON_Viewport_CameraAxis(ConstPointer(), 1, ref v);
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
        UnsafeNativeMethods.ON_Viewport_CameraAxis(ConstPointer(), 2, ref v);
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
      return 1==UnsafeNativeMethods.ON_Viewport_SetFrustum(NonConstPointer(), left, right, bottom, top, nearDistance, farDistance);
    }

    public bool GetFrustum( ref double left, ref double right, ref double bottom, ref double top, ref double nearDistance, ref double farDistance )
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetFrustum(ConstPointer(), ref left, ref right, ref bottom, ref top, ref nearDistance, ref farDistance);
    }

    /// <summary>
    /// SetFrustumAspect() changes the larger of the frustum's widht/height
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
        if (1!=UnsafeNativeMethods.ON_Viewport_GetFrustrumAspect(ConstPointer(), ref dAspect))
        {
          throw new InvalidOperationException("Cannot get frustrum aspect");
        }
        return dAspect;
      }
      set
      {
        if (1!=UnsafeNativeMethods.ON_Viewport_SetFrustumAspect(NonConstPointer(), value))
        {
          throw new InvalidOperationException("Cannot set frustrum aspect");
        }
      }
    }

    public Rhino.Geometry.Point3d FrustumCenter
    {
      get
      {
        Rhino.Geometry.Point3d cen = new Rhino.Geometry.Point3d();
        if (1!=UnsafeNativeMethods.ON_Viewport_GetFrustumCenter(ConstPointer(), ref cen))
        {
          throw new InvalidOperationException("Cannot get frustrum center");
        }
        return cen;
      }
    }

    public double FrustumLeft     { get { return UnsafeNativeMethods.ON_Viewport_FrustumLeft(ConstPointer()); } }
    public double FrustumRight    { get { return UnsafeNativeMethods.ON_Viewport_FrustumRight(ConstPointer()); } }
    public double FrustumBottom   { get { return UnsafeNativeMethods.ON_Viewport_FrustumBottom(ConstPointer()); } }
    public double FrustumTop      { get { return UnsafeNativeMethods.ON_Viewport_FrustumTop(ConstPointer()); } }
    public double FrustumNear     { get { return UnsafeNativeMethods.ON_Viewport_FrustumNear(ConstPointer()); } }
    public double FrustumFar      { get { return UnsafeNativeMethods.ON_Viewport_FrustumFar(ConstPointer()); } }

    public double FrustumWidth    { get { return FrustumRight - FrustumLeft; } }
    public double FrustumHeight   { get { return FrustumTop - FrustumBottom; } }

    public double FrustumMinimumDiameter { get { return UnsafeNativeMethods.ON_Viewport_FrustumMinimumDiameter(ConstPointer()); } }
    public double FrustumMaximumDiameter { get { return UnsafeNativeMethods.ON_Viewport_FrustumMaximumDiameter(ConstPointer()); } }

    public bool SetFrustumNearFar(Rhino.Geometry.BoundingBox boundingBox)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarBoundingBox(NonConstPointer(), boundingBox.Min, boundingBox.Max);
    }
    public bool SetFrustumNearFar(Rhino.Geometry.Point3d center, double radius)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarSphere(NonConstPointer(), center, radius);
    }
    public bool SetFrustumNearFar(double nearDistance, double farDistance)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_SetFrustumNearFar(NonConstPointer(), nearDistance, farDistance);
    }

    /// <summary>
    /// If needed, adjust the current frustum so it has the 
    /// specified symmetries and adjust the camera location
    /// so the target plane remains visible.
    /// </summary>
    /// <param name="isLeftRightSymmetric">If true, the frustum will be adjusted so left = -right.</param>
    /// <param name="isTopBottomSymmetric">If true, the frustum will be adjusted so top = -bottom.</param>
    /// <param name="targetDistance">If projection is not perspective or target_distance 
    ///   is ON_UNSET_VALUE, this this parameter is ignored. 
    ///   If the projection is perspective and target_distance 
    ///   is not ON_UNSET_VALUE, then it must be > 0.0 and
    ///   it is used to determine which plane in the old
    ///   frustum will appear unchanged in the new frustum.</param>
    /// <returns>Returns true if the returned viewport has a frustum
    /// with the specified symmetries.</returns>
    public bool ChangeToSymmetricFrustum(bool isLeftRightSymmetric, bool isTopBottomSymmetric, double targetDistance)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_ChangeToSymmetricFrustum(NonConstPointer(), isLeftRightSymmetric, isTopBottomSymmetric, targetDistance);
    }

    /// <summary>
    /// Get near and far clipping distances of a point.
    /// This function ignores the current value of the viewport's 
    /// near and far settings. If the viewport is a perspective
    /// projection, the it intersects the semi infinite frustum
    /// volume with the bounding box and returns the near and far
    /// distances of the intersection.  If the viewport is a parallel
    /// projection, it instersects the infinte view region with the
    /// bounding box and returns the near and far distances of the
    /// projection.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="nearDistance">near distance of the point (can be &lt; 0)</param>
    /// <param name="farDistance">far distance of the point (can be equal to near_dist)</param>
    /// <returns>True if the bounding box intersects the view frustum and
    /// near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.</returns>
    public bool GetPointDepth(Rhino.Geometry.Point3d point, ref double nearDistance, ref double farDistance)
    {
      return GetPointDepth(point, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Get near and far clipping distances of a point.
    /// This function ignores the current value of the viewport's 
    /// near and far settings. If the viewport is a perspective
    /// projection, the it intersects the semi infinite frustum
    /// volume with the bounding box and returns the near and far
    /// distances of the intersection.  If the viewport is a parallel
    /// projection, it instersects the infinte view region with the
    /// bounding box and returns the near and far distances of the
    /// projection.
    /// </summary>
    /// <param name="point"></param>
    /// <param name="nearDistance">near distance of the point (can be &lt; 0)</param>
    /// <param name="farDistance">far distance of the point (can be equal to near_dist)</param>
    /// <param name="growNearFar">If true and input values of near_dist and far_dist
    ///  are not ON_UNSET_VALUE, the near_dist and far_dist
    ///  are enlarged to include bbox.</param>
    /// <returns>True if the bounding box intersects the view frustum and
    /// near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.</returns>
    public bool GetPointDepth(Rhino.Geometry.Point3d point, ref double nearDistance, ref double farDistance, bool growNearFar)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_GetPointDepth(ConstPointer(), point, ref nearDistance, ref farDistance, growNearFar);
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
    public bool GetBoundingBoxDepth(Rhino.Geometry.BoundingBox bbox, ref double nearDistance, ref double farDistance)
    {
      return GetBoundingBoxDepth(bbox, ref nearDistance, ref farDistance, false);
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
    /// <param name="growNearFar">If true and input values of near_dist and far_dist 
    /// are not ON_UNSET_VALUE, the near_dist and far_dist are enlarged to include bbox.</param>
    /// <returns>True if the bounding box intersects the view frustum and near_dist/far_dist were set. 
    /// False if the bounding box does not intesect the view frustum.</returns>
    public bool GetBoundingBoxDepth(Rhino.Geometry.BoundingBox bbox, ref double nearDistance, ref double farDistance, bool growNearFar)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_GetBoundingBoxDepth(ConstPointer(), bbox.Min, bbox.Max, ref nearDistance, ref farDistance, growNearFar);
    }

    /// <summary>
    /// Get near and far clipping distances of a bounding sphere.
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="nearDistance">Near distance of the sphere (can be < 0)</param>
    /// <param name="farDistance">Far distance of the sphere (can be equal to near_dist)</param>
    /// <returns>True if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// False if the sphere does not intesect the view frustum.</returns>
    public bool GetSphereDepth(Rhino.Geometry.Sphere sphere, ref double nearDistance, ref double farDistance)
    {
      return GetSphereDepth(sphere, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Get near and far clipping distances of a bounding sphere.
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="nearDistance">Near distance of the sphere (can be < 0)</param>
    /// <param name="farDistance">Far distance of the sphere (can be equal to near_dist)</param>
    /// <param name="growNearFar">If true and input values of near_dist and far_dist are not 
    /// ON_UNSET_VALUE, the near_dist and far_dist are enlarged to include bbox.</param>
    /// <returns>True if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// False if the sphere does not intesect the view frustum.</returns>
    public bool GetSphereDepth(Rhino.Geometry.Sphere sphere, ref double nearDistance, ref double farDistance, bool growNearFar)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_GetSphereDepth(ConstPointer(), sphere.Center, sphere.Radius, ref nearDistance, ref farDistance, growNearFar);
    }

    /// <summary>
    /// Set near and far clipping distance subject to constraints.
    /// </summary>
    /// <param name="nearDistance">(>0) desired near clipping distance</param>
    /// <param name="farDistance">(>near_dist) desired near clipping distance</param>
    /// <param name="minNearDistance">If min_near_dist &lt;= 0.0, it is ignored. If min_near_dist &gt; 0 and near_dist &lt; min_near_dist, then the frustum's near_dist will be increased to min_near_dist.</param>
    /// <param name="minNearOverFar">If min_near_over_far &lt;= 0.0, it is ignored.
    /// If near_dist &lt; far_dist*min_near_over_far, then
    /// near_dist is increased and/or far_dist is decreased
    /// so that near_dist = far_dist*min_near_over_far.
    /// If near_dist &lt; target_dist &lt; far_dist, then near_dist
    /// near_dist is increased and far_dist is decreased so that
    /// projection precision will be good at target_dist.
    /// Otherwise, near_dist is simply set to 
    /// far_dist*min_near_over_far.</param>
    /// <param name="targetDistance">If target_dist &lt;= 0.0, it is ignored.
    /// If target_dist &gt; 0, it is used as described in the
    /// description of the min_near_over_far parameter.</param>
    /// <returns></returns>
    public bool SetFrustumNearFar(double nearDistance, double farDistance, double minNearDistance, double minNearOverFar, double targetDistance)
    {
      return 1 == UnsafeNativeMethods.ON_Viewport_SetFrustrumNearFar(NonConstPointer(), nearDistance, farDistance, minNearDistance, minNearOverFar, targetDistance);
    }

    /// <summary>
    /// Get near clipping plane if camera and frustum
    /// are valid.  The plane's frame is the same as the camera's
    /// frame.  The origin is located at the intersection of the
    /// camera direction ray and the near clipping plane. The plane's
    /// normal points out of the frustum towards the camera
    /// location.
    /// Throws InvalidOperationException if not valid.
    /// </summary>
    public Rhino.Geometry.Plane NearPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetNearPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get near plane");
        }
        return plane;
      }
    }

    /// <summary>
    /// Get far clipping plane if camera and frustum
    /// are valid.  The plane's frame is the same as the camera's
    ///      frame.  The origin is located at the intersection of the
    ///      camera direction ray and the far clipping plane. The plane's
    ///      normal points into the frustum towards the camera location.
    /// Throws InvalidOperationException if not valid.
    /// </summary>
    public Rhino.Geometry.Plane FarPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetFarPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get far plane");
        }
        return plane;
      }
    }

    public Rhino.Geometry.Plane FrustumLeftPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetFrustumLeftPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get frustum left plane");
        }
        return plane;
      }
    }
    public Rhino.Geometry.Plane FrustumRightPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetFrustumRightPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get frustum right plane");
        }
        return plane;
      }
    }
    public Rhino.Geometry.Plane FrustumBottomPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetFrustumBottomPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get frustum bottom plane");
        }
        return plane;
      }
    }
    public Rhino.Geometry.Plane FrustumTopPlane
    {
      get
      {
        Rhino.Geometry.Plane plane = new Rhino.Geometry.Plane();
        if (1 != UnsafeNativeMethods.ON_Viewport_GetFrustumTopPlane(ConstPointer(), ref plane))
        {
          throw new InvalidOperationException("Cannot get frustum top plane");
        }
        return plane;
      }
    }
        
    /// <summary>
    /// Get corners of near clipping plane rectangle.
    /// </summary>
    /// <param name="leftBottom"></param>
    /// <param name="rightBottom"></param>
    /// <param name="leftTop"></param>
    /// <param name="rightTop"></param>
    /// <returns>true if camera and frustum are valid.</returns>
    public bool GetNearRect(ref Rhino.Geometry.Point3d leftBottom,
                             ref Rhino.Geometry.Point3d rightBottom,
                             ref Rhino.Geometry.Point3d leftTop,
                             ref Rhino.Geometry.Point3d rightTop)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetNearRect(ConstPointer(), ref leftBottom, ref rightBottom, ref leftTop, ref rightTop);
    }

    /// <summary>
    /// Get corners of far clipping plane rectangle.
    /// </summary>
    /// <param name="leftBottom"></param>
    /// <param name="rightBottom"></param>
    /// <param name="leftTop"></param>
    /// <param name="rightTop"></param>
    /// <returns>true if camera and frustum are valid.</returns>
    public bool GetFarRect(ref Rhino.Geometry.Point3d leftBottom,
                             ref Rhino.Geometry.Point3d rightBottom,
                             ref Rhino.Geometry.Point3d leftTop,
                             ref Rhino.Geometry.Point3d rightTop)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetFarRect(ConstPointer(), ref leftBottom, ref rightBottom, ref leftTop, ref rightTop);
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
      return 1==UnsafeNativeMethods.ON_Viewport_SetScreenPoint(NonConstPointer(), left, right, bottom, top, near, far);
    }

    /// <summary>
    /// See documentation for SetScreenPort
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    /// <param name="top"></param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns></returns>
    public bool GetScreenPort(ref int left, ref int right, ref int bottom, ref int top, ref int near, ref int far)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetScreenPoint(ConstPointer(), ref left, ref right, ref bottom, ref top, ref near, ref far);
    }

    public double ScreenPortAspect
    {
      get
      {
        double dAspect = 0.0;
        if (1 != UnsafeNativeMethods.ON_Viewport_GetScreenPortAspect(ConstPointer(), ref dAspect))
        {
          throw new InvalidOperationException("Cannot get screen port aspect");
        }
        return dAspect;
      }
    }

    /// <summary>
    /// Field of view
    /// </summary>
    /// <param name="halfDiagonalAngle">1/2 of diagonal subtended angle</param>
    /// <param name="halfVerticalAngle">1/2 of vertical subtended angle</param>
    /// <param name="halfHorizontalAngle">1/2 of horizontal subtended angle</param>
    /// <returns></returns>
    public bool GetCameraAngle( ref double halfDiagonalAngle, ref double halfVerticalAngle, ref double halfHorizontalAngle)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_GetCameraAngle2(ConstPointer(), ref halfDiagonalAngle, ref halfVerticalAngle, ref halfHorizontalAngle);
    }

    /// <summary>
    /// Half smallest angle - use GetCameraAngle for more information
    /// </summary>
    public double CameraAngle
    {
      get
      {
        double d = 0.0;
        if (1!=UnsafeNativeMethods.ON_Viewport_GetCameraAngle(ConstPointer(), ref d))
        {
          throw new InvalidOperationException("Cannot get camera angle");
        }
        return d;
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetCameraAngle(NonConstPointer(), value);
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
        if (1 != UnsafeNativeMethods.ON_Viewport_GetCamera35mmLensLength(ConstPointer(), ref d))
        {
          throw new InvalidOperationException("Cannot get camera lens length");
        }
        return d;
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetCamera35mmLensLength(NonConstPointer(), value);
      }
    }

    /// <summary>
    /// Used in GetXform
    /// </summary>
    public enum CoordinateSystems : int
    {
      World  = 0, 
      Camera = 1, 
      Clip   = 2, 
      Screen = 3 
    };

    /// <summary>
    /// Throws if the viewport is invalid
    /// </summary>
    /// <param name="sourceCoordSystem"></param>
    /// <param name="destinationCoordSystem"></param>
    /// <returns>4x4 transformation matrix (acts on the left)</returns>
    public Rhino.Geometry.Transform GetXform(CoordinateSystems sourceCoordSystem, CoordinateSystems destinationCoordSystem)
    {
      Rhino.Geometry.Transform matrix = new Rhino.Geometry.Transform();

      if (1 != UnsafeNativeMethods.ON_Viewport_GetXform(ConstPointer(), (int)sourceCoordSystem, (int)destinationCoordSystem, ref matrix))
      {
        throw new InvalidOperationException("Cannot get viewport xform");
      }
      return matrix;
    }

    /// <summary>
    /// Get the world coordinate line in the view frustum
    /// that projects to a point on the screen.
    /// Throws if the viewport is invalid
    /// </summary>
    /// <param name="screenX">(screenx,screeny) = screen location</param>
    /// <param name="screenY">(screenx,screeny) = screen location</param>
    /// <returns>3d world coordinate line segment starting on the near clipping plane and ending on the far clipping plane</returns>
    Rhino.Geometry.Line GetFrustumLine( double screenX, double screenY)
    {
      Rhino.Geometry.Line line = new Rhino.Geometry.Line();
      
      if (1 != UnsafeNativeMethods.ON_Viewport_GetFrustumLine(ConstPointer(), screenX, screenY, ref line))
      {
        throw new InvalidOperationException("Cannot get frustum line");
      }
      return line;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pointInFrustum">point in viewing frustum.</param>
    /// <returns>number of pixels per world unit at the 3d point</returns>
    double GetWorldToScreenScale( Rhino.Geometry.Point3d pointInFrustum)
    {
      double d = 0.0;
      if (1!=UnsafeNativeMethods.ON_Viewport_GetWorldToScreenScale(ConstPointer(), pointInFrustum, ref d))
      {
        throw new InvalidOperationException("Cannot get world to screen scale");
      }
      return d;
    }

    //TODO bool GetCoordinateSprite(

    /// <summary>
    /// Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.
    /// </summary>
    /// <param name="halfViewAngle"></param>
    /// <param name="bbox"></param>
    /// <returns></returns>
    public bool Extents( double halfViewAngle, Rhino.Geometry.BoundingBox bbox)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_ExtentsBBox(NonConstPointer(), halfViewAngle, bbox.Min, bbox.Max);
    }

    /// <summary>
    /// Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.
    /// </summary>
    /// <param name="halfViewAngle"></param>
    /// <param name="sphere"></param>
    /// <returns></returns>
    public bool Extents( double halfViewAngle, Rhino.Geometry.Sphere sphere)
    {
      return 1==UnsafeNativeMethods.ON_Viewport_ExtentsBBox(NonConstPointer(), halfViewAngle, sphere.Center, sphere.Radius);
    }

    /// <summary>
    ///  View changing from screen input points.  Handy for
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
      return 1==UnsafeNativeMethods.ON_Viewport_ZoomToScreenRect(NonConstPointer(), left, top, right, bottom);
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
      return 1==UnsafeNativeMethods.ON_Viewport_DollyCamera(NonConstPointer(), dollyVector);
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
      if (1!=UnsafeNativeMethods.ON_Viewport_GetDollyCameraVector(ConstPointer(), screenX0, screenY0, screenX1, screenY1, projectionPlaneDistance, ref v))
      {
        throw new InvalidOperationException("Cannot get dolly camera vector");
      }
      return v;
    }

    /// <summary>
    /// Moves frustum's clipping planes
    /// </summary>
    /// <param name="dollyDistance">distance to move in camera direction</param>
    /// <returns></returns>
    public bool DollyFrustum( double dollyDistance )
    {
      return 1==UnsafeNativeMethods.ON_Viewport_DollyFrustum(NonConstPointer(), dollyDistance);
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
        System.Drawing.SizeF size = new System.Drawing.SizeF();
        if (1!=UnsafeNativeMethods.ON_Viewport_GetViewScale(ConstPointer(), (double)size.Width, (double)size.Height))
        {
          throw new InvalidOperationException("Cannot get view scale");
        }
        return size;
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetViewScale(NonConstPointer(), (double)value.Width, (double)value.Height);
      }
    }

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
        return UnsafeNativeMethods.ON_Viewport_ClipModXformIsIdentity(ConstPointer());
      }
    }

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
      Rhino.Geometry.Point3d point = new Rhino.Geometry.Point3d();
      UnsafeNativeMethods.ON_Viewport_FrustumCenterPoint(ConstPointer(), targetDistance, ref point);
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
        UnsafeNativeMethods.ON_Viewport_TargetPoint(ConstPointer(), ref point);
        return point;
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetTargetPoint(NonConstPointer(), value);
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
      return UnsafeNativeMethods.ON_Viewport_TargetDistance(useFrustumCenterFallback);
    }

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

    /// <summary>
    /// Sets the viewport's id to the value used to 
    /// uniquely identify this viewport.
    /// There is no approved way to change the viewport 
    /// id once it is set in order to maintain consistency
    /// across multiple viewports and those routines that 
    /// manage them.
    /// </summary>
    public Guid ViewportId
    {
      get
      {
        return UnsafeNativeMethods.ON_Viewport_GetViewportId(ConstPointer());
      }
      set
      {
        UnsafeNativeMethods.ON_Viewport_SetViewportId(NonConstPointer(), value);
      }
    }

    public static double DefaultNearDistance        { get { return 0.005; } }
    public static double DefaultFarDistance         { get { return 1000.0; } } 
    public static double DefaultMinNearDistance     { get { return 0.0001; } }
    public static double DefaultMinNearOverFar      { get { return 0.0001; } } 

    #region internals
    internal bool InternalValid() { return m_pViewportPointer != IntPtr.Zero; }

    internal IntPtr ConstPointer() { return m_pViewportPointer; }
    internal IntPtr NonConstPointer() { return m_pViewportPointer; }

    #endregion


    #region IDisposable implementation

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    ~Viewport()
    {
      Dispose(false);
    }

    private bool disposed = false;
    private void Dispose(bool isDisposing)
    {
      if (!disposed)
      {
        if (m_bAutoDelete)
        {
          UnsafeNativeMethods.ON_Viewport_Delete(m_pViewportPointer);
          m_pViewportPointer = IntPtr.Zero;
          m_bAutoDelete = false;
        }
      }
      disposed = true;
    }

    #endregion
  }
}

*/

#endif




