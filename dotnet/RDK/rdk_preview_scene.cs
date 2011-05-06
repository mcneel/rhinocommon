using System;
using System.Diagnostics;
using System.Collections.Generic;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public class PreviewScene
  {
    private IntPtr m_pSceneServer;
    internal PreviewScene(IntPtr pSceneServer)
    {
      m_pSceneServer = pSceneServer;
    }

    internal void Initialize()
    {
      IntPtr pSceneServer = m_pSceneServer;

      if (pSceneServer != IntPtr.Zero)
      {
        //Pull in the list of objects
        m_scene_objects = new List<SceneObject>();

        UnsafeNativeMethods.Rdk_SceneServer_ResetObjectEnum(pSceneServer);

        IntPtr pObject = UnsafeNativeMethods.Rdk_SceneServer_NextObject(pSceneServer);
        while (pObject != IntPtr.Zero)
        {
          Rhino.Geometry.Mesh mesh = new Rhino.Geometry.Mesh();

          IntPtr pMaterial = UnsafeNativeMethods.Rdk_SceneServer_ObjectDetails(pObject, mesh.NonConstPointer());
          if (pMaterial != IntPtr.Zero)
          {
            SceneObject o = new SceneObject(mesh, RenderContent.FromPointer(pMaterial) as RenderMaterial);
            m_scene_objects.Add(o);
          }

          pObject = UnsafeNativeMethods.Rdk_SceneServer_NextObject(pSceneServer);
        }

        //Now get the lights
        m_scene_lights = new List<Rhino.Geometry.Light>();

        UnsafeNativeMethods.Rdk_SceneServer_ResetLightEnum(pSceneServer);

        IntPtr pLight = UnsafeNativeMethods.Rdk_SceneServer_NextLight(pSceneServer);
        while (pLight != IntPtr.Zero)
        {
          Rhino.Geometry.Light light = new Rhino.Geometry.Light();
          UnsafeNativeMethods.Rdk_SceneServer_LightDetails(pLight, light.NonConstPointer());

          m_scene_lights.Add(light);

          pLight = UnsafeNativeMethods.Rdk_SceneServer_NextLight(pSceneServer);
        }

        //And then fill in the blanks
        IntPtr pEnvironment = UnsafeNativeMethods.Rdk_SceneServer_Environment(pSceneServer);
        if (pEnvironment != IntPtr.Zero)
        {
          m_environment = RenderContent.FromPointer(pEnvironment) as RenderEnvironment;
        }
        else
        {
          m_environment = null;
        }

        m_content_instance_id = UnsafeNativeMethods.Rdk_SceneServer_InstanceId(pSceneServer);
        m_sig = UnsafeNativeMethods.Rdk_SceneServer_Signature(pSceneServer);

        //Just the view left...

        m_view = new Rhino.DocObjects.ViewportInfo();
        UnsafeNativeMethods.Rdk_SceneServer_View(pSceneServer, m_view.NonConstPointer());
      }

      m_pSceneServer = IntPtr.Zero;
    }

    private Int32 m_sig = 0;

    /// <summary>
    /// Call this function to return the unique signature for this scene
    /// </summary>
    public Int32 Signature
    {
      get { Initialize(); return m_sig; }
    }

    private Guid m_content_instance_id = Guid.Empty;

    /// <summary>
    /// The content being previewed
    /// </summary>
    public Rhino.Render.RenderContent PreviewContent
    {
      get { Initialize(); return Rhino.Render.RenderContent.FromInstanceId(m_content_instance_id); }
    }

    Rhino.Render.RenderEnvironment m_environment = null;

    /// <summary>
    /// The environment that the previewed object is rendered in
    /// </summary>
    public Rhino.Render.RenderEnvironment Environment
    {
      get { Initialize(); return m_environment; }
    }

    //TODO_RDK Get information about the view.
	  //virtual bool GetView(ON_Viewport& view) const = 0;
    
    public class SceneObject
    {
      internal SceneObject(Rhino.Geometry.Mesh mesh, Rhino.Render.RenderMaterial material)
      {
        m_mesh = mesh;
        m_material = material;
      }

      public Rhino.Geometry.Mesh Mesh             { get { return m_mesh; } }
      public Rhino.Render.RenderMaterial Material { get { return m_material; } }

      private Rhino.Geometry.Mesh m_mesh;
      private Rhino.Render.RenderMaterial m_material;
    }

    private List<SceneObject> m_scene_objects = null;
    public List<SceneObject> Objects
    {
      get
      {
        Initialize();
        return m_scene_objects;
      }
    }

    private List<Rhino.Geometry.Light> m_scene_lights = null;

    public List<Rhino.Geometry.Light> Lightts
    {
      get
      {
        Initialize();
        return m_scene_lights;
      }
    }

    private Rhino.DocObjects.ViewportInfo m_view = null;

    public Rhino.DocObjects.ViewportInfo View
    {
      get
      {
        Initialize();
        return m_view;
      }
    }
  }
}

#endif

 
