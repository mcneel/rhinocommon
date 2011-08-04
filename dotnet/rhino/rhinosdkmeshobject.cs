using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class MeshObject : RhinoObject 
  {
    internal MeshObject(uint serialNumber)
      : base(serialNumber) { }

    public Mesh MeshGeometry
    {
      get
      {
        Mesh rc = this.Geometry as Mesh;
        return rc;
      }
    }

    public Mesh DuplicateMeshGeometry()
    {
      Mesh rc = this.DuplicateGeometry() as Mesh;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoMeshObject_InternalCommitChanges;
    }
  }

  // skipping CRhinoMeshDensity, CRhinoObjectMesh, CRhinoMeshObjectsUI, CRhinoMeshStlUI
}
#endif