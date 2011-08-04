using System;

//public class ON_PolynomialCurve { }
//public class ON_PolynomialSurface { }
namespace Rhino.Geometry
{
  /// <summary>Note that bezier curve is NOT derived from Curve</summary>
  public class BezierCurve : IDisposable
  {
    IntPtr m_ptr; // This class is never const
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }
    private BezierCurve(IntPtr ptr)
    {
      m_ptr = ptr;
    }

    ~BezierCurve()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_BezierCurve_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

#region Rhino SDK functions
#if !USING_OPENNURBS
    /// <summary>
    /// Make an array of cubic, non-rational beziers that fit a curve to a tolerance
    /// </summary>
    /// <param name="sourceCurve"></param>
    /// <param name="distanceTolerance">
    /// max fitting error.  Will use RhinoMath.SqrtEpsilon as a minimum.
    /// </param>
    /// <param name="kinkTolerance">
    /// If the input curve has a g1-discontinuity with angle radian measure
    /// greater than kinkTolerance at some point P, the list of beziers will
    /// also have a kink at P
    /// </param>
    /// <returns></returns>
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
