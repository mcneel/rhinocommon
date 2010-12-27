using System;
using System.Runtime.InteropServices;

namespace Rhino.Display
{
  public class DisplayModeDescription : IDisposable
  {
    #region statics
    public static DisplayModeDescription[] GetDisplayModeList()
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
    #endregion

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
    DisplayPipelineAttributes DisplayAttributes
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
    public string LocalName
    {
      get { return DisplayAttributes.LocalName; }
    }


    #endregion
    
  }
}