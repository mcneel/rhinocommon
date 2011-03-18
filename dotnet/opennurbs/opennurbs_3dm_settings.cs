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
      m_thin_line_color = ApplicationSettings.GridSettings.ThinLineColor;
      m_thick_line_color = ApplicationSettings.GridSettings.ThickLineColor;
      m_grid_x_color = ApplicationSettings.GridSettings.XAxisLineColor;
      m_grid_y_color = ApplicationSettings.GridSettings.YAxisLineColor;
      m_grid_z_color = ApplicationSettings.GridSettings.ZAxisLineColor;
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

      if (m_index >= 0)
      {
        Rhino.RhinoDoc doc = m_parent as Rhino.RhinoDoc;
        if (doc != null)
          return UnsafeNativeMethods.CRhinoDocProperties_GetNamedView(doc.m_docId, m_index);
      }
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

    internal EarthAnchorPoint(RhinoDoc doc)
    {
      m_ptr = UnsafeNativeMethods.CRhinoDocProperties_GetEarthAnchorPoint(doc.m_docId);
    }
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
