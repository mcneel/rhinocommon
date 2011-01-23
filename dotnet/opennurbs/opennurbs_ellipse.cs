using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  [Serializable]
  public struct Ellipse
  {
    #region members
    internal Plane m_plane;
    internal double m_radius1;
    internal double m_radius2;
    #endregion

    #region constructors
    /// <summary>
    /// Create a new Ellipse from base plane and both principal radii.
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
    /// Create a new Ellipse from three points.
    /// </summary>
    /// <param name="center"></param>
    /// <param name="second"></param>
    /// <param name="third"></param>
    public Ellipse(Point3d center, Point3d second, Point3d third)
    {
      //David: This constructor needs better comments. What do point 2 and 3 do?
      m_plane = new Plane( center, second, third );
      m_radius1 = center.DistanceTo(second);
      m_radius2 = center.DistanceTo(third);
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the Base pane of the Ellipse.
    /// </summary>
    public Plane Plane 
    { 
      get { return m_plane; }
      set { m_plane = value; }
    }

    /// <summary>
    /// Gets or sets the Radius of the ellipse along the Base plane X direction.
    /// </summary>
    public double Radius1 
    {
      get { return m_radius1; }
      set { m_radius1 = value; }
    }

    /// <summary>
    /// Gets or sets the Radius of the ellipse along the Base plane Y direction.
    /// </summary>
    public double Radius2 
    {
      get { return m_radius2; }
      set { m_radius2 = value; }
    }
    #endregion

    #region methods
    /// <summary>
    /// Create a nurbs curve representation of this ellipse. 
    /// This amounts to the same as calling NurbsCurve.CreateFromEllipse().
    /// </summary>
    /// <returns>A nurbs curve representation of this ellipse or null if no such representation could be made.</returns>
    public NurbsCurve ToNurbsCurve()
    {
      return NurbsCurve.CreateFromEllipse(this);
    }
    #endregion
  }
}