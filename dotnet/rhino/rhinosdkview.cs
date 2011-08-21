using System;
using System.Collections.Generic;

#if RHINO_SDK

namespace Rhino.Display
{
  /// <summary>
  /// A RhinoView represents a single "window" display of a document. A view could
  /// contain one or many RhinoViewports (many in the case of Layout views with detail viewports).
  /// Standard Rhino modeling views have one viewport
  /// </summary>
  public class RhinoView
  {
    private readonly Guid m_mainviewport_id; // id of mainviewport for this view. The view m_ptr is nulled
                                    // out at the end of a command. This is is used to reassocaite
                                    // the view pointer is if a plug-in developer attempts to hold
                                    // on to a view longer than a command
    private readonly IntPtr m_ptr; // CRhinoView*

    private static readonly List<RhinoView> m_view_list = new List<RhinoView>();

    // Users should never be able to directly make a new instance of a rhino view
    internal static RhinoView FromIntPtr(IntPtr view_pointer)
    {
      if (IntPtr.Zero == view_pointer)
        return null;

      // look through the cached viewlist first
      int count = m_view_list.Count;
      RhinoView view = null;
      for (int i = 0; i < count; i++)
      {
        view = m_view_list[i];
        if (view.m_ptr == view_pointer)
        {
          if( i>0 )
          {
            RhinoView tmp = m_view_list[0];
            m_view_list[0] = m_view_list[i];
            m_view_list[i] = tmp;
          }
          return view;
        }
      }

      // view is not in the list, add it
      bool isPageView = false;
      Guid id = UnsafeNativeMethods.CRhinoView_Details(view_pointer, ref isPageView);
      view = isPageView ? new RhinoPageView(view_pointer, id) : new RhinoView(view_pointer, id);
      m_view_list.Add(view);
      return view;
    }

    internal IntPtr NonConstPointer()
    {
      return m_ptr;
    }
    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    internal RhinoView(IntPtr viewptr, Guid mainviewport_id)
    {
      m_ptr = viewptr;
      m_mainviewport_id = mainviewport_id;
    }


    // use functions/propertities of System.Windows.Forms.Control
    // to wrap CView base class functions

    /// <summary>
    /// Gets the window handle that this view is bound to
    /// </summary>
    public IntPtr Handle
    {
      get
      {
        IntPtr pConstView = ConstPointer();
        return UnsafeNativeMethods.CRhinoView_HWND(pConstView);
      }
    }

    const int idxBounds = 0;
    const int idxClientRectangle = 1;
    const int idxScreenRectangle = 2;
    /// <summary>
    /// Gets or sets the size and location of the view including its nonclient elements, in pixels, relative to the parent control.
    /// </summary>
    public System.Drawing.Rectangle Bounds
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxBounds, ref lrtb[0]);
        return System.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
      //set { }
    }
    /// <summary>
    /// Gets the rectangle that represents the client area of the view. 
    /// </summary>
    public System.Drawing.Rectangle ClientRectangle
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxClientRectangle, ref lrtb[0]);
        return System.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
    }

    /// <summary>
    /// Gets the rectangle that represents the client area of the view in screen coordinates.
    /// </summary>
    public System.Drawing.Rectangle ScreenRectangle
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxScreenRectangle, ref lrtb[0]);
        return System.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
    }

    /// <summary>
    /// Convert a point in screen coordinates to client coordinates for this view
    /// </summary>
    /// <param name="screenPoint"></param>
    /// <returns></returns>
    public System.Drawing.Point ScreenToClient(System.Drawing.Point screenPoint)
    {
      System.Drawing.Rectangle screen = ScreenRectangle;
      int x = screenPoint.X - screen.Left;
      int y = screenPoint.Y - screen.Top;
      return new System.Drawing.Point(x,y);
    }

    public System.Drawing.Point ClientToScreen(System.Drawing.Point clientPoint)
    {
      System.Drawing.Rectangle screen = ScreenRectangle;
      int x = clientPoint.X + screen.Left;
      int y = clientPoint.Y + screen.Top;
      return new System.Drawing.Point(x, y);
    }

    public Rhino.Geometry.Point2d ClientToScreen(Rhino.Geometry.Point2d clientPoint)
    {
      System.Drawing.Rectangle screen = ScreenRectangle;
      double x = clientPoint.X + screen.Left;
      double y = clientPoint.Y + screen.Top;
      return new Rhino.Geometry.Point2d(x, y);
    }

    //[skipping]
    //  functionality in CView base class
    //  UUID PlugInID() const;

    /// <summary>Redraw this view</summary>
    /// <remarks>
    /// If you change something in "this" view like the projection, construction plane,
    /// background bitmap, etc., then you need to call RhinoView.Redraw() to redraw
    /// "this" view./ The other views will not be changed. If you change something in
    /// the document (like adding new geometry to the model), then you need to call
    /// RhinoDoc.Views.Redraw() to redraw all the views.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public void Redraw()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoView_Redraw(ptr);
    }

    //[skipping]
    //  void SetRedrawDisplayHint( unsigned int display_hint ) const;

    /// <summary>
    /// Enables drawing. By default, drawing is enabled.  There are some rare
    /// situations where scipts want to disable drawing for a while.
    /// </summary>
    public static bool EnableDrawing
    {
      get
      {
        bool rc = true;
        UnsafeNativeMethods.CRhinoView_EnableDrawing(false, ref rc);
        return rc;
      }
      set { UnsafeNativeMethods.CRhinoView_EnableDrawing(true, ref value); }
    }

    // [skipping]
    //  bool ScreenCaptureToBitmap( CRhinoUiDib& dib, BOOL bIncludeCursor = true, BOOL bClientAreaOnly = false);

    ///<summary>Creates a bitmap preview image of model.</summary>
    ///<param name='imagePath'>
    ///[in] The name of the bitmap file to create.  The extension of the imagePath controls
    ///the format of the bitmap file created (bmp, tga, jpg, pcx, png, tif).
    ///</param>
    ///<param name='size'>[in] The width and height of the bitmap in pixels.</param>
    ///<param name="ignoreHighlights"></param>
    ///<param name="drawConstructionPlane"></param>
    ///<returns>true if successful</returns>
    public bool CreateWireframePreviewImage(string imagePath,
                                            System.Drawing.Size size,
                                            bool ignoreHighlights,
                                            bool drawConstructionPlane)
    {
      int settings = 0;
      if (ignoreHighlights)
        settings |= 0x1;
      if (drawConstructionPlane)
        settings |= 0x2;

      Rhino.RhinoDoc doc = Document;
      Guid id = MainViewport.Id;
      return doc.CreatePreviewImage(imagePath, id, size, settings, true);
    }
    ///<summary>Creates a bitmap preview image of model.</summary>
    ///<param name='imagePath'>
    ///[in] The name of the bitmap file to create.  The extension of the imagePath controls
    ///the format of the bitmap file created (bmp, tga, jpg, pcx, png, tif).
    ///</param>
    ///<param name='size'>[in] The width and height of the bitmap in pixels.</param>
    /// <param name="ignoreHighlights"></param>
    /// <param name="drawConstructionPlane"></param>
    /// <param name="useGhostedShading"></param>
    ///<returns>true if successful</returns>
    public bool CreateShadedPreviewImage(string imagePath,
                                         System.Drawing.Size size,
                                         bool ignoreHighlights,
                                         bool drawConstructionPlane,
                                         bool useGhostedShading)
    {
      int settings = 0;
      if (ignoreHighlights)
        settings |= 0x1;
      if (drawConstructionPlane)
        settings |= 0x2;
      if (useGhostedShading)
        settings |= 0x4;
      Rhino.RhinoDoc doc = Document;
      Guid id = MainViewport.Id;
      return doc.CreatePreviewImage(imagePath, id, size, settings, true);
    }

    public RhinoDoc Document
    {
      get
      {
        IntPtr pThis = NonConstPointer();
        IntPtr pDoc = UnsafeNativeMethods.CRhinoView_Document(pThis);
        return RhinoDoc.FromIntPtr(pDoc);
      }
    }

    const int idxMaximized = 0;
    const int idxIsFloatingRhinoView = 1;
    const int idxViewportTitle = 2;
    bool GetBool(int which)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoView_GetSetBool(ptr, which, false, false);
    }

    //[skipping]
    //  enum DISPLAY_HINT
    //  bool InterruptDrawing() const;
    //  void  OnDrawMasked(CDC* pDC, COLORREF crMaskColor=ON_UNSET_COLOR);
    //  virtual void OnDraw(CDC* pDC);  // overridden to draw this view
    //  virtual BOOL PreCreateWindow(CREATESTRUCT& cs);
    //  virtual void OnInitialUpdate();
    //  virtual BOOL PreTranslateMessage(MSG* pMsg);
    //  virtual ~CRhinoView();
    //  void SetProjection( const ON_3dmView& view, bool bMainViewport);

    RhinoViewport m_mainviewport; // = null; // each CRhinoView has a main viewport [runtime default is null]
    /// <summary>
    /// A RhinoView contains a "main viewport" that fills the entire view client window.
    /// RhinoPageViews may also contain nested child RhinoViewports for implementing
    /// detail viewports.
    /// The MainViewport will always return this RhinoView's m_vp
    /// </summary>
    public RhinoViewport MainViewport
    {
      get
      {
        if (null == m_mainviewport)
        {
          IntPtr view_ptr = NonConstPointer();
          IntPtr viewport_ptr = UnsafeNativeMethods.CRhinoView_MainViewport(view_ptr);
          m_mainviewport = new RhinoViewport(this, viewport_ptr);
        }
        return m_mainviewport;
      }
    }

    /// <summary>
    /// The ActiveViewport is the same as the MainViewport for standard RhinoViews. In
    /// a RhinoPageView, the active viewport may be the RhinoViewport of a child detail object.
    /// Most of the time, you will use ActiveViewport unless you explicitly need to work with
    /// the main viewport.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public virtual RhinoViewport ActiveViewport
    {
      get
      {
        return MainViewport;
      }
    }

    /// <summary>
    /// Returns viewport ID for the active viewport. Faster than ActiveViewport function when
    /// working with page views.
    /// </summary>
    public virtual Guid ActiveViewportID
    {
      get
      {
        // note: this function is virtual and the PageView implements the case where
        // a little bit of work needs to be done
        return m_mainviewport_id;
      }
    }

    //[skipping]
    //  const ON_3dmViewPosition& Position() const; 
    //  static void AddDrawCallback( CRhinoDrawCallback* );
    //  static void RemoveDrawCallback( CRhinoDrawCallback* );
    //  enum drag_plane
    //  static drag_plane DragPlane();
    //  static void SetDragPlane( drag_plane );
    //  void UpdateTitle( BOOL );

    /// <summary>
    /// Visibility of the viewport title window
    /// </summary>
    public bool TitleVisible
    {
      get
      {
        return GetBool(idxViewportTitle); 
      }
      set
      {
        IntPtr pView = NonConstPointer();
        UnsafeNativeMethods.CRhinoView_GetSetBool(pView, idxViewportTitle, value, true);
      }
    }

    public bool Maximized
    {
      get
      {
        return GetBool(idxMaximized);
      }
      set
      {
        IntPtr pView = NonConstPointer();
        UnsafeNativeMethods.CRhinoView_GetSetBool(pView, idxMaximized, value, true);
      }
    }

    //[skipping]
    //  int GetVisibleObjects( CRect  pick_rect, ON_SimpleArray<const CRhinoObject*>& visible_objects );
    //  enum rhino_view_type
    //  int RhinoViewType() const;
    //  class CRhViewSdkExtension* m__view_sdk_extension;
    //  virtual const CRuntimeClass* GetDefaultDisplayPipelineClass() const;
    //  CRhinoDisplayPipeline*        CreateDisplayPipeline(const CRuntimeClass*,  bool  bAttach = true);
    //  bool                          SetupDisplayPipeline(void);
    //  bool                          AttachDisplayPipeline(CRhinoDisplayPipeline* = NULL, bool  bDeleteExisting = true);
    //  CRhinoDisplayPipeline*        DetachDisplayPipeline(void);
    //  CRhinoDisplayPipeline*        DisplayPipeline(void);
    //  const CRhinoDisplayPipeline*  DisplayPipeline(void) const;
    //  CDisplayPipelineAttributes*         DisplayAttributes(void);
    //  const CDisplayPipelineAttributes*   DisplayAttributes(void) const;
    //  struct tagDRAW_CALLBACK
    //  virtual bool RecreateHWND();

    /// <summary>
    /// Used by Rhino main frame and doc/view manager to determine if this view is
    /// in a floating frame or a child of the MDIClient window associated with the
    /// Rhino main frame window.
    /// 
    /// Returns true If this view is in a free floating frame window.
    /// </summary>
    [Obsolete("Use Floating property - this will be removed in a future WIP")]
    public bool IsFloatingRhinoView
    {
      get{ return GetBool(idxIsFloatingRhinoView); }
    }

    //[skipping]
    //  void SetFloatingRhinoViewPlacement( const WINDOWPLACEMENT& wp);
    //  WINDOWPLACEMENT* FloatingRhinoViewPlacement() const;

    /// <summary>
    /// Change floating state of RhinoView 
    /// </summary>
    /// <param name="floating">
    /// if true, then the view will be in a floating frame window. Otherwise
    /// the view will be embeded in the main frame
    /// </param>
    /// <returns>true on success</returns>
    [Obsolete("Use Floating property - this will be removed in a future WIP")]
    public bool FloatRhinoView(bool floating)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoView_FloatRhinoView(ptr, floating);
    }

    /// <summary>
    /// Floating state of RhinoView.
    /// if true, then the view will be in a floating frame window. Otherwise
    /// the view will be embeded in the main frame
    /// </summary>
    public bool Floating
    {
      get
      {
        return GetBool(idxIsFloatingRhinoView);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.CRhinoView_FloatRhinoView(ptr, value);
      }
    }

    // THESE ARE IN CRhinoDoc, but should probably be in CRhinoView
    //  void EnableCameraIcon( CRhinoView* view );
    //  bool CloseRhinoView( CRhinoView* pView);

    #region events
    internal delegate void ViewCallback(IntPtr pView);
    private static ViewCallback m_OnCreateView;
    private static ViewCallback m_OnDestroyView;
    private static ViewCallback m_OnSetActiveView;
    private static ViewCallback m_OnRenameView;
    private static EventHandler<ViewEventArgs> m_create_view;
    private static EventHandler<ViewEventArgs> m_destroy_view;
    private static EventHandler<ViewEventArgs> m_setactive_view;
    private static EventHandler<ViewEventArgs> m_rename_view;

    private static void OnCreateView(IntPtr pView)
    {
      if (m_create_view != null)
        m_create_view(null, new ViewEventArgs(pView));
    }
    private static void OnDestroyView(IntPtr pView)
    {
      if (m_destroy_view != null)
        m_destroy_view(null, new ViewEventArgs(pView));
    }
    private static void OnSetActiveView(IntPtr pView)
    {
      if (m_setactive_view != null)
        m_setactive_view(null, new ViewEventArgs(pView));
    }
    private static void OnRenameView(IntPtr pView)
    {
      if (m_rename_view != null)
        m_rename_view(null, new ViewEventArgs(pView));
    }

    public static event EventHandler<ViewEventArgs> Create
    {
      add
      {
        if (m_create_view == null)
        {
          m_OnCreateView = OnCreateView;
          UnsafeNativeMethods.CRhinoEventWatcher_SetCreateViewCallback(m_OnCreateView, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_create_view += value;
      }
      remove
      {
        m_create_view -= value;
        if (m_create_view == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetCreateViewCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnCreateView = null;
        }
      }
    }
    public static event EventHandler<ViewEventArgs> Destroy
    {
      add
      {
        if (m_destroy_view == null)
        {
          m_OnDestroyView = OnDestroyView;
          UnsafeNativeMethods.CRhinoEventWatcher_SetDestroyViewCallback(m_OnDestroyView, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_destroy_view += value;
      }
      remove
      {
        m_destroy_view -= value;
        if (m_destroy_view == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetDestroyViewCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnDestroyView = null;
        }
      }
    }
    public static event EventHandler<ViewEventArgs> SetActive
    {
      add
      {
        if (m_setactive_view == null)
        {
          m_OnSetActiveView = OnSetActiveView;
          UnsafeNativeMethods.CRhinoEventWatcher_SetActiveViewCallback(m_OnSetActiveView, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_setactive_view += value;
      }
      remove
      {
        m_setactive_view -= value;
        if (m_setactive_view == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetActiveViewCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnSetActiveView = null;
        }
      }
    }
    public static event EventHandler<ViewEventArgs> Rename
    {
      add
      {
        if (m_rename_view == null)
        {
          m_OnRenameView = OnRenameView;
          UnsafeNativeMethods.CRhinoEventWatcher_SetRenameViewCallback(m_OnRenameView, Rhino.Runtime.HostUtils.m_ew_report);
        }
        m_rename_view += value;
      }
      remove
      {
        m_rename_view -= value;
        if (m_rename_view == null)
        {
          UnsafeNativeMethods.CRhinoEventWatcher_SetRenameViewCallback(null, Rhino.Runtime.HostUtils.m_ew_report);
          m_OnRenameView = null;
        }
      }
    }
    #endregion
  }

  public class ViewEventArgs : EventArgs
  {
    readonly IntPtr m_pView;
    internal ViewEventArgs(IntPtr pView)
    {
      m_pView = pView;
    }

    RhinoView m_view;
    public RhinoView View
    {
      get
      {
        if( null==m_view && IntPtr.Zero!=m_pView )
          m_view = RhinoView.FromIntPtr(m_pView);
        return m_view;
      }
    }
  }
}

#endif