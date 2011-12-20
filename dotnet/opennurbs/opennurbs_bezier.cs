using System;

//public class ON_PolynomialCurve { }
//public class ON_PolynomialSurface { }
namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a Bezier curve.
  /// <para>Note: the bezier curve is not derived from <see cref="Curve"/>.</para>
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
