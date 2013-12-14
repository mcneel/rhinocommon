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
      var instance_definition_names = (from instance_definition in doc.InstanceDefinitions 
                                       where instance_definition != null && !instance_definition.IsDeleted
                                       select instance_definition.Name);

      foreach (var n in instance_definition_names)
        RhinoApp.WriteLine(String.Format("Instance definition = {0}", n));

      return Result.Success;
    }
  }
}