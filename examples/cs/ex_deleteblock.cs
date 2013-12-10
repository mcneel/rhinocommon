using Rhino;
using Rhino.Commands;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("E0699E15-C801-4FC9-AE0A-17B5B9168CD0")]
  public class DeleteBlockCommand : Command
  {
    public override string EnglishName { get { return "csDeleteInstanceDefinition"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Get the name of the instance definition to rename
      string instanceDefinitionName = "";
      var rc = Rhino.Input.RhinoGet.GetString("Name of block to delete", true, ref instanceDefinitionName);
      if (rc != Result.Success)
        return rc;
      if (String.IsNullOrWhiteSpace(instanceDefinitionName))
        return Result.Nothing;
     
      // Verify instance definition exists
      var instanceDefinition = doc.InstanceDefinitions.Find(instanceDefinitionName, true);
      if (instanceDefinition == null) {
        RhinoApp.WriteLine(String.Format("Block \"{0}\" not found.", instanceDefinitionName));
        return Result.Nothing;
      }

      // Verify instance definition can be deleted
      if (instanceDefinition.IsReference) {
        RhinoApp.WriteLine(String.Format("Unable to delete block \"{0}\".", instanceDefinitionName));
        return Result.Nothing;
      }

      // delete block and all references
      if (!doc.InstanceDefinitions.Delete(instanceDefinition.Index, true, true)) {
        RhinoApp.WriteLine(String.Format("Could not delete {0} block", instanceDefinition.Name));
        return Result.Failure;
      }

      return Result.Success;
    }
  }
}