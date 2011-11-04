#pragma warning disable 1591
using System;
using System.Runtime.Serialization;

namespace Rhino.DocObjects
{
  [Serializable]
  public class Texture : Rhino.Runtime.CommonObject
  {
    readonly int m_index = 0;

    public Texture()
    {
      IntPtr pTexture = UnsafeNativeMethods.ON_Texture_New();
      base.ConstructNonConstObject(pTexture);
    }

    internal Texture(int index, Rhino.DocObjects.Material parent)
    {
      m_index = index;
      this.m__parent = parent;
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

#if RDK_UNCHECKED
    internal Texture(Rhino.Render.SimulatedTexture parent)
    {
      this.m__parent = parent;
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
#if RDK_UNCHECKED
      Rhino.Render.SimulatedTexture parent_simulated_texture = m__parent as Rhino.Render.SimulatedTexture;
      if (parent_simulated_texture != null)
      {
        IntPtr pSimulatedTexture = parent_simulated_texture.ConstPointer();
        return UnsafeNativeMethods.Rdk_SimulatedTexture_OnTexturePointer(pSimulatedTexture);
      }
#endif
      return IntPtr.Zero;
    }

    // serialization constructor
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

    public Guid Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Texture_GetId(pConstThis);
      }
    }
  }
}
