using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class RevSurface : Surface, ISerializable
  {
    #region static create functions

    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      IntPtr pConstCurve = revoluteCurve.ConstPointer();
      IntPtr pRevSurface = UnsafeNativeMethods.ON_RevSurface_Create(pConstCurve, ref axisOfRevolution, startAngleRadians, endAngleRadians);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
    /// </example>
    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution)
    {
      return Create(revoluteCurve, axisOfRevolution, 0, 2.0 * Math.PI);
    }
    public static RevSurface Create(Line revoluteLine, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      using (LineCurve lc = new LineCurve(revoluteLine))
      {
        return Create(lc, axisOfRevolution, startAngleRadians, endAngleRadians);
      }
    }
    public static RevSurface Create(Line revoluteLine, Line axisOfRevolution)
    {
      return Create(revoluteLine, axisOfRevolution, 0, 2.0 * Math.PI);
    }
    public static RevSurface Create(Polyline revolutePolyline, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      using (PolylineCurve plc = new PolylineCurve(revolutePolyline))
      {
        return Create(plc, axisOfRevolution, startAngleRadians, endAngleRadians);
      }
    }
    public static RevSurface Create(Polyline revolutePolyline, Line axisOfRevolution)
    {
      return Create(revolutePolyline, axisOfRevolution, 0, 2.0 * Math.PI);
    }

    public static RevSurface CreateFromCone(Cone cone)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cone_RevSurfaceForm(ref cone);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }
    public static RevSurface CreateFromCylinder(Cylinder cylinder)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cylinder_RevSurfaceForm(ref cylinder);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }
    public static RevSurface CreateFromSphere(Sphere sphere)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Sphere_RevSurfaceForm(ref sphere);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null);
    }
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

    // serialization constructor
    protected RevSurface(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new RevSurface(IntPtr.Zero, null);
    }
    #endregion

  }
}
