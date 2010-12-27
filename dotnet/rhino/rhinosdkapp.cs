using System;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.ApplicationSettings;

namespace Rhino.ApplicationSettings
{
  public enum LicenseNode : int
  {
    Standalone = 0,
    /// <summary>
    /// Network (obtains license from Zoo server)
    /// </summary>
    Network = 1,
    /// <summary>
    /// Network (has license checked out from Zoo server)
    /// </summary>
    NetworkCheckedOut = 2
  }

  public enum Installation : int
  {
    Undefined = 0,   // = CRhinoApp::installation_undefined,
    Commercial,      // = CRhinoApp::installation_commercial,
    Educational,     // = CRhinoApp::installation_educational,
    EducationalLab,  // = CRhinoApp::installation_educational_lab,
    NotForResale,    // = CRhinoApp::installation_not_for_resale,
    NotForResaleLab, // = CRhinoApp::installation_not_for_resale_lab,
    Beta,            // = CRhinoApp::installation_beta,
    BetaLab,         // = CRhinoApp::installation_beta_lab,
    Evaluation,      // = CRhinoApp::installation_evaluation,
    Corporate        // = CRhinoApp::installation_corporate
  }
}

namespace Rhino
{
  ///<summary>.NET RhinoApp is parallel to C++ CRhinoApp</summary>
  public static class RhinoApp
  {
    const int idxSdkVersion = 0;
    const int idxSdkServiceRelease = 1;
    const int idxExeVersion = 2;
    const int idxExeServiceRelease = 3;
    const int idxInstallation = 4;
    const int idxNodeType = 5;
    internal const int idxInScriptRunner = 6;


    ///<summary>
    ///Rhino SDK 9 digit SDK version number in the form YYYYMMDDn
    ///
    ///Rhino will only load plug-ins that were build with exactly the
    ///same version of the SDK.
    ///</summary>
    public static int SdkVersion
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetInt(idxSdkVersion); }
    }

    ///<summary>
    ///Rhino SDK 9 digit SDK service release number in the form YYYYMMDDn
    ///
    ///Service service release of the Rhino SDK supported by this executable. Rhino will only
    ///load plug-ins that require a service release of &lt;= this release number.
    ///For example, SR1 will load all plug-ins made with any SDK released up through and including
    ///the SR1 SDK. But, SR1 will not load a plug-in built using the SR2 SDK. If an &quot;old&quot; Rhino
    ///tries to load a &quot;new&quot; plug-in, the user is told that they have to get a free Rhino.exe
    ///update in order for the plug-in to load. Rhino.exe updates are available from http://www.rhino3d.com.
    ///</summary>
    public static int SdkServiceRelease
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetInt(idxSdkServiceRelease); }
    }

    ///<summary>Major version of Rhino executable 4, 5, ...</summary>
    public static int ExeVersion
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetInt(idxExeVersion); }
    }

    ///<summary>
    ///Service release version of Rhino executable (0, 1, 2, ...)  
    ///The integer is the service release number of Rhino.  For example,
    ///this function returns &quot;0&quot; if Rhino V4SR0 is running and returns
    ///&quot;1&quot; if Rhino V4SR1 is running.
    ///</summary>
    public static int ExeServiceRelease
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetInt(idxExeServiceRelease); }
    }

    public static System.DateTime BuildDate
    {
      get
      {
        int year = 0;
        int month = 0;
        int day = 0;
        UnsafeNativeMethods.CRhinoApp_GetBuildDate(ref year, ref month, ref day);
        // debug builds are 0000-00-00
        if( year==0 && month==0 && day==0 )
          return DateTime.MinValue;
        return new DateTime(year, month, day);
      }
    }

    const int idxSerialNumber = 0;
    const int idxApplicationName = 1;
    const int idxCommandPrompt = 2;
    internal const int idxExecutableFolder = 3;
    internal const int idxInstallFolder = 4;
    internal const int idxHelpFilePath = 5;
    internal const int idxDefaultRuiFile = 6;

    /// <summary>
    /// The product serial number, as seen in Rhino's ABOUT dialog box.
    /// </summary>
    public static string SerialNumber
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.CRhinoApp_GetString(idxSerialNumber);
        if (IntPtr.Zero == rc)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
    }

    public static string Name
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.CRhinoApp_GetString(idxApplicationName);
        if (IntPtr.Zero == rc)
          return null;
        return System.Runtime.InteropServices.Marshal.PtrToStringUni(rc);
      }
    }

    public static LicenseNode NodeType
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoApp_GetInt(idxNodeType);
        return (LicenseNode)rc;
      }
    }

    ///<summary>The product installation type, as seen in Rhino's ABOUT dialog box.</summary>
    public static Installation InstallationType
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoApp_GetInt(idxInstallation);
        return (Installation)rc;
      }
    }

    //static property System::String^ Name{ System::String^ get(); }
    //static property System::String^ RegistryKeyName{ System::String^ get(); }

    const int idxRhino2Id = 0;
    const int idxRhino3Id = 1;
    const int idxRhino4Id = 2;
    const int idxRhino5Id = 3;
    const int idxCurrentRhinoId = 4;


    public static Guid Rhino2Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(idxRhino2Id); }
    }
    public static Guid Rhino3Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(idxRhino3Id); }
    }
    public static Guid Rhino4Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(idxRhino4Id); }
    }
    public static Guid Rhino5Id
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(idxRhino5Id); }
    }
    public static Guid CurrentRhinoId
    {
      get { return UnsafeNativeMethods.CRhinoApp_GetGUID(idxCurrentRhinoId); }
    }

    //static bool IsRhinoId( System::Guid id );
    static object m_lock_object = new object();
    ///<summary>Print formatted text in the command window</summary>
    public static void Write(string message)
    {
      lock (m_lock_object)
      {
        UnsafeNativeMethods.CRhinoApp_Print(message);
      }
    }
    ///<summary>Print formatted text in the command window</summary>
    public static void Write(string format, object arg0)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0));
    }
    ///<summary>Print formatted text in the command window</summary>
    public static void Write(string format, object arg0, object arg1)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0, arg1));
    }
    ///<summary>Print formatted text in the command window</summary>
    public static void Write(string format, object arg0, object arg1, object arg2)
    {
      Write(String.Format(System.Globalization.CultureInfo.InvariantCulture, format, arg0, arg1, arg2));
    }

    ///<summary>Print a newline in the command window</summary>
    public static void WriteLine()
    {
      Write("\n");
    }
    ///<summary>Print text in the command window</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static void WriteLine(string message)
    {
      Write(message + "\n");
    }
    ///<summary>Print formatted text with a newline in the command window</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayer.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayer.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayer.py' lang='py'/>
    /// </example>
    public static void WriteLine(string format, object arg0)
    {
      Write(format + "\n", arg0);
    }
    ///<summary>Print formatted text with a newline in the command window</summary>
    public static void WriteLine(string format, object arg0, object arg1)
    {
      Write(format + "\n", arg0, arg1);
    }
    ///<summary>Print formatted text with a newline in the command window</summary>
    public static void WriteLine(string format, object arg0, object arg1, object arg2)
    {
      Write(format + "\n", arg0, arg1, arg2);
    }

    ///<summary>Set Rhino command prompt</summary>
    ///<param name="prompt"></param>
    ///<param name="promptDefault">
    ///text that appears in angle brackets and indicates what will happen if the user pressed ENTER.
    ///</param>
    public static void SetCommandPrompt(string prompt, string promptDefault)
    {
      UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(prompt, promptDefault);
    }
    ///<summary>Set Rhino command prompt</summary>
    ///<param name="prompt"></param>
    public static void SetCommandPrompt(string prompt)
    {
      UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(prompt, null);
    }

    ///<summary>Rhino command prompt.</summary>
    public static string CommandPrompt
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.CRhinoApp_GetString(idxCommandPrompt);
        if (IntPtr.Zero == rc)
          return null;
        return System.Runtime.InteropServices.Marshal.PtrToStringUni(rc);
      }
      set
      {
        UnsafeNativeMethods.CRhinoApp_SetCommandPrompt(value, null);
      }
    }

    /// <summary>
    /// Text in Rhino's command history window
    /// </summary>
    public static string CommandHistoryWindowText
    {
      get
      {
        using (Rhino.Runtime.StringHolder holder = new Rhino.Runtime.StringHolder())
        {
          UnsafeNativeMethods.CRhinoApp_GetCommandHistoryWindowText(holder.NonConstPointer());
          string rc = holder.ToString();
          if (string.IsNullOrEmpty(rc))
            return string.Empty;
          rc = rc.Replace('\r', '\n');
          return rc;
        }
      }
    }
    /// <summary>
    /// Clear the text in Rhino's command history window
    /// </summary>
    public static void ClearCommandHistoryWindow()
    {
      UnsafeNativeMethods.CRhinoApp_ClearCommandHistoryWindowText();
    }

    ///<summary>Sends a string of printable characters, including spaces, to Rhino&apos;s command line.</summary>
    ///<param name='characters'>[in] A string to characters to send to the command line. This can be null.</param>
    ///<param name='appendReturn'>[in] Append a return character to the end of the string.</param>
    public static void SendKeystrokes(string characters, bool appendReturn)
    {
      UnsafeNativeMethods.CRhinoApp_SendKeystrokes(characters, appendReturn);
    }

    public static void SetFocusToMainWindow()
    {
      UnsafeNativeMethods.CRhinoApp_SetFocusToMainWindow();
    }

    public static bool ReleaseMouseCapture()
    {
      return UnsafeNativeMethods.CRhinoApp_ReleaseCapture();
    }

    //[DllImport(Import.lib)]
    //static extern IntPtr CRhinoApp_DefaultRenderer([MarshalAs(UnmanagedType.LPWStr)] string str);
    /////<summary>Rhino's current, or default, render plug-in.</summary>
    //public static string DefaultRenderer
    //{
    //  get
    //  {
    //    IntPtr rc = CRhinoApp_DefaultRenderer(null);
    //    if (IntPtr.Zero == rc)
    //      return null;
    //    return Marshal.PtrToStringUni(rc);
    //  }
    //  set
    //  {
    //    CRhinoApp_DefaultRenderer(value);
    //  }
    //}

    ///<summary>Exits, or closes, Rhino.</summary>
    public static void Exit()
    {
      UnsafeNativeMethods.CRhinoApp_Exit();
    }


    ///<summary>Run a Rhino command script.</summary>
    ///<param name="script">[in] script to run</param>
    ///<param name="echo">
    ///Controls how the script is echoed in the command output window.
    ///false = silent - nothing is echoed
    ///true = verbatim - the script is echoed literally
    ///</param>
    ///<remarks>
    ///Rhino acts as if each character in the script string had been typed in the command prompt.
    ///When RunScript is called from a &quot;script runner&quot; command, it completely runs the
    ///script before returning. When RunScript is called outside of a command, it returns and the
    ///script is run. This way menus and buttons can use RunScript to execute complicated functions.
    ///</remarks>
    public static bool RunScript(string script, bool echo)
    {
      int echoMode = echo ? 1 : 0;
      return UnsafeNativeMethods.CRhinoApp_RunScript(script, echoMode);
    }

    /// <summary>
    /// Pause to keep Windows message pump alive so views will update
    /// and windows will repaint
    /// </summary>
    public static void Wait()
    {
      UnsafeNativeMethods.CRhinoApp_Wait(0);
    }

    class MainWindowWrapper : System.Windows.Forms.IWin32Window
    {
      IntPtr m_handle;
      public MainWindowWrapper(IntPtr handle)
      {
        m_handle = handle;
      }
      public IntPtr Handle
      {
        get { return m_handle; }
      }
    }
    static MainWindowWrapper m_mainwnd;

    public static System.Windows.Forms.IWin32Window MainWindow()
    {
      if (null == m_mainwnd)
      {
        IntPtr pWnd = MainWindowHandle();
        if (IntPtr.Zero != pWnd)
          m_mainwnd = new MainWindowWrapper(pWnd);
      }
      return m_mainwnd;
    }

    /// <summary>
    /// Gets the WindowHandle of the Rhino main window.
    /// </summary>
    public static IntPtr MainWindowHandle()
    {
      if (null != m_mainwnd)
        return m_mainwnd.Handle;

      System.Diagnostics.Process process = System.Diagnostics.Process.GetCurrentProcess();
      if (process == null)
        return IntPtr.Zero;

      return process.MainWindowHandle;
    }

    /// <summary>
    /// Get the object that is returned by PlugIn.GetPlugInObject for a given
    /// plug-in. This function attempts to find and load a plug-in with a given Id.
    /// When a plug-in is found, it's GetPlugInObject function is called and the
    /// result is returned here.
    /// Note the plug-in must have already been installed in Rhino or the plug-in manager
    /// will not know where to look for a plug-in with a matching id.
    /// </summary>
    /// <param name="pluginId">Guid for a given plug-in</param>
    /// <returns>
    /// Result of PlugIn.GetPlugInObject for a given plug-in on success.
    /// </returns>
    public static object GetPlugInObject(Guid pluginId)
    {
      if (pluginId == Guid.Empty)
        return null;

      // see if the plug-in is already loaded before doing any heavy lifting
      PlugIns.PlugIn p = Rhino.PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (p != null)
        return p.GetPlugInObject();


      // load plug-in
      UnsafeNativeMethods.CRhinoPlugInManager_LoadPlugIn(pluginId);
      p = Rhino.PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (p != null)
        return p.GetPlugInObject();

      IntPtr iunknown = UnsafeNativeMethods.CRhinoApp_GetPlugInObject(pluginId);
      if (IntPtr.Zero == iunknown)
        return null;

      object rc = null;
      try
      {
        rc = System.Runtime.InteropServices.Marshal.GetObjectForIUnknown(iunknown);
      }
      catch (Exception)
      {
        rc = null;
      }
      return rc;
    }

    /// <summary>
    /// Get the object that is returned by PlugIn.GetPlugInObject for a given
    /// plug-in. This function attempts to find and load a plug-in with a given name.
    /// When a plug-in is found, it's GetPlugInObject function is called and the
    /// result is returned here.
    /// Note the plug-in must have already been installed in Rhino or the plug-in manager
    /// will not know where to look for a plug-in with a matching name.
    /// </summary>
    /// <param name="plugin">Name of a plug-in</param>
    /// <returns>
    /// Result of PlugIn.GetPlugInObject for a given plug-in on success.
    /// </returns>
    public static object GetPlugInObject(string plugin)
    {
      Guid plugin_id = Guid.Empty;
      try
      {
        plugin_id = new Guid(plugin);
      }
      catch (Exception)
      {
        plugin_id = Guid.Empty;
      }

      if (plugin_id == Guid.Empty)
        plugin_id = UnsafeNativeMethods.CRhinoPlugInManager_GetPlugInId(plugin);

      return GetPlugInObject(plugin_id);
    }

    #region events
    // Callback that doesn't pass any parameters or return values
    internal delegate void RhCmnEmptyCallback();

    private static RhCmnEmptyCallback m_OnEscapeKey;
    private static void OnEscapeKey()
    {
      if (m_escape_key != null)
      {
        try
        {
          m_escape_key(null, System.EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static EventHandler m_escape_key;
    public static event EventHandler EscapeKeyPressed
    {
      add
      {
        if (Runtime.HostUtils.ContainsDelegate(m_escape_key, value))
          return;

        m_escape_key += value;
        m_OnEscapeKey = OnEscapeKey;
        UnsafeNativeMethods.RHC_SetEscapeKeyCallback(m_OnEscapeKey);
      }
      remove
      {
        m_escape_key -= value;
        if (null == m_escape_key)
        {
          UnsafeNativeMethods.RHC_SetEscapeKeyCallback(null);
          m_OnEscapeKey = null;
        }
      }
    }


    private static RhCmnEmptyCallback m_OnInitApp;
    private static void OnInitApp()
    {
      if (m_init_app != null)
      {
        try
        {
          m_init_app(null, System.EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler m_init_app;
    public static event EventHandler Initialized
    {
      add
      {
        if (m_init_app == null)
        {
          m_OnInitApp = OnInitApp;
          UnsafeNativeMethods.CRhinoEventWatcher_SetInitAppCallback(m_OnInitApp, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_init_app += value;
      }
      remove
      {
        m_init_app -= value;
        if (m_init_app == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetInitAppCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnInitApp = null;
        }
      }
    }


    private static RhCmnEmptyCallback m_OnCloseApp;
    private static void OnCloseApp()
    {
      if (m_close_app != null)
      {
        try
        {
          m_close_app(null, System.EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal static EventHandler m_close_app;
    public static event EventHandler Closing
    {
      add
      {
        if (m_close_app == null)
        {
          m_OnCloseApp = OnCloseApp;
          UnsafeNativeMethods.CRhinoEventWatcher_SetCloseAppCallback(m_OnCloseApp, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_close_app += value;
      }
      remove
      {
        m_close_app -= value;
        if (m_close_app == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetCloseAppCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnCloseApp = null;
        }
      }
    }


    private static RhCmnEmptyCallback m_OnAppSettingsChanged;
    private static void OnAppSettingsChanged()
    {
      if (m_appsettings_changed != null)
      {
        try
        {
          m_appsettings_changed(null, System.EventArgs.Empty);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler m_appsettings_changed;
    public static event EventHandler AppSettingsChanged
    {
      add
      {
        if (m_appsettings_changed == null)
        {
          m_OnAppSettingsChanged = OnAppSettingsChanged;
          UnsafeNativeMethods.CRhinoEventWatcher_SetAppSettingsChangeCallback(m_OnAppSettingsChanged, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_appsettings_changed += value;
      }
      remove
      {
        m_appsettings_changed -= value;
        if (m_appsettings_changed == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetAppSettingsChangeCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnAppSettingsChanged = null;
        }
      }
    }
    #endregion
  }
}
