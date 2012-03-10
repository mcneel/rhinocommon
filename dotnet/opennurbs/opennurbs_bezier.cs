using System;
using System.Collections.Generic;

//public class ON_PolynomialCurve { }
//public class ON_PolynomialSurface { }
namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a Bezier curve.
  /// <para>Note: as an exception, the bezier curve <b>is not</b> derived from <see cref="Curve"/>.</para>
  /// </summary>
  public class BezierCurve : IDisposable
  {
    IntPtr m_ptr; // This class is never const
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }
    private BezierCurve(IntPtr ptr)
    {
      m_ptr = ptr;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~BezierCurve()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }


    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_BezierCurve_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Create bezier curve with controls defined by a list of 2d points
    /// </summary>
    /// <param name="controlPoints"></param>
    public BezierCurve(IEnumerable<Point2d> controlPoints)
    {
      List<Point2d> pts = new List<Point2d>(controlPoints);
      Point2d[] points = pts.ToArray();
      m_ptr = UnsafeNativeMethods.ON_BezierCurve_New2d(points.Length, points);
    }

    /// <summary>
    /// Create bezier curve with controls defined by a list of 3d points
    /// </summary>
    /// <param name="controlPoints"></param>
    public BezierCurve(IEnumerable<Point3d> controlPoints)
    {
      List<Point3d> pts = new List<Point3d>(controlPoints);
      Point3d[] points = pts.ToArray();
      m_ptr = UnsafeNativeMethods.ON_BezierCurve_New3d(points.Length, points);
    }

    /// <summary>
    /// Create bezier curve with controls defined by a list of 4d points
    /// </summary>
    /// <param name="controlPoints"></param>
    public BezierCurve(IEnumerable<Point4d> controlPoints)
    {
      List<Point4d> pts = new List<Point4d>(controlPoints);
      Point4d[] points = pts.ToArray();
      m_ptr = UnsafeNativeMethods.ON_BezierCurve_New4d(points.Length, points);
    }

    /// <summary>Tests an object to see if it is valid.</summary>
    public bool IsValid
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_BezierCurve_IsValid(pConstThis);
      }
    }

    /// <summary>
    /// Loft a bezier through a list of points
    /// </summary>
    /// <param name="points">2 or more points to interpolate</param>
    /// <returns>new bezier curve if successful</returns>
    public static BezierCurve CreateLoftedBezier(IEnumerable<Point3d> points)
    {
      List<Point3d> _pts = new List<Point3d>(points);
      Point3d[] ptarray = _pts.ToArray();
      IntPtr pBez = UnsafeNativeMethods.ON_BezierCurve_Loft(ptarray.Length, ptarray);
      if (pBez == IntPtr.Zero)
        return null;
      return new BezierCurve(pBez);
    }

    /// <summary>
    /// Loft a bezier through a list of points
    /// </summary>
    /// <param name="points">2 or more points to interpolate</param>
    /// <returns>new bezier curve if successful</returns>
    public static BezierCurve CreateLoftedBezier(IEnumerable<Point2d> points)
    {
      List<Point2d> _pts = new List<Point2d>(points);
      Point2d[] ptarray = _pts.ToArray();
      IntPtr pBez = UnsafeNativeMethods.ON_BezierCurve_Loft2(ptarray.Length, ptarray);
      if (pBez == IntPtr.Zero)
        return null;
      return new BezierCurve(pBez);
    }

    /// <summary>
    /// Boundingbox solver. Gets the world axis aligned boundingbox for the curve.
    /// </summary>
    /// <param name="accurate">If true, a physically accurate boundingbox will be computed. 
    /// If not, a boundingbox estimate will be computed. For some geometry types there is no 
    /// difference between the estimate and the accurate boundingbox. Estimated boundingboxes 
    /// can be computed much (much) faster than accurate (or "tight") bounding boxes. 
    /// Estimated bounding boxes are always similar to or larger than accurate bounding boxes.</param>
    /// <returns>
    /// The boundingbox of the geometry in world coordinates or BoundingBox.Empty 
    /// if not bounding box could be found.
    /// </returns>
    public BoundingBox GetBoundingBox(bool accurate)
    {
      BoundingBox bbox = new BoundingBox();
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.ON_BezierCurve_BoundingBox(pConstThis, accurate, ref bbox);
      return bbox;
    }

    /// <summary>Evaluates point at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Point (location of curve at the parameter t).</returns>
    /// <remarks>No error handling.</remarks>
    public Point3d PointAt(double t)
    {
      Point3d rc = new Point3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_BezierCurve_PointAt(ptr, t, ref rc);
      return rc;
    }

    /// <summary>Evaluates the unit tangent vector at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Unit tangent vector of the curve at the parameter t.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d TangentAt(double t)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_BezierCurve_TangentAt(ptr, t, ref rc);
      return rc;
    }

    /// <summary>Evaluate the curvature vector at a curve parameter.</summary>
    /// <param name="t">Evaluation parameter.</param>
    /// <returns>Curvature vector of the curve at the parameter t.</returns>
    /// <remarks>No error handling.</remarks>
    public Vector3d CurvatureAt(double t)
    {
      Vector3d rc = new Vector3d();
      IntPtr ptr = ConstPointer();
      UnsafeNativeMethods.ON_BezierCurve_CurvatureAt(ptr, t, ref rc);
      return rc;
    }

    /// <summary>
    /// Constructs a NURBS curve representation of this curve.
    /// </summary>
    /// <returns>NURBS representation of the curve on success, null on failure.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pNurbsCurve = UnsafeNativeMethods.ON_BezierCurve_GetNurbForm(pConstThis);
      return GeometryBase.CreateGeometryHelper(pNurbsCurve, null) as NurbsCurve;
    }

    /// <summary>
    /// Gets a value indicating whether or not the curve is rational. 
    /// Rational curves have control-points with custom weights.
    /// </summary>
    public bool IsRational
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_BezierCurve_IsRational(pConstThis);
      }
    }

    /// <summary>
    /// Number of control vertices in this curve
    /// </summary>
    public int ControlVertexCount
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_BezierCurve_CVCount(pConstThis);
      }
    }

    /// <summary>Get location of a control vertex.</summary>
    /// <param name="index">
    /// Control vertex index (0 &lt;= index &lt; ControlVertexCount)
    /// </param>
    /// <returns>
    /// If the bezier is rational, the euclidean location is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
    public Point2d GetControlVertex2d(int index)
    {
      Point3d pt = GetControlVertex3d(index);
      return new Point2d(pt.X, pt.Y);
    }

    /// <summary>Get location of a control vertex.</summary>
    /// <param name="index">
    /// Control vertex index (0 &lt;= index &lt; ControlVertexCount)
    /// </param>
    /// <returns>
    /// If the bezier is rational, the euclidean location is returned.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
    public Point3d GetControlVertex3d(int index)
    {
      if (index < 0 || index >= ControlVertexCount)
        throw new ArgumentOutOfRangeException("index");
      Point3d rc = new Point3d();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_BezierCurve_GetCV3d(pConstThis, index, ref rc))
        return Point3d.Unset;
      return rc;
    }

    /// <summary>Get location of a control vertex.</summary>
    /// <param name="index">
    /// Control vertex index (0 &lt;= index &lt; ControlVertexCount)
    /// </param>
    /// <returns>
    /// Homogenous value of control vertex. If the bezier is not
    /// rational, the weight is 1.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">when index is out of range</exception>
    public Point4d GetControlVertex4d(int index)
    {
      if( index<0 || index>=ControlVertexCount )
        throw new ArgumentOutOfRangeException("index");
      Point4d rc = new Point4d();
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_BezierCurve_GetCV4d(pConstThis, index, ref rc))
        return Point4d.Unset;
      return rc;
    }

    /// <summary>Make bezier rational</summary>
    /// <returns>true if successful</returns>
    public bool MakeRational()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_BezierCurve_MakeRational(pThis, true);
    }
    /// <summary>Make bezier non-rational</summary>
    /// <returns>treu if successful</returns>
    public bool MakeNonRational()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_BezierCurve_MakeRational(pThis, false);
    }

    /// <summary>Increase degree of bezier</summary>
    /// <param name="desiredDegree"></param>
    /// <returns>true if successful.  false if desiredDegree &lt; current degree.</returns>
    public bool IncreaseDegree(int desiredDegree)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_BezierCurve_ChangeInt(pThis, true, desiredDegree);
    }

    /// <summary>Change dimension of bezier.</summary>
    /// <param name="desiredDimension"></param>
    /// <returns>true if successful.  false if desired_dimension &lt; 1</returns>
    public bool ChangeDimension(int desiredDimension)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_BezierCurve_ChangeInt(pThis, false, desiredDimension);
    }

#region Rhino SDK functions
#if RHINO_SDK
    /// <summary>
    /// Constructs an array of cubic, non-rational beziers that fit a curve to a tolerance.
    /// </summary>
    /// <param name="sourceCurve">A curve to approximate.</param>
    /// <param name="distanceTolerance">
    /// The max fitting error. Use RhinoMath.SqrtEpsilon as a minimum.
    /// </param>
    /// <param name="kinkTolerance">
    /// If the input curve has a g1-discontinuity with angle radian measure
    /// greater than kinkTolerance at some point P, the list of beziers will
    /// also have a kink at P.
    /// </param>
    /// <returns>A new array of bezier curves. The array can be empty and might contain null items.</returns>
    public static BezierCurve[] CreateCubicBeziers(Curve sourceCurve, double distanceTolerance, double kinkTolerance)
    {
      IntPtr pConstCurve = sourceCurve.ConstPointer();
      IntPtr pBezArray = UnsafeNativeMethods.ON_SimpleArray_BezierCurveNew();
      int count = UnsafeNativeMethods.RHC_RhinoMakeCubicBeziers(pConstCurve, pBezArray, distanceTolerance, kinkTolerance);
      BezierCurve[] rc = new BezierCurve[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr = UnsafeNativeMethods.ON_SimpleArray_BezierCurvePtr(pBezArray, i);
        if (ptr != IntPtr.Zero)
          rc[i] = new BezierCurve(ptr);
      }
      UnsafeNativeMethods.ON_SimpleArray_BezierCurveDelete(pBezArray);
      return rc;
    }
#endif
#endregion
  }
}

//public class ON_BezierSurface { }
//public class ON_BezierCage { }
//public class ON_BezierCageMorph { }
