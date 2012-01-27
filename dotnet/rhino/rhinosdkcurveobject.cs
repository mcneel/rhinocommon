#pragma warning disable 1591
using Rhino.Geometry;

#if RHINO_SDK
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
        Curve rc = Geometry as Curve;
        return rc;
      }
    }

    public Curve DuplicateCurveGeometry()
    {
      Curve rc = DuplicateGeometry() as Curve;
      return rc;
    }

    internal override RhinoObject.CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoCurveObject_InternalCommitChanges;
    }
  }
}
#endif