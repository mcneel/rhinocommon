using System;
using Rhino;
using Rhino.Geometry;
using Rhino.Input.Custom;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("DA8EA15E-977F-4FB3-8123-235578D18548")]
  public class ex_curveclosestpoint : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csFindCurveParameterAtPoint"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      Rhino.DocObjects.ObjRef objref;
      var rc = Rhino.Input.RhinoGet.GetOneObject("Select curve", true, Rhino.DocObjects.ObjectType.Curve,out objref);
      if(rc!= Rhino.Commands.Result.Success)
        return rc;
      var crv = objref.Curve();
      if( crv==null )
        return Rhino.Commands.Result.Failure;

      var gp = new Rhino.Input.Custom.GetPoint();
      gp.SetCommandPrompt("Pick a location on the curve");
      gp.Constrain(crv, false);
      gp.Get();
      if (gp.CommandResult() != Rhino.Commands.Result.Success)
        return gp.CommandResult();

      var p = gp.Point();
      double cp;
      if (crv.ClosestPoint(p, out cp))
      {
        Rhino.RhinoApp.WriteLine(String.Format("point: ({0},{1},{2}), parameter: {3}", p.X, p.Y, p.Z, cp));
        doc.Objects.AddPoint(p);
        doc.Views.Redraw();
      }
      return Rhino.Commands.Result.Success;
    }
  }
}
