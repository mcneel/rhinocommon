#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RHINO_SDK

namespace Rhino.UI
{
  public class MouseCallbackEventArgs : System.ComponentModel.CancelEventArgs
  {
    private readonly IntPtr m_pRhinoView;
    private Rhino.Display.RhinoView m_view;
    private readonly System.Windows.Forms.MouseButtons m_button;
    private readonly Rhino.Drawing.Point m_point;

    internal MouseCallbackEventArgs(IntPtr pRhinoView, int button, int x, int y)
    {
      //const int btnNone = 0;
      const int btnLeft = 1;
      const int btnRight = 2;
      const int btnMiddle = 3;
      const int btnX = 4;

      m_pRhinoView = pRhinoView;
      switch (button)
      {
        case btnLeft:
          m_button = System.Windows.Forms.MouseButtons.Left;
          break;
        case btnMiddle:
          m_button = System.Windows.Forms.MouseButtons.Middle;
          break;
        case btnRight:
          m_button = System.Windows.Forms.MouseButtons.Right;
          break;
        case btnX:
          m_button = System.Windows.Forms.MouseButtons.XButton1;
          break;
        default: // or btnNone
          m_button = System.Windows.Forms.MouseButtons.None;
          break;
      }
      m_point = new Rhino.Drawing.Point(x, y);
    }

    public Rhino.Display.RhinoView View
    {
      get { return m_view ?? (m_view = Rhino.Display.RhinoView.FromIntPtr(m_pRhinoView)); }
    }

    public System.Windows.Forms.MouseButtons Button
    {
      get { return m_button; }
    }

    public Rhino.Drawing.Point ViewportPoint
    {
      get { return m_point; }
    }

  }

  /// <summary>
  /// Used for intercepting mouse events in the Rhino viewports.
  /// </summary>
  public abstract class MouseCallback
  {
    protected virtual void OnMouseDown(MouseCallbackEventArgs e) { }

    protected virtual void OnMouseMove(MouseCallbackEventArgs e) { }

    protected virtual void OnMouseUp(MouseCallbackEventArgs e) { }

    protected virtual void OnMouseDoubleClick(MouseCallbackEventArgs e) { }

    protected virtual void OnMouseEnter(Rhino.Display.RhinoView view) { }
    protected virtual void OnMouseLeave(Rhino.Display.RhinoView view) { }
    protected virtual void OnMouseHover(Rhino.Display.RhinoView view) { }
    
    private static readonly List<MouseCallback> m_enabled_list = new List<MouseCallback>();

    public bool Enabled
    {
      get
      {
        // see if this class is in the enabled list
        return m_enabled_list.Contains(this);
      }
      set
      {
        if (value)
        {
          if (!m_enabled_list.Contains(this))
            m_enabled_list.Add(this);
        }
        else
        {
          m_enabled_list.Remove(this);
        }

        // turn the mouse callbacks on/off based on the number of items in the list
        if (m_enabled_list.Count > 0)
        {
          m_MouseCallbackFromCPP = OnMouseCallbackFromCPP;
          UnsafeNativeMethods.CRhinoMouseCallback_Enable(true, m_MouseCallbackFromCPP);
        }
        else
        {
          UnsafeNativeMethods.CRhinoMouseCallback_Enable(false, null);
        }
      }
    }

    public delegate int MouseCallbackFromCPP(IntPtr pRhinoView, int which, int button, int x, int y);
    private static MouseCallbackFromCPP m_MouseCallbackFromCPP;

    private static int OnMouseCallbackFromCPP(IntPtr pRhinoView, int which_callback, int which_button, int x, int y)
    {
      int rc = 0;
      const int callbackMouseDown = 0;
      const int callbackMouseUp = 1;
      const int callbackMouseMove = 2;
      const int callbackMouseDoubleClick = 3;
      const int callbackMouseEnter = 4;
      const int callbackMouseLeave = 5;
      const int callbackMouseHover = 6;

      if (m_enabled_list.Count > 0)
      {
        MouseCallbackEventArgs e = new MouseCallbackEventArgs(pRhinoView, which_button, x, y);

        for (int i = 0; i < m_enabled_list.Count; i++)
        {
          switch (which_callback)
          {
            case callbackMouseDown:
              m_enabled_list[i].OnMouseDown(e);
              break;
            case callbackMouseMove:
              m_enabled_list[i].OnMouseMove(e);
              break;
            case callbackMouseUp:
              m_enabled_list[i].OnMouseUp(e);
              break;
            case callbackMouseDoubleClick:
              m_enabled_list[i].OnMouseDoubleClick(e);
              break;
            case callbackMouseEnter:
              m_enabled_list[i].OnMouseEnter(e.View);
              break;
            case callbackMouseLeave:
              m_enabled_list[i].OnMouseLeave(e.View);
              break;
            case callbackMouseHover:
              m_enabled_list[i].OnMouseHover(e.View);
              break;
          }

          rc = e.Cancel ? 1 : 0;
          if (1 == rc)
            break;
        }
      }
      return rc;
    }
  }
}

#endif