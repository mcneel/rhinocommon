using System;
using System.Collections.Generic;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the values of a plane and two intervals
  /// that form an oriented rectangle in three dimensions.
  /// </summary>
  //[Serializable]
  public struct Rectangle3d : IEpsilonComparable<Rectangle3d>
  {
    #region Members
    internal Plane m_plane;
    internal Interval m_x;
    internal Interval m_y;
    #endregion

    #region static methods
    /// <summary>
    /// Attempts to create a rectangle from a polyline. In order for the polyline to qualify 
    /// as a rectangle, it must have 4 or 5 corner points (i.e. it need not be closed).
    /// </summary>
    /// <param name="polyline">Polyline to parse.</param>
    /// <returns>A rectangle that is shaped similarly to the polyline or Rectangle3d.Unset 
    /// if the polyline does not represent a rectangle.</returns>
    public static Rectangle3d CreateFromPolyline(IEnumerable<Point3d> polyline)
    {
      double dev, angdev;
      return CreateFromPolyline(polyline, out dev, out angdev);
    }
    /// <summary>
    /// Attempts to create a Rectangle from a Polyline. In order for a polyline to qualify 
    /// as a rectangle, it must have 4 or 5 corner points (i.e. it need not be closed).
    /// <para>This overload also returns deviations.</para>
    /// </summary>
    /// <param name="polyline">Polyline to parse.</param>
    /// <param name="deviation">On success, the deviation will contain the largest deviation between the polyline and the rectangle.</param>
    /// <param name="angleDeviation">On success, the angleDeviation will contain the largest deviation (in radians) between the polyline edges and the rectangle edges.</param>
    /// <returns>A rectangle that is shaped similarly to the polyline or Rectangle3d.Unset 
    /// if the polyline does not represent a rectangle.</returns>
    public static Rectangle3d CreateFromPolyline(IEnumerable<Point3d> polyline, out double deviation, out double angleDeviation)
    {
      if (polyline == null) { throw new ArgumentNullException("polyline"); }

      deviation = 0.0;
      angleDeviation = 0.0;

      Rhino.Collections.Point3dList pts = new Rhino.Collections.Point3dList(5);
      foreach (Point3d pt in polyline)
      {
        pts.Add(pt);
        if (pts.Count > 5) { return Rectangle3d.Unset; }
      }
      if (pts.Count < 4) { return Rectangle3d.Unset; }
      if (pts.Count == 5) { pts.RemoveAt(4); }

      Vector3d AB = pts[1] - pts[0];
      Vector3d DC = pts[2] - pts[3];
      Vector3d AD = pts[3] - pts[0];
      Vector3d BC = pts[2] - pts[1];
      Vector3d x = AB + DC;
      Vector3d y = AD + BC;

      if (x.IsZero && y.IsZero)
      {
        Rectangle3d null_rec = new Rectangle3d(new Plane(pts[0], Vector3d.XAxis, Vector3d.YAxis), 0.0, 0.0);
        ComputeDeviation(null_rec, pts, out deviation, out angleDeviation);
        return null_rec;
      }
      if (x.IsZero) { x.PerpendicularTo(y); }
      if (y.IsZero) { y.PerpendicularTo(x); }

      if (!x.Unitize()) { return Rectangle3d.Unset; }
      if (!y.Unitize()) { return Rectangle3d.Unset; }

      Rectangle3d rc = new Rectangle3d();
      rc.m_plane = new Plane(pts[0], x, y);
      rc.m_x = new Interval(0, 0.5 * (AB.Length + DC.Length));
      rc.m_y = new Interval(0, 0.5 * (AD.Length + BC.Length));

      ComputeDeviation(rc, pts, out deviation, out angleDeviation);
      return rc;
    }
    private static void ComputeDeviation(Rectangle3d rec, Rhino.Collections.Point3dList pts, out double dev, out double angdev)
    {
      dev = double.MaxValue;
      for (int i = 0; i < 4; i++)
      {
        dev = Math.Min(dev, rec.Corner(i).DistanceTo(pts[i]));
      }

      angdev = double.MaxValue;
      for (int i = 0; i < 4; i++)
      {
        int j = (i == 3) ? 0 : i + 1;
        Vector3d re = rec.Corner(i) - rec.Corner(j);
        Vector3d pe = pts[i] - pts[j];
        double ad = Vector3d.VectorAngle(re, pe);
        if (RhinoMath.IsValidDouble(ad)) { angdev = Math.Min(angdev, ad); }
      }
    }
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new rectangle from width and height.
    /// </summary>
    /// <param name="plane">Base plane for Rectangle.</param>
    /// <param name="width">Width (as measured along the base plane x-axis) of rectangle.</param>
    /// <param name="height">Height (as measured along the base plane y-axis) of rectangle.</param>
    public Rectangle3d(Plane plane, double width, double height)
    {
      m_plane = plane;
      m_x = new Interval(0, width);
      m_y = new Interval(0, height);
      MakeIncreasing();
    }
    /// <summary>
    /// Initializes a new rectangle from dimensions.
    /// </summary>
    /// <param name="plane">Base plane for Rectangle.</param>
    /// <param name="width">Dimension of rectangle along the base plane x-axis.</param>
    /// <param name="height">Dimension of rectangle along the base plane y-axis.</param>
    public Rectangle3d(Plane plane, Interval width, Interval height)
    {
      m_plane = plane;
      m_x = width;
      m_y = height;
    }
    /// <summary>
    /// Initializes a new rectangle from a base plane and two corner points.
    /// </summary>
    /// <param name="plane">Base plane for Rectangle.</param>
    /// <param name="cornerA">First corner of Rectangle (will be projected onto plane).</param>
    /// <param name="cornerB">Second corner of Rectangle (will be projected onto plane).</param>
    public Rectangle3d(Plane plane, Point3d cornerA, Point3d cornerB)
    {
      m_plane = plane;

      double s0, t0;
      double s1, t1;

      if (!plane.ClosestParameter(cornerA, out s0, out t0))
      {
        throw new InvalidOperationException("cornerA could not be projected onto rectangle plane.");
      }
      if (!plane.ClosestParameter(cornerB, out s1, out t1))
      {
        throw new InvalidOperationException("cornerB could not be projected onto rectangle plane.");
      }

      m_x = new Interval(s0, s1);
      m_y = new Interval(t0, t1);

      MakeIncreasing();
    }
    #endregion

    #region constants
    /// <summary>
    /// Gets a rectangle with Unset components.
    /// </summary>
    static public Rectangle3d Unset
    {
      get { return new Rectangle3d(Plane.Unset, Interval.Unset, Interval.Unset); }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets a value indicating whether or not this is a valid rectangle. 
    /// A rectangle is considered to be valid when the base plane and both dimensions are valid.
    /// </summary>
    public bool IsValid
    {
      get
      {
        if (!m_plane.IsValid) { return false; }
        if (!m_x.IsValid) { return false; }
        if (!m_y.IsValid) { return false; }
        return true;
      }
    }

    /// <summary>
    /// Gets or sets the base plane of the rectangle.
    /// </summary>
    public Plane Plane
    {
      get { return m_plane; }
      set { m_plane = value; }
    }
    /// <summary>
    /// Gets or sets the dimensions of the rectangle along the base plane X-Axis (i.e. the width).
    /// </summary>
    public Interval X
    {
      get { return m_x; }
      set { m_x = value; }
    }
    /// <summary>
    /// Gets or sets the dimensions of the rectangle along the base plane Y-Axis (i.e. the height).
    /// </summary>
    public Interval Y
    {
      get { return m_y; }
      set { m_y = value; }
    }
    /// <summary>
    /// Gets the signed width of the rectangle. If the X dimension is decreasing, the width will be negative.
    /// </summary>
    public double Width
    {
      get { return m_x.Length; }
    }
    /// <summary>
    /// Gets the signed height of the rectangle. If the Y dimension is decreasing, the height will be negative.
    /// </summary>
    public double Height
    {
      get { return m_y.Length; }
    }
    /// <summary>
    /// Gets the unsigned Area of the rectangle.
    /// </summary>
    public double Area
    {
      get
      {
        return Math.Abs(m_x.Length) * Math.Abs(m_y.Length);
      }
    }
    /// <summary>
    /// Gets the circumference of the rectangle.
    /// </summary>
    public double Circumference
    {
      get
      {
        return 2 * m_x.Length + 2 * m_y.Length;
      }
    }
    /// <summary>
    /// Gets the world aligned boundingbox for this rectangle.
    /// </summary>
    public BoundingBox BoundingBox
    {
      get
      {
        BoundingBox box = new BoundingBox(Corner(0), Corner(1));
        box.MakeValid();
        box.Union(Corner(2));
        box.Union(Corner(3));
        return box;
      }
    }
    /// <summary>
    /// Gets the point in the center of the rectangle.
    /// </summary>
    public Point3d Center
    {
      get { return m_plane.PointAt(m_x.Mid, m_y.Mid, 0.0); }
    }
    #endregion

    #region methods
    /// <summary>
    /// Ensures the X and Y dimensions are increasing or singleton intervals.
    /// </summary>
    public void MakeIncreasing()
    {
      m_x.MakeIncreasing();
      m_y.MakeIncreasing();
    }
    /// <summary>
    /// Gets the corner at the given index.
    /// </summary>
    /// <param name="index">
    /// Index of corner, valid values are:
    /// <para>0 = lower left (min-x, min-y)</para>
    /// <para>1 = lower right (max-x, min-y)</para>
    /// <para>2 = upper right (max-x, max-y)</para>
    /// <para>3 = upper left (min-x, max-y)</para>
    /// </param>
    /// <returns>The point at the given corner index.</returns>
    public Point3d Corner(int index)
    {
      switch (index)
      {
        case 0: return m_plane.PointAt(m_x.T0, m_y.T0);
        case 1: return m_plane.PointAt(m_x.T1, m_y.T0);
        case 2: return m_plane.PointAt(m_x.T1, m_y.T1);
        case 3: return m_plane.PointAt(m_x.T0, m_y.T1);
        default:
          throw new IndexOutOfRangeException("Rectangle corner index must be between and including 0 and 3");
      }
    }
    /// <summary>
    /// Recenters the base plane on one of the corners.
    /// </summary>
    /// <param name="index">
    /// Index of corner, valid values are:
    /// <para>0 = lower left (min-x, min-y)</para>
    /// <para>1 = lower right (max-x, min-y)</para>
    /// <para>2 = upper right (max-x, max-y)</para>
    /// <para>3 = upper left (min-x, max-y)</para>
    /// </param>
    public void RecenterPlane(int index)
    {
      RecenterPlane(Corner(index));
    }
    /// <summary>
    /// Recenters the base plane on a new origin.
    /// </summary>
    /// <param name="origin">New origin for plane.</param>
    public void RecenterPlane(Point3d origin)
    {
      double s, t;
      m_plane.ClosestParameter(origin, out s, out t);

      m_plane.Origin = origin;
      m_x -= s;
      m_y -= t;
    }

    /// <summary>
    /// Gets a point in Rectangle space.
    /// </summary>
    /// <param name="x">Normalized parameter along Rectangle width.</param>
    /// <param name="y">Normalized parameter along Rectangle height.</param>
    /// <returns>The point at the given x,y parameter.</returns>
    public Point3d PointAt(double x, double y)
    {
      return m_plane.PointAt(m_x.ParameterAt(x), m_y.ParameterAt(y));
    }
    /// <summary>
    /// Gets a point along the rectangle boundary.
    /// </summary>
    /// <param name="t">Parameter along rectangle boundary. Valid values range from 0.0 to 4.0, 
    /// where each integer domain represents a single boundary edge.</param>
    /// <returns>The point at the given boundary parameter.</returns>
    public Point3d PointAt(double t)
    {
      int segment = (int)Math.Floor(t);
      double remainder = t - segment;

      if (segment < 0)
      {
        segment = 0;
        remainder = 0;
      }
      else if (segment >= 4)
      {
        segment = 3;
        remainder = 1.0;
      }

      switch (segment)
      {
        case 0: return new Line(Corner(0), Corner(1)).PointAt(remainder);
        case 1: return new Line(Corner(1), Corner(2)).PointAt(remainder);
        case 2: return new Line(Corner(2), Corner(3)).PointAt(remainder);
        case 3: return new Line(Corner(3), Corner(0)).PointAt(remainder);
        default:
          throw new IndexOutOfRangeException("Rectangle boundary parameter out of range");
      }
    }

    /// <summary>
    /// Gets the point on the rectangle that is closest to a test-point.
    /// </summary>
    /// <param name="point">Point to project.</param>
    /// <returns>The point on or in the rectangle closest to the test point or Point3d.Unset on failure.</returns>
    public Point3d ClosestPoint(Point3d point)
    {
      return ClosestPoint(point, true);
    }
    /// <summary>
    /// Gets the point on the rectangle that is closest to a test-point.
    /// </summary>
    /// <param name="point">Point to project.</param>
    /// <param name="includeInterior">If false, the point is projected onto the boundary edge only, 
    /// otherwise the interior of the rectangle is also taken into consideration.</param>
    /// <returns>The point on the rectangle closest to the test point or Point3d.Unset on failure.</returns>
    public Point3d ClosestPoint(Point3d point, bool includeInterior)
    {
      double s, t;
      if (!m_plane.ClosestParameter(point, out s, out t)) { return Point3d.Unset; }

      double x = s;
      double y = t;

      x = Math.Max(x, m_x.Min);
      x = Math.Min(x, m_x.Max);
      y = Math.Max(y, m_y.Min);
      y = Math.Min(y, m_y.Max);

      if (includeInterior) { return m_plane.PointAt(x, y); }

      //Offset of test point.
      double dx0 = Math.Abs(x - m_x.Min);
      double dx1 = Math.Abs(x - m_x.Max);
      double dy0 = Math.Abs(y - m_y.Min);
      double dy1 = Math.Abs(y - m_y.Max);

      //Absolute width and height of rectangle.
      double w = Math.Abs(Width);
      double h = Math.Abs(Height);

      if (w > h)
      {
        if (dx0 < dy0 && dx0 < dy1) { return m_plane.PointAt(m_x.Min, y); } //Project to left edge
        if (dx1 < dy0 && dx1 < dy1) { return m_plane.PointAt(m_x.Max, y); } //Project to right edge
        if (dy0 < dy1) { return m_plane.PointAt(x, m_y.Min); } //Project to bottom edge
        return m_plane.PointAt(x, m_y.Max); //Project to top edge
      }

      if (dy0 < dx0 && dy0 < dx1) { return m_plane.PointAt(x, m_y.Min); } //Project to bottom edge
      if (dy1 < dx0 && dy1 < dx1) { return m_plane.PointAt(x, m_y.Max); } //Project to top edge
      if (dx0 < dx1) { return m_plane.PointAt(m_x.Min, y); } //Project to left edge
      return m_plane.PointAt(m_x.Max, y); //Project to right edge
    }

    /// <summary>
    /// Determines if a point is included in this rectangle.
    /// </summary>
    /// <param name="pt">Point to test. The point will be projected onto the Rectangle plane before inclusion is determined.</param>
    /// <returns>Point Rectangle relationship.</returns>
    public PointContainment Contains(Point3d pt)
    {
      double s, t;
      if (!m_plane.ClosestParameter(pt, out s, out t)) { return PointContainment.Unset; }
      return Contains(s, t);
    }
    /// <summary>
    /// Determines if two plane parameters are included in this rectangle.
    /// </summary>
    /// <param name="x">Parameter along base plane X direction.</param>
    /// <param name="y">Parameter along base plane Y direction.</param>
    /// <returns>Parameter Rectangle relationship.</returns>
    public PointContainment Contains(double x, double y)
    {
      if (x < m_x.Min) { return PointContainment.Outside; }
      if (x > m_x.Max) { return PointContainment.Outside; }
      if (y < m_y.Min) { return PointContainment.Outside; }
      if (y > m_y.Max) { return PointContainment.Outside; }

      if (x == m_x.T0 || x == m_x.T1 || y == m_y.T0 || y == m_y.T1) { return PointContainment.Coincident; }
      return PointContainment.Inside;
    }

    /// <summary>
    /// Transforms this rectangle. Note that rectangles cannot be skewed or tapered.
    /// </summary>
    /// <param name="xform">Transformation to apply.</param>
    public bool Transform(Transform xform)
    {
      Point3d p0 = Corner(0);
      Point3d p1 = Corner(1);
      Point3d p2 = Corner(2);
      Point3d p3 = Corner(3);

      if (!m_plane.Transform(xform)) { return false; }
      p0.Transform(xform);
      p1.Transform(xform);
      p2.Transform(xform);
      p3.Transform(xform);

      double s0, t0; if (!m_plane.ClosestParameter(p0, out s0, out t0)) { return false; }
      double s1, t1; if (!m_plane.ClosestParameter(p1, out s1, out t1)) { return false; }
      double s2, t2; if (!m_plane.ClosestParameter(p2, out s2, out t2)) { return false; }
      double s3, t3; if (!m_plane.ClosestParameter(p3, out s3, out t3)) { return false; }

      m_x = new Interval(0.5 * (s0 + s3), 0.5 * (s1 + s2));
      m_y = new Interval(0.5 * (t0 + t1), 0.5 * (t2 + t3));

      return true;
    }

    /// <summary>
    /// Constructs a polyline from this rectangle.
    /// </summary>
    /// <returns>A polyline with the same shape as this rectangle.</returns>
    public Polyline ToPolyline()
    {
      Polyline rc = new Polyline(5);
      rc.Add(Corner(0));
      rc.Add(Corner(1));
      rc.Add(Corner(2));
      rc.Add(Corner(3));
      rc.Add(Corner(0));
      return rc;
    }
    /// <summary>
    /// Constructs a nurbs curve representation of this rectangle.
    /// </summary>
    /// <returns>A nurbs curve with the same shape as this rectangle.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      return ToPolyline().ToNurbsCurve();
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(Rectangle3d other, double epsilon)
    {
      return m_plane.EpsilonEquals(other.m_plane, epsilon) &&
             m_x.EpsilonEquals(other.m_x, epsilon) &&
             m_y.EpsilonEquals(other.m_y, epsilon);
    }
  }
}