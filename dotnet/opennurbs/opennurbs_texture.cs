using System;

namespace Rhino.DocObjects
{
  public class Texture : Rhino.Runtime.CommonObject
  {
    #region members
    Rhino.DocObjects.Material m_parent_material;
    int m_index;
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

    internal override IntPtr _InternalGetConstPointer()
    {
      if (m_parent_material != null)
      {
        IntPtr pRhinoMaterial = m_parent_material.ConstPointer();
        return UnsafeNativeMethods.ON_Material_GetTexturePointer(pRhinoMaterial, m_index);
      }
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
