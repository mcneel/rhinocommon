using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a single line segment.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 48)]
  [Serializable()]
  public struct Line
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
    /// Create a new line segment between two points.
    /// </summary>
    /// <param name="from">Start point of line.</param>
    /// <param name="to">End point of line.</param>
    public Line(Point3d from, Point3d to)
    {
      m_from = from;
      m_to = to;
    }

    /// <summary>
    /// Create a new line segment from start point and span vector.
    /// </summary>
    /// <param name="start">Start point of line segment.</param>
    /// <param name="span">Direction and length of line segment.</param>
    public Line(Point3d start, Vector3d span)
    {
      m_from = start;
      m_to = start + span;
    }

    /// <summary>
    /// Create a new line segment from start point, direction and length.
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
    #endregion

    #region constants
    /// <summary>
    /// Get a line segment which has Unset end points.
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
    /// <value>Sets both the direction and the length of the line.</value>
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
    /// <summary>
    /// Attempt to fit a line through a set of points.
    /// </summary>
    /// <param name="points">The points through which to fit.</param>
    /// <param name="fitLine">The resulting line on success</param>
    /// <returns>True on success, false on failure.</returns>
    public static bool TryFitLineToPoints(IEnumerable<Point3d> points, out Line fitLine)
    {
      fitLine = new Line();
      if (null == points)
        return false;

      int count = 0;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (count < 2)
        return false;
      bool rc = UnsafeNativeMethods.RHC_FitLineToPoints(count, ptArray, ref fitLine);
      return rc;
    }
    #endregion

    #region methods
    public override string ToString()
    {
      return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0},{1}", From, To);
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
    public Point3d PointAt(double t)
    {
      double s = 1.0 - t;
      return new Point3d((From.m_x == To.m_x) ? From.m_x : s * From.m_x + t * To.m_x,
                         (From.m_y == To.m_y) ? From.m_y : s * From.m_y + t * To.m_y,
                         (From.m_z == To.m_z) ? From.m_z : s * From.m_z + t * To.m_z);
    }

    /// <summary>
    /// This method is Obsolete, use ClosestParameter() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestParameter() instead.")]
    public double ClosestParameterTo(Point3d testPoint)
    {
      return ClosestParameter(testPoint);
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
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead.")]
    public Point3d ClosestPointTo(Point3d testPoint, bool limitToFiniteSegment)
    {
      return ClosestPoint(testPoint, limitToFiniteSegment);
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
    /// Find the shortest distance between this line as a finite segment
    /// and a test point
    /// </summary>
    /// <param name="testPoint"></param>
    /// <returns></returns>
    public double MinimumDistanceTo(Point3d testPoint)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToPoint(ref this, testPoint, true);
    }
    /// <summary>
    /// Find the shortest distance between this line as a finite segment
    /// and another finite segment
    /// </summary>
    /// <param name="testLine"></param>
    /// <returns></returns>
    public double MinimumDistanceTo(Line testLine)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToLine(ref this, ref testLine, true);
    }
    /// <summary>
    /// Find the largest distance between this line as a finite segment
    /// and a test point
    /// </summary>
    /// <param name="testPoint"></param>
    /// <returns></returns>
    public double MaximumDistanceTo(Point3d testPoint)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToPoint(ref this, testPoint, false);
    }
    /// <summary>
    /// Find the largest distance between this line as a finite segment
    /// and another finite segment
    /// </summary>
    /// <param name="testLine"></param>
    /// <returns></returns>
    public double MaximumDistanceTo(Line testLine)
    {
      return UnsafeNativeMethods.ON_Line_DistanceToLine(ref this, ref testLine, false);
    }

    /// <summary>
    /// Transform the line using a Transformation matrix.
    /// </summary>
    /// <param name="xform">Transform to apply to this line.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool Transform(Transform xform)
    {
      return UnsafeNativeMethods.ON_Line_Transform(ref this, ref xform);
    }

    /// <summary>
    /// Create a nurbs curve representation of this line. 
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
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
    /// <returns>True on success, false on failure.</returns>
    public bool ExtendThroughBox(Box box, double additionalLength)
    {
      if (!IsValid) { return false; }
      if (!box.IsValid) { return false; }

      return ExtendThroughPointSet(box.GetCorners(), additionalLength);
    }
    internal bool ExtendThroughPointSet(IEnumerable<Point3d> pts, double additionalLength)
    {
      double t0 = double.MaxValue;
      double t1 = double.MinValue;

      foreach (Point3d pt in pts)
      {
        double t = ClosestParameter(pt);
        t0 = Math.Min(t0, t);
        t1 = Math.Max(t1, t);
      }

      Point3d A = PointAt(t0);
      Point3d B = PointAt(t1);

      m_from = A;
      m_to = B;

      return Extend(additionalLength, additionalLength);
    }

    /// <summary>
    /// Get a plane that contains the line. The origin of the plane is at the start of the line.
    /// If possible, a plane parallel to the world xy, yz, or zx plane is returned.
    /// </summary>
    /// <param name="plane"></param>
    /// <returns>true on success</returns>
    public bool TryGetPlane(out Plane plane)
    {
      plane = new Plane();
      return UnsafeNativeMethods.ON_Line_InPlane(ref this, ref plane);
    }
    #endregion
  }
}