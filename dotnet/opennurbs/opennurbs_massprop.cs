using System;
using System.Collections.Generic;

#if RHINO_SDK

namespace Rhino.Geometry
{
  /// <summary>
  /// Contains static initialization methods and allows access to the computed
  /// metrics of area, area centroid and area moments in closed
  /// planar curves, in meshes, in surfaces, in hatches and in boundary representations.
  /// </summary>
  public class AreaMassProperties : IDisposable
  {
    #region members
    private IntPtr m_ptr; // ON_MassProperties*
    private readonly bool m_bIsConst;
    #endregion

    #region constructors
    private AreaMassProperties(IntPtr ptr, bool isConst)
    {
      m_ptr = ptr;
      m_bIsConst = isConst;
    }

    //public AreaMassProperties()
    //{
    //  m_ptr = UnsafeNativeMethods.ON_MassProperties_New();
    //  m_bIsConst = false;
    //}

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~AreaMassProperties()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!m_bIsConst && IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_MassProperties_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
    #endregion

    /// <summary>
    /// Computes an AreaMassProperties for a closed planar curve.
    /// </summary>
    /// <param name="closedPlanarCurve">Curve to measure.</param>
    /// <returns>The AreaMassProperties for the given curve or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When closedPlanarCurve is null.</exception>
    public static AreaMassProperties Compute(Curve closedPlanarCurve)
    {
      const double absoluteTolerance = 1.0e-6;
      return Compute(closedPlanarCurve, absoluteTolerance);
    }

    /// <summary>
    /// Computes an AreaMassProperties for a closed planar curve.
    /// </summary>
    /// <param name="closedPlanarCurve">Curve to measure.</param>
    /// <param name="planarTolerance">absolute tolerance used to insure the closed curve is planar</param>
    /// <returns>The AreaMassProperties for the given curve or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When closedPlanarCurve is null.</exception>
    public static AreaMassProperties Compute(Curve closedPlanarCurve, double planarTolerance)
    {
      if (closedPlanarCurve == null)
        throw new ArgumentNullException("closedPlanarCurve");

      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr ptr = closedPlanarCurve.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Curve_AreaMassProperties(ptr, relativeTolerance, absoluteTolerance, planarTolerance);
      return rc == IntPtr.Zero ? null : new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Computes an AreaMassProperties for a hatch.
    /// </summary>
    /// <param name="hatch">Hatch to measure.</param>
    /// <returns>The AreaMassProperties for the given hatch or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When hatch is null.</exception>
    public static AreaMassProperties Compute(Hatch hatch)
    {
      if (hatch == null)
        throw new ArgumentNullException("hatch");

      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr ptr = hatch.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Hatch_AreaMassProperties(ptr, relativeTolerance, absoluteTolerance);
      return rc == IntPtr.Zero ? null : new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Computes an AreaMassProperties for a mesh.
    /// </summary>
    /// <param name="mesh">Mesh to measure.</param>
    /// <returns>The AreaMassProperties for the given Mesh or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When mesh is null.</exception>
    public static AreaMassProperties Compute(Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      IntPtr pMesh = mesh.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Mesh_MassProperties(true, pMesh);
      if (IntPtr.Zero == rc)
        return null;
      return new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Computes an AreaMassProperties for a brep.
    /// </summary>
    /// <param name="brep">Brep to measure.</param>
    /// <returns>The AreaMassProperties for the given Brep or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When brep is null.</exception>
    public static AreaMassProperties Compute(Brep brep)
    {
      if (brep == null)
        throw new ArgumentNullException("brep");

      IntPtr pBrep = brep.ConstPointer();
      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Brep_MassProperties(true, pBrep, relativeTolerance, absoluteTolerance);
      return IntPtr.Zero == rc ? null : new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Computes an AreaMassProperties for a surface.
    /// </summary>
    /// <param name="surface">Surface to measure.</param>
    /// <returns>The AreaMassProperties for the given Surface or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When surface is null.</exception>
    public static AreaMassProperties Compute(Surface surface)
    {
      if (surface == null)
        throw new ArgumentNullException("surface");

      IntPtr pSurface = surface.ConstPointer();
      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Surface_MassProperties(true, pSurface, relativeTolerance, absoluteTolerance);
      return IntPtr.Zero == rc ? null : new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Computes the Area properties for a collection of geometric objects. 
    /// At present only Breps, Surfaces, Meshes and Planar Closed Curves are supported.
    /// </summary>
    /// <param name="geometry">Objects to include in the area computation.</param>
    /// <returns>The Area properties for the entire collection or null on failure.</returns>
    public static AreaMassProperties Compute(IEnumerable<GeometryBase> geometry)
    {
      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;

      Rhino.Runtime.InteropWrappers.SimpleArrayGeometryPointer array = new Runtime.InteropWrappers.SimpleArrayGeometryPointer(geometry);
      IntPtr pConstGeometryArray = array.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Geometry_AreaMassProperties(pConstGeometryArray, relativeTolerance, absoluteTolerance);
      return IntPtr.Zero == rc ? null : new AreaMassProperties(rc, false);
    }

    #region properties
    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Gets the area solution.
    /// </summary>
    public double Area
    {
      get { return UnsafeNativeMethods.ON_MassProperties_Area(m_ptr); }
    }

    /// <summary>
    /// Gets the uncertainty in the area calculation.
    /// </summary>
    public double AreaError
    {
      get { return UnsafeNativeMethods.ON_MassProperties_MassError(m_ptr); }
    }

    /// <summary>
    /// Gets the area centroid in the world coordinate system.
    /// </summary>
    public Point3d Centroid
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_MassProperties_Centroid(ptr, ref rc);
        return rc;
      }
    }

    /// <summary>
    /// Gets the uncertainty in the centroid calculation.
    /// </summary>
    public Vector3d CentroidError
    {
      get
      {
        Vector3d rc = new Vector3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_MassProperties_CentroidError(ptr, ref rc);
        return rc;
      }
    }
    #endregion

    #region moments
    const int idx_wc_firstmoments = 0;
    const int idx_wc_secondmoments = 1;
    const int idx_wc_productmoments = 2;
    const int idx_wc_momentsofinertia = 3;
    const int idx_wc_radiiofgyration = 4;
    const int idx_cc_secondmoments = 5;
    const int idx_cc_momentsofinertia = 6;
    const int idx_cc_radiiofgyration = 7;

    bool GetMoments(int which, out Vector3d moment, out Vector3d error)
    {
      moment = new Vector3d();
      error = new Vector3d();
      IntPtr pConstThis = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_MassProperties_GetMoments(pConstThis, which, ref moment, ref error);
      return rc;
    }

    /// <summary>
    /// Returns the world coordinate first moments if they were able to be calculated.
    /// X is integral of "x dm" over the area
    /// Y is integral of "y dm" over the area
    /// Z is integral of "z dm" over the area.
    /// </summary>
    public Vector3d WorldCoordinatesFirstMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_firstmoments, out moment, out error);
        return moment;
      }
    }

    /// <summary>
    /// Uncertainty in world coordinates first moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesFirstMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_firstmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Returns the world coordinate first moments if they were able to be calculated.
    /// X is integral of "xx dm" over the area
    /// Y is integral of "yy dm" over the area
    /// Z is integral of "zz dm" over the area.
    /// </summary>
    public Vector3d WorldCoordinatesSecondMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_secondmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates second moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesSecondMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_secondmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Returns the world coordinate product moments if they were able to be calculated.
    /// X is integral of "xy dm" over the area
    /// Y is integral of "yz dm" over the area
    /// Z is integral of "zx dm" over the area.
    /// </summary>
    public Vector3d WorldCoordinatesProductMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_productmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates second moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesProductMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_productmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// The moments of inertia about the world coordinate axes.
    /// X = integral of (y^2 + z^2) dm
    /// Y = integral of (z^2 + x^2) dm
    /// Z = integral of (z^2 + y^2) dm.
    /// </summary>
    /// <remarks>
    /// What is meant by "moments of intertia" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Summary section.
    /// Some applications may want the values from WorldCoordinatesSecondMoments
    /// instead of the values returned here.
    /// </remarks>
    public Vector3d WorldCoordinatesMomentsOfInertia
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_momentsofinertia, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates moments of inertia calculation.
    /// </summary>
    public Vector3d WorldCoordinatesMomentsOfInertiaError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_momentsofinertia, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Radii of gyration with respect to world coordinate system.
    /// X = sqrt(integral of (y^2 + z^2) dm/M)
    /// Y = sqrt(integral of (z^2 + x^2) dm/M)
    /// Z = sqrt(integral of (z^2 + y^2) dm/M)
    /// </summary>
    /// <remarks>
    /// What is meant by "radii of gyration" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Returns section.
    /// </remarks>
    public Vector3d WorldCoordinatesRadiiOfGyration
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_radiiofgyration, out moment, out error);
        return moment;
      }
    }

    /// <summary>
    /// Second moments with respect to centroid coordinate system.
    /// X = integral of (x-x0)^2 dm
    /// Y = integral of (y-y0)^2 dm
    /// Z = integral of (z-z0)^2 dm
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    public Vector3d CentroidCoordinatesSecondMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_secondmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in centroid coordinates second moments calculation.
    /// </summary>
    public Vector3d CentroidCoordinatesSecondMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_secondmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Moments of inertia with respect to centroid coordinate system.
    /// X = integral of ((y-y0)^2 + (z-z0)^2) dm
    /// Y = integral of ((z-z0)^2 + (x-x0)^2) dm
    /// Z = integral of ((z-z0)^2 + (y-y0)^2) dm
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    /// <remarks>
    /// What is meant by "moments of intertia" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Summary section.
    /// Some applications may want the values from WorldCoordinatesSecondMoments
    /// instead of the values returned here.
    /// </remarks>
    public Vector3d CentroidCoordinatesMomentsOfInertia
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_momentsofinertia, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in centroid coordinates moments of inertia calculation.
    /// </summary>
    public Vector3d CentroidCoordinatesMomentsOfInertiaError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_momentsofinertia, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Radii of gyration with respect to centroid coordinate system.
    /// X = sqrt(integral of ((y-y0)^2 + (z-z0)^2) dm/M)
    /// Y = sqrt(integral of ((z-z0)^2 + (x-x0)^2) dm/M)
    /// Z = sqrt(integral of ((z-z0)^2 + (y-y0)^2) dm/M)
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    /// <remarks>
    /// What is meant by "radii of gyration" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Returns section.
    /// </remarks>
    public Vector3d CentroidCoordinatesRadiiOfGyration
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_radiiofgyration, out moment, out error);
        return moment;
      }
    }
    #endregion

    #region methods
    ///// <summary>
    ///// Sum mass properties together to get an aggregate mass.
    ///// </summary>
    ///// <param name="summand">mass properties to add.</param>
    ///// <returns>true if successful.</returns>
    //public bool Sum(AreaMassProperties summand)
    //{
    //  IntPtr pSum = summand.ConstPointer();
    //  return UnsafeNativeMethods.ON_MassProperties_Sum(m_ptr, pSum);
    //}
    #endregion
  }

  /// <summary>
  /// Contains static initialization methods and allows access to the computed
  /// metrics of volume, volume centroid and volume moments in 
  /// in solid meshes, in solid surfaces and in solid (closed) boundary representations.
  /// </summary>
  public class VolumeMassProperties : IDisposable
  {
    #region members
    private IntPtr m_ptr; // ON_MassProperties*
    private readonly bool m_bIsConst;
    #endregion

    #region constructors
    private VolumeMassProperties(IntPtr ptr, bool isConst)
    {
      m_ptr = ptr;
      m_bIsConst = isConst;
    }

    //public VolumeMassProperties()
    //{
    //  m_ptr = UnsafeNativeMethods.ON_MassProperties_New();
    //  m_bIsConst = false;
    //}

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~VolumeMassProperties()
    {
      Dispose(false);
    }
    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (!m_bIsConst && IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_MassProperties_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }
    #endregion

    /// <summary>
    /// Compute the VolumeMassProperties for a single Mesh.
    /// </summary>
    /// <param name="mesh">Mesh to measure.</param>
    /// <returns>The VolumeMassProperties for the given Mesh or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When mesh is null.</exception>
    /// <example>
    /// <code source='examples\vbnet\ex_meshvolume.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_meshvolume.cs' lang='cs'/>
    /// <code source='examples\py\ex_meshvolume.py' lang='py'/>
    /// </example>
    public static VolumeMassProperties Compute(Mesh mesh)
    {
      if (mesh == null)
        throw new ArgumentNullException("mesh");

      IntPtr pMesh = mesh.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Mesh_MassProperties(false, pMesh);
      if (IntPtr.Zero == rc)
        return null;
      return new VolumeMassProperties(rc, false);
    }

    /// <summary>
    /// Compute the VolumeMassProperties for a single Brep.
    /// </summary>
    /// <param name="brep">Brep to measure.</param>
    /// <returns>The VolumeMassProperties for the given Brep or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When brep is null.</exception>
    public static VolumeMassProperties Compute(Brep brep)
    {
      if (brep == null)
        throw new ArgumentNullException("brep");

      IntPtr pBrep = brep.ConstPointer();
      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Brep_MassProperties(false, pBrep, relativeTolerance, absoluteTolerance);
      return IntPtr.Zero == rc ? null : new VolumeMassProperties(rc, false);
    }

    /// <summary>
    /// Compute the VolumeMassProperties for a single Surface.
    /// </summary>
    /// <param name="surface">Surface to measure.</param>
    /// <returns>The VolumeMassProperties for the given Surface or null on failure.</returns>
    /// <exception cref="System.ArgumentNullException">When surface is null.</exception>
    public static VolumeMassProperties Compute(Surface surface)
    {
      if (surface == null)
        throw new ArgumentNullException("surface");

      IntPtr pSurface = surface.ConstPointer();
      const double relativeTolerance = 1.0e-6;
      const double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Surface_MassProperties(false, pSurface, relativeTolerance, absoluteTolerance);
      return IntPtr.Zero == rc ? null : new VolumeMassProperties(rc, false);
    }
    #region properties
    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Gets the volume solution.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_meshvolume.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_meshvolume.cs' lang='cs'/>
    /// <code source='examples\py\ex_meshvolume.py' lang='py'/>
    /// </example>
    public double Volume
    {
      get { return UnsafeNativeMethods.ON_MassProperties_Mass(m_ptr); }
    }

    /// <summary>
    /// Gets the uncertainty in the volume calculation.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_meshvolume.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_meshvolume.cs' lang='cs'/>
    /// <code source='examples\py\ex_meshvolume.py' lang='py'/>
    /// </example>
    public double VolumeError
    {
      get { return UnsafeNativeMethods.ON_MassProperties_MassError(m_ptr); }
    }

    /// <summary>
    /// Gets the volume centroid in the world coordinate system.
    /// </summary>
    public Point3d Centroid
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_MassProperties_Centroid(ptr, ref rc);
        return rc;
      }
    }

    /// <summary>
    /// Gets the uncertainty in the Centroid calculation.
    /// </summary>
    public Vector3d CentroidError
    {
      get
      {
        Vector3d rc = new Vector3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_MassProperties_CentroidError(ptr, ref rc);
        return rc;
      }
    }
    #endregion

    #region moments
    const int idx_wc_firstmoments = 0;
    const int idx_wc_secondmoments = 1;
    const int idx_wc_productmoments = 2;
    const int idx_wc_momentsofinertia = 3;
    const int idx_wc_radiiofgyration = 4;
    const int idx_cc_secondmoments = 5;
    const int idx_cc_momentsofinertia = 6;
    const int idx_cc_radiiofgyration = 7;

    bool GetMoments(int which, out Vector3d moment, out Vector3d error)
    {
      moment = new Vector3d();
      error = new Vector3d();
      IntPtr pConstThis = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_MassProperties_GetMoments(pConstThis, which, ref moment, ref error);
      return rc;
    }

    /// <summary>
    /// Returns the world coordinate first moments if they were able to be calculated.
    /// X is integral of "x dm" over the volume
    /// Y is integral of "y dm" over the volume
    /// Z is integral of "z dm" over the volume.
    /// </summary>
    public Vector3d WorldCoordinatesFirstMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_firstmoments, out moment, out error);
        return moment;
      }
    }

    /// <summary>
    /// Uncertainty in world coordinates first moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesFirstMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_firstmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Returns the world coordinate first moments if they were able to be calculated.
    /// X is integral of "xx dm" over the area
    /// Y is integral of "yy dm" over the area
    /// Z is integral of "zz dm" over the area.
    /// </summary>
    public Vector3d WorldCoordinatesSecondMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_secondmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates second moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesSecondMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_secondmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Returns the world coordinate product moments if they were able to be calculated.
    /// X is integral of "xy dm" over the area
    /// Y is integral of "yz dm" over the area
    /// Z is integral of "zx dm" over the area.
    /// </summary>
    public Vector3d WorldCoordinatesProductMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_productmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates second moments calculation.
    /// </summary>
    public Vector3d WorldCoordinatesProductMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_productmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// The moments of inertia about the world coordinate axes.
    /// X = integral of (y^2 + z^2) dm
    /// Y = integral of (z^2 + x^2) dm
    /// Z = integral of (z^2 + y^2) dm.
    /// </summary>
    /// <remarks>
    /// What is meant by "moments of intertia" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Summary section.
    /// Some applications may want the values from WorldCoordinatesSecondMoments
    /// instead of the values returned here.
    /// </remarks>
    public Vector3d WorldCoordinatesMomentsOfInertia
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_momentsofinertia, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in world coordinates moments of inertia calculation.
    /// </summary>
    public Vector3d WorldCoordinatesMomentsOfInertiaError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_momentsofinertia, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Radii of gyration with respect to world coordinate system.
    /// X = sqrt(integral of (y^2 + z^2) dm/M)
    /// Y = sqrt(integral of (z^2 + x^2) dm/M)
    /// Z = sqrt(integral of (z^2 + y^2) dm/M)
    /// </summary>
    /// <remarks>
    /// What is meant by "radii of gyration" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Returns section.
    /// </remarks>
    public Vector3d WorldCoordinatesRadiiOfGyration
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_wc_radiiofgyration, out moment, out error);
        return moment;
      }
    }

    /// <summary>
    /// Second moments with respect to centroid coordinate system.
    /// X = integral of (x-x0)^2 dm
    /// Y = integral of (y-y0)^2 dm
    /// Z = integral of (z-z0)^2 dm
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    public Vector3d CentroidCoordinatesSecondMoments
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_secondmoments, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in centroid coordinates second moments calculation.
    /// </summary>
    public Vector3d CentroidCoordinatesSecondMomentsError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_secondmoments, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Moments of inertia with respect to centroid coordinate system.
    /// X = integral of ((y-y0)^2 + (z-z0)^2) dm
    /// Y = integral of ((z-z0)^2 + (x-x0)^2) dm
    /// Z = integral of ((z-z0)^2 + (y-y0)^2) dm
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    /// <remarks>
    /// What is meant by "moments of intertia" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Summary section.
    /// Some applications may want the values from WorldCoordinatesSecondMoments
    /// instead of the values returned here.
    /// </remarks>
    public Vector3d CentroidCoordinatesMomentsOfInertia
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_momentsofinertia, out moment, out error);
        return moment;
      }
    }
    /// <summary>
    /// Uncertainty in centroid coordinates moments of inertia calculation.
    /// </summary>
    public Vector3d CentroidCoordinatesMomentsOfInertiaError
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_momentsofinertia, out moment, out error);
        return error;
      }
    }

    /// <summary>
    /// Radii of gyration with respect to centroid coordinate system.
    /// X = sqrt(integral of ((y-y0)^2 + (z-z0)^2) dm/M)
    /// Y = sqrt(integral of ((z-z0)^2 + (x-x0)^2) dm/M)
    /// Z = sqrt(integral of ((z-z0)^2 + (y-y0)^2) dm/M)
    /// where (x0,y0,z0) = centroid.
    /// </summary>
    /// <remarks>
    /// What is meant by "radii of gyration" varies widely in textbooks and papers.
    /// The values returned here are the integrals listed in the Returns section.
    /// </remarks>
    public Vector3d CentroidCoordinatesRadiiOfGyration
    {
      get
      {
        Vector3d moment, error;
        GetMoments(idx_cc_radiiofgyration, out moment, out error);
        return moment;
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Sum mass properties together to get an aggregate mass.
    /// </summary>
    /// <param name="summand">mass properties to add.</param>
    /// <returns>true if successful.</returns>
    public bool Sum(VolumeMassProperties summand)
    {
      IntPtr pSum = summand.ConstPointer();
      return UnsafeNativeMethods.ON_MassProperties_Sum(m_ptr, pSum);
    }
    #endregion
  }
}

#endif