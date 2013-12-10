using System;
using System.Collections.Generic;
using System.Text;

namespace buildwikisamples
{
  class Program
  {
    static void Main(string[] args)
    {
      //string rootDir = "C:\\dev\\rhino\\V5Beta1\\src4\\DotNetSDK";
      string rootDir = @"C:\src\mcneel.com";
      string[] csfiles = System.IO.Directory.GetFiles(System.IO.Path.Combine(rootDir, "rhinocommon\\examples\\cs"), "ex_*");
      string vbdir = System.IO.Path.Combine(rootDir, "rhinocommon\\examples\\vbnet");
      string pydir = System.IO.Path.Combine(rootDir, "rhinocommon\\examples\\py");
      string wikidir = System.IO.Path.Combine(rootDir, "rhinocommon\\examples\\wiki");

      for (int i = 0; i < csfiles.Length; i++)
      {
        string filename = System.IO.Path.GetFileNameWithoutExtension(csfiles[i]);
        if (filename != "ex_printinstancedefinitiontree") continue;
        string wikiname = filename.Substring(3) + ".txt"; // prune "ex_"
        string wikifile = System.IO.Path.Combine(wikidir, wikiname);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("===== " + filename.Substring(3) + " =====");
        sb.AppendLine();
        sb.AppendLine("===== C# =====");
        sb.AppendLine("<code c#>");
        string[] lines = System.IO.File.ReadAllLines(csfiles[i]);
        bool inblock = false;
        for (int j = 0; j < lines.Length; j++)
        {
          if (!inblock)
          {
            if (lines[j].StartsWith("{"))
              inblock = true;
            continue;
          }
          if (lines[j].StartsWith("}"))
            break;
          if (lines[j].Length < 2)
            sb.AppendLine(lines[j]);
          else
            sb.AppendLine(lines[j].Substring(2));
        }
        sb.AppendLine("</code>");

        string vbpath = System.IO.Path.Combine(vbdir, filename + ".vb");
        if (System.IO.File.Exists(vbpath))
        {
          lines = System.IO.File.ReadAllLines(vbpath);
          sb.AppendLine("===== VB.NET =====");
          sb.AppendLine("<code vb>");
          inblock = false;
          for (int j = 0; j < lines.Length; j++)
          {
            if (!inblock)
            {
              if (lines[j].StartsWith(" "))
                inblock = true;
              else
                continue;
            }
            if (lines[j].StartsWith("End Class"))
              break;
            if (lines[j].Length < 2)
              sb.AppendLine(lines[j]);
            else
              sb.AppendLine(lines[j].Substring(2));
          }
          sb.AppendLine("</code>");
        }

        string pypath = System.IO.Path.Combine(pydir, filename + ".py");
        if (System.IO.File.Exists(pypath))
        {
          lines = System.IO.File.ReadAllLines(pypath);
          sb.AppendLine("===== Python =====");
          sb.AppendLine("<code python>");
          for (int j = 0; j < lines.Length; j++)
          {
            sb.AppendLine(lines[j]);
          }
          sb.AppendLine("</code>");
        }

        sb.AppendLine();
        sb.AppendLine("{{tag>Developer rhinocommon}}");
        System.IO.File.WriteAllText(wikifile, sb.ToString());
      }
    }
  }
}
