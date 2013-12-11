using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using Rhino.Geometry;
using System.Drawing;
using Rhino.Input;

namespace examples_cs
{
  class DeviationConduit : Rhino.Display.DisplayConduit
  {
    private readonly Curve _curveA;
    private readonly Curve _curveB;
    private readonly Point3d _minDistPointA ;
    private readonly Point3d _minDistPointB ;
    private readonly Point3d _maxDistPointA ;
    private readonly Point3d _maxDistPointB ;

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
      e.Display.DrawCurve(_curveA, Color.Red);
      e.Display.DrawCurve(_curveB, Color.Red);

      e.Display.DrawPoint(_minDistPointA, Color.LawnGreen);
      e.Display.DrawPoint(_minDistPointB, Color.LawnGreen);
      e.Display.DrawLine(new Line(_minDistPointA, _minDistPointB), Color.LawnGreen);
      e.Display.DrawPoint(_maxDistPointA, Color.Red);
      e.Display.DrawPoint(_maxDistPointB, Color.Red);
      e.Display.DrawLine(new Line(_maxDistPointA, _maxDistPointB), Color.Red);
    }
  }

  [System.Runtime.InteropServices.Guid("B2A5B553-FAB5-48B9-B4C8-0EF653983D04")]
  public class CurveDeviationCommand : Command
  {
    public override string EnglishName { get { return "csCurveDeviation"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      doc.Objects.UnselectAll();

      ObjRef objRef1;
      var rc1 = RhinoGet.GetOneObject("first curve", true, ObjectType.Curve, out objRef1);
      if (rc1 != Result.Success)
        return rc1;
      Curve curveA = null;
      if (objRef1 != null)
        curveA = objRef1.Curve();
      if (curveA == null)
        return Result.Failure;

      // Since you already selected a curve if you don't unselect it
      // the next GetOneObject won't stop as it considers that curve 
      // input, i.e., curveA and curveB will point to the same curve.
      // Another option would be to use an instance of Rhino.Input.Custom.GetObject
      // instead of Rhino.Input.RhinoGet as GetObject has a DisablePreSelect() method.
      doc.Objects.UnselectAll();

      ObjRef objRef2;
      var rc2 = RhinoGet.GetOneObject("second curve", true, ObjectType.Curve, out objRef2);
      if (rc2 != Result.Success)
        return rc2;
      Curve curveB = null;
      if (objRef2 != null)
        curveB = objRef2.Curve();
      if (curveB == null)
        return Result.Failure;

      var tolerance = doc.ModelAbsoluteTolerance;

      double maxDistance;
      double maxDistanceParameterA;
      double maxDistanceParameterB;
      double minDistance;
      double minDistanceParameterA;
      double minDistanceParameterB;

      DeviationConduit conduit;
      if (!Curve.GetDistancesBetweenCurves(curveA, curveB, tolerance, out maxDistance, 
                out maxDistanceParameterA, out maxDistanceParameterB,
                out minDistance, out minDistanceParameterA, out minDistanceParameterB))
      {
        Rhino.RhinoApp.WriteLine("Unable to find overlap intervals.");
        return Result.Success;
      } else
      {
        if (minDistance <= RhinoMath.ZeroTolerance)
          minDistance = 0.0;
        var maxDistPtA = curveA.PointAt(maxDistanceParameterA);
        var maxDistPtB = curveB.PointAt(maxDistanceParameterB);
        var minDistPtA = curveA.PointAt(minDistanceParameterA);
        var minDistPtB = curveB.PointAt(minDistanceParameterB);

        conduit = new DeviationConduit(curveA, curveB, minDistPtA, minDistPtB, maxDistPtA, maxDistPtB) {Enabled = true};
        doc.Views.Redraw();

        RhinoApp.WriteLine(string.Format("Minimum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", minDistance, 
          minDistPtA.X, minDistPtA.Y, minDistPtA.Z, minDistPtB.X, minDistPtB.Y, minDistPtB.Z));
        RhinoApp.WriteLine(string.Format("Maximum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", maxDistance, 
          maxDistPtA.X, maxDistPtA.Y, maxDistPtA.Z, maxDistPtB.X, maxDistPtB.Y, maxDistPtB.Z));
      }

      string str = "";
      var s = RhinoGet.GetString("Press Enter when done", true, ref str);
      conduit.Enabled = false;

      return Result.Success;
    }
  }
}
