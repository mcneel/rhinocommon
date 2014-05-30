#if FILESYSTEM_SUPPORTED

#pragma warning disable 1591
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
        m_assembly_resolve = CurrentDomain_AssemblyResolve;
        AppDomain.CurrentDomain.AssemblyResolve += m_assembly_resolve;
      }
    }

    private static Dictionary<string, Assembly> m_match_dictionary;
    private static ResolveEventHandler m_assembly_resolve;
    private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
    {
      // Handle the RhinoCommon case.
      if (args.Name.StartsWith("RhinoCommon", StringComparison.OrdinalIgnoreCase) &&
         !args.Name.StartsWith("RhinoCommon.resources", StringComparison.OrdinalIgnoreCase))
        return Assembly.GetExecutingAssembly();

      // Get the significant name of the assembly we're looking for.
      string searchname = args.Name;
      // do not attempt to handle resource searching
      int index = searchname.IndexOf(".resources");
      if (index > 0)
        return null;

      // The resolver is commonly called multiple times with the same search name.
      // Keep the results around so we don't keep doing the same job over and over
      if (m_match_dictionary != null && m_match_dictionary.ContainsKey(args.Name))
        return m_match_dictionary[args.Name];

      bool probably_python = false;
      index = searchname.IndexOf(',');
      if (index > 0)
      {
        searchname = searchname.Substring(0, index);
      }
      else
      {
        // Python scripts typically just look for very short names, like "MyFunctions.dll"
        if (searchname.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
          searchname = searchname.Substring(0, searchname.Length - ".dll".Length);
        probably_python = true;
      }

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
#if RHINO_SDK
      string[] plugin_folders = Rhino.Runtime.HostUtils.GetAssemblySearchPaths();
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
#endif
      if (probably_python)
      {
        string current_dir = System.IO.Directory.GetCurrentDirectory();
        if (!string.IsNullOrEmpty(current_dir))
        {
          string[] files = Directory.GetFiles(current_dir, @"*.dll", SearchOption.TopDirectoryOnly); //Why TopDirectoryOnly?
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

      // 23 August 2012 S. Baer
      // Make sure that at least part of the searchname matches part of the filename
      // Just use the first 5 characters as a required pattern in the filename
      const int length_match = 5;
      string must_be_in_filename = searchname.Substring(0, searchname.Length > length_match ? length_match : searchname.Length);

      Assembly asm = null;
      foreach (string file in potential_files)
      {
        if (file.IndexOf(must_be_in_filename, StringComparison.InvariantCultureIgnoreCase) == -1)
          continue;
        asm = TryLoadAssembly(file, searchname, args.Name);
        if (asm != null)
          break;
      }

      if (m_match_dictionary == null)
        m_match_dictionary = new Dictionary<string, Assembly>();
      if( !m_match_dictionary.ContainsKey(args.Name) )
        m_match_dictionary.Add(args.Name, asm);

      return asm;
    }

    private static Assembly TryLoadAssembly(string filename, string searchname, string asmname)
    {
      // Don't try to load an assembly that already failed to load in the past.
      if (m_loadfailures != null && m_loadfailures.ContainsKey(filename))
        return null;


      // David: restrict loading to known file-types. Steve, you'll need to handle rhp loading as I have no idea how.
      if (filename.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)) //TODO: implement .rhp loading
      {
        try
        {
          // First load a Reflection Only flavour so we can trivially reject incompatible assemblies.
          Assembly ro_asm = Assembly.ReflectionOnlyLoadFrom(filename);

          if (!ro_asm.FullName.StartsWith(searchname + ",", StringComparison.OrdinalIgnoreCase))
            return null;

          // Load for real.
          Assembly asm = Assembly.LoadFile(filename);

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

          if (returnAssembly)
            return asm;
        }
        catch (Exception)
        {
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
        bool x_exists = System.IO.File.Exists(x);
        bool y_exists = System.IO.File.Exists(y);
        if (!x_exists && !y_exists)
          return 0;
        if (!x_exists)
          return 1;
        if (!y_exists)
          return -1;

        //This can be made a lot smarter with substring searches.
        string filename_x = System.IO.Path.GetFileName(x);
        string filename_y = System.IO.Path.GetFileName(y);

        int index_x = filename_x == null ? -1 : filename_x.IndexOf(m_search, StringComparison.OrdinalIgnoreCase);
        int index_y = filename_y == null ? -1 : filename_y.IndexOf(m_search, StringComparison.OrdinalIgnoreCase);

        // 4 April 2012 - S. Baer
        // If the file names are the same, the highest version number or most
        // recent file date is sorted to the top.  Plug-ins like PanelingTools
        // have historically moved around on where they are installed on a user's
        // computer, so we may end up with duplicates
        if (index_x>=0 && index_y>=0 && string.Compare(filename_x, filename_y, StringComparison.OrdinalIgnoreCase) == 0)
        {
          try
          {
            var assembly_x = Assembly.ReflectionOnlyLoadFrom(x);
            var assembly_y = Assembly.ReflectionOnlyLoadFrom(y);
            Version version_x = assembly_x.GetName().Version;
            Version version_y = assembly_y.GetName().Version;
            int rc = version_x.CompareTo(version_y);
            if( rc!=0 )
              return rc;
            //same version. try using the file date
            FileInfo info_x = new FileInfo(x);
            FileInfo info_y = new FileInfo(y);
            rc = info_x.LastAccessTimeUtc.CompareTo(info_y.LastAccessTimeUtc);
            if( rc!=0 )
              return rc;
          }
          catch (Exception ex)
          {
            Rhino.Runtime.HostUtils.ExceptionReport("duplicate assembly resolve", ex);
          }
        }

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

#endif