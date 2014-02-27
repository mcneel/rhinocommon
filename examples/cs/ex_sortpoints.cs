using System.Collections.Generic;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;

namespace examples_cs
{
  public class SortPointsCommand : Command
  {
    public override string EnglishName { get { return "csSortPoints"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var points = new List<Point3d>
      {
        new Point3d(0, 0, 0),
        new Point3d(0, 0, 1),
        new Point3d(0, 1, 0),
        new Point3d(0, 1, 1),
        new Point3d(1, 0, 0),
        new Point3d(1, 0, 1),
        new Point3d(1, 1, 0),
        new Point3d(1, 1, 1)
      };

      RhinoApp.WriteLine("Before sort ...");
      foreach (var point in points)
        RhinoApp.WriteLine("point: {0}", point);

      var sorted_points = Point3d.SortAndCullPointList(points, doc.ModelAbsoluteTolerance);

      RhinoApp.WriteLine("After sort ...");
      foreach (var point in sorted_points)
        RhinoApp.WriteLine("point: {0}", point);

      doc.Objects.AddPoints(sorted_points);
      doc.Views.Redraw();
      return Result.Success;
    }
  }
}