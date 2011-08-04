using System;
using Rhino.Geometry;
using System.Runtime.InteropServices;

// Most of these should not need to be wrapped. Some of their
// functionality is merged into other wrapper classes
namespace Rhino.DocObjects
{
  //public class ON_3dmUnitsAndTolerances{}
  //public class ON_3dmAnnotationSettings { }
  //public class ON_3dmConstructionPlaneGridDefaults { }
  public class ConstructionPlane
  {
    internal static ConstructionPlane FromIntPtr(IntPtr pConstructionPlane)
    {
      if (IntPtr.Zero == pConstructionPlane)
        return null;
      ConstructionPlane rc = new ConstructionPlane();
      using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ON_3dmConstructionPlane_Copy(pConstructionPlane,
                                                         ref rc.m_plane,
                                                         ref rc.m_grid_spacing,
                                                         ref rc.m_snap_spacing,
                                                         ref rc.m_grid_line_count,
                                                         ref rc.m_grid_thick_frequency,
                                                         ref rc.m_bDepthBuffered,
                                                         pString);
        rc.m_name = sh.ToString();
      }
      return rc;
    }

    internal IntPtr CopyToNative()
    {
      IntPtr pCPlane = UnsafeNativeMethods.ON_3dmConstructionPlane_New( ref m_plane,
                                                                        m_grid_spacing,
                                                                        m_snap_spacing,
                                                                        m_grid_line_count,
                                                                        m_grid_thick_frequency,
                                                                        m_bDepthBuffered,
                                                                        m_name);
      return pCPlane;
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
    System.Drawing.Color m_thin_line_color;
    System.Drawing.Color m_thick_line_color;
    System.Drawing.Color m_grid_x_color;
    System.Drawing.Color m_grid_y_color;
    System.Drawing.Color m_grid_z_color;

    #region ON_3dmConstructionPlane
    public ConstructionPlane()
    {
      m_plane = Plane.WorldXY;
      m_grid_spacing = 1.0;
      m_grid_line_count = 70;
      m_grid_thick_frequency = 5;
      m_bDepthBuffered = true;
      m_bShowGrid = true;
      m_bShowAxes = true;
      m_thin_line_color = ApplicationSettings.AppearanceSettings.GridThinLineColor;
      m_thick_line_color = ApplicationSettings.AppearanceSettings.GridThickLineColor;
      m_grid_x_color = ApplicationSettings.AppearanceSettings.GridXAxisLineColor;
      m_grid_y_color = ApplicationSettings.AppearanceSettings.GridYAxisLineColor;
      m_grid_z_color = ApplicationSettings.AppearanceSettings.GridZAxisLineColor;
    }

    public Plane Plane
    {
      get { return m_plane; }
      set { m_plane = value; }
    }

    /// <summary>distance between grid lines</summary>
    public double GridSpacing
    {
      get { return m_grid_spacing; }
      set { m_grid_spacing = value; }
    }

    /// <summary>number of grid lines in each direction</summary>
    public int GridLineCount
    {
      get { return m_grid_line_count; }
      set { m_grid_line_count = value; }
    }

    /// <summary>
    /// 0: none, 
    /// 1: all lines are thick, 
    /// 2: every other is thick, ...
    /// </summary>
    public int ThickLineFrequency
    {
      get { return m_grid_thick_frequency; }
      set { m_grid_thick_frequency = value; }
    }

    /// <summary>
    /// false=grid is always drawn behind 3d geometry
    /// true=grid is drawn at its depth as a 3d plane and grid lines obscure things behind the grid.
    /// </summary>
    public bool DepthBuffered
    {
      get { return m_bDepthBuffered; }
      set { m_bDepthBuffered = value; }
    }

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

    public bool ShowGrid
    {
      get { return m_bShowGrid; }
      set { m_bShowGrid = value; }
    }
    public bool ShowAxes
    {
      get { return m_bShowAxes; }
      set { m_bShowAxes = value; }
    }

    public System.Drawing.Color ThinLineColor
    {
      get { return m_thin_line_color; }
      set { m_thin_line_color = value; }
    }
    public System.Drawing.Color ThickLineColor
    {
      get { return m_thick_line_color; }
      set { m_thick_line_color = value; }
    }
    public System.Drawing.Color GridXColor
    {
      get { return m_grid_x_color; }
      set { m_grid_x_color = value; }
    }
    public System.Drawing.Color GridYColor
    {
      get { return m_grid_y_color; }
      set { m_grid_y_color = value; }
    }
    public System.Drawing.Color GridZColor
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
  
  public class ViewInfo : IDisposable // ON_3dmView
  {
    private object m_parent;
    private int m_index=-1;

    private IntPtr m_ptr; // ON_3dmView*

    internal ViewInfo(Rhino.RhinoDoc doc, int index)
    {
      m_parent = doc;
      m_index = index;
    }

    internal IntPtr ConstPointer()
    {
      if (m_ptr != IntPtr.Zero)
        return m_ptr;
#if !USING_OPENNURBS
      if (m_index >= 0)
      {
        Rhino.RhinoDoc doc = m_parent as Rhino.RhinoDoc;
        if (doc != null)
          return UnsafeNativeMethods.CRhinoDocProperties_GetNamedView(doc.m_docId, m_index);
      }
#endif
      throw new Rhino.Runtime.DocumentCollectedException();
    }

    internal IntPtr NonConstPointer()
    {
      if (m_ptr == IntPtr.Zero)
      {
        IntPtr pConstThis = ConstPointer();
        m_ptr = UnsafeNativeMethods.ON_3dmView_New(pConstThis);
        m_index = -1;
        m_parent = null;
      }
      return m_ptr;
    }

    ~ViewInfo() { Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_3dmView_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }

    public string Name
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_3dmView_NameGet(ptr, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_3dmView_NameSet(ptr, value);
      }
    }

    ViewportInfo m_viewport = null;
    public ViewportInfo Viewport
    {
      get
      {
        return m_viewport ?? new ViewportInfo(this);
      }
    }
    internal IntPtr ConstViewportPointer()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_3dmView_ViewportPointer(pConstThis);
    }
    internal IntPtr NonConstViewportPointer()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_3dmView_ViewportPointer(pThis);
    }
  }

  //public class ON_3dmRenderSettings { }

  /// <summary>
  /// Information about the model's position in latitude, longitude,
  /// and elevation for GIS mapping applications
  /// </summary>
  public class EarthAnchorPoint : IDisposable
  {
    IntPtr m_ptr = IntPtr.Zero;

    public EarthAnchorPoint()
    {
      m_ptr = UnsafeNativeMethods.ON_EarthAnchorPoint_New();
    }

#if !USING_OPENNURBS
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
    
    ~EarthAnchorPoint() { Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

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
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_EarthAnchorPoint_GetDouble(pConstThis, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_SetDouble(pThis, which, val);
    }

    /// <summary>
    /// Point on the earth in decimal degrees.
    /// +90 = north pole, 0 = equator, -90 = south pole
    /// </summary>
    public double EarthBasepointLatitude
    {
      get { return GetDouble(idxEarthBasepointLatitude); }
      set { SetDouble(idxEarthBasepointLatitude, value); }
    }

    /// <summary>
    /// Point on the earth in decimal degrees.
    /// 0 = prime meridian (Greenwich meridian)
    /// </summary>
    public double EarthBasepointLongitude
    {
      get { return GetDouble(idxEarthBasepointLongitude); }
      set { SetDouble(idxEarthBasepointLongitude, value); }
    }

    /// <summary>
    /// Point on the earth in meters
    /// </summary>
    public double EarthBasepointElevation
    {
      get { return GetDouble(idxEarthBasepointElevation); }
      set { SetDouble(idxEarthBasepointElevation, value); }
    }

    /// <summary>
    /// 0 = ground level, 1 = mean sea level, 2 = center of earth
    /// </summary>
    public int EarthBasepointElevationZero
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_EarthAnchorPoint_GetEarthBasepointElevationZero(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetEarthBasepointElevationZero(pThis, value);
      }
    }

    /// <summary>Corresponding model point in model coordinates</summary>
    public Point3d ModelBasePoint
    {
      get
      {
        Point3d rc = new Point3d(0,0,0);
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelBasePoint(pConstThis, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelBasePoint(pThis, true, ref value);
      }
    }

    /// <summary>Earth directions in model coordinates</summary>
    public Vector3d ModelNorth
    {
      get
      {
        Vector3d rc = new Vector3d(0, 0, 0);
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(pConstThis, true, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(pThis, true, true, ref value);
      }
    }

    /// <summary>Earth directions in model coordinates</summary>
    public Vector3d ModelEast
    {
      get
      {
        Vector3d rc = new Vector3d(0, 0, 0);
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(pConstThis, false, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_ModelDirection(pThis, false, true, ref value);
      }
    }

     // Identification information about this location
     //ON_UUID    m_id;           // unique id for this anchor point

    /// <summary>
    /// Indentifying information about this location
    /// </summary>
    public string Name
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_EarthAnchorPoint_GetString(pConstThis, true, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetString(pThis, true, value);
      }
    }

    /// <summary>
    /// Indentifying information about this location
    /// </summary>
    public string Description
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pConstThis = ConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_EarthAnchorPoint_GetString(pConstThis, false, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_EarthAnchorPoint_SetString(pThis, false, value);
      }
    }
    //ON_wString m_url;
    //ON_wString m_url_tag;      // UI link text for m_url


    /// <summary>
    /// Returns a plane in model coordinates whose xaxis points East,
    /// yaxis points North and zaxis points up.  The origin
    /// is set to ModelBasepoint. An Invalid plane is returned
    /// on error.
    /// </summary>
    /// <returns></returns>
    public Plane GetModelCompass()
    {
      Plane rc = Plane.Unset;
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_GetModelCompass(pConstThis, ref rc);
      return rc;
    }

    /// <summary>
    /// Get a transformation from model coordinates to earth coordinates.
    /// This transformation assumes the model is small enough that
    /// the curvature of the earth can be ignored.
    /// </summary>
    /// <param name="modelUnitSystem"></param>
    /// <returns>
    /// Transform on success. Inalid Transform on error
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
    /// elevation error &lt;= 8 centimeters
    /// </remarks>
    public Transform GetModelToEarthTransform(UnitSystem modelUnitSystem)
    {
      Transform rc = Transform.Unset;
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.ON_EarthAnchorPoint_GetModelToEarthTransform(pConstThis, (int)modelUnitSystem, ref rc);
      return rc;
    }
  }

  //public class ON_3dmIOSettings { }
  //public class ON_3dmSettings { }
}

namespace Rhino.FileIO
{
  public class File3dmSettings
  {
    File3dm m_parent;
    internal File3dmSettings(File3dm parent)
    {
      m_parent = parent;
    }

    IntPtr ConstPointer()
    {
      IntPtr pConstParent = m_parent.ConstPointer();
      return UnsafeNativeMethods.ONX_Model_3dmSettingsPointer(pConstParent);
    }
    IntPtr NonConstPointer()
    {
      IntPtr pParent = m_parent.NonConstPointer();
      return UnsafeNativeMethods.ONX_Model_3dmSettingsPointer(pParent);
    }

    public string ModelUrl
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_3dmSettings_GetModelUrl(pConstThis, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_3dmSettings_SetModelUrl(pThis, value);
      }
    }

    /// <summary>
    /// Model basepoint is used when the file is read as an instance definition
    /// and is the point that is mapped to the origin in the instance definition.
    /// </summary>
    public Point3d ModelBasepoint
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.ON_3dmSettings_GetModelBasepoint(pConstThis, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_3dmSettings_SetModelBasepoint(pThis, value);
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

    /*
  // Model space tolerances and unit system
  ON_3dmUnitsAndTolerances m_ModelUnitsAndTolerances;

  // Page space (printing/paper) tolerances and unit system
  ON_3dmUnitsAndTolerances m_PageUnitsAndTolerances;

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