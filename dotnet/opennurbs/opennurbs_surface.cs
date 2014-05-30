using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Defines enumerated values for isoparametric curve direction on a surface, such as X or Y,
  /// and curve sides, such as North or West boundary.
  /// <para>Note: odd values are all x-constant; even values > 0 are all y-constant.</para>
  /// </summary>
  public enum IsoStatus : int
  {
    /// <summary>
    /// curve is not an isoparameteric curve.
    /// </summary>
    None = 0,
    /// <summary>
    /// curve is a "x" = constant (vertical) isoparametric curve in the interior of the surface's domain.
    /// </summary>
    X = 1,
    /// <summary>
    /// curve is a "y" = constant (horizontal) isoparametric curve in the interior of the surface's domain.
    /// </summary>
    Y = 2,
    /// <summary>
    /// curve is a "x" = constant isoparametric curve along the west side of the surface's domain.
    /// </summary>
    West = 3,
    /// <summary>
    /// curve is a "y" = constant isoparametric curve along the south side of the surface's domain.
    /// </summary>
    South = 4,
    /// <summary>
    /// curve is a "x" = constant isoparametric curve along the east side of the surface's domain.
    /// </summary>
    East = 5,
    /// <summary>
    /// curve is a "y" = constant isoparametric curve along the north side of the surface's domain.
    /// </summary>
    North = 6
  }

  /// <summary>
  /// Maintains computed information for surface curvature evaluation.
  /// </summary>
  public class SurfaceCurvature
  {
    #region members
    private readonly Point2d m_uv;
    private Point3d m_point;
    private Vector3d m_normal;
    private Vector3d m_dir1;
    private Vector3d m_dir2;

    private double m_gauss;
    private double m_mean;
    private double m_kappa1;
    private double m_kappa2;
    #endregion

    #region constructors
    private SurfaceCurvature(double u, double v)
    {
      m_uv = new Point2d(u, v);
    }
    internal static SurfaceCurvature _FromSurfacePointer(IntPtr pConstSurface, double u, double v)
    {
      if (IntPtr.Zero == pConstSurface)
        return null;

      SurfaceCurvature rc = new SurfaceCurvature(u, v);

      if (!UnsafeNativeMethods.ON_Surface_EvCurvature(pConstSurface, u, v,
                                                     ref rc.m_point, ref rc.m_normal,
                                                     ref rc.m_dir1, ref rc.m_dir2,
                                                     ref rc.m_gauss, ref rc.m_mean,
                                                     ref rc.m_kappa1, ref rc.m_kappa2))
      {
        rc = null;
      }

      return rc;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the UV location where the curvature was computed.
    /// </summary>
    public Point2d UVPoint
    {
      get { return m_uv; }
    }
    /// <summary>
    /// Gets the surface point at UV.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public Point3d Point
    {
      get { return m_point; }
    }
    /// <summary>
    /// Gets the surface normal at UV.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public Vector3d Normal
    {
      get { return m_normal; }
    }

    /// <summary>
    /// Gets the principal curvature direction vector.
    /// </summary>
    /// <param name="direction">Direction index, valid values are 0 and 1.</param>
    /// <returns>The specified direction vector.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public Vector3d Direction(int direction)
    {
      return direction == 0 ? m_dir1 : m_dir2;
    }

    /// <summary>
    /// Gets the Kappa curvature value.
    /// </summary>
    /// <param name="direction">Kappa index, valid values are 0 and 1.</param>
    /// <returns>The specified kappa value.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public double Kappa(int direction)
    {
      return direction == 0 ? m_kappa1 : m_kappa2;
    }

    /// <summary>
    /// Gets the Gaussian curvature value at UV.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public double Gaussian
    {
      get { return m_gauss; }
    }
    /// <summary>
    /// Gets the Mean curvature value at UV.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public double Mean
    {
      get { return m_mean; }
    }
    #endregion

    #region methods
    /// <summary>
    /// Computes the osculating circle along the given direction.
    /// </summary>
    /// <param name="direction">Direction index, valid values are 0 and 1.</param>
    /// <returns>The osculating circle in the given direction or Circle.Unset on failure.</returns>
    public Circle OsculatingCircle(int direction)
    {
      if (Math.Abs(Kappa(direction)) < 1e-16 || Math.Abs(Kappa(direction)) > 1e16)
      {
        return Circle.Unset;
      }
      double r = 1.0 / Kappa(direction);
      Point3d pc = m_point + m_normal * r;
      Point3d p0 = pc - Direction(direction) * r;
      Point3d p1 = pc + Direction(direction) * r;
      return new Circle(p0, m_point, p1);
    }
    #endregion
  }

  /// <summary>
  /// Represents a base class that is common to most RhinoCommon surface types.
  /// <para>A surface represents an entity that can be all visited by providing
  /// two independent parameters, usually called (u, v), or sometimes (s, t).</para>
  /// </summary>
  //[Serializable]
  public class Surface : GeometryBase
  {
    #region statics
#if RHINO_SDK
    /// <summary>
    /// Constructs a rolling ball fillet between two surfaces.
    /// </summary>
    /// <param name="surfaceA">A first surface.</param>
    /// <param name="surfaceB">A second surface.</param>
    /// <param name="radius">A radius value.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
    /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
    public static Surface[] CreateRollingBallFillet(Surface surfaceA, Surface surfaceB, double radius, double tolerance)
    {
      return CreateRollingBallFillet(surfaceA, false, surfaceB, false, radius, tolerance);
    }

    /// <summary>
    /// Constructs a rolling ball fillet between two surfaces.
    /// </summary>
    /// <param name="surfaceA">A first surface.</param>
    /// <param name="flipA">A value that indicates whether A should be used in flipped mode.</param>
    /// <param name="surfaceB">A second surface.</param>
    /// <param name="flipB">A value that indicates whether B should be used in flipped mode.</param>
    /// <param name="radius">A radius value.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
    /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
    public static Surface[] CreateRollingBallFillet(Surface surfaceA, bool flipA, Surface surfaceB, bool flipB, double radius, double tolerance)
    {
      if (surfaceA == null) throw new ArgumentNullException("surfaceA");
      if (surfaceB == null) throw new ArgumentNullException("surfaceB");

      IntPtr pConstSurfaceA = surfaceA.ConstPointer();
      IntPtr pConstSurfaceB = surfaceB.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArraySurfacePointer srfs = new Rhino.Runtime.InteropWrappers.SimpleArraySurfacePointer())
      {
        IntPtr pSurfaces = srfs.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoSimpleRollingBallFillet(pConstSurfaceA, flipA,
          pConstSurfaceB, flipB, radius, tolerance, pSurfaces);
        return srfs.ToNonConstArray();
      }
    }

    /// <summary>
    /// Constructs a rolling ball fillet between two surfaces.
    /// </summary>
    /// <param name="surfaceA">A first surface.</param>
    /// <param name="uvA">A point in the parameter space of FaceA near where the fillet is expected to hit the surface.</param>
    /// <param name="surfaceB">A second surface.</param>
    /// <param name="uvB">A point in the parameter space of FaceB near where the fillet is expected to hit the surface.</param>
    /// <param name="radius">A radius value.</param>
    /// <param name="tolerance">A tolerance value used for approximating and intersecting offset surfaces.</param>
    /// <returns>A new array of rolling ball fillet surfaces; this array can be empty on failure.</returns>
    /// <exception cref="ArgumentNullException">If surfaceA or surfaceB are null.</exception>
    public static Surface[] CreateRollingBallFillet(Surface surfaceA, Point2d uvA, Surface surfaceB, Point2d uvB, double radius, double tolerance)
    {
      if (surfaceA == null) throw new ArgumentNullException("surfaceA");
      if (surfaceB == null) throw new ArgumentNullException("surfaceB");

      IntPtr pConstSurfaceA = surfaceA.ConstPointer();
      IntPtr pConstSurfaceB = surfaceB.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArraySurfacePointer srfs = new Rhino.Runtime.InteropWrappers.SimpleArraySurfacePointer())
      {
        IntPtr pSurfaces = srfs.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoSimpleRollingBallFillet2(pConstSurfaceA, uvA,
          pConstSurfaceB, uvB, radius, tolerance, pSurfaces);
        return srfs.ToNonConstArray();
      }
    }

    /// <summary>
    /// Constructs a surface by extruding a curve along a vector.
    /// </summary>
    /// <param name="profile">Profile curve to extrude.</param>
    /// <param name="direction">Direction and length of extrusion.</param>
    /// <returns>A surface on success or null on failure.</returns>
    public static Surface CreateExtrusion(Curve profile, Vector3d direction)
    {
      IntPtr pConstCurve = profile.ConstPointer();
      IntPtr pSurface = UnsafeNativeMethods.RHC_RhinoExtrudeCurveStraight(pConstCurve, direction);
      if (IntPtr.Zero == pSurface)
        return null;
      // CreateGeometryHelper will create the "actual" surface type (Nurbs, Sum, Rev,...)
      GeometryBase g = GeometryBase.CreateGeometryHelper(pSurface, null);
      Surface rc = g as Surface;
      return rc;
    }

    /// <summary>
    /// Constructs a surface by extruding a curve to a point.
    /// </summary>
    /// <param name="profile">Profile curve to extrude.</param>
    /// <param name="apexPoint">Apex point of extrusion.</param>
    /// <returns>A Surface on success or null on failure.</returns>
    public static Surface CreateExtrusionToPoint(Curve profile, Point3d apexPoint)
    {
      IntPtr pConstCurve = profile.ConstPointer();
      IntPtr pSurface = UnsafeNativeMethods.RHC_RhinoExtrudeCurveToPoint(pConstCurve, apexPoint);
      if (IntPtr.Zero == pSurface)
        return null;
      // CreateGeometryHelper will create the "actual" surface type (Nurbs, Sum, Rev,...)
      GeometryBase g = GeometryBase.CreateGeometryHelper(pSurface, null);
      Surface rc = g as Surface;
      return rc;
    }

    /// <summary>
    /// Constructs a periodic surface from a base surface and a direction.
    /// </summary>
    /// <param name="baseSurface">A base surface.</param>
    /// <param name="direction">0 is first parameter, 1 is second parameter.</param>
    /// <returns>A new surface; or null on error.</returns>
    public static Surface CreatePeriodicSurface(Surface baseSurface, int direction)
    {
      IntPtr pConstSurface = baseSurface.ConstPointer();
      IntPtr pNewSurf = UnsafeNativeMethods.TL_Surface_MakePeriodic(pConstSurface, direction);
      return GeometryBase.CreateGeometryHelper(pNewSurf, null) as Surface;
    }
#endif

    #endregion

    #region constructors

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    protected Surface()
    {
      // the base class always handles set up of pointers
    }

    internal Surface(IntPtr nativePointer, object parent)
      : base(nativePointer, parent, -1)
    {
      if (parent == null)
        ApplyMemoryPressure();
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = true;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_DuplicateSurface(pConstPointer);
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      Rhino.Geometry.SurfaceOfHolder holder = m__parent as Rhino.Geometry.SurfaceOfHolder;
      if (null != holder)
      {
        return holder.SurfacePointer();
      }
      Rhino.Geometry.SurfaceHolder holder2 = m__parent as Rhino.Geometry.SurfaceHolder;
      if (null != holder2)
      {
        return holder2.ConstSurfacePointer();
      }
      return base._InternalGetConstPointer();
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Surface(IntPtr.Zero, null);
    }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected Surface(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}
    #endregion

    #region methods
    //[skipping]
    //virtual ON_Mesh* CreateMesh( const ON_MeshParameters& mp, ON_Mesh* mesh = NULL ) const;

    /// <summary>Gets the domain in a direction.</summary>
    /// <param name="direction">0 gets first parameter, 1 gets second parameter.</param>
    /// <returns>An interval value.</returns>
    public Interval Domain(int direction)
    {
      if (direction != 0)
        direction = 1;
      Interval domain = new Interval();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Surface_Domain(ptr, direction, ref domain);
      return domain;
    }

    /// <summary>
    /// Sets the domain in a direction.
    /// </summary>
    /// <param name="direction">
    /// 0 sets first parameter's domain, 1 sets second parameter's domain.
    /// </param>
    /// <param name="domain">A new domain to be assigned.</param>
    /// <returns>true if setting succeeded, otherwise false.</returns>
    public virtual bool SetDomain(int direction, Interval domain)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Surface_SetDomain(ptr, direction, domain);
    }

    /// <summary>
    /// Returns the maximum algebraic degree of any span
    /// (or a good estimate if curve spans are not algebraic).
    /// </summary>
    /// <param name="direction">
    /// 0 gets first parameter's domain, 1 gets second parameter's domain.
    /// </param>
    /// <returns>The maximum degree.</returns>
    public int Degree(int direction)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_Degree(ptr, direction);
    }

#if RHINO_SDK
    /// <summary>
    /// Gets an estimate of the size of the rectangle that would be created
    /// if the 3d surface where flattened into a rectangle.
    /// </summary>
    /// <param name="width">corresponds to the first surface parameter.</param>
    /// <param name="height">corresponds to the second surface parameter.</param>
    /// <returns>true if successful.</returns>
    /// <example>
    /// Reparameterize a surface to minimize distortion in the map from parameter space to 3d.
    /// Surface surf = ...;
    /// double width, height;
    /// if ( surf.GetSurfaceSize( out width, out height ) )
    /// {
    ///   surf.SetDomain( 0, new ON_Interval( 0.0, width ) );
    ///   surf.SetDomain( 1, new ON_Interval( 0.0, height ) );
    /// }
    /// </example>
    public bool GetSurfaceSize(out double width, out double height)
    {
      width = 0;
      height = 0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_GetSurfaceSize(ptr, ref width, ref height);
    }
#endif

    /// <summary>
    /// Gets number of smooth nonempty spans in the parameter direction.
    /// </summary>
    /// <param name="direction">
    /// 0 gets first parameter's domain, 1 gets second parameter's domain.
    /// </param>
    /// <returns>The span count.</returns>
    public int SpanCount(int direction)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_SpanCount(ptr, direction);
    }

    /// <summary>
    /// Gets array of span "knots".
    /// </summary>
    /// <param name="direction">
    /// 0 gets first parameter's domain, 1 gets second parameter's domain.
    /// </param>
    /// <returns>An array with span vectors; or null on error.</returns>
    public double[] GetSpanVector(int direction)
    {
      int count = SpanCount(direction) + 1;
      if (count < 1)
        return null;

      double[] rc = new double[count];
      IntPtr ptr = ConstPointer();
      bool success = UnsafeNativeMethods.ON_Surface_GetSpanVector(ptr, direction, rc);
      if (success)
        return rc;
      return null;
    }

#if RHINO_SDK
    /// <summary>
    /// Gets the side that is closest, in terms of 3D-distance, to a U and V parameter.
    /// </summary>
    /// <param name="u">A u parameter.</param>
    /// <param name="v">A v parameter.</param>
    /// <returns>A side.</returns>
    public IsoStatus ClosestSide(double u, double v)
    {
      IntPtr pConstThis = ConstPointer();
      return (IsoStatus)UnsafeNativeMethods.ON_Surface_ClosestSide(pConstThis, u, v);
    }

    /// <summary>
    /// Extends an untrimmed surface along one edge.
    /// </summary>
    /// <param name="edge">
    /// Edge to extend.  Must be North, South, East, or West.
    /// </param>
    /// <param name="extensionLength">distance to extend.</param>
    /// <param name="smooth">
    /// true for smooth (C-infinity) extension. 
    /// false for a C1- ruled extension.
    /// </param>
    /// <returns>New extended surface on success.</returns>
    public Surface Extend(IsoStatus edge, double extensionLength, bool smooth)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNewSurface = UnsafeNativeMethods.RHC_RhinoExtendSurface(pConstThis, (int)edge, extensionLength, smooth);
      return GeometryBase.CreateGeometryHelper(pNewSurface, null) as Surface;
    }

    /// <summary>
    /// Rebuilds an existing surface to a given degree and point count.
    /// </summary>
    /// <param name="uDegree">the output surface u degree.</param>
    /// <param name="vDegree">the output surface u degree.</param>
    /// <param name="uPointCount">
    /// The number of points in the output surface u direction. Must be bigger
    /// than uDegree (maximum value is 1000)
    /// </param>
    /// <param name="vPointCount">
    /// The number of points in the output surface v direction. Must be bigger
    /// than vDegree (maximum value is 1000)
    /// </param>
    /// <returns>new rebuilt surface on success. null on failure.</returns>
    public NurbsSurface Rebuild(int uDegree, int vDegree, int uPointCount, int vPointCount)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNewSurface = UnsafeNativeMethods.RHC_RhinoRebuildSurface(pConstThis, uDegree, vDegree, uPointCount, vPointCount);
      return GeometryBase.CreateGeometryHelper(pNewSurface, null) as NurbsSurface;
    }
#endif

    /// <summary>
    /// Reverses parameterization Domain changes from [a,b] to [-b,-a]
    /// </summary>
    /// <param name="direction">
    /// 0 for first parameter's domain, 1 for second parameter's domain.
    /// </param>
    /// <returns>a new reversed surface on success.</returns>
    public Surface Reverse(int direction)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNewSurface = UnsafeNativeMethods.ON_Surface_Reverse(pConstThis, direction);
      return GeometryBase.CreateGeometryHelper(pNewSurface, null) as Surface;
    }

    /// <summary>
    /// Same as Reverse, but if inPlace is set to true this Surface is modified
    /// instead of a new copy being created.
    /// </summary>
    /// <param name="direction">
    /// 0 for first parameter's domain, 1 for second parameter's domain.
    /// </param>
    /// <param name="inPlace"></param>
    /// <returns>
    /// If inPlace is False, a new reversed surface on success. If inPlace is
    /// true, this surface instance is returned on success.
    /// </returns>
    public Surface Reverse(int direction, bool inPlace)
    {
      if (!inPlace)
        return Reverse(direction);
      IntPtr ptr_this = NonConstPointer();
      if( UnsafeNativeMethods.ON_Surface_Reverse2(ptr_this, direction) )
        return this;
      return null;
    }

    /// <summary>
    /// Transposes surface parameterization (swap U and V)
    /// </summary>
    /// <returns>New transposed surface on success, null on failure.</returns>
    public Surface Transpose()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNewSurface = UnsafeNativeMethods.ON_Surface_Transpose(pConstThis);
      return GeometryBase.CreateGeometryHelper(pNewSurface, null) as Surface;
    }

    /// <summary>
    /// Transposes surface parameterization (swap U and V)
    /// </summary>
    /// <param name="inPlace"></param>
    /// <returns>New transposed surface on success, null on failure.</returns>
    public Surface Transpose(bool inPlace)
    {
      if (!inPlace)
        return Transpose();
      IntPtr ptr_this = NonConstPointer();
      if (UnsafeNativeMethods.ON_Surface_Transpose2(ptr_this))
        return this;
      return null;
    }

    /// <summary>
    /// Evaluates a point at a given parameter.
    /// </summary>
    /// <param name="u">evaluation parameters.</param>
    /// <param name="v">evaluation parameters.</param>
    /// <returns>Point3d.Unset on failure.</returns>
    public Point3d PointAt(double u, double v)
    {
      Point3d rc = new Point3d();
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.ON_Surface_EvPoint(ptr, u, v, ref rc))
        rc = Point3d.Unset;
      return rc;
    }
    /// <summary>
    /// Computes the surface normal at a point.
    /// <para>This is the simple evaluation call - it does not support error handling.</para>
    /// </summary>
    /// <param name="u">A U parameter.</param>
    /// <param name="v">A V parameter.</param>
    /// <returns>The normal.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_evnormal.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_evnormal.cs' lang='cs'/>
    /// <code source='examples\py\ex_evnormal.py' lang='py'/>
    /// </example>
    public Vector3d NormalAt(double u, double v)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_Surface_NormalAt(ptr, u, v, ref rc);
      return rc;
    }
    /// <summary>
    /// Computes the orient plane on a surface given a U and V parameter.
    /// <para>This is the simple evaluation call with no error handling.</para>
    /// </summary>
    /// <param name="u">A first parameter.</param>
    /// <param name="v">A second parameter.</param>
    /// <param name="frame">A frame plane that will be computed during this call.</param>
    /// <returns>true if this operation succeeded; otherwise false.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool FrameAt(double u, double v, out Plane frame)
    {
      frame = new Plane();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_FrameAt(ptr, u, v, ref frame);
    }
    /// <summary>
    /// Computes the curvature at the given UV coordinate.
    /// </summary>
    /// <param name="u">U parameter for evaluation.</param>
    /// <param name="v">V parameter for evaluation.</param>
    /// <returns>Surface Curvature data for the point at uv or null on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_principalcurvature.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_principalcurvature.cs' lang='cs'/>
    /// <code source='examples\py\ex_principalcurvature.py' lang='py'/>
    /// </example>
    public SurfaceCurvature CurvatureAt(double u, double v)
    {
      IntPtr pConstThis = ConstPointer();
      return SurfaceCurvature._FromSurfacePointer(pConstThis, u, v);
    }

    //[skipping]
    //  virtual BOOL GetParameterTolerance( // returns tminus < tplus: parameters tminus <= s <= tplus

    /// <summary>
    /// Determines if a 2D curve is iso-parameteric in the parameter space of this surface.
    /// </summary>
    /// <param name="curve">Curve to test.</param>
    /// <param name="curveDomain">Sub domain of the curve.</param>
    /// <returns>IsoStatus flag describing the iso-parametric relationship between the surface and the curve.</returns>
    public IsoStatus IsIsoparametric(Curve curve, Interval curveDomain)
    {
      if (null == curve)
        return IsoStatus.None;
      IntPtr ptr = ConstPointer();
      IntPtr pCurve = curve.ConstPointer();
      int rc = UnsafeNativeMethods.ON_Surface_IsIsoparametric(ptr, pCurve, curveDomain);
      return (IsoStatus)rc;
    }
    /// <summary>
    /// Determines if a 2d curve is iso-parameteric in the parameter space of this surface.
    /// </summary>
    /// <param name="curve">Curve to test.</param>
    /// <returns>IsoStatus flag describing the iso-parametric relationship between the surface and the curve.</returns>
    public IsoStatus IsIsoparametric(Curve curve)
    {
      return IsIsoparametric(curve, Interval.Unset);
    }
    /// <summary>
    /// Determines if a 2d bounding box is iso-parameteric in the parameter space of this surface.
    /// </summary>
    /// <param name="bbox">Bounding box to test.</param>
    /// <returns>IsoStatus flag describing the iso-parametric relationship between the surface and the bounding box.</returns>
    public IsoStatus IsIsoparametric(BoundingBox bbox)
    {
      IntPtr ptr = ConstPointer();
      int rc = UnsafeNativeMethods.ON_Surface_IsIsoparametric2(ptr, bbox.Min, bbox.Max);
      return (IsoStatus)rc;
    }

    /// <summary>
    /// Gets a value indicating if the surface is closed in a direction.
    /// </summary>
    /// <param name="direction">0 = U, 1 = V.</param>
    /// <returns>The indicating boolean value.</returns>
    public bool IsClosed(int direction)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_GetBool(ptr, direction, idxIsClosed);
    }
    /// <summary>
    /// Gets a value indicating if thr surface is periodic in a direction (default is false).
    /// </summary>
    /// <param name="direction">0 = U, 1 = V.</param>
    /// <returns>The indicating boolean value.</returns>
    public bool IsPeriodic(int direction)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_GetBool(ptr, direction, idxIsPeriodic);
    }
    /// <summary>
    /// true if surface side is collapsed to a point.
    /// </summary>
    /// <param name="side">
    /// side of parameter space to test
    /// 0 = south, 1 = east, 2 = north, 3 = west.
    /// </param>
    /// <returns>True if this specific side of the surface is singular; otherwise, false.</returns>
    public bool IsSingular(int side)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_GetBool(ptr, side, idxIsSingular);
    }
    /// <summary>
    /// Tests if a surface parameter value is at a singularity.
    /// </summary>
    /// <param name="u">Surface u parameter to test.</param>
    /// <param name="v">Surface v parameter to test.</param>
    /// <param name="exact">
    /// If true, test if (u,v) is exactly at a singularity.
    /// If false, test if close enough to cause numerical problems.
    /// </param>
    /// <returns>true if surface is singular at (s,t)</returns>
    public bool IsAtSingularity(double u, double v, bool exact)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsAtSingularity(ptr, u, v, exact);
    }
    /// <summary>
    /// Tests if a surface parameter value is at a seam.
    /// </summary>
    /// <param name="u">Surface u parameter to test.</param>
    /// <param name="v">Surface v parameter to test.</param>
    /// <returns>
    /// 0 if not a seam,
    /// 1 if u == Domain(0)[i] and srf(u, v) == srf(Domain(0)[1-i], v)
    /// 2 if v == Domain(1)[i] and srf(u, v) == srf(u, Domain(1)[1-i])
    /// 3 if 1 and 2 are true.
    /// </returns>
    public int IsAtSeam(double u, double v)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsAtSeam(ptr, u, v);
    }

    /// <summary>
    /// Tests continuity at a surface parameter value.
    /// </summary>
    /// <param name="continuityType">The continuity type to sample.</param>
    /// <param name="u">Surface u parameter to test.</param>
    /// <param name="v">Surface v parameter to test.</param>
    /// <returns>true if the surface has at least the specified continuity at the (u,v) parameter.</returns>
    public bool IsContinuous(Continuity continuityType, double u, double v)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsContinuous(ptr, (int)continuityType, u, v);
    }
    /// <summary>
    /// Searches for a derivative, tangent, or curvature discontinuity.
    /// </summary>
    /// <param name="direction">
    /// If 0, then "u" parameter is checked. If 1, then the "v" parameter is checked.
    /// </param>
    /// <param name="continuityType">The desired continuity.</param>
    /// <param name="t0">
    /// Search begins at t0. If there is a discontinuity at t0, it will be ignored. 
    /// This makes it possible to repeatedly call GetNextDiscontinuity and step through the discontinuities.
    /// </param>
    /// <param name="t1">
    /// (t0 != t1) If there is a discontinuity at t1 is will be ingored unless c is a locus discontinuity
    /// type and t1 is at the start or end of the curve.
    /// </param>
    /// <param name="t">
    /// if a discontinuity is found, then t reports the parameter at the discontinuity.
    /// </param>
    /// <returns>
    /// Parametric continuity tests c = (C0_continuous, ..., G2_continuous):
    /// TRUE if a parametric discontinuity was found strictly between t0 and t1.
    /// Note well that all curves are parametrically continuous at the ends of their domains.
    /// 
    /// Locus continuity tests c = (C0_locus_continuous, ...,G2_locus_continuous):
    /// TRUE if a locus discontinuity was found strictly between t0 and t1 or at t1 is the
    /// at the end of a curve. Note well that all open curves (IsClosed()=false) are locus
    /// discontinuous at the ends of their domains.  All closed curves (IsClosed()=true) are
    /// at least C0_locus_continuous at the ends of their domains.
    /// </returns>
    public bool GetNextDiscontinuity(int direction, Continuity continuityType, double t0, double t1, out double t)
    {
      IntPtr ptr = ConstPointer();
      t = 0;
      return UnsafeNativeMethods.ON_Surface_GetNextDiscontinuity(ptr, direction, (int)continuityType, t0, t1, ref t);
    }

    // [skipping]
    //  ON_NurbsSurface* NurbsSurface(
    //  void DestroySurfaceTree();
    //  const ON_SurfaceTree* SurfaceTree() const;
    //  virtual ON_SurfaceTree* CreateSurfaceTree() const;

#if RHINO_SDK
    /// <summary>
    /// Input the parameters of the point on the surface that is closest to testPoint.
    /// </summary>
    /// <param name="testPoint">A point to test against.</param>
    /// <param name="u">U parameter of the surface that is closest to testPoint.</param>
    /// <param name="v">V parameter of the surface that is closest to testPoint.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_orientonsrf.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_orientonsrf.cs' lang='cs'/>
    /// <code source='examples\py\ex_orientonsrf.py' lang='py'/>
    /// </example>
    public bool ClosestPoint(Point3d testPoint, out double u, out double v)
    {
      u = 0;
      v = 0;
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Surface_GetClosestPoint(ptr, testPoint, ref u, ref v);
      return rc;
    }
#endif

    /// <summary>
    /// Constructs a sub-surface that covers the specified UV trimming domain.
    /// </summary>
    /// <param name="u">Domain of surface along U direction to include in the subsurface.</param>
    /// <param name="v">Domain of surface along V direction to include in the subsurface.</param>
    /// <returns>SubSurface on success, null on failure.</returns>
    public Surface Trim(Interval u, Interval v)
    {
      u.MakeIncreasing();
      v.MakeIncreasing();

      if (!u.IsValid || u.IsSingleton)
        return null;
      if (!v.IsValid || v.IsSingleton)
        return null;

      IntPtr ptr = ConstPointer();
      IntPtr pSurface = UnsafeNativeMethods.ON_Surface_Trim(ptr, u, v);
      GeometryBase g = GeometryBase.CreateGeometryHelper(pSurface, null);
      Surface rc = g as Surface;
      return rc;
    }

#if RHINO_SDK
    /// <summary>
    /// Constructs a new surface which is offset from the current surface.
    /// </summary>
    /// <param name="distance">Distance (along surface normal) to offset.</param>
    /// <param name="tolerance">Offset accuracy.</param>
    /// <returns>The offsetted surface or null on failure.</returns>
    public Surface Offset(double distance, double tolerance)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNewSurface = UnsafeNativeMethods.ON_Surface_Offset(pConstThis, distance, tolerance);
      GeometryBase g = GeometryBase.CreateGeometryHelper(pNewSurface, null);
      Surface rc = g as Surface;
      return rc;
    }

    /// <summary>Fits a new surface through an existing surface.</summary>
    /// <param name="uDegree">the output surface U degree. Must be bigger than 1.</param>
    /// <param name="vDegree">the output surface V degree. Must be bigger than 1.</param>
    /// <param name="fitTolerance">The fitting tolerance.</param>
    /// <returns>A surface, or null on error.</returns>
    public Surface Fit(int uDegree, int vDegree, double fitTolerance)
    {
      IntPtr pConstThis = ConstPointer();
      double achievedTol = 0;
      IntPtr rc = UnsafeNativeMethods.RHC_RhinoFitSurface(pConstThis, uDegree, vDegree, fitTolerance, ref achievedTol);
      return GeometryBase.CreateGeometryHelper(rc, null) as Surface;
    }
#endif

    /// <summary>
    /// Evaluates a surface mathematically.
    /// </summary>
    /// <param name="u">A U parameter.</param>
    /// <param name="v">A V parameter.</param>
    /// <param name="numberDerivatives">The number of derivatives.</param>
    /// <param name="point">A point. This out parameter will be assigned during this call.</param>
    /// <param name="derivatives">A vector array. This out parameter will be assigned during this call. This can be null.</param>
    /// <returns>true if the operation succeeded; false otherwise.</returns>
    public bool Evaluate(double u, double v, int numberDerivatives, out Point3d point, out Vector3d[] derivatives)
    {
      point = Point3d.Unset;
      derivatives = null;
      if (numberDerivatives < 0)
        return false;
      IntPtr pConstThis = ConstPointer();
      const int stride = 3;
      int count = (numberDerivatives + 1) * (numberDerivatives + 2) / 2;
      int size = stride * count;
      double[] der_array = new double[size];
      bool rc = UnsafeNativeMethods.ON_Surface_Evaluate(pConstThis, u, v, numberDerivatives, stride, der_array);
      if (rc)
      {
        point = new Point3d(der_array[0], der_array[1], der_array[2]);
        if (count > 1)
        {
          derivatives = new Vector3d[count - 1];
          for (int i = 1; i < count; i++)
          {
            int index = i * stride;
            derivatives[i - 1] = new Vector3d(der_array[index], der_array[index + 1], der_array[index + 2]);
          }
        }
      }
      return rc;
    }

    /// <summary>Gets isoparametric curve.</summary>
    /// <param name="direction">
    /// 0 first parameter varies and second parameter is constant
    /// e.g., point on IsoCurve(0,c) at t is srf(t,c)
    /// This is a horizontal line from left to right
    /// 
    /// 1 first parameter is constant and second parameter varies
    /// e.g., point on IsoCurve(1,c) at t is srf(c,t
    /// This is a vertical line from bottom to top.
    /// </param>
    /// <param name="constantParameter">The parameter that was constant on the original surface.</param>
    /// <returns>An isoparametric curve or null on error.</returns>
    /// <remarks>
    /// In this function "direction" indicates which direction the resulting curve runs.
    /// 0: horizontal, 1: vertical
    /// In the other Surface functions that take a "direction" argument,
    /// "direction" indicates if "constantParameter" is a "u" or "v" parameter.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_extractisocurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_extractisocurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_extractisocurve.py' lang='py'/>
    /// </example>
    public Curve IsoCurve(int direction, double constantParameter)
    {
      IntPtr ptr = ConstPointer();
      IntPtr pCurve = UnsafeNativeMethods.ON_Surface_IsoCurve(ptr, direction, constantParameter);
      return GeometryBase.CreateGeometryHelper(pCurve, null) as Curve;
    }

#if RHINO_SDK
    /// <summary>
    /// Constructs an interpolated curve on a surface, using 2D surface points.
    /// </summary>
    /// <param name="points">A list, an array or any enumerable set of 2D points.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <returns>A new nurbs curve, or null on error.</returns>
    public NurbsCurve InterpolatedCurveOnSurfaceUV(System.Collections.Generic.IEnumerable<Point2d> points, double tolerance)
    {
      NurbsCurve rc = null;
      if (null == points)
        return null;

      Point2d[] ptArray = points as Point2d[];
      if (null == ptArray)
      {
        System.Collections.Generic.IList<Point2d> pointList = points as System.Collections.Generic.IList<Point2d>;
        if (pointList != null)
        {
          ptArray = new Point2d[pointList.Count];
          pointList.CopyTo(ptArray, 0);
        }
        else
        {
          System.Collections.Generic.List<Point2d> list = new System.Collections.Generic.List<Point2d>();
          foreach (Point2d pt in list)
          {
            list.Add(pt);
          }
          ptArray = list.ToArray();
        }
      }

      int count = ptArray.Length;
      if (count >= 2)
      {
        // check for closed curve
        int is_closed = 0;
        if (count > 3)
        {
          Point2d pt = ptArray[0];
          if (pt.DistanceTo(ptArray[count - 1]) < RhinoMath.SqrtEpsilon)
            is_closed = 1;
        }

        IntPtr ptr = ConstPointer();
        IntPtr pNC = UnsafeNativeMethods.ON_Surface_InterpCrvOnSrf(ptr, count, ptArray, is_closed, tolerance, 1);
        rc = GeometryBase.CreateGeometryHelper(pNC, null) as NurbsCurve;
      }
      return rc;
    }

    /// <summary>
    /// Constructs an interpolated curve on a surface, using 3D points.
    /// </summary>
    /// <param name="points">A list, an array or any enumerable set of points.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <returns>A new nurbs curve, or null on error.</returns>
    public NurbsCurve InterpolatedCurveOnSurface(System.Collections.Generic.IEnumerable<Point3d> points, double tolerance)
    {
      if (null == points)
        return null;

      // Input points on the surface
      System.Collections.Generic.List<Point2d> points2d = new System.Collections.Generic.List<Point2d>();
      foreach (Point3d pt in points)
      {
        double s, t;
        if (!ClosestPoint(pt, out s, out t))
          continue;
        Point3d srf_pt = PointAt(s, t);
        if (!srf_pt.IsValid)
          continue;
        if (srf_pt.DistanceTo(pt) > RhinoMath.SqrtEpsilon)
          continue;
        points2d.Add(new Point2d(s, t));
      }

      NurbsCurve rc = InterpolatedCurveOnSurfaceUV(points2d, tolerance);
      return rc;
    }

    /// <summary>
    /// Constructs a geodesic between 2 points, used by ShortPath command in Rhino.
    /// </summary>
    /// <param name="start">start point of curve in parameter space. Points must be distinct in the domain of thie surface.</param>
    /// <param name="end">end point of curve in parameter space. Points must be distinct in the domain of thie surface.</param>
    /// <param name="tolerance">tolerance used in fitting discrete solution.</param>
    /// <returns>a geodesic curve on the surface on success. null on failure.</returns>
    public Curve ShortPath(Point2d start, Point2d end, double tolerance)
    {
      IntPtr pConstSurface = ConstPointer();
      IntPtr pNewCurve = UnsafeNativeMethods.RHC_RhinoShortPath(pConstSurface, start, end, tolerance);
      return GeometryBase.CreateGeometryHelper(pNewCurve, null) as Curve;
    }

    /// <summary>
    /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
    /// </summary>
    /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
    /// <param name="tolerance">
    /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
    /// </param>
    /// <param name="curve2dSubdomain">The curve interval (a sub-domain of the original curve) to use.</param>
    /// <returns>3d curve.</returns>
    public Curve Pushup(Curve curve2d, double tolerance, Interval curve2dSubdomain)
    {
      if (null == curve2d)
        return null;
      IntPtr ptr = ConstPointer();
      IntPtr pCurve2d = curve2d.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Surface_Pushup(ptr, pCurve2d, tolerance, curve2dSubdomain);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }
    /// <summary>
    /// Computes a 3d curve that is the composite of a 2d curve and the surface map.
    /// </summary>
    /// <param name="curve2d">a 2d curve whose image is in the surface's domain.</param>
    /// <param name="tolerance">
    /// the maximum acceptable distance from the returned 3d curve to the image of curve_2d on the surface.
    /// </param>
    /// <returns>3d curve.</returns>
    public Curve Pushup(Curve curve2d, double tolerance)
    {
      return Pushup(curve2d, tolerance, Interval.Unset);
    }
    /// <summary>
    /// Pulls a 3d curve back to the surface's parameter space.
    /// </summary>
    /// <param name="curve3d">The curve to pull.</param>
    /// <param name="tolerance">
    /// the maximum acceptable 3d distance between from surface(curve_2d(t))
    /// to the locus of points on the surface that are closest to curve_3d.
    /// </param>
    /// <returns>2d curve.</returns>
    public Curve Pullback(Curve curve3d, double tolerance)
    {
      return Pullback(curve3d, tolerance, Interval.Unset);
    }
    /// <summary>
    /// Pulls a 3d curve back to the surface's parameter space.
    /// </summary>
    /// <param name="curve3d">A curve.</param>
    /// <param name="tolerance">
    /// the maximum acceptable 3d distance between from surface(curve_2d(t))
    /// to the locus of points on the surface that are closest to curve_3d.
    /// </param>
    /// <param name="curve3dSubdomain">A subdomain of the curve to sample.</param>
    /// <returns>2d curve.</returns>
    public Curve Pullback(Curve curve3d, double tolerance, Interval curve3dSubdomain)
    {
      if (null == curve3d)
        return null;

      IntPtr ptr = ConstPointer();
      IntPtr pCurve3d = curve3d.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Surface_Pullback(ptr, pCurve3d, tolerance, curve3dSubdomain);
      return GeometryBase.CreateGeometryHelper(rc, null) as Curve;
    }
#endif

    #region converters
    /// <summary>
    /// Converts the surface into a Brep.
    /// </summary>
    /// <returns>A Brep with a similar shape like this surface or null.</returns>
    public Brep ToBrep()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pBrep = UnsafeNativeMethods.ON_Surface_BrepForm(ptr);
      if (IntPtr.Zero == pBrep)
        return null;
      return new Brep(pBrep, null);
    }

    /// <summary>
    /// Is there a NURBS surface representation of this surface.
    /// </summary>
    /// <returns>
    /// 0 unable to create NURBS representation with desired accuracy.
    /// 1 success - NURBS parameterization matches the surface's
    /// 2 success - NURBS point locus matches the surface's and the
    /// domain of the NURBS surface is correct. However, This surface's
    /// parameterization and the NURBS surface parameterization may not
    /// match.  This situation happens when getting NURBS representations
    /// of surfaces that have a transendental parameterization like spheres,
    /// cylinders, and cones.
    /// </returns>
    public int HasNurbsForm()
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_HasNurbsForm(ptr);
    }
    /// <summary>
    /// Gets a NURBS surface representation of this surface. Default 
    /// tolerance of 0.0 is used. 
    /// </summary>
    /// <returns>NurbsSurface on success, null on failure.</returns>
    public NurbsSurface ToNurbsSurface()
    {
      int accuracy;
      return ToNurbsSurface(0.0, out accuracy);
    }
    /// <summary>
    /// Gets a NURBS surface representation of this surface.
    /// </summary>
    /// <param name="tolerance">tolerance to use when creating NURBS representation.</param>
    /// <param name="accuracy">
    /// <para>
    /// 0 = unable to create NURBS representation with desired accuracy.
    /// </para>
    /// <para>
    /// 1 = success - returned NURBS parameterization matches the surface's
    /// to the desired accuracy.
    /// </para>
    /// <para>
    /// 2 = success - returned NURBS point locus matches the surface's to the
    /// desired accuracy and the domain of the NURBS surface is correct. 
    /// However, this surface's parameterization and the NURBS surface
    /// parameterization may not match to the desired accuracy. This 
    /// situation happens when getting NURBS representations of surfaces
    /// that have a transendental parameterization like spheres, cylinders,
    /// and cones.
    /// </para>
    /// </param>
    /// <returns>NurbsSurface on success, null on failure.</returns>
    public NurbsSurface ToNurbsSurface(double tolerance, out int accuracy)
    {
      accuracy = 0;
      IntPtr ptr = ConstPointer();

      IntPtr rc = UnsafeNativeMethods.ON_Surface_GetNurbForm(ptr, tolerance, ref accuracy);

      if (rc == IntPtr.Zero)
        return null;
      return new NurbsSurface(rc, null);
    }

    /// <summary>
    /// Tests a surface to see if it is planar to zero tolerance.
    /// </summary>
    /// <returns>
    /// true if the surface is planar (flat) to within RhinoMath.ZeroTolerance units (1e-12).
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_issurfaceinplane.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_issurfaceinplane.cs' lang='cs'/>
    /// <code source='examples\py\ex_issurfaceinplane.py' lang='py'/>
    /// </example>
    public bool IsPlanar()
    {
      return IsPlanar(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Tests a surface to see if it is planar to a given tolerance.
    /// </summary>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>
    /// true if there is a plane such that the maximum distance from
    /// the surface to the plane is &lt;= tolerance.
    /// </returns>
    public bool IsPlanar(double tolerance)
    {
      Plane plane = new Plane();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsPlanar(ptr, ref plane, tolerance, false);
    }
    /// <summary>Tests a surface for planarity and return the plane.</summary>
    /// <param name="plane">On success, the plane parameters are filled in.</param>
    /// <returns>
    /// true if there is a plane such that the maximum distance from the surface to the plane is &lt;= RhinoMath.ZeroTolerance.
    /// </returns>
    public bool TryGetPlane(out Plane plane)
    {
      return TryGetPlane(out plane, RhinoMath.ZeroTolerance);
    }
    /// <summary>Tests a surface for planarity and return the plane.</summary>
    /// <param name="plane">On success, the plane parameters are filled in.</param>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>
    /// true if there is a plane such that the maximum distance from the surface to the plane is &lt;= tolerance.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_isbrepbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_isbrepbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_isbrepbox.py' lang='py'/>
    /// </example>
    public bool TryGetPlane(out Plane plane, double tolerance)
    {
      plane = new Plane();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsPlanar(ptr, ref plane, tolerance, true);
    }

    /// <summary>
    /// Determines if the surface is a portion of a sphere within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>true if the surface is a portion of a sphere.</returns>
    public bool IsSphere()
    {
      return IsSphere(RhinoMath.ZeroTolerance);
    }
    /// <summary>
    /// Determines if the surface is a portion of a sphere within a given tolerance.
    /// </summary>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a sphere.</returns>
    public bool IsSphere(double tolerance)
    {
      Sphere sphere = new Sphere();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsSphere(pThis, ref sphere, tolerance, false);
    }
    /// <summary>Test a surface to see if it is a portion of a sphere and return the sphere.</summary>
    /// <param name="sphere">On success, the sphere parameters are filled in.</param>
    /// <returns>true if the surface is a portion of a sphere.</returns>
    public bool TryGetSphere(out Sphere sphere)
    {
      return TryGetSphere(out sphere, RhinoMath.ZeroTolerance);
    }
    /// <summary>Test a surface to see if it is a portion of a sphere and return the sphere.</summary>
    /// <param name="sphere">On success, the sphere parameters are filled in.</param>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a sphere.</returns>
    public bool TryGetSphere(out Sphere sphere, double tolerance)
    {
      sphere = new Sphere();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsSphere(pThis, ref sphere, tolerance, true);
    }

    /// <summary>
    /// Determines if the surface is a portion of a cylinder within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>true if the surface is a portion of a cylinder.</returns>
    public bool IsCylinder()
    {
      return IsCylinder(RhinoMath.ZeroTolerance);
    }
    /// <summary>Determines if the surface is a portion of a cylinder within a given tolerance.</summary>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a cylinder.</returns>
    public bool IsCylinder(double tolerance)
    {
      Cylinder cylinder = new Cylinder();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsCylinder(pThis, ref cylinder, tolerance, false);
    }
    /// <summary>Tests a surface to see if it is a portion of a cylinder within RhinoMath.ZeroTolerance and return the cylinder.</summary>
    /// <param name="cylinder">On success, the cylinder parameters are filled in.</param>
    /// <returns>true if the surface is a portion of a cylinder.</returns>
    public bool TryGetCylinder(out Cylinder cylinder)
    {
      return TryGetCylinder(out cylinder, RhinoMath.ZeroTolerance);
    }
    /// <summary>Tests a surface to see if it is a portion of a cylinder and return the cylinder.</summary>
    /// <param name="cylinder">On success, the cylinder parameters are filled in.</param>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a cylinder.</returns>
    public bool TryGetCylinder(out Cylinder cylinder, double tolerance)
    {
      cylinder = new Cylinder();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsCylinder(pThis, ref cylinder, tolerance, true);
    }

    /// <summary>
    /// Determines if the surface is a portion of a cone within RhinoMath.ZeroTolerance.
    /// </summary>
    /// <returns>true if the surface is a portion of a cone.</returns>
    public bool IsCone()
    {
      return IsCone(RhinoMath.ZeroTolerance);
    }
    /// <summary>Determines if the surface is a portion of a cone within a given tolerance.</summary>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a cone.</returns>
    public bool IsCone(double tolerance)
    {
      Cone cone = new Cone();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsCone(pThis, ref cone, tolerance, false);
    }
    /// <summary>Tests a surface to see if it is a portion of a cone within RhinoMath.ZeroTolerance and return the cone.</summary>
    /// <param name="cone">On success, the cone parameters are filled in.</param>
    /// <returns>true if the surface is a portion of a cone.</returns>
    public bool TryGetCone(out Cone cone)
    {
      return TryGetCone(out cone, RhinoMath.ZeroTolerance);
    }
    /// <summary>Tests a surface to see if it is a portion of a cone and returns the cone.</summary>
    /// <param name="cone">On success, the cone parameters are filled in.</param>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a cone.</returns>
    public bool TryGetCone(out Cone cone, double tolerance)
    {
      cone = new Cone();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsCone(pThis, ref cone, tolerance, true);
    }

    /// <summary>Determines if the surface is a portion of a torus within RhinoMath.ZeroTolerance.</summary>
    /// <returns>true if the surface is a portion of a torus.</returns>
    public bool IsTorus()
    {
      return IsTorus(RhinoMath.ZeroTolerance);
    }
    /// <summary>Determines if the surface is a portion of a torus within a given tolerance.</summary>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a torus.</returns>
    public bool IsTorus(double tolerance)
    {
      Torus torus = new Torus();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsTorus(pThis, ref torus, tolerance, false);
    }
    /// <summary>Tests a surface to see if it is a portion of a torus within RhinoMath.ZeroTolerance and returns the torus.</summary>
    /// <param name="torus">On success, the torus parameters are filled in.</param>
    /// <returns>true if the surface is a portion of a torus.</returns>
    public bool TryGetTorus(out Torus torus)
    {
      return TryGetTorus(out torus, RhinoMath.ZeroTolerance);
    }
    /// <summary>Tests a surface to see if it is a portion of a torus and returns the torus.</summary>
    /// <param name="torus">On success, the torus parameters are filled in.</param>
    /// <param name="tolerance">tolerance to use when checking.</param>
    /// <returns>true if the surface is a portion of a torus.</returns>
    public bool TryGetTorus(out Torus torus, double tolerance)
    {
      torus = new Torus();
      IntPtr pThis = ConstPointer();
      return UnsafeNativeMethods.ON_Surface_IsTorus(pThis, ref torus, tolerance, true);
    }
    #endregion
    #endregion

    const int idxIsClosed = 0;
    const int idxIsPeriodic = 1;
    const int idxIsSingular = 2;
    const int idxIsSolid = 3;

    /// <summary>
    /// Splits (divides) the surface into two parts at the specified parameter
    /// </summary>
    /// <param name="direction">
    /// 0 = The surface is split vertically. The "west" side is returned as the first
    /// surface in the array and the "east" side is returned as the second surface in
    /// the array.
    /// 1 = The surface is split horizontally. The "south" side is returned as the first surface in the array and the "north"
    /// side is returned as the second surfae in the array
    /// </param>
    /// <param name="parameter">
    /// value of constant parameter in interval returned by Domain(direction)
    /// </param>
    /// <returns>Array of two surfaces on success</returns>
    public Surface[] Split(int direction, double parameter)
    {
      using (var surfaces = new Runtime.InteropWrappers.SimpleArraySurfacePointer())
      {
        IntPtr pSurfaces = surfaces.NonConstPointer();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Surface_Split(pConstThis, direction, parameter, pSurfaces);
        return surfaces.ToNonConstArray();
      }
    }

    //[skipping]
    //  bool Extend( int dir,  const ON_Interval& domain );
    //  BOOL GetLocalClosestPoint( const ON_3dPoint&, // test_point

    //  virtual int GetNurbForm( ON_NurbsSurface& nurbs_surface,
    //const int idxSurfParamFromNurbs = 0;
    //const int idxNurbsParamFromSurf = 1;
    /* Leaving out until we have a request for this
        public bool SurfaceParameterFromNurbsFormParameter(double nurbsS, double nurbsT, out double surfaceS, out double surfaceT)
        {
          IntPtr ptr = ConstPointer();
          surfaceS = 0;
          surfaceT = 0;
          return UnsafeNativeMethods.ON_Surface_GetParameter(ptr, nurbsS, nurbsT, ref surfaceS, ref surfaceT, idxSurfParamFromNurbs);
        }
        public bool NurbsFormParameterFromSurfaceParameter(double surfaceS, double surfaceT, out double nurbsS, out double nurbsT)
        {
          IntPtr ptr = ConstPointer();
          nurbsS = 0;
          nurbsT = 0;
          return UnsafeNativeMethods.ON_Surface_GetParameter(ptr, surfaceS, surfaceT, ref nurbsS, ref nurbsT, idxNurbsParamFromSurf);
        }
    */

    /// <summary>
    /// Gets a values indicating whether a surface is solid.
    /// </summary>
    public virtual bool IsSolid
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Surface_GetBool(ptr, 0, idxIsSolid);
      }
    }

#if RHINO_SDK
    internal virtual void Draw(Display.DisplayPipeline pipeline, Rhino.Drawing.Color color, int density)
    {
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      IntPtr ptr = ConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawSurface(pDisplayPipeline, ptr, argb, density);
    }
#endif
  }
}