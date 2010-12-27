using System;

namespace Rhino.Geometry
{
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
