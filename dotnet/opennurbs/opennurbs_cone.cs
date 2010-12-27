using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  [Serializable()]
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
    #endregion

    #region methods
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
    #endregion
  }
}