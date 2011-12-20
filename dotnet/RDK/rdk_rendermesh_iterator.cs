#pragma warning disable 1591
using System;
using System.Collections;
using System.Diagnostics;
using Rhino.Geometry;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public class RenderMesh
  {
    private IntPtr m_renderMesh = IntPtr.Zero;
    private RenderMeshIterator m_iterator;

    internal RenderMesh(IntPtr pRenderMesh, RenderMeshIterator iterator)
    {
      m_renderMesh = pRenderMesh;
      m_iterator = iterator;
    }

    public Rhino.DocObjects.RhinoObject Object
    {
      get
      {
        IntPtr pObject = UnsafeNativeMethods.Rdk_RenderMesh_Object(ConstPointer());
        return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pObject);
      }
    }



    public enum PrimitiveTypes : int
    {
      None = 0,
      Mesh = 1,
      Sphere = 2,
      Plane = 3,
      Box = 4,
      Cone = 5,
    }

    /// <summary>
    /// Call this before extracting meshes. Although the Mesh property
    /// will always work correctly no matter what the primitive type is,
    /// if your renderer supports rendering any of these primitives, you can
    /// extract the data using the Sphere, Plane, Cone and Box properties.
    /// </summary>
    public PrimitiveTypes PrimitiveType
    {
      get
      {
        return (PrimitiveTypes)UnsafeNativeMethods.Rdk_RenderMesh_PrimitiveType(ConstPointer());
      }
    }

    /// <summary>
    /// Returns the mesh associated with the object.
    /// </summary>
    public Rhino.Geometry.Mesh Mesh
    {
      get
      {
        IntPtr pMesh = UnsafeNativeMethods.Rdk_RenderMesh_Mesh(ConstPointer());
        if (pMesh == IntPtr.Zero)
          return null;
        return new Rhino.Geometry.Mesh(pMesh, this);
      }
    }

    internal IntPtr GetConst_ON_Mesh_Pointer()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.Rdk_RenderMesh_Mesh(pConstThis);
    }

    /// <summary>
    /// Returns a sphere if this object represents a primitive sphere, or null.
    /// </summary>
    public Rhino.Geometry.Sphere Sphere
    {
      get
      {
        double radius = 1.0;
        Rhino.Geometry.Point3d center = new Rhino.Geometry.Point3d();
        bool isSphere = 1 == UnsafeNativeMethods.Rdk_RenderMesh_Sphere(ConstPointer(), ref center, ref radius);

        if (isSphere)
        {
          return new Rhino.Geometry.Sphere(center, radius);
        }
        return new Rhino.Geometry.Sphere();
      }
    }

    //bool Box(ON_PlaneSurface& plane, ON_Interval& z_interval) const;
    //void SetBox(const ON_PlaneSurface& plane, const ON_Interval& z_interval);

    //bool Plane(ON_PlaneSurface& plane) const;
    //void SetPlane(const ON_PlaneSurface& plane);	

    //bool Cone(ON_Cone& cone, ON_Plane& truncation) const;
    //void SetCone(const ON_Cone& cone, const ON_Plane& plane);

    public Rhino.Geometry.Transform InstanceTransform
    {
      get
      {
        Rhino.Geometry.Transform t = new Transform();
        UnsafeNativeMethods.Rdk_RenderMesh_XformInstance(ConstPointer(), ref t);
        return t;
      }
    }

    public bool IsRenderMaterial
    {
      get
      {
        return 1 == UnsafeNativeMethods.Rdk_RenderMesh_IsRdkMaterial(ConstPointer());
      }
    }

    /// <summary>
    /// Implmented as !IsRenderMaterial - the material is always either a RenderMaterial or a Legacy material (Rhino.DocObjects.Material)
    /// </summary>
    public bool IsLegacyMaterial
    {
      get
      {
        return !IsRenderMaterial;
      }
    }

    /// <summary>
    /// Will return null if "IsRenderMaterial" returns false.
    /// </summary>
    public Rhino.Render.RenderMaterial Material
    {
      get
      {
        IntPtr pMaterial = UnsafeNativeMethods.Rdk_RenderMesh_RdkMaterial(ConstPointer());
        if (pMaterial != IntPtr.Zero)
        {
          return RenderContent.FromPointer(pMaterial) as RenderMaterial;
        }
        return null;
      }
    }

    //Will return null if "IsLegacyMaterial" returns false
    public Rhino.DocObjects.Material LegacyMaterial
    {
      get
      {
        IntPtr pMaterial = UnsafeNativeMethods.Rdk_RenderMesh_OnMaterial(ConstPointer());

        return pMaterial!=IntPtr.Zero ? Rhino.DocObjects.Material.NewTemporaryMaterial(pMaterial) : null;
      }
    }

    public Rhino.Geometry.BoundingBox BoundingBox
    {
      get
      {
        Point3d min = new Point3d();
        Point3d max = new Point3d();

        UnsafeNativeMethods.Rdk_RenderMesh_BoundingBox(ConstPointer(), ref min, ref max);

        return new BoundingBox(min, max);
      }
    }

    #region internals
    internal IntPtr ConstPointer()
    {
      return m_renderMesh;
    }
    internal IntPtr NonConstPointer()
    {
      return m_renderMesh;
    }
    #endregion


  }

  public class RenderMeshIterator : IDisposable, IEnumerator
  {
    public RenderMeshIterator(Guid plugInId, Rhino.DocObjects.ViewportInfo vp, bool needTriangleMesh)
    {
      IntPtr pIt = UnsafeNativeMethods.Rdk_RenderMeshIterator_New(plugInId, needTriangleMesh, vp.ConstPointer());
      if (pIt != IntPtr.Zero)
      {
        m_pIterator = pIt;
      }
    }

    private IntPtr m_pIterator = IntPtr.Zero;
    internal RenderMeshIterator(IntPtr pIterator)
    {
      m_pIterator = pIterator;
    }

    public void EnsureRenderMeshesCreated()
    {
      UnsafeNativeMethods.Rdk_RenderMeshIterator_EnsureRenderMeshesCreated(ConstPointer());
    }


    public Rhino.Geometry.BoundingBox SceneBoundingBox
    {
      get
      {
        Point3d min = new Point3d();
        Point3d max = new Point3d();

        UnsafeNativeMethods.Rdk_RenderMeshIterator_SceneBoundingBox(ConstPointer(), ref min, ref max);

        return new BoundingBox(min, max);
      }
    }

    public bool SupportsAutomaticInstancing
    {
      get
      {
        return 1 == UnsafeNativeMethods.Rdk_RenderMeshIterator_SupportsAutomaticInstancing(ConstPointer());
      }
    }



    #region IDisposable Members

    ~RenderMeshIterator()
    {
      Dispose(false);
      GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
      Dispose(true);
    }

    protected void Dispose(bool isDisposing)
    {
      UnsafeNativeMethods.Rdk_RenderMeshIterator_Delete(m_pIterator);
    }

    #endregion

    #region internals
    internal IntPtr ConstPointer()
    {
      return m_pIterator;
    }
    #endregion





    #region IEnumerator Members

    private RenderMesh m_current;
    public object Current
    {
      get
      {
        return m_current;
      }
    }

    public bool MoveNext()
    {
      RenderMesh mesh = new RenderMesh(UnsafeNativeMethods.Rdk_RenderMesh_New(), this);

      bool bRet = 1 == UnsafeNativeMethods.Rdk_RenderMeshIterator_Next(ConstPointer(), mesh.NonConstPointer());

      if (bRet)
      {
        m_current = mesh;
      }
      else
      {
        m_current = null;
      }

      return bRet;
    }

    public void Reset()
    {
      UnsafeNativeMethods.Rdk_RenderMeshIterator_Reset(ConstPointer());
    }

    #endregion
  }
}

#endif
