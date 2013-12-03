using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("5EB284F1-60E6-420A-AE53-6D99732AAE1D")]
  public class ex_customgeometryfilter : Rhino.Commands.Command
  {
    private double _tolerance;
    public override string EnglishName { get { return "csCustomGeoFilter"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      _tolerance = doc.ModelAbsoluteTolerance;
      
      // only use a custom geometry filter if no simpler filter does the job

      // only curves
      var gc = new Rhino.Input.Custom.GetObject();
      gc.SetCommandPrompt("select curve");
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gc.DisablePreSelect();
      gc.SubObjectSelect = false;
      gc.Get();
      if (gc.CommandResult() != Result.Success)
        return gc.CommandResult();
      if (null == gc.Object(0).Curve())
        return Result.Failure;
      Rhino.RhinoApp.WriteLine("curve was selected");

      // only closed curves
      var gcc = new Rhino.Input.Custom.GetObject();
      gcc.SetCommandPrompt("select closed curve");
      gcc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gcc.GeometryAttributeFilter = GeometryAttributeFilter.ClosedCurve;
      gcc.DisablePreSelect();
      gcc.SubObjectSelect = false;
      gcc.Get();
      if (gcc.CommandResult() != Result.Success)
        return gcc.CommandResult();
      if (null == gcc.Object(0).Curve())
        return Result.Failure;
      Rhino.RhinoApp.WriteLine("closed curve was selected");

      // only circles with a radius of 10
      var gcc10 = new Rhino.Input.Custom.GetObject();
      gcc10.SetCommandPrompt("select circle with radius of 10");
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gcc10.SetCustomGeometryFilter(circleWithRadiusOf10GeometryFilter); // custom geometry filter
      gcc10.DisablePreSelect();
      gcc10.SubObjectSelect = false;
      gcc10.Get();
      if (gcc10.CommandResult() != Result.Success)
        return gcc10.CommandResult();
      if (null == gcc10.Object(0).Curve())
        return Result.Failure;
      Rhino.RhinoApp.WriteLine("circle with radius of 10 was selected");

      return Rhino.Commands.Result.Success;
    }

    private bool circleWithRadiusOf10GeometryFilter (Rhino.DocObjects.RhinoObject rhObject, GeometryBase geometry,
      ComponentIndex componentIndex)
    {
      bool isCircleWithRadiusOf10 = false;
      Circle circle;
      if (geometry is Curve && (geometry as Curve).TryGetCircle(out circle))
        isCircleWithRadiusOf10 = circle.Radius <= 10.0 + _tolerance && circle.Radius >= 10.0 - _tolerance;
      return isCircleWithRadiusOf10;
    }
  }
}
