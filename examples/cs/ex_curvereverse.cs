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
      ObjRef[] objRefs; 
      var rc = RhinoGet.GetMultipleObjects("Select curves to reverse", true, ObjectType.Curve, out objRefs);
      if (rc != Result.Success)
        return rc;

      foreach (var objRef in objRefs)
      {
        var curveCopy = objRef.Curve().DuplicateCurve();
        if (curveCopy != null)
        {
          curveCopy.Reverse();
          doc.Objects.Replace(objRef, curveCopy);
        }
      }
      return Result.Success;
    }
  }
}