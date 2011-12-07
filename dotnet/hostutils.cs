#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
using Rhino.PlugIns;
#endif

namespace Rhino.Runtime
{
#if RHINO_SDK
  /// <summary>
  /// Skin DLLs must contain a single class that derives from the Skin class.
  /// </summary>
  public abstract class Skin
  {
    internal delegate void ShowSplashCallback(int mode, [MarshalAs(UnmanagedType.LPWStr)] string description);
    private static ShowSplashCallback m_ShowSplash;
    private static Skin m_theSingleSkin;

    /// <summary>
    /// Any time Rhino is running there is at most one skin being used (and
    /// possibly no skin).  If a RhinoCommon based Skin class is being used, use
    /// ActiveSkin to get at the instance of this Skin class. May return null
    /// if no Skin is being used or if the skin is not a RhinoCommon based skin.
    /// </summary>
    public static Skin ActiveSkin
    {
      get { return m_theSingleSkin; }
    }

    internal void OnShowSplash(int mode, string description)
    {
      const int HIDESPLASH = 0;
      const int SHOWSPLASH = 1;
      const int MAINFRAMECREATED = 1000;
      const int LICENSECHECKED = 2000;
      const int BUILTIN_COMMANDS_REGISTERED = 3000;
      const int BEGIN_LOAD_PLUGIN = 4000;
      const int END_LOAD_PLUGIN = 5000;
      const int BEGIN_LOAD_PLUGINS_BASE = 100000;
      try
      {
        if (m_theSingleSkin != null)
        {
          switch (mode)
          {
            case HIDESPLASH:
              m_theSingleSkin.HideSplash();
              break;
            case SHOWSPLASH:
              m_theSingleSkin.ShowSplash();
              break;
            case MAINFRAMECREATED:
              m_theSingleSkin.OnMainFrameWindowCreated();
              break;
            case LICENSECHECKED:
              m_theSingleSkin.OnLicenseCheckCompleted();
              break;
            case BUILTIN_COMMANDS_REGISTERED:
              m_theSingleSkin.OnBuiltInCommandsRegistered();
              break;
            case BEGIN_LOAD_PLUGIN:
              m_theSingleSkin.OnBeginLoadPlugIn(description);
              break;
            case END_LOAD_PLUGIN:
              m_theSingleSkin.OnEndLoadPlugIn();
              break;
          }
          if (mode >= BEGIN_LOAD_PLUGINS_BASE)
          {
            int count = (mode - BEGIN_LOAD_PLUGINS_BASE);
            m_theSingleSkin.OnBeginLoadAtStartPlugIns(count);
          }
        }
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.DebugString("Exception caught during Show/Hide Splash");
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    IntPtr m_pSkin;
    protected Skin()
    {
      if (m_theSingleSkin != null) return;
      // set callback if it hasn't already been set
      if (null == m_ShowSplash)
      {
        m_ShowSplash = OnShowSplash;
      }

      System.Drawing.Bitmap icon = MainRhinoIcon;
      string name = ApplicationName;

      IntPtr hicon = IntPtr.Zero;
      if (icon != null)
        hicon = icon.GetHicon();

      m_pSkin = UnsafeNativeMethods.CRhinoSkin_New(m_ShowSplash, name, hicon);
      m_theSingleSkin = this;
    }
    protected virtual void ShowSplash() { }
    protected virtual void HideSplash() { }

    protected virtual void OnMainFrameWindowCreated() { }
    protected virtual void OnLicenseCheckCompleted() { }
    protected virtual void OnBuiltInCommandsRegistered() { }
    protected virtual void OnBeginLoadAtStartPlugIns(int expectedCount) { }
    protected virtual void OnBeginLoadPlugIn(string description) { }
    protected virtual void OnEndLoadPlugIn() { }

    /// <summary>If you want to provide a custom icon for your skin</summary>
    protected virtual System.Drawing.Bitmap MainRhinoIcon
    {
      get { return null; }
    }

    /// <summary>If you want to provide a custom name for your skin</summary>
    protected virtual string ApplicationName
    {
      get { return string.Empty; }
    }

    PersistentSettingsManager m_SettingsManager;
    public PersistentSettings Settings
    {
      get
      {
        if (m_SettingsManager == null)
          m_SettingsManager = PersistentSettingsManager.Create(this);
        return m_SettingsManager.PluginSettings;
      }
    }

    static bool m_settings_written;
    internal static void WriteSettings()
    {
      if (!m_settings_written)
      {
        if (m_theSingleSkin != null && m_theSingleSkin.m_SettingsManager != null)
        {
          if (m_theSingleSkin.m_SettingsManager.m_plugin == null)
            m_theSingleSkin.m_SettingsManager.WriteSettings();
        }
      }
      m_settings_written = true;
    }
  }

  public abstract class PythonCompiledCode
  {
    public abstract void Execute(PythonScript scope);
  }

  public abstract class PythonScript
  {
    public static PythonScript Create()
    {
      Guid ip_id = new Guid("814d908a-e25c-493d-97e9-ee3861957f49");
      object obj = Rhino.RhinoApp.GetPlugInObject(ip_id);
      if (null == obj)
        return null;
      PythonScript pyscript = obj as PythonScript;
      return pyscript;
    }

    protected PythonScript()
    {
      m_output = RhinoApp.Write;
    }

    public abstract PythonCompiledCode Compile(string script);
    public abstract bool ContainsVariable(string name);
    public abstract System.Collections.Generic.IEnumerable<string> GetVariableNames();
    public abstract object GetVariable(string name);
    public abstract void SetVariable(string name, object value);
    public virtual void SetIntellisenseVariable(string name, object value) { }
    public abstract void RemoveVariable(string name);

    public abstract object EvaluateExpression(string statements, string expression);
    public abstract bool ExecuteFile(string path);
    public abstract bool ExecuteScript(string script);

    public abstract string GetStackTraceFromException(Exception ex);

    /// <summary>
    /// By default string output goes to the Rhino.RhinoApp.Write function
    /// Set Output if you want to redirect the output from python to a different function
    /// while this script executes.
    /// </summary>
    public Action<string> Output
    {
      get { return m_output; }
      set { m_output = value; }
    }
    Action<string> m_output;

    /// <summary>
    /// object set to variable held in scriptcontext.doc
    /// </summary>
    public object ScriptContextDoc
    {
      get { return m_scriptcontext_doc; }
      set { m_scriptcontext_doc = value; }
    }
    object m_scriptcontext_doc = null;

    public int ContextId
    {
      get { return m_context_id; }
      set { m_context_id = value; }
    }
    int m_context_id = 1;

    public abstract System.Windows.Forms.Control CreateTextEditorControl(string script, Action<string> helpcallback);
  }
#endif

  public static class HostUtils
  {
    /// <summary>
    /// DO NOT USE UNLESS YOU ABSOLUTELY KNOW WHAT YOU ARE DOING!!!
    /// Expert user function which should not be needed in most cases. This
    /// function is similar to a const_cast in C++ to allow an object to be
    /// made temporarily modifiable without causing RhinoCommon to convert
    /// the class from const to non-const by creating a duplicate.
    /// 
    /// You must call this function with a true parameter, make your
    /// modifications, and then restore the const flag by calling this function
    /// again with a false parameter. If you have any questions, please
    /// contact McNeel developer support before using!
    /// </summary>
    /// <param name="geometry"></param>
    /// <param name="makeNonConst"></param>
    public static void InPlaceConstCast(Rhino.Geometry.GeometryBase geometry, bool makeNonConst)
    {
      if (makeNonConst)
      {
        geometry.ApplyConstCast();
      }
      else
      {
        geometry.RemoveConstCast();
      }
    }

    /// <summary>
    /// Test if this process is currently executing on the Windows platform
    /// </summary>
    public static bool RunningOnWindows
    {
      get { return !RunningOnOSX; }
    }

    /// <summary>
    /// Test if this process is currently executing on the Mac OSX platform
    /// </summary>
    public static bool RunningOnOSX
    {
      get
      {
        System.PlatformID pid = System.Environment.OSVersion.Platform;
        // unfortunately Mono reports Unix when running on Mac
        return (System.PlatformID.MacOSX == pid || System.PlatformID.Unix == pid);
      }
    }

    /// <summary>
    /// Test if this process is currently executing under the Mono runtime
    /// </summary>
    public static bool RunningInMono
    {
      get { return Type.GetType("Mono.Runtime") != null; }
    }

    /// <summary>
    /// Test if RhinoCommon is currently executing inside of the Rhino.exe process.
    /// There are other cases where RhinoCommon could be running; specifically inside
    /// of Visual Studio when something like a windows form is being worked on in the
    /// resource editor or running stand-alone when compiled to be used as a version
    /// of OpenNURBS
    /// </summary>
    public static bool RunningInRhino
    {
      get
      {
        bool rc = false;
#if RHINO_SDK
        try
        {
          return Rhino.RhinoApp.SdkVersion>0;
        }
        catch (Exception)
        {
          rc = false;
        }
#endif
        return rc;
      }
    }

    // 0== unknown
    // 1== loaded
    //-1== not loaded
    static int m_rdk_loadtest = 0;
    public static bool CheckForRdk(bool throwOnFalse, bool usePreviousResult)
    {
      const int UNKNOWN = 0;
      const int LOADED = 1;
      const int NOT_LOADED = -1;

      if (UNKNOWN == m_rdk_loadtest || !usePreviousResult)
      {
        try
        {
          UnsafeNativeMethods.Rdk_LoadTest();
          m_rdk_loadtest = LOADED;
        }
        catch (Exception)
        {
          m_rdk_loadtest = NOT_LOADED;
        }
      }

      if (LOADED == m_rdk_loadtest)
        return true;

      if (throwOnFalse)
        throw new RdkNotLoadedException();

      return false;
    }

    static bool m_bSendDebugToRhino; // = false; initialized by runtime
    /// <summary>
    /// Print a debug message to the Rhino Command Line. 
    /// The message will only appear if the SendDebugToCommandLine property is set to True.
    /// </summary>
    /// <param name="msg">Message to print.</param>
    public static void DebugString(string msg)
    {
      if (m_bSendDebugToRhino)
        UnsafeNativeMethods.RHC_DebugPrint(msg);
    }
    /// <summary>
    /// Print a debug message to the Rhino Command Line. 
    /// The messae will only appear if the SendDebugToCommandLine property is set to True.
    /// </summary>
    /// <param name="format">Message to format and print.</param>
    /// <param name="args">An Object array containing zero or more objects to format.</param>
    public static void DebugString(string format, params object[] args)
    {
      string msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, format, args);
      DebugString(msg);
    }
    /// <summary>
    /// Gets or sets whether debug messages are printed to the command line.
    /// </summary>
    public static bool SendDebugToCommandLine
    {
      get { return m_bSendDebugToRhino; }
      set { m_bSendDebugToRhino = value; }
    }

    public static void ExceptionReport(Exception ex)
    {
      if (null == ex)
        return;
      string msg = ex.ToString();

      TypeLoadException tle = ex as TypeLoadException;
      if (tle != null)
      {
        string name = tle.TypeName;
        //if (!string.IsNullOrEmpty(name))
        msg = string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0}\nMissing Type = {1}", msg, name);
      }
      DebugString(msg);
    }

    static System.Windows.Forms.Form m_invoke_window = null;
    static void CreateInvokeWindow()
    {
      // 27 July 2011 - S. Baer
      // David: uncomment the following and test to see if this works for you. My tests appeared to work,
      //        but they really weren't thorough enough.
      /*
      if (m_invoke_window != null)
        return;

      m_invoke_window = new System.Windows.Forms.Form();
      m_invoke_window.Text = "RhinoCommon_Invoke";
      m_invoke_window.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
      m_invoke_window.ShowInTaskbar = false;
      m_invoke_window.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
      m_invoke_window.Location = new System.Drawing.Point(-1000, -1000);
      m_invoke_window.Size = new System.Drawing.Size(1, 1);
      m_invoke_window.Visible = false;
      m_invoke_window.Enabled = false;
      m_invoke_window.Show();
      m_invoke_window.Hide();
      */
    }

    public static object InvokeOnMainUiThread(Delegate method)
    {
      if (m_invoke_window != null && RunningOnWindows && m_invoke_window.InvokeRequired)
        return m_invoke_window.Invoke(method);

      return method.DynamicInvoke(null);
    }

    public static object InvokeOnMainUiThread(Delegate method, params object[] args)
    {
      if (m_invoke_window != null && RunningOnWindows && m_invoke_window.InvokeRequired)
        return m_invoke_window.Invoke(method, args);

      return method.DynamicInvoke(args);
    }

    /// <summary>
    /// Text description of the geometry's contents. DebugDump()
    /// is intended for debugging and is not suitable for
    /// creating high quality text descriptions of an object.
    /// </summary>
    /// <param name="geometry"></param>
    /// <returns></returns>
    public static string DebugDumpToString(Rhino.Geometry.GeometryBase geometry)
    {
      IntPtr pConstThis = geometry.ConstPointer();
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_Object_Dump(pConstThis, pString);
        return sh.ToString();
      }
    }

#if RHINO_SDK
    /// <summary>
    /// Parse a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="plugin">Plugin to harvest for commands.</param>
    public static void CreateCommands(PlugIn plugin)
    {
      if (plugin!=null)
        plugin.InternalCreateCommands();
    }
    /// <summary>
    /// Parse a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="pPlugIn">Plugin to harvest for commands.</param>
    /// <param name="pluginAssembly">Assembly associated with the plugin.</param>
    /// <returns>The number of newly created commands.</returns>
    public static int CreateCommands(IntPtr pPlugIn, System.Reflection.Assembly pluginAssembly)
    {
      int rc = 0;
      // This function must ONLY be called by Rhino_DotNet.Dll
      if (IntPtr.Zero == pPlugIn || null == pluginAssembly)
        return rc;

      Type[] exported_types = pluginAssembly.GetExportedTypes();
      if (null == exported_types)
        return rc;

      Type command_type = typeof(Commands.Command);
      for (int i = 0; i < exported_types.Length; i++)
      {
        if (exported_types[i].IsAbstract)
          continue;
        if (command_type.IsAssignableFrom(exported_types[i]))
        {
          if( Rhino.PlugIns.PlugIn.CreateCommandsHelper(null, pPlugIn, exported_types[i], null))
            rc++;
        }
      }

      return rc;
    }

    /// <summary>
    /// Adds a new dynamic command to Rhino.
    /// </summary>
    /// <param name="plugin">Plugin that owns the command.</param>
    /// <param name="cmd">Command to add.</param>
    /// <returns>True on success, false on failure.</returns>
    public static bool RegisterDynamicCommand(PlugIn plugin, Commands.Command cmd)
    {
      // every command must have a RhinoId and Name attribute
      bool rc = false;
      try
      {
        if (null != plugin)
          plugin.m_commands.Add(cmd);

        int sn = cmd.m_runtime_serial_number;
        IntPtr pPlugIn = plugin.NonConstPointer();
        string englishName = cmd.EnglishName;
        string localName = cmd.LocalName;
        const int commandStyle = 2; //scripted
        Guid id = cmd.Id;
        UnsafeNativeMethods.CRhinoCommand_Create(pPlugIn, id, englishName, localName, sn, commandStyle, 0);

        rc = true;
      }
      catch (Exception ex)
      {
        ExceptionReport(ex);
      }

      return rc;
    }
#endif
    static int GetNowHelper(int locale_id, IntPtr format, IntPtr pResultString)
    {
      int rc;
      try
      {
        string dateformat = Marshal.PtrToStringUni(format);
        // surround apostrophe with quotes in order to keep the formatter happy
        dateformat = dateformat.Replace("'", "\"'\"");
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(locale_id);
        DateTime now = System.DateTime.Now;
        string s = string.IsNullOrEmpty(dateformat) ? now.ToString(ci) : now.ToString(dateformat, ci);
        UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
      return rc;
    }

    static int GetFormattedTimeHelper(int locale_id, int sec, int min, int hour, int day, int month, int year, IntPtr format, IntPtr pResultString)
    {
      int rc;
      try
      {
        string dateformat = Marshal.PtrToStringUni(format);
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(locale_id);
        DateTime dt = new DateTime(year, month, day, hour, min, sec);
        string s = string.IsNullOrEmpty(dateformat) ? dt.ToString(ci) : dt.ToString(dateformat, ci);
        UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
      return rc;
    }

    static int EvaluateExpressionHelper(IntPtr statements, IntPtr expression, int rhinoDocId, IntPtr pResultString)
    {
      int rc = 0;
#if RHINO_SDK
      try
      {
        string expr = Marshal.PtrToStringUni(expression);
        string state = Marshal.PtrToStringUni(statements);
        PythonScript py = PythonScript.Create();
        object eval_result = py.EvaluateExpression(state, expr);
        if (null != eval_result)
        {
          string s = null;
          RhinoDoc doc = RhinoDoc.FromId(rhinoDocId);
          if (eval_result is double || eval_result is float)
          {
            if (doc != null)
            {
              int display_precision = doc.DistanceDisplayPrecision;
              string format = "{0:0.";
              format = format.PadRight(display_precision + format.Length, '0') + "}";
              s = string.Format(format, eval_result);
            }
            else
              s = eval_result.ToString();
          }
          else if (eval_result is string)
          {
            s = eval_result.ToString();
          }
          System.Collections.IEnumerable enumerable = eval_result as System.Collections.IEnumerable;
          if (string.IsNullOrEmpty(s) && enumerable != null)
          {
            string format = null;
            if (doc != null)
            {
              int display_precision = doc.DistanceDisplayPrecision;
              format = "{0:0.";
              format = format.PadRight(display_precision + format.Length, '0') + "}";
            }
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (object obj in enumerable)
            {
              if (sb.Length > 0)
                sb.Append(", ");
              if ( (obj is double || obj is float) && !string.IsNullOrEmpty(format) )
              {
                sb.AppendFormat(format, obj);
              }
              else
              {
                sb.Append(obj);
              }
            }
            s = sb.ToString();
          }
          if (string.IsNullOrEmpty(s))
            s = eval_result.ToString();
          UnsafeNativeMethods.ON_wString_Set(pResultString, s);
        }
        rc = 1;
      }
      catch (Exception ex)
      {
        UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
        rc = 0;
      }
#endif
      return rc;
    }
    internal delegate int EvaluateExpressionCallback(IntPtr statements, IntPtr expression, int rhinoDocId, IntPtr resultString);
    static EvaluateExpressionCallback m_evaluate_callback = EvaluateExpressionHelper;
    internal delegate int GetNowCallback(int locale_id, IntPtr format, IntPtr resultString);
    static GetNowCallback m_getnow_callback = GetNowHelper;
    internal delegate int GetFormattedTimeCallback(int locale, int sec, int min, int hour, int day, int month, int year, IntPtr format, IntPtr resultString);
    static GetFormattedTimeCallback m_getformattedtime_callback = GetFormattedTimeHelper;
    static HostUtils()
    {
      // These need to be moved somewhere else because they throw security exceptions
      // when defined in a static constructor
      /*
      UnsafeNativeMethods.RHC_SetPythonEvaluateCallback(m_evaluate_callback);
      UnsafeNativeMethods.RHC_SetGetNowProc(m_getnow_callback, m_getformattedtime_callback);
      */
    }

    private static bool m_rhinocommoninitialized = false;
    /// <summary>
    /// Makes sure all static RhinoCommon components is set up correctly. 
    /// This happens automatically when a plug-in is loaded, so you probably won't 
    /// have to call this method.
    /// </summary>
    /// <remarks>Subsequent calls to this method will be ignored.</remarks>
    public static void InitializeRhinoCommon()
    {
      if (m_rhinocommoninitialized)
        return;
      m_rhinocommoninitialized = true;

      AssemblyResolver.InitializeAssemblyResolving();
      {
        Type t = typeof(Rhino.DocObjects.Custom.UserDictionary);
        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID);
        Rhino.DocObjects.Custom.UserData.RegisterType(t);

        t = typeof(Rhino.DocObjects.Custom.SharedUserDictionary);
        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID);
        Rhino.DocObjects.Custom.UserData.RegisterType(t);
      }

      UnsafeNativeMethods.RHC_SetGetNowProc(m_getnow_callback, m_getformattedtime_callback);
      UnsafeNativeMethods.RHC_SetPythonEvaluateCallback(m_evaluate_callback);
      CreateInvokeWindow();
    }

#if RHINO_SDK
    public static PlugIn CreatePlugIn(Type pluginType, bool printDebugMessages)
    {
      if (null == pluginType || !typeof(PlugIn).IsAssignableFrom(pluginType))
        return null;

      InitializeRhinoCommon();

      // If we turn on debug messages, we always get debug output
      if (printDebugMessages)
        SendDebugToCommandLine = printDebugMessages;

      // this function should only be called by Rhino_DotNet.dll
      // we could add some safety checks by performing validation on
      // the calling assembly
      //System.Reflection.Assembly.GetCallingAssembly();

      System.Reflection.Assembly plugin_assembly = pluginType.Assembly;
      object[] name = plugin_assembly.GetCustomAttributes(typeof(System.Reflection.AssemblyTitleAttribute), false);
      string plugin_name = ((System.Reflection.AssemblyTitleAttribute)name[0]).Title;
      string plugin_version = plugin_assembly.GetName().Version.ToString();

      PlugIn plugin = PlugIn.Create(pluginType, plugin_name, plugin_version);

      if (plugin == null)
        return null;

      PlugIn.m_plugins.Add(plugin);
      return plugin;
    }
#endif

    static void DelegateReport(System.Delegate d, string name)
    {
      if (d == null) return;
      IFormatProvider fp = System.Globalization.CultureInfo.InvariantCulture;
      string title = string.Format(fp, "{0} Event\n", name);
      UnsafeNativeMethods.CRhinoEventWatcher_LogState(title);
      Delegate[] list = d.GetInvocationList();
      if (list != null && list.Length > 0)
      {
        for (int i = 0; i < list.Length; i++)
        {
          Delegate subD = list[i];
          Type t = subD.Target.GetType();
          string msg = string.Format(fp, "- Plug-In = {0}\n", t.Assembly.GetName().Name);
          UnsafeNativeMethods.CRhinoEventWatcher_LogState(msg);
        }
      }
    }

    internal delegate void ReportCallback(int c);
    internal static ReportCallback m_ew_report = EventWatcherReport;
    internal static void EventWatcherReport(int c)
    {
#if RHINO_SDK
      UnsafeNativeMethods.CRhinoEventWatcher_LogState("RhinoCommon delegate based event watcher\n");
      DelegateReport(RhinoApp.m_init_app, "InitApp");
      DelegateReport(RhinoApp.m_close_app, "CloseApp");
      DelegateReport(RhinoApp.m_appsettings_changed, "AppSettingsChanged");
      DelegateReport(Rhino.Commands.Command.m_begin_command, "BeginCommand");
      DelegateReport(Rhino.Commands.Command.m_end_command, "EndCommand");
      DelegateReport(Rhino.Commands.Command.m_undo_event, "Undo");
      DelegateReport(RhinoDoc.m_close_document, "CloseDocument");
      DelegateReport(RhinoDoc.m_new_document, "NewDocument");
      DelegateReport(RhinoDoc.m_document_properties_changed, "DocuemtnPropertiesChanged");
      DelegateReport(RhinoDoc.m_begin_open_document, "BeginOpenDocument");
      DelegateReport(RhinoDoc.m_end_open_document, "EndOpenDocument");
      DelegateReport(RhinoDoc.m_begin_save_document, "BeginSaveDocument");
      DelegateReport(RhinoDoc.m_end_save_document, "EndSaveDocument");
      DelegateReport(RhinoDoc.m_add_object, "AddObject");
      DelegateReport(RhinoDoc.m_delete_object, "DeleteObject");
      DelegateReport(RhinoDoc.m_replace_object, "ReplaceObject");
      DelegateReport(RhinoDoc.m_undelete_object, "UndeleteObject");
      DelegateReport(RhinoDoc.m_purge_object, "PurgeObject");
#endif
    }

#if RDK_UNCHECKED
    internal delegate void RdkReportCallback(int c);
    internal static RdkReportCallback m_rdk_ew_report = RdkEventWatcherReport;
    internal static void RdkEventWatcherReport(int c)
    {
      UnsafeNativeMethods.CRdkCmnEventWatcher_LogState("RhinoRdkCommon delegate based event watcher\n");
      DelegateReport(Rhino.Render.RenderContent.m_content_added_event, "RenderContentAdded");
    }
#endif

#if RHINO_SDK
    internal static object m_rhinoscript;
    internal static object GetRhinoScriptObject()
    {
      return m_rhinoscript ?? (m_rhinoscript = Rhino.RhinoApp.GetPlugInObject("RhinoScript"));
    }

    /// <summary>
    /// This function makes no sense on Mono
    /// </summary>
    /// <param name="display"></param>
    public static void DisplayOleAlerts(bool display)
    {
      UnsafeNativeMethods.RHC_DisplayOleAlerts(display);
    }
#endif

    internal static bool ContainsDelegate(MulticastDelegate source, Delegate d)
    {
      if (null != source && null != d)
      {
        Delegate[] list = source.GetInvocationList();
        if (null != list)
        {
          for (int i = 0; i < list.Length; i++)
          {
            if (list[i].Equals(d))
              return true;
          }
        }
      }
      return false;
    }

    /// <summary>
    /// Only works on Windows. Returns null on Mac
    /// </summary>
    /// <returns></returns>
    public static System.Reflection.Assembly GetRhinoDotNetAssembly()
    {
      if (m_rhdn_assembly == null && RunningOnWindows)
      {
        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
        for (int i = 0; i < assemblies.Length; i++)
        {
          if (assemblies[i].FullName.StartsWith("Rhino_DotNet", StringComparison.OrdinalIgnoreCase))
          {
            m_rhdn_assembly = assemblies[i];
            break;
          }
        }
      }
      return m_rhdn_assembly;
    }
    static System.Reflection.Assembly m_rhdn_assembly;

    public static void SetInShutDown()
    {
      try
      {
        UnsafeNativeMethods.RhCmn_SetInShutDown();
        // Remove callbacks that should not happen after this point in time
#if RDK_UNCHECKED
        Rhino.Render.RdkPlugIn.SetRdkCallbackFunctions(false);
#endif
      }
      catch
      {
        //throw away, we are shutting down
      }
    }

    internal static void WriteIntoSerializationInfo(IntPtr pRhCmnProfileContext, System.Runtime.Serialization.SerializationInfo info, string prefixStrip)
    {
      const int _string = 1;
      const int _multistring = 2;
      const int _uuid = 3;
      const int _color = 4;
      const int _int = 5;
      const int _double = 6;
      const int _rect = 7;
      const int _point = 8;
      const int _3dpoint = 9;
      const int _xform = 10;
      const int _3dvector = 11;
      const int _meshparams = 12;
      const int _buffer = 13;
      const int _bool = 14;
      int count = UnsafeNativeMethods.CRhCmnProfileContext_Count(pRhCmnProfileContext);
      using (StringHolder sectionholder = new StringHolder())
      using (StringHolder entryholder = new StringHolder())
      {
        IntPtr pStringSection = sectionholder.NonConstPointer();
        IntPtr pStringEntry = entryholder.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          int pctype = 0;
          UnsafeNativeMethods.CRhCmnProfileContext_Item(pRhCmnProfileContext, i, pStringSection, pStringEntry, ref pctype);
          string section = sectionholder.ToString();
          string entry = entryholder.ToString();
          if (string.IsNullOrEmpty(entry))
            continue;
          string name = string.IsNullOrEmpty(section) ? entry : section + "\\" + entry;
          if (name.StartsWith(prefixStrip + "\\"))
            name = name.Substring(prefixStrip.Length + 1);
          name = name.Replace("\\", "::");

          switch (pctype)
          {
            case _string:
              {
                UnsafeNativeMethods.CRhinoProfileContext_LoadString(pRhCmnProfileContext, section, entry, pStringEntry);
                string val = entryholder.ToString();
                info.AddValue(name, val);
              }
              break;
            case _multistring:
              {
                IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
                int array_count = UnsafeNativeMethods.CRhinoProfileContext_LoadStrings(pRhCmnProfileContext, section, entry, pStrings);
                string[] s = new string[array_count];
                for( int j=0; j<array_count; j++ )
                {
                  UnsafeNativeMethods.ON_StringArray_Get(pStrings, j, pStringEntry);
                  s[j] = entryholder.ToString();
                }
                info.AddValue(name, s);
              }
              break;
            case _uuid:
              {
                Guid id = Guid.Empty;
                UnsafeNativeMethods.CRhinoProfileContext_LoadGuid(pRhCmnProfileContext, section, entry, ref id);
                info.AddValue(name, id);
              }
              break;
            case _color:
              {
                int abgr = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadColor(pRhCmnProfileContext, section, entry, ref abgr);
                System.Drawing.Color c = System.Drawing.ColorTranslator.FromWin32(abgr);
                //string s = System.Drawing.ColorTranslator.ToHtml(c);
                info.AddValue(name, c);
              }
              break;
            case _int:
              {
                int ival = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadInt(pRhCmnProfileContext, section, entry, ref ival);
                info.AddValue(name, ival);
              }
              break;
            case _double:
              {
                double dval = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadDouble(pRhCmnProfileContext, section, entry, ref dval);
                info.AddValue(name, dval);
              }
              break;
            case _rect:
              {
                int left = 0, top = 0, right = 0, bottom = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadRect(pRhCmnProfileContext, section, entry, ref left, ref top, ref right, ref bottom);
                System.Drawing.Rectangle r = System.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
                info.AddValue(name, r);
              }
              break;
            case _point:
              {
                int x = 0, y = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadPoint(pRhCmnProfileContext, section, entry, ref x, ref y);
                System.Drawing.Point pt = new System.Drawing.Point(x, y);
                info.AddValue(name, pt);
              }
              break;
            case _3dpoint:
              {
                Rhino.Geometry.Point3d pt = new Geometry.Point3d();
                UnsafeNativeMethods.CRhinoProfileContext_LoadPoint3d(pRhCmnProfileContext, section, entry, ref pt);
                info.AddValue(name, pt);
              }
              break;
            case _xform:
              {
                Rhino.Geometry.Transform xf = new Geometry.Transform();
                UnsafeNativeMethods.CRhinoProfileContext_LoadXform(pRhCmnProfileContext, section, entry, ref xf);
                info.AddValue(name, xf);
              }
              break;
            case _3dvector:
              {
                Rhino.Geometry.Vector3d vec = new Geometry.Vector3d();
                UnsafeNativeMethods.CRhinoProfileContext_LoadVector3d(pRhCmnProfileContext, section, entry, ref vec);
                info.AddValue(name, vec);
              }
              break;
            case _meshparams:
              {
                Rhino.Geometry.MeshingParameters mp = new Geometry.MeshingParameters();
                UnsafeNativeMethods.CRhinoProfileContext_LoadMeshParameters(pRhCmnProfileContext, section, entry, mp.NonConstPointer());
                info.AddValue(name, mp);
                mp.Dispose();
              }
              break;
            case _buffer:
              {
                //not supported yet
                //int buffer_length = UnsafeNativeMethods.CRhinoProfileContext_BufferLength(pRhCmnProfileContext, section, entry);
                //byte[] buffer = new byte[buffer_length];
                //UnsafeNativeMethods.CRhinoProfileContext_LoadBuffer(pRhCmnProfileContext, section, entry, buffer_length, buffer);
                //info.AddValue(name, buffer);
              }
              break;
            case _bool:
              {
                bool b = false;
                UnsafeNativeMethods.CRhinoProfileContext_LoadBool(pRhCmnProfileContext, section, entry, ref b);
                info.AddValue(name, b);
              }
              break;
          }
        }
      }
    }

    internal static IntPtr ReadIntoProfileContext(System.Runtime.Serialization.SerializationInfo info, string sectionBase)
    {
      IntPtr pProfileContext = UnsafeNativeMethods.CRhCmnProfileContext_New();
      var e = info.GetEnumerator();
      while (e.MoveNext())
      {
        string entry = e.Name.Replace("::", "\\");
        string section = sectionBase;
        int split_index = entry.LastIndexOf("\\");
        if (split_index > -1)
        {
          section = sectionBase + "\\" + entry.Substring(0, split_index);
          entry = entry.Substring(split_index + 1);
        }

        
        Type t = e.ObjectType;
        if( typeof(string) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileString(pProfileContext, section, entry, e.Value as string);
        else if( typeof(Guid) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileUuid(pProfileContext, section, entry, (Guid)e.Value);
        else if( typeof(System.Drawing.Color) == t )
        {
          System.Drawing.Color c = (System.Drawing.Color)e.Value;
          int argb = c.ToArgb();
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileColor(pProfileContext, section, entry, argb);
        }
        else if( typeof(int) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileInt(pProfileContext, section, entry, (int)e.Value);
        else if( typeof(double) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileDouble(pProfileContext, section, entry, (double)e.Value);
        else if( typeof(System.Drawing.Rectangle) == t )
        {
          System.Drawing.Rectangle r = (System.Drawing.Rectangle)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileRect(pProfileContext, section, entry, r.Left, r.Top, r.Right, r.Bottom);
        }
        else if( typeof(System.Drawing.Point) == t )
        {
          System.Drawing.Point pt = (System.Drawing.Point)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfilePoint(pProfileContext, section, entry, pt.X, pt.Y);
        }
        else if( typeof(Rhino.Geometry.Point3d) == t )
        {
          Rhino.Geometry.Point3d pt = (Rhino.Geometry.Point3d)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfilePoint3d(pProfileContext, section, entry, pt);
        }
        else if( typeof(Rhino.Geometry.Transform) == t )
        {
          Rhino.Geometry.Transform xf = (Rhino.Geometry.Transform)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileXform(pProfileContext, section, entry, ref xf);
        }
        else if( typeof(Rhino.Geometry.Vector3d) == t )
        {
          Rhino.Geometry.Vector3d v = (Rhino.Geometry.Vector3d)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileVector3d(pProfileContext, section, entry, v);
        }
        else if( typeof(Rhino.Geometry.MeshingParameters) == t )
        {
          Rhino.Geometry.MeshingParameters mp = e.Value as Rhino.Geometry.MeshingParameters;
          IntPtr pMp = mp.ConstPointer();
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileMeshingParameters(pProfileContext, section, entry, pMp);
        }
        else if( typeof(byte[]) == t )
        {
          byte[] b = e.Value as byte[];
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileBuffer(pProfileContext, section, entry, b.Length, b);
        }
        else if (typeof(bool) == t)
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileBool(pProfileContext, section, entry, (bool)e.Value);
        else
        {
          //try
          //{
            string s = info.GetString(e.Name);
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileString(pProfileContext, section, entry, s);
          //}
          //catch (Exception ex)
          //{
          //  throw;
          //}
        }
      }
      return pProfileContext;
    }
  }

  [Serializable]
  public class RdkNotLoadedException : Exception
  {
  }
}