#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

#if RHINO_SDK
namespace Rhino.Commands
{
  [Flags]
  public enum Style : int
  {
    None = 0,
    /// <summary>
    /// Also known as a "test" command. The command name does not auto-complete
    /// when typed on the command line an is therefore not discoverable. Useful
    /// for writing commands that users don't normally have access to.
    /// </summary>
    Hidden = 1,
    /// <summary>
    /// For commands that want to run scripts as if they were typed at the command
    /// line (like RhinoScript's RunScript command)
    /// </summary>
    ScriptRunner = 2,
    /// <summary>
    /// Transparent commands can be run inside of other commands.
    /// The command does not modify the contents of the model's geometry in any way.
    /// Examples of transparent commands include commands that change views and toggle
    /// snap states.  Any command that adds or deletes, a view cannot be transparent.
    /// </summary>
    Transparent = 4,
    /// <summary>
    /// The command should not be repeated by pressing "ENTER" immediately after
    /// the command finishes
    /// </summary>
    DoNotRepeat = 8,
    /// <summary>
    /// By default, all commands are undoable.
    /// </summary>
    NotUndoable = 16
  }

  public enum RunMode : int
  {
    /// <summary>
    /// Can use dialogs for input. Must use message boxes to
    /// report serious error conditions.
    /// </summary>
    Interactive = 0,
    /// <summary>
    /// All input must come from command line, GetPoint, GetObject,
    /// GetString, etc.  Must use message boxes to report serious
    /// error conditions.  Script mode gets used when a command is
    /// run with a hyphen (-) prefix.
    /// </summary>
    Scripted = 1
  }

  [AttributeUsage(AttributeTargets.Class)]
  public sealed class CommandStyleAttribute : System.Attribute
  {
    private readonly Style m_style;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="styles">
    /// set of values combined using a bitwise OR operation to get the desired combination
    /// of command styles
    /// </param>
    public CommandStyleAttribute(Style styles)
    {
      m_style = styles;
    }

    public Style Styles
    {
      get { return m_style; }
    }
  }

  public enum Result : int
  {
    /// <summary>command worked</summary>
    Success = 0,
    /// <summary>user canceled command</summary>
    Cancel  = 1, 
    /// <summary>command did nothing but cancel was not pressed</summary>
    Nothing = 2,
    /// <summary>command failed (bad input, computational problem, etc.)</summary>
    Failure,
    /// <summary>command not found (user probably had a typo in command name)</summary>
    UnknownCommand,
    CancelModelessDialog,
    /// <summary>exit RhinoCommon</summary>
    ExitRhino = 0x0FFFFFFF
  }

  public class MostRecentCommandDescription
  {
    public string DisplayString { get; set; }
    public string Macro { get; set; }
  }

  public abstract class Command
  {
    public static bool IsValidCommandName(string name)
    {
      return UnsafeNativeMethods.CRhinoCommand_IsValidCommandName(name);
    }

    public static Guid LastCommandId
    {
      get
      {
        return UnsafeNativeMethods.CRhinoEventWatcher_LastCommandId();
      }
    }
    public static Result LastCommandResult
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoEventWatcher_LastCommandResult();
        return (Result)rc;
      }
    }

    public static MostRecentCommandDescription[] GetMostRecentCommands()
    {
      IntPtr pDisplayStrings = UnsafeNativeMethods.ON_StringArray_New();
      IntPtr pMacros = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoApp_GetMRUCommands(pDisplayStrings, pMacros);
      MostRecentCommandDescription[] rc = new MostRecentCommandDescription[count];
      using(var sh = new Rhino.Runtime.StringHolder() )
      {
        IntPtr pString = sh.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          var mru = new MostRecentCommandDescription();
          UnsafeNativeMethods.ON_StringArray_Get(pDisplayStrings, i, pString);
          mru.DisplayString = sh.ToString();
          UnsafeNativeMethods.ON_StringArray_Get(pMacros, i, pString);
          mru.Macro = sh.ToString();
          rc[i] = mru;
        }
      }
      UnsafeNativeMethods.ON_StringArray_Delete(pDisplayStrings);
      UnsafeNativeMethods.ON_StringArray_Delete(pMacros);
      return rc;
    }


    static int m_serial_number_counter = 1;
    static readonly Collections.RhinoList<Command> m_all_commands = new Rhino.Collections.RhinoList<Command>();

    internal int m_runtime_serial_number;
    internal Style m_style_flags;
    internal Rhino.PlugIns.PlugIn m_plugin;

    internal static Command LookUpBySerialNumber(int sn)
    {
      for (int i = 0; i < m_all_commands.Count; i++)
      {
        Command cmd = m_all_commands[i];
        if (cmd.m_runtime_serial_number == sn)
          return cmd;
      }
      return null;
    }


    // only allow instantiation through subclassing
    protected Command()
    {
      m_runtime_serial_number = m_serial_number_counter;
      m_serial_number_counter++;
      m_all_commands.Add(this);

      // set RunCommand and callback if it hasn't already been set
      if (null == m_RunCommand)
      {
        m_RunCommand = OnRunCommand;
        m_DoHelp = OnDoHelp;
        m_ContextHelp = OnCommandContextHelpUrl;
        m_ReplayHistory = OnReplayHistory;
        UnsafeNativeMethods.CRhinoCommand_SetRunCommandCallbacks(m_RunCommand, m_DoHelp, m_ContextHelp, m_ReplayHistory);
      }
    }

#region properties
    public PlugIns.PlugIn PlugIn
    {
      get
      {
        return this.m_plugin;
      }
    }

    public virtual Guid Id
    {
      get
      {
        return this.GetType().GUID;
      }
    }

    public abstract string EnglishName{ get; }
    public virtual string LocalName
    {
      get { return Rhino.UI.Localization.LocalizeCommandName(EnglishName, this); }
    }

    public PersistentSettings Settings
    {
      get { return m_plugin.CommandSettings( this.EnglishName ); }
    }

#endregion

    protected abstract Result RunCommand(RhinoDoc doc, RunMode mode);
    internal int OnRunCommand(int command_serial_number, int doc_id, int mode)
    {
      Result rc = Result.Failure;
      try
      {
        Command cmd = LookUpBySerialNumber(command_serial_number);
        RhinoDoc doc = RhinoDoc.FromId(doc_id);
        RunMode rm = RunMode.Interactive;
        if (mode > 0)
          rm = RunMode.Scripted;
        if (cmd == null || doc == null)
          return (int)Result.Failure;

        rc = cmd.RunCommand(doc, rm);
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.DebugString("Exception caught during RunCommand");
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return (int)rc;
    }

    protected virtual void OnHelp() { }
    protected string CommandContextHelpUrl{ get { return string.Empty; } }
    static void OnDoHelp(int command_serial_number)
    {
      try
      {
        Command cmd = LookUpBySerialNumber(command_serial_number);
        if (cmd != null)
          cmd.OnHelp();
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }
    static int OnCommandContextHelpUrl(int command_serial_number, IntPtr pON_wString)
    {
      int rc = 0;
      try
      {
        Command cmd = LookUpBySerialNumber(command_serial_number);
        if (cmd != null && IntPtr.Zero != pON_wString)
        {
          string url = cmd.CommandContextHelpUrl;
          if (!string.IsNullOrEmpty(url))
          {
            rc = 1;
            UnsafeNativeMethods.ON_wString_Set(pON_wString, url);
          }
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
        rc = 0;
      }
      return rc;
    }
    
    /// <summary>
    /// Determines if Rhino is currently running a command. Because Rhino allow for transparent commands
    /// (commands that can be run from inside of other commands), this method returns the total ids of
    /// active commands.
    /// </summary>
    /// <returns>
    /// Ids of running commands or null if no commands are currently running. 
    /// The "active" command is at the end of this list.
    /// </returns>
    public static Guid[] GetCommandStack()
    {
      System.Collections.Generic.List<Guid> ids = new System.Collections.Generic.List<Guid>();
      int i = 0;
      while (true)
      {
        Guid id = UnsafeNativeMethods.CRhinoApp_GetRunningCommandId(i);
        if (id == Guid.Empty)
          break;
        ids.Add(id);
        i++;
      }
      return ids.Count < 1 ? null : ids.ToArray();
    }

    /// <summary>
    /// Determines if Rhino is currently running a command.
    /// </summary>
    /// <returns>True if a command is currently running, false if no commands are currently running.</returns>
    public static bool InCommand()
    {
      return GetCommandStack() != null;
    }

    /// <summary>
    /// This is a low level tool to determine if Rhino is currently running
    /// a script running command like "ReadCommandFile" or the RhinoScript
    /// plug-in's "RunScript".
    /// </summary>
    /// <returns>True if a script running command is active.</returns>
    public static bool InScriptRunnerCommand()
    {
      int rc = UnsafeNativeMethods.CRhinoApp_GetInt(RhinoApp.idxInScriptRunner);
      return (1 == rc) ? true : false;
    }

    public static bool IsCommand(string name)
    {
      return UnsafeNativeMethods.RhCommand_IsCommand(name);
    }

    public static Guid LookupCommandId(string name, bool searchForEnglishName)
    {
      if( searchForEnglishName && !name.StartsWith("_", StringComparison.Ordinal))
        name = "_" + name;

      Guid rc = UnsafeNativeMethods.CRhinoApp_LookupCommandByName(name);
      return rc;
    }

    public static string LookupCommandName(Guid commandId, bool englishName)
    {
      IntPtr pName = UnsafeNativeMethods.CRhinoApp_LookupCommandById(commandId, englishName);
      if (IntPtr.Zero == pName)
        return null;
      return Marshal.PtrToStringUni(pName);
    }

    /// <summary>
    /// Get list of command names in Rhino. This list does not include Test, Alpha, or System commands
    /// </summary>
    /// <param name="english">
    ///  if true, retrieve the english name for every command.
    ///  if false, retrieve the local name for every command
    /// </param>
    /// <param name="loaded">
    /// if true, only get names of currently loaded commands.
    /// if false, get names of all registered (may not be currently loaded) commands
    /// </param>
    /// <returns></returns>
    public static string[] GetCommandNames(bool english, bool loaded)
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoCommandManager_GetCommandNames(pStrings, english, loaded);
      string[] rc = new string[count];
      using( Rhino.Runtime.StringHolder sh = new Runtime.StringHolder() )
      {
        IntPtr pString = sh.NonConstPointer();
        for( int i=0; i<count; i++ )
        {
          UnsafeNativeMethods.ON_StringArray_Get(pStrings, i, pString);
          rc[i] = sh.ToString();
        }
      }
      UnsafeNativeMethods.ON_StringArray_Delete(pStrings);
      return rc;
    }

    public static void DisplayHelp(Guid commandId)
    {
      UnsafeNativeMethods.CRhinoApp_DisplayCommandHelp(commandId);
    }

    #region events
    internal delegate int RunCommandCallback(int command_serial_number, int doc_id, int mode);
    private static RunCommandCallback m_RunCommand;
    internal delegate void DoHelpCallback(int command_serial_number);
    private static DoHelpCallback m_DoHelp;
    internal delegate int ContextHelpCallback(int command_serial_number, IntPtr pON_wString);
    private static ContextHelpCallback m_ContextHelp;
    internal delegate int ReplayHistoryCallback(int command_serial_number, IntPtr pConstRhinoHistoryRecord, IntPtr pObjectPairArray);
    private static ReplayHistoryCallback m_ReplayHistory;

    internal delegate void CommandCallback(IntPtr pCommand, int rc);
    private static CommandCallback m_OnBeginCommand;
    private static CommandCallback m_OnEndCommand;
    private static void OnBeginCommand(IntPtr pCommand, int rc)
    {
      if (m_begin_command != null)
      {
        try
        {
          CommandEventArgs e = new CommandEventArgs(pCommand, rc);
          m_begin_command(null, e);
          e.m_pCommand = IntPtr.Zero;
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void OnEndCommand(IntPtr pCommand, int rc)
    {
      if (m_end_command != null)
      {
        try
        {
          CommandEventArgs e = new CommandEventArgs(pCommand, rc);
          m_end_command(null, e);
          e.m_pCommand = IntPtr.Zero;
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    internal static EventHandler<CommandEventArgs> m_begin_command;

    /// <summary>
    /// Called just before command.RunCommand().
    /// </summary>
    public static event EventHandler<CommandEventArgs> BeginCommand
    {
      add
      {
        if (m_begin_command == null)
        {
          m_OnBeginCommand = OnBeginCommand;
          UnsafeNativeMethods.CRhinoEventWatcher_SetBeginCommandCallback(m_OnBeginCommand, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_begin_command += value;
      }
      remove
      {
        m_begin_command -= value;
        if (m_begin_command == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetBeginCommandCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnBeginCommand = null;
        }
      }
    }

    internal static EventHandler<CommandEventArgs> m_end_command;

    /// <summary>
    /// Called immediately after command.RunCommand().
    /// </summary>
    public static event EventHandler<CommandEventArgs> EndCommand
    {
      add
      {
        if (m_end_command == null)
        {
          m_OnEndCommand = OnEndCommand;
          UnsafeNativeMethods.CRhinoEventWatcher_SetEndCommandCallback(m_OnEndCommand, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_end_command += value;
      }
      remove
      {
        m_end_command -= value;
        if (m_end_command == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetEndCommandCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnEndCommand = null;
        }
      }
    }


    internal delegate void UndoCallback(int undo_event, uint undo_record_sn, Guid command_id);
    private static UndoCallback m_OnUndoEvent;
    private static void OnUndoEvent(int undo_event, uint undo_record_sn, Guid command_id)
    {
      if (m_undo_event != null)
      {
        try
        {
          m_undo_event(null, new UndoRedoEventArgs(undo_event, undo_record_sn, command_id));
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    internal static EventHandler<UndoRedoEventArgs> m_undo_event;
    /// <summary>
    /// Used to monitor Rhino's built in undo/redo support.
    /// </summary>
    public static event EventHandler<UndoRedoEventArgs> UndoRedo
    {
      add
      {
        if (m_undo_event == null)
        {
          m_OnUndoEvent = OnUndoEvent;
          UnsafeNativeMethods.CRhinoEventWatcher_SetUndoEventCallback(m_OnUndoEvent, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_undo_event += value;
      }
      remove
      {
        m_undo_event -= value;
        if (m_undo_event == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetUndoEventCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnUndoEvent = null;
        }
      }
    }

    #endregion


    protected virtual bool ReplayHistory(Rhino.DocObjects.ReplayHistoryData replayData)
    {
      return false;
    }
    private static int OnReplayHistory(int command_serial_number, IntPtr pConstRhinoHistoryRecord, IntPtr pObjectPairArray)
    {
      int rc = 0;
      try
      {
        Command cmd = LookUpBySerialNumber(command_serial_number);
        using (var replayData = new DocObjects.ReplayHistoryData(pConstRhinoHistoryRecord, pObjectPairArray))
        {
          rc = cmd.ReplayHistory(replayData) ? 1 : 0;
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport("Command.ReplayHistory", ex);
      }
      return rc;
    }
  }

  public class CommandEventArgs : EventArgs
  {
    internal IntPtr m_pCommand;
    readonly Result m_result;

    internal CommandEventArgs(IntPtr pCommand, int result)
    {
      m_pCommand = pCommand;
      m_result = (Result)result;
    }

    /// <summary>
    /// Gets the ID of the command that raised this event.
    /// </summary>
    public Guid CommandId
    {
      get { return UnsafeNativeMethods.CRhinoCommand_Id(m_pCommand); }
    }

    string m_english_name;
    string m_local_name;
    /// <summary>
    /// Gets the English name of the command that raised this event.
    /// </summary>
    public string CommandEnglishName
    {
      get
      {
        if (m_english_name == null)
        {
          IntPtr pName = UnsafeNativeMethods.CRhinoCommand_Name(m_pCommand, true);
          if (IntPtr.Zero == pName)
            m_english_name = string.Empty;
          else
            m_english_name = Marshal.PtrToStringUni(pName);
        }
        return m_english_name;
      }
    }

    /// <summary>
    /// Gets the name of the command that raised this event in the local language.
    /// </summary>
    public string CommandLocalName
    {
      get
      {
        if (m_local_name == null)
        {
          IntPtr pName = UnsafeNativeMethods.CRhinoCommand_Name(m_pCommand, false);
          if (IntPtr.Zero == pName)
            m_local_name = string.Empty;
          else
            m_local_name = Marshal.PtrToStringUni(pName);
        }
        return m_local_name;
      }
    }

    string m_plugin_name;
    /// <summary>
    /// Gets the name of the plug-in that this command belongs to.  If the command is internal
    /// to Rhino, then this propert is an empty string
    /// </summary>
    public string CommandPluginName
    {
      get
      {
        if (m_plugin_name == null)
        {
          IntPtr pName = UnsafeNativeMethods.CRhinoCommand_PlugInName(m_pCommand);
          if (IntPtr.Zero == pName)
            m_plugin_name = string.Empty;
          else
            m_plugin_name = Marshal.PtrToStringUni(pName);
        }
        return m_plugin_name;
      }
    }

    /// <summary>
    /// Gets the result of the command that raised this event. 
    /// This value is only meaningful during EndCommand events.
    /// </summary>
    public Result CommandResult
    {
      get { return m_result; }
    }
  }

  public class UndoRedoEventArgs : EventArgs
  {
    readonly int m_event_type;
    readonly uint m_serial_number;
    readonly Guid m_command_id;
    internal UndoRedoEventArgs(int undo_event, uint sn, Guid id)
    {
      m_event_type = undo_event;
      m_serial_number = sn;
      m_command_id = id;
    }

    public Guid CommandId
    {
      get { return m_command_id; }
    }

    [CLSCompliant(false)]
    public uint UndoSerialNumber
    {
      get { return m_serial_number; }
    }

    public bool IsBeginRecording { get { return 1 == m_event_type; } }
    public bool IsEndRecording { get { return 2 == m_event_type; } }
    public bool IsBeginUndo { get { return 3 == m_event_type; } }
    public bool IsEndUndo { get { return 4 == m_event_type; } }
    public bool IsBeginRedo { get { return 5 == m_event_type; } }
    public bool IsEndRedo { get { return 6 == m_event_type; } }
    public bool IsPurgeRecord { get { return 86 == m_event_type; } }
  }

  /// <summary>
  /// For adding nestable selection commands that work like the native Rhino
  /// SelCrv command, derive your command from SelCommand and override the
  /// virtual SelFilter function.
  /// </summary>
  public abstract class SelCommand : Command
  {
    // only allow instantiation through subclassing
    protected SelCommand()
    {
      // set RunCommand and callback if it hasn't already been set
      if (null == m_SelFilter)
      {
        m_SelFilter = OnSelFilter;
        UnsafeNativeMethods.CRhinoCommand_SetSelCommandCallback(m_SelFilter);
      }
    }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode) { return Result.Success; }

    /// <summary>
    /// Override this virtual function and return true if object should be selected.
    /// </summary>
    /// <param name="rhObj"></param>
    /// <returns></returns>
    protected abstract bool SelFilter(Rhino.DocObjects.RhinoObject rhObj);
    static int OnSelFilter(int command_serial_number, IntPtr pRhinoObject)
    {
      int rc = 0;
      try
      {
        SelCommand cmd = Command.LookUpBySerialNumber(command_serial_number) as SelCommand;
        rc = cmd.SelFilter(Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pRhinoObject)) ? 1:0;
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.DebugString("Exception caught during SelFilter");
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return rc;
    }

    const int idxTestLights = 0;
    const int idxTestGrips = 1;
    const int idxBeQuite = 2;

    public bool TestLights
    {
      get { return UnsafeNativeMethods.CRhinoSelCommand_GetBool(Id, idxTestLights); }
      set { UnsafeNativeMethods.CRhinoSelCommand_SetBool(Id, idxTestLights, value); }
    }
    public bool TestGrips
    {
      get { return UnsafeNativeMethods.CRhinoSelCommand_GetBool(Id, idxTestGrips); }
      set { UnsafeNativeMethods.CRhinoSelCommand_SetBool(Id, idxTestGrips, value); }
    }
    public bool BeQuiet
    {
      get { return UnsafeNativeMethods.CRhinoSelCommand_GetBool(Id, idxBeQuite); }
      set { UnsafeNativeMethods.CRhinoSelCommand_SetBool(Id, idxBeQuite, value); }
    }

    internal delegate int SelFilterCallback(int command_id, IntPtr pRhinoObject);
    private static SelFilterCallback m_SelFilter;

  }

#if USING_V5_SDK
  public abstract class TransformCommand : Command
  {
    protected Result SelectObjects( string prompt, Rhino.Collections.TransformObjectList list )
    {
      IntPtr pList = list.NonConstPointer();
      int rc = UnsafeNativeMethods.CRhinoTransformCommand_SelectObjects(Id, prompt, pList);
      return (Result)rc;
    }
    
    protected void TransformObjects( Rhino.Collections.TransformObjectList list, Rhino.Geometry.Transform xform, bool copy, bool autoHistory)
    {
      IntPtr pList = list.NonConstPointer();
      UnsafeNativeMethods.CRhinoTransformCommand_TransformObjects(Id, pList, ref xform, copy, autoHistory);
    }

    protected void DuplicateObjects( Rhino.Collections.TransformObjectList list )
    {
      IntPtr pList = list.NonConstPointer();
      UnsafeNativeMethods.CRhinoTransformCommand_DuplicateObjects(Id, pList);
    }

    /// <summary>
    /// Sets dynamic grip locations back to starting grip locations. This makes things
    /// like the Copy command work when grips are "copied".
    /// </summary>
    /// <param name="list"></param>
    protected void ResetGrips( Rhino.Collections.TransformObjectList list )
    {
      IntPtr pList = list.NonConstPointer();
      UnsafeNativeMethods.CRhinoTransformCommand_ResetGrips(Id, pList);
    }



    //CRhinoView* View() { return m_view; }
    //bool ObjectsWerePreSelected() { return m_objects_were_preselected; }
  }
#endif
}


namespace Rhino.DocObjects
{
  // this is the same as CRhinoHistory
  public class HistoryRecord : IDisposable
  {
    private IntPtr m_pRhinoHistory; // CRhinoHistory*

    public HistoryRecord(Commands.Command command, int version)
    {
      m_pRhinoHistory = UnsafeNativeMethods.CRhinoHistory_New(command.Id, version);
    }

    IntPtr NonConstPointer()
    {
      return m_pRhinoHistory;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~HistoryRecord() { Dispose(false); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pRhinoHistory)
      {
        UnsafeNativeMethods.CRhinoHistory_Delete(m_pRhinoHistory);
      }
      m_pRhinoHistory = IntPtr.Zero;
    }
    
    public bool SetBool( int id, bool value )
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetBool(pThis, id, value);
    }
    public bool SetInt(int id, int value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetInt(pThis, id, value);
    }
    public bool SetDouble(int id, double value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetDouble(pThis, id, value);
    }
    public bool SetPoint3d(int id, Rhino.Geometry.Point3d value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetPoint3d(pThis, id, value);
    }
    public bool SetVector3d(int id, Rhino.Geometry.Vector3d value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetVector3d(pThis, id, value);
    }
    public bool SetTransorm(int id, Rhino.Geometry.Transform value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetXform(pThis, id, ref value);
    }
    public bool SetColor(int id, System.Drawing.Color value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetColor(pThis, id, value.ToArgb());
    }
    public bool SetObjRef(int id, ObjRef value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstObjRef = value.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetObjRef(pThis, id, pConstObjRef);
    }
    public bool SetPoint3dOnObject(int id, ObjRef objref, Rhino.Geometry.Point3d value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstObjRef = objref.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetPoint3dOnObject(pThis, id, pConstObjRef, value);
    }
    public bool SetGuid(int id, Guid value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetUuid(pThis, id, value);
    }
    public bool SetString(int id, string value)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetString(pThis, id, value);
    }
    // ON_Geometry* is non-const. I think we can't delete it from under the history record.
    // Don't wrap until we really need it
    //public bool SetGeometry( int id, Geometry.GeometryBase value){ return false; }

    public bool SetCurve(int id, Geometry.Curve value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstCurve = value.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetCurve(pThis, id, pConstCurve);
    }
    public bool SetSurface(int id, Geometry.Surface value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstSurface = value.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetSurface(pThis, id, pConstSurface);
    }
    public bool SetBrep(int id, Geometry.Brep value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstBrep = value.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetBrep(pThis, id, pConstBrep);
    }
    public bool SetMesh(int id, Geometry.Mesh value)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstMesh = value.ConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetMesh(pThis, id, pConstMesh);
    }

    // PolyEdge not wrapped yet
    //bool SetPolyEdgeValue( CRhinoDoc& doc, int value_id, const class CRhinoPolyEdge& polyedge );

    public bool SetBools(int id, IEnumerable<bool> values)
    {
      List<bool> v = new List<bool>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetBools(pThis, id, _v.Length, _v);
    }
    public bool SetInts(int id, IEnumerable<int> values)
    {
      List<int> v = new List<int>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetInts(pThis, id, _v.Length, _v);
    }
    public bool SetDoubles(int id, IEnumerable<double> values)
    {
      List<double> v = new List<double>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetDoubles(pThis, id, _v.Length, _v);
    }
    public bool SetPoint3ds(int id, IEnumerable<Rhino.Geometry.Point3d> values)
    {
      List<Geometry.Point3d> v = new List<Geometry.Point3d>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetPoints(pThis, id, _v.Length, _v);
    }
    public bool SetVector3ds(int id, IEnumerable<Rhino.Geometry.Vector3d> values)
    {
      List<Geometry.Vector3d> v = new List<Geometry.Vector3d>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetVectors(pThis, id, _v.Length, _v);
    }
    //public bool SetTransorms(int id, IEnumerable<Rhino.Geometry.Transform> values)
    //{
    //  List<Geometry.Transform> v = new List<Geometry.Transform>(values);
    //  var _v = v.ToArray();
    //  IntPtr pThis = NonConstPointer();
    //  return UnsafeNativeMethods.CRhinoHistory_SetXforms(pThis, id, _v.Length, _v);
    //}

    public bool SetColors(int id, IEnumerable<System.Drawing.Color> values)
    {
      List<int> argb = new List<int>();
      foreach (System.Drawing.Color c in values)
        argb.Add(c.ToArgb());
      var _v = argb.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetColors(pThis, id, _v.Length, _v);
    }

    // need ON_ClassArray<CRhinoObjRef>* wrapper
    //public bool SetObjRefs(int id, IEnumerable<ObjRef> values);

    public bool SetGuids(int id, IEnumerable<Guid> values)
    {
      List<Guid> v = new List<Guid>(values);
      var _v = v.ToArray();
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoHistory_SetUuids(pThis, id, _v.Length, _v);
    }

    public bool SetStrings(int id, IEnumerable<string> values)
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      foreach (string v in values)
        UnsafeNativeMethods.ON_StringArray_Append(pStrings, v);
      IntPtr pThis = NonConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoHistory_SetStrings(pThis, id, pStrings);
      UnsafeNativeMethods.ON_StringArray_Delete(pStrings);
      return rc;
    }
    // ON_Geometry* is non-const. I think we can't delete it from under the history record.
    // Don't wrap until we really need it
    //bool SetGeometryValues( int value_id, const ON_SimpleArray<ON_Geometry*> a);

    // PolyEdge not wrapped yet
    //bool SetPolyEdgeValues( CRhinoDoc& doc, int value_id, int count, const class CRhinoPolyEdge* const* polyedges );
  }

  // TODO: Implement ReplayHistoryData in order to support history on commands
  // this is the same as CRhinoHistoryRecord
  public class ReplayHistoryData : IDisposable
  {
    IntPtr m_pConstRhinoHistoryRecord;
    IntPtr m_pObjectPairArray;
    // this should only be constructed in the ReplayHistory callback
    internal ReplayHistoryData(IntPtr pConstRhinoHistoryRecord, IntPtr pObjectPairArray)
    {
      m_pConstRhinoHistoryRecord = pConstRhinoHistoryRecord;
      m_pObjectPairArray = pObjectPairArray;
    }

    public void Dispose()
    {
      m_pConstRhinoHistoryRecord = IntPtr.Zero;
      m_pObjectPairArray = IntPtr.Zero;
    }
  }
}
#endif