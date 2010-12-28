using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Rhino;
using Rhino.Geometry;

namespace Rhino
{
  //public class ON_BrepVertex { }
  //public class ON_BrepEdge { }
  //public struct ON_BrepTrimPoint { }
  //public class ON_BrepTrim { }
  //public class ON_BrepLoop { }
  //public class ON_BrepFaceSide { }
  //public class ON_BrepRegion { }
  //public class ON_BrepRegionTopology { }
}
namespace Rhino.Geometry
{
  /// <summary>
  /// Enumerates all possible Loft types.
  /// </summary>
  public enum LoftType : int
  {
    /// <summary>
    /// Uses chord-length parameterization in the loft direction
    /// </summary>
    Normal = 0,
    /// <summary>
    /// The surface is allowed to move away from the original curves to make a smoother surface.
    /// The surface control points are created at the same locations as the control points
    /// of the loft input curves.
    /// </summary>
    Loose = 1,
    /// <summary>
    /// The surface sticks closely to the original curves. Uses square root of chord-length
    /// parameterization in the loft direction
    /// </summary>
    Tight = 2,
    /// <summary>
    /// The sections between the curves are straight. This is also known as a ruled surface.
    /// </summary>
    Straight = 3,
    /// <summary>
    /// Creates a separate developable surface or polysurface from each pair of curves.
    /// </summary>
    Developable = 4,
    Uniform = 5
  }

  public class Brep : GeometryBase
  {
    #region statics
    /// <summary>
    /// Attempts to convert a generic Geometry object into a Brep.
    /// </summary>
    /// <param name="geometry">Geometry to convert, not all types of GeometryBase can be represented by BReps.</param>
    /// <returns>Brep if a brep form could be created or null if this is not possible. If geometry was of type Brep to 
    /// begin with, the same object is returned, i.e. it is not duplicated.</returns>
    public static Brep TryConvertBrep(GeometryBase geometry)
    {
      if (null == geometry)
        return null;

      Brep brep = geometry as Brep;
      // special case for Breps in an attempt to not be creating copies willy-nilly
      if (brep != null && brep.IsDocumentControlled)
      {
        return brep;
      }

      IntPtr ptr = geometry.ConstPointer();
      IntPtr pNewBrep = UnsafeNativeMethods.ON_Geometry_BrepForm(ptr);
      if (IntPtr.Zero == ptr)
        return null;

      return new Brep(pNewBrep, null, null);
    }

    /// <summary>
    /// Create a Brep using the trimming information of a brep face and a surface. 
    /// Surface must be roughly the same shape and in the same location as the trimming brep face.
    /// </summary>
    /// <param name="trimSource">BrepFace which contains trimmingSource brep.</param>
    /// <param name="surfaceSource">Surface that trims of BrepFace will be applied to.</param>
    /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
    public static Brep CreateTrimmedSurface(BrepFace trimSource, Surface surfaceSource)
    {
      IntPtr pConstBrepFace = trimSource.ConstPointer();
      IntPtr pConstSurface = surfaceSource.ConstPointer();

      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoRetrimSurface(pConstBrepFace, pConstSurface);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// Copy all trims from a Brep face onto a surface.
    /// </summary>
    /// <param name="trimSource">Brep face which defines the trimming curves.</param>
    /// <param name="surfaceSource">The surface to trim.</param>
    /// <param name="tolerance">Tolerance to use for rebuilding 3D trim curves.</param>
    /// <returns>A brep with the shape of surfaceSource and the trims of trimSource or null on failure.</returns>
    public static Brep CopyTrimCurves(BrepFace trimSource, Surface surfaceSource, double tolerance)
    {
      IntPtr pConstBrepFace = trimSource.ConstPointer();
      IntPtr pConstSurface = surfaceSource.ConstPointer();

      IntPtr ptr = UnsafeNativeMethods.ON_Brep_CopyTrims(pConstBrepFace, pConstSurface, tolerance);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// Create new Brep that matches a bounding box
    /// </summary>
    /// <param name="box"></param>
    /// <returns></returns>
    public static Brep CreateFromBox(BoundingBox box)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Brep_FromBox(box.Min, box.Max);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }
    /// <summary>
    /// Create new Brep that matches an aligned box.
    /// </summary>
    /// <param name="box">Box to match.</param>
    /// <returns>A Brep with 6 faces that is similar to the Box.</returns>
    public static Brep CreateFromBox(Box box)
    {
      return CreateFromBox(box.GetCorners());
    }
    /// <summary>
    /// Create new Brep from 8 corner points
    /// </summary>
    /// <param name="corners">
    /// 8 points defining the box corners arranged as the vN lables indicate.
    /// v7_______e6_____v6
    /// |\             |\
    /// | e7           | e5
    /// |  \ ______e4_____\ 
    /// e11 v4         |   v5
    /// |   |        e10   |
    /// |   |          |   |
    /// v3--|---e2----v2   e9
    /// \   e8         \   |
    ///  e3 |           e1 |
    ///   \ |            \ |
    ///    \v0_____e0_____\v1
    /// </param>
    /// <returns></returns>
    public static Brep CreateFromBox(System.Collections.Generic.IEnumerable<Point3d> corners)
    {
      Point3d[] box_corners = new Point3d[8];
      if (corners == null) { return null; }

      int i = 0;
      foreach (Point3d p in corners)
      {
        box_corners[i] = p;
        i++;
        if (8 == i) { break; }
      }

      if (i < 8) { return null; }

      IntPtr ptr = UnsafeNativeMethods.ON_Brep_FromBox2(box_corners);
      if (IntPtr.Zero == ptr) { return null; }

      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// Get a Brep definition of a cylinder
    /// </summary>
    /// <param name="cylinder">cylinder.IsFinite() must be true</param>
    /// <param name="capBottom">if true end at cylinder.m_height[0] should be capped</param>
    /// <param name="capTop">if true end at cylinder.m_height[1] should be capped</param>
    /// <returns>
    /// A Brep representation of the cylinder with a single face for the cylinder,
    /// an edge along the cylinder seam, and vertices at the bottom and top ends of this
    /// seam edge. The optional bottom/top caps are single faces with one circular edge
    /// starting and ending at the bottom/top vertex.
    /// </returns>
    public static Brep CreateFromCylinder(Cylinder cylinder, bool capBottom, bool capTop)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Brep_FromCylinder(ref cylinder, capBottom, capTop);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// Get a Brep representation of the cone with a single
    /// face for the cone, an edge along the cone seam, 
    /// and vertices at the base and apex ends of this seam edge.
    /// The optional cap is a single face with one circular edge 
    /// starting and ending at the base vertex.
    /// </summary>
    /// <param name="cone"></param>
    /// <param name="capBottom">if true the base of the cone should be capped</param>
    /// <returns></returns>
    public static Brep CreateFromCone(Cone cone, bool capBottom)
    {
      IntPtr ptr = UnsafeNativeMethods.ONC_ON_BrepCone(ref cone, capBottom);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// Get an Brep form of a surface of revolution.
    /// </summary>
    /// <param name="surface">
    /// </param>
    /// <param name="capStart">
    /// if true, the start of the revolute is not on the axis of revolution,
    /// and the surface of revolution is closed, then a circular cap will be
    /// added to close of the hole at the start of the revolute.
    /// </param>
    /// <param name="capEnd">
    /// if true, the end of the revolute is not on the axis of revolution,
    /// and the surface of revolution is closed, then a circular cap will be
    /// added to close of the hole at the end of the revolute.
    /// </param>
    /// <returns></returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addtruncatedcone.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addtruncatedcone.cs' lang='cs'/>
    /// <code source='examples\py\ex_addtruncatedcone.py' lang='py'/>
    /// </example>
    public static Brep CreateFromRevSurface(RevSurface surface, bool capStart, bool capEnd)
    {
      IntPtr pSurface = surface.ConstPointer();
      IntPtr ptr = UnsafeNativeMethods.ONC_ON_BrepRevSurface(pSurface, capStart, capEnd);
      if (IntPtr.Zero == ptr)
        return null;
      return new Brep(ptr, null, null);
    }

    /// <summary>
    /// make a Brep with one face
    /// </summary>
    /// <param name="corner1"></param>
    /// <param name="corner2"></param>
    /// <param name="corner3"></param>
    /// <param name="tolerance">
    /// minimum edge length without collapsing to a singularity
    /// </param>
    /// <returns></returns>
    public static Brep CreateFromCornerPoints(Point3d corner1, Point3d corner2, Point3d corner3, double tolerance)
    {
      Point3d[] points = new Point3d[] { corner1, corner2, corner3 };
      IntPtr pBrep = UnsafeNativeMethods.RHC_RhinoCreate1FaceBrepFromPoints(3, points, tolerance);
      if (IntPtr.Zero == pBrep)
        return null;
      return new Brep(pBrep, null, null);
    }
    /// <summary>
    /// make a Brep with one face
    /// </summary>
    /// <param name="corner1"></param>
    /// <param name="corner2"></param>
    /// <param name="corner3"></param>
    /// <param name="corner4"></param>
    /// <param name="tolerance">
    /// minimum edge length without collapsing to a singularity
    /// </param>
    /// <returns></returns>
    public static Brep CreateFromCornerPoints(Point3d corner1, Point3d corner2, Point3d corner3, Point3d corner4, double tolerance)
    {
      Point3d[] points = new Point3d[] { corner1, corner2, corner3, corner4 };
      IntPtr pBrep = UnsafeNativeMethods.RHC_RhinoCreate1FaceBrepFromPoints(4, points, tolerance);
      if (IntPtr.Zero == pBrep)
        return null;
      return new Brep(pBrep, null, null);
    }

    /// <summary>
    /// Creates a coons patch from 2, 3, or 4 curves
    /// </summary>
    /// <param name="curves"></param>
    /// <returns>resulting brep or null on failure</returns>
    public static Brep CreateEdgeSurface(System.Collections.Generic.IEnumerable<Curve> curves)
    {
      NurbsCurve[] nurbs_curves = new NurbsCurve[4];
      IntPtr[] pNurbsCurves = new IntPtr[4];
      for (int i = 0; i < 4; i++)
        pNurbsCurves[i] = IntPtr.Zero;
      int index = 0;
      foreach (Curve crv in curves)
      {
        if (null == crv)
          continue;
        NurbsCurve nc = crv as NurbsCurve;
        if (nc == null)
          nc = crv.ToNurbsCurve();
        if (nc == null)
          continue;
        nurbs_curves[index] = nc; // this forces GC to not collect while we are using the pointers
        pNurbsCurves[index] = nc.ConstPointer();
        index++;
        if (index > 3)
          break;
      }
      int count = index;
      if (count < 2 || count > 4)
        return null;
      IntPtr pBrep = UnsafeNativeMethods.RHC_RhinoCreateEdgeSrf(pNurbsCurves[0], pNurbsCurves[1], pNurbsCurves[2], pNurbsCurves[3]);
      if (IntPtr.Zero == pBrep)
        return null;
      return new Brep(pBrep, null, null);
    }

    /// <summary>
    /// Create a set of planar Breps as outlines by the loops.
    /// </summary>
    /// <param name="inputLoops">Curve loops that delineate the planar boundaries.</param>
    /// <returns>An array of Planar Breps.</returns>
    public static Brep[] CreatePlanarBreps(Rhino.Collections.CurveList inputLoops)
    {
      if (null == inputLoops)
        return null;
      Runtime.InteropWrappers.SimpleArrayCurvePointer crvs = new Runtime.InteropWrappers.SimpleArrayCurvePointer(inputLoops.m_items);
      Runtime.INTERNAL_BrepArray breps = new Runtime.INTERNAL_BrepArray();
      IntPtr pInputLoops = crvs.ConstPointer();
      IntPtr pBreps = breps.NonConstPointer();
      int brepCount = UnsafeNativeMethods.RHC_RhinoMakePlanarBreps(pInputLoops, pBreps);
      Brep[] rc = null;
      if (brepCount > 0)
      {
        rc = breps.ToNonConstArray();
      }

      crvs.Dispose();
      breps.Dispose();
      return rc;
    }

    /// <summary>
    /// Create a Brep from a surface.  The resulting Brep has an outer boundary made
    /// from four trims. The trims are ordered so that they run along the south, east,
    /// north, and then west side of the surface's parameter space.
    /// </summary>
    /// <param name="surface"></param>
    /// <returns>resulting brep or null on failure</returns>
    public static Brep CreateFromSurface(Rhino.Geometry.Surface surface)
    {
      if (null == surface)
        return null;
      IntPtr pSurface = surface.ConstPointer();
      IntPtr pNewBrep = UnsafeNativeMethods.ON_Brep_FromSurface(pSurface);
      if (IntPtr.Zero == pNewBrep)
        return null;

      return new Brep(pNewBrep, null, null);
    }

#if USING_V5_SDK
    /// <summary>
    /// Offsets a face including trim information to create a new brep
    /// </summary>
    /// <param name="face">the face to offset</param>
    /// <param name="offsetDistance"></param>
    /// <param name="offsetTolerance">
    ///  Use 0.0 to make a loose offset. Otherwise, the document's absolute tolerance is usually sufficient.
    /// </param>
    /// <param name="bothSides">When true, offset to both sides of the input face.</param>
    /// <param name="createSolid">When true, make a solid object.</param>
    /// <returns>
    /// A new brep if successful. The brep can be disjoint if bothSides is true and createSolid is false,
    /// or if createSolid is true and connecting the offsets with side surfaces fails.
    /// NULL if unsuccessful.
    /// </returns>
    public static Brep CreateFromOffsetFace(BrepFace face, double offsetDistance, double offsetTolerance, bool bothSides, bool createSolid)
    {
      IntPtr pConstFace = face.ConstPointer();
      IntPtr pNewBrep = UnsafeNativeMethods.RHC_RhinoOffsetSurface(pConstFace, offsetDistance, offsetTolerance, bothSides, createSolid);
      if (IntPtr.Zero == pNewBrep)
        return null;
      return new Brep(pNewBrep, null, null);
    }
#endif

    /// <summary>
    /// Creates one or more Breps by lofting through a set of curves.
    /// </summary>
    /// <param name="curves">
    /// The curves to loft through. This function will not perform any curve sorting. You must pass in
    /// curves in the order you want them lofted. This function will not adjust the directions of open
    /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
    /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
    /// adjust the seam of closed curves.
    /// </param>
    /// <param name="start">
    /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point
    /// </param>
    /// <param name="end">
    /// Optional ending point of loft. Use Point3d.Unset if you do not want to include an end point
    /// </param>
    /// <param name="loftType">type of loft to perform</param>
    /// <param name="closed"></param>
    /// <returns>
    /// Creates a closed surface, continuing the surface past the last curve around to the
    /// first curve. Available when you have selected three shape curves
    /// </returns>
    public static Brep[] CreateFromLoft(System.Collections.Generic.IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed)
    {
      return LoftHelper(curves, start, end, loftType, 0, 0, 0.0, closed);
    }
    /// <summary>
    /// Creates one or more Breps by lofting through a set of curves. Input for the loft is simplified by
    /// rebuilding to a specified number of control points.
    /// </summary>
    /// <param name="curves">
    /// The curves to loft through. This function will not perform any curve sorting. You must pass in
    /// curves in the order you want them lofted. This function will not adjust the directions of open
    /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
    /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
    /// adjust the seam of closed curves.
    /// </param>
    /// <param name="start">
    /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point
    /// </param>
    /// <param name="end">
    /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point
    /// </param>
    /// <param name="loftType">type of loft to perform</param>
    /// <param name="closed"></param>
    /// <param name="rebuildPointCount"></param>
    /// <returns>
    /// Creates a closed surface, continuing the surface past the last curve around to the
    /// first curve. Available when you have selected three shape curves
    /// </returns>
    public static Brep[] CreateFromLoftRebuild(System.Collections.Generic.IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed, int rebuildPointCount)
    {
      return LoftHelper(curves, start, end, loftType, 1, rebuildPointCount, 0.0, closed);
    }
    /// <summary>
    /// Creates one or more Breps by lofting through a set of curves. Input for the loft is simplified by
    /// refitting to a specified tolerance
    /// </summary>
    /// <param name="curves">
    /// The curves to loft through. This function will not perform any curve sorting. You must pass in
    /// curves in the order you want them lofted. This function will not adjust the directions of open
    /// curves. Use Curve.DoDirectionsMatch and Curve.Reverse to adjust the directions of open curves.
    /// This function will not adjust the seams of closed curves. Use Curve.ChangeClosedCurveSeam to
    /// adjust the seam of closed curves.
    /// </param>
    /// <param name="start">
    /// Optional starting point of loft. Use Point3d.Unset if you do not want to include a start point
    /// </param>
    /// <param name="end">
    /// Optional ending point of lost. Use Point3d.Unset if you do not want to include an end point
    /// </param>
    /// <param name="loftType">type of loft to perform</param>
    /// <param name="closed"></param>
    /// <param name="refitTolerance"></param>
    /// <returns>
    /// Creates a closed surface, continuing the surface past the last curve around to the
    /// first curve. Available when you have selected three shape curves
    /// </returns>
    public static Brep[] CreateFromLoftRefit(System.Collections.Generic.IEnumerable<Curve> curves, Point3d start, Point3d end, LoftType loftType, bool closed, double refitTolerance)
    {
      return LoftHelper(curves, start, end, loftType, 2, 0, refitTolerance, closed);
    }
    static Brep[] LoftHelper(System.Collections.Generic.IEnumerable<Curve> curves, Point3d start, Point3d end,
      LoftType loftType, int simplifyMethod, int rebuildCount, double refitTol, bool closed)
    {
      Runtime.InteropWrappers.SimpleArrayCurvePointer _curves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer(curves);
      IntPtr pCurves = _curves.ConstPointer();
      Runtime.INTERNAL_BrepArray _breps = new Rhino.Runtime.INTERNAL_BrepArray();
      IntPtr pBreps = _breps.NonConstPointer();
      int count = UnsafeNativeMethods.RHC_RhinoSdkLoft(pCurves, start, end, (int)loftType, simplifyMethod, rebuildCount, refitTol, closed, pBreps);
      _curves.Dispose();
      Brep[] rc = null;
      if (count > 0)
        rc = _breps.ToNonConstArray();
      _breps.Dispose();
      return rc;
    }

    /// <summary>
    /// Compute the Boolean Union of a set of Breps.
    /// </summary>
    /// <param name="breps">Breps to union.</param>
    /// <param name="tolerance">Tolerance to use for union operation.</param>
    /// <returns>An array of Brep results or null on failure.</returns>
    public static Brep[] CreateBooleanUnion(System.Collections.Generic.IEnumerable<Brep> breps, double tolerance)
    {
      if (null == breps)
        return null;

      Runtime.INTERNAL_BrepArray input = new Runtime.INTERNAL_BrepArray();
      foreach (Brep brep in breps)
      {
        if (null == brep)
          continue;
        input.AddBrep(brep, true);
      }
      Runtime.INTERNAL_BrepArray output = new Runtime.INTERNAL_BrepArray();

      IntPtr pInput = input.ConstPointer();
      IntPtr pOutput = output.NonConstPointer();

      int joinCount = UnsafeNativeMethods.RHC_RhinoBooleanUnion(pInput, pOutput, tolerance);
      Brep[] rc = null;
      if (joinCount > 0)
      {
        rc = output.ToNonConstArray();
      }
      input.Dispose();
      output.Dispose();

      return rc;
    }

    static Brep[] BooleanIntDiffHelper(System.Collections.Generic.IEnumerable<Brep> firstSet,
      System.Collections.Generic.IEnumerable<Brep> secondSet,
      double tolerance,
      bool intersection)
    {
      if (null == firstSet || null == secondSet)
        return null;

      Runtime.INTERNAL_BrepArray inputSet1 = new Runtime.INTERNAL_BrepArray();
      foreach (Brep brep in firstSet)
      {
        if (null == brep)
          continue;
        inputSet1.AddBrep(brep, true);
      }

      Runtime.INTERNAL_BrepArray inputSet2 = new Runtime.INTERNAL_BrepArray();
      foreach (Brep brep in secondSet)
      {
        if (null == brep)
          continue;
        inputSet2.AddBrep(brep, true);
      }

      Runtime.INTERNAL_BrepArray output = new Runtime.INTERNAL_BrepArray();

      IntPtr pInputSet1 = inputSet1.ConstPointer();
      IntPtr pInputSet2 = inputSet2.ConstPointer();
      IntPtr pOutput = output.NonConstPointer();

      Brep[] rc = null;
      if (UnsafeNativeMethods.RHC_RhinoBooleanIntDiff(pInputSet1, pInputSet2, pOutput, tolerance, intersection))
      {
        rc = output.ToNonConstArray();
      }

      inputSet1.Dispose();
      inputSet2.Dispose();
      output.Dispose();
      return rc;
    }
    /// <summary>
    /// Compute the Solid Intersection of two sets of Breps.
    /// </summary>
    /// <param name="firstSet">First set of Breps.</param>
    /// <param name="secondSet">Second set of Breps.</param>
    /// <param name="tolerance">Tolerance to use for intersection operation.</param>
    /// <returns>An array of Brep results or null on failure.</returns>
    public static Brep[] CreateBooleanIntersection(System.Collections.Generic.IEnumerable<Brep> firstSet,
      System.Collections.Generic.IEnumerable<Brep> secondSet,
      double tolerance)
    {
      return BooleanIntDiffHelper(firstSet, secondSet, tolerance, true);
    }

    /// <summary>
    /// Compute the Solid Difference of two sets of Breps.
    /// </summary>
    /// <param name="firstSet">First set of Breps (the set to subtract from).</param>
    /// <param name="secondSet">Second set of Breps (the set to subtract).</param>
    /// <param name="tolerance">Tolerance to use for difference operation.</param>
    /// <returns>An array of Brep results or null on failure.</returns>
    public static Brep[] CreateBooleanDifference(System.Collections.Generic.IEnumerable<Brep> firstSet,
      System.Collections.Generic.IEnumerable<Brep> secondSet,
      double tolerance)
    {
      return BooleanIntDiffHelper(firstSet, secondSet, tolerance, false);
    }

    /// <summary>
    /// Joins the breps in the input array at any overlapping edges to form
    /// as few as possible resulting breps. There may be more than one brep in the result array
    /// </summary>
    /// <param name="brepsToJoin"></param>
    /// <param name="tolerance">3d distance tolerance for detecting overlapping edges</param>
    /// <returns>new joined breps on success, null on failure</returns>
    public static Brep[] JoinBreps(System.Collections.Generic.IEnumerable<Brep> brepsToJoin, double tolerance)
    {
      if (null == brepsToJoin)
        return null;

      Runtime.INTERNAL_BrepArray input = new Runtime.INTERNAL_BrepArray();
      foreach (Brep brep in brepsToJoin)
      {
        if (null == brep)
          continue;
        input.AddBrep(brep, true);
      }
      Runtime.INTERNAL_BrepArray output = new Runtime.INTERNAL_BrepArray();

      IntPtr pInput = input.NonConstPointer();
      IntPtr pOutput = output.NonConstPointer();

      int joinCount = UnsafeNativeMethods.RHC_RhinoJoinBreps(pInput, pOutput, tolerance);
      Brep[] rc = null;
      if (joinCount > 0)
      {
        rc = output.ToNonConstArray();

        //David added this on June 9th 2010. Flip Breps that are inside out.
        for (int i = 0; i < rc.Length; i++)
        {
          if (!rc[i].IsSolid) { continue; }
          if (rc[i].SolidOrientation != BrepSolidOrientation.Inward) { continue; }
          rc[i].Flip();
        }
      }
      input.Dispose();
      output.Dispose();
      return rc;
    }
    #endregion

    #region constructors

    internal Brep(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref)
      : base(ptr, parent_object, obj_ref)
    {
      if (null == parent_object && null == obj_ref)
        ApplyMemoryPressure();
    }

    #endregion

    #region properties
    const int idxSolidOrientation = 0;
    internal const int idxFaceCount = 1;
    const int idxIsManifold = 2;
    internal const int idxEdgeCount = 3;

    Rhino.Geometry.Collections.BrepFaceList m_facelist;
    public Rhino.Geometry.Collections.BrepFaceList Faces
    {
      get
      {
        if (null == m_facelist)
          m_facelist = new Rhino.Geometry.Collections.BrepFaceList(this);
        return m_facelist;
      }
    }

    Rhino.Geometry.Collections.BrepEdgeList m_edgelist;
    public Rhino.Geometry.Collections.BrepEdgeList Edges
    {
      get
      {
        if (null == m_edgelist)
          m_edgelist = new Rhino.Geometry.Collections.BrepEdgeList(this);
        return m_edgelist;
      }
    }


    /// <summary>
    /// Test Brep to see if it is a solid. (A "solid" is a closed oriented manifold.)
    /// </summary>
    public bool IsSolid
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Brep_GetInt(ptr, idxSolidOrientation);
        return rc != 0;
      }
    }

    /// <summary>
    /// Gets the Solid orientation state of this Brep.
    /// </summary>
    public BrepSolidOrientation SolidOrientation
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Brep_GetInt(ptr, idxSolidOrientation);
        return (BrepSolidOrientation)rc;
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not the Brep is Manifold. 
    /// Non-Manifold breps have at least one edge that is shared among three or more faces.
    /// </summary>
    public bool IsManifold
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Brep_GetInt(ptr, idxIsManifold);
        return rc != 0;
      }
    }

    /// <summary>
    /// Returns true if the Brep has a single face and that face is geometrically the same
    /// as the underlying surface.  I.e., the face has trivial trimming. In this case, the
    /// surface is m_S[0]. The flag m_F[0].m_bRev records the correspondence between the
    /// surface's natural parametric orientation and the orientation of the Brep.
    /// </summary>
    public bool IsSurface
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Brep_FaceIsSurface(ptr, -1);
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Creates copy of this Brep
    /// </summary>
    /// <returns></returns>
    public override GeometryBase Duplicate()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pNewBrep = UnsafeNativeMethods.ON_Brep_New(ptr);
      return new Brep(pNewBrep, null, null);
    }
    /// <summary>
    /// Same as Duplicate() function, but performs the casting to a Brep for you.
    /// </summary>
    /// <returns></returns>
    public Brep DuplicateBrep()
    {
      Brep rc = Duplicate() as Brep;
      return rc;
    }

    /// <summary>
    /// Duplicate all the edges of this Brep.
    /// </summary>
    /// <returns>An array of edge curves.</returns>
    public Curve[] DuplicateEdgeCurves()
    {
      return DuplicateEdgeCurves(false);
    }

    public Curve[] DuplicateEdgeCurves(bool nakedOnly)
    {
      IntPtr pConstPtr = ConstPointer();
      Runtime.InteropWrappers.SimpleArrayCurvePointer output = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();

      UnsafeNativeMethods.ON_Brep_DuplicateEdgeCurves(pConstPtr, outputPtr, nakedOnly);
      if (output == null)
        return null;
      return output.ToNonConstArray();
    }

    /// <summary>
    /// Create all the Wireframe curves for this Brep.
    /// </summary>
    /// <param name="density">Wireframe density. Valid values range between -1 and 99.</param>
    /// <returns>An array of Wireframe curves or null on failure.</returns>
    public Curve[] GetWireframe(int density)
    {
      IntPtr pConstPtr = ConstPointer();
      Runtime.InteropWrappers.SimpleArrayCurvePointer output = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr outputPtr = output.NonConstPointer();

      UnsafeNativeMethods.ON_Brep_GetWireframe(pConstPtr, density, outputPtr);
      if (output == null)
        return null;
      return output.ToNonConstArray();
    }

    /// <summary>
    /// Duplicate all the corner vertices of this Brep.
    /// </summary>
    /// <returns>An array or corner vertices.</returns>
    public Point3d[] DuplicateVertices()
    {
      IntPtr pConstPtr = ConstPointer();

      Runtime.InteropWrappers.SimpleArrayPoint3d outputPoints = new Runtime.InteropWrappers.SimpleArrayPoint3d();
      IntPtr outputPointsPtr = outputPoints.NonConstPointer();

      UnsafeNativeMethods.ON_Brep_DuplicateVertices(pConstPtr, outputPointsPtr);
      if (outputPoints.Count == 0) { return null; }

      return outputPoints.ToArray();
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method will disappear soon, use ClosestPoint() instead")]
    public Point3d GetClosestPoint(Point3d testPoint)
    {
      return ClosestPoint(testPoint);
    }
    /// <summary>
    /// Finds a point on the brep that is closest to testPoint.
    /// </summary>
    /// <param name="testPoint">Base point to project to brep.</param>
    /// <returns>The point on the Brep closest to testPoint or Point3d.Unset if the operation failed.</returns>
    public Point3d ClosestPoint(Point3d testPoint)
    {
      Point3d pt_cp;
      Vector3d vc_cp;
      ComponentIndex ci;
      double s, t;

      if (!ClosestPoint(testPoint, out pt_cp, out ci, out s, out t, double.MaxValue, out vc_cp))
      {
        return Point3d.Unset;
      }
      else
      {
        return pt_cp;
      }
    }

    /// <summary>
    /// This method is Obsolete, use ClosestPoint() instead.
    /// </summary>
    [Obsolete("This method is Obsolete, use ClosestPoint() instead")]
    public bool GetClosestPoint(Point3d testPoint,
      out Point3d closestPoint, out ComponentIndex ci,
      out double s, out double t, double maximumDistance, out Vector3d normal)
    {
      return ClosestPoint(testPoint, out closestPoint, out ci, out s, out t, maximumDistance, out normal);
    }
    /// <summary>
    /// Finds a point on a brep that is closest to testPoint.
    /// </summary>
    /// <param name="testPoint">base point to project to surface</param>
    /// <param name="closestPoint">location of the closest brep point</param>
    /// <param name="ci">Component index of the brep component that contains
    /// the closest point. Possible types are brep_face, brep_edge or brep_vertex</param>
    /// <param name="s">If the ci type is brep_edge, then s is the parameter
    /// of the closest edge point.</param>
    /// <param name="t">If the ci type is brep_face, then (s,t) is the parameter
    /// of the closest edge point.</param>
    /// <param name="maximumDistance">
    /// If maximumDistance &gt; 0, then only points whose distance
    /// is &lt;= maximumDistance will be returned. Using a positive
    /// value of maximumDistance can substantially speed up the search.</param>
    /// <param name="normal">The normal to the face if ci is a brep_face
    /// and the tangent to the edge if ci is brep_edge.
    /// </param>
    /// <returns></returns>
    public bool ClosestPoint(Point3d testPoint,
      out Point3d closestPoint, out ComponentIndex ci,
      out double s, out double t, double maximumDistance, out Vector3d normal)
    {
      ci = Rhino.Geometry.ComponentIndex.Unset;
      s = RhinoMath.UnsetValue;
      t = RhinoMath.UnsetValue;
      normal = Vector3d.Unset;
      closestPoint = Point3d.Unset;

      IntPtr pConstPtr = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Brep_GetClosestPoint(pConstPtr, testPoint,
        ref closestPoint, ref ci, ref s, ref t, maximumDistance, ref normal);

      return rc;
    }

    /// <summary>
    /// Determine if a 3D point is inside of a brep. This
    /// function only makes sense for closed manifold Breps.
    /// </summary>
    /// <param name="point">3d point to test</param>
    /// <param name="tolerance">
    /// 3d distance tolerance used for intersection and determining strict inclusion.
    /// A good default is RhinoMath.SqrtEpsilon
    /// </param>
    /// <param name="strictlyIn">
    /// if true, point is in if inside brep by at least tolerance.
    /// if false, point is in if truly in or within tolerance of boundary.
    /// </param>
    /// <returns>
    /// true if point is in, false if not
    /// </returns>
    public bool IsPointInside(Point3d point, double tolerance, bool strictlyIn)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoIsPointInBrep(ptr, point, tolerance, strictlyIn);
    }

    /// <summary>
    /// Reverses entire brep orientation of all faces.
    /// </summary>
    public void Flip()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Brep_Flip(pThis);
    }

    /// <summary>
    /// Return a new Brep that is equivalent to this Brep with all planar holes capped.
    /// </summary>
    /// <param name="tolerance">Tolerance to use for capping.</param>
    /// <returns>New brep on success. null on error.</returns>
    public Brep CapPlanarHoles(double tolerance)
    {
      IntPtr pThis = ConstPointer();
      IntPtr pCapped = UnsafeNativeMethods.RHC_CapPlanarHoles(pThis, tolerance);
      if (IntPtr.Zero == pCapped)
        return null;

      return new Brep(pCapped, null, null);
    }

    /// <summary>
    /// If any edges of this brep overlap edges of otherBrep, merge a copy of otherBrep into this
    /// brep joining all edges that overlap within tolerance
    /// </summary>
    /// <param name="otherBrep">Brep to be added to this brep</param>
    /// <param name="tolerance">3d distance tolerance for detecting overlapping edges</param>
    /// <param name="compact">if true, set brep flags and tolerances, remove unused faces and edges</param>
    /// <returns>true if any edges were joined</returns>
    /// <remarks>
    /// if no edges overlap, this brep is unchanged.
    /// otherBrep is copied if it is merged with this, and otherBrep is always unchanged
    /// Use this to join a list of breps in a series.
    /// When joining multiple breps in series, compact should be set to false.
    /// Call compact on the last Join
    /// </remarks>
    public bool Join(Brep otherBrep, double tolerance, bool compact)
    {
      IntPtr pThisBrep = this.NonConstPointer();
      IntPtr pOther = otherBrep.ConstPointer();
      return UnsafeNativeMethods.RHC_RhinoJoinBreps2(pThisBrep, pOther, tolerance, compact);
    }

    // making static because there will be IEnumerable<Brep> versions of this function
    public static Curve[] CreateContourCurves(Brep brepToContour, Point3d contourStart, Point3d contourEnd, double interval)
    {
      IntPtr pConstBrep = brepToContour.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArrayCurvePointer outputcurves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        IntPtr pCurves = outputcurves.NonConstPointer();
        int count = UnsafeNativeMethods.RHC_MakeRhinoContours2(pConstBrep, contourStart, contourEnd, interval, pCurves);
        if (0 == count)
          return null;
        return outputcurves.ToNonConstArray();
      }
    }
    public static Curve[] CreateContourCurves(Brep brepToContour, Plane sectionPlane)
    {
      IntPtr pConstBrep = brepToContour.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArrayCurvePointer outputcurves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        IntPtr pCurves = outputcurves.NonConstPointer();
        int count = UnsafeNativeMethods.RHC_MakeRhinoContours3(pConstBrep, ref sectionPlane, pCurves);
        if (0 == count)
          return null;
        return outputcurves.ToNonConstArray();
      }
    }
    #endregion

    #region internal helpers

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Brep(IntPtr.Zero, null, null);
    }

    #endregion
  }

  /// <summary>
  /// Enumerates the possible point/BrepFace spatial relationships.
  /// </summary>
  public enum PointFaceRelation : int
  {
    /// <summary>
    /// Point is on the exterior (the trimmed part) of the face.
    /// </summary>
    Exterior = 0,

    /// <summary>
    /// Point is on the interior (the existing part) of the face.
    /// </summary>
    Interior = 1,

    /// <summary>
    /// Point is in limbo.
    /// </summary>
    Boundary = 2
  }

  /// <summary>
  /// Enumerates all possible Solid Orientations for a Brep.
  /// </summary>
  public enum BrepSolidOrientation : int
  {
    /// <summary>
    /// Brep is not a Solid.
    /// </summary>
    None = 0,

    /// <summary>
    /// Brep is a Solid with inward facing normals.
    /// </summary>
    Inward = -1,

    /// <summary>
    /// Brep is a Solid with outward facing normals.
    /// </summary>
    Outward = 1,

    /// <summary>
    /// Breps is a Solid but no orientation could be computed.
    /// </summary>
    Unknown = 2
  }

  /// <summary>
  /// Enumerates all possible Topological Edge adjacency types.
  /// </summary>
  public enum EdgeAdjacency : int
  {
    /// <summary>
    /// Edge is not used by any faces and is therefore superfluous.
    /// </summary>
    None = 0,

    /// <summary>
    /// Edge is used by a single face.
    /// </summary>
    Naked = 1,

    /// <summary>
    /// Edge is used by two adjacent faces.
    /// </summary>
    Interior = 2,

    /// <summary>
    /// Edge is used by three or more adjacent faces.
    /// </summary>
    NonManifold = 3
  }

  /// <summary>
  /// Represents a single edge curve in a Brep object.
  /// </summary>
  public class BrepEdge : CurveProxy
  {
    #region fields
    internal int m_index;
    internal Brep m_brep;
    internal BrepEdge(int index, Brep owner)
    {
      m_index = index;
      m_brep = owner;
    }
    #endregion

    #region properties
    /// <summary>
    /// Number of trim-curves that use this edge.
    /// </summary>
    public int TrimCount
    {
      get
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_EdgeTrimCount(pConstBrep, m_index);
      }
    }

    /// <summary>
    /// Gets the topological valency of this edge. The topological valency 
    /// is defined by how many adjacent faces share this edge.
    /// </summary>
    public EdgeAdjacency Valence
    {
      get
      {
        switch (TrimCount)
        {
          case 0:
            return EdgeAdjacency.None;

          case 1:
            return EdgeAdjacency.Naked;

          case 2:
            return EdgeAdjacency.Interior;

          default:
            return EdgeAdjacency.NonManifold;
        }
      }
    }

    /// <summary>
    /// Gets the indices of all the BrepFaces that use this edge.
    /// </summary>
    public int[] AdjacentFaces()
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      Runtime.INTERNAL_IntArray fi = new Rhino.Runtime.INTERNAL_IntArray();

      int rc = UnsafeNativeMethods.ON_Brep_EdgeFaceIndices(pConstBrep, m_index, fi.m_ptr);
      if (rc == 0) { return null; }

      return fi.ToArray();
    }
    #endregion

    #region methods
    internal override IntPtr _InternalGetConstPointer()
    {
      if (null != m_brep)
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_BrepEdgePointer(pConstBrep, m_index);
      }
      return IntPtr.Zero;
    }
    #endregion
  }

  /// <summary>
  /// A Brep face is composed of one surface and trimming curves
  /// </summary>
  public class BrepFace : SurfaceProxy
  {
    #region fields
    internal int m_index;
    internal Brep m_brep;
    #endregion

    #region constructors
    internal BrepFace(int index, Brep owner)
    {
      m_index = index;
      m_brep = owner;
    }
    #endregion

    #region properties
    /// <summary>
    /// True if face orientation is opposite of natural surface orientation.
    /// </summary>
    public bool OrientationIsReversed
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_BrepFace_IsReversed(pConstThis);
      }
    }

    /// <summary>
    /// Gets a value indicating whether the face is synonymous with the underlying surface. 
    /// If a Face has no trimming curves then it is considered a Surface.
    /// </summary>
    public bool IsSurface
    {
      get
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_FaceIsSurface(pConstBrep, m_index);
      }
    }
    #endregion

    #region methods
    internal override IntPtr _InternalGetConstPointer()
    {
      if (null != m_brep)
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_BrepFacePointer(pConstBrep, m_index);
      }
      return IntPtr.Zero;
    }

#if USING_V5_SDK // only available in V5
    /// <summary>
    /// Pulls one or more points to a brep face
    /// </summary>
    /// <param name="points"></param>
    /// <param name="tolerance"></param>
    /// <returns></returns>
    public Point3d[] PullPointsToFace(IEnumerable<Point3d> points, double tolerance)
    {
      int count;
      Point3d[] inpoints = Rhino.Collections.Point3dList.GetConstPointArray(points, out count);
      if (inpoints == null || inpoints.Length < 1)
        return null;
      IntPtr pBrep = m_brep.NonConstPointer();
      using (Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d outpoints = new Rhino.Runtime.InteropWrappers.SimpleArrayPoint3d())
      {
        IntPtr pOutPoints = outpoints.NonConstPointer();
        int points_pulled = UnsafeNativeMethods.RHC_RhinoPullPointsToFace(pBrep, m_index, count, inpoints, pOutPoints, tolerance);
        if (points_pulled < 1)
          return null;
        return outpoints.ToArray();
      }
    }
#endif

    /// <summary>
    /// Set the surface domain of this Face.
    /// </summary>
    /// <param name="direction">Direction of face to set (0 = U, 1 = V).</param>
    /// <param name="domain">Domain to apply.</param>
    /// <returns>True on success, false on failure.</returns>
    public override bool SetDomain(int direction, Interval domain)
    {
      bool rc = false;
      IntPtr pBrep = m_brep.NonConstPointer();
      IntPtr pFace = UnsafeNativeMethods.ON_Brep_BrepFacePointer(pBrep, m_index);
      if (IntPtr.Zero != pFace)
      {
        rc = UnsafeNativeMethods.ON_Surface_SetDomain(pFace, direction, domain);
      }
      return rc;
    }

    /// <summary>
    /// Duplicate a face from the brep to create new single face brep.
    /// </summary>
    /// <param name="duplicateMeshes">If true, shading meshes will be copied as well.</param>
    /// <returns>A new single-face brep synonymous with the current Face.</returns>
    public Brep DuplicateFace(bool duplicateMeshes)
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      IntPtr pNewBrep = UnsafeNativeMethods.ON_Brep_DuplicateFace(pConstBrep, m_index, duplicateMeshes);
      if (IntPtr.Zero == pNewBrep)
        return null;
      return new Brep(pNewBrep, null, null);
    }
    /// <summary>
    /// Get a copy to the untrimmed surface that this face is based on.
    /// </summary>
    /// <returns>A copy of this face's underlying surface.</returns>
    public Surface DuplicateSurface()
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      IntPtr pSurface = UnsafeNativeMethods.ON_Brep_DuplicateFaceSurface(pConstBrep, m_index);
      return GeometryBase.CreateGeometryHelper(pSurface, null) as Surface;
    }

    /// <summary>
    /// Split this face using 3D trimming curves.
    /// </summary>
    /// <param name="curves">Curves to split with.</param>
    /// <param name="tolerance">Tolerance for splitting, when in doubt use the Document Absolute Tolerance.</param>
    /// <returns>A Brep consisting of all the split fragments, or null on failure.</returns>
    public Brep Split(IEnumerable<Curve> curves, double tolerance)
    {
      if (null == curves) { return DuplicateFace(false); }

      Rhino.Collections.CurveList crv_list = new Rhino.Collections.CurveList(curves);
      if (crv_list.Count == 0) { return DuplicateFace(false); }

      IntPtr p_curves = new Runtime.InteropWrappers.SimpleArrayCurvePointer(crv_list.m_items).ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Brep_SplitFace(m_brep.ConstPointer(), m_index, p_curves, tolerance);

      if (IntPtr.Zero == rc) { return null; }
      return new Brep(rc, null, null);
    }

    /// <summary>
    /// Test if a parameter space point is on the interior of a trimmed face.
    /// </summary>
    /// <param name="u">Parameter space point u value.</param>
    /// <param name="v">Parameter space point v value.</param>
    /// <returns>A value describing the spatial relationship between the point and the face.</returns>
    public PointFaceRelation IsPointOnFace(double u, double v)
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      int rc = UnsafeNativeMethods.ON_Brep_PointIsOnFace(pConstBrep, m_index, u, v);
      if (1 == rc)
        return PointFaceRelation.Interior;
      if (2 == rc)
        return PointFaceRelation.Boundary;

      return PointFaceRelation.Exterior;
    }

    /// <summary>
    /// Similar to IsoCurve function, except this function pays attention to trims on faces 
    /// and may return multiple curves.
    /// </summary>
    /// <param name="direction">Direction of isocurve.
    /// <para>0 = Isocurve connects all points with a constant U value.</para>
    /// <para>1 = Isocurve connects all points with a constant V value.</para>
    /// </param>
    /// <param name="constantParameter">Surface parameter that remains identical along the isocurves.</param>
    /// <returns>Isoparametric curves connecting all points with the constantParameter value.</returns>
    /// <remarks>
    /// In this function "direction" indicates which direction the resulting curve runs.
    /// 0: horizontal, 1: vertical
    /// In the other Surface functions that take a "direction" argument,
    /// "direction" indicates if "constantParameter" is a "u" or "v" parameter.
    /// </remarks>
    public Curve[] TrimAwareIsoCurve(int direction, double constantParameter)
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer curves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr pCurves = curves.NonConstPointer();
      int count = UnsafeNativeMethods.RHC_RhinoGetBrepFaceIsoCurves(pConstBrep, m_index, direction, constantParameter, pCurves);
      Curve[] rc = null;
      if (count > 0)
        rc = curves.ToNonConstArray();
      curves.Dispose();
      return rc;
    }

    /// <summary>
    /// Get the shading mesh that is associated with this Face.
    /// </summary>
    public Mesh GetMesh()
    {
      if (null != m_render_mesh && m_render_mesh._GetConstObjectParent() == this)
        return m_render_mesh;

      m_render_mesh = null;
      IntPtr pMesh = _GetMeshPointer();
      if (IntPtr.Zero == pMesh)
        return null;

      m_render_mesh = new Mesh(this);
      return m_render_mesh;
    }
    Mesh m_render_mesh;
    internal IntPtr _GetMeshPointer()
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      IntPtr pConstMesh = UnsafeNativeMethods.ON_BrepFace_Mesh(pConstBrep, m_index);
      return pConstMesh;
    }

    /// <summary>
    /// Gets the indices of all the BrepEdges that delineate this Face.
    /// </summary>
    public int[] AdjacentEdges()
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      Runtime.INTERNAL_IntArray ei = new Rhino.Runtime.INTERNAL_IntArray();

      int rc = UnsafeNativeMethods.ON_Brep_FaceEdgeIndices(pConstBrep, m_index, ei.m_ptr);
      if (rc == 0) { return null; }

      return ei.ToArray();
    }
    /// <summary>
    /// Gets the indices of all the BrepFaces that surround (are adjacent to) this face.
    /// </summary>
    public int[] AdjacentFaces()
    {
      IntPtr pConstBrep = m_brep.ConstPointer();
      Runtime.INTERNAL_IntArray fi = new Rhino.Runtime.INTERNAL_IntArray();

      int rc = UnsafeNativeMethods.ON_Brep_FaceFaceIndices(pConstBrep, m_index, fi.m_ptr);
      if (rc == 0) { return null; }

      return fi.ToArray();
    }
    #endregion
  }
}

namespace Rhino.Geometry.Collections
{
  /// <summary>
  /// Provides access to all the Faces in a Brep object.
  /// </summary>
  public class BrepFaceList : IEnumerable<BrepFace>
  {
    internal Brep m_brep;

    #region constructors
    internal BrepFaceList(Brep ownerBrep)
    {
      m_brep = ownerBrep;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the number of brep faces.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_GetInt(pConstBrep, Brep.idxFaceCount);
      }
    }

    /// <summary>
    /// Gets the BrepFace at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of BrepFace to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The BrepFace at [index].</returns>
    public Rhino.Geometry.BrepFace this[int index]
    {
      get
      {
        int count = Count;
        if (index < 0 || index >= count)
        {
          throw new IndexOutOfRangeException();
        }
        if (m_faces == null)
          m_faces = new System.Collections.Generic.List<BrepFace>(count);
        int existing_list_count = m_faces.Count;
        for (int i = existing_list_count; i < count; i++)
        {
          m_faces.Add(new BrepFace(i, m_brep));
        }

        return m_faces[index];
      }
    }
    System.Collections.Generic.List<BrepFace> m_faces; // = null; initialized to null by runtime
    #endregion

    #region methods
    /// <summary>
    /// Shrink all the faces in this Brep. Sometimes the surfaces extend far beyond the trimming 
    /// boundaries of the Brep Face. This function will remove those portions of the surfaces 
    /// that are not used.
    /// </summary>
    /// <returns>True on success, false on failure.</returns>
    public bool ShrinkFaces()
    {
      IntPtr pBrep = m_brep.NonConstPointer();
      return UnsafeNativeMethods.ON_Brep_ShrinkFaces(pBrep);
    }

    /// <summary>
    /// Split any faces with creases into G1 pieces.
    /// </summary>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>If you need to detect whether splitting occured, 
    /// compare the before and after values of Faces.Count </remarks>
    public bool SplitKinkyFaces()
    {
      return SplitKinkyFaces(1e-2, false);
    }
    /// <summary>
    /// Split any faces with creases into G1 pieces.
    /// </summary>
    /// <param name="kinkTolerance">Tolerance (in radians) to use for crease detection.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>If you need to detect whether splitting occured, 
    /// compare the before and after values of Faces.Count </remarks>
    public bool SplitKinkyFaces(double kinkTolerance)
    {
      return SplitKinkyFaces(kinkTolerance, false);
    }
    /// <summary>
    /// Split any faces with creases into G1 pieces.
    /// </summary>
    /// <param name="kinkTolerance">Tolerance (in radians) to use for crease detection.</param>
    /// <param name="compact">If true, the Brep will be compacted if possible.</param>
    /// <returns>True on success, false on failure.</returns>
    /// <remarks>If you need to detect whether splitting occured, 
    /// compare the before and after values of Faces.Count </remarks>
    public bool SplitKinkyFaces(double kinkTolerance, bool compact)
    {
      IntPtr pBrep = m_brep.NonConstPointer();
      return UnsafeNativeMethods.ON_Brep_SplitKinkyFaces(pBrep, kinkTolerance, compact);
    }
    #endregion

    #region IEnumerable Implementation
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
    public IEnumerator<BrepFace> GetEnumerator()
    {
      return new BrepFaceEnumerator(this);
    }
    #endregion
  }

  internal class BrepFaceEnumerator : IEnumerator<BrepFace>
  {
    #region members
    BrepFaceList m_list;
    int position = -1;
    #endregion

    #region constructor
    public BrepFaceEnumerator(BrepFaceList faceList)
    {
      m_list = faceList;
    }
    #endregion

    #region enumeration logic
    public bool MoveNext()
    {
      position++;
      return (position < m_list.Count);
    }
    public void Reset()
    {
      position = -1;
    }

    public BrepFace Current
    {
      get
      {
        try
        {
          return m_list[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
    object System.Collections.IEnumerator.Current
    {
      get
      {
        try
        {
          return m_list[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
    #endregion

    #region IDisposable logic
    private bool m_disposed; // = false; <- set by framework
    public void Dispose()
    {
      if (m_disposed) { return; }
      m_disposed = true;

      GC.SuppressFinalize(this);
    }
    #endregion
  }

  /// <summary>
  /// Provides access to all the Edges in a Brep object.
  /// </summary>
  public class BrepEdgeList : IEnumerable<BrepEdge>
  {
    private Brep m_brep;

    #region constructors
    internal BrepEdgeList(Brep ownerBrep)
    {
      m_brep = ownerBrep;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the number of brep edges.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr pConstBrep = m_brep.ConstPointer();
        return UnsafeNativeMethods.ON_Brep_GetInt(pConstBrep, Brep.idxEdgeCount);
      }
    }

    /// <summary>
    /// Gets the BrepEdge at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of BrepEdge to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The BrepEdge at [index].</returns>
    public Rhino.Geometry.BrepEdge this[int index]
    {
      get
      {
        int count = Count;
        if (index < 0 || index >= count)
        {
          throw new IndexOutOfRangeException();
        }
        if (m_edges == null)
          m_edges = new System.Collections.Generic.List<BrepEdge>(count);
        int existing_list_count = m_edges.Count;
        for (int i = existing_list_count; i < count; i++)
        {
          m_edges.Add(new BrepEdge(i, m_brep));
        }

        return m_edges[index];
      }
    }
    System.Collections.Generic.List<BrepEdge> m_edges; // = null; initialized to null by runtime
    #endregion

    #region methods
    #endregion

    #region IEnumerable Implementation
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return this.GetEnumerator();
    }
    public IEnumerator<BrepEdge> GetEnumerator()
    {
      return new BrepEdgeEnumerator(this);
    }
    #endregion
  }

  internal class BrepEdgeEnumerator : IEnumerator<BrepEdge>
  {
    #region members
    BrepEdgeList m_list;
    int position = -1;
    #endregion

    #region constructor
    public BrepEdgeEnumerator(BrepEdgeList edgeList)
    {
      m_list = edgeList;
    }
    #endregion

    #region enumeration logic
    public bool MoveNext()
    {
      position++;
      return (position < m_list.Count);
    }
    public void Reset()
    {
      position = -1;
    }

    public BrepEdge Current
    {
      get
      {
        try
        {
          return m_list[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
    object System.Collections.IEnumerator.Current
    {
      get
      {
        try
        {
          return m_list[position];
        }
        catch (IndexOutOfRangeException)
        {
          throw new InvalidOperationException();
        }
      }
    }
    #endregion

    #region IDisposable logic
    private bool m_disposed; // = false; <- set by framework
    public void Dispose()
    {
      if (m_disposed) { return; }
      m_disposed = true;

      GC.SuppressFinalize(this);
    }
    #endregion
  }
}