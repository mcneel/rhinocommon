using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using Rhino.Commands;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("7C98E0BC-C177-46E1-A9AE-092C63911450")]
  public class InstanceDefinitionTreeCommand : Command
  {
    public override string EnglishName { get { return "csInstanceDefinitionTree"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var instanceDefinitions = doc.InstanceDefinitions;
      var instanceDefinitionCount = instanceDefinitions.Count;

      if (instanceDefinitionCount == 0)
      {
        RhinoApp.WriteLine("Document contains no instance definitions.");
        return Result.Nothing;
      }

      var dump = new TextLog();
      dump.IndentSize = 4;

      for (int i = 0; i < instanceDefinitionCount; i++)
        DumpInstanceDefinition(instanceDefinitions[i], ref dump, true);

      RhinoApp.WriteLine(dump.ToString());

      return Result.Success;
    }

    private void DumpInstanceDefinition(InstanceDefinition instanceDefinition, ref TextLog dump, bool isRoot)
    {
      if (instanceDefinition != null && !instanceDefinition.IsDeleted)
      {
        string node;
        node = isRoot ? "─" : "└"; // "\u2500" : "\u2514"
        dump.Print(string.Format("{0} Instance definition {1} = {2}\n", node, instanceDefinition.Index, instanceDefinition.Name));

        if (instanceDefinition.ObjectCount  > 0)
        {
          dump.PushIndent();
          for (int i = 0; i < instanceDefinition.ObjectCount ; i++)
          {
            var obj = instanceDefinition.Object(i);
            if (obj == null) continue;
            if (obj is InstanceObject)
              DumpInstanceDefinition((obj as InstanceObject).InstanceDefinition, ref dump, false); // Recursive...
            else
              dump.Print(string.Format("\u2514 Object {0} = {1}\n", i, obj.ShortDescription(false)));
          }
          dump.PopIndent();
        }
      }
    }
  }
}

