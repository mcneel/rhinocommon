using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the value of start and end points in a single line segment.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 48)]
  [Serializable]
  public struct Line : IEquatable<Line>, IEpsilonComparable<Line>
  {
    #region members
    internal Point3d m_from;
    internal Point3d m_to;

    /// <summary>
    /// Start point of line segment.
    /// </summary>
    public Point3d From
    {
      get { return m_from; }
      set { m_from = value; }
    }
    /// <summary>
    /// Gets or sets the X coordinate of the line From point.
    /// </summary>
    public double FromX
    {
      get { return m_from.X; }
      set { m_from.X = value; }
    }
    /// <summary>
    /// Gets or sets the Y coordinate of the line From point.
    /// </summary>
    public double FromY
    {
      get { return m_from.Y; }
      set { m_from.Y = value; }
    }
    /// <summary>
    /// Gets or sets the Z coordinate of the line From point.
    /// </summary>
    public double FromZ
    {
      get { return m_from.Z; }
      set { m_from.Z = value; }
    }

    /// <summary>
    /// End point of line segment.
    /// </summary>
    public Point3d To
    {
      get { return m_to; }
      set { m_to = value; }
    }
    /// <summary>
    /// Gets or sets the X coordinate of the line To point.
    /// </summary>
    public double ToX
    {
      get { return m_to.X; }
      set { m_to.X = value; }
    }
    /// <summary>
    /// Gets or sets the Y coordinate of the line To point.
    /// </summary>
    public double ToY
    {
      get { return m_to.Y; }
      set { m_to.Y = value; }
    }
    /// <summary>
    /// Gets or sets the Z coordinate of the line To point.
    /// </summary>
    public double ToZ
    {
      get { return m_to.Z; }
      set { m_to.Z = value; }
    }
    #endregion

    #region constructors
    /// <summary>
    /// Constructs a new line segment between two points.
    /// </summary>
    /// <param name="from">Start point of line.</param>
    /// <param name="to">End point of line.</param>
    public Line(Point3d from, Point3d to)
    {
      m_from = from;
      m_to = to;
    }

    /// <summary>
    /// Constructs a new line segment from start point and span vector.
    /// </summary>
    /// <param name="start">Start point of line segment.</param>
    /// <param name="span">Direction and length of line segment.</param>
    public Line(Point3d start, Vector3d span)
    {
      m_from = start;
      m_to = start + span;
    }

    /// <summary>
    /// Constructs a new line segment from start point, direction and length.
    /// </summary>
    /// <param name="start">Start point of line segment.</param>
    /// <param name="direction">Direction of line segment.</param>
    /// <param name="length">Length of line segment.</param>
    public Line(Point3d start, Vector3d direction, double length)
    {
      Vector3d dir = direction;
      if (!dir.Unitize())
        dir = new Vector3d(0, 0, 1);

      m_from = start;
      m_to = start + dir * length;
    }

    /// <summary>
    /// Constructs a new line segment between two points.
    /// </summary>
    /// <param name="x0">The X coordinate of the first point.</param>
    /// <param name="y0">The Y coordinate of the first point.</param>
    /// <param name="z0">The Z coordinate of the first point.</param>
    /// <param name="x1">The X coordinate of the second point.</param>
    /// <param name="y1">The Y coordinate of the second point.</param>
    /// <param name="z1">The Z coordinate of the second point.</param>
    public Line(double x0, double y0, double z0, double x1, double y1, double z1)
    {
      m_from = new Point3d(x0, y0, z0);
      m_to = new Point3d(x1, y1, z1);
    }
    #endregion

    #region constants
    /// <summary>
    /// Gets a line segment which has <see cref="Point3d.Unset"/> end points.
    /// </summary>
    static public Line Unset
    {
      get { return new Line(Point3d.Unset, Point3d.Unset); }
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets a value indicating whether or not this line is valid. 
    /// Valid lines must have valid start and end points.
    /// </summary>
    public bool IsValid
    {
      get
      {
        return From.IsValid && To.IsValid;
      }
    }

    /// <summary>
    /// Gets or sets the length of this line segment. 
    /// Note that a negative length will invert the line segment without 
    /// making the actual length negative. The line From point will remain fixed 
    /// when a new Length is set.
    /// </summary>
    public double Length
    {
      get { return From.DistanceTo(To); }
      set
      {
        Vector3d dir = To - From;
        if (!dir.Unitize())
          dir = new Vector3d(0, 0, 1);

        To = From + dir * value;
      }
    }

    /// <summary>
    /// Gets the direction of this line segment. 
    /// The length of the direction vector equals the length of 
    /// the line segment.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_intersectlines.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectlines.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectlines.py' lang='py'/>
    /// </example>
    public Vector3d Direction
    {
      get { return To - From; }
    }

    /// <summary>
    /// Gets the tangent of the line segment. 
    /// Note that tangent vectors are always unit vectors.
    /// </summary>
    /// <value>Sets only the direction of the line, the length is maintained.</value>
    public Vector3d UnitTangent
    {
      get
      {
        Vector3d v = To - From;
        v.Unitize();
        return v;
      }
      //set
      //{
      //  Vector3d dir = value;
      //  if (!dir.Unitize()) { dir.Set(0, 0, 1); }

      //  To = From + value * Length;
      //}
    }

    /// <summary>
    /// Gets the line's 3d axis aligned bounding box.
    /// </summary>
    /// <returns>3d bounding box.</returns>
    public BoundingBox BoundingBox
    {
      get
      {
        BoundingBox rc = new BoundingBox(From, To);
        rc.MakeValid();
        return rc;
      }
    }
    #endregion

    #region static methods
#if RHINO_SDK
    /// <summary>
    /// Attempt to fit a line through a set of points.
    /// </summary>
    /// <param name="points">The points through which to fit.</param>
    /// <param name="fitLine">The resulting line on success.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool TryFitLineToPoints(IEnumerable<Point3d> points, out Line fitLine)
    {
      fitLine = new Line();
      if (null == points)
        return false;

      int count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (count < 2)
        return false;
      bool rc = UnsafeNativeMethods.RHC_FitLineToPoints(count, ptArray, ref fitLine);
      return rc;
    }

    /// <summary>
    /// Creates a line segment between a pair of curves such that the line segment is either tangent or perpendicular to each of the curves.
    /// </summary>
    /// <param name="curve0">The first curve.</param>
    /// <param name="curve1">The second curve.</param>
    /// <param name="t0">Parameter value of point on curve0. Seed value at input and solution at output.</param>
    /// <param name="t1">Parameter value of point on curve 0.  Seed value at input and solution at output.</param>
    /// <param name="perpendicular0">Find line Perpendicuar to (true) or tangent to (false) curve0.</param>
    /// <param name="perpendicular1">Find line Perpendicuar to (true) or tangent to (false) curve1.</param>
    /// <param name="line">The line segment if successful.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool TryCreateBetweenCurves(Curve curve0, Curve curve1, ref double t0, ref double t1, bool perpendicular0, bool perpendicular1, out Line line)
    {
      line = Line.Unset;
      IntPtr pCurve0 = curve0.ConstPointer();
      IntPtr pCurve1 = curve1.ConstPointer();
      bool rc = UnsafeNativeMethods.RHC_RhGetTanPerpPoint(pCurve0, pCurve1, ref t0, ref t1, perpendicular0, perpendicular1, ref line);
      return rc;
    }
#endif

    #endregion

    #region methods
    /// <summary>
    /// Determines whether an object is a line that has the same value as this line.
    /// </summary>
    /// <param name="obj">An object.</param>
    /// <returns>true if obj is a Line and has the same coordinates as this; otherwise false.</returns>
    public override bool Equals(object obj)
    {
      return obj is Line && this == (Line)obj;
    }

    /// <summary>
    /// Determines whether a line has the same value as this line.
    /// </summary>
    /// <param name="other">A line.</param>
    /// <returns>true if other has the same coordinates as this; otherwise false.</returns>
    public bool Equals(Line other)
    {
      return this == other;
    }

    public bool EpsilonEquals(Line other, double epsilon)
    {
        return m_from.EpsilonEquals(other.m_from, epsilon) &&
               m_to.EpsilonEquals(other.m_to, epsilon);
    }

    /// <summary>
    /// Computes a hash number that represents this line.
    /// </summary>
    /// <returns>A number that is not unique to the value of this line.</returns>
    public override int GetHashCode()
    {
      return From.GetHashCode() ^ To.GetHashCode();
    }

    /// <summary>
    /// Contructs the string representation of this line, in the form "From,To".
    /// </summary>
    /// <returns>A text string.</returns>
    public override string ToString()
    {
      return string.Format("{0},{1}", From.ToString(), To.ToString());
    }

    /// <summary>
    /// Flip the endpoints of the line segment.
    /// </summary>
    public void Flip()
    {
      Point3d temp = From;
      From = To;
      To = temp;
    }

    /// <summary>
    /// Evaluates the line at the specified parameter.
    /// </summary>
    /// <param name="t">Parameter to evaluate line segment at. Line parameters are normalised parameters.</param>
    /// <returns>The point at the specified parameter.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_intersectlines.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectlines.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectlines.py' lang='py'/>
    /// </example>
    public Point3d PointAt(double t)
    {
      double s = 1.0 - t;
      return new Point3d((From.m_x == To.m_x) ? From.m_x : s * From.m_x + t * To.m_x,
                         (From.m_y == To.m_y) ? From.m_y : s * From.m_y + t * To.m_y,
                         (From.m_z == To.m_z) ? From.m_z : s * From.m_z + t * To.m_z);
    }

    /// <summary>
    /// Finds the parameter on the infinite line segment that is closest to a test point.
    /// </summary>
    /// <param name="testPoint">Point to project onto the line.</param>
    /// <returns>The parameter on the line that is closest to testPoint.</returns>
    public double ClosestParameter(Point3d testPoint)
    {
      double rc = 0.0;
      UnsafeNativeMethods.ON_Line_ClosestPointTo(testPoint, From, To, ref rc);
      return rc;
    }

    /// <summary>
    /// Finds the point on the (in)finite line segment that is closest to a test point.
    /// </summary>
    /// <param name="testPoint">Point to project onto the line.</param>
    /// <param name="limitToFiniteSegment">If true, the projection is limited to the finite line segment.</param>
    /// <returns>The point on the (in)finite line that is closest to testPoint.</returns>
    public Point3d ClosestPoint(Point3d testPoint, bool limitToFiniteSegment)
    {
      double t = ClosestParameter(testPoint);

      if (limitToFiniteSegment)
      {
        t = Math.Max(t, 0.0);
        t = Math.Min(t, 1.0);
      }

      return PointAt(t);
    }

    /// <summary>
    /// Compute the shortest distance between this line segment and a test point.
    /// </summary>
    /// <param name="testPoint">Point for distance computation.</param>
    /// <param name="limitToFiniteSegment">If true, the distance is limited to the finite line segment.</param>
    /// <returns>The shortest distance between this line segment and testPoint.</returns>
    public double DistanceTo(Point3d testPoint, bool limitToFiniteSegment)
    {
      Point3d pp = ClosestPoint(testPoint, limitToFiniteSegment);
      return pp.DistanceTo(testPoint);
    }
    /// <summary>
    /// Finds the shortest distance between this line as a finite segment
    /// and a test point.
    /// </summary>
    /// <param name="testPoint">A point to test.</param>
    /// <returns>The minimum distance.</returns>
    public double MinimumDistanceTo(Point3d testPoint)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToPoint(ref this, testPoint, true);
    }
    /// <summary>
    /// Finds the shortest distance between this line as a finite segment
    /// and another finite segment.
    /// </summary>
    /// <param name="testLine">A line to test.</param>
    /// <returns>The minimum distance.</returns>
    public double MinimumDistanceTo(Line testLine)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToLine(ref this, ref testLine, true);
    }
    /// <summary>
    /// Finds the largest distance between this line as a finite segment
    /// and a test point.
    /// </summary>
    /// <param name="testPoint">A point to test.</param>
    /// <returns>The maximum distance.</returns>
    public double MaximumDistanceTo(Point3d testPoint)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToPoint(ref this, testPoint, false);
    }
    /// <summary>
    /// Finds the largest distance between this line as a finite segment
    /// and another finite segment.
    /// </summary>
    /// <param name="testLine">A line to test.</param>
    /// <returns>The maximum distance.</returns>
    public double MaximumDistanceTo(Line testLine)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToLine(ref this, ref testLine, false);
    }

    /// <summary>
    /// Transform the line using a Transformation matrix.
    /// </summary>
    /// <param name="xform">Transform to apply to this line.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Transform(Transform xform)
    {
      return UnsafeNativeMethods.ON_Line_Transform(ref this, ref xform);
    }

    /// <summary>
    /// Constructs a nurbs curve representation of this line. 
    /// This amounts to the same as calling NurbsCurve.CreateFromLine().
    /// </summary>
    /// <returns>A nurbs curve representation of this line or null if no such representation could be made.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      return NurbsCurve.CreateFromLine(this);
    }

    //David: all extend methods are untested as of yet.

    /// <summary>
    /// Extend the line by custom distances on both sides.
    /// </summary>
    /// <param name="startLength">
    /// Distance to extend the line at the start point. 
    /// Positive distance result in longer lines.
    /// </param>
    /// <param name="endLength">
    /// Distance to extend the line at the end point. 
    /// Positive distance result in longer lines.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool Extend(double startLength, double endLength)
    {
      if (!IsValid) { return false; }
      if (Length == 0.0) { return false; }

      Point3d A = m_from;
      Point3d B = m_to;

      Vector3d tan = UnitTangent;

      if (startLength != 0.0) { A = m_from - startLength * tan; }
      if (endLength != 0.0) { B = m_to + endLength * tan; }

      m_from = A;
      m_to = B;

      return true;
    }

    /// <summary>
    /// Ensure the line extends all the way through a box. 
    /// Note, this does not result in the shortest possible line 
    /// that overlaps the box.
    /// </summary>
    /// <param name="box">Box to extend through.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool ExtendThroughBox(BoundingBox box)
    {
      if (!IsValid) { return false; }
      if (!box.IsValid) { return false; }

      return ExtendThroughPointSet(box.GetCorners(), 0.0);
    }
    /// <summary>
    /// Ensure the line extends all the way through a box. 
    /// Note, this does not result in the shortest possible line that overlaps the box.
    /// </summary>
    /// <param name="box">Box to extend through.</param>
    /// <param name="additionalLength">Additional length to append at both sides of the line.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool ExtendThroughBox(BoundingBox box, double additionalLength)
    {
      if (!IsValid) { return false; }
      if (!box.IsValid) { return false; }

      return ExtendThroughPointSet(box.GetCorners(), additionalLength);
    }
    /// <summary>
    /// Ensure the line extends all the way through a box. 
    /// Note, this does not result in the shortest possible line that overlaps the box.
    /// </summary>
    /// <param name="box">Box to extend through.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool ExtendThroughBox(Box box)
    {
      if (!IsValid) { return false; }
      if (!box.IsValid) { return false; }

      return ExtendThroughPointSet(box.GetCorners(), 0.0);
    }
    /// <summary>
    /// Ensure the line extends all the way through a box. 
    /// Note, this does not result in the shortest possible line that overlaps the box.
    /// </summary>
    /// <param name="box">Box to extend through.</param>
    /// <param name="additionalLength">Additional length to append at both sides of the line.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool ExtendThroughBox(Box box, double additionalLength)
    {
      if (!IsValid) { return false; }
      if (!box.IsValid) { return false; }

      return ExtendThroughPointSet(box.GetCorners(), additionalLength);
    }
    internal bool ExtendThroughPointSet(IEnumerable<Point3d> pts, double additionalLength)
    {
      Vector3d unit = UnitTangent;
      if (!unit.IsValid) { return false; }

      double t0 = double.MaxValue;
      double t1 = double.MinValue;

      foreach (Point3d pt in pts)
      {
        double t = ClosestParameter(pt);
        t0 = Math.Min(t0, t);
        t1 = Math.Max(t1, t);
      }

      if (t0 <= t1)
      {
        Point3d A = PointAt(t0) - (additionalLength * unit);
        Point3d B = PointAt(t1) + (additionalLength * unit);
        m_from = A;
        m_to = B;
      }
      else
      {
        Point3d A = PointAt(t0) + (additionalLength * unit);
        Point3d B = PointAt(t1) - (additionalLength * unit);
        m_from = A;
        m_to = B;
      }

      return true;
    }

    /// <summary>
    /// Gets a plane that contains the line. The origin of the plane is at the start of the line.
    /// If possible, a plane parallel to the world xy, yz, or zx plane is returned.
    /// </summary>
    /// <param name="plane">If the return value is true, the plane out parameter is assigned during this call.</param>
    /// <returns>true on success.</returns>
    public bool TryGetPlane(out Plane plane)
    {
      plane = new Plane();
      return UnsafeNativeMethods.ON_Line_InPlane(ref this, ref plane);
    }
    #endregion

    /// <summary>
    /// Determines whether two lines have the same value.
    /// </summary>
    /// <param name="a">A line.</param>
    /// <param name="b">Another line.</param>
    /// <returns>true if a has the same coordinates as b; otherwise false.</returns>
    public static bool operator ==(Line a, Line b)
    {
      return a.From == b.From && a.To == b.To;
    }

    /// <summary>
    /// Determines whether two lines have different values.
    /// </summary>
    /// <param name="a">A line.</param>
    /// <param name="b">Another line.</param>
    /// <returns>true if a has any coordinate that distinguishes it from b; otherwise false.</returns>
    public static bool operator !=(Line a, Line b)
    {
      return a.From != b.From || a.To != b.To;
    }
  }
}