#pragma warning disable 1591
using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;

#if RHINO_SDK
namespace Rhino.Display
{
  /// <summary>
  /// Description of a how Rhino will display in a viewport. These are the modes
  /// that are listed under "Advanced display" in the options dialog.
  /// </summary>
  //[Serializable]
  public class DisplayModeDescription : IDisposable, ISerializable
  {
    #region pointer tracking
    // A local copy of a DisplayAttrsMgrListDesc is made every time
    // so we don't need to worry about it being deleted by core Rhino
    private IntPtr m_ptr; // DisplayAttrsMgrListDesc*

    internal DisplayModeDescription(IntPtr ptr)
    {
      m_ptr = ptr;
    }

    private DisplayModeDescription(SerializationInfo info, StreamingContext context)
    {
      m_ptr = UnsafeNativeMethods.DisplayAttrsMgrListDesc_New();
      InMenu = info.GetBoolean("InMenu");
      SupportsShadeCommand = info.GetBoolean("SupportsShadeCommand");
      SupportsShading = info.GetBoolean("SupportsShading");
      AllowObjectAssignment = info.GetBoolean("AllowObjectAssignment");
      ShadedPipelineRequired = info.GetBoolean("ShadedPipelineRequired");
      WireframePipelineRequired = info.GetBoolean("WireframePipelineRequired");
      PipelineLocked = info.GetBoolean("PipelineLocked");

      Rhino.Display.DisplayPipelineAttributes attrs = info.GetValue("DisplayAttributes", typeof(Rhino.Display.DisplayPipelineAttributes)) as Rhino.Display.DisplayPipelineAttributes;
      Rhino.Display.DisplayPipelineAttributes current = DisplayAttributes;
      current.CopyContents(attrs);
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
    {
      info.AddValue("InMenu", InMenu);
      info.AddValue("SupportsShadeCommand", SupportsShadeCommand);
      info.AddValue("SupportsShading", SupportsShading);
      info.AddValue("AllowObjectAssignment", AllowObjectAssignment);
      info.AddValue("ShadedPipelineRequired", ShadedPipelineRequired);
      info.AddValue("WireframePipelineRequired", WireframePipelineRequired);
      info.AddValue("PipelineLocked", PipelineLocked);

      info.AddValue("DisplayAttributes", DisplayAttributes);
    }


    ~DisplayModeDescription()
    {
      Dispose(false);
    }

    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }

    internal IntPtr DisplayAttributeConstPointer()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.DisplayAttrsMgrListDesc_DisplayAttributes(pConstThis);
    }
    internal IntPtr DisplayAttributeNonConstPointer()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.DisplayAttrsMgrListDesc_DisplayAttributes(pThis);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.DisplayAttrsMgrListDesc_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
#endregion

    #region statics
    /// <summary>
    /// Gets all display mode descriptions that Rhino currently knows about.
    /// </summary>
    /// <returns>
    /// Copies of all of the display mode descriptions. If you want to modify
    /// these descriptions, you must call UpdateDisplayMode or AddDisplayMode.
    /// </returns>
    /// <example>
    /// <code source='examples\vbnet\ex_advanceddisplay.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_advanceddisplay.cs' lang='cs'/>
    /// <code source='examples\py\ex_advanceddisplay.py' lang='py'/>
    /// </example>
    public static DisplayModeDescription[] GetDisplayModes()
    {
      IntPtr pDisplayAttrsMgrList = UnsafeNativeMethods.DisplayAttrsMgrList_New();
      int count = UnsafeNativeMethods.CRhinoDisplayAttrsMgr_GetDisplayAttrsList(pDisplayAttrsMgrList);
      if (count < 1)
        return new DisplayModeDescription[0];
      DisplayModeDescription[] rc = new DisplayModeDescription[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pDisplayMode = UnsafeNativeMethods.DisplayAttrsMgrListDesc_NewFromList(pDisplayAttrsMgrList, i);
        if (pDisplayMode != IntPtr.Zero)
          rc[i] = new DisplayModeDescription(pDisplayMode);
      }
      UnsafeNativeMethods.DisplayAttrsMgrList_Delete(pDisplayAttrsMgrList);
      return rc;
    }

    public static DisplayModeDescription GetDisplayMode(Guid id)
    {
      // 18 Jan. 2011 S. Baer
      // yes, this is a bit heavy handed, but it will do for now
      DisplayModeDescription[] modes = GetDisplayModes();
      if (modes != null)
      {
        for (int i = 0; i < modes.Length; i++)
        {
          if (modes[i].Id == id)
            return modes[i];
        }
      }
      return null;
    }

    public static DisplayModeDescription FindByName(string englishName)
    {
      DisplayModeDescription[] modes = GetDisplayModes();
      if (modes != null)
      {
        for (int i = 0; i < modes.Length; i++)
        {
          if( string.Compare( modes[i].EnglishName, englishName, StringComparison.OrdinalIgnoreCase )== 0 )
            return modes[i];
        }
      }
      return null;
    }

    public static Guid AddDisplayMode(DisplayModeDescription displayMode)
    {
      IntPtr pDisplayMode = displayMode.NonConstPointer();
      return UnsafeNativeMethods.CRhinoDisplayAttrsMgr_Add(pDisplayMode);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_advanceddisplay.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_advanceddisplay.cs' lang='cs'/>
    /// <code source='examples\py\ex_advanceddisplay.py' lang='py'/>
    /// </example>
    public static bool UpdateDisplayMode(DisplayModeDescription displayMode)
    {
      IntPtr pConstDisplayMode = displayMode.ConstPointer();
      return UnsafeNativeMethods.CRhinoDisplayAttrsMgr_Update(pConstDisplayMode);
    }

    public static bool DeleteDiplayMode(Guid id)
    {
      return UnsafeNativeMethods.CRhinoDisplayAttrsMgr_DeleteDescription(id);
    }

    #endregion

    
    #region properties
    const int idxSupportsShadeCmd = 0;
    const int idxSupportsShading = 1;
    const int idxAddToMenu = 2;
    const int idxAllowObjectAssignment = 3;
    const int idxShadedPipelineRequired = 4;
    const int idxWireframePipelineRequired = 5;
    const int idxPipelineLocked = 6;

    bool GetBool(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.DisplayAttrsMgrListDesc_GetBool(pConstThis, which);
    }
    void SetBool(int which, bool b)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.DisplayAttrsMgrListDesc_SetBool(pThis, which, b);
    }
    
    public bool InMenu
    {
      get { return GetBool(idxAddToMenu); }
      set { SetBool(idxAddToMenu, value); }
    }
    public bool SupportsShadeCommand
    {
      get { return GetBool(idxSupportsShadeCmd); }
      set { SetBool(idxSupportsShadeCmd, value); }
    }
    public bool SupportsShading
    {
      get { return GetBool(idxSupportsShading); }
      set { SetBool(idxSupportsShading, value); }
    }
    public bool AllowObjectAssignment
    {
      get { return GetBool(idxAllowObjectAssignment); }
      set { SetBool(idxAllowObjectAssignment, value); }
    }
    public bool ShadedPipelineRequired
    {
      get { return GetBool(idxShadedPipelineRequired); }
      set { SetBool(idxShadedPipelineRequired, value); }
    }
    public bool WireframePipelineRequired
    {
      get { return GetBool(idxWireframePipelineRequired); }
      set { SetBool(idxWireframePipelineRequired, value); }
    }
    public bool PipelineLocked
    {
      get { return GetBool(idxPipelineLocked); }
      set { SetBool(idxPipelineLocked, value); }
    }
    
    DisplayPipelineAttributes m_display_attrs;
    /// <example>
    /// <code source='examples\vbnet\ex_advanceddisplay.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_advanceddisplay.cs' lang='cs'/>
    /// <code source='examples\py\ex_advanceddisplay.py' lang='py'/>
    /// </example>
    public DisplayPipelineAttributes DisplayAttributes
    {
      get { return m_display_attrs ?? (m_display_attrs = new DisplayPipelineAttributes(this)); }
    }

    public string EnglishName
    {
      get { return DisplayAttributes.EnglishName; }
      set { DisplayAttributes.EnglishName = value; }
    }
    public Guid Id
    {
      get { return DisplayAttributes.Id; }
    }

    public string LocalName
    {
      get { return DisplayAttributes.LocalName; }
    }


    #endregion
    
  }
}
#endif