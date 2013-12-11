using Rhino;
using Rhino.Input;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("76B20E41-A462-4C75-8FCC-4E07AE5E14BB")]
  public class RenameBlockCommand : Command
  {
    public override string EnglishName { get { return "csRenameInstanceDefinition"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Get the name of the insance definition to rename
      string instanceDefinitionName = "";
      var rc = RhinoGet.GetString("Name of block to rename", true, ref instanceDefinitionName);
      if (rc != Result.Success)
        return rc;
      if (string.IsNullOrWhiteSpace(instanceDefinitionName))
        return Result.Nothing;
     
      // Verify instance definition exists
      var instanceDefinition = doc.InstanceDefinitions.Find(instanceDefinitionName, true);
      if (instanceDefinition == null) {
        RhinoApp.WriteLine(string.Format("Block \"{0}\" not found.", instanceDefinitionName));
        return Result.Nothing;
      }

      // Verify instance definition is rename-able
      if (instanceDefinition.IsDeleted || instanceDefinition.IsReference) {
        RhinoApp.WriteLine(string.Format("Unable to rename block \"{0}\".", instanceDefinitionName));
        return Result.Nothing;
      }
     
      // Get the new instance definition name
      string instanceDefinitionNewName = "";
      rc = RhinoGet.GetString("Name of block to rename", true, ref instanceDefinitionNewName);
      if (rc != Result.Success)
        return rc;
      if (string.IsNullOrWhiteSpace(instanceDefinitionNewName))
        return Result.Nothing;

      // Verify the new instance definition name is not already in use
      var existingInstanceDefinition = doc.InstanceDefinitions.Find(instanceDefinitionNewName, true);
      if (existingInstanceDefinition != null && !existingInstanceDefinition.IsDeleted) {
        RhinoApp.WriteLine(string.Format("Block \"{0}\" already exists.", existingInstanceDefinition));
        return Result.Nothing;
      }
     
      // change the block name
      if (!doc.InstanceDefinitions.Modify(instanceDefinition.Index, instanceDefinitionNewName, instanceDefinition.Description, true)) {
        RhinoApp.WriteLine(string.Format("Could not rename {0} to {1}", instanceDefinition.Name, instanceDefinitionNewName));
        return Result.Failure;
      }

      return Result.Success;
    }
  }
}