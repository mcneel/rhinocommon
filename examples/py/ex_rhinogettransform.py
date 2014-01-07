import Rhino
from Rhino.Geometry import *
from Rhino.Input.Custom import *
from Rhino.Commands import *
from scriptcontext import doc

class GetTranslation(GetTransform):
  def CalculateTransform(self, viewport, point):
    xform = Transform.Identity
    b, base_point = TryGetBasePoint()
    if (b):
      v = point - base_point
      if (not v.IsZero):
        xform = Transform.Translation(v)
        if (not xform.IsValid):
          xform = Transform.Identity
    return xform

class GetTransformCommand (TransformCommand):
  def RunCommand(self, doc, mode):
    list = Rhino.Collections.TransformObjectList()
    rc = self.SelectObjects("Select objects to move", list)
    if rc != Rhino.Commands.Result.Success:
      return rc

    gp = GetPoint()
    gp.SetCommandPrompt("Point to move from")
    gp.Get()
    if gp.CommandResult() != Result.Success:
      return gp.CommandResult()

    gt = GetTranslation()
    gt.SetCommandPrompt("Point to move to")
    gt.SetBasePoint(gp.Point(), true)
    gt.DrawLineFromPoint(gp.Point(), true)
    gt.AddTransformObjects(list)
    gt.GetXform()
    if gt.CommandResult() != Result.Success:
      return gt.CommandResult()

    xform = gt.CalculateTransform(gt.View().ActiveViewport, gt.Point())
    TransformObjects(list, xform, false, false)
    doc.Views.Redraw()
    return Result.Success

if __name__ == "__main__":
  command = GetTransformCommand()
  command.RunCommand(doc, None)