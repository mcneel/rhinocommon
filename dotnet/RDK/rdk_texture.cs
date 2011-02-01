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

      internal RenderTexture(IntPtr pRenderTexture)
    {
      // Could be from anywhere
        m_pRenderContent = pRenderTexture;
      // serial number stays zero and this is not added to the custom content list
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

      //for (int i = 0; i < m_fields.Count; i++)
      //{
      //    m_fields[i].CreateCppPointer(this);
      //}

      System.Reflection.FieldInfo[] fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic); ;//System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
        
      if (fields != null)
      {
        for (int i = 0; i < fields.Length; i++)
        {
          if (fields[i].FieldType.IsSubclassOf(typeof(Field)))
          {
            Field f = fields[i].GetValue(this) as Field;
            if (f != null)
              f.CreateCppPointer(this, fields[i].IsPublic);
          }
        }
      }
    }

    public Rhino.Geometry.Transform LocalMappingTransform
    {
          get
          {
              Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform();
              UnsafeNativeMethods.Rdk_Texture_LocalMappingTransform(ConstPointer(), ref xform);
              return xform;
          }
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
        IntPtr pTE = UnsafeNativeMethods.Rdk_RenderTexture_NewTextureEvaluator(ConstPointer());
        if (pTE != IntPtr.Zero)
        {
            TextureEvaluator te = new TextureEvaluator(pTE);
            return te;
        }
      return null;
    }

    #region callbacks from c++

    public virtual void SimulateTexture(Rhino.Render.SimulatedTexture sim, bool bForDataOnly)
    {
        UnsafeNativeMethods.Rdk_CallSimulateTextureBase(NonConstPointer(), sim.ConstPointer(), bForDataOnly);
    }

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
          //RenderContent.m_fields.Clear();
          RenderTexture texture = System.Activator.CreateInstance(t) as RenderTexture;
          texture.Construct(plugin_id);
          rc = texture.NonConstPointer();
        }
      }
      catch
      {
        rc = IntPtr.Zero;
      }
      //RenderContent.m_fields.Clear();
      return rc;
    }

    internal delegate void GetSimulateTextureCallback(int serial_number, IntPtr p, int bDataOnly);
    internal static GetSimulateTextureCallback m_SimulateTexture = OnSimulateTexture;
    static void OnSimulateTexture(int serial_number, IntPtr pSim, int bDataOnly)
    {
      try
      {
        RenderTexture texture = RenderContent.FromSerialNumber(serial_number) as RenderTexture;
        if (texture != null)
        {
            if (pSim != IntPtr.Zero)
            {
                SimulatedTexture sim = new SimulatedTexture(pSim);
                texture.SimulateTexture(sim, 1==bDataOnly);
            }
        }
      }
      catch
      {
      }
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
    internal TextureEvaluator(IntPtr pTextureEvaluator)
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

    internal delegate void OnDeleteThisCallback(int serial_number);
    internal static OnDeleteThisCallback m_OnDeleteThis = OnDeleteThis;
    static void OnDeleteThis(int serial_number)
    {
      TextureEvaluator eval = FromSerialNumber(serial_number);
      if (eval != null)
      {
        eval.m_pRhRdkTextureEvaluator = IntPtr.Zero;
      }
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

//#define RDK_TEX_2COL_COLOR1             L"color-one"
//#define RDK_TEX_2COL_COLOR2             L"color-two"
//#define RDK_TEX_2COL_SWAP_COLORS        L"swap-colors"
//#define RDK_TEX_2COL_SUPERSAMPLE        L"super-sample"
//#define RDK_TEX_2COL_TEXTURE_ON1        L"texture-on-one"
//#define RDK_TEX_2COL_TEXTURE_ON2        L"texture-on-two"
//#define RDK_TEX_2COL_TEXTURE_AMOUNT1    L"texture-amount-one"
//#define RDK_TEX_2COL_TEXTURE_AMOUNT2    L"texture-amount-two"
//#define RDK_TEX_2COL_TILE				L"tile"*/

  public abstract class TwoColorRenderTexture : RenderTexture
  {

      public override sealed void AddUISections()
      {
          UnsafeNativeMethods.Rdk_Texture_AddTwoColorSection(NonConstPointer());
          AddAdditionalUISections();
          base.AddUISections();
      }

      protected abstract void AddAdditionalUISections();

      protected ColorField m_color1 = new ColorField("color-one", "Color 1", Rhino.Display.Color4f.Black);
      protected ColorField m_color2 = new ColorField("color-two", "Color 2", Rhino.Display.Color4f.White);

      protected BoolField m_texture1_on = new BoolField("texture-on-one", "Texture1 On", true);
      protected BoolField m_texture2_on = new BoolField("texture-on-two", "Texture2 On", true);

      protected DoubleField m_texture1_amount = new DoubleField("texture-amount-one", "Texture1 Amt", 1.0);
      protected DoubleField m_texture2_amount = new DoubleField("texture-amount-two", "Texture2 Amt", 1.0);

      protected BoolField m_swap_colors = new BoolField("swap-colors", "Swap Colors", false);
      protected BoolField m_super_sample = new BoolField("super-sample", "Super sample", false);

      public Rhino.Display.Color4f Color1
      {
          get { return m_color1.Value; }
          set { m_color1.Value = value; }
      }
      public Rhino.Display.Color4f Color2
      {
          get { return m_color2.Value; }
          set { m_color2.Value = value; }
      }
      public bool Texture1On
      {
          get { return m_texture1_on.Value; }
          set { m_texture1_on.Value = value; }
      }
      public bool Texture2On
      {
          get { return m_texture2_on.Value; }
          set { m_texture2_on.Value = value; }
      }
      public double Texture1Amount
      {
          get { return m_texture1_amount.Value; }
          set { m_texture1_amount.Value = value; }
      }
      public double Texture2Amount
      {
          get { return m_texture2_amount.Value; }
          set { m_texture2_amount.Value = value; }
      }
      public bool SwapColors
      {
          get { return m_swap_colors.Value; }
          set { m_swap_colors.Value = value; }
      }
      public bool SuperSample
      {
          get { return m_super_sample.Value; }
          set { m_super_sample.Value = value; }
      }
  }

  // DO NOT make public
  class NativeRenderTexture : RenderTexture
  {
    public NativeRenderTexture(IntPtr pRenderTexture)
        : base(pRenderTexture)
    {
    }

    public override string Name { get { return "TODO"; } }
    public override string Description { get { return "TODO"; } }
  }
}

#endif