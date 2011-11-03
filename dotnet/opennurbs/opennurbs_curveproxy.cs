using System;

namespace Rhino.Geometry
{
  /// <summary>
  /// Provides strongly-typed access to Brep edges
  /// </summary>
  public class CurveProxy : Curve
  {
    protected CurveProxy()
    {
    }

    // Non-Const operations are not allowed on CurveProxy classes
    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      return IntPtr.Zero;
    }
  }
}
