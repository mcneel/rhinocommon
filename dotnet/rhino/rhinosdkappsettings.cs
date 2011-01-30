using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Rhino.ApplicationSettings
{
  [System.ComponentModel.Browsable(false)]
  public enum PaintColor : int
  {
    NormalStart = 0,
    NormalEnd = 1,
    NormalBorder = 2,
    HotStart = 3,
    HotEnd = 4,
    HotBorder = 5,
    PressedStart = 6,
    PressedEnd = 7,
    PressedBorder = 8,
    TextEnabled = 9,
    TextDisabled = 10,
    MouseOverControlStart = 11,
    MouseOverControlEnd = 12,
    MouseOverControlBorder = 13,
  }

  //public enum CommandPromptPosition : int
  //{
  //  Top = 0,  // = CRhinoAppAppearanceSettings::command_prompt_top,
  //  Bottom,   // = CRhinoAppAppearanceSettings::command_prompt_bottom,
  //  Floating, // = CRhinoAppAppearanceSettings::command_prompt_floating,
  //  Hidden    // = CRhinoAppAppearanceSettings::command_prompt_hidden
  //}

  public static class AppearanceSettings
  {
    public static string DefaultFontFaceName
    {
      get
      {
        using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoAppearanceSettings_DefaultFontFaceNameGet(pString);
          return sh.ToString();
        }
      }
      set
      {
        UnsafeNativeMethods.CRhinoAppearanceSettings_DefaultFontFaceNameSet(value);
      }
    }

#region Colors
    const int idxDefaultLayerColor = 0;
    const int idxSelectedObjectColor = 1;
    //const int idxSelectedReferenceObjectColor = 2;
    const int idxLockedObjectColor = 3;
    //const int idxLockedReferenceObjectColor = 4;
    const int idxWorldIconXColor = 5;
    const int idxWorldIconYColor = 6;
    const int idxWorldIconZColor = 7;
    const int idxTrackingColor = 8;
    const int idxFeedbackColor = 9;
    const int idxDefaultObjectColor = 10;
    const int idxViewportBackgroundColor = 11;
    const int idxFrameBackgroundColor = 12;
    const int idxCommandPromptTextColor = 13;
    const int idxCommandPromptHypertextColor = 14;
    const int idxCommandPromptBackgroundColor = 15;
    const int idxCrosshairColor = 16;
    const int idxPageviewPaperColor = 17;
    const int idxCurrentLayerBackgroundColor = 18;

    static Color GetColor(int which)
    {
      int argb = UnsafeNativeMethods.RhAppearanceSettings_GetSetColor(which, false, 0);
      return Color.FromArgb(argb);
    }
    static void SetColor(int which, Color c)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhAppearanceSettings_GetSetColor(which, true, argb);
    }
#if USING_V5_SDK
    [System.ComponentModel.Browsable(false)]
    public static bool UsingNewSchoolColors
    {
      get
      {
        return UnsafeNativeMethods.RhColors_UsingNewSchool();
      }
    }

    [System.ComponentModel.Browsable(false)]
    public static Color GetPaintColor(PaintColor c)
    {
      int argb = UnsafeNativeMethods.RhColors_GetColor((int)c);
      return Color.FromArgb(argb);
    }

    [System.ComponentModel.Browsable(false)]
    public static void SetPaintColor(PaintColor whichColor, Color c)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhColors_SetColor((int)whichColor, argb);
    }

    [System.ComponentModel.Browsable(false)]
    public static bool UsePaintColors
    {
      get
      {
        return UnsafeNativeMethods.RhColors_UsingNewSchool();
      }
      set
      {
        UnsafeNativeMethods.RhColors_SetUsingNewSchool(value);
      }
    }
#endif


    public static Color DefaultLayerColor
    {
      get { return GetColor(idxDefaultLayerColor); }
      set { SetColor(idxDefaultLayerColor, value); }
    }
    
    ///<summary>
    ///The color used to draw selected objects.
    ///The default is yellow, but this can be customized by the user.
    ///</summary>
    public static Color SelectedObjectColor
    {
      get { return GetColor(idxSelectedObjectColor); }
      set { SetColor(idxSelectedObjectColor, value); }
    }

    //public static Color SelectedReferenceObjectColor
    //{
    //  get { return GetColor(idxSelectedReferenceObjectColor); }
    //  set { SetColor(idxSelectedReferenceObjectColor, value); }
    //}

    ///<summary>color used to draw locked objects.</summary>
    public static Color LockedObjectColor
    {
      get { return GetColor(idxLockedObjectColor); }
      set { SetColor(idxLockedObjectColor, value); }
    }

    //public static Color LockedRefereceObjectColor
    //{
    //  get { return GetColor(idxLockedReferenceObjectColor); }
    //  set { SetColor(idxLockedReferenceObjectColor, value); }
    //}

    public static Color WorldCoordIconXAxisColor
    {
      get { return GetColor(idxWorldIconXColor); }
      set { SetColor(idxWorldIconXColor, value); }
    }
    public static Color WorldCoordIconYAxisColor
    {
      get { return GetColor(idxWorldIconYColor); }
      set { SetColor(idxWorldIconYColor, value); }
    }
    public static Color WorldCoordIconZAxisColor
    {
      get { return GetColor(idxWorldIconZColor); }
      set { SetColor(idxWorldIconZColor, value); }
    }

    public static Color TrackingColor
    {
      get { return GetColor(idxTrackingColor); }
      set { SetColor(idxTrackingColor, value); }
    }

    public static Color FeedbackColor
    {
      get { return GetColor(idxFeedbackColor); }
      set { SetColor(idxFeedbackColor, value); }
    }

    public static Color DefaultObjectColor
    {
      get { return GetColor(idxDefaultObjectColor); }
      set { SetColor(idxDefaultObjectColor, value); }
    }

    public static Color ViewportBackgroundColor
    {
      get { return GetColor(idxViewportBackgroundColor); }
      set { SetColor(idxViewportBackgroundColor, value); }
    }

    public static Color FrameBackgroundColor
    {
      get { return GetColor(idxFrameBackgroundColor); }
      set { SetColor(idxFrameBackgroundColor, value); }
    }

    public static Color CommandPromptTextColor
    {
      get { return GetColor(idxCommandPromptTextColor); }
      set { SetColor(idxCommandPromptTextColor, value); }
    }

    public static Color CommandPromptHypertextColor
    {
      get { return GetColor(idxCommandPromptHypertextColor); }
      set { SetColor(idxCommandPromptHypertextColor, value); }
    }

    public static Color CommandPromptBackgroundColor
    {
      get { return GetColor(idxCommandPromptBackgroundColor); }
      set { SetColor(idxCommandPromptBackgroundColor, value); }
    }

    public static Color CrosshairColor
    {
      get { return GetColor(idxCrosshairColor); }
      set { SetColor(idxCrosshairColor, value); }
    }

    ///<summary>
    ///CRhinoPageView paper background. A rectangle is drawn into the background
    ///of page views to represent the printed area. The alpha portion of the color
    ///is used to draw the paper blended into the background
    ///</summary>
    public static Color PageviewPaperColor
    {
      get { return GetColor(idxPageviewPaperColor); }
      set { SetColor(idxPageviewPaperColor, value); }
    }

    ///<summary>
    ///color used by the layer manager dialog as the background color for the current layer.
    ///</summary>
    public static Color CurrentLayerBackgroundColor
    {
      get { return GetColor(idxCurrentLayerBackgroundColor); }
      set { SetColor(idxCurrentLayerBackgroundColor, value); }
    }
#endregion

    /*
    ///<summary>length of world coordinate sprite axis in pixels</summary>
    public static property int WorldCoordIconAxisSize{ int get(); void set(int); }
    ///<summary>&quot;radius&quot; of letter in pixels</summary>
    public static property int WorldCoordIconLabelSize{ int get(); void set(int); }
    ///<summary>true to move axis letters as sprite rotates</summary>
    public static property bool WorldCoordIconMoveLabels{ bool get(); void set(bool); }

    ///<summary>length of direction arrow shaft icon in pixels</summary>
    public static property int DirectionArrowIconShaftSize{ int get(); void set(int); }
    ///<summary>length of direction arrowhead icon in pixels</summary>
    public static property int DirectionArrowIconHeadSize{ int get(); void set(int); }

    ///<summary>
    ///3d "flag" text (like the Dot command) can either be depth 
    ///tested or shown on top. true means on top.
    ///</summary>
    public static property bool FlagTextOnTop{ bool get(); void set(bool); }

    public static property System::String^ CommandPromptFontName{System::String^ get(); void set(System::String^);}
    public static property int CommandPromptFontHeight{ int get(); void set(int); }
    public static property int CommandPromptHeightInLines{ int get(); void set(int); }
    
    public static property PromptPosition CommandPromptPosition{ PromptPosition get(); void set(PromptPosition); }

    public static property bool StatusBarVisible{ bool get(); void set(bool); }
    public static property bool OsnapDialogVisible{ bool get(); void set(bool); }
    */

    const int idxEchoPromptsToHistoryWindow = 0;
    const int idxEchoCommandsToHistoryWindow = 1;
    const int idxFullPathInTitleBar = 2;
    const int idxCrosshairsVisible = 3;


    public static bool EchoPromptsToHistoryWindow
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoPromptsToHistoryWindow); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxEchoPromptsToHistoryWindow, value); }
    }
    public static bool EchoCommandsToHistoryWindow
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoCommandsToHistoryWindow); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxEchoCommandsToHistoryWindow, value); }
    }
    public static bool ShowFullPathInTitleBar
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxFullPathInTitleBar); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxFullPathInTitleBar, value); }
    }
    public static bool ShowCrosshairs
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxCrosshairsVisible); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxCrosshairsVisible, value); }
    }
    /*
    public static property bool ViewportTitleVisible{ bool get(); void set(bool); }
    public static property bool MainWindowTitleVisible{ bool get(); void set(bool); }
    public static property bool MenuVisible{ bool get(); void set(bool); }
    */


    public static int LanguageIdentifier
    {
      get
      {
        uint rc = UnsafeNativeMethods.RhAppearanceSettings_GetSetUINT(0, false, 0);
        return (int)rc;
      }
      set
      {
        UnsafeNativeMethods.RhAppearanceSettings_GetSetUINT(0, true, (uint)value);
      }
    }
    public static int PreviousLanguageIdentifier
    {
      get
      {
        uint rc = UnsafeNativeMethods.RhAppearanceSettings_GetSetUINT(1, false, 0);
        return (int)rc;
      }
      set
      {
        UnsafeNativeMethods.RhAppearanceSettings_GetSetUINT(1, true, (uint)value); 
      }
    }
  }

  
  public static class CommandAliasList
  {
    ///<summary>Returns the number of command alias in Rhino.</summary>
    public static int Count
    {
      get
      {
        return UnsafeNativeMethods.RhCommandAliasList_Count();
      }
    }

    ///<summary>Returns a list of command alias names.</summary>
    public static string[] GetNames()
    {
      int count = UnsafeNativeMethods.RhCommandAliasList_Count();
      string[] rc = new string[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr = UnsafeNativeMethods.RhCommandAliasList_Item(i);
        if (ptr != IntPtr.Zero)
          rc[i] = Marshal.PtrToStringUni(ptr);
      }
      return rc;
    }

    ///<summary>Remove all aliases from the list</summary>
    public static void DestroyList()
    {
      UnsafeNativeMethods.RhCommandAliasList_DestroyList();
    }

    ///<summary>Returns the macro of a command alias.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    public static string GetMacro(string alias)
    {
      IntPtr rc = UnsafeNativeMethods.RhCommandAliasList_GetMacro(alias);
      if (rc == IntPtr.Zero)
        return null;
      return Marshal.PtrToStringUni(rc);
    }

    ///<summary>Modifies the macro of a command alias.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<param name='macro'>[in] The new command macro to run when the alias is executed.</param>
    ///<returns>true if successful</returns>
    public static bool SetMacro(string alias, string macro)
    {
      return UnsafeNativeMethods.RhCommandAliasList_SetMacro(alias, macro);
    }

    ///<summary>Adds a new command alias to Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<param name='macro'>[in] The command macro to run when the alias is executed.</param>
    ///<returns>true if successful</returns>
    public static bool Add(string alias, string macro)
    {
      return UnsafeNativeMethods.RhCommandAliasList_Add(alias, macro);
    }

    ///<summary>Deletes an existing command alias from Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<returns>true if successful</returns>
    public static bool Delete(string alias)
    {
      return UnsafeNativeMethods.RhCommandAliasList_Delete(alias);
    }

    ///<summary>Verifies that a command alias exists in Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<returns>true if the alias exists</returns>
    public static bool IsAlias(string alias)
    {
      return UnsafeNativeMethods.RhCommandAliasList_IsAlias(alias);
    }
  }
  
  public static class NeverRepeatList
  {
    ///<summary>
    ///Only use the m_dont_repeat_list if somebody modifies it via CRhinoAppSettings::SetDontRepeatCommands()
    ///
    ///A return value of true means CRhinoCommand don&apos;t repeat flags will be ignored and the m_dont_repeat_list
    ///will be used instead.  False means the individual CRhinoCommands will determine if they are repeatable.
    ///</summary>
    public static bool UseNeverRepeatList()
    {
      return UnsafeNativeMethods.RhDontRepeatList_UseList();
    }

    ///<summary>put command name tokens in m_dont_repeat_list.</summary>
    ///<returns>Number of items added to m_dont_repeat_list.</returns>
    public static int SetList(string[] commandNames)
    {
      if (commandNames == null || commandNames.Length < 1)
        return UnsafeNativeMethods.RhDontRepeatList_SetList(null);

      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      for (int i = 0; i < commandNames.Length; i++)
      {
        if (i > 0)
          sb.Append(' ');
        sb.Append(commandNames[i]);
      }
      return UnsafeNativeMethods.RhDontRepeatList_SetList(sb.ToString());
    }

    ///<summary>Convert m_dont_repeat_list to space delimited string</summary>
    public static string CommandNames
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.RhDontRepeatList_GetList();
        if (rc == IntPtr.Zero)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
    }
  }

  
  public static class EdgeAnalysisSettings
  {
    ///<summary>color used to enhance display edges in commands like ShowEdges and ShowNakedEdges.</summary>
    public static Color ShowEdgeColor
    {
      get
      {
        int argb = UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdgeColor(false, 0);
        return Color.FromArgb(argb);
      }
      set
      {
        int argb = value.ToArgb();
        UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdgeColor(true, argb);
      }
    }

    ///<summary>0 = all, 1 = naked, 2 = non-manifold</summary>
    public static int ShowEdges
    {
      get
      {
        return UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdges(false, 0);
      }
      set
      {
        UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdges(true, value);
      }
    }
  }


  public enum ClipboardState : int
  {
    ///<summary>Always keep clipboard data, regardless of size and never prompt the user</summary>
    KeepData = 0, //CRhinoAppFileSettings::keep_clipboard_data=0
    ///<summary>Always delete clipboard data, regardless of size and never prompt the user</summary>
    DeleteData,  // = CRhinoAppFileSettings::delete_clipboard_data,
    ///<summary>Prompt user when clipboard memory is large</summary>
    PromptWhenBig //= CRhinoAppFileSettings::prompt_user_when_clipboard_big
  }

  public static class FileSettings
  {
    ///<summary>
    ///Adds a new imagePath to Rhino&apos;s search imagePath list.
    ///See "Options Files settings" in the Rhino help file for more details.
    ///</summary>
    ///<param name='folder'>[in] The valid folder, or imagePath, to add.</param>
    ///<param name='index'>
    ///[in] A zero-based position index in the search imagePath list to insert the string.
    ///If -1, the imagePath will be appended to the end of the list.
    ///</param>
    ///<returns>
    ///the index where the item was inserted if success
    ///-1 on failure
    ///</returns>
    public static int AddSearchPath(string folder, int index)
    {
      return UnsafeNativeMethods.RhDirectoryManager_AddSearchPath(folder, index);
    }

    ///<summary>
    ///Removes an existing imagePath from Rhino's search imagePath list.
    ///See "Options Files settings" in the Rhino help file for more details.
    ///</summary>
    ///<param name='folder'>[in] The valid folder, or imagePath, to remove.</param>
    ///<returns>true or false indicating success or failure.</returns>
    public static bool DeleteSearchPath(string folder)
    {
      return UnsafeNativeMethods.RhDirectoryManager_DeleteSearchPath(folder);
    }

    /// <summary>
    /// Searches for a file using Rhino's search imagePath. Rhino will look for a file in the following locations:
    /// 1. The current document's folder.
    /// 2. Folder's specified in Options dialog, File tab.
    /// 3. Rhino's System folders
    /// </summary>
    /// <param name="fileName">short file name to search for</param>
    /// <returns> full imagePath on success; null on error</returns>
    public static string FindFile(string fileName)
    {
      IntPtr rc = UnsafeNativeMethods.RhDirectoryManager_FindFile(fileName);
      if (IntPtr.Zero == rc)
        return null;
      return Marshal.PtrToStringUni(rc);
    }

    public static int SearchPathCount
    {
      get
      {
        return UnsafeNativeMethods.RhDirectoryManager_SearchPathCount();
      }
    }

    /// <summary>
    /// Returns all of the imagePath items in Rhino's search imagePath list. See "Options Files settings" in the Rhino help file for more details.
    /// </summary>
    public static string[] GetSearchPaths()
    {
      int count = SearchPathCount;
      string[] rc = new string[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr = UnsafeNativeMethods.RhDirectoryManager_SearchPath(i);
        if (ptr != IntPtr.Zero)
        {
          rc[i] = Marshal.PtrToStringUni(ptr);
        }
      }
      return rc;
    }

    /// <summary>
    /// Returns or sets Rhino's working directory, or folder.
    /// The working folder is the default folder for all file operations.
    /// </summary>
    public static string WorkingFolder
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.RhDirectoryManager_WorkingFolder(null);
        if (IntPtr.Zero == rc)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
      set
      {
        UnsafeNativeMethods.RhDirectoryManager_WorkingFolder(value);
      }
    }

    ///<summary>Returns or sets the location of Rhino's template files.</summary>
    public static string TemplateFolder
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.RhFileSettings_FileGetSet(null, 0);
        if (IntPtr.Zero == rc)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
      set
      {
        UnsafeNativeMethods.RhFileSettings_FileGetSet(value, 0);
      }
    }

    ///<summary>Returns or sets the location of Rhino&apos;s template file.</summary>
    public static string TemplateFile
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.RhFileSettings_FileGetSet(null, 1);
        if (IntPtr.Zero == rc)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
      set
      {
        UnsafeNativeMethods.RhFileSettings_FileGetSet(value, 1);
      }
    }

    ///<summary>the file name used by Rhino&apos;s automatic file saving mechanism.</summary>
    public static string AutoSaveFile
    {
      get
      {
        IntPtr rc = UnsafeNativeMethods.RhFileSettings_FileGetSet(null, 2);
        if (IntPtr.Zero == rc)
          return null;
        return Marshal.PtrToStringUni(rc);
      }
      set
      {
        UnsafeNativeMethods.RhFileSettings_FileGetSet(value, 2);
      }
    }

    ///<summary>how often the document will be saved when Rhino&apos;s automatic file saving mechanism is enabled</summary>
    public static System.TimeSpan AutoSaveInterval
    {
      get
      {
        int minutes = UnsafeNativeMethods.RhFileSettings_AutosaveInterval(-1);
        return System.TimeSpan.FromMinutes(minutes);
      }
      set
      {
        double minutes = value.TotalMinutes;
        if (minutes > -10.0 && minutes < int.MaxValue)
          UnsafeNativeMethods.RhFileSettings_AutosaveInterval((int)minutes);
      }
    }

    ///<summary>Enables or disables Rhino&apos;s automatic file saving mechanism.</summary>
    public static bool AutoSaveEnabled
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(0, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(0, true, value); }
    }

    ///<summary>save render and display meshes in autosave file</summary>
    public static bool AutoSaveMeshes
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(1, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(1, true, value); }
    }

    ///<summary>Input list of commands that force AutoSave prior to running</summary>
    public static string[] AutoSaveBeforeCommands()
    {
      IntPtr rc = UnsafeNativeMethods.RhFileSettings_AutosaveBeforeCommands();
      if (IntPtr.Zero == rc)
        return null;
      string s = Marshal.PtrToStringUni(rc);
      return s==null ? null : s.Split(new char[] { ' ' });
    }

    ///<summary>Set list of commands that force AutoSave prior to running</summary>
    public static void SetAutoSaveBeforeCommands(string[] commands)
    {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();
      if (commands != null)
      {
        for (int i = 0; i < commands.Length; i++)
        {
          if (i > 0)
            sb.Append(' ');
          sb.Append(commands[i]);
        }
      }
      UnsafeNativeMethods.RhFileSettings_SetAutosaveBeforeCommands(sb.ToString());
    }

    ///<summary>true for users who consider view changes a document change</summary>
    public static bool SaveViewChanges
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(2, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(2, true, value); }
    }

    ///<summary>Ensure that only one person at a time can have a file open for saving</summary>
    public static bool FileLockingEnabled
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(3, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(3, true, value); }
    }

    ///<summary>Display information dialog which identifies computer file is open on</summary>
    public static bool FileLockingOpenWarning
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(4, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(4, true, value); }
    }

    ///<summary>
    ///Copy both objects to the clipboard in both the current and previous Rhino clipboard formats.  This
    ///means you will double the size of what is saved in the clipboard but will be able to copy from
    ///the current to the previous version using the clipboard.
    ///</summary>
    public static bool ClipboardCopyToPreviousRhinoVersion
    {
      get { return UnsafeNativeMethods.RhFileSettings_BoolProperty(5, false, false); }
      set { UnsafeNativeMethods.RhFileSettings_BoolProperty(5, true, value); }
    }

    public static ClipboardState ClipboardOnExit
    {
      get
      {
        int rc = UnsafeNativeMethods.RhFileSettings_ClipboardOnExit(false, 0);
        return (ClipboardState)rc;
      }
      set
      {
        UnsafeNativeMethods.RhFileSettings_ClipboardOnExit(true, (int)value);
      }
    }

    /// <summary>Returns directory where the main Rhino executable is located</summary>
    public static System.IO.DirectoryInfo ExecutableFolder
    {
      get
      {
        using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxExecutableFolder, pString);
          string rc = sh.ToString();
          if (!System.IO.Directory.Exists(rc))
            return null;
          return new System.IO.DirectoryInfo(rc);
        }
      }
    }

    /// <summary>Returns Rhino's installation folder</summary>
    public static System.IO.DirectoryInfo InstallFolder
    {
      get
      {
        using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxInstallFolder, pString);
          string rc = sh.ToString();
          if (!System.IO.Directory.Exists(rc))
            return null;
          return new System.IO.DirectoryInfo(rc);
        }
      }
    }

    public static string HelpFilePath
    {
      get
      {
        using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxHelpFilePath, pString);
          return sh.ToString();
        }
      }
    }

    public static string DefaultRuiFile
    {
      get
      {
        using (Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxDefaultRuiFile, pString);
          return sh.ToString();
        }
      }
    }
  }

  
  /// <summary>construction plane grid line properties</summary>
  public static class GridSettings
  {
    //int          m_thick_line_width; // 1 or 2
    //int          m_axis_line_width;  // 1 or 2
    //unsigned int m_line_stipple_pattern; 
    //bool         m_show_zaxis;
    const int idxThinLineColor = 0;
    const int idxThickLineColor = 1;
    const int idxXAxisColor = 2;
    const int idxYAxisColor = 3;
    const int idxZAxisColor = 4;

    static Color GetColor(int which)
    {
      int argb = UnsafeNativeMethods.RhGridSettings_GetSetColor(which, false, 0);
      return Color.FromArgb(argb);
    }
    static void SetColor(int which, Color c)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhGridSettings_GetSetColor(which, true, argb);
    }

    public static Color ThinLineColor
    {
      get { return GetColor(idxThinLineColor); }
      set { SetColor(idxThinLineColor, value); }
    }

    public static Color ThickLineColor
    {
      get { return GetColor(idxThickLineColor); }
      set { SetColor(idxThickLineColor, value); }
    }

    public static Color XAxisLineColor
    {
      get { return GetColor(idxXAxisColor); }
      set { SetColor(idxXAxisColor, value); }
    }
    public static Color YAxisLineColor
    {
      get { return GetColor(idxYAxisColor); }
      set { SetColor(idxYAxisColor, value); }
    }
    public static Color ZAxisLineColor
    {
      get { return GetColor(idxZAxisColor); }
      set { SetColor(idxZAxisColor, value); }
    }
  }

  public enum CursorMode : int
  {
    None = 0,       // = CRhinoAppModelAidSettings::no_osnap_cursor,
    BlackOnWhite, // = CRhinoAppModelAidSettings::black_on_white_osnap_cursor,
    WhiteOnBlack  // = CRhinoAppModelAidSettings::white_on_black_osnap_cursor
  };

  [FlagsAttribute]
  public enum OsnapModes : int
  {
    None = 0,
    Near = 2,
    Focus = 8,
    Center = 0x20,
    Vertex = 0x40,
    Knot = 0x80,
    Quadrant = 0x200,
    Midpoint = 0x800,
    Intersection = 0x2000,
    End = 0x20000,
    Perpendicular = 0x80000,
    Tangent = 0x200000,
    Point =  0x8000000,
    //All = 0xFFFFFFFF
  };

  public enum PointDisplayMode : int
  {
    ///<summary>points are displayed in world coordinates</summary>
    WorldPoint = 0, // = CRhinoAppModelAidSettings::world_point,
    ///<summary>points are displayed in cplane coordinates</summary>
    CplanePoint     // = CRhinoAppModelAidSettings::cplane_point
  };

  public static class ModelAidSettings
  {
    static bool GetBool(int which) { return UnsafeNativeMethods.RhModelAidSettings_GetSetBool(which, false, false); }
    static void SetBool(int which, bool b) { UnsafeNativeMethods.RhModelAidSettings_GetSetBool(which, true, b); }
    const int idxGridSnap = 0;
    const int idxOrtho = 1;
    const int idxPlanar = 2;
    const int idxProjectSnapToCPlane = 3;
    const int idxUseHorizontalDialog = 4;
    const int idxExtendTrimLines = 5;
    const int idxExtendToApparentIntersection = 6;
    const int idxAltPlusArrow = 7;
    const int idxDisplayControlPolygon = 8;
    const int idxHighlightControlPolygon = 9;
    const int idxOsnap = 10;
    const int idxSnapToLocked = 11;
    const int idxUniversalConstructionPlaneMode = 12;

    ///<summary>Enables or disables Rhino's grid snap modeling aid.</summary>
    public static bool GridSnap
    {
      get { return GetBool(idxGridSnap); }
      set { SetBool(idxGridSnap, value); }
    }
    ///<summary>Enables or disables Rhino&apos;s ortho modeling aid.</summary>
    public static bool Ortho
    {
      get { return GetBool(idxOrtho); }
      set { SetBool(idxOrtho, value); }
    }
    public static bool Planar
    {
      get { return GetBool(idxPlanar); }
      set { SetBool(idxPlanar, value); }
    }
    public static bool ProjectSnapToCPlane
    {
      get { return GetBool(idxProjectSnapToCPlane); }
      set { SetBool(idxProjectSnapToCPlane, value); }
    }
    public static bool UseHorizontalDialog
    {
      get { return GetBool(idxUseHorizontalDialog); }
      set { SetBool(idxUseHorizontalDialog, value); }
    }
    public static bool ExtendTrimLines
    {
      get { return GetBool(idxExtendTrimLines); }
      set { SetBool(idxExtendTrimLines, value); }
    }
    public static bool ExtendToApparentIntersection
    {
      get { return GetBool(idxExtendToApparentIntersection); }
      set { SetBool(idxExtendToApparentIntersection, value); }
    }
    ///<summary>true mean Alt+arrow is used for nudging.</summary>
    public static bool AltPlusArrow
    {
      get { return GetBool(idxAltPlusArrow); }
      set { SetBool(idxAltPlusArrow, value); }
    }
    public static bool DisplayControlPolygon
    {
      get { return GetBool(idxDisplayControlPolygon); }
      set { SetBool(idxDisplayControlPolygon, value); }
    }
    public static bool HighlightControlPolygon
    {
      get { return GetBool(idxHighlightControlPolygon); }
      set { SetBool(idxHighlightControlPolygon, value); }
    }
    ///<summary>Enables or disables Rhino&apos;s object snap modeling aid.</summary>
    public static bool Osnap
    {
      get
      {
        // The osnap toggle in C++ is m_suspend_osnap which is a double negative
        // Flip value passed to and returned from C++
        bool rc = GetBool(idxOsnap);
        return !rc;
      }
      set
      {
        value = !value;
        SetBool(idxOsnap, value);
      }
    }
    public static bool SnapToLocked
    {
      get { return GetBool(idxSnapToLocked); }
      set { SetBool(idxSnapToLocked, value); }
    }
    public static bool UniversalConstructionPlaneMode
    {
      get { return GetBool(idxUniversalConstructionPlaneMode); }
      set { SetBool(idxUniversalConstructionPlaneMode, value); }
    }


    static double GetDouble(int which) { return UnsafeNativeMethods.RhModelAidSettings_GetSetDouble(which, false, 0); }
    static void SetDouble(int which, double d) { UnsafeNativeMethods.RhModelAidSettings_GetSetDouble(which, true, d); }
    const int idxOrthoAngle = 0;
    const int idxNudgeKeyStep = 1;
    const int idxCtrlNudgeKeyStep = 2;
    const int idxShiftNudgeKeyStep = 3;

    public static double OrthoAngle
    {
      get { return GetDouble(idxOrthoAngle); }
      set { SetDouble(idxOrthoAngle, value); }
    }
    ///<summary>Enables or disables Rhino&apos;s object snap projection.</summary>
    public static double NudgeKeyStep
    {
      get { return GetDouble(idxNudgeKeyStep); }
      set { SetDouble(idxNudgeKeyStep, value); }
    }
    public static double CtrlNudgeKeyStep
    {
      get { return GetDouble(idxCtrlNudgeKeyStep); }
      set { SetDouble(idxCtrlNudgeKeyStep, value); }
    }
    public static double ShiftNudgeKeyStep
    {
      get { return GetDouble(idxShiftNudgeKeyStep); }
      set { SetDouble(idxShiftNudgeKeyStep, value); }
    }

    static int GetInt(int which) { return UnsafeNativeMethods.RhModelAidSettings_GetSetInt(which, false, 0); }
    static void SetInt(int which, int i) { UnsafeNativeMethods.RhModelAidSettings_GetSetInt(which, true, i); }
    const int idxOsnapPickboxRadius = 0;
    const int idxNudgeMode = 1;
    const int idxControlPolygonDisplayDensity = 2;
    const int idxOSnapCursorMode = 3;
    const int idxOSnapModes = 4;
    const int idxMousePickboxRadius = 5;
    const int idxPointDisplay = 6;

    ///<summary>Enables or disables Rhino's planar modeling aid.</summary>
    public static int OsnapPickboxRadius
    {
      get { return GetInt(idxOsnapPickboxRadius); }
      set { SetInt(idxOsnapPickboxRadius, value); }
    }
    ///<summary>0 = world, 1 = cplane, 2 = view, 3 = uvn, -1 = not set</summary>
    public static int NudgeMode
    {
      get { return GetInt(idxNudgeMode); }
      set { SetInt(idxNudgeMode, value); }
    }
    public static int ControlPolygonDisplayDensity
    {
      get { return GetInt(idxControlPolygonDisplayDensity); }
      set { SetInt(idxControlPolygonDisplayDensity, value); }
    }

    public static CursorMode OsnapCursorMode
    {
      get
      {
        int mode = GetInt(idxOSnapCursorMode);
        return (CursorMode)mode;
      }
      set
      {
        int mode = (int)value;
        SetInt(idxOSnapCursorMode, mode);
      }
    }
    ///<summary>
    ///Returns or sets Rhino's current object snap mode.
    ///The mode is a bitwise value based on the OsnapModes enumeration.
    ///</summary>
    public static OsnapModes OsnapModes
    {
      get
      {
        int rc = GetInt(idxOSnapModes);
        return (OsnapModes)rc;
      }
      set
      {
        SetInt(idxOSnapModes, (int)value);
      }
    }
    ///<summary>radius of mouse pick box in pixels</summary>
    public static int MousePickboxRadius
    {
      get { return GetInt(idxMousePickboxRadius); }
      set { SetInt(idxMousePickboxRadius, value); }
    }

    public static PointDisplayMode PointDisplay
    {
      get
      {
        int mode = GetInt(idxPointDisplay);
        return (PointDisplayMode)mode;
      }
      set
      {
        int mode = (int)value;
        SetInt(idxPointDisplay, mode);
      }
    }
  }

  public static class ViewSettings
  {
    //double items
    const int idxPanScreenFraction = 0;
    const int idxZoomScale = 1;

    // bool items
    const int idxPanReverseKeyboardAction = 0;
    const int idxAlwaysPanParallelViews = 1;
    const int idxRotateReverseKeyboard = 2;
    const int idxRotateToView = 3;
    const int idxDefinedViewSetCPlane = 4;
    const int idxDefinedViewSetProjection = 5;
    const int idxSingleClickMaximize = 6;
    const int idxLinkedViewports = 7;

    // int items
    const int idxRotateCircleIncrement = 0;

    static double GetDouble(int which) { return UnsafeNativeMethods.RhViewSettings_GetSetDouble(which, false, 0); }
    static void SetDouble(int which, double d) { UnsafeNativeMethods.RhViewSettings_GetSetDouble(which, true, d); }
    static bool GetBool(int which) { return UnsafeNativeMethods.RhViewSettings_GetSetBool(which, false, false); }
    static void SetBool(int which, bool b) { UnsafeNativeMethods.RhViewSettings_GetSetBool(which, true, b); }
    

    public static double PanScreenFraction
    {
      get { return GetDouble(idxPanScreenFraction); }
      set { SetDouble(idxPanScreenFraction, value); }
    }

    public static bool PanReverseKeyboardAction
    {
      get { return GetBool(idxPanReverseKeyboardAction); }
      set { SetBool(idxPanReverseKeyboardAction, value); }
    }

    public static bool AlwaysPanParallelViews
    {
      get { return GetBool(idxAlwaysPanParallelViews); }
      set { SetBool(idxAlwaysPanParallelViews, value); }
    }

    public static double ZoomScale
    {
      get { return GetDouble(idxZoomScale); }
      set { SetDouble(idxZoomScale, value); }
    }

    public static int RotateCircleIncrement
    {
      get
      {
        return UnsafeNativeMethods.RhViewSettings_GetSetInt(idxRotateCircleIncrement, false, 0);
      }
      set
      {
        UnsafeNativeMethods.RhViewSettings_GetSetInt(idxRotateCircleIncrement, true, value);
      }
    }

    public static bool RotateReverseKeyboard
    {
      get { return GetBool(idxRotateReverseKeyboard); }
      set { SetBool(idxRotateReverseKeyboard, value); }
    }

    /// <summary>
    /// false means around world axes
    /// </summary>
    public static bool RotateToView
    {
      get { return GetBool(idxRotateToView); }
      set { SetBool(idxRotateToView, value); }
    }

    public static bool DefinedViewSetCPlane
    {
      get { return GetBool(idxDefinedViewSetCPlane); }
      set { SetBool(idxDefinedViewSetCPlane, value); }
    }

    public static bool DefinedViewSetProjection
    {
      get { return GetBool(idxDefinedViewSetProjection); }
      set { SetBool(idxDefinedViewSetProjection, value); }
    }

    public static bool SingleClickMaximize
    {
      get { return GetBool(idxSingleClickMaximize); }
      set { SetBool(idxSingleClickMaximize, value); }
    }

    public static bool LinkedViewports
    {
      get { return GetBool(idxLinkedViewports); }
      set { SetBool(idxLinkedViewports, value); }
    }
  }

  public static class SmartTrackSettings
  {
    const int idxUseSmartTrack = 0;
    // skipping the following until we can come up with good
    // descriptions of what each does
    //BOOL m_bDottedLines;
    //BOOL m_bSmartOrtho;
    //BOOL m_bMarkerSmartPoint;
    //BOOL m_bSmartTangents;
    //BOOL m_bSmartSuppress;
    //BOOL m_bStrongOrtho;
    //BOOL m_bSemiPermanentPoints;
    //BOOL m_bShowMultipleTypes;
    //BOOL m_bParallels;
    //BOOL m_bSmartBasePoint;

    static bool GetBool(int which) { return UnsafeNativeMethods.RhSmartTrackSettings_GetSetBool(which, false, false); }
    static void SetBool(int which, bool b) { UnsafeNativeMethods.RhSmartTrackSettings_GetSetBool(which, true, b); }

    public static bool UseSmartTrack
    {
      get { return GetBool(idxUseSmartTrack); }
      set { SetBool(idxUseSmartTrack, value); }
    }

    // skipping the following until we can come up with good
    // descriptions of what each does
    //int m_smartpoint_wait_ms;
    //int m_max_smart_points;

    const int idxLineColor = 0;
    const int idxTanPerpLineColor = 1;
    const int idxPointColor = 2;
    const int idxActivePointColor = 3;

    static Color GetColor(int which)
    {
      int argb = UnsafeNativeMethods.RhSmartTrackSettings_GetSetColor(which, false, 0);
      return Color.FromArgb(argb);
    }
    static void SetColor(int which, Color c)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhSmartTrackSettings_GetSetColor(which, true, argb);
    }

    public static Color LineColor
    {
      get { return GetColor(idxLineColor); }
      set { SetColor(idxLineColor, value); }
    }
    public static Color TanPerpLineColor
    {
      get { return GetColor(idxTanPerpLineColor); }
      set { SetColor(idxTanPerpLineColor, value); }
    }
    public static Color PointColor
    {
      get { return GetColor(idxPointColor); }
      set { SetColor(idxPointColor, value); }
    }
    public static Color ActivePointColor
    {
      get { return GetColor(idxActivePointColor); }
      set { SetColor(idxActivePointColor, value); }
    }
  }
}
