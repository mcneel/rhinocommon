using System;
using Rhino;
using Rhino.Geometry.Intersect;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino.Commands;
using System.Collections.Generic;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("D3E509D4-3791-42C1-A136-FFBA37359290")]
  public class CurveSurfaceIntersectCommand : Command
  {
    public override string EnglishName { get { return "csCurveSurfaceIntersect"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var gs = new GetObject();
      gs.SetCommandPrompt("select surface");
      gs.GeometryFilter = ObjectType.Surface;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      var surface = gs.Object(0).Surface();

      var gc = new GetObject();
      gc.SetCommandPrompt("select curve");
      gc.GeometryFilter = ObjectType.Curve;
      gc.DisablePreSelect();
      gc.SubObjectSelect = false;
      gc.Get();
      if (gc.CommandResult() != Result.Success)
        return gc.CommandResult();
      var curve = gc.Object(0).Curve();

      if (surface == null || curve == null)
        return Result.Failure;

      var tolerance = doc.ModelAbsoluteTolerance;

      var curveIntersections = Intersection.CurveSurface(curve, surface, tolerance, tolerance);
      if (curveIntersections != null)
      {
        var addedObjects = new List<Guid>();
        foreach (var curveIntersection in curveIntersections)
        {
          if (curveIntersection.IsOverlap)
          {
            double t0;
            double t1;
            curve.ClosestPoint(curveIntersection.PointA, out t0);
            curve.ClosestPoint(curveIntersection.PointA2, out t1);
            var overlapCurve = curve.DuplicateCurve().Trim(t0, t1);
            addedObjects.Add(doc.Objects.AddCurve(overlapCurve));
          }
          else // IsPoint
          {
            addedObjects.Add(doc.Objects.AddPoint(curveIntersection.PointA));
          }
        }
        if (addedObjects.Count > 0)
          doc.Objects.Select(addedObjects);
      }

      doc.Views.Redraw();

      return Result.Success;
    }
  }
}