using Rhino;
using System;
using System.Linq;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("71692574-73D4-4665-A617-72A937EBADEE")]
  public class ex_printinstancedefinitions : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csInstanceDefinitionNames"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var idefNames = (from idef in doc.InstanceDefinitions 
                       where idef != null && !idef.IsDeleted
                       select idef.Name);

      foreach (var n in idefNames)
        RhinoApp.WriteLine(String.Format("Instance definition = {0}", n));

      return Rhino.Commands.Result.Success;
    }
  }
}