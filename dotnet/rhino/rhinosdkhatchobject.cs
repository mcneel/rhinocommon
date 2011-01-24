using Rhino.Geometry;

namespace Rhino.DocObjects
{
  public class HatchObject : RhinoObject
  {
    internal HatchObject(uint serialNumber)
      : base(serialNumber) { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoHatch_InternalCommitChanges;
    }

    public Hatch HatchGeometry
    {
      get { return this.Geometry as Hatch; }
    }
  }
}
