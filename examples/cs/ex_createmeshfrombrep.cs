using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.DocObjects;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("BF92CA69-3628-4167-96C1-3FA90E460333")]
  public class CreateMeshFromBrepCommand : Command
  {
    public override string EnglishName { get { return "csCreateMeshesFromBrep"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      ObjRef objRef;
      var rc = Rhino.Input.RhinoGet.GetOneObject("Select surface or polysurface to mesh", true, ObjectType.Surface | ObjectType.PolysrfFilter, out objRef);
      if (rc != Result.Success)
        return rc;
      var brep = objRef.Brep();
      if (null == brep)
        return Result.Failure;

      // you could choose anyone of these for example
      var jaggedAndFaster = MeshingParameters.Coarse;
      var smoothAndSlower = MeshingParameters.Smooth;
      var defaultMeshParams = MeshingParameters.Default;
      var minimal = MeshingParameters.Minimal;

      var meshes = Mesh.CreateFromBrep(brep, smoothAndSlower);
      if (meshes == null || meshes.Length == 0)
        return Result.Failure;

      var brepMesh = new Mesh();
      foreach (var mesh in meshes)
        brepMesh.Append(mesh);
      doc.Objects.AddMesh(brepMesh);
      doc.Views.Redraw();

      return Result.Success;
    }
  }
}