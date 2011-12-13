#pragma warning disable 1591
using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class Point : GeometryBase, ISerializable
  {
    internal Point(IntPtr native_pointer, object parent)
      : base(native_pointer, parent, -1)
    { }

    public Point(Point3d location)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Point_New(location);
      ConstructNonConstObject(ptr);
    }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected Point(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    public Point3d Location
    {
      get
      {
        Point3d pt = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_Point_GetSetPoint(ptr, false, ref pt);
        return pt;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_Point_GetSetPoint(ptr, true, ref value);
      }
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Point(IntPtr.Zero, null);
    }
  }
}
