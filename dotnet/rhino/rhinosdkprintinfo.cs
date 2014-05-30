#pragma warning disable 1591
using System;

namespace Rhino.Display
{
  //NOT public on purpose. This still needs a lot of work
#if RHINO_SDK
  /// <summary>
  /// Used to hold the information required to generate high resolution output
  /// of a RhinoViewport.  This is used for generating paper prints or image files
  /// </summary>
  class ViewCapture : IDisposable
  {
    IntPtr m_pPrintInfo; //CRhinoPrintInfo*

    IntPtr NonConstPointer() { return m_pPrintInfo; }

    public ViewCapture()
    {
      m_pPrintInfo = UnsafeNativeMethods.CRhinoPrintInfo_New();
    }

    RhinoViewport m_viewport;
    /// <summary>
    /// The RhinoViewport that this view capture info is based off of.
    /// </summary>
    public RhinoViewport Viewport
    {
      get { return m_viewport; }
      set
      {
        m_viewport = value;
        IntPtr pViewport = (m_viewport==null) ? IntPtr.Zero : m_viewport.ConstPointer();
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoPrintInfo_SetViewport(pThis, pViewport );
      }
    }

    //public Rhino.Drawing.Bitmap ToBitmap(Rhino.Drawing.Size size)
    //{
    //  return ToBitmap(size, DrawFrameStages.All);
    //}
    //
    //[CLSCompliant(false)]
    //public Rhino.Drawing.Bitmap ToBitmap(Rhino.Drawing.Size size, DrawFrameStages enabledStages)
    //{
    //  Rhino.Drawing.Bitmap rc = new Rhino.Drawing.Bitmap(size.Width, size.Height, Rhino.Drawing.Imaging.PixelFormat.Format32bppArgb);
    //  //IntPtr pConstThis = ConstPointer();
    //
    //  //var bitmap_data = rc.LockBits(new Rhino.Drawing.Rectangle(0, 0, size.Width, size.Height), Rhino.Drawing.Imaging.ImageLockMode.WriteOnly, Rhino.Drawing.Imaging.PixelFormat.Format32bppArgb);
    //  //if (UnsafeNativeMethods.CRhinoPrintInfo_DrawToSingleDib(pConstThis, bitmap_data.Scan0, size.Width, size.Height, TransparentBackground, (uint)enabledStages))
    //  //{
    //  //}
    //  //rc.UnlockBits(bitmap_data);
    //  return rc;
    //}

    public bool TransparentBackground { get; set; }

    const int idxDrawBackground = 0;
    const int idxDrawGrid = 1;
    const int idxDrawAxis = 2;
    const int idxDrawLineWeights = 3;
    const int idxDrawBackgroundBitmap = 4;
    const int idxDrawWallpaper = 5;
    const int idxDrawLockedObjects = 6;

    /*
    bool GetBool(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoPrintInfo_GetBool(pConstThis, which);
    }
    void SetBool(int which, bool val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoPrintInfo_SetBool(pThis, which, val);
    }

    public bool DrawBackground
    {
      get { return GetBool(idxDrawBackground); }
      set { SetBool(idxDrawBackground, value); }
    }

    public bool DrawGrid
    {
      get { return GetBool(idxDrawGrid); }
      set { SetBool(idxDrawGrid, value); }
    }

    public bool DrawAxis
    {
      get { return GetBool(idxDrawAxis); }
      set { SetBool(idxDrawAxis, value); }
    }

    public bool DrawLineWeights
    {
      get { return GetBool(idxDrawLineWeights); }
      set { SetBool(idxDrawLineWeights, value); }
    }

    public bool DrawBackgroundBitmap
    {
      get { return GetBool(idxDrawBackgroundBitmap); }
      set { SetBool(idxDrawBackgroundBitmap, value); }
    }

    public bool DrawWallpaper
    {
      get { return GetBool(idxDrawWallpaper); }
      set { SetBool(idxDrawWallpaper, value); }
    }

    public bool DrawLockedObjects
    {
      get { return GetBool(idxDrawLockedObjects); }
      set { SetBool(idxDrawLockedObjects, value); }
    }
    */

    #region IDisposable implementation
    /// <summary>Actively reclaims unmanaged resources that this instance uses.</summary>
    public void Dispose()
    {
      Dispose(true);
    }

    /// <summary>Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().</summary>
    ~ViewCapture()
    {
      Dispose(false);
    }

    private void Dispose(bool isDisposing)
    {
      if( m_pPrintInfo != IntPtr.Zero )
      {
        UnsafeNativeMethods.CRhinoPrintInfo_Delete(m_pPrintInfo);
      }
      m_pPrintInfo = IntPtr.Zero;

      if (isDisposing)
        GC.SuppressFinalize(this);
    }

    #endregion
  }
#endif
}



/*
 * /////////////////////////////////////////////////////////////////////  
// The CRhinoPrintInfo class is used to hold the information required
// to generate high resolution output of a CRhinoViewport. This is
// used for generating paper prints or image files.
/////////////////////////////////////////////////////////////////////  
class RHINO_SDK_CLASS CRhinoPrintInfo
{
public:
  // color settings for generated output
  enum color_mode { display_color=0,
                    print_color=1,
                    black_white=2 };

  // how the CRhinoViewport is mapped to the output
  enum view_area { area_view = 0,     //best fit rectangle of what is displayed in a CRhinoView
                   area_extents = 1,  //zoom projection to all visible geometry in a viewport
                   area_window = 2 }; //use window rectangle defined by user

  // how the window is expanded off of the window anchor (location of m_anchor_location)
  enum window_target_location { wt_lowerleft  = 0,
                                wt_upperleft  = 1,
                                wt_lowerright = 2,
                                wt_upperright = 3,
                                wt_center     = 4};
public:
  // Description:
  //   Create an invalid info that needs to be filled
  CRhinoPrintInfo();

  // Description:
  //   Determines if this has valid rectangles and a CRhinoViewport to work with
  // Return:
  //   true if this is a valid CRhinoPrintInfo
  bool IsValid() const;

  // Description:
  //   Set the CRhinoViewport that this info is based off of. This version only
  //   works with CRhinoViewports that are "attached" to CRhinoViews since it needs to 
  //   look up CRhinoViewports from the available view list.
  // Parameters:
  //   viewport_id: [in] unique ID of an existing CRhinoViewport. This can be
  //   extracted from a viewport through CRhinoViewport::ViewportId()
  void SetViewport(const ON_UUID& viewport_id);

  // Description:
  //   Set the CRhinoViewport that this layout info is based off of.
  //   Make sure that pViewport is valid during the entire scope of this info object
  // Parameters:
  //   pViewport: [in] the viewport to use
  void SetViewport(const CRhinoViewport* pViewport);

  // Description:
  //   Get the CRhinoViewport that this info is set up to print.
  const CRhinoViewport* GetRhinoViewport() const;

  // Description:
  //   Get the Id for the viewport this info is set up to print
  ON_UUID GetRhinoViewportId() const;

  // Description:
  //   Set up the rectangles used to define the output image. This function
  //   is used for image file output because there typically is no "unprintable"
  //   portion for the output
  // Parameters:
  //   rect: [in] used for the paper rect, available rect, and draw rect
  //              all three rects are set to the same size. The rectangles are
  //              defined in dot positions on a printed page (no implied unit system)
  void SetLayoutRectangles(const CRect& rect);

  // Description:
  //   Sets the draw rect equal to the available rect. This sets the drawable area to the
  //   maximum available with the current settings.
  void MaximizeDrawRect();

  // Description:
  //   Update the layout rectangles and dpi settings to match a printer dc
  // Parameters:
  //   printer_dc: [in] device context for the printer this layout will
  //              probably be eventually sent to
  //   maintain_margins: [in] set the m_draw_rect to keep the same margins as are currently set
  void UpdateFromPrinterDC(CDC& printer_dc, bool maintain_margins);

  // Description:
  //   A CRhinoPrintInfo's display attributes are based on it's CRhinoViewport with
  //   a few possible overrides that are specific to this layout (background, grid, axis)
  // Return:
  //   display attributes specific to this CRhinoPrintInfo
  CDisplayPipelineAttributes DisplayAttributes() const;

  // Description:
  //   Get factor used to scale tiles at the target DPI up to the device DPI
  //   Images may be generated at a lower resolution than the device DPI and then
  //   stretch blitted up to the device resolution.
  //   DeviceDPI / StretchFactor = the resolution that images are being generated at
  int StretchFactor() const;

  // Description:
  //   Margins are always measured from the edge of the page (m_paper_rect)
  // Parameters:
  //   units: [in] units to return margins in
  //   left,right,top,bottom: [out] the margins to return
  // Return
  //   true if successful
  //   false if unsuccessful (this could happen if there is no set device_dpi)
  bool GetMargins(ON::unit_system units, double& left, double& right, double& top, double& bottom) const;
  bool SetMargins(ON::unit_system units, double left, double right, double top, double bottom);

  // Description:
  //   Get minimum/maximum print margins available for this page.
  //   Margins are always measured from the edge of the page
  // Parameters:
  //   minimum: [in] return minimum margins if true (otherwise maximum margins)
  //   units: [in] units to return margins in
  //   left,right,top,bottom: [out] the margins to return
  // Return
  //   true if successful
  //   false if unsuccessful (this could happen if there is no set device_dpi)
  bool GetMarginLimits(bool minimum,
                       ON::unit_system units,
                       double& left, double& right, double& top, double& bottom) const;

  // Description:
  //   Sets up an ON_Viewport that would be used for printing. This is the core function used to
  //   get all of the information stored in this class and generate a viewport mapped to an output device.
  // Parameters:
  //   viewport: [out] this will get set up with the projection information for an output device if the
  //             function is successful
  // Returns:
  //   true on success
  bool GetDeviceViewport(ON_Viewport& viewport) const;

  // Description:
  //   Creates a CRhinoViewport mapped to the output device with the appropriate view_area settings.
  //   NOTE: you MUST delete the returned CRhinoViewport when you are done with it.
  // Return:
  //   a CRhinoViewport on success
  //   NULL on failure
  CRhinoViewport* CreateDeviceViewport() const;

  // Description:
  //   returns the width/height of the currently defined paper area
  //   0.0 if the paper area is not defined or invalid
  double PaperAspectRatio() const;

  // Description:
  //   returns the width/height of the currently defined draw area
  //   0.0 if the draw area is not defined or invalid
  double DrawAspectRatio() const;

  // Description:
  //   Get the size of the printed output paper in a specified unit system. Custom
  //   units are not allowed.
  // Parameters:
  //   units: [in] unit system for the output sizes. custom units are not allowed
  //   width, height: [out] size of output paper on success
  // Return:
  //   true on success, false on failure
  bool GetPaperSize(ON::unit_system units, double& width, double& height) const;
  bool GetAvailableSize(ON::unit_system units, double& width, double& height) const;

  // Description:
  //   Gets the weight used for drawing an object with the current print info's settings
  // Parameters:
  //   pAttributes: [in] rhino object attributes to use for getting weight
  // Returns:
  //   -1 if the object has "No Plot" set weight
  //   otherwise, return a value greater than zero describing the weight used for draw functions
  int GetPrintWeight( const CRhinoObjectAttributes* pAttributes ) const;

  // Description:
  //   Gets the color used for drawing an object with the current print info's settings
  // Parameters:
  //   pAttributes: [in] rhino object attributes to use for getting color
  // Returns:
  //   The Color for the object
  ON_Color GetPrintColor( const CRhinoObjectAttributes* pAttributes  ) const;


  CRect GetScaledDrawRect() const;
  double ScaledDPI() const;
  double GetLineTypeScale() const;

public:
  //////////////////////////// Draw Routines //////////////////////////////////
  // Description:
  //   Draw the layout to a single DIB. This is all done in memory, so make sure that the resulting
  //   DIB is not too large. Tiled drawing may occur if the DIB is larger than the screen resolution,
  //   but this is handled internally by this function.
  // Parameters:
  //   full_dib: [out] the dib to draw to
  //   pAttributes: [in] display attributes to use for drawing
  //                     (typically from CRhinoPrintInfo::DisplayAttributes())
  //   pPipeline: [in] Display Pipeline to base drawing off of.
  //   force_GDI_pipeline: [in] if set to true, drawing is performed using a GDI display pipeline.
  // Return:
  //   true if successful
  bool DrawToSingleDib(CRhinoUiDib& full_dib,
                       const CDisplayPipelineAttributes* pAttributes,
                       const CRhinoDisplayPipeline* pPipeline,
                       bool force_GDI_pipeline) const;

  // Description:
  //   Draw the layout to a single DIB. This is all done in memory, so make sure that the resulting
  //   DIB is not too large. Tiled drawing may occur if the DIB is larger than the screen resolution,
  //   but this is handled internally by this function. This version of the function just "cooks" up
  //   the appropriate display pipelines and attributes and calls the other version of this function.
  // Parameters:
  //   full_dib: [out] the dib to draw to
  //   force_GDI_pipeline: [in] if set to true, drawing is performed using a GDI display pipeline and
  //                       not the pipeline attached to the viewport (which may or may not be GDI).
  // Return:
  //   true if successful
  bool DrawToSingleDib(CRhinoUiDib& full_dib,
                       bool force_GDI_pipeline = false,
                       bool bForPrinting = true) const;


  // Description:
  //   Draws a sub-rect area of the full printable area rectangle. Tiled drawing is used for generating
  //   large images from hardware accelerated graphics. Graphics cards have a limited area that they
  //   can reasonably render to (typically screen resolution). This is called by the DrawToSingleDib
  //   function and directly from printing code to send smaller bitmaps to the printer.
  // Parameters:
  //   tile_dib:  [out] the dib to draw to
  //   tile_rect: [in] portion of printable area to draw
  //   full_vp:   [in] viewport that we are using to get geometry and projection information from
  //   pPipeline: [in] display pipeline to use for drawing
  // Return:
  //   true if successful
  bool DrawTileToDib( CRhinoUiDib& tile_dib,
                      const CRect& tile_rect,
                      const CRhinoViewport& full_vp,
                      CRhinoDisplayPipeline* pPipeline,
                      CDisplayPipelineAttributes* pAttributes) const;

  // Description:
  //   Draws a display pipeline to a device context using GDI
  // Parameters:
  //   draw_dc: [in] The device context to draw to. This could be a printer or a DIB.
  //   pAttributes: [in] display attributes to use for drawing. If NULL, the values from
  //                CRhinoPrintInfo::DisplayAttributes() are used
  //   printer_output: [in] Use styles in print info that draw to DC as if it was being sent to a printer
  // Return:
  //   true if successful
  bool DrawWithGDI( CDC& draw_dc,
                    const CDisplayPipelineAttributes* pAttributes = NULL,
                    bool printer_output = true ) const;

  // Description:
  //   Uses GDI to draw m_HeaderText and m_FooterText on top of the final image.
  //   This should be called after calling the above Draw routines
  // Return:
  //   true is successful
  bool DrawHeaderAndFooter( CDC* pDC ) const;

  // Description:
  //   Gets the print information that is currently being used inside a CRhinoPrintInfo::Draw.... routine.
  // Returns:
  //   pointer to the active print info that is drawing.
  //   NULL if no CRhinoPrintInfo is currently drawing
  // Remarks:
  //   DO NOT hold onto this pointer outside the scope of your function as it can quickly change
  static const CRhinoPrintInfo* GetActiveDrawPrintInfo();
public:
  //////////////////////////// Tiling Functions //////////////////////////////////
  // Shaded/Rendered viewports are printed as a series of tileds bitmaps using the
  // display pipeline's graphics technology (i.e. OpenGL or DirectX).
  // These technologies are designed for drawing to an area the size of a screen,
  // so tiles are kept to these sizes and a final bitmap is constructed from the tiles.

  // Description:
  //   Get the preferred tile size for tiled image generation. The width and height must
  //   be less than the desktop size in order to allow for accelerated graphics.
  // Parameters:
  //   width, height: [out] preferred tile size in pixels
  void GetPreferredTileSize(int& width, int& height) const;

  // Description:
  //   Get number of tiles required to create a tiled image at the current settings.
  //   Tile size is based on drawing size, target dpi, and preferred tile height/width
  // Parameters:
  //   rows:    [out] if not NULL, the number of tiles vertical
  //   columns: [out] if not NULL, the number of tiles across
  // Return:
  //   total number of tiles on success. 0 if an error occurs
  int GetTileCount(int* rows = NULL, int* columns = NULL) const;

  // Description:
  //   Get a rectangle that represents a subtile of the entire image. Tiles are
  //   in a coordinate system local to the m_draw_rect and scaled to the scale
  //   factor. This means Tile(0,0) will always have a top,left value of 0,0 and
  //   this maps to the top,left corner of the draw rectangle.
  // Parameters:
  //   row, column: [in] position of tile to retrieve
  // Returns:
  //   tile rectangle on success. empty rectangle on failure
  CRect GetTile(int row, int column) const;

  // Description:
  //   Same as GetTile(row,column), but addresses the tile by index number. This
  //   way you can do the following:
  //       for(int i=0; i<GetTileCount(); i++){
  //         CRect tile_rect = GetTile(i);
  //         ...
  //       }
  CRect GetTile(int index) const;

  // Description:
  //   Creates a rectangle that represents an image tile on the output device.
  //   This tile is at full resolution and positioned relative to the output
  //   device available print rectangle (m_available_print_rect)
  // Parameters:
  //   tile_rect: [in] tile relative to the draw rect and scaled down by the
  //              scale factor (typically from the GetTile functions.)
  CRect GetDeviceTile(const CRect& tile_rect) const;

public:
  ////// display attribute overrides //////
  //   When sending the image to a file or printer, it is sometimes best to override the display
  //   attributes that are associated with the CRhinoView. This is typical with printing where
  //   we may not want to fill in the background with a color or draw the display grid.
  bool m_bDrawBackground;
  bool m_bDrawGrid;
  bool m_bDrawAxis;
  bool m_bDrawLineWeights;
  bool m_bDrawBackgroundBitmap;
  bool m_bDrawWallpaper;
  bool m_bDrawLockedObjects; //default true
private:
  unsigned char m_reserved1;
public:
  double m_point_scale;       // size of point objects in millimeters
                              // if scale <= 0 the size is minimized so points are always drawn as small as possible
  double m_arrowhead_size_mm; // arrowhead size in millimeters
  double m_textdot_size;      // Font point size use for printing text dots (default = 10.0)

  color_mode m_ColorMode;

  ELineEndCapStyle m_endcap_style;  // Curve pen styles for thick lines
  ELineJoinStyle   m_join_style;

  // default is false. Linetype scales are normally generated right before
  // printing in order to get linetypes to print to the same lengths as
  // defined. If true, the m_saved_linetype_scale is used. This is useful
  // if you want to print using the current display linetype scale or if
  // you want to print using some arbitrary linetype scale
  bool m_bUseSavedLineTypeScale;

  // scaling factor to apply to object print widths (typically 1.0). This is
  // helpful when printing something at 1/2 scale and having all of the curves
  // print 1/2 as thick
  double m_print_width_scale;

  // Line thickness used to print objects with no defined thickness (in mm)
  double m_print_width_default;

  //how to map the viewport to the output device
  view_area m_view_area; 

  //only display/print selected objects (default is false)
  bool m_bOnlySelectedObjects;

  double m_saved_linetype_scale;

  // If we want to put text on top of the print assign it to the header and footer text
  // #NOTES# will be replaced with the document notes
  // #FILENAME# will be replaced with the document filename
  CString m_HeaderText;
  CString m_FooterText;

  static const wchar_t* FormatString_Notes();
  static const wchar_t* FormatString_Filename();

  ////// layout rectangles //////

  CRect m_paper_rect; // physical size of paper in dots (no margins - the actual paper)
  CRect m_available_print_rect; //maximum portion of paper that can be drawn to
  CRect m_draw_rect;  // location on m_paper_rect that image is drawn to in dots
                      // this recangle holds information about where to draw and
                      // what the margins are.
                      // m_draw_rect must always be inside of m_available_print_rect

  ////// Output resolution settings //////

  // The actual dot per inch resolution of the final output. This is the resolution
  // of the selected printer when sending an image to a printer. For raster image files,
  // this is used to determine image space sizes (things like line witdhs)
  double m_device_dpi;

  // Resolution cap when generating rester output. Images are stretched to
  // m_device_dpi when m_device_dpi > m_maximum_dpi
  // Many photo printers have default resolutions of around 1200 dpi
  // which would demand enormous image size requirements if printing at full resolution
  // default DPI resolutions are set at 300 DPI. 
  // Personal Opinion (S. Baer):
  //   300DPI is a great setting for almost all prints
  //   600DPI looks only slightly better (but not worth 4x memory use)
  //   I can't see the difference between 600DPI and 1200 DPI
  double m_maximum_dpi;

public:
  window_target_location GetAnchorLocation() const;
  void SetAnchorLocation(window_target_location anchor);

  void SetOffset( ON::unit_system offset_units, bool from_margin, double x, double y);
  void GetOffset( ON::unit_system offset_units, bool& from_margin, double& x, double& y) const;
  void GetCenterEquivalientOffset( ON::unit_system offset_units, bool& from_margin, double& x, double& y) const;
private:
  const CRhinoViewport* m_pViewport;  // the viewport that is used to generate drawings
  ON_UUID m_viewport_uuid;            // If m_pViewport==NULL, this can be used to look
                                      // up a viewport from the view list

  // window points are defined as u,v locations on plane parallel to the
  // camera frame with an origin at 0,0,0 when the viewport projection is parallel
  // When the viewport projection is perspective, the points are percent positions
  // on the screen port
  ON_2dPoint m_window_point1, m_window_point2;

  window_target_location m_anchor_location;

  bool   m_offset_from_margin;  // if true, offset is relative to margin edge
                                // if false, offset is relative to paper edge
  double m_offset_x; // horizontal offset in millimeters
  double m_offset_y; // vertical offset in millimeters
public:
  ////// scaling functions /////////
  double GetModelScale(ON::unit_system paper_units, ON::unit_system model_units) const;

  void SetModelScaleToValue(double scale);
  void SetModelScaleToFit(bool prompt_if_change);

  void SetModelScaleType(int type);
  int  GetModelScaleType() const;

  bool IsScaledToFit() const;

  static void GetPlotScaleNames(ON_ClassArray<ON_wString>& names, bool include_architectural);

private:
  static ON_ClassArray<ON_wString> m_plot_scale_names;
  static ON_SimpleArray<double> m_plot_scale_values;
  static int m_architectural_index; //start index of architectural scales in the plot_scale arrays
  // Description:
  //   Plot scale arrays are lazily created. ConstructPlotScaleArrays is always internally called
  //   before directly accessing the arrays.
  static void ConstructPlotScaleArrays();

  int m_scale_index;
  double m_custom_model_scale;  //if this is -1.0, use scale from list of hardcoded scales


public:
  // Functions used by specific classes
  // NOTE: For Internal Use

  // Description:
  //   Get the rectangles in screen space coordinates. This is used by the print dialog during
  //   window selection of the print area to show preview rectangles on the view.
  // Parameters:
  //   paper: [out] the full page rectangle
  //   available: [out] the portion of the paper that can drawn/printed to. Always inside paper rectangle
  //   draw: [out] defined area the will be drawn/printed to. Always inside available rectangle
  // Return:
  //   true if the layout is valid and was able to successfully return rectangles
  bool GetScreenRects(CRect& paper, CRect& available, CRect& draw) const;

  // Description:
  //   Used by DrawTileToDib to create tile viewports from a large single viewport
  // Parameters:
  //   full_viewport: [in] large single viewport that tiles are taken from
  //   tile_viewport: [out] sub portion of the full viewport
  //   tile_rect: [in] 
  // Returns:
  //   true on success
  bool GetTileViewport(const ON_Viewport& full_viewport, ON_Viewport& tile_viewport, const CRect& tile_rect) const;

  // Description:
  //   Create a layout info that is based on this layout info but sized to fit inside
  //   a preview area. Used by the print preview window to generate a preview image
  // Parameters:
  //   preview_area: [in] size to fit the layout info inside of
  //   preview: [out] sized layout for preview purposes
  // Return:
  //   true if successful
  bool GetPreviewLayout(const CSize& preview_area, CRhinoPrintInfo& preview) const;

  // Description:
  //   Defines the window print area rectangle.
  // Parameters:
  //   corner1: [in] world coordinate corner first pick for defining the window rectangle
  //   corner2: [in] world coordinate corner second pick for defining the window rectangle
  void SetWindowRect( ON_3dPoint corner1, ON_3dPoint corner2 );
  void SetWindowRectFromScreen( ON_2dPoint screen1, ON_2dPoint screen2 );

  void MoveWindowRect( ON_2dVector screen_delta );

  void InvalidateWindowRect();

  // Description:
  //   Return Window Area Size
  double GetWindowAreaWidth() const;

  void LoadProfile(LPCTSTR lpszSection, CRhinoProfileContext& pc);
  void SaveProfile(LPCTSTR lpszSection, CRhinoProfileContext& pc) const;
};


 */