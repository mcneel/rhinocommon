#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  /// <summary>
  /// The Rhino cliping plane object, as accessible in the Rhino document.
  /// </summary>
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