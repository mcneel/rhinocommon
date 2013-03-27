#pragma warning disable 1591
using System;
using System.Collections.Generic;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  public enum DecalMapping : int
  {
    /// <summary>
    /// Planar mapping. Uses projection, origin, up and across vectors (not unitized).
    /// </summary>
    Planar = 0,
    /// <summary>
    /// Cylindrical mapping. Uses origin, up, across, height, radius, latitude start and stop.
    /// </summary>
    Cylindrical = 1,
    /// <summary>
    /// Spherical mapping. Uses origin, up, across, radius, latitude/longitude start and stop.
    /// </summary>
    Spherical = 2,
    /// <summary>
    /// UV mapping.
    /// </summary>
    UV = 3
  }

  public enum DecalProjection : int
  {
    /// <summary>Project forward</summary>
    Forward = 0,
    /// <summary>Project backward</summary>
    Backward = 1,
    /// <summary>Project forward and backward</summary>
    Both = 2
  }

  /// <summary>
  /// Represents a decal, or a picture that can be moved on an object.
  /// </summary>
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

    /// <summary>
    /// Gets the decal ID associated with this decal.
    /// </summary>
    public Int32 Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_Id(pConstThis);
      }
    }

    /// <summary>
    /// Gets the texture ID for this decal.
    /// </summary>
    public Guid TextureInstanceId
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_TextureInstanceId(pConstThis);
      }
    }

    /// <summary>
    /// Gets the mapping of the decal.
    /// </summary>
    public DecalMapping DecalMapping
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (DecalMapping)UnsafeNativeMethods.Rdk_Decal_Mapping(pConstThis);
      }
    }

    /// <summary>
    /// Gets the decal's projection. Used only when mapping is planar.
    /// </summary>
    public DecalProjection DecalProjection
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return (DecalProjection)UnsafeNativeMethods.Rdk_Decal_Projection(pConstThis);
      }
    }

    /// <summary>
    /// Used only when mapping is cylindrical or spherical.
    /// </summary>
    /// <value>true if texture is mapped to inside of sphere or cylinder, else \e false.</value>
    public bool MapToInside
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return 1 == UnsafeNativeMethods.Rdk_Decal_MapToInside(pConstThis);
      }
    }

    /// <summary>
    /// Gets the decal's transparency in the range 0 to 1.
    /// </summary>
    public double Transparency
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_Transparency(pConstThis);
      }
    }

    /// <summary>
    /// Gets the origin of the decal in world space.
    /// </summary>
    public Rhino.Geometry.Point3d Origin
    {
      get
      {
        Rhino.Geometry.Point3d v = new Rhino.Geometry.Point3d();
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_Decal_Origin(pConstThis, ref v);
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
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_Decal_VectorUp(pConstThis, ref v);
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
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.Rdk_Decal_VectorAcross(pConstThis, ref v);
        return v;
      }
    }

    /// <summary>
    /// Gets the height of the decal. Only used when mapping is cylindrical.
    /// </summary>
    public double Height
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_Height(pConstThis);
      }
    }

    /// <summary>
    /// Gets thhe radius of the decal. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double Radius
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_Radius(pConstThis);
      }
    }

    /// <summary>
    /// Gets the start latitude of the decal's sweep. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double StartLatitude
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_LatStart(pConstThis);
      }
    }

    /// <summary>
    /// Gets the stop latitude of the decal's sweep. Only used when mapping is cylindrical or spherical.
    /// </summary>
    public double EndLatitude
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_LatEnd(pConstThis);
      }
    }

    /// <summary>
    /// Gets the start longitude of the decal's sweep. Only used when mapping is spherical.
    /// </summary>
    public double StartLongitude
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_LonStart(pConstThis);
      }
    }

    /// <summary>
    /// Gets the stop longitude of the decal's sweep. Only used when mapping is spherical.
    /// </summary>
    public double EndLongitude
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.Rdk_Decal_LonEnd(pConstThis);
      }
    }

    /// <summary>
    /// The UV bounds of the decal. Only used when mapping is UV.
    /// </summary>
    public void UVBounds(ref double minUOut, ref double minVOut, ref double maxUOut, ref double maxVOut)
    {
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.Rdk_Decal_UVBounds(pConstThis, ref minUOut, ref minVOut, ref maxUOut, ref maxVOut);
    }

    /// <summary>
    /// Gets custom data associated with this decal - see Rhino.Plugins.RenderPlugIn.ShowDecalProperties.
    /// </summary>
    /// <returns>The return value can be null if there is no data associated with this decal.</returns>
    public List<Rhino.Render.NamedValue> CustomData()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pXmlSection = UnsafeNativeMethods.Rdk_Decal_CustomData(pConstThis);
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
    /// <returns>true if the given point hits the decal, else false.</returns>
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




  /// <summary>Represents all the decals of an object.</summary>
  public class ObjectDecals : IEnumerator<Decal>
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

