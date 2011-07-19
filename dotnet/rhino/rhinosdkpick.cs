using System;

namespace Rhino.Input.Custom
{
  public enum PickStyle : int
  {
    None = 0,
    PointPick = 1,
    WindowPick = 2,
    CrossingPick = 3
  }


  public class PickContext : IDisposable
  {
    public PickContext()
    {
      m_pRhinoPickContext = UnsafeNativeMethods.CRhinoPickContext_New();
    }

    #region IDisposable/Pointer handling
    IntPtr m_pRhinoPickContext;
    internal IntPtr ConstPointer() { return m_pRhinoPickContext; }
    internal IntPtr NonConstPointer() { return m_pRhinoPickContext; }

    ~PickContext()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_pRhinoPickContext != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoPickContext_Delete(m_pRhinoPickContext);
      }
      m_pRhinoPickContext = IntPtr.Zero;
    }
    #endregion

    public Rhino.Display.RhinoView View
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pView = UnsafeNativeMethods.CRhinoPickContext_GetView(pConstThis);
        return Rhino.Display.RhinoView.FromIntPtr(pView);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        IntPtr pView = value.NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetView(pThis, pView);
      }
    }

    /// <summary>
    /// pick chord starts on near clipping plane and ends on far clipping plane
    /// </summary>
    public Rhino.Geometry.Line PickLine
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Rhino.Geometry.Line rc = new Geometry.Line();
        UnsafeNativeMethods.CRhinoPickContext_PickLine(pConstThis, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickLine(pThis, ref value);
      }
    }


    public PickStyle PickStyle
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (PickStyle)UnsafeNativeMethods.CRhinoPickContext_PickStyle(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPickContext_SetPickStyle(pThis, (int)value);
      }
    }

    public void SetPickTransform(Rhino.Geometry.Transform transform)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoPickContext_SetPickTransform(pThis, ref transform);
    }

    /// <summary>
    /// Updates the clipping plane information in pick region. The
    /// SetClippingPlanes and View fields must be called before calling
    /// UpdateClippingPlanes().
    /// </summary>
    public void UpdateClippingPlanes()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoPickContext_UpdateClippingPlanes(pThis);
    }
  }
}
