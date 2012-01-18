using System;
using Rhino.Geometry;

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

  /// <summary>
  /// 
  /// </summary>
  public class TextureMapping : Rhino.Runtime.CommonObject
  {
    private TextureMapping()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_TextureMapping_New();
      ConstructNonConstObject(ptr);
    }
    internal TextureMapping(IntPtr pTextureMapping)
    {
      ConstructNonConstObject(pTextureMapping);
    }

    /// <summary>Create a planar projection texture mapping</summary>
    /// <param name="plane">A plane to use for mapping.</param>
    /// <param name="dx">portion of the plane's x axis that is mapped to [0,1] (can be a decreasing interval)</param>
    /// <param name="dy">portion of the plane's y axis that is mapped to [0,1] (can be a decreasing interval)</param>
    /// <param name="dz">portion of the plane's z axis that is mapped to [0,1] (can be a decreasing interval)</param>
    /// <returns>TextureMapping instance if input is valid</returns>
    public static TextureMapping CreatePlaneMapping(Plane plane, Interval dx, Interval dy, Interval dz)
    {
      TextureMapping rc = new TextureMapping();
      IntPtr pMapping = rc.NonConstPointer();
      if (!UnsafeNativeMethods.ON_TextureMapping_SetPlaneMapping(pMapping, ref plane, dx, dy, dz))
      {
        rc.Dispose();
        rc = null;
      }
      return rc;
    }

    /// <summary>Create a cylindrical projection texture mapping.</summary>
    /// <param name="cylinder">
    /// cylinder in world space used to define a cylindrical coordinate system.
    /// The angular parameter maps (0,2pi) to texture "u" (0,1), The height
    /// parameter maps (height[0],height[1]) to texture "v" (0,1), and the
    /// radial parameter maps (0,r) to texture "w" (0,1).
    /// </param>
    /// <param name="capped">
    /// If true, the cylinder is treated as a finite capped cylinder
    /// </param>
    /// <remarks>
    /// When the cylinder is capped and m_texture_space = divided, the
    /// cylinder is mapped to texture space as follows:
    /// The side is mapped to 0 &lt;= "u" &lt;= 2/3.
    /// The bottom is mapped to 2/3 &lt;= "u" &lt;= 5/6.
    /// The top is mapped to 5/6 &lt;= "u" &lt;= 5/6.
    /// This is the same convention box mapping uses.
    /// </remarks>
    /// <returns>TextureMapping instance if input is valid</returns>
    public static TextureMapping CreateCylinderMapping(Cylinder cylinder, bool capped)
    {
      TextureMapping rc = new TextureMapping();
      IntPtr pMapping = rc.NonConstPointer();
      if (!UnsafeNativeMethods.ON_TextureMapping_SetCylinderMapping(pMapping, ref cylinder, capped))
      {
        rc.Dispose();
        rc = null;
      }
      return rc;
    }

    /// <summary>
    /// Create a spherical projection texture mapping.
    /// </summary>
    /// <param name="sphere">
    /// sphere in world space used to define a spherical coordinate system.
    /// The longitude parameter maps (0,2pi) to texture "u" (0,1).
    /// The latitude paramter maps (-pi/2,+pi/2) to texture "v" (0,1).
    /// The radial parameter maps (0,r) to texture "w" (0,1).
    /// </param>
    /// <returns>TextureMapping instance if input is valid</returns>
    public static TextureMapping CreateSphereMapping(Sphere sphere)
    {
      TextureMapping rc = new TextureMapping();
      IntPtr pMapping = rc.NonConstPointer();
      if( !UnsafeNativeMethods.ON_TextureMapping_SetSphereMapping(pMapping, ref sphere) )
      {
        rc.Dispose();
        rc = null;
      }
      return rc;
    }

    /// <summary>Create a box projection texture mapping.</summary>
    /// <param name="plane">
    /// The sides of the box the box are parallel to the plane's coordinate
    /// planes.  The dx, dy, dz intervals determine the location of the sides.
    /// </param>
    /// <param name="dx">
    /// Determines the location of the front and back planes. The vector
    /// plane.xaxis is perpendicular to these planes and they pass through
    /// plane.PointAt(dx[0],0,0) and plane.PointAt(dx[1],0,0), respectivly.
    /// </param>
    /// <param name="dy">
    /// Determines the location of the left and right planes. The vector
    /// plane.yaxis is perpendicular to these planes and they pass through
    /// plane.PointAt(0,dy[0],0) and plane.PointAt(0,dy[1],0), respectivly.
    /// </param>
    /// <param name="dz">
    /// Determines the location of the top and bottom planes. The vector
    /// plane.zaxis is perpendicular to these planes and they pass through
    /// plane.PointAt(0,0,dz[0]) and plane.PointAt(0,0,dz[1]), respectivly.
    /// </param>
    /// <param name="capped">
    /// If true, the box is treated as a finite capped box.
    /// </param>
    /// <remarks>
    /// When m_texture_space = divided, the box is mapped to texture space as follows:
    /// If the box is not capped, then each side maps to 1/4 of the texture map.
    /// v=1+---------+---------+---------+---------+
    ///   | x=dx[1] | y=dy[1] | x=dx[0] | y=dy[0] |
    ///   | Front   | Right   | Back    | Left    |
    ///   | --y-&gt;   | &lt;-x--   | &lt;-y--   | --x-&gt;   |
    /// v=0+---------+---------+---------+---------+
    /// 0/4 &lt;=u&lt;= 1/4 &lt;=u&lt;= 2/4 &lt;=u&lt;= 3/4 &lt;=u&lt;= 4/4
    /// If the box is capped, then each side and cap gets 1/6 of the texture map.
    /// v=1+---------+---------+---------+---------+---------+---------+
    ///   | x=dx[1] | y=dy[1] | x=dx[0] | y=dy[0] | z=dx[1] | z=dz[0] |
    ///   | Front   | Right   | Back    | Left    | Top     |  Bottom |
    ///   | --y-&gt;   | &lt;x--   | &lt;-y--   | --x-&gt;   | --x-&gt;   | --x-&gt;   |
    /// v=0+---------+---------+---------+---------+---------+---------+
    /// 0/6 &lt;=u&lt;= 1/6 &lt;=u&lt;= 2/6 &lt;=u&lt;= 3/6 &lt;=u&lt;= 4/6 &lt;=u&lt;= 5/6 &lt;=u&lt;= 6/6 
    /// </remarks>
    /// <returns>TextureMapping instance if input is valid</returns>
    public static TextureMapping CreateBoxMapping(Plane plane, Interval dx, Interval dy, Interval dz, bool capped)
    {
      TextureMapping rc = new TextureMapping();
      IntPtr pMapping = rc.NonConstPointer();
      if (!UnsafeNativeMethods.ON_TextureMapping_SetBoxMapping(pMapping, ref plane, dx, dy, dz, capped))
      {
        rc.Dispose();
        rc = null;
      }
      return rc;
    }

    internal override IntPtr _InternalGetConstPointer()
    {
      return IntPtr.Zero;
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      applymempressure = false;
      IntPtr pConstPointer = ConstPointer();
      return UnsafeNativeMethods.ON_Object_Duplicate(pConstPointer);
    }
  }
}
