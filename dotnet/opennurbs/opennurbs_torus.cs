using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the value of a plane and two radii in a torus that is oriented in three-dimensional space.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  //[Serializable]
  public struct Torus : IEpsilonComparable<Torus>
  {
    internal Plane m_majorCirclePlane;
    internal double m_majorRadius;
    internal double m_minorRadius;

    #region constants
    /// <summary>
    /// Gets an invalid Torus.
    /// </summary>
    public static Torus Unset
    {
      get
      {
        return new Torus(Plane.Unset, RhinoMath.UnsetValue, RhinoMath.UnsetValue);
      }
    }
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new Torus from base pane and two radii.
    /// </summary>
    /// <param name="basePlane">Base plane for major radius circle.</param>
    /// <param name="majorRadius">Radius of circle that lies at the heart of the torus.</param>
    /// <param name="minorRadius">Radius of torus section.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_addtorus.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtorus.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtorus.py' lang='py'/>
    /// </example>
    public Torus(Plane basePlane, double majorRadius, double minorRadius)
    {
      m_majorCirclePlane = basePlane;
      m_minorRadius = minorRadius;
      m_majorRadius = majorRadius;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets a value indicating whether this torus is valid.
    /// </summary>
    public bool IsValid
    {
      get
      {
        if (!RhinoMath.IsValidDouble(m_majorRadius)) { return false; }
        if (!RhinoMath.IsValidDouble(m_minorRadius)) { return false; }
        if (m_majorRadius == 0.0) { return false; }
        if (m_minorRadius == 0.0) { return false; }
        return m_majorCirclePlane.IsValid;
      }
    }

    /// <summary>
    /// Gets or sets the plane for the torus large circle.
    /// </summary>
    public Plane Plane
    {
      get { return m_majorCirclePlane; }
      set { m_majorCirclePlane = value; }
    }

    /// <summary>
    /// Gets or sets the radius of the circle that lies at the heart of the torus.
    /// </summary>
    public double MajorRadius
    {
      get { return m_majorRadius; }
      set { m_majorRadius = value; }
    }

    /// <summary>
    /// Gets or sets the radius of the torus section.
    /// </summary>
    public double MinorRadius
    {
      get { return m_minorRadius; }
      set { m_minorRadius = value; }
    }
    #endregion

    #region methods
    /// <summary>
    /// Converts this torus to its nurbs surface representation. 
    /// This is synonymous with calling NurbsSurface.CreateFromTorus().
    /// </summary>
    /// <returns>A nurbs surface representation of this torus, or null on error.</returns>
    public NurbsSurface ToNurbsSurface()
    {
      return NurbsSurface.CreateFromTorus(this);
    }

    /// <summary>
    /// Convert this torus to a surface of revolution representation. 
    /// This is synonymous with calling RevSurface.CreateFromTorus().
    /// </summary>
    /// <returns>A surface of revolution representation of this torus, or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addtorus.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtorus.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtorus.py' lang='py'/>
    /// </example>
    public RevSurface ToRevSurface()
    {
      return RevSurface.CreateFromTorus(this);
    }
    #endregion

    /// <summary>
    /// Check that all values in other are within epsilon of the values in this
    /// </summary>
    /// <param name="other"></param>
    /// <param name="epsilon"></param>
    /// <returns></returns>
    public bool EpsilonEquals(Torus other, double epsilon)
    {
      return m_majorCirclePlane.EpsilonEquals(other.m_majorCirclePlane, epsilon) &&
             RhinoMath.EpsilonEquals(m_majorRadius, other.m_majorRadius, epsilon) &&
             RhinoMath.EpsilonEquals(m_minorRadius, other.m_minorRadius, epsilon);
    }
  }
}
