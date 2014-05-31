#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;
using Rhino.Runtime;
using System.Collections.Generic;
using Rhino.Drawing;
using Rhino.Runtime.InteropWrappers;

#if RDK_CHECKED
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
    /// <summary>never load plug-in.</summary>
    Disabled = 0,
    /// <summary>Load when Rhino starts.</summary>
    AtStartup = 1,
    /// <summary>(default) Load the first time a plug-in command used.</summary>
    WhenNeeded = 2,
    /// <summary>Load the first time a plug-in command used NOT when restoring docking control bars.</summary>
    WhenNeededIgnoreDockingBars = 6,
    /// <summary>When a plug-in command is used or the options dialog is shown.</summary>
    WhenNeededOrOptionsDialog = 10
  }

  [Flags]
  public enum PlugInType : int
  {
    None=0,
    Render = 1,
    FileImport = 2,
    FileExport = 4,
    Digitizer = 8,
    Utility = 16,
    DisplayPipeline = 32,
    DisplayEngine = 64,
    Any = Render | FileImport | FileExport | Digitizer | Utility | DisplayPipeline | DisplayEngine
  }

  public class PlugIn
  {
    System.Reflection.Assembly m_assembly;
    internal int m_runtime_serial_number; // = 0; runtime initializes this to 0
    internal List<Commands.Command> m_commands = new List<Commands.Command>();
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
    /// Only searches through list of RhinoCommon plug-ins.
    /// </summary>
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
    /// Finds the plug-in instance that was loaded from a given assembly.
    /// </summary>
    /// <param name="pluginAssembly">The plug-in assembly.
    /// <para>You can get the assembly instance at runtime with the <see cref="System.Type.Assembly"/> instance property.</para></param>
    /// <returns>The assembly plug-in instance if successful. Otherwise, null.</returns>
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

    /// <summary>
    /// Finds the plug-in instance that was loaded from a given plug-in Id.
    /// </summary>
    /// <param name="plugInId">The plug-in Id.</param>
    /// <returns>The plug-in instance if successful. Otherwise, null.</returns>
    public static PlugIn Find(System.Guid plugInId)
    {
      if (System.Guid.Empty == plugInId)
        return null;
      for (int i = 0; i < m_plugins.Count; i++)
      {
        if (m_plugins[i].Id == plugInId)
          return m_plugins[i];
      }
      return null;
    }

    /// <summary>Source assembly for this plug-in.</summary>
    public System.Reflection.Assembly Assembly { get { return m_assembly; } }

    public Guid Id { get { return m_id; } }

    public String Name { get { return m_name; } }

    public String Version { get { return m_version; } }

    /// <summary>All of the commands associated with this plug-in.</summary>
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
    /// this property.
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
        m_OnAddPagesToObjectProperties = InternalAddPagesToObjectProperties;
        UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks(m_OnLoad, m_OnShutDown, m_OnGetPlugInObject);
        UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks2(m_OnCallWriteDocument, m_OnWriteDocument, m_OnReadDocument);
        UnsafeNativeMethods.CRhinoPlugIn_SetCallbacks3(m_OnAddPagesToOptions, m_OnAddPagesToObjectProperties);
      }
    }

    #region virtual function callbacks
    internal delegate int OnLoadDelegate(int plugin_serial_number);
    internal delegate void OnShutdownDelegate(int plugin_serial_number);
    internal delegate IntPtr OnGetPlugInObjectDelegate(int plugin_serial_number);
    internal delegate int CallWriteDocumentDelegate(int plugin_serial_number, IntPtr pWriteOptions);
    internal delegate int WriteDocumentDelegate(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pWriteOptions);
    internal delegate int ReadDocumentDelegate(int plugin_serial_number, int doc_id, IntPtr pBinaryArchive, IntPtr pReadOptions);
    internal delegate void OnAddPagesToObjectPropertiesDelegate(int plugin_serial_number, IntPtr pPageList);
    internal delegate void OnAddPagesToOptionsDelegate(int plugin_serial_number, IntPtr pPageList, int options_page, int doc_id);

    private static OnLoadDelegate m_OnLoad;
    private static OnShutdownDelegate m_OnShutDown;
    private static OnGetPlugInObjectDelegate m_OnGetPlugInObject;
    private static CallWriteDocumentDelegate m_OnCallWriteDocument;
    private static WriteDocumentDelegate m_OnWriteDocument;
    private static ReadDocumentDelegate m_OnReadDocument;
    private static OnAddPagesToObjectPropertiesDelegate m_OnAddPagesToObjectProperties;
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

#if RDK_CHECKED
          // after calling the OnLoad function, check to see if we should be creating
          // an RDK plugin. This is the typical spot where C++ plug-ins perform their
          // RDK initialization.
          if (rc == LoadReturnCode.Success && p is RenderPlugIn)
            RdkPlugIn.GetRdkPlugIn(p.Id, plugin_serial_number);
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

          // See if there is a Skin that has settings and is not associated with
          // a plug-in. If there is one, write the settings and mark that we have
          // done this once
          Rhino.Runtime.Skin.WriteSettings();
          

#if RDK_CHECKED
          // check to see if we should be uninitializing an RDK plugin
          RdkPlugIn pRdk = RdkPlugIn.FromRhinoPlugIn(p);
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

    private static void InternalAddPagesToObjectProperties(int plugin_serial_number, IntPtr pPageList)
    {
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        try
        {
          System.Collections.Generic.List<Rhino.UI.ObjectPropertiesPage> pages = new System.Collections.Generic.List<Rhino.UI.ObjectPropertiesPage>();
          p.ObjectPropertiesPages(pages);
          for (int i = 0; i < pages.Count; i++)
          {
            IntPtr ptr = pages[i].ConstructWithRhinoDotNet();
            if (ptr != IntPtr.Zero)
              UnsafeNativeMethods.CRhinoPlugIn_AddObjectPropertiesPage(pPageList, ptr);
          }
        }
        catch (Exception e)
        {
          HostUtils.ExceptionReport(e);
        }
      }
    }

    private static void InternalAddPagesToOptions(int plugin_serial_number, IntPtr pPageList, int options_page, int doc_id)
    {
      PlugIn p = LookUpBySerialNumber(plugin_serial_number);
      if (p != null)
      {
        try
        {
          System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages = new System.Collections.Generic.List<Rhino.UI.OptionsDialogPage>();
          if (options_page == 1)
          {
            p.OptionsDialogPages(pages);
            for (int i = 0; i < pages.Count; i++)
            {
              IntPtr ptr = pages[i].ConstructWithRhinoDotNet();
              if (ptr != IntPtr.Zero)
                UnsafeNativeMethods.CRhinoPlugIn_AddOptionPage(pPageList, ptr);
            }
          }
          else
          {
            p.DocumentPropertiesDialogPages(RhinoDoc.FromId(doc_id), pages);
            for (int i = 0; i < pages.Count; i++)
            {
              IntPtr ptr = pages[i].ConstructWithRhinoDotNet();
              if (ptr != IntPtr.Zero)
                UnsafeNativeMethods.CRhinoPlugIn_AddOptionPage(pPageList, ptr);
            }
          }
        }
        catch (Exception e)
        {
          HostUtils.ExceptionReport(e);
        }
      }
    }
    #endregion

    #region default virtual function implementations
    /// <summary>
    /// Is called when the plug-in is being loaded.
    /// </summary>
    /// <param name="errorMessage">
    /// If a load error is returned and this string is set. This string is the 
    /// error message that will be reported back to the user.
    /// </param>
    /// <returns>An appropriate load return code.
    /// <para>The default implementation returns <see cref="LoadReturnCode.Success"/>.</para></returns>
    protected virtual LoadReturnCode OnLoad(ref string errorMessage)
    {
      return LoadReturnCode.Success;
    }

    protected virtual void OnShutdown()
    {
    }

    bool m_create_commands_called = false;
    internal void InternalCreateCommands()
    {
      CreateCommands();
      m_create_commands_called = true;
    }

    /// <summary>
    /// Called right after plug-in is created and is responsible for creating
    /// all of the commands in a given plug-in.  The base class implementation
    /// Constructs an instance of every publicly exported command class in your
    /// plug-in's assembly.
    /// </summary>
    protected virtual void CreateCommands()
    {
      if (m_create_commands_called)
        return;
      m_create_commands_called = true;

      Type[] exported_types = this.Assembly.GetExportedTypes();
      if (null == exported_types)
        return;

      Type command_type = typeof(Commands.Command);
      for (int i = 0; i < exported_types.Length; i++)
      {
        if (command_type.IsAssignableFrom(exported_types[i]) && !exported_types[i].IsAbstract)
        {
          CreateCommandsHelper(this, this.NonConstPointer(), exported_types[i], null);
        }
      }
    }

    protected bool RegisterCommand(Rhino.Commands.Command command)
    {
      return CreateCommandsHelper(this, this.NonConstPointer(), command.GetType(), command);
    }

    internal static PlugIn m_active_plugin_at_command_creation = null;
    internal static bool CreateCommandsHelper(PlugIn plugin, IntPtr pPlugIn, Type command_type, Commands.Command new_command)
    {
      bool rc = false;
      try
      {
        // added in case the command tries to access it's plug-in in the constructor
        m_active_plugin_at_command_creation = plugin;
        if( new_command==null )
          new_command = (Commands.Command)System.Activator.CreateInstance(command_type);
        new_command.PlugIn = plugin;
        m_active_plugin_at_command_creation = null;

        if (null != plugin)
          plugin.m_commands.Add(new_command);

        int commandStyle = 0;
        object[] styleattr = command_type.GetCustomAttributes(typeof(Commands.CommandStyleAttribute), true);
        if (styleattr != null && styleattr.Length > 0)
        {
          Commands.CommandStyleAttribute a = (Commands.CommandStyleAttribute)styleattr[0];
          new_command.m_style_flags = a.Styles;
          commandStyle = (int)new_command.m_style_flags;
        }

        int sn = new_command.m_runtime_serial_number;
        Guid id = new_command.Id;
        string englishName = new_command.EnglishName;
        string localName = new_command.LocalName;

        int ct = 0;
        if (command_type.IsSubclassOf(typeof(Commands.TransformCommand)))
          ct = 1;
        if (command_type.IsSubclassOf(typeof(Commands.SelCommand)))
          ct = 2;

        UnsafeNativeMethods.CRhinoCommand_Create(pPlugIn, id, englishName, localName, sn, commandStyle, ct);

        rc = true;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return rc;
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
    /// <param name="options">The file write options, such as "include preview image" and "include render meshes".</param>
    /// <returns>
    /// true if the plug-in wants to save document user data in the
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
    /// <param name="doc">The Rhino document instance that is being saved.</param>
    /// <param name="archive">
    /// OpenNURBS file archive object Rhino is using to write the file.
    /// Use BinaryArchiveWriter.Write*() functions to write plug-in data.
    /// OR use the ArchivableDictionary
    /// 
    /// If any BinaryArchiveWriter.Write*() functions throw an exception, 
    /// then archive.WriteErrorOccured will be true and you should immediately return.
    /// Setting archive.WriteErrorOccured to true will cause Rhino to stop saving the file.
    /// </param>
    /// <param name="options">The file write options, such as "include preview image" and "include render meshes".</param>
    protected virtual void WriteDocument(Rhino.RhinoDoc doc, Rhino.FileIO.BinaryArchiveWriter archive, Rhino.FileIO.FileWriteOptions options)
    {
    }

    /// <summary>
    /// Called whenever a Rhino document is being loaded and plug-in user data was
    /// encountered written by a plug-in with this plug-in's GUID.
    /// </summary>
    /// <param name="doc">A Rhino document that is being loaded.</param>
    /// <param name="archive">
    /// OpenNURBS file archive object Rhino is using to read this file.
    /// Use BinaryArchiveReader.Read*() functions to read plug-in data.
    /// 
    /// If any BinaryArchive.Read*() functions throws an exception then
    /// archive.ReadErrorOccurve will be true and you should immediately return.
    /// </param>
    /// <param name="options">Describes what is being written.</param>
    protected virtual void ReadDocument(Rhino.RhinoDoc doc, Rhino.FileIO.BinaryArchiveReader archive, Rhino.FileIO.FileReadOptions options)
    {
    }

#if RHINO_SDK
    /// <summary>
    /// Override this function if you want to extend the options dialog. This function is
    /// called whenever the user brings up the Options dialog.
    /// </summary>
    /// <param name="pages">list of pages to add your custom options dialog page(s) to.</param>
    protected virtual void OptionsDialogPages( System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages )
    {
    }

    /// <summary>
    /// Override this function if you want to extend the document properties sections
    /// of the options dialog. This function is called whenever the user brings up the
    /// Options dialog.
    /// </summary>
    /// <param name="doc">document that the pages are set up for</param>
    /// <param name="pages">list of pages to add your custom options dialog page(s) to.</param>
    protected virtual void DocumentPropertiesDialogPages(RhinoDoc doc, System.Collections.Generic.List<Rhino.UI.OptionsDialogPage> pages)
    {
    }

    /// <summary>
    /// Override this function is you want to extend the object properties dialog
    /// </summary>
    /// <param name="pages"></param>
    protected virtual void ObjectPropertiesPages(System.Collections.Generic.List<Rhino.UI.ObjectPropertiesPage> pages)
    {
    }
#endif
    #endregion

    #region licensing functions

    /// <summary>
    /// Verifies that there is a valid product license for your plug-in, using
    /// the Rhino licensing system. If the plug-in is installed as a standalone
    /// node, the locally installed license will be validated. If the plug-in
    /// is installed as a network node, a loaner license will be requested by
    /// the system's assigned Zoo server. If the Zoo server finds and returns 
    /// a license, then this license will be validated. If no license is found,
    /// then the user will be prompted to provide a license key, which will be
    /// validated.
    /// </summary>
    /// <param name="productBuildType">
    /// The product build contentType required by your plug-in.
    /// </param>
    /// <param name="validateDelegate">
    /// Since the Rhino licensing system knows nothing about your product license,
    /// you will need to validate the product license provided by the Rhino 
    /// licensing system. This is done by supplying a callback function, or delegate,
    /// that can be called to perform the validation.
    /// </param>
    /// <returns>
    /// true if a valid license was found. false otherwise.
    /// </returns>
    public bool GetLicense(LicenseBuildType productBuildType, ValidateProductKeyDelegate validateDelegate)
    {
      string productPath = this.Assembly.Location;
      Guid productId = this.Id;
      string productTitle = this.Name;
      bool rc = LicenseUtils.GetLicense(productPath, productId, (int)productBuildType, productTitle, validateDelegate);
      return rc;
    }

    public bool AskUserForLicense(LicenseBuildType productBuildType, bool standAlone, string textMask, System.Windows.Forms.IWin32Window parent, ValidateProductKeyDelegate validateDelegate)
    {
      string productPath = this.Assembly.Location;
      Guid productId = this.Id;
      string productTitle = this.Name;
      bool rc = LicenseUtils.AskUserForLicense(productPath, standAlone, parent, productId, (int)productBuildType, productTitle, textMask, validateDelegate);
      return rc;
    }


    /// <summary>
    /// Verifies that there is a valid product license for your plug-in, using
    /// the Rhino licensing system. If the plug-in is installed as a standalone
    /// node, the locally installed license will be validated. If the plug-in
    /// is installed as a network node, a loaner license will be requested by
    /// the system's assigned Zoo server. If the Zoo server finds and returns 
    /// a license, then this license will be validated. If no license is found,
    /// then the user will be prompted to provide a license key, which will be
    /// validated.
    /// </summary>
    /// <param name="licenseCapabilities">
    /// In the event that a license was not found, or if the user wants to change
    /// the way your plug-in is licenses, then provide what capabilities your
    /// license has by using this enumeration flag.
    /// </param>
    /// <param name="textMask">
    /// In the event that the user needs to be asked for a license, then you can
    /// provide a text mask, which helps the user to distinguish between proper
    /// and improper user input of your license code. Note, if you do not want
    /// to use a text mask, then pass in a null value for this parameter.
    /// For more information on text masks, search MSDN for the System.Windows.Forms.MaskedTextBox class.
    /// </param>
    /// <param name="validateDelegate">
    /// Since the Rhino licensing system knows nothing about your product license,
    /// you will need to validate the product license provided by the Rhino 
    /// licensing system. This is done by supplying a callback function, or delegate,
    /// that can be called to perform the validation.
    /// </param>
    /// <returns>
    /// true if a valid license was found. false otherwise.
    /// </returns>
    public bool GetLicense(LicenseCapabilities licenseCapabilities, string textMask, ValidateProductKeyDelegate validateDelegate)
    {
      string productPath = this.Assembly.Location;
      Guid productId = this.Id;
      string productTitle = this.Name;
      bool rc = LicenseUtils.GetLicense(productPath, productId, productTitle, licenseCapabilities, textMask, validateDelegate);
      return rc;
    }

    /// <summary>
    /// Returns, or releases, a product license that was obtained from the Rhino
    /// licensing system. Note, most plug-ins do not need to call this as the
    /// Rhino licensing system will return all licenses when Rhino shuts down. 
    /// </summary>
    public bool ReturnLicense()
    {
      string productPath = this.Assembly.Location;
      Guid productId = this.Id;
      string productTitle = this.Name;
      bool rc = LicenseUtils.ReturnLicense(productPath, productId, productTitle);
      return rc;
    }

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

    static string PlugInNameFromAssembly(System.Reflection.Assembly assembly)
    {
      object[] name = assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
      string plugin_name;
      if (name != null && name.Length > 0)
        plugin_name = ((System.Reflection.AssemblyTitleAttribute)name[0]).Title;
      else
        plugin_name = assembly.GetName().Name;
      return plugin_name;
    }

    static string PlugInNameFromId(Guid pluginId)
    {
      Dictionary<Guid,string>plugins = PlugIn.GetInstalledPlugIns();
      string result;
      plugins.TryGetValue(pluginId, out result);
      return result;
    }

    internal delegate bool GetPlugInSettingsFolderDelegate(bool localUser, Guid plugInId, IntPtr pResult);
    internal static GetPlugInSettingsFolderDelegate GetPlugInSettingsFolderHook = GetPlugInSettingsFolderHelper;
    internal static bool GetPlugInSettingsFolderHelper(bool localUser, Guid plugInId, IntPtr pResult)
    {
      var result = SettingsDirectoryHelper(localUser, null, plugInId);
      if (string.IsNullOrEmpty(result))
        return false;
      UnsafeNativeMethods.ON_wString_Set(pResult, result);
      return true;
    }

    private static bool CopyRuiFile(Guid plugInId, string fromFullPath, string fileNamePlusExtension, IntPtr pResult)
    {
      var settings_directory = SettingsDirectoryHelper(true, null, plugInId);
      if (string.IsNullOrEmpty(settings_directory))
        return false;
      var settings_rui_file = settings_directory + System.IO.Path.DirectorySeparatorChar + fileNamePlusExtension;
      m_plug_in_rui_dictionary[fromFullPath] = settings_rui_file;
      if (System.IO.File.Exists(settings_rui_file))
      {
        UnsafeNativeMethods.ON_wString_Set(pResult, settings_rui_file);
        return true;
      }
      if (!System.IO.Directory.Exists(settings_directory))
      {
        try
        {
          System.IO.Directory.CreateDirectory(settings_directory);
        }
        catch (Exception e1)
        {
          var message = string.Format(UI.LOC.STR("Error creating plug-in settings folder {0}.\n\nException:{1}\n"), settings_directory, e1.Message);
          System.Windows.Forms.MessageBox.Show(message,
                                                UI.LOC.STR("Create Plug-in Settings Folder Error"),
                                                System.Windows.Forms.MessageBoxButtons.OK,
                                                System.Windows.Forms.MessageBoxIcon.Asterisk,
                                                System.Windows.Forms.MessageBoxDefaultButton.Button1,
                                                System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly
                                              );
          return false;
        }
      }
      try
      {
        System.IO.File.Copy(fromFullPath, settings_rui_file);
      }
      catch (Exception e2)
      {
        var message = string.Format(UI.LOC.STR("Error copying plug-in RUI file from {0} to {1}.\n\nException:{2}\n"), fromFullPath, settings_rui_file, e2.Message);
        System.Windows.Forms.MessageBox.Show(message,
                                              UI.LOC.STR("Copy Plug-in RUI File Error"),
                                              System.Windows.Forms.MessageBoxButtons.OK,
                                              System.Windows.Forms.MessageBoxIcon.Asterisk,
                                              System.Windows.Forms.MessageBoxDefaultButton.Button1,
                                              System.Windows.Forms.MessageBoxOptions.DefaultDesktopOnly
                                            );
        return false;
      }
      UnsafeNativeMethods.ON_wString_Set(pResult, settings_rui_file);
      return true;
    }

    internal delegate bool GetPlugInRuiFileNameDelegate(IntPtr fullPathToPlugIn, Guid plugInId, IntPtr pResult);
    internal static GetPlugInRuiFileNameDelegate GetPlugInRuiFileNameHook = GetPlugInRuiFileNameHelper;
    internal static bool GetPlugInRuiFileNameHelper(IntPtr fullPathToPlugIn, Guid plugInId, IntPtr pResult)
    {
      var full_path_to_plug_in = Marshal.PtrToStringUni(fullPathToPlugIn);
      if (string.IsNullOrEmpty(full_path_to_plug_in) || !System.IO.File.Exists(full_path_to_plug_in))
        return false;
      var directory = System.IO.Path.GetDirectoryName(full_path_to_plug_in);
      var file_name = System.IO.Path.GetFileNameWithoutExtension(full_path_to_plug_in);
      const string extension = ".rui";
      var rui_file = directory + System.IO.Path.DirectorySeparatorChar + file_name + extension;
      if (!System.IO.File.Exists(rui_file))
      {
        var parent_dir = System.IO.Path.GetDirectoryName(directory);
        rui_file = parent_dir + System.IO.Path.DirectorySeparatorChar + file_name + extension;
        if (!System.IO.File.Exists(rui_file))
          return false;
      }
      UnsafeNativeMethods.ON_wString_Set(pResult, rui_file);
      return CopyRuiFile(plugInId, rui_file, file_name + extension, pResult);
    }

    /// <summary>
    /// Toolbars uses reflection to access this method, do NOT remove or rename it
    /// without updating the references in the Toolbars plug-in.  This is a hack for
    /// Rhino 5 only, in V6 all of the toolbar related plug-in stuff has been moved
    /// into the Toolbars plug-in.
    /// </summary>
    /// <returns></returns>
    internal static Dictionary<string, string> PlugInRuiDictionary()
    {
      return m_plug_in_rui_dictionary;
    }
    internal delegate void ValidateRegisteredPlugInRuiFileNameDelegate(IntPtr registerdRuiFile, IntPtr fullPathToPlugIn, Guid plugInId, IntPtr pResult);
    internal static ValidateRegisteredPlugInRuiFileNameDelegate ValidateRegisteredPlugInRuiFileNameHook = ValidateRegisteredPlugInRuiFileNameHelper;
    private static readonly Dictionary<string,string> m_plug_in_rui_dictionary = new Dictionary<string, string>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registeredRuiFile"></param>
    /// <param name="fullPathToPlugIn"></param>
    /// <param name="plugInId"></param>
    /// <param name="pResult"></param>
    /// <returns>
    /// 0 = no file name or do noting
    /// 1 = file name found and useable
    /// 2 = file name modified, update the registered file name
    /// </returns>
    internal static void ValidateRegisteredPlugInRuiFileNameHelper(IntPtr registeredRuiFile, IntPtr fullPathToPlugIn, Guid plugInId, IntPtr pResult)
    {
      var registerd_rui_file = Marshal.PtrToStringUni(registeredRuiFile);
      if (string.IsNullOrEmpty(registerd_rui_file)) return;
      var full_path_to_plug_in = Marshal.PtrToStringUni(fullPathToPlugIn);
      if (string.IsNullOrEmpty(full_path_to_plug_in)) return;
      var plug_in_folder = System.IO.Path.GetDirectoryName(full_path_to_plug_in);
      if (plug_in_folder == null) return;
      var rui_folder = System.IO.Path.GetDirectoryName(registerd_rui_file);
      // Check to see if the registered file name is equal to the plug-in file name
      var rui_file_name = System.IO.Path.GetFileNameWithoutExtension(registerd_rui_file);
      var plug_in_file_name = System.IO.Path.GetFileNameWithoutExtension(full_path_to_plug_in);
      if (rui_file_name != null && rui_file_name.Equals(plug_in_file_name, StringComparison.Ordinal))
      {
        // Now check to see if the registered folder is the same as the settings folder
        var settings_folder = SettingsDirectoryHelper(true, null, plugInId);
        // If pointing to the settings folder make sure the file gets copied as necessary
        // and return.
        if (rui_folder != null && rui_folder.Equals(settings_folder, StringComparison.Ordinal))
        {
          GetPlugInRuiFileNameHelper(fullPathToPlugIn, plugInId, pResult);
          return;

        }
      }
      // Now check to see if the registered file name is in the plug-in folder
      var in_plug_in_folder = plug_in_folder.Equals(rui_folder, StringComparison.Ordinal);
      if (!in_plug_in_folder)
      {
        // Check to see if it is in the plug-in's parent folder
        var parent_folder = System.IO.Path.GetDirectoryName(rui_folder);
        in_plug_in_folder = plug_in_folder.Equals(parent_folder, StringComparison.Ordinal);
      }
      if (!in_plug_in_folder)
      {
        // Not in the plug-in or plug-in parent folder
        UnsafeNativeMethods.ON_wString_Set(pResult, registerd_rui_file);
        return;
      }
      // In the plug-in folder so check to see if it has the same name as the RHP file 
      var rui_file_test = System.IO.Path.GetDirectoryName(registerd_rui_file) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(registerd_rui_file);
      var plug_in_file_test = System.IO.Path.GetDirectoryName(full_path_to_plug_in) + System.IO.Path.DirectorySeparatorChar + System.IO.Path.GetFileNameWithoutExtension(full_path_to_plug_in);
      if (!rui_file_test.Equals(plug_in_file_test, StringComparison.Ordinal))
      {
        // The file name does not match the plug-in file name so use the file name
        UnsafeNativeMethods.ON_wString_Set(pResult, registerd_rui_file);
        return;
      }
      // The file name is the same as the plug-in file so copy RUI file to the
      // plug-in's settings folder and use the settings folder name.
      CopyRuiFile(plugInId, registerd_rui_file, System.IO.Path.GetFileName(registerd_rui_file), pResult);
    }

    internal static string SettingsDirectoryHelper(bool bLocalUser, System.Reflection.Assembly assembly, Guid pluginId)
    {
      string result = null;
      string path = null;
      if (HostUtils.RunningOnWindows)
      {
        string name;
        if (null == assembly)
          name = PlugInNameFromId(pluginId);
        else
        {
          name = PlugInNameFromAssembly(assembly);
          object[] idAttr = assembly.GetCustomAttributes(typeof(GuidAttribute), false);
          GuidAttribute idattr = (GuidAttribute)(idAttr[0]);
          pluginId = new Guid(idattr.Value);
        }

        System.Globalization.CultureInfo ci = System.Globalization.CultureInfo.InvariantCulture;
        string pluginName = string.Format(ci, "{0} ({1})", name, pluginId.ToString().ToLower(ci));
        // remove invalid characters from string
        char[] invalid_chars = System.IO.Path.GetInvalidFileNameChars();
        int index = pluginName.IndexOfAny(invalid_chars);
        while (index >= 0)
        {
          pluginName = pluginName.Remove(index, 1);
          index = pluginName.IndexOfAny(invalid_chars);
        }
        string commonDir = Environment.GetFolderPath(bLocalUser ? Environment.SpecialFolder.ApplicationData : Environment.SpecialFolder.CommonApplicationData);
        char sep = System.IO.Path.DirectorySeparatorChar;
        commonDir = System.IO.Path.Combine(commonDir, "McNeel" + sep + "Rhinoceros" + sep + "5.0" + sep + "Plug-ins");
        path = System.IO.Path.Combine(commonDir, pluginName);
      }
      else if (HostUtils.RunningOnOSX)
      {
        if (null == assembly)
        {
          // TODO: Add support for settings classes in plug-in SDK dll's
          throw new NotImplementedException("Tell steve@mcneel.com about this");
        }
        // put the settings directory next to the rhp
        path = System.IO.Path.GetDirectoryName(assembly.Location);
      }
      if (!string.IsNullOrEmpty(path))
        result = System.IO.Path.Combine(path, "settings");
      return result;
    }

    public PersistentSettings Settings
    {
      get 
      {
        if (m_SettingsManager == null)
          m_SettingsManager = PersistentSettingsManager.Create(this);
        return m_SettingsManager.PluginSettings;
      }
    }

    public PersistentSettings CommandSettings(string name)
    {
      if (m_SettingsManager == null)
        m_SettingsManager = PersistentSettingsManager.Create(this);
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

    public static bool PlugInExists(Guid id, out bool loaded, out bool loadProtected)
    {
      loaded = loadProtected = false;
      int index = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInIndexFromId(id);
      if (index < 0)
        return false;
      loaded = UnsafeNativeMethods.CRhinoPlugInManager_PassesFilter(index, (int)PlugInType.Any, true, false, false);
      loadProtected = UnsafeNativeMethods.CRhinoPlugInManager_PassesFilter(index, (int)PlugInType.Any, false, false, true);
      return true;
    }

    public static Dictionary<Guid, string> GetInstalledPlugIns()
    {
      int count = InstalledPlugInCount;
      Dictionary<Guid, string> plug_in_dictionary = new Dictionary<Guid, string>(32);
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr_name = UnsafeNativeMethods.CRhinoPlugInManager_GetName(i);
        if (ptr_name != IntPtr.Zero)
        {
          string name = Marshal.PtrToStringUni(ptr_name);
          if (!string.IsNullOrEmpty(name))
          {
            Guid id = UnsafeNativeMethods.CRhinoPlugInManager_GetID(i);
            if (id != Guid.Empty && !plug_in_dictionary.ContainsKey(id))
              plug_in_dictionary.Add(id, name);
          }
        }
      }
      return plug_in_dictionary;
    }

    public static string[] GetInstalledPlugInNames()
    {
      return GetInstalledPlugInNames(PlugInType.Any, true, true);
    }

    /// <summary>
    /// Gets a list of installed plug-in names.  The list can be restricted by some filters.
    /// </summary>
    /// <param name="typeFilter">
    /// The enumeration flags that determine which types of plug-ins are included.
    /// </param>
    /// <param name="loaded">true if loaded plug-ins are returned.</param>
    /// <param name="unloaded">true if unloaded plug-ins are returned.</param>
    /// <returns>An array of installed plug-in names. This can be empty, but not null.</returns>
    public static string[] GetInstalledPlugInNames(PlugInType typeFilter, bool loaded, bool unloaded)
    {
      int count = InstalledPlugInCount;
      List<string> names = new List<string>(32);
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr_name = UnsafeNativeMethods.CRhinoPlugInManager_GetName(i);
        if (ptr_name != IntPtr.Zero)
        {
          if (UnsafeNativeMethods.CRhinoPlugInManager_PassesFilter(i, (int)typeFilter, loaded, unloaded, false))
          {
            string name = Marshal.PtrToStringUni(ptr_name);
            if (!string.IsNullOrEmpty(name))
              names.Add(name);

          }
        }
      }
      return names.ToArray();
    }

    public static string[] GetInstalledPlugInFolders()
    {
      List<string> dirs = new List<string>(32);
      for( int i=0; i<m_plugins.Count; i++ )
      {
        var dir = System.IO.Path.GetDirectoryName(m_plugins[i].Assembly.Location);
        if( !dirs.Contains(dir) )
          dirs.Add(dir);
      }

      int count = InstalledPlugInCount;
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr_filename = UnsafeNativeMethods.CRhinoPlugInManager_GetFileName(i);
        if (ptr_filename != IntPtr.Zero)
        {
          string path = Marshal.PtrToStringUni(ptr_filename);
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
    /// Gets a plug-in name for an installed plug-in given the path to that plug-in.
    /// </summary>
    /// <param name="pluginPath">The path of the plug-in.</param>
    /// <returns>The plug-in name.</returns>
    public static string NameFromPath(string pluginPath)
    {
      string rc;
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoPlugInManager_NameFromPath(pluginPath, ptr_string);
        rc = sh.ToString();
      }
      if (string.IsNullOrEmpty(rc))
      {
        // 2-Nov-2011 Dale Fugier
        // Look in our local collection of plug-ins. We may be in "OnLoad"
        // and the plug-in hasn't officially been registered with Rhino.
        for (int i = 0; i < m_plugins.Count; i++)
        {
          if (string.Compare(m_plugins[i].Assembly.Location, pluginPath, StringComparison.OrdinalIgnoreCase) == 0)
          {
            rc = m_plugins[i].Name;
            break;
          }
        }
      }
      return rc;
    }

    /// <summary>
    /// Gets the path to an installed plug-in given the name of that plug-in
    /// </summary>
    /// <param name="pluginName"></param>
    /// <returns></returns>
    public static string PathFromName(string pluginName)
    {
      Guid id = IdFromName(pluginName);
      return PathFromId(id);
    }

    /// <summary>
    /// Gets the path to an installed plug-in given the id of that plug-in
    /// </summary>
    /// <param name="pluginId"></param>
    /// <returns></returns>
    public static string PathFromId(Guid pluginId)
    {
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoPlugInManager_PathFromId(pluginId, ptr_string);
        return sh.ToString();
      }
    }

    public static Guid IdFromPath(string pluginPath)
    {
      Guid rc = UnsafeNativeMethods.CRhinoPlugInManager_IdFromPath(pluginPath);
      if (rc.Equals(Guid.Empty))
      {
        // 2-Nov-2011 Dale Fugier
        // Look in our local collection of plug-ins. We may be in "OnLoad"
        // and the plug-in hasn't officially been registered with Rhino.
        for (int i = 0; i < m_plugins.Count; i++)
        {
          if (string.Compare(m_plugins[i].Assembly.Location, pluginPath, true) == 0)
          {
            rc = m_plugins[i].Id;
            break;
          }
        }
      }
      return rc;
    }

    public static Guid IdFromName(string pluginName)
    {
      Guid rc = UnsafeNativeMethods.CRhinoPlugInManager_IdFromName(pluginName);
      if (rc.Equals(Guid.Empty))
      {
        // Look in our local collection of plug-ins. We may be in "OnLoad"
        // and the plug-in hasn't officially been registered with Rhino.
        for (int i = 0; i < m_plugins.Count; i++)
        {
          if (string.Compare(m_plugins[i].Name, pluginName, true) == 0)
          {
            rc = m_plugins[i].Id;
            break;
          }
        }
      }
      return rc;
    }

    public static bool LoadPlugIn(Guid pluginId)
    {
      return UnsafeNativeMethods.CRhinoPlugInManager_LoadPlugIn(pluginId);
    }

    /// <summary>
    /// Gets names of all "non-test" commands for a given plug-in.
    /// </summary>
    /// <param name="pluginId">The plug-in ID.</param>
    /// <returns>An array with all plug-in names. This can be empty, but not null.</returns>
    public static string[] GetEnglishCommandNames(Guid pluginId)
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoPluginManager_GetCommandNames(pluginId, pStrings);
      string[] rc = new string[count];
      using (var sh = new StringHolder())
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

    /// <summary>
    /// Set load protection state for a certain plug-in
    /// </summary>
    /// <param name="pluginId"></param>
    /// <param name="loadSilently"></param>
    public static void SetLoadProtection(Guid pluginId, bool loadSilently)
    {
      int state = loadSilently ? 1 : 2;
      UnsafeNativeMethods.CRhinoPluginRecord_SetLoadProtection(pluginId, state);
    }

    /// <summary>
    /// Get load protection state for a plug-in
    /// </summary>
    /// <param name="pluginId"></param>
    /// <param name="loadSilently"></param>
    /// <returns></returns>
    public static bool GetLoadProtection(Guid pluginId, out bool loadSilently)
    {
      loadSilently = true;
      int state = 0;
      bool rc = UnsafeNativeMethods.CRhinoPluginRecord_GetLoadProtection(pluginId, ref state);
      if (rc)
        loadSilently = (state == 0 || state == 1);
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
    private static readonly RenderFunc m_OnRender = InternalOnRender;
    private static readonly RenderWindowFunc m_OnRenderWindow = InternalOnRenderWindow;
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
          Rhino.Drawing.Rectangle rect = Rhino.Drawing.Rectangle.FromLTRB(rLeft, rTop, rRight, rBottom);
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

#if RDK_CHECKED
      UnsafeNativeMethods.CRhinoRenderPlugIn_SetRdkCallbacks(m_OnSupportsFeature, 
                                                             m_OnAbortRender, 
                                                             m_OnAllowChooseContent, 
                                                             m_OnCreateDefaultContent,
                                                             m_OnOutputTypes,
                                                             m_OnCreateTexturePreview,
                                                             m_OnCreatePreview,
                                                             m_OnDecalProperties,
                                                             g_register_content_io_callback);
#endif
    }

#if RDK_CHECKED
    internal delegate void RegisterContentIoCallback(int serialNumber, IntPtr ioList);
    private static readonly RegisterContentIoCallback g_register_content_io_callback = OnRegisterContentIo;
    private static void OnRegisterContentIo(int serialNumber, IntPtr ioList)
    {
      var render_plug_in = LookUpBySerialNumber(serialNumber) as RenderPlugIn;
      if (null == render_plug_in)
        return;
      var serializers = render_plug_in.RenderContentSerializers();
      if (null == serializers)
        return;
      foreach (var serializer in serializers)
      {
        if (null == serializer)
          continue;
        // Make sure a file extension is provided and that it was not previously registered.
        var extension = serializer.FileExtension;
        if (string.IsNullOrEmpty(extension) || UnsafeNativeMethods.Rdk_RenderContentIo_IsExtensionRegistered(extension))
          continue;
        // Create a C++ object and attach it to the serialize object
        serializer.Construct(render_plug_in.Id);
      }
    }

    /// <summary>
    /// Called by Rhino when it is time to register RenderContentSerializer
    /// derived classes.  Override this method and return an array of an
    /// instance of each serialize custom content object you wish to add.
    /// </summary>
    /// <returns>
    /// List of RenderContentSerializer objects to register with the Rhino
    /// render content browsers.
    /// </returns>
    protected virtual IEnumerable<RenderContentSerializer> RenderContentSerializers()
    {
      return null;
    }

    public enum RenderFeature : int
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
    /// Determines if your renderer supports a specific feature.
    /// </summary>
    /// <param name="feature">A feature to be controlled.</param>
    /// <returns>true if the renderer indeed supports the feature.</returns>
    protected virtual bool SupportsFeature(RenderFeature feature)
    {
      return true;
    }
    /// <summary>
    /// Creates the preview bitmap that will appear in the content editor's
    /// thumbnail display when previewing materials and environments. If this
    /// function is not overridden or the PreviewImage is not set on the
    /// args, then the internal OpenGL renderer will generate a simulation of
    /// the content.
    /// 
    /// This function is called with four different preview quality settings.
    /// The first quality level of RealtimeQuick is called on the main thread
    /// and needs to be drawn as fast as possible.  This function is called
    /// with the other three quality settings on a separate thread and are
    /// meant for generating progressively refined preview.
    /// </summary>
    /// <param name="args">Event argument with several preview option state properties.</param>
    protected virtual void CreatePreview(CreatePreviewEventArgs args) { }

    /// <summary>
    /// Creates the preview bitmap that will appear in the content editor's
    /// thumbnail display when previewing textures in 2d (UV) mode.
    ///
    /// If this function is not overridden or the PreviewImage is not set on the
    /// args, then the internal OpenGL renderer will generate a simulation.
    /// </summary>
    /// <param name="args">Event argument with several preview option state properties.</param>
    protected virtual void CreateTexture2dPreview(CreateTexture2dPreviewEventArgs args) { }

    /// <summary>
    /// Default implementation returns true which means the content can be
    /// picked from the content browser by the user. Override this method and
    /// return false if you don't want to allow a certain content contentType to be
    /// picked from the content browser while your render engine is current.
    /// </summary>
    /// <param name="content">A render context.</param>
    /// <returns>true if the operation was successful.</returns>
    protected virtual bool AllowChooseContent(Rhino.Render.RenderContent content) { return true; }

    /// <summary>
    /// Returns a list of output types which your renderer can write.
    /// <para>The default implementation returns bmp, jpg, png, tif, tga.</para>
    /// </summary>
    /// <returns>A list of file types.</returns>
    protected virtual List<Rhino.FileIO.FileType> SupportedOutputTypes()
    {
      using (var shExt = new StringHolder())
      using (var shDesc = new StringHolder())
      {
        var rc = new List<Rhino.FileIO.FileType>();
        int iIndex = 0;
        while (1 == UnsafeNativeMethods.Rdk_RenderPlugIn_BaseOutputTypeAtIndex(NonConstPointer(), iIndex++, shExt.NonConstPointer(), shDesc.NonConstPointer()))
        {
          rc.Add(new Rhino.FileIO.FileType(shExt.ToString(), shDesc.ToString()));
        }
        return rc;
      }
    }

    /* 17 Oct 2012 - S. Baer
     * Removed this virtual function until I understand what it is needed for.
     * It seems like you can register default content in the plug-in's OnLoad
     * virtual function and everything works fine
     * 
    // override this method to create extra default content for your renderer in
    // addition to any content in the default content folder.
    protected virtual void CreateDefaultContent(RhinoDoc doc)
    {
    }
    */


    /// <summary>
    /// Override this function to handle showing a modal dialog with your plugin's
    /// custom decal properties.  You will be passed the current properties for the 
    /// object being edited.  The defaults will be set in InitializeDecalProperties.
    /// </summary>
    /// <param name="properties">A list of named values that will be stored on the object
    /// the input values are the current ones, you should modify the values after the dialog
    /// closes.</param>
    /// <returns>true if the user pressed "OK", otherwise false.</returns>
    protected virtual bool ShowDecalProperties(ref List<NamedValue> properties)
    {
      return false;
    }

    /// <summary>
    /// Initialize your custom decal properties here.  The input list will be empty - add your
    /// default named property values and return.
    /// </summary>
    /// <param name="properties">A list of named values that will be stored on the object
    /// the input values are the current ones, you should modify the values after the dialog
    /// closes.</param>
    protected virtual void InitializeDecalProperties(ref List<NamedValue> properties)
    {
    }


    #region other virtual function implementation
    internal delegate int SupportsFeatureCallback(int serial_number, RenderFeature f);
    private static readonly SupportsFeatureCallback m_OnSupportsFeature = OnSupportsFeature;
    private static int OnSupportsFeature(int serial_number, RenderFeature f)
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
    private static readonly AbortRenderCallback m_OnAbortRender = OnAbortRender;
    private static void OnAbortRender(int plugin_serial_number)
    {
      RenderPlugIn p = LookUpBySerialNumber(plugin_serial_number) as RenderPlugIn;
      if (null == p)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnAbortRender");
      }
      else
      {
        try
        {
          var args = p.ActivePreviewArgs.ToArray();
          for (int i = 0; i < args.Length; i++)
            args[i].Cancel = true;
        }
        catch (Exception ex)
        {
          string error_msg = "Error occured during plug-in OnCreateScenePreviewAbort\n Details:\n";
          error_msg += ex.Message;
          HostUtils.DebugString("Error " + error_msg);
        }
      }
    }

    internal delegate int AllowChooseContentCallback(int serial_number, IntPtr pConstContent);
    private static readonly AllowChooseContentCallback m_OnAllowChooseContent = OnAllowChooseContent;
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
    private static readonly CreateDefaultContentCallback m_OnCreateDefaultContent = OnCreateDefaultContent;
    private static void OnCreateDefaultContent(int serial_number, int docId)
    {
      /* 17 Oct 2012 S. Baer
       * Removed virtual CreateDefaultContent for the time being. Don't
       * understand yet why this is needed
       * 
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
          HostUtils.ExceptionReport(ex);
        }
      }
      */
    }

    internal delegate void OutputTypesCallback(int serial_number, IntPtr pON_wStringExt, IntPtr pON_wStringDesc);
    private static readonly OutputTypesCallback m_OnOutputTypes = OnOutputTypes;
    private static void OnOutputTypes(int plugin_serial_number, IntPtr pON_wStringExt, IntPtr pON_wStringDesc)
    {
      RenderPlugIn p = LookUpBySerialNumber(plugin_serial_number) as RenderPlugIn;

      if (null == p || (IntPtr.Zero == pON_wStringDesc) || (IntPtr.Zero == pON_wStringExt))
      {
        HostUtils.DebugString("ERROR: Invalid input for OnOutputTypes");
        return;
      }

      try
      {
        var types = p.SupportedOutputTypes();

        System.Text.StringBuilder sbExt = new System.Text.StringBuilder();
        System.Text.StringBuilder sbDesc = new System.Text.StringBuilder();

        foreach (var type in types)
        {
          if (sbExt.Length != 0)
            sbExt.Append(";");

          sbExt.Append(type.Extension);

          if (sbDesc.Length != 0)
            sbDesc.Append(";");

          sbDesc.Append(type.Description);
        }

        UnsafeNativeMethods.ON_wString_Set(pON_wStringExt, sbExt.ToString());
        UnsafeNativeMethods.ON_wString_Set(pON_wStringDesc, sbDesc.ToString());
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport("OnOutputTypes", ex);
      }
    }

    internal delegate IntPtr CreateTexturePreviewCallback(int serial_number, int x, int y, IntPtr pTexture);
    private static readonly CreateTexturePreviewCallback m_OnCreateTexturePreview = OnCreateTexturePreview;
    private static IntPtr OnCreateTexturePreview(int serial_number, int x, int y, IntPtr pTexture)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      if (p == null || x < 1 || y < 1 || pTexture == IntPtr.Zero)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnCreateTexturePreview");
        return IntPtr.Zero;
      }

      var texture = RenderContent.FromPointer(pTexture) as RenderTexture;

      IntPtr pBitmap = IntPtr.Zero;
      CreateTexture2dPreviewEventArgs args = new CreateTexture2dPreviewEventArgs(texture, new Size(x, y));
      try
      {
        p.CreateTexture2dPreview(args);
        if (args.PreviewImage != null)
          pBitmap = args.PreviewImage.GetHbitmap();
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport("OnCreateTexturePreview", ex);
        pBitmap = IntPtr.Zero;
      }

      return pBitmap;
    }

    List<CreatePreviewEventArgs> m_active_preview_args;
    List<CreatePreviewEventArgs> ActivePreviewArgs { get { return m_active_preview_args ?? (m_active_preview_args = new List<CreatePreviewEventArgs>()); } }

    internal delegate IntPtr CreatePreviewCallback(int serial_number, int x, int y, int iQuality, IntPtr pScene);
    private static readonly CreatePreviewCallback m_OnCreatePreview = OnCreatePreview;
    private static IntPtr OnCreatePreview(int plugin_serial_number, int x, int y, int iQuality, IntPtr pPreviewScene)
    {
      RenderPlugIn p = LookUpBySerialNumber(plugin_serial_number) as RenderPlugIn;
      if (p == null || pPreviewScene == IntPtr.Zero || x < 1 || y < 1)
      {
        HostUtils.DebugString("ERROR: Invalid input for OnCreatePreview");
        return IntPtr.Zero;
      }

      var size = new Rhino.Drawing.Size(x, y);
      var args = new CreatePreviewEventArgs(pPreviewScene, size, (PreviewSceneQuality)iQuality);

      IntPtr pBitmap = IntPtr.Zero;
      try
      {
        p.ActivePreviewArgs.Add(args);
        p.CreatePreview(args);
        if (args.PreviewImage != null)
          pBitmap = args.PreviewImage.GetHbitmap();
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport("OnCreatePreview", ex);
        pBitmap = IntPtr.Zero;
      }
      finally
      {
        // 3 March 2014, John Morse
        // Fixed crash report: http://mcneel.myjetbrains.com/youtrack/issue/RH-24622
        if (p.ActivePreviewArgs.Contains(args))
          p.ActivePreviewArgs.Remove(args);
      }

      return pBitmap;
    }

    internal delegate int DecalCallback(int serial_number, IntPtr pXmlSection, int bInitialize);
    private static readonly DecalCallback m_OnDecalProperties = OnDecalProperties;
    private static int OnDecalProperties(int serial_number, IntPtr pXmlSection, int bInitialize)
    {
      RenderPlugIn p = LookUpBySerialNumber(serial_number) as RenderPlugIn;
      
      if (null == p || pXmlSection!=IntPtr.Zero)
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
          HostUtils.ExceptionReport("OnDecalProperties", ex);
        }
      }

      return 0;
    }
    #endregion

#endif


    /// <summary>
    /// Called by Render and RenderPreview commands if this plug-in is set as the default render engine. 
    /// </summary>
    /// <param name="doc">A document.</param>
    /// <param name="mode">A command running mode.</param>
    /// <param name="fastPreview">If true, lower quality faster render expected.</param>
    /// <returns>If true, then the renderer is reuired to construct a rapid preview and not the high-quality final result.</returns>
    protected abstract Rhino.Commands.Result Render(RhinoDoc doc, Rhino.Commands.RunMode mode, bool fastPreview);

    protected abstract Rhino.Commands.Result RenderWindow(RhinoDoc doc, Rhino.Commands.RunMode modes, bool fastPreview, Rhino.Display.RhinoView view, Rhino.Drawing.Rectangle rect, bool inWindow);
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
    /// If true, enable the digitizer. If false, disable the digitizer.
    /// </param>
    /// <returns>true if the operation succeeded; otherwise, false.</returns>
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
    /// <param name="point">3d point in digitizer coordinates.</param>
    /// <param name="mousebuttons">corresponding digitizer button is down.</param>
    /// <param name="shiftKey">true if the Shift keyboard key was pressed. Otherwise, false.</param>
    /// <param name="controlKey">true if the Control keyboard key was pressed. Otherwise, false.</param>
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
  /// Internal class used strictly to verify that the Zoo Client is being called
  /// from Rhino Common.
  /// </summary>
  class VerifyFromZooCommon { }
  /// <summary>
  /// License Manager Utilities.
  /// </summary>
  public static class LicenseUtils
  {
    private static System.Reflection.Assembly m_license_client_assembly;

    /// <summary>
    /// Returns the license client assembly.
    /// </summary>
    private static System.Reflection.Assembly GetLicenseClientAssembly()
    {
      try
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
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return null;
    }

    /// <summary>
    /// Initializes the license manager.
    /// </summary>
    public static bool Initialize()
    {
      LicenseManager.SetCallbacks();
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("Initialize");
        if (mi == null)
          return false;

        var args = new object[]{new VerifyFromZooCommon()};
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// Tests connectivity with the Zoo.
    /// </summary>
    public static string Echo(string message)
    {
      if (string.IsNullOrEmpty(message))
        return null;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return null;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return null;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("Echo");
        if (mi == null)
          return null;

        var args = new object[] { new VerifyFromZooCommon(), message };
        string invoke_rc = mi.Invoke(null, args) as String;
        return invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return null;
    }

    /// <summary>
    /// ShowLicenseValidationUi
    /// </summary>
    public static bool ShowLicenseValidationUi(string cdkey)
    {
      if (string.IsNullOrEmpty(cdkey))
        return false;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("ShowLicenseValidationUi");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), cdkey };
        object invoke_rc = mi.Invoke(null, args);
        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// This (internal) version of Rhino.PlugIns.LicenseUtils.GetLicense
    /// is used by Rhino.PlugIns.PlugIn objects.
    /// </summary>
    internal static bool GetLicense(string productPath, Guid productId, int productBuildType, string productTitle, ValidateProductKeyDelegate validateDelegate)
    {
      if (null == validateDelegate           ||
          string.IsNullOrEmpty(productPath)  || 
          string.IsNullOrEmpty(productTitle) || 
          productId.Equals(Guid.Empty)
        )
        return false;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicense");
        if (mi == null)
          return false;

        // 20-May-2013 Dale Fugier, use default capabilities
        LicenseCapabilities licenseCapabilities = LicenseCapabilities.NoCapabilities;
        // 29-May-2013 Dale Fugier, use no text mask
        string textMask = null;

        var args = new object[] { new VerifyFromZooCommon(), productPath, productId, productBuildType, productTitle, licenseCapabilities, textMask, validateDelegate };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }


    /// <summary>
    /// 20-May-2013 Dale Fugier
    /// This (internal) version of Rhino.PlugIns.LicenseUtils.GetLicense
    /// is used by Rhino.PlugIns.PlugIn objects.
    /// </summary>
    internal static bool GetLicense(string productPath, Guid productId, string productTitle, LicenseCapabilities licenseCapabilities, string textMask, ValidateProductKeyDelegate validateDelegate)
    {
      if (null == validateDelegate ||
          string.IsNullOrEmpty(productPath) ||
          string.IsNullOrEmpty(productTitle) ||
          productId.Equals(Guid.Empty)
        )
        return false;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicense");
        if (mi == null)
          return false;

        // 20-May-2013 Dale Fugier, 0 == any build
        int productBuildType = 0;

        var args = new object[] { new VerifyFromZooCommon(), productPath, productId, productBuildType, productTitle, licenseCapabilities, textMask, validateDelegate };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }


    /// <summary>
    /// This version of Rhino.PlugIns.LicenseUtils.GetLicense
    /// is used by Rhino C++ plug-ins.
    /// </summary>
    public static bool GetLicense(int productType, ValidateProductKeyDelegate validateDelegate)
    {
      if (null == validateDelegate)
        return false;

      // 26 Jan 2012 - S. Baer (RR 97943)
      // We were able to get this function to thrown exceptions with a bogus license file, but
      // don't quite understand exactly where the problem was occuring.  Adding a ExceptionReport
      // to this function in order to try and log the exception before Rhino goes down in a blaze
      // of glory
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicense");
        if (mi == null)
          return false;

        // If this delegate is defined in a C++ plug-in, find the plug-in's descriptive
        // information from the Rhino_DotNet wrapper class which is the delegate's target.

        System.Reflection.MethodInfo delegate_method = validateDelegate.Method;
        System.Reflection.Assembly rhCommon = typeof (HostUtils).Assembly;
        if (delegate_method.Module.Assembly != rhCommon)
          return false;

        object wrapper_class = validateDelegate.Target;
        if (null == wrapper_class)
          return false;

        Type wrapper_type = wrapper_class.GetType();
        System.Reflection.MethodInfo get_path_method = wrapper_type.GetRuntimeMethod("Path");
        System.Reflection.MethodInfo get_id_method = wrapper_type.GetRuntimeMethod("ProductId");
        System.Reflection.MethodInfo get_title_method = wrapper_type.GetRuntimeMethod("ProductTitle");
        string productPath = get_path_method.Invoke(wrapper_class, null) as string;
        string productTitle = get_title_method.Invoke(wrapper_class, null) as string;
        Guid productId = (Guid)get_id_method.Invoke(wrapper_class, null);

        // 20-May-2013 Dale Fugier, use default capabilities
        LicenseCapabilities licenseCapabilities = LicenseCapabilities.NoCapabilities;
        // 29-May-2013 Dale Fugier, use no text mask
        string textMask = null;

        var args = new object[] { new VerifyFromZooCommon(), productPath, productId, productType, productTitle, licenseCapabilities, textMask, validateDelegate };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }


    internal static bool AskUserForLicense(string productPath, bool standAlone, System.Windows.Forms.IWin32Window parentWindow, Guid productId, 
                                           int productBuildType, string productTitle, string textMask,
                                           ValidateProductKeyDelegate validateDelegate)
    {
      // 10-Jul-2013 Dale Fugier - don't test for validation function
      if (/*null == validateDelegate ||*/
          string.IsNullOrEmpty(productPath) ||
          string.IsNullOrEmpty(productTitle) ||
          productId.Equals(Guid.Empty)
        )
        return false;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("AskUserForLicense");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), productPath, standAlone, parentWindow, productId, productBuildType, productTitle, textMask, validateDelegate, null };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// This version of Rhino.PlugIns.LicenseUtils.GetLicense
    /// is used by Rhino C++ plug-ins.
    /// </summary>
    public static bool GetLicense(ValidateProductKeyDelegate validateDelegate, int capabilities, string textMask)
    {
      // 20-May-2013 Dale Fugier

      if (null == validateDelegate)
        return false;

      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicense");
        if (mi == null)
          return false;

        // If this delegate is defined in a C++ plug-in, find the plug-in's descriptive
        // information from the Rhino_DotNet wrapper class which is the delegate's target.

        System.Reflection.MethodInfo delegate_method = validateDelegate.Method;
        System.Reflection.Assembly rhCommon = typeof (HostUtils).Assembly;
        if (delegate_method.Module.Assembly != rhCommon)
          return false;

        object wrapper_class = validateDelegate.Target;
        if (null == wrapper_class)
          return false;

        Type wrapper_type = wrapper_class.GetType();
        System.Reflection.MethodInfo get_path_method = wrapper_type.GetRuntimeMethod("Path");
        System.Reflection.MethodInfo get_id_method = wrapper_type.GetRuntimeMethod("ProductId");
        System.Reflection.MethodInfo get_title_method = wrapper_type.GetRuntimeMethod("ProductTitle");
        string productPath = get_path_method.Invoke(wrapper_class, null) as string;
        string productTitle = get_title_method.Invoke(wrapper_class, null) as string;
        Guid productId = (Guid)get_id_method.Invoke(wrapper_class, null);

        // 20-May-2013 Dale Fugier, 0 == any build
        int productBuildType = 0;
        // Convert int to enum
        LicenseCapabilities licenseCapabilities = GetLicenseCapabilities(capabilities);

        var args = new object[] { new VerifyFromZooCommon(), productPath, productId, productBuildType, productTitle, licenseCapabilities, textMask, validateDelegate };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// This version of Rhino.PlugIns.LicenseUtils.AskUserForLicense
    /// is used by Rhino C++ plug-ins.
    /// </summary>
    public static bool AskUserForLicense(int productType, bool standAlone, System.Windows.Forms.IWin32Window parentWindow, string textMask, ValidateProductKeyDelegate validateDelegate)
    {
      if (null == validateDelegate)
        return false;

      // 26 Jan 2012 - S. Baer (RR 97943)
      // We were able to get this function to thrown exceptions with a bogus license file, but
      // don't quite understand exactly where the problem was occuring.  Adding a ExceptionReport
      // to this function in order to try and log the exception before Rhino goes down in a blaze
      // of glory
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("AskUserForLicense");
        if (mi == null)
          return false;

        // If this delegate is defined in a C++ plug-in, find the plug-in's descriptive
        // information from the Rhino_DotNet wrapper class which is the delegate's target.

        System.Reflection.MethodInfo delegate_method = validateDelegate.Method;
        System.Reflection.Assembly rhCommon = typeof(HostUtils).Assembly;
        if (delegate_method.Module.Assembly != rhCommon)
          return false;

        object wrapper_class = validateDelegate.Target;
        if (null == wrapper_class)
          return false;

        Type wrapper_type = wrapper_class.GetType();
        System.Reflection.MethodInfo get_path_method = wrapper_type.GetRuntimeMethod("Path");
        System.Reflection.MethodInfo get_id_method = wrapper_type.GetRuntimeMethod("ProductId");
        System.Reflection.MethodInfo get_title_method = wrapper_type.GetRuntimeMethod("ProductTitle");
        string productPath = get_path_method.Invoke(wrapper_class, null) as string;
        string productTitle = get_title_method.Invoke(wrapper_class, null) as string;
        Guid productId = (Guid)get_id_method.Invoke(wrapper_class, null);

        var args = new object[] { new VerifyFromZooCommon(), productPath, standAlone, parentWindow, productId, productType, productTitle, textMask, validateDelegate, null };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// This (internal) version of Rhino.PlugIns.LicenseUtils.ReturnLicense is used
    /// is used by Rhino.PlugIns.PlugIn objects.
    /// </summary>
    internal static bool ReturnLicense(string productPath, Guid productId, string productTitle)
    {
      if (string.IsNullOrEmpty(productPath)  ||
          string.IsNullOrEmpty(productTitle) ||
          productId.Equals(Guid.Empty)
        )
        return false;

      try
      {
        var method_info = GetReturnLicenseMethod();
        if (method_info == null) return false;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        var invoke_rc = method_info.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// This (internal) version of Rhino.PlugIns.LicenseUtils.ReturnLicense is used
    /// is used by Rhino C++ plug-ins.
    /// </summary>
    public static bool ReturnLicense(ValidateProductKeyDelegate validateDelegate)
    {
      if (null == validateDelegate)
        return false;

      try
      {
        var method_info = GetReturnLicenseMethod();
        if (method_info == null) return false;

        // If this delegate is defined in a C++ plug-in, find the plug-in's descriptive
        // information from the Rhino_DotNet wrapper class which is the delegate's target.

        var delegate_method = validateDelegate.Method;
        var rhino_dot_net_assembly = HostUtils.GetRhinoDotNetAssembly();
        if (delegate_method.Module.Assembly != rhino_dot_net_assembly)
          return false;

        var wrapper_class = validateDelegate.Target;
        if (null == wrapper_class)
          return false;

        var wrapper_type = wrapper_class.GetType();
        var get_id_method = wrapper_type.GetRuntimeMethod("ProductId");
        var product_id = (Guid)get_id_method.Invoke(wrapper_class, null);

        var args = new object[] { new VerifyFromZooCommon(), product_id };
        var invoke_rc = method_info.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// OBSOLETE - REMOVE WHEN POSSIBLE.
    /// </summary>
    public static bool ReturnLicense(Guid productId)
    {
      try
      {
        var method_info = GetReturnLicenseMethod();
        if (method_info == null) return false;
        var args = new object[] { new VerifyFromZooCommon(), productId };
        var invoke_rc = method_info.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    static System.Reflection.MethodInfo GetReturnLicenseMethod()
    {
      var assembly = GetLicenseClientAssembly();
      if (null == assembly) return null;
      var type = assembly.GetType("ZooClient.ZooClientUtilities", false);
      if (type == null) return null;
      var method_info = type.GetRuntimeMethod("ReturnLicense", new [] { typeof(string), typeof(Guid)});
      return method_info;
    }

    /// <summary>
    /// Checks out a license that is on loan from a Zoo server
    /// on a permanent basis.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check out.
    /// </param>
    /// <returns>
    /// true if the license was checked out successful.
    /// false if not successful or on error.
    /// </returns>
    public static bool CheckOutLicense(Guid productId)
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("CheckOutLicense");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// Checks in a previously checked out license to
    /// the Zoo server from which it was checked out.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check in.
    /// </param>
    /// <returns>
    /// true if the license was checked in successful.
    /// false if not successful or on error.
    /// </returns>
    public static bool CheckInLicense(Guid productId)
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("CheckInLicense");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// Converts a product license from a standalone node
    /// to a network node.
    /// </summary>
    /// <param name="productId">
    /// The Guid of the product that you want to check in.
    /// </param>
    /// <returns>
    /// true if the license was successfully converted.
    /// false if not successful or on error.
    /// </returns>
    public static bool ConvertLicense(Guid productId)
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("ConvertLicense");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// Returns the contentType of a specified product license
    /// </summary>
    public static int GetLicenseType(Guid productId)
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return -1;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return -1;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicenseType");
        if (mi == null)
          return -1;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return -1;

        return (int)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return -1;
    }

    /// <summary>
    /// Returns whether or not license checkout is enabled.
    /// </summary>
    public static bool IsCheckOutEnabled()
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("IsCheckOutEnabled");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon() };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    /// <summary>
    /// Returns the current status of every license for ui purposes.
    /// </summary>
    public static LicenseStatus[] GetLicenseStatus()
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return null;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return null;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetLicenseStatus");
        if (mi == null)
          return null;

        var args = new object[] { new VerifyFromZooCommon() };
        LicenseStatus[] invoke_rc = mi.Invoke(null, args) as LicenseStatus[];
        if (null == invoke_rc)
          return null;

        return invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return null;
    }

    /// <summary>
    /// Returns the current status of a license for ui purposes.
    /// </summary>
    public static LicenseStatus GetOneLicenseStatus(Guid productid)
    {
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return null;

        System.Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return null;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("GetOneLicenseStatus");
        if (mi == null)
          return null;

        var args = new object[] { new VerifyFromZooCommon(), productid };
        LicenseStatus invoke_rc = mi.Invoke(null, args) as LicenseStatus;
        if (null == invoke_rc)
          return null;

        return invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return null;
    }

    /// <summary>
    /// Converts an integer to a LicenseCapabilities flag
    /// </summary>
    public static LicenseCapabilities GetLicenseCapabilities(int filter)
    {
      LicenseCapabilities licenseCapabilities = LicenseCapabilities.NoCapabilities;
      if ((filter & (int)LicenseCapabilities.CanBePurchased) == (int)LicenseCapabilities.CanBePurchased)
        licenseCapabilities |= LicenseCapabilities.CanBePurchased;
      if ((filter & (int)LicenseCapabilities.CanBeSpecified) == (int)LicenseCapabilities.CanBeSpecified)
        licenseCapabilities |= LicenseCapabilities.CanBeSpecified;
      if ((filter & (int)LicenseCapabilities.CanBeEvaluated) == (int)LicenseCapabilities.CanBeEvaluated)
        licenseCapabilities |= LicenseCapabilities.CanBeEvaluated;
      if ((filter & (int)LicenseCapabilities.EvaluationIsExpired) == (int)LicenseCapabilities.EvaluationIsExpired)
        licenseCapabilities |= LicenseCapabilities.EvaluationIsExpired;
      return licenseCapabilities;
    }

    public static bool LicenseOptionsHandler(Guid productId, string productTitle, bool bStandalone)
    {
      // 11-Jul-2013 Dale Fugier
      try
      {
        System.Reflection.Assembly assembly = System.Reflection.Assembly.GetCallingAssembly();
        if (null == assembly)
          return false;

        var assemblyName = assembly.GetName().Name;
        if (!assemblyName.Equals("LicenseOptions", StringComparison.OrdinalIgnoreCase))
          return false;

        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return false;

        Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return false;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("LicenseOptionsHandler");
        if (mi == null)
          return false;

        var args = new object[] { new VerifyFromZooCommon(), productId, productTitle, bStandalone };
        object invoke_rc = mi.Invoke(null, args);
        if (null == invoke_rc)
          return false;

        return (bool)invoke_rc;
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }

      return false;
    }

    public static void ShowBuyLicenseUi(Guid productId)
    {
      // 11-Jul-2013 Dale Fugier
      try
      {
        System.Reflection.Assembly zooAss = GetLicenseClientAssembly();
        if (null == zooAss)
          return;

        Type t = zooAss.GetType("ZooClient.ZooClientUtilities", false);
        if (t == null)
          return;

        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("ShowBuyLicenseUi");
        if (mi == null)
          return;

        var args = new object[] { new VerifyFromZooCommon(), productId };
        mi.Invoke(null, args);
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }
  }

  /// <summary>ValidateProductKeyDelegate result code.</summary>
  public enum ValidateResult : int
  {
    /// <summary>The product key or license is validated successfully.</summary>
    Success = 1,
    /// <summary>
    /// There was an error validating the product key or license, the license
    /// manager show an error message.
    /// </summary>
    ErrorShowMessage = 0,
    /// <summary>
    /// There was an error validating the product key or license. The validating
    /// delegate will show an error message, not the license manager.
    /// </summary>
    ErrorHideMessage = -1
  }

  /// <summary>
  /// Validates a product key or license.
  /// </summary>
  public delegate ValidateResult ValidateProductKeyDelegate(string productKey, out LicenseData licenseData);

  /// <summary>License build contentType enumerations.</summary>
  public enum LicenseBuildType
  {
    /// <summary>A release build (e.g. commercial, education, nfr, etc.)</summary>
    Release = 100,
    /// <summary>A evaluation build</summary>
    Evaluation = 200,
    /// <summary>A beta build (e.g. wip)</summary>
    Beta = 300
  }

  /// <summary>
  /// Controls the buttons that will appear on the license notification window
  /// that is displayed if a license for the requesting product is not found.
  /// Note, the "Close" button will always be displayed.
  /// </summary>
  [Flags]
  public enum LicenseCapabilities
  {
    /// <summary>Only the "Close" button will be displayed</summary>
    NoCapabilities = 0x0,
    /// <summary>Shows "Buy a license" button</summary>
    CanBePurchased = 0x1,
    /// <summary>Shows ""Enter a license" and "Use a Zoo" buttons</summary>
    CanBeSpecified = 0x2,
    /// <summary>Shows "Evaluate" button</summary>
    CanBeEvaluated = 0x4,
    /// <summary>Shows "Evaluate" button disabled</summary>
    EvaluationIsExpired = 0x8
  }

  /// <summary>Zoo plugin license data.</summary>
  public class LicenseData
  {
    #region LicenseData data

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
    public string ProductLicense{ get; set; }

    /// <summary>
    /// The "for display only" product license.
    /// This is provided by the plugin that validated the license.
    /// </summary>
    public string SerialNumber{ get; set; }

    /// <summary>
    /// The title of the license.
    /// This is provided by the plugin that validated the license.
    /// (e.g. "Rhinoceros 5.0 Commercial")
    /// </summary>
    public string LicenseTitle{ get; set; }

    /// <summary>
    /// The build of the product that this license work with.
    /// When your product requests a license from the Zoo, it
    /// will specify one of these build types.
    /// </summary>
    public LicenseBuildType BuildType{ get; set; }

    /// <summary>
    /// The number of instances supported by this license.
    /// This is provided by the plugin that validated the license.
    /// </summary>
    public int LicenseCount{ get; set; }

    /// <summary>
    /// The date and time the license is set to expire.
    /// This is provided by the plugin that validated the license.
    /// This time value should be in Coordinated Universal Time (UTC).
    /// </summary>
    public DateTime? DateToExpire{ get; set;}

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
    /// Public constructor.
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
    /// Public constructor.
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
    /// Public constructor.
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
    /// Public constructor.
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
    /// Public constructor.
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
    /// Public constructor.
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
    /// Public validator.
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

  /// <summary>LicenseType enumeration.</summary>
  public enum LicenseType
  {
    /// <summary>A standalone license</summary>
    Standalone,
    /// <summary>A network license that has not been fulfilled by a Zoo</summary>
    Network,
    /// <summary>A license on temporary loan from a Zoo</summary>
    NetworkLoanedOut,
    /// <summary>A license on permanent check out from a Zoo</summary>
    NetworkCheckedOut
  }

  /// <summary>LicenseStatus class.</summary>
  public class LicenseStatus
  {
    #region LicenseStatus data

    /// <summary>The id of the product or plugin.</summary>
    public Guid ProductId{ get; set; }

    /// <summary>
    /// The build contentType of the product, where:
    ///   100 = A release build, either commercical, education, nfr, etc.
    ///   200 = A evaluation build
    ///   300 = A beta build, such as a wip.
    /// </summary>
    public LicenseBuildType BuildType{ get; set; }

    /// <summary>The title of the license. (e.g. "Rhinoceros 5.0 Commercial")</summary>
    public string LicenseTitle{ get; set; }

    /// <summary>The "for display only" product license or serial number.</summary>
    public string SerialNumber{ get; set; }

    /// <summary>The license contentType. (e.g. Standalone, Network, etc.)</summary>
    public LicenseType LicenseType{ get; set; }

    /// <summary>
    /// The date and time the license will expire.
    /// This value can be null if:
    ///   1.) The license contentType is "Standalone" and the license does not expire.
    ///   2.) The license contentType is "Network".
    ///   3.) The license contentType is "NetworkCheckedOut" and the checkout does not expire
    /// Note, date and time is in local time coordinates.
    /// </summary>
    public DateTime? ExpirationDate{ get; set; }

    /// <summary>
    /// The date and time the checked out license will expire.
    /// Note, this is only set if m_license_type = LicenceType.Standalone
    /// and if "limited license checkout" was enabled on the Zoo server.
    /// Note, date and time is in local time coordinates.
    /// </summary>
    public DateTime? CheckOutExpirationDate{ get; set; }

    /// <summary>The registered owner of the product. (e.g. "Dale Fugier")</summary>
    public string RegisteredOwner{ get; set; }

    /// <summary>The registered organization of the product (e.g. "Robert McNeel and Associates")</summary>
    public string RegisteredOrganization { get; set; }

    /// <summary>The product's icon. Note, this can be null.</summary>
    public Icon ProductIcon{ get; set; }

    #endregion

    /// <summary>Public constructor.</summary>
    public LicenseStatus()
    {
      ProductId = Guid.Empty;
      BuildType = 0;
      LicenseTitle = string.Empty;
      SerialNumber = string.Empty;
      LicenseType = PlugIns.LicenseType.Standalone;
      ExpirationDate = null;
      CheckOutExpirationDate = null;
      RegisteredOwner = string.Empty;
      RegisteredOrganization = string.Empty;
      ProductIcon = null;
    }
  }

}
#endif

namespace Rhino.FileIO
{
  public class FileType
  {
    public FileType(string extension, string description)
    {
      Description = description;
      Extension = extension;
    }
    public string Description { get; private set; }
    public string Extension { get; private set; }
  }
}