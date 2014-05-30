using System;
using Rhino.Geometry;
using Rhino.Runtime.InteropWrappers;

// Most of these should not need to be wrapped. Some of their
// functionality is merged into other wrapper classes
namespace Rhino.DocObjects
{
  //public class ON_3dmUnitsAndTolerances{}
  //public class ON_3dmAnnotationSettings { }
  //public class ON_3dmConstructionPlaneGridDefaults { }

  // Can't add a cref to an XML comment here since the NamedConstructionPlaneTable
  // is not included in the OpenNURBS flavor build of RhinoCommon

  /// <summary>
  /// Represents a contruction plane inside the document.
  /// <para>Use Rhino.DocObjects.Tables.NamedConstructionPlaneTable
  /// methods and indexers to add and access a <see cref="ConstructionPlane"/>.</para>
  /// </summary>
  public class ConstructionPlane
  {
    internal static ConstructionPlane FromIntPtr(IntPtr pConstructionPlane)
    {
      if (IntPtr.Zero == pConstructionPlane)
        return null;
      ConstructionPlane rc = new ConstructionPlane();
      using (var sh = new StringHolder())
      {
        IntPtr ptr_string = sh.NonConstPointer();
        UnsafeNativeMethods.ON_3dmConstructionPlane_Copy(pConstructionPlane,
                                                         ref rc.m_plane,
                                                         ref rc.m_grid_spacing,
                                                         ref rc.m_snap_spacing,
                                                         ref rc.m_grid_line_count,
                                                         ref rc.m_grid_thick_frequency,
                                                         ref rc.m_bDepthBuffered,
                                                         ptr_string);
        rc.m_name = sh.ToString();
      }
      return rc;
    }

    internal IntPtr CopyToNative()
    {
      IntPtr ptr_cplane = UnsafeNativeMethods.ON_3dmConstructionPlane_New( ref m_plane,
                                                                        m_grid_spacing,
                                                                        m_snap_spacing,
                                                                        m_grid_line_count,
                                                                        m_grid_thick_frequency,
                                                                        m_bDepthBuffered,
                                                                        m_name);
      return ptr_cplane;
    }

    internal Plane m_plane;
    internal double m_grid_spacing;
    private double m_snap_spacing;
    internal int m_grid_line_count;
    internal int m_grid_thick_frequency;
    internal bool m_bDepthBuffered;
    private string m_name;

    internal bool m_bShowGrid;
    internal bool m_bShowAxes;
    Rhino.Drawing.Color m_thick_line_color;
    Rhino.Drawing.Color m_grid_x_color;
    Rhino.Drawing.Color m_grid_y_color;
    Rhino.Drawing.Color m_grid_z_color;

    #region ON_3dmConstructionPlane

    /// <summary>
    /// Initializes a new instance of <see cref="ConstructionPlane"/>.
    /// </summary>
    public ConstructionPlane()
    {
      m_plane = Plane.WorldXY;
      m_grid_spacing = 1.0;
      m_grid_line_count = 70;
      m_grid_thick_frequency = 5;
      m_bDepthBuffered = true;
      m_bShowGrid = true;
      m_bShowAxes = true;
#if RHINO_SDK
      ThinLineColor = ApplicationSettings.AppearanceSettings.GridThinLineColor;
      m_thick_line_color = ApplicationSettings.AppearanceSettings.GridThickLineColor;
      m_grid_x_color = ApplicationSettings.AppearanceSettings.GridXAxisLineColor;
      m_grid_y_color = ApplicationSettings.AppearanceSettings.GridYAxisLineColor;
      m_grid_z_color = ApplicationSettings.AppearanceSettings.GridZAxisLineColor;
#endif
    }

    /// <summary>
    /// Gets or sets the geometric plane to use for construction.
    /// </summary>
    public Plane Plane
    {
      get { return m_plane; }
      set { m_plane = value; }
    }

    /// <summary>
    /// Gets or sets the distance between grid lines.
    /// </summary>
    public double GridSpacing
    {
      get { return m_grid_spacing; }
      set { m_grid_spacing = value; }
    }

    /// <summary>
    /// when "grid snap" is enabled, the distance between snap points.
    /// Typically this is the same distance as grid spacing.
    /// </summary>
    public double SnapSpacing
    {
      get { return m_snap_spacing; }
      set { m_snap_spacing = value; }
    }

    /// <summary>
    /// Gets or sets the total amount of grid lines in each direction.
    /// </summary>
    public int GridLineCount
    {
      get { return m_grid_line_count; }
      set { m_grid_line_count = value; }
    }

    /// <summary>
    /// Gets or sets the recurrence of a wider line on the grid.
    /// <para>0: No lines are thick, all are drawn thin.</para>
    /// <para>1: All lines are thick.</para>
    /// <para>2: Every other line is thick.</para>
    /// <para>3: One line in three lines is thick (and two are thin).</para>
    /// <para>4: ...</para>
    /// </summary>
    public int ThickLineFrequency
    {
      get { return m_grid_thick_frequency; }
      set { m_grid_thick_frequency = value; }
    }

    /// <summary>
    /// Gets or sets whether the grid is drawn on top of geometry.
    /// <para>false=grid is always drawn behind 3d geometry</para>
    /// <para>true=grid is drawn at its depth as a 3d plane and grid lines obscure things behind the grid.</para>
    /// </summary>
    public bool DepthBuffered
    {
      get { return m_bDepthBuffered; }
      set { m_bDepthBuffered = value; }
    }

    /// <summary>
    /// Gets or sets the name of the grid.
    /// </summary>
    public string Name
    {
      get
      {
        return m_name;
      }
    }
#endregion

    #region display extras
    internal int[] ArgbColors()
    {
      int[] rc = new int[5];
      rc[0] = m_thick_line_color.ToArgb();
      rc[1] = m_thick_line_color.ToArgb();
      rc[2] = m_grid_x_color.ToArgb();
      rc[3] = m_grid_y_color.ToArgb();
      rc[4] = m_grid_z_color.ToArgb();
      return rc;
    }

    /// <summary>
    /// Gets or sets whether the grid itself should be visible. 
    /// </summary>
    public bool ShowGrid
    {
      get { return m_bShowGrid; }
      set { m_bShowGrid = value; }
    }

    /// <summary>
    /// Gets or sets whether the axes of the grid shuld be visible.
    /// </summary>
    public bool ShowAxes
    {
      get { return m_bShowAxes; }
      set { m_bShowAxes = value; }
    }

    /// <summary>
    /// Gets or sets the color of the thinner, less prominent line.
    /// </summary>
    public Rhino.Drawing.Color ThinLineColor { get; set; }

    /// <summary>
    /// Gets or sets the color of the thicker, wider line.
    /// </summary>
    public Rhino.Drawing.Color ThickLineColor
    {
      get { return m_thick_line_color; }
      set { m_thick_line_color = value; }
    }

    /// <summary>
    /// Gets or sets the color of the grid X-axis mark.
    /// </summary>
    public Rhino.Drawing.Color GridXColor
    {
      get { return m_grid_x_color; }
      set { m_grid_x_color = value; }
    }

    /// <summary>
    /// Gets or sets the color of the grid Y-axis mark.
    /// </summary>
    public Rhino.Drawing.Color GridYColor
    {
      get { return m_grid_y_color; }
      set { m_grid_y_color = value; }
    }

    /// <summary>
    /// Gets or sets the color of the grid Z-axis mark.
    /// </summary>
    public Rhino.Drawing.Color GridZColor
    {
      get { return m_grid_z_color; }
      set { m_grid_z_color = value; }
    }
    #endregion
  }

  //public class ON_3dmViewPosition { }
  //public class ON_3dmViewTraceImage { }
  //public class ON_3dmWallpaperImage { }
  //public class ON_3dmPageSettings { }
  
  /// <summary>
  /// Represents the name and orientation of a View (and named view).
  /// <para>views can be thought of as cameras.</para>
  /// </summary>
  public class ViewInfo : IDisposable // ON_3dmView
  {
    private IntPtr m_ptr; // ON_3dmView*
    internal object m_parent;

    // for when parent is File3dm
    private readonly Guid m_id = Guid.Empty;
    readonly bool m_named_view_table;

#if RHINO_SDK
    private int m_index = -1;
    internal ViewInfo(RhinoDoc doc, int index)
    {
      m_parent = doc;
      m_index = index;
    }
#endif

    internal ViewInfo(FileIO.File3dm parent, Guid id, IntPtr ptr, bool namedViewTable)
    {
      m_parent = parent;
      m_id = id;
      m_ptr = ptr;
      m_named_view_table = namedViewTable;
    }

    internal IntPtr ConstPointer()
    {
      if (m_ptr != IntPtr.Zero)
        return m_ptr;
      FileIO.File3dm parent_file = m_parent as FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr ptr_const_parent_file = parent_file.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_ViewPointer(ptr_const_parent_file, m_id, m_ptr, m_named_view_table);
      }
#if RHINO_SDK
      if (m_index >= 0)
      {
        RhinoDoc doc = m_parent as RhinoDoc;
        if (doc != null)
          return UnsafeNativeMethods.CRhinoDocProperties_GetNamedView(doc.m_docId, m_index);
      }
#endif
      throw new Runtime.DocumentCollectedException();
    }

    internal IntPtr NonConstPointer()
    {
      FileIO.File3dm parent_file = m_parent as FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr ptr_const_parent_file = parent_file.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_ViewPointer(ptr_const_parent_file, m_id, m_ptr, m_named_view_table);
      }

      if (m_ptr == IntPtr.Zero)
      {
        IntPtr ptr_const_this = ConstPointer();
        m_ptr = UnsafeNativeMethods.ON_3dmView_New(ptr_const_this);
#if RHINO_SDK
        m_index = -1;
        m_parent = null;
#endif
      }
      return m_ptr;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~ViewInfo() { Dispose(false); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr && !(m_parent is FileIO.File3dm))
      {
        UnsafeNativeMethods.ON_3dmView_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }

    /// <summary>
    /// Gets or sets the name of the NamedView.
    /// </summary>
    public string Name
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_3dmView_NameGet(ptr_const_this, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_3dmView_NameSet(ptr_this, value);
      }
    }

    ViewportInfo m_viewport;

    /// <summary>
    /// Gets the viewport, or viewing frustum, associated with this view.
    /// </summary>
    public ViewportInfo Viewport
    {
      get
      {
        return m_viewport ?? (m_viewport = new ViewportInfo(this));
      }
    }
    internal IntPtr ConstViewportPointer()
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_3dmView_ViewportPointer(ptr_const_this);
    }
    internal IntPtr NonConstViewportPointer()
    {
      IntPtr ptr_this = NonConstPointer();
      return UnsafeNativeMethods.ON_3dmView_ViewportPointer(ptr_this);
    }
  }

  /// <summary>
  /// Contains information about the model's position in latitude, longitude,
  /// and elevation for GIS mapping applications.
  /// </summary>
  public class EarthAnchorPoint : IDisposable
  {
    IntPtr m_ptr = IntPtr.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="EarthAnchorPoint"/> class.
    /// </summary>
    public EarthAnchorPoint()
    {
      m_ptr = UnsafeNativeMethods.ON_EarthAnchorPoint_New();
    }

#if RHINO_SDK
    internal EarthAnchorPoint(RhinoDoc doc)
    {
      m_ptr = UnsafeNativeMethods.CRhinoDocProperties_GetEarthAnchorPoint(doc.m_docId);
    }
#endif
    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }
    IntPtr NonConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~EarthAnchorPoint() { Dispose(false); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_EarthAnchorPoint_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }

    const int idxEarthBasepointLatitude = 0;
    const int idxEarthBasepointLongitude = 1;
    const int idxEarthBasepointElevation = 2;
    double GetDouble(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_EarthAnchorPoint_GetDouble(ptr_const_this, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_SetDouble(ptr_this, which, val);
    }

    /// <summary>
    /// Gets or sets a point latitude on earth, in decimal degrees.
    /// +90 = north pole, 0 = equator, -90 = south pole.
    /// </summary>
    public double EarthBasepointLatitude
    {
      get { return GetDouble(idxEarthBasepointLatitude); }
      set { SetDouble(idxEarthBasepointLatitude, value); }
    }

    /// <summary>
    /// Gets or sets the point longitude on earth, in decimal degrees.
    /// <para>0 = prime meridian (Greenwich meridian)</para>
    /// <para>Values increase towards West</para>
    /// </summary>
    public double EarthBasepointLongitude
    {
      get { return GetDouble(idxEarthBasepointLongitude); }
      set { SetDouble(idxEarthBasepointLongitude, value); }
    }

    /// <summary>
    /// Gets or sets the point elevation on earth, in meters.
    /// </summary>
    public double EarthBasepointElevation
    {
      get { return GetDouble(idxEarthBasepointElevation); }
      set { SetDouble(idxEarthBasepointElevation, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating the zero level convention relating to a location on Earth.
    /// </summary>
    public BasepointZero EarthBasepointElevationZero
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        return (BasepointZero)UnsafeNativeMethods.ON_EarthAnchorPoint_GetEarthBasepointElevationZero(ptr_const_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetEarthBasepointElevationZero(ptr_this, (int)value);
      }
    }

    /// <summary>Corresponding model point in model coordinates.</summary>
    public Point3d ModelBasePoint
    {
      get
      {
        Point3d rc = new Point3d(0,0,0);
        IntPtr ptr_const_this = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelBasePoint(ptr_const_this, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelBasePoint(ptr_this, true, ref value);
      }
    }

    /// <summary>Earth directions in model coordinates.</summary>
    public Vector3d ModelNorth
    {
      get
      {
        Vector3d rc = new Vector3d(0, 0, 0);
        IntPtr ptr_const_this = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(ptr_const_this, true, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(ptr_this, true, true, ref value);
      }
    }

    /// <summary>Earth directions in model coordinates.</summary>
    public Vector3d ModelEast
    {
      get
      {
        Vector3d rc = new Vector3d(0, 0, 0);
        IntPtr ptr_const_this = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(ptr_const_this, false, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(ptr_this, false, true, ref value);
      }
    }

     // Identification information about this location
     //ON_UUID    m_id;           // unique id for this anchor point

    /// <summary>
    /// Gets or sets the short form of the identifying information about this location.
    /// </summary>
    public string Name
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_const_this = ConstPointer();
          IntPtr ptr_this = sh.NonConstPointer();
          UnsafeNativeMethods.ON_EarthAnchorPoint_GetString(ptr_const_this, true, ptr_this);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetString(ptr_this, true, value);
      }
    }

    /// <summary>
    /// Gets or sets the long form of the identifying information about this location.
    /// </summary>
    public string Description
    {
      get
      {
        using (var sh = new StringHolder())
        {
          IntPtr ptr_const_this = ConstPointer();
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_EarthAnchorPoint_GetString(ptr_const_this, false, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetString(ptr_this, false, value);
      }
    }
    //ON_wString m_url;
    //ON_wString m_url_tag;      // UI link text for m_url


    /// <summary>
    /// Returns a plane in model coordinates whose X axis points East,
    /// Y axis points North and Z axis points Up. The origin
    /// is set to ModelBasepoint.
    /// </summary>
    /// <returns>A plane value. This might be invalid on error.</returns>
    public Plane GetModelCompass()
    {
      Plane rc = Plane.Unset;
      IntPtr ptr_const_this = ConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_GetModelCompass(ptr_const_this, ref rc);
      return rc;
    }

    /// <summary>
    /// Gets a transformation from model coordinates to earth coordinates.
    /// This transformation assumes the model is small enough that
    /// the curvature of the earth can be ignored.
    /// </summary>
    /// <param name="modelUnitSystem">The model unit system.</param>
    /// <returns>
    /// Transform on success. Inalid Transform on error.
    /// </returns>
    /// <remarks>
    /// If M is a point in model coordinates and E = model_to_earth*M,
    /// then 
    ///   E.x = latitude in decimal degrees
    ///   E.y = longitude in decimal degrees
    ///   E.z = elevation in meters above mean sea level
    /// Because the earth is not flat, there is a small amount of error
    /// when using a linear transformation to calculate oblate spherical 
    /// coordinates.  This error is small.  If the distance from P to M
    /// is d meters, then the approximation error is
    /// latitude error  &lt;=
    /// longitude error &lt;=
    /// elevation error &lt;= 6379000*((1 + (d/6356000)^2)-1) meters
    /// 
    /// In particular, if every point in the model is within 1000 meters of
    /// the m_model_basepoint, then the maximum approximation errors are
    /// latitude error  &lt;=
    /// longitude error &lt;=
    /// elevation error &lt;= 8 centimeters.
    /// </remarks>
    public Transform GetModelToEarthTransform(UnitSystem modelUnitSystem)
    {
      Transform rc = Transform.Unset;
      IntPtr ptr_const_this = ConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_GetModelToEarthTransform(ptr_const_this, (int)modelUnitSystem, ref rc);
      return rc;
    }
  }

  /// <summary>
  /// Specifies enumerated constants used to indicate the zero level convention relating to a location on Earth.
  /// <para>This is used in conjunction with the <see cref="EarthAnchorPoint"/> class.</para>
  /// </summary>
  public enum BasepointZero
  {
    /// <summary>
    /// The ground level is the convention for 0.
    /// </summary>
    GroundLevel = 0,

    /// <summary>
    /// The mean sea level is the convention for 0.
    /// </summary>
    MeanSeaLevel = 1,

    /// <summary>
    /// The center of the planet is the convention for 0.
    /// </summary>
    CenterOfEarth = 2,
  }

  //public class ON_3dmIOSettings { }
  //public class ON_3dmSettings { }
}

namespace Rhino.Render
{
  /// <summary>
  /// Contains settings used in rendering.
  /// </summary>
  public class RenderSettings : IDisposable
  {
#if RHINO_SDK
    readonly RhinoDoc m_doc;
#endif
    IntPtr m_ptr = IntPtr.Zero;

    /// <summary>
    /// Initializes a new instance of the <see cref="RenderSettings"/> class.
    /// </summary>
    public RenderSettings()
    {
      m_ptr = UnsafeNativeMethods.ON_3dmRenderSettings_New(IntPtr.Zero);
    }

#if RHINO_SDK
    internal RenderSettings(RhinoDoc doc)
    {
      m_doc = doc;
    }
#endif
    internal IntPtr ConstPointer()
    {
      if( m_ptr!=IntPtr.Zero )
        return m_ptr;
#if RHINO_SDK
      return UnsafeNativeMethods.ON_3dmRenderSettings_ConstPointer(m_doc.m_docId);
#else
      return IntPtr.Zero;
#endif
    }
    IntPtr NonConstPointer()
    {
      if (m_ptr == IntPtr.Zero)
      {
        IntPtr ptr = ConstPointer();
        m_ptr = UnsafeNativeMethods.ON_3dmRenderSettings_New(ptr);
      }
      return m_ptr;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~RenderSettings() { Dispose(false); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
        UnsafeNativeMethods.ON_3dmRenderSettings_Delete(m_ptr);
      m_ptr = IntPtr.Zero;
    }

    void Commit()
    {
#if RHINO_SDK
      // If this class is not associated with a doc or it is already const then bail
      if (m_doc == null || m_ptr==IntPtr.Zero)
        return;
      // This class is associated with a doc so commit the settings change
      m_doc.RenderSettings = this;
      // Delete the current settings pointer, the next time it is
      // accessed by NonConstPointer() or ConstPointer() it will
      // make a copy of the documents render settings
      UnsafeNativeMethods.ON_3dmRenderSettings_Delete(m_ptr);
      m_ptr = IntPtr.Zero;
#endif
    }

    const int idxAmbientLight = 0;
    const int idxBackgroundColorTop = 1;
    const int idxBackgroundColorBottom = 2;
    Rhino.Drawing.Color GetColor(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      int argb = UnsafeNativeMethods.ON_3dmRenderSettings_GetColor(ptr_const_this, which);
      return Rhino.Drawing.Color.FromArgb(argb);
    }
    void SetColor(int which, Rhino.Drawing.Color c)
    {
      IntPtr ptr_this = NonConstPointer();
      int argb = c.ToArgb();
      UnsafeNativeMethods.ON_3dmRenderSettings_SetColor(ptr_this, which, argb);
      Commit();
    }

    /// <summary>
    /// Gets or sets the ambient light color used in rendering.
    /// </summary>
    public Rhino.Drawing.Color AmbientLight
    {
      get { return GetColor(idxAmbientLight); }
      set { SetColor(idxAmbientLight, value); }
    }

    /// <summary>
    /// Gets or sets the background top color used in rendering.
    /// <para>Sets also the background color if a solid background color is set.</para>
    /// </summary>
    public Rhino.Drawing.Color BackgroundColorTop
    {
      get { return GetColor(idxBackgroundColorTop); }
      set { SetColor(idxBackgroundColorTop, value); }
    }

    /// <summary>
    /// Gets or sets the background bottom color used in rendering.
    /// </summary>
    public Rhino.Drawing.Color BackgroundColorBottom
    {
      get { return GetColor(idxBackgroundColorBottom); }
      set { SetColor(idxBackgroundColorBottom, value); }
    }

    bool GetBool(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_3dmRenderSettings_GetBool(ptr_const_this, which);
    }
    void SetBool(int which, bool b)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_3dmRenderSettings_SetBool(ptr_this, which, b);
      Commit();
    }

    const int idxUseHiddenLights = 0;
    const int idxDepthCue = 1;
    const int idxFlatShade = 2;
    const int idxRenderBackFaces = 3;
    const int idxRenderPoints = 4;
    const int idxRenderCurves = 5;
    const int idxRenderIsoparams = 6;
    const int idxRenderMeshEdges = 7;
    const int idxRenderAnnotation = 8;
    const int idxUseViewportSize = 9;

    /// <summary>
    /// Gets or sets a value indicating whether to render using lights that are on layers that are off.
    /// </summary>
    public bool UseHiddenLights
    {
      get { return GetBool(idxUseHiddenLights); }
      set { SetBool(idxUseHiddenLights, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to render using depth cues.
    /// <para>These are clues to help the perception of position and orientation of objects in the image.</para>
    /// </summary>
    public bool DepthCue
    {
      get { return GetBool(idxDepthCue); }
      set { SetBool(idxDepthCue, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to render using flat shading.
    /// </summary>
    public bool FlatShade
    {
      get { return GetBool(idxFlatShade); }
      set { SetBool(idxFlatShade, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to render back faces.
    /// </summary>
    public bool RenderBackfaces
    {
      get { return GetBool(idxRenderBackFaces); }
      set { SetBool(idxRenderBackFaces, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to instruct the rendering engine to show points.
    /// </summary>
    public bool RenderPoints
    {
      get { return GetBool(idxRenderPoints); }
      set { SetBool(idxRenderPoints, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to instruct the rendering engine to show curves.
    /// </summary>
    public bool RenderCurves
    {
      get { return GetBool(idxRenderCurves); }
      set { SetBool(idxRenderCurves, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to instruct the rendering engine to show isocurves.
    /// </summary>
    public bool RenderIsoparams
    {
      get { return GetBool(idxRenderIsoparams); }
      set { SetBool(idxRenderIsoparams, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to instruct the rendering engine to show mesh edges.
    /// </summary>
    public bool RenderMeshEdges
    {
      get { return GetBool(idxRenderMeshEdges); }
      set { SetBool(idxRenderMeshEdges, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to instruct the rendering engine to show annotations,
    /// such as linear dimensions or angular dimensions.
    /// </summary>
    public bool RenderAnnotations
    {
      get { return GetBool(idxRenderAnnotation); }
      set { SetBool(idxRenderAnnotation, value); }
    }

    int GetInt(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_3dmRenderSettings_GetInt(ptr_const_this, which);
    }
    void SetInt(int which, int i)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_3dmRenderSettings_SetInt(ptr_this, which, i);
      Commit();
    }

    const int idxBackgroundStyle = 0;
    const int idxAntialiasStyle = 1;
    const int idxShadowmapStyle = 2;
    //const int idxShadowmapWidth = 3;
    //const int idxShadowmapHeight = 4;
    const int idxImageWidth = 5;
    const int idxImageHeight = 6;

    /// <summary>
    /// 0=none, 1=normal, 2=best.
    /// </summary>
    public int AntialiasLevel
    {
      get { return GetInt(idxAntialiasStyle); }
      set { SetInt(idxAntialiasStyle, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether to use the resolution of the
    /// viewport being rendered or ImageSize when rendering
    /// </summary>
    public bool UseViewportSize
    {
      get { return GetBool(idxUseViewportSize); }
      set { SetBool(idxUseViewportSize, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating the size of the rendering result if
    /// UseViewportSize is set to false.  If UseViewportSize is set to true,
    /// then this value is ignored.
    /// </summary>
    public Rhino.Drawing.Size ImageSize
    {
      get
      {
        int width = GetInt(idxImageWidth);
        int height = GetInt(idxImageHeight);
        return new Rhino.Drawing.Size(width, height);
      }
      set
      {
        SetInt(idxImageWidth, value.Width);
        SetInt(idxImageHeight, value.Height);
      }
    }

    /// <summary>
    /// 0=none, 1=normal, 2=best.
    /// </summary>
    public int ShadowmapLevel
    {
      get { return GetInt(idxShadowmapStyle); }
      set { SetInt(idxShadowmapStyle, value); }
    }

    /// <summary>
    /// How the viewport's backgroun should be filled.
    /// </summary>
    public Display.BackgroundStyle BackgroundStyle
    {
      get { return (Display.BackgroundStyle)GetInt(idxBackgroundStyle); }
      set { SetInt(idxBackgroundStyle, (int)value); }
    }

    //ON_wString m_background_bitmap_filename;
    //int m_shadowmap_width;
    //int m_shadowmap_height;
    //double m_shadowmap_offset;
    
  // Flags that are used to determine which render settings a render
  // plugin uses, and which ones the display pipeline should use.
  // Note: Render plugins set these, and they don't need to persist
  //       in the document...Also, when set, they turn OFF their
  //       corresponding setting in the Display Attributes Manager's
  //       UI pages for "Rendered" mode.
  //bool    m_bUsesAmbientAttr;
  //bool    m_bUsesBackgroundAttr;
  //bool    m_bUsesBackfaceAttr;
  //bool    m_bUsesPointsAttr;
  //bool    m_bUsesCurvesAttr;
  //bool    m_bUsesIsoparmsAttr;
  //bool    m_bUsesMeshEdgesAttr;
  //bool    m_bUsesAnnotationAttr;
  //bool    m_bUsesHiddenLightsAttr;
  }
}

namespace Rhino.Display
{
  /// <summary>
  /// Contains enumerated constants that define how the background of
  /// a viewport should be filled.
  /// </summary>
  public enum BackgroundStyle : int
  {
    /// <summary>Single solid color fill.</summary>
    SolidColor = 0,
    /// <summary>Simple image background wallpaper.</summary>
    WallpaperImage = 1,
    /// <summary>Two color top/bottom color gradient.</summary>
    Gradient = 2,
    /// <summary>Using a special environment.</summary>
    Environment = 3
  }

}

namespace Rhino.FileIO
{
  /// <summary>
  /// Contains settings used within the whole 3dm file.
  /// </summary>
  public class File3dmSettings
  {
    readonly File3dm m_parent;
    internal File3dmSettings(File3dm parent)
    {
      m_parent = parent;
    }

    IntPtr ConstPointer()
    {
      IntPtr ptr_const_parent = m_parent.ConstPointer();
      return UnsafeNativeMethods.ONX_Model_3dmSettingsPointer(ptr_const_parent);
    }
    IntPtr NonConstPointer()
    {
      IntPtr ptr_parent = m_parent.NonConstPointer();
      return UnsafeNativeMethods.ONX_Model_3dmSettingsPointer(ptr_parent);
    }

    /// <summary>
    /// Gets or sets a Uniform Resource Locator (URL) direction for the model.
    /// </summary>
    public string ModelUrl
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_3dmSettings_GetModelUrl(ptr_const_this, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_3dmSettings_SetModelUrl(ptr_this, value);
      }
    }

    /// <summary>
    /// Gets or sets the model basepoint that is used when the file is read as an instance definition.
    /// <para>This point is mapped to the origin in the instance definition.</para>
    /// </summary>
    public Point3d ModelBasepoint
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.ON_3dmSettings_GetModelBasepoint(ptr_const_this, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_3dmSettings_SetModelBasepoint(ptr_this, value);
      }
    }

    /*
    Rhino.DocObjects.EarthAnchorPoint m_earth_anchor;
    /// <summary>
    /// If set, this is the model's location on the earth.  This information is
    /// used when the model is used with GIS information.
    /// </summary>
    Rhino.DocObjects.EarthAnchorPoint EarthAnchorPoint
    {
      get
      {
        return m_earth_anchor ?? (m_earth_anchor = new DocObjects.EarthAnchorPoint(this));
      }
      set
      {
        if (m_earth_anchor == null)
          m_earth_anchor = new DocObjects.EarthAnchorPoint(this);
        m_earth_anchor.CopyFrom(value);
      }
    }
    */

    double GetDouble(int which)
    {
      IntPtr ptr_const_this = ConstPointer();
      return UnsafeNativeMethods.ON_3dmSettings_GetDouble(ptr_const_this, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_3dmSettings_SetDouble(ptr_this, which, val);
    }

    const int idxModelAbsTol = 0;
    const int idxModelAngleTol = 1;
    const int idxModelRelTol = 2;
    const int idxPageAbsTol = 3;
    const int idxPageAngleTol = 4;
    const int idxPageRelTol = 5;

    /// <summary>Gets or sets the model space absolute tolerance.</summary>
    public double ModelAbsoluteTolerance
    {
      get { return GetDouble(idxModelAbsTol); }
      set { SetDouble(idxModelAbsTol, value); }
    }
    /// <summary>Gets or sets the model space angle tolerance.</summary>
    public double ModelAngleToleranceRadians
    {
      get { return GetDouble(idxModelAngleTol); }
      set { SetDouble(idxModelAngleTol, value); }
    }
    /// <summary>Gets or sets the model space angle tolerance.</summary>
    public double ModelAngleToleranceDegrees
    {
      get
      {
        double rc = ModelAngleToleranceRadians;
        rc = RhinoMath.ToDegrees(rc);
        return rc;
      }
      set
      {
        double radians = RhinoMath.ToRadians(value);
        ModelAngleToleranceRadians = radians;
      }
    }
    /// <summary>Gets or sets the model space relative tolerance.</summary>
    public double ModelRelativeTolerance
    {
      get { return GetDouble(idxModelRelTol); }
      set { SetDouble(idxModelRelTol, value); }
    }
    /// <summary>Gets or sets the page space absolute tolerance.</summary>
    public double PageAbsoluteTolerance
    {
      get { return GetDouble(idxPageAbsTol); }
      set { SetDouble(idxPageRelTol, value); }
    }
    /// <summary>Gets or sets the page space angle tolerance.</summary>
    public double PageAngleToleranceRadians
    {
      get { return GetDouble(idxPageAngleTol); }
      set { SetDouble(idxPageAngleTol, value); }
    }
    /// <summary>Gets or sets the page space angle tolerance.</summary>
    public double PageAngleToleranceDegrees
    {
      get
      {
        double rc = PageAngleToleranceRadians;
        rc = RhinoMath.ToDegrees(rc);
        return rc;
      }
      set
      {
        double radians = RhinoMath.ToRadians(value);
        PageAngleToleranceRadians = radians;
      }
    }
    /// <summary>Gets or sets the page space relative tolerance.</summary>
    public double PageRelativeTolerance
    {
      get { return GetDouble(idxPageRelTol); }
      set { SetDouble(idxPageRelTol, value); }
    }

    /// <summary>
    /// Gets or sets the model unit system, using <see cref="Rhino.UnitSystem"/> enumeration.
    /// </summary>
    public UnitSystem ModelUnitSystem
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        int rc = UnsafeNativeMethods.ON_3dmSettings_GetSetUnitSystem(ptr_const_this, true, false, 0);
        return (UnitSystem)rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        int set_val = (int)value;
        UnsafeNativeMethods.ON_3dmSettings_GetSetUnitSystem(ptr_this, true, true, set_val);
      }
    }

    /// <summary>
    /// Gets or sets the page unit system, using <see cref="Rhino.UnitSystem"/> enumeration.
    /// </summary>
    public UnitSystem PageUnitSystem
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        int rc = UnsafeNativeMethods.ON_3dmSettings_GetSetUnitSystem(ptr_const_this, false, false, 0);
        return (UnitSystem)rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        int set_val = (int)value;
        UnsafeNativeMethods.ON_3dmSettings_GetSetUnitSystem(ptr_this, false, true, set_val);
      }
    }

    /*
  // settings used for automatically created rendering meshes
  ON_MeshParameters m_RenderMeshSettings;

  // saved custom settings
  ON_MeshParameters m_CustomRenderMeshSettings;

  // settings used for automatically created analysis meshes
  ON_MeshParameters m_AnalysisMeshSettings;

  // settings used when annotation objects are created
  ON_3dmAnnotationSettings m_AnnotationSettings;

  ON_ClassArray<ON_3dmConstructionPlane> m_named_cplanes;
  ON_ClassArray<ON_3dmView>              m_named_views;
  ON_ClassArray<ON_3dmView>              m_views; // current viewports
  ON_UUID m_active_view_id; // id of "active" viewport              

  // These fields determine what layer, material, color, line style, and
  // wire density are used for new objects.
  int m_current_layer_index;

  int m_current_material_index;
  ON::object_material_source m_current_material_source;
  
  ON_Color m_current_color;
  ON::object_color_source m_current_color_source;

  ON_Color m_current_plot_color;
  ON::plot_color_source m_current_plot_color_source;

  int m_current_linetype_index;
  ON::object_linetype_source m_current_linetype_source;

  int m_current_font_index;

  int m_current_dimstyle_index;
 
  // Surface wireframe density
  //
  //   @untitled table
  //   0       boundary + "knot" wires 
  //   1       boundary + "knot" wires + 1 interior wire if no interior "knots"
  //   N>=2    boundry + "knot" wires + (N-1) interior wires
  int m_current_wire_density;

  ON_3dmRenderSettings m_RenderSettings;

  // default settings for construction plane grids
  ON_3dmConstructionPlaneGridDefaults m_GridDefaults;

  // World scale factor to apply to non-solid linetypes
  // for model display.  For plotting, the linetype settings
  // are used without scaling.
  double m_linetype_display_scale;

  // Plugins that were loaded when the file was saved.
  ON_ClassArray<ON_PlugInRef> m_plugin_list;

  ON_3dmIOSettings m_IO_settings;
     */
  }
}