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
  [Serializable]
  public class Hatch : GeometryBase
  {
    internal Hatch(IntPtr native_ptr, object parent)
      : base(native_ptr, parent, -1)
    { }

    /// <summary>
    /// Protected constructor for internal use.
    /// </summary>
    /// <param name="info">Serialization data.</param>
    /// <param name="context">Serialization stream.</param>
    protected Hatch(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Hatch(IntPtr.Zero, null);
    }

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
      Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer geometry = new Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer();
      IntPtr pParentRhinoObject = IntPtr.Zero;

#if RHINO_SDK
      if (IsDocumentControlled)
      {
        Rhino.DocObjects.RhinoObject rhobj = ParentRhinoObject();
        if (rhobj != null)
          pParentRhinoObject = rhobj.ConstPointer();
      }
#endif
      IntPtr pGeometryArray = geometry.NonConstPointer();
      IntPtr pConstThis = ConstPointer();

      UnsafeNativeMethods.ON_Hatch_Explode(pConstThis, pParentRhinoObject, pGeometryArray);
      GeometryBase[] rc = geometry.ToNonConstArray();
      geometry.Dispose();
      return rc;
    }

    /// <summary>
    /// Gets or sets the index of the pattern in the document hatch pattern table.
    /// </summary>
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
