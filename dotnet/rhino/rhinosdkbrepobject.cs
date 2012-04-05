using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  /// <summary>
  /// Represents a <see cref="Rhino.Geometry.Brep">brep</see> in a document.
  /// </summary>
  public class BrepObject : RhinoObject
  {
    internal BrepObject(uint serialNumber)
      : base(serialNumber) { }

    /// <summary>
    /// Protected constructor for custom subclasses
    /// </summary>
    protected BrepObject() { }

    /// <summary>
    /// Gets the brep geometry linked with this object.
    /// </summary>
    public Brep BrepGeometry
    {
      get
      {
        Brep rc = Geometry as Brep;
        return rc;
      }
    }

    /// <summary>
    /// Constructs a new deep copy of the brep geometry.
    /// </summary>
    /// <returns>The copy of the geometry.</returns>
    public Brep DuplicateBrepGeometry()
    {
      Brep rc = DuplicateGeometry() as Brep;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoBrepObject_InternalCommitChanges;
    }
  }

  /// <summary>
  /// Represents a <see cref="Rhino.Geometry.Surface">surface</see> in a document.
  /// </summary>
  public class SurfaceObject : RhinoObject
  {
    internal SurfaceObject(uint serialNumber)
      : base(serialNumber) { }

    /// <summary>
    /// Gets the surface geometry linked with this object.
    /// </summary>
    public Surface SurfaceGeometry
    {
      get
      {
        Surface rc = Geometry as Surface;
        return rc;
      }
    }

    /// <summary>
    /// Constructs a new deep copy of the surface geometry.
    /// </summary>
    /// <returns>The copy of the geometry.</returns>
    public Surface DuplicateSurfaceGeometry()
    {
      Surface rc = DuplicateGeometry() as Surface;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoSurfaceObject_InternalCommitChanges;
    }
  }
}

#endif