using Rhino;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("E0699E15-C801-4FC9-AE0A-17B5B9168CD0")]
  public class ex_deleteblock : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csDeleteInstanceDefinition"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      // Get the name of the instance definition to rename
      string idefName = "";
      var rc = Rhino.Input.RhinoGet.GetString("Name of block to delete", true, ref idefName);
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

      // Verify instance definition can be deleted
      if (idef.IsReference) {
        RhinoApp.WriteLine(String.Format("Unable to delete block \"{0}\".", idefName));
        return Rhino.Commands.Result.Nothing;
      }

      // delete block and all references
      if (!doc.InstanceDefinitions.Delete(idef.Index, true, true)) {
        RhinoApp.WriteLine(String.Format("Could not delete {0} block", idef.Name));
        return Rhino.Commands.Result.Failure;
      }

      return Rhino.Commands.Result.Success;
    }
  }
}