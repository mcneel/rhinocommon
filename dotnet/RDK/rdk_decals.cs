#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public class Decal
  {
    readonly IntPtr m_pDecal = IntPtr.Zero;
    //Forces a reference to the decal iterator to stick around until this object is GCd.
    ObjectDecals m_decals;

    internal Decal(IntPtr pDecal, ObjectDecals decals)
    {
      m_pDecal = pDecal;
      m_decals = decals;
    }

    public enum MappingTypes : int
    {
      Planar = 0, // Planar mapping. Uses projection, origin, up and across vectors (not unitized).
      Cylindrical = 1, // Cylindrical mapping. Uses origin, up, across, height, radius, latitude start and stop.
      Spherical = 2, // Spherical mapping. Uses origin, up, across, radius, latitude/longitude start and stop.
      UV = 3, // UV mapping.
    }

    public enum ProjectionTypes : int
    {
      Forward = 0, // Project forward.
      Backward = 1, // Project backward.
      Both = 2, // Project forward and backward.
    }

    /// <summary>
    /// Gets the decal ID associated with this decal.
    /// </summary>
    public Int32 Id { get { return UnsafeNativeMethods.Rdk_Decal_Id(ConstPointer()); } }

    /// <summary>
    /// Gets the texture ID for this decal.
    /// </summary>
    public Guid TextureInstanceId { get { return UnsafeNativeMethods.Rdk_Decal_TextureInstanceId(ConstPointer()); } }

    /// <summary>
    /// Gets the mapping of the decal.
    /// </summary>
    public MappingTypes MappingType { get { return (MappingTypes)UnsafeNativeMethods.Rdk_Decal_Mapping(ConstPointer()); } }

    /// <summary>
    /// Gets the decal's projection. Used only when mapping is planar.
    /// </summary>
    public ProjectionTypes ProjectionType { get { return (ProjectionTypes)UnsafeNativeMethods.Rdk_Decal_Projection(ConstPointer()); } }

    /// <summary>
    /// Used only when mapping is cylindrical or spherical.
    /// </summary>
    /// <value>true if texture is mapped to inside of sphere or cylinder, else \e false.</value>
    public bool MapToInside { get { return 1 == UnsafeNativeMethods.Rdk_Decal_MapToInside(ConstPointer()); } }

    /// <summary>
    /// Gets the decal's transparency in the range 0 to 1.
    /// </summary>
    public double Transparency { get { return UnsafeNativeMethods.Rdk_Decal_Transparency(ConstPointer()); } }

    /// <summary>
    /// Gets the origin of the decal in world space.
    /// </summary>
    public Rhino.Geometry.Point3d Origin
    {
      get
      {
        Rhino.Geometry.Point3d v = new Rhino.Geometry.Point3d();
        UnsafeNativeMethods.Rdk_Decal_Origin(ConstPointer(), ref v);
        return v;
      }
    }

    /// <summary>
    /// For cylindrical and spherical mapping, the vector is unitized.
    /// </summary>
    /// <returns>The 'up' vector of the decal. For planar mapping the length of the vector is relevant.</returns>
    public Rhino.Geometry.Vector3d VectorUp
    {
      get
      {
        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.Rdk_Decal_VectorUp(ConstPointer(), ref v);
        return v;
      }
    }

    /// <summary>
    /// Gets the vector across. For cylindrical and spherical mapping, the vector is unitized.
    /// </summary>
    /// <value>The 'across' vector of the decal. For planar mapping the length of the vector is relevant.</value>
    public Rhino.Geometry.Vector3d VectorAcross
    {
      get
      {

        Rhino.Geometry.Vector3d v = new Rhino.Geometry.Vector3d();
        UnsafeNativeMethods.Rdk_Decal_VectorAcross(ConstPointer(), ref v);
        return v;
      }
    }

    /// <summary>
    /// Gets the height of the decal. Only used when mapping is cylindrical.
    /// </summary>
    public double Height { get { return UnsafeNativeMethods.Rdk_Decal_Height(ConstPointer()); } }

    /// <summary>
    /// Gets thhe radius of the decal. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double Radius { get { return UnsafeNativeMethods.Rdk_Decal_Radius(ConstPointer()); } }

    /// <summary>
    /// Gets the start latitude of the decal's sweep. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double LatStart { get { return UnsafeNativeMethods.Rdk_Decal_LatStart(ConstPointer()); } }

    /// <summary>
    /// Gets the stop latitude of the decal's sweep. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double LatEnd { get { return UnsafeNativeMethods.Rdk_Decal_LatEnd(ConstPointer()); } }

    /// <summary>
    /// Gets the start longitude of the decal's sweep. Only used when mapping is spherical.
    /// </summary>
    public double LonStart { get { return UnsafeNativeMethods.Rdk_Decal_LonStart(ConstPointer()); } }

    /// <summary>
    /// Gets the stop longitude of the decal's sweep. Only used when mapping is spherical.
    /// </summary>
    public double LonEnd { get { return UnsafeNativeMethods.Rdk_Decal_LonEnd(ConstPointer()); } }

    /// <summary>
    /// The UV bounds of the decal. Only used when mapping is UV.
    /// </summary>
    public void UVBounds(ref double minUOut, ref double minVOut, ref double maxUOut, ref double maxVOut)
    {
      UnsafeNativeMethods.Rdk_Decal_UVBounds(ConstPointer(), ref minUOut, ref minVOut, ref maxUOut, ref maxVOut);
    }

    /// <summary>
    /// Gets custom data associated with this decal - see Rhino.Plugins.RenderPlugIn.ShowDecalProperties.
    /// </summary>
    /// <returns>The return value can be null if there is no data associated with this decal.</returns>
    public List<Rhino.Render.NamedValue> CustomData()
    {
      IntPtr pXmlSection = UnsafeNativeMethods.Rdk_Decal_CustomData(ConstPointer());
      if (IntPtr.Zero == pXmlSection)
        return null;

      return Rhino.Render.XMLSectionUtilities.ConvertToNamedValueList(pXmlSection);
    }    

    /// <summary>
    /// Blend color with the decal color at a given point.
    /// </summary>
    /// <param name="point">The point in space or, if the decal is uv-mapped, the uv-coordinate of that point.</param>
    /// <param name="normal">The face normal of the given point.</param>
    /// <param name="colInOut">The color to blend the decal color to.</param>
    /// <param name="uvOut">the UV on the texture that the color point was read from.</param>
    /// <returns>true if the given point hits the decal, else \e false.</returns>
    public bool Color(Rhino.Geometry.Point3d point, Rhino.Geometry.Vector3d normal, ref Rhino.Display.Color4f colInOut, ref Rhino.Geometry.Point2d uvOut)
    {
      return 1 == UnsafeNativeMethods.Rdk_Decal_Color(ConstPointer(), point, normal, ref colInOut, ref uvOut);
    }

    #region internals
    IntPtr ConstPointer()
    {
      return m_pDecal;
    }
    #endregion
  }





  public class ObjectDecals : IEnumerator<Decal>, IDisposable
  {
    private readonly IntPtr m_pDecalIterator;
    internal ObjectDecals(Rhino.DocObjects.RhinoObject obj)
    {
      m_pDecalIterator = UnsafeNativeMethods.Rdk_Decals_NewDecalIterator(obj.Attributes.ObjectId);
    }

    ~ObjectDecals()
    {
      Dispose(false);
    }

    #region IEnumerator Members

    Decal m_current;
    public Decal Current { get { return m_current; } }

    object System.Collections.IEnumerator.Current { get { return Current; } }

    public bool MoveNext()
    {
      IntPtr pDecal = UnsafeNativeMethods.Rdk_Decals_Next(NonConstPointer());

      if (pDecal == IntPtr.Zero)
      {
        m_current = null;
        return false;
      }

      m_current = new Decal(pDecal, this);
      return true;
    }

    public void Reset()
    {
      UnsafeNativeMethods.Rdk_Decals_ResetIterator(NonConstPointer());
    }

    #endregion

    #region internals
    private IntPtr NonConstPointer()
    {
      return m_pDecalIterator;
    }
    #endregion

    #region IDisposable Members

    public void Dispose()
    {
      Dispose(true);
    }

    protected void Dispose(bool isDisposiing)
    {
      UnsafeNativeMethods.Rdk_Decals_DeleteDecalIterator(NonConstPointer());
    }

    #endregion
  }
}

#endif

