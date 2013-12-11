using Rhino;
using Rhino.Input;
using Rhino.Input.Custom;
using Rhino.DocObjects;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("DA8EA15E-977F-4FB3-8123-235578D18548")]
  public class CurveClosestPointCommand : Command
  {
    public override string EnglishName { get { return "csFindCurveParameterAtPoint"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      Rhino.DocObjects.ObjRef objref;
      var rc = RhinoGet.GetOneObject("Select curve", true, ObjectType.Curve,out objref);
      if(rc!= Result.Success)
        return rc;
      var curve = objref.Curve();
      if( curve==null )
        return Result.Failure;

      var gp = new GetPoint();
      gp.SetCommandPrompt("Pick a location on the curve");
      gp.Constrain(curve, false);
      gp.Get();
      if (gp.CommandResult() != Result.Success)
        return gp.CommandResult();

      var point = gp.Point();
      double closestPointParam;
      if (curve.ClosestPoint(point, out closestPointParam))
      {
        RhinoApp.WriteLine(string.Format("point: ({0},{1},{2}), parameter: {3}", point.X, point.Y, point.Z, closestPointParam));
        doc.Objects.AddPoint(point);
        doc.Views.Redraw();
      }
      return Result.Success;
    }
  }
}
