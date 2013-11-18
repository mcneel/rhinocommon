using Rhino;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino.Input.Custom;

namespace examples_cs
{
  class DrawArrowHeadsConduit : Rhino.Display.DisplayConduit
  {
    private Line _line;
    private int _screenSize;
    private double _worldSize;

    public DrawArrowHeadsConduit(Line line, int screenSize, double worldSize)
    {
      _line = line;
      _screenSize = screenSize;
      _worldSize = worldSize;
    }

    protected override void DrawForeground(Rhino.Display.DrawEventArgs e)
    {
      e.Display.DrawArrow(_line, System.Drawing.Color.Black, _screenSize, _worldSize);
    }
  }

  [System.Runtime.InteropServices.Guid("A7236E94-85BD-4D63-9950-19C268E63661")]
  public class ex_conduitarrowheads : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csDrawArrowHeads"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // get arrow head size
      var go = new Rhino.Input.Custom.GetOption();
      go.SetCommandPrompt("ArrowHead length in screen size (pixles) or world size (percentage of arrow lenght)?");
      go.AddOption("screen");
      go.AddOption("world");
      go.Get();
      if (go.CommandResult() != Rhino.Commands.Result.Success)
        return go.CommandResult();

      int screenSize = 0;
      double worldSize = 0.0;
      if (go.Option().EnglishName == "screen")
      {
        var gi = new Rhino.Input.Custom.GetInteger();
        gi.SetLowerLimit(0,true);
        gi.SetCommandPrompt("Length of arrow head in pixels");
        gi.Get();
        if (gi.CommandResult() != Rhino.Commands.Result.Success)
          return gi.CommandResult();
        screenSize = gi.Number();
      }
      else
      {
        var gi = new Rhino.Input.Custom.GetInteger();
        gi.SetLowerLimit(0, true);
        gi.SetUpperLimit(100, false);
        gi.SetCommandPrompt("Lenght of arrow head in percentage of total arrow lenght");
        gi.Get();
        if (gi.CommandResult() != Rhino.Commands.Result.Success)
          return gi.CommandResult();
        worldSize = gi.Number()/100.0;
      }


      // get arrow start and end points
      var gp = new Rhino.Input.Custom.GetPoint();
      gp.SetCommandPrompt("Start of line");
      gp.Get();
      if (gp.CommandResult() != Rhino.Commands.Result.Success)
        return gp.CommandResult();
      var ptStart = gp.Point();
  
      gp.SetCommandPrompt("End of line");
      gp.SetBasePoint(ptStart, false);
      gp.DrawLineFromPoint(ptStart, true);
      gp.Get();
      if (gp.CommandResult() != Rhino.Commands.Result.Success)
        return gp.CommandResult();
      var ptEnd = gp.Point();

      var v = ptEnd - ptStart;
      if (v.IsTiny(Rhino.RhinoMath.ZeroTolerance))
        return Rhino.Commands.Result.Nothing;

      var line = new Line(ptStart, ptEnd);

      var conduit = new DrawArrowHeadsConduit(line, screenSize, worldSize);
      // toggle conduit on/off
      conduit.Enabled = !conduit.Enabled;
      RhinoApp.WriteLine("draw arrowheads conduit enabled = {0}", conduit.Enabled);

      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
  }
}

