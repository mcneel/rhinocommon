using System;
using System.Runtime.InteropServices;

namespace Rhino.Display
{
  /// <summary>
  /// Description of a how Rhino will display in a viewport. These are the modes
  /// that are listed under "Advanced display" in the options dialog
  /// </summary>
  public class DisplayModeDescription : IDisposable
  {
    #region pointer tracking
    // A local copy of a DisplayAttrsMgrListDesc is made every time
    // so we don't need to worry about it being deleted by core Rhino
    private IntPtr m_ptr; // DisplayAttrsMgrListDesc*

    internal DisplayModeDescription(IntPtr ptr)
    {
      m_ptr = ptr;
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
    /// Get all display mode descriptions that Rhino currently knows about
    /// </summary>
    /// <returns>
    /// Copies of all of the display mode descriptions. If you want to modify
    /// these descriptions, you must call UpdateDisplayMode or AddDisplayMode
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
        return null;
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
    
    public bool InMenu
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.DisplayAttrsMgrListDesc_InMenu(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.DisplayAttrsMgrListDesc_SetInMenu(pThis, value);
      }
    }
    
    DisplayPipelineAttributes m_display_attrs;
    /// <example>
    /// <code source='examples\vbnet\ex_advanceddisplay.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_advanceddisplay.cs' lang='cs'/>
    /// <code source='examples\py\ex_advanceddisplay.py' lang='py'/>
    /// </example>
    public DisplayPipelineAttributes DisplayAttributes
    {
      get
      {
        if (null == m_display_attrs)
          m_display_attrs = new DisplayPipelineAttributes(this);
        return m_display_attrs;
      }
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