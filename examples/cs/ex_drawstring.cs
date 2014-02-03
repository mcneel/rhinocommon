﻿using Rhino;
using Rhino.DocObjects;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace examples_cs
{
  public class DrawStringCommand : Command
  {
    public override string EnglishName { get { return "csDrawString"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var gp = new GetDrawStringPoint();
      gp.SetCommandPrompt("Point");
      gp.Get();
      return gp.CommandResult();
    }
  }

  public class GetDrawStringPoint : GetPoint
  {
    protected override void OnDynamicDraw(GetPointDrawEventArgs e)
    {
      base.OnDynamicDraw(e);
      var xform = e.Viewport.GetTransform(CoordinateSystem.World, CoordinateSystem.Screen);
      var current_point = e.CurrentPoint;
      current_point.Transform(xform);
      var screen_point = new Point2d(current_point.X, current_point.Y);
      var msg = string.Format("screen {0:F}, {1:F}", current_point.X, current_point.Y);
      e.Display.Draw2dText(msg, System.Drawing.Color.Blue, screen_point, false);
    }
  }
}