using System;

namespace Rhino.Geometry
{
  public class RevSurface : Surface
  {
    #region static create functions
    /// <example>
    /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
    /// </example>
    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution)
    {
      return Create(revoluteCurve, axisOfRevolution, 0, 2.0 * Math.PI);
    }
    public static RevSurface Create(Curve revoluteCurve, Line axisOfRevolution, double startAngleRadians, double endAngleRadians)
    {
      IntPtr pConstCurve = revoluteCurve.ConstPointer();
      IntPtr pRevSurface = UnsafeNativeMethods.ON_RevSurface_Create(pConstCurve, ref axisOfRevolution, startAngleRadians, endAngleRadians);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null, null);
    }

    public static RevSurface CreateFromCone(Cone cone)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cone_RevSurfaceForm(ref cone);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null, null);
    }
    public static RevSurface CreateFromCylinder(Cylinder cylinder)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Cylinder_RevSurfaceForm(ref cylinder);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null, null);
    }
    public static RevSurface CreateFromSphere(Sphere sphere)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Sphere_RevSurfaceForm(ref sphere);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null, null);
    }
    public static RevSurface CreateFromTorus(Torus torus)
    {
      IntPtr pRevSurface = UnsafeNativeMethods.ON_Torus_RevSurfaceForm(ref torus);
      if (IntPtr.Zero == pRevSurface)
        return null;
      return new RevSurface(pRevSurface, null, null);
    }
    #endregion

    #region constructors
    internal RevSurface(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(ptr, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new RevSurface(IntPtr.Zero, null, null);
    }
    #endregion

  }
}
