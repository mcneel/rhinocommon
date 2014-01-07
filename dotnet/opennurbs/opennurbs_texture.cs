using System;
using System.Runtime.Serialization;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
  /// <summary>
  /// The TextureType controls how the pixels in the bitmap
  /// are interpreted.
  /// </summary>
  public enum TextureType : int
  {
    /// <summary> 
    /// </summary>
    None = 0,
    /// <summary>
    /// Standard image texture
    /// </summary>
    Bitmap = 1,
    /// <summary>
    /// bump map
    /// </summary>
    Bump = 2,
    /// <summary>
    /// value = alpha
    /// </summary>
    Transparency = 3
  }

  /// <summary>
  /// Determines how this texture is combined with others in a material's
  /// texture list.
  /// </summary>
  public enum TextureCombineMode : int
  {
    /// <summary>
    /// </summary>
    None = 0,
    /// <summary>
    /// Modulate with material diffuse color
    /// </summary>
    Modulate = 1,
    /// <summary>
    /// Decal
    /// </summary>
    Decal = 2,
    /// <summary>
    /// Blend texture with others in the material
    ///   To "add" a texture, set BlendAmount = +1
    ///   To "subtract" a texture, set BlendAmount = -1
    /// </summary>
    Blend = 3,
  }

  /// <summary>
  /// Defines Texture UVW wrapping modes
  /// </summary>
  public enum TextureUvwWrapping : int
  {
    /// <summary>
    /// Repeat the texture
    /// </summary>
    Repeat = 0,
    /// <summary>
    /// Clamp the texture
    /// </summary>
    Clamp = 1
  }

  /// <summary>
  /// Represents a texture that is mapped on objects.
  /// </summary>
  [Serializable]
  public class Texture : Runtime.CommonObject
  {
    private readonly int m_index;

    /// <summary>
    /// Initializes a new texture.
    /// </summary>
    public Texture()
    {
      IntPtr ptr_this = UnsafeNativeMethods.ON_Texture_New();
      ConstructNonConstObject(ptr_this);
    }

    internal Texture(int index, Material parent)
    {
      m_index = index;
      m__parent = parent;
    }

#if RHINO_SDK
    private readonly bool m_front = true;

    internal Texture(int index, Display.DisplayMaterial parent, bool front)
    {
      m_index = index;
      m__parent = parent;
      m_front = front;
    }
#endif

#if RDK_CHECKED
    internal Texture(Render.SimulatedTexture parent)
    {
      m__parent = parent;
    }
#endif

    internal override IntPtr _InternalGetConstPointer()
    {
      DocObjects.Material parent_material = m__parent as DocObjects.Material;
      if (parent_material != null)
      {
        IntPtr pRhinoMaterial = parent_material.ConstPointer();
        return UnsafeNativeMethods.ON_Material_GetTexturePointer(pRhinoMaterial, m_index);
      }

#if RHINO_SDK
      Display.DisplayMaterial parent_display_material = m__parent as Display.DisplayMaterial;
      if (parent_display_material != null)
      {
        IntPtr pDisplayPipelineMaterial = parent_display_material.ConstPointer();
        IntPtr pMaterial = UnsafeNativeMethods.CDisplayPipelineMaterial_MaterialPointer(pDisplayPipelineMaterial,
                                                                                        m_front);
        return UnsafeNativeMethods.ON_Material_GetTexturePointer(pMaterial, m_index);
      }
#endif
#if RDK_CHECKED
      Rhino.Render.SimulatedTexture parent_simulated_texture = m__parent as Rhino.Render.SimulatedTexture;
      if (parent_simulated_texture != null)
      {
        IntPtr pSimulatedTexture = parent_simulated_texture.ConstPointer();
        return UnsafeNativeMethods.Rdk_SimulatedTexture_OnTexturePointer(pSimulatedTexture);
      }
#endif
      return IntPtr.Zero;
    }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected Texture(SerializationInfo info, StreamingContext context)
      : base(info, context)
    {
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    /// <summary>
    /// Gets or sets a file name that is used by this texture.
    /// NOTE: this filename may well not be a path that makes sense
    /// on a user's computer because it was a path initially set on
    /// a different user's computer. If you want to get a workable path
    /// for this user, use the BitmapTable.Find function using this
    /// property.
    /// </summary>
    public string FileName
    {
      get
      {
        IntPtr pConstTexture = ConstPointer();
        if (IntPtr.Zero == pConstTexture)
          return String.Empty;
        using (var sh = new Runtime.InteropWrappers.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_Texture_GetFileName(pConstTexture, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pTexture = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetFileName(pTexture, value);
      }
    }

    /// <summary>
    /// Gets the globally unique identifier of this texture.
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Texture_GetId(pConstThis);
      }
    }

    /// <summary>
    /// If the texture is enabled then it will be visible in the rendered
    /// display otherwise it will not.
    /// </summary>
    public bool Enabled
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        return UnsafeNativeMethods.ON_Texture_GetEnabled(const_ptr_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetEnabled(ptr_this, value);
      }
    }

    /// <summary>
    /// Controls how the pixels in the bitmap are interpreted
    /// </summary>
    public TextureType TextureType
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Texture_TextureType(const_ptr_this);
        return (TextureType)rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetTextureType(ptr_this, (int)value);
      }
    }

    /// <summary>
    /// Determines how this texture is combined with others in a material's
    /// texture list.
    /// </summary>
    public TextureCombineMode TextureCombineMode
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Texture_Mode(const_ptr_this);
        return (TextureCombineMode)rc;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetMode(ptr_this, (int)value);
      }
    }

    //skipping for now
    //  FILTER m_minfilter;
    //  FILTER m_magfilter;

    const int IDX_WRAPMODE_U = 0;
    const int IDX_WRAPMODE_V = 1;
    const int IDX_WRAPMODE_W = 2;

    /// <summary>
    /// Helper function for getting the ON_Texture::WRAP mode and converting
    /// it to a TextureUvwWrapping value.
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    TextureUvwWrapping WrapUvwHelper(int mode)
    {
      IntPtr const_ptr_texture = ConstPointer();
      int value = UnsafeNativeMethods.ON_Texture_wrapuvw(const_ptr_texture, mode);
      return (value == (int)TextureUvwWrapping.Clamp ? TextureUvwWrapping.Clamp : TextureUvwWrapping.Repeat);
    }

    /// <summary>
    /// Texture wrapping mode in the U direction
    /// </summary>
    public TextureUvwWrapping WrapU
    {
      get { return WrapUvwHelper(IDX_WRAPMODE_U); }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_Set_wrapuvw(ptr_this, IDX_WRAPMODE_U, (int)value);
      }
    }

    /// <summary>
    /// Texture wrapping mode in the V direction
    /// </summary>
    public TextureUvwWrapping WrapV
    {
      get { return WrapUvwHelper(IDX_WRAPMODE_V); }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_Set_wrapuvw(ptr_this, IDX_WRAPMODE_V, (int)value);
      }
    }

    /// <summary>
    /// Texture wrapping mode in the W direction
    /// </summary>
    public TextureUvwWrapping WrapW
    {
      get { return WrapUvwHelper(IDX_WRAPMODE_W); }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_Set_wrapuvw(ptr_this, IDX_WRAPMODE_W, (int)value);
      }
    }

    /// <summary>
    /// If true then the UVW transform is applied to the texture
    /// otherwise the UVW transform is ignored.
    /// </summary>
    public bool ApplyUvwTransform
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        bool value = UnsafeNativeMethods.ON_Texture_Apply_uvw(const_ptr_this);
        return value;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetApply_uvw(ptr_this, value);
      }
    }

    /// <summary>
    /// Transform to be applied to each instance of this texture
    /// if ApplyUvw is true
    /// </summary>
    public Transform UvwTransform
    {
      get
      {
        IntPtr const_ptr_this = ConstPointer();
        Transform value = new Transform(1);
        UnsafeNativeMethods.ON_Texture_uvw(const_ptr_this, ref value);
        return value;
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_Setuvw(ptr_this, ref value);
      }
    }

    // skipping for now
    //  ON_Color m_border_color;
    //  ON_Color m_transparent_color;
    //  ON_UUID m_transparency_texture_id;
    //  ON_Interval m_bump_scale;

    /// <summary>
    /// If the TextureCombineMode is Blend, then the blending function
    /// for alpha is determined by
    /// <para>
    /// new alpha = constant
    ///             + a0*(current alpha)
    ///             + a1*(texture alpha)
    ///             + a2*min(current alpha,texture alpha)
    ///             + a3*max(current alpha,texture alpha)
    /// </para>
    /// </summary>
    /// <param name="constant"></param>
    /// <param name="a0"></param>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <param name="a3"></param>
    public void GetAlphaBlendValues(out double constant, out double a0, out double a1, out double a2, out double a3)
    {
      constant = 0;
      a0 = 0;
      a1 = 0;
      a2 = 0;
      a3 = 0;
      IntPtr const_ptr_this = ConstPointer();
      UnsafeNativeMethods.ON_Texture_GetAlphaBlendValues(const_ptr_this, ref constant, ref a0, ref a1, ref a2, ref a3);
    }

    /// <summary>
    /// If the TextureCombineMode is Blend, then the blending function
    /// for alpha is determined by
    /// <para>
    /// new alpha = constant
    ///             + a0*(current alpha)
    ///             + a1*(texture alpha)
    ///             + a2*min(current alpha,texture alpha)
    ///             + a3*max(current alpha,texture alpha)
    /// </para>
    /// </summary>
    /// <param name="constant"></param>
    /// <param name="a0"></param>
    /// <param name="a1"></param>
    /// <param name="a2"></param>
    /// <param name="a3"></param>
    public void SetAlphaBlendValues(double constant, double a0, double a1, double a2, double a3)
    {
      IntPtr ptr_this = NonConstPointer();
      UnsafeNativeMethods.ON_Texture_SetAlphaBlendValues(ptr_this, constant, a0, a1, a2, a3);
    }

    // skipping for now
    //  ON_Color m_blend_constant_RGB;
    //  double m_blend_RGB[4];
    //  int m_blend_order;
  }
}
