#pragma warning disable 1591
using System;

namespace Rhino.Render
{
  public enum TextureMappingType : int
  {
    None = 0,
    /// <summary>u,v = linear transform of surface params,w = 0</summary>
    SurfaceParameters = 1,
    /// <summary>u,v,w = 3d coordinates wrt frame</summary>
    PlaneMapping = 2,
    /// <summary>u,v,w = logitude, height, radius</summary>
    CylinderMapping = 3,
    /// <summary>(u,v,w) = longitude,latitude,radius</summary>
    SphereMapping = 4,
    BoxMapping = 5,
    /// <summary>mapping primitive is a Mesh</summary>
    MeshMappingPrimitive = 6,
    /// <summary>mapping primitive is a Surface</summary>
    SurfaceMappingPrimitive = 7,
    /// <summary>mapping primitive is a Brep</summary>
    BrepMappingPrimitive = 8
  }
}
//public class ON_TextureMapping { }
