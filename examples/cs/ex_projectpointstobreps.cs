using Rhino;
using Rhino.Commands;
using System.Collections.Generic;
using Rhino.Geometry;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("B4B659D8-DAA0-4970-8149-E4D5C2CF99B7")]
  public class ex_projectpointstobreps : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csProjPtsToBreps"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("select surface");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface | Rhino.DocObjects.ObjectType.PolysrfFilter;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      var brep = gs.Object(0).Brep();
      if (brep == null)
        return Result.Failure;

      var pts = Rhino.Geometry.Intersect.Intersection.ProjectPointsToBreps(
                  new List<Brep> {brep}, // brep on which to project
                  new List<Point3d> {new Point3d(0, 0, 0), new Point3d(3,0,3), new Point3d(-2,0,-2)}, // some random points to project
                  new Vector3d(0, 1, 0), // project on Y axis
                  doc.ModelAbsoluteTolerance);

      if (pts != null && pts.Length > 0)
      {
        foreach (var pt in pts)
        {
          doc.Objects.AddPoint(pt);
        }
      }
      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
  }
}