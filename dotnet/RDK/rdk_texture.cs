#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rhino.Geometry;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public abstract class RenderTexture : RenderContent
  {
    /// <summary>
    /// Gets the transformation that can be applied to the UVW vector to convert it
    /// from normalized texture space into locally mapped space (ie - with repeat,
    /// offset and rotation applied.)
    /// </summary>
    public Rhino.Geometry.Transform LocalMappingTransform
    {
      get
      {
        Rhino.Geometry.Transform xform = new Rhino.Geometry.Transform();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_RenderTexture_LocalMappingTransform(pConstThis, ref xform);
        return xform;
      }
    }

    /// <summary>
    /// Constructs a texture evaluator. This is an independent lightweight object
    /// capable of evaluating texture color throughout uvw space. May be called
    /// from within a rendering shade pipeline.
    /// </summary>
    /// <returns>A texture evaluator instance.</returns>
    public virtual TextureEvaluator CreateEvaluator()
    {
      if (IsNativeWrapper())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pTE = UnsafeNativeMethods.Rdk_RenderTexture_NewTextureEvaluator(pConstThis);
        if (pTE != IntPtr.Zero)
        {
          TextureEvaluator te = new TextureEvaluator(pTE);
          return te;
        }
      }
      return null;
    }

    public virtual void SimulateTexture(ref SimulatedTexture simulation, bool isForDataOnly)
    {
      if (IsNativeWrapper())
      {
        UnsafeNativeMethods.Rdk_RenderTexture_SimulateTexture(NonConstPointer(), simulation.ConstPointer(), isForDataOnly);
      }
      else
      {
        UnsafeNativeMethods.Rdk_RenderTexture_CallSimulateTextureBase(NonConstPointer(), simulation.ConstPointer(), isForDataOnly);
      }
    }

    #region callbacks from c++

    internal static NewRenderContentCallbackEvent m_NewTextureCallback = OnNewTexture;
    static IntPtr OnNewTexture(Guid typeId)
    {
      var renderContent = NewRenderContent(typeId, typeof(RenderTexture));
      return (null == renderContent ? IntPtr.Zero : renderContent.NonConstPointer());
    }

    internal delegate void SimulateTextureCallback(int serial_number, IntPtr p, int bDataOnly);
    internal static SimulateTextureCallback m_SimulateTexture = OnSimulateTexture;
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
            texture.SimulateTexture(ref sim, 1 == bDataOnly);
          }
        }
      }
      catch (Exception exception)
      {
        Runtime.HostUtils.ExceptionReport(exception);
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
      if (m_runtime_serial_number > 0)
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
    readonly int m_runtime_serial_number;
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

  public abstract class TwoColorRenderTexture : RenderTexture
  {

    protected override sealed void OnAddUserInterfaceSections()
    {
      UnsafeNativeMethods.Rdk_RenderTexture_AddTwoColorSection(NonConstPointer());
      AddAdditionalUISections();
      base.OnAddUserInterfaceSections();
    }

    protected abstract void AddAdditionalUISections();

    public TwoColorRenderTexture()
    {
      m_color1 = Fields.Add("color-one", Display.Color4f.Black, UI.LOC.STR("Color 1"));
      m_color2 = Fields.Add("color-two", Display.Color4f.White, "Color 2");

      m_texture1On = Fields.Add("texture-on-one", true, "Texture1 On");
      m_texture2On = Fields.Add("texture-on-two", true, "Texture2 On");

      m_texture1Amount = Fields.Add("texture-amount-one", 1.0, "Texture1 Amt");
      m_texture2Amount = Fields.Add("texture-amount-two", 1.0, "Texture2 Amt");

      m_swapColors = Fields.Add("swap-colors", false, "Swap Colors");
      m_superSample = Fields.Add("super-sample", false, "Super sample");
    }

    private readonly Fields.Color4fField m_color1;
    private readonly Fields.Color4fField m_color2;

    private readonly Fields.BoolField m_texture1On;
    private readonly Fields.BoolField m_texture2On;

    private readonly Fields.DoubleField m_texture1Amount;
    private readonly Fields.DoubleField m_texture2Amount;

    private readonly Fields.BoolField m_swapColors;
    private readonly Fields.BoolField m_superSample;

    public Display.Color4f Color1
    {
      get { return m_color1.Value; }
      set { m_color1.Value = value; }
    }
    public Display.Color4f Color2
    {
      get { return m_color2.Value; }
      set { m_color2.Value = value; }
    }
    public bool Texture1On
    {
      get { return m_texture1On.Value; }
      set { m_texture1On.Value = value; }
    }
    public bool Texture2On
    {
      get { return m_texture2On.Value; }
      set { m_texture2On.Value = value; }
    }
    public double Texture1Amount
    {
      get { return m_texture1Amount.Value; }
      set { m_texture1Amount.Value = value; }
    }
    public double Texture2Amount
    {
      get { return m_texture2Amount.Value; }
      set { m_texture2Amount.Value = value; }
    }
    public bool SwapColors
    {
      get { return m_swapColors.Value; }
      set { m_swapColors.Value = value; }
    }
    public bool SuperSample
    {
      get { return m_superSample.Value; }
      set { m_superSample.Value = value; }
    }
  }


  #region native wrapper
  // DO NOT make public
  internal class NativeRenderTexture : RenderTexture
  {
    readonly Guid m_native_instance_id = Guid.Empty;
    public NativeRenderTexture(IntPtr pRenderContent)
    {
      m_native_instance_id = UnsafeNativeMethods.Rdk_RenderContent_InstanceId(pRenderContent);
    }
    public override string TypeName { get { return GetString(StringIds.TypeName); } }
    public override string TypeDescription { get { return GetString(StringIds.TypeDescription); } }
    internal override IntPtr ConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return pContent;
    }
    internal override IntPtr NonConstPointer()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return pContent;
    }
  }
  #endregion
}

#endif