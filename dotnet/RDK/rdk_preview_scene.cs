#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RDK_CHECKED

namespace Rhino.Render
{
  /// <summary>Quality levels when creating preview images</summary>
  public enum PreviewSceneQuality : int
  {
    /// <summary>Very fast preview. Typically using the internal OpenGL preview generator.</summary>
    RealtimeQuick = -1,
    /// <summary>Low quality rendering for quick preview.</summary>
    RefineFirstPass = 1,
    /// <summary>Medium quality rendering for intermediate preview.</summary>
    RefineSecondPass = 2,
    /// <summary>Full quality rendering (quality comes from user settings)</summary>
    RefineThirdPass = 3,
  }

  /// <summary>Used in RenderPlugIn virtual CreatePreview function</summary>
  public class CreatePreviewEventArgs : EventArgs
  {
    IntPtr m_pSceneServer;
    readonly Rhino.Drawing.Size m_preview_size;
    readonly PreviewSceneQuality m_quality;
    int m_sig;
    Rhino.DocObjects.ViewportInfo m_viewport;


    internal CreatePreviewEventArgs(IntPtr pSceneServer, Rhino.Drawing.Size preview_size, PreviewSceneQuality quality)
    {
      m_pSceneServer = pSceneServer;
      m_preview_size = preview_size;
      m_quality = quality;
    }

    /// <summary>
    /// Pixel size of the image that is being requested for the preview scene
    /// </summary>
    public Rhino.Drawing.Size PreviewImageSize
    {
      get { return m_preview_size; }
    }

    /// <summary>
    /// Quality of the preview image that is being requested for the preview scene
    /// </summary>
    public PreviewSceneQuality Quality
    {
      get { return m_quality; }
    }

    /// <summary>
    /// Initially null.  If this image is set, then this image will be used for
    /// the preview.  If never set, the default internal simulation preview will
    /// be used.
    /// </summary>
    public Rhino.Drawing.Bitmap PreviewImage { get; set; }

    /// <summary>
    /// Get set by Rhino if the preview generation should be canceled for this 
    /// </summary>
    public bool Cancel { get; internal set; }

    void Initialize()
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

        m_viewport = new Rhino.DocObjects.ViewportInfo();
        UnsafeNativeMethods.Rdk_SceneServer_View(pSceneServer, m_viewport.NonConstPointer());
      }

      m_pSceneServer = IntPtr.Zero;
    }

    /// <summary>Unique Id for this scene.</summary>
    public int Id
    {
      get
      {
        Initialize();
        return m_sig;
      }
    }

    private Guid m_content_instance_id = Guid.Empty;

    /// <summary>The content being previewed.</summary>
    public Rhino.Render.RenderContent PreviewContent
    {
      get
      {
        Initialize();
        var doc = RhinoDoc.ActiveDoc;
        return Rhino.Render.RenderContent.FromId(doc, m_content_instance_id);
      }
    }

    Rhino.Render.RenderEnvironment m_environment;

    /// <summary>
    /// The environment that the previewed object is rendered in.
    /// </summary>
    public Rhino.Render.RenderEnvironment Environment
    {
      get
      {
        Initialize();
        return m_environment;
      }
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

      public Rhino.Geometry.Mesh Mesh { get { return m_mesh; } }
      public Rhino.Render.RenderMaterial Material { get { return m_material; } }

      private readonly Rhino.Geometry.Mesh m_mesh;
      private readonly Rhino.Render.RenderMaterial m_material;
    }

    private List<SceneObject> m_scene_objects;
    public List<SceneObject> Objects
    {
      get
      {
        Initialize();
        return m_scene_objects;
      }
    }

    private List<Rhino.Geometry.Light> m_scene_lights;

    public List<Rhino.Geometry.Light> Lights
    {
      get
      {
        Initialize();
        return m_scene_lights;
      }
    }

    public Rhino.DocObjects.ViewportInfo Viewport
    {
      get
      {
        Initialize();
        return m_viewport;
      }
    }
  }

  public class CreateTexture2dPreviewEventArgs : EventArgs
  {
    readonly Rhino.Drawing.Size m_preview_size;
    readonly RenderTexture m_render_texture;

    internal CreateTexture2dPreviewEventArgs(RenderTexture texture, Rhino.Drawing.Size size)
    {
      m_preview_size = size;
      m_render_texture = texture;
    }

    RenderTexture Texture { get { return m_render_texture; } }

    /// <summary>
    /// Pixel size of the image that is being requested for the preview scene
    /// </summary>
    public Rhino.Drawing.Size PreviewImageSize
    {
      get { return m_preview_size; }
    }

    /// <summary>
    /// Initially null.  If this image is set, then this image will be used for
    /// the preview.  If never set, the default internal simulation preview will
    /// be used.
    /// </summary>
    public Rhino.Drawing.Bitmap PreviewImage { get; set; }
  }
}

#endif

 
