import rhinoscriptsyntax as rs
from scriptcontext import doc
import Rhino
import System

def RunCommand():
  gs = Rhino.Input.Custom.GetObject()
  gs.SetCommandPrompt("select sphere")
  gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface
  gs.DisablePreSelect()
  gs.SubObjectSelect = False
  gs.Get()
  if gs.CommandResult() != Rhino.Commands.Result.Success:
    return gs.CommandResult()

  b, sphere = gs.Object(0).Surface().TryGetSphere()
  if sphere.IsValid:
    mesh = Rhino.Geometry.Mesh.CreateFromSphere(sphere, 10, 10)
    if mesh == None:
      return Rhino.Commands.Result.Failure

    conduit = DrawBlueMeshConduit(mesh)
    conduit.Enabled = True
    doc.Views.Redraw()

    inStr = rs.GetString("press <Enter> to continue")

    conduit.Enabled = False
    doc.Views.Redraw()
    return Rhino.Commands.Result.Success
  else:
    return Rhino.Commands.Result.Failure

class DrawBlueMeshConduit(Rhino.Display.DisplayConduit):
  def __init__(self, mesh):
    self.mesh = mesh
    self.color = System.Drawing.Color.Blue
    self.material = Rhino.Display.DisplayMaterial()
    self.material.Diffuse = self.color
    if mesh != None and mesh.IsValid:
      self.bbox = mesh.GetBoundingBox(True)

  def CalculateBoundingBox(self, calculateBoundingBoxEventArgs):
    base.CalculateBoundingBox(calculateBoundingBoxEventArgs)
    calculateBoundingBoxEventArgs.BoundingBox.Union(bbox)

  def PreDrawObjects(self, drawEventArgs):
    base.PreDrawObjects(rawEventArgs)
    gvp = rawEventArgs.Display.Viewport
    if vp.DisplayMode.EnglishName.ToLower() == "wireframe":
      rawEventArgs.Display.DrawMeshWires(mesh, color)
    else:
      rawEventArgs.Display.DrawMeshShaded(mesh, material)

if __name__ == "__main__":
    RunCommand()