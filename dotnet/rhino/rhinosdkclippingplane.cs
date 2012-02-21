#if RHINO_SDK
namespace Rhino.DocObjects
{
  /// <summary>
  /// Represents the object of a <see cref="Rhino.Geometry.ClippingPlaneSurface">clipping plane</see>,
  /// stored in the Rhino document and with attributes.
  /// </summary>
  public class ClippingPlaneObject : RhinoObject
  {
    internal ClippingPlaneObject(uint serialNumber)
      : base(serialNumber)
    { }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoClippingPlaneObject_InternalCommitChanges;
    }

    /// <summary>
    /// Gets the clipping plane surface.
    /// </summary>
    public Rhino.Geometry.ClippingPlaneSurface ClippingPlaneGeometry
    {
      get
      {
        Rhino.Geometry.ClippingPlaneSurface rc = Geometry as Rhino.Geometry.ClippingPlaneSurface;
        return rc;
      }
    }

  }
}
#endif