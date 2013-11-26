using System;
using System.Collections.Generic;
using System.Linq;
using Rhino;
using Rhino.Commands;
using Rhino.Geometry;
using Rhino.Geometry.Intersect;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("F26DAC86-F6D6-46D7-9796-8770E6B51F18")]
  public class ex_elevation : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csFurthestZOnSrfGivenXY"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      #region user input
      // select a surface
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("select surface");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      // get the brep
      var brep = gs.Object(0).Brep();
      if (brep == null)
        return Result.Failure;

      // get X and Y
      double x = 0.0, y = 0.0;
      var gx = new Rhino.Input.Custom.GetNumber();
      gx.SetCommandPrompt("value of X coordinate");
      gx.SetDefaultNumber(x);
      gx.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      x = gx.Number();

      var gy = new Rhino.Input.Custom.GetNumber();
      gy.SetCommandPrompt("value of Y coordinate");
      gy.SetDefaultNumber(y);
      gy.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      y = gy.Number();
      #endregion
      
      // an earlier version of this sample used a curve-brep intersection to find Z
      //var maxZ = maxZIntersectionMethod(brep, x, y, doc.ModelAbsoluteTolerance);

      // projecting points is another way to find Z
      var maxZ = maxZProjectionMethod(brep, x, y, doc.ModelAbsoluteTolerance);

      if (maxZ != null)
      {
        RhinoApp.WriteLine(String.Format("Maximum surface Z coordinate at X={0}, Y={1} is {2}", x, y, maxZ));
        doc.Objects.AddPoint(new Point3d(x, y, maxZ.Value));
        doc.Views.Redraw();
      }
      else
        RhinoApp.WriteLine(String.Format("no maximum surface Z coordinate at X={0}, Y={1} found.", x, y));

      return Rhino.Commands.Result.Success;
    }

    private double? maxZProjectionMethod(Brep brep, double x, double y, double tolerance)
    {
      double? maxZ = null;
      var breps = new List<Brep> {brep};
      var points = new List<Point3d> {new Point3d(x, y, 0)};
      // grab all the points projected in Z dir.  Aggregate finds furthest Z from XY plane
      try {
        maxZ = (from pt in Intersection.ProjectPointsToBreps(breps, points, new Vector3d(0, 0, 1), tolerance) select pt.Z)
          .Aggregate((z1, z2) => Math.Abs(z1) > Math.Abs(z2) ? z1 : z2);
      } catch (InvalidOperationException ioe) {/*Sequence contains no elements*/}
      return maxZ;
    }

    private double? maxZIntersectionMethod(Brep brep, double x, double y, double tolerance)
    {
      double? maxZ = null;

      var bbox = brep.GetBoundingBox(true);
      var maxDistFromXY = (from corner in bbox.GetCorners() select corner.Z)
                          // furthest Z from XY plane.  Max() doesn't work because of possible negative Z values
                          .Aggregate((z1, z2) => Math.Abs(z1) > Math.Abs(z2) ? z1 : z2);
      // multiply distance by 2 to make sure line intersects completely
      var lineCurve = new LineCurve(new Point3d(x, y, 0), new Point3d(x, y, maxDistFromXY*2));

      Curve[] overlapCurves;
      Point3d[] interPoints;
      if (Intersection.CurveBrep(lineCurve, brep, tolerance, out overlapCurves, out interPoints))
      {
        if (overlapCurves.Length > 0 || interPoints.Length > 0)
        {
          // grab all the points resulting frem the intersection. 
          //    1st set: points from overlapping curves, 
          //    2nd set: points when there was no overlap
          //    .Aggregate: furthest Z from XY plane.
          maxZ = (from c in overlapCurves select Math.Abs(c.PointAtEnd.Z) > Math.Abs(c.PointAtStart.Z) ? c.PointAtEnd.Z : c.PointAtStart.Z)
                 .Union
                 (from p in interPoints select p.Z)
                 .Aggregate((z1, z2) => Math.Abs(z1) > Math.Abs(z2) ? z1 : z2);
        }
      }
      return maxZ;
    }
  }
}