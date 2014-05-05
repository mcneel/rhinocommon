using System;
using System.Collections.Generic;


namespace MethodGen
{
  /// <summary>
  /// The primary purpose of this application is to auto-generate the function
  /// declarations for the UnsafeNativeMethods in RhinoCommon by parsing exported
  /// C functions in rhcommon_c
  /// 
  /// This application can also be used for generating UnsafeNatimeMethod C#
  /// declarations for other sets of C++ source code by using command line
  /// parameters or a config file which contains the directory for reading the
  /// C++ files followed by the directory for writing the AutoNativeMethods.cs
  /// file
  /// </summary>
  class Program
  {
    public static string m_namespace;
    public static bool m_includeRhinoDeclarations = true;
    public static List<string> m_extra_usings = new List<string>();

    static void Main(string[] args)
    {
      bool rhinocommon_build = false;
      string dirCPP=null;
      string dirCS=null;
      List<string> preprocessor_defines = null;
      if (args.Length >= 2)
      {
        dirCPP = args[0];
        dirCS = args[1];
      }
      else
      {
        // See if there is a configuration file sitting in the same directory
        string location = System.Reflection.Assembly.GetExecutingAssembly().Location;
        string path = System.IO.Path.GetDirectoryName(location);
        path = System.IO.Path.Combine(path, "methodgen.cfg.txt");
        if (System.IO.File.Exists(path))
        {
          string[] lines = System.IO.File.ReadAllLines(path);
          dirCPP = System.IO.Path.GetFullPath(lines[0]);
          dirCS = System.IO.Path.GetFullPath(lines[1]);
          for (int i = 2; i < lines.Length; i++)
          {
            if (lines[i].StartsWith("using"))
            {
              m_includeRhinoDeclarations = false;
              m_extra_usings.Add(lines[i].Trim());
            }
            if (lines[i].StartsWith("define"))
            {
              if (preprocessor_defines == null)
                preprocessor_defines = new List<string>();
              string define = lines[i].Substring("define".Length).Trim();
              preprocessor_defines.Add(define);
            }
          }
        }
        else
        {
          rhinocommon_build = true;
          // find directories for rhcommon_c and RhinoCommon
          GetProjectDirectories(out dirCPP, out dirCS, false);
        }
      }

      if (System.IO.Directory.Exists(dirCPP) && !string.IsNullOrWhiteSpace(dirCS))
      {
        Console.WriteLine("Parsing C files from {0}", dirCPP);
        Console.WriteLine("Saving AutoNativeMethods.cs to {0}", dirCS);
      }
      else
      {
        var color = System.Console.ForegroundColor;
        System.Console.ForegroundColor = ConsoleColor.Red;
        System.Console.WriteLine("ERROR: Unable to locate project directories");
        System.Console.ForegroundColor = color;
        System.Console.WriteLine("Press any key to exit");
        System.Console.Read();
        return;
      }

      CommandlineParser cmd = new CommandlineParser(args);
      m_namespace = cmd["namespace"];
      if (cmd.ContainsKey("IncludeRhinoDeclarations"))
        m_includeRhinoDeclarations = bool.Parse(cmd["IncludeRhinoDeclarations"]);

      NativeMethodDeclares nmd = new NativeMethodDeclares();

      // get all of the .cpp files
      string[] files = System.IO.Directory.GetFiles(dirCPP, "*.cpp");
      foreach (var file in files)
        nmd.BuildDeclarations(file, preprocessor_defines);
      // get all of the .h files
      files = System.IO.Directory.GetFiles(dirCPP, "*.h");
      foreach (var file in files)
        nmd.BuildDeclarations(file, preprocessor_defines);

      string outputfile = System.IO.Path.Combine(dirCS, "AutoNativeMethods.cs");
      nmd.Write(outputfile, "lib");

      if(rhinocommon_build)
      {
        if (!GetProjectDirectories(out dirCPP, out dirCS, true))
        {
          System.Console.WriteLine("Can't locate RDK project directories. This is OK if you are compiling for standalone openNURBS build");
          return;
        }
        //write native methods for rdk
        nmd = new NativeMethodDeclares();

        // get all of the .cpp files
        files = System.IO.Directory.GetFiles(dirCPP, "*.cpp");
        foreach (var file in files)
          nmd.BuildDeclarations(file, preprocessor_defines);
        // get all of the .h files
        files = System.IO.Directory.GetFiles(dirCPP, "*.h");
        foreach (var file in files)
          nmd.BuildDeclarations(file, preprocessor_defines);

        outputfile = System.IO.Path.Combine(dirCS, "AutoNativeMethodsRdk.cs");
        nmd.Write(outputfile, "librdk");
      }
    }

    static bool GetProjectDirectories(out string c, out string dotnet, bool rdk)
    {
			c = null;
			dotnet = null;

      bool rc = false;
      // start with the directory that this executable is located in and work up
      string path = System.Reflection.Assembly.GetExecutingAssembly().Location;
      path = System.IO.Path.GetDirectoryName(path);
      System.IO.DirectoryInfo dirInfo = new System.IO.DirectoryInfo(path);
      while(true)
      {
        if( string.Compare(dirInfo.Name, "RhinoCommon", true)==0 )
        {
          if( rdk )
            c = System.IO.Path.Combine(dirInfo.FullName, "c_rdk");
          else
            c = System.IO.Path.Combine( dirInfo.FullName, "c");
          dotnet = System.IO.Path.Combine( dirInfo.FullName, "dotnet");
          if( System.IO.Directory.Exists(c) && System.IO.Directory.Exists(dotnet) )
          {
            rc = true;
            break;
          }
        }
        // get parent directory
        dirInfo = dirInfo.Parent;
				if (dirInfo == null)
					break;
      }

      if (!rc)
      {
        c = null;
        dotnet = null;
      }
      return rc;
    }
  }
}
