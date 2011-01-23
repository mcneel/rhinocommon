using System;
using System.Runtime.InteropServices;

namespace Rhino.Geometry
{
  [StructLayout(LayoutKind.Sequential, Pack = 8, Size = 136)]
  [Serializable]
  public struct Sphere
  {
    #region members
    internal Plane m_plane;
    internal double m_radius;
    #endregion

    #region constructors
    /// <example>
    /// <code source='examples\vbnet\ex_addsphere.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addsphere.cs' lang='cs'/>
    /// <code source='examples\py\ex_addsphere.py' lang='py'/>
    /// </example>
    public Sphere(Point3d center, double radius)
    {
      m_plane = Plane.WorldXY;
      m_plane.Origin = center;
      m_radius = radius;
    }
    public Sphere(Plane equatorialPlane, double radius)
    {
      m_plane = equatorialPlane;
      m_radius = radius;
    }
    #endregion

    #region properties
    public bool IsValid
    {
      get
      {
        return RhinoMath.IsValidDouble(m_radius) && m_radius > 0.0 && m_plane.IsValid;
      }
    }

    /// <summary>
    /// Gets the world aligned boundingbox for this Sphere. 
    /// If the Sphere is Invalid, an empty box is returned.
    /// </summary>
    public BoundingBox BoundingBox
    {
      get
      {
        if (!m_plane.IsValid) { return BoundingBox.Empty; }
        if (!RhinoMath.IsValidDouble(m_radius)) { return BoundingBox.Empty; }

        double r = Math.Abs(m_radius);
        return new BoundingBox(m_plane.Origin - new Vector3d(r, r, r),
                               m_plane.Origin + new Vector3d(r, r, r));
      }
    }

    /// <summary>
    /// Gets or sets the diameter for this Sphere.
    /// </summary>
    public double Diameter
    {
      get { return 2.0 * m_radius; }
      set { m_radius = value * 0.5; }
    }

    /// <summary>
    /// Gets or sets the Radius for this Sphere.
    /// </summary>
    public double Radius
    {
      get { return m_radius; }
      set { m_radius = value; }
    }

    /// <summary>
    /// Gets or sets the Equatorial plane for this sphere.
    /// </summary>
    public Plane EquitorialPlane
    {
      get
      {
        return m_plane;
      }
      set
      {
        m_plane = value;
      }
    }

    /// <summary>
    /// Gets or sets the center point for the sphere.
    /// </summary>
    public Point3d Center
    {
      get
      {
        return m_plane.Origin;
      }
      set
      {
        m_plane.Origin = value;
      }
    }

    /// <summary>
    /// Gets the point at the North Pole of the sphere.
    /// </summary>
    public Point3d NorthPole
    {
      get
      {
        return PointAt(0.0, 0.5 * Math.PI);
      }
    }

    /// <summary>
    /// Gets the point at the South Pole of the sphere.
    /// </summary>
    public Point3d SouthPole
    {
      get
      {
        return PointAt(0.0, -0.5 * Math.PI);
      }
    }
    #endregion

    #region methods
    #region evaluators
    public Circle LatitudeRadians(double radians)
    {
      Point3d p1 = PointAt(0.0, radians);
      Point3d p2 = PointAt(0.5 * Math.PI, radians);
      Point3d p3 = PointAt(Math.PI, radians);
      return new Circle(p1, p2, p3);
    }
    public Circle LatitudeDegrees(double degrees)
    {
      return LatitudeRadians(RhinoMath.ToRadians(degrees));
    }
    public Circle LongitudeRadians(double radians)
    {
      Point3d p0 = PointAt(radians, 0.0);
      Point3d p2 = PointAt(radians + Math.PI, 0.0);
      return new Circle(p0, NorthPole, p2);
    }
    public Circle LongitudeDegrees(double degrees)
    {
      return LongitudeRadians(RhinoMath.ToRadians(degrees));
    }

    /// <summary>
    /// </summary>
    /// <param name="longitudeRadians">[0,2pi]</param>
    /// <param name="latitudeRadians">[-pi/2,pi/2]</param>
    /// <returns></returns>
    public Point3d PointAt(double longitudeRadians, double latitudeRadians)
    {
      return m_radius * NormalAt(longitudeRadians, latitudeRadians) + m_plane.Origin;
    }

    /// <summary>
    /// </summary>
    /// <param name="longitudeRadians">[0,2pi]</param>
    /// <param name="latitudeRadians">[-pi/2,pi/2]</param>
    /// <returns></returns>
    public Vector3d NormalAt(double longitudeRadians, double latitudeRadians)
    {
      return Math.Cos(latitudeRadians) * (Math.Cos(longitudeRadians) * m_plane.XAxis
        + Math.Sin(longitudeRadians) * m_plane.YAxis)
        + Math.Sin(latitudeRadians) * m_plane.ZAxis;
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead.")]
    public Point3d ClosestPointTo(Point3d testPoint)
    {
      return ClosestPoint(testPoint);
    }
    /// <summary>
    /// returns point on sphere that is closest to given point
    /// </summary>
    /// <param name="testPoint"></param>
    /// <returns></returns>
    public Point3d ClosestPoint(Point3d testPoint)
    {
      Vector3d v = testPoint - m_plane.Origin;
      v.Unitize();
      return m_plane.Origin + m_radius * v;
    }
    #endregion

    /// <summary>
    /// Rotate the sphere about the center point.
    /// </summary>
    /// <param name="sinAngle">sin(angle)</param>
    /// <param name="cosAngle">cos(angle)</param>
    /// <param name="axisOfRotation"></param>
    public bool Rotate(double sinAngle, double cosAngle, Vector3d axisOfRotation)
    {
      return Rotate(sinAngle, cosAngle, axisOfRotation, m_plane.Origin);
    }

    /// <summary>
    /// Rotate the sphere about the center point.
    /// </summary>
    /// <param name="angleRadians">Angle of rotation (in radians)</param>
    /// <param name="axisOfRotation">Rotation axis.</param>
    public bool Rotate(double angleRadians, Vector3d axisOfRotation)
    {
      double sinAngle = Math.Sin(angleRadians);
      double cosAngle = Math.Cos(angleRadians);
      return Rotate(sinAngle, cosAngle, axisOfRotation, m_plane.Origin);
    }

    /// <summary>
    /// Rotate the sphere about a point and an axis.
    /// </summary>
    /// <param name="sinAngle">sin(angle)</param>
    /// <param name="cosAngle">cod(angle)</param>
    /// <param name="axisOfRotation">Axis of rotation.</param>
    /// <param name="centerOfRotation">Center of rotation.</param>
    public bool Rotate(double sinAngle, double cosAngle, Vector3d axisOfRotation, Point3d centerOfRotation)
    {
      return m_plane.Rotate(sinAngle, cosAngle, axisOfRotation, centerOfRotation);
    }

    /// <summary>
    /// rotate sphere about a point and axis
    /// </summary>
    /// <param name="angleRadians">Rotation angle (in Radians)</param>
    /// <param name="axisOfRotation">Axis of rotation.</param>
    /// <param name="centerOfRotation">Center of rotation.</param>
    public bool Rotate(double angleRadians, Vector3d axisOfRotation, Point3d centerOfRotation)
    {
      double sinAngle = Math.Sin(angleRadians);
      double cosAngle = Math.Cos(angleRadians);
      return Rotate(sinAngle, cosAngle, axisOfRotation, centerOfRotation);
    }

    /// <summary>
    /// Translate (move) the sphere along a motion vector.
    /// </summary>
    /// <param name="delta">Motion vector.</param>
    public bool Translate(Vector3d delta)
    {
      return m_plane.Translate(delta);
    }

    /// <summary>
    /// Transform the Sphere. Note that non-similarity preserving transformations 
    /// cannot be applied to a sphere as that would result in an ellipsoid.
    /// </summary>
    /// <param name="xform">Transformation matrix to apply.</param>
    /// <returns>True on success, false on failure.</returns>
    public bool Transform(Transform xform)
    {
      Circle xc = new Circle(m_plane, m_radius);
      bool rc = xc.Transform(xform);
      if (rc)
      {
        m_plane = xc.m_plane;
        m_radius = xc.m_radius;
      }
      return rc;
    }

    /// <summary>
    /// Convert this Sphere to a NurbsSurface representation. 
    /// This is synonymous with calling NurbsSurface.CreateFromSphere().
    /// </summary>
    /// <returns>A nurbs surface representation of this sphere or null.</returns>
    public NurbsSurface ToNurbsSurface()
    {
      return NurbsSurface.CreateFromSphere(this);
    }

    /// <summary>
    /// Convert this Sphere to a RevSurface representation. 
    /// This is synonymous with calling RevSurface.CreateFromSphere().
    /// </summary>
    /// <returns>A surface of revolution representation of this sphere or null.</returns>
    public RevSurface ToRevSurface()
    {
      return RevSurface.CreateFromSphere(this);
    }
    #endregion

    /* Moved to static Create functions on NurbsSurface and RevSurface
    /// <summary>
    /// parameterization of NURBS surface does not match sphere's transcendental paramaterization
    /// </summary>
    /// <returns></returns>
    public Geometry.NurbsSurface GetNurbsForm()
    {
      IntPtr pSurface = UnsafeNativeMethods.ON_Sphere_GetNurbsForm(ref this);
      if (IntPtr.Zero == pSurface)
        return null;
      return new Geometry.NurbsSurface(pSurface, false, 0);
    }
    
    /// <summary>
    /// Creates a surface of revolution definition of the sphere
    /// </summary>
    /// <returns>
    /// A surface of revolution or NULL if the sphere is not valid
    /// </returns>
    public Geometry.RevSurface RevSurfaceForm()
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Sphere_RevSurfaceForm(ref this);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, false, 0);
    }
    */
  }
}
