using System;
using System.Runtime.InteropServices;
using Rhino.Runtime;

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
    DescriptionType m_type;
    string m_value;
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


  public class PlugIn
  {
    System.Reflection.Assembly m_assembly;
    internal int m_runtime_serial_number; // = 0; runtime initializes this to 0
    internal Rhino.Collections.RhinoList<Commands.Command> m_commands = new Rhino.Collections.RhinoList<Rhino.Commands.Command>();
    private PersistentSettingsManager m_SettingsManager;
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
      PlugIn rc = null;
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
#if !BUILDING_MONO
        else if (rc is DigitizerPlugIn)
          plugin_class = 3;
#endif
        else if (rc is RenderPlugIn)
          plugin_class = 4;

        UnsafeNativeMethods.CRhinoPlugIn_Create(sn, plugin_id, plugin_name, plugin_version, plugin_class);
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
        catch(Exception){}
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
        catch (Exception)
        {
        }
      }
    }
    #endregion

    #region default virtual function implementations
    protected virtual LoadReturnCode OnLoad(ref string message)
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

    /// <summary>
    /// Override this function if you want to extend the options dialog. This function is
    /// called whenever the user brings up the Options dialog.
    /// </summary>
    /// <param name="pages">list of pages to add your custom options dialog page(s) to</param>
    protected virtual void OptionsDialogPages( System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages )
    {
    }
    #endregion

    string m_settings_dir;
    public string SettingsDirectory
    {
      get
      {
        if (null == m_settings_dir)
        {
          string path = null;
          if (HostUtils.RunningOnWindows)
          {
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
            string commonDir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
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
            m_settings_dir = System.IO.Path.Combine(path, "settings");
        }
        return m_settings_dir;
      }
    }


    public PersistentSettings Settings
    {
      get 
      {
        if (m_SettingsManager == null)
        {
          m_SettingsManager = new PersistentSettingsManager(this);
        }

        return m_SettingsManager.PluginSettings;
      }
    }

    public PersistentSettings CommandSettings(string name)
    {
      if (m_SettingsManager == null)
      {
        m_SettingsManager = new PersistentSettingsManager(this);
      }

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

    public static bool LoadPlugIn(Guid pluginId)
    {
      return UnsafeNativeMethods.CRhinoPlugInManager_LoadPlugIn(pluginId);
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
    public FileTypeList()
    {
    }

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
      if (IntPtr.Zero == rc)
        return null;
      return Marshal.PtrToStringUni(rc);
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
  
#if !BUILDING_MONO
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
#endif
  // I think there is an RDK version that we are supposed to use instead
  /*public*/ abstract class RenderPlugIn : PlugIn
  {
    // Render
  }

  public static class LicenseUtils
  {
    // The entire functionality of this class is implemented through making calls to well defined class names/functions
    // in a different DLL through reflection

    private static System.Reflection.Assembly m_license_client_assembly;
    private static System.Reflection.Assembly GetLicenseClientAssembly()
    {
      if (null == m_license_client_assembly)
      {
        string filename = "ZooClient.dll";
        System.Reflection.Assembly thisAssembly = System.Reflection.Assembly.GetExecutingAssembly();
        if (thisAssembly != null)
        {
          string path = thisAssembly.Location;
          path = System.IO.Path.GetDirectoryName(path);
          path = System.IO.Path.Combine(path, filename);
          if (System.IO.File.Exists(path))
          {
            m_license_client_assembly = System.Reflection.Assembly.LoadFrom(path);
          }
        }
      }
      return m_license_client_assembly;
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
    /// <param name="productId">
    /// The Guid of the product whose license you want to verify
    /// or request from a Zoo server.
    /// </param>
    /// <param name="productTitle">
    /// A localized product title, such as "Rhinoceros 5.0", that can
    /// be used in license request forms or message boxes, if needed.
    /// </param>
    /// <param name="validateDelegate">
    /// Since the license client knows nothing about your product license,
    /// you will need to valiate your product license by supplying a 
    /// callback function, or delegate, that can be called for valiation.
    /// </param>
    /// <returns>
    /// True if the license was obtained and validated successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool GetLicense(Guid productId, string productTitle, ValidateProductKeyDelegate validateDelegate)
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

      object invoke_rc = mi.Invoke(null, new object[] { productId, productTitle, validateDelegate });
      if (null == invoke_rc)
        return false;

      return System.Convert.ToBoolean(invoke_rc);
    }

    /// <summary>
    /// Returns a loaned out license to the Zoo server from which
    /// it was borrowed.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to return.
    /// </param>
    /// <returns>
    /// True if the license was returned successful.
    /// False if not successful or on error.
    /// </returns>
    public static bool ReturnLicense(Guid productId)
    {
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

      return System.Convert.ToBoolean(invoke_rc);
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

      return System.Convert.ToBoolean(invoke_rc);
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

      return System.Convert.ToBoolean(invoke_rc);
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

      return System.Convert.ToBoolean(invoke_rc);
    }
  }

  /// <summary>
  /// Validates a product key or license
  /// </summary>
  public delegate bool ValidateProductKeyDelegate(string productKey, out LicenseData licenseData);

  /// <summary>
  /// Zoo plugin license data
  /// </summary>
  public class LicenseData
  {
    #region LicenseData data

    string m_product_license;
    string m_display_license;
    string m_license_title;
    int m_license_count;
    DateTime? m_date_to_expire;

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
    public string DisplayLicense
    {
      get { return m_display_license; }
      set { m_display_license = value; }
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

    #endregion

    #region LicenseData construction

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData()
    {
      ProductLicense = string.Empty;
      DisplayLicense = string.Empty;
      LicenseTitle = string.Empty;
      LicenseCount = 1;
      DateToExpire = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string displayLicense, string licenseTitle)
    {
      ProductLicense = productLicense;
      DisplayLicense = displayLicense;
      LicenseTitle = licenseTitle;
      LicenseCount = 1;
      DateToExpire = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string displayLicense, string licenseTitle, int licenseCount)
    {
      ProductLicense = productLicense;
      DisplayLicense = displayLicense;
      LicenseTitle = licenseTitle;
      LicenseCount = licenseCount;
      DateToExpire = null;
    }

    /// <summary>
    /// Public constructor
    /// </summary>
    public LicenseData(string productLicense, string displayLicense, string licenseTitle, int licenseCount, DateTime? expirationDate)
    {
      ProductLicense = productLicense;
      DisplayLicense = displayLicense;
      LicenseTitle = licenseTitle;
      LicenseCount = licenseCount;
      DateToExpire = expirationDate;
    }

    /// <summary>
    /// Public validator
    /// </summary>
    public bool IsValid()
    {
      bool rc = false;
      try // Try-catch block will catch null values
      {
        rc = !string.IsNullOrEmpty(ProductLicense);
        if (rc)
          rc = !string.IsNullOrEmpty(DisplayLicense);
        if (rc)
          rc = !string.IsNullOrEmpty(LicenseTitle);
        if (rc)
          rc = (0 < LicenseCount);
        if (rc && DateToExpire.HasValue)
          rc = (0 < DateTime.Compare(DateToExpire.Value, DateTime.Now.ToUniversalTime()));
      }
      catch
      {
        rc = false;
      }
      return rc;
    }

    #endregion

    #region LicenseData static methods

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

}

