#pragma warning disable 1591
using Rhino.Geometry;
using System;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class MeshObject : RhinoObject 
  {
    internal MeshObject(uint serialNumber)
      : base(serialNumber) { }

    protected MeshObject() { }

    public Mesh MeshGeometry
    {
      get
      {
        Mesh rc = Geometry as Mesh;
        return rc;
      }
    }

    public Mesh DuplicateMeshGeometry()
    {
      Mesh rc = DuplicateGeometry() as Mesh;
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