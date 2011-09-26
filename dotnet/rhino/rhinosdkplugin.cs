using System;
using System.Runtime.InteropServices;
using Rhino.Runtime;
using System.Collections.Generic;
using System.Drawing;

#if RDK_UNCHECKED
using Rhino.Render;
#endif

#if RHINO_SDK
namespace Rhino.PlugIns
{
  public enum DescriptionType
  {
    Organization,
    Address,
    Country,
    Phone,
    WebSite,
    Email,
    UpdateUrl,
    Fax
  }

  public enum LoadReturnCode : int
  {
    Success = 1,
    ErrorShowDialog = 0,
    ErrorNoDialog = -1
  }

  [AttributeUsage( AttributeTargets.Assembly,AllowMultiple=true)]
  public sealed class PlugInDescriptionAttribute : System.Attribute
  {
    readonly DescriptionType m_type;
    readonly string m_value;
    public PlugInDescriptionAttribute(DescriptionType descriptionType, string value)
    {
      m_type = descriptionType;
      m_value = value;
    }
    public DescriptionType DescriptionType
    {
      get { return m_type; }
    }
    public string Value
    {
      get { return m_value; }
    }
  }

  public enum PlugInLoadTime : int
  {
    /// <summary>never load plug-in</summary>
    Disabled = 0,
    /// <summary>Load when Rhino starts</summary>
    AtStartup = 1,
    /// <summary>(default) Load the first time a plug-in command used</summary>
    WhenNeeded = 2,
    /// <summary>Load the first time a plug-in command used NOT when restoring docking control bars</summary>
    WhenNeededIgnoreDockingBars = 6,
    /// <summary>When a plug-in command is used or the options dialog is shown</summary>
    WhenNeededOrOptionsDialog = 10
  }

  public class PlugIn
  {
    System.Reflection.Assembly m_assembly;
    internal int m_runtime_serial_number; // = 0; runtime initializes this to 0
    internal Rhino.Collections.RhinoList<Commands.Command> m_commands = new Rhino.Collections.RhinoList<Rhino.Commands.Command>();
    PersistentSettingsManager m_SettingsManager;
    Guid m_id;
    string m_name;
    string m_version;

    #region internals
    internal static Rhino.Collections.RhinoList<PlugIn> m_plugins = new Rhino.Collections.RhinoList<PlugIn>();
    static int m_serialnumber_counter = 1;
    static bool m_bOkToConstruct; // = false; runtime initializes this to false

    internal static PlugIn Create(Type plugin_type, string plugin_name, string plugin_version)
    {
      HostUtils.DebugString("[PlugIn::Create] Start\n");
      if (!string.IsNullOrEmpty(plugin_name))
        HostUtils.DebugString("  plugin_name = " + plugin_name + "\n");
      PlugIn rc;
      Guid plugin_id = Guid.Empty;
      m_bOkToConstruct = true;
      try
      {
        HostUtils.DebugString("  Looking for plug-in's GuidAttribute\n");
        object[] idAttr = plugin_type.Assembly.GetCustomAttributes(typeof(GuidAttribute), false);
        GuidAttribute id = (GuidAttribute)(idAttr[0]);
        plugin_id = new Guid(id.Value);

        if (string.IsNullOrEmpty(plugin_name))
        {
          HostUtils.DebugString("  Looking for plug-in's AssemblyTitleAttribute\n");
          object[] titleAttr = plugin_type.Assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
          System.Reflection.AssemblyTitleAttribute title = (System.Reflection.AssemblyTitleAttribute)(titleAttr[0]);
          plugin_name = title.Title;
        }

        rc = (Rhino.PlugIns.PlugIn)System.Activator.CreateInstance(plugin_type);
      }
      catch (Exception ex)
      {
        HostUtils.DebugString("  Exception thrown while creating Managed plug-in\n");
        HostUtils.DebugString("  Message = " + ex.Message + "\n");
        if (null != ex.InnerException)
          HostUtils.DebugString("    Inner exception message = " + ex.InnerException.Message + "\n");
        rc = null;
      }

      m_bOkToConstruct = false;
      if (rc != null)
      {
        HostUtils.DebugString("  Created PlugIn Instance\n");
        if (string.IsNullOrEmpty(plugin_version))
        {
          plugin_version = plugin_type.Assembly.GetName().Version.ToString();
        }

        rc.m_assembly = plugin_type.Assembly;
        rc.m_runtime_serial_number = m_serialnumber_counter;
        rc.m_id = plugin_id;
        rc.m_name = plugin_name;
        rc.m_version = plugin_version;
        m_serialnumber_counter++;

        int sn = rc.m_runtime_serial_number;

        int plugin_class = 0;
        if (rc is FileImportPlugIn)
          plugin_class = 1;
        else if (rc is FileExportPlugIn)
          plugin_class = 2;
        else if (rc is DigitizerPlugIn)
          plugin_class = 3;
        else if (rc is RenderPlugIn)
          plugin_class = 4;
        // 2 Aug 2011 S. Baer
        // Stop using this function after a few builds
#pragma warning disable 0618
        bool load_at_start = rc.LoadAtStartup;
#pragma warning restore 0618
        PlugInLoadTime lt = rc.LoadTime;
        if (load_at_start)
          lt = PlugInLoadTime.AtStartup;

        UnsafeNativeMethods.CRhinoPlugIn_Create(sn, plugin_id, plugin_name, plugin_version, plugin_class, (int)lt);

        // Once the plugin has been created, look through the plug-in for UserData derived classes
        Type userdata_type = typeof(Rhino.DocObjects.Custom.UserData);
        var internal_types = plugin_type.Assembly.GetExportedTypes();
        for( int i=0; i<internal_types.Length; i++ )
        {
          if( internal_types[i].IsSubclassOf(userdata_type) && !internal_types[i].IsAbstract )
          {
            string name = internal_types[i].FullName;
            Guid id = internal_types[i].GUID;
            UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(name, id);
            Rhino.DocObjects.Custom.UserData.RegisterType(internal_types[i]);
          }
        }
      }
      HostUtils.DebugString("[PlugIn::Create] Finished\n");
      return rc;
    }
    /// <summary>
    /// Only searches through list of RhinoCommon plug-ins
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    internal static PlugIn GetLoadedPlugIn(Guid id)
    {
      PlugIn rc = null;
      for (int i = 0; i < m_plugins.Count; i++)
      {
        if (m_plugins[i].Id == id)
        {
          rc = m_plugins[i];
          break;
        }
      }
      return rc;
    }

    internal IntPtr NonConstPointer()
    {
      return UnsafeNativeMethods.CRhinoPlugIn_Pointer(m_runtime_serial_number);
    }

    internal static PlugIn LookUpBySerialNumber(int serial_number)
    {
      for (int i = 0; i < m_plugins.Count; i++)
      {
        PlugIn p = m_plugins[i];
        if (p.m_runtime_serial_number == serial_number)
          return p;
      }
      HostUtils.DebugString("ERROR: Unable to find RhinoCommon plug-in by serial number");
      return null;
    }

    #endregion
    /// <summary>
    /// Find the plug-in instance that was loaded from a given assembly
    /// </summary>
    /// <param name="pluginAssembly"></param>
    /// <returns></returns>
    public static PlugIn Find(System.Reflection.Assembly pluginAssembly)
    {
      if (null == pluginAssembly)
        return null;
      string compareName = pluginAssembly.FullName;
      for (int i = 0; i < m_plugins.Count; i++)
      {
        if (m_plugins[i].Assembly.FullName == compareName)
          return m_plugins[i];
      }
      return null;
    }



    /// <summary>Source assembly for this plug-in</summary>
    public System.Reflection.Assembly Assembly { get { return m_assembly; } }

    public Guid Id { get { return m_id; } }

    public String Name { get { return m_name; } }

    public String Version { get { return m_version; } }

    /// <summary>All of the commands associated with this plug-in</summary>
    public Commands.Command[] GetCommands()
    {
      return m_commands.ToArray();
    }

    [Obsolete("Use LoadTime virtual property instead")]
    public virtual bool LoadAtStartup
    {
      get { return LoadTime==PlugInLoadTime.AtStartup; }
    }

    /// <summary>
    /// Plug-ins are typically loaded on demand when they are first needed. You can change
    /// this behavior to load the plug-in at during different stages in time by overriding
    /// this property
    /// </summary>
    public virtual PlugInLoadTime LoadTime
    {
      get { return PlugInLoadTime.WhenNeeded; }
    }

    protected PlugIn()
    {
      if (!m_bOkToConstruct)
        throw new System.ApplicationException("Never attempt to create an instance of a PlugIn class, this is the job of the plug-in manager");

      // Set callbacks if they haven't been set yet
      if( null==m_OnLoad || null==m_OnShutDown || null==m_OnGetPlugInObject || 
          null==m_OnCallWriteDocument || null==m_OnWriteDocument || null==m_OnReadDocument ||
          null==m_OnAddPagesToOptions)
      {
        m_OnLoad = InternalOnLoad;
        m_OnShutDown = InternalOnShutdown;
        m_OnGetPlugInObject = InternalOnGetPlugInObject;
        m_OnCallWriteDocument = InternalCallWriteDocument;
        m_OnWriteDocument = InternalWriteDocument;
        m_OnReadDocument = InternalReadDocument;
        m_OnAddPagesToOptions = InternalAddPagesToOptions;
        UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks(m_OnLoad, m_OnShutDown, m_OnGetPlugInObject);
        UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks2(m_OnCallWriteDocument, m_OnWriteDocument, m_OnReadDocument);

        // 12 Dec 2010 S. Baer
        // use empty try/catch for a little while to allow github compiles to catch
        // up with the current WIP
        try
        {
          UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks3(m_OnAddPagesToOptions);
        }
        catch(Exception e)
        {
          HostUtils.ExceptionReport(e);
        }
      }
    }

    #region virtual function callbacks
    internal delegate int OnLoadDelegate(int plugin_serial_number);
    internal delegate void OnShutdownDelegate(int plugin_serial_number);
    internal delegate IntPtr OnGetPlugInObjectDelegate(int plugin_serial_number);
    internal delegate int CallWriteDocumentDelegate(int plugin_serial_number, IntPtr pWriteOptions);
    internal delegate int WriteDocumentDelegate(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pWriteOptions);
    internal delegate int ReadDocumentDelegate(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pReadOptions);
    internal delegate void OnAddPagesToOptionsDelegate(int plugin_serial_number, IntPtr pPageList);

    private static OnLoadDelegate m_OnLoad;
    private static OnShutdownDelegate m_OnShutDown;
    private static OnGetPlugInObjectDelegate m_OnGetPlugInObject;
    private static CallWriteDocumentDelegate m_OnCallWriteDocument;
    private static WriteDocumentDelegate m_OnWriteDocument;
    private static ReadDocumentDelegate m_OnReadDocument;
    private static OnAddPagesToOptionsDelegate m_OnAddPagesToOptions;
    

    private static int InternalOnLoad(int plugin_serial_number)
    {
      LoadReturnCode rc = LoadReturnCode.Success;
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        string error_msg = "";

        try
        {
          rc = p.OnLoad(ref error_msg);

#if RDK_UNCHECKED
          // after calling the OnLoad function, check to see if we should be creating
          // an RDK plugin. This is the typical spot where C++ plug-ins perform their
          // RDK initialization.
          if (rc == LoadReturnCode.Success)
          {
            if (p is RenderPlugIn)
            {
              Rhino.Render.RdkPlugIn.GetRdkPlugIn(p.Id, plugin_serial_number);
            }

            Rhino.Render.RenderContent.RegisterContent(p.Assembly, p.Id);
            Rhino.Render.CustomRenderMesh.Provider.RegisterProviders(p.Assembly, p.Id);
          }
#endif

        }
        catch (Exception ex)
        {
          rc = LoadReturnCode.ErrorShowDialog;
          error_msg = "Error occured loading plug-in\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }

        if (LoadReturnCode.ErrorShowDialog == rc && !string.IsNullOrEmpty(error_msg))
        {
          UnsafeNativeMethods.CRhinoPlugIn_SetLoadErrorMessage(plugin_serial_number, error_msg);
        }
      }
      return (int)rc;
    }

    private static void InternalOnShutdown(int plugin_serial_number)
    {
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        try
        {
          p.OnShutdown();
          // Write the settings after the virtual OnShutDown. This should be
          // the last function that the plug-in can use to save settings.
          if (p.m_SettingsManager != null)
            p.m_SettingsManager.WriteSettings();

#if RDK_UNCHECKED
          // check to see if we should be uninitializing an RDK plugin
          Rhino.Render.RdkPlugIn pRdk = Rhino.Render.RdkPlugIn.GetRdkPlugIn(p);
          if (pRdk != null)
            pRdk.Dispose();
#endif
        }
        catch (Exception ex)
        {
          HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static IntPtr InternalOnGetPlugInObject(int plugin_serial_number)
    {
      IntPtr rc = IntPtr.Zero;
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        try
        {
          object obj = p.GetPlugInObject();
          if (obj != null)
            rc = Marshal.GetIDispatchForObject(obj);
        }
        catch (Exception)
        {
          rc = IntPtr.Zero;
        }
      }
      return rc;
    }
    private static int InternalCallWriteDocument(int plugin_serial_number, IntPtr pWriteOptions)
    {
      int rc = 0; //FALSE
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null && pWriteOptions!=IntPtr.Zero)
      {
        Rhino.FileIO.FileWriteOptions wo = new Rhino.FileIO.FileWriteOptions(pWriteOptions);
        rc = p.ShouldCallWriteDocument(wo) ? 1 : 0;
        wo.Dispose();
      }
      return rc;
    }
    private static int InternalWriteDocument(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pWriteOptions)
    {
      int rc = 1; //TRUE
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      RhinoDoc doc = RhinoDoc.FromId(doc_id);
      if (p != null && doc != null && pBinaryArchive != IntPtr.Zero && pWriteOptions != IntPtr.Zero)
      {
        Rhino.FileIO.BinaryArchiveWriter writer = new Rhino.FileIO.BinaryArchiveWriter(pBinaryArchive);
        Rhino.FileIO.FileWriteOptions wo = new Rhino.FileIO.FileWriteOptions(pWriteOptions);
        try
        {
          p.WriteDocument(doc, writer, wo);
          rc = writer.WriteErrorOccured ? 0 : 1;
        }
        catch (Exception ex)
        {
          HostUtils.ExceptionReport(ex);
          rc = 0; //FALSE
        }
        // in case someone tries to hold on to instances of these classes
        writer.ClearPointer();
        wo.Dispose();
      }
      return rc;
    }
    private static int InternalReadDocument(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pReadOptions)
    {
      int rc = 1; //TRUE
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      RhinoDoc doc = RhinoDoc.FromId(doc_id);
      if (p != null && doc != null && pBinaryArchive != IntPtr.Zero && pReadOptions != IntPtr.Zero)
      {
        Rhino.FileIO.BinaryArchiveReader reader = new Rhino.FileIO.BinaryArchiveReader(pBinaryArchive);
        Rhino.FileIO.FileReadOptions ro = new Rhino.FileIO.FileReadOptions(pReadOptions);
        try
        {
          p.ReadDocument(doc, reader, ro);
          rc = reader.ReadErrorOccured ? 0 : 1;
        }
        catch (Exception ex)
        {
          HostUtils.ExceptionReport(ex);
          rc = 0; //FALSE
        }
        // in case someone tries to hold on to instances of these classes
        reader.ClearPointer();
        ro.Dispose();
      }
      return rc;
    }
    private static void InternalAddPagesToOptions(int plugin_serial_number, IntPtr pPageList)
    {
#if RHINO_SDK
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        try
        {
          System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages = new System.Collections.Generic.List<Rhino.UI.OptionsDialogPage>();
          p.OptionsDialogPages(pages);
          for (int i = 0; i < pages.Count; i++)
          {
            IntPtr ptr = pages[i].ConstructWithRhinoDotNet();
            if (ptr != IntPtr.Zero)
              UnsafeNativeMethods.CRhinoPlugIn_AddOptionPage(pPageList, ptr);
          }
        }
        catch (Exception e)
        {
          HostUtils.ExceptionReport(e);
        }
      }
#endif
    }
    #endregion

    #region default virtual function implementations
    /// <summary>
    /// </summary>
    /// <param name="errorMessage">
    /// If a load error is returned and this string is set. This string is the 
    /// error message that will be reported back to the user
    /// </param>
    /// <returns></returns>
    protected virtual LoadReturnCode OnLoad(ref string errorMessage)
    {
      return LoadReturnCode.Success;
    }

    protected virtual void OnShutdown()
    {
    }

    public virtual object GetPlugInObject()
    {
      return null;
    }

    /// <summary>
    /// Called whenever a Rhino is about to save a .3dm file.
    /// If you want to save plug-in document data when a model is 
    /// saved in a version 5 .3dm file, then you must override this
    /// function to return true and you must override WriteDocument().
    /// </summary>
    /// <param name="options"></param>
    /// <returns>
    /// True if the plug-in wants to save document user data in the
    /// version 5 .3dm file.  The default returns false.
    /// </returns>
    protected virtual bool ShouldCallWriteDocument(Rhino.FileIO.FileWriteOptions options)
    {
      return false;
    }

    /// <summary>
    /// Called when Rhino is saving a .3dm file to allow the plug-in
    /// to save document user data.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="archive">
    /// OpenNURBS file archive object Rhino is using to write the file.
    /// Use BinaryArchiveWriter.Write*() functions to write plug-in data.
    /// OR use the ArchivableDictionary
    /// 
    /// If any BinaryArchiveWriter.Write*() functions throw an exception, 
    /// then archive.WriteErrorOccured will be true and you should immediately return.
    /// Setting archive.WriteErrorOccured to true will cause Rhino to stop saving the file.
    /// </param>
    /// <param name="options"></param>
    protected virtual void WriteDocument(Rhino.RhinoDoc doc, Rhino.FileIO.BinaryArchiveWriter archive, Rhino.FileIO.FileWriteOptions options)
    {
    }

    /// <summary>
    /// Called whenever a Rhino document is being loaded and plug-in user data was
    /// encountered written by a plug-in with this plug-in's GUID.
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="archive">
    /// OpenNURBS file archive object Rhino is using to read this file.
    /// Use BinaryArchiveReader.Read*() functions to read plug-in data.
    /// 
    /// If any BinaryArchive.Read*() functions throws an exception then
    /// archive.ReadErrorOccurve will be true and you should immediately return.
    /// </param>
    /// <param name="options">Describes what is being written</param>
    protected virtual void ReadDocument(Rhino.RhinoDoc doc, Rhino.FileIO.BinaryArchiveReader archive, Rhino.FileIO.FileReadOptions options)
    {
    }

#if RHINO_SDK
    /// <summary>
    /// Override this function if you want to extend the options dialog. This function is
    /// called whenever the user brings up the Options dialog.
    /// </summary>
    /// <param name="pages">list of pages to add your custom options dialog page(s) to</param>
    protected virtual void OptionsDialogPages( System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages )
    {
    }
#endif
    #endregion


    string m_all_users_settings_dir;
    public string SettingsDirectoryAllUsers
    {
      get
      {
        if (string.IsNullOrEmpty(m_all_users_settings_dir))
          m_all_users_settings_dir = SettingsDirectoryHelper(false);
        return m_all_users_settings_dir;
      }
    }

    string m_local_user_settings_dir;
    public string SettingsDirectory
    {
      get
      {
        if (string.IsNullOrEmpty(m_local_user_settings_dir))
          m_local_user_settings_dir = SettingsDirectoryHelper(true);
        return m_local_user_settings_dir;
      }
    }

    private string SettingsDirectoryHelper(bool bLocalUser)
    {
      string result = null;
      string path = null;
      if (HostUtils.RunningOnWindows)
      {
        if (string.IsNullOrEmpty(Name) || Id == Guid.Empty)
          throw new Exception("PlugIn.SettingsDirectory can not be called before the Name and Id properties have been initialized.");
        System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
        string pluginName = string.Format(ci, "{0} ({1})", Name, Id.ToString().ToLower(ci));
        // remove invalid characters from string
        char[] invalid_chars = System.IO.Path.GetInvalidFileNameChars();
        int index = pluginName.IndexOfAny(invalid_chars);
        while (index >= 0)
        {
          pluginName = pluginName.Remove(index, 1);
          index = pluginName.IndexOfAny(invalid_chars);
        }
        string commonDir = System.Environment.GetFolderPath(bLocalUser ? System.Environment.SpecialFolder.ApplicationData : System.Environment.SpecialFolder.CommonApplicationData);
        char sep = System.IO.Path.DirectorySeparatorChar;
        commonDir = System.IO.Path.Combine(commonDir, "McNeel" + sep + "Rhinoceros" + sep + "5.0" + sep + "Plug-ins");
        path = System.IO.Path.Combine(commonDir, pluginName);
      }
      else if(HostUtils.RunningOnOSX)
      {
        // put the settings directory next to the rhp
        path = System.IO.Path.GetDirectoryName(this.Assembly.Location);
      }
      if( path!=null)
        result = System.IO.Path.Combine(path, "settings");
      return result;
    }


    public PersistentSettings Settings
    {
      get 
      {
        if (m_SettingsManager == null)
          m_SettingsManager = new PersistentSettingsManager(this);
        return m_SettingsManager.PluginSettings;
      }
    }

    public PersistentSettings CommandSettings(string name)
    {
      if (m_SettingsManager == null)
        m_SettingsManager = new PersistentSettingsManager(this);
      return m_SettingsManager.CommandSettings(name);
    }

    #region plugin manager items
    public static int InstalledPlugInCount
    {
      get
      {
        return UnsafeNativeMethods.CRhinoPlugInManager_PlugInCount();
      }
    }

    public static System.Collections.Generic.Dictionary<Guid, string> GetInstalledPlugIns()
    {
      int count = InstalledPlugInCount;
      System.Collections.Generic.Dictionary<Guid, string> plug_in_dictionary = new System.Collections.Generic.Dictionary<Guid, string>(32);
      for (int i = 0; i < count; i++)
      {
        IntPtr name = UnsafeNativeMethods.CRhinoPlugInManager_GetName(i);
        if (name != IntPtr.Zero)
        {
          string sName = Marshal.PtrToStringUni(name);
          if (!string.IsNullOrEmpty(sName))
          {
            Guid id = UnsafeNativeMethods.CRhinoPlugInManager_GetID(i);
            if (id != Guid.Empty && !plug_in_dictionary.ContainsKey(id))
              plug_in_dictionary.Add(id, sName);
          }
        }
      }
      return plug_in_dictionary;
    }

    public static string[] GetInstalledPlugInNames()
    {
      int count = InstalledPlugInCount;
      System.Collections.Generic.List<string> names = new System.Collections.Generic.List<string>(32);
      for (int i = 0; i < count; i++)
      {
        IntPtr name = UnsafeNativeMethods.CRhinoPlugInManager_GetName(i);
        if (name != IntPtr.Zero)
        {
          string sName = Marshal.PtrToStringUni(name);
          if (string.IsNullOrEmpty(sName))
            names.Add(sName);
        }
      }
      return names.ToArray();
    }
    public static string[] GetInstalledPlugInFolders()
    {
      int count = InstalledPlugInCount;
      System.Collections.Generic.List<string> dirs = new System.Collections.Generic.List<string>(32);
      for (int i = 0; i < count; i++)
      {
        IntPtr pFile = UnsafeNativeMethods.CRhinoPlugInManager_GetFileName(i);
        if (pFile != IntPtr.Zero)
        {
          string path = Marshal.PtrToStringUni(pFile);
          if (System.IO.File.Exists(path))
          {
            path = System.IO.Path.GetDirectoryName(path);
            if (dirs.Contains(path))
              continue;
            dirs.Add(path);
          }
        }
      }
      return dirs.ToArray();
    }

    /// <summary>
    /// Get a plug-in name for an installed plug-in given the path to that plug-in
    /// </summary>
    /// <param name="pluginPath"></param>
    /// <returns></returns>
    public static string NameFromPath(string pluginPath)
    {
      using(StringHolder sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoPlugInManager_NameFromPath(pluginPath, pString);
        return sh.ToString();
      }
    }

    public static Guid IdFromPath(string pluginPath)
    {
      return UnsafeNativeMethods.CRhinoPlugInManager_IdFromPath(pluginPath);
    }

    public static bool LoadPlugIn(Guid pluginId)
    {
      return UnsafeNativeMethods.CRhinoPlugInManager_LoadPlugIn(pluginId);
    }

    /// <summary>
    /// Get names of all "non-test" commands for a given plug-in
    /// </summary>
    /// <param name="pluginId"></param>
    /// <returns></returns>
    public static string[] GetEnglishCommandNames(Guid pluginId)
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoPluginManager_GetCommandNames(pluginId, pStrings);
      string[] rc = new string[count];
      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          UnsafeNativeMethods.ON_StringArray_Get(pStrings, i, pString);
          rc[i] = sh.ToString();
        }
      }
      UnsafeNativeMethods.ON_StringArray_Delete(pStrings);
      return rc;
    }
    #endregion
  }

  // privately used by FileTypeList
  class FileType
  {
    public string m_description;
    public System.Collections.Generic.List<string> m_extensions = new System.Collections.Generic.List<string>();
  }

  public sealed class FileTypeList
  {
    internal System.Collections.Generic.List<FileType> m_filetypes = new System.Collections.Generic.List<FileType>();

    public int AddFileType(string description, string extension)
    {
      return AddFileType(description, new string[] { extension });
    }
    public int AddFileType(string description, string extension1, string extension2)
    {
      return AddFileType(description, new string[] { extension1, extension2 });
    }
    public int AddFileType(string description, System.Collections.Generic.IEnumerable<string> extensions)
    {
      if (string.IsNullOrEmpty(description))
        return -1;

      FileType ft = new FileType();
      ft.m_description = description;
      foreach (string ext in extensions)
      {
        if( !string.IsNullOrEmpty(ext))
          ft.m_extensions.Add(ext);
      }
      m_filetypes.Add(ft);
      return m_filetypes.Count - 1;
    }

  }

  public abstract class FileImportPlugIn : PlugIn
  {
    protected FileImportPlugIn()
    {
      // Set callbacks if they haven't been set yet
      if (null == m_OnAddFileType || null == m_OnReadFile)
      {
        m_OnAddFileType = InternalOnAddFileType;
        m_OnReadFile = InternalOnReadFile;
        UnsafeNativeMethods.CRhinoFileImportPlugIn_SetCallbacks(m_OnAddFileType, m_OnReadFile);
      }
    }

    internal delegate void AddFileType(int plugin_serial_number, IntPtr pFileList, IntPtr readoptions);
    internal delegate int ReadFileFunc(int plugin_serial_number, IntPtr filename, int index, int docId, IntPtr readoptions);
    private static AddFileType m_OnAddFileType;
    private static ReadFileFunc m_OnReadFile;

    private static void InternalOnAddFileType(int plugin_serial_number, IntPtr pFileList, IntPtr readoptions)
    {
      FileImportPlugIn p = LookUpBySerialNumber(plugin_serial_number) as FileImportPlugIn;
      if (null == p || IntPtr.Zero == pFileList || IntPtr.Zero == readoptions)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalOnAddFileType");
      }
      else
      {
        try
        {
          Rhino.FileIO.FileReadOptions ro = new Rhino.FileIO.FileReadOptions(readoptions);
          FileTypeList list = p.AddFileTypes(ro);
          ro.Dispose();
          if (list != null)
          {
            Guid id = p.Id;
            System.Collections.Generic.List<FileType> fts = list.m_filetypes;
            for (int i = 0; i < fts.Count; i++)
            {
              FileType ft = fts[i];
              if( !string.IsNullOrEmpty(ft.m_description) && ft.m_extensions.Count>0 )
              {
                int index = UnsafeNativeMethods.CRhinoFileTypeList_Add(pFileList, id ,ft.m_description);
                for (int j = 0; j < ft.m_extensions.Count; j++ )
                  UnsafeNativeMethods.CRhinoFileTypeList_SetExtension(pFileList, index, ft.m_extensions[j]);
              }
            }
          }
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in AddFileType\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    private static int InternalOnReadFile(int plugin_serial_number, IntPtr filename, int index, int docId, IntPtr readoptions)
    {
      int rc = 0;
      FileImportPlugIn p = LookUpBySerialNumber(plugin_serial_number) as FileImportPlugIn;
      if (null == p || IntPtr.Zero == filename || IntPtr.Zero == readoptions)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalOnReadFile");
      }
      else
      {
        try
        {
          Rhino.FileIO.FileReadOptions ropts = new Rhino.FileIO.FileReadOptions(readoptions);
          Rhino.RhinoDoc doc = Rhino.RhinoDoc.FromId(docId);
          string _filename = Marshal.PtrToStringUni(filename);
          rc = p.ReadFile(_filename, index, doc, ropts) ? 1 : 0;
          ropts.Dispose();
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in ReadFile\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return rc;
    }

    protected abstract FileTypeList AddFileTypes( Rhino.FileIO.FileReadOptions options );
    protected abstract bool ReadFile(string filename, int index, RhinoDoc doc, Rhino.FileIO.FileReadOptions options);

    protected string MakeReferenceTableName(string nameToPrefix)
    {
      IntPtr rc = UnsafeNativeMethods.CRhinoFileImportPlugIn_MakeReferenceTableName(m_runtime_serial_number, nameToPrefix);
      return IntPtr.Zero == rc ? String.Empty : Marshal.PtrToStringUni(rc);
    }
  }

  public enum WriteFileResult : int
  {
    Cancel = -1,
    Failure = 0,
    Success = 1
  }
  public abstract class FileExportPlugIn : PlugIn
  {
    protected FileExportPlugIn()
    {
      // Set callbacks if they haven't been set yet
      if (null == m_OnAddFileType || null == m_OnWriteFile)
      {
        m_OnAddFileType = InternalOnAddFileType;
        m_OnWriteFile = InternalOnWriteFile;
        UnsafeNativeMethods.CRhinoFileExportPlugIn_SetCallbacks(m_OnAddFileType, m_OnWriteFile);
      }
    }

    internal delegate void AddFileType(int plugin_serial_number, IntPtr pFileList, IntPtr writeoptions);
    internal delegate int WriteFileFunc(int plugin_serial_number, IntPtr filename, int index, int docId, IntPtr writeoptions);
    private static AddFileType m_OnAddFileType;
    private static WriteFileFunc m_OnWriteFile;

    private static void InternalOnAddFileType(int plugin_serial_number, IntPtr pFileList, IntPtr pWriteOptions)
    {
      FileExportPlugIn p = LookUpBySerialNumber(plugin_serial_number) as FileExportPlugIn;
      if (null == p || IntPtr.Zero == pFileList || IntPtr.Zero == pWriteOptions)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalOnAddFileType");
      }
      else
      {
        try
        {
          Rhino.FileIO.FileWriteOptions writeoptions = new Rhino.FileIO.FileWriteOptions(pWriteOptions);
          FileTypeList list = p.AddFileTypes(writeoptions);
          writeoptions.Dispose();
          if (list != null)
          {
            Guid id = p.Id;
            System.Collections.Generic.List<FileType> fts = list.m_filetypes;
            for (int i = 0; i < fts.Count; i++)
            {
              FileType ft = fts[i];
              if( !string.IsNullOrEmpty(ft.m_description) && ft.m_extensions.Count>0 )
              {
                int index = UnsafeNativeMethods.CRhinoFileTypeList_Add(pFileList, id ,ft.m_description);
                for (int j = 0; j < ft.m_extensions.Count; j++ )
                  UnsafeNativeMethods.CRhinoFileTypeList_SetExtension(pFileList, index, ft.m_extensions[j]);
              }
            }
          }
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in AddFileType\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    private static int InternalOnWriteFile(int plugin_serial_number, IntPtr filename, int index, int docId, IntPtr writeoptions)
    {
      int rc = 0;
      FileExportPlugIn p = LookUpBySerialNumber(plugin_serial_number) as FileExportPlugIn;
      if (null == p || IntPtr.Zero == filename || IntPtr.Zero == writeoptions)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalOnWriteFile");
      }
      else
      {
        try
        {
          Rhino.FileIO.FileWriteOptions wopts = new Rhino.FileIO.FileWriteOptions(writeoptions);
          Rhino.RhinoDoc doc = Rhino.RhinoDoc.FromId(docId);
          string _filename = Marshal.PtrToStringUni(filename);
          rc = (int)(p.WriteFile(_filename, index, doc, wopts));
          wopts.Dispose();
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in WriteFile\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return rc;
    }

    
    protected abstract FileTypeList AddFileTypes(Rhino.FileIO.FileWriteOptions options);
    protected abstract WriteFileResult WriteFile(string filename, int index, RhinoDoc doc, Rhino.FileIO.FileWriteOptions options);
  }

  public abstract class RenderPlugIn : PlugIn
  {
    private static IntPtr m_render_command_context = IntPtr.Zero;
    internal static IntPtr RenderCommandContextPointer
    {
      get
      {
        return m_render_command_context;
      }
    }

    #region render and render window virtual function implementation
    internal delegate int RenderFunc(int plugin_serial_number, int doc_id, int modes, int render_preview, IntPtr context);
    internal delegate int RenderWindowFunc(int plugin_serial_number, int doc_id, int modes, int render_preview, IntPtr pRhinoView, int rLeft, int rTop, int rRight, int rBottom, int inWindow, IntPtr context);
    private static RenderFunc m_OnRender = InternalOnRender;
    private static RenderWindowFunc m_OnRenderWindow = InternalOnRenderWindow;
    private static int InternalOnRender(int plugin_serial_number, int doc_id, int modes, int render_preview, IntPtr context)
    {
      m_render_command_context = context;
      Rhino.Commands.Result rc = Rhino.Commands.Result.Failure;
      RenderPlugIn p = LookUpBySerialNumber(plugin_serial_number) as RenderPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnRenderWindow");
      }
      else
      {
        try
        {
          RhinoDoc doc = RhinoDoc.FromId(doc_id);
          Rhino.Commands.RunMode rm = Rhino.Commands.RunMode.Interactive;
          if (modes > 0)
            rm = Rhino.Commands.RunMode.Scripted;
          rc = p.Render(doc, rm, render_preview != 0);
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in Render\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      m_render_command_context = IntPtr.Zero;
      return (int)rc;
    }

    private static int InternalOnRenderWindow(int plugin_serial_number, int doc_id, int modes, int render_preview, IntPtr pRhinoView, int rLeft, int rTop, int rRight, int rBottom, int inWindow, IntPtr context)
    {
      m_render_command_context = context;
      Rhino.Commands.Result rc = Rhino.Commands.Result.Failure;
      RenderPlugIn p = LookUpBySerialNumber(plugin_serial_number) as RenderPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnRenderWindow");
      }
      else
      {
        try
        {
          RhinoDoc doc = RhinoDoc.FromId(doc_id);
          Rhino.Commands.RunMode rm = Rhino.Commands.RunMode.Interactive;
          if (modes > 0)
            rm = Rhino.Commands.RunMode.Scripted;
          Rhino.Display.RhinoView view = Rhino.Display.RhinoView.FromIntPtr(pRhinoView);
          System.Drawing.Rectangle rect = System.Drawing.Rectangle.FromLTRB(rLeft, rTop, rRight, rBottom);
          rc = p.RenderWindow(doc, rm, render_preview != 0, view, rect, inWindow != 0);
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in RenderWindow\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      m_render_command_context = IntPtr.Zero;
      return (int)rc;
    }
    #endregion

    protected RenderPlugIn()
    {
      UnsafeNativeMethods.CRhinoRenderPlugIn_SetCallbacks(m_OnRender, m_OnRenderWindow);

#if RDK_UNCHECKED
      UnsafeNativeMethods.CRhinoRenderPlugIn_SetRdkCallbacks(m_OnSupportsFeature, 
                                                             m_OnAbortRender, 
                                                             m_OnAllowChooseContent, 
                                                             m_OnCreateDefaultContent,
                                                             m_OnOutputTypes,
                                                             m_OnCreateTexturePreview,
                                                             m_OnCreatePreview,
                                                             m_OnDecalProperties);
#endif
    }

#if RDK_UNCHECKED
    public enum Features : int
    {
      Materials = 0,
      Environments = 1,
      Textures = 2,
      PostEffects = 3,
      Sun = 4,
      CustomRenderMeshes = 5,
      Decals = 6,
      GroundPlane = 7,
      SkyLight = 8,
      CustomDecalProperties = 9,
    }

    /// <summary>
    /// Return true if your renderer supports the specific feature.
    /// </summary>
    /// <param name="f"></param>
    /// <returns></returns>
    protected virtual bool SupportsFeature(Features f)
    {
      return true;
    }

    /// <summary>
    /// You must implement this function to abort preview renderings initiated using CreatePreview (if possible)
    /// </summary>
    protected virtual void AbortPreviewRender()
    {
    }

    protected virtual bool AllowChooseContent(Rhino.Render.RenderContent content)
    {
      return true;
    }

    protected virtual void CreateDefaultContent(RhinoDoc doc)
    {
    }

    public class OutputTypeInfo
    {
      public OutputTypeInfo(string ext, string type)
      {
        _fileExtension = ext;
        _typeDescription = type;
      }

      private readonly string _fileExtension;
      public string fileExtension
      {
        get { return _fileExtension; }
      }
      private readonly string _typeDescription;
      public string typeDescription
      {
        get { return _typeDescription; }
      }
    }

    protected virtual List<OutputTypeInfo> OutputTypes()
    {
      //TODO - base class call
      int iIndex = 0;

      StringHolder shExt = new StringHolder();
      StringHolder shDesc = new StringHolder();

      List<OutputTypeInfo> list = new List<OutputTypeInfo>();

      while ( 1==UnsafeNativeMethods.Rdk_RenderPlugIn_BaseOutputTypeAtIndex(NonConstPointer(), iIndex++, shExt.NonConstPointer(), shDesc.NonConstPointer()))
      {
        list.Add(new OutputTypeInfo(shExt.ToString(), shDesc.ToString()));
      }
      return list;
    }

    /// <summary>
    /// You should implement this method to create the preview bitmap that will appear in the 
    /// content editor's thumbnail display when previewing textures in 2d (UV) mode.
    /// </summary>
    /// <param name="pixels">The pixel dimensions of the bitmap you should return</param>
    /// <param name="texture">The texture you should render as a 2D image</param>
    /// <returns>Return null if you want Rhino to generate its own texture preview.</returns>
    protected virtual System.Drawing.Image CreateTexturePreview(System.Drawing.Size pixels, Rhino.Render.RenderTexture texture)
    {
      return null;
    }

    public enum PreviewQualityLevels : int
    {
      None    = 0, // No quality set.
      Low     = 1, // Low quality rendering for quick preview.
      Medium  = 2, // Medium quality rendering for intermediate preview.
      Full    = 3, // Full quality rendering (quality comes from user settings).
    };

    /// <summary>
    /// You must implement this method to create the preview bitmap that will appear
    /// in the content editor's thumbnail display when previewing materials and environments.
    /// NB. This preview is the "renderer preview" and is called 3 times with varying levels
    /// of quality. If you don't want to implement this kind of preview, and are satisfied with the
    /// "QuickPreview" generated from CreateQuickPreview, just return null. If you don't support
    /// progressive refinement, return NULL from the first two quality levels.
    /// </summary>
    /// <param name="pixels"></param>
    /// <param name="quality"></param>
    /// <param name="scene"></param>
    /// <returns></returns>
    protected virtual System.Drawing.Image CreatePreview(System.Drawing.Size pixels, PreviewQualityLevels quality, PreviewScene scene)
    {
      return null;
    }

    /// <summary>
    /// Optionally implement this method to change the way quick content previews are generated.
    /// By default, this is handled by the internal RDK OpenGL renderer and is based on the
    /// simulation of the content. If you want to implement an instant render based on the
    /// actual content parameters, or if you just think you can do a better job, override this
		/// Note: The first plug-in to return a non-null value will get to draw the preview, so if you
    /// decide not to draw based on the contents of the scene server, please return null.
    /// </summary>
    /// <param name="pixels"></param>
    /// <param name="scene"></param>
    /// <returns></returns>
    protected virtual System.Drawing.Image CreateQuickPreview(System.Drawing.Size pixels, PreviewScene scene)
    {
      return null;
    }

    /// <summary>
    /// Override this function to handle showing a modal dialog with your plugin's
    /// custom decal properties.  You will be passed the current properties for the 
    /// object being edited.  The defaults will be set in InitializeDecalProperties
    /// </summary>
    /// <param name="properties">A list of named values that will be stored on the object
    /// the input values are the current ones, you should modify the values after the dialog
    /// closes.</param>
    /// <returns>true if the user pressed "OK", otherwise false</returns>
    protected virtual bool ShowDecalProperties(ref List<NamedValue> properties)
    {
      return false;
    }

    /// <summary>
    /// Initialize your custom decal properties here.  The input list will be empty - add your
    /// default named property values and return.
    /// </summary>
    /// <param name="properties"></param>
    protected virtual void InitializeDecalProperties(ref List<NamedValue> properties)
    {
    }


    #region other virtual function implementation
    internal delegate int SupportsFeatureCallback(int serial_number, Features f);
    private static SupportsFeatureCallback m_OnSupportsFeature = OnSupportsFeature;
    private static int OnSupportsFeature(int serial_number, Features f)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnSupportsFeature");
      }
      else
      {
        try
        {
          return p.SupportsFeature(f) ? 1:0;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnSupportsFeature\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return 0;
    }

    internal delegate void AbortRenderCallback(int serial_number);
    private static AbortRenderCallback m_OnAbortRender = OnAbortRender;
    private static void OnAbortRender(int serial_number)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnAbortRender");
      }
      else
      {
        try
        {
          p.AbortPreviewRender();
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnAbortRender\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    internal delegate int AllowChooseContentCallback(int serial_number, IntPtr pConstContent);
    private static AllowChooseContentCallback m_OnAllowChooseContent = OnAllowChooseContent;
    private static int OnAllowChooseContent(int serial_number, IntPtr pConstContent)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      RenderContent c = RenderContent.FromPointer(pConstContent);
      if (null == p || null == c)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnAllowChooseContent");
      }
      else
      {
        try
        {
          return p.AllowChooseContent(c) ? 1:0;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnAllowChooseContent\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return 0;
    }

    internal delegate void CreateDefaultContentCallback(int serial_number, int docId);
    private static CreateDefaultContentCallback m_OnCreateDefaultContent = OnCreateDefaultContent;
    private static void OnCreateDefaultContent(int serial_number, int docId)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      RhinoDoc doc = RhinoDoc.FromId(docId);

      if (null == p || null == doc)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnCreateDefaultContent");
      }
      else
      {
        try
        {
          p.CreateDefaultContent(doc);
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnCreateDefaultContent\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    internal delegate void OutputTypesCallback(int serial_number, IntPtr pON_wStringExt, IntPtr pON_wStringDesc);
    private static OutputTypesCallback m_OnOutputTypes = OnOutputTypes;
    private static void OnOutputTypes(int serial_number, IntPtr pON_wStringExt, IntPtr pON_wStringDesc)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;

      if (null == p || (IntPtr.Zero == pON_wStringDesc) || (IntPtr.Zero == pON_wStringExt))
      {
        HostUtils.DebugString("ERROR: Invalid input for OnOutputTypes");
      }
      else
      {
        try
        {
          List<OutputTypeInfo> types = p.OutputTypes();

          System.Text.StringBuilder sbExt = new System.Text.StringBuilder();
          System.Text.StringBuilder sbDesc = new System.Text.StringBuilder();

          foreach (OutputTypeInfo type in types)
          {
            if (sbExt.Length != 0)
              sbExt.Append(";");

            sbExt.Append(type.fileExtension);

            if (sbDesc.Length != 0)
              sbDesc.Append(";");

            sbDesc.Append(type.typeDescription);
          }

          UnsafeNativeMethods.ON_wString_Set(pON_wStringExt, sbExt.ToString());
          UnsafeNativeMethods.ON_wString_Set(pON_wStringDesc, sbDesc.ToString());
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnOutputTypes\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    internal delegate IntPtr CreateTexturePreviewCallback(int serial_number, int x, int y, IntPtr pTexture);
    private static CreateTexturePreviewCallback m_OnCreateTexturePreview = OnCreateTexturePreview;
    private static IntPtr OnCreateTexturePreview(int serial_number, int x, int y, IntPtr pTexture)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      RenderTexture texture = IntPtr.Zero == pTexture ? null : RenderContent.FromPointer(pTexture) as RenderTexture;

      if (null == p || null == texture || x == 0 || y == 0)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnCreateTexturePreview");
      }
      else
      {
        try
        {
          System.Drawing.Image preview = p.CreateTexturePreview(new System.Drawing.Size(x, y), texture);

          if (preview != null)
          {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            preview.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            IntPtr pBitmap = new System.Drawing.Bitmap(ms).GetHbitmap();

            return pBitmap;
          }
          return IntPtr.Zero;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnCreateTexturePreview\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }

      return IntPtr.Zero;
    }


    internal delegate IntPtr CreatePreviewCallback(int serial_number, int x, int y, int iQuality, IntPtr pScene);
    private static CreatePreviewCallback m_OnCreatePreview = OnCreatePreview;
    private static IntPtr OnCreatePreview(int serial_number, int x, int y, int iQuality, IntPtr pPreviewScene)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      PreviewScene scene = IntPtr.Zero == pPreviewScene ? null : new PreviewScene(pPreviewScene);

      if (null == p || null == scene || x == 0 || y == 0)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnCreatePreview");
      }
      else
      {
        try
        {
          System.Drawing.Image preview = null;
          if (-1 == iQuality)
          {
            preview = p.CreateQuickPreview(new System.Drawing.Size(x, y), scene);
          }
          else
          {
            preview = p.CreatePreview(new System.Drawing.Size(x, y), (PreviewQualityLevels)iQuality, scene);
          }

          if (preview != null)
          {
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            preview.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);

            IntPtr pBitmap = new System.Drawing.Bitmap(ms).GetHbitmap();

            return pBitmap;
          }
          return IntPtr.Zero;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnCreatePreview\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }

      return IntPtr.Zero;
    }

    internal delegate int DecalCallback(int serial_number, IntPtr pXmlSection, int bInitialize);
    private static DecalCallback m_OnDecalProperties = OnDecalProperties;
    private static int OnDecalProperties(int serial_number, IntPtr pXmlSection, int bInitialize)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      
      if (null == p && pXmlSection!=IntPtr.Zero)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnDecalProperties");
      }
      else
      {
        try
        {
          List<NamedValue> propertyList = XMLSectionUtilities.ConvertToNamedValueList(pXmlSection);

          if (1 != bInitialize)
          {
            if (!p.ShowDecalProperties(ref propertyList))
              return 0;
          }
          else
          {
            p.InitializeDecalProperties(ref propertyList);
          }

          XMLSectionUtilities.SetFromNamedValueList(pXmlSection, propertyList);

          return 1;          
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnDecalProperties\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }

      return 0;
    }
    #endregion

#endif


    /// <summary>
    /// Called by Render and RenderPreview commands if this plug-in is set as the default render engine. 
    /// </summary>
    /// <param name="doc"></param>
    /// <param name="mode"></param>
    /// <param name="fastPreview">If true, lower quality faster render expected</param>
    /// <returns></returns>
    protected abstract Rhino.Commands.Result Render(RhinoDoc doc, Rhino.Commands.RunMode mode, bool fastPreview);

    protected abstract Rhino.Commands.Result RenderWindow(RhinoDoc doc, Rhino.Commands.RunMode modes, bool fastPreview, Rhino.Display.RhinoView view, System.Drawing.Rectangle rect, bool inWindow);
  }

  
  public abstract class DigitizerPlugIn : PlugIn
  {
    protected DigitizerPlugIn()
    {
      // Set callbacks if they haven't been set yet
      if (null == m_OnEnableDigitizer || null == m_OnUnitSystem || null == m_OnPointTolerance)
      {
        m_OnEnableDigitizer = InternalEnableDigitizer;
        m_OnUnitSystem = InternalUnitSystem;
        m_OnPointTolerance = InternalPointTolerance;
        UnsafeNativeMethods.CRhinoDigitizerPlugIn_SetCallbacks(m_OnEnableDigitizer, m_OnUnitSystem, m_OnPointTolerance);
      }
    }

    internal delegate int EnableDigitizerFunc(int plugin_serial_number, int enable);
    internal delegate int UnitSystemFunc(int plugin_serial_number);
    internal delegate double PointToleranceFunc(int plugin_serial_number);
    private static EnableDigitizerFunc m_OnEnableDigitizer;
    private static UnitSystemFunc m_OnUnitSystem;
    private static PointToleranceFunc m_OnPointTolerance;

    private static int InternalEnableDigitizer(int plugin_serial_number, int enable)
    {
      int rc = 0;
      DigitizerPlugIn p = LookUpBySerialNumber(plugin_serial_number) as DigitizerPlugIn;
      if (null == p )
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalEnableDigitizer");
      }
      else
      {
        try
        {
          bool _enable = enable != 0;
          rc = p.EnableDigitizer(_enable) ? 1:0;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in EnableDigitizer\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return rc;
    }

    private static int InternalUnitSystem(int plugin_serial_number)
    {
      Rhino.UnitSystem rc = Rhino.UnitSystem.None;
      DigitizerPlugIn p = LookUpBySerialNumber(plugin_serial_number) as DigitizerPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalUnitSystem");
      }
      else
      {
        try
        {
          rc = p.DigitizerUnitSystem;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in UnitSystem\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return (int)rc;
    }

    private static double InternalPointTolerance(int plugin_serial_number)
    {
      double rc = 0.1;
      DigitizerPlugIn p = LookUpBySerialNumber(plugin_serial_number) as DigitizerPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for InternalPointTolerance");
      }
      else
      {
        try
        {
          rc = p.PointTolerance;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in PointTolerance\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
      return rc;
    }

    /// <summary>
    /// Called by Rhino to enable/disable input from the digitizer.
    /// If enable is true and EnableDigitizer() returns false, then
    /// Rhino will not calibrate the digitizer.
    /// </summary>
    /// <param name="enable">
    /// If true, enable the digitizer.  If false, disable the digitizer.
    /// </param>
    /// <returns></returns>
    protected abstract bool EnableDigitizer(bool enable);

    /// <summary>
    /// Unit system of the points the digitizer passes to SendPoint().
    /// Rhino uses this value when it calibrates a digitizer.
    /// This unit system must be not change.
    /// </summary>
    protected abstract Rhino.UnitSystem DigitizerUnitSystem { get; }

    /// <summary>
    /// The point tolerance is the distance the digitizer must move 
    /// (in digitizer coordinates) for a new point to be considered
    /// real rather than noise. Small desktop digitizer arms have
    /// values like 0.001 inches and 0.01 millimeters.  This value
    /// should never be smaller than the accuracy of the digitizing
    /// device.
    /// </summary>
    protected abstract double PointTolerance { get; }

    /// <summary>
    /// If the digitizer is enabled, call this function to send a point to Rhino.
    /// Call this function as much as you like.  The digitizers that Rhino currently
    /// supports send a point every 15 milliseconds or so. This function should be
    /// called when users press or release any digitizer button.
    /// </summary>
    /// <param name="point">3d point in digitizer coordinates</param>
    /// <param name="mousebuttons">corresponding digitizer button is down</param>
    /// <param name="shiftKey"></param>
    /// <param name="controlKey"></param>
    public void SendPoint(Rhino.Geometry.Point3d point, System.Windows.Forms.MouseButtons mousebuttons, bool shiftKey, bool controlKey)
    {
      uint flags = CreateFlags(mousebuttons, shiftKey, controlKey);
      UnsafeNativeMethods.CRhinoDigitizerPlugIn_SendPoint(m_runtime_serial_number, point, flags);
    }
    public void SendRay(Rhino.Geometry.Ray3d ray, System.Windows.Forms.MouseButtons mousebuttons, bool shiftKey, bool controlKey)
    {
      uint flags = CreateFlags(mousebuttons, shiftKey, controlKey);
      UnsafeNativeMethods.CRhinoDigitizerPlugIn_SendRay(m_runtime_serial_number, ray.Position, ray.Direction, flags);
    }

    static uint CreateFlags(System.Windows.Forms.MouseButtons mousebuttons, bool shiftKey, bool controlKey)
    {
      const int MK_LBUTTON = 0x0001;
      const int MK_RBUTTON = 0x0002;
      const int MK_SHIFT = 0x0004;
      const int MK_CONTROL = 0x0008;
      const int MK_MBUTTON = 0x0010;
      uint flags = 0;
      if (mousebuttons == System.Windows.Forms.MouseButtons.Left)
        flags |= MK_LBUTTON;
      else if (mousebuttons == System.Windows.Forms.MouseButtons.Middle)
        flags |= MK_MBUTTON;
      else if (mousebuttons == System.Windows.Forms.MouseButtons.Right)
        flags |= MK_RBUTTON;
      if (shiftKey)
        flags |= MK_SHIFT;
      if (controlKey)
        flags |= MK_CONTROL;
      return flags;
    }
  }

  /// <summary>
  /// License Manager Utilities
  /// </summary>
  public static class LicenseUtils
  {
    // The entire functionality of this class is implemented through making calls to well defined class names/functions
    // in a different DLL through reflection

    private static System.Reflection.Assembly m_license_client_assembly;
    private static System.Reflection.Assembly GetLicenseClientAssembly()
    {
      if (null == m_license_client_assembly)
      {
        const string filename = "ZooClient.dll";
        System.Reflection.Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        if (thisAssembly != null)
        {
          string path = thisAssembly.Location;
          path = System.IO.Path.GetDirectoryName(path);
          path = string.IsNullOrEmpty(path) ? filename : System.IO.Path.Combine(path, filename);
          if (System.IO.File.Exists(path))
          {
            m_license_client_assembly = System.Reflection.Assembly.LoadFrom(path);
          }
        }
      }
      return m_license_client_assembly;
    }

    /// <summary>
    /// Initializes the license manager
    /// </summary>
    public static bool Initialize()
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("Initialize");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, null);
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Sets the license manager's language id.
    /// </summary>
    public static bool SetLanguage(int languageid)
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("SetLanguage");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, new object[] { languageid });
      if (null == invoke_rc)
        return false;

      return System.Convert.ToBoolean(invoke_rc);
    }

    /// <summary>
    /// Tests connectivity with the Zoo.
    /// </summary>
    public static string Echo(string message)
    {
      if (string.IsNullOrEmpty(message))
        return null;

      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return null;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return null;

      System.Reflection.MethodInfo mi = t.GetMethod("Echo");
      if (mi == null)
        return null;

      string invoke_rc = mi.Invoke(null, new object[] { message }) as String;
      return invoke_rc;
    }

    /// <summary>
    /// Verifies that the valid product license is available for use.
    /// If the product is installed as a standalone node, the local
    /// license will be validated. If the product is installed as a
    /// network node, a loaner license will be requested by the
    /// system's assigned Zoo server.
    /// </summary>
    /// <param name="productType"></param>
    /// <param name="validateDelegate">
    /// Since the license client knows nothing about your product license,
    /// you will need to validate your product license by supplying a 
    /// callback function, or delegate, that can be called for valiation.
    /// </param>
    /// <returns>
    /// True if the license was obtained and validated successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool GetLicense(int productType, ValidateProductKeyDelegate validateDelegate)
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("GetLicense");
      if (mi == null)
        return false;

      // Figure out where this validateDelegate is defined.
      // - If this delegate is defined in a .NET plug-in, use the Assembly location
      //   to get all of the plug-in descriptive information
      // - If this delegate is defined in a C++ plug-in, find the plug-in descriptive
      //   information from the Rhino_DotNet wrapper class which is the delegate's
      //   target
      string path = null;
      string productTitle = null;
      Guid productId = Guid.Empty;

      System.Reflection.MethodInfo delegate_method = validateDelegate.Method;
      System.Reflection.Assembly rhDotNet = HostUtils.GetRhinoDotNetAssembly();
      if (delegate_method.Module.Assembly == rhDotNet)
      {
        object wrapper_class = validateDelegate.Target;
        Type wrapper_type = wrapper_class.GetType();
        System.Reflection.MethodInfo get_path_method = wrapper_type.GetMethod("Path");
        System.Reflection.MethodInfo get_id_method = wrapper_type.GetMethod("ProductId");
        System.Reflection.MethodInfo get_title_method = wrapper_type.GetMethod("ProductTitle");
        path = get_path_method.Invoke(wrapper_class, null) as string;
        productTitle = get_title_method.Invoke(wrapper_class, null) as string;
        productId = (Guid)get_id_method.Invoke(wrapper_class, null);
      }
      else
      {
        path = delegate_method.Module.Assembly.Location;
      }

      if (string.IsNullOrEmpty(path))
        return false;

      if (string.IsNullOrEmpty(productTitle) || productId == Guid.Empty)
      {
        productTitle = PlugIn.NameFromPath(path);
        productId = PlugIn.IdFromPath(path);
      }

      object invoke_rc = mi.Invoke(null, new object[] { path, productId, productType, productTitle, validateDelegate });
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Returns a loaned out license to the Zoo server from which
    /// it was borrowed.
    /// </summary>
    /// <param name="productId">
    /// The plugin that want to return the license.
    /// </param>
    /// <returns>
    /// True if the license was returned successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool ReturnLicense(Guid productId)
    {
      // If the args are changed, go to RhDN_NativeMgr.cpp and make the changes
      // there too so C++ works...
      /*
      System.Reflection.Assembly a = System.Reflection.Assembly.GetCallingAssembly();
      if (plugIn.Assembly != a)
        return false;
      Guid productId = plugIn.Id;
      */

      // Steve and Dale need to figure out what is needed here 
      // to support c++ plugins...

      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("ReturnLicense");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, new object[] { productId });
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Checks out a license that is on loan from a Zoo server
    /// on a permanent basis.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check out.
    /// </param>
    /// <returns>
    /// True if the license was checked out successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool CheckOutLicense(Guid productId)
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("CheckOutLicense");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, new object[] { productId });
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Checks in a previously checked out license to
    /// the Zoo server from which it was checked out.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check in.
    /// </param>
    /// <returns>
    /// True if the license was checked in successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool CheckInLicense(Guid productId)
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("CheckInLicense");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, new object[] { productId });
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Converts a product license from a standalone node
    /// to a network node.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check in.
    /// </param>
    /// <returns>
    /// True if the license was successfully converted.
    /// False if not successful or on error.
    /// </returns>
    public static bool ConvertLicense(Guid productId)
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("ConvertLicense");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, new object[] { productId });
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Returns whether or not license checkout is enabled
    /// </summary>
    public static bool IsCheckOutEnabled()
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return false;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return false;

      System.Reflection.MethodInfo mi = t.GetMethod("IsCheckOutEnabled");
      if (mi == null)
        return false;

      object invoke_rc = mi.Invoke(null, null);
      if (null == invoke_rc)
        return false;

      return (bool)invoke_rc;
    }

    /// <summary>
    /// Returns the current status of every license for ui purposes
    /// </summary>
    public static LicenseStatus[] GetLicenseStatus()
    {
      System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
      if (null == zooAss)
        return null;

      System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
      if (t == null)
        return null;

      System.Reflection.MethodInfo mi = t.GetMethod("GetLicenseStatus");
      if (mi == null)
        return null;

      LicenseStatus[] invoke_rc = mi.Invoke(null, null) as LicenseStatus[];
      if (null == invoke_rc)
        return null;

      return invoke_rc;
    }
  }

  /// <summary>
  /// ValidateProductKeyDelegate result code
  /// </summary>
  public enum ValidateResult : int
  {
    Success = 1,          // The product key or license is validated successfully.
    ErrorShowMessage = 0, // There was an error validating the product key or license,
                          //   the license manager show an error message.
    ErrorHideMessage = -1 // There was an error validating the product key or license,
                          //   the validating delegate will show an error message,
                          //   not the license manager.
  }

  /// <summary>
  /// Validates a product key or license
  /// </summary>
  public delegate ValidateResult ValidateProductKeyDelegate(string productKey, out LicenseData licenseData);

  /// <summary>
  /// License build type enumerations
  /// </summary>
  public enum LicenseBuildType
  {
    Release = 100,      // A release build (e.g. commercical, education, nfr, etc.)
    Evaluation = 200,   // A evaluation build
    Beta = 300          // A beta build (e.g. wip)
  }

  /// <summary>
  /// Zoo plugin license data
  /// </summary>
  public class LicenseData
  {
    #region LicenseData data

    string m_product_license;
    string m_serial_number;
    string m_license_title;
    LicenseBuildType m_build_type;
    int m_license_count;
    DateTime? m_date_to_expire;
    Icon m_product_icon;

    public void Dispose()
    {
      if (null != m_product_icon)
        m_product_icon.Dispose();
      m_product_icon = null;
    }

    /// <summary>
    /// The actual product license. 
    /// This is provided by the plugin that validated the license.
    /// </summary>
    public string ProductLicense
    {
      get { return m_product_license; }
      set { m_product_license = value; }
    }

    /// <summary>
    /// The "for display only" product license.
    /// This is provided by the plugin that validated the license.
    /// </summary>
    public string SerialNumber
    {
      get { return m_serial_number; }
      set { m_serial_number = value; }
    }

    /// <summary>
    /// The title of the license.
    /// This is provided by the plugin that validated the license.
    /// (e.g. "Rhinoceros 5.0 Commercial")
    /// </summary>
    public string LicenseTitle
    {
      get { return m_license_title; }
      set { m_license_title = value; }
    }

    /// <summary>
    /// The build of the product that this license work with.
    /// When your product requests a license from the Zoo, it
    /// will specify one of these build types.
    /// </summary>
    public LicenseBuildType BuildType
    {
      get { return m_build_type; }
      set { m_build_type = value; }
    }

    /// <summary>
    /// The number of instances supported by this license.
    /// This is provided by the plugin that validated the license.
    /// </summary>
    public int LicenseCount
    {
      get { return m_license_count; }
      set { m_license_count = value; }
    }

    /// <summary>
    /// The date and time the license is set to expire.
    /// This is provided by the plugin that validated the license.
    /// This time value should be in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime? DateToExpire
    {
      get { return m_date_to_expire; }
      set { m_date_to_expire = value; }
    }

    /// <summary>
    /// The product's icon. This will displayed in the "license"
    /// page in the Options dialog. Note, this can be null.
    /// Note, LicenseData creates it's own copy of the icon.
    /// </summary>
    public Icon ProductIcon
    {
      get { return m_product_icon; }
      set
      {
        if (null != m_product_icon)
          m_product_icon.Dispose();
        m_product_icon = null;
        if (null != value)
          m_product_icon = (Icon)value.Clone();
      }
    }

    #endregion

    #region LicenseData construction

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData()
    {
      ProductLicense = string.Empty;
      SerialNumber = string.Empty;
      LicenseTitle = string.Empty;
      BuildType = LicenseBuildType.Release;
      LicenseCount = 1;
      DateToExpire = null;
      ProductIcon = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string serialNumber, string licenseTitle)
    {
      ProductLicense = productLicense;
      SerialNumber = serialNumber;
      LicenseTitle = licenseTitle;
      BuildType = LicenseBuildType.Release;
      LicenseCount = 1;
      DateToExpire = null;
      ProductIcon = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string serialNumber, string licenseTitle, LicenseBuildType buildType)
    {
      ProductLicense = productLicense;
      SerialNumber = serialNumber;
      LicenseTitle = licenseTitle;
      BuildType = buildType;
      LicenseCount = 1;
      DateToExpire = null;
      ProductIcon = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string serialNumber, string licenseTitle, LicenseBuildType buildType, int licenseCount)
    {
      ProductLicense = productLicense;
      SerialNumber = serialNumber;
      LicenseTitle = licenseTitle;
      BuildType = buildType;
      LicenseCount = licenseCount;
      DateToExpire = null;
      ProductIcon = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string serialNumber, string licenseTitle, LicenseBuildType buildType, int licenseCount, DateTime? expirationDate)
    {
      ProductLicense = productLicense;
      SerialNumber = serialNumber;
      LicenseTitle = licenseTitle;
      BuildType = buildType;
      LicenseCount = licenseCount;
      DateToExpire = expirationDate;
      ProductIcon = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string serialNumber, string licenseTitle, LicenseBuildType buildType, int licenseCount, DateTime? expirationDate, Icon productIcon)
    {
      ProductLicense = productLicense;
      SerialNumber = serialNumber;
      LicenseTitle = licenseTitle;
      BuildType = buildType;
      LicenseCount = licenseCount;
      DateToExpire = expirationDate;
      ProductIcon = productIcon;
    }


    /// <summary>
    /// Public validator
    /// </summary>
    public bool IsValid()
    {
      bool rc;
      try // Try-catch block will catch null values
      {
        rc = !string.IsNullOrEmpty(ProductLicense);
        if (rc)
          rc = !string.IsNullOrEmpty(SerialNumber);
        if (rc)
          rc = !string.IsNullOrEmpty(LicenseTitle);
        if (rc)
          rc = Enum.IsDefined(typeof(LicenseBuildType), BuildType);
        if (rc)
          rc = (0 < LicenseCount);
        if (rc && DateToExpire.HasValue)
          rc = (0 < DateTime.Compare(DateToExpire.Value, DateTime.UtcNow));
        // Note, ProductIcon can be null
      }
      catch
      {
        rc = false;
      }
      return rc;
    }

    #endregion

    #region LicenseData static members

    /// <summary>
    /// Indicates whether a LicenseData object is either null or invalid.
    /// </summary>
    public static bool IsNotValid(LicenseData data)
    {
      if (null != data && data.IsValid())
        return false;
      return true;
    }

    /// <summary>
    /// Indicates whether a LicenseData object is not null and valid.
    /// </summary>
    public static bool IsValid(LicenseData data)
    {
      if (null != data && data.IsValid())
        return true;
      return false;
    }

    #endregion
  }

  /// <summary>
  /// LicenseType enumeration
  /// </summary>
  public enum LicenseType
  {
    Standalone,       // A standalone license
    Network,          // A network license that has not been fulfilled by a Zoo
    NetworkLoanedOut, // A license on temporary loan from a Zoo
    NetworkCheckedOut // A license on permanent check out from a Zoo
  }

  /// <summary>
  /// LicenseStatus class
  /// </summary>
  public class LicenseStatus
  {
    #region LicenseStatus data

    Guid m_product_id;
    LicenseBuildType m_build_type;
    string m_license_title;
    string m_serial_number;
    LicenseType m_license_type;
    DateTime? m_expiration_date;
    DateTime? m_checkout_expiration_date;
    string m_registered_owner;
    string m_registered_organization;
    Icon m_product_icon;

    /// <summary>
    /// The id of the product or plugin
    /// </summary>
    public Guid ProductId
    {
      get { return m_product_id; }
      set { m_product_id = value; }
    }

    /// <summary>
    /// The build type of the product, where:
    ///   100 = A release build, either commercical, education, nfr, etc.
    ///   200 = A evaluation build
    ///   300 = A beta build, such as a wip
    /// </summary>
    public LicenseBuildType BuildType
    {
      get { return m_build_type; }
      set { m_build_type = value; }
    }

    /// <summary>
    /// The title of the license.
    /// (e.g. "Rhinoceros 5.0 Commercial")
    /// </summary>
    public string LicenseTitle
    {
      get { return m_license_title; }
      set { m_license_title = value; }
    }

    /// <summary>
    /// The "for display only" product license or serial number.
    /// </summary>
    public string SerialNumber
    {
      get { return m_serial_number; }
      set { m_serial_number = value; }
    }

    /// <summary>
    /// The license type.
    /// (e.g. Standalone, Network, etc.)
    /// </summary>
    public LicenseType LicenseType
    {
      get { return m_license_type; }
      set { m_license_type = value; }
    }

    /// <summary>
    /// The date and time the license will expire.
    /// This value can be null if:
    ///   1.) The license type is "Standalone" and the license does not expire.
    ///   2.) The license type is "Network".
    ///   3.) The license type is "NetworkCheckedOut" and the checkout does not expire
    /// Note, date and time is in local time coordinates
    /// </summary>
    public DateTime? ExpirationDate
    {
      get { return m_expiration_date; }
      set { m_expiration_date = value; }
    }

    /// <summary>
    /// The date and time the checked out license will expire.
    /// Note, this is only set if m_license_type = LicenceType.Standalone
    /// and if "limited license checkout" was enabled on the Zoo server.
    /// Note, date and time is in local time coordinates
    /// </summary>
    public DateTime? CheckOutExpirationDate
    {
      get { return m_checkout_expiration_date; }
      set { m_checkout_expiration_date = value; }
    }

    /// <summary>
    /// The registered owner of the product.
    /// (e.g. "Dale Fugier")
    /// </summary>
    public string RegisteredOwner
    {
      get { return m_registered_owner; }
      set { m_registered_owner = value; }
    }

    /// <summary>
    /// The registered organization of the product
    /// (e.g. "Robert McNeel and Associates")
    /// </summary>
    public string RegisteredOrganization
    {
      get { return m_registered_organization; }
      set { m_registered_organization = value; }
    }

    /// <summary>
    /// The product's icon. Note, this can be null.
    /// </summary>
    public Icon ProductIcon
    {
      get { return m_product_icon; }
      set { m_product_icon = value; }
    }

    #endregion

    #region LicenseStatus construction

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseStatus()
    {
      m_product_id = Guid.Empty;
      m_build_type = 0;
      m_license_title = string.Empty;
      m_serial_number = string.Empty;
      m_license_type = PlugIns.LicenseType.Standalone;
      m_expiration_date = null;
      m_checkout_expiration_date = null;
      m_registered_owner = string.Empty;
      m_registered_organization = string.Empty;
      m_product_icon = null;
    }

    #endregion
  }

}
#endif
