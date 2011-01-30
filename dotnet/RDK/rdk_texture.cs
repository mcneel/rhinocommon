using System;
using System.Collections.Generic;
using Rhino.Geometry;

#if USING_RDK

namespace Rhino.Render
{
  public abstract class RenderTexture : RenderContent
  {
    protected RenderTexture()
    {
    }

    private void Construct(Guid plugin_id)
    {
      Type t = GetType();
      Guid render_engine = Guid.Empty;
      bool image_based = false;
      object[] attr = t.GetCustomAttributes(typeof(CustomRenderContentAttribute), false);
      if (attr != null && attr.Length > 0)
      {
        CustomRenderContentAttribute custom = attr[0] as CustomRenderContentAttribute;
        image_based = custom.ImageBased;
        render_engine = custom.RenderEngineId;
      }
      int category = 0;
      Guid type_id = t.GUID;
      m_pRenderContent = UnsafeNativeMethods.CRhCmnTexture_New(m_runtime_serial_number, image_based, render_engine, plugin_id, type_id, category);
/*
      System.Reflection.FieldInfo[] fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
      if (fields != null)
      {
        for (int i = 0; i < fields.Length; i++)
        {
          if (fields[i].FieldType.IsSubclassOf(typeof(Field)))
          {
            Field f = fields[i].GetValue(this) as Field;
            if (f != null)
              f.ConstructCppPointer(this);
          }
        }
      }
*/
    }

    protected System.Drawing.Icon Icon
    {
      get { return null; }
      set { }
    }

    // will be implemented in RenderContent class
    //public string Kind { get;}
    //public string FileExtension { get; }
    //public virtual UpdateDocumentTables(); // can this be an event?
    //public RenderContentStyle Style { get;} // DWORD BitFlags()

    // [later] virtual Bitmap CreateLibraryPreview(Size imageSize, PreviewSceneServer sceneServer);

    // will be implemented in RenderContent class
    // virtual eParamSerialMethod ParameterSerializationMethod (void) const;
    // virtual bool SetParameters (IRhRdk_XMLSection &section, eSetParamsContext context) const = 0;
    // virtual bool GetParameters (const IRhRdk_XMLSection &section, eGetParamsContext context)=0;
    // virtual eHarvested HarvestData (const CRhRdkContent &oldContent)

    // [later] virtual Bitmap CreatePreview(CRhRdkRenderPlugIn &plugIn, Size imageSize, eRhRdkRenderQuality qual, PreviewSceneServer pSceneServer)

    //protected virtual void AddUiSections() { }

    //public Transform LocalMappingTransform { get; }

    public virtual TextureEvaluator CreateEvaluator()
    {
      return null;
    }

    #region callbacks from c++
    internal delegate IntPtr NewTextureCallback(Guid type_id);
    internal static NewTextureCallback m_NewTexture = OnNewTexture;
    static IntPtr OnNewTexture(Guid type_id)
    {
      IntPtr rc = IntPtr.Zero;
      try
      {
        Guid plugin_id;
        Type t = RdkPlugIn.GetRenderTextureType(type_id, out plugin_id);
        if( t != null && plugin_id!=Guid.Empty )
        {
          RenderTexture texture = System.Activator.CreateInstance(t) as RenderTexture;
          texture.Construct(plugin_id);
          rc = texture.NonConstPointer();
        }
      }
      catch
      {
        rc = IntPtr.Zero;
      }
      return rc;
    }

    internal delegate IntPtr GetNewTextureEvaluatorCallback(int serial_number);
    internal static GetNewTextureEvaluatorCallback m_NewTextureEvaluator = OnNewTextureEvaluator;
    static IntPtr OnNewTextureEvaluator(int serial_number)
    {
      IntPtr rc = IntPtr.Zero;
      try
      {
        RenderTexture texture = RenderContent.FromSerialNumber(serial_number) as RenderTexture;
        if (texture != null)
        {
          TextureEvaluator eval = texture.CreateEvaluator();
          if (eval != null)
          {
            rc = eval.NonConstPointer();
          }
        }
      }
      catch
      {
        rc = IntPtr.Zero;
      }
      return rc;
    }
    #endregion
  }

  public class TextureEvaluator : IDisposable
  {
    protected TextureEvaluator()
    {
      // This constructor is being called because we have a custom .NET subclass
      m_runtime_serial_number = m_serial_number_counter++;
      m_pRhRdkTextureEvaluator = UnsafeNativeMethods.CRhCmnRdkTextureEvaluator_New(m_runtime_serial_number);
      m_all_custom_evaluators.Add(this);
    }

    internal static TextureEvaluator FromPointer(IntPtr pTextureEvaluator)
    {
      if (pTextureEvaluator == IntPtr.Zero) return null;
      int serial_number = UnsafeNativeMethods.CRhCmnRdkTextureEvaluator_IsRhCmnEvaluator(pTextureEvaluator);
      return serial_number > 0 ? FromSerialNumber(serial_number) : new TextureEvaluator(pTextureEvaluator);
    }
    private TextureEvaluator(IntPtr pTextureEvaluator)
    {
      // Could be a texture evaluator from anywhere
      m_pRhRdkTextureEvaluator = pTextureEvaluator;
      // serial number stays zero and this is not added to the custom evaluator list
    }

    public virtual Rhino.Display.Color4f GetColor(Rhino.Geometry.Point3d uvw, Rhino.Geometry.Vector3d duvwdx, Rhino.Geometry.Vector3d duvwdy)
    {
      if (this.m_runtime_serial_number > 0)
        return Rhino.Display.Color4f.Empty;
      IntPtr pConstThis = ConstPointer();
      Rhino.Display.Color4f rc = new Rhino.Display.Color4f();

      if (!UnsafeNativeMethods.Rdk_TextureEvaluator_GetColor(pConstThis, uvw, duvwdx, duvwdy, ref rc))
        return Rhino.Display.Color4f.Empty;
      return rc;
    }

    internal delegate int GetColorCallback(int serial_number, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy, ref Rhino.Display.Color4f color);
    internal static GetColorCallback m_GetColor = OnGetColor;
    static int OnGetColor(int serial_number, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy, ref Rhino.Display.Color4f color)
    {
      int rc = 0;
      TextureEvaluator eval = FromSerialNumber(serial_number);
      if (eval != null)
      {
        Rhino.Display.Color4f c = eval.GetColor(uvw, duvwdx, duvwdy);
        if (c != Rhino.Display.Color4f.Empty)
        {
          color = c;
          rc = 1;
        }
      }
      return rc;
    }


    #region pointer tracking
    IntPtr m_pRhRdkTextureEvaluator = IntPtr.Zero;
    readonly int m_runtime_serial_number = 0;
    static int m_serial_number_counter = 1;
    static readonly List<TextureEvaluator> m_all_custom_evaluators = new List<TextureEvaluator>();

    static TextureEvaluator FromSerialNumber(int serial_number)
    {
      int index = serial_number - 1;
      if (index >= 0 && index < m_all_custom_evaluators.Count)
      {
        TextureEvaluator rc = m_all_custom_evaluators[index];
        if (rc != null && rc.m_runtime_serial_number == serial_number)
          return rc;
      }
      return null;
    }
    IntPtr ConstPointer()
    {
      return m_pRhRdkTextureEvaluator;
    }
    internal IntPtr NonConstPointer()
    {
      return m_pRhRdkTextureEvaluator;
    }
    #endregion

    #region disposable implementation
    ~TextureEvaluator()
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
      if (IntPtr.Zero != m_pRhRdkTextureEvaluator)
      {
        UnsafeNativeMethods.Rdk_TextureEvaluator_DeleteThis(m_pRhRdkTextureEvaluator);
        m_pRhRdkTextureEvaluator = IntPtr.Zero;
      }
    }
    #endregion
  }
}

#endif