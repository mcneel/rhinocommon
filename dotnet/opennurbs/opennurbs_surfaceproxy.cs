using System;

namespace Rhino.Geometry
{
  public class SurfaceProxy : Surface
  {
    protected SurfaceProxy()
    {
    }

    // Non-Const operations are not allowed on SurfaceProxy classes
    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      return IntPtr.Zero;
    }
  }
}
