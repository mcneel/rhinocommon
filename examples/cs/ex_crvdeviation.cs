using System;
using System.Runtime.ExceptionServices;
using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Collections.Generic;
using Rhino.Input.Custom;

namespace examples_cs
{
  class DeviationConduit : Rhino.Display.DisplayConduit
  {
    private Curve _curveA;
    private Curve _curveB;
    private Point3d _minDistPointA ;
    private Point3d _minDistPointB ;
    private Point3d _maxDistPointA ;
    private Point3d _maxDistPointB ;

    public DeviationConduit(Curve curveA, Curve curveB, Point3d minDistPointA, Point3d minDistPointB, Point3d maxDistPointA, Point3d maxDistPointB)
    {
      _curveA = curveA;
      _curveB = curveB;
      _minDistPointA = minDistPointA;
      _minDistPointB = minDistPointB;
      _maxDistPointA = maxDistPointA;
      _maxDistPointB = maxDistPointB;
    }

    protected override void DrawForeground(Rhino.Display.DrawEventArgs e)
    {
      e.Display.DrawCurve(_curveA, System.Drawing.Color.Red);
      e.Display.DrawCurve(_curveB, System.Drawing.Color.Red);

      e.Display.DrawPoint(_minDistPointA, System.Drawing.Color.LawnGreen);
      e.Display.DrawPoint(_minDistPointB, System.Drawing.Color.LawnGreen);
      e.Display.DrawLine(new Line(_minDistPointA, _minDistPointB), System.Drawing.Color.LawnGreen);
      e.Display.DrawPoint(_maxDistPointA, System.Drawing.Color.Red);
      e.Display.DrawPoint(_maxDistPointB, System.Drawing.Color.Red);
      e.Display.DrawLine(new Line(_maxDistPointA, _maxDistPointB), System.Drawing.Color.Red);
    }
  }

  [System.Runtime.InteropServices.Guid("B2A5B553-FAB5-48B9-B4C8-0EF653983D04")]
  public class ex_crvdeviation : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csCrvDeviation"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var gc1 = new Rhino.Input.Custom.GetObject();
      gc1.SetCommandPrompt("first curve");
      gc1.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gc1.AcceptNothing(false);
      gc1.DisablePreSelect();
      gc1.Get();
      if (gc1.CommandResult() != Result.Success)
        return gc1.CommandResult();
      var crvA = gc1.Object(0).Curve();
      if (crvA == null)
        return Result.Failure;

      var gc2 = new Rhino.Input.Custom.GetObject();
      gc2.SetCommandPrompt("second curve");
      gc2.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gc2.AcceptNothing(false);
      gc2.DisablePreSelect();
      gc2.Get();
      if (gc2.CommandResult() != Result.Success)
        return gc2.CommandResult();
      var crvB = gc2.Object(0).Curve();
      if (crvB == null)
        return Result.Failure;

      var tolerance = doc.ModelAbsoluteTolerance;

      double maxDistance;
      double maxDistanceParameterA;
      double maxDistanceParameterB;
      double minDistance;
      double minDistanceParameterA;
      double minDistanceParameterB;

      DeviationConduit conduit;
      if (!Curve.GetDistancesBetweenCurves(crvA, crvB, tolerance, out maxDistance, out maxDistanceParameterA, out maxDistanceParameterB,
                out minDistance, out minDistanceParameterA, out minDistanceParameterB))
      {
        Rhino.RhinoApp.WriteLine("Unable to find overlap intervals.");
        return Rhino.Commands.Result.Success;
      } else
      {
        if (minDistance <= RhinoMath.ZeroTolerance)
          minDistance = 0.0;
        var maxDistPtA = crvA.PointAt(maxDistanceParameterA);
        var maxDistPtB = crvB.PointAt(maxDistanceParameterB);
        var minDistPtA = crvA.PointAt(minDistanceParameterA);
        var minDistPtB = crvB.PointAt(minDistanceParameterB);

        conduit = new DeviationConduit(crvA, crvB, minDistPtA, minDistPtB, maxDistPtA, maxDistPtB);
        conduit.Enabled = true;
        doc.Views.Redraw();

        Rhino.RhinoApp.WriteLine(String.Format("Minimum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", minDistance, 
          minDistPtA.X, minDistPtA.Y, minDistPtA.Z, minDistPtB.X, minDistPtB.Y, minDistPtB.Z));
        Rhino.RhinoApp.WriteLine(String.Format("Maximum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", maxDistance, 
          maxDistPtA.X, maxDistPtA.Y, maxDistPtA.Z, maxDistPtB.X, maxDistPtB.Y, maxDistPtB.Z));
      }

      string str = "";
      var s = Rhino.Input.RhinoGet.GetString("Press Enter when done", true, ref str);
      conduit.Enabled = false;

      return Rhino.Commands.Result.Success;
    }
  }
}
