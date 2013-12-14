using Rhino;
using Rhino.Input;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("E0699E15-C801-4FC9-AE0A-17B5B9168CD0")]
  public class DeleteBlockCommand : Command
  {
    public override string EnglishName { get { return "csDeleteInstanceDefinition"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      // Get the name of the instance definition to rename
      string instance_definition_name = "";
      var rc = RhinoGet.GetString("Name of block to delete", true, ref instance_definition_name);
      if (rc != Result.Success)
        return rc;
      if (string.IsNullOrWhiteSpace(instance_definition_name))
        return Result.Nothing;
     
      // Verify instance definition exists
      var instance_definition = doc.InstanceDefinitions.Find(instance_definition_name, true);
      if (instance_definition == null) {
        RhinoApp.WriteLine(string.Format("Block \"{0}\" not found.", instance_definition_name));
        return Result.Nothing;
      }

      // Verify instance definition can be deleted
      if (instance_definition.IsReference) {
        RhinoApp.WriteLine(string.Format("Unable to delete block \"{0}\".", instance_definition_name));
        return Result.Nothing;
      }

      // delete block and all references
      if (!doc.InstanceDefinitions.Delete(instance_definition.Index, true, true)) {
        RhinoApp.WriteLine(string.Format("Could not delete {0} block", instance_definition.Name));
        return Result.Failure;
      }

      return Result.Success;
    }
  }
}