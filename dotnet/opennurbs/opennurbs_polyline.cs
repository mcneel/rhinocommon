using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Threading;

using Rhino.Geometry;
using Rhino;

namespace Rhino.Geometry
{
  /// <summary>
  /// An ordered set of points connected by linear segments
  /// </summary>
  public class Polyline : Rhino.Collections.Point3dList
  {
    #region constructors
    /// <summary>
    /// Create a new empty polyline.
    /// </summary>
    public Polyline()
      : base()
    {
    }
    /// <summary>
    /// Create a new empty polyline with an initial capacity.
    /// </summary>
    /// <param name="initialCapacity">Number of vertices this polyline can contain without resizing.</param>
    public Polyline(int initialCapacity)
      : base(initialCapacity)
    {
    }
    /// <summary>
    /// Create a new polyline from a collection of points.
    /// </summary>
    /// <param name="collection">Points to copy into the local vertex array.</param>
    public Polyline(IEnumerable<Point3d> collection)
      : base(collection)
    {
    }

    //David: I just copied this function from Point3dList...
    internal static Polyline PolyLineFromNativeArray(Runtime.InteropWrappers.SimpleArrayPoint3d pts)
    {
      if (null == pts)
        return null;
      int count = pts.Count;
      Polyline list = new Polyline(count);
      if (count > 0)
      {
        IntPtr pNativeArray = pts.ConstPointer();
        UnsafeNativeMethods.ON_3dPointArray_CopyValues(pNativeArray, list.m_items);
        list.m_size = count;
      }
      return list;
    }
    #endregion

    #region Properties
    /// <summary>
    /// Gets a value that indicates whether or not this polyline is valid. 
    /// Valid polylines have at least one segment, no Invalid points and no zero length segments. 
    /// Closed Polylines with only two segments are also not considered valid.
    /// </summary>
    public bool IsValid
    {
      get
      {
        if (m_size < 2) { return false; }
        if (!m_items[0].IsValid) { return false; }

        for (int i = 1; i < m_size; i++)
        {
          if (!m_items[i].IsValid) { return false; }
          if (m_items[i] == m_items[i - 1]) { return false; }
        }

        if (m_size < 4) { if (IsClosed) { return false; } }

        return true;
      }
    }

    /// <summary>
    /// Gets the number of segments for this polyline.
    /// </summary>
    public int SegmentCount
    {
      get
      {
        return Math.Max(m_size - 1, 0);
      }
    }

    /// <summary>
    /// Gets whether or not this polyline is closed. 
    /// The polyline is considered to be closed if the start-point is 
    /// identical to the end-point.
    /// </summary>
    /// <seealso cref="IsClosedWithinTolerance"/>
    public bool IsClosed
    {
      get
      {
        if (m_size <= 2) { return false; }
        return First == Last;
      }
    }

    /// <summary>
    /// Test to see whether or not the polyline is closed to within tolerance.
    /// </summary>
    /// <param name="tolerance">If the distance between the start and end point of the polyline 
    /// is less than tolerance, the polyline is considered to be closed.</param>
    /// <returns>True if the polyline is closed to within tolerance, false if not.</returns>
    public bool IsClosedWithinTolerance(double tolerance)
    {
      if (m_size <= 2) { return false; }

      if (tolerance <= 0.0)
      {
        int rc = UnsafeNativeMethods.ONC_ComparePoint(3, false, First, Last);
        return (rc == 0);
      }
      else
      {
        return (First.DistanceTo(Last) <= tolerance);
      }
    }

    /// <summary>
    /// Gets the total length of the polyline.
    /// </summary>
    public double Length
    {
      get
      {
        if (m_size < 2) { return 0.0; }

        double L = 0.0;

        for (int i = 0; i < (m_size - 1); i++)
        {
          L += this[i].DistanceTo(this[i + 1]);
        }

        return L;
      }
    }
    #endregion

    #region Methods
    /// <summary>
    /// Gets the line segment at the given index.
    /// </summary>
    /// <param name="index">Index of segment to retrieve.</param>
    /// <returns>Line segment at index or Line.Unset on failure.</returns>
    public Line SegmentAt(int index)
    {
      if (index < 0) { return Line.Unset; }
      if (index >= Count - 1) { return Line.Unset; }

      return new Line(m_items[index], m_items[index + 1]);
    }

    /// <summary>
    /// Gets the point on the polyline at the given parameter. 
    /// The integer part of the parameter indicates the index of the segment.
    /// </summary>
    /// <param name="t">Polyline parameter.</param>
    /// <returns>The point on the polyline at t.</returns>
    public Point3d PointAt(double t)
    {
      int count = Count;
      if (count < 1) { return Point3d.Origin; }
      if (count == 1) { return this[0]; }

      double floor = Math.Floor(t);

      int idx = (int)floor;
      if (idx < 0) { return this[0]; }
      if (idx >= count - 1) { return this[count - 1]; }

      t -= floor;
      if (t <= 0.0)
      {
        return this[idx];
      }
      else if (t >= 1.0)
      {
        return this[idx + 1];
      }

      Point3d A = this[idx];
      Point3d B = this[idx + 1];

      double s = 1.0 - t;
      return new Point3d((A.m_x == B.m_x) ? A.m_x : s * A.m_x + t * B.m_x,
                         (A.m_y == B.m_y) ? A.m_y : s * A.m_y + t * B.m_y,
                         (A.m_z == B.m_z) ? A.m_z : s * A.m_z + t * B.m_z);
    }

    /// <summary>
    /// Gets the unit tangent vector along the polyline at the given parameter. 
    /// The integer part of the parameter indicates the index of the segment.
    /// </summary>
    /// <param name="t">Polyline parameter.</param>
    /// <returns>The tangent along the polyline at t.</returns>
    public Vector3d TangentAt(double t)
    {
      int count = Count;
      if (count < 2) { return Vector3d.Zero; }

      int segment_index = (int)Math.Floor(t);
      if (segment_index < 0)
      {
        segment_index = 0;
      }
      else if (segment_index > count - 2)
      {
        segment_index = count - 2;
      }

      Vector3d tangent = m_items[segment_index + 1] - m_items[segment_index];
      tangent.Unitize();

      return tangent;
    }

    /// <summary>
    /// Create a polyline out of a parameter subdomain.
    /// </summary>
    /// <param name="domain">The subdomain of the polyline. 
    /// The integer part of the domain parameters indicate the index of the segment.</param>
    /// <returns>The polyline as defined by the subdomain, or null on failure.</returns>
    public Polyline Trim(Interval domain)
    {
      int count = Count;
      int N = count - 1;

      // Polyline parameters
      double t0 = domain.Min;
      double t1 = domain.Max;

      // Segment indices
      int si0 = (int)Math.Floor(t0);
      int si1 = (int)Math.Floor(t1);

      // Segment parameters
      double st0 = t0 - si0;
      double st1 = t1 - si1;
      if (st0 < 0.0) { st0 = 0.0; }
      if (st0 >= 1.0) { si0++; st0 = 0.0; }
      if (st1 < 0.0) { st1 = 0.0; }
      if (st1 >= 1.0) { si1++; st1 = 0.0; }

      // Limit to polyline domain.
      if (si0 < 0) { si0 = 0; st0 = 0.0; }
      if (si0 >= N) { si0 = N; st0 = 0.0; }
      if (si1 < 0) { si1 = 0; st1 = 0.0; }
      if (si1 >= N) { si1 = N; st1 = 0.0; }

      // Build trimmed polyline.
      Polyline rc = new Polyline();
      rc.Add(PointAt(t0));
      for (int i = si0 + 1; i <= si1; i++)
      {
        rc.Add(m_items[i]);
      }
      if (st1 > 0.0) { rc.Add(PointAt(t1)); }
      return rc;
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead.")]
    public Point3d ClosestPointTo(Point3d testPoint)
    {
      return ClosestPoint(testPoint);
    }
    /// <summary>
    /// Gets the point on the polyline which is closest to a test-point.
    /// </summary>
    /// <param name="testPoint">Point to approximate.</param>
    /// <returns>The point on the polyline closest to testPoint.</returns>
    public Point3d ClosestPoint(Point3d testPoint)
    {
      if (Count == 0) { return Point3d.Unset; }
      double t = ClosestParameter(testPoint);
      return PointAt(t);
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
    /// Gets the parameter along the polyline which is closest to a test-point.
    /// </summary>
    /// <param name="testPoint">Point to approximate.</param>
    /// <returns>The parameter along the polyline closest to testPoint.</returns>
    public double ClosestParameter(Point3d testPoint)
    {
      int count = Count;
      if (count < 2) { return 0.0; }

      int s_min = 0;
      double t_min = 0.0;
      double d_min = double.MaxValue;

      for (int i = 0; i < count - 1; i++)
      {
        Line seg = new Line(this[i], this[i + 1]);
        double d;
        double t;

        if (seg.Direction.IsTiny(1e-32))
        {
          t = 0.0;
          d = this[i].DistanceTo(testPoint);
        }
        else
        {
          t = seg.ClosestParameter(testPoint);
          if (t <= 0.0) { t = 0.0; }
          else if (t > 1.0) { t = 1.0; }
          d = seg.PointAt(t).DistanceTo(testPoint);
        }

        if (d < d_min)
        {
          d_min = d;
          t_min = t;
          s_min = i;
        }
      }

      return s_min + t_min;
    }

    /// <summary>
    /// Get an array of line segments that make up the entire polyline.
    /// </summary>
    /// <returns>An array of line segments or null if the polyline contains fewer than 2 points.</returns>
    public Line[] GetSegments()
    {
      if (m_size < 2) { return null; }

      Line[] segments = new Line[m_size - 1];

      for (int i = 0; i < (m_size - 1); i++)
      {
        segments[i] = new Line(this[i], this[i + 1]);
      }

      return segments;
    }

    /// <summary>
    /// Create a nurbs curve representation of this polyline.
    /// </summary>
    /// <returns>A Nurbs curve shaped like this polyline or null on failure.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      if (m_size < 2) { return null; }
      PolylineCurve pl_crv = new PolylineCurve(this);

      return pl_crv.ToNurbsCurve();
    }

    /// <summary>
    /// Remove all points that are closer than tolerance to the previous point. 
    /// Start and end points are left intact.
    /// </summary>
    /// <param name="tolerance">Vertices closer together than tolerance will be removed.</param>
    /// <returns>Number of points (and segments) removed.</returns>
    public int DeleteShortSegments(double tolerance)
    {
      //David: this code is untested

      int count = m_size;
      if (count < 3) { return 0; }

      // Create an inclusion map
      bool[] map = new bool[count];
      // Always include the first and last point.
      map[0] = true;
      map[count - 1] = true;

      // Iterate over all internal points.
      int j = 0;
      for (int i = 1; i < (count - 1); i++)
      {
        if (m_items[i].DistanceTo(m_items[j]) <= tolerance)
        {
          // The distance between this point and the last added point 
          // is less than tolerance. We do not include it.
          map[i] = false;
        }
        else
        {
          // The distance between this point and the last added point 
          // is more than the tolerance. Append this point to the clean list.
          j = i;
          map[i] = true;
        }
      }

      // Iterate backwards over the clean points, in an attempt to try and find
      // all the points too close to the end of the curve.
      for (int i = count - 2; i > 0; i--)
      {
        if (map[i])
        {
          if (m_items[i].DistanceTo(m_items[count - 1]) <= tolerance)
          {
            // Point is too close to the end of the polyline, disable it.
            map[i] = false;
          }
          else
          {
            // Point is further than tolerance from the end of the polyline, 
            // we can safely exhale.
            break;
          }
        }
      }

      // Create a new array of points
      Point3d[] pts = new Point3d[count];
      int N = 0;

      for (int i = 0; i < count; i++)
      {
        if (map[i])
        {
          pts[N] = m_items[i];
          N++;
        }
      }

      m_items = pts;
      m_size = N;

      return count - N;
    }

    /// <summary>
    /// Collapse all segments until none are shorter than tolerance. 
    /// This function is significantly slower than DeleteShortSegments, 
    /// since it recursively operates on the shortest segment. 
    /// When a segment is collapsed the end-points are placed in the center of the segment.
    /// </summary>
    /// <param name="tolerance">Tolerance to use during collapsing.</param>
    /// <returns>The number of segments that were collapsed.</returns>
    /// <seealso cref="DeleteShortSegments"/>
    public int CollapseShortSegments(double tolerance)
    {
      if (m_size < 3) { return 0; }

      int count0 = m_size;
      List<Point3d> P = new List<Point3d>(this.ToArray());
      List<double> L = new List<double>(m_size);

      // Build the Segment length list.
      for (int i = 0; i < m_size - 1; i++)
      { L.Add(m_items[i].DistanceTo(m_items[i + 1])); }

      while (true)
      {
        // Abort if we've collapsed all but the last segment.
        if (L.Count < 2) { break; }

        // Find the shortest segment and abort if it is longer than tolerance.
        int index = Collapse_ShortestEdgeIndex(L);
        if (L[index] > tolerance) { break; }

        // Collapse the shortest segment.
        Collapse_CollapseSegment(index, L, P);
      }

      // Now we have the new points for this polyline in P.
      // Copy the data into my fields.
      m_items = P.ToArray();
      m_size = P.Count;

      return count0 - m_size;
    }
    private int Collapse_ShortestEdgeIndex(List<double> L)
    {
      if (L.Count < 2) { return L.Count - 1; }

      int i_min = 0;
      double d_min = L[0];

      for (int i = 1; i < L.Count; i++)
      {
        if (L[i] < d_min)
        {
          d_min = L[i];
          i_min = i;
        }
      }
      return i_min;
    }
    private void Collapse_CollapseSegment(int segment_index, List<double> L, List<Point3d> P)
    {
      if (segment_index == 0)
      {
        // Collapse first segment
        P.RemoveAt(1);
        L.RemoveAt(0);
        L[0] = P[0].DistanceTo(P[1]);
      }
      else if (segment_index == L.Count - 1)
      {
        // Collapse last segment
        P.RemoveAt(P.Count - 2);
        L.RemoveAt(L.Count - 1);
        L[L.Count - 1] = P[P.Count - 1].DistanceTo(P[P.Count - 2]);
      }
      else
      {
        // Collapse interior segment
        Point3d A = P[segment_index];
        Point3d B = P[segment_index + 1];
        Point3d M = new Point3d(0.5 * (A.m_x + B.m_x),
                                0.5 * (A.m_y + B.m_y),
                                0.5 * (A.m_z + B.m_z));

        P[segment_index] = M;
        P.RemoveAt(segment_index + 1);
        L.RemoveAt(segment_index);
        L[segment_index - 1] = P[segment_index].DistanceTo(P[segment_index - 1]);
        L[segment_index + 0] = P[segment_index].DistanceTo(P[segment_index + 1]);
      }
    }

    /// <summary>
    /// Create a reduction of this polyline by recursively removing the least significant segments. 
    /// </summary>
    /// <param name="tolerance">Tolerance for reduction. Whenever a vertex of the polyline is more 
    /// significant than tolerance, it will be included in the reduction.</param>
    /// <returns>The number of vertices that disappeared due to reduction.</returns>
    public int ReduceSegments(double tolerance)
    {
      if (m_size < 3) { return 0; }
      if (m_size < 5 && IsClosed) { return 0; }

      // Create a new vertex map. 
      // We assume all vertices except the end points are insignificant.
      bool[] vertex_map = new bool[m_size];
      vertex_map[0] = true;
      vertex_map[m_size - 1] = true;

      // Perform reduction logic.
      Reduce_RecursiveComponent(m_items, vertex_map, tolerance, 0, m_size - 1);

      // Create new vertex array.
      int N = 0;
      for (int i = 0; i < vertex_map.Length; i++)
      {
        if (vertex_map[i])
        {
          m_items[N] = m_items[i];
          N++;
        }
      }

      // Hmm, we probably shouldn't create an empty polyline, but I don't know what else to do.
      if (N < 2)
      {
        Clear();
        return vertex_map.Length;
      }

      m_size = N;

      return vertex_map.Length - m_size;
    }
    private void Reduce_RecursiveComponent(Point3d[] P, bool[] vertex_map, double tolerance, int A, int B)
    {
      //Abort if there is nothing left to collapse.
      if (B <= (A + 1)) { return; }

      // Create basis segment.E
      Line segment = new Line(P[A], P[B]);

      double max_d = 0.0;
      double loc_d = 0.0;
      int max_i = A;

      // Iterate over remaining points in subset and find furthest point.
      for (int i = A + 1; i < B; i++)
      {
        loc_d = segment.MinimumDistanceTo(P[i]);
        if (loc_d > max_d)
        {
          max_d = loc_d;
          max_i = i;
        }
      }

      if (max_d > tolerance)
      {
        vertex_map[max_i] = true;

        Reduce_RecursiveComponent(P, vertex_map, tolerance, A, max_i);
        Reduce_RecursiveComponent(P, vertex_map, tolerance, max_i, B);
      }
    }

    /// <summary>
    /// Smooth the polyline segments by averaging adjacent vertices. 
    /// Smoothing requires a polyline with exclusively valid vertices.
    /// </summary>
    /// <param name="amount">Amount to smooth. Zero equals no smoothing, one equals complete smoothing.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool Smooth(double amount)
    {
      int count = this.Count;
      if (count < 3) { return false; }

      int N = count - 1;
      amount *= 0.5;

      Point3d[] v = new Point3d[count];

      if (IsClosed)
      {
        // Closed polyline, smooth the end-points
        v[0] = Smooth(m_items[N - 1], m_items[0], m_items[1], amount);
        v[N] = v[0];
      }
      else
      {
        // Open polyline, copy the end points without smoothing.
        v[0] = m_items[0];
        v[N] = m_items[N];
      }

      // Iterate over the internal vertices
      for (int i = 1; i < N; i++)
      {
        v[i] = Smooth(m_items[i - 1], m_items[i], m_items[i + 1], amount);
      }

      // Overwrite internal array.
      m_items = v;

      return true;
    }
    private Point3d Smooth(Point3d v0, Point3d v1, Point3d v2, double amount)
    {
      double x = 0.5 * (v0.m_x + v2.m_x);
      double y = 0.5 * (v0.m_y + v2.m_y);
      double z = 0.5 * (v0.m_z + v2.m_z);

      double bx = (x == v1.m_x) ? x : v1.m_x + amount * (x - v1.m_x);
      double by = (y == v1.m_y) ? y : v1.m_y + amount * (y - v1.m_y);
      double bz = (z == v1.m_z) ? z : v1.m_z + amount * (z - v1.m_z);

      return new Point3d(bx, by, bz);
    }

    /// <summary>
    /// Break this polyline into sections at sharp kinks. 
    /// Closed polylines will also be broken at the first and last vertex.
    /// </summary>
    /// <param name="angle">Angle (in radians) between adjacent segments for a break to occur.</param>
    /// <returns>An array of polyline segments or null on error.</returns>
    public Polyline[] BreakAtAngles(double angle)
    {
      int count = Count;
      if (count == 0) { return null; }
      if (count <= 2) { return new Polyline[] { new Polyline(this) }; }

      bool[] frac = new bool[count];

      for (int i = 1; i < (count - 1); i++)
      {
        Point3d p0 = this[i - 1];
        Point3d p1 = this[i];
        Point3d p2 = this[i + 1];

        Vector3d t0 = p0 - p1;
        Vector3d t1 = p2 - p1;

        if (!t0.IsZero && !t1.IsZero)
        {
          frac[i] = (Vector3d.VectorAngle(t0, t1) <= angle);
        }
      }

      List<Polyline> segments = new List<Polyline>();
      Polyline segment = new Polyline();

      for (int i = 0; i < count; i++)
      {
        segment.Add(this[i]);

        if (i == (count - 1))
        {
          segments.Add(segment);
          segment = null;
          break;
        }
        else
        {
          if (frac[i])
          {
            segments.Add(segment);
            segment = new Polyline();
            segment.Add(this[i]);
          }
        }
      }

      return segments.ToArray();
    }
    #endregion
  }
}