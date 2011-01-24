using Rhino.Geometry;

namespace Rhino.DocObjects
{
  public class CurveObject : RhinoObject
  {
    internal CurveObject(uint serialNumber)
      : base(serialNumber)
    { }

    public Curve CurveGeometry
    {
      get
      {
        Curve rc = this.Geometry as Curve;
        return rc;
      }
    }

    public Curve DuplicateCurveGeometry()
    {
      Curve rc = this.DuplicateGeometry() as Curve;
      return rc;
    }

    internal override RhinoObject.CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoCurveObject_InternalCommitChanges;
    }
  }
}
