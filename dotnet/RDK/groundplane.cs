#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

#if RDK_CHECKED
namespace Rhino.Render
{
  /// <summary>
  /// Represents an infinite plane for implementation by renderers.
  /// See <see cref="RenderPlugin.SupportsFeature"/> and <see cref="RenderFeature"/>.
  /// </summary>
  public class GroundPlane
  {
    // Functions/Properties in this class do not need to check for Rdk since the only
    // way to access the Rdk is throuh the GroundPlane property on the RhinoDoc. That
    // propertt does the check before returning this class
    private Rhino.RhinoDoc m_doc;

    internal GroundPlane(Rhino.RhinoDoc doc)
    {
      Rhino.PlugIns.RenderPlugIn.RenderFeature
      m_doc = doc;
    }

    /// <summary>Document this groundplane is associated with.</summary>
    public Rhino.RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>
    /// Determines whether the document ground plane is enabled.
    /// </summary>
    public bool Enabled
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_Enabled(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetEnabled(value); }
    }

    /// <summary>
    /// Height above world XY plane in model units.
    /// </summary>
    public double Altitude
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_Altitude(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetAltitude(value); }
    }

    /// <summary>
    /// Id of material in material table for this ground plane.
    /// </summary>
    public Guid MaterialInstanceId
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_MaterialInstanceId(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetMaterialInstanceId(value); }
    }

    /// <summary>
    /// Texture mapping offset in world units.
    /// </summary>
    public Rhino.Geometry.Vector2d TextureOffset
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_GroundPlane_TextureOffset(ref v);
        return v;
      }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureOffset(value); }
    }

    /// <summary>
    /// Texture mapping single UV span size in world units.
    /// </summary>
    public Rhino.Geometry.Vector2d TextureSize
    {
      get
      {
        Rhino.Geometry.Vector2d v = new Rhino.Geometry.Vector2d();
        UnsafeNativeMethods.Rdk_GroundPlane_TextureSize(ref v);
        return v;
      }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureSize(value); }
    }

    /// <summary>
    /// Texture mapping rotation around world origin + offset in degrees.
    /// </summary>
    public double TextureRotation
    {
      get { return UnsafeNativeMethods.Rdk_GroundPlane_TextureRotation(); }
      set { UnsafeNativeMethods.Rdk_GroundPlane_SetTextureRotation(value); }
    }
  }
}
#endif