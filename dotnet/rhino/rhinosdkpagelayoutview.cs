#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.Display
{
  public class RhinoPageView : RhinoView
  {
    internal RhinoPageView(IntPtr ptr, Guid mainviewport_id)
      : base(ptr, mainviewport_id)
    {
    }

    /// <summary>
    /// The ActiveViewport is the same as the MainViewport for standard RhinoViews. In
    /// a RhinoPageView, the active viewport may be the RhinoViewport of a child detail object.
    /// Most of the time, you will use ActiveViewport unless you explicitly need to work with
    /// the main viewport.
    /// </summary>
    public override RhinoViewport ActiveViewport
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        bool bMainViewport = false;
        IntPtr viewport_ptr = UnsafeNativeMethods.CRhinoView_ActiveViewport(ptr, ref bMainViewport);
        if (bMainViewport)
          return MainViewport;
        return new RhinoViewport(this, viewport_ptr);
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public void SetPageAsActive()
    {
      if (!PageIsActive)
      {
        Rhino.DocObjects.DetailViewObject[] details = GetDetailViews();
        if (details != null)
        {
          for (int i = 0; i < details.Length; i++)
            details[i].IsActive = false;
        }
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public bool SetActiveDetail(Guid detailId)
    {
      bool rc = false;
      Rhino.DocObjects.DetailViewObject[] details = GetDetailViews();
      if (details != null)
      {
        for (int i = 0; i < details.Length; i++)
        {
          if (details[i].Id == detailId)
          {
            details[i].IsActive = true;
            rc = true;
            break;
          }
        }
      }
      return rc;
    }

    public bool SetActiveDetail(string detailName, bool compareCase)
    {
      bool rc = false;
      Rhino.DocObjects.DetailViewObject[] details = GetDetailViews();
      if (details != null)
      {
        for (int i = 0; i < details.Length; i++)
        {
          Rhino.Display.RhinoViewport vp = details[i].Viewport;
          if (string.Equals(vp.Name, detailName, StringComparison.OrdinalIgnoreCase))
          {
            details[i].IsActive = true;
            rc = true;
            break;
          }
        }
      }
      return rc;
    }

    /// <summary>
    /// Returns viewport ID for the active viewport. Faster than ActiveViewport function when
    /// working with page views.
    /// </summary>
    public override Guid ActiveViewportID
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoView_ActiveViewportID(ptr);
      }
    }

    /// <summary>
    /// true if the page is active instead of any detail views. This occurs
    /// when the MainViewport.Id == ActiveViewportID.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_activeviewport.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_activeviewport.cs' lang='cs'/>
    /// <code source='examples\py\ex_activeviewport.py' lang='py'/>
    /// </example>
    public bool PageIsActive
    {
      get
      {
        return MainViewport.Id == ActiveViewportID;
      }
    }

    /// <summary>
    /// Creates a detail view object that is displayed on this page and adds it to the doc.
    /// </summary>
    /// <param name="title">The detail view title.</param>
    /// <param name="corner0">Corners of the detail view in world coordinates.</param>
    /// <param name="corner1">Corners of the detail view in world coordinates.</param>
    /// <param name="initialProjection">The defined initial projection type.</param>
    /// <returns>Newly created detail view on success. null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public Rhino.DocObjects.DetailViewObject AddDetailView(string title,
      Rhino.Geometry.Point2d corner0,
      Rhino.Geometry.Point2d corner1,
      DefinedViewportProjection initialProjection)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pDetail = UnsafeNativeMethods.CRhinoPageView_AddDetailView(pThis, corner0, corner1, title, (int)initialProjection);
      Rhino.DocObjects.DetailViewObject rc = null;
      if (pDetail != IntPtr.Zero)
      {
        uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pDetail);
        rc = new Rhino.DocObjects.DetailViewObject(sn);
      }
      return rc;
    }


    /// <summary>
    /// Gets a list of the detail view objects associated with this layout.
    /// </summary>
    /// <returns>A detail view object array. This can be null, but not empty.</returns>
    public Rhino.DocObjects.DetailViewObject[] GetDetailViews()
    {
      IntPtr pList = UnsafeNativeMethods.CRhinoDetailViewArray_New();
      IntPtr ptr = NonConstPointer();
      int count = UnsafeNativeMethods.CRhinoPageView_GetDetailViewObjects(ptr, pList);
      if (count < 1)
      {
        UnsafeNativeMethods.CRhinoDetailViewArray_Delete(pList);
        return null;
      }

      Rhino.DocObjects.DetailViewObject[] rc = new Rhino.DocObjects.DetailViewObject[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr pDetail = UnsafeNativeMethods.CRhinoDetailViewArray_Item(pList, i);
        if (pDetail != IntPtr.Zero)
        {
          uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(pDetail);
          rc[i] = new Rhino.DocObjects.DetailViewObject(sn);
        }
      }
      UnsafeNativeMethods.CRhinoDetailViewArray_Delete(pList);
      return rc;
    }

    public int PageNumber
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoPageView_GetPageNumber(pConstThis);
      }
    }

    /// <summary>Same as the MainViewport.Name.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_activeviewport.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_activeviewport.cs' lang='cs'/>
    /// <code source='examples\py\ex_activeviewport.py' lang='py'/>
    /// </example>
    public string PageName
    {
      get
      {
        RhinoViewport vp = MainViewport;
        if (vp != null)
          return vp.Name;
        return String.Empty;
      }
      set
      {
        RhinoViewport vp = MainViewport;
        if (vp != null)
          vp.Name = value;
      }
    }

    internal delegate void PageViewCallback(IntPtr pView, Guid newDetailId, Guid oldDetailId);
    private static PageViewCallback m_OnPageSpaceChanged;
    private static EventHandler<PageViewSpaceChangeEventArgs> m_detail_space_change;
    private static void OnActiveDetailChange(IntPtr pPageView, Guid newDetailId, Guid oldDetailId)
    {
      if (m_detail_space_change != null)
        m_detail_space_change(null, new PageViewSpaceChangeEventArgs(pPageView, newDetailId, oldDetailId));
    }
    public static event EventHandler<PageViewSpaceChangeEventArgs> PageViewSpaceChange
    {
      add
      {
        if( Rhino.Runtime.HostUtils.ContainsDelegate(m_detail_space_change, value) )
          return;
        if (m_detail_space_change == null)
        {
          m_OnPageSpaceChanged = OnActiveDetailChange;
          UnsafeNativeMethods.CRhinoEventWatcher_SetDetailEventCallback(m_OnPageSpaceChanged);
        }
        m_detail_space_change += value;
      }
      remove
      {
        m_detail_space_change -= value;
        if (m_detail_space_change == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetDetailEventCallback(null);
          m_OnPageSpaceChanged = null;
        }
      }
    }

  }
}
#endif