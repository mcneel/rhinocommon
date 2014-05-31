#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.UI
{
  /// <summary>
  /// Provides a base class to inherit from for the addition of stacked dialog pages.
  /// </summary>
  public abstract class StackedDialogPage
  {
    readonly string m_english_page_title;

    protected StackedDialogPage(string englishPageTitle)
    {
      m_english_page_title = englishPageTitle;
    }

    System.Collections.Generic.List<StackedDialogPage> m_children;
    public System.Collections.Generic.List<StackedDialogPage> Children
    {
      get
      {
        return m_children ?? (m_children = new System.Collections.Generic.List<StackedDialogPage>());
      }
    }
    public bool HasChildren
    {
      get { return (m_children!=null && m_children.Count>0); }
    }

    internal IntPtr ConstructWithRhinoDotNet()
    {
      // This is only going to work on Windows. Use functions defined in Rhino.NET through
      // reflection to hook the pages up. This is done so we don't have to bring in all of
      // the WinForms/MFC interop code into RhinoCommon
      System.Reflection.Assembly rhdn = Rhino.Runtime.HostUtils.GetRhinoDotNetAssembly();
      if (rhdn != null)
      {
        Type t = rhdn.GetType("RMA.UI.MRhinoStackedDialogPage");
        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("RhinoCommonCreate");
        object rc = mi.Invoke(null, new object[] { this });
        if (rc == null)
          return IntPtr.Zero;
        return (IntPtr)rc;
      }
      return IntPtr.Zero;
    }

    ///<summary>
    /// Return the control that represents this page. This will typically be a custom user control.
    /// </summary> 
    public abstract System.Windows.Forms.Control PageControl { get;}

    public virtual void OnCreateParent(IntPtr hwndParent) { }
    public virtual void OnSizeParent(int width, int height) { }

    public string EnglishPageTitle
    {
      get { return m_english_page_title; }
    }
    public virtual string LocalPageTitle
    {
      get { return EnglishPageTitle; }
    }
    
    /// <summary>Called when stacked dialog OK button is pressed.</summary>
    /// <returns>
    /// If return value is true then the dialog will be closed. A return of false means
    /// there was an error and dialog remains open so page can be properly updated.
    /// </returns>
    public virtual bool OnApply() { return true; }

    ///<summary>Called when stacked dialog Cancel button is pressed.</summary>
    public virtual void OnCancel(){}

    ///<summary>Called when this page is activated/deactivated.</summary>
    ///<param name="active">If true then this page is on top otherwise it is about to be hidden.</param>
    ///<returns>
    ///If true then the page is hidden and the requested page is not
    ///activated otherwise will not allow you to change the current page.
    ///Default returns true
    ///</returns>
    public virtual bool OnActivate( bool active) { return true; }

    ///<summary>Called when this page is activated.</summary>
    ///<returns>
    ///true  : if the page wants the "Defaults" button to appear.
    ///false : if the page does not want the "Defaults" button to appear.
    ///
    ///Default returns false
    ///Note: returning false implies that OnDefaults() method will never get called.
    ///</returns>
    public virtual bool ShowDefaultsButton { get { return false; } }
  
    ///<summary>Called when stacked dialog Defaults button is pressed (see ShowDefaultsButton).</summary>
    public virtual void OnDefaults() { }


    public virtual void OnHelp() { }
  }
}
#endif
