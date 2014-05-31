using System;
using System.Runtime.InteropServices;
using Rhino.Runtime.InteropWrappers;

#if RHINO_SDK
using Rhino.PlugIns;

#endif

namespace Rhino.Runtime
{
#if RHINO_SDK
  /// <summary>
  /// Represents a customized environment that changes the appearance of Rhino.
  /// <para>Skin DLLs must contain a single class that derives from the Skin class.</para>
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
      const int SHOWHELP = 2;
      const int MAINFRAMECREATED = 1000;
      const int LICENSECHECKED = 2000;
      const int BUILTIN_COMMANDS_REGISTERED = 3000;
      const int BEGIN_LOAD_PLUGIN = 4000;
      const int END_LOAD_PLUGIN = 5000;
      const int END_LOAD_AT_START_PLUGINS = 6000;
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
            case SHOWHELP:
              m_theSingleSkin.ShowHelp();
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
            case END_LOAD_AT_START_PLUGINS:
              m_theSingleSkin.OnEndLoadAtStartPlugIns();
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

    /// <summary>
    /// Initializes a new instance of the <see cref="Skin"/> class.
    /// </summary>
    protected Skin()
    {
      if (m_theSingleSkin != null) return;
      // set callback if it hasn't already been set
      if (null == m_ShowSplash)
      {
        m_ShowSplash = OnShowSplash;
      }

      Rhino.Drawing.Bitmap icon = MainRhinoIcon;
      string name = ApplicationName;

      IntPtr hicon = IntPtr.Zero;
      if (icon != null)
        hicon = icon.GetHicon();

      m_pSkin = UnsafeNativeMethods.CRhinoSkin_New(m_ShowSplash, name, hicon);
      m_theSingleSkin = this;
    }
    /// <summary>Is called when the splash screen should be shown.</summary>
    protected virtual void ShowSplash() { }

    /// <summary>
    /// Called when the "help" splash screen should be shown. Default
    /// implementation just calls ShowSplash()
    /// </summary>
    protected virtual void ShowHelp() { ShowSplash(); }

    /// <summary>Is called when the splash screen should be hidden.</summary>
    protected virtual void HideSplash() { }

    /// <summary>Is called when the main frame window is created.</summary>
    protected virtual void OnMainFrameWindowCreated() { }

    /// <summary>Is called when the license check is completed.</summary>
    protected virtual void OnLicenseCheckCompleted() { }

    /// <summary>Is called when built-in commands are registered.</summary>
    protected virtual void OnBuiltInCommandsRegistered() { }

    /// <summary>Is called when the first plug-in that loads at start-up is going to be loaded.</summary>
    /// <param name="expectedCount">The complete amount of plug-ins.</param>
    protected virtual void OnBeginLoadAtStartPlugIns(int expectedCount) { }

    /// <summary>Is called when a specific plug-in is going to be loaded.</summary>
    /// <param name="description">The plug-in description.</param>
    protected virtual void OnBeginLoadPlugIn(string description) { }

    /// <summary>Is called after each plug-in has been loaded.</summary>
    protected virtual void OnEndLoadPlugIn() { }

    /// <summary>Is called after all of the load at start plug-ins have been loaded.</summary>
    protected virtual void OnEndLoadAtStartPlugIns() { }

    /// <summary>If you want to provide a custom icon for your skin.</summary>
    protected virtual Rhino.Drawing.Bitmap MainRhinoIcon
    {
      get { return null; }
    }

    /// <summary>If you want to provide a custom name for your skin.</summary>
    protected virtual string ApplicationName
    {
      get { return string.Empty; }
    }

    PersistentSettingsManager m_SettingsManager;

    /// <summary>
    /// Gets access to the skin persistent settings.
    /// </summary>
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
          if (m_theSingleSkin.m_SettingsManager.m_plugin_id == Guid.Empty)
            m_theSingleSkin.m_SettingsManager.WriteSettings();
        }
      }
      m_settings_written = true;
    }
  }

  /// <summary>
  /// Represents scripting compiled code.
  /// </summary>
  public abstract class PythonCompiledCode
  {
    /// <summary>
    /// Executes the script in a specific scope.
    /// </summary>
    /// <param name="scope">The scope where the script should be executed.</param>
    public abstract void Execute(PythonScript scope);
  }

  /// <summary>
  /// Represents a Python script.
  /// </summary>
  public abstract class PythonScript
  {
    /// <summary>
    /// Constructs a new Python script context.
    /// </summary>
    /// <returns>A new Python script, or null if none could be created. Rhino 4 always returns null.</returns>
    public static PythonScript Create()
    {
      Guid ip_id = new Guid("814d908a-e25c-493d-97e9-ee3861957f49");
      object obj = Rhino.RhinoApp.GetPlugInObject(ip_id);
      if (null == obj)
        return null;
      PythonScript pyscript = obj as PythonScript;
      return pyscript;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PythonScript"/> class.
    /// </summary>
    protected PythonScript()
    {
      ScriptContextDoc = null;
      Output = RhinoApp.Write;
    }

    /// <summary>
    /// Compiles a class in a quick-to-execute proxy.
    /// </summary>
    /// <param name="script">A string text.</param>
    /// <returns>A Python compiled code instance.</returns>
    public abstract PythonCompiledCode Compile(string script);

    /// <summary>
    /// Determines if the main scripting context has a variable with a name.
    /// </summary>
    /// <param name="name">The variable name.</param>
    /// <returns>true if the variable is present.</returns>
    public abstract bool ContainsVariable(string name);

    /// <summary>
    /// Retrieves all variable names in the script.
    /// </summary>
    /// <returns>An enumerable set with all names of the variables.</returns>
    public abstract System.Collections.Generic.IEnumerable<string> GetVariableNames();

    /// <summary>
    /// Gets the object associated with a variable name in the main scripting context.
    /// </summary>
    /// <param name="name">A variable name.</param>
    /// <returns>The variable object.</returns>
    public abstract object GetVariable(string name);

    /// <summary>
    /// Sets a variable with a name and an object. Object can be null (Nothing in Visual Basic).
    /// </summary>
    /// <param name="name">A valid variable name in Python.</param>
    /// <param name="value">A valid value for that variable name.</param>
    public abstract void SetVariable(string name, object value);

    /// <summary>
    /// Sets a variable for runtime introspection.
    /// </summary>
    /// <param name="name">A variable name.</param>
    /// <param name="value">A variable value.</param>
    public virtual void SetIntellisenseVariable(string name, object value) { }

    /// <summary>
    /// Removes a defined variable from the main scripting context.
    /// </summary>
    /// <param name="name">The variable name.</param>
    public abstract void RemoveVariable(string name);

    /// <summary>
    /// Evaluates statements and an expression in the main scripting context.
    /// </summary>
    /// <param name="statements">One or several statements.</param>
    /// <param name="expression">An expression.</param>
    /// <returns>The expression result.</returns>
    public abstract object EvaluateExpression(string statements, string expression);

    /// <summary>
    /// Executes a Python file.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>true if the file executed. This method can throw scripting-runtime based exceptions.</returns>
    public abstract bool ExecuteFile(string path);

    /// <summary>
    /// Executes a Python string.
    /// </summary>
    /// <param name="script">A Python text.</param>
    /// <returns>true if the file executed. This method can throw scripting-runtime based exceptions.</returns>
    public abstract bool ExecuteScript(string script);

    /// <summary>
    /// Retrieves a meaningful representation of the call stack.
    /// </summary>
    /// <param name="ex">An exception that was thrown by some of the methods in this class.</param>
    /// <returns>A string that represents the Python exception.</returns>
    public abstract string GetStackTraceFromException(Exception ex);

    /// <summary>
    /// Gets or sets the Python script "print()" target.
    /// <para>By default string output goes to the Rhino.RhinoApp.Write function.
    /// Set Output if you want to redirect the output from python to a different function
    /// while this script executes.</para>
    /// </summary>
    public Action<string> Output { get; set; }

    /// <summary>
    /// object set to variable held in scriptcontext.doc.
    /// </summary>
    public object ScriptContextDoc { get; set; }

    /// <summary>
    /// Gets or sets a context unique identified.
    /// </summary>
    public int ContextId
    {
      get { return m_context_id; }
      set { m_context_id = value; }
    }
    int m_context_id = 1;

    /// <summary>
    /// Creates a control where the user is able to type Python code.
    /// </summary>
    /// <param name="script">A starting script.</param>
    /// <param name="helpcallback">A method that is called when help is shown for a function, a class or a method.</param>
    /// <returns>A Windows Forms control.</returns>
    public abstract System.Windows.Forms.Control CreateTextEditorControl(string script, Action<string> helpcallback);
  }
#endif

  /// <summary>
  /// Contains static methods to deal with teh runtime environment.
  /// </summary>
  public static class HostUtils
  {
    /// <summary>
    /// Returns list of directory names where additional assemblies (plug-ins, DLLs, Grasshopper components)
    /// may be located
    /// </summary>
    /// <returns></returns>
    public static string[] GetAssemblySearchPaths()
    {
#if RHINO_SDK
      return PlugIn.GetInstalledPlugInFolders();
#else
      return new string[0];
#endif
    }

    /// <summary>
    /// DO NOT USE UNLESS YOU ARE CERTAIN ABOUT THE IMPLICATIONS.
    /// <para>This is an expert user function which should not be needed in most
    /// cases. This function is similar to a const_cast in C++ to allow an object
    /// to be made temporarily modifiable without causing RhinoCommon to convert
    /// the class from const to non-const by creating a duplicate.</para>
    /// 
    /// <para>You must call this function with a true parameter, make your
    /// modifications, and then restore the const flag by calling this function
    /// again with a false parameter. If you have any questions, please
    /// contact McNeel developer support before using!</para>
    /// </summary>
    /// <param name="geometry">Some geometry.</param>
    /// <param name="makeNonConst">A boolean value.</param>
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

    ///// <summary>
    ///// Tests if this process is currently executing on the Windows platform.
    ///// </summary>
    //public static bool RunningOnWindows
    //{
    //  get { return !RunningOnOSX; }
    //}

    ///// <summary>
    ///// Tests if this process is currently executing on the Mac OSX platform.
    ///// </summary>
    //public static bool RunningOnOSX
    //{
    //  get
    //  {
    //    System.PlatformID pid = System.Environment.OSVersion.Platform;
    //    // unfortunately Mono reports Unix when running on Mac
    //    return (System.PlatformID.MacOSX == pid || System.PlatformID.Unix == pid);
    //  }
    //}

    ///// <summary>
    ///// Tests if this process is currently executing under the Mono runtime.
    ///// </summary>
    //public static bool RunningInMono
    //{
    //  get { return Type.GetType("Mono.Runtime") != null; }
    //}

    static int m_running_in_rhino_state; //0=unknown, 1=false, 2=true
    /// <summary>
    /// Tests if RhinoCommon is currently executing inside of the Rhino.exe process.
    /// There are other cases where RhinoCommon could be running; specifically inside
    /// of Visual Studio when something like a windows form is being worked on in the
    /// resource editor or running stand-alone when compiled to be used as a version
    /// of OpenNURBS.
    /// </summary>
    public static bool RunningInRhino
    {
      get
      {
        if (m_running_in_rhino_state == 0)
        {
#if RHINO_SDK
          m_running_in_rhino_state = 1;
          try
          {
            if (0 != Rhino.RhinoApp.SdkVersion )
              m_running_in_rhino_state = 2;
          }
          catch (Exception)
          {
            m_running_in_rhino_state = 1;
          }
#else
          m_running_in_rhino_state = 1;
#endif
        }
        return (m_running_in_rhino_state == 2);
      }
    }

#if RHINO_SDK
    // 0== unknown
    // 1== loaded
    //-1== not loaded
    static int m_rdk_loadtest;
    /// <summary>
    /// Determines if the RDK is loaded.
    /// </summary>
    /// <param name="throwOnFalse">if the RDK is not loaded, then throws a
    /// <see cref="RdkNotLoadedException"/>.</param>
    /// <param name="usePreviousResult">if true, then the last result can be used instaed of
    /// performing a full check.</param>
    /// <returns>true if the RDK is loaded; false if the RDK is not loaded. Note that the
    /// <see cref="RdkNotLoadedException"/> will hinder the retrieval of any return value.</returns>
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
#endif

    static bool m_bSendDebugToRhino; // = false; initialized by runtime
    /// <summary>
    /// Prints a debug message to the Rhino Command Line. 
    /// The message will only appear if the SendDebugToCommandLine property is set to true.
    /// </summary>
    /// <param name="msg">Message to print.</param>
    public static void DebugString(string msg)
    {
      if (m_bSendDebugToRhino)
      {
#if RHINO_SDK
        UnsafeNativeMethods.RHC_DebugPrint(msg);
#else
        //Console.Write(msg);
#endif
      }
    }
    /// <summary>
    /// Prints a debug message to the Rhino Command Line. 
    /// The message will only appear if the SendDebugToCommandLine property is set to true.
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

    /// <summary>
    /// Informs RhinoCommon of an exception that has been handled but that the developer wants to screen.
    /// </summary>
    /// <param name="ex">An exception.</param>
    public static void ExceptionReport(Exception ex)
    {
      ExceptionReport(null, ex);
    }

    /// <summary>
    /// Informs RhinoCommon of an exception that has been handled but that the developer wants to screen.
    /// </summary>
    /// <param name="source">An exception source text.</param>
    /// <param name="ex">An exception.</param>
    public static void ExceptionReport(string source, Exception ex)
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
      if (!string.IsNullOrEmpty(source))
        DebugString(source);
      DebugString(msg);

      if (OnExceptionReport != null)
        OnExceptionReport(source, ex);
    }

    /// <summary>
    /// Represents a reference to a method that will be called when an exception occurs.
    /// </summary>
    /// <param name="source">An exception source text.</param>
    /// <param name="ex">An exception.</param>
    public delegate void ExceptionReportDelegate(string source, Exception ex);

    /// <summary>
    /// Is raised when an exception is reported with one of the <see cref="ExceptionReport(Exception)"/> method.
    /// </summary>
    public static event ExceptionReportDelegate OnExceptionReport;

#if !OPENNURBS_SDK
    // April 4, 2012 Tim
    // If you don't explicitly set this to null, even though it gets initialized to null, you get compiler
    // warnings in the build process.  This makes Dale jumpy.  So, don't remove the "= null", even though it 
    // isn't necessary.
    static readonly System.Windows.Forms.Form m_invoke_window = null;

    /// <summary>
    /// Calls a method on the main Rhino UI thread if this is necessary.
    /// </summary>
    /// <param name="method">A method. This method is called with no arguments.</param>
    /// <returns>A return object, or null.</returns>
    public static object InvokeOnMainUiThread(Delegate method)
    {
      if (m_invoke_window != null && RunningOnWindows && m_invoke_window.InvokeRequired)
        return m_invoke_window.Invoke(method);

      return method.DynamicInvoke(null);
    }

    /// <summary>
    /// Calls a method on the main Rhino UI thread if this is necessary.
    /// </summary>
    /// <param name="method">A method. This method is called with args arguments.</param>
    /// <param name="args">The method arguments.</param>
    /// <returns>A return object, or null.</returns>
    public static object InvokeOnMainUiThread(Delegate method, params object[] args)
    {
      if (m_invoke_window != null && RunningOnWindows && m_invoke_window.InvokeRequired)
        return m_invoke_window.Invoke(method, args);

      return method.DynamicInvoke(args);
    }
#endif

    /// <summary>
    /// Gets the debug dumps. This is a text description of the geometric contents.
    /// DebugDump() is intended for debugging and is not suitable for creating high
    /// quality text descriptions of an object.
    /// </summary>
    /// <param name="geometry">Some geometry.</param>
    /// <returns>A debug dump text.</returns>
    public static string DebugDumpToString(Rhino.Geometry.GeometryBase geometry)
    {
      IntPtr pConstThis = geometry.ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_Object_Dump(pConstThis, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Gets the debug dumps. This is a text description of the geometric contents.
    /// DebugDump() is intended for debugging and is not suitable for creating high
    /// quality text descriptions of an object.
    /// </summary>
    /// <param name="bezierCurve">curve to evaluate</param>
    /// <returns>A debug dump text.</returns>
    public static string DebugDumpToString(Rhino.Geometry.BezierCurve bezierCurve)
    {
      IntPtr pConstThis = bezierCurve.ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_BezierCurve_Dump(pConstThis, pString);
        return sh.ToString();
      }
    }

#if RHINO_SDK
    /// <summary>
    /// Parses a plugin and create all the commands defined therein.
    /// </summary>
    /// <param name="plugin">Plugin to harvest for commands.</param>
    public static void CreateCommands(PlugIn plugin) 
    {
      if (plugin!=null)
        plugin.InternalCreateCommands();
    }
    /// <summary>
    /// Parses a plugin and create all the commands defined therein.
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
    /// <returns>true on success, false on failure.</returns>
    public static bool RegisterDynamicCommand(PlugIn plugin, Commands.Command cmd)
    {
      // every command must have a RhinoId and Name attribute
      bool rc = false;
      if (plugin != null)
      {
        try
        {
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
      }
      return rc;
    }
#endif
    //static int GetNowHelper(int localeId, IntPtr pStringHolderFormat, IntPtr pResultString)
    //{
    //  int rc;
    //  try
    //  {
    //    string dateformat = StringHolder.GetString(pStringHolderFormat);
    //    if (string.IsNullOrEmpty(dateformat))
    //      return 0;
    //    // surround apostrophe with quotes in order to keep the formatter happy
    //    dateformat = dateformat.Replace("'", "\"'\"");
    //    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(localeId);
    //    DateTime now = System.DateTime.Now;
    //    string s = string.IsNullOrEmpty(dateformat) ? now.ToString(ci) : now.ToString(dateformat, ci);
    //    UnsafeNativeMethods.ON_wString_Set(pResultString, s);
    //    rc = 1;
    //  }
    //  catch (Exception ex)
    //  {
    //    UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
    //    rc = 0;
    //  }
    //  return rc;
    //}

    //static int GetFormattedTimeHelper(int localeId, int sec, int min, int hour, int day, int month, int year, IntPtr pStringHolderFormat, IntPtr pResultString)
    //{
    //  int rc;
    //  try
    //  {
    //    string dateformat = StringHolder.GetString(pStringHolderFormat);
    //    System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo(localeId);
    //    DateTime dt = new DateTime(year, month, day, hour, min, sec);
    //    dt = dt.ToLocalTime();
    //    string s = string.IsNullOrEmpty(dateformat) ? dt.ToString(ci) : dt.ToString(dateformat, ci);
    //    UnsafeNativeMethods.ON_wString_Set(pResultString, s);
    //    rc = 1;
    //  }
    //  catch (Exception ex)
    //  {
    //    UnsafeNativeMethods.ON_wString_Set(pResultString, ex.Message);
    //    rc = 0;
    //  }
    //  return rc;
    //}

    static int EvaluateExpressionHelper(IntPtr statementsAsStringHolder, IntPtr expressionAsStringHolder, int rhinoDocId, IntPtr pResultString)
    {
      int rc = 0;
#if RHINO_SDK
      try
      {
        string state = StringHolder.GetString(statementsAsStringHolder);
        string expr = StringHolder.GetString(expressionAsStringHolder);
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
    internal delegate int EvaluateExpressionCallback(IntPtr statementsAsStringHolder, IntPtr expressionAsStringHolder, int rhinoDocId, IntPtr resultString);
    static readonly EvaluateExpressionCallback m_evaluate_callback = EvaluateExpressionHelper;
    //internal delegate int GetNowCallback(int localeId, IntPtr formatAsStringHolder, IntPtr resultString);
    //static readonly GetNowCallback m_getnow_callback = GetNowHelper;
    //internal delegate int GetFormattedTimeCallback(int locale, int sec, int min, int hour, int day, int month, int year, IntPtr formatAsStringHolder, IntPtr resultString);
    //static readonly GetFormattedTimeCallback m_getformattedtime_callback = GetFormattedTimeHelper;

    static HostUtils()
    {
      // These need to be moved somewhere else because they throw security exceptions
      // when defined in a static constructor
      /*
      UnsafeNativeMethods.RHC_SetPythonEvaluateCallback(m_evaluate_callback);
      UnsafeNativeMethods.RHC_SetGetNowProc(m_getnow_callback, m_getformattedtime_callback);
      */
    }

//    private static bool m_rhinocommoninitialized;
//    /// <summary>
//    /// Makes sure all static RhinoCommon components is set up correctly. 
//    /// This happens automatically when a plug-in is loaded, so you probably won't 
//    /// have to call this method.
//    /// </summary>
//    /// <remarks>Subsequent calls to this method will be ignored.</remarks>
//    public static void InitializeRhinoCommon()
//    {
//      if (m_rhinocommoninitialized)
//        return;
//      m_rhinocommoninitialized = true;

//      AssemblyResolver.InitializeAssemblyResolving();
//      {
//        Type t = typeof(Rhino.DocObjects.Custom.UserDictionary);
//        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID);
//        Rhino.DocObjects.Custom.UserData.RegisterType(t);

//        t = typeof(Rhino.DocObjects.Custom.SharedUserDictionary);
//        UnsafeNativeMethods.ON_UserData_RegisterCustomUserData(t.FullName, t.GUID);
//        Rhino.DocObjects.Custom.UserData.RegisterType(t);
//      }

//#if RHINO_SDK
//      UnsafeNativeMethods.RHC_SetGetNowProc(m_getnow_callback, m_getformattedtime_callback);
//      UnsafeNativeMethods.RHC_SetPythonEvaluateCallback(m_evaluate_callback);

//      UnsafeNativeMethods.RHC_SetCmnUtilitiesCallbacks(PlugIn.GetPlugInSettingsFolderHook, PlugIn.GetPlugInRuiFileNameHook, PlugIn.ValidateRegisteredPlugInRuiFileNameHook);
//#endif
//      InitializeZooClient();
//    }

    /// <summary>
    /// Initializes the ZooClient and Rhino license manager, this should get
    /// called automatically when RhinoCommon is loaded so you probably won't
    /// have to call this method.
    /// </summary>
    public static void InitializeZooClient()
    {
#if RHINO_SDK
      LicenseManager.SetCallbacks();
#endif
    }

#if RHINO_SDK
    /// <summary>
    /// Instantiates a plug-in type and registers the associated commands and classes.
    /// </summary>
    /// <param name="pluginType">A plug-in type. This type must derive from <see cref="PlugIn"/>.</param>
    /// <param name="printDebugMessages">true if debug messages should be printed.</param>
    /// <returns>A new plug-in instance.</returns>
    public static PlugIn CreatePlugIn(Type pluginType, bool printDebugMessages)
    {
      if (null == pluginType || !typeof(PlugIn).IsAssignableFrom(pluginType))
        return null;

      InitializeRhinoCommon();

      // If we turn on debug messages, we always get debug output
      if (printDebugMessages)
        SendDebugToCommandLine = true;

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
#endif

#if RDK_CHECKED
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
    /// Defines if Ole alerts ("Server busy") alerts should be visualized.
    /// <para>This function makes no sense on Mono.</para>
    /// </summary>
    /// <param name="display">Whether alerts should be visible.</param>
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

    ///// <summary>
    ///// Only works on Windows. Returns null on Mac.
    ///// </summary>
    ///// <returns>An assembly.</returns>
    //public static System.Reflection.Assembly GetRhinoDotNetAssembly()
    //{
    //    if (m_rhdn_assembly == null && RunningOnWindows)
    //    {
    //        System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
    //        for (int i = 0; i < assemblies.Length; i++)
    //        {
    //            if (assemblies[i].FullName.StartsWith("Rhino_DotNet", StringComparison.OrdinalIgnoreCase))
    //            {
    //                m_rhdn_assembly = assemblies[i];
    //                break;
    //            }
    //        }
    //    }
    //    return m_rhdn_assembly;
    //}
    //static System.Reflection.Assembly m_rhdn_assembly;

    /// <summary>
    /// Informs the runtime that the application is shutting down.
    /// </summary>
    public static void SetInShutDown()
    {
      try
      {
        UnsafeNativeMethods.RhCmn_SetInShutDown();
        // Remove callbacks that should not happen after this point in time
#if RDK_CHECKED
        Rhino.Render.RdkPlugIn.SetRdkCallbackFunctions(false);
#endif
      }
      catch
      {
        //throw away, we are shutting down
      }
    }

#if RHINO_SDK
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
                Rhino.Drawing.Color c = Interop.ColorFromWin32(abgr);
                //string s = Rhino.Drawing.ColorTranslator.ToHtml(c);
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
                Rhino.Drawing.Rectangle r = Rhino.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
                info.AddValue(name, r);
              }
              break;
            case _point:
              {
                int x = 0, y = 0;
                UnsafeNativeMethods.CRhinoProfileContext_LoadPoint(pRhCmnProfileContext, section, entry, ref x, ref y);
                Rhino.Drawing.Point pt = new Rhino.Drawing.Point(x, y);
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
        int split_index = entry.LastIndexOf("\\", System.StringComparison.Ordinal);
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
        else if( typeof(Rhino.Drawing.Color) == t )
        {
          Rhino.Drawing.Color c = (Rhino.Drawing.Color)e.Value;
          int argb = c.ToArgb();
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileColor(pProfileContext, section, entry, argb);
        }
        else if( typeof(int) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileInt(pProfileContext, section, entry, (int)e.Value);
        else if( typeof(double) == t )
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileDouble(pProfileContext, section, entry, (double)e.Value);
        else if( typeof(Rhino.Drawing.Rectangle) == t )
        {
          Rhino.Drawing.Rectangle r = (Rhino.Drawing.Rectangle)e.Value;
          UnsafeNativeMethods.CRhinoProfileContext_SaveProfileRect(pProfileContext, section, entry, r.Left, r.Top, r.Right, r.Bottom);
        }
        else if( typeof(Rhino.Drawing.Point) == t )
        {
          Rhino.Drawing.Point pt = (Rhino.Drawing.Point)e.Value;
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
          if (mp != null)
          {
            IntPtr pMp = mp.ConstPointer();
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileMeshingParameters(pProfileContext, section, entry, pMp);
          }
        }
        else if( typeof(byte[]) == t )
        {
          byte[] b = e.Value as byte[];
          if (b != null)
          {
            UnsafeNativeMethods.CRhinoProfileContext_SaveProfileBuffer(pProfileContext, section, entry, b.Length, b);
          }
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
#endif
  }

  /// <summary>
  /// Is thrown when the RDK is not loaded.
  /// </summary>
  //[Serializable]
  public class RdkNotLoadedException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the RDK not loaded exception with a standard message.
    /// </summary>
    public RdkNotLoadedException() : base("The Rhino Rdk is not loaded.") { }
  }
}
