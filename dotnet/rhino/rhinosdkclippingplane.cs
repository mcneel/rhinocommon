using System;

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
  }
}
