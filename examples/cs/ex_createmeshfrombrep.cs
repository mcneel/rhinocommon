using Rhino;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("BF92CA69-3628-4167-96C1-3FA90E460333")]
  public class ex_createmeshfrombrep : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csCreateMeshesFromBreps"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("Select surface or polysurface to mesh");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.PolysrfFilter;
      gs.AcceptNothing(true);
      gs.Get();
      if (gs.CommandResult() != Rhino.Commands.Result.Success)
        return gs.CommandResult();
      var brep = gs.Object(0).Brep();
      if (null == brep)
        return Rhino.Commands.Result.Failure;

      var jaggedAndFaster = Rhino.Geometry.MeshingParameters.Coarse;
      var smoothAndSlower = Rhino.Geometry.MeshingParameters.Smooth;
      var defaultMeshParams = Rhino.Geometry.MeshingParameters.Default;
      var minimal = Rhino.Geometry.MeshingParameters.Minimal;

      var meshes = Rhino.Geometry.Mesh.CreateFromBrep(brep, smoothAndSlower);
      if (meshes == null || meshes.Length == 0)
        return Rhino.Commands.Result.Failure;

      var brepMesh = new Rhino.Geometry.Mesh();
      foreach (var mesh in meshes)
        brepMesh.Append(mesh);
      doc.Objects.AddMesh(brepMesh);
      doc.Views.Redraw();

      return Rhino.Commands.Result.Success;
    }
  }
}