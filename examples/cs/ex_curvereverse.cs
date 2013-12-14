using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.DocObjects;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("B2BD61F7-FFB8-46CA-A03D-92F4811C0D98")]
  public class ReverseCurveCommand : Command
  {
    public override string EnglishName { get { return "csReverseCurve"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      ObjRef[] obj_refs; 
      var rc = RhinoGet.GetMultipleObjects("Select curves to reverse", true, ObjectType.Curve, out obj_refs);
      if (rc != Result.Success)
        return rc;

      foreach (var obj_ref in obj_refs)
      {
        var curve_copy = obj_ref.Curve().DuplicateCurve();
        if (curve_copy != null)
        {
          curve_copy.Reverse();
          doc.Objects.Replace(obj_ref, curve_copy);
        }
      }
      return Result.Success;
    }
  }
}