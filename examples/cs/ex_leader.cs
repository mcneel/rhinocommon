using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Input.Custom;
using System.Collections.Generic;
using System.Linq;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("891F5AE0-DBE0-40A2-8C28-F59B8A757933")]
  public class ex_leader : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csLeader"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var points = new List<Point3d>();
      points.Add(new Point3d(1, 1, 0));
      points.Add(new Point3d(5, 1, 0));
      points.Add(new Point3d(5, 5, 0));
      points.Add(new Point3d(9, 5, 0));

      var xyPlane = Plane.WorldXY;

      var pts2d = new List<Point2d>();
      foreach (var pt3d in points)
      {
        double x, y;
        if (xyPlane.ClosestParameter(pt3d, out x, out y))
        {
          var pt2d = new Point2d(x, y);
          if (pts2d.Count < 1 || pt2d.DistanceTo(pts2d.Last<Point2d>()) > RhinoMath.SqrtEpsilon)
            pts2d.Add(pt2d);
        }
      }

      doc.Objects.AddLeader(xyPlane, pts2d);
      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
  }
}