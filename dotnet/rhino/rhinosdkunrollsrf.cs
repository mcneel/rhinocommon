using System;
using System.Collections.Generic;

#if USING_V5_SDK && RHINO_SDK
namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the operation of unrolling a single surface.
  /// </summary>
  public class Unroller
  {
    readonly List<Curve> m_curves = new List<Curve>();
    readonly List<Point3d> m_points = new List<Point3d>();
    readonly List<TextDot> m_dots = new List<TextDot>();
    bool m_bExplodeOutput; // = false initialized by Runtime
    double m_dExplodeSpacing = 2.0;
    readonly Surface m_surface;
    readonly Brep m_brep;
    double m_dAbsoluteTolerance = 0.01;
    double m_dRelativeTolerance = 0.01;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="Unroller"/> class with surface.
    /// </summary>
    /// <param name="surface">A surface to be unrolled.</param>
    public Unroller(Surface surface)
    {
      m_surface = surface;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Unroller"/> class with a brep.
    /// </summary>
    /// <param name="brep">A brep to be unrolled.</param>
    public Unroller(Brep brep)
    {
      m_brep = brep;
    }

    /// <summary>
    /// Adds curves that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="curves">An array, a list or any enumerable set of curves.</param>
    public void AddFollowingGeometry(IEnumerable<Curve> curves)
    {
      m_curves.AddRange(curves);
    }
    /// <summary>
    /// Adds a curve that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="curve">The curve.</param>
    public void AddFollowingGeometry(Curve curve)
    {
      m_curves.Add(curve);
    }

    /// <summary>
    /// Adds points that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="points">An array, a list or any enumerable set of points.</param>
    public void AddFollowingGeometry(IEnumerable<Point3d> points)
    {
      m_points.AddRange(points);
    }
    /// <summary>
    /// Adds a point that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="point">A point.</param>
    public void AddFollowingGeometry(Point3d point)
    {
      m_points.Add(point);
    }
    /// <summary>
    /// Adds a point that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="point">A point.</param>
    public void AddFollowingGeometry(Point point)
    {
      m_points.Add(point.Location);
    }

    /// <summary>
    /// Adds text dots that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="dots">An array, a list or any enumerable set of text dots.</param>
    public void AddFollowingGeometry(IEnumerable<TextDot> dots)
    {
      m_dots.AddRange(dots);
    }
    /// <summary>
    /// Adds a text dot that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="dot">A text dot.</param>
    public void AddFollowingGeometry(TextDot dot)
    {
      m_dots.Add(dot);
    }

    /// <summary>
    /// Adds text dots that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="dotLocations">An array, a list, or any enumerable set of dot locations.</param>
    /// <param name="dotText">An array, a list, or any enumerable set of dot strings.</param>
    public void AddFollowingGeometry(IEnumerable<Point3d> dotLocations, IEnumerable<string> dotText)
    {
      List<Point3d> pts = new List<Point3d>(dotLocations);
      List<string> text = new List<string>(dotText);
      if (pts.Count != text.Count)
        throw new ArgumentException("locations and dotText must be of same length");
      for (int i = 0; i < pts.Count; i++)
        AddFollowingGeometry(pts[i], text[i]);
    }

    /// <summary>
    /// Adds a text dot that should be unrolled along with the surface/brep.
    /// </summary>
    /// <param name="dotLocation">A dot point.</param>
    /// <param name="dotText">A dot text.</param>
    public void AddFollowingGeometry(Point3d dotLocation, string dotText)
    {
      TextDot dot = new TextDot(dotText, dotLocation);
      AddFollowingGeometry(dot);
    }

    /// <summary>
    /// Gets or sets a value determining whether geometry should be exploded.
    /// </summary>
    public bool ExplodeOutput
    {
      get { return m_bExplodeOutput; }
      set { m_bExplodeOutput = value; }
    }

    /// <summary>
    /// Gets or sets a value determining whether spacing should be exploded.
    /// </summary>
    public double ExplodeSpacing
    {
      get { return m_dExplodeSpacing; }
      set { m_dExplodeSpacing = value; }
    }
    
    /// <summary>
    /// Gets or sets the absolute tolerance for the unrolling operation.
    /// <para>Absolute tolerance is used in the evaluation of new entities,
    /// such as intersections, reprojections and splits.</para>
    /// <para>In the current implementation, absolute tolerance is used 
    /// in tessellating rails, fitting curves and pulling back trims.</para>
    /// </summary>
    public double AbsoluteTolerance
    {
      get { return m_dAbsoluteTolerance; }
      set { m_dAbsoluteTolerance = value; }
    }

    /// <summary>
    /// Gets or sets the relative tolerance for the unrolling operation.
    /// <para>Relative tolerance is used in the evaluation of intrinsic properties,
    /// such as computations "along" the surface or brep.</para>
    /// <para>In the current implementation, relative tolerance is used to decide
    /// if a surface is flat enough to try to unroll. That helps ease the scale dependency.
    /// The surface has to be linear in one direction within (length * RelativeTolerance)
    /// to be considered linear for that purpose. Otherwise smash will ignore that tolerance and
    /// unroll anything.</para>
    /// </summary>
    public double RelativeTolerance
    {
      get { return m_dRelativeTolerance; }
      set { m_dRelativeTolerance = value; }
    }

    /// <summary>
    /// Executes unrolling operations.
    /// </summary>
    /// <param name="unrolledCurves">An array of unrolled curves is assigned during the call in this out parameter.</param>
    /// <param name="unrolledPoints">An array of unrolled points is assigned during the call in this out parameter.</param>
    /// <param name="unrolledDots">An array of unrolled text dots is assigned during the call in this out parameter.</param>
    /// <returns>An array of breps. This array can be empty.</returns>
    public Brep[] PerformUnroll(out Curve[] unrolledCurves, out Point3d[] unrolledPoints, out TextDot[] unrolledDots)
    {
      unrolledCurves = new Curve[0];
      unrolledPoints = new Point3d[0];
      unrolledDots = new TextDot[0];

      IntPtr pUnroller = IntPtr.Zero;
      if (m_surface != null)
      {
        IntPtr pSurface = m_surface.ConstPointer();
        pUnroller = UnsafeNativeMethods.CRhinoUnroll_NewSrf(pSurface, m_dAbsoluteTolerance, m_dRelativeTolerance);
      }
      else if (m_brep != null)
      {
        IntPtr pBrep = m_brep.ConstPointer();
        pUnroller = UnsafeNativeMethods.CRhinoUnroll_NewBrp(pBrep, m_dAbsoluteTolerance, m_dRelativeTolerance);
      }
      if (pUnroller == IntPtr.Zero)
        throw new Exception("Unable to access input surface or brep");

      Brep[] rc = new Brep[0];
      if (0 == UnsafeNativeMethods.CRhinoUnroll_PrepareFaces(pUnroller))
      {
        if (m_curves.Count > 0)
        {
          Runtime.InteropWrappers.SimpleArrayCurvePointer crvs = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer(m_curves);
          IntPtr pCrvs = crvs.ConstPointer();
          UnsafeNativeMethods.CRhinoUnroll_PrepareCurves(pUnroller, pCrvs);
        }
        if (m_points.Count > 0)
        {
          Point3d[] pts =  m_points.ToArray();
          UnsafeNativeMethods.CRhinoUnroll_PreparePoints(pUnroller, pts.Length, pts);
        }
        if (m_dots.Count > 0)
        {
          using (Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer dots = new Runtime.InteropWrappers.SimpleArrayGeometryPointer(m_dots))
          {
            IntPtr pDots = dots.ConstPointer();
            UnsafeNativeMethods.CRhinoUnroll_PrepareDots(pUnroller, pDots);
          }
        }

        int brepCount = 0;
        int curveCount = 0;
        int pointCount = 0;
        int dotCount = 0;
        double explode_dist = -1;
        if (m_bExplodeOutput)
          explode_dist = m_dExplodeSpacing;
        IntPtr pResults = UnsafeNativeMethods.CRhinoUnroll_CreateFlatBreps(pUnroller,
          explode_dist, ref brepCount, ref curveCount, ref pointCount, ref dotCount);
        if (pResults != IntPtr.Zero)
        {
          if (brepCount > 0)
          {
            rc = new Brep[brepCount];
            for (int i = 0; i < brepCount; i++)
            {
              IntPtr pBrep = UnsafeNativeMethods.CRhinoUnrollResults_GetBrep(pResults, i);
              if (pBrep != IntPtr.Zero) rc[i] = new Brep(pBrep, null);
            }
          }
          if (curveCount > 0)
          {
            unrolledCurves = new Curve[curveCount];
            for (int i = 0; i < curveCount; i++)
            {
              IntPtr pCurve = UnsafeNativeMethods.CRhinoUnrollResults_GetCurve(pResults, i);
              if (pCurve != IntPtr.Zero) unrolledCurves[i] = new Curve(pCurve, null);
            }
          }
          if (pointCount > 0)
          {
            unrolledPoints = new Point3d[pointCount];
            UnsafeNativeMethods.CRhinoUnrollResults_GetPoints(pResults, pointCount, unrolledPoints);
          }
          if (dotCount > 0)
          {
            unrolledDots = new TextDot[dotCount];
            for (int i = 0; i < dotCount; i++)
            {
              IntPtr pDot = UnsafeNativeMethods.CRhinoUnrollResults_GetDot(pResults, i);
              if (pDot != IntPtr.Zero) unrolledDots[i] = new TextDot(pDot, null);
            }
          }

          UnsafeNativeMethods.CRhinoUnrollResults_Delete(pResults);
        }
      }
      UnsafeNativeMethods.CRhinoUnroll_Delete(pUnroller);
      return rc;
    }
  }
}
#endif