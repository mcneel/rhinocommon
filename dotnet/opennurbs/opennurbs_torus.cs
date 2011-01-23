using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 144)]
  [Serializable]
  public struct Torus
  {
    internal Plane m_majorCirclePlane;
    internal double m_majorRadius;
    internal double m_minorRadius;

    #region constructors
    /// <summary>
    /// Create a new Torus from base pane and two radii.
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
    /// Convert this Torus to a NurbsSurface representation. 
    /// This is synonymous with calling NurbsSurface.CreateFromTorus().
    /// </summary>
    /// <returns>A nurbs surface representation of this torus or null.</returns>
    public NurbsSurface ToNurbsSurface()
    {
      return NurbsSurface.CreateFromTorus(this);
    }

    /// <summary>
    /// Convert this Torus to a RevSurface representation. 
    /// This is synonymous with calling RevSurface.CreateFromTorus().
    /// </summary>
    /// <returns>A surface of revolution representation of this torus or null.</returns>
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
  }
}
