#pragma warning disable 1591
using System;
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  /// <summary>
  /// A Rhino Object that represents curve geometry and attributes
  /// </summary>
  public class CurveObject : RhinoObject
  {
    internal CurveObject(uint serialNumber)
      : base(serialNumber)
    { }

    protected CurveObject() { }

    public Curve CurveGeometry
    {
      get
      {
        Curve rc = Geometry as Curve;
        return rc;
      }
    }

    /// <summary>
    /// Only for developers who are defining custom subclasses of CurveObject.
    /// Directly sets the internal curve geometry for this object.  Note that
    /// this function does not work with Rhino's "undo".
    /// </summary>
    /// <param name="curve"></param>
    /// <returns>
    /// The old curve geometry that was set for this object
    /// </returns>
    /// <remarks>
    /// Note that this function does not work with Rhino's "undo".  The typical
    /// approach for adjusting the curve geometry is to modify the object that you
    /// get when you call the CurveGeometry property and then call CommitChanges.
    /// </remarks>
    protected Curve SetCurve(Curve curve)
    {
      var parent = curve.ParentRhinoObject();
      if (parent != null && parent.RuntimeSerialNumber == this.RuntimeSerialNumber)
        return curve;

      IntPtr pThis = this.NonConstPointer_I_KnowWhatImDoing();

      IntPtr pCurve = curve.NonConstPointer();
      IntPtr pOldCurve = UnsafeNativeMethods.CRhinoCurveObject_SetCurve(pThis, pCurve);
      curve.ChangeToConstObject(this);
      if (pOldCurve != pCurve && pOldCurve != IntPtr.Zero)
        return new Curve(pOldCurve, null);
      return curve;
    }

    public Curve DuplicateCurveGeometry()
    {
      Curve rc = DuplicateGeometry() as Curve;
      return rc;
    }

    internal override RhinoObject.CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoCurveObject_InternalCommitChanges;
    }
  }
}
#endif