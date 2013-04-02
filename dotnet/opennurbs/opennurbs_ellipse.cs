using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the values of a plane and the two semiaxes radii in an ellipse.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  [Serializable]
  public struct Ellipse : IEpsilonComparable<Ellipse>
  {
    #region members
    internal Plane m_plane;
    internal double m_radius1;
    internal double m_radius2;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new ellipse from base plane and both principal radii.
    /// </summary>
    /// <param name="plane">Base plane of ellipse.</param>
    /// <param name="radius1">Ellipse radius along base plane X direction.</param>
    /// <param name="radius2">Ellipse radius along base plane Y direction.</param>
    public Ellipse(Plane plane, double radius1, double radius2)
    {
      m_plane = plane;
      m_radius1 = radius1;
      m_radius2 = radius2;
    }

    /// <summary>
    /// Initializes a new ellipse from a center point and the two semiaxes intersections.
    /// </summary>
    /// <param name="center">A center for the ellipse. The avarage of the foci.</param>
    /// <param name="second">The intersection of the ellipse X axis with the ellipse itself.</param>
    /// <param name="third">A point that determines the radius along the Y semiaxis.
    /// <para>If the point is at right angle with the (center - second point) vector,
    /// it will be the intersection of the ellipse Y axis with the ellipse itself.</para>
    /// </param>
    public Ellipse(Point3d center, Point3d second, Point3d third)
    {
      m_plane = new Plane(center, second, third);
      m_radius1 = center.DistanceTo(second);
      m_radius2 = center.DistanceTo(third);
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the base plane of the ellipse.
    /// </summary>
    public Plane Plane 
    { 
      get { return m_plane; }
      set { m_plane = value; }
    }

    /// <summary>
    /// Gets or sets the radius of the ellipse along the base plane X semiaxis.
    /// </summary>
    public double Radius1 
    {
      get { return m_radius1; }
      set { m_radius1 = value; }
    }

    /// <summary>
    /// Gets or sets the radius of the ellipse along the base plane Y semiaxis.
    /// </summary>
    public double Radius2 
    {
      get { return m_radius2; }
      set { m_radius2 = value; }
    }
    #endregion

    #region methods
    /// <summary>
    /// Constructs a nurbs curve representation of this ellipse. 
    /// <para>This is equivalent to calling NurbsCurve.CreateFromEllipse().</para>
    /// </summary>
    /// <returns>A nurbs curve representation of this ellipse or null if no such representation could be made.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      return NurbsCurve.CreateFromEllipse(this);
    }
    #endregion

    public bool EpsilonEquals(Ellipse other, double epsilon)
    {
        return m_plane.EpsilonEquals(other.m_plane, epsilon) &&
               FloatingPointCompare.EpsilonEquals(m_radius1, other.m_radius1, epsilon) &&
               FloatingPointCompare.EpsilonEquals(m_radius2, other.m_radius2, epsilon);
    }
  }
}