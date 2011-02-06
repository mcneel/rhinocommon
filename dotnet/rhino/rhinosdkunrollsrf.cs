using System;
using System.Collections.Generic;

#if USING_V5_SDK
namespace Rhino.Geometry
{
  public class Unroller
  {
    List<Curve> m_curves = new List<Curve>();
    List<Point3d> m_points = new List<Point3d>();
    List<TextDot> m_dots = new List<TextDot>();
    bool m_bExplodeOutput = false;
    double m_dExplodeSpacing = 2.0;
    Surface m_surface;
    Brep m_brep;
    double m_dAbsoluteTolerance = 0.1;
    double m_dRelativeTolerance = 0.1;
    
    public Unroller(Surface surface)
    {
      m_surface = surface;
    }

    public Unroller(Brep brep)
    {
      m_brep = brep;
    }

    /// <summary>
    /// Add curves that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="curves"></param>
    public void AddFollowingGeometry(IEnumerable<Curve> curves)
    {
      m_curves.AddRange(curves);
    }
    /// <summary>
    /// Add curve that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="curve"></param>
    public void AddFollowingGeometry(Curve curve)
    {
      m_curves.Add(curve);
    }

    /// <summary>
    /// Add points that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="points"></param>
    public void AddFollowingGeometry(IEnumerable<Point3d> points)
    {
      m_points.AddRange(points);
    }
    /// <summary>
    /// Add point that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="point"></param>
    public void AddFollowingGeometry(Point3d point)
    {
      m_points.Add(point);
    }
    /// <summary>
    /// Add point that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="point"></param>
    public void AddFollowingGeometry(Point point)
    {
      m_points.Add(point.Location);
    }

    /// <summary>
    /// Add text dots that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="dots"></param>
    public void AddFollowingGeometry(IEnumerable<TextDot> dots)
    {
      m_dots.AddRange(dots);
    }
    /// <summary>
    /// Add text dot that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="dot"></param>
    public void AddFollowingGeometry(TextDot dot)
    {
      m_dots.Add(dot);
    }

    /// <summary>
    /// Add text dots that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="dotLocations"></param>
    /// <param name="dotText"></param>
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
    /// Add text dot that should be unrolled along with the Surface/Brep
    /// </summary>
    /// <param name="dotLocation"></param>
    /// <param name="dotText"></param>
    public void AddFollowingGeometry(Point3d dotLocation, string dotText)
    {
      TextDot dot = new TextDot(dotText, dotLocation);
      AddFollowingGeometry(dot);
    }

    public bool ExplodeOutput
    {
      get { return m_bExplodeOutput; }
      set { m_bExplodeOutput = value; }
    }
    public double ExplodeSpacing
    {
      get { return m_dExplodeSpacing; }
      set { m_dExplodeSpacing = value; }
    }

    public double AbsoluteTolerance
    {
      get { return m_dAbsoluteTolerance; }
      set { m_dAbsoluteTolerance = value; }
    }
    public double RelativeTolerance
    {
      get { return m_dRelativeTolerance; }
      set { m_dRelativeTolerance = value; }
    }

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
          Runtime.INTERNAL_GeometryArray dots = new Rhino.Runtime.INTERNAL_GeometryArray(m_dots);
          IntPtr pDots = dots.ConstPointer();
          UnsafeNativeMethods.CRhinoUnroll_PrepareDots(pUnroller, pDots);
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
              if (pBrep != IntPtr.Zero) rc[i] = new Brep(pBrep, null, null);
            }
          }
          if (curveCount > 0)
          {
            unrolledCurves = new Curve[curveCount];
            for (int i = 0; i < curveCount; i++)
            {
              IntPtr pCurve = UnsafeNativeMethods.CRhinoUnrollResults_GetCurve(pResults, i);
              if (pCurve != IntPtr.Zero) unrolledCurves[i] = new Curve(pCurve, null, null);
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
              if (pDot != IntPtr.Zero) unrolledDots[i] = new TextDot(pDot, null, null);
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