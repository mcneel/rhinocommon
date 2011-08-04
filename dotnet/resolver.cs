using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;

namespace Rhino.Runtime
{
  [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
  internal sealed class DependencyAttribute : System.Attribute
  {
    //David made this class internal instead of public on december 10th 2010.
    readonly string m_value;
    public DependencyAttribute(string id)
    {
      m_value = id;
    }
    public string Value
    {
      get { return m_value; }
    }
  }

  /// <summary>
  /// Assembly Resolver for the Rhino App Domain.
  /// </summary>
  public static class AssemblyResolver
  {
    internal static void InitializeAssemblyResolving()
    {
      if (null == m_assembly_resolve)
      {
        //Rhino.Runtime.HostUtils.DebugString("Assembly Resolver initialized\n");
        m_assembly_resolve = new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        AppDomain.CurrentDomain.AssemblyResolve += m_assembly_resolve;
      }
    }

    private static ResolveEventHandler m_assembly_resolve;
    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      // Handle the RhinoCommon case.
      if (args.Name.StartsWith("RhinoCommon", StringComparison.OrdinalIgnoreCase) &&
         !args.Name.StartsWith("RhinoCommon.resources", StringComparison.OrdinalIgnoreCase))
        return Assembly.GetExecutingAssembly();

      // Get the significant name of the assembly we're looking for.
      string searchname = args.Name;
      int index = searchname.IndexOf(',');
      if (index > 0)
      {
        searchname = searchname.Substring(0, index);
      }

      //do not attempt to handle resource searching
      if (searchname.EndsWith(".resources", StringComparison.OrdinalIgnoreCase))
        return null;

      HostUtils.DebugString("CurrentDomain_AssemblyResolve called for " + args.Name + "\n");
      // See if the assembly is already loaded.
      Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
      for (int i = 0; i < assemblies.Length; i++)
      {
        if (assemblies[i].FullName.StartsWith(searchname + ",", StringComparison.OrdinalIgnoreCase))
          return assemblies[i];
      }

      // David: We shouldn't just limit our file search to the assembly name, they might not match.
      // Assembly file search pattern.
      //string pattern = searchname;
      //if (!searchname.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) &&
      //    !searchname.EndsWith(".rhp", StringComparison.OrdinalIgnoreCase))
      //  pattern += "*";

      List<string> potential_files = new List<string>();

      // Collect all potential files in the plug-in directories.
      string[] plugin_folders = null;
#if RHINO_SDK
      plugin_folders = Rhino.PlugIns.PlugIn.GetInstalledPlugInFolders();
#endif
      if (plugin_folders != null)
      {
        foreach (string plugin_folder in plugin_folders)
        {
          string[] files = Directory.GetFiles(plugin_folder, @"*.dll", SearchOption.TopDirectoryOnly); //Why TopDirectoryOnly?
          if (files != null) { potential_files.AddRange(files); }

          files = Directory.GetFiles(plugin_folder, @"*.rhp", SearchOption.TopDirectoryOnly); //Why TopDirectoryOnly?
          if (files != null) { potential_files.AddRange(files); }
        }
      }

      // Collect all potential files in the custom directories.
      if (m_custom_folders != null)
      {
        foreach (string custom_folder in m_custom_folders)
        {
          string[] files = Directory.GetFiles(custom_folder, @"*.dll", SearchOption.TopDirectoryOnly); //Why TopDirectoryOnly?
          if (files != null) { potential_files.AddRange(files); }

          files = Directory.GetFiles(custom_folder, @"*.rhp", SearchOption.TopDirectoryOnly); //Why TopDirectoryOnly?
          if (files != null) { potential_files.AddRange(files); }
        }
      }

      // Collect all potential files in the custom file list.
      if (m_custom_files != null) { potential_files.AddRange(m_custom_files); }

      // Remove the already loaded assemblies from the "potential" list. We've
      // already tested these.
      for (int i = 0; i < assemblies.Length; i++)
      {
        if (assemblies[i].GlobalAssemblyCache)
          continue;
        try
        {
          string path = assemblies[i].Location;
          potential_files.Remove(path);
        } catch{}
      }

      // Sort all potential files based on fuzzy name matches.
      FuzzyComparer fuzzy = new FuzzyComparer(searchname);
      potential_files.Sort(fuzzy);

      foreach (string file in potential_files)
      {
        Assembly asm = TryLoadAssembly(file, searchname, args.Name);
        if (asm != null) { return asm; }
      }

      return null;
    }
    private static Assembly TryLoadAssembly(string filename, string searchname, string asmname)
    {
      // Don't try to load an assembly that already failed to load in the past.
      if (m_loadfailures != null)
      {
        if (m_loadfailures.ContainsKey(filename)) { return null; }
      }

      // David: restrict loading to known file-types. Steve, you'll need to handle rhp loading as I have no idea how.
      if (filename.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) //TODO: implement .rhp loading
      {
        try
        {
          // First load a Reflection Only flavour so we can trivially reject incompatible assemblies.
          Assembly ro_asm = Assembly.ReflectionOnlyLoadFrom(filename);
          if ((ro_asm.FullName != asmname) &&
             (!ro_asm.FullName.StartsWith(searchname)))
            return null;

          // Load for real.
          Assembly asm = Assembly.LoadFile(filename);
          if (asm == null) { return null; }

          // Check dependencies
          bool returnAssembly = true;
          object[] dependencies = asm.GetCustomAttributes(typeof(DependencyAttribute), false);
          if (dependencies != null)
          {
            for (int i = 0; i < dependencies.Length; i++)
            {
              DependencyAttribute dependency = dependencies[i] as DependencyAttribute;
              if (dependency != null)
              {
                System.Guid id = new Guid(dependency.Value);
#if RHINO_SDK
                returnAssembly = Rhino.PlugIns.PlugIn.LoadPlugIn(id); //what, why?
#endif
              }
            }
          }

          if (returnAssembly) { return asm; }
        }
        catch (Exception ex)
        {
          // Print out recursive exception messages.
          for (; ex != null; ex = ex.InnerException)
          {
            HostUtils.DebugString("Exception in ReflectionOnlyLoad: " + ex.Message + "\n");
          }

          // If the assembly fails to load, don't try again.
          if (m_loadfailures != null && !m_loadfailures.ContainsKey(filename))
          {
            m_loadfailures.Add(filename, false);
          }
        }
      }

      return null;
    }

    private class FuzzyComparer : System.Collections.Generic.IComparer<string>
    {
      private readonly string m_search;
      internal FuzzyComparer(string search)
      {
        m_search = search;
      }

      #region IComparer<string> Members
      public int Compare(string x, string y)
      {
        //This can be made a lot smarter with substring searches.
        x = System.IO.Path.GetFileName(x);
        y = System.IO.Path.GetFileName(y);

        int index_x = x.IndexOf(m_search, StringComparison.OrdinalIgnoreCase);
        int index_y = y.IndexOf(m_search, StringComparison.OrdinalIgnoreCase);

        if (index_x < 0) { index_x = int.MaxValue; }
        if (index_y < 0) { index_y = int.MaxValue; }

        return index_x.CompareTo(index_y);
      }
      #endregion
    }

    #region Additional Resolver Locations
    static readonly SortedList<string, bool> m_loadfailures = new SortedList<string, bool>();
    static readonly List<string> m_custom_folders = new List<string>();
    static readonly List<string> m_custom_files = new List<string>();

    /// <summary>
    /// Register a custom folder with the Assembly Resolver. Folders will be 
    /// searched recursively, so this could potentially be a very expensive operation. 
    /// If at all possible, you should consider only registering individual files.
    /// </summary>
    /// <param name="folder">Path of folder to include during Assembly Resolver events.</param>
    public static void AddSearchFolder(string folder)
    {
      // ? is it smart to discard the folder this early on?
      if (!System.IO.Directory.Exists(folder)) { return; }

      // Don't add duplicate folders
      foreach (string existing_folder in m_custom_folders)
      {
        if (existing_folder.Equals(folder, StringComparison.OrdinalIgnoreCase)) { return; }
      }

      m_custom_folders.Add(folder);
    }
    /// <summary>
    /// Register another file with the Assembly Resolver. File must be a .NET assembly, 
    /// so it should probably be a dll, rhp or exe.
    /// </summary>
    /// <param name="file">Path of file to include during Assembly Resolver events.</param>
    public static void AddSearchFile(string file)
    {
      // ? is it smart to discard the file this early on?
      if (!System.IO.File.Exists(file)) { return; }

      // Don't add duplicate files
      foreach (string existing_file in m_custom_files)
      {
        if (existing_file.Equals(file, StringComparison.OrdinalIgnoreCase)) { return; }
      }

      m_custom_files.Add(file);
    }
    #endregion
  }
}