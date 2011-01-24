using Rhino.Geometry;

#if USING_V5_SDK
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
        Extrusion rc = this.Geometry as Extrusion;
        return rc;
      }
    }
    public Extrusion DuplicateBrepGeometry()
    {
      Extrusion rc = this.DuplicateGeometry() as Extrusion;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoExtrusionObject_InternalCommitChanges;
    }
  }
}
#endif