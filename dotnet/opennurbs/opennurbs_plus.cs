using System;
using System.Collections.Generic;

namespace Rhino.Geometry.Intersect
{
  // keep private until we have something that works/makes sense
  /*public*/ class RayShooter //: IDisposable
  {
    //IntPtr m_mesh_rtree = IntPtr.Zero;
    //Mesh m_target_mesh;
    //NOTE! This is NOT a direct wrapper around ON_RayShooter. The class name was used to define
    //      a general ray shooter against geometry. Different low level unmanaged classes are used
    //      by this class
    public RayShooter(Mesh targetMesh)
    {
      //m_target_mesh = targetMesh;
      //IntPtr pConstMesh = m_target_mesh.ConstPointer();
      //m_mesh_rtree = UnsafeNativeMethods.ON_RTree_NewFromMesh(pConstMesh);
    }

    public double Shoot(Ray3d ray)
    {
      return 0;
      //return UnsafeNativeMethods.ON_RTree_ShootRay(m_mesh_rtree, ref ray);
    }

  }

  //public class ON_CurveLeafBox { }
  //public class ON_CurveTreeBezier : ON_BezierCurve { }
  //public class ON_SurfaceLeafBox { }
  //public class ON_SurfaceTreeBezier : ON_BezierSurface { }
  //public class ON_CurveTreeNode { }
  //public class ON_CurveTree { }
  //public class ON_SurfaceTreeNode { }
  //public class ON_SurfaceTree { }
  //public class ON_RayShooter { }
  //public class ON_MESH_POINT { }
  //public class ON_MMX_POINT { }
  //public class ON_MMX_Polyline { }
  //public class ON_CURVE_POINT { }
  //public class ON_CMX_EVENT { }
  //public class ON_MeshTreeNode { }
  //public class ON_MeshTree { }

  /*
  //also add ON_RTree

  public class MeshClash
  {
    Mesh m_mesh_a;
    Mesh m_mesh_b;
    Point3d m_P;
    double m_radius;

    public Mesh MeshA { get { return m_mesh_a; } }
    public Mesh MeshB { get { return m_mesh_b; } }
    public Point3d ClashPoint { get { return m_P; } }


    public static MeshClash[] Search(IEnumerable<Mesh> setA, IEnumerable<Mesh> setB, double distance, int maxEventCount)
    {
      //call ON_MeshClashSearch
      return null;
    }
  }
   */
}
