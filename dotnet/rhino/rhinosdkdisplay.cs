#pragma warning disable 1591
using System;
using System.Collections.Generic;

namespace Rhino.Display
{
  public enum BlendMode : int
  {
    Zero = 0,
    One = 1,
    SourceColor = 0x0300,
    OneMinusSourceColor = 0x0301,
    SourceAlpha = 0x0302,
    OneMinusSourceAlpha = 0x0303,
    DestinationAlpha = 0x0304,
    OneMinusDestinationAlpha = 0x0305,
    DestinationColor = 0x0306,
    OneMinusDestinationColor = 0x0307,
    SourceAlphaSaturate = 0x0308
  }

  /// <summary>
  /// A bitmap resource that can be used by the display pipeline (currently only
  /// in OpenGL display).  Current limitation is that the bitmap should always
  /// have a witdh and height which are a power of 2 in value.  Reuse DisplayBitmaps
  /// for drawing if possible;  it is much more expensive to construct new DisplayBitmaps
  /// than it is to reuse existing DisplayBitmaps.
  /// </summary>
  public class DisplayBitmap : IDisposable
  {
    IntPtr m_pDisplayBmp;
    internal IntPtr NonConstPointer() { return m_pDisplayBmp; }

    /// <summary>
    /// Create a DisplayBitmap from an existing bitmap
    /// </summary>
    /// <param name="bitmap"></param>
    public DisplayBitmap(System.Drawing.Bitmap bitmap)
    {
      IntPtr hBitmap = bitmap.GetHbitmap();
      m_pDisplayBmp = UnsafeNativeMethods.CRhCmnDisplayBitmap_New(hBitmap);
    }

    private DisplayBitmap(IntPtr pBmp)
    {
      m_pDisplayBmp = pBmp;
    }

    /// <summary>
    /// Load a DisplayBitmap from and image file on disk
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static DisplayBitmap Load(string path)
    {
      IntPtr pBmp = UnsafeNativeMethods.CRhCmnDisplayBitmap_New2(path);
      if (IntPtr.Zero == pBmp)
        return null;
      return new DisplayBitmap(pBmp);
    }

    /// <summary>
    /// Sets blending function used to determine how this bitmap is blended
    /// with the current framebuffer color.  The default setting is SourceAlpha
    /// for source and OneMinusSourceAlpha for destination.  See OpenGL's
    /// glBlendFunc for details
    /// http://www.opengl.org/sdk/docs/man/xhtml/glBlendFunc.xml
    /// </summary>
    /// <param name="source"></param>
    /// <param name="destination"></param>
    public void SetBlendFunction( BlendMode source, BlendMode destination )
    {
      UnsafeNativeMethods.CRhCmnDisplayBitmap_SetBlendFunction(m_pDisplayBmp, (int)source, (int)destination);
    }

    public void GetBlendModes(out BlendMode source, out BlendMode destination)
    {
      int s = 0, d = 0;
      UnsafeNativeMethods.CRhCmnDisplayBitmap_GetBlendFunction(m_pDisplayBmp, ref s, ref d);
      source = (BlendMode)s;
      destination = (BlendMode)d;
    }

    ~DisplayBitmap() { Dispose(false); }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pDisplayBmp)
      {
        UnsafeNativeMethods.CRhCmnDisplayBitmap_Delete(m_pDisplayBmp);
        // This is probably where we want to store the HGLRC/GLuint
        // combinations or textures that need to be purged. We can
        // then perform the purging the next time a different bitmap
        // is drawn
      }
      m_pDisplayBmp = IntPtr.Zero;
    }

  }

  public class DisplayBitmapDrawList
  {
    internal Rhino.Geometry.Point3d[] m_points;
    internal int[] m_colors_argb;

    public DisplayBitmapDrawList()
    {
      MaximumCachedSortLists = 10;
      SortAngleTolerance = RhinoMath.ToRadians(5);
    }

    /// <summary>
    /// Maximum number of cached sort order index lists stored on this class.
    /// Default is 10, but depending on the number of points in this list you
    /// may get better performance by setting this value to a certain percentage
    /// of the point count
    /// </summary>
    public int MaximumCachedSortLists { get; set; }

    /// <summary>
    /// Angle in radians used to determine if an index list is "parallel enough"
    /// to a viewports camera angle. Default is 0.0873 radians (5 degrees)
    /// </summary>
    public double SortAngleTolerance { get; set; }

    
    class DirectedOrder
    {
      int[] m_indices;
      public DirectedOrder(int count)
      {
        m_indices = new int[count];
        for (int i = 0; i < count; i++)
          m_indices[i] = i;
      }
      public Rhino.Geometry.Vector3d Direction { get; set; }
      public int[] Indices
      {
        get { return m_indices; }
        set { m_indices = value; }
      }
    }
    LinkedList<DirectedOrder> m_order = new LinkedList<DirectedOrder>();

    Rhino.Geometry.Vector3d m_camera_vector;
    int IndexComparison(int a, int b)
    {
      double dist_a = m_points[a].m_x * m_camera_vector.m_x + m_points[a].m_y * m_camera_vector.m_y + m_points[a].m_z * m_camera_vector.m_z;
      double dist_b = m_points[b].m_x * m_camera_vector.m_x + m_points[b].m_y * m_camera_vector.m_y + m_points[b].m_z * m_camera_vector.m_z;
      if (dist_a < dist_b)
        return 1;
      if (dist_a > dist_b)
        return -1;
      return 0;
    }

    public int[] Sort(Rhino.Geometry.Vector3d cameraDirection)
    {
      DirectedOrder d = null;
      foreach (DirectedOrder order in m_order)
      {
        // allow for 5 degrees of slop in the parallel comparison
        if (order.Direction.IsParallelTo(cameraDirection, SortAngleTolerance) == 1)
          return order.Indices;
        d = order;
      }

      if (d == null || m_order.Count < MaximumCachedSortLists)
        d = new DirectedOrder(m_points.Length);

      m_camera_vector = cameraDirection;
      int[] indices = d.Indices;
      Array.Sort<int>(indices, IndexComparison);
      d.Direction = cameraDirection;
      m_order.AddFirst(d);
      if (m_order.Count > MaximumCachedSortLists)
        m_order.RemoveLast();
      return indices;
    }

    public void SetPoints(IEnumerable<Rhino.Geometry.Point3d> points)
    {
      SetPoints(points, System.Drawing.Color.White);
    }

    public void SetPoints(IEnumerable<Rhino.Geometry.Point3d> points, System.Drawing.Color blendColor)
    {
      m_order = new LinkedList<DirectedOrder>();
      List<Rhino.Geometry.Point3d> _points = new List<Geometry.Point3d>(points);
      m_points = _points.ToArray();
      m_colors_argb = new int[] { blendColor.ToArgb() };
    }

    public void SetPoints(IEnumerable<Rhino.Geometry.Point3d> points, IEnumerable<System.Drawing.Color> colors)
    {
      var _points = new List<Geometry.Point3d>(points);
      var _colors = new List<System.Drawing.Color>(colors);
      if (_points.Count != _colors.Count)
        throw new ArgumentException("length of points must be the same as length of colors");

      m_order = new LinkedList<DirectedOrder>();
      m_points = _points.ToArray();
      m_colors_argb = new int[_colors.Count];
      for (int i = 0; i < _colors.Count; i++)
        m_colors_argb[i] = _colors[i].ToArgb();
    }
  }
}
