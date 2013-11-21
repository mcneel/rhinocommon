using System;
using Rhino;
using Rhino.Commands;
using System.Collections.Generic;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("D3E509D4-3791-42C1-A136-FFBA37359290")]
  public class ex_curvesurfaceintersect : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csCrvSrfIntersect"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("select surface");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.Get();
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();
      var srf = gs.Object(0).Surface();

      var gc = new Rhino.Input.Custom.GetObject();
      gc.SetCommandPrompt("select curve");
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gc.DisablePreSelect();
      gc.SubObjectSelect = false;
      gc.Get();
      if (gc.CommandResult() != Result.Success)
        return gc.CommandResult();
      var crv = gc.Object(0).Curve();

      if (srf == null || crv == null)
        return Result.Failure;

      var tol = doc.ModelAbsoluteTolerance;

      var cis = Rhino.Geometry.Intersect.Intersection.CurveSurface(crv, srf, tol, tol);
      if (cis != null)
      {
        var addedObjs = new List<Guid>();
        foreach (var ie in cis)
        {
          if (ie.IsOverlap)
          {
            double t0;
            double t1;
            crv.ClosestPoint(ie.PointA, out t0);
            crv.ClosestPoint(ie.PointA2, out t1);
            var overlapCrv = crv.DuplicateCurve().Trim(t0, t1);
            addedObjs.Add(doc.Objects.AddCurve(overlapCrv));
          }
          else // IsPoint
          {
            addedObjs.Add(doc.Objects.AddPoint(ie.PointA));
          }
        }
        if (addedObjs.Count > 0)
          doc.Objects.Select(addedObjs);
      }

      doc.Views.Redraw();

      return Rhino.Commands.Result.Success;
    }
  }
}