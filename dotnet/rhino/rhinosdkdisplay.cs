#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RHINO_SDK
namespace Rhino.Display
{
  /// <summary>
  /// Defines enmerated constants for display blend modes.
  /// </summary>
  public enum BlendMode : int
  {
    /// <summary>
    /// Blends using 0.
    /// </summary>
    Zero = 0,

    /// <summary>
    /// Blends using 1.
    /// </summary>
    One = 1,

    /// <summary>
    /// Blends using source color.
    /// </summary>
    SourceColor = 0x0300,

    /// <summary>
    /// Blends using 1-source color.
    /// </summary>
    OneMinusSourceColor = 0x0301,

    /// <summary>
    /// Blends using the source alpha channel.
    /// </summary>
    SourceAlpha = 0x0302,

    /// <summary>
    /// Blends using 1-the source alpha channel.
    /// </summary>
    OneMinusSourceAlpha = 0x0303,

    /// <summary>
    /// Blends using the destination alpha channel.
    /// </summary>
    DestinationAlpha = 0x0304,

    /// <summary>
    /// Blends using 1-the destination alpha channel.
    /// </summary>
    OneMinusDestinationAlpha = 0x0305,

    /// <summary>
    /// Blends using the destination color.
    /// </summary>
    DestinationColor = 0x0306,

    /// <summary>
    /// Blends using 1-the destination color.
    /// </summary>
    OneMinusDestinationColor = 0x0307,

    /// <summary>
    /// Blends using the source alpha saturation.
    /// </summary>
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
    /// Constructs a DisplayBitmap from an existing bitmap.
    /// </summary>
    /// <param name="bitmap">The original bitmap.</param>
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
    /// Load a DisplayBitmap from and image file on disk.
    /// </summary>
    /// <param name="path">A location from which to load the file.</param>
    /// <returns>The new display bitmap, or null on error.</returns>
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
    /// glBlendFunc for details.
    /// <para>http://www.opengl.org/sdk/docs/man/xhtml/glBlendFunc.xml</para>
    /// </summary>
    /// <param name="source">The source blend mode.</param>
    /// <param name="destination">The destination blend mode.</param>
    public void SetBlendFunction(BlendMode source, BlendMode destination)
    {
      UnsafeNativeMethods.CRhCmnDisplayBitmap_SetBlendFunction(m_pDisplayBmp, (int)source, (int)destination);
    }

    /// <summary>
    /// Gets the source and destination blend modes.
    /// </summary>
    /// <param name="source">The source blend mode is assigned to this out parameter.</param>
    /// <param name="destination">The destination blend mode is assigned to this out parameter.</param>
    public void GetBlendModes(out BlendMode source, out BlendMode destination)
    {
      int s = 0, d = 0;
      UnsafeNativeMethods.CRhCmnDisplayBitmap_GetBlendFunction(m_pDisplayBmp, ref s, ref d);
      source = (BlendMode)s;
      destination = (BlendMode)d;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~DisplayBitmap() { Dispose(false); }

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

    Rhino.Geometry.BoundingBox m_bbox = Rhino.Geometry.BoundingBox.Unset;
    public Rhino.Geometry.BoundingBox BoundingBox
    {
      get{ return m_bbox; }
    }

    /// <summary>
    /// Maximum number of cached sort order index lists stored on this class.
    /// Default is 10, but depending on the number of points in this list you
    /// may get better performance by setting this value to a certain percentage
    /// of the point count.
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

      var node = m_order.First;
      while(node!=null )
      {
        if( node.Value.Direction.IsParallelTo(cameraDirection, SortAngleTolerance) == 1 )
        {
          // move node to head to keep at top of MRU cache
          m_order.Remove(node);
          m_order.AddFirst(node);
          return node.Value.Indices;
        }
        d = node.Value;
        node = node.Next;
      }

      if (d == null || m_order.Count < MaximumCachedSortLists)
        d = new DirectedOrder(m_points.Length);

      m_camera_vector = cameraDirection;
      int[] indices = d.Indices;
      Array.Sort(indices, IndexComparison);
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
      m_bbox = new Geometry.BoundingBox(m_points);
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
      m_bbox = new Geometry.BoundingBox(m_points);
    }
  }
}

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a simple particle.
  /// <para>This base class only defines position and display properties (size, color, bitmap id).
  /// You will most likely create a class that derives from this particle class to perform some
  /// sort of physical simulation (movement over time or frames).
  /// </para>
  /// </summary>
  public class Particle
  {
    int m_index = -1;
    Point3d m_location;

    /// <summary>
    /// Initializes a new instance of the <see cref="Particle"/> class.
    /// </summary>
    public Particle()
    {
      Color = System.Drawing.Color.White;
    }

    /// <summary>
    /// Gets the parent particle system of this particle.
    /// </summary>
    public ParticleSystem ParentSystem { get; internal set; }

    /// <summary>
    /// Index in ParentSystem for this Particle. Can change when the particle
    /// system is modified.
    /// </summary>
    public int Index
    {
      get
      {
        if( ParentSystem==null )
          return -1;
        return m_index;
      }
      internal set
      {
        m_index = value;
      }
    }

    /// <summary>3d Location of the Particle.</summary>
    public Point3d Location
    {
      get { return m_location; }
      set
      {
        if( value != m_location )
        {
          if (ParentSystem != null)
            ParentSystem.UpdateParticleLocation(m_index, value);
          m_location = value;
        }
      }
    }

    public float Size { get; set; }

    public System.Drawing.Color Color { get; set; }

    public int DisplayBitmapIndex { get; set; }

    /// <summary>
    /// Base class implementation does nothing.
    /// </summary>
    public virtual void Update(){}
  }

  public class ParticleSystem : IEnumerable<Particle>
  {
    readonly List<Particle> m_particles = new List<Particle>();
    int m_empty_slot_count; // = 0 initialized by runtime
    BoundingBox m_bbox = BoundingBox.Unset;

    //cache data used for drawing
    internal Rhino.Geometry.Point3d[] m_points = new Point3d[0];
    internal int[] m_colors_argb = new int[0];
    internal float[] m_sizes = new float[0];
    internal int[] m_display_bitmap_ids = new int[0];

    public ParticleSystem()
    {
    }

    public bool DrawRequiresDepthSorting { get; set; }
    public bool DisplaySizesInWorldUnits { get; set; }


    public BoundingBox BoundingBox
    {
      get
      {
        if (!m_bbox.IsValid)
        {
          foreach (Particle p in this)
            m_bbox.Union(p.Location);
        }
        return m_bbox;
      }
    }

    /// <summary>
    /// Adds a particle to this ParticleSystem. A Particle can only be in one system
    /// at a time.  If the Particle already exists in a different system, this function
    /// will return false. You should remove the particle from the other system first
    /// before adding it.
    /// </summary>
    /// <param name="particle">A particle to be added.</param>
    /// <returns>
    /// true if this particle was added to the system or if is already in the system.
    /// false if the particle already exists in a different system.
    /// </returns>
    public virtual bool Add(Particle particle)
    {
      ParticleSystem existing_system = particle.ParentSystem;
      if( existing_system==this )
        return true; // already in system

      if (existing_system != null)
        return false;

      particle.Index = -1;
      if (m_empty_slot_count > 0)
      {
        for (int i = 0; i < m_particles.Count; i++)
        {
          if (m_particles[i] == null)
          {
            m_particles[i] = particle;
            particle.Index = i;
            if (m_points.Length == m_particles.Count)
              m_points[i] = particle.Location;
            m_empty_slot_count--;
            break;
          }
        }
      }
      if (particle.Index == -1)
      {
        m_particles.Add(particle);
        particle.Index = m_particles.Count - 1;
      }
      if (m_bbox.IsValid)
        m_bbox.Union(particle.Location);
      return true;
    }

    /// <summary>
    /// Removes a single particle from this system.
    /// </summary>
    /// <param name="particle">The particle to be removed.</param>
    public virtual void Remove(Particle particle)
    {
      int index = particle.Index;
      if (particle.ParentSystem == this && index >= 0 && index < m_particles.Count)
      {
        var particle_in_list = m_particles[index];
        if (particle == particle_in_list)
        {
          m_particles[index] = null; //don't remove as this will mess up other particle slots
          particle.Index = -1;
          particle.ParentSystem = null;
          m_empty_slot_count++;
          if( m_bbox.IsValid && !m_bbox.Contains(particle.Location, true) )
            m_bbox = BoundingBox.Unset;
        }
      }
    }

    /// <summary>
    /// Remove all Particles from this system.
    /// </summary>
    public virtual void Clear()
    {
      for (int i = 0; i < m_particles.Count; i++)
      {
        var particle = m_particles[i];
        if (particle != null)
        {
          particle.Index = -1;
          particle.ParentSystem = null;
        }
        m_particles.Clear();
        m_empty_slot_count = 0;
      }
      m_bbox = BoundingBox.Unset;
    }

    /// <summary>
    /// Calls Update on every particle in the system.
    /// </summary>
    public virtual void Update()
    {
      foreach (var particle in this)
      {
        if (particle == null)
          continue;
        particle.Update();
      }
    }

    internal void UpdateDrawCache()
    {
      if (m_points.Length != m_particles.Count || m_empty_slot_count > 0)
      {
        if (m_empty_slot_count > 0)
        {
          int count = m_particles.Count;
          for (int i = count-1; i>=0; i--)
          {
            if (m_particles[i] == null)
              m_particles.RemoveAt(i);
          }
          m_empty_slot_count = 0;
        }
        m_points = new Point3d[m_particles.Count];
        m_colors_argb = new int[m_particles.Count];
        m_sizes = new float[m_particles.Count];
        m_display_bitmap_ids = new int[m_particles.Count];
        for (int i = 0; i < m_particles.Count; i++)
        {
          Particle p = m_particles[i];
          m_points[i] = p.Location;
          m_colors_argb[i] = p.Color.ToArgb();
          m_sizes[i] = p.Size;
          m_display_bitmap_ids[i] = p.DisplayBitmapIndex;
        }
      }
    }

    internal void UpdateParticleLocation(int index, Point3d newLocation)
    {
      if (m_points.Length == m_particles.Count && index >= 0 && index < m_points.Length)
        m_points[index] = newLocation;
      ClearDepthSortCache();
      if (m_bbox.IsValid)
      {
        var particle = m_particles[index];
        if (particle != null)
        {
          if (m_bbox.Contains(particle.Location, true))
            m_bbox.Union(newLocation);
          else
            m_bbox = BoundingBox.Unset;
        }
      }
    }

    void ClearDepthSortCache()
    {
    }

    #region enumerable support
    public IEnumerator<Particle> GetEnumerator()
    {
      for( int i=0; i<m_particles.Count; i++ )
      {
        Particle p = m_particles[i];
        if (p != null)
          yield return p;
      }
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }
}
#endif