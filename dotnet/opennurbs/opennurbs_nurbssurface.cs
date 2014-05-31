using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

//public class ON_TensorProduct { } never seen this used
//  public class ON_CageMorph { }

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a Non Uniform Rational B-Splines (NURBS) surface.
  /// </summary>
  //[Serializable]
  public class NurbsSurface : Surface, IEpsilonComparable<NurbsSurface>
  {
    #region static create functions
    /// <summary>
    /// Constructs a new NURBS surface with internal uninitialized arrays.
    /// </summary>
    /// <param name="dimension">The number of dimensions.<para>&gt;= 1. This value is usually 3.</para></param>
    /// <param name="isRational">true to make a rational NURBS.</param>
    /// <param name="order0">The order in U direction.<para>&gt;= 2.</para></param>
    /// <param name="order1">The order in V direction.<para>&gt;= 2.</para></param>
    /// <param name="controlPointCount0">Control point count in U direction.<para>&gt;= order0.</para></param>
    /// <param name="controlPointCount1">Control point count in V direction.<para>&gt;= order1.</para></param>
    /// <returns>A new NURBS surface, or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_createsurfaceexample.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createsurfaceexample.cs' lang='cs'/>
    /// <code source='examples\py\ex_createsurfaceexample.py' lang='py'/>
    /// </example>
    public static NurbsSurface Create(int dimension, bool isRational, int order0, int order1, int controlPointCount0, int controlPointCount1)
    {
      if (dimension < 1 || order0 < 2 || order1 < 2 || controlPointCount0 < order0 || controlPointCount1 < order1)
        return null;
      IntPtr ptr = UnsafeNativeMethods.ON_NurbsSurface_New(dimension, isRational, order0, order1, controlPointCount0, controlPointCount1);
      if (IntPtr.Zero == ptr)
        return null;
      return new NurbsSurface(ptr, null);
    }

    /// <summary>
    /// Constructs a new NURBS surfaces from cone data.
    /// </summary>
    /// <param name="cone">A cone value.</param>
    /// <returns>A new NURBS surface, or null on error.</returns>
    public static NurbsSurface CreateFromCone(Cone cone)
    {
      IntPtr pNurbsSurface = UnsafeNativeMethods.ON_Cone_GetNurbForm(ref cone);
      if (IntPtr.Zero == pNurbsSurface)
        return null;
      return new NurbsSurface(pNurbsSurface, null);
    }

    /// <summary>
    /// Constructs a new NURBS surfaces from cylinder data.
    /// </summary>
    /// <param name="cylinder">A cylinder value.</param>
    /// <returns>A new NURBS surface, or null on error.</returns>
    public static NurbsSurface CreateFromCylinder(Cylinder cylinder)
    {
      IntPtr pNurbsSurface = UnsafeNativeMethods.ON_Cylinder_GetNurbForm(ref cylinder);
      if (IntPtr.Zero == pNurbsSurface)
        return null;
      return new NurbsSurface(pNurbsSurface, null);
    }

    /// <summary>
    /// Constructs a new NURBS surfaces from sphere data.
    /// </summary>
    /// <param name="sphere">A sphere value.</param>
    /// <returns>A new NURBS surface, or null on error.</returns>
    public static NurbsSurface CreateFromSphere(Sphere sphere)
    {
      IntPtr pNurbsSurface = UnsafeNativeMethods.ON_Sphere_GetNurbsForm(ref sphere);
      if (IntPtr.Zero == pNurbsSurface)
        return null;
      return new NurbsSurface(pNurbsSurface, null);
    }

    /// <summary>
    /// Constructs a new NURBS surfaces from torus data.
    /// </summary>
    /// <param name="torus">A torus value.</param>
    /// <returns>A new NURBS surface, or null on error.</returns>
    public static NurbsSurface CreateFromTorus(Torus torus)
    {
      IntPtr pNurbsSurface = UnsafeNativeMethods.ON_Torus_GetNurbForm(ref torus);
      if (IntPtr.Zero == pNurbsSurface)
        return null;
      return new NurbsSurface(pNurbsSurface, null);
    }

    /// <summary>
    /// Constructs a ruled surface between two curves. Curves must share the same knot-vector.
    /// </summary>
    /// <param name="curveA">First curve.</param>
    /// <param name="curveB">Second curve.</param>
    /// <returns>A ruled surface on success or null on failure.</returns>
    public static NurbsSurface CreateRuledSurface(Curve curveA, Curve curveB)
    {
      if (curveA == null) { throw new ArgumentNullException("curveA"); }
      if (curveB == null) { throw new ArgumentNullException("curveB"); }

      IntPtr ptr = UnsafeNativeMethods.ON_NurbsSurface_CreateRuledSurface(curveA.ConstPointer(), curveB.ConstPointer());

      if (ptr == IntPtr.Zero)
        return null;
      return new NurbsSurface(ptr, null);
    }

#if RHINO_SDK
    /// <summary>
    /// Constructs a NURBS surface from a 2D grid of control points.
    /// </summary>
    /// <param name="points">Control point locations.</param>
    /// <param name="uCount">Number of points in U direction.</param>
    /// <param name="vCount">Number of points in V direction.</param>
    /// <param name="uDegree">Degree of surface in U direction.</param>
    /// <param name="vDegree">Degree of surface in V direction.</param>
    /// <returns>A NurbsSurface on success or null on failure.</returns>
    /// <remarks>uCount multiplied by vCount must equal the number of points supplied.</remarks>
    public static NurbsSurface CreateFromPoints(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree)
    {
      if (null == points) { throw new ArgumentNullException("points"); }

      int total_count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out total_count);
      if (total_count < 4)
      {
        throw new InvalidOperationException("Insufficient points for a nurbs surface");
      }

      if ((uCount * vCount) != total_count)
      {
        throw new InvalidOperationException("Invalid U and V counts.");
      }

      uDegree = Math.Max(uDegree, 1);
      uDegree = Math.Min(uDegree, 11);
      uDegree = Math.Min(uDegree, uCount - 1);
      vDegree = Math.Max(vDegree, 1);
      vDegree = Math.Min(vDegree, 11);
      vDegree = Math.Min(vDegree, vCount - 1);

      IntPtr ptr = UnsafeNativeMethods.ON_NurbsSurface_SurfaceFromPoints(ptArray, uCount, vCount, uDegree, vDegree);

      if (IntPtr.Zero == ptr) { return null; }
      return new NurbsSurface(ptr, null);
    }

    /// <summary>
    /// Constructs a NURBS surface from a 2D grid of points.
    /// </summary>
    /// <param name="points">Control point locations.</param>
    /// <param name="uCount">Number of points in U direction.</param>
    /// <param name="vCount">Number of points in V direction.</param>
    /// <param name="uDegree">Degree of surface in U direction.</param>
    /// <param name="vDegree">Degree of surface in V direction.</param>
    /// <param name="uClosed">true if the surface should be closed in the U direction.</param>
    /// <param name="vClosed">true if the surface should be closed in the V direction.</param>
    /// <returns>A NurbsSurface on success or null on failure.</returns>
    /// <remarks>uCount multiplied by vCount must equal the number of points supplied.</remarks>
    public static NurbsSurface CreateThroughPoints(IEnumerable<Point3d> points, int uCount, int vCount, int uDegree, int vDegree, bool uClosed, bool vClosed)
    {
      if (null == points) { throw new ArgumentNullException("points"); }

      int total_count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out total_count);
      if (total_count < 4)
      {
        throw new InvalidOperationException("Insufficient points for a nurbs surface");
      }

      if ((uCount * vCount) != total_count)
      {
        throw new InvalidOperationException("Invalid U and V counts.");
      }

      uDegree = Math.Max(uDegree, 1);
      uDegree = Math.Min(uDegree, 11);
      uDegree = Math.Min(uDegree, uCount - 1);
      vDegree = Math.Max(vDegree, 1);
      vDegree = Math.Min(vDegree, 11);
      vDegree = Math.Min(vDegree, vCount - 1);

      IntPtr ptr = UnsafeNativeMethods.ON_NurbsSurface_SurfaceThroughPoints(ptArray, uCount, vCount, uDegree, vDegree, uClosed, vClosed);
      if (IntPtr.Zero == ptr)
        return null;
      return new NurbsSurface(ptr, null);
    }

    /// <summary>
    /// Makes a surface from 4 corner points.
    /// <para>This is the same as calling <see cref="CreateFromCorners(Point3d,Point3d,Point3d,Point3d,double)"/> with tolerance 0.</para>
    /// </summary>
    /// <param name="corner1">The first corner.</param>
    /// <param name="corner2">The second corner.</param>
    /// <param name="corner3">The third corner.</param>
    /// <param name="corner4">The fourth corner.</param>
    /// <returns>the resulting surface or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_srfpt.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_srfpt.cs' lang='cs'/>
    /// <code source='examples\py\ex_srfpt.py' lang='py'/>
    /// </example>
    public static NurbsSurface CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4)
    {
      return CreateFromCorners(corner1, corner2, corner3, corner4, 0.0);
    }
    /// <summary>
    /// Makes a surface from 4 corner points.
    /// </summary>
    /// <param name="corner1">The first corner.</param>
    /// <param name="corner2">The second corner.</param>
    /// <param name="corner3">The third corner.</param>
    /// <param name="corner4">The fourth corner.</param>
    /// <param name="tolerance">Minimum edge length without collapsing to a singularity.</param>
    /// <returns>The resulting surface or null on error.</returns>
    public static NurbsSurface CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4, double tolerance)
    {
      IntPtr pSurface = UnsafeNativeMethods.RHC_RhinoCreateSurfaceFromCorners(corner1, corner2, corner3, corner4, tolerance);
      if (IntPtr.Zero == pSurface)
        return null;
      return new NurbsSurface(pSurface, null);
    }
    /// <summary>
    /// Makes a surface from 3 corner points.
    /// </summary>
    /// <param name="corner1">The first corner.</param>
    /// <param name="corner2">The second corner.</param>
    /// <param name="corner3">The third corner.</param>
    /// <returns>The resulting surface or null on error.</returns>
    public static NurbsSurface CreateFromCorners(Point3d corner1, Point3d corner2, Point3d corner3)
    {
      return CreateFromCorners(corner1, corner2, corner3, corner3, 0.0);
    }

    /// <summary>
    /// Constructs a railed Surface-of-Revolution.
    /// </summary>
    /// <param name="profile">Profile curve for revolution.</param>
    /// <param name="rail">Rail curve for revolution.</param>
    /// <param name="axis">Axis of revolution.</param>
    /// <param name="scaleHeight">If true, surface will be locally scaled.</param>
    /// <returns>A NurbsSurface or null on failure.</returns>
    public static NurbsSurface CreateRailRevolvedSurface(Curve profile, Curve rail, Line axis, bool scaleHeight)
    {
      IntPtr pConstProfile = profile.ConstPointer();
      IntPtr pConstRail = rail.ConstPointer();
      IntPtr pNurbsSurface = UnsafeNativeMethods.RHC_RhinoRailRevolve(pConstProfile, pConstRail, ref axis, scaleHeight);
      if (IntPtr.Zero == pNurbsSurface)
        return null;
      return new NurbsSurface(pNurbsSurface, null);
    }

    /// <summary>
    /// Builds a surface from an ordered network of curves/edges.
    /// </summary>
    /// <param name="uCurves">An array, a list or any enumerable set of U curves.</param>
    /// <param name="uContinuityStart">
    /// continuity at first U segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
    /// </param>
    /// <param name="uContinuityEnd">
    /// continuity at last U segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
    /// </param>
    /// <param name="vCurves">An array, a list or any enumerable set of V curves.</param>
    /// <param name="vContinuityStart">
    /// continuity at first V segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
    /// </param>
    /// <param name="vContinuityEnd">
    /// continuity at last V segment, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.
    /// </param>
    /// <param name="edgeTolerance">tolerance to use along network surface edge.</param>
    /// <param name="interiorTolerance">tolerance to use for the interior curves.</param>
    /// <param name="angleTolerance">angle tolerance to use.</param>
    /// <param name="error">
    /// If the NurbsSurface could not be created, the error value describes where
    /// the failure occured.  0 = success,  1 = curve sorter failed, 2 = network initializing failed,
    /// 3 = failed to build surface, 4 = network surface is not valid.
    /// </param>
    /// <returns>A NurbsSurface or null on failure.</returns>
    public static NurbsSurface CreateNetworkSurface(IEnumerable<Curve> uCurves, int uContinuityStart, int uContinuityEnd,
                                                    IEnumerable<Curve> vCurves, int vContinuityStart, int vContinuityEnd,
                                                    double edgeTolerance, double interiorTolerance, double angleTolerance,
                                                    out int error)
    {
      Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer _uCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer(uCurves);
      Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer _vCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer(vCurves);
      IntPtr pUCurves = _uCurves.NonConstPointer();
      IntPtr pVCurves = _vCurves.NonConstPointer();
      error = 0;
      IntPtr pNS = UnsafeNativeMethods.RHC_RhinoNetworkSurface(pUCurves, uContinuityStart, uContinuityEnd, pVCurves, vContinuityStart, vContinuityEnd, edgeTolerance, interiorTolerance, angleTolerance, ref error);
      _uCurves.Dispose();
      _vCurves.Dispose();
      if (pNS != IntPtr.Zero)
        return new NurbsSurface(pNS, null);
      return null;
    }

    /// <summary>
    /// Builds a surface from an autosorted network of curves/edges.
    /// </summary>
    /// <param name="curves">An array, a list or any enumerable set of curves/edges, sorted automatically into U and V curves.</param>
    /// <param name="continuity">continuity along edges, 0 = loose, 1 = pos, 2 = tan, 3 = curvature.</param>
    /// <param name="edgeTolerance">tolerance to use along network surface edge.</param>
    /// <param name="interiorTolerance">tolerance to use for the interior curves.</param>
    /// <param name="angleTolerance">angle tolerance to use.</param>
    /// <param name="error">
    /// If the NurbsSurface could not be created, the error value describes where
    /// the failure occured.  0 = success,  1 = curve sorter failed, 2 = network initializing failed,
    /// 3 = failed to build surface, 4 = network surface is not valid.
    /// </param>
    /// <returns>A NurbsSurface or null on failure.</returns>
    public static NurbsSurface CreateNetworkSurface(IEnumerable<Curve> curves, int continuity,
                                                    double edgeTolerance, double interiorTolerance, double angleTolerance,
                                                    out int error)
    {
      Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer _curves = new Runtime.InteropWrappers.SimpleArrayCurvePointer(curves);
      IntPtr pCurves = _curves.NonConstPointer();
      error = 0;
      IntPtr pNS = UnsafeNativeMethods.RHC_RhinoNetworkSurface2(pCurves, continuity, edgeTolerance, interiorTolerance, angleTolerance, ref error);
      _curves.Dispose();
      if (pNS != IntPtr.Zero)
        return new NurbsSurface(pNS, null);
      return null;
    }
#endif    
#endregion

    #region constructors
    /// <summary>
    /// Initializes a new NURBS surface by copying the values from another surface.
    /// </summary>
    /// <param name="other">Another surface.</param>
    public NurbsSurface(NurbsSurface other)
    {
      IntPtr pConstOther = other.ConstPointer();
      IntPtr pThis = UnsafeNativeMethods.ON_NurbsSurface_New2(pConstOther);
      ConstructNonConstObject(pThis);
    }

    internal NurbsSurface(IntPtr ptr, object parent)
      : base(ptr, parent)
    { }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected NurbsSurface(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new NurbsSurface(IntPtr.Zero, null);
    }
    #endregion

    #region properties
    private Collections.NurbsSurfaceKnotList m_KnotsU;
    private Collections.NurbsSurfaceKnotList m_KnotsV;
    /// <summary>
    /// The U direction knot vector.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createsurfaceexample.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createsurfaceexample.cs' lang='cs'/>
    /// <code source='examples\py\ex_createsurfaceexample.py' lang='py'/>
    /// </example>
    public Collections.NurbsSurfaceKnotList KnotsU
    {
      get { return m_KnotsU ?? (m_KnotsU = new Rhino.Geometry.Collections.NurbsSurfaceKnotList(this, 0)); }
    }

    /// <summary>
    /// The V direction knot vector.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createsurfaceexample.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createsurfaceexample.cs' lang='cs'/>
    /// <code source='examples\py\ex_createsurfaceexample.py' lang='py'/>
    /// </example>
    public Collections.NurbsSurfaceKnotList KnotsV
    {
      get { return m_KnotsV ?? (m_KnotsV = new Rhino.Geometry.Collections.NurbsSurfaceKnotList(this, 1)); }
    }

    private Collections.NurbsSurfacePointList m_Points;

    /// <summary>
    /// Gets a collection of surface control points that form this surface.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createsurfaceexample.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createsurfaceexample.cs' lang='cs'/>
    /// <code source='examples\py\ex_createsurfaceexample.py' lang='py'/>
    /// </example>
    public Collections.NurbsSurfacePointList Points
    {
      get { return m_Points ?? (m_Points = new Rhino.Geometry.Collections.NurbsSurfacePointList(this)); }
    }

    /// <summary>
    /// Gets a value indicating whether or not the nurbs surface is rational.
    /// </summary>
    public bool IsRational
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_NurbsSurface_GetBool(ptr, idxIsRational);
      }
    }

    /// <summary>
    /// Makes this surface rational.
    /// </summary>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool MakeRational()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_GetBool(pThis, idxMakeRational);
    }

    /// <summary>
    /// Makes this surface non-rational.
    /// </summary>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
    public bool MakeNonRational()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_GetBool(pThis, idxMakeNonRational);
    }

    #endregion

    /// <summary>
    /// Copies this NURBS surface from another NURBS surface.
    /// </summary>
    /// <param name="other">The other NURBS surface to use as source.</param>
    public void CopyFrom(NurbsSurface other)
    {
      IntPtr pConstOther = other.ConstPointer();
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_NurbsSurface_CopyFrom(pConstOther, pThis);
    }
    /*
    public double[] GetGrevilleAbcissae(int direction)
    {
      int count = direction==0?Points.CountU:Points.CountV;
      double[] rc = new double[count];
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_NurbsSurface_GetGrevilleAbcissae(pConstThis, direction, count, rc))
        return new double[0];
      return rc;
    }
     */

    // GetBool indices
    const int idxIsRational = 0;
    internal const int idxIsClampedStart = 1;
    internal const int idxIsClampedEnd = 2;
    //const int idxZeroCVs = 3;
    internal const int idxClampStart = 4;
    internal const int idxClampEnd = 5;
    const int idxMakeRational = 6;
    const int idxMakeNonRational = 7;
    //const int idxHasBezierSpans = 8;

    // GetInt indices
    //const int idxCVSize = 0;
    const int idxOrder = 1;
    internal const int idxCVCount = 2;
    internal const int idxKnotCount = 3;
    //const int idxCVStyle = 4;

    internal int GetIntDir(int which, int direction)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_GetIntDir(ptr, which, direction);
    }

#if RHINO_SDK
    internal override void Draw(Display.DisplayPipeline pipeline, Rhino.Drawing.Color color, int density)
    {
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      IntPtr ptr = ConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.CRhinoDisplayPipeline_DrawNurbsSurface(pDisplayPipeline, ptr, argb, density);
    }
#endif

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsSurface other, double epsilon)
    {
      if (null == other) throw new ArgumentNullException("other");

      if (ReferenceEquals(this, other))
        return true;

      if ((Degree(0) != other.Degree(0)) || (Degree(1) != other.Degree(1)))
        return false;

      if (IsRational != other.IsRational)
        return false;

      if (Points.CountU != other.Points.CountU || Points.CountV != other.Points.CountV)
        return false;

      if (!KnotsU.EpsilonEquals(other.KnotsU, epsilon))
        return false;

      if (!KnotsV.EpsilonEquals(other.KnotsV, epsilon))
        return false;

      return true;
    }

    /// <summary>
    /// Gets the order in the U direction.
    /// </summary>
    public int OrderU
    {
      get { return GetIntDir(idxOrder, 0); }
    }

    /// <summary>
    /// Gets the order in the V direction.
    /// </summary>
    public int OrderV
    {
      get { return GetIntDir(idxOrder, 1); }
    }
  }
  //  public class ON_NurbsCage : ON_Geometry { }
  //  public class ON_MorphControl : ON_Geometry { }

  /// <summary>
  /// Represents a geometry that is able to control the morphing behaviour of some other geometry.
  /// </summary>
  //[Serializable]
  public class MorphControl : GeometryBase
  {
    #region constructors
    //public MorphControl()
    //{
    //  IntPtr pThis = UnsafeNativeMethods.ON_MorphControl_New(IntPtr.Zero);
    //  this.ConstructNonConstObject(pThis);
    //}

    //public MorphControl(MorphControl other)
    //{
    //  IntPtr pConstOther = other.ConstPointer();
    //  IntPtr pThis = UnsafeNativeMethods.ON_MorphControl_New(pConstOther);
    //  this.ConstructNonConstObject(pThis);
    //}

    /// <summary>
    /// Constructs a MorphControl that allows for morphing between two curves.
    /// </summary>
    /// <param name="originCurve">The origin curve for morphing.</param>
    /// <param name="targetCurve">The target curve for morphing.</param>
    public MorphControl(NurbsCurve originCurve, NurbsCurve targetCurve)
    {
      IntPtr pCurve0 = originCurve.ConstPointer();
      IntPtr pCurve1 = targetCurve.ConstPointer();

      IntPtr pThis = UnsafeNativeMethods.ON_MorphControl_New(IntPtr.Zero);
      ConstructNonConstObject(pThis);
      UnsafeNativeMethods.ON_MorphControl_SetCurves(pThis, pCurve0, pCurve1);
    }


    internal MorphControl(IntPtr ptr, object parent)
      : base(ptr, parent, -1)
    { }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected MorphControl(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}
    #endregion

    /// <summary>
    /// The 3d fitting tolerance used when morphing surfaces and breps.
    /// The default is 0.0 and any value &lt;= 0.0 is ignored by morphing functions.
    /// The value returned by Tolerance does not affect the way meshes and points are morphed.
    /// </summary>
    public double SpaceMorphTolerance
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_MorphControl_GetSporhTolerance(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_MorphControl_SetSporhTolerance(pThis, value);
      }
    }

    /// <summary>
    /// true if the morph should be done as quickly as possible because the
    /// result is being used for some type of dynamic preview.  If QuickPreview
    /// is true, the tolerance may be ignored. The QuickPreview value does not
    /// affect the way meshes and points are morphed. The default is false.
    /// </summary>
    public bool QuickPreview
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_MorphControl_GetBool(pConstThis, true);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_MorphControl_SetBool(pThis, value, true);
      }
    }

    /// <summary>
    /// true if the morph should be done in a way that preserves the structure
    /// of the geometry.  In particular, for NURBS objects, true  eans that
    /// only the control points are moved.  The PreserveStructure value does not
    /// affect the way meshes and points are morphed. The default is false.
    /// </summary>
    public bool PreserveStructure
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_MorphControl_GetBool(pConstThis, false);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_MorphControl_SetBool(pThis, value, false);
      }
    }

#if RHINO_SDK
    /// <summary>Applies the space morph to geometry.</summary>
    /// <param name="geometry">The geometry to be morphed.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Morph(GeometryBase geometry)
    {
      // dont' copy a const geometry if we don't have to
      if (null == geometry || !SpaceMorph.IsMorphable(geometry))
        return false;

      IntPtr pConstThis = ConstPointer();
      IntPtr pGeometry = geometry.NonConstPointer();
      return UnsafeNativeMethods.ON_MorphControl_MorphGeometry(pConstThis, pGeometry);
    }
#endif
  }
}


namespace Rhino.Geometry.Collections
{
  /// <summary>
  /// Provides access to the control points of a nurbs surface.
  /// </summary>
  public sealed class NurbsSurfacePointList : IEnumerable<ControlPoint>, IEpsilonComparable<NurbsSurfacePointList>
  {
    private readonly NurbsSurface m_surface;

    #region constructors
    internal NurbsSurfacePointList(NurbsSurface ownerSurface)
    {
      m_surface = ownerSurface;
    }
    #endregion
    /// <summary>
    /// If you want to keep a copy of this class around by holding onto it in a variable after a command
    /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
    /// call this function as many times as you want.
    /// </summary>
    public void EnsurePrivateCopy()
    {
      m_surface.EnsurePrivateCopy();
    }

    #region properties
    /// <summary>
    /// Gets the number of control points in the U direction of this surface.
    /// </summary>
    public int CountU
    {
      get
      {
        return m_surface.GetIntDir(NurbsSurface.idxCVCount, 0);
      }
    }

    /// <summary>
    /// Gets the number of control points in the V direction of this surface.
    /// </summary>
    public int CountV
    {
      get
      {
        return m_surface.GetIntDir(NurbsSurface.idxCVCount, 1);
      }
    }

    #endregion

    /// <summary>
    /// Gets the Greville point (u, v) coordinates associated with the control point at the given indices.
    /// </summary>
    /// <param name="u">Index of control-point along surface U direction.</param>
    /// <param name="v">Index of control-point along surface V direction.</param>
    /// <returns>A Surface UV coordinate on success, Point2d.Unset on failure.</returns>
    public Point2d GetGrevillePoint(int u, int v)
    {
      if (u < 0) { throw new IndexOutOfRangeException("u must be larger than or equal to zero."); }
      if (v < 0) { throw new IndexOutOfRangeException("v must be larger than or equal to zero."); }
      if (u >= CountU) { throw new IndexOutOfRangeException("u must be less than CountU."); }
      if (v >= CountV) { throw new IndexOutOfRangeException("v must be less than CountV."); }

      IntPtr ptr = m_surface.ConstPointer();
      Point2d gp = Point2d.Unset;

      UnsafeNativeMethods.ON_NurbsSurface_GetGrevillePoint(ptr, u, v, ref gp);
      return gp;
    }

    /// <summary>
    /// Gets the control point at the given (u, v) index.
    /// </summary>
    /// <param name="u">Index of control-point along surface U direction.</param>
    /// <param name="v">Index of control-point along surface V direction.</param>
    /// <returns>The control point at the given (u, v) index.</returns>
    public ControlPoint GetControlPoint(int u, int v)
    {
      if (u < 0) { throw new IndexOutOfRangeException("u must be larger than or equal to zero."); }
      if (v < 0) { throw new IndexOutOfRangeException("v must be larger than or equal to zero."); }
      if (u >= CountU) { throw new IndexOutOfRangeException("u must be less than CountU."); }
      if (v >= CountV) { throw new IndexOutOfRangeException("v must be less than CountV."); }

      Point4d pt = new Point4d();
      IntPtr ptr = m_surface.ConstPointer();
      if (UnsafeNativeMethods.ON_NurbsSurface_GetCV(ptr, u, v, ref pt))
        return new ControlPoint(pt);

      return ControlPoint.Unset;
    }

    /// <summary>
    /// Sets the control point at the given (u, v) index.
    /// </summary>
    /// <param name="u">Index of control-point along surface U direction.</param>
    /// <param name="v">Index of control-point along surface V direction.</param>
    /// <param name="cp">The control point location to set (weight is assumed to be 1.0).</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_createsurfaceexample.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createsurfaceexample.cs' lang='cs'/>
    /// <code source='examples\py\ex_createsurfaceexample.py' lang='py'/>
    /// </example>
    public bool SetControlPoint(int u, int v, Point3d cp)
    {
      return SetControlPoint(u, v, new ControlPoint(cp));
    }

    /// <summary>
    /// Sets the control point at the given (u, v) index.
    /// </summary>
    /// <param name="u">Index of control-point along surface U direction.</param>
    /// <param name="v">Index of control-point along surface V direction.</param>
    /// <param name="cp">The control point to set.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetControlPoint(int u, int v, ControlPoint cp)
    {
      if (u < 0) { throw new IndexOutOfRangeException("u must be larger than or equal to zero."); }
      if (v < 0) { throw new IndexOutOfRangeException("v must be larger than or equal to zero."); }
      if (u >= CountU) { throw new IndexOutOfRangeException("u must be less than CountU."); }
      if (v >= CountV) { throw new IndexOutOfRangeException("v must be less than CountV."); }

      IntPtr ptr = m_surface.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_SetCV(ptr, u, v, ref cp.m_vertex);
    }

    #region IEnumerable<Point3d> Members
    IEnumerator<ControlPoint> IEnumerable<ControlPoint>.GetEnumerator()
    {
      return new NurbsSrfEnum(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new NurbsSrfEnum(this);
    }

    private class NurbsSrfEnum : IEnumerator<ControlPoint>
    {
      #region members
      readonly NurbsSurfacePointList m_surface_cv;
      readonly int m_count_u = -1;
      readonly int m_count_v = -1;
      bool m_disposed; // = false; <- initialized by runtime
      int position = -1;
      #endregion

      #region constructor
      public NurbsSrfEnum(NurbsSurfacePointList surface_cv)
      {
        m_surface_cv = surface_cv;
        m_count_u = surface_cv.CountU;
        m_count_v = surface_cv.CountV;
      }
      #endregion

      #region enumeration logic
      int Count
      {
        get { return m_count_u*m_count_v; }
      }

      public bool MoveNext()
      {
        position++;
        return (position < Count);
      }
      public void Reset()
      {
        position = -1;
      }

      public ControlPoint Current
      {
        get
        {
          try
          {
            int u = position / m_count_v;
            int v = position % m_count_v;
            return m_surface_cv.GetControlPoint(u, v);
          }
          catch (IndexOutOfRangeException)
          {
            throw new InvalidOperationException();
          }
        }
      }
      object IEnumerator.Current
      {
        get
        {
          try
          {
            int u = position / m_count_v;
            int v = position % m_count_v;
            return m_surface_cv.GetControlPoint(u, v);
          }
          catch (IndexOutOfRangeException)
          {
            throw new InvalidOperationException();
          }
        }
      }
      #endregion

      #region IDisposable logic
      public void Dispose()
      {
        if (m_disposed)
          return;
        m_disposed = true;
        GC.SuppressFinalize(this);
      }
      #endregion
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsSurfacePointList other, double epsilon)
    {
      if (null == other) throw new ArgumentNullException("other");

      if (ReferenceEquals(this, other))
        return true;

      if (CountU != other.CountU)
        return false;

      if (CountV != other.CountV)
        return false;


      for (int u = 0; u < CountU; ++u)
      {
        for (int v = 0; v < CountV; ++v)
        {
          ControlPoint mine = GetControlPoint(u, v);
          ControlPoint theirs = other.GetControlPoint(u, v);

          if (!mine.EpsilonEquals(theirs, epsilon))
            return false;
        }
      }

      return true;
    }
  }
  /// <summary>
  /// Provides access to the knot vector of a nurbs surface.
  /// </summary>
  public sealed class NurbsSurfaceKnotList : IEnumerable<double>, Rhino.Collections.IRhinoTable<double>, IEpsilonComparable<NurbsSurfaceKnotList>
  {
    private readonly NurbsSurface m_surface;
    private readonly int m_direction;

    #region constructors
    internal NurbsSurfaceKnotList(NurbsSurface ownerSurface, int direction)
    {
      m_surface = ownerSurface;
      m_direction = direction;
    }
    #endregion

    #region properties

    /// <summary>Gets the total number of knots in this curve.</summary>
    public int Count
    {
      get
      {
        return m_surface.GetIntDir(NurbsSurface.idxKnotCount, m_direction);
      }
    }

    /// <summary>Determines if a knot vector is clamped.</summary>
    public bool ClampedAtStart
    {
      get
      {
        IntPtr pConstSurf = m_surface.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsSurface_GetBoolDir(pConstSurf, NurbsSurface.idxIsClampedStart, m_direction);
      }
    }
    /// <summary>Determines if a knot vector is clamped.</summary>
    public bool ClampedAtEnd
    {
      get
      {
        IntPtr pConstSurf = m_surface.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsSurface_GetBoolDir(pConstSurf, NurbsSurface.idxIsClampedEnd, m_direction);
      }
    }

    /// <summary>
    /// Computes the knots that are superfluous because they are not used in NURBs evaluation.
    /// These make it appear so that the first and last surface spans are different from interior spans.
    /// <para>http://wiki.mcneel.com/developer/onsuperfluousknot</para>
    /// </summary>
    /// <param name="start">true if the query targets the first knot. Otherwise, the last knot.</param>
    /// <returns>A component.</returns>
    public double SuperfluousKnot(bool start)
    {
      IntPtr pConstSurf = m_surface.ConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_SuperfluousKnot(pConstSurf, m_direction, start ? 0 : 1);
    }

    /// <summary>
    /// Gets or sets the knot vector value at the given index.
    /// </summary>
    /// <param name="index">Index of knot to access.</param>
    /// <returns>The knot value at [index]</returns>
    public double this[int index]
    {
      get
      {
        if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero."); }
        if (index >= Count) { throw new IndexOutOfRangeException("Index must be less than the number of knots."); }
        IntPtr ptr = m_surface.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsSurface_Knot(ptr, m_direction, index);
      }
      set
      {
        if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero."); }
        if (index >= Count) { throw new IndexOutOfRangeException("Index must be less than the number of knots."); }
        IntPtr ptr = m_surface.NonConstPointer();
        UnsafeNativeMethods.ON_NurbsSurface_SetKnot(ptr, m_direction, index, value);
      }
    }
    #endregion

    #region knot utility methods
    /// <summary>
    /// If you want to keep a copy of this class around by holding onto it in a variable after a command
    /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
    /// call this function as many times as you want.
    /// </summary>
    public void EnsurePrivateCopy()
    {
      m_surface.EnsurePrivateCopy();
    }

    /// <summary>
    /// Inserts a knot and update control point locations.
    /// Does not change parameterization or locus of curve.
    /// </summary>
    /// <param name="value">Knot value to insert.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool InsertKnot(double value)
    {
      return InsertKnot(value, 1);
    }

    /// <summary>
    /// Inserts a knot and update control point locations.
    /// Does not change parameterization or locus of curve.
    /// </summary>
    /// <param name="value">Knot value to insert.</param>
    /// <param name="multiplicity">Multiplicity of knot to insert.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool InsertKnot(double value, int multiplicity)
    {
      IntPtr ptr = m_surface.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_InsertKnot(ptr, m_direction, value, multiplicity);
    }

    /// <summary>Get knot multiplicity.</summary>
    /// <param name="index">Index of knot to query.</param>
    /// <returns>The multiplicity (valence) of the knot.</returns>
    public int KnotMultiplicity(int index)
    {
      IntPtr ptr = m_surface.ConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_KnotMultiplicity(ptr, m_direction, index);
    }

    /// <summary>
    /// Compute a clamped, uniform knot vector based on the current
    /// degree and control point count. Does not change values of control
    /// vertices.
    /// </summary>
    /// <param name="knotSpacing">Spacing of subsequent knots.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool CreateUniformKnots(double knotSpacing)
    {
      IntPtr ptr = m_surface.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_MakeUniformKnotVector(ptr, m_direction, knotSpacing, true);
    }

    /// <summary>
    /// Compute a clamped, uniform, periodic knot vector based on the current
    /// degree and control point count. Does not change values of control
    /// vertices.
    /// </summary>
    /// <param name="knotSpacing">Spacing of subsequent knots.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool CreatePeriodicKnots(double knotSpacing)
    {
      IntPtr ptr = m_surface.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsSurface_MakeUniformKnotVector(ptr, m_direction, knotSpacing, false);
    }
    #endregion

    #region IEnumerable<double> Members
    IEnumerator<double> IEnumerable<double>.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsSurfaceKnotList, double>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsSurfaceKnotList, double>(this);
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsSurfaceKnotList other, double epsilon)
    {
        if (null == other) throw new ArgumentNullException("other");

        if (ReferenceEquals(this, other))
            return true;

        if (m_direction != other.m_direction)
            return false;

        if (Count != other.Count)
            return false;

        // check for equality of spans
        for (int i = 1; i < Count; ++i)
        {
            double myDelta = this[i] - this[i - 1];
            double theirDelta = other[i] - other[i - 1];
            if (!RhinoMath.EpsilonEquals(myDelta, theirDelta, epsilon))
                return false;
        }

        return true;
    }
  }
}