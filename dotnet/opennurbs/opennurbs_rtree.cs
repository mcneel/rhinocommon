using System;
using System.Collections.Generic;


namespace Rhino.Geometry
{
  public class RTreeEventArgs : EventArgs
  {
    IntPtr m_element_a;
    IntPtr m_element_b = IntPtr.Zero;
    bool m_bCancel = false;
    internal RTreeEventArgs(IntPtr a)
    {
      m_element_a = a;
    }
    internal RTreeEventArgs(IntPtr a, IntPtr b)
    {
      m_element_a = a;
      m_element_b = b;
    }

    public int Id { get { return m_element_a.ToInt32(); } }
    public IntPtr IdPtr { get { return m_element_a; } }

    public bool Cancel
    {
      get { return m_bCancel; }
      set { m_bCancel = value; }
    }

    /// <summary>
    /// If search is using two r-trees, IdB is element b in the search
    /// </summary>
    public int IdB { get { return m_element_b.ToInt32(); } }
    public IntPtr IdBPtr { get { return m_element_b; } }
  }

  /// <summary>
  /// Spatial search structure based on implementations of the R-Tree algorithm by Toni Gutman
  /// </summary>
  /// <remarks>
  /// The opennurbs rtree code is a modifed version of the free and unrestricted
  /// R-tree implementation obtianed from http://www.superliminal.com/sources/sources.htm
  /// </remarks>
  public class RTree : IDisposable
  {
    IntPtr m_ptr; //ON_rTree* - this class is never const
    long m_memory_pressure = 0;
    int m_count = -1;

    public RTree()
    {
      m_ptr = UnsafeNativeMethods.ON_RTree_New();
    }

    /// <summary>
    /// Create an R-tree with an element for each face in the mesh.
    /// The element id is set to the index of the face.
    /// </summary>
    /// <param name="mesh"></param>
    /// <returns></returns>
    public static RTree CreateMeshFaceTree(Mesh mesh)
    {
      RTree rc = new RTree();
      IntPtr pRtree = rc.NonConstPointer();
      IntPtr pConstMesh = mesh.ConstPointer();
      if (!UnsafeNativeMethods.ON_RTree_CreateMeshFaceTree(pRtree, pConstMesh))
      {
        rc.Dispose();
        rc = null;
      }
      uint size = UnsafeNativeMethods.ON_RTree_SizeOf(pRtree);
      rc.m_memory_pressure = size;
      GC.AddMemoryPressure(rc.m_memory_pressure);
      return rc;
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(Point3d point, int elementId)
    {
      return Insert(new BoundingBox(point, point), elementId);
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(Point3d point, IntPtr elementId)
    {
      return Insert(new BoundingBox(point, point), elementId);
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="box"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(BoundingBox box, int elementId)
    {
      return Insert(box, new IntPtr(elementId));
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="box"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(BoundingBox box, IntPtr elementId)
    {
      m_count = -1; 
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_RTree_InsertRemove(pThis, true, box.Min, box.Max, elementId);
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(Point2d point, int elementId)
    {
      return Insert(new Point3d(point.X, point.Y, 0), elementId);
    }

    /// <summary>Insert an element into the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully inserted</returns>
    public bool Insert(Point2d point, IntPtr elementId)
    {
      return Insert(new Point3d(point.X, point.Y, 0), elementId);
    }

    /// <summary>Remove an element from the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully removed</returns>
    public bool Remove(Point3d point, int elementId)
    {
      return Remove(new BoundingBox(point, point), elementId);
    }

    /// <summary>Remove an element from the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully removed</returns>
    public bool Remove(Point3d point, IntPtr elementId)
    {
      return Remove(new BoundingBox(point, point), elementId);
    }

    /// <summary>Remove an element from the tree</summary>
    /// <param name="box"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully removed</returns>
    public bool Remove(BoundingBox box, int elementId)
    {
      return Remove(box, new IntPtr(elementId));
    }

    /// <summary>Remove an element from the tree</summary>
    /// <param name="box"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully removed</returns>
    public bool Remove(BoundingBox box, IntPtr elementId)
    {
      m_count = -1; 
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_RTree_InsertRemove(pThis, false, box.Min, box.Max, elementId);
    }

    /// <summary>Remove an element from the tree</summary>
    /// <param name="point"></param>
    /// <param name="elementId"></param>
    /// <returns>True if element was successfully removed</returns>
    public bool Remove(Point2d point, int elementId)
    {
      return Remove(new Point3d(point.X, point.Y, 0), elementId);
    }

    /// <summary>
    /// Removes all elements
    /// </summary>
    public void Clear()
    {
      m_count = -1; 
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_RTree_RemoveAll(pThis);
    }

    public int Count
    {
      get
      {
        if (m_count < 0)
        {
          IntPtr pThis = NonConstPointer();
          m_count = UnsafeNativeMethods.ON_RTree_ElementCount(pThis);
        }
        return m_count;
      }
    }
    
    static int m_next_serial_number = 1;
    class Callbackholder
    {
      public RTree Sender { get; set; }
      public int SerialNumber { get; set; }
      public EventHandler<RTreeEventArgs> Callback { get; set; }
    }
    static List<Callbackholder> m_callbacks;

    internal delegate int SearchCallback(int serial_number, IntPtr idA, IntPtr idB);
    private static int CustomSearchCallback(int serial_number, IntPtr idA, IntPtr idB)
    {
      Callbackholder cbh = null;
      for (int i = 0; i < m_callbacks.Count; i++)
      {
        Callbackholder holder = m_callbacks[i];
        if (holder.SerialNumber == serial_number)
        {
          cbh = holder;
          break;
        }
      }
      int rc = 1;
      if (cbh != null)
      {
        RTreeEventArgs e = new RTreeEventArgs(idA, idB);
        cbh.Callback(cbh.Sender, e);
        if (e.Cancel)
          rc = 0;
      }
      return rc;
    }

    /// <summary>
    /// </summary>
    /// <param name="box"></param>
    /// <param name="callback"></param>
    /// <returns>
    /// True if entire tree was searched.  It is possible no results were found.
    /// </returns>
    public bool Search(BoundingBox box, EventHandler<RTreeEventArgs> callback)
    {
      IntPtr pConstTree = ConstPointer();
      if (m_callbacks == null)
        m_callbacks = new List<Callbackholder>();
      Callbackholder cbh = new Callbackholder();
      cbh.SerialNumber = m_next_serial_number++;
      cbh.Callback = callback;
      cbh.Sender = this;
      m_callbacks.Add(cbh);
      SearchCallback searcher = CustomSearchCallback;
      bool rc = UnsafeNativeMethods.ON_RTree_Search(pConstTree, box.Min, box.Max, cbh.SerialNumber, searcher);
      for (int i = 0; i < m_callbacks.Count; i++)
      {
        if (m_callbacks[i].SerialNumber == cbh.SerialNumber)
        {
          m_callbacks.RemoveAt(i);
          break;
        }
      }
      return rc;
    }

    /// <summary>
    /// Search two R-trees for all pairs elements whose bounding boxes overlap.
    /// </summary>
    /// <param name="treeA"></param>
    /// <param name="treeB"></param>
    /// <param name="tolerance">
    /// If the distance between a pair of bounding boxes is less than tolerance,
    /// then callback is called.
    /// </param>
    /// <param name="callback"></param>
    /// <returns>
    /// True if entire tree was searched.  It is possible no results were found.
    /// </returns>
    public static bool SearchOverlaps(RTree treeA, RTree treeB, double tolerance, EventHandler<RTreeEventArgs> callback)
    {
      IntPtr pConstTreeA = treeA.ConstPointer();
      IntPtr pConstTreeB = treeB.ConstPointer();
      if (m_callbacks == null)
        m_callbacks = new List<Callbackholder>();
      Callbackholder cbh = new Callbackholder();
      cbh.SerialNumber = m_next_serial_number++;
      cbh.Callback = callback;
      cbh.Sender = null;
      m_callbacks.Add(cbh);
      SearchCallback searcher = CustomSearchCallback;
      bool rc = UnsafeNativeMethods.ON_RTree_Search2(pConstTreeA, pConstTreeB, tolerance, cbh.SerialNumber, searcher);
      for (int i = 0; i < m_callbacks.Count; i++)
      {
        if (m_callbacks[i].SerialNumber == cbh.SerialNumber)
        {
          m_callbacks.RemoveAt(i);
          break;
        }
      }
      return rc;
    }

    #region pointer / disposable handlers
    IntPtr ConstPointer() { return m_ptr; }
    IntPtr NonConstPointer() { return m_ptr; }
    ~RTree()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_RTree_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
      if (m_memory_pressure > 0)
      {
        GC.RemoveMemoryPressure(m_memory_pressure);
        m_memory_pressure = 0;
      }
    }
    #endregion

  }
}
