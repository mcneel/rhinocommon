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
  //public class ON_EarthAnchorPoint { }
  //public class ON_3dmIOSettings { }
  //public class ON_3dmSettings { }
}
