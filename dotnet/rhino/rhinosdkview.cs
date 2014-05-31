#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RHINO_SDK

namespace Rhino.Display
{
  /// <summary>
  /// A RhinoView represents a single "window" display of a document. A view could
  /// contain one or many RhinoViewports (many in the case of Layout views with detail viewports).
  /// Standard Rhino modeling views have one viewport.
  /// </summary>
  public class RhinoView
  {
    private Guid m_main_viewport_id; // id of mainviewport for this view. The view m_ptr is nulled
                                    // out at the end of a command. This is is used to reassocaite
                                    // the view pointer is if a plug-in developer attempts to hold
                                    // on to a view longer than a command
    private IntPtr m_ptr; // CRhinoView*

    private static readonly List<RhinoView> g_view_list = new List<RhinoView>();

    // Users should never be able to directly make a new instance of a rhino view
    internal static RhinoView FromIntPtr(IntPtr viewPointer)
    {
      if (IntPtr.Zero == viewPointer)
        return null;

      // look through the cached viewlist first
      int count = g_view_list.Count;
      RhinoView view;
      for (int i = 0; i < count; i++)
      {
        view = g_view_list[i];
        if (view.m_ptr == viewPointer)
        {
          if( i>0 )
          {
            RhinoView tmp = g_view_list[0];
            g_view_list[0] = g_view_list[i];
            g_view_list[i] = tmp;
          }
          return view;
        }
      }

      // view is not in the list, add it
      bool is_page_view = false;
      Guid id = UnsafeNativeMethods.CRhinoView_Details(viewPointer, ref is_page_view);
      view = is_page_view ? new RhinoPageView(viewPointer, id) : new RhinoView(viewPointer, id);
      g_view_list.Add(view);
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

    internal RhinoView(IntPtr viewptr, Guid mainViewportId)
    {
      m_ptr = viewptr;
      m_main_viewport_id = mainViewportId;
    }


    // use functions/propertities of System.Windows.Forms.Control
    // to wrap CView base class functions

    /// <summary>
    /// Gets the window handle that this view is bound to.
    /// </summary>
    public IntPtr Handle
    {
      get
      {
        IntPtr ptr_const_view = ConstPointer();
        return UnsafeNativeMethods.CRhinoView_HWND(ptr_const_view);
      }
    }

    const int idxBounds = 0;
    const int idxClientRectangle = 1;
    const int idxScreenRectangle = 2;
    /// <summary>
    /// Gets or sets the size and location of the view including its nonclient elements, in pixels, relative to the parent control.
    /// </summary>
    public Rhino.Drawing.Rectangle Bounds
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxBounds, ref lrtb[0]);
        return Rhino.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
      //set { }
    }
    /// <summary>
    /// Gets the rectangle that represents the client area of the view. 
    /// </summary>
    public Rhino.Drawing.Rectangle ClientRectangle
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxClientRectangle, ref lrtb[0]);
        return Rhino.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
    }

    /// <summary>
    /// Gets the rectangle that represents the client area of the view in screen coordinates.
    /// </summary>
    public Rhino.Drawing.Rectangle ScreenRectangle
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int[] lrtb = new int[4];
        UnsafeNativeMethods.CRhinoView_GetRect(ptr, idxScreenRectangle, ref lrtb[0]);
        return Rhino.Drawing.Rectangle.FromLTRB(lrtb[0], lrtb[2], lrtb[1], lrtb[3]);
      }
    }

    /// <summary>
    /// Converts a point in screen coordinates to client coordinates for this view.
    /// </summary>
    /// <param name="screenPoint">The 2D screen point.</param>
    /// <returns>A 2D point in client coordinates.</returns>
    public Rhino.Drawing.Point ScreenToClient(Rhino.Drawing.Point screenPoint)
    {
      Rhino.Drawing.Rectangle screen = ScreenRectangle;
      int x = screenPoint.X - screen.Left;
      int y = screenPoint.Y - screen.Top;
      return new Rhino.Drawing.Point(x,y);
    }

    public Geometry.Point2d ScreenToClient(Geometry.Point2d screenPoint)
    {
      Rhino.Drawing.Rectangle screen = ScreenRectangle;
      double x = screenPoint.X - screen.Left;
      double y = screenPoint.Y - screen.Top;
      return new Geometry.Point2d(x, y);
    }

    public Rhino.Drawing.Point ClientToScreen(Rhino.Drawing.Point clientPoint)
    {
      Rhino.Drawing.Rectangle screen = ScreenRectangle;
      int x = clientPoint.X + screen.Left;
      int y = clientPoint.Y + screen.Top;
      return new Rhino.Drawing.Point(x, y);
    }

    public Geometry.Point2d ClientToScreen(Geometry.Point2d clientPoint)
    {
      Rhino.Drawing.Rectangle screen = ScreenRectangle;
      double x = clientPoint.X + screen.Left;
      double y = clientPoint.Y + screen.Top;
      return new Geometry.Point2d(x, y);
    }

    //[skipping]
    //  functionality in CView base class
    //  UUID PlugInID() const;

    /// <summary>Redraws this view.</summary>
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
    /// Gets or sets the 'drawing enabled' flag. By default, drawing is enabled.
    /// <para>There are some rare situations where scipts want to disable drawing for a while.</para>
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

    public double SpeedTest(int frameCount, bool freezeDrawList, int direction, double angleDeltaRadians)
    {
      IntPtr ptr_this = NonConstPointer();
      return UnsafeNativeMethods.RhViewSpeedTest(ptr_this, frameCount, freezeDrawList, direction, angleDeltaRadians);
    }

    // [skipping]
    //  bool ScreenCaptureToBitmap( CRhinoUiDib& dib, BOOL bIncludeCursor = true, BOOL bClientAreaOnly = false);

    ///<summary>Creates a bitmap preview image of model.</summary>
    ///<param name='imagePath'>
    ///[in] The name of the bitmap file to create.  The extension of the imagePath controls
    ///the format of the bitmap file created (bmp, tga, jpg, pcx, png, tif).
    ///</param>
    ///<param name='size'>[in] The width and height of the bitmap in pixels.</param>
    ///<param name="ignoreHighlights">true if highlighted elements should be drawn normally.</param>
    ///<param name="drawConstructionPlane">true if the CPlane should be drawn.</param>
    ///<returns>true if successful.</returns>
    public bool CreateWireframePreviewImage(string imagePath,
                                            Rhino.Drawing.Size size,
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
    ///<param name="ignoreHighlights">true if highlighted elements should be drawn normally.</param>
    ///<param name="drawConstructionPlane">true if the CPlane should be drawn.</param>
    /// <param name="useGhostedShading">true if ghosted shading (partially transparent shading) should be used.</param>
    ///<returns>true if successful.</returns>
    public bool CreateShadedPreviewImage(string imagePath,
                                         Rhino.Drawing.Size size,
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
      // 11-Apr-2013 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-18332
      // return doc.CreatePreviewImage(imagePath, id, size, settings, true);
      return doc.CreatePreviewImage(imagePath, id, size, settings, false);
    }

    /// <summary>
    /// Capture View contents to a bitmap.
    /// </summary>
    /// <returns>The bitmap of the complete view.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap()
    {
      return CaptureToBitmap(ClientRectangle.Size);
    }

    /// <summary>
    /// Capture View contents to a bitmap.
    /// </summary>
    /// <param name="size">Size of Bitmap to capture to.</param>
    /// <returns>The bitmap of the specified part of the view.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Drawing.Size size)
    {
      IntPtr pConstView = ConstPointer();
      IntPtr pRhinoDib = UnsafeNativeMethods.CRhinoDib_New();
      Rhino.Drawing.Bitmap rc = null;
      if (UnsafeNativeMethods.CRhinoView_CaptureToBitmap(pConstView, pRhinoDib, size.Width, size.Height, IntPtr.Zero))
      {
        IntPtr hBmp = UnsafeNativeMethods.CRhinoDib_Bitmap(pRhinoDib);
        if (IntPtr.Zero != hBmp)
          rc = Rhino.Drawing.Image.FromHbitmap(hBmp);
      }
      UnsafeNativeMethods.CRhinoDib_Delete(pRhinoDib);
      return rc;
    }

    /// <summary>
    /// Captures a part of the view contents to a bitmap allowing for visibility of grid and axes.
    /// </summary>
    /// <param name="size">The width and height of the returned bitmap.</param>
    /// <param name="grid">true if the construction plane grid should be visible.</param>
    /// <param name="worldAxes">true if the world axis should be visible.</param>
    /// <param name="cplaneAxes">true if the construction plane close the the grid should be visible.</param>
    /// <returns>A new bitmap.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Drawing.Size size, bool grid, bool worldAxes, bool cplaneAxes)
    {
      IntPtr pConstView = ConstPointer();
      IntPtr pRhinoDib = UnsafeNativeMethods.CRhinoDib_New();
      Rhino.Drawing.Bitmap rc = null;
      if (UnsafeNativeMethods.CRhinoView_CaptureToBitmap2(pConstView, pRhinoDib, size.Width, size.Height, grid, worldAxes, cplaneAxes))
      {
        IntPtr hBmp = UnsafeNativeMethods.CRhinoDib_Bitmap(pRhinoDib);
        if (IntPtr.Zero != hBmp)
          rc = Rhino.Drawing.Image.FromHbitmap(hBmp);
      }
      UnsafeNativeMethods.CRhinoDib_Delete(pRhinoDib);
      return rc;
    }

    /// <summary>
    /// Captures the view contents to a bitmap allowing for visibility of grid and axes.
    /// </summary>
    /// <param name="grid">true if the construction plane grid should be visible.</param>
    /// <param name="worldAxes">true if the world axis should be visible.</param>
    /// <param name="cplaneAxes">true if the construction plane close the the grid should be visible.</param>
    /// <returns>A new bitmap.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_screencaptureview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_screencaptureview.cs' lang='cs'/>
    /// <code source='examples\py\ex_screencaptureview.py' lang='py'/>
    /// </example>
    public Rhino.Drawing.Bitmap CaptureToBitmap(bool grid, bool worldAxes, bool cplaneAxes)
    {
      return CaptureToBitmap(ClientRectangle.Size, grid, worldAxes, cplaneAxes);
    }

    /// <summary>
    /// Capture View contents to a bitmap using a display mode description to define
    /// how drawing is performed.
    /// </summary>
    /// <param name="size">The width and height of the returned bitmap.</param>
    /// <param name="mode">The display mode.</param>
    /// <returns>A new bitmap.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Drawing.Size size, Rhino.Display.DisplayModeDescription mode)
    {
      Rhino.Display.DisplayPipelineAttributes attr = new DisplayPipelineAttributes(mode);
      return CaptureToBitmap(size, attr);
    }

    /// <summary>
    /// Capture View contents to a bitmap using a display mode description to define
    /// how drawing is performed.
    /// </summary>
    /// <param name="mode">The display mode.</param>
    /// <returns>A new bitmap.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Display.DisplayModeDescription mode)
    {
      return CaptureToBitmap(ClientRectangle.Size, mode);
    }

    /// <summary>
    /// Capture View contents to a bitmap using display attributes to define how
    /// drawing is performed.
    /// </summary>
    /// <param name="size">The width and height of the returned bitmap.</param>
    /// <param name="attributes">The specific display mode attributes.</param>
    /// <returns>A new bitmap.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Drawing.Size size, Rhino.Display.DisplayPipelineAttributes attributes)
    {
      IntPtr pConstView = ConstPointer();
      IntPtr pAttributes = attributes.ConstPointer();
      IntPtr pRhinoDib = UnsafeNativeMethods.CRhinoDib_New();
      Rhino.Drawing.Bitmap rc = null;
      if (UnsafeNativeMethods.CRhinoView_CaptureToBitmap(pConstView, pRhinoDib, size.Width, size.Height, pAttributes))
      {
        IntPtr hBmp = UnsafeNativeMethods.CRhinoDib_Bitmap(pRhinoDib);
        if (IntPtr.Zero != hBmp)
          rc = Rhino.Drawing.Image.FromHbitmap(hBmp);
      }
      UnsafeNativeMethods.CRhinoDib_Delete(pRhinoDib);
      return rc;
    }

    /// <summary>
    /// Captures view contents to a bitmap using display attributes to define how
    /// drawing is performed.
    /// </summary>
    /// <param name="attributes">The specific display mode attributes.</param>
    /// <returns>A new bitmap.</returns>
    public Rhino.Drawing.Bitmap CaptureToBitmap(Rhino.Display.DisplayPipelineAttributes attributes)
    {
      return CaptureToBitmap(ClientRectangle.Size, attributes);
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
    /// The MainViewport will always return this RhinoView's m_vp.
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
        return m_main_viewport_id;
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
    /// Visibility of the viewport title window.
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
    //  virtual const CRuntimeClass* GetDefaultDisplayPipelineClass() const;
    //  CRhinoDisplayPipeline*        CreateDisplayPipeline(const CRuntimeClass*,  bool  bAttach = true);
    //  bool                          SetupDisplayPipeline(void);
    //  bool                          AttachDisplayPipeline(CRhinoDisplayPipeline* = NULL, bool  bDeleteExisting = true);
    //  CRhinoDisplayPipeline*        DetachDisplayPipeline(void);
    //  CRhinoDisplayPipeline*        DisplayPipeline(void);
    //  const CRhinoDisplayPipeline*  DisplayPipeline(void) const;
    //  CDisplayPipelineAttributes*         DisplayAttributes(void);
    //  const CDisplayPipelineAttributes*   DisplayAttributes(void) const;
    //  void SetFloatingRhinoViewPlacement( const WINDOWPLACEMENT& wp);
    //  WINDOWPLACEMENT* FloatingRhinoViewPlacement() const;
    // 
    //  void EnableCameraIcon( CRhinoView* view );  <-IN CRhinoDoc, but should probably be in CRhinoView

    /// <summary>
    /// Floating state of RhinoView.
    /// if true, then the view will be in a floating frame window. Otherwise
    /// the view will be embeded in the main frame.
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

    /// <summary>
    /// Remove this View from Rhino. DO NOT attempt to use this instance of this
    /// class after calling Close.
    /// </summary>
    /// <returns>true on success</returns>
    public bool Close()
    {
      IntPtr pView = NonConstPointer();
      int doc_id = this.Document.DocumentId;
      bool rc = UnsafeNativeMethods.CRhinoDoc_CloseRhinoView(doc_id, pView);
      if (rc)
      {
        m_ptr = IntPtr.Zero;
        m_main_viewport_id = Guid.Empty;
        g_view_list.Remove(this);
      }
      return rc;
    }

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

    static void ViewEventHelper(EventHandler<ViewEventArgs> handler, IntPtr pView)
    {
      if (handler == null) return;
      try
      {
        handler(null, new ViewEventArgs(pView));
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    private static void OnCreateView(IntPtr pView)   { ViewEventHelper(m_create_view, pView); }
    private static void OnDestroyView(IntPtr pView)  { ViewEventHelper(m_destroy_view, pView); }
    private static void OnSetActiveView(IntPtr pView){ ViewEventHelper(m_setactive_view, pView); }
    private static void OnRenameView(IntPtr pView)   { ViewEventHelper(m_rename_view, pView); }

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

  public class PageViewSpaceChangeEventArgs : EventArgs
  {
    readonly IntPtr m_pPageView;
    internal PageViewSpaceChangeEventArgs(IntPtr pPageView, Guid newActiveId, Guid oldActiveId)
    {
      m_pPageView = pPageView;
      NewActiveDetailId = newActiveId;
      OldActiveDetailId = oldActiveId;
    }

    RhinoPageView m_pageview;
    /// <summary>
    /// The page view on which a different detail object was set active.
    /// </summary>
    public RhinoPageView PageView
    {
      get
      {
        if (m_pageview == null && m_pPageView != IntPtr.Zero)
          m_pageview = RhinoView.FromIntPtr(m_pPageView) as RhinoPageView;
        return m_pageview;
      }
    }

    /// <summary>
    /// The id of the detail object was set active.  Note, if this id is
    /// equal to Guid.Empty, then the active detail object is the page
    /// view itself.
    /// </summary>
    public Guid NewActiveDetailId { get; private set; }

    /// <summary>
    /// The id of the previously active detail object. Note, if this id
    /// is equal to Guid.Empty, then the active detail object was the
    /// page view itself.
    /// </summary>
    public Guid OldActiveDetailId { get; private set; }
  }
}

#endif