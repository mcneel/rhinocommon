using System;
using System.Runtime.InteropServices;

namespace Rhino.DocObjects
{
  /// <summary>
  /// Provides material definition information
  /// </summary>
  [Obsolete("Use Material - will be removed in a future WIP")]
  public class MaterialInfo : Runtime.CommonObject
  {
    public MaterialInfo()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Material_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    // Track materials in the document by doc ID and index
    internal MaterialInfo(IntPtr pSource)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Material_New(pSource);
      ConstructNonConstObject(ptr);
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      return IntPtr.Zero;
    }

    public override bool IsValid
    {
      get
      {
        return InternalIsValid();
      }
    }

    /// <summary>
    /// Set material to default settings
    /// </summary>
    public void Default()
    {
      IntPtr pConstThis = NonConstPointer();
      UnsafeNativeMethods.ON_Material_Default(pConstThis);
    }

    /// <summary>
    /// Searches for a texure with matching filename and type. If more
    /// than one texture matches, the first match is returned.
    /// </summary>
    /// <param name="filename">if null then any filename matches</param>
    /// <returns>-1 if no match is found</returns>
    public int FindBitmapTexture(string filename)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Material_FindBitmapTexture(pConstThis, filename);
    }

    /// <summary>
    /// Searches for any bitmap texture
    /// </summary>
    /// <returns>-1 if no match is found</returns>
    public int FindBitmapTexture()
    {
      return FindBitmapTexture(null);
    }

    public void SetTextureFilename(int index, string filename)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Material_SetBitmapTexture(pThis, index, filename);
    }

    public int AddBitmapTexture(string filename)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Material_AddBitmapTexture(pThis, filename);
    }

  }
}
