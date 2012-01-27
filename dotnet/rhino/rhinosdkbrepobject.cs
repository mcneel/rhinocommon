#pragma warning disable 1591
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
        Brep rc = Geometry as Brep;
        return rc;
      }
    }
    public Brep DuplicateBrepGeometry()
    {
      Brep rc = DuplicateGeometry() as Brep;
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
        Surface rc = Geometry as Surface;
        return rc;
      }
    }

    public Surface DuplicateSurfaceGeometry()
    {
      Surface rc = DuplicateGeometry() as Surface;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoSurfaceObject_InternalCommitChanges;
    }
  }
}

#endif