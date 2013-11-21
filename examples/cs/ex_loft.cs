using Rhino;
using Rhino.Commands;
using System.Collections.Generic;
using Rhino.Geometry;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("5D48CD31-B300-42B4-98F0-A7A4004B5227")]
  public class ex_loft : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csLoft"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // select curves to loft
      var gs = new Rhino.Input.Custom.GetObject();
      gs.SetCommandPrompt("select curves to loft");
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      gs.DisablePreSelect();
      gs.SubObjectSelect = false;
      gs.GetMultiple(2, 0);
      if (gs.CommandResult() != Result.Success)
        return gs.CommandResult();

      var crvs = new List<Curve>();
      foreach (var obj in gs.Objects())
        crvs.Add(obj.Curve());

      var breps = Rhino.Geometry.Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Tight, false);
      foreach (var brep in breps)
        doc.Objects.AddBrep(brep);

      doc.Views.Redraw();
      return Rhino.Commands.Result.Success;
    }
  }
}