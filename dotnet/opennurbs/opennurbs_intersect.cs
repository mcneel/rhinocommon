using System;
using System.Collections.Generic;

namespace Rhino.Geometry.Intersect
{
  /// <summary>
  /// Provides static methods for the computation of intersections, projections, sections and similar.
  /// </summary>
  public static class Intersection
  {
    #region analytic
    /// <summary>
    /// Intersects two lines.
    /// </summary>
    /// <param name="lineA">First line for intersection.</param>
    /// <param name="lineB">Second line for intersection.</param>
    /// <param name="a">
    /// Parameter on lineA that is closest to LineB. 
    /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
    /// </param>
    /// <param name="b">
    /// Parameter on lineB that is closest to LineA. 
    /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
    /// </param>
    /// <param name="tolerance">
    /// If tolerance > 0.0, then an intersection is reported only if the distance between the points is &lt;= tolerance. 
    /// If tolerance &lt;= 0.0, then the closest point between the lines is reported.
    /// </param>
    /// <param name="finiteSegments">
    /// If true, the input lines are treated as finite segments. 
    /// If false, the input lines are treated as infinite lines.
    /// </param>
    /// <returns>
    /// true if a closest point can be calculated and the result passes the tolerance parameter test; otherwise false.
    /// </returns>
    /// <remarks>
    /// If the lines are exactly parallel, meaning the system of equations used to find a and b 
    /// has no numerical solution, then false is returned. If the lines are nearly parallel, which 
    /// is often numerically true even if you think the lines look exactly parallel, then the 
    /// closest points are found and true is returned. So, if you care about weeding out "parallel" 
    /// lines, then you need to do something like the following:
    /// <code lang="cs">
    /// bool rc = Intersect.LineLine(lineA, lineB, out a, out b, tolerance, segments);
    /// if (rc)
    /// {
    ///   double angle_tol = RhinoMath.ToRadians(1.0); // or whatever
    ///   double parallel_tol = Math.Cos(angle_tol);
    ///   if ( Math.Abs(lineA.UnitTangent * lineB.UnitTangent) >= parallel_tol )
    ///   {
    ///     ... do whatever you think is appropriate
    ///   }
    /// }
    /// </code>
    /// <code lang="vb">
    /// Dim rc As Boolean = Intersect.LineLine(lineA, lineB, a, b, tolerance, segments)
    /// If (rc) Then
    ///   Dim angle_tol As Double = RhinoMath.ToRadians(1.0) 'or whatever
    ///   Dim parallel_tolerance As Double = Math.Cos(angle_tol)
    ///   If (Math.Abs(lineA.UnitTangent * lineB.UnitTangent) >= parallel_tolerance) Then
    ///     ... do whatever you think is appropriate
    ///   End If
    /// End If
    /// </code>
    /// </remarks>
    public static bool LineLine(Line lineA, Line lineB, out double a, out double b, double tolerance, bool finiteSegments)
    {
      bool rc = LineLine(lineA, lineB, out a, out b);
      if (rc)
      {
        if (finiteSegments)
        {
          if (a < 0.0)
            a = 0.0;
          else if (a > 1.0)
            a = 1.0;
          if (b < 0.0)
            b = 0.0;
          else if (b > 1.0)
            b = 1.0;
        }
        if (tolerance > 0.0)
        {
          rc = (lineA.PointAt(a).DistanceTo(lineB.PointAt(b)) <= tolerance);
        }
      }
      return rc;
    }
    /// <summary>
    /// Finds the closest point between two infinite lines.
    /// </summary>
    /// <param name="lineA">First line.</param>
    /// <param name="lineB">Second line.</param>
    /// <param name="a">
    /// Parameter on lineA that is closest to lineB. 
    /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
    /// </param>
    /// <param name="b">
    /// Parameter on lineB that is closest to lineA. 
    /// The shortest distance between the lines is the chord from lineA.PointAt(a) to lineB.PointAt(b)
    /// </param>
    /// <returns>
    /// true if points are found and false if the lines are numerically parallel. 
    /// Numerically parallel means the 2x2 matrix:
    /// <para>+AoA  -AoB</para>
    /// <para>-AoB  +BoB</para>
    /// is numerically singular, where A = (lineA.To - lineA.From) and B = (lineB.To-lineB.From)
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_intersectlines.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectlines.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectlines.py' lang='py'/>
    /// </example>
    public static bool LineLine(Line lineA, Line lineB, out double a, out double b)
    {
      a = 0; b = 0;
      return UnsafeNativeMethods.ON_Intersect_LineLine(ref lineA, ref lineB, ref a, ref b);
    }
    /// <summary>
    /// Intersects a line and a plane. This function only returns true if the 
    /// intersection result is a single point (i.e. if the line is coincident with 
    /// the plane then no intersection is assumed).
    /// </summary>
    /// <param name="line">Line for intersection.</param>
    /// <param name="plane">Plane to intersect.</param>
    /// <param name="lineParameter">Parameter on line where intersection occurs. 
    /// If the parameter is not within the {0, 1} Interval then the finite segment 
    /// does not intersect the plane.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool LinePlane(Line line, Plane plane, out double lineParameter)
    {
      lineParameter = 0.0;
      return UnsafeNativeMethods.ON_Intersect_LinePlane(ref line, ref plane, ref lineParameter);
    }
    /// <summary>
    /// Intersects two planes and return the intersection line. If the planes are 
    /// parallel or coincident, no intersection is assumed.
    /// </summary>
    /// <param name="planeA">First plane for intersection.</param>
    /// <param name="planeB">Second plane for intersection.</param>
    /// <param name="intersectionLine">If this function returns true, 
    /// the intersectionLine parameter will return the line where the planes intersect.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool PlanePlane(Plane planeA, Plane planeB, out Line intersectionLine)
    {
      intersectionLine = new Line();
      return UnsafeNativeMethods.ON_Intersect_PlanePlane(ref planeA, ref planeB, ref intersectionLine);
    }
    /// <summary>
    /// Intersects three planes to find the single point they all share.
    /// </summary>
    /// <param name="planeA">First plane for intersection.</param>
    /// <param name="planeB">Second plane for intersection.</param>
    /// <param name="planeC">Third plane for intersection.</param>
    /// <param name="intersectionPoint">Point where all three planes converge.</param>
    /// <returns>true on success, false on failure. If at least two out of the three planes 
    /// are parallel or coincident, failure is assumed.</returns>
    public static bool PlanePlanePlane(Plane planeA, Plane planeB, Plane planeC, out Point3d intersectionPoint)
    {
      intersectionPoint = new Point3d();
      return UnsafeNativeMethods.ON_Intersect_PlanePlanePlane(ref planeA, ref planeB, ref planeC, ref intersectionPoint);
    }

    /// <summary>
    /// Intersects a plane with a circle using exact calculations.
    /// </summary>
    /// <param name="plane">Plane to intersect.</param>
    /// <param name="circle">Circe to intersect.</param>
    /// <param name="firstCircleParameter">First intersection parameter on circle if successful or RhinoMath.UnsetValue if not.</param>
    /// <param name="secondCircleParameter">Second intersection parameter on circle if successful or RhinoMath.UnsetValue if not.</param>
    /// <returns>The type of intersection that occured.</returns>
    public static PlaneCircleIntersection PlaneCircle(Plane plane, Circle circle, out double firstCircleParameter, out double secondCircleParameter)
    {
      firstCircleParameter = RhinoMath.UnsetValue;
      secondCircleParameter = RhinoMath.UnsetValue;

      if (plane.ZAxis.IsParallelTo(circle.Plane.ZAxis, RhinoMath.ZeroTolerance * Math.PI) != 0)
      {
        if (Math.Abs(plane.DistanceTo(circle.Center)) < RhinoMath.ZeroTolerance)
          return PlaneCircleIntersection.Coincident;
        return PlaneCircleIntersection.Parallel;
      }

      Line L;

      //At this point, the PlanePlane should never fail since I already checked for parallellillity.
      if (!PlanePlane(plane, circle.Plane, out L)) { return PlaneCircleIntersection.Parallel; }

      double Lt = L.ClosestParameter(circle.Center);
      Point3d Lp = L.PointAt(Lt);

      double d = circle.Center.DistanceTo(Lp);

      //If circle radius equals the projection distance, we have a tangent intersection.
      if (Math.Abs(d - circle.Radius) < RhinoMath.ZeroTolerance)
      {
        circle.ClosestParameter(Lp, out firstCircleParameter);
        secondCircleParameter = firstCircleParameter;
        return PlaneCircleIntersection.Tangent;
      }

      //If circle radius too small to get an intersection, then abort.
      if (d > circle.Radius) { return PlaneCircleIntersection.None; }

      double offset = Math.Sqrt((circle.Radius * circle.Radius) - (d * d));
      Vector3d dir = offset * L.UnitTangent;

      if (!circle.ClosestParameter(Lp + dir, out firstCircleParameter)) { return PlaneCircleIntersection.None; }
      if (!circle.ClosestParameter(Lp - dir, out secondCircleParameter)) { return PlaneCircleIntersection.None; }

      return PlaneCircleIntersection.Secant;
    }
    /// <summary>
    /// Intersects a plane with a sphere using exact calculations.
    /// </summary>
    /// <param name="plane">Plane to intersect.</param>
    /// <param name="sphere">Sphere to intersect.</param>
    /// <param name="intersectionCircle">Intersection result.</param>
    /// <returns>If <see cref="PlaneSphereIntersection.None"/> is returned, the intersectionCircle has a radius of zero and the center point 
    /// is the point on the plane closest to the sphere.</returns>
    public static PlaneSphereIntersection PlaneSphere(Plane plane, Sphere sphere, out Circle intersectionCircle)
    {
      intersectionCircle = new Circle();
      int rc = UnsafeNativeMethods.ON_Intersect_PlaneSphere(ref plane, ref sphere, ref intersectionCircle);

      return (PlaneSphereIntersection)rc;
    }
    /// <summary>
    /// Intersects a line with a circle using exact calculations.
    /// </summary>
    /// <param name="line">Line for intersection.</param>
    /// <param name="circle">Circle for intersection.</param>
    /// <param name="t1">Parameter on line for first intersection.</param>
    /// <param name="point1">Point on circle closest to first intersection.</param>
    /// <param name="t2">Parameter on line for second intersection.</param>
    /// <param name="point2">Point on circle closest to second intersection.</param>
    /// <returns>
    /// If <see cref="LineCircleIntersection.Single"/> is returned, only t1 and point1 will have valid values. 
    /// If <see cref="LineCircleIntersection.Multiple"/> is returned, t2 and point2 will also be filled out.
    /// </returns>
    public static LineCircleIntersection LineCircle(Line line, Circle circle, out double t1, out Point3d point1, out double t2, out Point3d point2)
    {
      t1 = 0.0;
      t2 = 0.0;
      point1 = new Point3d();
      point2 = new Point3d();

      if (!line.IsValid || !circle.IsValid) { return LineCircleIntersection.None; }

      int rc = UnsafeNativeMethods.ON_Intersect_LineCircle(ref line, ref circle, ref t1, ref point1, ref t2, ref point2);
      return (LineCircleIntersection)rc;
    }
    /// <summary>
    /// Intersects a line with a sphere using exact calculations.
    /// </summary>
    /// <param name="line">Line for intersection.</param>
    /// <param name="sphere">Sphere for intersection.</param>
    /// <param name="intersectionPoint1">First intersection point.</param>
    /// <param name="intersectionPoint2">Second intersection point.</param>
    /// <returns>If <see cref="LineSphereIntersection.None"/> is returned, the first point is the point on the line closest to the sphere and 
    /// the second point is the point on the sphere closest to the line. 
    /// If <see cref="LineSphereIntersection.Single"/> is returned, the first point is the point on the line and the second point is the 
    /// same point on the sphere.</returns>
    public static LineSphereIntersection LineSphere(Line line, Sphere sphere, out Point3d intersectionPoint1, out Point3d intersectionPoint2)
    {
      intersectionPoint1 = new Point3d();
      intersectionPoint2 = new Point3d();
      int rc = UnsafeNativeMethods.ON_Intersect_LineSphere(ref line, ref sphere, ref intersectionPoint1, ref intersectionPoint2);

      return (LineSphereIntersection)rc;
    }
    /// <summary>
    /// Intersects a line with a cylinder using exact calculations.
    /// </summary>
    /// <param name="line">Line for intersection.</param>
    /// <param name="cylinder">Cylinder for intersection.</param>
    /// <param name="intersectionPoint1">First intersection point.</param>
    /// <param name="intersectionPoint2">Second intersection point.</param>
    /// <returns>If None is returned, the first point is the point on the line closest
    /// to the cylinder and the second point is the point on the cylinder closest to
    /// the line. 
    /// <para>If <see cref="LineCylinderIntersection.Single"/> is returned, the first point
    /// is the point on the line and the second point is the  same point on the
    /// cylinder.</para></returns>
    public static LineCylinderIntersection LineCylinder(Line line, Cylinder cylinder, out Point3d intersectionPoint1, out Point3d intersectionPoint2)
    {
      intersectionPoint1 = new Point3d();
      intersectionPoint2 = new Point3d();
      int rc = UnsafeNativeMethods.ON_Intersect_LineCylinder(ref line, ref cylinder, ref intersectionPoint1, ref intersectionPoint2);

      return (LineCylinderIntersection)rc;
    }
    /// <summary>
    /// Intersects two spheres using exact calculations.
    /// </summary>
    /// <param name="sphereA">First sphere to intersect.</param>
    /// <param name="sphereB">Second sphere to intersect.</param>
    /// <param name="intersectionCircle">
    /// If intersection is a point, then that point will be the center, radius 0.
    /// </param>
    /// <returns>
    /// The intersection type.
    /// </returns>
    public static SphereSphereIntersection SphereSphere(Sphere sphereA, Sphere sphereB, out Circle intersectionCircle)
    {
      intersectionCircle = new Circle();
      int rc = UnsafeNativeMethods.ON_Intersect_SphereSphere(ref sphereA, ref sphereB, ref intersectionCircle);

      if (rc <= 0 || rc > 3)
      {
        return SphereSphereIntersection.None;
      }

      return (SphereSphereIntersection)rc;
    }
    /// <summary>
    /// Intersects an infinite line and an axis aligned bounding box.
    /// </summary>
    /// <param name="box">BoundingBox to intersect.</param>
    /// <param name="line">Line for intersection.</param>
    /// <param name="tolerance">
    /// If tolerance &gt; 0.0, then the intersection is performed against a box 
    /// that has each side moved out by tolerance.
    /// </param>
    /// <param name="lineParameters">
    /// The chord from line.PointAt(lineParameters.T0) to line.PointAt(lineParameters.T1) is the intersection.
    /// </param>
    /// <returns>true if the line intersects the box, false if no intersection occurs.</returns>
    public static bool LineBox(Line line, BoundingBox box, double tolerance, out Interval lineParameters)
    {
      lineParameters = new Interval();
      return UnsafeNativeMethods.ON_Intersect_BoundingBoxLine(ref box, ref line, tolerance, ref lineParameters);
    }
    /// <summary>
    /// Intersects an infinite line with a box volume.
    /// </summary>
    /// <param name="box">Box to intersect.</param>
    /// <param name="line">Line for intersection.</param>
    /// <param name="tolerance">
    /// If tolerance &gt; 0.0, then the intersection is performed against a box 
    /// that has each side moved out by tolerance.
    /// </param>
    /// <param name="lineParameters">
    /// The chord from line.PointAt(lineParameters.T0) to line.PointAt(lineParameters.T1) is the intersection.
    /// </param>
    /// <returns>true if the line intersects the box, false if no intersection occurs.</returns>
    public static bool LineBox(Line line, Box box, double tolerance, out Interval lineParameters)
    {
      //David: test this!
      BoundingBox bbox = new BoundingBox(new Point3d(box.X.Min, box.Y.Min, box.Z.Min),
                                         new Point3d(box.X.Max, box.Y.Max, box.Z.Max));
      Transform xform = Transform.ChangeBasis(Plane.WorldXY, box.Plane);
      line.Transform(xform);

      return LineBox(line, bbox, tolerance, out lineParameters);
    }
    #endregion

    #region sections
#if RHINO_SDK
    /// <summary>
    /// Intersects a curve with an (infinite) plane.
    /// </summary>
    /// <param name="curve">Curve to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <param name="tolerance">Tolerance to use during intersection.</param>
    /// <returns>A list of intersection events or null if no intersections were recorded.</returns>
    public static CurveIntersections CurvePlane(Curve curve, Plane plane, double tolerance)
    {
      if (!plane.IsValid)
        return null;

      // Use dedicated plane intersector in Rhino5
      IntPtr pConstCurve = curve.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Curve_IntersectPlane(pConstCurve, ref plane, tolerance);
      return CurveIntersections.Create(pIntersectArray);
    }

    /// <summary>
    /// Intersects a mesh with an (infinite) plane.
    /// </summary>
    /// <param name="mesh">Mesh to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <returns>An array of polylines describing the intersection loops or null (Nothing in Visual Basic) if no intersections could be found.</returns>
    public static Polyline[] MeshPlane(Mesh mesh, Plane plane)
    {
      Rhino.Collections.RhinoList<Plane> planes = new Rhino.Collections.RhinoList<Plane>(1, plane);
      return MeshPlane(mesh, planes);
    }
    /// <summary>
    /// Intersects a mesh with a collection of (infinite) planes.
    /// </summary>
    /// <param name="mesh">Mesh to intersect.</param>
    /// <param name="planes">Planes to intersect with.</param>
    /// <returns>An array of polylines describing the intersection loops or null (Nothing in Visual Basic) if no intersections could be found.</returns>
    /// <exception cref="ArgumentNullException">If planes is null.</exception>
    public static Polyline[] MeshPlane(Mesh mesh, IEnumerable<Plane> planes)
    {
      if (planes == null) throw new ArgumentNullException("planes");

      Rhino.Collections.RhinoList<Plane> list = planes as Rhino.Collections.RhinoList<Plane> ??
                                                new Rhino.Collections.RhinoList<Plane>(planes);
      if (list.Count < 1)
        return null;

      IntPtr pMesh = mesh.ConstPointer();
      int polylines_created = 0;
      IntPtr pPolys = UnsafeNativeMethods.TL_Intersect_MeshPlanes1(pMesh, list.Count, list.m_items, ref polylines_created);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_Intersect_MeshPlanes2(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_Intersect_MeshPlanes3(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_Intersect_MeshPlanes4(pPolys);

      return rc;
    }

    /// <summary>
    /// Intersects a Brep with an (infinite) plane.
    /// </summary>
    /// <param name="brep">Brep to intersect.</param>
    /// <param name="plane">Plane to intersect with.</param>
    /// <param name="tolerance">Tolerance to use for intersections.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool BrepPlane(Brep brep, Plane plane, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionPoints = null;
      intersectionCurves = null;

      //David sez: replace this logic with the dedicated Geometry/Plane intersector methods in Rhino5.
      if (!plane.IsValid)
        return false;

      PlaneSurface section = ExtendThroughBox(plane, brep.GetBoundingBox(false), 1.0); //should this be 1.0 or 100.0*tolerance?
      bool rc = false;
      if (section != null)
      {
        rc = BrepSurface(brep, section, tolerance, out intersectionCurves, out intersectionPoints);
        section.Dispose();
      }
      return rc;
    }
#endif

    /// <summary>
    /// Utility function for creating a PlaneSurface through a Box.
    /// </summary>
    /// <param name="plane">Plane to extend.</param>
    /// <param name="box">Box to extend through.</param>
    /// <param name="fuzzyness">Box will be inflated by this amount.</param>
    /// <returns>A Plane surface through the box or null.</returns>
    internal static PlaneSurface ExtendThroughBox(Plane plane, BoundingBox box, double fuzzyness)
    {
      if (fuzzyness != 0.0) { box.Inflate(fuzzyness); }

      Point3d[] corners = box.GetCorners();
      int side = 0;
      bool valid = false;

      for (int i = 0; i < corners.Length; i++)
      {
        double d = plane.DistanceTo(corners[i]);
        if (d == 0.0) { continue; }

        if (d < 0.0)
        {
          if (side > 0) { valid = true; break; }
          side = -1;
        }
        else
        {
          if (side < 0) { valid = true; break; }
          side = +1;
        }
      }

      if (!valid) { return null; }

      Interval s, t;
      if (!plane.ExtendThroughBox(box, out s, out t)) { return null; }

      if (s.IsSingleton || t.IsSingleton)
        return null;

      return new PlaneSurface(plane, s, t);
    }
    #endregion

    #region geometric
    /// <summary>
    /// Finds the places where a curve intersects itself. 
    /// </summary>
    /// <param name="curve">Curve for self-intersections.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches itself to within tolerance, 
    /// an intersection is assumed.</param>
    /// <returns>A collection of intersection events.</returns>
    public static CurveIntersections CurveSelf(Curve curve, double tolerance)
    {
      IntPtr pCurve = curve.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSelf(pCurve, tolerance);
      return CurveIntersections.Create(pIntersectArray);
    }
    /// <summary>
    /// Finds the intersections between two curves. 
    /// </summary>
    /// <param name="curveA">First curve for intersection.</param>
    /// <param name="curveB">Second curve for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curves approach each other to within tolerance, 
    /// an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_intersectcurves.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_intersectcurves.cs' lang='cs'/>
    /// <code source='examples\py\ex_intersectcurves.py' lang='py'/>
    /// </example>
    public static CurveIntersections CurveCurve(Curve curveA, Curve curveB, double tolerance, double overlapTolerance)
    {
      IntPtr pCurveA = curveA.ConstPointer();
      IntPtr pCurveB = curveB.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveCurve(pCurveA, pCurveB, tolerance, overlapTolerance);
      return CurveIntersections.Create(pIntersectArray);
    }
    /// <summary>
    /// Intersects a curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, 
    /// an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    public static CurveIntersections CurveSurface(Curve curve, Surface surface, double tolerance, double overlapTolerance)
    {
      IntPtr pCurve = curve.ConstPointer();
      IntPtr pSurface = surface.ConstPointer();
      if (overlapTolerance > 0.0 && overlapTolerance < tolerance)
        overlapTolerance = tolerance;

      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSurface(pCurve, pSurface, tolerance, overlapTolerance);
      return CurveIntersections.Create(pIntersectArray);
    }
    /// <summary>
    /// Intersects a (sub)curve and a surface.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="curveDomain">Domain of surbcurve to take into consideration for Intersections.</param>
    /// <param name="surface">Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance. If the curve approaches the surface to within tolerance, 
    /// an intersection is assumed.</param>
    /// <param name="overlapTolerance">The tolerance with which the curves are tested.</param>
    /// <returns>A collection of intersection events.</returns>
    public static CurveIntersections CurveSurface(Curve curve, Interval curveDomain, Surface surface, double tolerance, double overlapTolerance)
    {
      Interval domain = curve.Domain;
      double t0 = Math.Max(domain.Min, curveDomain.Min);
      double t1 = Math.Min(domain.Max, curveDomain.Max);
      if (t0 >= t1) { return null; }

      IntPtr pCurve = curve.ConstPointer();
      IntPtr pSurface = surface.ConstPointer();
      IntPtr pIntersectArray = UnsafeNativeMethods.ON_Intersect_CurveSurface2(pCurve, pSurface, t0, t1, tolerance, overlapTolerance);
      return CurveIntersections.Create(pIntersectArray);
    }

#if RHINO_SDK
    /// <summary>
    /// Intersects a curve with a Brep. This function returns the 3D points of intersection
    /// and 3D overlap curves. If an error occurs while processing overlap curves, this function 
    /// will return false, but it will still provide partial results.
    /// </summary>
    /// <param name="curve">Curve for intersection.</param>
    /// <param name="brep">Brep for intersection.</param>
    /// <param name="tolerance">Fitting and near miss tolerance.</param>
    /// <param name="overlapCurves">The overlap curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool CurveBrep(Curve curve, Brep brep, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints)
    {
      overlapCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr brepPtr = brep.ConstPointer();

      bool rc = UnsafeNativeMethods.ON_Intersect_CurveBrep(curvePtr, brepPtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        overlapCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();

      return rc;
    }

    /// <summary>
    /// Intersects a curve with a Brep face.
    /// </summary>
    /// <param name="curve">A curve.</param>
    /// <param name="face">A brep face.</param>
    /// <param name="tolerance">Fitting and near miss tolerance.</param>
    /// <param name="overlapCurves">A overlap curves array argument. This out reference is assigned during the call.</param>
    /// <param name="intersectionPoints">A points array argument. This out reference is assigned during the call.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool CurveBrepFace(Curve curve, BrepFace face, double tolerance, out Curve[] overlapCurves, out Point3d[] intersectionPoints)
    {
      overlapCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr curvePtr = curve.ConstPointer();
      IntPtr facePtr = face.ConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoCurveFaceIntersect(curvePtr, facePtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        overlapCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();

      return rc;
    }

    /// <summary>
    /// Intersects two Surfaces.
    /// </summary>
    /// <param name="surfaceA">First Surface for intersection.</param>
    /// <param name="surfaceB">Second Surface for intersection.</param>
    /// <param name="tolerance">Intersection tolerance.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success, false on failure.</returns>
    public static bool SurfaceSurface(Surface surfaceA, Surface surfaceB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr srfPtrA = surfaceA.ConstPointer();
      IntPtr srfPtrB = surfaceB.ConstPointer();

      bool rc = UnsafeNativeMethods.RHC_RhinoIntersectSurfaces( srfPtrA, srfPtrB, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();

      return rc;
    }

    /// <summary>
    /// Intersects two Breps.
    /// </summary>
    /// <param name="brepA">First Brep for intersection.</param>
    /// <param name="brepB">Second Brep for intersection.</param>
    /// <param name="tolerance">Intersection tolerance.</param>
    /// <param name="intersectionCurves">The intersection curves will be returned here.</param>
    /// <param name="intersectionPoints">The intersection points will be returned here.</param>
    /// <returns>true on success; false on failure.</returns>
    public static bool BrepBrep(Brep brepA, Brep brepB, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr brepPtrA = brepA.ConstPointer();
      IntPtr brepPtrB = brepB.ConstPointer();

      bool rc = UnsafeNativeMethods.ON_Intersect_BrepBrep(brepPtrA, brepPtrB, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();

      return rc;
    }

    /// <summary>
    /// Intersects a Brep and a Surface.
    /// </summary>
    /// <param name="brep">A brep to be intersected.</param>
    /// <param name="surface">A surface to be intersected.</param>
    /// <param name="tolerance">A tolerance value.</param>
    /// <param name="intersectionCurves">The intersection curves array argument. This out reference is assigned during the call.</param>
    /// <param name="intersectionPoints">The intersection points array argument. This out reference is assigned during the call.</param>
    /// <returns>true on success; false on failure.</returns>
    public static bool BrepSurface(Brep brep, Surface surface, double tolerance, out Curve[] intersectionCurves, out Point3d[] intersectionPoints)
    {
      intersectionCurves = new Curve[0];
      intersectionPoints = new Point3d[0];

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      Runtime.InteropWrappers.SimpleArrayCurvePointer outputCurves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputCurvesPtr = outputCurves.NonConstPointer();

      IntPtr brepPtr = brep.ConstPointer();
      IntPtr surfacePtr = surface.ConstPointer();

      bool rc = UnsafeNativeMethods.ON_Intersect_BrepSurface(brepPtr, surfacePtr, tolerance, outputCurvesPtr, outputPointsPtr);

      if (rc)
      {
        intersectionCurves = outputCurves.ToNonConstArray();
        intersectionPoints = outputPoints.ToArray();
      }

      outputPoints.Dispose();
      outputCurves.Dispose();

      return rc;
    }
#endif
    
    /// <summary>
    /// Quickly intersects two meshes. Overlaps and near misses are ignored.
    /// </summary>
    /// <param name="meshA">First mesh for intersection.</param>
    /// <param name="meshB">Second mesh for intersection.</param>
    /// <returns>An array of intersection line segments.</returns>
    public static Line[] MeshMeshFast(Mesh meshA, Mesh meshB)
    {
      IntPtr ptrA = meshA.ConstPointer();
      IntPtr ptrB = meshB.ConstPointer();
      Line[] intersectionLines = new Line[0];

      using (Runtime.InteropWrappers.SimpleArrayLine arr = new Runtime.InteropWrappers.SimpleArrayLine())
      {
        IntPtr pLines = arr.NonConstPointer();
        int rc = UnsafeNativeMethods.ON_Mesh_IntersectMesh(ptrA, ptrB, pLines);
        if (rc > 0)
          intersectionLines = arr.ToArray();
      }
      return intersectionLines;
    }

    /// <summary>
    /// Intersects two meshes. Overlaps and near misses are handled.
    /// </summary>
    /// <param name="meshA">First mesh for intersection.</param>
    /// <param name="meshB">Second mesh for intersection.</param>
    /// <param name="tolerance">Intersection tolerance.</param>
    /// <returns>An array of intersection polylines.</returns>
    public static Polyline[] MeshMeshAccurate(Mesh meshA, Mesh meshB, double tolerance)
    {
      IntPtr pMeshA = meshA.ConstPointer();
      IntPtr pMeshB = meshB.ConstPointer();
      int polylines_created = 0;
      IntPtr pPolys = UnsafeNativeMethods.ON_Intersect_MeshMesh1(pMeshA, pMeshB, ref polylines_created, tolerance);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines. We can reuse the meshplane functions
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_Intersect_MeshPlanes2(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_Intersect_MeshPlanes3(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_Intersect_MeshPlanes4(pPolys);

      return rc;
    }

    /// <summary>Finds the first intersection of a ray with a mesh.</summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="ray">A ray to be casted.</param>
    /// <returns>
    /// >= 0.0 parameter along ray if successful.
    /// &lt; 0.0 if no intersection found.
    /// </returns>
    public static double MeshRay(Mesh mesh, Ray3d ray)
    {
      IntPtr pConstMesh = mesh.ConstPointer();
      return UnsafeNativeMethods.ON_Intersect_MeshRay1(pConstMesh, ref ray, IntPtr.Zero);
    }

    /// <summary>Finds the first intersection of a ray with a mesh.</summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="ray">A ray to be casted.</param>
    /// <param name="meshFaceIndices">faces on mesh that ray intersects.</param>
    /// <returns>
    /// >= 0.0 parameter along ray if successful.
    /// &lt; 0.0 if no intersection found.
    /// </returns>
    /// <remarks>
    /// The ray may intersect more than one face in cases where the ray hits
    /// the edge between two faces or the vertex corner shared by multiple faces.
    /// </remarks>
    public static double MeshRay(Mesh mesh, Ray3d ray, out int[] meshFaceIndices)
    {
      meshFaceIndices = null;
      using (Runtime.InteropWrappers.SimpleArrayInt indices = new Rhino.Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr pConstMesh = mesh.ConstPointer();
        double rc = UnsafeNativeMethods.ON_Intersect_MeshRay1(pConstMesh, ref ray, indices.m_ptr );
        int[] vals = indices.ToArray();
        if (vals!=null && vals.Length > 0)
          meshFaceIndices = vals;
        return rc;
      }
    }

    /// <summary>
    /// Finds the intersection of a mesh and a polyline.
    /// </summary>
    /// <param name="mesh">A mesh to intersect.</param>
    /// <param name="curve">A polyline curves to intersect.</param>
    /// <param name="faceIds">The indices of the intersecting faces. This out reference is assigned during the call.</param>
    /// <returns>An array of points: one for each face that was passed by the faceIds out reference.</returns>
    public static Point3d[] MeshPolyline(Mesh mesh, PolylineCurve curve, out int[] faceIds)
    {
      faceIds = null;
      IntPtr pConstMesh = mesh.ConstPointer();
      IntPtr pConstCurve = curve.ConstPointer();
      int count = 0;
      IntPtr rc = UnsafeNativeMethods.ON_Intersect_MeshPolyline1(pConstMesh, pConstCurve, ref count);
      if (0 == count || IntPtr.Zero == rc)
        return new Point3d[0];

      Point3d[] points = new Point3d[count];
      faceIds = new int[count];
      UnsafeNativeMethods.ON_Intersect_MeshPolyline_Fill(rc, count, points, faceIds);
      return points;
    }

    /// <summary>
    /// Finds the intersection of a mesh and a line
    /// </summary>
    /// <param name="mesh">A mesh to intersect</param>
    /// <param name="line">The line to intersect with the mesh</param>
    /// <param name="faceIds">The indices of the intersecting faces. This out reference is assigned during the call.</param>
    /// <returns>An array of points: one for each face that was passed by the faceIds out reference.</returns>
    public static Point3d[] MeshLine(Mesh mesh, Line line, out int[] faceIds)
    {
      faceIds = null;
      IntPtr pConstMesh = mesh.ConstPointer();
      int count = 0;
      IntPtr rc = UnsafeNativeMethods.ON_Intersect_MeshLine(pConstMesh, line.From, line.To, ref count);
      if (0 == count || IntPtr.Zero == rc)
        return new Point3d[0];

      Point3d[] points = new Point3d[count];
      faceIds = new int[count];
      UnsafeNativeMethods.ON_Intersect_MeshPolyline_Fill(rc, count, points, faceIds);
      return points;
    }

    /// <summary>
    /// Computes point intersections that occur when shooting a ray to a collection of surfaces.
    /// </summary>
    /// <param name="ray">A ray used in intersection.</param>
    /// <param name="geometry">Only Surface and Brep objects are currently supported. Trims are ignored on Breps.</param>
    /// <param name="maxReflections">The maximum number of reflections. This value should be any value between 1 and 1000, inclusive.</param>
    /// <returns>An array of points: one for each face that was passed by the faceIds out reference.</returns>
    /// <exception cref="ArgumentNullException">geometry is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">maxReflections is strictly outside the [1-1000] range.</exception>
    public static Point3d[] RayShoot(Ray3d ray, IEnumerable<GeometryBase> geometry, int maxReflections)
    {
      if (geometry == null) throw new ArgumentNullException("geometry");
      if (maxReflections < 1 || maxReflections > 1000)
        throw new ArgumentOutOfRangeException("maxReflections", "maxReflections must be between 1-1000");
      // We should handle better the case of a null entry inside the geometry collection.
      // Currently a NullReferenceException occurs.

      using (Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer geom = new Runtime.InteropWrappers.SimpleArrayGeometryPointer(geometry))
      {
        IntPtr pGeometry = geom.ConstPointer();
        Point3d[] rc = null;
        using (Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d points = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d())
        {
          IntPtr pPoints = points.NonConstPointer();
          int count = UnsafeNativeMethods.ON_RayShooter_ShootRay(ray.Position, ray.Direction, pGeometry, pPoints, maxReflections);
          if (count > 0) rc = points.ToArray();
        }
        return rc;
      }
    }

    //public static Point3d[] RaySurfaces(Ray3d ray, IEnumerable<Surface> surfaces, int maxReflections)
    //{
    //  if (maxReflections < 1 || maxReflections > 1000)
    //    throw new ArgumentException("maxReflections must be between 1-1000");
    //  Rhino.Runtime.INTERNAL_GeometryArray srfs = new Rhino.Runtime.INTERNAL_GeometryArray(surfaces);
    //  IntPtr pSrfs = srfs.ConstPointer();
    //  Point3d[] rc = null;
    //  using (Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d points = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d())
    //  {
    //    IntPtr pPoints = points.NonConstPointer();
    //    int count = UnsafeNativeMethods.ON_RayShooter_MultiSurface(ray.Position, ray.Direction, pSrfs, pPoints, maxReflections);
    //    if (count > 0) rc = points.ToArray();
    //  }
    //  srfs.Dispose();
    //  return rc;
    //}
    #endregion

#if RHINO_SDK
    /// <summary>
    /// Projects points onto meshes.
    /// </summary>
    /// <param name="meshes">the meshes to project on to.</param>
    /// <param name="points">the points to project.</param>
    /// <param name="direction">the direction to project.</param>
    /// <param name="tolerance">
    /// Projection tolerances used for culling close points and for line-mesh intersection.
    /// </param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    public static Point3d[] ProjectPointsToMeshes(IEnumerable<Mesh> meshes, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
    {
      Point3d[] rc = null;
      if (meshes != null && points != null)
      {
        Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer mesh_array = new Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer();
        foreach (Mesh mesh in meshes)
          mesh_array.Add(mesh, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr pConstMeshArray = mesh_array.ConstPointer();

          using (Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d output = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d())
          {
            IntPtr pOutput = output.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToMeshes(pConstMeshArray, direction, tolerance, inputpoints.Count, inputpoints.m_items, pOutput))
              rc = output.ToArray();
          }
        }
      }
      return rc;
    }

    /// <summary>
    /// Projects points onto breps.
    /// </summary>
    /// <param name="breps">The breps projection targets.</param>
    /// <param name="points">The points to project.</param>
    /// <param name="direction">The direction to project.</param>
    /// <param name="tolerance">The tolerance used for intersections.</param>
    /// <returns>
    /// Array of projected points, or null in case of any error or invalid input.
    /// </returns>
    public static Point3d[] ProjectPointsToBreps(IEnumerable<Brep> breps, IEnumerable<Point3d> points, Vector3d direction, double tolerance)
    {
      Point3d[] rc = null;
      if (breps != null && points != null)
      {
        Rhino.Runtime.InteropWrappers.SimpleArrayBrepPointer brep_array = new Rhino.Runtime.InteropWrappers.SimpleArrayBrepPointer();
        foreach (Brep brep in breps)
          brep_array.Add(brep, true);

        Rhino.Collections.Point3dList inputpoints = new Rhino.Collections.Point3dList(points);
        if (inputpoints.Count > 0)
        {
          IntPtr pConstBrepArray = brep_array.ConstPointer();

          using (Runtime.InteropWrappers.SimpleArrayPoint3d output = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d())
          {
            IntPtr pOutput = output.NonConstPointer();
            if (UnsafeNativeMethods.RHC_RhinoProjectPointsToBreps(pConstBrepArray, direction, tolerance, inputpoints.Count, inputpoints.m_items, pOutput))
              rc = output.ToArray();
          }
        }
      }
      return rc;
    }
#endif
  }

  /// <summary>
  /// Represents all possible cases of a Plane|Circle intersection event.
  /// </summary>
  public enum PlaneCircleIntersection : int
  {
    /// <summary>
    /// No intersections. Either because radius is too small or because circle plane is parallel but not coincident with the intersection plane.
    /// </summary>
    None = 0,

    /// <summary>
    /// Tangent (one point) intersection.
    /// </summary>
    Tangent = 1,

    /// <summary>
    /// Secant (two point) intersection.
    /// </summary>
    Secant = 2,

    /// <summary>
    /// Circle and plane are planar but not coincident. 
    /// Parallel indicates no intersection took place.
    /// </summary>
    Parallel = 3,

    /// <summary>
    /// Circle and plane are co-planar, they intersect everywhere.
    /// </summary>
    Coincident = 4
  }

  /// <summary>
  /// Represents all possible cases of a Plane|Sphere intersection event.
  /// </summary>
  public enum PlaneSphereIntersection : int
  {
    /// <summary>
    /// No intersections.
    /// </summary>
    None = 0,

    /// <summary>
    /// Tangent intersection.
    /// </summary>
    Point = 1,

    /// <summary>
    /// Circular intersection.
    /// </summary>
    Circle = 2,
  }

  /// <summary>
  /// Represents all possible cases of a Line|Circle intersection event.
  /// </summary>
  public enum LineCircleIntersection : int
  {
    /// <summary>
    /// No intersections.
    /// </summary>
    None = 0,

    /// <summary>
    /// One intersection.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Two intersections.
    /// </summary>
    Multiple = 2,
  }

  /// <summary>
  /// Represents all possible cases of a Line|Sphere intersection event.
  /// </summary>
  public enum LineSphereIntersection : int
  {
    /// <summary>
    /// No intersections.
    /// </summary>
    None = 0,

    /// <summary>
    /// One intersection.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Two intersections.
    /// </summary>
    Multiple = 2,
  }

  /// <summary>
  /// Represents all possible cases of a Line|Cylinder intersection event.
  /// </summary>
  public enum LineCylinderIntersection : int
  {
    /// <summary>
    /// No intersections.
    /// </summary>
    None = 0,

    /// <summary>
    /// One intersection.
    /// </summary>
    Single = 1,

    /// <summary>
    /// Two intersections.
    /// </summary>
    Multiple = 2,

    /// <summary>
    /// Line lies on cylinder.
    /// </summary>
    Overlap = 3
  }

  /// <summary>
  /// Represents all possible cases of a Sphere|Sphere intersection event.
  /// </summary>
  public enum SphereSphereIntersection : int
  {
    /// <summary>
    /// Spheres do not intersect.
    /// </summary>
    None = 0,

    /// <summary>
    /// Spheres touch at a single point.
    /// </summary>
    Point = 1,

    /// <summary>
    /// Spheres intersect at a circle.
    /// </summary>
    Circle = 2,

    /// <summary>
    /// Spheres are identical.
    /// </summary>
    Overlap = 3
  }
}