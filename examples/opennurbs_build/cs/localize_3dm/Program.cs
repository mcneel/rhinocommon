using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace localize_3dm
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 1)
      {
        var exe_name = System.AppDomain.CurrentDomain.FriendlyName;
        Console.WriteLine("Syntax: {0} [-out:outputfilename.txt] file1.3dm file2.3dm ...", exe_name);
        return;
      }

      bool verbose_text_dump = false;
      System.IO.TextWriter alternate_output_stream = null;
      for (int i = 0; i < args.Length; i++)
      {
        string arg = args[i];
        // check for -out or /out option
        if (arg.StartsWith("-out:", StringComparison.InvariantCultureIgnoreCase) || arg.StartsWith("/out:", StringComparison.InvariantCultureIgnoreCase))
        {
          // change destination of dump file
          string dump_file_name = arg.Substring(5).Trim();
          alternate_output_stream = new System.IO.StreamWriter(dump_file_name);
          Console.SetOut(alternate_output_stream);
          continue;
        }

        string filename = arg;
        Console.Out.WriteLine("OpenNURBS Archive File:  {0}", filename);
        using (var model = Rhino.FileIO.File3dm.Read(filename))
        {
          if (model == null)
          {
            Console.Out.WriteLine("Unable to open file");
            continue;
          }

          Console.Out.WriteLine("Layers:");
          foreach (var layer in model.Layers)
          {
            Console.Out.WriteLine("  {0} ({1}) color: {2}", layer.FullPath, layer.Name, layer.Color);
          }

          Console.Out.WriteLine("Objects:");
          foreach (var obj in model.Objects)
          {
            Console.Out.WriteLine("  {0} ({1}) {3}->{2}", obj.Attributes.ObjectId, obj.Attributes.Name, obj.Attributes.ObjectColor, obj.Attributes.ColorSource);
          }

          if (null != model.Views)
          {
            Console.Out.WriteLine("Views:");
            foreach (var view in model.Views)
            {
              Console.Out.WriteLine("  {0}", view.Name);
            }
          }

          if (null != model.NamedViews)
          {
            Console.Out.WriteLine("Named Views:");
            foreach (var view in model.NamedViews)
            {
              Console.Out.WriteLine("  {0}", view.Name);
            }
          }

          Console.Out.WriteLine("Notes:");
          Console.Out.WriteLine(model.Notes.Notes);

        }
      }

      if (alternate_output_stream != null)
      {
        // close the text dump file
        alternate_output_stream.Close();
      }
    }
  }
}