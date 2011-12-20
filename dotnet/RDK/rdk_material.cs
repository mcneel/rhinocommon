#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rhino.Geometry;

#if RDK_UNCHECKED
namespace Rhino.Render
{
  public abstract class RenderMaterial : RenderContent
  {
    protected RenderMaterial()
      : base(true)
    {
    }

    internal RenderMaterial(bool isCustom)
      : base(false)
    {
      //This constructor is only used to construct native wrappers
      Debug.Assert(isCustom == false);
    }

    /// <summary>
    /// Enumeration type for use in TextureChildSlotName method.
    /// </summary>
    public enum StandardChildSlots : int
    {
      /// <summary>
      /// Corresponds to ON_Texture::bitmap_texture.
      /// </summary>
      Diffuse = 0,
      /// <summary>
      /// Corresponds to ON_Texture::transparancy_texture.
      /// </summary>
      Transparency = 1,
      /// <summary>
      /// Corresponds to ON_Texture::bump_texture.
      /// </summary>
      Bump = 2,
      /// <summary>
      /// Corresponds to ON_Texture::emap_texture.
      /// </summary>
      Environment = 3,
    }

    /// <summary>
    /// Parameter names for use in GetNamedParameter and SetNamedParameter with basic materials.
    /// </summary>
    public class BasicMaterialParameterNames
    {
      public const String Ambient = "ambient";
      public const String Emission = "emission";
      public const String FlamingoLibrary = "flamingo-library";
      public const String DisableLighting = "disable-lighting";
      public const String Diffuse = "diffuse";
      public const String Specular = "specular";
      public const String TransparencyColor = "transparency-color";
      public const String ReflectivityColor = "reflectivity-color";
      public const String Shine = "shine";
      public const String Transparency = "transparency";
      public const String Reflectivity = "reflectivity";
      public const String Ior = "ior";
    }

    /// <summary>
    /// Override this function to provide information about which texture is used for
    /// the standard (ie - defined in ON_Texture) texture channels.
    /// </summary>
    /// <param name="slot">See the StandardChildSlots enumeration for valid values.</param>
    /// <returns></returns>
    public virtual String TextureChildSlotName(StandardChildSlots slot)
    {
      if (IsNativeWrapper())
      {
        StringIds iString = StringIds.DiffuseChildSlotName;
        switch (slot)
        {
          case StandardChildSlots.Diffuse:
            iString = StringIds.DiffuseChildSlotName;
            break;
          case StandardChildSlots.Transparency:
            iString = StringIds.TransparencyChildSlotName;
            break;
          case StandardChildSlots.Bump:
            iString = StringIds.BumpChildSlotName;
            break;
          case StandardChildSlots.Environment:
            iString = StringIds.EnvironmentChildSlotName;
            break;
        }
        return GetString(iString);
      }
      else
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_RenderMaterial_CallTextureChildSlotNameBase(ConstPointer(), pString, (int)slot);
          return sh.ToString();
        }
      }
    }

    /// <summary>
    /// Override this function to provide a Rhino.DocObjects.Material definition for this material
    /// to be used by other rendering engines including the display.
    /// </summary>
    /// <param name="simulation">Set the properties of the input basic material to provide the simulation for this material.</param>
    /// <param name="isForDataOnly">Called when only asking for a hash - don't write any textures to the disk - just provide the filenames they will get.</param>
    public virtual void SimulateMaterial(ref Rhino.DocObjects.Material simulation, bool isForDataOnly)
    {
      if (IsNativeWrapper())
      {
        UnsafeNativeMethods.Rdk_RenderMaterial_SimulateMaterial(NonConstPointer(), simulation.ConstPointer(), isForDataOnly);
      }
      else
      {
        UnsafeNativeMethods.Rdk_RenderMaterial_CallSimulateMaterialBase(NonConstPointer(), simulation.ConstPointer(), isForDataOnly);
      }
    }


    #region callbacks from c++

    internal delegate IntPtr NewMaterialCallback(Guid type_id);
    internal static NewMaterialCallback m_NewMaterial = OnNewMaterial;
    static IntPtr OnNewMaterial(Guid type_id)
    {
      IntPtr rc = IntPtr.Zero;
      try
      {
        Guid plugin_id;
        Type t = RdkPlugIn.GetRenderContentType(type_id, out plugin_id);
        if (t != null && plugin_id != Guid.Empty)
        {
          RenderMaterial Material = System.Activator.CreateInstance(t) as RenderMaterial;
          Material.Construct(plugin_id);
          rc = Material.NonConstPointer();
        }
      }
      catch
      {
        rc = IntPtr.Zero;
      }
      return rc;
    }

    internal delegate void TextureChildSlotNameCallback(int serial_number, int which, IntPtr pON_wString);
    internal static TextureChildSlotNameCallback m_TextureChildSlotName = OnTextureChildSlotName;
    static void OnTextureChildSlotName(int serial_number, int which, IntPtr pON_wString)
    {
      try
      {
        RenderMaterial material = RenderContent.FromSerialNumber(serial_number) as RenderMaterial;
        if (material != null)
        {
          String str = material.TextureChildSlotName((StandardChildSlots)which);
          if (!String.IsNullOrEmpty(str))
          {
            UnsafeNativeMethods.ON_wString_Set(pON_wString, str);
          }
        }
      }
      catch
      {
      }
    }

    internal delegate void SimulateMaterialCallback(int serial_number, IntPtr p, int bDataOnly);
    internal static SimulateMaterialCallback m_SimulateMaterial = OnSimulateMaterial;
    static void OnSimulateMaterial(int serial_number, IntPtr pSim, int bDataOnly)
    {
      try
      {
        RenderMaterial material = RenderContent.FromSerialNumber(serial_number) as RenderMaterial;
        if (material != null && pSim != IntPtr.Zero)
        {
          Rhino.DocObjects.Material temp_material = Rhino.DocObjects.Material.NewTemporaryMaterial(pSim);
          if (temp_material != null)
          {
            material.SimulateMaterial(ref temp_material, bDataOnly != 0);
            temp_material.ReleaseNonConstPointer();
          }
        }
      }
      catch (Exception ex)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(ex);
      }
    }

    #endregion
  }











  #region Native wrapper
  // DO NOT make public
  internal class NativeRenderMaterial : RenderMaterial
  {
    readonly Guid m_native_instance_id = Guid.Empty;
    public NativeRenderMaterial(IntPtr pRenderContent)
      : base(false)
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
    protected override bool IsNativeWrapper()
    {
      return true;
    }
  }
  #endregion
}
#endif