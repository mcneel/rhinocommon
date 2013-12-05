using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("6F954183-C359-4CEF-94C1-3A108D36B366")]
  public class ex_curvebrepbox : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csBoxFromCrvsBBox"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
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
      var crv = gc.Object(0).Curve();


      var view = doc.Views.ActiveView;
      var plane = view.ActiveViewport.ConstructionPlane();
      // Create a construction plane aligned bounding box
      var bbox = crv.GetBoundingBox(plane);

      if (bbox.IsDegenerate(doc.ModelAbsoluteTolerance) > 0) {
        RhinoApp.WriteLine("the curve's bounding box is degenerate (flat) in at least one direction so a box cannot be created.");
        return Rhino.Commands.Result.Failure;
      }
      var box = new Box(bbox);
      var brep = Brep.CreateFromBox(box);
      doc.Objects.AddBrep(brep);
      doc.Views.Redraw();

      return Rhino.Commands.Result.Success;
    }
  }
}