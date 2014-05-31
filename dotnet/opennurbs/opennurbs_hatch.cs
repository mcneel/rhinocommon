using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a hatch in planar boundary loop or loops.
  /// This is a 2d entity with a plane defining a local coordinate system.
  /// The loops, patterns, angles, etc are all in this local coordinate system.
  /// The Hatch object manages the plane and loop array
  /// Fill definitions are in the HatchPattern or class derived from HatchPattern
  /// Hatch has an index to get the pattern definition from the pattern table.
  /// </summary>
  //[Serializable]
  public class Hatch : GeometryBase
  {
    internal Hatch(IntPtr native_ptr, object parent)
      : base(native_ptr, parent, -1)
    { }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected Hatch(SerializationInfo info, StreamingContext context)
    //  : base (info, context)
    //{
    //}

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Hatch(IntPtr.Zero, null);
    }

#if RHINO_SDK
    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from a set of curves.
    /// </summary>
    /// <param name="curves">An array, a list or any enumarable set of <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    /// <exception cref="ArgumentNullException">If curves is null.</exception>
    public static Hatch[] Create(IEnumerable<Curve> curves, int hatchPatternIndex, double rotationRadians, double scale)
    {
      if (curves == null) throw new ArgumentNullException("curves");

      Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer curvearray = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer(curves);
      IntPtr pCurveArray = curvearray.NonConstPointer();
      Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer hatcharray = new Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer();
      IntPtr pOutput = hatcharray.NonConstPointer();
      UnsafeNativeMethods.RHC_RhinoCreateHatches(pCurveArray, hatchPatternIndex, rotationRadians, scale, pOutput);
      GeometryBase[] g = hatcharray.ToNonConstArray();
      if( g==null )
        return new Hatch[0];
      List<Hatch> hatches = new List<Hatch>();
      for (int i = 0; i < g.Length; i++)
      {
        Hatch hatch = g[i] as Hatch;
        if (hatch != null)
          hatches.Add(hatch);
      }
      return hatches.ToArray();
    }

    /// <example>
    /// <code source='examples\vbnet\ex_hatchcurve.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_hatchcurve.cs' lang='cs'/>
    /// <code source='examples\py\ex_hatchcurve.py' lang='py'/>
    /// </example>
    /// <summary>
    /// Constructs an array of <see cref="Hatch">hatches</see> from one curve.
    /// </summary>
    /// <param name="curve">A <see cref="Curve"/>.</param>
    /// <param name="hatchPatternIndex">The index of the hatch pattern in the document hatch pattern table.</param>
    /// <param name="rotationRadians">The relative rotation of the pattern.</param>
    /// <param name="scale">A scaling factor.</param>
    /// <returns>An array of hatches. The array might be empty on error.</returns>
    public static Hatch[] Create(Curve curve, int hatchPatternIndex, double rotationRadians, double scale)
    {
      return Create(new Curve[] { curve }, hatchPatternIndex, rotationRadians, scale);
    }

    /// <summary>
    /// Generate geometry that would be used to draw the hatch with a given hatch pattern
    /// </summary>
    /// <param name="pattern"></param>
    /// <param name="patternScale"></param>
    /// <param name="bounds"></param>
    /// <param name="lines"></param>
    /// <param name="solidBrep"></param>
    public void CreateDisplayGeometry(DocObjects.HatchPattern pattern, double patternScale, out Curve[] bounds, out Line[] lines, out Brep solidBrep)
    {
      IntPtr const_ptr_this = ConstPointer();
      IntPtr const_ptr_pattern = pattern.ConstPointer();
      using(var curve_array = new Runtime.InteropWrappers.SimpleArrayCurvePointer())
      using(var line_array = new Runtime.InteropWrappers.SimpleArrayLine())
      {
        IntPtr ptr_curves = curve_array.NonConstPointer();
        IntPtr ptr_lines = line_array.NonConstPointer();
        IntPtr ptr_brep = UnsafeNativeMethods.CRhinoHatchPattern_CreateDisplay(const_ptr_this, const_ptr_pattern, patternScale, ptr_curves, ptr_lines);
        solidBrep = (ptr_brep==IntPtr.Zero) ? null : new Brep(ptr_brep, null);
        bounds = curve_array.ToNonConstArray();
        lines = line_array.ToArray();
      }
    }

    /// <summary>
    /// Decomposes the hatch pattern into an array of geometry.
    /// </summary>
    /// <returns>An array of geometry that formed the appearance of the original elements.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_explodehatch.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_explodehatch.cs' lang='cs'/>
    /// <code source='examples\py\ex_explodehatch.py' lang='py'/>
    /// </example>
    public GeometryBase[] Explode()
    {
      using (Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer geometry = new Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer())
      {
        IntPtr pParentRhinoObject = IntPtr.Zero;

        if (IsDocumentControlled)
        {
          Rhino.DocObjects.RhinoObject rhobj = ParentRhinoObject();
          if (rhobj != null)
            pParentRhinoObject = rhobj.ConstPointer();
        }
        IntPtr pGeometryArray = geometry.NonConstPointer();
        IntPtr pConstThis = ConstPointer();

        UnsafeNativeMethods.ON_Hatch_Explode(pConstThis, pParentRhinoObject, pGeometryArray);
        GeometryBase[] rc = geometry.ToNonConstArray();
        return rc;
      }
    }
#endif

    /// <summary>
    /// Gets 3d curves that define the boundaries of the hatch
    /// </summary>
    /// <param name="outer">true to get the outer curves, false to get the inner curves</param>
    /// <returns></returns>
    public Curve[] Get3dCurves(bool outer)
    {
      using (Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer curves = new Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        IntPtr pCurveArray = curves.NonConstPointer();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.ON_Hatch_LoopCurve3d(pConstThis, pCurveArray, outer);
        return curves.ToNonConstArray();
      }
    }

    /// <summary>
    /// Gets or sets the index of the pattern in the document hatch pattern table.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_replacehatchpattern.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_replacehatchpattern.cs' lang='cs'/>
    /// <code source='examples\py\ex_replacehatchpattern.py' lang='py'/>
    /// </example>
    public int PatternIndex
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_PatternIndex(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetPatternIndex(pThis, value);
      }
    }

    /// <summary>
    /// Gets or sets the relative rotation of the pattern.
    /// </summary>
    public double PatternRotation
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_GetRotation(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetRotation(pThis, value);
      }
    }

    /// <summary>
    /// Gets or sets the scaling factor of the pattern.
    /// </summary>
    public double PatternScale
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Hatch_GetScale(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Hatch_SetScale(pThis, value);
      }
    }
  }
}
