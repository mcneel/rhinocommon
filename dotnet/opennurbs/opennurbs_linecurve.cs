#pragma warning disable 1591
using System;
using Rhino.Display;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  [Serializable]
  public class LineCurve : Curve, ISerializable
  {
    public LineCurve()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }
    public LineCurve(LineCurve other)
    {
      IntPtr pOther = IntPtr.Zero;
      if( null != other )
        pOther = other.ConstPointer();
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New(pOther);
      ConstructNonConstObject(ptr);
    }
    public LineCurve(Point2d from, Point2d to)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New2(from,to);
      ConstructNonConstObject(ptr);
    }
    /// <example>
    /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
    /// </example>
    public LineCurve(Point3d from, Point3d to)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New3(from,to);
      ConstructNonConstObject(ptr);
    }
    public LineCurve(Line line)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New3(line.From, line.To);
      ConstructNonConstObject(ptr);
    }
    public LineCurve(Line line, double t0, double t1)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LineCurve_New4(line.From, line.To, t0, t1);
      ConstructNonConstObject(ptr);
    }
    internal LineCurve(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }

    // serialization constructor
    protected LineCurve(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new LineCurve(IntPtr.Zero, null, -1);
    }

#if RHINO_SDK
    internal override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr ptr = ConstPointer();
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      UnsafeNativeMethods.ON_LineCurve_Draw(ptr, pDisplayPipeline, argb, thickness);
    }
#endif

    /// <summary>
    /// Gets or sets the Line data inside this curve.
    /// </summary>
    public Line Line
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Line line = new Line();
        UnsafeNativeMethods.ON_LineCurve_GetSetLine(ptr, false, ref line);
        return line;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_LineCurve_GetSetLine(ptr, true, ref value);
      }
    }
  }
}
