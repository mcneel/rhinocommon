using System;

namespace Rhino.Geometry
{
  public class PlaneSurface : Surface
  {
    internal PlaneSurface(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref) 
      : base(ptr, parent_object, obj_ref)
    { }

    /// <summary>
    /// Constructs a PlaneSurface with x and y intervals
    /// </summary>
    /// <param name="plane">The plane.</param>
    /// <param name="xExtents">The x interval of the plane that defines the rectangle.
    /// The corresponding evaluation interval domain is set so that it matches the
    /// extents interval</param>
    /// <param name="yExtents">The y interval of the plane that defines the rectangle.
    /// The corresponding evaluation interval domain is set so that it matches the
    /// extents interval</param>
    public PlaneSurface(Plane plane, Interval xExtents, Interval yExtents)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PlaneSurface_New(ref plane, xExtents, yExtents);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Make a plane that includes a line and a vector and goes through a bounding box
    /// </summary>
    /// <param name="lineInPlane"></param>
    /// <param name="vectorInPlane"></param>
    /// <param name="box">box to cut through</param>
    /// <returns>new PlaneSurface on success, NULL on error</returns>
    public static PlaneSurface CreateThroughBox(Line lineInPlane, Vector3d vectorInPlane, BoundingBox box)
    {
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoPlaneThroughBox(ref lineInPlane, vectorInPlane, ref box);
      if (IntPtr.Zero == ptr)
        return null;
      return new PlaneSurface(ptr, null, null);
    }

    /// <summary>
    /// Extend a plane so that is goes through a bounding box
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="box"></param>
    /// <returns>new PlaneSurface on success, NULL on error</returns>
    public static PlaneSurface CreateThroughBox(Plane plane, BoundingBox box)
    {
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoPlaneThroughBox2(ref plane, ref box);
      if (IntPtr.Zero == ptr)
        return null;
      return new PlaneSurface(ptr, null, null);
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new PlaneSurface(IntPtr.Zero, null, null);
    }
  }

  public class ClippingPlaneSurface : PlaneSurface
  {
    internal ClippingPlaneSurface(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(ptr, parent_object, obj_ref)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new ClippingPlaneSurface(IntPtr.Zero, null, null);
    }
  }
}
