using System;
using System.Runtime.Serialization;

namespace Rhino.DocObjects
{
  [Serializable]
  public class Texture : Rhino.Runtime.CommonObject
  {
    #region members
    readonly Rhino.DocObjects.Material m_parent_material;
#if RDK_UNCHECKED
    readonly Rhino.Render.SimulatedTexture m_parent_simulated_texture;
#endif
    readonly int m_index = 0;
    #endregion

    public Texture()
    {
      IntPtr pTexture = UnsafeNativeMethods.ON_Texture_New();
      base.ConstructNonConstObject(pTexture);
    }

    internal Texture(int index, Rhino.DocObjects.Material parent)
    {
      m_parent_material = parent;
      m_index = index;
      this.m__parent = parent;
    }

#if RDK_UNCHECKED
    internal Texture(Rhino.Render.SimulatedTexture parent)
    {
      m_parent_simulated_texture = parent;
      this.m__parent = parent;
    }
#endif

    internal override IntPtr _InternalGetConstPointer()
    {
      if (m_parent_material != null)
      {
        IntPtr pRhinoMaterial = m_parent_material.ConstPointer();
        return UnsafeNativeMethods.ON_Material_GetTexturePointer(pRhinoMaterial, m_index);
      }
#if RDK_UNCHECKED
      if (m_parent_simulated_texture != null)
      {
          IntPtr pSimulatedTexture = m_parent_simulated_texture.ConstPointer();
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
