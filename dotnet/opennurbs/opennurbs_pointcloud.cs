using System;
using Rhino.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a single item in a pointcloud. A PointCloud item 
  /// always has a location, but it has an optional normal vector and color.
  /// </summary>
  public class PointCloudItem
  {
    #region fields

    readonly PointCloud m_parent;
    readonly int m_index = -1;
    #endregion

    #region constructors
    internal PointCloudItem(PointCloud parent, int index)
    {
      m_parent = parent;
      m_index = index;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the location of this point cloud item.
    /// </summary>
    public Point3d Location
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = m_parent.ConstPointer();
        UnsafeNativeMethods.ON_PointCloud_GetPoint(ptr, m_index, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = m_parent.NonConstPointer();
        UnsafeNativeMethods.ON_PointCloud_SetPoint(ptr, m_index, value);
      }
    }
    /// <summary>
    /// Gets or sets the X component of this point cloud item location.
    /// </summary>
    public double X
    {
      get
      {
        return Location.X;
      }
      set
      {
        Point3d pt = Location;
        pt.X = value;
        Location = pt;
      }
    }
    /// <summary>
    /// Gets or sets the Y component of this point cloud item location.
    /// </summary>
    public double Y
    {
      get
      {
        return Location.Y;
      }
      set
      {
        Point3d pt = Location;
        pt.Y = value;
        Location = pt;
      }
    }
    /// <summary>
    /// Gets or sets the Z component of this point cloud item location.
    /// </summary>
    public double Z
    {
      get
      {
        return Location.Z;
      }
      set
      {
        Point3d pt = Location;
        pt.Z = value;
        Location = pt;
      }
    }

    /// <summary>
    /// Gets or sets the normal vector for this point cloud item.
    /// </summary>
    public Vector3d Normal
    {
      get
      {
        Vector3d rc = new Vector3d();
        IntPtr ptr = m_parent.ConstPointer();
        UnsafeNativeMethods.ON_PointCloud_GetNormal(ptr, m_index, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = m_parent.NonConstPointer();
        UnsafeNativeMethods.ON_PointCloud_SetNormal(ptr, m_index, value);
      }
    }
    /// <summary>
    /// Gets or sets the color of this point cloud item.
    /// </summary>
    public Color Color
    {
      get
      {
        int argb = 0;
        IntPtr ptr = m_parent.ConstPointer();
        UnsafeNativeMethods.ON_PointCloud_GetColor(ptr, m_index, ref argb);
        return Color.FromArgb(argb);
      }
      set
      {
        IntPtr ptr = m_parent.NonConstPointer();
        UnsafeNativeMethods.ON_PointCloud_SetColor(ptr, m_index, value.ToArgb());
      }
    }
    /// <summary>
    /// Gets or sets the hidden flag of this point cloud item.
    /// </summary>
    public bool Hidden
    {
      get
      {
        bool rc = false;
        IntPtr ptr = m_parent.ConstPointer();
        UnsafeNativeMethods.ON_PointCloud_GetHiddenFlag(ptr, m_index, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = m_parent.NonConstPointer();
        UnsafeNativeMethods.ON_PointCloud_SetHiddenFlag(ptr, m_index, value);
      }
    }

    /// <summary>
    /// Gets the index of this point cloud item.
    /// </summary>
    public int Index
    {
      get { return m_index; }
    }
    #endregion
  }

  /// <summary>
  /// Represents a collection of coordinates with optional normal vectors and colors.
  /// </summary>
  //[Serializable]
  public class PointCloud : GeometryBase, IEnumerable<PointCloudItem>
  {
    #region constructors
    internal PointCloud(IntPtr native_pointer, object parent)
      : base(native_pointer, parent, -1)
    {
      if (null == parent)
        ApplyMemoryPressure();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointCloud"/> class
    /// that is empty.
    /// </summary>
    public PointCloud()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PointCloud_New();
      ConstructNonConstObject(ptr);
      ApplyMemoryPressure();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointCloud"/> class,
    /// copying (Merge) the content of another pointcloud.
    /// </summary>
    public PointCloud(PointCloud other)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PointCloud_New();
      ConstructNonConstObject(ptr);

      if (other != null)
        Merge(other);

      ApplyMemoryPressure();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PointCloud"/> class,
    /// copying the content from a set of points.
    /// </summary>
    /// <param name="points">A list or an array of Point3d, or any object that implements <see cref="IEnumerable{Point3d}"/>.</param>
    public PointCloud(IEnumerable<Point3d> points)
    {
      int count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      IntPtr ptr;
      if (null == ptArray || count < 1)
      {
        ptr = UnsafeNativeMethods.ON_PointCloud_New();
      }
      else
      {
        ptr = UnsafeNativeMethods.ON_PointCloud_New1(count, ptArray);
      }
      ConstructNonConstObject(ptr);
      ApplyMemoryPressure();
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new PointCloud(IntPtr.Zero, null);
    }

    //// serialization constructor
    ///// <summary>
    ///// Binds with the Rhino default serializer to support object persistence.
    ///// </summary>
    ///// <param name="info">Some storage.</param>
    ///// <param name="context">The source and destination of the stream.</param>
    //protected PointCloud(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}
    #endregion

    const int idx_PointCount = 0;
    const int idx_NormalCount = 1;
    const int idx_ColorCount = 2;
    //const int idx_HiddenCount = 3;
    const int idx_HiddenPointCount = 4;

    #region properties
    /// <summary>
    /// Gets the number of points in this pointcloud.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_PointCloud_GetInt(pConstThis, idx_PointCount);
      }
    }
    /// <summary>
    /// Gets the item at the given index.
    /// </summary>
    /// <param name="index">Index of item to retrieve.</param>
    /// <returns>The item at the given index.</returns>
    public PointCloudItem this[int index]
    {
      get
      {
        if (index < 0) { throw new IndexOutOfRangeException("index must be larger than or equal to zero"); }
        if (index >= Count) { throw new IndexOutOfRangeException("index must be smaller than the Number of points in the PointCloud"); }
        return new PointCloudItem(this, index);
      }
    }

    /// <summary>
    /// Gets the number of points that have their Hidden flag set.
    /// </summary>
    public int HiddenPointCount
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_PointCloud_GetInt(pConstThis, idx_HiddenPointCount);
      }
    }

    const int idx_Colors = 0;
    const int idx_Normals = 1;
    const int idx_Hidden = 2;

    /// <summary>
    /// Gets a value indicating whether or not the points in this 
    /// pointcloud have colors assigned to them.
    /// </summary>
    public bool ContainsColors
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_PointCloud_GetBool(pConstThis, idx_Colors);
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not the points in this 
    /// pointcloud have normals assigned to them.
    /// </summary>
    public bool ContainsNormals
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_PointCloud_GetBool(pConstThis, idx_Normals);
      }
    }
    /// <summary>
    /// Gets a value indicating whether or not the points in this 
    /// pointcloud have hidden flags assigned to them.
    /// </summary>
    public bool ContainsHiddenFlags
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_PointCloud_GetBool(pConstThis, idx_Hidden);
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Destroys the color information in this point cloud.
    /// </summary>
    public void ClearColors()
    {
      if (!ContainsColors)
        return;

      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_DestroyArray(pThis, idx_Colors);
    }
    /// <summary>
    /// Destroys the normal vector information in this point cloud.
    /// </summary>
    public void ClearNormals()
    {
      if (!ContainsNormals)
        return;

      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_DestroyArray(pThis, idx_Normals);
    }
    /// <summary>
    /// Destroys the hidden flag information in this point cloud.
    /// </summary>
    public void ClearHiddenFlags()
    {
      if (!ContainsHiddenFlags)
        return;

      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_DestroyArray(pThis, idx_Hidden);
    }

    /// <summary>
    /// Appends a new PointCloudItem to the end of this point cloud.
    /// </summary>
    /// <returns>The newly appended item.</returns>
    public PointCloudItem AppendNew()
    {
      Add(Point3d.Origin);
      return this[Count - 1];
    }
    /// <summary>
    /// Inserts a new <see cref="PointCloudItem"/> at a specific position of the point cloud.
    /// </summary>
    /// <param name="index">Index of new item.</param>
    /// <returns>The newly inserted item.</returns>
    public PointCloudItem InsertNew(int index)
    {
      Insert(index, Point3d.Origin);
      return this[index];
    }

    /// <summary>
    /// Copies the point values of another pointcloud into this one.
    /// </summary>
    /// <param name="other">PointCloud to merge with this one.</param>
    public void Merge(PointCloud other)
    {
      IntPtr ptr_other = other.ConstPointer();
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_MergeCloud(ptr, ptr_other);
    }

    /// <summary>
    /// Append a new point to the end of the list.
    /// </summary>
    /// <param name="point">Point to append.</param>
    public void Add(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_AppendPoint1(ptr, point);
    }
    /// <summary>
    /// Append a new point to the end of the list.
    /// </summary>
    /// <param name="point">Point to append.</param>
    /// <param name="normal">Normal vector of new point.</param>
    public void Add(Point3d point, Vector3d normal)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_AppendPoint3(ptr, point, normal);
    }
    /// <summary>
    /// Append a new point to the end of the list.
    /// </summary>
    /// <param name="point">Point to append.</param>
    /// <param name="color">Color of new point.</param>
    public void Add(Point3d point, Color color)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_AppendPoint2(ptr, point, color.ToArgb());
    }
    /// <summary>
    /// Append a new point to the end of the list.
    /// </summary>
    /// <param name="point">Point to append.</param>
    /// <param name="normal">Normal vector of new point.</param>
    /// <param name="color">Color of new point.</param>
    public void Add(Point3d point, Vector3d normal, Color color)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_AppendPoint4(ptr, point, color.ToArgb(), normal);
    }
    /// <summary>
    /// Append a collection of points to this point cloud.
    /// </summary>
    /// <param name="points">Points to append.</param>
    public void AddRange(IEnumerable<Point3d> points)
    {
      int count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (ptArray == null) { return; }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_AppendPoints(ptr, count, ptArray);
    }

    /// <summary>
    /// Inserts a new point into the point list.
    /// </summary>
    /// <param name="index">Insertion index.</param>
    /// <param name="point">Point to append.</param>
    public void Insert(int index, Point3d point)
    {
      if (index < 0) { throw new IndexOutOfRangeException("index must be equal to or larger than zero"); }
      if (index > Count) { throw new IndexOutOfRangeException("index must be equal to or smaller than Count"); }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_InsertPoint1(ptr, index, point);
    }
    /// <summary>
    /// Inserts a new point into the point list.
    /// </summary>
    /// <param name="index">Insertion index.</param>
    /// <param name="point">Point to append.</param>
    /// <param name="normal">Normal vector of new point.</param>
    public void Insert(int index, Point3d point, Vector3d normal)
    {
      if (index < 0) { throw new IndexOutOfRangeException("index must be equal to or larger than zero"); }
      if (index > Count) { throw new IndexOutOfRangeException("index must be equal to or smaller than Count"); }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_InsertPoint3(ptr, index, point, normal);
    }
    /// <summary>
    /// Inserts a new point into the point list.
    /// </summary>
    /// <param name="index">Insertion index.</param>
    /// <param name="point">Point to append.</param>
    /// <param name="color">Color of new point.</param>
    public void Insert(int index, Point3d point, Color color)
    {
      if (index < 0) { throw new IndexOutOfRangeException("index must be equal to or larger than zero"); }
      if (index > Count) { throw new IndexOutOfRangeException("index must be equal to or smaller than Count"); }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_InsertPoint2(ptr, index, point, color.ToArgb());
    }
    /// <summary>
    /// Inserts a new point into the point list.
    /// </summary>
    /// <param name="index">Insertion index.</param>
    /// <param name="point">Point to append.</param>
    /// <param name="normal">Normal vector of new point.</param>
    /// <param name="color">Color of new point.</param>
    public void Insert(int index, Point3d point, Vector3d normal, Color color)
    {
      if (index < 0) { throw new IndexOutOfRangeException("index must be equal to or larger than zero"); }
      if (index > Count) { throw new IndexOutOfRangeException("index must be equal to or smaller than Count"); }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_InsertPoint4(ptr, index, point, color.ToArgb(), normal);
    }
    /// <summary>
    /// Append a collection of points to this point cloud.
    /// </summary>
    /// <param name="index">Index at which to insert the new collection.</param>
    /// <param name="points">Points to append.</param>
    public void InsertRange(int index, IEnumerable<Point3d> points)
    {
      if (index < 0) { throw new IndexOutOfRangeException("Index must be larger than or equal to zero"); }
      if (index > Count) { throw new IndexOutOfRangeException("Index must be smaller than or equal to Count"); }

      int count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (ptArray == null) { return; }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_InsertPoints(ptr, index, count, ptArray);
    }

    /// <summary>
    /// Remove the point at the given index.
    /// </summary>
    /// <param name="index">Index of point to remove.</param>
    public void RemoveAt(int index)
    {
      if (index < 0) { throw new IndexOutOfRangeException("index must be equal to or larger than zero"); }
      if (index >= Count) { throw new IndexOutOfRangeException("index must be smaller than Count"); }

      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PointCloud_RemovePoint(ptr, index);
    }

    /// <summary>
    /// Copy all the point coordinates in this point cloud to an array.
    /// </summary>
    /// <returns>An array containing all the points in this point cloud.</returns>
    public Point3d[] GetPoints()
    {
      IntPtr pConstThis = ConstPointer();
      int count = UnsafeNativeMethods.ON_PointCloud_GetInt(pConstThis, idx_PointCount);
      if (count < 1)
        return new Point3d[0];

      Point3d[] rc = new Point3d[count];
      UnsafeNativeMethods.ON_PointCloud_GetPoints(pConstThis, count, rc);
      return rc;
    }
    /// <summary>
    /// Copy all the normal vectors in this point cloud to an array.
    /// </summary>
    /// <returns>An array containing all the normals in this point cloud.</returns>
    public Vector3d[] GetNormals()
    {
      IntPtr pConstThis = ConstPointer();
      int count = UnsafeNativeMethods.ON_PointCloud_GetInt(pConstThis, idx_NormalCount);
      if (count < 1)
        return new Vector3d[0];

      Vector3d[] rc = new Vector3d[count];
      UnsafeNativeMethods.ON_PointCloud_GetNormals(pConstThis, count, rc);
      return rc;
    }
    /// <summary>
    /// Copy all the point colors in this point cloud to an array.
    /// </summary>
    /// <returns>An array containing all the colors in this point cloud.</returns>
    public Color[] GetColors()
    {
      IntPtr pConstThis = ConstPointer();
      int count = UnsafeNativeMethods.ON_PointCloud_GetInt(pConstThis, idx_ColorCount);
      if (count < 1)
        return new Color[0];

      int[] rc = new int[count];
      UnsafeNativeMethods.ON_PointCloud_GetColors(pConstThis, count, rc);

      Color[] res = new Color[count];
      for (int i = 0; i < count; i++)
        res[i] = Color.FromArgb(rc[i]);
      return res;
    }

#if RHINO_SDK
    /// <summary>
    /// Returns index of the closest point in the point cloud to a given test point.
    /// </summary>
    /// <param name="testPoint">.</param>
    /// <returns>Index of point in the point cloud on success. -1 on failure.</returns>
    public int ClosestPoint(Point3d testPoint)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_PointCloud_GetClosestPoint(pConstThis, testPoint);
    }
#endif
    #endregion

    #region IEnumerable<PointCloudItem> Members

    /// <summary>
    /// Gets an enumerator that allows to modify each pointcloud point.
    /// </summary>
    /// <returns>A instance of <see cref="IEnumerator{PointCloudItem}"/>.</returns>
    public IEnumerator<PointCloudItem> GetEnumerator()
    {
      return new PointCloudItemEnumerator(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    private class PointCloudItemEnumerator : IEnumerator<PointCloudItem>
    {
      #region members
      private readonly PointCloud m_owner;
      int position = -1;
      #endregion

      #region constructor
      public PointCloudItemEnumerator(PointCloud cloud_points)
      {
        m_owner = cloud_points;
      }
      #endregion

      #region enumeration logic
      public bool MoveNext()
      {
        position++;
        return (position < m_owner.Count);
      }
      public void Reset()
      {
        position = -1;
      }

      public PointCloudItem Current
      {
        get
        {
          try
          {
            return m_owner[position];
          }
          catch (IndexOutOfRangeException)
          {
            throw new InvalidOperationException();
          }
        }
      }
      object IEnumerator.Current
      {
        get
        {
          try
          {
            return m_owner[position];
          }
          catch (IndexOutOfRangeException)
          {
            throw new InvalidOperationException();
          }
        }
      }
      #endregion

      #region IDisposable logic
      private bool m_disposed; // = false; <- initialized by runtime
      public void Dispose()
      {
        if (m_disposed) { return; }
        m_disposed = true;
        GC.SuppressFinalize(this);
      }
      #endregion
    }
    #endregion
  }
}

//namespace Rhino.Geometry.Collections
//{
  /////// <summary>
  /////// Provides access to the Point coordinates in a PointCloud.
  /////// </summary>
  ////public class PointCloudItemList : IEnumerable<PointCloudItem>
  ////{
  ////  #region fields
  ////  private PointCloud m_cloud;
  ////  #endregion

  ////  #region constructors
  ////  internal PointCloudItemList(PointCloud ownerCloud)
  ////  {
  ////    m_cloud = ownerCloud;
  ////  }
  ////  #endregion

  ////  #region properties
  ////  /// <summary>
  ////  /// Gets or sets the number of PointCloud points.
  ////  /// </summary>
  ////  public int Count
  ////  {
  ////    get
  ////    {
  ////      IntPtr ptr = m_cloud.ConstPointer();
  ////      return UnsafeNativeMethods.ON_PointCloud_PointCount(ptr);
  ////    }
  ////    set
  ////    {
  ////      if ((value >= 0) && (value != Count))
  ////      {
  ////        IntPtr ptr = m_cloud.NonConstPointer();
  ////        UnsafeNativeMethods.ON_PointCloud_SetPointCount(ptr, value);
  ////      }
  ////    }
  ////  }

  ////  /// <summary>
  ////  /// Gets or sets the point at the given index. 
  ////  /// The index must be valid or an IndexOutOfRangeException will be thrown.
  ////  /// </summary>
  ////  /// <param name="index">Index of point to access.</param>
  ////  /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
  ////  /// <returns>The point at [index].</returns>
  ////  public Point3d this[int index]
  ////  {
  ////    get
  ////    {
  ////      if (index < 0 || index >= Count)
  ////      {
  ////        throw new IndexOutOfRangeException();
  ////      }

  ////      Point3d rc = new Point3d();
  ////      IntPtr ptr = m_cloud.ConstPointer();
  ////      UnsafeNativeMethods.ON_PointCloud_GetPoint(ptr, index, ref rc);
  ////      return rc;
  ////    }
  ////    set
  ////    {
  ////      if (index < 0 || index >= Count)
  ////      {
  ////        throw new IndexOutOfRangeException();
  ////      }

  ////      IntPtr ptr = m_cloud.NonConstPointer();
  ////      UnsafeNativeMethods.ON_PointCloud_SetPoint(ptr, index, ref value);
  ////    }
  ////  }
  ////  #endregion

  ////  #region methods
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  public void Add(Point3d point)
  ////  {
  ////    IntPtr ptr = m_cloud.NonConstPointer();
  ////    UnsafeNativeMethods.ON_PointCloud_AppendPoint(ptr, ref point);
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  public void Add(Point3f point)
  ////  {
  ////    Add(new Point3d(point));
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="color">Color of new point.</param>
  ////  public void Add(Point3d point, Color color)
  ////  {
  ////    IntPtr ptr = m_cloud.NonConstPointer();
  ////    UnsafeNativeMethods.ON_PointCloud_AppendPoint2(ptr, ref point, color.ToArgb());
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="color">Color of new point.</param>
  ////  public void Add(Point3f point, Color color)
  ////  {
  ////    Add(new Point3d(point), color);
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="normal">Normal vector of new point.</param>
  ////  public void Add(Point3d point, Vector3d normal)
  ////  {
  ////    IntPtr ptr = m_cloud.NonConstPointer();
  ////    UnsafeNativeMethods.ON_PointCloud_AppendPoint3(ptr, ref point, ref normal);
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="normal">Normal vector of new point.</param>
  ////  public void Add(Point3f point, Vector3f normal)
  ////  {
  ////    Add(new Point3d(point), new Vector3d(normal));
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="color">Color of new point.</param>
  ////  /// <param name="normal">Normal vector of new point.</param>
  ////  public void Add(Point3d point, Color color, Vector3d normal)
  ////  {
  ////    IntPtr ptr = m_cloud.NonConstPointer();
  ////    UnsafeNativeMethods.ON_PointCloud_AppendPoint4(ptr, ref point, color.ToArgb(), ref normal);
  ////  }
  ////  /// <summary>
  ////  /// Adds a new point to this point cloud.
  ////  /// </summary>
  ////  /// <param name="point">Point to add.</param>
  ////  /// <param name="color">Color of new point.</param>
  ////  /// <param name="normal">Normal vector of new point.</param>
  ////  public void Add(Point3f point, Color color, Vector3f normal)
  ////  {
  ////    Add(new Point3d(point), color, new Vector3d(normal));
  ////  }
  ////  #endregion
  ////}

  ///// <summary>
  ///// Provides access to the Point coordinates in a PointCloud.
  ///// </summary>
  //public class PointCloudEnumerator : IEnumerable<Point3d>
  //{
  //  #region fields
  //  private PointCloud m_cloud;
  //  #endregion

  //  #region constructors
  //  internal PointCloudPointList(PointCloud ownerCloud)
  //  {
  //    m_cloud = ownerCloud;
  //  }
  //  #endregion

  //  #region properties
  //  /// <summary>
  //  /// Gets or sets the number of PointCloud points.
  //  /// </summary>
  //  public int Count
  //  {
  //    get
  //    {
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      return UnsafeNativeMethods.ON_PointCloud_PointCount(ptr);
  //    }
  //    set
  //    {
  //      if ((value >= 0) && (value != Count))
  //      {
  //        IntPtr ptr = m_cloud.NonConstPointer();
  //        UnsafeNativeMethods.ON_PointCloud_SetPointCount(ptr, value);
  //      }
  //    }
  //  }

  //  /// <summary>
  //  /// Gets or sets the point at the given index. 
  //  /// The index must be valid or an IndexOutOfRangeException will be thrown.
  //  /// </summary>
  //  /// <param name="index">Index of point to access.</param>
  //  /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
  //  /// <returns>The point at [index].</returns>
  //  public Point3d this[int index]
  //  {
  //    get
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      Point3d rc = new Point3d();
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_GetPoint(ptr, index, ref rc);
  //      return rc;
  //    }
  //    set
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      IntPtr ptr = m_cloud.NonConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_SetPoint(ptr, index, ref value);
  //    }
  //  }
  //  #endregion

  //  #region methods
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  public void Add(Point3d point)
  //  {
  //    IntPtr ptr = m_cloud.NonConstPointer();
  //    UnsafeNativeMethods.ON_PointCloud_AppendPoint(ptr, ref point);
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  public void Add(Point3f point)
  //  {
  //    Add(new Point3d(point));
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="color">Color of new point.</param>
  //  public void Add(Point3d point, Color color)
  //  {
  //    IntPtr ptr = m_cloud.NonConstPointer();
  //    UnsafeNativeMethods.ON_PointCloud_AppendPoint2(ptr, ref point, color.ToArgb());
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="color">Color of new point.</param>
  //  public void Add(Point3f point, Color color)
  //  {
  //    Add(new Point3d(point), color);
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="normal">Normal vector of new point.</param>
  //  public void Add(Point3d point, Vector3d normal)
  //  {
  //    IntPtr ptr = m_cloud.NonConstPointer();
  //    UnsafeNativeMethods.ON_PointCloud_AppendPoint3(ptr, ref point, ref normal);
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="normal">Normal vector of new point.</param>
  //  public void Add(Point3f point, Vector3f normal)
  //  {
  //    Add(new Point3d(point), new Vector3d(normal));
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="color">Color of new point.</param>
  //  /// <param name="normal">Normal vector of new point.</param>
  //  public void Add(Point3d point, Color color, Vector3d normal)
  //  {
  //    IntPtr ptr = m_cloud.NonConstPointer();
  //    UnsafeNativeMethods.ON_PointCloud_AppendPoint4(ptr, ref point, color.ToArgb(), ref normal);
  //  }
  //  /// <summary>
  //  /// Adds a new point to this point cloud.
  //  /// </summary>
  //  /// <param name="point">Point to add.</param>
  //  /// <param name="color">Color of new point.</param>
  //  /// <param name="normal">Normal vector of new point.</param>
  //  public void Add(Point3f point, Color color, Vector3f normal)
  //  {
  //    Add(new Point3d(point), color, new Vector3d(normal));
  //  }
  //  #endregion

  //  #region IEnumerable<Point3d> Members
  //  public IEnumerator<Point3d> GetEnumerator()
  //  {
  //    return new PPEnum(this);
  //  }
  //  IEnumerator IEnumerable.GetEnumerator()
  //  {
  //    return GetEnumerator();
  //  }
  //  private class PPEnum : IEnumerator<Point3d>
  //  {
  //    #region members
  //    private PointCloudPointList m_owner;
  //    int position = -1;
  //    #endregion

  //    #region constructor
  //    public PPEnum(PointCloudPointList cloud_points)
  //    {
  //      m_owner = cloud_points;
  //    }
  //    #endregion

  //    #region enumeration logic
  //    public bool MoveNext()
  //    {
  //      position++;
  //      return (position < m_owner.Count);
  //    }
  //    public void Reset()
  //    {
  //      position = -1;
  //    }

  //    public Point3d Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    object IEnumerator.Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    #endregion

  //    #region IDisposable logic
  //    private bool m_disposed; // = false; <- initialized by runtime
  //    public void Dispose()
  //    {
  //      if (m_disposed) { return; }
  //      m_disposed = true;
  //      GC.SuppressFinalize(this);
  //    }
  //    #endregion
  //  }
  //  #endregion
  //}

  ///// <summary>
  ///// Provides access to the Normal vectors in a PointCloud.
  ///// </summary>
  //public class PointCloudNormalList : IEnumerable<Vector3d>
  //{
  //  #region fields
  //  private PointCloud m_cloud;
  //  #endregion

  //  #region constructors
  //  internal PointCloudNormalList(PointCloud ownerCloud)
  //  {
  //    m_cloud = ownerCloud;
  //  }
  //  #endregion

  //  #region properties
  //  /// <summary>
  //  /// Gets or sets the number of PointCloud normals.
  //  /// </summary>
  //  public int Count
  //  {
  //    get
  //    {
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      return UnsafeNativeMethods.ON_PointCloud_NormalCount(ptr);
  //    }
  //    set
  //    {
  //      if (value >= 0 && value != Count)
  //      {
  //        IntPtr ptr = m_cloud.NonConstPointer();
  //        UnsafeNativeMethods.ON_PointCloud_SetNormalCount(ptr, value);
  //      }
  //    }
  //  }

  //  /// <summary>
  //  /// Gets a value indicating whether or not Normals have been defined for this cloud. 
  //  /// In order for normals to truly 'exist', the Count must be equal to the number of 
  //  /// points in the cloud.
  //  /// </summary>
  //  public bool Exists
  //  {
  //    get
  //    {
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      return UnsafeNativeMethods.ON_PointCloud_HasNormals(ptr);
  //    }
  //  }

  //  /// <summary>
  //  /// Gets or sets the normal at the given index. 
  //  /// The index must be valid or an IndexOutOfRangeException will be thrown.
  //  /// </summary>
  //  /// <param name="index">Index of normal to access.</param>
  //  /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
  //  /// <returns>The normal at [index].</returns>
  //  public Vector3d this[int index]
  //  {
  //    get
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      Vector3d rc = new Vector3d();
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_GetNormal(ptr, index, ref rc);
  //      return rc;
  //    }
  //    set
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      IntPtr ptr = m_cloud.NonConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_SetNormal(ptr, index, ref value);
  //    }
  //  }
  //  #endregion

  //  #region IEnumerable<Vector3d> Members
  //  public IEnumerator<Vector3d> GetEnumerator()
  //  {
  //    return new PNEnum(this);
  //  }
  //  IEnumerator IEnumerable.GetEnumerator()
  //  {
  //    return GetEnumerator();
  //  }
  //  private class PNEnum : IEnumerator<Vector3d>
  //  {
  //    #region members
  //    private PointCloudNormalList m_owner;
  //    int position = -1;
  //    #endregion

  //    #region constructor
  //    public PNEnum(PointCloudNormalList cloud_normals)
  //    {
  //      m_owner = cloud_normals;
  //    }
  //    #endregion

  //    #region enumeration logic
  //    public bool MoveNext()
  //    {
  //      position++;
  //      return (position < m_owner.Count);
  //    }
  //    public void Reset()
  //    {
  //      position = -1;
  //    }

  //    public Vector3d Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    object IEnumerator.Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    #endregion

  //    #region IDisposable logic
  //    private bool m_disposed; // = false; <- initialized by runtime
  //    public void Dispose()
  //    {
  //      if (m_disposed) { return; }
  //      m_disposed = true;
  //      GC.SuppressFinalize(this);
  //    }
  //    #endregion
  //  }
  //  #endregion
  //}

  ///// <summary>
  ///// Provides access to the colors in a point cloud.
  ///// </summary>
  //public class PointCloudColorList : IEnumerable<Color>
  //{
  //  #region fields
  //  private PointCloud m_cloud;
  //  #endregion

  //  #region constructors
  //  internal PointCloudColorList(PointCloud ownerCloud)
  //  {
  //    m_cloud = ownerCloud;
  //  }
  //  #endregion

  //  #region properties
  //  /// <summary>
  //  /// Gets or sets the number of PointCloud colors.
  //  /// </summary>
  //  public int Count
  //  {
  //    get
  //    {
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      return UnsafeNativeMethods.ON_PointCloud_ColorCount(ptr);
  //    }
  //    set
  //    {
  //      if (value >= 0 && value != Count)
  //      {
  //        IntPtr ptr = m_cloud.NonConstPointer();
  //        UnsafeNativeMethods.ON_PointCloud_SetColorCount(ptr, value);
  //      }
  //    }
  //  }

  //  /// <summary>
  //  /// Gets a value indicating whether colors have been defined for this cloud. 
  //  /// In order for colors to truly 'exist', the Count must be equal to the number of 
  //  /// points in the cloud.
  //  /// </summary>
  //  public bool Exists
  //  {
  //    get
  //    {
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      return UnsafeNativeMethods.ON_PointCloud_HasColors(ptr);
  //    }
  //  }

  //  /// <summary>
  //  /// Gets or sets the color at the given index. 
  //  /// The index must be valid or an IndexOutOfRangeException will be thrown.
  //  /// </summary>
  //  /// <param name="index">Index of color to access.</param>
  //  /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
  //  /// <returns>The color at [index].</returns>
  //  public Color this[int index]
  //  {
  //    get
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      int rc = 0;
  //      IntPtr ptr = m_cloud.ConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_GetColor(ptr, index, ref rc);
  //      return Color.FromArgb(rc);
  //    }
  //    set
  //    {
  //      if (index < 0 || index >= Count)
  //      {
  //        throw new IndexOutOfRangeException();
  //      }

  //      IntPtr ptr = m_cloud.NonConstPointer();
  //      UnsafeNativeMethods.ON_PointCloud_SetColor(ptr, index, value.ToArgb());
  //    }
  //  }
  //  #endregion

  //  #region IEnumerable<Color> Members
  //  public IEnumerator<Color> GetEnumerator()
  //  {
  //    return new PCEnum(this);
  //  }
  //  IEnumerator IEnumerable.GetEnumerator()
  //  {
  //    return GetEnumerator();
  //  }
  //  private class PCEnum : IEnumerator<Color>
  //  {
  //    #region members
  //    private PointCloudColorList m_owner;
  //    int position = -1;
  //    #endregion

  //    #region constructor
  //    public PCEnum(PointCloudColorList cloud_colors)
  //    {
  //      m_owner = cloud_colors;
  //    }
  //    #endregion

  //    #region enumeration logic
  //    public bool MoveNext()
  //    {
  //      position++;
  //      return (position < m_owner.Count);
  //    }
  //    public void Reset()
  //    {
  //      position = -1;
  //    }

  //    public Color Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    object IEnumerator.Current
  //    {
  //      get
  //      {
  //        try
  //        {
  //          return m_owner[position];
  //        }
  //        catch (IndexOutOfRangeException)
  //        {
  //          throw new InvalidOperationException();
  //        }
  //      }
  //    }
  //    #endregion

  //    #region IDisposable logic
  //    private bool m_disposed; // = false; <- initialized by runtime
  //    public void Dispose()
  //    {
  //      if (m_disposed) { return; }
  //      m_disposed = true;
  //      GC.SuppressFinalize(this);
  //    }
  //    #endregion
  //  }
  //  #endregion
  //}
//}