using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("FCD0CD1E-B92C-44A9-B17C-05E7044650C8")]
  public class IntersectLineCircleCommand : Command
  {
    public override string EnglishName { get { return "csIntersectLineCircle"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      Circle circle;
      var rc = RhinoGet.GetCircle(out circle);
      if (rc != Result.Success)
        return rc;
      doc.Objects.AddCircle(circle);
      doc.Views.Redraw();

      Line line;
      rc = RhinoGet.GetLine(out line);
      if (rc != Result.Success)
        return rc;
      doc.Objects.AddLine(line);
      doc.Views.Redraw();

      double t1, t2;
      Point3d point1, point2;
      var lineCircleIntersect = Intersection.LineCircle(line, circle, out t1, out point1, out t2, out point2);
      string msg = "";
      switch (lineCircleIntersect) {
        case LineCircleIntersection.None:
          msg = "line does not intersect circle";
          break;
        case LineCircleIntersection.Single:
          msg = string.Format("line intersects circle at point ({0},{1},{2})", point1.X, point1.Y, point1.Z);
          doc.Objects.AddPoint(point1);
          break;
        case LineCircleIntersection.Multiple:
          msg = string.Format("line intersects circle at points ({0},{1},{2}) and ({3},{4},{5})",
            point1.X, point1.Y, point1.Z, point2.X, point2.Y, point2.Z);
          doc.Objects.AddPoint(point1);
          doc.Objects.AddPoint(point2);
          break;
      }
      RhinoApp.WriteLine(msg);
      doc.Views.Redraw();
      return Result.Success;
    }
  }
}