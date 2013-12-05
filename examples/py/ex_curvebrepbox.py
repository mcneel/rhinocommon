import Rhino
from Rhino.Geometry import *
from Rhino.Commands import Result
import rhinoscriptsyntax as rs
from scriptcontext import doc

def RunCommand():
  gc = Rhino.Input.Custom.GetObject()
  gc.SetCommandPrompt("select curve")
  gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
  gc.DisablePreSelect()
  gc.SubObjectSelect = False
  gc.Get()
  if gc.CommandResult() != Result.Success:
    return gc.CommandResult()
  if None == gc.Object(0).Curve():
    return Result.Failure
  crv = gc.Object(0).Curve()

  view = doc.Views.ActiveView
  plane = view.ActiveViewport.ConstructionPlane()
  # Create a construction plane aligned bounding box
  bbox = crv.GetBoundingBox(plane)

  if bbox.IsDegenerate(doc.ModelAbsoluteTolerance) > 0:
    print "the curve's bounding box is degenerate (flat) in at least one direction so a box cannot be created."
    return Rhino.Commands.Result.Failure

  box = Box(bbox)
  brep = Brep.CreateFromBox(box)
  doc.Objects.AddBrep(brep)
  doc.Views.Redraw()

if __name__ == "__main__":
  RunCommand()
