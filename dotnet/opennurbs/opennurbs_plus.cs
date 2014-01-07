using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.Runtime.InteropWrappers
{
  /// <summary>
  /// This is only needed when passing values to the Rhino C++ core, ignore
  /// for .NET plug-ins.
  /// </summary>
  [CLSCompliant(false)]
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 88)]
  public struct MeshPointDataStruct
  {
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_et;

    //ON_COMPONENT_INDEX m_ci;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public uint m_ci_type;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public int m_ci_index;

    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public int m_edge_index;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public int m_face_index;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public char m_Triangle;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_t0;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_t1;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_t2;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_t3;

    //ON_3dPoint m_P;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_Px;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_Py;
    /// <summary>
    /// This is only needed when passing values to the Rhino C++ core, ignore
    /// for .NET plug-ins.
    /// </summary>
    public double m_Pz;
  }
}

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a point that is found on a mesh.
  /// </summary>
  public class MeshPoint
  {
    internal Mesh m_parent;
    internal MeshPointDataStruct m_data;
    internal MeshPoint(Mesh parent, MeshPointDataStruct ds)
    {
      m_parent = parent;
      m_data = ds;
    }

    /// <summary>
    /// The mesh that is ralated to this point.
    /// </summary>
    public Mesh Mesh
    {
      get { return m_parent; }
    }

    /// <summary>
    /// Edge parameter when found.
    /// </summary>
    public double EdgeParameter
    {
      get { return m_data.m_et; }
    }

    /// <summary>
    /// Gets the component index of the intersecting element in the mesh.
    /// </summary>
    public ComponentIndex ComponentIndex
    {
      get
      {
        return new ComponentIndex((ComponentIndexType)m_data.m_ci_type, m_data.m_ci_index);
      }
    }

    /// <summary>
    /// When set, EdgeIndex is an index of an edge in the mesh's edge list.
    /// </summary>
    public int EdgeIndex
    {
      get { return m_data.m_edge_index; }
    }

    /// <summary>
    /// FaceIndex is an index of a face in mesh.Faces.
    /// When ComponentIndex refers to a vertex, any face that uses the vertex
    /// may appear as FaceIndex.  When ComponenctIndex refers to an Edge or
    /// EdgeIndex is set, then any face that uses that edge may appear as FaceIndex.
    /// </summary>
    public int FaceIndex
    {
      get { return m_data.m_face_index; }
    }

    //bool IsValid( ON_TextLog* text_log ) const;

    /// <summary>
    /// Gets the mesh face indices of the triangle where the
    /// intersection is on the face takes into consideration
    /// the way the quad was split during the intersection.
    /// </summary>
    public bool GetTriangle(out int a, out int b, out int c)
    {
      IntPtr pConstMesh = m_parent.ConstPointer();
      a = -1;
      b = -1;
      c = -1;
      return UnsafeNativeMethods.ON_MESHPOINT_GetTriangle(pConstMesh, ref m_data, ref a, ref b, ref b);
    }

    /// <summary>
    /// Face triangle where the intersection takes place:
    /// <para>0 is unset</para>
    /// <para>A is 0,1,2</para>
    /// <para>B is 0,2,3</para>
    /// <para>C is 0,1,3</para>
    /// <para>D is 1,2,3</para>
    /// </summary>
    public char Triangle
    {
      get { return m_data.m_Triangle; }
    }


    /// <summary>
    /// Barycentric quad coordinates for the point on the mesh
    /// face mesh.Faces[FaceIndex].  If the face is a triangle
    /// disregard T[3] (it should be set to 0.0). If the face is
    /// a quad and is split between vertexes 0 and 2, then T[3]
    /// will be 0.0 when point is on the triangle defined by vi[0],
    /// vi[1], vi[2] and T[1] will be 0.0 when point is on the
    /// triangle defined by vi[0], vi[2], vi[3]. If the face is a
    /// quad and is split between vertexes 1 and 3, then T[2] will
    /// be -1 when point is on the triangle defined by vi[0],
    /// vi[1], vi[3] and m_t[0] will be -1 when point is on the
    /// triangle defined by vi[1], vi[2], vi[3].
    /// </summary>
    public double[] T
    {
      get { return m_t ?? (m_t = new double[] { m_data.m_t0, m_data.m_t1, m_data.m_t2, m_data.m_t3 }); }
    }
    double[] m_t;

    /// <summary>
    /// Gets the location (position) of this point.
    /// </summary>
    public Point3d Point
    {
      get { return new Point3d(m_data.m_Px, m_data.m_Py, m_data.m_Pz); }
    }
  }
}

namespace Rhino.Geometry.Intersect
{
  //// keep private until we have something that works/makes sense
  //public class RayShooter //: IDisposable
  //{
  //  //IntPtr m_mesh_rtree = IntPtr.Zero;
  //  //Mesh m_target_mesh;
  //  //NOTE! This is NOT a direct wrapper around ON_RayShooter. The class name was used to define
  //  //      a general ray shooter against geometry. Different low level unmanaged classes are used
  //  //      by this class
  //  public RayShooter(Mesh targetMesh)
  //  {
  //    //m_target_mesh = targetMesh;
  //    //IntPtr pConstMesh = m_target_mesh.ConstPointer();
  //    //m_mesh_rtree = UnsafeNativeMethods.ON_RTree_NewFromMesh(pConstMesh);
  //  }

  //  public double Shoot(Ray3d ray)
  //  {
  //    return 0;
  //    //return UnsafeNativeMethods.ON_RTree_ShootRay(m_mesh_rtree, ref ray);
  //  }

  //}

  //public class ON_CurveLeafBox { }
  //public class ON_CurveTreeBezier : ON_BezierCurve { }
  //public class ON_SurfaceLeafBox { }
  //public class ON_SurfaceTreeBezier : ON_BezierSurface { }
  //public class ON_CurveTreeNode { }
  //public class ON_CurveTree { }
  //public class ON_SurfaceTreeNode { }
  //public class ON_SurfaceTree { }
  //public class ON_RayShooter { }
  //public class ON_MMX_POINT { }
  //public class ON_MMX_Polyline { }
  //public class ON_CURVE_POINT { }
  //public class ON_CMX_EVENT { }
  //public class ON_MeshTreeNode { }
  //public class ON_MeshTree { }

  //also add ON_RTree

#if RHINO_SDK

  /// <summary>
  /// Represents a particular instance of a clash or intersection between two meshes.
  /// </summary>
  public class MeshClash
  {
    Mesh m_mesh_a;
    Mesh m_mesh_b;
    Point3d m_P = Point3d.Unset;
    double m_radius;

    private MeshClash() { }

    /// <summary>
    /// Gets the first mesh.
    /// </summary>
    public Mesh MeshA { get { return m_mesh_a; } }

    /// <summary>
    /// Gets the second mesh.
    /// </summary>
    public Mesh MeshB { get { return m_mesh_b; } }

    /// <summary>
    /// If valid, then the sphere centered at ClashPoint of ClashRadius
    /// distance interesects the clashing meshes.
    /// </summary>
    public Point3d ClashPoint { get { return m_P; } }

    /// <summary>
    /// Gets the clash, or intersection, radius.
    /// </summary>
    public double ClashRadius { get { return m_radius; } }

    /// <summary>
    /// Searches for locations where the distance from <i>a mesh in one set</i> of meshes
    /// is less than distance to <i>another mesh in a second set</i> of meshes.
    /// </summary>
    /// <param name="setA">The first set of meshes.</param>
    /// <param name="setB">The second set of meshes.</param>
    /// <param name="distance">The largest distance at which there is a clash.
    /// All values smaller than this cause a clash as well.</param>
    /// <param name="maxEventCount">The maximum number of clash objects.</param>
    /// <returns>An array of clash objects.</returns>
    public static MeshClash[] Search(IEnumerable<Mesh> setA, IEnumerable<Mesh> setB, double distance, int maxEventCount)
    {
      IList<Mesh> _setA = setA as IList<Mesh> ?? new List<Mesh>(setA);

      IList<Mesh> _setB = setB as IList<Mesh> ?? new List<Mesh>(setB);


      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes_a = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh m in setA)
        meshes_a.Add(m, true);
      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes_b = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh m in setB)
        meshes_b.Add(m, true);

      IntPtr pClashEventList = UnsafeNativeMethods.ON_SimpleArray_ClashEvent_New();
      IntPtr pMeshesA = meshes_a.ConstPointer();
      IntPtr pMeshesB = meshes_b.ConstPointer();
      int count = UnsafeNativeMethods.ONC_MeshClashSearch(pMeshesA, pMeshesB, distance, maxEventCount, true, pClashEventList);

      MeshClash[] rc = new MeshClash[count];
      Point3d pt = new Point3d();
      int indexA = 0;
      int indexB = 0;
      double radius = distance / 2.0;
      for (int i = 0; i < count; i++)
      {
        MeshClash mc = new MeshClash();
        UnsafeNativeMethods.ON_SimpleArray_ClashEvent_GetEvent(pClashEventList, i, ref indexA, ref indexB, ref pt);
        if (indexA >= 0 && indexB >= 0)
        {
          mc.m_mesh_a = _setA[indexA];
          mc.m_mesh_b = _setB[indexB];
          mc.m_P = pt;
          mc.m_radius = radius;
        }
        rc[i] = mc;
      }

      meshes_a.Dispose();
      meshes_b.Dispose();

      UnsafeNativeMethods.ON_SimpleArray_ClashEvent_Delete(pClashEventList);
      return rc;
    }

    /// <summary>
    /// Searches the locations where the distance from <i>the first mesh</i> to <i>a mesh in the second set</i> of meshes
    /// is less than the provided value.
    /// </summary>
    /// <param name="meshA">The first mesh.</param>
    /// <param name="setB">The second set of meshes.</param>
    /// <param name="distance">The largest distance at which there is a clash.
    /// All values smaller than this cause a clash as well.</param>
    /// <param name="maxEventCount">The maximum number of clash objects.</param>
    /// <returns>An array of clash objects.</returns>
    public static MeshClash[] Search(Mesh meshA, IEnumerable<Mesh> setB, double distance, int maxEventCount)
    {
      return Search(new Mesh[] { meshA }, setB, distance, maxEventCount);
    }

    /// <summary>
    /// Searches the locations where the distance from <i>the first mesh</i> to <i>the second mesh</i>
    /// is less than the provided value.
    /// </summary>
    /// <param name="meshA">The first mesh.</param>
    /// <param name="meshB">The second mesh.</param>
    /// <param name="distance">The largest distance at which there is a clash.
    /// All values smaller than this cause a clash as well.</param>
    /// <param name="maxEventCount">The maximum number of clash objects.</param>
    /// <returns>An array of clash objects.</returns>
    public static MeshClash[] Search(Mesh meshA, Mesh meshB, double distance, int maxEventCount)
    {
      return Search(new Mesh[] { meshA }, new Mesh[] { meshB }, distance, maxEventCount);
    }
  }
#endif
}
