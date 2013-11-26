using System.Runtime.InteropServices;
using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using System.Collections.Generic;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("9F849A9F-ED1D-41F8-9413-C2104939C126")]
  public class ex_pickobjects : Rhino.Commands.Command
  {
    private List<ConduitPoint> conduitPoints = new List<ConduitPoint>();

    public override string EnglishName { get { return "csPickPoints"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var conduit = new PointsConduit(conduitPoints);
      conduit.Enabled = true;

      var gp = new Rhino.Input.Custom.GetPoint();
      while (true)
      {
        gp.SetCommandPrompt("click location to create point. (<ESC> exit)");
        gp.AcceptNothing(true);
        gp.Get();
        if (gp.CommandResult() != Rhino.Commands.Result.Success)
          break;
        conduitPoints.Add(new ConduitPoint(gp.Point()));
        doc.Views.Redraw();
      }

      var gcp = new GetConduitPoint(conduitPoints);
      while (true)
      {
        gcp.SetCommandPrompt("select conduit point. (<ESC> to exit)");
        gcp.AcceptNothing(true);
        gcp.Get(true);
        doc.Views.Redraw();
        if (gcp.CommandResult() != Rhino.Commands.Result.Success)
          break;
      }

      return Rhino.Commands.Result.Success;
    }
  }

  public class ConduitPoint
  {
    public ConduitPoint(Point3d point)
    {
      Color = System.Drawing.Color.White;
      Point = point;
    }
    public System.Drawing.Color Color { get; set; }
    public Point3d Point { get; set; }
  }

  public class GetConduitPoint : GetPoint
  {
    private List<ConduitPoint> _conduitPoints;
 
    public GetConduitPoint(List<ConduitPoint> conduitPoints )
    {
      _conduitPoints = conduitPoints;
    }

    protected override void OnMouseDown(GetPointMouseEventArgs e)
    {
      base.OnMouseDown(e);
      var picker = new PickContext();
      picker.View = e.Viewport.ParentView;

      picker.PickStyle = PickStyle.PointPick;

      var xform = e.Viewport.GetPickTransform(e.WindowPoint);
      picker.SetPickTransform(xform);

      foreach (var cp in _conduitPoints)
      {
        double depth;
        double distance;
        if (picker.PickFrustumTest(cp.Point, out depth, out distance))
          cp.Color = System.Drawing.Color.Red;
        else
          cp.Color = System.Drawing.Color.White;
      }
    }
  }

  class PointsConduit : Rhino.Display.DisplayConduit
  {
    private List<ConduitPoint> _conduitPoints;
 
    public PointsConduit(List<ConduitPoint> conduitPoints )
    {
      _conduitPoints = conduitPoints;
    }

    protected override void DrawForeground(Rhino.Display.DrawEventArgs e)
    {
      if (_conduitPoints != null)
        foreach (var cp in _conduitPoints) 
        e.Display.DrawPoint(cp.Point, PointStyle.Simple, 3, cp.Color);
    }
  }
}