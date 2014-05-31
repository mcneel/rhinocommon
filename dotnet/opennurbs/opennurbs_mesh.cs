using System;
using System.Collections;
using System.Collections.Generic;
using Rhino.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Rhino.Collections;
using System.Runtime.Serialization;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.Render
{
  /// <summary>
  /// Holds texture mapping information.
  /// </summary>
  public class MappingTag
  {
    /// <summary>
    ///  Gets or sets a map globally unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    ///  Gets or sets a texture mapping type: linear, cylinder, etc...
    /// </summary>
    public TextureMappingType MappingType { get; set; }

    /// <summary>
    /// Gets or sets the cyclic redundancy check on the mapping.
    /// See also <see cref="RhinoMath.CRC32(uint,byte[])" />.
    /// </summary>
    [CLSCompliant(false)]
    public uint MappingCRC { get; set; }

    /// <summary>
    ///  Gets or sets a 4x4 matrix tranform.
    /// </summary>
    public Rhino.Geometry.Transform MeshTransform { get; set; }
  }
}

namespace Rhino.Geometry
{
  /// <summary>
  /// Type of Mesh Parameters used by the RhinoDoc for meshing objects
  /// </summary>
  public enum MeshingParameterStyle : int
  {
    /// <summary>No style</summary>
    None = 0,
    /// <summary></summary>
    Fast = 1,
    /// <summary></summary>
    Quality = 2,
    /// <summary></summary>
    Custom = 9,
    /// <summary></summary>
    PerObject = 10
  }

  /// <summary>
  /// Represents settings used for creating a mesh representation of a brep or surface.
  /// </summary>
  public class MeshingParameters : IDisposable
  {
    IntPtr m_ptr; // This class is never const
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }

    /// <summary>
    /// Initializes a new instance with default values.
    /// <para>Initial values are same as <see cref="Default"/>.</para>
    /// </summary>
    public MeshingParameters()
    {
      m_ptr = UnsafeNativeMethods.ON_MeshParameters_New();
    }

    internal MeshingParameters(IntPtr pMeshingParameters)
    {
      m_ptr = pMeshingParameters;
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~MeshingParameters()
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
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_MeshParameters_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    #region constants
#if RHINO_SDK
    /// <summary>
    /// Gets the MeshingParameters that are currently set for a document.
    /// These are the same settings that are shown in the DocumentProperties
    /// "mesh settings" user interface.
    /// </summary>
    /// <param name="doc">A Rhino document to query.</param>
    /// <returns>Meshing parameters of the document.</returns>
    /// <exception cref="ArgumentNullException">If doc is null.</exception>
    public static MeshingParameters DocumentCurrentSetting(RhinoDoc doc)
    {
      if (doc == null) throw new ArgumentNullException("doc");

      IntPtr ptr_mesh_parameters = UnsafeNativeMethods.CRhinoDocProperties_RenderMeshSettings(doc.m_docId);
      if (IntPtr.Zero == ptr_mesh_parameters)
        return null;
      return new MeshingParameters(ptr_mesh_parameters);
    }
#endif

    /// <summary>Gets minimal meshing parameters.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public static MeshingParameters Minimal
    {
      get
      {
        MeshingParameters mp = new MeshingParameters();
        mp.JaggedSeams = true;
        mp.RefineGrid = false;
        mp.SimplePlanes = false;
        mp.ComputeCurvature = false;

        //mp.Facetype = 0;
        mp.GridMinCount = 16;
        mp.GridMaxCount = 0;

        mp.GridAmplification = 1.0;
        mp.GridAngle = 0.0;
        mp.GridAspectRatio = 6.0;

        mp.Tolerance = 0.0;
        mp.MinimumTolerance = 0.0;

        mp.MinimumEdgeLength = 0.0001;
        mp.MaximumEdgeLength = 0.0;

        mp.RefineAngle = 0.0;
        mp.RelativeTolerance = 0.0;

        return mp;
      }
    }

    /// <summary>
    /// Get default meshing parameters.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public static MeshingParameters Default
    {
      get
      {
        MeshingParameters mp = new MeshingParameters();
        /*
        mp.JaggedSeams = false;
        mp.RefineGrid = true;
        mp.SimplePlanes = false;
        mp.ComputeCurvature = false;

        mp.Facetype = 0;
        mp.GridMinCount = 0;
        mp.GridMaxCount = 0;

        mp.GridAmplification = 1.0;
        mp.GridAngle = (20.0 * Math.PI) / 180.0;
        mp.GridAspectRatio = 6.0;

        mp.Tolerance = 0.0;
        mp.MinimumTolerance = 0.0;

        mp.MinimumEdgeLength = 0.0001;
        mp.MaximumEdgeLength = 0.0;

        mp.RefineAngle = (20.0 * Math.PI) / 180.0;
        mp.RelativeTolerance = 0.0;
        */
        return mp;
      }
    }

    /// <summary>
    /// Gets meshing parameters for coarse meshing. 
    /// <para>This corresponds with the "Jagged and Faster" default in Rhino.</para>
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public static MeshingParameters Coarse
    {
      get
      {
        MeshingParameters mp = new MeshingParameters();

        mp.GridAmplification = 0.0;
        mp.GridAngle = 0.0;
        mp.GridAspectRatio = 0.0;
        mp.RefineAngle = 0.0;

        mp.RelativeTolerance = 0.65;
        mp.GridMinCount = 16;
        mp.MinimumEdgeLength = 0.0001;
        mp.SimplePlanes = true;

        return mp;
      }
    }
    /// <summary>
    /// Gets meshing parameters for smooth meshing. 
    /// <para>This corresponds with the "Smooth and Slower" default in Rhino.</para>
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public static MeshingParameters Smooth
    {
      get
      {
        MeshingParameters mp = new MeshingParameters();

        mp.GridAmplification = 0.0;
        mp.GridAngle = 0.0;
        mp.GridAspectRatio = 0.0;

        mp.RelativeTolerance = 0.8;
        mp.GridMinCount = 16;
        mp.MinimumEdgeLength = 0.0001;
        mp.SimplePlanes = true;
        mp.RefineAngle = (20.0 * Math.PI) / 180.0;

        return mp;
      }
    }
    #endregion

    #region properties
    const int idxJaggedSeams = 0;
    const int idxRefineGrid = 1;
    const int idxSimplePlanes = 2;
    const int idxComputeCurvature = 3;
    bool GetBool(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_MeshParameters_GetBool(ptr, which);
    }
    void SetBool(int which, bool val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_MeshParameters_SetBool(ptr, which, val);
    }
    /// <summary>
    /// Gets or sets whether or not the mesh is allowed to have jagged seams. 
    /// When this flag is set to true, meshes on either side of a Brep Edge will not match up.
    /// </summary>
    public bool JaggedSeams
    {
      get { return GetBool(idxJaggedSeams); }
      set { SetBool(idxJaggedSeams, value); }
    }
    /// <summary>
    /// Gets or sets a value indicating whether or not the sampling grid can be refined 
    /// when certain tolerances are not met.
    /// </summary>
    public bool RefineGrid
    {
      get { return GetBool(idxRefineGrid); }
      set { SetBool(idxRefineGrid, value); }
    }
    /// <summary>
    /// Gets or sets a value indicating whether or not planar areas are allowed 
    /// to be meshed in a simplified manner.
    /// </summary>
    public bool SimplePlanes
    {
      get { return GetBool(idxSimplePlanes); }
      set { SetBool(idxSimplePlanes, value); }
    }
    /// <summary>
    /// Gets or sets a value indicating whether or not surface curvature 
    /// data will be embedded in the mesh.
    /// </summary>
    public bool ComputeCurvature
    {
      get { return GetBool(idxComputeCurvature); }
      set { SetBool(idxComputeCurvature, value); }
    }


    int GetGridCount(bool min)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_MeshParameters_GetGridCount(ptr, min);
    }
    void SetGridCount(bool min, int val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_MeshParameters_SetGridCount(ptr, min, val);
    }

    /// <summary>
    /// Gets or sets the minimum number of grid quads in the initial sampling grid.
    /// </summary>
    public int GridMinCount
    {
      get { return GetGridCount(true); }
      set { SetGridCount(true, value); }
    }
    /// <summary>
    /// Gets or sets the maximum number of grid quads in the initial sampling grid.
    /// </summary>
    public int GridMaxCount
    {
      get { return GetGridCount(false); }
      set { SetGridCount(false, value); }
    }


    const int idxGridAngle = 0;
    const int idxGridAspectRatio = 1;
    const int idxGridAmplification = 2;
    const int idxTolerance = 3;
    const int idxMinimumTolerance = 4;
    const int idxRelativeTolerance = 5;
    const int idxMinimumEdgeLength = 6;
    const int idxMaximumEdgeLength = 7;
    const int idxRefineAngle = 8;
    double GetDouble(int which)
    {
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_MeshParameters_GetDouble(ptr, which);
    }
    void SetDouble(int which, double val)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_MeshParameters_SetDouble(ptr, which, val);
    }

    /// <summary>
    /// Gets or sets the maximum allowed angle difference (in radians) 
    /// for a single sampling quad. The angle pertains to the surface normals.
    /// </summary>
    public double GridAngle
    {
      get { return GetDouble(idxGridAngle); }
      set { SetDouble(idxGridAngle, value); }
    }
    /// <summary>
    /// Gets or sets the maximum allowed aspect ratio of sampling quads.
    /// </summary>
    public double GridAspectRatio
    {
      get { return GetDouble(idxGridAspectRatio); }
      set { SetDouble(idxGridAspectRatio, value); }
    }
    /// <summary>
    /// Gets or sets the grid amplification factor. 
    /// Values lower than 1.0 will decrease the number of initial quads, 
    /// values higher than 1.0 will increase the number of initial quads.
    /// </summary>
    public double GridAmplification
    {
      get { return GetDouble(idxGridAmplification); }
      set { SetDouble(idxGridAmplification, value); }
    }

    /// <summary>
    /// Gets or sets the maximum allowed edge deviation. 
    /// This tolerance is measured between the center of the mesh edge and the surface.
    /// </summary>
    public double Tolerance
    {
      get { return GetDouble(idxTolerance); }
      set { SetDouble(idxTolerance, value); }
    }
    /// <summary>
    /// Gets or sets the minimum tolerance.
    /// </summary>
    public double MinimumTolerance
    {
      get { return GetDouble(idxMinimumTolerance); }
      set { SetDouble(idxMinimumTolerance, value); }
    }
    /// <summary>
    /// Gets or sets the relative tolerance.
    /// </summary>
    public double RelativeTolerance
    {
      get { return GetDouble(idxRelativeTolerance); }
      set { SetDouble(idxRelativeTolerance, value); }
    }

    /// <summary>
    /// Gets or sets the minimum allowed mesh edge length.
    /// </summary>
    public double MinimumEdgeLength
    {
      get { return GetDouble(idxMinimumEdgeLength); }
      set { SetDouble(idxMinimumEdgeLength, value); }
    }
    /// <summary>
    /// Gets or sets the maximum allowed mesh edge length.
    /// </summary>
    public double MaximumEdgeLength
    {
      get { return GetDouble(idxMaximumEdgeLength); }
      set { SetDouble(idxMaximumEdgeLength, value); }
    }

    /// <summary>
    /// Gets or sets the mesh parameter refine angle.
    /// </summary>
    public double RefineAngle
    {
      get { return GetDouble(idxRefineAngle); }
      set { SetDouble(idxRefineAngle, value); }
    }
    #endregion
  }

  /// <summary>
  /// Represents a portion of a mesh for partitioning
  /// </summary>
  public class MeshPart
  {
    private int m_vi0;
    private int m_vi1;
    private int m_fi0;
    private int m_fi1;
    private int m_vertex_count;
    private int m_triangle_count;

    internal MeshPart(int vertexStart, int vertexEnd, int faceStart, int faceEnd, int vertexCount, int triangleCount)
    {
      m_vi0 = vertexStart;
      m_vi1 = vertexEnd;
      m_fi0 = faceStart;
      m_fi1 = faceEnd;
      m_vertex_count = vertexCount;
      m_triangle_count = triangleCount;
    }

    /// <summary>Start of subinterval of parent mesh vertex array</summary>
    public int StartVertexIndex { get { return m_vi0; } }
    /// <summary>End of subinterval of parent mesh vertex array</summary>
    public int EndVertexIndex { get { return m_vi1; } }
    /// <summary>Start of subinterval of parent mesh face array</summary>
    public int StartFaceIndex { get { return m_fi0; } }
    /// <summary>End of subinterval of parent mesh face array</summary>
    public int EndFaceIndex { get { return m_fi1; } }

    /// <summary>EndVertexIndex - StartVertexIndex</summary>
    public int VertexCount { get { return m_vertex_count; } }
    /// <summary></summary>
    public int TriangleCount { get { return m_triangle_count; } }
  }

  /// <summary>
  /// Represents a geometry type that is defined by vertices and faces.
  /// <para>This is often called a face-vertex mesh.</para>
  /// </summary>
  //[Serializable]
  public class Mesh : GeometryBase
  {
    #region static mesh creation
#if RHINO_SDK
    /// <summary>
    /// Constructs a planar mesh grid.
    /// </summary>
    /// <param name="plane">Plane of mesh.</param>
    /// <param name="xInterval">Interval describing size and extends of mesh along plane x-direction.</param>
    /// <param name="yInterval">Interval describing size and extends of mesh along plane y-direction.</param>
    /// <param name="xCount">Number of faces in x-direction.</param>
    /// <param name="yCount">Number of faces in y-direction.</param>
    /// <exception cref="ArgumentException">Thrown when plane is a null reference.</exception>
    /// <exception cref="ArgumentException">Thrown when xInterval is a null reference.</exception>
    /// <exception cref="ArgumentException">Thrown when yInterval is a null reference.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when xCount is less than or equal to zero.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when yCount is less than or equal to zero.</exception>
    public static Mesh CreateFromPlane(Plane plane, Interval xInterval, Interval yInterval, int xCount, int yCount)
    {
      if (!plane.IsValid) { throw new ArgumentException("plane is invalid"); }
      if (!xInterval.IsValid) { throw new ArgumentException("xInterval is invalid"); }
      if (!yInterval.IsValid) { throw new ArgumentException("yInterval is invalid"); }
      if (xCount <= 0) { throw new ArgumentOutOfRangeException("xCount"); }
      if (yCount <= 0) { throw new ArgumentOutOfRangeException("yCount"); }

      IntPtr ptr = UnsafeNativeMethods.ON_Mesh_CreateMeshPlane(ref plane, xInterval, yInterval, xCount, yCount);

      if (IntPtr.Zero == ptr)
        return null;
      return new Mesh(ptr, null);
    }

    /// <summary>
    /// Constructs new mesh that matches a bounding box.
    /// </summary>
    /// <param name="box">A box to use for creation.</param>
    /// <param name="xCount">Number of faces in x-direction.</param>
    /// <param name="yCount">Number of faces in y-direction.</param>
    /// <param name="zCount">Number of faces in z-direction.</param>
    /// <returns>A new brep, or null on failure.</returns>
    public static Mesh CreateFromBox(BoundingBox box, int xCount, int yCount, int zCount)
    {
      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoMeshBox2(box.Min, box.Max, xCount, yCount, zCount);
      return IntPtr.Zero == ptr ? null : new Mesh(ptr, null);
    }

    /// <summary>
    ///  Constructs new mesh that matches an aligned box.
    /// </summary>
    /// <param name="box">Box to match.</param>
    /// <param name="xCount">Number of faces in x-direction.</param>
    /// <param name="yCount">Number of faces in y-direction.</param>
    /// <param name="zCount">Number of faces in z-direction.</param>
    /// <returns></returns>
    public static Mesh CreateFromBox(Box box, int xCount, int yCount, int zCount)
    {
      return CreateFromBox(box.GetCorners(), xCount, yCount, zCount);
    }

    /// <summary>
    /// Constructs new mesh from 8 corner points.
    /// </summary>
    /// <param name="corners">
    /// 8 points defining the box corners arranged as the vN labels indicate.
    /// <pre>
    /// <para>v7_____________v6</para>
    /// <para>|\             |\</para>
    /// <para>| \            | \</para>
    /// <para>|  \ _____________\</para>
    /// <para>|   v4         |   v5</para>
    /// <para>|   |          |   |</para>
    /// <para>|   |          |   |</para>
    /// <para>v3--|----------v2  |</para>
    /// <para> \  |           \  |</para>
    /// <para>  \ |            \ |</para>
    /// <para>   \|             \|</para>
    /// <para>    v0_____________v1</para>
    /// </pre>
    /// </param>
    /// <param name="xCount">Number of faces in x-direction.</param>
    /// <param name="yCount">Number of faces in y-direction.</param>
    /// <param name="zCount">Number of faces in z-direction.</param>
    /// <returns>A new brep, or null on failure.</returns>
    /// <returns>A new box mesh, on null on error.</returns>
    public static Mesh CreateFromBox(IEnumerable<Point3d> corners, int xCount, int yCount, int zCount)
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

      IntPtr ptr = UnsafeNativeMethods.RHC_RhinoMeshBox(box_corners, xCount, yCount, zCount);
      return IntPtr.Zero == ptr ? null : new Mesh(ptr, null);
    }

    /// <summary>
    /// Constructs a mesh sphere.
    /// </summary>
    /// <param name="sphere">Base sphere for mesh.</param>
    /// <param name="xCount">Number of faces in the around direction.</param>
    /// <param name="yCount">Number of faces in the top-to-bottom direction.</param>
    /// <exception cref="ArgumentException">Thrown when sphere is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when xCount is less than or equal to two.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when yCount is less than or equal to two.</exception>
    /// <returns></returns>
    public static Mesh CreateFromSphere(Sphere sphere, int xCount, int yCount)
    {
      if (!sphere.IsValid) { throw new ArgumentException("sphere is invalid"); }
      if (xCount < 2) { throw new ArgumentOutOfRangeException("xCount"); }
      if (yCount < 2) { throw new ArgumentOutOfRangeException("yCount"); }

      IntPtr ptr = UnsafeNativeMethods.ON_Mesh_CreateMeshSphere(ref sphere.m_plane, sphere.m_radius, xCount, yCount);

      if (IntPtr.Zero == ptr)
        return null;
      return new Mesh(ptr, null);
    }

    /// <summary>Constructs a mesh cylinder</summary>
    /// <param name="cylinder"></param>
    /// <param name="vertical">Number of faces in the top-to-bottom direction</param>
    /// <param name="around">Number of faces around the cylinder</param>
    /// <exception cref="ArgumentException">Thrown when cylinder is invalid.</exception>
    /// <returns></returns>
    public static Mesh CreateFromCylinder(Cylinder cylinder, int vertical, int around)
    {
      if (!cylinder.IsValid) { throw new ArgumentException("cylinder is invalid"); }
      IntPtr ptr_mesh = UnsafeNativeMethods.RHC_RhinoMeshCylinder(ref cylinder, vertical, around);
      return CreateGeometryHelper(ptr_mesh, null) as Mesh;
    }

    /// <summary>Constructs a mesh cone</summary>
    /// <param name="cone"></param>
    /// <param name="vertical">Number of faces in the top-to-bottom direction</param>
    /// <param name="around">Number of faces around the cone</param>
    /// <exception cref="ArgumentException">Thrown when cone is invalid.</exception>
    /// <returns></returns>
    public static Mesh CreateFromCone(Cone cone, int vertical, int around)
    {
      if (!cone.IsValid) { throw new ArgumentException("cone is invalid"); }
      IntPtr ptr_mesh = UnsafeNativeMethods.RHC_RhinoMeshCone(ref cone, vertical, around);
      return CreateGeometryHelper(ptr_mesh, null) as Mesh;
    }

    /// <summary>
    /// Attempts to construct a mesh from a closed planar curve.
    /// </summary>
    /// <param name="boundary">must be a closed planar curve.</param>
    /// <param name="parameters">parameters used for creating the mesh.</param>
    /// <returns>
    /// New mesh on success or null on failure.
    /// </returns>
    public static Mesh CreateFromPlanarBoundary(Curve boundary, MeshingParameters parameters)
    {
      IntPtr ptr_const_curve = boundary.ConstPointer();
      IntPtr ptr_const_mesh_parameters = parameters.ConstPointer();
      IntPtr ptr_mesh = UnsafeNativeMethods.RHC_RhinoMakePlanarMeshes(ptr_const_curve, ptr_const_mesh_parameters);
      return CreateGeometryHelper(ptr_mesh, null) as Mesh;
    }

    /// <summary>
    /// Attempts to create a Mesh that is a triangulation of a closed polyline
    /// </summary>
    /// <param name="polyline">must be closed</param>
    /// <returns>
    /// New mesh on success or null on failure.
    /// </returns>
    public static Mesh CreateFromClosedPolyline(Polyline polyline)
    {
      if (!polyline.IsClosed)
        return null;
      Mesh rc = new Mesh();
      IntPtr ptr_mesh = rc.NonConstPointer();
      if (UnsafeNativeMethods.TLC_MeshPolyline(polyline.Count, polyline.ToArray(), ptr_mesh))
        return rc;
      return null;
    }

    /// <summary>
    /// Constructs a mesh from a brep.
    /// </summary>
    /// <param name="brep">Brep to approximate.</param>
    /// <returns>An array of meshes.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_tightboundingbox.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_tightboundingbox.cs' lang='cs'/>
    /// <code source='examples\py\ex_tightboundingbox.py' lang='py'/>
    /// </example>
    public static Mesh[] CreateFromBrep(Brep brep)
    {
      using (Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr ptr_const_brep = brep.ConstPointer();
        IntPtr ptr_mesh_array = meshes.NonConstPointer();
        int count = UnsafeNativeMethods.ON_Brep_CreateMesh(ptr_const_brep, ptr_mesh_array);
        return count < 1 ? null : meshes.ToNonConstArray();
      }
    }

    /// <summary>
    /// Constructs a mesh from a brep.
    /// </summary>
    /// <param name="brep">Brep to approximate.</param>
    /// <param name="meshingParameters">Parameters to use during meshing.</param>
    /// <returns>An array of meshes.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public static Mesh[] CreateFromBrep(Brep brep, MeshingParameters meshingParameters)
    {
      IntPtr ptr_const_brep = brep.ConstPointer();
      IntPtr pMeshParameters = meshingParameters.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr ptr_mesh_array = meshes.NonConstPointer();
        int count = UnsafeNativeMethods.ON_Brep_CreateMesh3(ptr_const_brep, ptr_mesh_array, pMeshParameters);
        return count < 1 ? null : meshes.ToNonConstArray();
      }
    }

    /// <summary>
    /// Computes the solid union of a set of meshes.
    /// </summary>
    /// <param name="meshes">Meshes to union.</param>
    /// <returns>An array of Mesh results or null on failure.</returns>
    public static Mesh[] CreateBooleanUnion(IEnumerable<Mesh> meshes)
    {
      if (null == meshes)
        return null;

      Runtime.InteropWrappers.SimpleArrayMeshPointer input = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh mesh in meshes)
      {
        if (null == mesh)
          continue;
        input.Add(mesh, true);
      }
      Runtime.InteropWrappers.SimpleArrayMeshPointer output = new Runtime.InteropWrappers.SimpleArrayMeshPointer();

      IntPtr pInput = input.ConstPointer();
      IntPtr pOutput = output.NonConstPointer();

      // Fugier uses the following two tolerances in RhinoScript for all MeshBooleanUnion
      // calculations.
      const double MeshBoolIntersectionTolerance = RhinoMath.SqrtEpsilon * 10.0;
      const double MeshBoolOverlapTolerance = RhinoMath.SqrtEpsilon * 10.0;

      Mesh[] rc = null;
      if (UnsafeNativeMethods.RHC_RhinoMeshBooleanUnion(pInput, MeshBoolIntersectionTolerance, MeshBoolOverlapTolerance, pOutput))
      {
        rc = output.ToNonConstArray();
      }

      input.Dispose();
      output.Dispose();

      return rc;
    }

    const int idxIntersect = 0;
    const int idxDifference = 1;
    const int idxSplit = 2;
    static Mesh[] MeshBooleanHelper(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet, int which)
    {
      if (null == firstSet || null == secondSet)
        return null;

      Runtime.InteropWrappers.SimpleArrayMeshPointer input1 = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh mesh in firstSet)
      {
        if (null == mesh)
          continue;
        input1.Add(mesh, true);
      }

      Runtime.InteropWrappers.SimpleArrayMeshPointer input2 = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh mesh in secondSet)
      {
        if (null == mesh)
          continue;
        input2.Add(mesh, true);
      }

      Runtime.InteropWrappers.SimpleArrayMeshPointer output = new Runtime.InteropWrappers.SimpleArrayMeshPointer();

      IntPtr pInput1 = input1.ConstPointer();
      IntPtr pInput2 = input2.ConstPointer();
      IntPtr pOutput = output.NonConstPointer();

      // Fugier uses the following two tolerances in RhinoScript for all MeshBoolean...
      // calculations.
      const double MeshBoolIntersectionTolerance = RhinoMath.SqrtEpsilon * 10.0;
      const double MeshBoolOverlapTolerance = RhinoMath.SqrtEpsilon * 10.0;

      Mesh[] rc = null;
      if (UnsafeNativeMethods.RHC_RhinoMeshBooleanIntDiff(pInput1, pInput2, MeshBoolIntersectionTolerance, MeshBoolOverlapTolerance, pOutput, which))
      {
        rc = output.ToNonConstArray();
      }

      input1.Dispose();
      input2.Dispose();
      output.Dispose();

      return rc;
    }

    /// <summary>
    /// Computes the solid difference of two sets of Meshes.
    /// </summary>
    /// <param name="firstSet">First set of Meshes (the set to subtract from).</param>
    /// <param name="secondSet">Second set of Meshes (the set to subtract).</param>
    /// <returns>An array of Mesh results or null on failure.</returns>
    public static Mesh[] CreateBooleanDifference(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet)
    {
      return MeshBooleanHelper(firstSet, secondSet, idxDifference);
    }
    /// <summary>
    /// Computes the solid intersection of two sets of meshes.
    /// </summary>
    /// <param name="firstSet">First set of Meshes.</param>
    /// <param name="secondSet">Second set of Meshes.</param>
    /// <returns>An array of Mesh results or null on failure.</returns>
    public static Mesh[] CreateBooleanIntersection(IEnumerable<Mesh> firstSet, IEnumerable<Mesh> secondSet)
    {
      return MeshBooleanHelper(firstSet, secondSet, idxIntersect);
    }

    /// <summary>
    /// Splits a set of meshes with another set.
    /// </summary>
    /// <param name="meshesToSplit">A list, an array, or any enumerable set of meshes to be split. If this is null, null will be returned.</param>
    /// <param name="meshSplitters">A list, an array, or any enumerable set of meshes that cut. If this is null, null will be returned.</param>
    /// <returns>A new mesh array, or null on error.</returns>
    public static Mesh[] CreateBooleanSplit(IEnumerable<Mesh> meshesToSplit, IEnumerable<Mesh> meshSplitters)
    {
      return MeshBooleanHelper(meshesToSplit, meshSplitters, idxSplit);
    }
#endif
    #endregion

    #region constructors
    /// <summary>Initializes a new empty mesh.</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public Mesh()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_Mesh_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
      ApplyMemoryPressure();
    }

    ///// <summary>
    ///// Protected constructor for internal use.
    ///// </summary>
    ///// <param name="info">Serialization data.</param>
    ///// <param name="context">Serialization stream.</param>
    //protected Mesh(SerializationInfo info, StreamingContext context)
    //  : base(info, context)
    //{
    //}

    internal override IntPtr _InternalGetConstPointer()
    {
      MeshHolder mh = m__parent as MeshHolder;
      if (mh != null)
        return mh.MeshPointer();

      return base._InternalGetConstPointer();
    }

    /// <summary>
    /// Performs some memory cleanup if necessary
    /// </summary>
    protected override void OnSwitchToNonConst()
    {
      MeshHolder mh = m__parent as MeshHolder;
      base.OnSwitchToNonConst();

      if (mh != null)
      {
        m__parent = null;
        mh.ReleaseMesh();
      }
    }

    internal override object _GetConstObjectParent()
    {
      if (!IsDocumentControlled)
        return null;
      return base._GetConstObjectParent();
    }

    internal Mesh(IntPtr nativePointer, object parent)
      : base(nativePointer, parent, -1)
    {
      if (null == parent)
        ApplyMemoryPressure();
    }

    /// <summary>
    /// Copies mesh values into this mesh from another mesh.
    /// </summary>
    /// <param name="other">The other mesh to copy from.</param>
    /// <exception cref="ArgumentNullException">If other is null.</exception>
    public void CopyFrom(Mesh other)
    {
      if (other == null) throw new ArgumentNullException("other");

      IntPtr pConstOther = other.ConstPointer();
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_CopyFrom(pConstOther, pThis);
    }

    /// <summary>
    /// Constructs a copy of this mesh.
    /// This is the same as <see cref="DuplicateMesh"/>.
    /// </summary>
    /// <returns>A mesh.</returns>
    public override GeometryBase Duplicate()
    {
      IntPtr ptr = ConstPointer();
      IntPtr pNewMesh = UnsafeNativeMethods.ON_Mesh_New(ptr);
      return new Mesh(pNewMesh, null);
    }

    /// <summary>Constructs a copy of this mesh.
    /// This is the same as <see cref="Duplicate"/>.
    /// </summary>
    public Mesh DuplicateMesh()
    {
      return Duplicate() as Mesh;
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new Mesh(IntPtr.Zero, null);
    }
    #endregion

    #region constants
    internal const int idxUnitizeVertexNormals = 0;
    internal const int idxUnitizeFaceNormals = 1;
    internal const int idxConvertQuadsToTriangles = 2;
    internal const int idxComputeFaceNormals = 3;
    internal const int idxCompact = 4;
    internal const int idxComputeVertexNormals = 5;
    internal const int idxNormalizeTextureCoordinates = 6;
    internal const int idxTransposeTextureCoordinates = 7;
    internal const int idxTransposeSurfaceParameters = 8;

    // UnsafeNativeMethods.ON_Mesh_GetInt
    internal const int idxVertexCount = 0;
    internal const int idxFaceCount = 1;
    internal const int idxQuadCount = 2;
    internal const int idxTriangleCount = 3;
    internal const int idxHiddenVertexCount = 4;
    const int idxDisjointMeshCount = 5;
    internal const int idxFaceNormalCount = 6;
    internal const int idxNormalCount = 7;
    internal const int idxColorCount = 8;
    internal const int idxTextureCoordinateCount = 9;
    internal const int idxMeshTopologyVertexCount = 10;
    const int idxSolidOrientation = 11;
    internal const int idxMeshTopologyEdgeCount = 12;

    internal const int idxHasVertexNormals = 0;
    internal const int idxHasFaceNormals = 1;
    internal const int idxHasTextureCoordinates = 2;
    internal const int idxHasSurfaceParameters = 3;
    internal const int idxHasPrincipalCurvatures = 4;
    internal const int idxHasVertexColors = 5;
    const int idxIsClosed = 6;

    // UnsafeNativeMethods.ON_Mesh_ClearList
    internal const int idxClearVertices = 0;
    internal const int idxClearFaces = 1;
    internal const int idxClearNormals = 2;
    internal const int idxClearFaceNormals = 3;
    internal const int idxClearColors = 4;
    internal const int idxClearTextureCoordinates = 5;
    internal const int idxClearHiddenVertices = 6;

    internal const int idxHideVertex = 0;
    internal const int idxShowVertex = 1;
    internal const int idxHideAll = 2;
    internal const int idxShowAll = 3;
    internal const int idxEnsureHiddenList = 4;
    internal const int idxCleanHiddenList = 5;

    // IndexOpBool
    internal const int idxCollapseEdge = 0;
    internal const int idxIsSwappableEdge = 1;
    internal const int idxSwapEdge = 2;

    // TopItemIsHidden
    internal const int idxTopVertexIsHidden = 0;
    internal const int idxTopEdgeIsHidden = 1;
    internal const int idxTopFaceIsHidden = 2;

    #endregion

    #region properties
    /// <summary>
    /// Gets the number of disjoint (topologically unconnected) pieces in this mesh.
    /// </summary>
    public int DisjointMeshCount
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, idxDisjointMeshCount);
      }
    }

    /// <summary>
    /// Gets a value indicating whether a mesh is considered to be closed (solid).
    /// A mesh is considered solid when every mesh edge borders two or more faces.
    /// </summary>
    /// <returns>true if the mesh is closed, false if it is not.</returns>
    public bool IsClosed
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetBool(ptr, idxIsClosed);
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not the mesh is manifold. 
    /// A manifold mesh does not have any edge that borders more than two faces.
    /// </summary>
    /// <param name="topologicalTest">
    /// If true, the query treats coincident vertices as the same.
    /// </param>
    /// <param name="isOriented">
    /// isOriented will be set to true if the mesh is a manifold 
    /// and adjacent faces have compatible face normals.
    /// </param>
    /// <param name="hasBoundary">
    /// hasBoundary will be set to true if the mesh is a manifold 
    /// and there is at least one "edge" with no more than one adjacent face.
    /// </param>
    /// <returns>true if every mesh "edge" has at most two adjacent faces.</returns>
    public bool IsManifold(bool topologicalTest, out bool isOriented, out bool hasBoundary)
    {
      isOriented = false;
      hasBoundary = false;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_IsManifold(ptr, topologicalTest, ref isOriented, ref hasBoundary);
    }

    #region fake list access
    private Rhino.Geometry.Collections.MeshVertexList m_vertices;
    /// <summary>
    /// Gets access to the vertices set of this mesh.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public Rhino.Geometry.Collections.MeshVertexList Vertices
    {
      get { return m_vertices ?? (m_vertices = new Rhino.Geometry.Collections.MeshVertexList(this)); }
    }

    private Rhino.Geometry.Collections.MeshTopologyVertexList m_topology_vertices;

    /// <summary>
    /// Gets the <see cref="Rhino.Geometry.Collections.MeshTopologyVertexList"/> object associated with this mesh.
    /// <para>This object stores vertex connectivity and the indices of vertices
    /// that were unified while computing the edge topology.</para>
    /// </summary>
    public Rhino.Geometry.Collections.MeshTopologyVertexList TopologyVertices
    {
      get
      {
        return m_topology_vertices ?? (m_topology_vertices = new Collections.MeshTopologyVertexList(this));
      }
    }

    private Rhino.Geometry.Collections.MeshTopologyEdgeList m_topology_edges;

    /// <summary>
    /// Gets the <see cref="Rhino.Geometry.Collections.MeshTopologyEdgeList"/> object associated with this mesh.
    /// <para>This object stores edge connectivity.</para>
    /// </summary>
    public Rhino.Geometry.Collections.MeshTopologyEdgeList TopologyEdges
    {
      get
      {
        return m_topology_edges ?? (m_topology_edges = new Collections.MeshTopologyEdgeList(this));
      }
    }

    private Rhino.Geometry.Collections.MeshVertexNormalList m_normals;
    /// <summary>
    /// Gets access to the vertex normal collection in this mesh.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public Rhino.Geometry.Collections.MeshVertexNormalList Normals
    {
      get { return m_normals ?? (m_normals = new Rhino.Geometry.Collections.MeshVertexNormalList(this)); }
    }

    private Rhino.Geometry.Collections.MeshFaceList m_faces;
    /// <summary>
    /// Gets access to the faces collection in this mesh.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public Rhino.Geometry.Collections.MeshFaceList Faces
    {
      get { return m_faces ?? (m_faces = new Rhino.Geometry.Collections.MeshFaceList(this)); }
    }

    private Rhino.Geometry.Collections.MeshFaceNormalList m_facenormals;
    /// <summary>
    /// Gets access to the face normal collection in this mesh.
    /// </summary>
    public Rhino.Geometry.Collections.MeshFaceNormalList FaceNormals
    {
      get { return m_facenormals ?? (m_facenormals = new Rhino.Geometry.Collections.MeshFaceNormalList(this)); }
    }

    private Rhino.Geometry.Collections.MeshVertexColorList m_vertexcolors;
    /// <summary>
    /// Gets access to the (optional) vertex color collection in this mesh.
    /// </summary>
    public Rhino.Geometry.Collections.MeshVertexColorList VertexColors
    {
      get { return m_vertexcolors ?? (m_vertexcolors = new Rhino.Geometry.Collections.MeshVertexColorList(this)); }
    }

    private Rhino.Geometry.Collections.MeshTextureCoordinateList m_texcoords;
    /// <summary>
    /// Gets access to the vertex texture coordinate collection in this mesh.
    /// </summary>
    public Rhino.Geometry.Collections.MeshTextureCoordinateList TextureCoordinates
    {
      get { return m_texcoords ?? (m_texcoords = new Rhino.Geometry.Collections.MeshTextureCoordinateList(this)); }
    }

    /// <summary>
    /// Remove all texture coordinate information from this mesh.
    /// </summary>
    public void ClearTextureData()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_DestroyTextureData(pThis);
    }
    #endregion
    #endregion properties

    #region methods
    /// <summary>
    /// If the mesh has SurfaceParameters, the surface is evaluated at
    /// these parameters and the mesh geometry is updated.
    /// </summary>
    /// <param name="surface">An input surface.</param>
    /// <returns>true if the operation succceeded; false otherwise.</returns>
    public bool EvaluateMeshGeometry(Surface surface)
    {
      // don't switch to non-const if we don't have to
      IntPtr pConstThis = ConstPointer();
      if (!UnsafeNativeMethods.ON_Mesh_HasSurfaceParameters(pConstThis))
        return false;
      IntPtr pThis = NonConstPointer();
      IntPtr pConstSurface = surface.ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_EvaluateMeshGeometry(pThis, pConstSurface);
    }

    /// <summary>
    /// Removes any unreferenced objects from arrays, reindexes as needed 
    /// and shrinks arrays to minimum required size.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public bool Compact()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, idxCompact);
    }

    /// <summary>Reverses the direction of the mesh.</summary>
    /// <param name="vertexNormals">If true, vertex normals will be reversed.</param>
    /// <param name="faceNormals">If true, face normals will be reversed.</param>
    /// <param name="faceOrientation">If true, face orientations will be reversed.</param>
    public void Flip(bool vertexNormals, bool faceNormals, bool faceOrientation)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_Flip(ptr, vertexNormals, faceNormals, faceOrientation);
    }

    /// <summary>
    /// Determines orientation of a "solid" mesh.
    /// </summary>
    /// <returns>
    /// <para>+1 = mesh is solid with outward facing normals.</para>
    /// <para>-1 = mesh is solid with inward facing normals.</para>
    /// <para>0 = mesh is not solid.</para>
    /// </returns>
    public int SolidOrientation()
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_GetInt(pConstThis, idxSolidOrientation);
    }

    /// <summary>
    /// Determines if a point is inside a solid mesh.
    /// </summary>
    /// <param name="point">3d point to test.</param>
    /// <param name="tolerance">
    /// (&gt;=0) 3d distance tolerance used for ray-mesh intersection
    /// and determining strict inclusion.
    /// </param>
    /// <param name="strictlyIn">
    /// If strictlyIn is true, then point must be inside mesh by at least
    /// tolerance in order for this function to return true.
    /// If strictlyIn is false, then this function will return true if
    /// point is inside or the distance from point to a mesh face is &lt;= tolerance.
    /// </param>
    /// <returns>
    /// true if point is inside the solid mesh, false if not.
    /// </returns>
    /// <remarks>
    /// The caller is responsible for making certing the mesh is solid before
    /// calling this function. If the mesh is not solid, the behavior is unpredictable.
    /// </remarks>
    public bool IsPointInside(Point3d point, double tolerance, bool strictlyIn)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_IsPointInside(pConstThis, point, tolerance, strictlyIn);
    }

    // need to implement
    //int GetMeshEdges( ON_SimpleArray<ON_2dex>& edges ) const;
    //int* GetVertexLocationIds(int first_vid, int* Vid, int* Vindex) const;
    //int GetMeshFaceSideList( const int* Vid, struct ON_MeshFaceSide*& sides) const;
    //int GetMeshEdgeList(ON_SimpleArray<ON_2dex>& edge_list, int edge_type_partition[5] ) const;
    //int GetMeshEdgeList(ON_SimpleArray<ON_2dex>& edge_list, ON_SimpleArray<int>& ci_meshtop_edge_map, int edge_type_partition[5]) const;
    //int GetMeshEdgeList(ON_SimpleArray<ON_2dex>& edge_list, ON_SimpleArray<int>& ci_meshtop_edge_map, ON_SimpleArray<int>& ci_meshtop_vertex_map, int edge_type_partition[5]) const;

#if RHINO_SDK
    /// <summary>
    /// Makes sure that faces sharing an edge and having a difference of normal greater
    /// than or equal to angleToleranceRadians have unique vertexes along that edge,
    /// adding vertices if necessary.
    /// </summary>
    /// <param name="angleToleranceRadians">Angle at which to make unique vertices.</param>
    /// <param name="modifyNormals">
    /// Determines whether new vertex normals will have the same vertex normal as the original (false)
    /// or vertex normals made from the corrsponding face normals (true)
    /// </param>
    public void Unweld(double angleToleranceRadians, bool modifyNormals)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.RHC_RhinoUnWeldMesh(pThis, angleToleranceRadians, modifyNormals);
    }

    /// <summary>
    /// Makes sure that faces sharing an edge and having a difference of normal greater
    /// than or equal to angleToleranceRadians share vertexes along that edge, vertex normals
    /// are averaged.
    /// </summary>
    /// <param name="angleToleranceRadians">Angle at which to weld vertices.</param>
    public void Weld(double angleToleranceRadians)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.RHC_RhinoWeldMesh(pThis, angleToleranceRadians);
    }

    /// <summary>
    /// Attempts to fix inconsistencies in the directions of meshfaces for a mesh. This function
    /// does not modify the vertex normals, but rather rearranges the mesh face winding and face
    /// normals to make them all consistent. You may want to call Mesh.Normals.ComputeNormals()
    /// to recompute vertex normals after calling this functions.
    /// </summary>
    /// <returns>number of faces that were modified.</returns>
    public int UnifyNormals()
    {
      int rc;
      if (IsDocumentControlled)
      {
        IntPtr pConstThis = ConstPointer();
        rc = UnsafeNativeMethods.RHC_RhinoUnifyMeshNormals(pConstThis, true);
        if (rc < 1)
          return 0;
      }

      IntPtr pThis = NonConstPointer();
      rc = UnsafeNativeMethods.RHC_RhinoUnifyMeshNormals(pThis, false);
      return rc;
    }

    /// <summary>
    /// Splits up the mesh into its unconnected pieces.
    /// </summary>
    /// <returns>An array containing all the disjoint pieces that make up this Mesh.</returns>
    public Mesh[] SplitDisjointPieces()
    {
      IntPtr pConstThis = ConstPointer();
      using (Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pMeshArray = meshes.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoSplitDisjointMesh(pConstThis, pMeshArray);
        return meshes.ToNonConstArray();
      }
    }

    /// <summary>
    /// Split a mesh by an infinite plane.
    /// </summary>
    /// <param name="plane">The splitting plane.</param>
    /// <returns>A new mesh array with the split result. This can be null if no result was found.</returns>
    public Mesh[] Split(Plane plane)
    {
      IntPtr pConstThis = ConstPointer();
      using (Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pMeshArray = meshes.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoMeshBooleanSplit(pConstThis, pMeshArray, ref plane);
        return meshes.ToNonConstArray();
      }
    }
    /// <summary>
    /// Split a mesh with another mesh.
    /// </summary>
    /// <param name="mesh">Mesh to split with.</param>
    /// <returns>An array of mesh segments representing the split result.</returns>
    public Mesh[] Split(Mesh mesh)
    {
      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer pMeshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      pMeshes.Add(this, true);

      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer pSplitters = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      pSplitters.Add(mesh, true);

      using (Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pResult = meshes.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoMeshBooleanSplit2(pMeshes.ConstPointer(), pSplitters.ConstPointer(), pResult);
        return meshes.ToNonConstArray();
      }
    }
    /// <summary>
    /// Split a mesh with a collection of meshes.
    /// </summary>
    /// <param name="meshes">Meshes to split with.</param>
    /// <returns>An array of mesh segments representing the split result.</returns>
    public Mesh[] Split(IEnumerable<Mesh> meshes)
    {
      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer pMeshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      pMeshes.Add(this, true);

      Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer pSplitters = new Runtime.InteropWrappers.SimpleArrayMeshPointer();
      foreach (Mesh mesh in meshes)
      {
        if (mesh != null)
        {
          pSplitters.Add(mesh, true);
        }
      }

      using (Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer on_meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pResult = on_meshes.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoMeshBooleanSplit2(pMeshes.ConstPointer(), pSplitters.ConstPointer(),pResult);
        return on_meshes.ToNonConstArray();
      }
    }

    /// <summary>
    /// Constructs the outlines of a mesh projected against a plane.
    /// </summary>
    /// <param name="plane">A plane to project against.</param>
    /// <returns>An array of polylines, or null on error.</returns>
    public Polyline[] GetOutlines(Plane plane)
    {
      IntPtr pConstMesh = ConstPointer();
      int polylines_created = 0;
      IntPtr pPolys = UnsafeNativeMethods.TL_GetMeshOutline(pConstMesh, ref plane, ref polylines_created);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetCount(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetPoints(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_Delete(pPolys, true);

      return rc;
    }

    /// <summary>
    /// Constructs the outlines of a mesh. The projection information in the
    /// viewport is used to determine how the outlines are projected.
    /// </summary>
    /// <param name="viewport">A viewport to determine projection direction.</param>
    /// <returns>An array of polylines, or null on error.</returns>
    public Polyline[] GetOutlines(Rhino.Display.RhinoViewport viewport)
    {
      IntPtr pConstMesh = ConstPointer();
      int polylines_created = 0;
      IntPtr pConstRhinoViewport = viewport.ConstPointer();
      IntPtr pPolys = UnsafeNativeMethods.TL_GetMeshOutline2(pConstMesh, pConstRhinoViewport, ref polylines_created);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetCount(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetPoints(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_Delete(pPolys, true);

      return rc;
    }

    /// <summary>
    /// Returns all edges of a mesh that are considered "naked" in the
    /// sense that the edge only has one face.
    /// </summary>
    /// <returns>An array of polylines, or null on error.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_dupmeshboundary.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_dupmeshboundary.cs' lang='cs'/>
    /// <code source='examples\py\ex_dupmeshboundary.py' lang='py'/>
    /// </example>
    public Polyline[] GetNakedEdges()
    {
      IntPtr pConstThis = ConstPointer();
      int polylines_created = 0;
      IntPtr pPolys = UnsafeNativeMethods.ON_Mesh_GetNakedEdges(pConstThis, ref polylines_created);
      if (polylines_created < 1 || IntPtr.Zero == pPolys)
        return null;

      // convert the C++ polylines created into .NET polylines
      Polyline[] rc = new Polyline[polylines_created];
      for (int i = 0; i < polylines_created; i++)
      {
        int point_count = UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetCount(pPolys, i);
        Polyline pl = new Polyline(point_count);
        if (point_count > 0)
        {
          pl.m_size = point_count;
          UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_GetPoints(pPolys, i, point_count, pl.m_items);
        }
        rc[i] = pl;
      }
      UnsafeNativeMethods.ON_SimpleArray_PolylineCurve_Delete(pPolys, true);
      return rc;
    }

    /// <summary>
    /// Explode the mesh into submeshes where a submesh is a collection of faces that are contained
    /// within a closed loop of "unwelded" edges. Unwelded edges are edges where the faces that share
    /// the edge have unique mesh vertexes (not mesh topology vertexes) at both ends of the edge.
    /// </summary>
    /// <returns>
    /// Array of submeshes on success; null on error. If the count in the returned array is 1, then
    /// nothing happened and the ouput is essentially a copy of the input.
    /// </returns>
    public Mesh[] ExplodeAtUnweldedEdges()
    {
      IntPtr pConstThis = ConstPointer();
      using (Rhino.Runtime.InteropWrappers.SimpleArrayMeshPointer meshes = new Runtime.InteropWrappers.SimpleArrayMeshPointer())
      {
        IntPtr pMeshArray = meshes.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoExplodeMesh(pConstThis, pMeshArray);
        return meshes.ToNonConstArray();
      }
    }
#endif

    /// <summary>
    /// Appends a copy of another mesh to this one and updates indices of appended mesh parts.
    /// </summary>
    /// <param name="other">Mesh to append to this one.</param>
    /// <example>
    /// <code source='examples\vbnet\ex_createmeshfrombrep.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_createmeshfrombrep.cs' lang='cs'/>
    /// <code source='examples\py\ex_createmeshfrombrep.py' lang='py'/>
    /// </example>
    public void Append(Mesh other)
    {
      if (null == other || other.ConstPointer() == ConstPointer())
        return;
      IntPtr ptr = NonConstPointer();
      IntPtr otherPtr = other.ConstPointer();
      UnsafeNativeMethods.ON_Mesh_Append(ptr, otherPtr);
    }

#if RHINO_SDK
    /// <summary>
    /// Gets the point on the mesh that is closest to a given test point.
    /// </summary>
    /// <param name="testPoint">Point to seach for.</param>
    /// <returns>The point on the mesh closest to testPoint, or Point3d.Unset on failure.</returns>
    public Point3d ClosestPoint(Point3d testPoint)
    {
      Point3d pointOnMesh;
      if (ClosestPoint(testPoint, out pointOnMesh, 0.0) < 0)
      {
        return Point3d.Unset;
      }
      return pointOnMesh;
    }

    /// <summary>
    /// Gets the point on the mesh that is closest to a given test point. Similar to the 
    /// ClosestPoint function except this returns a MeshPoint class which includes
    /// extra information beyond just the location of the closest point.
    /// </summary>
    /// <param name="testPoint">The source of the search.</param>
    /// <param name="maximumDistance">
    /// Optional upper bound on the distance from test point to the mesh. 
    /// If you are only interested in finding a point Q on the mesh when 
    /// testPoint.DistanceTo(Q) &lt; maximumDistance, 
    /// then set maximumDistance to that value. 
    /// This parameter is ignored if you pass 0.0 for a maximumDistance.
    /// </param>
    /// <returns>closest point information on success. null on failure.</returns>
    public MeshPoint ClosestMeshPoint(Point3d testPoint, double maximumDistance)
    {
      IntPtr pConstThis = ConstPointer();
      MeshPointDataStruct ds = new MeshPointDataStruct();
      if (UnsafeNativeMethods.ON_Mesh_GetClosestPoint3(pConstThis, testPoint, ref ds, maximumDistance))
        return new MeshPoint(this, ds);
      return null;
    }

    /// <summary>
    /// Gets the point on the mesh that is closest to a given test point.
    /// </summary>
    /// <param name="testPoint">Point to seach for.</param>
    /// <param name="pointOnMesh">Point on the mesh closest to testPoint.</param>
    /// <param name="maximumDistance">
    /// Optional upper bound on the distance from test point to the mesh. 
    /// If you are only interested in finding a point Q on the mesh when 
    /// testPoint.DistanceTo(Q) &lt; maximumDistance, 
    /// then set maximumDistance to that value. 
    /// This parameter is ignored if you pass 0.0 for a maximumDistance.
    /// </param>
    /// <returns>
    /// Index of face that the closest point lies on if successful. 
    /// -1 if not successful; the value of pointOnMesh is undefined.
    /// </returns>
    public int ClosestPoint(Point3d testPoint, out Point3d pointOnMesh, double maximumDistance)
    {
      pointOnMesh = Point3d.Unset;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_GetClosestPoint(ptr, testPoint, ref pointOnMesh, maximumDistance);
    }

    /// <summary>
    /// Gets the point on the mesh that is closest to a given test point.
    /// </summary>
    /// <param name="testPoint">Point to seach for.</param>
    /// <param name="pointOnMesh">Point on the mesh closest to testPoint.</param>
    /// <param name="normalAtPoint">The normal vector of the mesh at the closest point.</param>
    /// <param name="maximumDistance">
    /// Optional upper bound on the distance from test point to the mesh. 
    /// If you are only interested in finding a point Q on the mesh when 
    /// testPoint.DistanceTo(Q) &lt; maximumDistance, 
    /// then set maximumDistance to that value. 
    /// This parameter is ignored if you pass 0.0 for a maximumDistance.
    /// </param>
    /// <returns>
    /// Index of face that the closest point lies on if successful. 
    /// -1 if not successful; the value of pointOnMesh is undefined.
    /// </returns>
    public int ClosestPoint(Point3d testPoint, out Point3d pointOnMesh, out Vector3d normalAtPoint, double maximumDistance)
    {
      pointOnMesh = Point3d.Unset;
      normalAtPoint = Vector3d.Unset;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_GetClosestPoint2(ptr, testPoint, ref pointOnMesh, ref normalAtPoint, maximumDistance);
    }

    /// <summary>
    /// Evaluate a mesh at a set of barycentric coordinates.
    /// </summary>
    /// <param name="meshPoint">MeshPoint instance contiaining a valid Face Index and Barycentric coordinates.</param>
    /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
    public Point3d PointAt(MeshPoint meshPoint)
    {
      if (meshPoint == null) { throw new ArgumentNullException("meshPoint"); }
      return PointAt(meshPoint.FaceIndex, meshPoint.T[0], meshPoint.T[1], meshPoint.T[2], meshPoint.T[3]);
    }
    /// <summary>
    /// Evaluates a mesh at a set of barycentric coordinates. Barycentric coordinates must 
    /// be assigned in accordance with the rules as defined by MeshPoint.T.
    /// </summary>
    /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
    /// <param name="t0">First barycentric coordinate.</param>
    /// <param name="t1">Second barycentric coordinate.</param>
    /// <param name="t2">Third barycentric coordinate.</param>
    /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
    /// <returns>A Point on the mesh or Point3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
    public Point3d PointAt(int faceIndex, double t0, double t1, double t2, double t3)
    {
      IntPtr pConstThis = ConstPointer();
      Point3d pt = Point3d.Unset;

      if (UnsafeNativeMethods.ON_Mesh_MeshPointAt(pConstThis, faceIndex, t0, t1, t2, t3, ref pt))
        return pt;
      return Point3d.Unset;
    }

    /// <summary>
    /// Evaluate a mesh normal at a set of barycentric coordinates.
    /// </summary>
    /// <param name="meshPoint">MeshPoint instance contiaining a valid Face Index and Barycentric coordinates.</param>
    /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
    public Vector3d NormalAt(MeshPoint meshPoint)
    {
      if (meshPoint == null) { throw new ArgumentNullException("meshPoint"); }
      return NormalAt(meshPoint.FaceIndex, meshPoint.T[0], meshPoint.T[1], meshPoint.T[2], meshPoint.T[3]);
    }
    /// <summary>
    /// Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must 
    /// be assigned in accordance with the rules as defined by MeshPoint.T.
    /// </summary>
    /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
    /// <param name="t0">First barycentric coordinate.</param>
    /// <param name="t1">Second barycentric coordinate.</param>
    /// <param name="t2">Third barycentric coordinate.</param>
    /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
    /// <returns>A Normal vector to the mesh or Vector3d.Unset if the faceIndex is not valid or if the barycentric coordinates could not be evaluated.</returns>
    public Vector3d NormalAt(int faceIndex, double t0, double t1, double t2, double t3)
    {
      IntPtr pConstThis = ConstPointer();
      Vector3d nr = Vector3d.Unset;

      if (UnsafeNativeMethods.ON_Mesh_MeshNormalAt(pConstThis, faceIndex, t0, t1, t2, t3, ref nr))
        return nr;
      return Vector3d.Unset;
    }

    /// <summary>
    /// Evaluate a mesh color at a set of barycentric coordinates.
    /// </summary>
    /// <param name="meshPoint">MeshPoint instance contiaining a valid Face Index and Barycentric coordinates.</param>
    /// <returns>The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid, 
    /// if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.</returns>
    public Color ColorAt(MeshPoint meshPoint)
    {
      if (meshPoint == null) { throw new ArgumentNullException("meshPoint"); }
      return ColorAt(meshPoint.FaceIndex, meshPoint.T[0], meshPoint.T[1], meshPoint.T[2], meshPoint.T[3]);
    }
    /// <summary>
    /// Evaluate a mesh normal at a set of barycentric coordinates. Barycentric coordinates must 
    /// be assigned in accordance with the rules as defined by MeshPoint.T.
    /// </summary>
    /// <param name="faceIndex">Index of triangle or quad to evaluate.</param>
    /// <param name="t0">First barycentric coordinate.</param>
    /// <param name="t1">Second barycentric coordinate.</param>
    /// <param name="t2">Third barycentric coordinate.</param>
    /// <param name="t3">Fourth barycentric coordinate. If the face is a triangle, this coordinate will be ignored.</param>
    /// <returns>The interpolated vertex color on the mesh or Color.Transparent if the faceIndex is not valid, 
    /// if the barycentric coordinates could not be evaluated, or if there are no colors defined on the mesh.</returns>
    public Color ColorAt(int faceIndex, double t0, double t1, double t2, double t3)
    {
      IntPtr pConstThis = ConstPointer();
      int argb = UnsafeNativeMethods.ON_Mesh_MeshColorAt(pConstThis, faceIndex, t0, t1, t2, t3);

      if (argb < 0) { return Color.Transparent; }
      return Rhino.Drawing.Color.FromArgb(argb);
    }

    /// <summary>
    /// Pulls a collection of points to a mesh.
    /// </summary>
    /// <param name="points">An array, a list or any enumerable set of points.</param>
    /// <returns>An array of points. This can be empty.</returns>
    public Point3d[] PullPointsToMesh(IEnumerable<Point3d> points)
    {
      List<Point3d> rc = new List<Point3d>();
      foreach (Point3d point in points)
      {
        Point3d closest = ClosestPoint(point);
        if (closest.IsValid)
          rc.Add(closest);
      }
      return rc.ToArray();
    }

    /// <summary>
    /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
    /// Same as Mesh.Offset(distance, false)
    /// </summary>
    /// <param name="distance">A distance value to use for offsetting.</param>
    /// <returns>A new mesh on success, or null on failure.</returns>
    public Mesh Offset(double distance)
    {
      return Offset(distance, false);
    }

    /// <summary>
    /// Makes a new mesh with vertices offset a distance in the opposite direction of the existing vertex normals.
    /// Optionally, based on the value of solidify, adds the input mesh and a ribbon of faces along any naked edges.
    /// If solidify is false it acts exactly as the Offset(distance) function.
    /// </summary>
    /// <param name="distance">A distance value.</param>
    /// <param name="solidify">true if the mesh should be solidified.</param>
    /// <returns>A new mesh on success, or null on failure.</returns>
    public Mesh Offset(double distance, bool solidify)
    {
      IntPtr pConstMesh = ConstPointer();
      IntPtr pNewMesh = UnsafeNativeMethods.RHC_RhinoMeshOffset(pConstMesh, distance, solidify);
      if (IntPtr.Zero == pNewMesh)
        return null;
      return new Mesh(pNewMesh, null);
    }
#endif
    #endregion

    internal bool IndexOpBool(int which, int index)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_IndexOpBool(pThis, which, index);
    }
    internal bool TopItemIsHidden(int which, int index)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.ON_MeshTopology_TopItemIsHidden(pConstThis, which, index);
    }

    ///// <summary>
    ///// Gets a value indicating whether or not the mesh has nurbs surface parameters.
    ///// </summary>
    //public bool HasSurfaceParameters
    //{
    //  get
    //  {
    //    IntPtr ptr = ConstPointer();
    //    return UnsafeNativeMethods.ON_Mesh_GetBool(ptr, idxHasSurfaceParameters);
    //  }
    //}

    ///// <summary>
    ///// Gets a value indicating whether or not the mesh has nurbs surface curvature data.
    ///// </summary>
    //public bool HasPrincipalCurvatures
    //{
    //  get
    //  {
    //    IntPtr ptr = ConstPointer();
    //    return UnsafeNativeMethods.ON_Mesh_GetBool(ptr, idxHasPrincipalCurvatures);
    //  }
    //}


    #region topological methods
    /// <summary>
    /// Returns an array of bool values equal in length to the number of vertices in this
    /// mesh. Each value corresponds to a mesh vertex and is set to true if the vertex is
    /// not completely surrounded by faces.
    /// </summary>
    /// <returns>An array of true/false flags that, at each index, reveals if the corresponding
    /// vertex is completely surrounded by faces.</returns>
    public bool[] GetNakedEdgePointStatus()
    {
      int count = Vertices.Count;
      if (count < 1)
        return null;

      // IMPORTANT!!! - DO NOT marshal arrays of bools. This can cause problems with
      // the marshaler because it will attempt to convert the items into U1 size
      int[] status = new int[count];
      IntPtr pThis = ConstPointer();
      if (UnsafeNativeMethods.ON_Mesh_NakedEdgePoints(pThis, status, count))
      {
        bool[] rc = new bool[count];
        for (int i = 0; i < count; i++)
        {
          if (status[i] != 0)
            rc[i] = true;
        }
        return rc;
      }
      return null;
    }

    //David: I have disabled these. It seems very, very geeky.
    //public bool TransposeSurfaceParameters()
    //{
    //  IntPtr ptr = NonConstPointer();
    //  return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, idxTransposeSurfaceParameters);
    //}

    //public bool ReverseSurfaceParameters(int direction)
    //{
    //  IntPtr ptr = NonConstPointer();
    //  return UnsafeNativeMethods.ON_Mesh_Reverse(ptr, false, direction);
    //}

    ///// <summary>
    ///// finds all coincident vertices and merges them if break angle is small enough
    ///// </summary>
    ///// <param name="tolerance">coordinate tols for considering vertices to be coincident.</param>
    ///// <param name="cosineNormalAngle">
    ///// cosine normal angle tolerance in radians
    ///// if vertices are coincident, then they are combined
    ///// if NormalA o NormalB >= this value
    ///// </param>
    ///// <returns>-</returns>
    //public bool CombineCoincidentVertices(Vector3f tolerance, double cosineNormalAngle)
    //{
    //  IntPtr ptr = NonConstPointer();
    //  return UnsafeNativeMethods.ON_Mesh_CombineCoincidentVertices(ptr, tolerance, cosineNormalAngle);
    //}

    #endregion

    /// <summary>
    /// In ancient times (or modern smartphone times), some rendering engines
    /// were only able to process small batches of triangles and the
    /// CreatePartitions() function was provided to partition the mesh into
    /// subsets of vertices and faces that those rendering engines could handle.
    /// </summary>
    /// <param name="maximumVertexCount"></param>
    /// <param name="maximumTriangleCount"></param>
    /// <returns>true on success</returns>
    public bool CreatePartitions(int maximumVertexCount, int maximumTriangleCount)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_CreatePartition(pThis, maximumVertexCount, maximumTriangleCount);
    }

    /// <summary>
    /// Number of partition information chunks stored on this mesh based
    /// on the last call to CreatePartitions
    /// </summary>
    public int PartitionCount
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_PartitionCount(pConstThis);
      }
    }

    /// <summary>
    /// Retrieves a partition. See <see cref="CreatePartitions"/> for details.
    /// </summary>
    /// <param name="which">The partition index.</param>
    /// <returns></returns>
    public MeshPart GetPartition(int which)
    {
      IntPtr pConstThis = NonConstPointer();
      int vi0=0, vi1=0, fi0=0, fi1=0;
      int vert_count = 0;
      int tri_count = 0;
      if (UnsafeNativeMethods.ON_Mesh_GetMeshPart(pConstThis, which, ref vi0, ref vi1, ref fi0, ref fi1, ref vert_count, ref tri_count))
        return new MeshPart(vi0, vi1, fi0, fi1, vert_count, tri_count);
      return null;
    }


    //[skipping]
    //  bool SetTextureCoordinates( 
    //  bool HasCachedTextureCoordinates() const;
    //  const ON_TextureCoordinates* CachedTextureCoordinates( 
    //  const ON_TextureCoordinates* SetCachedTextureCoordinates( 
    //  bool EvaluateMeshGeometry( const ON_Surface& ); // evaluate surface at tcoords
    //  int GetVertexEdges( 
    //  int GetMeshEdges( 

    //[skipping]
    // bool SwapEdge( int topei );
    //  void DestroyHiddenVertexArray();
    //  const bool* HiddenVertexArray() const;
    //  void SetVertexHiddenFlag( int meshvi, bool bHidden );
    //  bool VertexIsHidden( int meshvi ) const;
    //  bool FaceIsHidden( int meshvi ) const;
    //  const ON_MeshTopology& Topology() const;
    //  void DestroyTopology();
    //  const ON_MeshPartition* Partition() const;
    //  void DestroyPartition();
    //  const class ON_MeshNgonList* NgonList() const;
    //  class ON_MeshNgonList* ModifyNgonList();
    //  void DestroyNgonList();


    // [skipping]
    //  ON_3fVectorArray m_N;
    //  ON_3fVectorArray m_FN;
    //  ON_MappingTag m_Ttag; // OPTIONAL tag for values in m_T[]
    //  ON_2fPointArray m_T;  // OPTIONAL texture coordinates for each vertex
    //  ON_2dPointArray m_S;
    //  ON_Interval m_srf_domain[2]; // surface evaluation domain.
    //  double m_srf_scale[2];
    //  ON_Interval m_packed_tex_domain[2];
    //  bool m_packed_tex_rotate;
    //  bool HasPackedTextureRegion() const;
    //  ON_SimpleArray<ON_SurfaceCurvature> m_K;  // OPTIONAL surface curvatures
    //  ON_MappingTag m_Ctag; // OPTIONAL tag for values in m_C[]
    //  ON_SimpleArray<bool> m_H; // OPTIONAL vertex visibility.
    //  int m_hidden_count;       // number of vertices that are hidden
    //  const ON_Object* m_parent; // runtime parent geometry (use ...::Cast() to get it)

#if RHINO_SDK
    /// <summary>
    /// Constructs contour curves for a mesh, sectioned along a linear axis.
    /// </summary>
    /// <param name="meshToContour">A mesh to contour.</param>
    /// <param name="contourStart">A start point of the contouring axis.</param>
    /// <param name="contourEnd">An end point of the contouring axis.</param>
    /// <param name="interval">An interval distance.</param>
    /// <returns>An array of curves. This array can be empty.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_makerhinocontours.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_makerhinocontours.cs' lang='cs'/>
    /// <code source='examples\py\ex_makerhinocontours.py' lang='py'/>
    /// </example>
    public static Curve[] CreateContourCurves(Mesh meshToContour, Point3d contourStart, Point3d contourEnd, double interval)
    {
      IntPtr pConstMesh = meshToContour.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArrayCurvePointer outputcurves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        IntPtr pCurves = outputcurves.NonConstPointer();
        int count = UnsafeNativeMethods.RHC_MakeRhinoContours2(pConstMesh, contourStart, contourEnd, interval, pCurves, tolerance);
        return 0 == count ? new Curve[0] : outputcurves.ToNonConstArray();
      }
    }

    /// <summary>
    /// Constructs contour curves for a mesh, sectioned at a plane.
    /// </summary>
    /// <param name="meshToContour">A mesh to contour.</param>
    /// <param name="sectionPlane">A cutting plane.</param>
    /// <returns>An array of curves. This array can be empty.</returns>
    public static Curve[] CreateContourCurves(Mesh meshToContour, Plane sectionPlane)
    {
      IntPtr pConstMesh = meshToContour.ConstPointer();
      using (Runtime.InteropWrappers.SimpleArrayCurvePointer outputcurves = new Rhino.Runtime.InteropWrappers.SimpleArrayCurvePointer())
      {
        double tolerance = RhinoDoc.ActiveDoc.ModelAbsoluteTolerance;
        IntPtr pCurves = outputcurves.NonConstPointer();
        int count = UnsafeNativeMethods.RHC_MakeRhinoContours3(pConstMesh, ref sectionPlane, pCurves, tolerance);
        return 0 == count ? new Curve[0] : outputcurves.ToNonConstArray();
      }
    }
#endif
  }
}

namespace Rhino.Geometry.Collections
{
  /// <summary>
  /// Provides access to the vertices and vertex-related functionality of a mesh.
  /// </summary>
  public class MeshVertexList : IEnumerable<Point3f>, Rhino.Collections.IRhinoTable<Point3f>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshVertexList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh vertices.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxVertexCount);
      }
      set
      {
        if (value >= 0 && value != Count)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxVertexCount, value);
          UnsafeNativeMethods.ON_Mesh_RepairHiddenArray(pMesh);
        }
      }
    }

    /// <summary>
    /// Gets or sets the vertex at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of control vertex to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The control vertex at [index].</returns>
    public Point3f this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        Point3f rc = new Point3f();
        IntPtr ptr = m_mesh.ConstPointer();
        UnsafeNativeMethods.ON_Mesh_Vertex(ptr, index, ref rc);
        return rc;
      }
      set
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetVertex(ptr, index, value.m_x, value.m_y, value.m_z);
      }
    }
    #endregion

    #region access
    /// <summary>
    /// Clear the Vertex list on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearVertices);
      UnsafeNativeMethods.ON_Mesh_RepairHiddenArray(ptr);
    }

    /// <summary>
    /// Adds a new vertex to the end of the Vertex list.
    /// </summary>
    /// <param name="x">X component of new vertex coordinate.</param>
    /// <param name="y">Y component of new vertex coordinate.</param>
    /// <param name="z">Z component of new vertex coordinate.</param>
    /// <returns>The index of the newly added vertex.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public int Add(float x, float y, float z)
    {
      int count = Count;
      SetVertex(count, x, y, z);

      UnsafeNativeMethods.ON_Mesh_RepairHiddenArray(m_mesh.NonConstPointer());

      return count;
    }
    /// <summary>
    /// Adds a new vertex to the end of the Vertex list.
    /// </summary>
    /// <param name="x">X component of new vertex coordinate.</param>
    /// <param name="y">Y component of new vertex coordinate.</param>
    /// <param name="z">Z component of new vertex coordinate.</param>
    /// <returns>The index of the newly added vertex.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public int Add(double x, double y, double z)
    {
      return Add((float)x, (float)y, (float)z);
    }
    /// <summary>
    /// Adds a new vertex to the end of the Vertex list.
    /// </summary>
    /// <param name="vertex">Location of new vertex.</param>
    /// <returns>The index of the newly added vertex.</returns>
    public int Add(Point3f vertex)
    {
      return Add(vertex.m_x, vertex.m_y, vertex.m_z);
    }
    /// <summary>
    /// Adds a new vertex to the end of the Vertex list.
    /// </summary>
    /// <param name="vertex">Location of new vertex.</param>
    /// <returns>The index of the newly added vertex.</returns>
    public int Add(Point3d vertex)
    {
      return Add((float)vertex.X, (float)vertex.Y, (float)vertex.Z);
    }

    /// <summary>
    /// Adds a series of new vertices to the end of the vertex list.
    /// <para>This overload accepts double-precision points.</para>
    /// </summary>
    /// <param name="vertices">A list, an array or any enumerable set of <see cref="Point3d"/>.</param>
    public void AddVertices(IEnumerable<Point3d> vertices)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      int count = Count;
      int index = count;
      foreach (Point3d vertex in vertices)
      {
        float x = (float)vertex.X;
        float y = (float)vertex.Y;
        float z = (float)vertex.Z;
        UnsafeNativeMethods.ON_Mesh_SetVertex(pMesh, index, x, y, z);
        index++;
      }
      UnsafeNativeMethods.ON_Mesh_RepairHiddenArray(pMesh);
    }

    /// <summary>
    /// Adds a series of new vertices to the end of the vertex list.
    /// <para>This overload accepts single-precision points.</para>
    /// </summary>
    /// <param name="vertices">A list, an array or any enumerable set of <see cref="Point3f"/>.</param>
    public void AddVertices(IEnumerable<Point3f> vertices)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      int count = Count;
      int index = count;
      foreach (Point3f vertex in vertices)
      {
        float x = vertex.X;
        float y = vertex.Y;
        float z = vertex.Z;
        UnsafeNativeMethods.ON_Mesh_SetVertex(pMesh, index, x, y, z);
        index++;
      }
      UnsafeNativeMethods.ON_Mesh_RepairHiddenArray(pMesh);
    }

    /// <summary>
    /// Sets or adds a vertex to the Vertex List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex to set.</param>
    /// <param name="x">X component of vertex location.</param>
    /// <param name="y">Y component of vertex location.</param>
    /// <param name="z">Z component of vertex location.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetVertex(int index, float x, float y, float z)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      bool rc = UnsafeNativeMethods.ON_Mesh_SetVertex(ptr, index, x, y, z);
      return rc;
    }
    /// <summary>
    /// Sets or adds a vertex to the Vertex List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex to set.</param>
    /// <param name="x">X component of vertex location.</param>
    /// <param name="y">Y component of vertex location.</param>
    /// <param name="z">Z component of vertex location.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetVertex(int index, double x, double y, double z)
    {
      return SetVertex(index, (float)x, (float)y, (float)z);
    }
    /// <summary>
    /// Sets or adds a vertex to the Vertex List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex to set.</param>
    /// <param name="vertex">Vertex location.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetVertex(int index, Point3f vertex)
    {
      return SetVertex(index, vertex.X, vertex.Y, vertex.Z);
    }
    /// <summary>
    /// Sets or adds a vertex to the Vertex List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex to set.</param>
    /// <param name="vertex">Vertex location.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetVertex(int index, Point3d vertex)
    {
      return SetVertex(index, (float)vertex.X, (float)vertex.Y, (float)vertex.Z);
    }

    /// <summary>
    /// Gets a value indicating whether or not a vertex is hidden.
    /// </summary>
    /// <param name="vertexIndex">Index of vertex to query.</param>
    /// <returns>true if the vertex is hidden, false if it is not.</returns>
    public bool IsHidden(int vertexIndex)
    {
      return UnsafeNativeMethods.ON_Mesh_GetHiddenValue(m_mesh.ConstPointer(), vertexIndex);
    }

    /// <summary>
    /// Hides the vertex at the given index.
    /// </summary>
    /// <param name="vertexIndex">Index of vertex to hide.</param>
    public void Hide(int vertexIndex)
    {
      // If vertex is already hidden, DO NOT copy the mesh but return right away.
      if (UnsafeNativeMethods.ON_Mesh_GetHiddenValue(m_mesh.ConstPointer(), vertexIndex))
      { return; }

      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, 0, Mesh.idxEnsureHiddenList);
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, vertexIndex, Mesh.idxHideVertex);
    }
    /// <summary>
    /// Shows the vertex at the given index.
    /// </summary>
    /// <param name="vertexIndex">Index of vertex to show.</param>
    public void Show(int vertexIndex)
    {
      // If vertex is already visible, DO NOT copy the mesh but return right away.
      if (!UnsafeNativeMethods.ON_Mesh_GetHiddenValue(m_mesh.ConstPointer(), vertexIndex))
      { return; }

      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, 0, Mesh.idxEnsureHiddenList);
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, vertexIndex, Mesh.idxShowVertex);
    }
    /// <summary>
    /// Hides all vertices in the mesh.
    /// </summary>
    public void HideAll()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, 0, Mesh.idxEnsureHiddenList);
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, 0, Mesh.idxHideAll);
    }
    /// <summary>
    /// Shows all vertices in the mesh.
    /// </summary>
    public void ShowAll()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_HiddenVertexOp(ptr, 0, Mesh.idxShowAll);
    }
    #endregion

    #region methods
    /// <summary>
    /// Removes all vertices that are currently not used by the Face list.
    /// </summary>
    /// <returns>The number of unused vertices that were removed.</returns>
    public int CullUnused()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_CullOp(ptr, false);
    }

    /// <summary>
    /// Merges identical vertices.
    /// </summary>
    /// <param name="ignoreNormals">
    /// If true, vertex normals will not be taken into consideration when comparing vertices.
    /// </param>
    /// <param name="ignoreAdditional">
    /// If true, texture coordinates, colors, and principal curvatures 
    /// will not be taken into consideration when comparing vertices.
    /// </param>
    /// <returns>
    /// true if the mesh is changed, in which case the mesh will have fewer vertices than before.
    /// </returns>
    public bool CombineIdentical(bool ignoreNormals, bool ignoreAdditional)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_CombineIdenticalVertices(ptr, ignoreNormals, ignoreAdditional);
    }

    /// <summary>
    /// Gets a list of all of the faces that share a given vertex.
    /// </summary>
    /// <param name="vertexIndex">The index of a vertex in the mesh.</param>
    /// <returns>An array of indices of faces on success, null on failure.</returns>
    public int[] GetVertexFaces(int vertexIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      Rhino.Runtime.InteropWrappers.SimpleArrayInt face_ids = new Rhino.Runtime.InteropWrappers.SimpleArrayInt();
      int count = UnsafeNativeMethods.ON_Mesh_GetVertexFaces(pConstMesh, face_ids.m_ptr, vertexIndex);
      int[] ids = null;
      if (count > 0)
        ids = face_ids.ToArray();
      face_ids.Dispose();
      return ids;
    }

    /// <summary>
    /// Gets a list of other vertices which are "topologically" identical
    /// to this vertex.
    /// </summary>
    /// <param name="vertexIndex">A vertex index in the mesh.</param>
    /// <returns>
    /// Array of indices of vertices that are topoligically the same as this vertex. The
    /// array includes vertexIndex. Returns null on failure.
    /// </returns>
    public int[] GetTopologicalIndenticalVertices(int vertexIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int[] ids = null;
      using (Rhino.Runtime.InteropWrappers.SimpleArrayInt vertex_ids = new Rhino.Runtime.InteropWrappers.SimpleArrayInt())
      {
        int count = UnsafeNativeMethods.ON_Mesh_GetTopologicalVertices(pConstMesh, vertex_ids.m_ptr, vertexIndex);
        if (count > 0)
          ids = vertex_ids.ToArray();
      }
      return ids;
    }

    /// <summary>
    /// Gets indices of all vertices that form "edges" with a given vertex index.
    /// </summary>
    /// <param name="vertexIndex">The index of a vertex to query.</param>
    /// <returns>An array of vertex indices that are connected with the specified vertex.</returns>
    public int[] GetConnectedVertices(int vertexIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int[] ids = null;
      using (Rhino.Runtime.InteropWrappers.SimpleArrayInt vertex_ids = new Rhino.Runtime.InteropWrappers.SimpleArrayInt())
      {
        int count = UnsafeNativeMethods.ON_Mesh_GetConnectedVertices(pConstMesh, vertex_ids.m_ptr, vertexIndex);
        if (count > 0)
          ids = vertex_ids.ToArray();
      }
      return ids;
    }

    /// <summary>
    /// Copies all vertices to a new array of <see cref="Point3f"/>.
    /// </summary>
    /// <returns>A new array.</returns>
    public Point3f[] ToPoint3fArray()
    {
      int count = Count;
      Point3f[] rc = new Point3f[count];
      IntPtr pConstMesh = m_mesh.ConstPointer();
      for (int i = 0; i < count; i++)
      {
        Point3f pt = new Point3f();
        UnsafeNativeMethods.ON_Mesh_Vertex(pConstMesh, i, ref pt);
        rc[i] = pt;
      }
      return rc;
    }

    /// <summary>
    /// Copies all vertices to a new array of <see cref="Point3d"/>.
    /// </summary>
    /// <returns>A new array.</returns>
    public Point3d[] ToPoint3dArray()
    {
      int count = Count;
      Point3d[] rc = new Point3d[count];
      IntPtr pConstMesh = m_mesh.ConstPointer();
      for (int i = 0; i < count; i++)
      {
        Point3f pt = new Point3f();
        UnsafeNativeMethods.ON_Mesh_Vertex(pConstMesh, i, ref pt);
        rc[i] = new Point3d(pt);
      }
      return rc;
    }

    /// <summary>
    /// Copies all vertices to a linear array of float in x,y,z order
    /// </summary>
    /// <returns>The float array.</returns>
    public float[] ToFloatArray()
    {
      int count = Count;
      float[] rc = new float[count * 3];
      IntPtr const_ptr_mesh = m_mesh.ConstPointer();
      Point3f pt = new Point3f();
      int index = 0;
      // There is a much more efficient way to do this with
      // marshalling the whole array at once, but this will
      // do for now
      for (int i = 0; i < count; i++)
      {
        UnsafeNativeMethods.ON_Mesh_Vertex(const_ptr_mesh, i, ref pt);
        rc[index++] = pt.X;
        rc[index++] = pt.Y;
        rc[index++] = pt.Z;
      }
      return rc;
    }

    /// <summary>
    /// Removes the vertex at the given index and all faces that reference that index.
    /// </summary>
    /// <param name="index">Index of vertex to remove.</param>
    /// <param name="shrinkFaces">If true, quads that reference the deleted vertex will be converted to triangles.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Remove(int index, bool shrinkFaces)
    {
      return Remove(new int[] { index }, shrinkFaces);
    }
    /// <summary>
    /// Removes the vertices at the given indices and all faces that reference those vertices.
    /// </summary>
    /// <param name="indices">Vertex indices to remove.</param>
    /// <param name="shrinkFaces">If true, quads that reference the deleted vertex will be converted to triangles.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool Remove(IEnumerable<int> indices, bool shrinkFaces)
    {
      if (indices == null) { throw new ArgumentNullException("indices"); }
      List<int> idx = new List<int>(indices);
      if (idx.Count == 0) { return true; }

      int max = Count;
      foreach (int index in idx)
      {
        if (index < 0) { throw new IndexOutOfRangeException("Vertex index must be larger than or equal to zero"); }
        if (index >= max) { throw new IndexOutOfRangeException("Vertex index must be smaller than the size of the collection"); }
      }

      MeshFaceList faces = m_mesh.Faces;
      List<int> faceidx = new List<int>();

      for (int i = 0; i < faces.Count; i++)
      {
        MeshFace face = faces[i];
        int k = -1;
        int N = 0;

        if (idx.Contains(face.A)) { k = 0; N++; }
        if (idx.Contains(face.B)) { k = 1; N++; }
        if (N >= 2) { faceidx.Add(i); continue; }
        if (idx.Contains(face.C)) { k = 2; N++; }
        if (N >= 2) { faceidx.Add(i); continue; }
        if (face.IsQuad && idx.Contains(face.D)) { k = 3; N++; }
        if (N >= 2) { faceidx.Add(i); continue; }

        // Do not change face.
        if (N == 0) { continue; }

        // Always remove triangles.
        if (face.IsTriangle) { faceidx.Add(i); continue; }

        // Remove quads when shrinking is not allowed.
        if (face.IsQuad && !shrinkFaces) { faceidx.Add(i); continue; }

        // Convert quad to triangle.
        switch (k)
        {
          case 0:
            face.A = face.B;
            face.B = face.C;
            face.C = face.D;
            break;

          case 1:
            face.B = face.C;
            face.C = face.D;
            break;

          case 2:
            face.C = face.D;
            break;

          case 3:
            face.D = face.C;
            break;
        }
        faces.SetFace(i, face);
      }

      if (faceidx.Count > 0)
      {
        faces.DeleteFaces(faceidx);
      }

      CullUnused();
      return true;
    }
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all mesh vertices (points) in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Point3f> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshVertexList, Point3f>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }

  /// <summary>
  /// Provides access to the mesh topology vertices of a mesh. Topology vertices are
  /// sets of vertices in the MeshVertexList that can topologically be considered the
  /// same vertex.
  /// </summary>
  public class MeshTopologyVertexList : IEnumerable<Point3f>, Rhino.Collections.IRhinoTable<Point3f>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshTopologyVertexList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh topology vertices.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxMeshTopologyVertexCount);
      }
    }

    /// <summary>
    /// Gets or sets the vertex at the given index. Setting a location adjusts all vertices
    /// in the mesh's vertex list that are defined by this topological vertex
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of topology vertex to access.</param>
    /// <returns>The topological vertex at [index].</returns>
    public Point3f this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        Point3f rc = new Point3f();
        IntPtr ptr = m_mesh.ConstPointer();
        UnsafeNativeMethods.ON_Mesh_TopologyVertex(ptr, index, ref rc);
        return rc;
      }
      set
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetTopologyVertex(ptr, index, value);
      }
    }
    #endregion

    #region methods
    /// <summary>
    /// Gets the topology vertex index for an existing mesh vertex in the mesh's
    /// VertexList.
    /// </summary>
    /// <param name="vertexIndex">Index of a vertex in the Mesh.Vertices.</param>
    /// <returns>Index of a topology vertex in the Mesh.TopologyVertices.</returns>
    public int TopologyVertexIndex(int vertexIndex)
    {
      IntPtr ptr = m_mesh.ConstPointer();
      int rc = UnsafeNativeMethods.ON_Mesh_TopologyVertexIndex(ptr, vertexIndex);
      if (-1 == rc)
        throw new IndexOutOfRangeException();
      return rc;
    }

    /// <summary>
    /// Gets all indices of the mesh vertices that a given topology vertex represents.
    /// </summary>
    /// <param name="topologyVertexIndex">Index of a topology vertex in Mesh.TopologyVertices to query.</param>
    /// <returns>
    /// Indices of all vertices that in Mesh.Vertices that a topology vertex represents.
    /// </returns>
    public int[] MeshVertexIndices(int topologyVertexIndex)
    {
      IntPtr ptr = m_mesh.ConstPointer();
      int count = UnsafeNativeMethods.ON_MeshTopologyVertex_Count(ptr, topologyVertexIndex, true);
      if (-1 == count)
        throw new IndexOutOfRangeException();
      if (count < 1)
        return null;
      int[] rc = new int[count];
      UnsafeNativeMethods.ON_MeshTopologyVertex_GetIndices(ptr, topologyVertexIndex, count, rc);
      return rc;
    }

    /// <summary>
    /// Returns TopologyVertexIndices for a given mesh face index.
    /// </summary>
    /// <param name="faceIndex">The index of a face to query.</param>
    /// <returns>An array of vertex indices.</returns>
    public int[] IndicesFromFace(int faceIndex)
    {
      int[] rc;
      int a = 0, b = 0, c = 0, d = 0;
      IntPtr pConstMesh = m_mesh.ConstPointer();
      if (UnsafeNativeMethods.ON_MeshTopology_GetTopFaceVertices(pConstMesh, faceIndex, ref a, ref b, ref c, ref d))
      {
        rc = c == d ? new int[] { a, b, c } : new int[] { a, b, c, d };
      }
      else
        rc = new int[0];
      return rc;
    }

    /// <summary>
    /// Gets all topological vertices that are connected to a given vertex.
    /// </summary>
    /// <param name="topologyVertexIndex">index of a topology vertex in Mesh.TopologyVertices.</param>
    /// <returns>
    /// Indices of all topological vertices that are connected to this topological vertex.
    /// null if no vertices are connected to this vertex.
    /// </returns>
    public int[] ConnectedTopologyVertices(int topologyVertexIndex)
    {
      IntPtr ptr = m_mesh.ConstPointer();
      int count = UnsafeNativeMethods.ON_MeshTopologyVertex_Count(ptr, topologyVertexIndex, false);
      if (-1 == count)
        throw new IndexOutOfRangeException();
      if (count < 1)
        return null;
      int[] rc = new int[count];
      UnsafeNativeMethods.ON_MeshTopologyVertex_ConnectedVertices(ptr, topologyVertexIndex, count, rc);
      return rc;
    }

    /// <summary>
    /// Gets all topological vertices that are connected to a given vertex.
    /// </summary>
    /// <param name="topologyVertexIndex">index of a topology vertex in Mesh.TopologyVertices.</param>
    /// <param name="sorted">if true, thr vertices are returned in a radially sorted order.</param>
    /// <returns>
    /// Indices of all topological vertices that are connected to this topological vertex.
    /// null if no vertices are connected to this vertex.
    /// </returns>
    public int[] ConnectedTopologyVertices(int topologyVertexIndex, bool sorted)
    {
      if (sorted)
        SortEdges(topologyVertexIndex);
      return ConnectedTopologyVertices(topologyVertexIndex);
    }

    /// <summary>
    /// Sorts the edge list for the mesh topology vertex list so that
    /// the edges are in radial order when you call ConnectedTopologyVertices.
    /// A nonmanifold edge is treated as a boundary edge with respect
    /// to sorting.  If any boundary or nonmanifold edges end at the
    /// vertex, then the first edge will be a boundary or nonmanifold edge.
    /// </summary>
    /// <returns>true on success.</returns>
    public bool SortEdges()
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      return UnsafeNativeMethods.ON_MeshTopologyVertex_SortEdges(pConstMesh, -1);
    }

    /// <summary>
    /// Sorts the edge list for as single mesh topology vertex so that
    /// the edges are in radial order when you call ConnectedTopologyVertices.
    /// A nonmanifold edge is treated as a boundary edge with respect
    /// to sorting.  If any boundary or nonmanifold edges end at the
    /// vertex, then the first edge will be a boundary or nonmanifold edge.
    /// </summary>
    /// <param name="topologyVertexIndex">index of a topology vertex in Mesh.TopologyVertices></param>
    /// <returns>true on success.</returns>
    public bool SortEdges(int topologyVertexIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      return UnsafeNativeMethods.ON_MeshTopologyVertex_SortEdges(pConstMesh, topologyVertexIndex);
    }

    /// <summary>
    /// Returns true if the topological vertex is hidden. The mesh topology
    /// vertex is hidden if and only if all the ON_Mesh vertices it represents is hidden.
    /// </summary>
    /// <param name="topologyVertexIndex">index of a topology vertex in Mesh.TopologyVertices.</param>
    /// <returns>true if mesh topology vertex is hidden.</returns>
    public bool IsHidden(int topologyVertexIndex)
    {
      return m_mesh.TopItemIsHidden(Mesh.idxTopVertexIsHidden, topologyVertexIndex);
    }

    /// <summary>
    /// Gets all faces that are connected to a given vertex.
    /// </summary>
    /// <param name="topologyVertexIndex">Index of a topology vertex in Mesh.TopologyVertices.</param>
    /// <returns>
    /// Indices of all faces in Mesh.Faces that are connected to this topological vertex.
    /// null if no faces are connected to this vertex.
    /// </returns>
    public int[] ConnectedFaces(int topologyVertexIndex)
    {
      IntPtr ptr = m_mesh.ConstPointer();
      if (topologyVertexIndex < 0 || topologyVertexIndex >= Count)
        throw new IndexOutOfRangeException();
      using (Runtime.InteropWrappers.SimpleArrayInt arr = new Rhino.Runtime.InteropWrappers.SimpleArrayInt())
      {
        UnsafeNativeMethods.ON_MeshTopologyVertex_ConnectedFaces(ptr, topologyVertexIndex, arr.m_ptr);
        return arr.ToArray();
      }
    }
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all topology vertices in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Point3f> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshTopologyVertexList, Point3f>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion

  }

  /// <summary>
  /// Represents an entry point to the list of edges in a mesh topology.
  /// </summary>
  public class MeshTopologyEdgeList
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshTopologyEdgeList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets the amount of edges in this list.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxMeshTopologyEdgeCount);
      }
    }
    #endregion

    /// <summary>Gets the two topology vertices for a given topology edge.</summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge.</param>
    /// <returns>The pair of vertex indices the edge connects.</returns>
    public IndexPair GetTopologyVertices(int topologyEdgeIndex)
    {
      int i = -1, j = -1;
      IntPtr pConstMesh = m_mesh.ConstPointer();
      UnsafeNativeMethods.ON_MeshTopologyEdge_TopVi(pConstMesh, topologyEdgeIndex, ref i, ref j);
      return new IndexPair(i, j);
    }

    /// <summary>
    /// Gets indices of faces connected to an edge.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge that is queried.</param>
    /// <returns>An array of face indices the edge borders. This might be empty on error.</returns>
    public int[] GetConnectedFaces(int topologyEdgeIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int count = UnsafeNativeMethods.ON_MeshTopologyEdge_TopfCount(pConstMesh, topologyEdgeIndex);
      if (count <= 0)
        return new int[0];
      int[] rc = new int[count];
      UnsafeNativeMethods.ON_MeshTopologyEdge_TopfList(pConstMesh, topologyEdgeIndex, count, rc);
      return rc;
    }

    /// <summary>
    /// Gets indices of faces connected to an edge.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge that is queried.</param>
    /// <param name="faceOrientationMatchesEdgeDirection">An array of Boolean values that explains whether each face direction matches the direction of the specified edge.</param>
    /// <returns>An array of face indices the edge borders. This might be empty on error.</returns>
    public int[] GetConnectedFaces(int topologyEdgeIndex, out bool[] faceOrientationMatchesEdgeDirection)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int count = UnsafeNativeMethods.ON_MeshTopologyEdge_TopfCount(pConstMesh, topologyEdgeIndex);
      if (count <= 0)
      {
        faceOrientationMatchesEdgeDirection = new bool[0];
        return new int[0];
      }
      int[] rc = new int[count];
      faceOrientationMatchesEdgeDirection = new bool[count];
      UnsafeNativeMethods.ON_MeshTopologyEdge_TopfList2(pConstMesh, topologyEdgeIndex, count, rc, faceOrientationMatchesEdgeDirection);
      return rc;
    }

    /// <summary>
    /// Gets indices of edges that surround a given face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>A new array of indices to the topological edges that are connected with the specified face.</returns>
    public int[] GetEdgesForFace(int faceIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int a = 0, b = 0, c = 0, d = 0;
      UnsafeNativeMethods.ON_MeshTopologyFace_Edges(pConstMesh, faceIndex, ref a, ref b, ref c, ref d);

      if (a < 0 || b < 0 || c < 0 || d < 0)
      {
        if (faceIndex < 0 || faceIndex >= m_mesh.Faces.Count)
          throw new IndexOutOfRangeException();
        return new int[0];
      }

      if (c == d)
        return new int[] { a, b, c };
      return new int[] { a, b, c, d };
    }

    /// <summary>
    /// Gets indices of edges that surround a given face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <param name="sameOrientation">
    /// Same length as returned edge index array. For each edge, the sameOrientation value
    /// tells you if the edge orientation matches the face orientation (true), or is
    /// reversed (false) compared to it.
    /// </param>
    /// <returns>A new array of indices to the topological edges that are connected with the specified face.</returns>
    public int[] GetEdgesForFace(int faceIndex, out bool[] sameOrientation)
    {
      sameOrientation = new bool[0];
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int a = 0, b = 0, c = 0, d = 0;
      int[] orientation = new int[4];
      if (!UnsafeNativeMethods.ON_MeshTopologyFace_Edges2(pConstMesh, faceIndex, ref a, ref b, ref c, ref d, orientation))
      {
        if (faceIndex < 0 || faceIndex >= m_mesh.Faces.Count)
          throw new IndexOutOfRangeException();
        return new int[0];
      }

      if (c == d)
      {
        sameOrientation = new bool[] { orientation[0] == 1, orientation[1] == 1, orientation[2] == 1 };
        return new int[] { a, b, c };
      }
      sameOrientation = new bool[] { orientation[0] == 1, orientation[1] == 1, orientation[2] == 1, orientation[3] == 1 };
      return new int[] { a, b, c, d };
    }

    /// <summary>
    /// Returns index of edge that connects topological vertices. 
    /// returns -1 if no edge is found.
    /// </summary>
    /// <param name="topologyVertex1">The first topology vertex index.</param>
    /// <param name="topologyVertex2">The second topology vertex index.</param>
    /// <returns>The edge index.</returns>
    public int GetEdgeIndex(int topologyVertex1, int topologyVertex2)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      return UnsafeNativeMethods.ON_MeshTopology_TopEdge(pConstMesh, topologyVertex1, topologyVertex2);
    }

    /// <summary>Gets the 3d line along an edge.</summary>
    /// <param name="topologyEdgeIndex">The topology edge index.</param>
    /// <returns>
    /// Line along edge. If input is not valid, an Invalid Line is returned.
    /// </returns>
    public Line EdgeLine(int topologyEdgeIndex)
    {
      Line rc = new Line();
      IntPtr pConstMesh = m_mesh.ConstPointer();
      UnsafeNativeMethods.ON_MeshTopology_TopEdgeLine(pConstMesh, topologyEdgeIndex, ref rc);
      return rc;
    }

    /// <summary>
    /// Replaces a mesh edge with a vertex at its center and update adjacent faces as needed.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge in <see cref="Mesh.TopologyEdges"/>.</param>
    /// <returns>true if successful.</returns>
    public bool CollapseEdge(int topologyEdgeIndex)
    {
      return m_mesh.IndexOpBool(Mesh.idxCollapseEdge, topologyEdgeIndex);
    }

#if RHINO_SDK
    /// <summary>
    /// Divides a mesh edge to create two or more triangles
    /// </summary>
    /// <param name="topologyEdgeIndex">Edge to divide</param>
    /// <param name="t">
    /// Parameter along edge. This is the same as getting an EdgeLine and calling PointAt(t) on that line
    /// </param>
    /// <returns>true if successful</returns>
    public bool SplitEdge(int topologyEdgeIndex, double t)
    {
      Line l = EdgeLine(topologyEdgeIndex);
      if (!l.IsValid)
        return false;
      Point3d point = l.PointAt(t);
      return SplitEdge(topologyEdgeIndex, point);
    }

    /// <summary>
    /// Divides a mesh edge to create two or more triangles
    /// </summary>
    /// <param name="topologyEdgeIndex">Edge to divide</param>
    /// <param name="point">
    /// Location to perform the split
    /// </param>
    /// <returns>true if successful</returns>
    public bool SplitEdge(int topologyEdgeIndex, Point3d point)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SplitMeshEdge(pMesh, topologyEdgeIndex, point);
    }
#endif

    /// <summary>
    /// Determines if a mesh edge index is valid input for <see cref="SwapEdge"/>.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge in <see cref="Mesh.TopologyEdges"/>.</param>
    /// <returns>true if edge can be swapped.</returns>
    public bool IsSwappableEdge(int topologyEdgeIndex)
    {
      return m_mesh.IndexOpBool(Mesh.idxIsSwappableEdge, topologyEdgeIndex);
    }

    /// <summary>
    /// If the edge is shared by two triangular face, then the edge is swapped.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge in <see cref="Mesh.TopologyEdges"/>.</param>
    /// <returns>true if successful.</returns>
    public bool SwapEdge(int topologyEdgeIndex)
    {
      return m_mesh.IndexOpBool(Mesh.idxSwapEdge, topologyEdgeIndex);
    }

    /// <summary>
    /// Returns true if the topological edge is hidden. The mesh topology
    /// edge is hidden only if either of its mesh topology vertices is hidden.
    /// </summary>
    /// <param name="topologyEdgeIndex">An index of a topology edge in <see cref="Mesh.TopologyEdges"/>.</param>
    /// <returns>true if mesh topology edge is hidden.</returns>
    public bool IsHidden(int topologyEdgeIndex)
    {
      return m_mesh.TopItemIsHidden(Mesh.idxTopEdgeIsHidden, topologyEdgeIndex);
    }

  }

  /// <summary>
  /// Provides access to the Vertex Normals of a Mesh.
  /// </summary>
  public class MeshVertexNormalList : IEnumerable<Vector3f>, Rhino.Collections.IRhinoTable<Vector3f>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshVertexNormalList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh vertex normals.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxNormalCount);
      }
      set
      {
        if (value >= 0 && Count != value)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxNormalCount, value);
        }
      }
    }

    /// <summary>
    /// Gets or sets the vertex at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of control vertex to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The control vertex at [index].</returns>
    public Vector3f this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        Vector3f rc = new Vector3f();
        IntPtr ptr = m_mesh.ConstPointer();
        UnsafeNativeMethods.ON_Mesh_GetNormal(ptr, index, ref rc, false);
        return rc;
      }
      set
      {
        if (index < 0 || index >= Count)
        {
          throw new IndexOutOfRangeException();
        }

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetNormal(ptr, index, value, false);
      }
    }
    #endregion

    #region access
    /// <summary>
    /// Clears the vertex normal collection on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearNormals);
    }

    private bool SetNormalsHelper(Vector3f[] normals, bool append)
    {
      if (null == normals || normals.Length < 1)
        return false;
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetNormals(ptr, normals.Length, normals, append);
    }

    /// <summary>
    /// Adds a new vertex normal at the end of the list.
    /// </summary>
    /// <param name="x">X component of new vertex normal.</param>
    /// <param name="y">Y component of new vertex normal.</param>
    /// <param name="z">Z component of new vertex normal.</param>
    /// <returns>The index of the newly added vertex normal.</returns>
    public int Add(float x, float y, float z)
    {
      return Add(new Vector3f(x, y, z));
    }
    /// <summary>
    /// Adds a new vertex normal at the end of the list.
    /// </summary>
    /// <param name="x">X component of new vertex normal.</param>
    /// <param name="y">Y component of new vertex normal.</param>
    /// <param name="z">Z component of new vertex normal.</param>
    /// <returns>The index of the newly added vertex normal.</returns>
    public int Add(double x, double y, double z)
    {
      return Add(new Vector3f((float)x, (float)y, (float)z));
    }
    /// <summary>
    /// Adds a new vertex normal at the end of the list.
    /// </summary>
    /// <param name="normal">new vertex normal.</param>
    /// <returns>The index of the newly added vertex normal.</returns>
    public int Add(Vector3f normal)
    {
      int N = Count;
      if (!SetNormal(N, normal)) { return -1; }
      return N;
    }
    /// <summary>
    /// Adds a new vertex normal at the end of the list.
    /// </summary>
    /// <param name="normal">new vertex normal.</param>
    /// <returns>The index of the newly added vertex normal.</returns>
    public int Add(Vector3d normal)
    {
      return Add(new Vector3f((float)normal.X, (float)normal.Y, (float)normal.Z));
    }
    /// <summary>
    /// Appends a collection of normal vectors.
    /// </summary>
    /// <param name="normals">Normals to append.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool AddRange(Vector3f[] normals)
    {
      return SetNormalsHelper(normals, true);
    }

    /// <summary>
    /// Sets or adds a normal to the list.
    /// <para>If [index] is less than [Count], the existing vertex normal at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex normal is appended to the end of the list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex normal to set.</param>
    /// <param name="x">X component of vertex normal.</param>
    /// <param name="y">Y component of vertex normal.</param>
    /// <param name="z">Z component of vertex normal.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetNormal(int index, float x, float y, float z)
    {
      return SetNormal(index, new Vector3f(x, y, z));
    }
    /// <summary>
    /// Sets or adds a vertex normal to the list.
    /// <para>If [index] is less than [Count], the existing vertex normal at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex normal is appended to the end of the list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex normal to set.</param>
    /// <param name="x">X component of vertex normal.</param>
    /// <param name="y">Y component of vertex normal.</param>
    /// <param name="z">Z component of vertex normal.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetNormal(int index, double x, double y, double z)
    {
      return SetNormal(index, new Vector3f((float)x, (float)y, (float)z));
    }
    /// <summary>
    /// Sets or adds a vertex normal to the list.
    /// <para>If [index] is less than [Count], the existing vertex normal at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex normal is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex normal to set.</param>
    /// <param name="normal">The new normal at the index.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetNormal(int index, Vector3f normal)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetNormal(ptr, index, normal, false);
    }
    /// <summary>
    /// Sets or adds a vertex normal to the list.
    /// <para>If [index] is less than [Count], the existing vertex normal at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex normal is appended to the end of the list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex normal to set.</param>
    /// <param name="normal">The new normal at the index.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetNormal(int index, Vector3d normal)
    {
      return SetNormal(index, new Vector3f((float)normal.m_x, (float)normal.m_y, (float)normal.m_z));
    }
    /// <summary>
    /// Sets all normal vectors in one go. This method destroys the current normal array if it exists.
    /// </summary>
    /// <param name="normals">Normals for the entire mesh.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetNormals(Vector3f[] normals)
    {
      return SetNormalsHelper(normals, false);
    }
    #endregion

    #region methods
    /// <summary>
    /// Computes the vertex normals based on the physical shape of the mesh.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public bool ComputeNormals()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxComputeVertexNormals);
    }

    /// <summary>
    /// Unitizes all vertex normals.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool UnitizeNormals()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxUnitizeVertexNormals);
    }

    /// <summary>
    /// Reverses direction of all vertex normals
    /// <para>This is the same as Mesh.Flip(true, false, false)</para>
    /// </summary>
    public void Flip()
    {
      m_mesh.Flip(true, false, false);
    }
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all normals (vectors) in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Vector3f> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshVertexNormalList, Vector3f>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }

  /// <summary>
  /// Provides access to the faces and Face related functionality of a Mesh.
  /// </summary>
  public class MeshFaceList : IEnumerable<MeshFace>, Rhino.Collections.IRhinoTable<MeshFace>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshFaceList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh faces.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxFaceCount);
      }
      set
      {
        if (value >= 0 && Count != value)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxFaceCount, value);
        }
      }
    }

    /// <summary>
    /// Gets the number of faces that are quads (4 corners).
    /// </summary>
    public int QuadCount
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxQuadCount);
      }
    }

    /// <summary>
    /// Gets the number of faces that are triangles (3 corners).
    /// </summary>
    public int TriangleCount
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxTriangleCount);
      }
    }

    // 7 Mar 2010 S. Baer - skipping indexing operator for now. I'm a little concerned that this
    // would cause code that looks like
    // int v0 = mesh.Faces[0].A;
    // int v1 = mesh.Faces[0].B;
    // int v2 = mesh.Faces[0].C;
    // int v3 = mesh.Faces[0].D;
    // The above code would always be 4 times as slow as a single call to get all 4 indices at once
    //public MeshFace this[int index]
    #endregion

    #region methods
    #region face access
    /// <summary>
    /// Clears the Face list on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearFaces);
    }

    /// <summary>
    /// Appends a new mesh face to the end of the mesh face list.
    /// </summary>
    /// <param name="face">Face to add.</param>
    /// <returns>The index of the newly added face.</returns>
    public int AddFace(MeshFace face)
    {
      return AddFace(face.m_a, face.m_b, face.m_c, face.m_d);
    }
    /// <summary>
    /// Appends a new triangular face to the end of the mesh face list.
    /// </summary>
    /// <param name="vertex1">Index of first face corner.</param>
    /// <param name="vertex2">Index of second face corner.</param>
    /// <param name="vertex3">Index of third face corner.</param>
    /// <returns>The index of the newly added triangle.</returns>
    public int AddFace(int vertex1, int vertex2, int vertex3)
    {
      return AddFace(vertex1, vertex2, vertex3, vertex3);
    }
    /// <summary>
    /// Appends a new quadragular face to the end of the mesh face list.
    /// </summary>
    /// <param name="vertex1">Index of first face corner.</param>
    /// <param name="vertex2">Index of second face corner.</param>
    /// <param name="vertex3">Index of third face corner.</param>
    /// <param name="vertex4">Index of fourth face corner.</param>
    /// <returns>The index of the newly added quad.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addmesh.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addmesh.cs' lang='cs'/>
    /// <code source='examples\py\ex_addmesh.py' lang='py'/>
    /// </example>
    public int AddFace(int vertex1, int vertex2, int vertex3, int vertex4)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_AddFace(ptr, vertex1, vertex2, vertex3, vertex4);
    }

    /// <summary>
    /// Appends a list of faces to the end of the mesh face list.
    /// </summary>
    /// <param name="faces">Faces to add.</param>
    /// <returns>Indices of the newly created faces</returns>
    public int[] AddFaces(IEnumerable<MeshFace> faces)
    {
      List<int> rc = new List<int>();
      foreach(MeshFace face in faces)
      {
        int index = AddFace(face);
        rc.Add(index);
      }
      return rc.ToArray();
    }

    /// <summary>
    /// Inserts a mesh face at a defined index in this list.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="face">A face.</param>
    /// <exception cref="ArgumentOutOfRangeException">If index is negative or &gt;= Count.</exception>
    public void Insert(int index, MeshFace face)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      bool rc = UnsafeNativeMethods.ON_Mesh_InsertFace(pMesh, index, face.A, face.B, face.C, face.D);
      if (!rc && (index < 0 || index >= Count))
        throw new ArgumentOutOfRangeException("index");
    }

    /// <summary>
    /// Sets a face at a specific index of the mesh.
    /// </summary>
    /// <param name="index">A position in the list.</param>
    /// <param name="face">A face.</param>
    /// <returns>true if the operation succeeded, otherwise false.</returns>
    public bool SetFace(int index, MeshFace face)
    {
      return SetFace(index, face.m_a, face.m_b, face.m_c, face.m_d);
    }
    /// <summary>
    /// Sets a triangular face at a specific index of the mesh.
    /// </summary>
    /// <param name="index">A position in the list.</param>
    /// <param name="vertex1">The first vertex index.</param>
    /// <param name="vertex2">The second vertex index.</param>
    /// <param name="vertex3">The third vertex index.</param>
    /// <returns>true if the operation succeeded, otherwise false.</returns>
    public bool SetFace(int index, int vertex1, int vertex2, int vertex3)
    {
      return SetFace(index, vertex1, vertex2, vertex3, vertex3);
    }

    /// <summary>
    /// Sets a quadrangular face at a specific index of the mesh.
    /// </summary>
    /// <param name="index">A position in the list.</param>
    /// <param name="vertex1">The first vertex index.</param>
    /// <param name="vertex2">The second vertex index.</param>
    /// <param name="vertex3">The third vertex index.</param>
    /// <param name="vertex4">The fourth vertex index.</param>
    /// <returns>true if the operation succeeded, otherwise false.</returns>
    public bool SetFace(int index, int vertex1, int vertex2, int vertex3, int vertex4)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetFace(ptr, index, vertex1, vertex2, vertex3, vertex4);
    }

    /// <summary>
    /// Returns the mesh face at the given index. 
    /// </summary>
    /// <param name="index">Index of face to get. Must be larger than or equal to zero and 
    /// smaller than the Face Count of the mesh.</param>
    /// <returns>The mesh face at the given index on success or MeshFace.Unset if the index is out of range.</returns>
    public MeshFace GetFace(int index)
    {
      MeshFace rc = new MeshFace();
      IntPtr pMesh = m_mesh.ConstPointer();
      if (UnsafeNativeMethods.ON_Mesh_GetFace(pMesh, index, ref rc))
        return rc;

      return MeshFace.Unset;
    }


    /// <summary>
    /// Returns the mesh face at the given index. 
    /// </summary>
    /// <param name="index">Index of face to get. Must be larger than or equal to zero and 
    /// smaller than the Face Count of the mesh.</param>
    public MeshFace this[int index]
    {
      get
      {
        MeshFace face = GetFace(index);
        if (face.A == int.MinValue && (index < 0 || index >= Count))
          throw new IndexOutOfRangeException();
        return face;
      }
    }

    /// <summary>
    /// Gets the 3D location of the vertices forming a face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <param name="a">A first point. This out argument is assigned during the call.</param>
    /// <param name="b">A second point. This out argument is assigned during the call.</param>
    /// <param name="c">A third point. This out argument is assigned during the call.</param>
    /// <param name="d">A fourth point. This out argument is assigned during the call.</param>
    /// <returns>true if the operation succeeded, otherwise false.</returns>
    public bool GetFaceVertices(int faceIndex, out Point3f a, out Point3f b, out Point3f c, out Point3f d)
    {
      IntPtr pMesh = m_mesh.ConstPointer();
      a = new Point3f();
      b = new Point3f();
      c = new Point3f();
      d = new Point3f();
      bool rc = UnsafeNativeMethods.ON_Mesh_GetFaceVertices(pMesh, faceIndex, ref a, ref b, ref c, ref d);
      return rc;
    }

    /// <summary>
    /// Gets the bounding box of a face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>A new bounding box, or <see cref="BoundingBox.Empty"/> on error.</returns>
    public BoundingBox GetFaceBoundingBox(int faceIndex)
    {
      Point3f a, b, c, d;
      if (!GetFaceVertices(faceIndex, out a, out b, out c, out d))
        return BoundingBox.Empty;
      BoundingBox rc = BoundingBox.Empty;
      rc.Union(a);
      rc.Union(b);
      rc.Union(c);
      rc.Union(d);
      return rc;
    }

    /// <summary>
    /// Gets the center point of a face.
    /// <para>For a triangular face, this is considered the centroid or barycenter.</para>
    /// <para>For a quad, this is considered the bimedians intersection
    /// (the avarage of four points).</para>
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>The center point.</returns>
    public Point3d GetFaceCenter(int faceIndex)
    {
      IntPtr pConstThis = m_mesh.ConstPointer();
      Point3d rc = new Point3d();
      if (!UnsafeNativeMethods.ON_Mesh_GetFaceCenter(pConstThis, faceIndex, ref rc))
        throw new IndexOutOfRangeException();
      return rc;
    }

    /// <summary>
    /// Gets all faces that share a topological edge with a given face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>All indices that share an edge.</returns>
    public int[] AdjacentFaces(int faceIndex)
    {
      int[] edges = m_mesh.TopologyEdges.GetEdgesForFace(faceIndex);
      if (null == edges || edges.Length < 1)
        return new int[0];

      Dictionary<int, int> face_ids = new Dictionary<int, int>();
      for (int i = 0; i < edges.Length; i++)
      {
        int edgeIndex = edges[i];
        int[] faces = m_mesh.TopologyEdges.GetConnectedFaces(edgeIndex);
        if (faces == null)
          continue;
        for (int j = 0; j < faces.Length; j++)
        {
          int face_id = faces[j];
          if (face_id != faceIndex)
            face_ids[face_id] = face_id;
        }
      }

      int[] rc = new int[face_ids.Count];
      face_ids.Keys.CopyTo(rc, 0);
      return rc;
    }


    /// <summary>
    /// Copies all of the faces to a linear integer of indices
    /// </summary>
    /// <returns>The int array.</returns>
    /// <param name="asTriangles">If set to <c>true</c> as triangles.</param>
    public int[] ToIntArray(bool asTriangles)
    {
      int count = asTriangles ? (QuadCount * 2 + TriangleCount) * 3 : Count * 4;
      int[] rc = new int[count];
      int current = 0;
      int face_count = Count;
      MeshFace face = new MeshFace();
      IntPtr const_ptr_mesh = m_mesh.ConstPointer();
      for (int index = 0; index < face_count; index++)
      {
        UnsafeNativeMethods.ON_Mesh_GetFace(const_ptr_mesh, index, ref face);
        rc[current++] = face.A;
        rc[current++] = face.B;
        rc[current++] = face.C;
        if (asTriangles)
        {
          if (face.C != face.D)
          {
            rc[current++] = face.C;
            rc[current++] = face.D;
            rc[current++] = face.A;
          }
        }
        else
        {
          rc[current++] = face.D;
        }
      }
      return rc;
    }

    #endregion

    /// <summary>
    /// Removes a collection of faces from the mesh without affecting the remaining geometry.
    /// </summary>
    /// <param name="faceIndexes">An array containing all the face indices to be removed.</param>
    /// <returns>The number of faces deleted on success.</returns>
    public int DeleteFaces(IEnumerable<int> faceIndexes)
    {
      if (null == faceIndexes)
        return 0;
      RhinoList<int> _faceIndexes = new RhinoList<int>(faceIndexes);

      if (_faceIndexes.Count < 1)
        return 0;
      int[] f = _faceIndexes.m_items;
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_DeleteFace(ptr, _faceIndexes.Count, f);
    }

    /// <summary>
    /// Removes a face from the mesh.
    /// </summary>
    /// <param name="index">The index of the face that will be removed.</param>
    /// <exception cref="ArgumentOutOfRangeException">If index is &lt; 0 or &gt;= Count.</exception>
    public void RemoveAt(int index)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      int[] indices = { index };
      int count = UnsafeNativeMethods.ON_Mesh_DeleteFace(pMesh, 1, indices);
      if (count != 1 && (index < 0 || index >= Count))
        throw new ArgumentOutOfRangeException("index");
    }

    /// <summary>Splits all quads along the short diagonal.</summary>
    /// <returns>true on success, false on failure.</returns>
    public bool ConvertQuadsToTriangles()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxConvertQuadsToTriangles);
    }

    /// <summary>
    /// Joins adjacent triangles into quads if the resulting quad is 'nice'.
    /// </summary>
    /// <param name="angleToleranceRadians">
    /// Used to compare adjacent triangles' face normals. For two triangles 
    /// to be considered, the angle between their face normals has to 
    /// be &lt;= angleToleranceRadians. When in doubt use RhinoMath.PI/90.0 (2 degrees).
    /// </param>
    /// <param name="minimumDiagonalLengthRatio">
    /// ( &lt;= 1.0) For two triangles to be considered the ratio of the 
    /// resulting quad's diagonals 
    /// (length of the shortest diagonal)/(length of longest diagonal). 
    /// has to be >= minimumDiagonalLengthRatio. When in doubt us .875.
    /// </param>
    /// <returns>true on success, false on failure.</returns>
    public bool ConvertTrianglesToQuads(double angleToleranceRadians, double minimumDiagonalLengthRatio)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_ConvertTrianglesToQuads(ptr, angleToleranceRadians, minimumDiagonalLengthRatio);
    }

    /// <summary>
    /// Attempts to removes degenerate faces from the mesh.
    /// <para>Degenerate faces are faces that contains such a combination of indices,
    /// that their final shape collapsed in a line or point.</para>
    /// <para>Before returning, this method also attempts to repair faces by juggling
    /// vertex indices.</para>
    /// </summary>
    /// <returns>The number of degenerate faces that were removed.</returns>
    public int CullDegenerateFaces()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_CullOp(ptr, true);
    }

    /// <summary>
    /// Gets a value indicating whether a face is hidden.
    /// <para>A face is hidden if, and only if, at least one of its vertices is hidden.</para>
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>true if hidden, false if fully visible.</returns>
    public bool IsHidden(int faceIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_FaceIsHidden(pConstMesh, faceIndex);
    }

    /// <summary>
    /// Returns true if at least one of the face edges are not topologically
    /// connected to any other faces.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>true if that face makes the mesh open, otherwise false.</returns>
    public bool HasNakedEdges(int faceIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_FaceHasNakedEdges(pConstMesh, faceIndex);
    }

    /// <summary>
    /// Gets the topology vertex indices of a face.
    /// </summary>
    /// <param name="faceIndex">A face index.</param>
    /// <returns>An array of integers.</returns>
    public int[] GetTopologicalVertices(int faceIndex)
    {
      IntPtr pConstMesh = m_mesh.ConstPointer();
      int[] v = new int[4];
      if (!UnsafeNativeMethods.ON_Mesh_FaceTopologicalVertices(pConstMesh, faceIndex, v))
        return new int[0];
      return v;
    }

#if RHINO_SDK
    /// <summary>
    /// Find all connected face indices where adjacent face normals meet
    /// the criteria of angleRadians and greaterThanAngle
    /// </summary>
    /// <param name="faceIndex">face index to start from</param>
    /// <param name="angleRadians">angle to use for comparison of what is connected</param>
    /// <param name="greaterThanAngle">
    /// If true angles greater than or equal to are considered connected.
    /// If false, angles less than or equal to are considerd connected.</param>
    /// <returns>list of connected face indices</returns>
    public int[] GetConnectedFaces(int faceIndex, double angleRadians, bool greaterThanAngle)
    {
      IntPtr ptr_const_mesh = m_mesh.ConstPointer();
      using (var indices = new Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr ptr_simplearray_int = indices.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoMakeConnectedMeshFaceList(ptr_const_mesh, faceIndex, angleRadians, greaterThanAngle, ptr_simplearray_int);
        return indices.ToArray();
      }
    }

    /// <summary>
    /// Uses startFaceIndex and finds all connected face indexes up to unwelded
    /// or naked edges. If treatNonmanifoldLikeUnwelded is true then non-manifold
    /// edges will be considered as unwelded or naked
    /// </summary>
    /// <param name="startFaceIndex">Initial face index</param>
    /// <param name="treatNonmanifoldLikeUnwelded">
    /// True means non-manifold edges will be handled like unwelded edges, 
    /// False means they aren't considered
    /// </param>
    /// <returns>Array of connected face indexes</returns>
    public int[] GetConnectedFacesToEdges(int startFaceIndex, bool treatNonmanifoldLikeUnwelded)
    {
      IntPtr ptr_const_mesh = m_mesh.ConstPointer();
      using (var indices = new Runtime.InteropWrappers.SimpleArrayInt())
      {
        IntPtr ptr_simplearray_int = indices.NonConstPointer();
        UnsafeNativeMethods.RHC_RhinoMakeMeshPartFaceList(ptr_const_mesh, startFaceIndex, treatNonmanifoldLikeUnwelded, ptr_simplearray_int);
        return indices.ToArray();
      }
    }
#endif
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all faces in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<MeshFace> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshFaceList, MeshFace>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }

  /// <summary>
  /// Provides access to the Face normals of a Mesh.
  /// </summary>
  public class MeshFaceNormalList : IEnumerable<Vector3f>, Rhino.Collections.IRhinoTable<Vector3f>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshFaceNormalList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh face normals.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxFaceNormalCount);
      }
      set
      {
        if (value >= 0 && Count != value)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxFaceNormalCount, value);
        }
      }
    }

    /// <summary>
    /// Gets or sets the face normal at the given face index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of face normal to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The face normal at [index].</returns>
    public Vector3f this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();

        Vector3f rc = new Vector3f();
        IntPtr ptr = m_mesh.ConstPointer();
        UnsafeNativeMethods.ON_Mesh_GetNormal(ptr, index, ref rc, true);
        return rc;
      }
      set
      {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetNormal(ptr, index, value, true);
      }
    }
    #endregion

    #region methods
    #region face access
    /// <summary>
    /// Clears the Face Normal list on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearFaceNormals);
    }

    /// <summary>
    /// Appends a face normal to the list of mesh face normals.
    /// </summary>
    /// <param name="x">X component of face normal.</param>
    /// <param name="y">Y component of face normal.</param>
    /// <param name="z">Z component of face normal.</param>
    /// <returns>The index of the newly added face normal.</returns>
    public int AddFaceNormal(float x, float y, float z)
    {
      return AddFaceNormal(new Vector3f(x, y, z));
    }
    /// <summary>
    /// Appends a face normal to the list of mesh face normals.
    /// </summary>
    /// <param name="x">X component of face normal.</param>
    /// <param name="y">Y component of face normal.</param>
    /// <param name="z">Z component of face normal.</param>
    /// <returns>The index of the newly added face normal.</returns>
    public int AddFaceNormal(double x, double y, double z)
    {
      return AddFaceNormal(new Vector3f((float)x, (float)y, (float)z));
    }
    /// <summary>
    /// Appends a face normal to the list of mesh face normals.
    /// </summary>
    /// <param name="normal">New face normal.</param>
    /// <returns>The index of the newly added face normal.</returns>
    public int AddFaceNormal(Vector3d normal)
    {
      return AddFaceNormal(new Vector3f((float)normal.m_x, (float)normal.m_y, (float)normal.m_z));
    }
    /// <summary>
    /// Appends a face normal to the list of mesh face normals.
    /// </summary>
    /// <param name="normal">New face normal.</param>
    /// <returns>The index of the newly added face normal.</returns>
    public int AddFaceNormal(Vector3f normal)
    {
      SetFaceNormal(Count, normal);
      return Count - 1;
    }

    /// <summary>
    /// Sets a face normal vector at an index using three single-precision numbers.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="x">A x component.</param>
    /// <param name="y">A y component.</param>
    /// <param name="z">A z component.</param>
    /// <returns>true on success; false on error.</returns>
    public bool SetFaceNormal(int index, float x, float y, float z)
    {
      return SetFaceNormal(index, new Vector3f(x, y, z));
    }

    /// <summary>
    /// Sets a face normal vector at an index using three double-precision numbers.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="x">A x component.</param>
    /// <param name="y">A y component.</param>
    /// <param name="z">A z component.</param>
    /// <returns>true on success; false on error.</returns>
    public bool SetFaceNormal(int index, double x, double y, double z)
    {
      return SetFaceNormal(index, new Vector3f((float)x, (float)y, (float)z));
    }

    /// <summary>
    /// Sets a face normal vector at an index using a single-precision vector.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="normal">A normal vector.</param>
    /// <returns>true on success; false on error.</returns>
    public bool SetFaceNormal(int index, Vector3d normal)
    {
      return SetFaceNormal(index, new Vector3f((float)normal.m_x, (float)normal.m_y, (float)normal.m_z));
    }

    /// <summary>
    /// Sets a face normal vector at an index using a single-precision vector.
    /// </summary>
    /// <param name="index">An index.</param>
    /// <param name="normal">A normal vector.</param>
    /// <returns>true on success; false on error.</returns>
    public bool SetFaceNormal(int index, Vector3f normal)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetNormal(ptr, index, normal, true);
    }
    #endregion

    /// <summary>
    /// Unitizes all the existing face normals.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool UnitizeFaceNormals()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxUnitizeFaceNormals);
    }

    /// <summary>
    /// Computes all the face normals for this mesh based on the physical shape of the mesh.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool ComputeFaceNormals()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxComputeFaceNormals);
    }
    #endregion


    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all normals in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Vector3f> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshFaceNormalList, Vector3f>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }

  /// <summary>
  /// Provides access to the vertex colors of a mesh object.
  /// </summary>
  public class MeshVertexColorList : IEnumerable<Color>, Rhino.Collections.IRhinoTable<Color>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshVertexColorList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of mesh colors.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxColorCount);
      }
      set
      {
        if (value >= 0 && Count != value)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxColorCount, value);
        }
      }
    }

    /// <summary>
    /// Gets or sets the vertex color at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of vertex control to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The vertex color at [index].</returns>
    public Color this[int index]
    {
      get
      {
        int argb = 0;
        IntPtr ptr = m_mesh.ConstPointer();
        // get color will return false when the index is out of range
        if (!UnsafeNativeMethods.ON_Mesh_GetColor(ptr, index, ref argb))
          throw new IndexOutOfRangeException();
        return Color.FromArgb(argb);
      }
      set
      {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetColor(ptr, index, value.ToArgb());
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_analysismode.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_analysismode.cs' lang='cs'/>
    /// </example>
    /// <summary>
    /// Gets or sets a mapping information for the mesh associated with these vertex colors.
    /// </summary>
    public Rhino.Render.MappingTag Tag
    {
      get
      {
        IntPtr pConstThis = m_mesh.ConstPointer();
        Guid id = Guid.Empty;
        int mapping_type = 0;
        uint crc = 0;
        Transform xf = new Transform();
        UnsafeNativeMethods.ON_Mesh_GetMappingTag(pConstThis, 0, ref id, ref mapping_type, ref crc, ref xf);
        Rhino.Render.MappingTag mt = new Render.MappingTag();
        mt.Id = id;
        mt.MappingCRC = crc;
        mt.MappingType = (Render.TextureMappingType)mapping_type;
        mt.MeshTransform = xf;
        return mt;
      }
      set
      {
        IntPtr pThis = m_mesh.NonConstPointer();
        Transform xf = value.MeshTransform;
        UnsafeNativeMethods.ON_Mesh_SetMappingTag(pThis, 0, value.Id, (int)value.MappingType, value.MappingCRC, ref xf);
      }
    }
    #endregion

    #region access
    /// <summary>
    /// Clears the vertex color list on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearColors);
    }

    /// <summary>
    /// Adds a new vertex color to the end of the color list.
    /// </summary>
    /// <param name="red">Red component of color, must be in the 0~255 range.</param>
    /// <param name="green">Green component of color, must be in the 0~255 range.</param>
    /// <param name="blue">Blue component of color, must be in the 0~255 range.</param>
    /// <returns>The index of the newly added color.</returns>
    public int Add(int red, int green, int blue)
    {
      SetColor(Count, red, green, blue);
      return Count - 1;
    }
    /// <summary>
    /// Adds a new vertex color to the end of the color list.
    /// </summary>
    /// <param name="color">Color to append, Alpha channels will be ignored.</param>
    /// <returns>The index of the newly added color.</returns>
    public int Add(Color color)
    {
      SetColor(Count, color);
      return Count - 1;
    }

    /// <summary>
    /// Sets or adds a vertex color to the color List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex color to set. 
    /// If index equals Count, then the color will be appended.</param>
    /// <param name="red">Red component of vertex color. Value must be in the 0~255 range.</param>
    /// <param name="green">Green component of vertex color. Value must be in the 0~255 range.</param>
    /// <param name="blue">Blue component of vertex color. Value must be in the 0~255 range.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetColor(int index, int red, int green, int blue)
    {
      return SetColor(index, Color.FromArgb(red, green, blue));
    }
    /// <summary>
    /// Sets or adds a vertex to the Vertex List.
    /// <para>If [index] is less than [Count], the existing vertex at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new vertex is appended to the end of the vertex list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of vertex color to set. 
    /// If index equals Count, then the color will be appended.</param>
    /// <param name="color">Color to set, Alpha channels will be ignored.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetColor(int index, Color color)
    {
      if (index < 0 || index > Count)
      {
        throw new IndexOutOfRangeException();
      }

      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetColor(ptr, index, color.ToArgb());
    }

    /// <summary>
    /// Sets a color at the three or four vertex indices of a specified face.
    /// </summary>
    /// <param name="face">A face to use to retrieve indices.</param>
    /// <param name="color">A color.</param>
    /// <returns>true on success; false on error.</returns>
    public bool SetColor(MeshFace face, Color color)
    {
      return SetColor(face.A, color) &&
        SetColor(face.B, color) &&
        SetColor(face.C, color) &&
        SetColor(face.D, color);
    }
    #endregion

    #region methods
    private bool SetColorsHelper(Color[] colors, bool append)
    {
      if (colors == null) { return false; }

      IntPtr pThis = m_mesh.NonConstPointer();

      int count = colors.Length;
      int[] argb = new int[count];

      for (int i = 0; i < count; i++)
      { argb[i] = colors[i].ToArgb(); }

      return UnsafeNativeMethods.ON_Mesh_SetVertexColors(pThis, count, argb, append);
    }

    /// <summary>
    /// Constructs a valid vertex color list consisting of a single color.
    /// </summary>
    /// <param name="baseColor">Color to apply to every vertex.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool CreateMonotoneMesh(Color baseColor)
    {
      int count = m_mesh.Vertices.Count;
      Color[] colors = new Color[count];

      for (int i = 0; i < count; i++)
      { colors[i] = baseColor; }

      return SetColors(colors);
    }

    /// <summary>
    /// Sets all the vertex colors in one go. For the Mesh to be valid, the number 
    /// of colors must match the number of vertices.
    /// </summary>
    /// <param name="colors">Colors to set.</param>
    /// <returns>true on success, false on failure.</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_analysismode.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_analysismode.cs' lang='cs'/>
    /// </example>
    public bool SetColors(Color[] colors)
    {
      return SetColorsHelper(colors, false);
    }

    /// <summary>
    /// Appends a collection of colors to the vertex color list. 
    /// For the Mesh to be valid, the number of colors must match the number of vertices.
    /// </summary>
    /// <param name="colors">Colors to append.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool AppendColors(Color[] colors)
    {
      return SetColorsHelper(colors, true);
    }
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all colors in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Color> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshVertexColorList, Color>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }

  /// <summary>
  /// Provides access to the Vertex Texture coordinates of a Mesh.
  /// </summary>
  public class MeshTextureCoordinateList : IEnumerable<Point2f>, Rhino.Collections.IRhinoTable<Point2f>
  {
    private readonly Mesh m_mesh;

    #region constructors
    internal MeshTextureCoordinateList(Mesh ownerMesh)
    {
      m_mesh = ownerMesh;
    }
    #endregion

    #region properties
    /// <summary>
    /// Gets or sets the number of texture coordinates.
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr ptr = m_mesh.ConstPointer();
        return UnsafeNativeMethods.ON_Mesh_GetInt(ptr, Mesh.idxTextureCoordinateCount);
      }
      set
      {
        if (value >= 0 && Count != value)
        {
          IntPtr pMesh = m_mesh.NonConstPointer();
          UnsafeNativeMethods.ON_Mesh_SetInt(pMesh, Mesh.idxTextureCoordinateCount, value);
        }
      }
    }

    /// <summary>
    /// Gets or sets the texture coordinate at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of texture coordinates to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The texture coordinate at [index].</returns>
    public Point2f this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();

        IntPtr ptr = m_mesh.ConstPointer();
        float s = 0;
        float t = 0;
        if (!UnsafeNativeMethods.ON_Mesh_GetTextureCoordinate(ptr, index, ref s, ref t)) { return Point2f.Unset; }
        return new Point2f(s, t);
      }
      set
      {
        if (index < 0 || index >= Count)
          throw new IndexOutOfRangeException();

        IntPtr ptr = m_mesh.NonConstPointer();
        UnsafeNativeMethods.ON_Mesh_SetTextureCoordinate(ptr, index, value.m_x, value.m_y);
      }
    }
    #endregion

    #region access
    /// <summary>
    /// Clears the Texture Coordinate list on the mesh.
    /// </summary>
    public void Clear()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      UnsafeNativeMethods.ON_Mesh_ClearList(ptr, Mesh.idxClearTextureCoordinates);
    }

    /// <summary>
    /// Adds a new texture coordinate to the end of the Texture list.
    /// </summary>
    /// <param name="s">S component of new texture coordinate.</param>
    /// <param name="t">T component of new texture coordinate.</param>
    /// <returns>The index of the newly added texture coordinate.</returns>
    public int Add(float s, float t)
    {
      int N = Count;
      if (!SetTextureCoordinate(N, new Point2f(s, t))) { return -1; }
      return N;
    }
    /// <summary>
    /// Adds a new texture coordinate to the end of the Texture list.
    /// </summary>
    /// <param name="s">S component of new texture coordinate.</param>
    /// <param name="t">T component of new texture coordinate.</param>
    /// <returns>The index of the newly added texture coordinate.</returns>
    public int Add(double s, double t)
    {
      return Add((float)s, (float)t);
    }
    /// <summary>
    /// Adds a new texture coordinate to the end of the Texture list.
    /// </summary>
    /// <param name="tc">Texture coordinate to add.</param>
    /// <returns>The index of the newly added texture coordinate.</returns>
    public int Add(Point2f tc)
    {
      return Add(tc.m_x, tc.m_y);
    }
    /// <summary>
    /// Adds a new texture coordinate to the end of the Texture list.
    /// </summary>
    /// <param name="tc">Texture coordinate to add.</param>
    /// <returns>The index of the newly added texture coordinate.</returns>
    public int Add(Point3d tc)
    {
      return Add((float)tc.m_x, (float)tc.m_y);
    }
    /// <summary>
    /// Appends an array of texture coordinates.
    /// </summary>
    /// <param name="textureCoordinates">Texture coordinates to append.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool AddRange(Point2f[] textureCoordinates)
    {
      return SetTextureCoordinatesHelper(textureCoordinates, true);
    }

    /// <summary>
    /// Sets or adds a texture coordinate to the Texture Coordinate List.
    /// <para>If [index] is less than [Count], the existing coordinate at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new coordinate is appended to the end of the coordinate list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of texture coordinate to set.</param>
    /// <param name="s">S component of texture coordinate.</param>
    /// <param name="t">T component of texture coordinate.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinate(int index, float s, float t)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetTextureCoordinate(ptr, index, s, t);
    }
    /// <summary>
    /// Sets or adds a texture coordinate to the Texture Coordinate List.
    /// <para>If [index] is less than [Count], the existing coordinate at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new coordinate is appended to the end of the coordinate list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of texture coordinate to set.</param>
    /// <param name="s">S component of texture coordinate.</param>
    /// <param name="t">T component of texture coordinate.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinate(int index, double s, double t)
    {
      return SetTextureCoordinate(index, (float)s, (float)t);
    }
    /// <summary>
    /// Sets or adds a texture coordinate to the Texture Coordinate List.
    /// <para>If [index] is less than [Count], the existing coordinate at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new coordinate is appended to the end of the coordinate list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of texture coordinate to set.</param>
    /// <param name="tc">Texture coordinate point.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinate(int index, Point2f tc)
    {
      return SetTextureCoordinate(index, tc.m_x, tc.m_y);
    }
    /// <summary>
    /// Sets or adds a texture coordinate to the Texture Coordinate List.
    /// <para>If [index] is less than [Count], the existing coordinate at [index] will be modified.</para>
    /// <para>If [index] equals [Count], a new coordinate is appended to the end of the coordinate list.</para> 
    /// <para>If [index] is larger than [Count], the function will return false.</para>
    /// </summary>
    /// <param name="index">Index of texture coordinate to set.</param>
    /// <param name="tc">Texture coordinate point.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinate(int index, Point3f tc)
    {
      return SetTextureCoordinate(index, tc.m_x, tc.m_y);
    }
    /// <summary>
    /// Sets all texture coordinates in one go.
    /// </summary>
    /// <param name="textureCoordinates">Texture coordinates to assign to the mesh.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinates(Point2f[] textureCoordinates)
    {
      return SetTextureCoordinatesHelper(textureCoordinates, false);
    }

    private bool SetTextureCoordinatesHelper(Point2f[] textureCoordinates, bool append)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      if (textureCoordinates == null)
        return false;
      return UnsafeNativeMethods.ON_Mesh_SetTextureCoordinates(ptr, textureCoordinates.Length, ref textureCoordinates[0], append);
    }

    /// <summary>
    /// Set all texture coordinates based on a texture mapping function
    /// </summary>
    /// <param name="mapping">The new mapping type.</param>
    /// <returns>true on success, false on failure.</returns>
    public bool SetTextureCoordinates(Rhino.Render.TextureMapping mapping)
    {
      IntPtr pMesh = m_mesh.NonConstPointer();
      IntPtr pConstMapping = mapping.ConstPointer();
      return UnsafeNativeMethods.ON_Mesh_SetTextureCoordinates2(pMesh, pConstMapping);
    }
    #endregion

    #region methods
    /// <summary>
    /// Scales the texture coordinates so the texture domains are [0,1] 
    /// and eliminate any texture rotations.
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool NormalizeTextureCoordinates()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxNormalizeTextureCoordinates);
    }
    /// <summary>
    /// Transposes texture coordinates.
    /// <para>The region of the bitmap the texture uses does not change.
    /// All texture coordinates rows (Us) become columns (Vs), and vice versa.</para>
    /// </summary>
    /// <returns>true on success, false on failure.</returns>
    public bool TransposeTextureCoordinates()
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_NonConstBoolOp(ptr, Mesh.idxTransposeTextureCoordinates);
    }
    /// <summary>
    /// Reverses one coordinate direction of the texture coordinates.
    /// <para>The region of the bitmap the texture uses does not change.
    /// Either Us or Vs direction is flipped.</para>
    /// </summary>
    /// <param name="direction">
    /// <para>0 = first texture coordinate is reversed.</para>
    /// <para>1 = second texture coordinate is reversed.</para>
    /// </param>
    /// <returns>true if operation succeeded; otherwise, false.</returns>
    public bool ReverseTextureCoordinates(int direction)
    {
      IntPtr ptr = m_mesh.NonConstPointer();
      return UnsafeNativeMethods.ON_Mesh_Reverse(ptr, true, direction);
    }
    #endregion

    #region IEnumerable implementation
    /// <summary>
    /// Gets an enumerator that yields all texture coordinates in this collection.
    /// </summary>
    /// <returns>The enumerator.</returns>
    public IEnumerator<Point2f> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<MeshTextureCoordinateList, Point2f>(this);
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
    #endregion
  }
}

//class ON_CLASS ON_MeshVertexRef : public ON_Geometry
//{
//  ON_OBJECT_DECLARE(ON_MeshVertexRef);
//public:
//  ON_MeshVertexRef();
//  ~ON_MeshVertexRef();
//  ON_MeshVertexRef& operator=(const ON_MeshVertexRef&);


//  // parent mesh
//  const ON_Mesh* m_mesh;

//  // m_mesh->m_V[] index
//  // (can be -1 when m_top_vi references a shared vertex location)
//  int m_mesh_vi; 

//  // m_mesh->m_top.m_tope[] index
//  int m_top_vi; 


//  /*
//  Description:
//    Override of the virtual ON_Geometry::ComponentIndex().
//  Returns:
//    A component index for the vertex.  The type of the returned
//    component index can be 
//    ON_ComponentIndex::mesh_vertex, 
//    ON_ComponentIndex::meshtop_vertex, or
//    ON_ComponentIndex::invalid_type.
//  */
//  ON_ComponentIndex ComponentIndex() const;

//  /*
//  Returns:
//    The mesh topology associated with this 
//    mesh vertex reference or NULL if it doesn't
//    exist.
//  */
//  const ON_MeshTopology* MeshTopology() const;

//  /*
//  Returns:
//    The 3d location of the mesh vertex.  Returns
//    ON_UNSET_POINT is this ON_MeshVertexRef is not 
//    valid.
//  */
//  ON_3dPoint Point() const;

//  /*
//  Returns:
//    The mesh topology vertex associated with this 
//    mesh vertex reference.
//  */
//  const ON_MeshTopologyVertex* MeshTopologyVertex() const;

//  // overrides of virtual ON_Object functions
//  BOOL IsValid( ON_TextLog* text_log = NULL ) const;
//  void Dump( ON_TextLog& ) const;
//  unsigned int SizeOf() const;
//  ON::ObjectType ObjectType() const;

//  // overrides of virtual ON_Geometry functions
//  int Dimension() const;
//  BOOL GetBBox(
//         double* boxmin,
//         double* boxmax,
//         int bGrowBox = false
//         ) const;
//  BOOL Transform( 
//         const ON_Xform& xform
//         );
//};

//class ON_CLASS ON_MeshEdgeRef : public ON_Geometry
//{
//  ON_OBJECT_DECLARE(ON_MeshEdgeRef);
//public:
//  ON_MeshEdgeRef();
//  ~ON_MeshEdgeRef();
//  ON_MeshEdgeRef& operator=(const ON_MeshEdgeRef&);

//  // parent mesh
//  const ON_Mesh* m_mesh;

//  // m_mesh->m_top.m_tope[] index
//  int m_top_ei; 

//  /*
//  Description:
//    Override of the virtual ON_Geometry::ComponentIndex().
//  Returns:
//    A mesh component index for the edge.  The type is
//    ON_ComponentIndex::meshtop_edge and the index is the
//    index into the ON_MeshTopology.m_tope[] array.
//  */
//  ON_ComponentIndex ComponentIndex() const;

//  /*
//  Returns:
//    The mesh topology associated with this 
//    mesh edge reference or NULL if it doesn't
//    exist.
//  */

//  const ON_MeshTopology* MeshTopology() const;
//  /*
//  Returns:
//    The 3d location of the mesh edge.  Returns
//    ON_UNSET_POINT,ON_UNSET_POINT, is this ON_MeshEdgeRef
//    is not valid.
//  */
//  ON_Line Line() const;

//  /*
//  Returns:
//    The mesh topology edge associated with this 
//    mesh edge reference.
//  */
//  const ON_MeshTopologyEdge* MeshTopologyEdge() const;

//  // overrides of virtual ON_Object functions
//  BOOL IsValid( ON_TextLog* text_log = NULL ) const;
//  void Dump( ON_TextLog& ) const;
//  unsigned int SizeOf() const;
//  ON::ObjectType ObjectType() const;

//  // overrides of virtual ON_Geometry functions
//  int Dimension() const;
//  BOOL GetBBox(
//         double* boxmin,
//         double* boxmax,
//         int bGrowBox = false
//         ) const;
//  BOOL Transform( 
//         const ON_Xform& xform
//         );
//};

//class ON_CLASS ON_MeshFaceRef : public ON_Geometry
//{
//  ON_OBJECT_DECLARE(ON_MeshFaceRef);
//public:
//  ON_MeshFaceRef();
//  ~ON_MeshFaceRef();
//  ON_MeshFaceRef& operator=(const ON_MeshFaceRef&);

//  // parent mesh
//  const ON_Mesh* m_mesh;

//  // m_mesh->m_F[] and m_mesh->m_top.m_tope[] index.
//  int m_mesh_fi; 

//  /*
//  Description:
//    Override of the virtual ON_Geometry::ComponentIndex().
//  Returns:
//    A mesh component index for the face.  The type is
//    ON_ComponentIndex::mesh_face and the index is the
//    index into the ON_Mesh.m_F[] array.
//  */
//  ON_ComponentIndex ComponentIndex() const;

//  /*
//  Returns:
//    The mesh topology associated with this 
//    mesh face reference or NULL if it doesn't
//    exist.
//  */
//  const ON_MeshTopology* MeshTopology() const;

//  /*
//  Returns:
//    The mesh face associated with this mesh face reference.
//  */
//  const ON_MeshFace* MeshFace() const;

//  /*
//  Returns:
//    The mesh topology face associated with this 
//    mesh face reference.
//  */
//  const ON_MeshTopologyFace* MeshTopologyFace() const;

//  // overrides of virtual ON_Object functions
//  BOOL IsValid( ON_TextLog* text_log = NULL ) const;
//  void Dump( ON_TextLog& ) const;
//  unsigned int SizeOf() const;
//  ON::ObjectType ObjectType() const;

//  // overrides of virtual ON_Geometry functions
//  int Dimension() const;
//  BOOL GetBBox(
//         double* boxmin,
//         double* boxmax,
//         int bGrowBox = false
//         ) const;
//  BOOL Transform( 
//         const ON_Xform& xform
//         );
//};

//*
//Description:
//  Calculate a quick and dirty polygon mesh approximation
//  of a surface.
//Parameters:
//  surface - [in]
//  mesh_density - [in] If <= 10, this number controls
//        the relative polygon count.  If > 10, this number
//        specifies a target number of polygons.
//  mesh - [in] if not NULL, the polygon mesh will be put
//              on this mesh.
//Returns:
//  A polygon mesh approximation of the surface or NULL
//  if the surface could not be meshed.
//*/
//ON_DECL
//ON_Mesh* ON_MeshSurface( 
//            const ON_Surface& surface, 
//            int mesh_density = 0,
//            ON_Mesh* mesh = 0
//            );

//*
//Description:
//  Calculate a quick and dirty polygon mesh approximation
//  of a surface.
//Parameters:
//  surface - [in]
//  u_count - [in] >= 2 Number of "u" parameters in u[] array.
//  u       - [in] u parameters
//  v_count - [in] >= 2 Number of "v" parameters in v[] array.
//  v       - [in] v parameters
//  mesh - [in] if not NULL, the polygon mesh will be put
//              on this mesh.
//Returns:
//  A polygon mesh approximation of the surface or NULL
//  if the surface could not be meshed.
//*/
//ON_DECL
//ON_Mesh* ON_MeshSurface( 
//            const ON_Surface& surface, 
//            int u_count,
//            const double* u,
//            int v_count,
//            const double* v,
//            ON_Mesh* mesh = 0
//            );

//*
//Description:
//  Finds the barycentric coordinates of the point on a 
//  triangle that is closest to P.
//Parameters:
//  A - [in] triangle corner
//  B - [in] triangle corner
//  C - [in] triangle corner
//  P - [in] point to test
//  a - [out] barycentric coordinate
//  b - [out] barycentric coordinate
//  c - [out] barycentric coordinate
//        If ON_ClosestPointToTriangle() returns true, then
//        (*a)*A + (*b)*B + (*c)*C is the point on the 
//        triangle's plane that is closest to P.  It is 
//        always the case that *a + *b + *c = 1, but this
//        function will return negative barycentric 
//        coordinate if the point on the plane is not
//        inside the triangle.
//Returns:
//  True if the triangle is not degenerate.  False if the
//  triangle is degenerate; in this case the returned
//  closest point is the input point that is closest to P.
//*/
//ON_DECL
//bool ON_ClosestPointToTriangle( 
//        ON_3dPoint A, ON_3dPoint B, ON_3dPoint C,
//        ON_3dPoint P,
//        double* a, double* b, double* c
//        );


//*
//Description:
//  Finds the barycentric coordinates of the point on a 
//  triangle that is closest to P.
//Parameters:
//  A - [in] triangle corner
//  B - [in] triangle corner
//  C - [in] triangle corner
//  P - [in] point to test
//  a - [out] barycentric coordinate
//  b - [out] barycentric coordinate
//  c - [out] barycentric coordinate
//        If ON_ClosestPointToTriangle() returns true, then
//        (*a)*A + (*b)*B + (*c)*C is the point on the 
//        triangle's plane that is closest to P.  It is 
//        always the case that *a + *b + *c = 1, but this
//        function will return negative barycentric 
//        coordinate if the point on the plane is not
//        inside the triangle.
//Returns:
//  True if the triangle is not degenerate.  False if the
//  triangle is degenerate; in this case the returned
//  closest point is the input point that is closest to P.
//*/
//ON_DECL
//bool ON_ClosestPointToTriangleFast( 
//          const ON_3dPoint& A, 
//          const ON_3dPoint& B, 
//          const ON_3dPoint& C, 
//          ON_3dPoint P,
//          double* a, double* b, double* c
//          );


//*
//Description:
//  Calculate a mesh representation of the NURBS surface's control polygon.
//Parameters:
//  nurbs_surface - [in]
//  bCleanMesh - [in] If true, then degenerate quads are cleaned
//                    up to be triangles. Surfaces with singular
//                    sides are a common source of degenerate qauds.
//  input_mesh - [in] If NULL, then the returned mesh is created
//       by a class to new ON_Mesh().  If not null, then this 
//       mesh will be used to store the conrol polygon.
//Returns:
//  If successful, a pointer to a mesh.
//*/
//ON_DECL
//ON_Mesh* ON_ControlPolygonMesh( 
//          const ON_NurbsSurface& nurbs_surface, 
//          bool bCleanMesh,
//          ON_Mesh* input_mesh = NULL
//          );

//*
//Description:
//  Finds the intersection between a line segment an a triangle.
//Parameters:
//  A - [in] triangle corner
//  B - [in] triangle corner
//  C - [in] triangle corner
//  P - [in] start of line segment
//  Q - [in] end of line segment
//  abc - [out] 
//     barycentric coordinates of intersection point(s)
//  t - [out] line coordinate of intersection point(s)
//Returns:
//  0 - no intersection
//  1 - one intersection point
//  2 - intersection segment
//*/
//ON_DECL
//int ON_LineTriangleIntersect(
//        const ON_3dPoint& A,
//        const ON_3dPoint& B,
//        const ON_3dPoint& C,
//        const ON_3dPoint& P,
//        const ON_3dPoint& Q,
//        double abc[2][3], 
//        double t[2],
//        double tol
//        );

//*
//Description:
//  Finds the unit normal to the triangle
//Parameters:
//  A - [in] triangle corner
//  B - [in] triangle corner
//  C - [in] triangle corner
//Returns:
//  Unit normal
//*/
//ON_DECL
//ON_3dVector ON_TriangleNormal(
//        const ON_3dPoint& A,
//        const ON_3dPoint& B,
//        const ON_3dPoint& C
//        );

//*
//Description:
//  Triangulate a 2D simple closed polygon.
//Parameters:
//  point_count - [in] number of points in polygon ( >= 3 )
//  point_stride - [in]
//  P - [in] 
//    i-th point = (P[i*point_stride], P[i*point_stride+1])
//  tri_stride - [in]
//  triangle - [out]
//    array of (point_count-2)*tri_stride integers
//Returns:
//  True if successful.  In this case, the polygon is trianglulated into 
//  point_count-2 triangles.  The indexes of the 3 points that are the 
//  corner of the i-th (0<= i < point_count-2) triangle are
//    (triangle[i*tri_stride], triangle[i*tri_stride+1], triangle[i*tri_stride+2]).
//Remarks:
//  Do NOT duplicate the start/end point; i.e., a triangle will have
//  a point count of 3 and P will specify 3 distinct non-collinear points.
//*/
//ON_DECL
//bool ON_Mesh2dPolygon( 
//          int point_count,
//          int point_stride,
//          const double* P,
//          int tri_stride,
//          int* triangle 
//          );

//#endif
namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the values of the four indices of a mesh face quad.
  /// <para>If the third and fourth values are the same, this face represents a
  /// triangle.</para>
  /// </summary>
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 16)]
  [DebuggerDisplay("{DebuggerDisplayUtil}")]
  //[Serializable]
  public struct MeshFace
  {
    #region members
    internal int m_a;
    internal int m_b;
    internal int m_c;
    internal int m_d;
    #endregion

    #region constructors
    /// <summary>
    /// Constructs a new triangular Mesh face.
    /// </summary>
    /// <param name="a">Index of first corner.</param>
    /// <param name="b">Index of second corner.</param>
    /// <param name="c">Index of third corner.</param>
    public MeshFace(int a, int b, int c)
    {
      m_a = a;
      m_b = b;
      m_c = c;
      m_d = c;
    }
    /// <summary>
    /// Constructs a new quadrangular Mesh face.
    /// </summary>
    /// <param name="a">Index of first corner.</param>
    /// <param name="b">Index of second corner.</param>
    /// <param name="c">Index of third corner.</param>
    /// <param name="d">Index of fourth corner.</param>
    public MeshFace(int a, int b, int c, int d)
    {
      m_a = a;
      m_b = b;
      m_c = c;
      m_d = d;
    }

    /// <summary>
    /// Gets an Unset MeshFace. Unset faces have Int32.MinValue for all corner indices.
    /// </summary>
    public static MeshFace Unset
    {
      get { return new MeshFace(int.MinValue, int.MinValue, int.MinValue); }
    }
    #endregion

    #region properties
    /// <summary>
    /// Internal property that figures out the debugger display for Mesh Faces.
    /// </summary>
    internal string DebuggerDisplayUtil
    {
      get
      {
        if (IsTriangle)
        {
          return string.Format(System.Globalization.CultureInfo.InvariantCulture, "T({0}, {1}, {2})", m_a, m_b, m_c);
        }
        return string.Format(System.Globalization.CultureInfo.InvariantCulture, "Q({0}, {1}, {2}, {3})", m_a, m_b, m_c, m_d);
      }
    }

    /// <summary>
    /// Gets or sets the first corner index of the mesh face.
    /// </summary>
    public int A
    {
      get { return m_a; }
      set { m_a = value; }
    }
    /// <summary>
    /// Gets or sets the second corner index of the mesh face.
    /// </summary>
    public int B
    {
      get { return m_b; }
      set { m_b = value; }
    }
    /// <summary>
    /// Gets or sets the third corner index of the mesh face.
    /// </summary>
    public int C
    {
      get { return m_c; }
      set { m_c = value; }
    }
    /// <summary>
    /// Gets or sets the fourth corner index of the mesh face. 
    /// If D equals C, the mesh face is considered to be a triangle 
    /// rather than a quad.
    /// </summary>
    public int D
    {
      get { return m_d; }
      set { m_d = value; }
    }

    /// <summary>
    /// Gets or sets the vertex index associated with an entry in this face.
    /// </summary>
    /// <param name="index">A number in interval [0-3] that refers to an index of a vertex in this face.</param>
    /// <returns>The vertex index associated with this mesh face.</returns>
    public int this[int index]
    {
      get
      {
        if (index == 0) return m_a;
        if (index == 1) return m_b;
        if (index == 2) return m_c;
        if (index == 3) return m_d;
        throw new IndexOutOfRangeException();
      }
      set
      {
        if (index == 0) m_a = value;
        else if (index == 1) m_b = value;
        else if (index == 2) m_c = value;
        else if (index == 3) m_d = value;
        else
          throw new IndexOutOfRangeException();
      }
    }

    /// <summary>
    /// Gets a value indicating whether or not this mesh face 
    /// is considered to be valid. Note that even valid mesh faces 
    /// could potentially be invalid in the context of a specific Mesh, 
    /// if one or more of the corner indices exceeds the number of 
    /// vertices on the mesh. If you want to perform a complete 
    /// validity check, use IsValid(int) instead.
    /// </summary>
    public bool IsValid()
    {
      if (m_a < 0) { return false; }
      if (m_b < 0) { return false; }
      if (m_c < 0) { return false; }
      if (m_d < 0) { return false; }

      if (m_a == m_b) { return false; }
      if (m_a == m_c) { return false; }
      if (m_a == m_d) { return false; }
      if (m_b == m_c) { return false; }
      if (m_b == m_d) { return false; }

      return true;
    }
    /// <summary>
    /// Gets a value indicating whether or not this mesh face 
    /// is considered to be valid. Unlike the simple IsValid function, 
    /// this function takes upper bound indices into account.
    /// </summary>
    /// <param name="vertexCount">Number of vertices in the mesh that this face is a part of.</param>
    /// <returns>true if the face is considered valid, false if not.</returns>
    public bool IsValid(int vertexCount)
    {
      if (!IsValid()) { return false; }

      if (m_a >= vertexCount) { return false; }
      if (m_b >= vertexCount) { return false; }
      if (m_c >= vertexCount) { return false; }
      if (m_d >= vertexCount) { return false; }

      return true;
    }

    /// <summary>
    /// Gets a value indicating whether or not this mesh face is a triangle. 
    /// A mesh face is considered to be a triangle when C equals D, thus it is 
    /// possible for an Invalid mesh face to also be a triangle.
    /// </summary>
    public bool IsTriangle { get { return m_c == m_d; } }
    /// <summary>
    /// Gets a value indicating whether or not this mesh face is a quad. 
    /// A mesh face is considered to be a triangle when C does not equal D, 
    /// thus it is possible for an Invalid mesh face to also be a quad.
    /// </summary>
    public bool IsQuad { get { return m_c != m_d; } }
    #endregion

    #region methods
    /// <summary>
    /// Sets all the corners for this face as a triangle.
    /// </summary>
    /// <param name="a">Index of first corner.</param>
    /// <param name="b">Index of second corner.</param>
    /// <param name="c">Index of third corner.</param>
    public void Set(int a, int b, int c)
    {
      m_a = a;
      m_b = b;
      m_c = c;
      m_d = c;
    }
    /// <summary>
    /// Sets all the corners for this face as a quad.
    /// </summary>
    /// <param name="a">Index of first corner.</param>
    /// <param name="b">Index of second corner.</param>
    /// <param name="c">Index of third corner.</param>
    /// <param name="d">Index of fourth corner.</param>
    public void Set(int a, int b, int c, int d)
    {
      m_a = a;
      m_b = b;
      m_c = c;
      m_d = d;
    }

    /// <summary>
    /// Reverses the orientation of the face by swapping corners. 
    /// The first corner is always maintained.
    /// </summary>
    public MeshFace Flip()
    {
      if (m_c == m_d)
        return new MeshFace(m_a, m_c, m_b, m_b);
      return new MeshFace(m_a, m_d, m_c, m_a);
    }
    #endregion
  }
}
