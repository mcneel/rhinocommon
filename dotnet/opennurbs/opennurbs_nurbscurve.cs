#pragma warning disable 1591
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino.Display;
using Rhino.Collections;

namespace Rhino.Geometry
{
  [Serializable]
  public class NurbsCurve : Curve, ISerializable
  {
    #region statics
    /// <summary>
    /// Get a non-rational, degree 1 Nurbs curve representation of the line.
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
    /// Get a rational degree 2 NURBS curve representation
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
    /// Get a rational degree 2 NURBS curve representation
    /// of the circle. Note that the parameterization of NURBS curve
    /// does not match circle's transcendental paramaterization.  
    /// Use GetRadianFromNurbFormParameter() and
    /// GetParameterFromRadian() to convert between the NURBS curve 
    /// parameter and the transcendental parameter.
    /// </summary>
    /// <returns>Curve on success, null on failure</returns>
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
    /// Get a rational degree 2 NURBS curve representation
    /// of the ellipse. Note that the parameterization of NURBS curve
    /// does not match ellsipses transcendental paramaterization.  
    /// </summary>
    /// <returns>Curve on success, null on failure</returns>
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
    /// Test two curves for similarity.
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
    /// Create a 3D NURBS curve from a list of control points
    /// </summary>
    /// <param name="periodic">If true, create a periodic uniform curve. If false, create a clamped uniform curve</param>
    /// <param name="degree">(>=1) degree=order-1</param>
    /// <param name="points">control vertex locations</param>
    /// <returns>
    /// new NURBS curve on success
    /// null on error
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

      bool rc;
      if (periodic)
        rc = UnsafeNativeMethods.ON_NurbsCurve_CreatePeriodicUniformNurbs(pCurve, dimension, order, count, ptArray, knotDelta);
      else
        rc = UnsafeNativeMethods.ON_NurbsCurve_CreateClampedUniformNurbs(pCurve, dimension, order, count, ptArray, knotDelta);

      if (false == rc)
      {
        nc.Dispose();
        return null;
      }
      return nc;
    }
    #endregion

    #region constructors
    // The only public constructor right now is the copy constructor
    // There are several static creation routines that return null if the input is bogus
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

    protected NurbsCurve( SerializationInfo info, StreamingContext context)
      :base(info, context)
    {
    }

    //[skipping]
    // public ON_NurbsCurve(ON_BezierCurve)

    /// <summary>
    /// Create a new Nurbscurve with a specific degree and control-point count.
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
    /// Create a new NurbsCurve with knot and CV memory allocated
    /// </summary>
    /// <param name="dimension">&gt;=1</param>
    /// <param name="rational">true to make a rational NURBS</param>
    /// <param name="order">(&gt;= 2) The order=degree+1</param>
    /// <param name="pointCount">(&gt;= order) number of control vertices</param>
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
    /// Gets the order of the curve. Order = Degree + 1
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
      get
      {
        if (m_knots == null)
          m_knots = new Rhino.Geometry.Collections.NurbsCurveKnotList(this);
        return m_knots;
      }
    }

    /// <summary>
    /// Gets access to the control points of this nurbs curve.
    /// </summary>
    public Collections.NurbsCurvePointList Points
    {
      get
      {
        if (m_points == null)
          m_points = new Rhino.Geometry.Collections.NurbsCurvePointList(this);
        return m_points;
      }
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

    private IntPtr CurveDisplay()
    {
      if (IntPtr.Zero == m_pCurveDisplay)
      {
        IntPtr pThis = ConstPointer();
        m_pCurveDisplay = UnsafeNativeMethods.CurveDisplay_FromNurbsCurve(pThis);
      }
      return m_pCurveDisplay;
    }
#if RHINO_SDK
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>true if successful</returns>
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
    /// Get the greville (edit point) parameter that belongs 
    /// to the control point at the specified index.
    /// </summary>
    /// <param name="index">Index of Greville (Edit) point.</param>
    public double GrevilleParameter(int index)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GrevilleAbcissa(ptr, index);
    }

    /// <summary>
    /// Get the greville (edit point) parameter that belongs 
    /// to the control point at the specified index.
    /// </summary>
    /// <param name="index">Index of Greville (Edit) point.</param>
    public Point3d GrevillePoint(int index)
    {
      double t = GrevilleParameter(index);
      return PointAt(t);
    }

    /// <summary>
    /// Get all Greville (Edit point) parameters for this curve.
    /// </summary>
    public double[] GrevilleParameters()
    {
      int count = Points.Count;
      double[] rc = new double[count];
      IntPtr ptr = ConstPointer();
      bool success = UnsafeNativeMethods.ON_NurbsCurve_GetGrevilleAbcissae(ptr, count, rc);
      if (!success) { return null; }
      return rc;
    }

    /// <summary>
    /// Get all Greville (Edit) points for this curve.
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
  /// Represents a geometry control-point.
  /// </summary>
  [Serializable]
  public struct ControlPoint
  {
    #region members
    internal Point4d m_vertex;
    #endregion

    #region constructors
    /// <summary>
    /// Create a new unweighted Control Point.
    /// </summary>
    /// <param name="x">X coordinate of Control Point.</param>
    /// <param name="y">Y coordinate of Control Point.</param>
    /// <param name="z">Z coordinate of Control Point.</param>
    public ControlPoint(double x, double y, double z)
    {
      m_vertex = new Point4d(x, y, z, 1.0);
    }
    /// <summary>
    /// Create a new weighted Control Point.
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
    /// Create a new unweighted Control Point.
    /// </summary>
    /// <param name="pt">Coordinate of Control Point.</param>
    public ControlPoint(Point3d pt)
    {
      m_vertex = new Point4d(pt.X, pt.Y, pt.Z, 1.0);
    }
    /// <summary>
    /// Create a new weighted Control Point.
    /// </summary>
    /// <param name="pt">Coordinate of Control Point.</param>
    /// <param name="weight">Weight factor of Control Point. 
    /// You should not use weights equal to or less than zero.</param>
    public ControlPoint(Point3d pt, double weight)
    {
      m_vertex = new Point4d(pt.X, pt.Y, pt.Z, weight);
    }
    /// <summary>
    /// Create a new weighted Control Point.
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
  }
}

namespace Rhino.Geometry.Collections
{
  /// <summary>
  /// Provides access to the knot vector of a nurbs curve.
  /// </summary>
  public sealed class NurbsCurveKnotList : IEnumerable<double>, Rhino.Collections.IRhinoTable<double>
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
    public void EnsurePrivateCopy()
    {
      m_curve.EnsurePrivateCopy();
    }

    /// <summary>
    /// Insert a knot and update control point locations.
    /// Does not change parameterization or locus of curve.
    /// </summary>
    /// <param name="value">Knot value to insert.</param>
    /// <returns>True on success, false on failure.</returns>
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
    /// Insert a knot and update control point locations.
    /// Does not change parameterization or locus of curve.
    /// </summary>
    /// <param name="value">Knot value to insert.</param>
    /// <param name="multiplicity">Multiplicity of knot to insert.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool InsertKnot(double value, int multiplicity)
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_InsertKnot(ptr, value, multiplicity);
    }

    /// <summary>Get knot multiplicity</summary>
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
    /// <returns>True on success, False on failure.</returns>
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
    /// <returns>True on success, False on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
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
  }

  /// <summary>
  /// Provides access to the control points of a nurbs curve.
  /// </summary>
  public class NurbsCurvePointList : IEnumerable<ControlPoint>, Rhino.Collections.IRhinoTable<ControlPoint>
  {
    private readonly NurbsCurve m_curve;

    #region constructors
    internal NurbsCurvePointList(NurbsCurve ownerCurve)
    {
      m_curve = ownerCurve;
    }
    #endregion

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
    /// Create a polyline through all the control points. 
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
    public bool MakeRational()
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeRational);
    }

    /// <summary>
    /// Sets all the control points to 1.0
    /// </summary>
    /// <returns>True on success, false on failure.</returns>
    public bool MakeNonRational()
    {
      IntPtr ptr = m_curve.NonConstPointer();
      return UnsafeNativeMethods.ON_NurbsCurve_GetBool(ptr, NurbsCurve.idxMakeNonRational);
    }

    /// <summary>
    /// Set a specific control-point.
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
    /// Set a specific control-point.
    /// </summary>
    /// <param name="index">Index of control-point to set.</param>
    /// <param name="point">Coordinate of control-point.</param>
    public bool SetPoint(int index, Point3d point)
    {
      Point4d pt = new Point4d(point.X, point.Y, point.Z, 1.0);
      return SetPoint(index, pt);
    }
    /// <summary>
    /// Set a specific weighted control-point.
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