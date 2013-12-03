using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Display;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("422669A5-52B9-48A1-8DDD-61A13D9ACAB6")]
  public class ex_displayconduit : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csIntroToDisplayConduits"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var conduit = new MyConduit();
      conduit.Enabled = true;
      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
  }

  class MyConduit : Rhino.Display.DisplayConduit
  {
    protected override void CalculateBoundingBox(CalculateBoundingBoxEventArgs e)
    {
      base.CalculateBoundingBox(e);
      e.BoundingBox.Union(e.Display.Viewport.ConstructionPlane().Origin);
    }

    protected override void PreDrawObjects(DrawEventArgs e)
    {
      base.PreDrawObjects(e);

      var cPlane = e.Display.Viewport.ConstructionPlane();
      var xColor = Rhino.ApplicationSettings.AppearanceSettings.GridXAxisLineColor;
      var yColor = Rhino.ApplicationSettings.AppearanceSettings.GridYAxisLineColor;
      var zColor = Rhino.ApplicationSettings.AppearanceSettings.GridZAxisLineColor;

      e.Display.EnableDepthWriting(false);
      e.Display.EnableDepthTesting(false);

      e.Display.DrawPoint(cPlane.Origin, System.Drawing.Color.White);
      e.Display.DrawArrow(new Line(cPlane.Origin, new Vector3d(cPlane.XAxis) * 10.0), xColor);
      e.Display.DrawArrow(new Line(cPlane.Origin, new Vector3d(cPlane.YAxis) * 10.0), yColor);
      e.Display.DrawArrow(new Line(cPlane.Origin, new Vector3d(cPlane.ZAxis) * 10.0), zColor);

      e.Display.EnableDepthWriting(false);
      e.Display.EnableDepthTesting(false);
    }
  }
}
