using Rhino;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("76B20E41-A462-4C75-8FCC-4E07AE5E14BB")]
  public class ex_renameblock : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csRenameInstanceDefinition"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // Get the name of the instance definition to rename
      string idefName = "";
      var rc = Rhino.Input.RhinoGet.GetString("Name of block to rename", true, ref idefName);
      if (rc != Rhino.Commands.Result.Success)
        return rc;
      if (String.IsNullOrWhiteSpace(idefName))
        return Rhino.Commands.Result.Nothing;
     
      // Verify instance definition exists
      var idef = doc.InstanceDefinitions.Find(idefName, true);
      if (idef == null) {
        RhinoApp.WriteLine(String.Format("Block \"{0}\" not found.", idefName));
        return Rhino.Commands.Result.Nothing;
      }

      // Verify instance definition is rename-able
      if (idef.IsDeleted || idef.IsReference) {
        RhinoApp.WriteLine(String.Format("Unable to rename block \"{0}\".", idefName));
        return Rhino.Commands.Result.Nothing;
      }
     
      // Get the new instance definition name
      string idefNewName = "";
      rc = Rhino.Input.RhinoGet.GetString("Name of block to rename", true, ref idefNewName);
      if (rc != Rhino.Commands.Result.Success)
        return rc;
      if (String.IsNullOrWhiteSpace(idefNewName))
        return Rhino.Commands.Result.Nothing;

      // Verify the new instance definition name is not already in use
      var existingIdef = doc.InstanceDefinitions.Find(idefNewName, true);
      if (existingIdef != null && !existingIdef.IsDeleted) {
        RhinoApp.WriteLine(String.Format("Block \"{0}\" already exists.", existingIdef));
        return Rhino.Commands.Result.Nothing;
      }
     
      // change the block name
      if (!doc.InstanceDefinitions.Modify(idef.Index, idefNewName, idef.Description, true)) {
        RhinoApp.WriteLine(String.Format("Could not rename {0} to {1}", idef.Name, idefNewName));
        return Rhino.Commands.Result.Failure;
      }

      return Rhino.Commands.Result.Success;
    }
  }
}