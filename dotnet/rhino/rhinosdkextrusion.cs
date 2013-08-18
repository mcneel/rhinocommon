#pragma warning disable 1591
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class ExtrusionObject : RhinoObject
  {
    internal ExtrusionObject(uint serialNumber)
      : base(serialNumber) { }

    public Extrusion ExtrusionGeometry
    {
      get
      {
        Extrusion rc = Geometry as Extrusion;
        return rc;
      }
    }
    public Extrusion DuplicateExtrusionGeometry()
    {
      Extrusion rc = DuplicateGeometry() as Extrusion;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoExtrusionObject_InternalCommitChanges;
    }
  }
}
#endif