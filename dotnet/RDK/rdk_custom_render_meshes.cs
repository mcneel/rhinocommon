#pragma warning disable 1591
using System;
using System.Diagnostics;
using System.Collections.Generic;

#if RDK_UNCHECKED

using Rhino.Render;

namespace Rhino.Render.CustomRenderMesh
{
  public enum PrimitiveType : int
  {
      None = 0,
      Mesh = 1,
      Sphere = 2,
      Plane = 3,
      Box = 4,
      Cone = 5,
  }

  

  public class ObjectMeshes : IDisposable
  {
    public ObjectMeshes(Rhino.DocObjects.RhinoObject obj)
    {
      m_pRenderMeshes = UnsafeNativeMethods.Rdk_CustomMeshes_New(obj.ConstPointer());
      m_bAutoDelete = true;
    }

   internal ObjectMeshes(IntPtr pNative)
    {
      m_pRenderMeshes = pNative;
      m_bAutoDelete = false;
    }

    public Guid ProviderId
    {
        get { return UnsafeNativeMethods.Rdk_CustomMeshes_ProviderId(ConstPointer()); }
        set { UnsafeNativeMethods.Rdk_CustomMeshes_SetProviderId(NonConstPointer(), value); }
    }

    public void Add(Rhino.Geometry.Mesh mesh, RenderMaterial material)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_AddMesh(NonConstPointer(), mesh.ConstPointer(), material.ConstPointer());
    }

    public void Add(Rhino.Geometry.Sphere sphere, RenderMaterial material)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_AddSphere(NonConstPointer(), sphere.Center, 
                                                     sphere.EquitorialPlane.XAxis, 
                                                     sphere.EquitorialPlane.YAxis, 
                                                     sphere.Radius, 
                                                     material.ConstPointer());
    }

    public void Add(Rhino.Geometry.Cone cone, Rhino.Geometry.Plane truncation, RenderMaterial material)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_AddCone(NonConstPointer(), cone.BasePoint, 
                                                   cone.Plane.XAxis, 
                                                   cone.Plane.YAxis, 
                                                   cone.Height,
                                                   cone.Radius,
                                                   truncation.Origin, 
                                                   truncation.XAxis, 
                                                   truncation.YAxis, 
                                                   material.ConstPointer());
    }

    public void Add(Rhino.Geometry.PlaneSurface plane, RenderMaterial material)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_AddPlane(NonConstPointer(), plane.ConstPointer(), material.ConstPointer());
    }

    public void Add(Rhino.Geometry.Box box, RenderMaterial material)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_AddBox(NonConstPointer(), 
                                                  box.Plane.Origin, 
                                                  box.Plane.XAxis, 
                                                  box.Plane.YAxis, 
                                                  box.X.Min, box.X.Max, 
                                                  box.Y.Min, box.Y.Max, 
                                                  box.Z.Min, box.Z.Max, 
                                                  material.ConstPointer());
    }

    public int Count
    {
      get { return UnsafeNativeMethods.Rdk_CustomMeshes_Count(ConstPointer()); }
    }

    public bool UseObjectsMappingChannels
    {
      get
      {
        return UnsafeNativeMethods.Rdk_CustomMeshes_UseObjectsMappingChannels(ConstPointer());;
      }
      set
      {
        UnsafeNativeMethods.Rdk_CustomMeshes_SetUseObjectsMappingChannels(NonConstPointer(), value);
      }
    }

    public PrimitiveType Type(int index)
    {
        return (PrimitiveType)UnsafeNativeMethods.Rdk_CustomMeshes_PrimitiveType(ConstPointer(), index);
    }

    public Rhino.Geometry.Mesh Mesh(int index)
    {
      IntPtr pMesh = UnsafeNativeMethods.Rdk_CustomMeshes_Mesh(ConstPointer(), index);
      if (pMesh != IntPtr.Zero)
      {
        Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh(pMesh, null);
        mesh.DoNotDestructOnDispose();
        return mesh;
      }
      return null;
    }

    public bool Sphere(int index, ref Rhino.Geometry.Sphere sphere)
    {
      Rhino.Geometry.Point3d origin = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Vector3d xaxis = new Rhino.Geometry.Vector3d();
      Rhino.Geometry.Vector3d yaxis = new Rhino.Geometry.Vector3d();
      double radius = 0.0;

      if (UnsafeNativeMethods.Rdk_CustomMeshes_Sphere(ConstPointer(), index, ref origin, ref xaxis, ref yaxis, ref radius))
      {
        sphere = new Rhino.Geometry.Sphere(new Rhino.Geometry.Plane(origin, xaxis, yaxis), radius);
        return true;
      }
      return false;
    }

    public bool Box(int index, ref Rhino.Geometry.Box box)
    {
      Rhino.Geometry.Point3d origin = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Vector3d xaxis = new Rhino.Geometry.Vector3d();
      Rhino.Geometry.Vector3d yaxis = new Rhino.Geometry.Vector3d();
      double minX = 0.0, maxX = 0.0;
      double minY = 0.0, maxY = 0.0;
      double minZ = 0.0, maxZ = 0.0;

      if (UnsafeNativeMethods.Rdk_CustomMeshes_Box(ConstPointer(), index, ref origin, ref xaxis, ref yaxis, ref minX, ref maxX, ref minY, ref maxY, ref minZ, ref maxZ))
      {
        box = new Rhino.Geometry.Box(new Rhino.Geometry.Plane(origin, xaxis, yaxis),
                                                        new Rhino.Geometry.Interval(minX, maxX),
                                                        new Rhino.Geometry.Interval(minY, maxY),
                                                        new Rhino.Geometry.Interval(minZ, maxZ));
        return true;
      }
      return false;
    }

    public bool Plane(int index, ref Rhino.Geometry.PlaneSurface plane)
    {
      Rhino.Geometry.Point3d origin = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Vector3d xaxis = new Rhino.Geometry.Vector3d();
      Rhino.Geometry.Vector3d yaxis = new Rhino.Geometry.Vector3d();
      double minX = 0.0, maxX = 0.0;
      double minY = 0.0, maxY = 0.0;

      if (UnsafeNativeMethods.Rdk_CustomMeshes_Plane(ConstPointer(), index, ref origin, ref xaxis, ref yaxis, ref minX, ref maxX, ref minY, ref maxY))
      {
        plane = new Rhino.Geometry.PlaneSurface(new Rhino.Geometry.Plane(origin, xaxis, yaxis),
                                                        new Rhino.Geometry.Interval(minX, maxX),
                                                        new Rhino.Geometry.Interval(minY, maxY));
        return true;
      }
      return false;
    }

    public bool Cone(int index, ref Rhino.Geometry.Cone cone, ref Rhino.Geometry.Plane truncation)
    {
      Rhino.Geometry.Point3d origin = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Vector3d xaxis = new Rhino.Geometry.Vector3d();
      Rhino.Geometry.Vector3d yaxis = new Rhino.Geometry.Vector3d();

      double height = 0.0;
      double radius = 0.0;

      Rhino.Geometry.Point3d t_origin = new Rhino.Geometry.Point3d();
      Rhino.Geometry.Vector3d t_xaxis = new Rhino.Geometry.Vector3d();
      Rhino.Geometry.Vector3d t_yaxis = new Rhino.Geometry.Vector3d();


      if (UnsafeNativeMethods.Rdk_CustomMeshes_Cone(ConstPointer(), index, 
                                                       ref origin, 
                                                       ref xaxis, 
                                                       ref yaxis, 
                                                       ref height, ref radius,
                                                       ref t_origin, ref t_xaxis, ref t_yaxis))
      {
        cone = new Rhino.Geometry.Cone(new Rhino.Geometry.Plane(origin, xaxis, yaxis), height, radius);
        truncation = new Rhino.Geometry.Plane(t_origin, t_xaxis, t_yaxis);
        return true;
      }
      return false;
    }

    public RenderMaterial Material(int index)
    {
      IntPtr pMaterial = UnsafeNativeMethods.Rdk_CustomMeshes_Material(ConstPointer(), index);
      if (pMaterial != IntPtr.Zero)
      {
        RenderMaterial material = RenderContent.FromPointer(pMaterial) as RenderMaterial;
        return material;
      }
      return null;
    }

    public Rhino.DocObjects.RhinoObject Object
    {
      get
      {
        IntPtr pObject = UnsafeNativeMethods.Rdk_CustomMeshes_Object(ConstPointer());
        if (pObject != IntPtr.Zero)
        {
          return Rhino.DocObjects.RhinoObject.CreateRhinoObjectHelper(pObject);
        }
        return null;
      }
    }

    public Rhino.Geometry.Transform GetInstanceTransform(int index)
    {
      Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform();
      UnsafeNativeMethods.Rdk_CustomMeshes_GetInstanceTransform(ConstPointer(), index, ref xform);
      return xform;
    }

    public void SetInstanceTransform(int index, Rhino.Geometry.Transform xform)
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_SetInstanceTransform(NonConstPointer(), index, xform);
    }

    public void ConvertMeshesToTriangles()
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_ConvertMeshesToTriangles(NonConstPointer());
    }

    public void Clear()
    {
      UnsafeNativeMethods.Rdk_CustomMeshes_Clear(NonConstPointer());
    }

    public bool AutoDeleteMeshesOn()
    {
      return UnsafeNativeMethods.Rdk_CustomMeshes_AutoDeleteMeshesOn(ConstPointer());
    }

    public bool AutoDeleteMaterialsOn()
    {
      return UnsafeNativeMethods.Rdk_CustomMeshes_AutoDeleteMaterialsOn(ConstPointer());
    }

    #region internals
    private IntPtr m_pRenderMeshes = IntPtr.Zero;
    bool m_bAutoDelete;
    internal IntPtr ConstPointer()     { return m_pRenderMeshes; }
    internal IntPtr NonConstPointer()  { return m_pRenderMeshes; }
    #endregion

    #region IDisposable pattern implementation
    ~ObjectMeshes()
    {
      Dispose(false);
    }
    public void Dispose()
    {
      Dispose(true);
    }
    private bool disposed = false;
    private void Dispose(bool disposing)
    {
      if (!disposed)
      {
        if (m_bAutoDelete)
        {
          UnsafeNativeMethods.Rdk_CustomMeshes_Delete(m_pRenderMeshes);
        }
        m_pRenderMeshes = IntPtr.Zero;
      }
      disposed = true;
    }
    #endregion
  }

  public enum MeshTypes : int
  {
    Standard = 0,
    Preview  = 1,
  }

  public class Manager
  {
    public static void AllObjectsChanged()
    {
      UnsafeNativeMethods.Rdk_CRMManager_Changed();
    }

    public static void ObjectChanged(Rhino.DocObjects.RhinoObject obj)
    {
      UnsafeNativeMethods.Rdk_CRMManager_EVF("ObjectChanged", obj.ConstPointer());
    }

    public static ObjectMeshes PreviousMeshes()
    {
      IntPtr pMeshes = UnsafeNativeMethods.Rdk_CRMManager_EVF("PreviousMeshes", IntPtr.Zero);
      if (pMeshes != IntPtr.Zero)
      {
        return new ObjectMeshes(pMeshes);
      }
      return null;
    }

    public static void ForceObjectIntoPreviewCache(Rhino.DocObjects.RhinoObject obj)
    {
      UnsafeNativeMethods.Rdk_CRMManager_EVF("ForceObjectIntoPreviewCache", obj.ConstPointer());
    }

    /// <summary>
    /// Determine if custom render meshes will be built for a particular object.
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="obj">the Rhino object of interest.</param>
    /// <param name="requestingPlugIn">type of mesh to build.</param>
    /// <param name="meshType">UUID of the plug-in requesting the meshes.</param>
    /// <param name="soleProviderId">the UUID of the sole provider to call.</param>
    /// <returns>true if BuildCustomMeshes() will build custom render mesh(es) for the given object.</returns>
    public static bool WillBuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType, Guid soleProviderId)
    {
      return UnsafeNativeMethods.Rdk_CRMManager_WillBuildCustomMeshSole(vp.ConstPointer(), obj.ConstPointer(), requestingPlugIn, (int)meshType, soleProviderId);
    }

    /// <summary>
    /// Determine if custom render meshes will be built for a particular object.
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="obj">the Rhino object of interest.</param>
    /// <param name="requestingPlugIn">type of mesh to build.</param>
    /// <param name="meshType">UUID of the plug-in requesting the meshes.</param>
    /// <returns>true if BuildCustomMeshes() will build custom render mesh(es) for the given object.</returns>
    public static bool WillBuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType)
    {
      return UnsafeNativeMethods.Rdk_CRMManager_WillBuildCustomMesh(vp.ConstPointer(), obj.ConstPointer(), requestingPlugIn, (int)meshType);
    }

    /// <summary>
    /// Returns a bounding box for the custom render meshes for the given object.
    /// </summary>
    /// <param name="vp"> the viewport being rendered.</param>
    /// <param name="obj">Rhino object of interest.</param>
    /// <param name="requestingPlugIn">UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType"> type of mesh(es) to build the bounding box for.</param>
    /// <param name="soleProviderId">the sole provider to call.</param>
    /// <returns>ON_BoundingBox for the meshes.</returns>
    public static Rhino.Geometry.BoundingBox BoundingBox(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType, Guid soleProviderId)
    {
      Geometry.Point3d min = new Geometry.Point3d();
      Geometry.Point3d max = new Geometry.Point3d();

      if (UnsafeNativeMethods.Rdk_CRMManager_BoundingBoxSole(vp.ConstPointer(), obj.ConstPointer(), requestingPlugIn, (int)meshType, soleProviderId, ref min, ref max))
      {
        Rhino.Geometry.BoundingBox bb = new Geometry.BoundingBox();
        return bb;
      }

      return new Geometry.BoundingBox();
    }

    /// <summary>
    /// Returns a bounding box for the custom render meshes for the given object.
    /// </summary>
    /// <param name="vp"> the viewport being rendered.</param>
    /// <param name="obj">Rhino object of interest.</param>
    /// <param name="requestingPlugIn">UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType"> type of mesh(es) to build the bounding box for.</param>
    /// <returns>ON_BoundingBox for the meshes.</returns>
    public static Rhino.Geometry.BoundingBox BoundingBox(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType)
    {
      Rhino.Geometry.Point3d min = new Geometry.Point3d();
      Rhino.Geometry.Point3d max = new Geometry.Point3d();

      Rhino.Geometry.BoundingBox bb = new Geometry.BoundingBox();

      if (UnsafeNativeMethods.Rdk_CRMManager_BoundingBox(vp.ConstPointer(), obj.ConstPointer(), requestingPlugIn, (int)meshType, ref min, ref max))
      {
        bb.Min = min;
        bb.Max = max;
      }

      return bb;
    }

    /// <summary>
    /// Build custom render mesh(es) for the given object.
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="objMeshes">The meshes object to fill with custom meshes - the Object property will already be set.</param>
    /// <param name="requestingPlugIn">the UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType"> type of mesh(es) to build.</param>
    /// <param name="soleProviderId">the sole provider to call.</param>
    /// <returns>true if successful.</returns>
    public static bool BuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, ObjectMeshes objMeshes, Guid requestingPlugIn, MeshTypes meshType, Guid soleProviderId)
    {
      return UnsafeNativeMethods.Rdk_CRMManager_BuildCustomMeshesSole(vp.ConstPointer(), objMeshes.NonConstPointer(), requestingPlugIn, (int)meshType, soleProviderId);
    }

    /// <summary>
    /// Build custom render mesh(es) for the given object.
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="objMeshes">The meshes object to fill with custom meshes - the Object property will already be set.</param>
    /// <param name="requestingPlugIn">the UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType"> type of mesh(es) to build.</param>
    /// <returns>true if successful.</returns>
    public static bool BuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, ObjectMeshes objMeshes, Guid requestingPlugIn, MeshTypes meshType)
    {
      return UnsafeNativeMethods.Rdk_CRMManager_BuildCustomMeshes(vp.ConstPointer(), objMeshes.NonConstPointer(), requestingPlugIn, (int)meshType);
    }

    #region events

    internal delegate void CRMManagerEmptyCallback();

    private static CRMManagerEmptyCallback m_OnCustomRenderMeshesChanged;
    private static void OnCustomRenderMeshesChanged()
    {
      if (m_custom_render_meshes_changed != null)
      {
        try { m_custom_render_meshes_changed(null, System.EventArgs.Empty); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler m_custom_render_meshes_changed;

    /// <summary>
    /// Monitors when custom render meshes are changed.
    /// </summary>
    public static event EventHandler CustomRenderMeshesChanged
    {
      add
      {
        if (m_custom_render_meshes_changed == null)
        {
          m_OnCustomRenderMeshesChanged = OnCustomRenderMeshesChanged;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetCustomRenderMeshesChangedEventCallback(m_OnCustomRenderMeshesChanged, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_custom_render_meshes_changed += value;
      }
      remove
      {
        m_custom_render_meshes_changed -= value;
        if (m_custom_render_meshes_changed == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetCustomRenderMeshesChangedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnCustomRenderMeshesChanged = null;
        }
      }
    }

    #endregion

  }

  public abstract class Provider
  {
    internal int m_runtime_serial_number = 0;
    static int m_current_serial_number = 1;
    static readonly Dictionary<int, Provider> m_all_providers = new Dictionary<int, Provider>();

    public Provider()
    {
    }

    public abstract String Name { get; }

    protected IntPtr CreateCppObject(Guid pluginId)
    {
      Type t = GetType();
      Guid providerId = t.GUID;
      string name = String.Empty;

      m_runtime_serial_number = m_current_serial_number++;
      
      return UnsafeNativeMethods.CRhCmnCRMProvider_New(m_runtime_serial_number, providerId, Name, pluginId);
    }

    public static void RegisterProviders(System.Reflection.Assembly assembly, System.Guid pluginId)
    {
      Rhino.PlugIns.PlugIn plugin = Rhino.PlugIns.PlugIn.GetLoadedPlugIn(pluginId);
      if (plugin == null)
        return;

      Type[] exported_types = assembly.GetExportedTypes();

      if (exported_types != null)
      {
        List<Type> provider_types = new List<Type>();
        for (int i = 0; i < exported_types.Length; i++)
        {
          Type t = exported_types[i];
          if (!t.IsAbstract && t.IsSubclassOf(typeof(Rhino.Render.CustomRenderMesh.Provider)) && t.GetConstructor(new Type[] { }) != null)
            provider_types.Add(t);
        }

        if (provider_types.Count == 0)
          return;

        RdkPlugIn rdk_plugin = RdkPlugIn.GetRdkPlugIn(plugin);
        if (rdk_plugin == null)
          return;

        foreach (Type t in provider_types)
        {
          Provider p = System.Activator.CreateInstance(t) as Provider;

          IntPtr pCppObject = p.CreateCppObject(pluginId);
          if (pCppObject != IntPtr.Zero)
          {
            //rdk_plugin.AddRegisteredCRMProvider(p);
            m_all_providers.Add(p.m_runtime_serial_number, p);
            UnsafeNativeMethods.Rdk_RegisterCRMProvider(pCppObject);
          }
        }

      }
      return;
    }

    /// <summary>
    /// Determines if custom render meshes will be built for a particular object.
    /// </summary>
    /// <param name="vp">The viewport being rendered.</param>
    /// <param name="obj">The Rhino object of interest.</param>
    /// <param name="requestingPlugIn">UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType">Type of mesh to build.</param>
    /// <returns>true if custom meshes will be built.</returns>
    public abstract bool WillBuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType);

    /// <summary>
    /// Returns a bounding box for the custom render meshes for the given object.
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="obj">the Rhino object of interest.</param>
    /// <param name="requestingPlugIn">UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType">ype of mesh to build.</param>
    /// <returns>A bounding box value.</returns>
    public virtual Rhino.Geometry.BoundingBox BoundingBox(Rhino.DocObjects.ViewportInfo vp, Rhino.DocObjects.RhinoObject obj, Guid requestingPlugIn, MeshTypes meshType)
    {
      Geometry.Point3d min = new Geometry.Point3d();
      Geometry.Point3d max = new Geometry.Point3d();

      if (UnsafeNativeMethods.Rdk_RMPBoundingBoxImpl(m_runtime_serial_number, vp.ConstPointer(), obj.ConstPointer(), requestingPlugIn, (int)meshType, ref min, ref max))
      {
        return new Rhino.Geometry.BoundingBox(min, max);
      }
      else
      {
        return new Rhino.Geometry.BoundingBox();
      }
    }

    /// <summary>
    /// Build custom render mesh(es).
    /// </summary>
    /// <param name="vp">the viewport being rendered.</param>
    /// <param name="objMeshes">The meshes class to populate with custom meshes.</param>
    /// <param name="requestingPlugIn">UUID of the RDK plug-in requesting the meshes.</param>
    /// <param name="meshType">ype of mesh to build.</param>
    /// <returns></returns>
    public abstract bool BuildCustomMeshes(Rhino.DocObjects.ViewportInfo vp, ObjectMeshes objMeshes, Guid requestingPlugIn, MeshTypes meshType);


    #region callbacks
    internal delegate void CRMProviderDeleteThisCallback(int serialNumber);
    internal static CRMProviderDeleteThisCallback m_DeleteThis = OnDeleteRhCmnCRMProvider;
    static void OnDeleteRhCmnCRMProvider(int serialNumber)
    {
      try
      {
        Provider p = Provider.FromSerialNumber(serialNumber);
        if (p != null)
        {
          p.m_runtime_serial_number = -1;
          m_all_providers.Remove(serialNumber);
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }


    internal delegate int CRMProviderWillBuildCallback(int serialNumber, IntPtr pViewport, IntPtr pObject, Guid plugInId, int type);
    internal static CRMProviderWillBuildCallback m_WillBuild = OnWillBuild;
    static int OnWillBuild(int serialNumber, IntPtr pViewport, IntPtr pObject, Guid plugInId, int type)
    {
      try
      {
        Provider p = Provider.FromSerialNumber(serialNumber);
        if (p != null)
        {
          return p.WillBuildCustomMeshes(new Rhino.DocObjects.ViewportInfo(pViewport), DocObjects.RhinoObject.CreateRhinoObjectHelper(pObject), plugInId, (MeshTypes)type) ? 1 : 0;
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return 0;
    }

    internal delegate int CRMProviderBuildCallback(int serialNumber, IntPtr pViewport, IntPtr pCRM, Guid plugInId, int type);
    internal static CRMProviderBuildCallback m_Build = OnBuild;
    static int OnBuild(int serialNumber, IntPtr pViewport, IntPtr pCRM, Guid plugInId, int type)
    {
      try
      {
        Provider p = Provider.FromSerialNumber(serialNumber);
        if (p != null)
        {
          return p.BuildCustomMeshes(new Rhino.DocObjects.ViewportInfo(pViewport), new Render.CustomRenderMesh.ObjectMeshes(pCRM), plugInId, (MeshTypes)type) ? 1 : 0;
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return 0;
    }

    internal delegate int CRMProviderBBoxCallback(int serialNumber, IntPtr pViewport, IntPtr pObject, Guid plugInId, int type, ref Geometry.Point3d min, ref Geometry.Point3d max);
    internal static CRMProviderBBoxCallback m_BBox = OnBBox;
    static int OnBBox(int serialNumber, IntPtr pViewport, IntPtr pObject, Guid plugInId, int type, ref Geometry.Point3d min, ref Geometry.Point3d max)
    {
      try
      {
        Provider p = Provider.FromSerialNumber(serialNumber);
        if (p != null)
        {
          Geometry.BoundingBox bbox = p.BoundingBox(new Rhino.DocObjects.ViewportInfo(pViewport), DocObjects.RhinoObject.CreateRhinoObjectHelper(pObject), plugInId, (MeshTypes)type);
          min = bbox.Min;
          max = bbox.Max;
          return 1;
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
      return 0;
    }

    #endregion

    #region internals
    internal static Provider FromSerialNumber(int serial_number)
    {
      Provider rc = null;
      m_all_providers.TryGetValue(serial_number, out rc);
      return rc;
    }
    #endregion
  }
}

#endif
