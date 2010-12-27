using System;
using System.Runtime.InteropServices;
using Rhino;
using Rhino.Geometry;

namespace Rhino.Geometry
{
  public class AreaMassProperties : IDisposable
  {
    #region members
    private IntPtr m_ptr; // ON_MassProperties*
    private bool m_bIsConst;
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

    ~AreaMassProperties()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }
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
    /// Compute an AreaMassProperties for a single closed planar curve.
    /// </summary>
    /// <param name="closedPlanarCurve">Curve to measure.</param>
    /// <returns>The AreaMassProperties for the given curve or null on failure.</returns>
    public static AreaMassProperties Compute(Curve closedPlanarCurve)
    {
      if (null == closedPlanarCurve)
        return null;

      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr ptr = closedPlanarCurve.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Curve_AreaMassProperties(ptr, relativeTolerance, absoluteTolerance);
      if (rc == IntPtr.Zero)
        return null;
      return new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Compute an AreaMassProperties for a hatch
    /// </summary>
    /// <param name="hatch">Hatch to measure</param>
    /// <returns>The AreaMassProperties for the given hatch or null on failure.</returns>
    public static AreaMassProperties Compute(Hatch hatch)
    {
      if (null == hatch)
        return null;
      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr ptr = hatch.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Hatch_AreaMassProperties(ptr, relativeTolerance, absoluteTolerance);
      if (rc == IntPtr.Zero)
        return null;
      return new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Compute an AreaMassProperties for a single mesh.
    /// </summary>
    /// <param name="mesh">Mesh to measure.</param>
    /// <returns>The AreaMassProperties for the given Mesh or null on failure.</returns>
    public static AreaMassProperties Compute(Mesh mesh)
    {
      if (null == mesh)
        return null;

      IntPtr pMesh = mesh.ConstPointer();
      IntPtr rc = UnsafeNativeMethods.ON_Mesh_MassProperties(true, pMesh);
      if (IntPtr.Zero == rc)
        return null;
      return new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Compute an AreaMassProperties for a single Brep.
    /// </summary>
    /// <param name="brep">Brep to measure.</param>
    /// <returns>The AreaMassProperties for the given Brep or null on failure.</returns>
    public static AreaMassProperties Compute(Brep brep)
    {
      if (null == brep)
        return null;

      IntPtr pBrep = brep.ConstPointer();
      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Brep_MassProperties(true, pBrep, relativeTolerance, absoluteTolerance);
      if (IntPtr.Zero == rc)
        return null;
      return new AreaMassProperties(rc, false);
    }

    /// <summary>
    /// Compute an AreaMassProperties for a single Surface.
    /// </summary>
    /// <param name="surface">Surface to measure.</param>
    /// <returns>The AreaMassProperties for the given Surface or null on failure.</returns>
    public static AreaMassProperties Compute(Surface surface)
    {
      if (null == surface)
        return null;

      IntPtr pSurface = surface.ConstPointer();
      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Surface_MassProperties(true, pSurface, relativeTolerance, absoluteTolerance);
      if (IntPtr.Zero == rc)
        return null;
      return new AreaMassProperties(rc, false);
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
    /// Z is integral of "z dm" over the area
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
    /// Uncertainty in world coordinates first moments calculation
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
    /// Z is integral of "zz dm" over the area
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
    /// Uncertainty in world coordinates second moments calculation
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
    /// Z is integral of "zx dm" over the area
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
    /// Uncertainty in world coordinates second moments calculation
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
    /// Z = integral of (z^2 + y^2) dm
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
    /// Uncertainty in world coordinates moments of inertia calculation
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
    /// Uncertainty in centroid coordinates second moments calculation
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
    /// Uncertainty in centroid coordinates moments of inertia calculation
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
    ///// <param name="summand">mass properties to add</param>
    ///// <returns>True if successful.</returns>
    //public bool Sum(AreaMassProperties summand)
    //{
    //  IntPtr pSum = summand.ConstPointer();
    //  return UnsafeNativeMethods.ON_MassProperties_Sum(m_ptr, pSum);
    //}
    #endregion
  }

  public class VolumeMassProperties : IDisposable
  {
    #region members
    private IntPtr m_ptr; // ON_MassProperties*
    private bool m_bIsConst;
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

    ~VolumeMassProperties()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

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
    public static VolumeMassProperties Compute(Mesh mesh)
    {
      if (null == mesh)
        return null;

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
    public static VolumeMassProperties Compute(Brep brep)
    {
      if (null == brep)
        return null;

      IntPtr pBrep = brep.ConstPointer();
      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Brep_MassProperties(false, pBrep, relativeTolerance, absoluteTolerance);
      if (IntPtr.Zero == rc)
        return null;
      return new VolumeMassProperties(rc, false);
    }

    /// <summary>
    /// Compute the VolumeMassProperties for a single Surface.
    /// </summary>
    /// <param name="surface">Surface to measure.</param>
    /// <returns>The VolumeMassProperties for the given Surface or null on failure.</returns>
    public static VolumeMassProperties Compute(Surface surface)
    {
      if (null == surface)
        return null;

      IntPtr pSurface = surface.ConstPointer();
      double relativeTolerance = 1.0e-6;
      double absoluteTolerance = 1.0e-6;
      IntPtr rc = UnsafeNativeMethods.ON_Surface_MassProperties(false, pSurface, relativeTolerance, absoluteTolerance);
      if (IntPtr.Zero == rc)
        return null;
      return new VolumeMassProperties(rc, false);
    }
    #region properties
    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    /// <summary>
    /// Gets the volume solution.
    /// </summary>
    public double Volume
    {
      get { return UnsafeNativeMethods.ON_MassProperties_Mass(m_ptr); }
    }

    /// <summary>
    /// Gets the uncertainty in the volume calculation.
    /// </summary>
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
    /// Z is integral of "z dm" over the volume
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
    /// Uncertainty in world coordinates first moments calculation
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
    /// Z is integral of "zz dm" over the area
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
    /// Uncertainty in world coordinates second moments calculation
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
    /// Z is integral of "zx dm" over the area
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
    /// Uncertainty in world coordinates second moments calculation
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
    /// Z = integral of (z^2 + y^2) dm
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
    /// Uncertainty in world coordinates moments of inertia calculation
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
    /// Uncertainty in centroid coordinates second moments calculation
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
    /// Uncertainty in centroid coordinates moments of inertia calculation
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
    /// <param name="summand">mass properties to add</param>
    /// <returns>True if successful.</returns>
    public bool Sum(VolumeMassProperties summand)
    {
      IntPtr pSum = summand.ConstPointer();
      return UnsafeNativeMethods.ON_MassProperties_Sum(m_ptr, pSum);
    }
    #endregion
  }
}