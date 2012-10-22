using System;
using System.Runtime.Serialization;

namespace Rhino.DocObjects
{
  /// <summary>
  /// Represents a texture that is mapped on objects.
  /// </summary>
  [Serializable]
  public class Texture : Rhino.Runtime.CommonObject
  {
    readonly int m_index;

    /// <summary>
    /// Initializes a new texture.
    /// </summary>
    public Texture()
    {
      IntPtr pTexture = UnsafeNativeMethods.ON_Texture_New();
      ConstructNonConstObject(pTexture);
    }

    internal Texture(int index, Rhino.DocObjects.Material parent)
    {
      m_index = index;
      m__parent = parent;
    }

#if RHINO_SDK
    readonly bool m_front = true;
    internal Texture(int index, Rhino.Display.DisplayMaterial parent, bool front)
    {
      m_index = index;
      m__parent = parent;
      m_front = front;
    }
#endif

#if RDK_CHECKED
    internal Texture(Rhino.Render.SimulatedTexture parent)
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
        IntPtr pMaterial = UnsafeNativeMethods.CDisplayPipelineMaterial_MaterialPointer(pDisplayPipelineMaterial, m_front);
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
      : base (info, context)
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
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
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
        IntPtr pConstTexture = ConstPointer();
        return UnsafeNativeMethods.ON_Texture_GetEnabled(pConstTexture);
      }
      set
      {
        IntPtr pTexture = NonConstPointer();
        UnsafeNativeMethods.ON_Texture_SetEnabled(pTexture, value);
      }
    }
  }
}
