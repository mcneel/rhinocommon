#pragma warning disable 1591
using System;
using System.Collections.Generic;
using Rhino.Geometry;
using Rhino.Runtime;

#if RDK_CHECKED

namespace Rhino.Render
{
  public enum TextureProjectionMode : int
  {
    MappingChannel = 0,
    View = 1,
    Wcs = 2,
    EnvironmentMap = 3,  // Now means "environment mapped" - call "EnvironmentMappingMode" to get specific projection for this texture.
    WcsBox = 4,
    Screen = 5,
  }

  public enum TextureWrapType : int
  {
    Clamped = 0,
    Repeating = 1,
  }

  public enum TextureEnvironmentMappingMode : int
  {
    Automatic = 0,
    /// <summary>Equirectangular projection</summary>
    Spherical = 1,
    /// <summary>Mirrorball</summary>
    EnvironmentMap = 2,
    Box = 3,
    LightProbe = 5,
    Cube = 6,
    VerticalCrossCube = 7,
    HorizontalCrossCube = 8,
    Hemispherical = 9,
  }

  public abstract class RenderTexture : RenderContent
  {
    /// <summary>
    /// Constructs a new basic texture from a SimulatedTexture.
    /// </summary>
    /// <param name="texture">The texture to create the basic texture from.</param>
    /// <returns>A new render texture.</returns>
    public static RenderTexture NewBitmapTexture(SimulatedTexture texture)
    {
      IntPtr ptr_const_texture = texture == null ? IntPtr.Zero : texture.ConstPointer();
      NativeRenderTexture new_texture = FromPointer(UnsafeNativeMethods.Rdk_Globals_NewBasicTexture(ptr_const_texture)) as NativeRenderTexture;
      if (new_texture != null)
        new_texture.AutoDelete = true;
      return new_texture;
    }

    /// <summary>
    /// Gets the transformation that can be applied to the UVW vector to convert it
    /// from normalized texture space into locally mapped space (ie - with repeat,
    /// offset and rotation applied.)
    /// </summary>
    public Transform LocalMappingTransform
    {
      get
      {
        Transform xform = new Transform();
        IntPtr ptr_const_this = ConstPointer();
        UnsafeNativeMethods.Rdk_RenderTexture_LocalMappingTransform(ptr_const_this, ref xform);
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
        IntPtr ptr_const_this = ConstPointer();
        IntPtr ptr_texture_evaluator = UnsafeNativeMethods.Rdk_RenderTexture_NewTextureEvaluator(ptr_const_this);
        if (ptr_texture_evaluator != IntPtr.Zero)
        {
          TextureEvaluator te = new TextureEvaluator(ptr_texture_evaluator);
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

    public virtual TextureProjectionMode GetProjectionMode()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, PROJECTION_MODE, true);
      return (TextureProjectionMode)result;
    }

    public virtual void SetProjectionMode(TextureProjectionMode value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, PROJECTION_MODE, true, (int) value, (int) changeContext);
    }

    public virtual TextureWrapType GetWrapType()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, WRAP_TYPE_MODE, true);
      return (TextureWrapType)result;
    }

    public virtual void SetWrapType(TextureWrapType value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, WRAP_TYPE_MODE, true, (int) value, (int) changeContext);
    }

    public virtual int GetMappingChannel()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, MAPPING_CHANNEL_MODE, true);
      return result;
    }

    public virtual void SetMappingChannel(int value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, MAPPING_CHANNEL_MODE, true, value, (int)changeContext);
    }

    public virtual bool GetRepeatLocked()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, REPEAT_LOCKED_MODE, true);
      return (result != 0);
    }

    public virtual void SetRepeatLocked(bool value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, REPEAT_LOCKED_MODE, true, value ? 1 : 0, (int)changeContext);
    }

    public virtual bool GetOffsetLocked()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, OFFSET_LOCKED_MODE, true);
      return (result != 0);
    }

    public virtual void SetOffsetLocked(bool value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, OFFSET_LOCKED_MODE, true, value ? 1 : 0, (int)changeContext);
    }

    public virtual bool GetPreviewIn3D()
    {
      var const_pointer = ConstPointer();
      var result = UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(const_pointer, PREVIEW_IN_3D_MODE, true);
      return (result != 0);
    }

    public virtual void SetPreviewIn3D(bool value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, PREVIEW_IN_3D_MODE, true, value ? 1 : 0, (int)changeContext);
    }

    /// <summary>
    /// Get repeat value across UVW space. If the projection type is WCS or
    /// other type specified in model units, then this is the repeat across 1
    /// meter of the model.
    /// </summary>
    /// <returns></returns>
    public virtual Vector3d GetRepeat()
    {
      var const_pointer = ConstPointer();
      var vector = Vector3d.Unset;
      UnsafeNativeMethods.Rdk_RenderTexture_GetVirtual3dVector(const_pointer, REPEAT_MODE, true, ref vector);
      return vector;
    }

    /// <summary>
    /// Set repeat value across UVW space. If the projection type is WCS or
    /// other type specified in model units, then this is the repeat across 1
    /// meter of the model.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="changeContext"></param>
    public virtual void SetRepeat(Vector3d value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtual3dVector(pointer, REPEAT_MODE, true, value, (int)changeContext);
    }

    /// <summary>
    /// Get offset value across UVW space. If the projection type is WCS or
    /// other type specified in model units, then this is the offset in meters.
    /// </summary>
    /// <returns></returns>
    public virtual Vector3d GetOffset()
    {
      var const_pointer = ConstPointer();
      var vector = Vector3d.Unset;
      UnsafeNativeMethods.Rdk_RenderTexture_GetVirtual3dVector(const_pointer, OFFSET_MODE, true, ref vector);
      return vector;
    }

    /// <summary>
    /// Set offset value across UVW space. If the projection type is WCS or
    /// other type specified in model units, then this is the offset in meters.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="changeContext"></param>
    public virtual void SetOffset(Vector3d value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtual3dVector(pointer, OFFSET_MODE, true, value, (int)changeContext);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual Vector3d GetRotation()
    {
      var const_pointer = ConstPointer();
      var vector = Vector3d.Unset;
      UnsafeNativeMethods.Rdk_RenderTexture_GetVirtual3dVector(const_pointer, ROTATION_MODE, true, ref vector);
      return vector;
    }

    public virtual void SetRotation(Vector3d value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetVirtual3dVector(pointer, ROTATION_MODE, true, value, (int)changeContext);
    }

    public TextureEnvironmentMappingMode GetInternalEnvironmentMappingMode()
    {
      var const_pointer = ConstPointer();
      return (TextureEnvironmentMappingMode)UnsafeNativeMethods.Rdk_RenderTexture_GetIntValue(const_pointer, INTERNAL_ENVIRONMENT_MAPPING_MODE, (int)TextureEnvironmentMappingMode.Automatic);
    }

    public TextureEnvironmentMappingMode GetEnvironmentMappingMode()
    {
      var const_pointer = ConstPointer();
      return (TextureEnvironmentMappingMode)UnsafeNativeMethods.Rdk_RenderTexture_GetIntValue(const_pointer, ENVIRONMENT_MAPPING_MODE, (int)TextureEnvironmentMappingMode.Automatic);
    }

    public void SetEnvironmentMappingMode(TextureEnvironmentMappingMode value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetIntValue(pointer, ENVIRONMENT_MAPPING_MODE, (int)value, (int)changeContext);
    }

    public bool GetPreviewLocalMapping()
    {
      var const_pointer = ConstPointer();
      return (0 != UnsafeNativeMethods.Rdk_RenderTexture_GetIntValue(const_pointer, PREVIEW_LOCAL_MAPPING_MODE, 1));
    }

    public void SetPreviewLocalMapping(bool value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetIntValue(pointer, PREVIEW_LOCAL_MAPPING_MODE, value ? 1 : 0, (int) changeContext);
    }

    public bool GetDisplayInViewport()
    {
      var const_pointer = ConstPointer();
      return (0 != UnsafeNativeMethods.Rdk_RenderTexture_GetIntValue(const_pointer, DISPLAY_IN_VIEWPORT_MODE, 1));
    }

    public void SetDisplayInViewport(bool value, ChangeContexts changeContext)
    {
      var pointer = NonConstPointer();
      UnsafeNativeMethods.Rdk_RenderTexture_SetIntValue(pointer, DISPLAY_IN_VIEWPORT_MODE, value ? 1 : 0, (int)changeContext);
    }

    public bool IsHdrCapable()
    {
      var const_pointer = ConstPointer();
      return (0 != UnsafeNativeMethods.Rdk_RenderTexture_GetIntValue(const_pointer, IS_HDR_CAPABLE_MODE, 1));
    }

    public static bool GetEnvironmentMappingProjection(TextureEnvironmentMappingMode mode, Vector3d reflectionVector, out float u, out float v)
    {
      u = v = 0;
      return UnsafeNativeMethods.Rdk_RenderTexture_EnvironmentMappingProjection((int) mode, reflectionVector, ref u, ref v);
    }

    public static Point3d GetWcsBoxMapping(Point3d worldXyz, Vector3d normal)
    {
      var value = Point3d.Unset;
      UnsafeNativeMethods.Rdk_RenderTexture_WcsBoxMapping(worldXyz, normal, ref value);
      return value;
    }
    #region callbacks from c++

    internal static NewRenderContentCallbackEvent m_NewTextureCallback = OnNewTexture;
    static IntPtr OnNewTexture(Guid typeId)
    {
      var render_content = NewRenderContent(typeId, typeof(RenderTexture));
      return (null == render_content ? IntPtr.Zero : render_content.NonConstPointer());
    }

    internal delegate void SimulateTextureCallback(int serialNumber, IntPtr p, int bDataOnly);
    internal static SimulateTextureCallback m_SimulateTexture = OnSimulateTexture;
    static void OnSimulateTexture(int serialNumber, IntPtr pSim, int bDataOnly)
    {
      try
      {
        RenderTexture texture = FromSerialNumber(serialNumber) as RenderTexture;
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
        HostUtils.ExceptionReport(exception);
      }
    }

    // WORK IN PROGRESS, I will be using the rest of these shortly, just wanted to have
    // proof of concept checked so I am checking this in now.
    // VirtualIntValue properties
    const int PROJECTION_MODE = 0;
    const int MAPPING_CHANNEL_MODE = 1;
    const int WRAP_TYPE_MODE = 2;
    // bool values as int
    const int REPEAT_LOCKED_MODE = 3;
    const int OFFSET_LOCKED_MODE = 4;
    const int PREVIEW_IN_3D_MODE = 5;
    // VirtualVector3d properties
    const int REPEAT_MODE = 6;
    const int OFFSET_MODE = 7;
    const int ROTATION_MODE = 8;
    // Non virtual int properties
    const int ENVIRONMENT_MAPPING_MODE = 9;
    const int INTERNAL_ENVIRONMENT_MAPPING_MODE = 10;
    // Non virtual bool properties
    const int PREVIEW_LOCAL_MAPPING_MODE = 10;
    const int DISPLAY_IN_VIEWPORT_MODE = 11;
    const int IS_HDR_CAPABLE_MODE = 12; // (get only)

    internal delegate int GetVirtualIntCallback(int serialNumber, int propertyId, bool fromBaseClass);
    internal static GetVirtualIntCallback GetVirtualInt = OnGetVirtualInt;
    static int OnGetVirtualInt(int serialNumber, int propertyId, bool fromBaseClass)
    {
      try
      {
        var texture = FromSerialNumber(serialNumber) as RenderTexture;
        if (texture == null) return -1;
        if (fromBaseClass) return UnsafeNativeMethods.Rdk_RenderTexture_GetVirtualIntValue(texture.ConstPointer(), propertyId, true);
        switch (propertyId)
        {
          case PROJECTION_MODE:
            return (int)texture.GetProjectionMode();
          case WRAP_TYPE_MODE:
            return (int)texture.GetWrapType();
          case REPEAT_LOCKED_MODE:
            return texture.GetRepeatLocked() ? 1 : 0;
          case OFFSET_LOCKED_MODE:
            return texture.GetOffsetLocked() ? 1 : 0;
          case PREVIEW_IN_3D_MODE:
            return texture.GetPreviewIn3D() ? 1 : 0;
        }
        return -1;
      }
      catch (Exception exception)
      {
        HostUtils.ExceptionReport(exception);
      }
      return -1;
    }

    internal delegate void SetVirtualIntCallback(int serialNumber, int propertyId, bool callBaseClass, int value, int changeContext);
    internal static SetVirtualIntCallback SetVirtualInt = OnSetVirtualInt;
    static void OnSetVirtualInt(int serialNumber, int propertyId, bool callBaseClass, int value, int changeContext)
    {
      try
      {
        var texture = FromSerialNumber(serialNumber) as RenderTexture;
        if (texture == null) return;
        if (callBaseClass)
        {
          var pointer = texture.NonConstPointer();
          UnsafeNativeMethods.Rdk_RenderTexture_SetVirtualIntValue(pointer, propertyId, true, value, changeContext);
          return;
        }
        switch (propertyId)
        {
          case PROJECTION_MODE:
            texture.SetProjectionMode((TextureProjectionMode)value, (ChangeContexts)changeContext);
            break;
          case WRAP_TYPE_MODE:
            texture.SetWrapType((TextureWrapType)value, (ChangeContexts)changeContext);
            break;
          case REPEAT_LOCKED_MODE:
            texture.SetRepeatLocked(value != 0, (ChangeContexts)changeContext);
            break;
          case OFFSET_LOCKED_MODE:
            texture.SetOffsetLocked(value != 0, (ChangeContexts)changeContext);
            break;
          case PREVIEW_IN_3D_MODE:
            texture.SetPreviewIn3D(value != 0, (ChangeContexts)changeContext);
            break;
        }
      }
      catch (Exception exception)
      {
        HostUtils.ExceptionReport(exception);
      }
    }

    internal delegate void GetVirtual3DVectorCallback(int serialNumber, int propertyId, bool fromBaseClass, ref Vector3d value);
    internal static GetVirtual3DVectorCallback GetVirtual3DVector = OnGetVirtual3DVector;
    private static void OnGetVirtual3DVector(int serialNumber, int propertyId, bool fromBaseClass, ref Vector3d value)
    {
      try
      {
        var texture = FromSerialNumber(serialNumber) as RenderTexture;
        if (texture == null) return;
        var const_pointer = texture.ConstPointer();
        if (fromBaseClass)
        {
          UnsafeNativeMethods.Rdk_RenderTexture_GetVirtual3dVector(const_pointer, propertyId, true, ref value);
          return;
        }
        switch (propertyId)
        {
          case REPEAT_MODE:
            value = texture.GetRepeat();
            return;
          case OFFSET_MODE:
            value = texture.GetOffset();
            return;
          case ROTATION_MODE:
            value = texture.GetRotation();
            return;
        }
      }
      catch (Exception exception)
      {
        HostUtils.ExceptionReport(exception);
      }
    }


    internal delegate void SetVirtual3DVectorCallback(int serialNumber, int propertyId, bool callBaseClass, Vector3d value, int changeContext);
    internal static SetVirtual3DVectorCallback SetVirtual3DVector = OnSetVirtual3DVector;
    private static void OnSetVirtual3DVector(int serialNumber, int propertyId, bool callBaseClass, Vector3d value, int changeContext)
    {
      try
      {
        var texture = FromSerialNumber(serialNumber) as RenderTexture;
        if (texture == null) return;
        if (callBaseClass)
        {
          var pointer = texture.NonConstPointer();
          UnsafeNativeMethods.Rdk_RenderTexture_SetVirtual3dVector(pointer, propertyId, true, value, changeContext);
          return;
        }
        switch (propertyId)
        {
          case REPEAT_MODE:
            texture.SetRepeat(value, (ChangeContexts)changeContext);
            return;
          case OFFSET_MODE:
            texture.SetOffset(value, (ChangeContexts)changeContext);
            return;
          case ROTATION_MODE:
            texture.SetRotation(value, (ChangeContexts)changeContext);
            return;
        }
      }
      catch (Exception exception)
      {
        HostUtils.ExceptionReport(exception);
      }
    }

    internal delegate IntPtr GetNewTextureEvaluatorCallback(int serialNumber);
    internal static GetNewTextureEvaluatorCallback m_NewTextureEvaluator = OnNewTextureEvaluator;
    static IntPtr OnNewTextureEvaluator(int serialNumber)
    {
      IntPtr rc = IntPtr.Zero;
      try
      {
        RenderTexture texture = FromSerialNumber(serialNumber) as RenderTexture;
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

    public virtual Display.Color4f GetColor(Point3d uvw, Vector3d duvwdx, Vector3d duvwdy)
    {
      if (m_runtime_serial_number > 0)
        return Display.Color4f.Empty;
      IntPtr pConstThis = ConstPointer();
      Display.Color4f rc = new Display.Color4f();

      if (!UnsafeNativeMethods.Rdk_TextureEvaluator_GetColor(pConstThis, uvw, duvwdx, duvwdy, ref rc))
        return Display.Color4f.Empty;
      return rc;
    }

    internal delegate int GetColorCallback(int serialNumber, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy, ref Display.Color4f color);
    internal static GetColorCallback m_GetColor = OnGetColor;
    static int OnGetColor(int serialNumber, Point3d uvw, Vector3d duvwdx, Vector3d duvwdy, ref Display.Color4f color)
    {
      int rc = 0;
      TextureEvaluator eval = FromSerialNumber(serialNumber);
      if (eval != null)
      {
        Display.Color4f c = eval.GetColor(uvw, duvwdx, duvwdy);
        if (c != Display.Color4f.Empty)
        {
          color = c;
          rc = 1;
        }
      }
      return rc;
    }

    internal delegate void OnDeleteThisCallback(int serialNumber);
    internal static OnDeleteThisCallback m_OnDeleteThis = OnDeleteThis;
    static void OnDeleteThis(int serialNumber)
    {
      TextureEvaluator eval = FromSerialNumber(serialNumber);
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

    static TextureEvaluator FromSerialNumber(int serialNumber)
    {
      int index = serialNumber - 1;
      if (index >= 0 && index < m_all_custom_evaluators.Count)
      {
        TextureEvaluator rc = m_all_custom_evaluators[index];
        if (rc != null && rc.m_runtime_serial_number == serialNumber)
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

    protected TwoColorRenderTexture()
    {
      m_color1 = Fields.Add("color-one", Display.Color4f.Black, Rhino.UI.LOC.STR("Color 1"));
      m_color2 = Fields.Add("color-two", Display.Color4f.White, Rhino.UI.LOC.STR("Color 2"));

      m_texture1_on = Fields.Add("texture-on-one", true, Rhino.UI.LOC.STR("Texture1 On"));
      m_texture2_on = Fields.Add("texture-on-two", true, Rhino.UI.LOC.STR("Texture2 On"));

      m_texture1_amount = Fields.Add("texture-amount-one", 1.0, Rhino.UI.LOC.STR("Texture1 Amt"));
      m_texture2_amount = Fields.Add("texture-amount-two", 1.0, Rhino.UI.LOC.STR("Texture2 Amt"));

      m_swap_colors = Fields.Add("swap-colors", false, Rhino.UI.LOC.STR("Swap Colors"));
      m_super_sample = Fields.Add("super-sample", false, Rhino.UI.LOC.STR("Super sample"));
    }

    private readonly Fields.Color4fField m_color1;
    private readonly Fields.Color4fField m_color2;

    private readonly Fields.BoolField m_texture1_on;
    private readonly Fields.BoolField m_texture2_on;

    private readonly Fields.DoubleField m_texture1_amount;
    private readonly Fields.DoubleField m_texture2_amount;

    private readonly Fields.BoolField m_swap_colors;
    private readonly Fields.BoolField m_super_sample;

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
      IntPtr ptr_content = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return ptr_content;
    }
    internal override IntPtr NonConstPointer()
    {
      IntPtr ptr_content = UnsafeNativeMethods.Rdk_FindContentInstance(m_native_instance_id);
      return ptr_content;
    }
  }
  #endregion
}

#endif