using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class BrepObject : RhinoObject
  {
    internal BrepObject(uint serialNumber)
      : base(serialNumber) { }

    public Brep BrepGeometry
    {
      get
      {
        Brep rc = this.Geometry as Brep;
        return rc;
      }
    }
    public Brep DuplicateBrepGeometry()
    {
      Brep rc = this.DuplicateGeometry() as Brep;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoBrepObject_InternalCommitChanges;
    }
  }

  
  public class SurfaceObject : RhinoObject
  {
    internal SurfaceObject(uint serialNumber)
      : base(serialNumber) { }

    public Surface SurfaceGeometry
    {
      get
      {
        Surface rc = this.Geometry as Surface;
        return rc;
      }
    }

    public Surface DuplicateSurfaceGeometry()
    {
      Surface rc = this.DuplicateGeometry() as Surface;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoSurfaceObject_InternalCommitChanges;
    }
  }
}

#endif