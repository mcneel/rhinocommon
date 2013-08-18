using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Security.Permissions;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
  /// <summary>
  /// Represents a viewing frustum.
  /// </summary>
  [Serializable]
  public sealed class ViewportInfo : IDisposable, ISerializable
  {
    readonly object m_parent;
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

    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public ViewportInfo()
    {
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(IntPtr.Zero);
    }

    /// <summary>
    ///  Initializes a new instance by copying values from another instance.
    /// </summary>
    /// <param name="other">The other viewport info.</param>
    public ViewportInfo(ViewportInfo other)
    {
      IntPtr pOther = other.ConstPointer();
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(pOther);
    }

#if RHINO_SDK
    /// <summary>
    /// Copies all of the ViewportInfo data from an existing RhinoViewport.
    /// </summary>
    /// <param name="rhinoViewport">A viewport to copy.</param>
    public ViewportInfo(Rhino.Display.RhinoViewport rhinoViewport)
    {
      IntPtr pRhinoViewport = rhinoViewport.ConstPointer();
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New2(pRhinoViewport);
    }
#endif

    internal ViewportInfo(IntPtr pONViewport)
    {
      m_pViewportPointer = UnsafeNativeMethods.ON_Viewport_New(pONViewport);
    }

    internal ViewportInfo(ViewInfo parent)
    {
      m_parent = parent;
    }

    private ViewportInfo(SerializationInfo info, StreamingContext context)
    {
      m_pViewportPointer = Rhino.Runtime.CommonObject.SerializeReadON_Object(info, context);
     }

    /// <summary>
    /// Populates a System.Runtime.Serialization.SerializationInfo with the data needed to serialize the target object.
    /// </summary>
    /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
    /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      IntPtr pConstThis = ConstPointer();
      Rhino.Runtime.CommonObject.SerializeWriteON_Object(pConstThis, info, context);
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
    
    /// <summary>
    /// Gets a value that indicates whether the camera is valid.
    /// </summary>
    public bool IsValidCamera
    {
      get { return GetBool(idxIsValidCamera); }
    }

    /// <summary>
    /// Gets a value that indicates whether the frustum is valid.
    /// </summary>
    public bool IsValidFrustum
    {
      get { return GetBool(idxIsValidFrustum); }
    }

    /// <summary>
    /// Gets a value that indicates whether this complete object is valid.
    /// </summary>
    public bool IsValid
    {
      get { return GetBool(idxIsValid); }
    }

    /// <summary>
    /// Get or set whether this projection is perspective.
    /// </summary>
    public bool IsPerspectiveProjection
    {
      get { return GetBool(idxIsPerspectiveProjection); }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetProjection(ptr_this, !value);
      }
    }

    /// <summary>
    /// Get or set whether this projection is parallel.
    /// </summary>
    public bool IsParallelProjection
    {
      get { return GetBool(idxIsParallelProjection); }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetProjection(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets a value that indicates whether this projection is a two-point perspective.
    /// </summary>
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
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
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
    /// true if you want the resulting frustum to be symmetric.
    /// </param>
    /// <param name="lensLength">(pass 50.0 when in doubt)
    /// 35 mm lens length to use when changing from parallel
    /// to perspective projections. If the current projection
    /// is perspective or lens_length is &lt;= 0.0,
    /// then this parameter is ignored.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool ChangeToPerspectiveProjection( double targetDistance, bool symmetricFrustum, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToPerspectiveProjection(pThis, targetDistance, symmetricFrustum, lensLength);
    }

    /// <summary>
    /// Changes projections of valid viewports
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
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool ChangeToTwoPointPerspectiveProjection(double targetDistance, Rhino.Geometry.Vector3d up, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToTwoPointPerspectiveProjection(pThis, targetDistance, up, lensLength);
    }

    /// <summary>
    /// Gets the camera location (position) point.
    /// </summary>
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

    /// <summary>
    /// Sets the camera location (position) point.
    /// </summary>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool SetCameraLocation(Rhino.Geometry.Point3d location)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetCameraLocation(pThis, location);
    }

    /// <summary>
    /// Gets the direction that the camera faces.
    /// </summary>
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

    /// <summary>
    /// Sets the direction that the camera faces.
    /// </summary>
    /// <param name="direction">A new direction.</param>
    /// <returns>true if the direction was set; otherwise false.</returns>
    public bool SetCameraDirection(Rhino.Geometry.Vector3d direction)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetCameraDirection(pThis, direction);
    }

    /// <summary>
    /// Gets the camera up vector.
    /// </summary>
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

    /// <summary>
    /// Sets the camera up vector.
    /// </summary>
    /// <param name="up">A new direction.</param>
    /// <returns>true if the direction was set; otherwise false.</returns>
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

    /// <summary>
    /// Gets or sets a value that indicates whether the camera location is unmodifiable.
    /// </summary>
    public bool IsCameraLocationLocked
    {
      get { return GetBool(idxIsCameraLocationLocked); }
      set { SetCameraLock(idxCameraLocationLock, value); }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the direction that the camera faces is unmodifiable.
    /// </summary>
    public bool IsCameraDirectionLocked
    {
      get { return GetBool(idxIsCameraDirectionLocked); }
      set { SetCameraLock(idxCameraDirectionLock, value); }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the camera up vector is unmodifiable.
    /// </summary>
    public bool IsCameraUpLocked
    {
      get { return GetBool(idxIsCameraUpLocked); }
      set { SetCameraLock(idxCameraUpLock, value); }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the camera frustum has a vertical symmetry axis.
    /// </summary>
    public bool IsFrustumLeftRightSymmetric
    {
      get { return GetBool(idxIsFrustumLeftRightSymmetric); }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetIsFrustumSymmetry(pThis, true, value);
      }
    }

    /// <summary>
    /// Gets or sets a value that indicates whether the camera frustum has a horizontal symmetry axis.
    /// </summary>
    public bool IsFrustumTopBottomSymmetric
    {
      get { return GetBool(idxIsFrustumTopBottomSymmetric); }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Viewport_SetIsFrustumSymmetry(pThis, false, value);
      }
    }
    
    /// <summary>
    /// Unlocks the camera vectors and location.
    /// </summary>
    public void UnlockCamera()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Viewport_Unlock(pThis, true);
    }

    /// <summary>
    /// Unlocks frustum horizontal and vertical symmetries.
    /// </summary>
    public void UnlockFrustumSymmetry()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Viewport_Unlock(pThis, false);
    }

    /// <summary>
    /// Gets location and vectors of this camera.
    /// </summary>
    /// <param name="location">An out parameter that will be filled with a point during the call.</param>
    /// <param name="cameraX">An out parameter that will be filled with the X vector during the call.</param>
    /// <param name="cameraY">An out parameter that will be filled with the Y vector during the call.</param>
    /// <param name="cameraZ">An out parameter that will be filled with the Z vector during the call.</param>
    /// <returns>true if current camera orientation is valid; otherwise false.</returns>
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
    /// Gets the unit "to the right" vector.
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
    /// Gets the unit "up" vector.
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
    /// Gets the unit vector in -CameraDirection.
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

    /// <summary>
    /// Sets the view frustum. If FrustumSymmetryIsLocked() is true
    /// and left != -right or bottom != -top, then they will be
    /// adjusted so the resulting frustum is symmetric.
    /// </summary>
    /// <param name="left">A new left value.</param>
    /// <param name="right">A new right value.</param>
    /// <param name="bottom">A new bottom value.</param>
    /// <param name="top">A new top value.</param>
    /// <param name="nearDistance">A new near distance value.</param>
    /// <param name="farDistance">A new far distance value.</param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool SetFrustum(double left, double right, double bottom, double top, double nearDistance, double farDistance)
    {
      return UnsafeNativeMethods.ON_Viewport_SetFrustum(NonConstPointer(), left, right, bottom, top, nearDistance, farDistance);
    }

    /// <summary>
    /// Gets the view frustum.
    /// </summary>
    /// <param name="left">A left value that will be filled during the call.</param>
    /// <param name="right">A right value that will be filled during the call.</param>
    /// <param name="bottom">A bottom value that will be filled during the call.</param>
    /// <param name="top">A top value that will be filled during the call.</param>
    /// <param name="nearDistance">A near distance value that will be filled during the call.</param>
    /// <param name="farDistance">A far distance value that will be filled during the call.</param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool GetFrustum(out double left, out double right, out double bottom, out double top, out double nearDistance, out double farDistance)
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

    /// <summary>
    /// Gets the frustum center point.
    /// </summary>
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

    /// <summary>
    /// Gets the frustum left value. This is -right if the frustum has a vertical symmetry axis.
    /// <para>This number is usually negative.</para>
    /// </summary>
    public double FrustumLeft { get { return GetDouble(idxFrustumLeft); } }

    /// <summary>
    /// Gets the frustum right value. This is -left if the frustum has a vertical symmetry axis.
    /// <para>This number is usually positive.</para>
    /// </summary>
    public double FrustumRight { get { return GetDouble(idxFrustumRight); } }

    /// <summary>
    /// Gets the frustum bottom value. This is -top if the frustum has a horizontal symmetry axis.
    /// <para>This number is usually negative.</para>
    /// </summary>
    public double FrustumBottom { get { return GetDouble(idxFrustumBottom); } }

    /// <summary>
    /// Gets the frustum top value. This is -bottom if the frustum has a horizontal symmetry axis.
    /// <para>This number is usually positive.</para>
    /// </summary>
    public double FrustumTop { get { return GetDouble(idxFrustumTop); } }

    /// <summary>
    /// Gets the frustum near-cutting value.
    /// </summary>
    public double FrustumNear { get { return GetDouble(idxFrustumNear); } }

    /// <summary>
    /// Gets the frustum far-cutting value.
    /// </summary>
    public double FrustumFar { get { return GetDouble(idxFrustumFar); } }

    /// <summary>
    /// Gets the frustum width. This is <see cref="FrustumRight"/> - <see cref="FrustumLeft"/>.
    /// </summary>
    public double FrustumWidth    { get { return FrustumRight - FrustumLeft; } }

    /// <summary>
    /// Gets the frustum height. This is <see cref="FrustumTop"/> - <see cref="FrustumBottom"/>.
    /// </summary>
    public double FrustumHeight   { get { return FrustumTop - FrustumBottom; } }

    /// <summary>
    /// Gets the frustum minimum diameter, or the minimum between <see cref="FrustumWidth"/> and <see cref="FrustumHeight"/>.
    /// </summary>
    public double FrustumMinimumDiameter { get { return GetDouble(idxFrustumMinimumDiameter); } }

    /// <summary>
    /// Gets the frustum maximum diameter, or the maximum between <see cref="FrustumWidth"/> and <see cref="FrustumHeight"/>.
    /// </summary>
    public double FrustumMaximumDiameter { get { return GetDouble(idxFrustumMaximumDiameter); } }

    /// <summary>
    /// Sets the frustum near and far using a bounding box.
    /// </summary>
    /// <param name="boundingBox">A bounding box to use.</param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool SetFrustumNearFar(Rhino.Geometry.BoundingBox boundingBox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarBoundingBox(pThis, boundingBox.Min, boundingBox.Max);
    }

    /// <summary>
    /// Sets the frustum near and far using a center point and radius.
    /// </summary>
    /// <param name="center">A center point.</param>
    /// <param name="radius">A radius value.</param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool SetFrustumNearFar(Rhino.Geometry.Point3d center, double radius)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFarSphere(pThis, center, radius);
    }

    /// <summary>
    /// Sets the frustum near and far distances using two values.
    /// </summary>
    /// <param name="nearDistance">The new near distance.</param>
    /// <param name="farDistance">The new far distance.</param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool SetFrustumNearFar(double nearDistance, double farDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetFrustumNearFar(pThis, nearDistance, farDistance);
    }

    /// <summary>
    /// If needed, adjusts the current frustum so it has the 
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
    /// Returns true if the viewport has now a frustum with the specified symmetries.
    /// </returns>
    public bool ChangeToSymmetricFrustum(bool isLeftRightSymmetric, bool isTopBottomSymmetric, double targetDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ChangeToSymmetricFrustum(pThis, isLeftRightSymmetric, isTopBottomSymmetric, targetDistance);
    }

    /// <summary>
    /// Gets the clipping distance of a point. This function ignores the
    /// current value of the viewport's near and far settings. If
    /// the viewport is a perspective projection, then it intersects
    /// the semi infinite frustum volume with the bounding box and
    /// returns the near and far distances of the intersection.
    /// If the viewport is a parallel projection, it instersects the
    /// infinte view region with the bounding box and returns the
    /// near and far distances of the projection.
    /// </summary>
    /// <param name="point">A point to measure.</param>
    /// <param name="distance">distance of the point (can be &lt; 0)</param>
    /// <returns>true if the bounding box intersects the view frustum and
    /// near_dist/far_dist were set.
    /// false if the bounding box does not intesect the view frustum.</returns>
    public bool GetPointDepth(Rhino.Geometry.Point3d point, out double distance)
    {
      IntPtr pConstThis = ConstPointer();
      double farDistance = 0;
      distance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetPointDepth(pConstThis, point, ref distance, ref farDistance, false);
    }

    /// <summary>
    /// Gets near and far clipping distances of a bounding box.
    /// This function ignores the current value of the viewport's 
    /// near and far settings. If the viewport is a perspective
    /// projection, the it intersects the semi infinite frustum
    /// volume with the bounding box and returns the near and far
    /// distances of the intersection.  If the viewport is a parallel
    /// projection, it instersects the infinte view region with the
    /// bounding box and returns the near and far distances of the
    /// projection.
    /// </summary>
    /// <param name="bbox">The bounding box to sample.</param>
    /// <param name="nearDistance">Near distance of the box. This value can be zero or 
    /// negative when the camera location is inside bbox.</param>
    /// <param name="farDistance">Far distance of the box. This value can be equal to 
    /// near_dist, zero or negative when the camera location is in front of the bounding box.</param>
    /// <returns>true if the bounding box intersects the view frustum and near_dist/far_dist were set. 
    /// false if the bounding box does not intesect the view frustum.</returns>
    public bool GetBoundingBoxDepth(Rhino.Geometry.BoundingBox bbox, out double nearDistance, out double farDistance)
    {
      IntPtr pConstThis = ConstPointer();
      nearDistance = 0;
      farDistance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetBoundingBoxDepth(pConstThis, bbox.Min, bbox.Max, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Gets near and far clipping distances of a bounding sphere.
    /// </summary>
    /// <param name="sphere">The sphere to sample.</param>
    /// <param name="nearDistance">Near distance of the sphere (can be &lt; 0)</param>
    /// <param name="farDistance">Far distance of the sphere (can be equal to near_dist)</param>
    /// <returns>true if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// false if the sphere does not intesect the view frustum.</returns>
    public bool GetSphereDepth(Rhino.Geometry.Sphere sphere, out double nearDistance, out double farDistance)
    {
      IntPtr pConstThis = ConstPointer();
      nearDistance = 0;
      farDistance = 0;
      return UnsafeNativeMethods.ON_Viewport_GetSphereDepth(pConstThis, sphere.Center, sphere.Radius, ref nearDistance, ref farDistance, false);
    }

    /// <summary>
    /// Sets near and far clipping distance subject to constraints.
    /// </summary>
    /// <param name="nearDistance">(>0) desired near clipping distance.</param>
    /// <param name="farDistance">(>near_dist) desired near clipping distance.</param>
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
    /// <returns>true if operation succeeded; otherwise, false.</returns>
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
    /// Gets near clipping plane if camera and frustum
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
    /// Gets far clipping plane if camera and frustum
    /// are valid.  The plane's frame is the same as the camera's
    /// frame.  The origin is located at the intersection of the
    /// camera direction ray and the far clipping plane. The plane's
    /// normal points into the frustum towards the camera location.
    /// </summary>
    public Rhino.Geometry.Plane FrustumFarPlane
    {
      get { return GetPlane(idxFarPlane); }
    }

    /// <summary>
    /// Gets the frustum left plane that separates visibile from off-screen.
    /// </summary>
    public Rhino.Geometry.Plane FrustumLeftPlane
    {
      get { return GetPlane(idxLeftPlane); }
    }

    /// <summary>
    /// Gets the frustum right plane that separates visibile from off-screen.
    /// </summary>
    public Rhino.Geometry.Plane FrustumRightPlane
    {
      get { return GetPlane(idxRightPlane); }
    }

    /// <summary>
    /// Gets the frustum bottom plane that separates visibile from off-screen.
    /// </summary>
    public Rhino.Geometry.Plane FrustumBottomPlane
    {
      get { return GetPlane(idxBottomPlane); }
    }

    /// <summary>
    /// Gets the frustum top plane that separates visibile from off-screen.
    /// </summary>
    public Rhino.Geometry.Plane FrustumTopPlane
    {
      get { return GetPlane(idxTopPlane); }
    }
    
    /// <summary>
    /// Gets the corners of near clipping plane rectangle.
    /// 4 points are returned in the order of bottom left, bottom right,
    /// top left, top right.
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
    /// Gets the corners of far clipping plane rectangle.
    /// 4 points are returned in the order of bottom left, bottom right,
    /// top left, top right.
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
    /// <param name="left">A left value.</param>
    /// <param name="right">A left value. (port_left != port_right)</param>
    /// <param name="bottom">A bottom value.</param>
    /// <param name="top">A top value. (port_top != port_bottom)</param>
    /// <param name="near">A near value.</param>
    /// <param name="far">A far value.</param>
    /// <returns>true if input is valid.</returns>
    public bool SetScreenPort(int left, int right, int bottom, int top, int near, int far)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_SetScreenPort(pThis, left, right, bottom, top, near, far);
    }

    /// <summary>
    /// Gets the location of viewport in pixels.
    /// <para>See value meanings in <see cref="SetScreenPort(int, int, int, int, int, int)">SetScreenPort</see>.</para>
    /// </summary>
    /// <param name="windowRectangle">A new rectangle.</param>
    /// <param name="near">The near value.</param>
    /// <param name="far">The far value.</param>
    /// <returns>true if input is valid.</returns>
    public bool SetScreenPort(System.Drawing.Rectangle windowRectangle, int near, int far)
    {
      return SetScreenPort(windowRectangle.Left, windowRectangle.Right, windowRectangle.Bottom, windowRectangle.Top, near, far);
    }

    /// <summary>
    /// Gets the location of viewport in pixels.
    /// <para>See value meanings in <see cref="SetScreenPort(int, int, int, int, int, int)">SetScreenPort</see>.</para>
    /// </summary>
    /// <param name="windowRectangle">A new rectangle.</param>
    /// <returns>true if input is valid.</returns>
    public bool SetScreenPort(System.Drawing.Rectangle windowRectangle)
    {
      return SetScreenPort(windowRectangle, 0, 0);
    }

    /// <summary>
    /// Gets the location of viewport in pixels.
    /// <para>See value meanings in <see cref="SetScreenPort(int, int, int, int, int, int)">SetScreenPort</see>.</para>
    /// </summary>
    /// <param name="near">The near value. This out parameter is assigned during the call.</param>
    /// <param name="far">The far value. This out parameter is assigned during the call.</param>
    /// <returns>The rectangle, or <see cref="System.Drawing.Rectangle.Empty">Empty</see> rectangle on error.</returns>
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
      // .NET/Windows uses Y down so make sure the top and bottom are passed correctly to Rectangle.FromLTRB()
      // OpenNurbs appears to by Y-Down and the RDK appears to by Y-Up so this check should ensure a Rectangle
      // with a positive height.
      return System.Drawing.Rectangle.FromLTRB(left, top > bottom ? bottom : top, right, top > bottom ? top : bottom);
    }

    /// <summary>
    /// Gets the location of viewport in pixels.
    /// See documentation for <see cref="SetScreenPort(int, int, int, int, int, int)">SetScreenPort</see>.
    /// </summary>
    /// <returns>The rectangle, or <see cref="System.Drawing.Rectangle.Empty">Empty</see> rectangle on error.</returns>
    public System.Drawing.Rectangle GetScreenPort()
    {
      int near;
      int far;
      return GetScreenPort(out near, out far);
    }

    /// <summary>
    /// Gets the sceen aspect ratio.
    /// <para>This is width / height.</para>
    /// </summary>
    public double ScreenPortAspect
    {
      get
      {
        double dAspect = 0.0;
        IntPtr pConstThis = ConstPointer();
        if (!UnsafeNativeMethods.ON_Viewport_GetScreenPortAspect(pConstThis, ref dAspect))
          dAspect = 0;
        return dAspect;
      }
    }

    /// <summary>
    /// Gets the field of view angles.
    /// </summary>
    /// <param name="halfDiagonalAngleRadians">1/2 of diagonal subtended angle. This out parameter is assigned during this call.</param>
    /// <param name="halfVerticalAngleRadians">1/2 of vertical subtended angle. This out parameter is assigned during this call.</param>
    /// <param name="halfHorizontalAngleRadians">1/2 of horizontal subtended angle. This out parameter is assigned during this call.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool GetCameraAngles(out double halfDiagonalAngleRadians, out double halfVerticalAngleRadians, out double halfHorizontalAngleRadians)
    {
      IntPtr pConstThis = ConstPointer();
      halfDiagonalAngleRadians = 0;
      halfHorizontalAngleRadians = 0;
      halfVerticalAngleRadians = 0;
      return UnsafeNativeMethods.ON_Viewport_GetCameraAngle2(pConstThis, ref halfDiagonalAngleRadians, ref halfVerticalAngleRadians, ref halfHorizontalAngleRadians);
    }

    /// <summary>
    /// Gets or sets the 1/2 smallest angle. See <see cref="GetCameraAngles"/> for more information.
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
    /// Computes a transform from a coordinate system to another.
    /// </summary>
    /// <param name="sourceSystem">The coordinate system to map from.</param>
    /// <param name="destinationSystem">The coordinate system to map into.</param>
    /// <returns>The 4x4 transformation matrix (acts on the left).</returns>
    public Rhino.Geometry.Transform GetXform(Rhino.DocObjects.CoordinateSystem sourceSystem, Rhino.DocObjects.CoordinateSystem destinationSystem)
    {
      Rhino.Geometry.Transform matrix = new Rhino.Geometry.Transform();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetXform(pConstThis, (int)sourceSystem, (int)destinationSystem, ref matrix))
        matrix = Rhino.Geometry.Transform.Unset;
      return matrix;
    }

    /// <summary>
    /// Gets the world coordinate line in the view frustum
    /// that projects to a point on the screen.
    /// </summary>
    /// <param name="screenX">(screenx,screeny) = screen location.</param>
    /// <param name="screenY">(screenx,screeny) = screen location.</param>
    /// <returns>3d world coordinate line segment starting on the near clipping plane and ending on the far clipping plane.</returns>
    public Rhino.Geometry.Line GetFrustumLine( double screenX, double screenY)
    {
      Rhino.Geometry.Line line = new Rhino.Geometry.Line();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetFrustumLine(pConstThis, screenX, screenY, ref line))
        line = Rhino.Geometry.Line.Unset;
      return line;
    }

    /// <summary>
    /// Gets the world coordinate line in the view frustum
    /// that projects to a point on the screen.
    /// </summary>
    /// <param name="screenPoint">screen location</param>
    /// <returns>3d world coordinate line segment starting on the near clipping plane and ending on the far clipping plane.</returns>
    public Rhino.Geometry.Line GetFrustumLine(System.Drawing.Point screenPoint)
    {
      return GetFrustumLine(screenPoint.X, screenPoint.Y);
    }

    /// <summary>
    /// Gets the world coordinate line in the view frustum
    /// that projects to a point on the screen.
    /// </summary>
    /// <param name="screenPoint">screen location</param>
    /// <returns>3d world coordinate line segment starting on the near clipping plane and ending on the far clipping plane.</returns>
    public Rhino.Geometry.Line GetFrustumLine(System.Drawing.PointF screenPoint)
    {
      return GetFrustumLine(screenPoint.X, screenPoint.Y);
    }

    /// <summary>
    /// Gets the scale factor from point in frustum to screen scale.
    /// </summary>
    /// <param name="pointInFrustum">point in viewing frustum.</param>
    /// <returns>number of pixels per world unit at the 3d point.</returns>
    public double GetWorldToScreenScale(Rhino.Geometry.Point3d pointInFrustum)
    {
      double d = 0.0;
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Viewport_GetWorldToScreenScale(pConstThis, pointInFrustum, ref d))
        d = 0;
      return d;
    }

    /// <summary>
    /// Extends this viewport view to include a bounding box.
    /// <para>Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.</para>
    /// </summary>
    /// <param name="halfViewAngleRadians">1/2 smallest subtended view angle in radians.</param>
    /// <param name="bbox">A bounding box in 3d world coordinates.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool Extents(double halfViewAngleRadians, Rhino.Geometry.BoundingBox bbox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ExtentsBBox(pThis, halfViewAngleRadians, bbox.Min, bbox.Max);
    }

    /// <summary>
    /// Extends this viewport view to include a sphere.
    /// <para>Use Extents() as a quick way to set a viewport to so that bounding
    /// volume is inside of a viewports frustrum.
    /// The view angle is used to determine the position of the camera.</para>
    /// </summary>
    /// <param name="halfViewAngleRadians">1/2 smallest subtended view angle in radians.</param>
    /// <param name="sphere">A sphere in 3d world coordinates.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool Extents(double halfViewAngleRadians, Rhino.Geometry.Sphere sphere)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ExtentsSphere(pThis, halfViewAngleRadians, sphere.Center, sphere.Radius);
    }

    /// <summary>
    /// Zooms to a screen zone.
    /// <para>View changing from screen input points. Handy for
    /// using a mouse to manipulate a view.
    /// ZoomToScreenRect() may change camera and frustum settings.</para>
    /// </summary>
    /// <param name="left">Screen coord.</param>
    /// <param name="top">Screen coord.</param>
    /// <param name="right">Screen coord.</param>
    /// <param name="bottom">Screen coord.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool ZoomToScreenRect(int left, int top, int right, int bottom)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_ZoomToScreenRect(pThis, left, top, right, bottom);
    }

    /// <summary>
    /// Zooms to a screen zone.
    /// <para>View changing from screen input points. Handy for
    /// using a mouse to manipulate a view.
    /// ZoomToScreenRect() may change camera and frustum settings.</para>
    /// </summary>
    /// <param name="windowRectangle">The new window rectangle in screen space.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool ZoomToScreenRect(System.Drawing.Rectangle windowRectangle)
    {
      return ZoomToScreenRect(windowRectangle.Left, windowRectangle.Top, windowRectangle.Right, windowRectangle.Bottom);
    }

    /// <summary>
    /// DollyCamera() does not update the frustum's clipping planes.
    /// To update the frustum's clipping planes call DollyFrustum(d)
    /// with d = dollyVector o cameraFrameZ.  To convert screen locations
    /// into a dolly vector, use GetDollyCameraVector().
    /// Does not update frustum.  To update frustum use DollyFrustum(d) with d = dollyVector o cameraFrameZ.
    /// </summary>
    /// <param name="dollyVector">dolly vector in world coordinates.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool DollyCamera(Rhino.Geometry.Vector3d dollyVector)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_DollyCamera(pThis, dollyVector);
    }

    /// <summary>
    /// Gets a world coordinate dolly vector that can be passed to DollyCamera().
    /// </summary>
    /// <param name="screenX0">Screen coords of start point.</param>
    /// <param name="screenY0">Screen coords of start point.</param>
    /// <param name="screenX1">Screen coords of end point.</param>
    /// <param name="screenY1">Screen coords of end point.</param>
    /// <param name="projectionPlaneDistance">Distance of projection plane from camera. When in doubt, use 0.5*(frus_near+frus_far).</param>
    /// <returns>The world coordinate dolly vector.</returns>
    public Rhino.Geometry.Vector3d GetDollyCameraVector(int screenX0, int screenY0, int screenX1, int screenY1, double projectionPlaneDistance)
    {
      Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
      IntPtr pConstThis = ConstPointer();
      if (UnsafeNativeMethods.ON_Viewport_GetDollyCameraVector(pConstThis, screenX0, screenY0, screenX1, screenY1, projectionPlaneDistance, ref v))
        v = Rhino.Geometry.Vector3d.Unset;
      return v;
    }

    /// <summary>
    /// Gets a world coordinate dolly vector that can be passed to DollyCamera().
    /// </summary>
    /// <param name="screen0">Start point.</param>
    /// <param name="screen1">End point.</param>
    /// <param name="projectionPlaneDistance">Distance of projection plane from camera. When in doubt, use 0.5*(frus_near+frus_far).</param>
    /// <returns>The world coordinate dolly vector.</returns>
    public Rhino.Geometry.Vector3d GetDollyCameraVector(System.Drawing.Point screen0, System.Drawing.Point screen1, double projectionPlaneDistance)
    {
      return GetDollyCameraVector(screen0.X, screen0.Y, screen1.X, screen1.Y, projectionPlaneDistance);
    }

    /// <summary>
    /// Moves the frustum clipping planes.
    /// </summary>
    /// <param name="dollyDistance">Distance to move in camera direction.</param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool DollyFrustum(double dollyDistance)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Viewport_DollyFrustum(pThis, dollyDistance);
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains
    /// all of the document objects that can be seen in view.
    /// If the projection is perspective, the camera angle is not changed.
    /// </summary>
    /// <param name="geometry"></param>
    /// <param name="border">
    /// If border > 1.0, then the fustum in enlarged by this factor
    /// to provide a border around the view.  1.1 works well for
    /// parallel projections; 0.0 is suggested for perspective projections.
    /// </param>
    /// <returns>True if successful.</returns>
    public bool DollyExtents(IEnumerable<GeometryBase> geometry, double border)
    {
      var world_2_camera = GetXform(CoordinateSystem.World, CoordinateSystem.Camera);
      BoundingBox cam_bbox = BoundingBox.Unset;
      foreach( var g in geometry )
      {
        var bbox = g.GetBoundingBox(world_2_camera);
        cam_bbox.Union(bbox);
      }
      return DollyExtents(cam_bbox, border);
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains
    /// all of the document objects that can be seen in view.
    /// If the projection is perspective, the camera angle is not changed.
    /// </summary>
    /// <param name="cameraCoordinateBoundingBox"></param>
    /// <param name="border">
    /// If border > 1.0, then the fustum in enlarged by this factor
    /// to provide a border around the view.  1.1 works well for
    /// parallel projections; 0.0 is suggested for perspective projections.
    /// </param>
    /// <returns>True if successful.</returns>
    public bool DollyExtents(BoundingBox cameraCoordinateBoundingBox, double border)
    {
      bool rc = false;
      if (cameraCoordinateBoundingBox.IsValid)
      {
        if (border > 1.0 && RhinoMath.IsValidDouble(border))
        {
          double dx = cameraCoordinateBoundingBox.Max.X - cameraCoordinateBoundingBox.Min.X;
          dx *= 0.5 * (border - 1.0);
          double dy = cameraCoordinateBoundingBox.Max.Y - cameraCoordinateBoundingBox.Min.Y;
          dy *= 0.5 * (border - 1.0);
          var pt = cameraCoordinateBoundingBox.Max;
          cameraCoordinateBoundingBox.Max = new Point3d(pt.X + dx, pt.Y + dy, pt.Z);
          pt = cameraCoordinateBoundingBox.Min;
          cameraCoordinateBoundingBox.Min = new Point3d(pt.X - dx, pt.Y - dy, pt.Z);
        }
        IntPtr ptr_this = NonConstPointer();
        rc = UnsafeNativeMethods.ON_Viewport_DollyExtents(ptr_this, cameraCoordinateBoundingBox.Min, cameraCoordinateBoundingBox.Max);
      }
      return rc;
    }

    /// <summary>
    /// Applies scaling factors to parallel projection clipping coordinates
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
    /// Gets the m_clip_mod transformation.
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
    /// Gets the m_clip_mod inverse transformation.
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
    /// is not valid, then ON_3dPoint::UnsetPoint is returned.</returns>
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
    /// Gets the distance from the target point to the camera plane.
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
    /// Gets suggested values for setting the perspective minimum
    /// near distance and minimum near/far ratio.
    /// </summary>
    /// <param name="cameraLocation">-</param>
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
    /// Sets suggested the perspective minimum near distance and
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
    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
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



