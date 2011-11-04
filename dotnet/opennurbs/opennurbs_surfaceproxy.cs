#pragma warning disable 1591
using System;

namespace Rhino.Geometry
{
  /// <summary>
  /// Provides strongly-typed access to Brep faces
  /// </summary>
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
