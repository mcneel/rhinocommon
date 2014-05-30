#if needed

#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
using Rhino.PlugIns;
#endif

namespace Rhino.Runtime
{
  enum MonoLoadResult : int
  {
    success = 0,
    error_no_runtime = 1,     // unable to locate and load the mono runtime (mono.dll)
    error_init_appdomain = 2, // mono initialization failed (mono_jit_init returned NULL)
    error_mainassembly_load = 3, // unable to load the main .NET assembly (RhinoCommon.DLL)
    error_invalid_path = 4,
    error_not_dotnet = 5,     // the dll that we are attempting to load does not appear to be a .NET dll
    error_already_loaded = 6, // the plug-in looks like it is already loaded
    error_unable_to_load = 7,
    error_create_plugin = 8,  // can't create the plug-in with reflection, can't find the plug-in class,
                              // or more than one public plug-in class is defined
    error_incompatible_version = 9, // plug-in was built against a different version of RhinoCommon
    error_other = 1000 // a different error occured
  }

  /// <summary>
  /// This class should only ever be called from the MonoManager.rhp. Luckily
  /// when embedding mono, you can call private classes with no problem so we
  /// don't need to expose this to the SDK.
  /// </summary>
  class MonoHost
  {
#if RHINO_SDK
    static bool m_bUsingMono; // = false; initialized by runtime
    public static bool UsingMono
    {
      get { return m_bUsingMono; }
    }

    public static int LoadPlugIn(string path)
    {
      HostUtils.SendDebugToCommandLine = true;
      HostUtils.DebugString("[MonoHost::LoadPlugIn] Start");
      if (!m_bUsingMono)
      {
        m_bUsingMono = true;
        // Don't turn visual styles on yet, they seem pretty flakey as of
        // Mono 2.4.2
        //System.Windows.Forms.Application.EnableVisualStyles();

        InitializeExceptionHandling();
        HostUtils.InitializeRhinoCommon();

        HostUtils.DebugString("Attempt to initialize MonoMac");
        Type t = typeof(System.Windows.Forms.Application);
        if (t != null)
        {
          System.Reflection.MethodInfo mi = t.GetRuntimeMethod("MonoMacInit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
          if (mi != null)
          {
            HostUtils.DebugString("Got methodinfo for MonoMacInit");
            mi.Invoke(null, null);
          }
        }
      }

      // 7 Apr 2013, S. Baer
      // Mac plug-ins can now be located inside of OSX plug-in bundles.
      // The native plug-in manager may just pass us the bundle path
      // See if the path refers to a bundle. If that is the case,
      // look inside the bundle to find the actual assembly
      var temp_path = System.IO.Path.Combine(path,"Contents");
      temp_path = System.IO.Path.Combine(temp_path,"Mono");
      if( System.IO.Directory.Exists(temp_path) )
      {
        var files = System.IO.Directory.GetFiles(temp_path, "*.rhp");
        if( files!=null && files.Length>0 )
          path = files[0];
      }

      HostUtils.DebugString("path = " + path);

      // Reflection Load assembly first in order to check versioning
      // and make sure the plug-in hasn't already been loaded
      System.Reflection.Assembly plugin_assembly;
      try
      {
        plugin_assembly = System.Reflection.Assembly.ReflectionOnlyLoadFrom(path);
      }
      catch(Exception){ plugin_assembly = null; }
      HostUtils.DebugString("ReflectionOnlyLoadFrom succeeded");

      if( null==plugin_assembly )
        return (int)MonoLoadResult.error_not_dotnet;

      //make sure this assembly has not alread been loaded
      for (int i = 0; i < PlugIn.m_plugins.Count; i++)
      {
        System.Reflection.Assembly loadedAssembly = PlugIn.m_plugins[i].Assembly;
        if( string.Compare(loadedAssembly.FullName, plugin_assembly.FullName, StringComparison.OrdinalIgnoreCase) == 0 )
          return (int)MonoLoadResult.error_already_loaded;
      }

      // Check version of RhinoCommon that this plug-in uses
      // Don't allow plug-ins built against earlier versions of RhinoCommon to load
      bool passesVersionCheck = false;
      try
      {
        System.Reflection.AssemblyName[] referenced_assemblies = plugin_assembly.GetReferencedAssemblies();
        for (int i = 0; i < referenced_assemblies.Length; i++)
        {
          if (referenced_assemblies[i].Name.Equals("RhinoCommon", StringComparison.OrdinalIgnoreCase))
          {
            Version referenced_version = referenced_assemblies[i].Version;
            Version this_version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            HostUtils.DebugString("Installed RhinoCommon version = {0}", this_version);
            HostUtils.DebugString("Plug-In built against = {0}", referenced_version);
            // major and minor MUST match
            if (referenced_version.Major == this_version.Major && referenced_version.Minor == this_version.Minor)
            {
              // At this point the SDK is changing too rapidly to allow for "safe" updates
              // build of this_version must be == build of referenced version
              // revision of this_version must be >= build of referenced version
              if (this_version.Build == referenced_version.Build)
              {
                if (this_version.Revision >= referenced_version.Revision)
                {
                  passesVersionCheck = true;
                }
              }
            }
            break;
          }
        }
      }
      catch (Exception ex)
      {
        HostUtils.DebugString("Exception while checking versions");
        HostUtils.DebugString(ex.Message);
      }
      if (!passesVersionCheck)
        return (int)MonoLoadResult.error_incompatible_version;


      // TODO: Check version of RhinoCommon that this plug-in uses for versioning

      // At this point, we have determined that this is a RhinoCommon plug-in
      // We've done all the checking that we can do without actually loading
      // the DLL ( and resolving links)
      try
      {
        plugin_assembly = System.Reflection.Assembly.LoadFrom(path);
      }
      catch (Exception)
      {
        plugin_assembly = null;
      }
      if (null == plugin_assembly)
        return (int)MonoLoadResult.error_unable_to_load;
      HostUtils.DebugString("Assembly loaded for real... GetExportedTypes");

      Type[] internal_types = null;
      try
      {
        internal_types = plugin_assembly.GetExportedTypes();
      }
      catch (System.Reflection.ReflectionTypeLoadException ex)
      {
        HostUtils.DebugString("Caught reflection exception");
        Type[] loaded_types = ex.Types;
        if (loaded_types != null)
        {
          HostUtils.DebugString("Got {0} loaded types", loaded_types.Length);
          for (int i = 0; i < loaded_types.Length; i++)
          {
            Type t = loaded_types[i];
            if( t!=null )
            {
              HostUtils.DebugString("{0} - {1}", i, t);
            }
          }
          HostUtils.DebugString(loaded_types.ToString());
        }
        Exception[] e = ex.LoaderExceptions;
        if (e != null)
        {
          HostUtils.DebugString("Got some loader exceptions");
          for (int i = 0; i < e.Length; i++)
          {
            HostUtils.DebugString(ex.ToString());
            internal_types = null;
          }
        }
      }
      catch (Exception ex)
      {
        HostUtils.DebugString("Caught generic exception");
        HostUtils.DebugString(ex.ToString());
        internal_types = null;
        //        HostUtils.ExceptionReport(ex);
      }

      if (null == internal_types)
        return (int)MonoLoadResult.error_unable_to_load;

      HostUtils.DebugString("{0} Exported Types", internal_types.Length);

      // walk through list of exported types and find a single class
      // that derives from PlugIn
      Type plugin_type = typeof(PlugIn);
      int type_index = -1;
      try
      {
        for (int i = 0; i < internal_types.Length; i++)
        {
          if (plugin_type.IsAssignableFrom(internal_types[i]))
          {
            if (type_index >= 0)
              return (int)MonoLoadResult.error_create_plugin;
            type_index = i;
          }
        }
      }
      catch (Exception ex)
      {
        HostUtils.DebugString("Exception occured while trying to find type that is derived from PlugIn");
        HostUtils.ExceptionReport(ex);
        type_index = -1;
      }
      if (type_index < 0)
        return (int)MonoLoadResult.error_create_plugin;
      plugin_type = internal_types[type_index];
      HostUtils.DebugString("Found type derived from PlugIn  (" + plugin_type.ToString() + ")");

      object[] name = plugin_assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
      string plugin_name;
      if( name!=null && name.Length>0 )
        plugin_name = ((System.Reflection.AssemblyTitleAttribute)name[0]).Title;
      else
        plugin_name = plugin_assembly.GetName().Name;
      
      string plugin_version = plugin_assembly.GetName().Version.ToString();
      HostUtils.DebugString("  name = {0}; version = {1}",plugin_name,plugin_version);

      object[] descriptionAtts = plugin_assembly.GetCustomAttributes(typeof(PlugInDescriptionAttribute), false);
      if (descriptionAtts != null)
      {
        const int idxAddress = 0;
        const int idxCountry = 1;
        const int idxEmail = 2;
        const int idxFax = 3;
        const int idxOrganization = 4;
        const int idxPhone = 5;
        const int idxUpdateUrl = 6;
        const int idxWebsite = 7;

        for (int i = 0; i < descriptionAtts.Length; i++)
        {
          PlugInDescriptionAttribute description = (PlugInDescriptionAttribute)descriptionAtts[i];
          switch (description.DescriptionType)
          {
            case DescriptionType.Address:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxAddress, description.Value);
              break;
            case DescriptionType.Country:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxCountry, description.Value);
              break;
            case DescriptionType.Email:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxEmail, description.Value);
              break;
            case DescriptionType.Fax:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxFax, description.Value);
              break;
            case DescriptionType.Organization:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxOrganization, description.Value);
              break;
            case DescriptionType.Phone:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxPhone, description.Value);
              break;
            case DescriptionType.UpdateUrl:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxUpdateUrl, description.Value);
              break;
            case DescriptionType.WebSite:
              UnsafeNativeMethods.RhMono_SetPlugInLoadString(idxWebsite, description.Value);
              break;
          }
          HostUtils.DebugString("  {0} - {1}", description.DescriptionType, description.Value);
        }
      }

      PlugIn plugin = PlugIn.Create(plugin_type, plugin_name, plugin_version);
      if( plugin==null )
        return (int)MonoLoadResult.error_create_plugin;

      // We've created a plug-in so we're pretty committed at this point
      PlugIn.m_plugins.Add(plugin);

      // create all commands in the plug-in
      HostUtils.CreateCommands(plugin);
      HostUtils.DebugString("Created {0} commands", plugin.GetCommands().Length);
      HostUtils.DebugString("[MonoHost::LoadPlugIn] - DONE");

      return (int)MonoLoadResult.success;
    }
#endif

    static void InitializeExceptionHandling()
    {
      // Calling SetUnhandledExceptionMode can throw an exception if any windows have already been
      // created. I don't know how this could happen unless someone else starts writing a mono embedding
      // system in Rhino, but just to be careful

      /*
      // System.Windows.Forms does not work well for Mono yet. Skip the winforms exception handler for now
      try
      {
        System.Windows.Forms.Application.SetUnhandledExceptionMode(System.Windows.Forms.UnhandledExceptionMode.CatchException);
      }
      catch(Exception)
      {
      }
      System.Windows.Forms.Application.ThreadException += new System.Threading.ThreadExceptionEventHandler(Application_ThreadException);
      */

      AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
    }

    static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
      // this should ONLY ever be called if we are actually using Mono
      Exception ex = e.ExceptionObject as Exception;
      if (ex == null) return;
      string msg = ex.ToString() + "\n\nStackTrace:\n" + ex.StackTrace;
      if (sender != null)
      {
        msg += "\n\nSENDER = ";
        msg += sender.ToString();
      }
#if RHINO_SDK
      Rhino.UI.Dialogs.ShowMessageBox(msg, "Unhandled CurrentDomain Exception in .NET plug-in");
#else
      Console.Error.Write(msg);
#endif
    }

    static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
      // this should ONLY ever be called if we are actually using Mono
      Exception ex = e.Exception;
      string msg = ex.ToString() + "\n\nStackTrace:\n" + ex.StackTrace;
      if (sender != null)
      {
        msg += "\n\nSENDER = ";
        msg += sender.ToString();
      }
#if RHINO_SDK
      Rhino.UI.Dialogs.ShowMessageBox(msg, "Unhandled Thread Exception in .NET plug-in");
#else
      Console.Error.Write(msg);
#endif
    }
  }
}

#if RHINO_SDK
partial class UnsafeNativeMethods
{
  // These functions must never be called unless RhinoCommon is being run from Mono
  [DllImport("__Internal", CallingConvention=CallingConvention.Cdecl)]
  internal static extern void RhMono_SetPlugInLoadString(int which, [MarshalAs(UnmanagedType.LPWStr)]string str);
}

#endif

#endif