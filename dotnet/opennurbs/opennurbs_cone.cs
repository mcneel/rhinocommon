#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  [Serializable]
  public struct Cone
  {
    internal Plane m_baseplane;
    internal double m_height;
    internal double m_radius;

    #region constructors
    /// <summary>
    /// Create a new Cone with a specified baseplane, height and radius.
    /// </summary>
    /// <param name="plane">Base plane of cone.</param>
    /// <param name="height">Height of cone.</param>
    /// <param name="radius">Radius of cone.</param>
    public Cone(Plane plane, double height, double radius)
    {
      m_baseplane = plane;
      m_height = height;
      m_radius = radius;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the base plane of the Cone.
    /// </summary>
    public Plane Plane
    {
      get { return m_baseplane; }
      set { m_baseplane = value; }
    }

    /// <summary>
    /// Gets or sets the height of the cone.
    /// </summary>
    public double Height
    {
      get { return m_height; }
      set { m_height = value; }
    }

    /// <summary>
    /// Gets or sets the Radius of the cone.
    /// </summary>
    public double Radius
    {
      get { return m_radius; }
      set { m_radius = value; }
    }

    /// <summary>
    /// True is plane is valid, height is not zero and radius is not zero
    /// </summary>
    public bool IsValid
    {
      get
      {
        return m_baseplane.IsValid && m_height != 0 && m_radius != 0;
      }
    }

    /// <summary>Center of base circle</summary>
    public Point3d BasePoint
    {
      get
      {
        return m_baseplane.Origin + m_height * m_baseplane.ZAxis;
      }
    }

    /// <summary>Point at tip of the cone</summary>
    public Point3d ApexPoint
    {
      get
      {
        return m_baseplane.Origin;
      }
    }

    /// <summary>Unit vector axis of cone</summary>
    public Vector3d Axis
    {
      get
      {
        return m_baseplane.ZAxis;
      }
    }

    #endregion

    #region methods
    /// <summary>
    /// The angle (in radians) between the axis and the 
    /// side of the cone.
    /// The angle and the height have the same sign.
    /// </summary>
    /// <returns></returns>
    public double AngleInRadians()
    {
      return m_height == 0.0 ? (m_radius != 0.0 ? Math.PI : 0.0) : Math.Atan(m_radius / m_height);
    }
    
    /// <summary>
    /// The angle (in degrees) between the axis and the 
    /// side of the cone.
    /// The angle and the height have the same sign.
    /// </summary>
    /// <returns></returns>
    public double AngleInDegrees()
    {
      return 180.0 * AngleInRadians() / Math.PI;
    }

    /// <summary>
    /// Create a Nurbs surface representation of this Cone. 
    /// This is synonymous with calling NurbsSurface.CreateFromCone().
    /// </summary>
    /// <returns>A Nurbs surface representation of the cone or null.</returns>
    public NurbsSurface ToNurbsSurface()
    {
      return NurbsSurface.CreateFromCone(this);
    }

    /// <summary>
    /// Create a RevSurface representation of this Cone. 
    /// This is synonymous with calling RevSurface.CreateFromCone().
    /// </summary>
    /// <returns>A RevSurface representation of the cone or null.</returns>
    public RevSurface ToRevSurface()
    {
      return RevSurface.CreateFromCone(this);
    }

    /// <summary>
    /// Get a Brep representation of the cone with a single
    /// face for the cone, an edge along the cone seam, 
    /// and vertices at the base and apex ends of this seam edge.
    /// The optional cap is a single face with one circular edge 
    /// starting and ending at the base vertex.
    /// </summary>
    /// <param name="capBottom"></param>
    /// <returns></returns>
    public Brep ToBrep(bool capBottom)
    {
      return Brep.CreateFromCone(this, capBottom);
    }
    #endregion
  }
}