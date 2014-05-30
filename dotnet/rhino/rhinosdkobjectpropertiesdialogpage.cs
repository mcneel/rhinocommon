using System;
#pragma warning disable 1591

#if RHINO_SDK
namespace Rhino.UI
{
  public abstract class ObjectPropertiesPage
  {
    protected ObjectPropertiesPage()
    {

    }

    internal IntPtr ConstructWithRhinoDotNet()
    {
      // This is only going to work on Windows. Use functions defined in Rhino.NET through
      // reflection to hook the pages up. This is done so we don't have to bring in all of
      // the WinForms/MFC interop code into RhinoCommon
      System.Reflection.Assembly rhdn = Rhino.Runtime.HostUtils.GetRhinoDotNetAssembly();
      if (rhdn != null)
      {
        Type t = rhdn.GetType("RMA.UI.MRhinoObjectPropertiesDialogPage");
        System.Reflection.MethodInfo mi = t.GetRuntimeMethod("RhinoCommonCreateObjPropPage");
        object rc = mi.Invoke(null, new object[] { this });
        if (rc == null)
          return IntPtr.Zero;
        return (IntPtr)rc;
      }
      return IntPtr.Zero;
    }

    public abstract Rhino.Drawing.Icon Icon { get; }
    ///<summary>
    /// Return the control that represents this page. This will typically be a custom user control.
    /// </summary>
    public abstract System.Windows.Forms.Control PageControl { get; }

    public virtual void OnCreateParent(IntPtr hwndParent) { }
    public virtual void OnSizeParent(int width, int height) { }

    public abstract string EnglishPageTitle { get; }

    public virtual string LocalPageTitle
    {
      get { return EnglishPageTitle; }
    }

    ///<summary>Called when this page is activated/deactivated.</summary>
    ///<param name="active">If true then this page is on top otherwise it is about to be hidden.</param>
    ///<returns>
    ///If true then the page is hidden and the requested page is not
    ///activated otherwise will not allow you to change the current page.
    ///Default returns true
    ///</returns>
    public virtual bool OnActivate(bool active) { return true; }

    public virtual void OnHelp() { }

    public virtual bool ShouldDisplay(Rhino.DocObjects.RhinoObject rhObj) { return true; }
    public virtual void InitializeControls(Rhino.DocObjects.RhinoObject rhObj) { }
  }
}
#endif