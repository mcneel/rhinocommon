using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a surface of revolution.
  /// <para>Revolutions can be incomplete (they can form arcs).</para>
  /// </summary>
  //[Serializable]
  public class RevSurface : Surface
  {
    #region static create functions

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix curve and an axis.
    /// <para>This overload accepts a slice start and end angles.</para>
    /// </summary>
    /// <param name="revoluteCurve">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <param name="startAngleRadians">An angle in radias for the start.</param>
    /// <param name="endAngleRadians">An angle in radias for the end.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      IntPtr pConstCurve = revoluteCurve.ConstPointer();
      IntPtr pRevSurface = UnsafeNativeMethods.ON_RevSurface_Create(pConstCurve, ref axisOfRevolution, startAngleRadians, endAngleRadians);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix curve and an axis.
    /// </summary>
    /// <param name="revoluteCurve">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
    /// </example>
    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution)
    {
      return Create(revoluteCurve, axisOfRevolution, 0, 2.0 * Math.PI);
    }

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix line and an axis.
    /// <para>This overload accepts a slice start and end angles.</para>
    /// <para>Results can be (truncated) cones, cylinders and circular hyperboloids, or can fail.</para>
    /// </summary>
    /// <param name="revoluteLine">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <param name="startAngleRadians">An angle in radias for the start.</param>
    /// <param name="endAngleRadians">An angle in radias for the end.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface Create(Line revoluteLine, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      using (LineCurve lc = new LineCurve(revoluteLine))
      {
        return Create(lc, axisOfRevolution, startAngleRadians, endAngleRadians);
      }
    }

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix line and an axis.
    /// <para>If the operation succeeds, results can be (truncated) cones, cylinders and circular hyperboloids.</para>
    /// </summary>
    /// <param name="revoluteLine">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface Create(Line revoluteLine, Line axisOfRevolution)
    {
      return Create(revoluteLine, axisOfRevolution, 0, 2.0 * Math.PI);
    }

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix polyline and an axis.
    /// <para>This overload accepts a slice start and end angles.</para>
    /// </summary>
    /// <param name="revolutePolyline">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <param name="startAngleRadians">An angle in radias for the start.</param>
    /// <param name="endAngleRadians">An angle in radias for the end.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface Create(Polyline revolutePolyline, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      using (PolylineCurve plc = new PolylineCurve(revolutePolyline))
      {
        return Create(plc, axisOfRevolution, startAngleRadians, endAngleRadians);
      }
    }

    /// <summary>
    /// Constructs a new surface of revolution from a generatrix polyline and an axis.
    /// </summary>
    /// <param name="revolutePolyline">A generatrix.</param>
    /// <param name="axisOfRevolution">An axis.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface Create(Polyline revolutePolyline, Line axisOfRevolution)
    {
      return Create(revolutePolyline, axisOfRevolution, 0, 2.0 * Math.PI);
    }

    /// <summary>
    /// Constructs a new surface of revolution from the values of a cone.
    /// </summary>
    /// <param name="cone">A cone.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface CreateFromCone(Cone cone)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cone_RevSurfaceForm(ref cone);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }

    /// <summary>
    /// Constructs a new surface of revolution from the values of a cylinder.
    /// </summary>
    /// <param name="cylinder">A cylinder.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface CreateFromCylinder(Cylinder cylinder)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cylinder_RevSurfaceForm(ref cylinder);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }

    /// <summary>
    /// Constructs a new surface of revolution from the values of a sphere.
    /// </summary>
    /// <param name="sphere">A sphere.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface CreateFromSphere(Sphere sphere)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Sphere_RevSurfaceForm(ref sphere);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }

    /// <summary>
    /// Constructs a new surface of revolution from the values of a torus.
    /// </summary>
    /// <param name="torus">A torus.</param>
    /// <returns>A new surface of revolution, or null if any of the inputs is invalid or on error.</returns>
    public static RevSurface CreateFromTorus(Torus torus)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Torus_RevSurfaceForm(ref torus);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }
    #endregion

    #region constructors
    internal RevSurface(IntPtr ptr, object parent)
      : base(ptr, parent)
    { }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected RevSurface(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new RevSurface(IntPtr.Zero, null);
    }
    #endregion

  }
}
