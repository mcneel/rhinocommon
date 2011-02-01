using System;
using System.Runtime.InteropServices;
using Rhino.PlugIns;

namespace Rhino.Runtime
{
#if !BUILDING_MONO
  /// <summary>
  /// Skin DLLs must contain a single class that derives from the Skin class.
  /// </summary>
  public abstract class Skin
  {
    internal delegate void ShowSplashCallback(int mode);
    private static ShowSplashCallback m_ShowSplash;
    private static Skin m_theSingleSkin;

    internal void OnShowSplash(int mode)
    {
      try
      {
        if (m_theSingleSkin != null)
        {
          switch (mode)
          {
            case 0:
              m_theSingleSkin.HideSplash();
              break;
            case 1:
              m_theSingleSkin.ShowSplash();
              break;
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
      m_pSkin = UnsafeNativeMethods.CRhinoSkin_New(m_ShowSplash);
      m_theSingleSkin = this;
    }
    protected virtual void ShowSplash() { }
    protected virtual void HideSplash() { }
  }
#endif

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
    public ScriptOutput Output
    {
      get { return m_output; }
      set { m_output = value; }
    }
    ScriptOutput m_output;

    /// <summary>
    /// object set to variable held in scriptcontext.doc
    /// </summary>
    public object ScriptContextDoc
    {
      get { return m_scriptcontext_doc; }
      set { m_scriptcontext_doc = value; }
    }
    object m_scriptcontext_doc = null;
  }

  public delegate void ScriptOutput(string s);

  public static class HostUtils
  {
    public static bool RunningOnWindows
    {
      get
      {
        return !RunningOnOSX;
      }
    }
    public static bool RunningOnOSX
    {
      get
      {
        System.PlatformID pid = System.Environment.OSVersion.Platform;
        // unfortunately Mono reports Unix when running on Mac
        return (System.PlatformID.MacOSX == pid || System.PlatformID.Unix == pid);
      }
    }

    static bool m_bSendDebugToRhino; // = false; initialized by runtime
    /// <summary>
    /// Print a debug message to the Rhino Command Line. 
    /// The messae will only appear if the SendDebugToCommandLine property is set to True.
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

    /// <summary>
    /// Parse a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="plugin">Plugin to harvest for commands.</param>
    /// <returns>The number of newly created commands.</returns>
    public static int CreateCommands(PlugIn plugin)
    {
      int rc = 0;
      if (null == plugin)
        return rc;

      Type[] exported_types = plugin.Assembly.GetExportedTypes();
      if (null == exported_types)
        return rc;

      Type command_type = typeof(Commands.Command);
      for (int i = 0; i < exported_types.Length; i++)
      {
        if (command_type.IsAssignableFrom(exported_types[i]))
        {
          if (CreateCommandsHelper(plugin, plugin.NonConstPointer(), exported_types[i]))
            rc++;
        }
      }

      // 26 Feb 2010 S. Baer
      // Moved Dynamic Command functionality into RhinoCommon from Rhino.NET in order to
      // make this work on both Windows and Mac
      try
      {
        // See if the plug-in has a function with the "magic" name of BuildDynamicCommands
        System.Reflection.MethodInfo mi = plugin.GetType().GetMethod("BuildDynamicCommands");
        if (null != mi)
        {
          mi.Invoke(plugin, null);
        }
      }
      catch (Exception)
      {
      }

      return rc;
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
          if (CreateCommandsHelper(null, pPlugIn, exported_types[i]))
            rc++;
        }
      }

      return rc;
    }

    static bool CreateCommandsHelper(PlugIn plugin, IntPtr pPlugIn, Type command_type)
    {
      bool rc = false;
      try
      {
        Commands.Command new_command = (Commands.Command)System.Activator.CreateInstance(command_type);
        new_command.m_plugin = plugin;

        if (null != plugin)
          plugin.m_commands.Add(new_command);

        int commandStyle = 0;
        object[] styleattr = command_type.GetCustomAttributes(typeof(Commands.CommandStyleAttribute), false);
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
        UnsafeNativeMethods.CRhinoCommand_Create(pPlugIn, id, englishName, localName, sn, commandStyle);

        rc = true;
      }
      catch (Exception ex)
      {
        ExceptionReport(ex);
      }

      return rc;
    }

    /// <summary>
    /// Add a new dynamic command to Rhino.
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
        int commandStyle = 2; //scripted
        Guid id = cmd.Id;
        UnsafeNativeMethods.CRhinoCommand_Create(pPlugIn, id, englishName, localName, sn, commandStyle);

        rc = true;
      }
      catch (Exception ex)
      {
        ExceptionReport(ex);
      }

      return rc;
    }

    static int GetNowHelper(int locale_id, IntPtr format, IntPtr pResultString)
    {
      int rc = 0;
      try
      {
        string dateformat = Marshal.PtrToStringUni(format);
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(locale_id);
        DateTime now = System.DateTime.Now;
        string s = null;
        if (string.IsNullOrEmpty(dateformat))
          s = now.ToString(ci);
        else
          s = now.ToString(dateformat, ci);
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
      int rc = 0;
      try
      {
        string dateformat = Marshal.PtrToStringUni(format);
        System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(locale_id);
        DateTime dt = new DateTime(year, month, day, hour, min, sec);
        string s = null;
        if (string.IsNullOrEmpty(dateformat))
          s = dt.ToString(ci);
        else
          s = dt.ToString(dateformat, ci);
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
            int display_precision = doc.DistanceDisplayPrecision;
            string format = "{0:0.";
            format = format.PadRight(display_precision + format.Length, '0') + "}";
            s = string.Format(format, eval_result);
          }
          else if (eval_result is string)
          {
            s = eval_result.ToString();
          }
          System.Collections.IEnumerable enumerable = eval_result as System.Collections.IEnumerable;
          if (string.IsNullOrEmpty(s) && enumerable != null)
          {
            int display_precision = doc.DistanceDisplayPrecision;
            string format = "{0:0.";
            format = format.PadRight(display_precision + format.Length, '0') + "}";
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            foreach (object obj in enumerable)
            {
              if (sb.Length > 0)
                sb.Append(", ");
              if (obj is double || obj is float)
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
      if (m_rhinocommoninitialized) { return; }

      AssemblyResolver.InitializeAssemblyResolving();

      m_rhinocommoninitialized = true;
    }

    public static PlugIn CreatePlugIn(Type pluginType, bool printDebugMessages)
    {
      if (null == pluginType || !typeof(PlugIn).IsAssignableFrom(pluginType))
        return null;

      AssemblyResolver.InitializeAssemblyResolving();

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

    static void DelegateReport(System.Delegate d, string name)
    {
      if (d != null)
      {
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
    }

    internal delegate void ReportCallback(int c);
    internal static ReportCallback m_ew_report = EventWatcherReport;
    internal static void EventWatcherReport(int c)
    {
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
    }

    internal static object m_rhinoscript;
    internal static object GetRhinoScriptObject()
    {
      if (m_rhinoscript == null)
        m_rhinoscript = Rhino.RhinoApp.GetPlugInObject("RhinoScript");
      return m_rhinoscript;
    }

    /// <summary>
    /// This function makes no sense on Mono
    /// </summary>
    /// <param name="display"></param>
    public static void DisplayOleAlerts(bool display)
    {
      UnsafeNativeMethods.RHC_DisplayOleAlerts(display);
    }

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

    public static void SetInShutDown()
    {
      UnsafeNativeMethods.RhCmn_SetInShutDown();
    }
  }
}