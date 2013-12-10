#pragma warning disable 1591
using System;
using Rhino.Runtime.InteropWrappers;

#if RDK_CHECKED
namespace Rhino.Render
{
  public abstract class RenderMaterial : RenderContent
  {
    /// <summary>
    /// Constructs a new basic material from a <see cref="Rhino.DocObjects.Material">Material</see>.
    /// </summary>
    /// <param name="material">(optional)The material to create the basic material from.</param>
    /// <returns>A new basic material.</returns>
    public static RenderMaterial CreateBasicMaterial(DocObjects.Material material)
    {
      IntPtr const_ptr_source_material = (material == null ? IntPtr.Zero : material.ConstPointer());
      IntPtr ptr_new_material = UnsafeNativeMethods.Rdk_Globals_NewBasicMaterial(const_ptr_source_material);
      NativeRenderMaterial new_material = FromPointer(ptr_new_material) as NativeRenderMaterial;
      if (new_material != null)
        new_material.AutoDelete = true;
      return new_material;
    }

    /// <summary>
    /// Defines enumerated constant values for use in <see cref="TextureChildSlotName"/> method.
    /// </summary>
    public enum StandardChildSlots : int
    {
      /// <summary>
      /// Corresponds to ON_Texture::bitmap_texture.
      /// </summary>
      Diffuse = 100,
      /// <summary>
      /// Corresponds to ON_Texture::transparancy_texture.
      /// </summary>
      Transparency = 101,
      /// <summary>
      /// Corresponds to ON_Texture::bump_texture.
      /// </summary>
      Bump = 102,
      /// <summary>
      /// Corresponds to ON_Texture::emap_texture.
      /// </summary>
      Environment = 103,
    }

    /// <summary>
    /// Parameter names for use in GetNamedParameter and SetNamedParameter with basic materials.
    /// </summary>
    public class BasicMaterialParameterNames
    {
      public const string Ambient = "ambient";
      public const string Emission = "emission";
      public const string FlamingoLibrary = "flamingo-library";
      public const string DisableLighting = "disable-lighting";
      public const string Diffuse = "diffuse";
      public const string Specular = "specular";
      public const string TransparencyColor = "transparency-color";
      public const string ReflectivityColor = "reflectivity-color";
      public const string Shine = "shine";
      public const string Transparency = "transparency";
      public const string Reflectivity = "reflectivity";
      public const string Ior = "ior";
    }

    /// <summary>
    /// Override this function to provide information about which texture is used for
    /// the standard (ie - defined in ON_Texture) texture channels.
    /// </summary>
    /// <param name="slot">An valid slot.</param>
    /// <returns>The texture used for the channel.</returns>
    public virtual string TextureChildSlotName(StandardChildSlots slot)
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

      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.Rdk_RenderMaterial_CallTextureChildSlotNameBase(ConstPointer(), pString, (int)slot);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Override this function to provide a Rhino.DocObjects.Material definition for this material
    /// to be used by other rendering engines including the display.
    /// </summary>
    /// <param name="simulation">Set the properties of the input basic material to provide the simulation for this material.</param>
    /// <param name="isForDataOnly">Called when only asking for a hash - don't write any textures to the disk - just provide the filenames they will get.</param>
    public virtual void SimulateMaterial(ref DocObjects.Material simulation, bool isForDataOnly)
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

    internal static NewRenderContentCallbackEvent m_NewMaterialCallback = OnNewMaterial;
    static IntPtr OnNewMaterial(Guid typeId)
    {
      var renderContent = NewRenderContent(typeId, typeof(RenderMaterial));
      return (null == renderContent ? IntPtr.Zero : renderContent.NonConstPointer());
    }

    internal delegate void TextureChildSlotNameCallback(int serial_number, int which, IntPtr pON_wString);
    internal static TextureChildSlotNameCallback m_TextureChildSlotName = OnTextureChildSlotName;
    static void OnTextureChildSlotName(int serial_number, int which, IntPtr pON_wString)
    {
      try
      {
        var material = RenderContent.FromSerialNumber(serial_number) as RenderMaterial;
        if (material != null)
        {
          String str = material.TextureChildSlotName((StandardChildSlots)which);
          if (!String.IsNullOrEmpty(str))
          {
            UnsafeNativeMethods.ON_wString_Set(pON_wString, str);
          }
        }
      }
      catch (Exception exception)
      {
        Runtime.HostUtils.ExceptionReport(exception);
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