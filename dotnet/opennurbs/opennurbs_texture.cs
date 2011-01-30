using System;

namespace Rhino.DocObjects
{
  public class Texture : Rhino.Runtime.CommonObject
  {
    #region members
    readonly Rhino.DocObjects.Material m_parent_material;
#if USING_RDK
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

#if USING_RDK
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
#if USING_RDK
      if (m_parent_simulated_texture != null)
      {
          IntPtr pSimulatedTexture = m_parent_simulated_texture.ConstPointer();
          return UnsafeNativeMethods.Rdk_SimulatedTexture_OnTexturePointer(pSimulatedTexture);
      }
#endif
      return IntPtr.Zero;
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
