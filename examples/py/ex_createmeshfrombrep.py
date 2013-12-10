import Rhino
import System
import rhinoscriptsyntax as rs
from scriptcontext import doc

def RunCommand():
  gs = Rhino.Input.Custom.GetObject()
  gs.SetCommandPrompt("Select surface or polysurface to mesh")
  gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.PolysrfFilter
  gs.AcceptNothing(True)
  gs.Get()
  if gs.CommandResult() != Rhino.Commands.Result.Success:
    return gs.CommandResult()
  brep = gs.Object(0).Brep()
  if None == brep:
    return Rhino.Commands.Result.Failure

  jaggedAndFaster = Rhino.Geometry.MeshingParameters.Coarse
  smoothAndSlower = Rhino.Geometry.MeshingParameters.Smooth
  defaultMeshParams = Rhino.Geometry.MeshingParameters.Default
  minimal = Rhino.Geometry.MeshingParameters.Minimal

  meshes = Rhino.Geometry.Mesh.CreateFromBrep(brep, smoothAndSlower)
  if meshes == None or meshes.Length == 0:
    return Rhino.Commands.Result.Failure

  brepMesh = Rhino.Geometry.Mesh()
  for mesh in meshes:
    brepMesh.Append(mesh)
  doc.Objects.AddMesh(brepMesh)
  doc.Views.Redraw()

if __name__ == "__main__":
  RunCommand()
