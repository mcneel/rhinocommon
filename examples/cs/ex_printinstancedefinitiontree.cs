using Rhino;
using Rhino.DocObjects;
using Rhino.FileIO;
using System;
using System.Linq;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("7C98E0BC-C177-46E1-A9AE-092C63911450")]
  public class ex_printinstancedefinitiontree : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csInstanceDefinitionTree"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var idefs = doc.InstanceDefinitions;
      var idefCount = idefs.Count;

      if (idefCount == 0)
      {
        RhinoApp.WriteLine("Document contains no instance definitions.");
        return Rhino.Commands.Result.Nothing;
      }

      var dump = new TextLog();
      dump.IndentSize = 4;

      for (int i = 0; i < idefCount; i++)
        DumpInstanceDefinition(idefs[i], ref dump, true);

      RhinoApp.WriteLine(dump.ToString());

      return Rhino.Commands.Result.Success;
    }

    private void DumpInstanceDefinition(InstanceDefinition idef, ref TextLog dump, bool bRoot)
    {
      if (idef != null && !idef.IsDeleted)
      {
        string node;
        if (bRoot)
          node = "\u2500";
        else
          node = "\u2514";
        dump.Print(String.Format("{0} Instance definition {1} = {2}\n", node, idef.Index, idef.Name));

        int idefObjectCount = idef.ObjectCount;
        if (idefObjectCount  > 0)
        {
          dump.PushIndent();
          for (int i = 0; i < idefObjectCount ; i++)
          {
            var obj = idef.Object(i);
            if (obj != null && obj is InstanceObject)
              DumpInstanceDefinition((obj as InstanceObject).InstanceDefinition, ref dump, false); // Recursive...
            else
              dump.Print(String.Format("\u2514 Object {0} = {1}\n", i, obj.ShortDescription(false)));
          }
          dump.PopIndent();
        }
      }
    }
  }
}

