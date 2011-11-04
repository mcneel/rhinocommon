#pragma warning disable 1591
using System;
using System.Runtime.Serialization;

#if USING_V5_SDK
namespace Rhino.Geometry
{
  [Serializable]
  public class Extrusion : Surface, ISerializable
  {
    #region internals
    internal Extrusion(IntPtr native_ptr, object parent)
      : base(native_ptr, parent)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Extrusion(IntPtr.Zero, null);
    }
    #endregion

    // serialization constructor
    protected Extrusion(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
    #region statics
    /// <summary>
    /// Get an extrusion form of a cylinder
    /// </summary>
    /// <param name="cylinder">IsFinite must be true</param>
    /// <param name="capBottom">If true, the end at cylinder.Height1 will be capped</param>
    /// <param name="capTop">If true, the end at cylinder.Height2 will be capped</param>
    /// <returns>Extrusion on success. null on failure</returns>
    public static Extrusion CreateCylinderExtrusion(Cylinder cylinder, bool capBottom, bool capTop)
    {
      IntPtr pExtrusion = UnsafeNativeMethods.ON_Extrusion_CreateCylinder(ref cylinder, capBottom, capTop);
      return IntPtr.Zero == pExtrusion ? null : new Extrusion(pExtrusion, null);
    }

    /// <summary>
    /// Get an extrusion form of a pipe
    /// </summary>
    /// <param name="cylinder">IsFinite must be true.</param>
    /// <param name="otherRadius">
    /// If cylinder.Radius is less than other radius, then the cylinder will be the inside
    /// of the pipe.
    /// </param>
    /// <param name="capBottom">If true, the end at cylinder.Height1 will be capped</param>
    /// <param name="capTop">If true, the end at cylinder.Height2 will be capped</param>
    /// <returns>Extrusion on success. null on failure</returns>
    public static Extrusion CreatePipeExtrusion(Cylinder cylinder, double otherRadius, bool capTop, bool capBottom)
    {
      IntPtr pExtrusion = UnsafeNativeMethods.ON_Extrusion_CreatePipe(ref cylinder, otherRadius, capBottom, capTop);
      return IntPtr.Zero == pExtrusion ? null : new Extrusion(pExtrusion, null);
    }
    #endregion

    public Extrusion()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Extrusion_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Build a Brep form of the extrusion. The outer profile is always the first face of the Brep.
    /// If there are inner profiles, additional Brep faces are created for each profile. If the
    /// outer profile is closed, then end caps are added as the last two faces of the Brep.
    /// </summary>
    /// <param name="splitKinkyFaces">
    /// If true and the profiles have kinks, then the faces corresponding to those profiles are split
    /// so they will be G1.
    /// </param>
    /// <returns>A Brep with a similar shape like this extrustion or null</returns>
    public Brep ToBrep(bool splitKinkyFaces)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pBrep = UnsafeNativeMethods.ON_Extrusion_BrepForm(pConstThis, splitKinkyFaces);
      return GeometryBase.CreateGeometryHelper(pBrep, null) as Brep;
    }
    
    public bool SetPathAndUp(Point3d a, Point3d b, Vector3d up)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Extrusion_SetPathAndUp(pThis, a, b, up);
    }

    public Point3d PathStart
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.ON_Extrusion_GetPoint(pConstThis, true, ref rc);
        return rc;
      }
    }
    public Point3d PathEnd
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.ON_Extrusion_GetPoint(pConstThis, false, ref rc);
        return rc;
      }
    }
    public Vector3d PathTangent
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Vector3d rc = new Vector3d();
        UnsafeNativeMethods.ON_Extrusion_GetPathTangent(pConstThis, ref rc);
        return rc;
      }
    }

    public Vector3d MiterPlaneNormalAtStart
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Vector3d rc = new Vector3d();
        UnsafeNativeMethods.ON_Extrusion_GetMiterPlaneNormal(pConstThis, 0, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Extrusion_SetMiterPlaneNormal(pThis, 0, value);
      }
    }
    public Vector3d MiterPlaneNormalAtEnd
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Vector3d rc = new Vector3d();
        UnsafeNativeMethods.ON_Extrusion_GetMiterPlaneNormal(pConstThis, 1, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Extrusion_SetMiterPlaneNormal(pThis, 1, value);
      }
    }
    public bool IsMiteredAtStart
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Extrusion_IsMitered(pConstThis);
        return (1 == rc || 3 == rc);
      }
    }
    public bool IsMiteredAtEnd
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Extrusion_IsMitered(pConstThis);
        return (2 == rc || 3 == rc);
      }
    }

    public override bool IsSolid
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Extrusion_IsSolid(pConstThis);
      }
    }

    public bool IsCappedAtBottom
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Extrusion_IsCapped(pConstThis);
        return (1 == rc || 3 == rc);
      }
    }
    public bool IsCappedAtTop
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Extrusion_IsCapped(pConstThis);
        return (2 == rc || 3 == rc);
      }
    }

    public int CapCount
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Extrusion_CapCount(pConstThis);
      }
    }

    /// <summary>
    /// Get the transformation that maps the xy profile curve to its 3d location.
    /// </summary>
    /// <param name="s">
    /// 0.0 = starting profile
    /// 1.0 = ending profile
    /// </param>
    /// <returns>Transformation on success. Invalid transform on failure</returns>
    public Transform GetProfileTransformation(double s)
    {
      Transform xform = new Transform();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Extrusion_GetProfileTransformation(pConstThis, s, ref xform))
        xform = Rhino.Geometry.Transform.Unset;

      return xform;
    }

    /// <summary>
    /// Get the the 3d plane containing the profile curve at a normalized path parameter.
    /// </summary>
    /// <param name="s">
    /// 0.0 = starting profile
    /// 1.0 = ending profile
    /// </param>
    /// <returns>Plane on success. Invalid plane on failure</returns>
    /// <remarks>
    ///  When no mitering is happening, GetPathPlane() and GetProfilePlane() return the same plane.
    /// </remarks>
    public Plane GetProfilePlane(double s)
    {
      Plane plane = new Plane();
      IntPtr pConstThis = ConstPointer();
      if( !UnsafeNativeMethods.ON_Extrusion_GetPlane(pConstThis, true, s, ref plane) )
        plane = Plane.Unset;
      return plane;
    }

    /// <summary>
    /// Get the the 3d plane perpindicular to the path at a normalized path parameter.
    /// </summary>
    /// <param name="s">
    /// 0.0 = starting profile
    /// 1.0 = ending profile
    /// </param>
    /// <returns>Plane on success. Invalid plane on failure</returns>
    /// <remarks>
    ///  When no mitering is happening, GetPathPlane() and GetProfilePlane() return the same plane.
    /// </remarks>
    public Plane GetPathPlane(double s)
    {
      Plane plane = new Plane();
      IntPtr pConstThis = ConstPointer();
      if( !UnsafeNativeMethods.ON_Extrusion_GetPlane(pConstThis, false, s, ref plane) )
        plane = Plane.Unset;
      return plane;
    }

    /// <summary>
    /// Set the outer profile of the extrusion
    /// </summary>
    /// <param name="outerProfile">curve in the XY plane or a 2D curve</param>
    /// <param name="cap">
    /// If outerProfile is a closed curve, then cap determines if the extrusion
    /// has end caps. If outerProfile is an open curve, cap is ignored.
    /// </param>
    /// <returns>
    /// True if the profile was set. If the outer profile is closed, then the
    /// extrusion may also have inner profiles. If the outer profile is open,
    /// the extrusion may not have inner profiles. If the extrusion already
    /// has a profile, the set will fail.
    /// </returns>
    public bool SetOuterProfile(Curve outerProfile, bool cap)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstCurve = outerProfile.ConstPointer();
      return UnsafeNativeMethods.ON_Extrusion_SetOuterProfile(pThis, pConstCurve, cap);
    }

    /// <summary>
    /// Add an inner profile
    /// </summary>
    /// <param name="innerProfile">Closed curve in the XY plane or a 2d curve</param>
    /// <returns>True if the profile was set</returns>
    public bool AddInnerProfile(Curve innerProfile)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstCurve = innerProfile.ConstPointer();
      return UnsafeNativeMethods.ON_Extrusion_AddInnerProfile(pThis, pConstCurve);
    }

    /// <summary>
    /// Number of profile curves
    /// </summary>
    public int ProfileCount
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Extrusion_ProfileCount(pConstThis);
      }
    }

    //[skipping]
    //ProfileParamter
    //Profile

    /// <summary>
    /// 
    /// </summary>
    /// <param name="profileIndex">
    /// 0 &lt;= profileIndex &lt; ProfileCount
    /// The outer profile has index 0
    /// </param>
    /// <param name="s">
    /// 0.0 &lt;= s &lt;= 1.0
    /// A relative parameter controling which profile is returned.
    /// 0 = bottom profile and 1 = top profile
    /// </param>
    /// <returns></returns>
    public Curve Profile3d(int profileIndex, double s)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pCurve = UnsafeNativeMethods.ON_Extrusion_Profile3d(pConstThis, profileIndex, s);
      return GeometryBase.CreateGeometryHelper(pCurve, null) as Curve;
    }

    public Curve Profile3d(ComponentIndex ci)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pCurve = UnsafeNativeMethods.ON_Extrusion_Profile3d2(pConstThis, ci);
      return GeometryBase.CreateGeometryHelper(pCurve, null) as Curve;
    }

    public Curve WallEdge(ComponentIndex ci)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pCurve = UnsafeNativeMethods.ON_Extrusion_WallEdge(pConstThis, ci);
      return GeometryBase.CreateGeometryHelper(pCurve, null) as Curve;
    }

    public Surface WallSurface(ComponentIndex ci)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pSurface = UnsafeNativeMethods.ON_Extrusion_WallSurface(pConstThis, ci);
      return GeometryBase.CreateGeometryHelper(pSurface, null) as Surface;
    }

    public LineCurve PathLineCurve()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pLineCurve = UnsafeNativeMethods.ON_Extrusion_PathLineCurve(pConstThis);
      return GeometryBase.CreateGeometryHelper(pLineCurve, null) as LineCurve;
    }

    /// <summary>
    /// Get index of the profile curve with domain containing this paramter. 
    /// </summary>
    /// <param name="profileParameter">Parameter on profile curve</param>
    /// <returns>
    /// -1 if profileParameter does not correspond to a point on the profile curve.
    /// When the profileParameter corresponds to the end of one profile and the
    /// beginning of the next profile, the index of the next profile is returned.
    /// </returns>
    public int ProfileIndex(double profileParameter)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Extrusion_ProfileIndex(pConstThis, profileParameter);
    }

    //skipping
    //  const ON_PolyCurve* PolyProfile() const;
    //  int GetProfileCurves( ON_SimpleArray<const ON_Curve*>& profile_curves ) const;
  }
}
#endif