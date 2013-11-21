using Rhino;
using Rhino.Geometry;
using Rhino.Commands;
using Rhino.Input.Custom;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("B2BD61F7-FFB8-46CA-A03D-92F4811C0D98")]
  public class ex_curvereverse : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csReverseCurve"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      Rhino.Input.Custom.GetObject go = new Rhino.Input.Custom.GetObject();
      go.SetCommandPrompt("Select curves to reverse");
      go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve;
      go.GetMultiple(1, 0);
      if (go.CommandResult() != Rhino.Commands.Result.Success)
        return go.CommandResult();
  
      for (int i = 0; i < go.ObjectCount; i++)
      {
        var objRef = go.Object(i);
        var crv = objRef.Curve();
        var dup = crv.DuplicateCurve();
        if (dup != null)
        {
          dup.Reverse();
          doc.Objects.Replace(objRef, dup);
        }
      }      

      return Rhino.Commands.Result.Success;
    }
  }
}