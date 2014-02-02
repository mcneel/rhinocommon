using System;
using System.Runtime.InteropServices;
using System.Drawing;
using Rhino.Runtime.InteropWrappers;

#if RHINO_SDK
namespace Rhino.ApplicationSettings
{
  /// <summary>
  /// Contains enumerated constant values to represent logical colors associated with elements of the user interface.
  /// </summary>
  public enum PaintColor : int
  {
    /// <summary>Gradient start for active toolbar tab and non-client area of Rhino.</summary>
    NormalStart = 0,
    /// <summary>Gradient end for active toolbar tab and non-client area of Rhino.</summary>
    NormalEnd = 1,
    /// <summary>Edge color used for grippers, toolbar border, resize bars, status bar pane borders.</summary>
    NormalBorder = 2,
    /// <summary>Gradient start for inactive toolbar tab.</summary>
    HotStart = 3,
    /// <summary>Gradient end for inactive toolbar tab.</summary>
    HotEnd = 4,
    /// <summary>Inactive toolbar tab border.</summary>
    HotBorder = 5,
    /// <summary>Pressed gradient start.</summary>
    PressedStart = 6,
    /// <summary>Pressed gradient end.</summary>
    PressedEnd = 7,
    /// <summary>Pressed border.</summary>
    PressedBorder = 8,
    /// <summary>Toolbar tab text and status bar text.</summary>
    TextEnabled = 9,
    /// <summary>Disabled text color.</summary>
    TextDisabled = 10,
    /// <summary>Color for hovering gradient start.</summary>
    MouseOverControlStart = 11,
    /// <summary>Color for hovering gradient end.</summary>
    MouseOverControlEnd = 12,
    /// <summary>Color for hovering border.</summary>
    MouseOverControlBorder = 13,
  }

  //public enum CommandPromptPosition : int
  //{
  //  Top = 0,  // = CRhinoAppAppearanceSettings::command_prompt_top,
  //  Bottom,   // = CRhinoAppAppearanceSettings::command_prompt_bottom,
  //  Floating, // = CRhinoAppAppearanceSettings::command_prompt_floating,
  //  Hidden    // = CRhinoAppAppearanceSettings::command_prompt_hidden
  //}

  /// <summary>Represents a snapshot of the values in <see cref="AppearanceSettings"/>.</summary>
  public class AppearanceSettingsState
  {
    internal AppearanceSettingsState(){}

    /// <summary>
    /// Gets or sets the name of the default font face.
    /// </summary>
    public string DefaultFontFaceName { get; set; }

    /// <summary>
    /// Gets or sets the default layer color.
    /// </summary>
    public Color DefaultLayerColor { get; set; }

    ///<summary>
    ///The color used to draw selected objects.
    ///The default is yellow, but this can be customized by the user.
    ///</summary>
    public Color SelectedObjectColor { get; set; }

    //public static Color SelectedReferenceObjectColor
    //{
    //  get { return GetColor(idxSelectedReferenceObjectColor); }
    //  set { SetColor(idxSelectedReferenceObjectColor, value); }
    //}

    ///<summary>Gets or sets the color used to draw locked objects.</summary>
    public Color LockedObjectColor{ get; set; }

    //public static Color LockedRefereceObjectColor
    //{
    //  get { return GetColor(idxLockedReferenceObjectColor); }
    //  set { SetColor(idxLockedReferenceObjectColor, value); }
    //}

    /// <summary>Gets or sets the color of the world X axis of the world coordinates icon,
    /// appearing usually bottom left in viewports.</summary>
    public Color WorldCoordIconXAxisColor{ get; set; }
    /// <summary>Gets or sets the color of the world Y axis of the world coordinate icon,
    /// appearing usually bottom left in viewports.</summary>
    public Color WorldCoordIconYAxisColor{ get; set; }
    /// <summary>Gets or sets the color of the world Z axis of the world coordinate icon,
    /// appearing usually bottom left in viewports.</summary>
    public Color WorldCoordIconZAxisColor{ get; set; }

    /// <summary>Gets or sets the tracking color.</summary>
    public Color TrackingColor{ get; set; }
    /// <summary>Gets or sets the feedback color.</summary>
    public Color FeedbackColor{ get; set; }
    /// <summary>Gets or sets the default object color.</summary>
    public Color DefaultObjectColor{ get; set; }
    /// <summary>Gets or sets the viewport background color.</summary>
    public Color ViewportBackgroundColor{ get; set; }
    /// <summary>Gets or sets the frame background color.</summary>
    public Color FrameBackgroundColor{ get; set; }
    /// <summary>Gets or sets the command prompt text color.</summary>
    public Color CommandPromptTextColor{ get; set; }
    /// <summary>Gets or sets the comand prompt hypertext color.</summary>
    public Color CommandPromptHypertextColor{ get; set; }
    /// <summary>Gets or sets the comand prompt background color.</summary>
    public Color CommandPromptBackgroundColor{ get; set; }
    /// <summary>Gets or sets the crosshair color.</summary>
    public Color CrosshairColor{ get; set; }

    ///<summary>
    ///CRhinoPageView paper background. A rectangle is drawn into the background
    ///of page views to represent the printed area. The alpha portion of the color
    ///is used to draw the paper blended into the background
    ///</summary>
    public Color PageviewPaperColor{ get; set; }

    ///<summary>
    ///Gets or sets the color used by the layer manager dialog as the background color for the current layer.
    ///</summary>
    public Color CurrentLayerBackgroundColor{ get; set; }

    ///<summary>Gets or sets a value that determines if prompt messages are written to the history window.</summary>
    public bool EchoPromptsToHistoryWindow{ get; set; }
    ///<summary>Gets or sets a value that determines if command names are written to the history window.</summary>
    public bool EchoCommandsToHistoryWindow{ get; set; }
    ///<summary>Gets or sets a value that determines if the full path of the document is shown in the Rhino title bar.</summary>
    public bool ShowFullPathInTitleBar{ get; set; }
    ///<summary>Gets or sets a value that determines if cross hairs are visible.</summary>
    public bool ShowCrosshairs{ get; set; }

    ///<summary>Gets or sets the color of the thin line in the grid.</summary>
    public Color GridThinLineColor { get; set; }
    ///<summary>Gets or sets the color of the thick line in the grid.</summary>
    public Color GridThickLineColor { get; set; }

    ///<summary>Gets or sets the color of X axis line in the grid.</summary>
    public Color GridXAxisLineColor { get; set; }
    ///<summary>Gets or sets the color of Y axis line in the grid.</summary>
    public Color GridYAxisLineColor { get; set; }
    ///<summary>Gets or sets the color of Z axis line in the grid.</summary>
    public Color GridZAxisLineColor { get; set; }
  }

  /// <summary>
  /// Defines enumerated constant values for default positions of the command prompt inside the frame of the full editor window.
  /// </summary>
  public enum CommandPromptPosition : int
  {
    /// <summary>The command prompt is shown on top.</summary>
    Top = 0,
    /// <summary>The command prompt is shown at the bottom.</summary>
    Bottom = 1,
    /// <summary>The command prompt is shown floating.</summary>
    Floating = 2,
    /// <summary>The command prompt is shown hidden.</summary>
    Hidden = 3
  }

  /// <summary>
  /// Provides static methods and properties to deal with the appearance of the application.
  /// </summary>
  public static class AppearanceSettings
  {
    static AppearanceSettingsState CreateState(bool current)
    {
      IntPtr pAppearanceSettings = UnsafeNativeMethods.CRhinoAppAppearanceSettings_New(current);
      AppearanceSettingsState rc = new AppearanceSettingsState();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppearanceSettings_DefaultFontFaceNameGet(pString, pAppearanceSettings);
        rc.DefaultFontFaceName = sh.ToString();
      }
      rc.DefaultLayerColor = GetColor(idxDefaultLayerColor, pAppearanceSettings);
      rc.SelectedObjectColor = GetColor(idxSelectedObjectColor, pAppearanceSettings);
      rc.LockedObjectColor = GetColor(idxLockedObjectColor, pAppearanceSettings);
      rc.WorldCoordIconXAxisColor = GetColor(idxWorldIconXColor, pAppearanceSettings);
      rc.WorldCoordIconYAxisColor = GetColor(idxWorldIconYColor, pAppearanceSettings);
      rc.WorldCoordIconZAxisColor = GetColor(idxWorldIconZColor, pAppearanceSettings);
      rc.TrackingColor = GetColor(idxTrackingColor, pAppearanceSettings);
      rc.FeedbackColor = GetColor(idxFeedbackColor, pAppearanceSettings);
      rc.DefaultObjectColor = GetColor(idxDefaultObjectColor, pAppearanceSettings);
      rc.ViewportBackgroundColor = GetColor(idxViewportBackgroundColor, pAppearanceSettings);
      rc.FrameBackgroundColor = GetColor(idxFrameBackgroundColor, pAppearanceSettings);
      rc.CommandPromptTextColor = GetColor(idxCommandPromptTextColor, pAppearanceSettings);
      rc.CommandPromptHypertextColor = GetColor(idxCommandPromptHypertextColor, pAppearanceSettings);
      rc.CommandPromptBackgroundColor = GetColor(idxCommandPromptBackgroundColor, pAppearanceSettings);
      rc.CrosshairColor = GetColor(idxCrosshairColor, pAppearanceSettings);
      rc.PageviewPaperColor = GetColor(idxPageviewPaperColor, pAppearanceSettings);
      rc.CurrentLayerBackgroundColor = GetColor(idxCurrentLayerBackgroundColor, pAppearanceSettings);
      rc.EchoPromptsToHistoryWindow = UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoPromptsToHistoryWindow, pAppearanceSettings);
      rc.EchoCommandsToHistoryWindow = UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoCommandsToHistoryWindow, pAppearanceSettings);
      rc.ShowFullPathInTitleBar = UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxFullPathInTitleBar, pAppearanceSettings);
      rc.ShowCrosshairs = UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxCrosshairsVisible, pAppearanceSettings);
      UnsafeNativeMethods.CRhinoAppAppearanceSettings_Delete(pAppearanceSettings);

      // also add grid settings
      IntPtr pGridSettings = UnsafeNativeMethods.CRhinoAppGridSettings_New(current);

      rc.GridThickLineColor = GetGridColor(idxThickLineColor, pGridSettings);
      rc.GridThinLineColor = GetGridColor(idxThinLineColor, pGridSettings);
      rc.GridXAxisLineColor = GetGridColor(idxXAxisColor, pGridSettings);
      rc.GridYAxisLineColor = GetGridColor(idxYAxisColor, pGridSettings);
      rc.GridZAxisLineColor = GetGridColor(idxZAxisColor, pGridSettings);
      UnsafeNativeMethods.CRhinoAppGridSettings_Delete(pGridSettings);

      return rc;
    }

    /// <summary>
    /// Gets the factory settings of the application.
    /// </summary>
    /// <returns>An instance of a class that represents all the default settings joined together.</returns>
    public static AppearanceSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Gets the current settings of the application.
    /// </summary>
    /// <returns>An instance of a class that represents all the settings as they appear in the Rhino _Options dialog,
    /// joined in a single class.</returns>
    public static AppearanceSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Commits the default settings as the current settings.
    /// </summary>
    public static void RestoreDefaults()
    {
      UpdateFromState(GetDefaultState());
    }

    /// <summary>
    /// Sets all settings to a particular defined joined state.
    /// </summary>
    /// <param name="state">A joined settings object.</param>
    public static void UpdateFromState(AppearanceSettingsState state)
    {
      DefaultFontFaceName = state.DefaultFontFaceName;
      DefaultLayerColor = state.DefaultLayerColor;
      SelectedObjectColor = state.SelectedObjectColor;
      LockedObjectColor = state.LockedObjectColor;
      WorldCoordIconXAxisColor = state.WorldCoordIconXAxisColor;
      WorldCoordIconYAxisColor = state.WorldCoordIconYAxisColor;
      WorldCoordIconZAxisColor = state.WorldCoordIconZAxisColor;
      TrackingColor = state.TrackingColor;
      FeedbackColor = state.FeedbackColor;
      DefaultObjectColor = state.DefaultObjectColor;
      ViewportBackgroundColor = state.ViewportBackgroundColor;
      FrameBackgroundColor = state.FrameBackgroundColor;
      CommandPromptBackgroundColor = state.CommandPromptBackgroundColor;
      CommandPromptHypertextColor = state.CommandPromptHypertextColor;
      CommandPromptTextColor = state.CommandPromptTextColor;
      CrosshairColor = state.CrosshairColor;
      PageviewPaperColor = state.PageviewPaperColor;
      CurrentLayerBackgroundColor = state.CurrentLayerBackgroundColor;
      EchoCommandsToHistoryWindow = state.EchoCommandsToHistoryWindow;
      EchoPromptsToHistoryWindow = state.EchoPromptsToHistoryWindow;
      ShowFullPathInTitleBar = state.ShowFullPathInTitleBar;
      ShowCrosshairs = state.ShowCrosshairs;

      GridThickLineColor = state.GridThickLineColor;
      GridThinLineColor = state.GridThinLineColor;
      GridXAxisLineColor = state.GridXAxisLineColor;
      GridYAxisLineColor = state.GridYAxisLineColor;
      GridZAxisLineColor = state.GridZAxisLineColor;
    }

    /// <summary>
    /// Gets or sets the default font face name used in Rhino.
    /// </summary>
    public static string DefaultFontFaceName
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoAppearanceSettings_DefaultFontFaceNameGet(pString, IntPtr.Zero);
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

    static Color GetColor(int which, IntPtr pAppearanceSettings)
    {
      int abgr = UnsafeNativeMethods.RhAppearanceSettings_GetSetColor(which, false, 0, pAppearanceSettings);
      return Rhino.Runtime.Interop.ColorFromWin32(abgr);
    }

    static Color GetColor(int which)
    {
      return GetColor(which, IntPtr.Zero);
    }
    static void SetColor(int which, Color c)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhAppearanceSettings_GetSetColor(which, true, argb, IntPtr.Zero);
    }

    /// <summary>
    /// Gets the .Net library color that is currently associated with a paint color.
    /// </summary>
    /// <param name="whichColor">A color association.</param>
    /// <returns>A .Net library color.</returns>
    public static Color GetPaintColor(PaintColor whichColor)
    {
      int abgr = UnsafeNativeMethods.RhColors_GetColor((int)whichColor);
      return Rhino.Runtime.Interop.ColorFromWin32(abgr);
    }

    /// <summary>
    /// Sets the logical paint color association to a spacific .Net library color, without forced UI update.
    /// </summary>
    /// <param name="whichColor">A logical color association.</param>
    /// <param name="c">A .Net library color.</param>
    public static void SetPaintColor(PaintColor whichColor, Color c)
    {
      SetPaintColor(whichColor, c, false);
    }

    /// <summary>
    /// Sets the logical paint color association to a spacific .Net library color.
    /// </summary>
    /// <param name="whichColor">A logical color association.</param>
    /// <param name="c">A .Net library color.</param>
    /// <param name="forceUiUpdate">true if the UI should be forced to update.</param>
    public static void SetPaintColor(PaintColor whichColor, Color c, bool forceUiUpdate)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.RhColors_SetColor((int)whichColor, argb, forceUiUpdate);
    }

    /// <summary>
    /// Gets or sets a value indicating if logical paint colors should be used.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the default layer color.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the color of the world coordinate X axis.
    /// </summary>
    public static Color WorldCoordIconXAxisColor
    {
      get { return GetColor(idxWorldIconXColor); }
      set { SetColor(idxWorldIconXColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the world coordinate Y axis.
    /// </summary>
    public static Color WorldCoordIconYAxisColor
    {
      get { return GetColor(idxWorldIconYColor); }
      set { SetColor(idxWorldIconYColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the world coordinate Z axis.
    /// </summary>
    public static Color WorldCoordIconZAxisColor
    {
      get { return GetColor(idxWorldIconZColor); }
      set { SetColor(idxWorldIconZColor, value); }
    }

    /// <summary>
    /// Gets or sets the tracking color.
    /// </summary>
    public static Color TrackingColor
    {
      get { return GetColor(idxTrackingColor); }
      set { SetColor(idxTrackingColor, value); }
    }

    /// <summary>
    /// Gets or sets the feedback color.
    /// </summary>
    public static Color FeedbackColor
    {
      get { return GetColor(idxFeedbackColor); }
      set { SetColor(idxFeedbackColor, value); }
    }

    /// <summary>
    /// Gets or sets the default color for new objects.
    /// </summary>
    public static Color DefaultObjectColor
    {
      get { return GetColor(idxDefaultObjectColor); }
      set { SetColor(idxDefaultObjectColor, value); }
    }

    /// <summary>
    /// Gets or sets the viewport background color.
    /// </summary>
    public static Color ViewportBackgroundColor
    {
      get { return GetColor(idxViewportBackgroundColor); }
      set { SetColor(idxViewportBackgroundColor, value); }
    }

    /// <summary>
    /// Gets or sets the background color of the frame.
    /// </summary>
    public static Color FrameBackgroundColor
    {
      get { return GetColor(idxFrameBackgroundColor); }
      set { SetColor(idxFrameBackgroundColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the command prompt text.
    /// </summary>
    public static Color CommandPromptTextColor
    {
      get { return GetColor(idxCommandPromptTextColor); }
      set { SetColor(idxCommandPromptTextColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the command prompt hypertext.
    /// </summary>
    public static Color CommandPromptHypertextColor
    {
      get { return GetColor(idxCommandPromptHypertextColor); }
      set { SetColor(idxCommandPromptHypertextColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the command prompt background.
    /// </summary>
    public static Color CommandPromptBackgroundColor
    {
      get { return GetColor(idxCommandPromptBackgroundColor); }
      set { SetColor(idxCommandPromptBackgroundColor, value); }
    }

    /// <summary>
    /// Gets or sets the color of the crosshair icon.
    /// </summary>
    public static Color CrosshairColor
    {
      get { return GetColor(idxCrosshairColor); }
      set { SetColor(idxCrosshairColor, value); }
    }

    ///<summary>
    /// Gets or sets the paper background. A rectangle is drawn into the background
    /// of page views to represent the printed area. The alpha portion of the color
    /// is used to draw the paper blended into the background
    ///</summary>
    public static Color PageviewPaperColor
    {
      get { return GetColor(idxPageviewPaperColor); }
      set { SetColor(idxPageviewPaperColor, value); }
    }

    ///<summary>
    /// Gets or sets the color used by the layer manager dialog as the background color for the current layer.
    ///</summary>
    public static Color CurrentLayerBackgroundColor
    {
      get { return GetColor(idxCurrentLayerBackgroundColor); }
      set { SetColor(idxCurrentLayerBackgroundColor, value); }
    }

    const int idxThinLineColor = 0;
    const int idxThickLineColor = 1;
    const int idxXAxisColor = 2;
    const int idxYAxisColor = 3;
    const int idxZAxisColor = 4;
    static Color GetGridColor(int which, IntPtr pSettings)
    {
      int abgr = UnsafeNativeMethods.CRhinoAppGridSettings_GetSetColor(which, false, 0, pSettings);
      return Rhino.Runtime.Interop.ColorFromWin32(abgr);
    }
    static void SetGridColor(int which, Color c, IntPtr pSettings)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.CRhinoAppGridSettings_GetSetColor(which, true, argb, pSettings);
    }


    // merged grid color settings with appearance settings
    /// <summary>
    /// Gets or sets the color of the thin line of the grid.
    /// </summary>
    public static Color GridThinLineColor
    {
      get { return GetGridColor(idxThinLineColor, IntPtr.Zero); }
      set { SetGridColor(idxThinLineColor, value, IntPtr.Zero); }
    }

    /// <summary>
    /// Gets or sets the color of the thick line of the grid.
    /// </summary>
    public static Color GridThickLineColor
    {
      get { return GetGridColor(idxThickLineColor, IntPtr.Zero); }
      set { SetGridColor(idxThickLineColor, value, IntPtr.Zero); }
    }

    /// <summary>
    /// Gets or sets the color of the X axis of the grid.
    /// </summary>
    public static Color GridXAxisLineColor
    {
      get { return GetGridColor(idxXAxisColor, IntPtr.Zero); }
      set { SetGridColor(idxXAxisColor, value, IntPtr.Zero); }
    }

    /// <summary>
    /// Gets or sets the color of the Y axis of the grid.
    /// </summary>
    public static Color GridYAxisLineColor
    {
      get { return GetGridColor(idxYAxisColor, IntPtr.Zero); }
      set { SetGridColor(idxYAxisColor, value, IntPtr.Zero); }
    }

    /// <summary>
    /// Gets or sets the color of the Z axis of the grid.
    /// </summary>
    public static Color GridZAxisLineColor
    {
      get { return GetGridColor(idxZAxisColor, IntPtr.Zero); }
      set { SetGridColor(idxZAxisColor, value, IntPtr.Zero); }
    }
#endregion

    /*
    ///<summary>length of world coordinate sprite axis in pixels.</summary>
    public static property int WorldCoordIconAxisSize{ int get(); void set(int); }
    ///<summary>&quot;radius&quot; of letter in pixels.</summary>
    public static property int WorldCoordIconLabelSize{ int get(); void set(int); }
    ///<summary>true to move axis letters as sprite rotates.</summary>
    public static property bool WorldCoordIconMoveLabels{ bool get(); void set(bool); }

    ///<summary>length of direction arrow shaft icon in pixels.</summary>
    public static property int DirectionArrowIconShaftSize{ int get(); void set(int); }
    ///<summary>length of direction arrowhead icon in pixels.</summary>
    public static property int DirectionArrowIconHeadSize{ int get(); void set(int); }

    ///<summary>
    ///3d "flag" text (like the Dot command) can either be depth 
    ///tested or shown on top. true means on top.
    ///</summary>
    public static property bool FlagTextOnTop{ bool get(); void set(bool); }

    public static property System::String^ CommandPromptFontName{System::String^ get(); void set(System::String^);}
    public static property int CommandPromptFontHeight{ int get(); void set(int); }
    public static property int CommandPromptHeightInLines{ int get(); void set(int); }
    
    public static property bool StatusBarVisible{ bool get(); void set(bool); }
    public static property bool OsnapDialogVisible{ bool get(); void set(bool); }
    */

    const int idxCommandPromptPosition = 0;

    /// <summary>
    /// Gets or sets the command prompt position.
    /// </summary>
    public static CommandPromptPosition CommandPromptPosition
    {
      get
      {
        return (CommandPromptPosition)UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetInt(idxCommandPromptPosition, IntPtr.Zero);
      }
      set
      {
        UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetInt(idxCommandPromptPosition, (int)value);
      }
    }

    const int idxEchoPromptsToHistoryWindow = 0;
    const int idxEchoCommandsToHistoryWindow = 1;
    const int idxFullPathInTitleBar = 2;
    const int idxCrosshairsVisible = 3;
    const int idxMenuVisible = 4;

    ///<summary>Gets or sets a value that determines if prompt messages are written to the history window.</summary>
    public static bool EchoPromptsToHistoryWindow
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoPromptsToHistoryWindow, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxEchoPromptsToHistoryWindow, value); }
    }
    ///<summary>Gets or sets a value that determines if command names are written to the history window.</summary>
    public static bool EchoCommandsToHistoryWindow
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxEchoCommandsToHistoryWindow, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxEchoCommandsToHistoryWindow, value); }
    }
    ///<summary>Gets or sets a value that determines if the full path of the document is shown in the Rhino title bar.</summary>
    public static bool ShowFullPathInTitleBar
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxFullPathInTitleBar, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxFullPathInTitleBar, value); }
    }
    ///<summary>Gets or sets a value that determines if cross hairs are visible.</summary>
    public static bool ShowCrosshairs
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxCrosshairsVisible, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxCrosshairsVisible, value); }
    }
    /*
    public static property bool ViewportTitleVisible{ bool get(); void set(bool); }
    public static property bool MainWindowTitleVisible{ bool get(); void set(bool); }
    */
    ///<summary>Gets or sets a value that determines if the File menu is visible.</summary>
    public static bool MenuVisible
    {
      get { return UnsafeNativeMethods.CRhinoAppAppearanceSettings_GetBool(idxMenuVisible, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppAppearanceSettings_SetBool(idxMenuVisible, value); }
    }

    ///<summary>Gets or sets the language identifier.</summary>
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

    /// <summary>
    /// Gets or sets the previous language identifier.
    /// </summary>
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

    /// <summary>
    /// Location where the Main Rhino window attempts to show when the application is first
    /// started.
    /// </summary>
    /// <param name="bounds">The rectangle in which the main window attempts to shows is assigned to this out parameter during the call.</param>
    /// <param name="state">The form state is assigned to this out parameter during the call.</param>
    /// <returns>false if the information could not be retrieved.</returns>
    public static bool InitialMainWindowState(out System.Drawing.Rectangle bounds, out System.Windows.Forms.FormWindowState state)
    {
      bounds = Rectangle.Empty;
      state = System.Windows.Forms.FormWindowState.Normal;

      int left=0, top=0, right=0, bottom=0, flags=0;
      bool rc = UnsafeNativeMethods.CRhinoDockBarManager_InitialMainFramePosition(ref left, ref top, ref right, ref bottom, ref flags);
      if (rc)
      {
        bounds = Rectangle.FromLTRB(left, top, right, bottom);
        const int SW_SHOWMAXIMIZED = 3;
        const int SW_SHOWMINIMIZED = 2;
        if ((flags & SW_SHOWMAXIMIZED) == SW_SHOWMAXIMIZED)
          state = System.Windows.Forms.FormWindowState.Maximized;
        if ((flags & SW_SHOWMINIMIZED) == SW_SHOWMINIMIZED)
          state = System.Windows.Forms.FormWindowState.Minimized;
      }
      return rc;
    }
  }

  /// <summary>
  /// Contains static methods and properties to access command aliases.
  /// </summary>
  public static class CommandAliasList
  {
    ///<summary>Returns the number of command alias in Rhino.</summary>
    public static int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoAppAliasList_Count(IntPtr.Zero);
      }
    }

    ///<summary>Returns a list of command alias names.</summary>
    ///<returns>An array of strings. This can be empty.</returns>
    public static string[] GetNames()
    {
      int count = UnsafeNativeMethods.CRhinoAppAliasList_Count(IntPtr.Zero);
      string[] rc = new string[count];
      using(var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          if (UnsafeNativeMethods.CRhinoAppAliasList_Item(i, pString, IntPtr.Zero))
            rc[i] = sh.ToString();
        }
      }
      return rc;
    }

    ///<summary>Removes all aliases from the list.</summary>
    public static void Clear()
    {
      UnsafeNativeMethods.RhCommandAliasList_DestroyList();
    }

    ///<summary>Returns the macro of a command alias.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    public static string GetMacro(string alias)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pMacro = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppAliasList_GetMacro(alias, pMacro, IntPtr.Zero);
        return sh.ToString();
      }
    }

    ///<summary>Modifies the macro of a command alias.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<param name='macro'>[in] The new command macro to run when the alias is executed.</param>
    ///<returns>true if successful.</returns>
    public static bool SetMacro(string alias, string macro)
    {
      return UnsafeNativeMethods.RhCommandAliasList_SetMacro(alias, macro);
    }

    ///<summary>Adds a new command alias to Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<param name='macro'>[in] The command macro to run when the alias is executed.</param>
    ///<returns>true if successful.</returns>
    public static bool Add(string alias, string macro)
    {
      return UnsafeNativeMethods.RhCommandAliasList_Add(alias, macro);
    }

    ///<summary>Deletes an existing command alias from Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<returns>true if successful.</returns>
    public static bool Delete(string alias)
    {
      return UnsafeNativeMethods.RhCommandAliasList_Delete(alias);
    }

    ///<summary>Verifies that a command alias exists in Rhino.</summary>
    ///<param name='alias'>[in] The name of the command alias.</param>
    ///<returns>true if the alias exists.</returns>
    public static bool IsAlias(string alias)
    {
      return UnsafeNativeMethods.RhCommandAliasList_IsAlias(alias);
    }

    /// <summary>
    /// Constructs a new dictionary that contains: as keys all names and as values all macros.
    /// <para>Modifications to this dictionary do not affect any Rhino command alias.</para>
    /// </summary>
    /// <returns>The new dictionary.</returns>
    public static System.Collections.Generic.Dictionary<string,string> ToDictionary()
    {
      var rc = new System.Collections.Generic.Dictionary<string,string>();
      string[] names = GetNames();
      for (int i = 0; i < names.Length; i++)
      {
        string macro = GetMacro(names[i]);
        if (!string.IsNullOrEmpty(names[i]))
          rc[names[i]] = macro;
      }
      return rc;
    }

    /// <summary>
    /// Computes a value indicating if the current alias list is the same as the default alias list.
    /// </summary>
    /// <returns>true if the current alias list is exactly equal to the default alias list; false otherwise.</returns>
    public static bool IsDefault()
    {
      var current = ToDictionary();
      var defaults = GetDefaults();
      if (current.Count != defaults.Count)
        return false;

      foreach (string key in current.Keys)
      {
        if (!defaults.ContainsKey(key))
          return false;
        string currentMacro = current[key];
        string defaultMacro = defaults[key];
        if (!currentMacro.Equals(defaultMacro, StringComparison.InvariantCultureIgnoreCase))
          return false;
      }
      return true;
    }

    /// <summary>
    /// Constructs a dictionary containing as keys the default names and as value the default macro.
    /// <para>The returned dicionary contains a copy of the settings.</para>
    /// </summary>
    /// <returns>A new dictionary with the default name/macro combinantions.</returns>
    public static System.Collections.Generic.Dictionary<string, string> GetDefaults()
    {
      var rc = new System.Collections.Generic.Dictionary<string,string>();
      IntPtr pCommandAliasList = UnsafeNativeMethods.CRhinoAppAliasList_New();
      int count = UnsafeNativeMethods.CRhinoAppAliasList_Count(pCommandAliasList);
      using (var shName = new StringHolder())
      using (var shMacro = new StringHolder())
      {
        IntPtr pName = shName.NonConstPointer();
        IntPtr pMacro = shMacro.NonConstPointer();
        for (int i = 0; i < count; i++)
        {
          if (UnsafeNativeMethods.CRhinoAppAliasList_Item(i, pName, pCommandAliasList))
          {
            string name = shName.ToString();
            if (UnsafeNativeMethods.CRhinoAppAliasList_GetMacro(name, pMacro, pCommandAliasList))
            {
              string macro = shMacro.ToString();
              rc[name] = macro;
            }
          }
        }
      }
      UnsafeNativeMethods.CRhinoAppAliasList_Delete(pCommandAliasList);
      return rc;
    }
  }

  /// <summary>Represents a snapshot of <see cref="EdgeAnalysisSettings"/>.</summary>
  public class EdgeAnalysisSettingsState
  {
    internal EdgeAnalysisSettingsState() { }

    /// <summary>
    /// Gets or sets a color used to enhance display edges in commands like _ShowEdges and _ShowNakedEdges.
    /// </summary>
    public Color ShowEdgeColor { get; set; }

    /// <summary>
    /// Gets or sets a value referring to the group of edges that are targeted.
    /// <para>0 = all.</para>
    /// <para>1 = naked.</para>
    /// <para>2 = non-manifold.</para>
    /// </summary>
    public int ShowEdges { get; set; }
  }
  
  /// <summary>
  /// Contains static methods and properties to modify the visitbility of edges in edge-related commands.
  /// </summary>
  public static class EdgeAnalysisSettings
  {
    static EdgeAnalysisSettingsState CreateState(bool current)
    {
      IntPtr pSettings = UnsafeNativeMethods.CRhinoEdgeAnalysisSettings_New(current);
      EdgeAnalysisSettingsState rc = new EdgeAnalysisSettingsState();

      int abgr = UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdgeColor(false, 0, pSettings);
      rc.ShowEdgeColor = Rhino.Runtime.Interop.ColorFromWin32(abgr);
      rc.ShowEdges = UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdges(false, 0, pSettings);
      UnsafeNativeMethods.CRhinoEdgeAnalysisSettings_Delete(pSettings);
      return rc;
    }

    /// <summary>
    /// Gets the factory settings of the application.
    /// </summary>
    public static EdgeAnalysisSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Gets the current settings of the application.
    /// </summary>
    public static EdgeAnalysisSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Commits the default settings as the current settings.
    /// </summary>
    public static void RestoreDefaults()
    {
      UpdateFromState(GetDefaultState());
    }

    /// <summary>
    /// Sets all settings to a particular defined joined state.
    /// </summary>
    /// <param name="state">The particular state.</param>
    public static void UpdateFromState(EdgeAnalysisSettingsState state)
    {
      ShowEdgeColor = state.ShowEdgeColor;
      ShowEdges = state.ShowEdges;
    }


    ///<summary>Gets or sets a color used to enhance display
    ///edges in commands like _ShowEdges and _ShowNakedEdges.</summary>
    public static Color ShowEdgeColor
    {
      get
      {
        int abgr = UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdgeColor(false, 0, IntPtr.Zero);
        return Rhino.Runtime.Interop.ColorFromWin32(abgr);
      }
      set
      {
        int argb = value.ToArgb();
        UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdgeColor(true, argb, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Gets or sets a value referring to the group of edges that are targeted.
    /// <para>0 = all.</para>
    /// <para>1 = naked.</para>
    /// <para>2 = non-manifold.</para>
    /// </summary>
    public static int ShowEdges
    {
      get
      {
        return UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdges(false, 0, IntPtr.Zero);
      }
      set
      {
        UnsafeNativeMethods.RhEdgeAnalysisSettings_ShowEdges(true, value, IntPtr.Zero);
      }
    }
  }

  /// <summary>
  /// Represents a snapshot of <see cref="FileSettings"/>.
  /// </summary>
  public class FileSettingsState
  {
    internal FileSettingsState() { }

    ///<summary>How often the document will be saved when Rhino&apos;s automatic file saving mechanism is enabled.</summary>
    public System.TimeSpan AutoSaveInterval { get; set; }

    ///<summary>Enables or disables Rhino&apos;s automatic file saving mechanism.</summary>
    public bool AutoSaveEnabled { get; set; }

    ///<summary>Saves render and display meshes in autosave file.</summary>
    public bool AutoSaveMeshes { get; set; }
    
    ///<summary>true for users who consider view changes a document change.</summary>
    public bool SaveViewChanges { get; set; }

    ///<summary>Ensures that only one person at a time can have a file open for saving.</summary>
    public bool FileLockingEnabled { get; set; }

    ///<summary>Displays an information dialog which identifies computer file is open on.</summary>
    public bool FileLockingOpenWarning { get; set; }

    /// <summary>
    /// Gets or sets a value that decides if copies to the clipboard are performed in both the current
    /// and previous Rhino clipboard formats.  This means you will double the size of what is saved in
    /// the clipboard but will be able to copy from the current to the previous version using the
    /// clipboard.
    /// </summary>
    public bool ClipboardCopyToPreviousRhinoVersion { get; set; }

    /// <summary>
    /// Gets or sets a value that determines what to do with clipboad data on exit.
    /// </summary>
    public ClipboardState ClipboardOnExit { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create backup files.
    /// </summary>
    public bool CreateBackupFiles { get; set; }
  }

  /// <summary>
  /// Contains static methods and properties relating Rhino files.
  /// </summary>
  public static class FileSettings
  {
    static FileSettingsState CreateState(bool current)
    {
      IntPtr pFileSettings = UnsafeNativeMethods.CRhinoAppFileSettings_New(current);
      FileSettingsState rc = new FileSettingsState();
      int minutes = UnsafeNativeMethods.CRhinoAppFileSettings_AutosaveInterval(pFileSettings, -1);
      rc.AutoSaveInterval = System.TimeSpan.FromMinutes(minutes);
      rc.AutoSaveEnabled = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxAutoSaveEnabled);
      rc.AutoSaveMeshes = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxAutoSaveMeshes);
      rc.SaveViewChanges = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxSaveViewChanges);
      rc.FileLockingEnabled = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxFileLockingEnabled);
      rc.FileLockingOpenWarning = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxFileLockingOpenWarning);
      rc.ClipboardCopyToPreviousRhinoVersion = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxClipboardCopyToPreviousRhinoVersion);
      rc.ClipboardOnExit = (ClipboardState)UnsafeNativeMethods.CRhinoAppFileSettings_GetClipboardOnExit(pFileSettings);
      rc.CreateBackupFiles = UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(pFileSettings, idxCreateBackupFiles);
      UnsafeNativeMethods.CRhinoAppFileSettings_Delete(pFileSettings);

      return rc;
    }

    /// <summary>
    /// Returns the default state.
    /// </summary>
    /// <returns>A new instance containing the default state.</returns>
    public static FileSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Returns the current state.
    /// </summary>
    /// <returns>A new instance containing the current state.</returns>
    public static FileSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    
    const int idxGetRhinoRoamingProfileDataFolder = 0;
    const int idxGetRhinoApplicationDataFolder = 1;

    /// <summary>
    /// Gets the data folder for machine or current user.
    /// </summary>
    /// <param name="currentUser">true if the query relates to the current user.</param>
    /// <returns>A directory to user or machine data.</returns>
    public static string GetDataFolder(bool currentUser)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        int which = currentUser ? idxGetRhinoRoamingProfileDataFolder : idxGetRhinoApplicationDataFolder;
        UnsafeNativeMethods.CRhinoFileUtilities_GetDataFolder(pStringHolder, which);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Returns a list of recently opened files. Note that this function does not
    /// check to make sure that these files still exist.
    /// </summary>
    /// <returns>An array of strings with the paths to the recently opened files.</returns>
    public static string[] RecentlyOpenedFiles()
    {
      IntPtr pStrings = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoApp_RecentlyOpenedFiles(pStrings);
      string[] rc = new string[count];
      if (count > 0)
      {
        using (var sh = new StringHolder())
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          for (int i = 0; i < count; i++)
          {
            UnsafeNativeMethods.ON_StringArray_Get(pStrings, i, pStringHolder);
            rc[i] = sh.ToString();
          }
        }
      }
      UnsafeNativeMethods.ON_StringArray_Delete(pStrings);
      return rc;
    }

    /// <summary>
    /// Adds a new imagePath to Rhino&apos;s search imagePath list.
    /// See "Options Files settings" in the Rhino help file for more details.
    /// </summary>
    /// <param name='folder'>[in] The valid folder, or imagePath, to add.</param>
    /// <param name='index'>
    /// [in] A zero-based position index in the search imagePath list to insert the string.
    /// If -1, the imagePath will be appended to the end of the list.
    /// </param>
    /// <returns>
    /// The index where the item was inserted if success.
    /// <para>-1 on failure.</para>
    ///</returns>
    public static int AddSearchPath(string folder, int index)
    {
      return UnsafeNativeMethods.RhDirectoryManager_AddSearchPath(folder, index);
    }

    /// <summary>
    /// Removes an existing imagePath from Rhino's search imagePath list.
    /// See "Options Files settings" in the Rhino help file for more details.
    /// </summary>
    /// <param name='folder'>[in] The valid folder, or imagePath, to remove.</param>
    /// <returns>true or false indicating success or failure.</returns>
    public static bool DeleteSearchPath(string folder)
    {
      return UnsafeNativeMethods.RhDirectoryManager_DeleteSearchPath(folder);
    }

    /// <summary>
    /// Searches for a file using Rhino's search imagePath. Rhino will look for a file in the following locations:
    /// 1. The current document's folder.
    /// 2. Folder's specified in Options dialog, File tab.
    /// 3. Rhino's System folders.
    /// </summary>
    /// <param name="fileName">short file name to search for.</param>
    /// <returns> full imagePath on success; null on error.</returns>
    public static string FindFile(string fileName)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        UnsafeNativeMethods.RhDirectoryManager_FindFile(fileName, pStringHolder);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Gets the amount of search paths that are currently defined.
    /// </summary>
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
      using (var sh = new StringHolder())
      {
        int count = SearchPathCount;
        string[] rc = new string[count];
        for (int i = 0; i < count; i++)
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          UnsafeNativeMethods.RhDirectoryManager_SearchPath(i, pStringHolder);
          rc[i] = sh.ToString();
        }
        return rc;
      }
    }

    /// <summary>
    /// Returns or sets Rhino's working directory, or folder.
    /// The working folder is the default folder for all file operations.
    /// </summary>
    public static string WorkingFolder
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          UnsafeNativeMethods.RhDirectoryManager_WorkingFolder(null,pStringHolder);
          return sh.ToString();
        }
      }
      set
      {
        UnsafeNativeMethods.RhDirectoryManager_WorkingFolder(value,IntPtr.Zero);
      }
    }

    const int idxTemplateFolder = 0;
    const int idxTemplateFile = 1;
    const int idxAutoSaveFile = 2;
    static void SetFileString(string value, int which)
    {
      UnsafeNativeMethods.CRhinoAppFileSettings_SetFile(value, which);
    }
    static string GetFileString(int which)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppFileSettings_GetFile(which, pString);
        return sh.ToString();
      }
    }

    ///<summary>Returns or sets the location of Rhino's template files.</summary>
    public static string TemplateFolder
    {
      get
      {
        return GetFileString(idxTemplateFolder);
      }
      set
      {
        if (!string.IsNullOrEmpty(value) && !System.IO.Directory.Exists(value))
          return; //throw exception or just allow invalid strings??
        SetFileString(value, idxTemplateFolder);
      }
    }

    ///<summary>Returns or sets the location of Rhino&apos;s template file.</summary>
    public static string TemplateFile
    {
      get
      {
        return GetFileString(idxTemplateFile);
      }
      set
      {
        if (!string.IsNullOrEmpty(value) && !System.IO.File.Exists(value))
          return; //throw exception or just allow invalid strings??

        // This setting MUST be done on the main application thread in Windows
        if (Rhino.Runtime.HostUtils.RunningOnWindows)
        {
          RhinoWindow wnd = RhinoApp.MainApplicationWindow;
          if( wnd==null || wnd.InvokeRequired )
            return;
        }

        SetFileString(value, idxTemplateFile);
      }
    }

    ///<summary>the file name used by Rhino&apos;s automatic file saving mechanism.</summary>
    public static string AutoSaveFile
    {
      get
      {
        return GetFileString(idxAutoSaveFile);
      }
      set
      {
        if (!string.IsNullOrEmpty(value) && !System.IO.File.Exists(value))
          return; //throw exception or just allow invalid strings??
        SetFileString(value, idxAutoSaveFile);
      }
    }


    ///<summary>how often the document will be saved when Rhino&apos;s automatic file saving mechanism is enabled.</summary>
    public static System.TimeSpan AutoSaveInterval
    {
      get
      {
        int minutes = UnsafeNativeMethods.CRhinoAppFileSettings_AutosaveInterval(IntPtr.Zero,-1);
        return System.TimeSpan.FromMinutes(minutes);
      }
      set
      {
        double minutes = value.TotalMinutes;
        if (minutes > -10.0 && minutes < int.MaxValue)
          UnsafeNativeMethods.CRhinoAppFileSettings_AutosaveInterval(IntPtr.Zero, (int)minutes);
      }
    }

    const int idxAutoSaveEnabled = 0;
    const int idxAutoSaveMeshes = 1;
    const int idxSaveViewChanges = 2;
    const int idxFileLockingEnabled = 3;
    const int idxFileLockingOpenWarning = 4;
    const int idxClipboardCopyToPreviousRhinoVersion = 5;
    const int idxCreateBackupFiles = 6;

    ///<summary>Enables or disables Rhino&apos;s automatic file saving mechanism.</summary>
    public static bool AutoSaveEnabled
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxAutoSaveEnabled); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxAutoSaveEnabled, value); }
    }

    ///<summary>save render and display meshes in autosave file.</summary>
    public static bool AutoSaveMeshes
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxAutoSaveMeshes); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxAutoSaveMeshes, value); }
    }

    ///<summary>Input list of commands that force AutoSave prior to running.</summary>
    public static string[] AutoSaveBeforeCommands()
    {
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        UnsafeNativeMethods.RhFileSettings_AutosaveBeforeCommands(pStringHolder);
        string s = sh.ToString();
        if( string.IsNullOrEmpty(s) )
          return null;
        return s.Split(new char[] { ' ' });
      }
    }

    ///<summary>Set list of commands that force AutoSave prior to running.</summary>
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

    /// <summary>true for users who consider view changes a document change.</summary>
    public static bool SaveViewChanges
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxSaveViewChanges); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxSaveViewChanges, value); }
    }

    /// <summary>Ensure that only one person at a time can have a file open for saving.</summary>
    public static bool FileLockingEnabled
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxFileLockingEnabled); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxFileLockingEnabled, value); }
    }

    /// <summary>Gets or sets whether to display the information dialog which identifies computer files.</summary>
    public static bool FileLockingOpenWarning
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxFileLockingOpenWarning); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxFileLockingOpenWarning, value); }
    }

    /// <summary>Gets or sets a value that controls the creation of backup files.</summary>
    public static bool CreateBackupFiles
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxCreateBackupFiles); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxCreateBackupFiles, value); }
    }
    /// <summary>
    /// Gets or sets a value that decides if copies to the clipboard are performed in both the current
    /// and previous Rhino clipboard formats.  This means you will double the size of what is saved in
    /// the clipboard but will be able to copy from the current to the previous version using the
    /// clipboard.
    /// </summary>
    public static bool ClipboardCopyToPreviousRhinoVersion
    {
      get { return UnsafeNativeMethods.CRhinoAppFileSettings_GetBool(IntPtr.Zero, idxClipboardCopyToPreviousRhinoVersion); }
      set { UnsafeNativeMethods.CRhinoAppFileSettings_SetBool(IntPtr.Zero, idxClipboardCopyToPreviousRhinoVersion, value); }
    }

    /// <summary>
    /// Gets or sets a value that determines what to do with clipboad data on exit.
    /// </summary>
    public static ClipboardState ClipboardOnExit
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoAppFileSettings_GetClipboardOnExit(IntPtr.Zero);
        return (ClipboardState)rc;
      }
      set
      {
        UnsafeNativeMethods.RhFileSettings_ClipboardOnExit(true, (int)value);
      }
    }

    /// <summary>Returns the directory where the main Rhino executable is located.</summary>
    public static string ExecutableFolder
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxExecutableFolder, pString);
          return sh.ToString();
        }
      }
    }

    /// <summary>Returns Rhino's installation folder.</summary>
    public static System.IO.DirectoryInfo InstallFolder
    {
      get
      {
        using (var sh = new StringHolder())
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

    /// <summary>
    /// Gets the Rhino help file path.
    /// </summary>
    public static string HelpFilePath
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxHelpFilePath, pString);
          return sh.ToString();
        }
      }
    }

    /// <summary>
    /// Get full path to a Rhino specific sub-folder under the per-user Local
    /// (non-roaming) Profile folder.  This is the folder where user-specific
    /// data is stored.
    /// 
    /// Windows NT4, 2000, XP, usually someplace like:
    ///   "C:\Documents and Settings\[USERNAME]\Local Settings\Application Data\McNeel\Rhinoceros\[VERSION_NUMBER]\"
    /// Windows Vista, 7, usually someplace like:
    ///   "C:\Users\[USERNAME]\AppData\Local\McNeel\Rhinoceros\[VERSION_NUMBER]\"
    /// </summary>
    public static string LocalProfileDataFolder
    {
      get
      {
        using (var sh = new StringHolder())
        {
          var pointer = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxLocalProfileDataFolder, pointer);
          return sh.ToString();
        }
      }
    }

    /// <summary>
    /// Gets the path to the default RUI file.
    /// </summary>
    public static string DefaultRuiFile
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoApp_GetString(RhinoApp.idxDefaultRuiFile, pString);
          return sh.ToString();
        }
      }
    }
  }

  /// <summary>
  /// Contains static methods and properties relating to the list of commands that are never repeated.
  /// </summary>
  public static class NeverRepeatList
  {
    /// <summary>
    /// Only use the list if somebody modifies it via CRhinoAppSettings::SetDontRepeatCommands().
    /// Return value of true means CRhinoCommand don&apos;t repeat flags will be ignored and the m_dont_repeat_list
    /// will be used instead.  false means the individual CRhinoCommands will determine if they are repeatable.
    ///</summary>
    public static bool UseNeverRepeatList
    {
      get
      {
        return UnsafeNativeMethods.RhDontRepeatList_UseList();
      }
    }

    ///<summary>Puts the command name tokens in m_dont_repeat_list.</summary>
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

    ///<summary>The list of commands to not repeat.</summary>
    public static string[] CommandNames()
    {
      using(var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppDontRepeatCommandSettings_GetDontRepeatList(pString);
        string s = sh.ToString();
        string[] rc =  s.Split(new char[] { ' ', '\n' });
        for (int i = 0; i < rc.Length; i++)
        {
          rc[i] = rc[i].Trim();
        }
        return rc;
      }
    }
  }

  /// <summary>
  /// Defines enumerated constant values to indicate a particular window selection mode.
  /// </summary>
  public enum MouseSelectMode : int
  {
    /// <summary>Anything that crosses this window will be selected.</summary>
    Crossing = 0,
    /// <summary>Anything that is inside this window will be selected.</summary>
    Window = 1,
    /// <summary>Drag a rectangle from left to right for window select. Drag a rectangle from right to left for crossing select.</summary>
    Combo = 2
  }

  /// <summary>
  /// Defines enumerated constant values to define what happens when
  /// either the middle mouse button on a three-button mouse is clicked or after pressing the wheel on a wheeled mouse.
  /// </summary>
  public enum MiddleMouseMode : int
  {
    /// <summary>Pops up two-part menu at the cursor location.
    /// You can list your favorite commands in the top section.
    /// The bottom section is the list of most recent commands used.</summary>
    PopupMenu = 0,

    /// <summary>
    /// Choose a toolbar to pop up at the cursor location.
    /// Create a toolbar containing your favorite commands or object snaps to use as a pop-up toolbar.
    /// </summary>
    PopupToolbar = 1,

    /// <summary>
    /// Lists a series of commands that run when you click the middle mouse button.
    /// </summary>
    RunMacro = 2
  }

  /// <summary>
  /// Represents a snapshot of <see cref="GeneralSettings"/>.
  /// </summary>
  public class GeneralSettingsState
  {
    internal GeneralSettingsState() { }

    /// <summary>
    /// Gets or sets the current selection mode.
    /// </summary>
    public MouseSelectMode MouseSelectMode { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of popup menu lines.
    /// </summary>
    public int MaximumPopupMenuLines { get; set; }

    /// <summary>
    /// Gets or sets the minimum undo steps.
    /// <para>Undo records will be purged if there are more than MinimumUndoSteps and
    /// they use more than MaximumUndoMemoryMb.</para>
    /// </summary>
    public int MinimumUndoSteps { get; set; }

    /// <summary>
    /// Gets or sets the minimum undo memory Mb.
    /// <para>Undo records will be purged if there are more than MinimumUndoSteps and
    /// they use more than MaximumUndoMemoryMb.</para>
    /// </summary>
    public int MaximumUndoMemoryMb { get; set; }

    /// <summary>
    /// Gets or sets the number of isoparm curves to show on new objects.
    /// </summary>
    public int NewObjectIsoparmCount { get; set; }

    /// <summary>
    /// Gets or sets what happens when the user clicks the middle mouse.
    /// </summary>
    public MiddleMouseMode MiddleMouseMode { get; set; }

    /// <summary>
    /// Gets or sets the toolbar to popup when the middle mouse is clicked on
    /// a view, this value is only used when MiddleMouseMode is set to
    /// PopupToolbar.
    /// </summary>
    public string MiddleMousePopupToolbar { get; set; }

    /// <summary>
    /// Gets or sets the toolbar to popup when the middle mouse is clicked on
    /// a view, this value is only used when MiddleMouseMode is set to
    /// PopupToolbar.
    /// </summary>
    public string MiddleMouseMacro { get; set; }

    /// <summary>
    /// true if right mouse down + delay will pop up context menu on a mouse up if no move happens.
    /// </summary>
    public bool EnableContextMenu { get; set; }

    /// <summary>
    /// Gets or sets the time to wait before permitting context menu display.
    /// </summary>
    public System.TimeSpan ContextMenuDelay { get; set; }

    /// <summary>
    /// Gets or sets the command help dialog auto-update feature.
    /// </summary>
    public bool AutoUpdateCommandHelp { get; set; }
  }

  /// <summary>
  /// Contains static methods and properties to give access to Rhinoceros settings.
  /// </summary>
  public static class GeneralSettings
  {
    static GeneralSettingsState CreateState(bool current)
    {
      IntPtr pGeneralSettings = UnsafeNativeMethods.CRhinoAppGeneralSettings_New(current);
      GeneralSettingsState rc = new GeneralSettingsState();

      rc.MouseSelectMode = (MouseSelectMode)UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxMouseSelectMode);
      rc.MaximumPopupMenuLines = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxMaxPopupMenuLines);
      rc.MinimumUndoSteps = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxMinUndoSteps);
      rc.MaximumUndoMemoryMb = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxMaxUndoMemoryMb);
      rc.NewObjectIsoparmCount = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxNewObjectIsoparmCount);
      rc.MiddleMouseMode = (MiddleMouseMode)UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxMiddleMouseMode);

      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppGeneralSettings_GetString(IntPtr.Zero, idxMiddleMousePopupToolbar, pStringHolder);
        rc.MiddleMousePopupToolbar = sh.ToString();
        UnsafeNativeMethods.CRhinoAppGeneralSettings_GetString(IntPtr.Zero, idxMiddleMouseMacro, pStringHolder);
        rc.MiddleMouseMacro = sh.ToString();
      }
      rc.EnableContextMenu = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetBool(pGeneralSettings, idxEnableContextMenu);
      int ms = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(pGeneralSettings, idxContextMenuDelay);
      rc.ContextMenuDelay = TimeSpan.FromMilliseconds(ms);
      rc.AutoUpdateCommandHelp = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetBool(pGeneralSettings, idxAutoUpdateCommandContext);

      UnsafeNativeMethods.CRhinoAppGeneralSettings_Delete(pGeneralSettings);
      return rc;
    }

    /// <summary>
    /// Gets the factory settings.
    /// </summary>
    /// <returns>A new general state with factory settings.</returns>
    public static GeneralSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    /// <returns>A new general state with current settings.</returns>
    public static GeneralSettingsState GetCurrentState()
    {
      return CreateState(true);
    }


    const int idxMouseSelectMode = 0;
    const int idxMaxPopupMenuLines = 1;
    const int idxMinUndoSteps = 2;
    const int idxMaxUndoMemoryMb = 3;
    const int idxNewObjectIsoparmCount = 4;
    const int idxMiddleMouseMode = 5;
    const int idxContextMenuDelay = 6;

    /// <summary>
    /// Gets or sets the current selection mode.
    /// </summary>
    public static MouseSelectMode MouseSelectMode
    {
      get { return (MouseSelectMode)UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxMouseSelectMode); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxMouseSelectMode, (int)value); }
    }

    /// <summary>
    /// Gets or sets the maximum number of popup menu lines.
    /// </summary>
    public static int MaximumPopupMenuLines
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxMaxPopupMenuLines); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxMaxPopupMenuLines, value); }
    }

    //// Popup menu
    //ON_ClassArray<ON_wString> m_popup_favorites;
    //// Commands
    //ON_wString m_startup_commands;

    /// <summary>
    /// Gets or sets the minimum undo steps.
    /// <para>Undo records will be purged if there are more than MinimumUndoSteps and
    /// they use more than MaximumUndoMemoryMb.</para>
    /// </summary>
    public static int MinimumUndoSteps
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxMinUndoSteps); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxMinUndoSteps, value); }
    }

    /// <summary>
    /// Gets or sets the minimum undo memory Mb.
    /// <para>Undo records will be purged if there are more than MinimumUndoSteps and
    /// they use more than MaximumUndoMemoryMb.</para>
    /// </summary>
    public static int MaximumUndoMemoryMb
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxMaxUndoMemoryMb); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxMaxUndoMemoryMb, value); }
    }

    /// <summary>
    /// Gets or sets the number of isoparm curves to show on new objects.
    /// </summary>
    public static int NewObjectIsoparmCount
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxNewObjectIsoparmCount); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxNewObjectIsoparmCount, value); }
    }

    // This may belong somewhere else
    //BOOL m_show_surface_isoparms;

    /// <summary>
    /// Gets or sets what happens when the user clicks the middle mouse.
    /// </summary>
    public static MiddleMouseMode MiddleMouseMode
    {
      get { return (MiddleMouseMode)UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxMiddleMouseMode); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxMiddleMouseMode, (int)value); }
    }

    const int idxMiddleMousePopupToolbar = 0;
    const int idxMiddleMouseMacro = 1;

    /// <summary>
    /// Gets or sets the toolbar to popup when the middle mouse is clicked on
    /// a view, this value is only used when MiddleMouseMode is set to
    /// PopupToolbar.
    /// </summary>
    public static string MiddleMousePopupToolbar
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoAppGeneralSettings_GetString(IntPtr.Zero, idxMiddleMousePopupToolbar, pStringHolder);
          return sh.ToString();
        }
      }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetString(IntPtr.Zero, idxMiddleMousePopupToolbar, value); }
    }

    /// <summary>
    /// Gets or sets the toolbar to popup when the middle mouse is clicked on
    /// a view, this value is only used when MiddleMouseMode is set to
    /// PopupToolbar.
    /// </summary>
    public static string MiddleMouseMacro
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr pStringHolder = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoAppGeneralSettings_GetString(IntPtr.Zero, idxMiddleMouseMacro, pStringHolder);
          return sh.ToString();
        }
      }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetString(IntPtr.Zero, idxMiddleMouseMacro, value); }
    }

    ////Description:
    ////  Call this method to get the tool bar that will be displayed if
    ////  popup_toolbar == m_middle_mouse_mode
    ////Returns:
    ////  Returns pointer to tool bar to pop-up if found otherwise NULL
    //const class CRhinoUiToolBar* MiddleMouseToolBar() const;
    //ON_wString        m_middle_mouse_toolbar_name;
    //ON_wString        m_middle_mouse_macro;

    const int idxEnableContextMenu = 0;
    const int idxAutoUpdateCommandContext = 1;

    /// <summary>
    /// true if right mouse down + delay will pop up context menu on a mouse up if no move happens.
    /// </summary>
    public static bool EnableContextMenu
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetBool(IntPtr.Zero, idxEnableContextMenu); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetBool(IntPtr.Zero, idxEnableContextMenu, value); }
    }

    /// <summary>
    /// Time to wait before permitting context menu display.
    /// </summary>
    public static System.TimeSpan ContextMenuDelay
    {
      get
      {
        int ms = UnsafeNativeMethods.CRhinoAppGeneralSettings_GetInt(IntPtr.Zero, idxContextMenuDelay);
        return System.TimeSpan.FromMilliseconds(ms);
      }
      set
      {
        int ms = (int)value.TotalMilliseconds;
        UnsafeNativeMethods.CRhinoAppGeneralSettings_SetInt(IntPtr.Zero, idxContextMenuDelay, ms);
      }
    }

    /// <summary>
    /// Command help dialog auto-update feature.
    /// </summary>
    public static bool AutoUpdateCommandHelp
    {
      get { return UnsafeNativeMethods.CRhinoAppGeneralSettings_GetBool(IntPtr.Zero, idxAutoUpdateCommandContext); }
      set { UnsafeNativeMethods.CRhinoAppGeneralSettings_SetBool(IntPtr.Zero, idxAutoUpdateCommandContext, value); }
    }
    /*
    // Material persistence

    // If true, the "Save" command will save every material
    // including the ones that are not used by any object
    // or layer.
    bool m_bSaveUnreferencedMaterials;

    // If true, objects that are copied from other objects
    // will get the same material index.  Otherwise the new
    // object gets an identical material with a unique 
    // material index.
    bool m_bShareMaterials;

    // If m_bSplitCreasedSurfaces is true, then 
    // surfaces are automatically split into
    // polysurfaces with smooth faces when they are added
    // to the CRhinoDoc.  Never perminantly change the
    // value of this setting.  It was a mistake to
    // put this setting in the public SDK.
    //
    // To temporarily set m_bSplitCreasedSurfaces to false,
    // create a CRhinoKeepKinkySurfaces on the stack
    // like this:
    // {  
    //   CRhinoKeepKinkySurfaces keep_kinky_surfaces;
    //   ... code that adds kinky surfaces to CRhinoDoc ...
    // }
    bool m_bSplitCreasedSurfaces;

    // If true, then parent layers control the visible
    // and locked modes of sublayers. Otherwise, layers
    // operate independently.
    bool m_bEnableParentLayerControl;

    // If true, objects with texture mappings that are
    // copied from other objects will get the same 
    // texture mapping.  Otherwise the new object gets
    // a duplicate of the original texture mapping so
    // that the object's mappings can be independently
    // modified.  The default is false.
    bool m_bShareTextureMappings;
    */
  }

  /// <summary>
  /// Defines enumerated constant values for diferent behaviour that is related to clipboard data.
  /// </summary>
  public enum ClipboardState : int
  {
    ///<summary>Always keep clipboard data, regardless of size and never prompt the user.</summary>
    KeepData = 0, //CRhinoAppFileSettings::keep_clipboard_data=0
    ///<summary>Always delete clipboard data, regardless of size and never prompt the user.</summary>
    DeleteData,  // = CRhinoAppFileSettings::delete_clipboard_data,
    ///<summary>Prompt user when clipboard memory is large.</summary>
    PromptWhenBig //= CRhinoAppFileSettings::prompt_user_when_clipboard_big
  }

  /// <summary>
  /// Defines enumerated constant values for particular OSnap cursor colors.
  /// </summary>
  public enum CursorMode : int
  {
    /// <summary>
    /// No OSnap cursor.
    /// </summary>
    None = 0,       // = CRhinoAppModelAidSettings::no_osnap_cursor,

    /// <summary>
    /// Black on white OSnap cursor.
    /// </summary>
    BlackOnWhite, // = CRhinoAppModelAidSettings::black_on_white_osnap_cursor,

    /// <summary>
    /// White on black OSnap cursor.
    /// </summary>
    WhiteOnBlack  // = CRhinoAppModelAidSettings::white_on_black_osnap_cursor
  };

  /// <summary>
  /// Defines several bit masks for each of the OSnap that are defined.
  /// <para>Refer to the Rhino Help file for further information.</para>
  /// </summary>
  [FlagsAttribute]
  public enum OsnapModes : int
  {
    /// <summary>No OSnap.</summary>
    None = 0,
    /// <summary>Near OSnap.</summary>
    Near = 2,
    /// <summary>Focus OSnap.</summary>
    Focus = 8,
    /// <summary>Center OSnap.</summary>
    Center = 0x20,
    /// <summary>Vertex OSnap.</summary>
    Vertex = 0x40,
    /// <summary>Knot OSnap.</summary>
    Knot = 0x80,
    /// <summary>Quadrant OSnap.</summary>
    Quadrant = 0x200,
    /// <summary>Midpoint OSnap.</summary>
    Midpoint = 0x800,
    /// <summary>Intersection OSnap.</summary>
    Intersection = 0x2000,
    /// <summary>End OSnap.</summary>
    End = 0x20000,
    /// <summary>Perpendicular OSnap.</summary>
    Perpendicular = 0x80000,
    /// <summary>Tangent OSnap.</summary>
    Tangent = 0x200000,
    /// <summary>Point OSnap.</summary>
    Point =  0x8000000,
    //All = 0xFFFFFFFF
  };

  /// <summary>
  /// Defines enumerated constant values for world coordinates and CPlane point display modes.
  /// </summary>
  public enum PointDisplayMode : int
  {
    ///<summary>Points are displayed in world coordinates.</summary>
    WorldPoint = 0, // = CRhinoAppModelAidSettings::world_point,
    ///<summary>Points are displayed in CPlane coordinates.</summary>
    CplanePoint     // = CRhinoAppModelAidSettings::cplane_point
  };

  /// <summary>
  /// Represents a snapshot of <see cref="ModelAidSettings"/>.
  /// </summary>
  public class ModelAidSettingsState
  {
    internal ModelAidSettingsState() { }

    ///<summary>Gets or sets the enabled state of Rhino's grid snap modeling aid.</summary>
    public bool GridSnap{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s ortho modeling aid.</summary>
    public bool Ortho{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s Planar modeling aid.</summary>
    public bool Planar{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s Project modeling aid.</summary>
    public bool ProjectSnapToCPlane{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s use horizontal dialog modeling aid.</summary>
    public bool UseHorizontalDialog{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s extend trim lines.</summary>
    public bool ExtendTrimLines{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s extend to apparent intersections.</summary>
    public bool ExtendToApparentIntersection{ get; set; }

    ///<summary>true mean Alt+arrow is used for nudging.</summary>
    public bool AltPlusArrow{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s display control polygon.</summary>
    public bool DisplayControlPolygon{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s highlight dialog modeling aid.</summary>
    public bool HighlightControlPolygon{ get; set; }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s object snap modeling aid.</summary>
    public bool Osnap{ get; set; }

    /// <summary>Gets or sets the locked state of the snap modeling aid.</summary>
    public bool SnapToLocked{ get; set; }

    /// <summary>Gets or sets the locked state of the snap modeling aid.</summary>
    public bool UniversalConstructionPlaneMode{ get; set; }

    /// <summary>Gets or sets the base orthogonal angle.</summary>
    public double OrthoAngle{ get; set; }

    ///<summary>Enables or disables Rhino&apos;s object snap projection.</summary>
    public double NudgeKeyStep{ get; set; }

    /// <summary>Gets or sets the Ctrl-key based nurge step amount.</summary>
    public double CtrlNudgeKeyStep{ get; set; }

    /// <summary>Gets or sets the Shift-key based nurge step amount.</summary>
    public double ShiftNudgeKeyStep{ get; set; }

    ///<summary>Enables or disables Rhino's planar modeling aid.</summary>
    public int OsnapPickboxRadius{ get; set; }

    ///<summary>0 = world, 1 = cplane, 2 = view, 3 = uvn, -1 = not set.</summary>
    public int NudgeMode{ get; set; }

    /// <summary>Gets or sets the control polygon display density.</summary>
    public int ControlPolygonDisplayDensity{ get; set; }

    /// <summary>Gets or sets the OSnap cursor mode.</summary>
    public CursorMode OsnapCursorMode{ get; set; }

    /// <summary>
    /// Returns or sets Rhino's current object snap mode.
    /// <para>The mode is a bitwise value based on the OsnapModes enumeration.</para>
    /// </summary>
    public OsnapModes OsnapModes{ get; set; }

    ///<summary>Gets or sets the radius of the mouse pick box in pixels.</summary>
    public int MousePickboxRadius{ get; set; }

    /// <summary>Gets or sets the point display mode.</summary>
    public PointDisplayMode PointDisplay{ get; set; }
  }

  /// <summary>
  /// Contains static methods and properties to modify model aid settings.
  /// </summary>
  public static class ModelAidSettings
  {
    static ModelAidSettingsState CreateState(bool current)
    {
      IntPtr pSettings = UnsafeNativeMethods.CRhinoAppModelAidSettings_New(current);
      ModelAidSettingsState rc = new ModelAidSettingsState();
      rc.GridSnap = GetBool(idxGridSnap, pSettings);
      rc.Ortho = GetBool(idxOrtho, pSettings);
      rc.Planar = GetBool(idxPlanar, pSettings);
      rc.ProjectSnapToCPlane = GetBool(idxProjectSnapToCPlane, pSettings);
      rc.UseHorizontalDialog = GetBool(idxUseHorizontalDialog, pSettings);
      rc.ExtendTrimLines = GetBool(idxExtendTrimLines, pSettings);
      rc.ExtendToApparentIntersection = GetBool(idxExtendToApparentIntersection, pSettings);
      rc.AltPlusArrow = GetBool(idxAltPlusArrow, pSettings);
      rc.DisplayControlPolygon = GetBool(idxDisplayControlPolygon, pSettings);
      rc.HighlightControlPolygon = GetBool(idxHighlightControlPolygon, pSettings);
      rc.Osnap = !GetBool(idxOsnap, pSettings);
      rc.SnapToLocked = GetBool(idxSnapToLocked, pSettings);
      rc.UniversalConstructionPlaneMode = GetBool(idxUniversalConstructionPlaneMode, pSettings);
      rc.OrthoAngle = GetDouble(idxOrthoAngle, pSettings);
      rc.NudgeKeyStep = GetDouble(idxNudgeKeyStep, pSettings);
      rc.CtrlNudgeKeyStep = GetDouble(idxCtrlNudgeKeyStep, pSettings);
      rc.ShiftNudgeKeyStep = GetDouble(idxShiftNudgeKeyStep, pSettings);
      rc.OsnapPickboxRadius = GetInt(idxOsnapPickboxRadius, pSettings);
      rc.NudgeMode = GetInt(idxNudgeMode, pSettings);
      rc.ControlPolygonDisplayDensity = GetInt(idxControlPolygonDisplayDensity, pSettings);
      rc.OsnapCursorMode = (CursorMode)GetInt(idxOSnapCursorMode, pSettings);
      rc.OsnapModes = (OsnapModes)GetInt(idxOSnapModes, pSettings);
      rc.MousePickboxRadius = GetInt(idxMousePickboxRadius, pSettings);
      rc.PointDisplay = (PointDisplayMode)GetInt(idxPointDisplay, pSettings);

      UnsafeNativeMethods.CRhinoAppModelAidSettings_Delete(pSettings);
      return rc;
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    /// <returns>A new model aid state with current settings.</returns>
    public static ModelAidSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Gets the factory settings.
    /// </summary>
    /// <returns>A new model aid state with factory settings.</returns>
    public static ModelAidSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Updates from a particular setting state.
    /// </summary>
    /// <param name="state">The new states that will be set.</param>
    public static void UpdateFromState(ModelAidSettingsState state)
    {
      GridSnap = state.GridSnap;
      Ortho = state.Ortho;
      Planar = state.Planar;
      ProjectSnapToCPlane = state.ProjectSnapToCPlane;
      UseHorizontalDialog = state.UseHorizontalDialog;
      ExtendTrimLines = state.ExtendTrimLines;
      ExtendToApparentIntersection = state.ExtendToApparentIntersection;
      AltPlusArrow = state.AltPlusArrow;
      DisplayControlPolygon = state.DisplayControlPolygon;
      HighlightControlPolygon = state.HighlightControlPolygon;
      Osnap = state.Osnap;
      SnapToLocked = state.SnapToLocked;
      UniversalConstructionPlaneMode = state.UniversalConstructionPlaneMode;
      OrthoAngle = state.OrthoAngle;
      NudgeKeyStep = state.NudgeKeyStep;
      CtrlNudgeKeyStep = state.CtrlNudgeKeyStep;
      ShiftNudgeKeyStep = state.ShiftNudgeKeyStep;
      OsnapPickboxRadius = state.OsnapPickboxRadius;
      NudgeMode = state.NudgeMode;
      ControlPolygonDisplayDensity = state.ControlPolygonDisplayDensity;
      OsnapCursorMode = state.OsnapCursorMode;
      OsnapModes = state.OsnapModes;
      MousePickboxRadius = state.MousePickboxRadius;
      PointDisplay = state.PointDisplay;
    }

    static bool GetBool(int which, IntPtr pSettings)
    {
      return UnsafeNativeMethods.RhModelAidSettings_GetSetBool(which, false, false, pSettings);
    }
    static bool GetBool(int which) { return GetBool(which, IntPtr.Zero); }
    static void SetBool(int which, bool b) { UnsafeNativeMethods.RhModelAidSettings_GetSetBool(which, true, b, IntPtr.Zero); }
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
    const int idxShowAutoGumball = 13;
    const int idxSnappyGumball = 14;

    ///<summary>Gets or sets the enabled state of Rhino's grid snap modeling aid.</summary>
    public static bool GridSnap
    {
      get { return GetBool(idxGridSnap); }
      set { SetBool(idxGridSnap, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s ortho modeling aid.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_ortho.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_ortho.cs' lang='cs'/>
    /// <code source='examples\py\ex_ortho.py' lang='py'/>
    /// </example>
    public static bool Ortho
    {
      get { return GetBool(idxOrtho); }
      set { SetBool(idxOrtho, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s Planar modeling aid.</summary>
    public static bool Planar
    {
      get { return GetBool(idxPlanar); }
      set { SetBool(idxPlanar, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s Project modeling aid.</summary>
    public static bool ProjectSnapToCPlane
    {
      get { return GetBool(idxProjectSnapToCPlane); }
      set { SetBool(idxProjectSnapToCPlane, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s use horizontal dialog modeling aid.</summary>
    public static bool UseHorizontalDialog
    {
      get { return GetBool(idxUseHorizontalDialog); }
      set { SetBool(idxUseHorizontalDialog, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s extend trim lines.</summary>
    public static bool ExtendTrimLines
    {
      get { return GetBool(idxExtendTrimLines); }
      set { SetBool(idxExtendTrimLines, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s extend to apparent intersections.</summary>
    public static bool ExtendToApparentIntersection
    {
      get { return GetBool(idxExtendToApparentIntersection); }
      set { SetBool(idxExtendToApparentIntersection, value); }
    }

    ///<summary>true means Alt+arrow is used for nudging.</summary>
    public static bool AltPlusArrow
    {
      get { return GetBool(idxAltPlusArrow); }
      set { SetBool(idxAltPlusArrow, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s display control polygon.</summary>
    public static bool DisplayControlPolygon
    {
      get { return GetBool(idxDisplayControlPolygon); }
      set { SetBool(idxDisplayControlPolygon, value); }
    }

    ///<summary>Gets or sets the enabled state of Rhino&apos;s highlight dialog modeling aid.</summary>
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

    /// <summary>Gets or sets the locked state of the snap modeling aid.</summary>
    public static bool SnapToLocked
    {
      get { return GetBool(idxSnapToLocked); }
      set { SetBool(idxSnapToLocked, value); }
    }

    /// <summary>Gets or sets the locked state of the snap modeling aid.</summary>
    public static bool UniversalConstructionPlaneMode
    {
      get { return GetBool(idxUniversalConstructionPlaneMode); }
      set { SetBool(idxUniversalConstructionPlaneMode, value); }
    }

    static double GetDouble(int which, IntPtr pSettings) { return UnsafeNativeMethods.RhModelAidSettings_GetSetDouble(which, false, 0, pSettings); }
    static double GetDouble(int which) { return GetDouble(which, IntPtr.Zero); }
    static void SetDouble(int which, double d) { UnsafeNativeMethods.RhModelAidSettings_GetSetDouble(which, true, d, IntPtr.Zero); }
    const int idxOrthoAngle = 0;
    const int idxNudgeKeyStep = 1;
    const int idxCtrlNudgeKeyStep = 2;
    const int idxShiftNudgeKeyStep = 3;

    /// <summary>Gets or sets the base orthogonal angle.</summary>
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

    /// <summary>Gets or sets the Ctrl-key based nurge step amount.</summary>
    public static double CtrlNudgeKeyStep
    {
      get { return GetDouble(idxCtrlNudgeKeyStep); }
      set { SetDouble(idxCtrlNudgeKeyStep, value); }
    }

    /// <summary>Gets or sets the Shift-key based nurge step amount.</summary>
    public static double ShiftNudgeKeyStep
    {
      get { return GetDouble(idxShiftNudgeKeyStep); }
      set { SetDouble(idxShiftNudgeKeyStep, value); }
    }

    static int GetInt(int which, IntPtr pSettings) { return UnsafeNativeMethods.RhModelAidSettings_GetSetInt(which, false, 0, pSettings); }
    static int GetInt(int which) { return GetInt(which, IntPtr.Zero); }
    static void SetInt(int which, int i) { UnsafeNativeMethods.RhModelAidSettings_GetSetInt(which, true, i, IntPtr.Zero); }
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
    ///<summary>0 = world, 1 = cplane, 2 = view, 3 = uvn, -1 = not set.</summary>
    public static int NudgeMode
    {
      get { return GetInt(idxNudgeMode); }
      set { SetInt(idxNudgeMode, value); }
    }

    /// <summary>Gets or sets the control polygon display density.</summary>
    public static int ControlPolygonDisplayDensity
    {
      get { return GetInt(idxControlPolygonDisplayDensity); }
      set { SetInt(idxControlPolygonDisplayDensity, value); }
    }

    /// <summary>Gets or sets the OSnap cursor mode.</summary>
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
    ///<summary>radius of mouse pick box in pixels.</summary>
    public static int MousePickboxRadius
    {
      get { return GetInt(idxMousePickboxRadius); }
      set { SetInt(idxMousePickboxRadius, value); }
    }

    /// <summary>Gets or sets the point display mode.</summary>
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

    /// <summary>
    /// When AutoGumball is on, a gumball automatically appears
    /// when objects are prepicked.
    /// </summary>
    public static bool AutoGumballEnabled
    {
      get
      {
        return GetBool(idxShowAutoGumball);
      }
      set
      {
        SetBool(idxShowAutoGumball, value);
      }
    }

    /// <summary>
    /// When SnappyGumball is on, a dragging a gumball moves the center point.
    /// When snappy gumball is off, dragging a gumball moves the mouse down point.
    /// </summary>
    public static bool SnappyGumballEnabled
    {
      get
      {
        return GetBool(idxSnappyGumball);
      }
      set
      {
        SetBool(idxSnappyGumball, value);
      }
    }
  }

  /// <summary>
  /// Represents a snapshot of <see cref="ViewSettings"/>.
  /// </summary>
  public class ViewSettingsState
  {
    internal ViewSettingsState() { }

    /// <summary>Gets or sets the faction used as multiplier to pan the screen.</summary>
    public double PanScreenFraction { get; set; }

    /// <summary>Gets or sets if panning with the keyboard is reversed.
    /// <para>false, then Rhino pans the camera in the direction of the arrow key you press.
    /// true, then Rhino pans the scene instead.</para></summary>
    public bool PanReverseKeyboardAction { get; set; }

    /// <summary>Gets or sets the 'always pan parallel views' value.
    /// <para>If the view is not looking straight at the construction plane, then
    /// sets parallel viewports so they will not rotate.</para></summary>
    public bool AlwaysPanParallelViews { get; set; }

    /// <summary>
    /// Gets or sets the step size for zooming with a wheeled mouse or the Page Up and Page Down keys.
    /// </summary>
    public double ZoomScale { get; set; }

    /// <summary>
    /// Gets or sets the rotation increment.
    /// <para>When the user rotates a view with the keyboard, Rhino rotates the view in steps.
    /// The usual step is 1/60th of a circle, which equals six degrees.</para>
    /// </summary>
    public int RotateCircleIncrement { get; set; }

    /// <summary>
    /// Gets or sets the rotation direction.
    /// <para>If true, then Rhino rotates the camera around the scene, otherwise, rotates the scene itself.</para>
    /// </summary>
    public bool RotateReverseKeyboard { get; set; }

    /// <summary>
    /// Gets or sets the rotation reference.
    /// <para>If true, then the views rotates relative to the view axes; false, than relative to the world x, y, and z axes.</para>
    /// </summary>
    public bool RotateToView { get; set; }

    /// <summary>
    /// Gets or sets the 'named views set CPlane' value.
    /// <para>When true, restoring a named view causes the construction plane saved with that view to also restore.</para>
    /// </summary>
    public bool DefinedViewSetCPlane { get; set; }

    /// <summary>
    /// Gets or sets the 'named views set projection' value.
    /// <para>When true, restoring a named view causes the viewport projection saved with the view to also restore.</para>
    /// </summary>
    public bool DefinedViewSetProjection { get; set; }

    /// <summary>
    /// Gets or sets the 'single-click maximize' value.
    /// <para>When true, maximizing a viewport needs a single click on the viewport title rather than a double-click.</para>
    /// </summary>
    public bool SingleClickMaximize{ get; set; }

    /// <summary>
    /// Gets or sets the 'linked views' activated setting.
    /// <para>true enables real-time view synchronization.
    /// When a standard view is manipulated, the camera lens length of all parallel projection
    /// viewports are set to match the current viewport.</para>
    /// </summary>
    public bool LinkedViewports { get; set; }
  }

  /// <summary>
  /// Contains static methods and properties to control view settings.
  /// </summary>
  public static class ViewSettings
  {
    static ViewSettingsState CreateState(bool current)
    {
      IntPtr pViewSettings = UnsafeNativeMethods.CRhinoAppViewSettings_New(current);
      ViewSettingsState rc = new ViewSettingsState();
      rc.AlwaysPanParallelViews = GetBool(idxAlwaysPanParallelViews, pViewSettings);
      rc.DefinedViewSetCPlane = GetBool(idxDefinedViewSetCPlane, pViewSettings);
      rc.DefinedViewSetProjection = GetBool(idxDefinedViewSetProjection, pViewSettings);
      rc.LinkedViewports = GetBool(idxLinkedViewports, pViewSettings);
      rc.PanReverseKeyboardAction = GetBool(idxPanReverseKeyboardAction, pViewSettings);
      rc.PanScreenFraction = GetDouble(idxPanScreenFraction, pViewSettings);
      rc.RotateCircleIncrement = UnsafeNativeMethods.CRhinoAppViewSettings_GetSetInt(idxRotateCircleIncrement, false, 0, pViewSettings);
      rc.RotateReverseKeyboard = GetBool(idxRotateReverseKeyboard, pViewSettings);
      rc.RotateToView = GetBool(idxRotateToView, pViewSettings);
      rc.SingleClickMaximize = GetBool(idxSingleClickMaximize, pViewSettings);
      rc.ZoomScale = GetDouble(idxZoomScale, pViewSettings);
      
      UnsafeNativeMethods.CRhinoAppViewSettings_Delete(pViewSettings);
      return rc;
    }

    /// <summary>
    /// Gets the view factory settings.
    /// </summary>
    /// <returns>A new view state with factory settings.</returns>
    public static ViewSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    /// <returns>A new view state with current settings.</returns>
    public static ViewSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Updates from the default setting state.
    /// </summary>
    public static void RestoreDefaults()
    {
      UpdateFromState(GetDefaultState());
    }

    /// <summary>
    /// Updates from a particular setting state.
    /// </summary>
    /// <param name="state">The new state that will be set.</param>
    public static void UpdateFromState(ViewSettingsState state)
    {
      AlwaysPanParallelViews = state.AlwaysPanParallelViews;
      DefinedViewSetCPlane = state.DefinedViewSetCPlane;
      DefinedViewSetProjection = state.DefinedViewSetProjection;
      LinkedViewports = state.LinkedViewports;
      PanReverseKeyboardAction = state.PanReverseKeyboardAction;
      PanScreenFraction = state.PanScreenFraction;
      RotateCircleIncrement = state.RotateCircleIncrement;
      RotateReverseKeyboard = state.RotateReverseKeyboard;
      RotateToView = state.RotateToView;
      SingleClickMaximize = state.SingleClickMaximize;
      ZoomScale = state.ZoomScale;
    }


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

    static double GetDouble(int which, IntPtr pViewSettings)
    {
      return UnsafeNativeMethods.CRhinoAppViewSettings_GetSetDouble(which, false, 0, pViewSettings);
    }
    static void SetDouble(int which, double d, IntPtr pViewSettings)
    {
      UnsafeNativeMethods.CRhinoAppViewSettings_GetSetDouble(which, true, d, pViewSettings);
    }
    static bool GetBool(int which, IntPtr pViewSettings)
    {
      return UnsafeNativeMethods.CRhinoAppViewSettings_GetSetBool(which, false, false, pViewSettings);
    }
    static void SetBool(int which, bool b, IntPtr pViewSettings)
    {
      UnsafeNativeMethods.CRhinoAppViewSettings_GetSetBool(which, true, b, pViewSettings);
    }
    static bool GetBool(int which) { return GetBool(which, IntPtr.Zero); }
    static void SetBool(int which, bool b) { SetBool(which, b, IntPtr.Zero); }
    static double GetDouble(int which) { return GetDouble(which, IntPtr.Zero); }
    static void SetDouble(int which, double d) { SetDouble(which, d, IntPtr.Zero); }

    /// <summary>Gets or sets the faction used as multiplier to pan the screen.</summary>
    public static double PanScreenFraction
    {
      get { return GetDouble(idxPanScreenFraction); }
      set { SetDouble(idxPanScreenFraction, value); }
    }

    /// <summary>Gets or sets if panning with the keyboard is reversed.
    /// <para>false, then Rhino pans the camera in the direction of the arrow key you press.
    /// true, then Rhino pans the scene instead.</para></summary>
    public static bool PanReverseKeyboardAction
    {
      get { return GetBool(idxPanReverseKeyboardAction); }
      set { SetBool(idxPanReverseKeyboardAction, value); }
    }

    /// <summary>Gets or sets the 'always pan parallel views' value.
    /// <para>If the view is not looking straight at the construction plane, then
    /// sets parallel viewports so they will not rotate.</para></summary>
    public static bool AlwaysPanParallelViews
    {
      get { return GetBool(idxAlwaysPanParallelViews); }
      set { SetBool(idxAlwaysPanParallelViews, value); }
    }

    /// <summary>
    /// Gets or sets the step size for zooming with a wheeled mouse or the Page Up and Page Down keys.
    /// </summary>
    public static double ZoomScale
    {
      get { return GetDouble(idxZoomScale); }
      set { SetDouble(idxZoomScale, value); }
    }

    /// <summary>
    /// Gets or sets the rotation increment.
    /// <para>When the user rotates a view with the keyboard, Rhino rotates the view in steps.
    /// The usual step is 1/60th of a circle, which equals six degrees.</para>
    /// </summary>
    public static int RotateCircleIncrement
    {
      get
      {
        return UnsafeNativeMethods.CRhinoAppViewSettings_GetSetInt(idxRotateCircleIncrement, false, 0, IntPtr.Zero);
      }
      set
      {
        UnsafeNativeMethods.CRhinoAppViewSettings_GetSetInt(idxRotateCircleIncrement, true, value, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Gets or sets the rotation direction.
    /// <para>If true, then Rhino rotates the camera around the scene, otherwise, rotates the scene itself.</para>
    /// </summary>
    public static bool RotateReverseKeyboard
    {
      get { return GetBool(idxRotateReverseKeyboard); }
      set { SetBool(idxRotateReverseKeyboard, value); }
    }

    /// <summary>
    /// Gets or sets the rotation reference.
    /// <para>If true, then the views rotates relative to the view axes; false, than relative to the world x, y, and z axes.</para>
    /// </summary>
    public static bool RotateToView
    {
      get { return GetBool(idxRotateToView); }
      set { SetBool(idxRotateToView, value); }
    }

    /// <summary>
    /// Gets or sets the 'named views set CPlane' value.
    /// <para>When true, restoring a named view causes the construction plane saved with that view to also restore.</para>
    /// </summary>
    public static bool DefinedViewSetCPlane
    {
      get { return GetBool(idxDefinedViewSetCPlane); }
      set { SetBool(idxDefinedViewSetCPlane, value); }
    }

    /// <summary>
    /// Gets or sets the 'named views set projection' value.
    /// <para>When true, restoring a named view causes the viewport projection saved with the view to also restore.</para>
    /// </summary>
    public static bool DefinedViewSetProjection
    {
      get { return GetBool(idxDefinedViewSetProjection); }
      set { SetBool(idxDefinedViewSetProjection, value); }
    }

    /// <summary>
    /// Gets or sets the 'single-click maximize' value.
    /// <para>When true, maximizing a viewport needs a single click on the viewport title rather than a double-click.</para>
    /// </summary>
    public static bool SingleClickMaximize
    {
      get { return GetBool(idxSingleClickMaximize); }
      set { SetBool(idxSingleClickMaximize, value); }
    }

    /// <summary>
    /// Gets or sets the 'linked views' activated setting.
    /// <para>true enables real-time view synchronization.
    /// When a standard view is manipulated, the camera lens length of all parallel projection
    /// viewports are set to match the current viewport.</para>
    /// </summary>
    public static bool LinkedViewports
    {
      get { return GetBool(idxLinkedViewports); }
      set { SetBool(idxLinkedViewports, value); }
    }
  }

  /// <summary>
  /// Shortcut key combinations
  /// </summary>
  public enum ShortcutKey : int
  {
    /// <summary>F1</summary>
    F1 = 0,
    /// <summary>F2</summary>
    F2,
    /// <summary>F3</summary>
    F3,
    /// <summary>F4</summary>
    F4,
    /// <summary>F5</summary>
    F5,
    /// <summary>F6</summary>
    F6,
    /// <summary>F7</summary>
    F7,
    /// <summary>F8</summary>
    F8,
    /// <summary>F9</summary>
    F9,
    /// <summary>F10</summary>
    F10,
    /// <summary>F11</summary>
    F11,
    /// <summary>F12</summary>
    F12,
    /// <summary>Ctrl + F1</summary>
    CtrlF1,
    /// <summary>Ctrl + F2</summary>
    CtrlF2,
    /// <summary>Ctrl + F3</summary>
    CtrlF3,
    /// <summary>Ctrl + F4</summary>
    CtrlF4,
    /// <summary>Ctrl + F5</summary>
    CtrlF5,
    /// <summary>Ctrl + F6</summary>
    CtrlF6,
    /// <summary>Ctrl + F7</summary>
    CtrlF7,
    /// <summary>Ctrl + F8</summary>
    CtrlF8,
    /// <summary>Ctrl + F9</summary>
    CtrlF9,
    /// <summary>Ctrl + F10</summary>
    CtrlF10,
    /// <summary>Ctrl + F11</summary>
    CtrlF11,
    /// <summary>Ctrl + F12</summary>
    CtrlF12,
    /// <summary>Shift + Ctrl + F1</summary>
    ShiftCtrlF1,
    /// <summary>Shift + Ctrl + F2</summary>
    ShiftCtrlF2,
    /// <summary>Shift + Ctrl + F3</summary>
    ShiftCtrlF3,
    /// <summary>Shift + Ctrl + F4</summary>
    ShiftCtrlF4,
    /// <summary>Shift + Ctrl + F5</summary>
    ShiftCtrlF5,
    /// <summary>Shift + Ctrl + F6</summary>
    ShiftCtrlF6,
    /// <summary>Shift + Ctrl + F7</summary>
    ShiftCtrlF7,
    /// <summary>Shift + Ctrl + F8</summary>
    ShiftCtrlF8,
    /// <summary>Shift + Ctrl + F9</summary>
    ShiftCtrlF9,
    /// <summary>Shift + Ctrl + F10</summary>
    ShiftCtrlF10,
    /// <summary>Shift + Ctrl + F11</summary>
    ShiftCtrlF11,
    /// <summary>Shift + Ctrl + F12</summary>
    ShiftCtrlF12,
    /// <summary>Alt + Ctrl + F1</summary>
    AltCtrlF1,
    /// <summary>Alt + Ctrl + F2</summary>
    AltCtrlF2,
    /// <summary>Alt + Ctrl + F3</summary>
    AltCtrlF3,
    /// <summary>Alt + Ctrl + F4</summary>
    AltCtrlF4,
    /// <summary>Alt + Ctrl + F5</summary>
    AltCtrlF5,
    /// <summary>Alt + Ctrl + F6</summary>
    AltCtrlF6,
    /// <summary>Alt + Ctrl + F7</summary>
    AltCtrlF7,
    /// <summary>Alt + Ctrl + F8</summary>
    AltCtrlF8,
    /// <summary>Alt + Ctrl + F9</summary>
    AltCtrlF9,
    /// <summary>Alt + Ctrl + F10</summary>
    AltCtrlF10,
    /// <summary>Alt + Ctrl + F11</summary>
    AltCtrlF11,
    /// <summary>Alt + Ctrl + F12</summary>
    AltCtrlF12,
    /// <summary>Ctrl + A</summary>
    CtrlA,
    /// <summary>Ctrl + B</summary>
    CtrlB,
    /// <summary>Ctrl + C</summary>
    CtrlC,
    /// <summary>Ctrl + D</summary>
    CtrlD,
    /// <summary>Ctrl + E</summary>
    CtrlE,
    /// <summary>Ctrl + F</summary>
    CtrlF,
    /// <summary>Ctrl + G</summary>
    CtrlG,
    /// <summary>Ctrl + H</summary>
    CtrlH,
    /// <summary>Ctrl + I</summary>
    CtrlI,
    /// <summary>Ctrl + J</summary>
    CtrlJ,
    /// <summary>Ctrl + K</summary>
    CtrlK,
    /// <summary>Ctrl + L</summary>
    CtrlL,
    /// <summary>Ctrl + M</summary>
    CtrlM,
    /// <summary>Ctrl + N</summary>
    CtrlN,
    /// <summary>Ctrl + O</summary>
    CtrlO,
    /// <summary>Ctrl + P</summary>
    CtrlP,
    /// <summary>Ctrl + Q</summary>
    CtrlQ,
    /// <summary>Ctrl + R</summary>
    CtrlR,
    /// <summary>Ctrl + S</summary>
    CtrlS,
    /// <summary>Ctrl + T</summary>
    CtrlT,
    /// <summary>Ctrl + U</summary>
    CtrlU,
    /// <summary>Ctrl + V</summary>
    CtrlV,
    /// <summary>Ctrl + W</summary>
    CtrlW,
    /// <summary>Ctrl + X</summary>
    CtrlX,
    /// <summary>Ctrl + Y</summary>
    CtrlY,
    /// <summary>Ctrl + Z</summary>
    CtrlZ,
    /// <summary>Shift + Ctrl + A</summary>
    ShiftCtrlA,
    /// <summary>Shift + Ctrl + B</summary>
    ShiftCtrlB,
    /// <summary>Shift + Ctrl + C</summary>
    ShiftCtrlC,
    /// <summary>Shift + Ctrl + D</summary>
    ShiftCtrlD,
    /// <summary>Shift + Ctrl + E</summary>
    ShiftCtrlE,
    /// <summary>Shift + Ctrl + F</summary>
    ShiftCtrlF,
    /// <summary>Shift + Ctrl + G</summary>
    ShiftCtrlG,
    /// <summary>Shift + Ctrl + H</summary>
    ShiftCtrlH,
    /// <summary>Shift + Ctrl + I</summary>
    ShiftCtrlI,
    /// <summary>Shift + Ctrl + J</summary>
    ShiftCtrlJ,
    /// <summary>Shift + Ctrl + K</summary>
    ShiftCtrlK,
    /// <summary>Shift + Ctrl + L</summary>
    ShiftCtrlL,
    /// <summary>Shift + Ctrl + M</summary>
    ShiftCtrlM,
    /// <summary>Shift + Ctrl + N</summary>
    ShiftCtrlN,
    /// <summary>Shift + Ctrl + O</summary>
    ShiftCtrlO,
    /// <summary>Shift + Ctrl + P</summary>
    ShiftCtrlP,
    /// <summary>Shift + Ctrl + Q</summary>
    ShiftCtrlQ,
    /// <summary>Shift + Ctrl + R</summary>
    ShiftCtrlR,
    /// <summary>Shift + Ctrl + S</summary>
    ShiftCtrlS,
    /// <summary>Shift + Ctrl + T</summary>
    ShiftCtrlT,
    /// <summary>Shift + Ctrl + U</summary>
    ShiftCtrlU,
    /// <summary>Shift + Ctrl + V</summary>
    ShiftCtrlV,
    /// <summary>Shift + Ctrl + W</summary>
    ShiftCtrlW,
    /// <summary>Shift + Ctrl + X</summary>
    ShiftCtrlX,
    /// <summary>Shift + Ctrl + Y</summary>
    ShiftCtrlY,
    /// <summary>Shift + Ctrl + Z</summary>
    ShiftCtrlZ,
    /// <summary>Alt + Ctrl + A</summary>
    AltCtrlA,
    /// <summary>Alt + Ctrl + B</summary>
    AltCtrlB,
    /// <summary>Alt + Ctrl + C</summary>
    AltCtrlC,
    /// <summary>Alt + Ctrl + D</summary>
    AltCtrlD,
    /// <summary>Alt + Ctrl + E</summary>
    AltCtrlE,
    /// <summary>Alt + Ctrl + F</summary>
    AltCtrlF,
    /// <summary>Alt + Ctrl + G</summary>
    AltCtrlG,
    /// <summary>Alt + Ctrl + H</summary>
    AltCtrlH,
    /// <summary>Alt + Ctrl + I</summary>
    AltCtrlI,
    /// <summary>Alt + Ctrl + J</summary>
    AltCtrlJ,
    /// <summary>Alt + Ctrl + K</summary>
    AltCtrlK,
    /// <summary>Alt + Ctrl + L</summary>
    AltCtrlL,
    /// <summary>Alt + Ctrl + M</summary>
    AltCtrlM,
    /// <summary>Alt + Ctrl + N</summary>
    AltCtrlN,
    /// <summary>Alt + Ctrl + O</summary>
    AltCtrlO,
    /// <summary>Alt + Ctrl + P</summary>
    AltCtrlP,
    /// <summary>Alt + Ctrl + Q</summary>
    AltCtrlQ,
    /// <summary>Alt + Ctrl + R</summary>
    AltCtrlR,
    /// <summary>Alt + Ctrl + S</summary>
    AltCtrlS,
    /// <summary>Alt + Ctrl + T</summary>
    AltCtrlT,
    /// <summary>Alt + Ctrl + U</summary>
    AltCtrlU,
    /// <summary>Alt + Ctrl + V</summary>
    AltCtrlV,
    /// <summary>Alt + Ctrl + W</summary>
    AltCtrlW,
    /// <summary>Alt + Ctrl + X</summary>
    AltCtrlX,
    /// <summary>Alt + Ctrl + Y</summary>
    AltCtrlY,
    /// <summary>Alt + Ctrl + Z</summary>
    AltCtrlZ,
    /// <summary>Ctrl + 0</summary>
    Ctrl0,
    /// <summary>Ctrl + 1</summary>
    Ctrl1,
    /// <summary>Ctrl + 2</summary>
    Ctrl2,
    /// <summary>Ctrl + 3</summary>
    Ctrl3,
    /// <summary>Ctrl + 4</summary>
    Ctrl4,
    /// <summary>Ctrl + 5</summary>
    Ctrl5,
    /// <summary>Ctrl + 6</summary>
    Ctrl6,
    /// <summary>Ctrl + 7</summary>
    Ctrl7,
    /// <summary>Ctrl + 8</summary>
    Ctrl8,
    /// <summary>Ctrl + 9</summary>
    Ctrl9,
    /// <summary>Shift + Ctrl + 0</summary>
    ShiftCtrl0,
    /// <summary>Shift + Ctrl + 1</summary>
    ShiftCtrl1,
    /// <summary>Shift + Ctrl + 2</summary>
    ShiftCtrl2,
    /// <summary>Shift + Ctrl + 3</summary>
    ShiftCtrl3,
    /// <summary>Shift + Ctrl + 4</summary>
    ShiftCtrl4,
    /// <summary>Shift + Ctrl + 5</summary>
    ShiftCtrl5,
    /// <summary>Shift + Ctrl + 6</summary>
    ShiftCtrl6,
    /// <summary>Shift + Ctrl + 7</summary>
    ShiftCtrl7,
    /// <summary>Shift + Ctrl + 8</summary>
    ShiftCtrl8,
    /// <summary>Shift + Ctrl + 9</summary>
    ShiftCtrl9,
    /// <summary>Alt + Ctrl + 0</summary>
    AltCtrl0,
    /// <summary>Alt + Ctrl + 1</summary>
    AltCtrl1,
    /// <summary>Alt + Ctrl + 2</summary>
    AltCtrl2,
    /// <summary>Alt + Ctrl + 3</summary>
    AltCtrl3,
    /// <summary>Alt + Ctrl + 4</summary>
    AltCtrl4,
    /// <summary>Alt + Ctrl + 5</summary>
    AltCtrl5,
    /// <summary>Alt + Ctrl + 6</summary>
    AltCtrl6,
    /// <summary>Alt + Ctrl + 7</summary>
    AltCtrl7,
    /// <summary>Alt + Ctrl + 8</summary>
    AltCtrl8,
    /// <summary>Alt + Ctrl + 9</summary>
    AltCtrl9,
    /// <summary>Home</summary>
    Home,
    /// <summary>End</summary>
    End,
    /// <summary>Ctrl + Home</summary>
    CtrlHome,
    /// <summary>Ctrl + End</summary>
    CtrlEnd,
    /// <summary>Shift + Home</summary>
    ShiftHome,
    /// <summary>Shift + End</summary>
    ShiftEnd,
    /// <summary>Shift + Ctrl + Home</summary>
    ShiftCtrlHome,
    /// <summary>Shift + Ctrl + End</summary>
    ShiftCtrlEnd,
    /// <summary>Alt + Ctrl + Home</summary>
    AltCtrlHome,
    /// <summary>Alt + Ctrl + End</summary>
    AltCtrlEnd,
    /// <summary>Page Up</summary>
    PageUp,
    /// <summary>Page Down</summary>
    PageDown,
    /// <summary>Shift + Page Up</summary>
    ShiftPageUp,
    /// <summary>Shift + Page Down</summary>
    ShiftPageDown,
    /// <summary>Ctrl + Page Up</summary>
    CtrlPageUp,
    /// <summary>Ctrl + Page Down</summary>
    CtrlPageDown,
    /// <summary>Shift + Ctrl + Page Up</summary>
    ShiftCtrlPageUp,
    /// <summary>Shift + Ctrl + Page Down</summary>
    ShiftCtrlPageDown,
    /// <summary>Alt + Ctrl + Page Up</summary>
    AltCtrlPageUp,
    /// <summary>Alt + Ctrl + Page Down</summary>
    AltCtrlPageDown
  }

  /// <summary>
  /// Contains static methods and properties to control keyboard shortcut keys
  /// </summary>
  public static class ShortcutKeySettings
  {
    /// <summary>
    /// Get macro associated with a given keyboard shortcut
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string GetMacro(ShortcutKey key)
    {
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CRhinoAppShortcutKeys_Macro((int)key, pString);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Set macro associated with a keyboard shortcut
    /// </summary>
    /// <param name="key"></param>
    /// <param name="macro"></param>
    public static void SetMacro(ShortcutKey key, string macro)
    {
      UnsafeNativeMethods.CRhinoAppShortcutKeys_SetMacro((int)key, macro);
    }
  }

  /// <summary>
  /// Represents a snapshot of <see cref="SmartTrackSettings"/>.
  /// </summary>
  public class SmartTrackSettingsState
  {
    internal SmartTrackSettingsState() { }

    /// <summary>Gets or sets if the 'smart track' feature is active.</summary>
    public bool UseSmartTrack { get; set; }

    /// <summary>Gets or sets a value indicating if lines are drawn dotted.</summary>
    public bool UseDottedLines { get; set; }

    /// <summary>Gets or sets a value indicating if the 'Smart Ortho' feature is active.</summary>
    public bool SmartOrtho { get; set; }

    /// <summary>Gets or sets a value indicating if the 'Smart Tangents' feature is active.</summary>
    public bool SmartTangents { get; set; }

    /// <summary>Gets or sets the activation delay in milliseconds.</summary>
    public int ActivationDelayMilliseconds { get; set; }

    /// <summary>Gets or sets the maximum number of smart points.</summary>
    public static int MaxSmartPoints { get; set; }

    /// <summary>Gets or sets the smart track line color.</summary>
    public Color LineColor { get; set; }

    /// <summary>Gets or sets the tangent and perpendicular line color.</summary>
    public Color TanPerpLineColor { get; set; }

    /// <summary>Gets or sets the point color.</summary>
    public Color PointColor { get; set; }

    /// <summary>Gets or sets the active point color.</summary>
    public Color ActivePointColor { get; set; }
  }

  /// <summary>
  /// Contains static methods and properties that target the Smart Track feature behavior.
  /// </summary>
  public static class SmartTrackSettings
  {
    static SmartTrackSettingsState CreateState(bool current)
    {
      IntPtr pSettings = UnsafeNativeMethods.CRhinoAppSmartTrackSettings_New(current);
      SmartTrackSettingsState rc = new SmartTrackSettingsState();
      rc.ActivationDelayMilliseconds = UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetInt(true, pSettings);
      rc.ActivePointColor = GetColor(idxActivePointColor, pSettings);
      rc.LineColor = GetColor(idxLineColor, pSettings);
      rc.PointColor = GetColor(idxPointColor, pSettings);
      rc.SmartOrtho = GetBool(idxSmartOrtho, pSettings);
      rc.SmartTangents = GetBool(idxSmartTangents, pSettings);
      rc.TanPerpLineColor = GetColor(idxTanPerpLineColor, pSettings);
      rc.UseDottedLines = GetBool(idxDottedLines, pSettings);
      rc.UseSmartTrack = GetBool(idxUseSmartTrack, pSettings);

      UnsafeNativeMethods.CRhinoAppSmartTrackSettings_Delete(pSettings);
      return rc;
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    /// <returns>A new Smart Track state with current settings.</returns>
    public static SmartTrackSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Gets the Smart Track factory settings.
    /// </summary>
    /// <returns>A new Smart Track state with factory settings.</returns>
    public static SmartTrackSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Updates from a particular setting state.
    /// </summary>
    /// <param name="state">The new state that will be set.</param>
    public static void UpdateFromState(SmartTrackSettingsState state)
    {
      ActivationDelayMilliseconds = state.ActivationDelayMilliseconds;
      ActivePointColor = state.ActivePointColor;
      LineColor = state.LineColor;
      PointColor = state.PointColor;
      SmartOrtho = state.SmartOrtho;
      SmartTangents = state.SmartTangents;
      TanPerpLineColor = state.TanPerpLineColor;
      UseDottedLines = state.UseDottedLines;
      UseSmartTrack = state.UseSmartTrack;
    }

    const int idxUseSmartTrack = 0;
    const int idxDottedLines = 1;
    const int idxSmartOrtho = 2;
    const int idxSmartTangents = 3;
    // skipping the following until we can come up with good
    // descriptions of what each does
    //BOOL m_bMarkerSmartPoint;
    //BOOL m_bSmartSuppress;
    //BOOL m_bStrongOrtho;
    //BOOL m_bSemiPermanentPoints;
    //BOOL m_bShowMultipleTypes;
    //BOOL m_bParallels;
    //BOOL m_bSmartBasePoint;

    static bool GetBool(int which, IntPtr pSmartTrackSettings)
    {
      return UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetSetBool(which, false, false, pSmartTrackSettings);
    }
    static bool GetBool(int which) { return GetBool(which, IntPtr.Zero); }
    static void SetBool(int which, bool b, IntPtr pSmartTrackSettings)
    {
      UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetSetBool(which, true, b, pSmartTrackSettings);
    }
    static void SetBool(int which, bool b) { SetBool(which, b, IntPtr.Zero); }

    /// <summary>Gets or sets if the Smart Track feature is active.</summary>
    public static bool UseSmartTrack
    {
      get { return GetBool(idxUseSmartTrack); }
      set { SetBool(idxUseSmartTrack, value); }
    }

    /// <summary>Gets or sets a value indicating if lines are drawn dotted.</summary>
    public static bool UseDottedLines
    {
      get { return GetBool(idxDottedLines); }
      set { SetBool(idxDottedLines, value); }
    }

    /// <summary>Gets or sets a value indicating if the 'Smart Ortho' feature is active.
    /// <para>Orthogonal lines are then drawn automatically.</para></summary>
    public static bool SmartOrtho
    {
      get { return GetBool(idxSmartOrtho); }
      set { SetBool(idxSmartOrtho, value); }
    }

    /// <summary>Gets or sets a value indicating if the 'Smart Tangents' feature is active.</summary>
    public static bool SmartTangents
    {
      get { return GetBool(idxSmartTangents); }
      set { SetBool(idxSmartTangents, value); }
    }

    /// <summary>Gets or sets the activation delay in milliseconds.</summary>
    public static int ActivationDelayMilliseconds
    {
      get { return UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetInt(true, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppSmartTrackSettings_SetInt(true, value, IntPtr.Zero); }
    }

    /// <summary>Gets or sets the maximum number of smart points.</summary>
    public static int MaxSmartPoints
    {
      get { return UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetInt(false, IntPtr.Zero); }
      set { UnsafeNativeMethods.CRhinoAppSmartTrackSettings_SetInt(false, value, IntPtr.Zero); }
    }

    const int idxLineColor = 0;
    const int idxTanPerpLineColor = 1;
    const int idxPointColor = 2;
    const int idxActivePointColor = 3;

    static Color GetColor(int which, IntPtr pSmartTrackSettings)
    {
      int abgr = UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetSetColor(which, false, 0, pSmartTrackSettings);
      return Rhino.Runtime.Interop.ColorFromWin32(abgr);
    }
    static Color GetColor(int which) { return GetColor(which, IntPtr.Zero); }

    static void SetColor(int which, Color c, IntPtr pSmartTrackSettings)
    {
      int argb = c.ToArgb();
      UnsafeNativeMethods.CRhinoAppSmartTrackSettings_GetSetColor(which, true, argb, pSmartTrackSettings);
    }
    static void SetColor(int which, Color c) { SetColor(which, c, IntPtr.Zero); }

    /// <summary>Gets or sets the smart track line color.</summary>
    public static Color LineColor
    {
      get { return GetColor(idxLineColor); }
      set { SetColor(idxLineColor, value); }
    }

    /// <summary>Gets or sets the tangent and perpendicular line color.</summary>
    public static Color TanPerpLineColor
    {
      get { return GetColor(idxTanPerpLineColor); }
      set { SetColor(idxTanPerpLineColor, value); }
    }

    /// <summary>Gets or sets the point color.</summary>
    public static Color PointColor
    {
      get { return GetColor(idxPointColor); }
      set { SetColor(idxPointColor, value); }
    }

    /// <summary>Gets or sets the active point color.</summary>
    public static Color ActivePointColor
    {
      get { return GetColor(idxActivePointColor); }
      set { SetColor(idxActivePointColor, value); }
    }
  }

  /// <summary>
  /// Represents a snapshot of <see cref="CursorTooltipSettings"/>.
  /// </summary>
  public class CursorTooltipSettingsState
  {
    /// <summary>Turns on/off cursor tooltips.</summary>
    public bool TooltipsEnabled { get; set; }

    /// <summary>
    /// The x and y distances in pixels from the cursor location to the tooltip.
    /// </summary>
    public System.Drawing.Point Offset { get; set; }

    /// <summary>Tooltip background color.</summary>
    public System.Drawing.Color BackgroundColor { get; set; }

    /// <summary>Tooltip text color.</summary>
    public System.Drawing.Color TextColor { get; set; }

    /// <summary>
    /// Displays the current object snap selection.
    /// </summary>
    public bool OsnapPane { get; set; }

    /// <summary>
    /// Displays the distance from the last picked point.
    /// </summary>
    public bool DistancePane { get; set; }

    /// <summary>
    /// Displays the current construction plane coordinates.
    /// </summary>
    public bool PointPane { get; set; }

    /// <summary>
    /// Displays the relative construction plane coordinates and angle from the last picked point.
    /// </summary>
    public bool RelativePointPane { get; set; }

    /// <summary>
    /// Displays the current command prompt.
    /// </summary>
    public bool CommandPromptPane { get; set; }

    /// <summary>
    /// Attempts to display only the most useful tooltip.
    /// </summary>
    public bool AutoSuppress { get; set; }
  }

  /// <summary>
  /// Cursor tooltips place information at the cursor location.
  /// Note: Turning on cursor tooltips turns off object snap cursors.
  /// </summary>
  public static class CursorTooltipSettings
  {
    static CursorTooltipSettingsState CreateState(bool current)
    {
      IntPtr pSettings = UnsafeNativeMethods.CRhinoAppCursorToolTipSettings_New(current);
      CursorTooltipSettingsState rc = new CursorTooltipSettingsState();
      rc.TooltipsEnabled = GetInt(idx_EnableCursorToolTips, pSettings)!=0;
      int x = GetInt(idx_xoffset, pSettings);
      int y = GetInt(idx_yoffset, pSettings);
      rc.Offset = new Point(x, y);
      int abgr = GetInt(idx_background_color, pSettings);
      rc.BackgroundColor = Rhino.Runtime.Interop.ColorFromWin32(abgr);
      abgr = GetInt(idx_text_color, pSettings);
      rc.TextColor = Rhino.Runtime.Interop.ColorFromWin32(abgr);
      rc.OsnapPane = GetInt(idx_bOsnapPane, pSettings) != 0;
      rc.DistancePane = GetInt(idx_bDistancePane, pSettings) != 0;
      rc.PointPane = GetInt(idx_bPointPane, pSettings) != 0;
      rc.RelativePointPane = GetInt(idx_bRelativePointPane, pSettings) != 0;
      rc.CommandPromptPane = GetInt(idx_bCommandPromptPane, pSettings) != 0;
      rc.AutoSuppress = GetInt(idx_bAutoSuppress, pSettings) != 0;
      UnsafeNativeMethods.CRhinoAppCursorToolTipSettings_Delete(pSettings);
      return rc;
    }

    /// <summary>
    /// Gets the current settings.
    /// </summary>
    /// <returns>A new cursor tooltip state with current settings.</returns>
    public static CursorTooltipSettingsState GetCurrentState()
    {
      return CreateState(true);
    }

    /// <summary>
    /// Gets the cursor tooltip factory settings.
    /// </summary>
    /// <returns>A new cursor tooltip state with factory settings.</returns>
    public static CursorTooltipSettingsState GetDefaultState()
    {
      return CreateState(false);
    }

    /// <summary>
    /// Turns on/off cursor tooltips.
    /// </summary>
    public static bool TooltipsEnabled
    {
      get { return GetInt(idx_EnableCursorToolTips, IntPtr.Zero) != 0; }
      set { SetInt(idx_EnableCursorToolTips, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// The x and y distances in pixels from the cursor location to the tooltip.
    /// </summary>
    public static System.Drawing.Point Offset
    {
      get
      {
        int x = GetInt(idx_xoffset, IntPtr.Zero);
        int y = GetInt(idx_yoffset, IntPtr.Zero);
        return new Point(x, y);
      }
      set
      {
        SetInt(idx_xoffset, value.X, IntPtr.Zero);
        SetInt(idx_yoffset, value.Y, IntPtr.Zero);
      }
    }

    /// <summary>Tooltip background color.</summary>
    public static System.Drawing.Color BackgroundColor
    {
      get
      {
        int abgr = GetInt(idx_background_color, IntPtr.Zero);
        return Rhino.Runtime.Interop.ColorFromWin32(abgr);
      }
      set
      {
        int argb = value.ToArgb();
        SetInt(idx_background_color, argb, IntPtr.Zero);
      }
    }

    /// <summary>Tooltip text color.</summary>
    public static System.Drawing.Color TextColor
    {
      get
      {
        int abgr = GetInt(idx_text_color, IntPtr.Zero);
        return Rhino.Runtime.Interop.ColorFromWin32(abgr);
      }
      set
      {
        int argb = value.ToArgb();
        SetInt(idx_text_color, argb, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Displays the current object snap selection.
    /// </summary>
    public static bool OsnapPane
    {
      get { return GetInt(idx_bOsnapPane, IntPtr.Zero) != 0; }
      set { SetInt(idx_bOsnapPane, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// Displays the distance from the last picked point.
    /// </summary>
    public static bool DistancePane
    {
      get { return GetInt(idx_bDistancePane, IntPtr.Zero) != 0; }
      set { SetInt(idx_bDistancePane, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// Displays the current construction plane coordinates.
    /// </summary>
    public static bool PointPane
    {
      get { return GetInt(idx_bPointPane, IntPtr.Zero) != 0; }
      set { SetInt(idx_bPointPane, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// Displays the relative construction plane coordinates and angle from the last picked point.
    /// </summary>
    public static bool RelativePointPane
    {
      get { return GetInt(idx_bRelativePointPane, IntPtr.Zero) != 0; }
      set { SetInt(idx_bRelativePointPane, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// Displays the current command prompt.
    /// </summary>
    public static bool CommandPromptPane
    {
      get { return GetInt(idx_bCommandPromptPane, IntPtr.Zero) != 0; }
      set { SetInt(idx_bCommandPromptPane, value ? 1 : 0, IntPtr.Zero); }
    }

    /// <summary>
    /// Attempts to display only the most useful tooltip.
    /// </summary>
    public static bool AutoSuppress
    {
      get { return GetInt(idx_bAutoSuppress, IntPtr.Zero) != 0; }
      set { SetInt(idx_bAutoSuppress, value ? 1 : 0, IntPtr.Zero); }
    }

    const int idx_EnableCursorToolTips = 0;
    const int idx_xoffset = 1;
    const int idx_yoffset = 2;
    const int idx_background_color = 3;
    const int idx_text_color = 4;
    const int idx_bOsnapPane = 5;
    const int idx_bDistancePane = 6;
    const int idx_bPointPane = 7;
    const int idx_bRelativePointPane = 8;
    const int idx_bCommandPromptPane = 9;
    const int idx_bAutoSuppress = 10;

    static int GetInt(int which, IntPtr pCursorTooltipSettings)
    {
      return UnsafeNativeMethods.CRhinoAppCursorToolTipSettings_GetInt(pCursorTooltipSettings, which);
    }
    static void SetInt(int which, int value, IntPtr pCursorTooltipSettings)
    {
      UnsafeNativeMethods.CRhinoAppCursorToolTipSettings_SetInt(pCursorTooltipSettings, which, value);
    }

  }
}
#endif