using System;

namespace Rhino.Render
{
  /// <summary>
  /// Defines enumerated constants for mapping types such as planar, cylindrical or spherical.
  /// </summary>
  public enum TextureMappingType : int
  {
    /// <summary>No mapping is selected.</summary>
    None = 0,

    /// <summary>(u, v) = linear transform of surface params, w = 0.</summary>
    SurfaceParameters = 1,

    /// <summary>(u, v, w) = 3d coordinates wrt frame.</summary>
    PlaneMapping = 2,

    /// <summary>(u, v, w) = logitude, height, radius.</summary>
    CylinderMapping = 3,

    /// <summary>(u, v, w) = longitude,latitude,radius.</summary>
    SphereMapping = 4,

    /// <summary>Box mapping type.</summary>
    BoxMapping = 5,

    /// <summary>Mapping primitive is a mesh.</summary>
    MeshMappingPrimitive = 6,

    /// <summary>Mapping primitive is a surface.</summary>
    SurfaceMappingPrimitive = 7,

    /// <summary>Mapping primitive is a brep.</summary>
    BrepMappingPrimitive = 8
  }
}
//public class ON_TextureMapping { }
