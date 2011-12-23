using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a plane surface, with plane and two intervals.
  /// </summary>
  [Serializable]
  public class PlaneSurface : Surface, ISerializable
  {
    internal PlaneSurface(IntPtr ptr, object parent) 
      : base(ptr, parent)
    { }

    /// <summary>
    /// Initializes a plane surface with x and y intervals.
    /// </summary>
    /// <param name="plane">The plane.</param>
    /// <param name="xExtents">The x interval of the plane that defines the rectangle.
    /// The corresponding evaluation interval domain is set so that it matches the
    /// extents interval.</param>
    /// <param name="yExtents">The y interval of the plane that defines the rectangle.
    /// The corresponding evaluation interval domain is set so that it matches the
    /// extents interval.</param>
    public PlaneSurface(Plane plane, Interval xExtents, Interval yExtents)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PlaneSurface_New(ref plane, xExtents, yExtents);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected PlaneSurface(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    /// <summary>
    /// Makes a plane that includes a line and a vector and goes through a bounding box.
    /// </summary>
    /// <param name="lineInPlane"></param>
    /// <param name="vectorInPlane"></param>
    /// <param name="box">box to cut through.</param>
    /// <returns>new PlaneSurface on success, NULL on error.</returns>
    public static PlaneSurface CreateThroughBox(Line lineInPlane, Vector3d vectorInPlane, BoundingBox box)
    {
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoPlaneThroughBox(ref lineInPlane, vectorInPlane, ref box);
      if (IntPtr.Zero == ptr)
        return null;
      return new PlaneSurface(ptr, null);
    }

    /// <summary>
    /// Extends a plane so that is goes through a bounding box.
    /// </summary>
    /// <param name="plane"></param>
    /// <param name="box"></param>
    /// <returns>new PlaneSurface on success, NULL on error.</returns>
    public static PlaneSurface CreateThroughBox(Plane plane, BoundingBox box)
    {
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoPlaneThroughBox2(ref plane, ref box);
      if (IntPtr.Zero == ptr)
        return null;
      return new PlaneSurface(ptr, null);
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new PlaneSurface(IntPtr.Zero, null);
    }
  }

  /// <summary>
  /// Represents a planar surface that is used as clipping plane in viewports.
  /// </summary>
  [Serializable]
  public class ClippingPlaneSurface : PlaneSurface, ISerializable
  {
    internal ClippingPlaneSurface(IntPtr ptr, object parent)
      : base(ptr, parent)
    { }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected ClippingPlaneSurface(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new ClippingPlaneSurface(IntPtr.Zero, null);
    }

    /// <summary>
    /// Gets or sets the clipping plane.
    /// </summary>
    public Plane Plane
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Plane p = new Plane();
        UnsafeNativeMethods.ON_ClippingPlaneSurface_GetPlane(pConstThis, ref p);
        return p;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_ClippingPlaneSurface_SetPlane(pThis, ref value);
      }
    }

    /// <summary>
    /// Returns Ids of viewports that this clipping plane is supposed to clip.
    /// </summary>
    /// <returns>An array of globally unique ideantifiers (Guids) to the viewports.</returns>
    public Guid[] ViewportIds()
    {
      IntPtr pConstThis = ConstPointer();
      int count = UnsafeNativeMethods.ON_ClippingPlaneSurface_ViewportIdCount(pConstThis);
      Guid[] rc = new Guid[count];
      for (int i = 0; i < count; i++)
      {
        rc[i] = UnsafeNativeMethods.ON_ClippingPlaneSurface_ViewportId(pConstThis, i);
      }
      return rc;
    }
   
  }
}
