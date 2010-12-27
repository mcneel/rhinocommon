using System;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Geometry;
using Rhino.Display;

namespace Rhino.Geometry
{
  /// <summary>
  /// ArcCurve is used to represent arcs and circles.
  /// ArcCurve.IsCircle returns true if the curve is a complete circle.
  /// Details:
  /// an ArcCurve is a subcurve of a circle, with a constant speed
  /// parameterization. The parameterization is	an affine linear
  /// reparameterzation of the underlying arc	m_arc onto the domain m_t.
  /// A valid ArcCurve has Radius()>0 and  0&lt;AngleRadians()&lt;=2*PI
  /// and a strictly increasing Domain(). 
  /// </summary>
  public class ArcCurve : Curve
  {
    #region constructors
    internal ArcCurve(IntPtr ptr, object parent, int subobject_index)
      : base(ptr, parent, subobject_index)
    {
    }

    public ArcCurve()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }
    public ArcCurve(ArcCurve other)
    {
      IntPtr pOther = IntPtr.Zero;
      if (null != other)
        pOther = other.ConstPointer();
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New(pOther);
      ConstructNonConstObject(ptr);
    }
    public ArcCurve(Arc arc)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New2(ref arc);
      ConstructNonConstObject(ptr);
    }
    public ArcCurve(Arc arc, double t0, double t1)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New3(ref arc, t0, t1);
      ConstructNonConstObject(ptr);
    }
    public ArcCurve(Circle circle)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New4(ref circle);
      ConstructNonConstObject(ptr);
    }
    public ArcCurve(Circle circle, double t0, double t1)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_ArcCurve_New5(ref circle, t0, t1);
      ConstructNonConstObject(ptr);
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new ArcCurve(IntPtr.Zero, null, -1);
    }

    #endregion

    #region properties
    const int idxRadius = 0;
    const int idxAngleRadians = 1;
    const int idxAngleDegrees = 2;

    /// <summary>
    /// Gets or sets the Arc that is contained within this ArcCurve.
    /// </summary>
    public Arc Arc
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Arc rc = new Arc();
        UnsafeNativeMethods.ON_ArcCurve_GetArc(ptr, ref rc);
        return rc;
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not this curve can be represented by a complete circle.
    /// </summary>
    public bool IsCompleteCircle
    {
      get
      {
        //Do not use IsCircle as a property name. IsCircle is a function on the base class
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ArcCurve_IsCircle(ptr);
      }
    }

    /// <summary>
    /// Gets the radius of this ArcCurve.
    /// </summary>
    public double Radius
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ArcCurve_GetDouble(ptr, idxRadius);
      }
    }

    /// <summary>
    /// Gets the angles of this arc in radians.
    /// </summary>
    public double AngleRadians
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ArcCurve_GetDouble(ptr, idxAngleRadians);
      }
    }

    /// <summary>
    /// Gets the angles of this arc in degrees.
    /// </summary>
    public double AngleDegrees
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_ArcCurve_GetDouble(ptr, idxAngleDegrees);
      }
    }
    #endregion

    private IntPtr CurveDisplay()
    {
      if (IntPtr.Zero == m_pCurveDisplay)
      {
        IntPtr pThis = ConstPointer();
        m_pCurveDisplay = UnsafeNativeMethods.CurveDisplay_FromArcCurve(pThis);
      }
      return m_pCurveDisplay;
    }

    internal sealed override void Draw(DisplayPipeline pipeline, System.Drawing.Color color, int thickness)
    {
      IntPtr pDisplayPipeline = pipeline.NonConstPointer();
      int argb = color.ToArgb();
      IntPtr pCurveDisplay = CurveDisplay();
      UnsafeNativeMethods.CurveDisplay_Draw(pCurveDisplay, pDisplayPipeline, argb, thickness);
    }
  }
}