using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino.Display;
using Rhino.Collections;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a Non Uniform Rational B-Splines (NURBS) curve.
  /// </summary>
  [Serializable]
  public class NurbsCurve : Curve, IEpsilonComparable<NurbsCurve>
  {
    #region statics
    /// <summary>
    /// Gets a non-rational, degree 1 Nurbs curve representation of the line.
    /// </summary>
    /// <returns>Curve on success, null on failure.</returns>
    public static NurbsCurve CreateFromLine(Line line)
    {
      if (!line.IsValid) { return null; }

      NurbsCurve crv = new NurbsCurve(3, false, 2, 2);
      // Using implicit operator on Point3d
      crv.Points[0] = line.From;// new ControlPoint(line.From);
      crv.Points[1] = line.To;// new ControlPoint(line.To);

      crv.Knots.CreateUniformKnots(1.0);
      return crv;
    }

    /// <summary>
    /// Gets a rational degree 2 NURBS curve representation
    /// of the arc. Note that the parameterization of NURBS curve
    /// does not match arc's transcendental paramaterization.
    /// </summary>
    /// <returns>Curve on success, null on failure.</returns>
    public static NurbsCurve CreateFromArc(Arc arc)
    {
      IntPtr pNC = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      int success = UnsafeNativeMethods.ON_Arc_GetNurbForm(ref arc, pNC);
      if (0 == success)
      {
        UnsafeNativeMethods.ON_Object_Delete(pNC);
        return null;
      }
      return GeometryBase.CreateGeometryHelper(pNC, null) as NurbsCurve;
    }

    /// <summary>
    /// Gets a rational degree 2 NURBS curve representation
    /// of the circle. Note that the parameterization of NURBS curve
    /// does not match circle's transcendental paramaterization.  
    /// Use GetRadianFromNurbFormParameter() and
    /// GetParameterFromRadian() to convert between the NURBS curve 
    /// parameter and the transcendental parameter.
    /// </summary>
    /// <returns>Curve on success, null on failure.</returns>
    public static NurbsCurve CreateFromCircle(Circle circle)
    {
      IntPtr pNC = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      int success = UnsafeNativeMethods.ON_Circle_GetNurbForm(ref circle, pNC);
      if (0 == success)
      {
        UnsafeNativeMethods.ON_Object_Delete(pNC);
        return null;
      }
      return GeometryBase.CreateGeometryHelper(pNC, null) as NurbsCurve;
    }

    /// <summary>
    /// Gets a rational degree 2 NURBS curve representation of the ellipse.
    /// <para>Note that the parameterization of the NURBS curve does not match
    /// with the transcendental paramaterization of the ellipsis.</para>
    /// </summary>
    /// <returns>A nurbs curve representation of this ellipse or null if no such representation could be made.</returns>
    public static NurbsCurve CreateFromEllipse(Ellipse ellipse)
    {
      NurbsCurve nc = CreateFromCircle(new Circle(ellipse.Plane, 1.0));
      if (nc == null) { return null; }

      Transform scale = Rhino.Geometry.Transform.Scale(ellipse.Plane, ellipse.Radius1, ellipse.Radius2, 1.0);
      nc.Transform(scale);

      return nc;

      // Ellipses use Plane which is not castable to ON_Plane. Also, the OpenNurbs ON_Ellipse->ON_NurbsCurve logic fails on 
      // zero radii. Making a unit circle and scaling seems to be a good intermediate solution to this problem.


      //IntPtr pNC = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      //Plane plane = ellipse.Plane;
      //int success = UnsafeNativeMethods.ON_Ellipse_GetNurbForm2(ref plane, ellipse.Radius1, ellipse.Radius2, pNC);
      //if (0 == success)
      //{
      //  UnsafeNativeMethods.ON_Object_Delete(pNC);
      //  return null;
      //}
      //return new NurbsCurve(pNC, null, null);
    }

    /// <summary>
    /// Determines if two curves are similar.
    /// </summary>
    /// <param name="curveA">First curve used in comparison.</param>
    /// <param name="curveB">Second curve used in comparison.</param>
    /// <param name="ignoreParameterization">if true, parameterization and orientaion are ignored.</param>
    /// <param name="tolerance">tolerance to use when comparing control points.</param>
    /// <returns>true if curves are similar within tolerance.</returns>
    public static bool IsDuplicate(NurbsCurve curveA, NurbsCurve curveB, bool ignoreParameterization, double tolerance)
    {
      IntPtr ptrA = curveA.ConstPointer();
      IntPtr ptrB = curveB.ConstPointer();
      if (ptrA == ptrB)
        return true;

      return UnsafeNativeMethods.ON_NurbsCurve_IsDuplicate(ptrA, ptrB, ignoreParameterization, tolerance);
    }

    /// <summary>
    /// Constructs a 3D NURBS curve from a list of control points.
    /// </summary>
    /// <param name="periodic">If true, create a periodic uniform curve. If false, create a clamped uniform curve.</param>
    /// <param name="degree">(>=1) degree=order-1.</param>
    /// <param name="points">control vertex locations.</param>
    /// <returns>
    /// new NURBS curve on success
    /// null on error.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscurve.py' lang='py'/>
    /// </example>
    public static NurbsCurve Create(bool periodic, int degree, System.Collections.Generic.IEnumerable<Point3d> points)
    {
      if (degree < 1)
        return null;

      const int dimension = 3;
      const double knotDelta = 1.0;
      int count;
      int order = degree + 1;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (null == ptArray || count < 2)
        return null;

      NurbsCurve nc = new NurbsCurve();
      IntPtr pCurve = nc.NonConstPointer();

      bool rc = periodic ? UnsafeNativeMethods.ON_NurbsCurve_CreatePeriodicUniformNurbs(pCurve, dimension, order, count, ptArray, knotDelta) :
                           UnsafeNativeMethods.ON_NurbsCurve_CreateClampedUniformNurbs(pCurve, dimension, order, count, ptArray, knotDelta);

      if (false == rc)
      {
        nc.Dispose();
        return null;
      }
      return nc;
    }

#if RHINO_SDK
    /// <summary>
    /// Creates a C1 cubic NURBS approximation of a helix or spiral. For a helix,
    /// you may have radius0 == radius1. For a spiral radius0 == radius0 produces
    /// a circle. Zero and negative radii are permissible.
    /// </summary>
    /// <param name="axisStart">Helix's axis starting point or center of spiral.</param>
    /// <param name="axisDir">Helix's axis vector or normal to spiral's plane.</param>
    /// <param name="radiusPoint">
    /// Point used only to get a vector that is perpedicular to the axis. In
    /// particular, this vector must not be (anti)parallel to the axis vector.
    /// </param>
    /// <param name="pitch">
    /// The pitch, where a spiral has a pitch = 0, and pitch > 0 is the distance
    /// between the helix's "threads".
    /// </param>
    /// <param name="turnCount">The number of turns in spiral or helix. Positive
    /// values produce counter-clockwise orientation, negitive values produce
    /// clockwise orientation. Note, for a helix, turnCount * pitch = length of
    /// the helix's axis.
    /// </param>
    /// <param name="radius0">The starting radius.</param>
    /// <param name="radius1">The ending radius.</param>
    /// <returns>NurbsCurve on success, null on failure.</returns>
    public static NurbsCurve CreateSpiral(Point3d axisStart, Vector3d axisDir, Point3d radiusPoint,
      double pitch, double turnCount, double radius0, double radius1)
    {
      NurbsCurve curve = new NurbsCurve();
      IntPtr pCurve = curve.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoCreateSpiral0(axisStart, axisDir, radiusPoint, pitch, turnCount, radius0, radius1, pCurve);
      if (!rc)
      {
        curve.Dispose();
        return null;
      }
      return curve;
    }

    /// <summary>
    /// Create a C2 non-rational uniform cubic NURBS approximation of a swept helix or spiral.
    /// </summary>
    /// <param name="railCurve">The rail curve.</param>
    /// <param name="t0">Starting portion of rail curve's domain to sweep along.</param>
    /// <param name="t1">Ending portion of rail curve's domain to sweep along.</param>
    /// <param name="radiusPoint">
    /// Point used only to get a vector that is perpedicular to the axis. In
    /// particular, this vector must not be (anti)parallel to the axis vector.
    /// </param>
    /// <param name="pitch">
    /// The pitch. Positive values produce counter-clockwise orientation,
    /// negative values produce clockwise orientation.
    /// </param>
    /// <param name="turnCount">
    /// The turn count. If != 0, then the resulting helix will have this many
    /// turns. If = 0, then pitch must be != 0 and the approximate distance
    /// between turns will be set to pitch. Positive values produce counter-clockwise
    /// orientation, negitive values produce clockwise orientation.
    /// </param>
    /// <param name="radius0">
    /// The starting radius. At least one radii must benonzero. Negative values
    /// are allowed.
    /// </param>
    /// <param name="radius1">
    /// The ending radius. At least ont radii must be nonzero. Negative values
    /// are allowed.
    /// </param>
    /// <param name="pointsPerTurn">
    /// Number of points to intepolate per turn. Must be greater than 4.
    /// When in doubt, use 12.
    /// </param>
    /// <returns>NurbsCurve on success, null on failure.</returns>
    public static NurbsCurve CreateSpiral(Curve railCurve, double t0, double t1, Point3d radiusPoint, double pitch,
      double turnCount, double radius0, double radius1, int pointsPerTurn)
    {
      IntPtr pRail = railCurve.ConstPointer();
      NurbsCurve curve = new NurbsCurve();
      IntPtr pCurve = curve.NonConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhinoCreateSpiral1(pRail, t0, t1, radiusPoint, pitch, turnCount, radius0, radius1, pointsPerTurn, pCurve);
      if (!rc)
      {
        curve.Dispose();
        return null;
      }
      return curve;
    }
#endif

    #endregion

    #region constructors
    /// <summary>
    /// Initializes a NURBS curve by copying its values from another NURBS curve.
    /// </summary>
    /// <param name="other">The other curve. This value can be null.</param>
    public NurbsCurve(NurbsCurve other)
    {
      IntPtr pOther = IntPtr.Zero;
      if (other != null)
        pOther = other.ConstPointer();
      IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(pOther);
      ConstructNonConstObject(ptr);
    }
    internal NurbsCurve()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected NurbsCurve(SerializationInfo info, StreamingContext context)
      :base(info, context)
    {
    }

    //[skipping]
    // public ON_NurbsCurve(ON_BezierCurve)

    /// <summary>
    /// Constructs a new Nurbscurve with a specific degree and control-point count.
    /// </summary>
    /// <param name="degree">Degree of curve. Must be equal to or larger than 1 and smaller than or equal to 11.</param>
    /// <param name="pointCount">Number of control-points.</param>
    public NurbsCurve(int degree, int pointCount)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
      Create(3, false, degree + 1, pointCount);
    }

    /// <summary>
    /// Constructs a new NurbsCurve with knot and CV memory allocated.
    /// </summary>
    /// <param name="dimension">&gt;=1.</param>
    /// <param name="rational">true to make a rational NURBS.</param>
    /// <param name="order">(&gt;= 2) The order=degree+1.</param>
    /// <param name="pointCount">(&gt;= order) number of control vertices.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
    /// </example>
    public NurbsCurve(int dimension, bool rational, int order, int pointCount)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_NurbsCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
      Create(dimension, rational, order, pointCount);
    }
    private bool Create(int dimension, bool isRational, int order, int cvCount)
    {
      // keeping internal for now
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_Create(ptr, dimension, isRational, order, cvCount);
    }

    internal NurbsCurve(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new NurbsCurve(IntPtr.Zero, null, -1);
    }

    #endregion

    #region properties
    private Collections.NurbsCurveKnotList m_knots;
    private Collections.NurbsCurvePointList m_points;

    /// <summary>
    /// Gets the order of the curve. Order = Degree + 1.
    /// </summary>
    public int Order
    {
      get { return GetInt(idxOrder); }
    }

    /// <summary>
    /// Gets a value indicating whether or not the curve is rational. 
    /// Rational curves have control-points with custom weights.
    /// </summary>
    public bool IsRational
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, idxIsRational);
      }
    }

    /// <summary>
    /// Gets access to the knots (or "knot vector") of this nurbs curve.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
    /// </example>
    public Collections.NurbsCurveKnotList Knots
    {
      get { return m_knots ?? (m_knots = new Rhino.Geometry.Collections.NurbsCurveKnotList(this)); }
    }

    /// <summary>
    /// Gets access to the control points of this nurbs curve.
    /// </summary>
    public Collections.NurbsCurvePointList Points
    {
      get { return m_points ?? (m_points = new Rhino.Geometry.Collections.NurbsCurvePointList(this)); }
    }
    #endregion

    #region constants
    // GetBool indices
    internal const int idxIsRational = 0;
    internal const int idxIsClampedStart = 1;
    internal const int idxIsClampedEnd = 2;
    internal const int idxZeroCVs = 3;
    internal const int idxClampStart = 4;
    internal const int idxClampEnd = 5;
    internal const int idxMakeRational = 6;
    internal const int idxMakeNonRational = 7;
    internal const int idxHasBezierSpans = 8;

    // GetInt indices
    internal const int idxCVSize = 0;
    internal const int idxOrder = 1;
    internal const int idxCVCount = 2;
    internal const int idxKnotCount = 3;
    internal const int idxCVStyle = 4;
    internal int GetInt(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GetInt(ptr, which);
    }
    #endregion

#if RHINO_SDK
    private IntPtr CurveDisplay()
    {
      if (IntPtr.Zero == m_pCurveDisplay)
      {
        IntPtr pThis = ConstPointer();
        m_pCurveDisplay = UnsafeNativeMethods.CurveDisplay_FromNurbsCurve(pThis);
      }
      return m_pCurveDisplay;
    }

    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      IntPtr pCurveDisplay = CurveDisplay();
      UnsafeNativeMethods.CurveDisplay_Draw(pCurveDisplay, pDisplayPipeline, argb, thickness);
    }
#endif

    /// <summary>
    /// Increase the degree of this curve.
    /// </summary>
    /// <param name="desiredDegree">The desired degree. 
    /// Degrees should be number between and including 1 and 11.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool IncreaseDegree(int desiredDegree)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_IncreaseDegree(ptr, desiredDegree);
    }

    /// <summary>
    /// Returns true if the NURBS curve has bezier spans (all distinct knots have multiplitity = degree)
    /// </summary>
    public bool HasBezierSpans
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, idxHasBezierSpans);
      }
    }

    /// <summary>
    /// Clamps ends and adds knots so the NURBS curve has bezier spans 
    /// (all distinct knots have multiplitity = degree).
    /// </summary>
    /// <param name="setEndWeightsToOne">
    /// If true and the first or last weight is not one, then the first and
    /// last spans are reparameterized so that the end weights are one.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool MakePiecewiseBezier(bool setEndWeightsToOne)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_MakePiecewiseBezier(ptr, setEndWeightsToOne);
    }

    /// <summary>
    /// Use a linear fractional transformation to reparameterize the NURBS curve.
    /// This does not change the curve's domain.
    /// </summary>
    /// <param name="c">
    /// reparameterization constant (generally speaking, c should be > 0). The
    /// control points and knots are adjusted so that
    /// output_nurbs(t) = input_nurbs(lambda(t)), where lambda(t) = c*t/( (c-1)*t + 1 ).
    /// Note that lambda(0) = 0, lambda(1) = 1, lambda'(t) > 0, 
    /// lambda'(0) = c and lambda'(1) = 1/c.
    /// </param>
    /// <returns>true if successful.</returns>
    /// <remarks>
    /// The cv and knot values are values are changed so that output_nurbs(t) = input_nurbs(lambda(t)).
    /// </remarks>
    public bool Reparameterize(double c)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_Reparameterize(ptr, c);
    }

    #region greville point methods
    /// <summary>
    /// Gets the greville (edit point) parameter that belongs 
    /// to the control point at the specified index.
    /// </summary>
    /// <param name="index">Index of Greville (Edit) point.</param>
    public double GrevilleParameter(int index)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GrevilleAbcissa(ptr, index);
    }

    /// <summary>
    /// Gets the greville (edit point) parameter that belongs 
    /// to the control point at the specified index.
    /// </summary>
    /// <param name="index">Index of Greville (Edit) point.</param>
    public Point3d GrevillePoint(int index)
    {
      double t = GrevilleParameter(index);
      return PointAt(t);
    }

    /// <summary>
    /// Gets all Greville (Edit point) parameters for this curve.
    /// </summary>
    public double[] GrevilleParameters()
    {
      int count = Points.Count;
      double[] rc = new double[count];
      IntPtr ptr = ConstPointer();
      bool success = UnsafeNativeMethods.ON_NurbsCurve_GetGrevilleAbcissae(ptr, rc);
      if (!success) { return null; }
      return rc;
    }

    /// <summary>
    /// Gets all Greville (Edit) points for this curve.
    /// </summary>
    public Point3dList GrevillePoints()
    {
      double[] gr_ab = GrevilleParameters();
      if (gr_ab == null) { return null; }

      Point3dList gr_pts = new Point3dList(gr_ab.Length);

      foreach (double t in gr_ab)
      {
        gr_pts.Add(PointAt(t));
      }

      return gr_pts;
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsCurve other, double epsilon)
    {
      if (null == other) throw new ArgumentNullException("other");

      if (ReferenceEquals(this, other))
        return true;

      if (IsRational != other.IsRational)
        return false;

      if (Degree != other.Degree)
        return false;

      if (Points.Count != other.Points.Count)
        return false;

      if (!Knots.EpsilonEquals(other.Knots, epsilon))
        return false;

      if (!Points.EpsilonEquals(other.Points, epsilon))
        return false;

      return true;
    }
    //[skipping]
    //   bool RepairBadKnots(
    //[skipping - slightly unusual for plugin devs]
    //public bool ChangeDimension(int desiredDimension)
    //{
    //  IntPtr ptr = NonConstPointer();
    //  return UnsafeNativeMethods.ON_NurbsCurve_ChangeDimension(ptr, desiredDimension);
    //}

    //[skipping - slightly unusual for plugin devs]
    //public bool Append(NurbsCurve curve)
    //{
    //  if (null == curve)
    //    return false;
    //  IntPtr ptr = NonConstPointer();
    //  IntPtr pCurve = curve.ConstPointer();
    //  return UnsafeNativeMethods.ON_NurbsCurve_Append(ptr, pCurve);
    //}

    //[skipping]
    //  bool ReserveCVCapacity(
    //  bool ReserveKnotCapacity(

    //[skipping]
    //  bool ConvertSpanToBezier(
  }

  /// <summary>
  /// Represents control-point geometry with three-dimensional position and weight.
  /// </summary>
  [Serializable]
  public struct ControlPoint : IEpsilonComparable<ControlPoint>
  {
    #region members
    internal Point4d m_vertex;
    #endregion

    #region constructors
    /// <summary>
    /// Constructs a new unweighted control point.
    /// </summary>
    /// <param name="x">X coordinate of Control Point.</param>
    /// <param name="y">Y coordinate of Control Point.</param>
    /// <param name="z">Z coordinate of Control Point.</param>
    public ControlPoint(double x, double y, double z)
    {
      m_vertex = new Point4d(x, y, z, 1.0);
    }
    /// <summary>
    /// Constructs a new weighted control point.
    /// </summary>
    /// <param name="x">X coordinate of Control Point.</param>
    /// <param name="y">Y coordinate of Control Point.</param>
    /// <param name="z">Z coordinate of Control Point.</param>
    /// <param name="weight">Weight factor of Control Point. 
    /// You should not use weights equal to or less than zero.</param>
    public ControlPoint(double x, double y, double z, double weight)
    {
      m_vertex = new Point4d(x, y, z, weight);
    }
    /// <summary>
    /// Constructs a new unweighted control point.
    /// </summary>
    /// <param name="pt">Coordinate of Control Point.</param>
    public ControlPoint(Point3d pt)
    {
      m_vertex = new Point4d(pt.X, pt.Y, pt.Z, 1.0);
    }
    /// <summary>
    /// Constructs a new weighted control point.
    /// </summary>
    /// <param name="pt">Coordinate of Control Point.</param>
    /// <param name="weight">Weight factor of Control Point. 
    /// You should not use weights equal to or less than zero.</param>
    public ControlPoint(Point3d pt, double weight)
    {
      m_vertex = new Point4d(pt.X, pt.Y, pt.Z, weight);
    }
    /// <summary>
    /// Constructs a new weighted control point.
    /// </summary>
    /// <param name="pt">Control point values.</param>
    public ControlPoint(Point4d pt)
    {
      m_vertex = pt;
    }

    /// <summary>
    /// Gets the predefined unset control point.
    /// </summary>
    public static ControlPoint Unset
    {
      get
      {
        ControlPoint rc = new ControlPoint();
        Point3d unset = Point3d.Unset;
        rc.m_vertex.m_x = unset.m_x;
        rc.m_vertex.m_y = unset.m_y;
        rc.m_vertex.m_z = unset.m_z;
        rc.m_vertex.m_w = 1.0;
        return rc;
      }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the location of the control point. 
    /// Internally, Rhino stores the location of a weighted control-point 
    /// as a pre-multiplied coordinate, but RhinoCommon always provides 
    /// Euclidean coordinates for control-points, regardless of weight.
    /// </summary>
    public Point3d Location
    {
      get
      {
        return new Point3d(m_vertex.m_x, m_vertex.m_y, m_vertex.m_z);
      }
      set
      {
        m_vertex.m_x = value.m_x;
        m_vertex.m_y = value.m_y;
        m_vertex.m_z = value.m_z;
      }
    }

    /// <summary>
    /// Gets or sets the weight of this control point.
    /// </summary>
    public double Weight
    {
      get
      {
        return m_vertex.m_w;
      }
      set
      {
        m_vertex.m_w = value;
      }
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(ControlPoint other, double epsilon)
    {
      return m_vertex.EpsilonEquals(other.m_vertex, epsilon);
    }
  }
}

namespace Rhino.Geometry.Collections
{
  /// <summary>
  /// Provides access to the knot vector of a nurbs curve.
  /// </summary>
  public sealed class NurbsCurveKnotList : IEnumerable<double>, Rhino.Collections.IRhinoTable<double>, IEpsilonComparable<NurbsCurveKnotList>
  {
    private readonly NurbsCurve m_curve;

    #region constructors
    internal NurbsCurveKnotList(NurbsCurve ownerCurve)
    {
      m_curve = ownerCurve;
    }
    #endregion

    #region properties

    /// <summary>Total number of knots in this curve.</summary>
    public int Count
    {
      get
      {
        return m_curve.GetInt(NurbsCurve.idxKnotCount);
      }
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
        IntPtr ptr = m_curve.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_Knot(ptr, index);
      }
      set
      {
        if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero."); }
        if (index >= Count) { throw new IndexOutOfRangeException("Index must be less than the number of knots."); }
        IntPtr ptr = m_curve.NonConstPointer();
        UnsafeNativeMethods.ON_NurbsCurve_SetKnot(ptr, index, value);
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
      m_curve.EnsurePrivateCopy();
    }

    /// <summary>
    /// Inserts a knot and update control point locations.
    /// Does not change parameterization or locus of curve.
    /// </summary>
    /// <param name="value">Knot value to insert.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_insertknot.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_insertknot.cs' lang='cs'/>
    /// <code source='examples\py\ex_insertknot.py' lang='py'/>
    /// </example>
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
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_InsertKnot(ptr, value, multiplicity);
    }

    /// <summary>Get knot multiplicity.</summary>
    /// <param name="index">Index of knot to query.</param>
    /// <returns>The multiplicity (valence) of the knot.</returns>
    public int KnotMultiplicity(int index)
    {
      IntPtr ptr = m_curve.ConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_KnotMultiplicity(ptr, index);
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
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_MakeUniformKnotVector(ptr, knotSpacing, true);
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
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_MakeUniformKnotVector(ptr, knotSpacing, false);
    }

    /// <summary>
    /// Gets a value indicating whether or not the knot vector is clamped at the start of the curve. 
    /// Clamped curves start at the first control-point. This requires fully multiple knots.
    /// </summary>
    public bool IsClampedStart
    {
      get
      {
        IntPtr ptr = m_curve.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxIsClampedStart);
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not the knot vector is clamped at the end of the curve. 
    /// Clamped curves are coincident with the first and last control-point. This requires fully multiple knots.
    /// </summary>
    public bool IsClampedEnd
    {
      get
      {
        IntPtr ptr = m_curve.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxIsClampedEnd);
      }
    }

    /// <summary>
    /// Clamp end knots. Does not modify control point locations.
    /// </summary>
    /// <param name="end">Curve end to clamp.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool ClampEnd(CurveEnd end)
    {
      IntPtr ptr = m_curve.NonConstPointer();
      bool rc = true;
      if (CurveEnd.Start == end || CurveEnd.Both == end)
        rc = UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxClampStart);
      if (CurveEnd.End == end || CurveEnd.Both == end)
        rc = rc && UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxClampEnd);
      return rc;
    }

    /// <summary>
    /// Computes the knots that are superfluous because they are not used in NURBs evaluation.
    /// These make it appear so that the first and last curve spans are different from interior spans.
    /// <para>http://wiki.mcneel.com/developer/onsuperfluousknot</para>
    /// </summary>
    /// <param name="start">true if the query targets the first knot. Otherwise, the last knot.</param>
    /// <returns>A component.</returns>
    public double SuperfluousKnot(bool start)
    {
      IntPtr pConstCurve = m_curve.ConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_SuperfluousKnot(pConstCurve, start ? 0 : 1);
    }
    #endregion

    #region IEnumerable<double> Members
    IEnumerator<double> IEnumerable<double>.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsCurveKnotList, double>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsCurveKnotList, double>(this);
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsCurveKnotList other, double epsilon)
    {
        if (null == other) 
            throw new ArgumentNullException("other");

        if (ReferenceEquals(this, other))
            return true;

        if (Count != other.Count)
            return false;

        // check for span equality
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

  /// <summary>
  /// Provides access to the control points of a nurbs curve.
  /// </summary>
  public class NurbsCurvePointList : IEnumerable<ControlPoint>, Rhino.Collections.IRhinoTable<ControlPoint>, IEpsilonComparable<NurbsCurvePointList>
  {
    private readonly NurbsCurve m_curve;

    #region constructors
    internal NurbsCurvePointList(NurbsCurve ownerCurve)
    {
      m_curve = ownerCurve;
    }
    #endregion

    /// <summary>
    /// If you want to keep a copy of this class around by holding onto it in a variable after a command
    /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
    /// call this function as many times as you want.
    /// </summary>
    public void EnsurePrivateCopy()
    {
      m_curve.EnsurePrivateCopy();
    }

    #region properties
    /// <summary>
    /// Gets the number of control points in this curve.
    /// </summary>
    public int Count
    {
      get
      {
        return m_curve.GetInt(NurbsCurve.idxCVCount);
      }
    }

    /// <summary>
    /// Gets or sets the control point location at the given index.
    /// </summary>
    /// <param name="index">Index of control vertex to access.</param>
    /// <returns>The control vertex at [index]</returns>
    public ControlPoint this[int index]
    {
      get
      {
        if (index < 0)
          throw new IndexOutOfRangeException("Index must be larger than or equal to zero.");
        if (index >= Count)
          throw new IndexOutOfRangeException("Index must be less than the number of control points.");

        Point4d pt = new Point4d();
        IntPtr ptr = m_curve.ConstPointer();
        if (UnsafeNativeMethods.ON_NurbsCurve_GetCV2(ptr, index, ref pt))
          return new ControlPoint(pt);

        return ControlPoint.Unset;
      }
      set
      {
        if (index < 0)
          throw new IndexOutOfRangeException("Index must be larger than or equal to zero.");
        if (index >= Count)
          throw new IndexOutOfRangeException("Index must be less than the number of control points.");
        IntPtr ptr = m_curve.NonConstPointer();

        Point4d pt = value.m_vertex;
        UnsafeNativeMethods.ON_NurbsCurve_SetCV2(ptr, index, ref pt);
      }
    }

    /// <summary>
    /// Gets the length of the polyline connecting all control points.
    /// </summary>
    public double ControlPolygonLength
    {
      get
      {
        IntPtr ptr = m_curve.ConstPointer();
        return UnsafeNativeMethods.ON_NurbsCurve_ControlPolygonLength(ptr);
      }
    }

    /// <summary>
    /// Constructs a polyline through all the control points. 
    /// Note that periodic curves generate a closed polyline with <i>fewer</i> 
    /// points than control-points.
    /// </summary>
    /// <returns>A polyline connecting all control points.</returns>
    public Polyline ControlPolygon()
    {
      int count = Count;
      int i_max = count;
      if (m_curve.IsPeriodic) { i_max -= (m_curve.Degree - 1); }

      Polyline rc = new Polyline(count);
      for (int i = 0; i < i_max; i++)
      {
        rc.Add(this[i].Location);
      }

      return rc;
    }
    #endregion

    #region methods
    /// <summary>
    /// Use a combination of scaling and reparameterization to change the end weights to the specified values.
    /// </summary>
    /// <param name="w0">Weight for first control point.</param>
    /// <param name="w1">Weight for last control point.</param>
    /// <returns>true on success, false on failure.</returns>
    ///<remarks>
    /// The domain, euclidean locations of the control points, and locus of the curve
    /// do not change, but the weights, homogeneous cv values and internal knot values
    /// may change. If w0 and w1 are 1 and the curve is not rational, the curve is not changed.
    ///</remarks>
    public bool ChangeEndWeights(double w0, double w1)
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_ChangeEndWeights(ptr, w0, w1);
    }

    /// <summary>
    /// Turns the curve into a Rational nurbs curve.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool MakeRational()
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeRational);
    }

    /// <summary>
    /// Sets all the control points to 1.0.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool MakeNonRational()
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeNonRational);
    }

    /// <summary>
    /// Sets a specific control-point.
    /// </summary>
    /// <param name="index">Index of control-point to set.</param>
    /// <param name="x">X coordinate of control-point.</param>
    /// <param name="y">Y coordinate of control-point.</param>
    /// <param name="z">Z coordinate of control-point.</param>
    /// <param name="weight">Weight of control-point.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnurbscircle.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnurbscircle.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnurbscircle.py' lang='py'/>
    /// </example>
    public bool SetPoint(int index, double x, double y, double z, double weight)
    {
      Point4d point = new Point4d(x, y, z, weight);
      return SetPoint(index, point);
    }
    /// <summary>
    /// Sets a specific control-point.
    /// </summary>
    /// <param name="index">Index of control-point to set.</param>
    /// <param name="point">Coordinate of control-point.</param>
    public bool SetPoint(int index, Point3d point)
    {
      Point4d pt = new Point4d(point.X, point.Y, point.Z, 1.0);
      return SetPoint(index, pt);
    }
    /// <summary>
    /// Sets a specific weighted control-point.
    /// </summary>
    /// <param name="index">Index of control-point to set.</param>
    /// <param name="point">Coordinate and weight of control-point.</param>
    public bool SetPoint(int index, Point4d point)
    {
      IntPtr pCurve = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_SetCV2(pCurve, index, ref point);
    }
    #endregion

    #region IEnumerable<ControlPoint> Members
    IEnumerator<ControlPoint> IEnumerable<ControlPoint>.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsCurvePointList, ControlPoint>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<NurbsCurvePointList, ControlPoint>(this);
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(NurbsCurvePointList other, double epsilon)
    {
        if (null == other) throw new ArgumentNullException("other");

        if (ReferenceEquals(this, other))
            return true;

        if (Count != other.Count)
            return false;

        if (!RhinoMath.EpsilonEquals(ControlPolygonLength, other.ControlPolygonLength, epsilon))
            return false;

        for (int i = 0; i < Count; ++i)
        {
            ControlPoint mine = this[i];
            ControlPoint theirs = other[i];
            if (!mine.EpsilonEquals(theirs, epsilon))
                return false;
        }

        return true;
    }
  }
}

//public:
//  // NOTE: These members are left "public" so that expert users may efficiently
//  //       create NURBS curves using the default constructor and borrow the
//  //       knot and CV arrays from their native NURBS representation.
//  //       No technical support will be provided for users who access these
//  //       members directly.  If you can't get your stuff to work, then use
//  //       the constructor with the arguments and the SetKnot() and SetCV()
//  //       functions to fill in the arrays.

//  int     m_dim;            // (>=1)

//  int     m_is_rat;         // 1 for rational B-splines. (Rational control
//                            // vertices use homogeneous form.)
//                            // 0 for non-rational B-splines. (Control
//                            // verticies do not have a weight coordinate.)

//  int     m_order;          // order = degree+1 (>=2)

//  int     m_cv_count;       // number of control vertices ( >= order )

//  // knot vector memory

//  int     m_knot_capacity;  // If m_knot_capacity > 0, then m_knot[]
//                            // is an array of at least m_knot_capacity
//                            // doubles whose memory is managed by the
//                            // ON_NurbsCurve class using rhmalloc(),
//                            // onrealloc(), and rhfree().
//                            // If m_knot_capacity is 0 and m_knot is
//                            // not NULL, then  m_knot[] is assumed to
//                            // be big enough for any requested operation
//                            // and m_knot[] is not deleted by the
//                            // destructor.

//  double* m_knot;           // Knot vector. ( The knot vector has length
//                            // m_order+m_cv_count-2. )

//  // control vertex net memory

//  int     m_cv_stride;      // The pointer to start of "CV[i]" is
//                            //   m_cv + i*m_cv_stride.

//  int     m_cv_capacity;    // If m_cv_capacity > 0, then m_cv[] is an array
//                            // of at least m_cv_capacity doubles whose
//                            // memory is managed by the ON_NurbsCurve
//                            // class using rhmalloc(), onrealloc(), and rhfree().
//                            // If m_cv_capacity is 0 and m_cv is not
//                            // NULL, then m_cv[] is assumed to be big enough
//                            // for any requested operation and m_cv[] is not
//                            // deleted by the destructor.

//  double* m_cv;             // Control points.
//                            // If m_is_rat is FALSE, then control point is
//                            //
//                            //          ( CV(i)[0], ..., CV(i)[m_dim-1] ).
//                            //
//                            // If m_is_rat is TRUE, then the control point
//                            // is stored in HOMOGENEOUS form and is
//                            //
//                            //           [ CV(i)[0], ..., CV(i)[m_dim] ].
//                            //