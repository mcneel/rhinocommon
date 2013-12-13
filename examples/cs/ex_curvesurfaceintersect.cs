using Rhino;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("D3E509D4-3791-42C1-A136-FFBA37359290")]
  public class CurveSurfaceIntersectCommand : Command
  {
    public override string EnglishName { get { return "csCurveSurfaceIntersect"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var gs = new GetObject();
      gs.SetCommandPrompt("select brep");
      gs.GeometryFilter = ObjectType.Brep;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      var brep = gs.Object(0).Brep();

      var gc = new GetObject();
      gc.SetCommandPrompt("select curve");
      gc.GeometryFilter = ObjectType.Curve;
      gc.DisablePreSelect();
      gc.SubObjectSelect = false;
      gc.Get();
      if (gc.CommandResult() != Result.Success)
        return gc.CommandResult();
      var curve = gc.Object(0).Curve();

      if (brep == null || curve == null)
        return Result.Failure;

      var tolerance = doc.ModelAbsoluteTolerance;

      Point3d[] intersectionPoints;
      Curve[] overlapCurves;
      if (!Intersection.CurveBrep(curve, brep, tolerance, out overlapCurves, out intersectionPoints))
      {
        RhinoApp.WriteLine("curve brep intersection failed");
        return Result.Nothing;
      }

      foreach (var overlapCurve in overlapCurves)
        doc.Objects.AddCurve(overlapCurve);
      foreach (var intersectionPoint in intersectionPoints)
        doc.Objects.AddPoint(intersectionPoint);

      RhinoApp.WriteLine(string.Format("{0} overlap curves, and {1} intersection points", overlapCurves.Length, intersectionPoints.Length));
      doc.Views.Redraw();

      return Result.Success;
    }
  }
}