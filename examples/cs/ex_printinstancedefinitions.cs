using Rhino;
using Rhino.Commands;
using System;
using System.Linq;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("71692574-73D4-4665-A617-72A937EBADEE")]
  public class InstanceDefinitionNamesCommand : Command
  {
    public override string EnglishName { get { return "csInstanceDefinitionNames"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var instanceDefinitionNames = (from instanceDefinition in doc.InstanceDefinitions 
                                     where instanceDefinition != null && !instanceDefinition.IsDeleted
                                     select instanceDefinition.Name);

      foreach (var n in instanceDefinitionNames)
        RhinoApp.WriteLine(String.Format("Instance definition = {0}", n));

      return Result.Success;
    }
  }
}