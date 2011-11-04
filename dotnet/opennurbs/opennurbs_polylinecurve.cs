#pragma warning disable 1591
using System;
using Rhino.Display;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class PolylineCurve : Curve, ISerializable
  {
    #region constructors
    public PolylineCurve()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    public PolylineCurve(PolylineCurve other)
    {
      IntPtr pOther= IntPtr.Zero;
      if (null != other)
        pOther = other.ConstPointer();
      IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New(pOther);
      ConstructNonConstObject(ptr);
    }

    public PolylineCurve(System.Collections.Generic.IEnumerable<Point3d> points)
    {
      int count;
      Point3d[] ptArray = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      IntPtr ptr;
      if (null == ptArray || count < 1)
      {
        ptr = UnsafeNativeMethods.ON_PolylineCurve_New(IntPtr.Zero);
      }
      else
      {
        ptr = UnsafeNativeMethods.ON_PolylineCurve_New2(count, ptArray);
      }
      ConstructNonConstObject(ptr);
    }

    internal PolylineCurve(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }

    // serialization constructor
    protected PolylineCurve(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
    #endregion

    internal static PolylineCurve FromArray(Point3d[] points)
    {
      int count = points.Length;
      IntPtr ptr = UnsafeNativeMethods.ON_PolylineCurve_New2(count, points);
      return new PolylineCurve(ptr, null, -1);
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new PolylineCurve(IntPtr.Zero, null, -1);
    }

#if RHINO_SDK
    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr ptr = ConstPointer();
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.ON_PolylineCurve_Draw(ptr, pDisplayPipeline, argb, thickness);
    }
#endif

    /// <summary>
    /// number of points in polyline
    /// </summary>
    public int PointCount
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_PolylineCurve_PointCount(ptr);
      }
    }
	
    public Point3d Point(int index)
    {
      IntPtr ptr = ConstPointer();
      Point3d pt = new Point3d();
      UnsafeNativeMethods.ON_PolylineCurve_GetSetPoint(ptr, index, ref pt, false);
      return pt;
    }

    public void SetPoint(int index, Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_PolylineCurve_GetSetPoint(ptr, index, ref point, true);
    }
  }
}
