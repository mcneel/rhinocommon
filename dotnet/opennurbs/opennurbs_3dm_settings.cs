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
      IntPtr pName = UnsafeNativeMethods.ON_3dmConstructionPlane_Copy(pConstructionPlane,
                                                                      ref rc.m_plane,
                                                                      ref rc.m_grid_spacing,
                                                                      ref rc.m_snap_spacing,
                                                                      ref rc.m_grid_line_count,
                                                                      ref rc.m_grid_thick_frequency,
                                                                      ref rc.m_bDepthBuffered);
      if (IntPtr.Zero != pName)
      {
        rc.m_name = Marshal.PtrToStringUni(pName);
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
    private IntPtr m_ptr; // ON_3dmView*
    private bool m_bIsConst;

    internal ViewInfo(IntPtr ptr, bool isConst)
    {
      m_ptr = ptr;
      m_bIsConst = isConst;
    }

    //public ViewInfo()
    //{
    //  m_ptr = UnsafeNativeMethods.ON_3dmView_New();
    //  m_bIsConst = false;
    //}

    ~ViewInfo()
    {
      Dispose(false);
    }

    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }
    internal IntPtr NonConstPointer()
    {
      return m_ptr;
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (!m_bIsConst && IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_3dmView_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    public string Name
    {
      get
      {
        IntPtr ptr = ConstPointer();
        IntPtr pString = UnsafeNativeMethods.ON_3dmView_NameGet(ptr);
        if (pString == IntPtr.Zero)
          return null;
        return Marshal.PtrToStringUni(pString);
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_3dmView_NameSet(ptr, value);
      }
    }
  }

  //public class ON_3dmRenderSettings { }
  //public class ON_EarthAnchorPoint { }
  //public class ON_3dmIOSettings { }
  //public class ON_3dmSettings { }
}
