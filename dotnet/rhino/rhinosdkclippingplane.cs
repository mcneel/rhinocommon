using System;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class ClippingPlaneObject : RhinoObject
  {
    internal ClippingPlaneObject(uint serialNumber)
      : base(serialNumber)
    { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoClippingPlaneObject_InternalCommitChanges;
    }

    public Rhino.Geometry.ClippingPlaneSurface ClippingPlaneGeometry
    {
      get
      {
        Rhino.Geometry.ClippingPlaneSurface rc = this.Geometry as Rhino.Geometry.ClippingPlaneSurface;
        return rc;
      }
    }

  }
}
#endif