#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

namespace Rhino
{
  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  [System.Diagnostics.DebuggerDisplay("{m_i}, {m_j}")]
  [Serializable]
  public struct IndexPair
  {
    int m_i, m_j;

    public IndexPair(int i, int j)
    {
      m_i = i;
      m_j = j;
    }
    
    public int I
    {
      get { return m_i; }
      set { m_i = value; }
    }
    public int J
    {
      get { return m_j; }
      set { m_j = value; }
    }
  }


  public static class RhinoMath
  {
    // Only used inside of this class. Not exposed since there is already
    // a Math.PI that people can use
    const double PI = 3.141592653589793238462643;

    /// <summary>
    /// Gets the Zero Tolerance constant (1.0e-12).
    /// </summary>
    public const double ZeroTolerance = 1.0e-12;

    /// <summary>
    /// Gets the Rhino standard Unset value. Use this value rather than Double.NaN when 
    /// a bogus floating point value is required.
    /// </summary>
    public const double UnsetValue = -1.23432101234321e+308;

    public const double SqrtEpsilon = 1.490116119385000000e-8;
    public const double DefaultAngleTolerance = PI / 180.0;

    /// <summary>
    /// Gets the single precision floating point number that is considered 'unset' in Rhino.
    /// </summary>
    public const float UnsetSingle = -1.234321e+38f; 

    /// <summary>
    /// Convert an angle from degrees to radians.
    /// </summary>
    /// <param name="degrees">Degrees to convert (180 degrees equals pi radians).</param>
    public static double ToRadians(double degrees)
    {
      return degrees * PI / 180.0;
    }

    /// <summary>
    /// Convert an angle from radians to degrees.
    /// </summary>
    /// <param name="radians">Radians to convert (180 degrees equals pi radians).</param>
    public static double ToDegrees(double radians)
    {
      return radians * 180.0 / PI;
    }

    /// <summary>
    /// Determines whether a <see cref="double"/> value is valid within the RhinoCommon context.
    /// <para>Rhino does not use Double.NaN by convention, so this test evaluates to true if:</para>
    /// <para>x is not equal to RhinoMath.UnsetValue</para>
    /// <para>System.Double.IsNaN(x) evaluates to false</para>
    /// <para>System.Double.IsInfinity(x) evaluates to false</para>
    /// </summary>
    /// <param name="x"><see cref="double"/> number to test for validity</param>
    /// <returns>True if the number if valid, False if the number is NaN, Infinity or Unset</returns>
    public static bool IsValidDouble(double x)
    {
      return (x != UnsetValue) && (!double.IsInfinity(x)) && (!double.IsNaN(x));
    }

    /// <summary>
    /// Determines whether a <see cref="float"/> value is valid within the RhinoCommon context.
    /// <para>Rhino does not use Single.NaN by convention, so this test evaluates to true if:</para>
    /// <para>x is not equal to RhinoMath.UnsetValue,</para>
    /// <para>System.Single.IsNaN(x) evaluates to false</para>
    /// <para>System.Single.IsInfinity(x) evaluates to false</para>
    /// </summary>
    /// <param name="x"><see cref="float"/> number to test for validity</param>
    /// <returns>True if the number if valid, False if the number is NaN, Infinity or Unset</returns>
    public static bool IsValidSingle(float x)
    {
      return (x != UnsetSingle) && (!float.IsInfinity(x)) && (!float.IsNaN(x));
    }

    /// <summary>Scale factor for changing unit "standard" systems</summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static double UnitScale(UnitSystem from, UnitSystem to)
    {
      return UnsafeNativeMethods.ONC_UnitScale((int)from, (int)to);
    }

    public static int Clamp(int value, int bound1, int bound2)
    {
      int min = bound1;
      int max = bound2;

      if (bound1 > bound2)
      {
        min = bound2;
        max = bound1;
      }
      if (value > max)
        value = max;
      if (value < min)
        value = min;
      return value;
    }

    public static double Clamp(double value, double bound1, double bound2)
    {
      double min = bound1;
      double max = bound2;

      if (bound1 > bound2)
      {
        min = bound2;
        max = bound1;
      }
      if (value > max)
        value = max;
      if (value < min)
        value = min;
      return value;
    }

    [CLSCompliant(false)]
    public static uint CRC32(uint currentRemainder, byte[] buffer)
    {
      return UnsafeNativeMethods.ON_CRC32_Compute(currentRemainder, buffer.Length, buffer);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_analysismode.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_analysismode.cs' lang='cs'/>
    /// </example>
    [CLSCompliant(false)]
    public static uint CRC32(uint currentRemainder, double value)
    {
      return CRC32(currentRemainder, BitConverter.GetBytes(value));
    }
    [CLSCompliant(false)]
    public static uint CRC32(uint currentRemainder, int value)
    {
      return CRC32(currentRemainder, BitConverter.GetBytes(value));
    }
  }

  public enum UnitSystem : int
  {
    None = 0,
    /// <summary>1.0e-10 meters</summary>
    Angstroms = 12,
    /// <summary>1.0e-9 meters</summary>
    Nanometers = 13,
    /// <summary>1.0e-6 meters</summary>
    Microns = 1,
    /// <summary>1.0e-3 meters</summary>
    Millimeters = 2,
    /// <summary>1.0e-2 meters</summary>
    Centimeters = 3,
    /// <summary>1.0e-1 meters</summary>
    Decimeters = 14,
    Meters = 4,
    /// <summary>1.0e+1 meters</summary>
    Dekameters = 15,
    /// <summary>1.0e+2 meters</summary>
    Hectometers = 16,
    /// <summary>1.0e+3 meters</summary>
    Kilometers = 5,
    /// <summary>1.0e+6 meters</summary>
    Megameters = 17,
    /// <summary>1.0e+9 meters</summary>
    Gigameters = 18,
    /// <summary>2.54e-8 meters (1.0e-6 inches)</summary>
    Microinches = 6,
    /// <summary>2.54e-5 meters (0.001 inches)</summary>
    Mils = 7,
    /// <summary>0.0254 meters</summary>
    Inches = 8,
    /// <summary>0.3048 meters (12 inches)</summary>
    Feet = 9,
    /// <summary>0.9144 meters (36 inches)</summary>
    Yards = 19,
    /// <summary>1609.344 meters (5280 feet)</summary>
    Miles = 10,
    /// <summary>Printer Distance 1/72 inches (computer points)</summary>
    PrinterPoint = 20,
    /// <summary>Printer Distance 1/6 inches (computer picas)</summary>
    PrinterPica = 21,
    /// <summary>
    /// Terrestrial Distance, 1852 meters
    /// Approximately 1 minute of arc on a terrestrial great circle.
    /// See http://en.wikipedia.org/wiki/Nautical_mile.
    /// </summary>
    NauticalMile = 22,
    // astronomical distances
    /// <summary>
    /// Astronomical Distance
    /// http://en.wikipedia.org/wiki/Astronomical_unit
    /// 1.495979e+11  // http://units.nist.gov/Pubs/SP811/appenB9.htm
    /// An astronomical unit (au) is the mean distance from the
    /// center of the earth to the center of the sun.
    /// </summary>
    Astronomical = 23,
    /// <summary>
    /// Light Year
    /// http://en.wikipedia.org/wiki/Light_year
    /// 9.46073e+15 meters   http://units.nist.gov/Pubs/SP811/appenB9.htm
    /// A light year is the distance light travels in one Julian year.
    /// The speed of light is exactly 299792458 meters/second.
    /// A Julian year is exactly 365.25 * 86400 seconds and is
    /// approximately the time it takes for one earth orbit.
    /// </summary>
    Lightyears = 24,
    /// <summary>
    /// Parallax Second
    /// http://en.wikipedia.org/wiki/Parsec
    /// 3.085678e+16 meters   http://units.nist.gov/Pubs/SP811/appenB9.htm
    /// </summary>
    Parsecs = 25,
    /// <summary>
    /// Custom unit systems
    /// x meters with x defined in ON_3dmUnitsAndTolerances.m_custom_unit_scale
    /// </summary>
    CustomUnitSystem = 11
  }

  namespace Geometry
  {
    public enum Continuity : int
    {
      None = 0,

      /// <summary>
      /// Continuous Function : Test for parametric continuity. In particular, all types of curves
      /// are considered infinitely continuous at the start/end of the evaluation domain.
      /// </summary>
      C0_continuous = 1,
      /// <summary>
      /// Continuous first derivative : Test for parametric continuity. In particular,
      /// all types of curves are considered infinitely continuous at the start/end
      /// of the evaluation domain.
      /// </summary>
      C1_continuous = 2,
      /// <summary>
      /// Continuous first derivative and second derivative : Test for parametric continuity.
      /// In particular, all types of curves are considered infinitely continuous at the
      /// start/end of the evaluation domain.
      /// </summary>
      C2_continuous = 3,
      /// <summary>
      /// Continuous unit tangent : Test for parametric continuity. In particular, all types of
      /// curves are considered infinitely continuous at the start/end of the evaluation domain.
      /// </summary>
      G1_continuous = 4,
      /// <summary>
      /// Continuous unit tangent and curvature : Test for parametric continuity. In particular,
      /// all types of curves are considered infinitely continuous at the start/end of the
      /// evaluation domain.
      /// </summary>
      G2_continuous = 5,

      /// <summary>
      /// locus continuous function :
      /// Continuity tests using the following enum values are identical to tests using the
      /// preceding enum values on the INTERIOR of a curve's domain. At the END of a curve
      /// a "locus" test is performed in place of a parametric test. In particular, at the
      /// END of a domain, all open curves are locus discontinuous. At the END of a domain,
      /// all closed curves are at least C0_locus_continuous. By convention all Curves
      /// are considered locus continuous at the START of the evaluation domain. This
      /// convention is not strictly correct, but it was adopted to make iterative kink
      /// finding tools easier to use and so that locus discontinuities are reported once
      /// at the end parameter of a curve rather than twice.
      /// </summary>
      C0_locus_continuous = 6,
      /// <summary>
      /// locus continuous first derivative :
      /// Continuity tests using the following enum values are identical to tests using the
      /// preceding enum values on the INTERIOR of a curve's domain. At the END of a curve
      /// a "locus" test is performed in place of a parametric test. In particular, at the
      /// END of a domain, all open curves are locus discontinuous. At the END of a domain,
      /// all closed curves are at least C0_locus_continuous. By convention all Curves
      /// are considered locus continuous at the START of the evaluation domain. This
      /// convention is not strictly correct, but it was adopted to make iterative kink
      /// finding tools easier to use and so that locus discontinuities are reported once
      /// at the end parameter of a curve rather than twice.
      /// </summary>
      C1_locus_continuous = 7,
      /// <summary>
      /// locus continuous first and second derivative :
      /// Continuity tests using the following enum values are identical to tests using the
      /// preceding enum values on the INTERIOR of a curve's domain. At the END of a curve
      /// a "locus" test is performed in place of a parametric test. In particular, at the
      /// END of a domain, all open curves are locus discontinuous. At the END of a domain,
      /// all closed curves are at least C0_locus_continuous. By convention all Curves
      /// are considered locus continuous at the START of the evaluation domain. This
      /// convention is not strictly correct, but it was adopted to make iterative kink
      /// finding tools easier to use and so that locus discontinuities are reported once
      /// at the end parameter of a curve rather than twice.
      /// </summary>
      C2_locus_continuous = 8,
      /// <summary>
      /// locus continuous unit tangent :
      /// Continuity tests using the following enum values are identical to tests using the
      /// preceding enum values on the INTERIOR of a curve's domain. At the END of a curve
      /// a "locus" test is performed in place of a parametric test. In particular, at the
      /// END of a domain, all open curves are locus discontinuous. At the END of a domain,
      /// all closed curves are at least C0_locus_continuous. By convention all Curves
      /// are considered locus continuous at the START of the evaluation domain. This
      /// convention is not strictly correct, but it was adopted to make iterative kink
      /// finding tools easier to use and so that locus discontinuities are reported once
      /// at the end parameter of a curve rather than twice.
      /// </summary>
      G1_locus_continuous = 9,
      /// <summary>
      /// locus continuous unit tangent and curvature :
      /// Continuity tests using the following enum values are identical to tests using the
      /// preceding enum values on the INTERIOR of a curve's domain. At the END of a curve
      /// a "locus" test is performed in place of a parametric test. In particular, at the
      /// END of a domain, all open curves are locus discontinuous. At the END of a domain,
      /// all closed curves are at least C0_locus_continuous. By convention all Curves
      /// are considered locus continuous at the START of the evaluation domain. This
      /// convention is not strictly correct, but it was adopted to make iterative kink
      /// finding tools easier to use and so that locus discontinuities are reported once
      /// at the end parameter of a curve rather than twice.
      /// </summary>
      G2_locus_continuous = 10,
      /// <summary>
      /// analytic discontinuity
      /// </summary>
      Cinfinity_continuous = 11,
    }

    //public enum ControlPointStyle : int
    //{
    //  None = 0,
    //  NotRational = 1,
    //  HomogeneousRational = 2,
    //  EuclideanRational = 3,
    //  IntrinsicPointStyle = 4,
    //}

    /// <summary>
    /// Defines enumerated values for various mesh types.
    /// </summary>
    public enum MeshType : int
    {
      /// <summary>
      /// The default mesh.
      /// </summary>
      Default = 0,

      /// <summary>
      /// The render mesh.
      /// </summary>
      Render = 1,

      /// <summary>
      /// The analysis mesh.
      /// </summary>
      Analysis = 2,

      /// <summary>
      /// The preview mesh.
      /// </summary>
      Preview = 3,

      /// <summary>
      /// Any mesh that is available.
      /// </summary>
      Any = 4
    }
  }

  namespace DocObjects
  {
    /// <summary>Defines the current working space.</summary>
    public enum ActiveSpace : int
    {
      None = 0,
      /// <summary>3d modeling or "world" space</summary>
      ModelSpace = 1,
      /// <summary>page/layout/paper/printing space</summary>
      PageSpace = 2
    }

    public enum CoordinateSystem : int
    {
      World = 0,
      Camera = 1,
      Clip = 2,
      Screen = 3
    }

    public enum ObjectMode : int
    {
      ///<summary>object mode comes from layer</summary>
      Normal = 0,
      ///<summary>not visible, object cannot be selected or changed</summary>
      Hidden = 1,
      ///<summary>visible, object cannot be selected or changed</summary>
      Locked = 2,
      ///<summary>
      ///object is part of an InstanceDefinition. The InstanceDefinition
      ///m_object_uuid[] array will contain this object attribute's uuid.
      ///</summary>
      InstanceDefinitionObject = 3
      //ObjectModeCount = 4
    }

    public enum ObjectColorSource : int
    {
      /// <summary>use color assigned to layer</summary>
      ColorFromLayer = 0,
      /// <summary>use color assigned to object</summary>
      ColorFromObject = 1,
      /// <summary>use diffuse render material color</summary>
      ColorFromMaterial = 2,
      /// <summary>
      /// for objects with parents (like objects in instance references, use parent linetype)
      /// if no parent, treat as color_from_layer
      /// </summary>
      ColorFromParent = 3
    }

    public enum ObjectPlotColorSource : int
    {
      /// <summary>use plot color assigned to layer</summary>
      PlotColorFromLayer = 0,
      /// <summary>use plot color assigned to object</summary>
      PlotColorFromObject = 1,
      /// <summary>use display color</summary>
      PlotColorFromDisplay = 2,
      /// <summary>
      /// for objects with parents (like objects in instance references, use parent plot color)
      /// if no parent, treat as plot_color_from_layer
      /// </summary>
      PlotColorFromParent = 3
    }

    public enum ObjectPlotWeightSource : int
    {
      /// <summary>use plot color assigned to layer</summary>
      PlotWeightFromLayer = 0,
      /// <summary>use plot color assigned to object</summary>
      PlotWeightFromObject = 1,
      /// <summary>
      /// for objects with parents (like objects in instance references, use parent plot color)
      /// if no parent, treat as plot_color_from_layer
      /// </summary>
      PlotWeightFromParent = 3
    }

    public enum ObjectLinetypeSource : int
    {
      /// <summary>use line style assigned to layer</summary>
      LinetypeFromLayer = 0,
      /// <summary>use line style assigned to object</summary>
      LinetypeFromObject = 1,
      /// <summary>
      /// for objects with parents (like objects in instance references, use parent linetype)
      /// if not parent, treat as linetype_from_layer.
      /// </summary>
      LinetypeFromParent = 3
    }

    public enum ObjectMaterialSource : int
    {
      /// <summary>use material assigned to layer</summary>
      MaterialFromLayer = 0,
      /// <summary>use material assigned to object</summary>
      MaterialFromObject = 1,
      /// <summary>
      /// for objects with parents, like definition geometry in instance
      /// references and faces in polysurfaces, this value indicates the
      /// material definition should come from the parent. If the object
      /// does not have an obvious "parent", then treat it the same as
      /// material_from_layer.
      /// </summary>
      MaterialFromParent = 3
    }

    public enum DisplayMode : int
    {
      Default = 0,
      Wireframe = 1,
      Shaded = 2,
      RenderPreview = 3
    }

    public enum DistanceDisplayMode : int
    {
      Decimal = 0,
      Feet = 1,
      FeetAndInches = 2
    }

    public enum TextDisplayAlignment : int
    {
      Normal = 0, 
      Horizontal = 1,
      AboveLine = 2,
      InLine = 3
    }

    [Flags, CLSCompliant(false)]
    public enum ObjectType : uint
    {
      None = 0,
      Point = 1,
      PointSet = 2,
      Curve = 4,
      Surface = 8,
      Brep = 0x10,
      Mesh = 0x20,
      //Layer = 0x40,
      //Material = 0x80,
      Light = 0x100,
      Annotation = 0x200,
      //UserData = 0x400,
      InstanceDefinition = 0x800,
      InstanceReference = 0x1000,
      TextDot = 0x2000,
      /// <summary>Selection filter value - not a real object type</summary>
      Grip = 0x4000,
      Detail = 0x8000,
      Hatch = 0x10000,
      MorphControl = 0x20000,
      BrepLoop = 0x80000,
      /// <summary>Selection filter value - not a real object type</summary>
      PolysrfFilter = 0x200000,
      /// <summary>Selection filter value - not a real object type</summary>
      EdgeFilter = 0x400000,
      /// <summary>Selection filter value - not a real object type</summary>
      PolyedgeFilter = 0x800000,
      MeshVertex = 0x01000000,
      MeshEdge = 0x02000000,
      MeshFace = 0x04000000,
      Cage = 0x08000000,
      Phantom = 0x10000000,
      ClipPlane = 0x20000000,
      Extrusion = 0x40000000,
      AnyObject = 0xFFFFFFFF
    }

    [Flags]
    public enum ObjectDecoration : int
    {
      None = 0,
      /// <summary>arrow head at start</summary>
      StartArrowhead = 0x08,
      /// <summary>arrow head at end</summary>
      EndArrowhead = 0x10,
      /// <summary>arrow head at start and end</summary>
      BothArrowhead = 0x18
    }
  }
}

namespace Rhino.Geometry
{
  public enum LightStyle : int
  {
    None = 0,
    /// <summary>
    /// Light location and direction in camera coordinates.
    /// +x points to right, +y points up, +z points towards camera
    /// </summary>
    CameraDirectional = 4,
    /// <summary>
    /// Light location and direction in camera coordinates.
    /// +x points to right, +y points up, +z points towards camera
    /// </summary>
    CameraPoint = 5,
    /// <summary>
    /// Light location and direction in camera coordinates.
    /// +x points to right, +y points up, +z points towards camera
    /// </summary>
    CameraSpot = 6,
    /// <summary>Light location and direction in world coordinates</summary>
    WorldDirectional = 7,
    /// <summary>Light location and direction in world coordinates</summary>
    WorldPoint = 8,
    /// <summary>Light location and direction in world coordinates</summary>
    WorldSpot = 9,
    /// <summary>Pure ambient light</summary>
    Ambient = 10,
    WorldLinear = 11,
    WorldRectangular = 12
  }

  public enum ComponentIndexType : int
  {
    InvalidType = 0,
    BrepVertex = 1,
    BrepEdge = 2,
    BrepFace = 3,
    BrepTrim = 4,
    BrepLoop = 5,
    MeshVertex = 11,
    MeshTopologyVertex = 12,
    MeshTopologyEdge = 13,
    MeshFace = 14,
    InstanceDefinitionPart = 21,
    PolycurveSegment = 31,
    PointCloudPoint = 41,
    GroupMember = 51,
    DimLinearPoint = 100,
    DimRadialPoint = 101,
    DimAngularPoint = 102,
    DimOrdinatePoint = 103,
    DimTextPoint = 104,
    NoType = 0x0FFFFFFF // switched to 0fffffff from 0xffffffff in order to maintain cls compliance
  }

  [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
  [Serializable]
  public struct ComponentIndex
  {
    private readonly uint m_type;
    private readonly int m_index;

    internal ComponentIndex(ComponentIndexType type, int index)
    {
      m_type = (uint)type;
      m_index = index;
    }

    /// <summary>
    /// The interpretation of Index depends on the Type value.
    /// Type             m_index interpretation (0 based indices)
    /// no_type            used when context makes it clear what array is being index
    /// brep_vertex        Brep.m_V[] array index
    /// brep_edge          Brep.m_E[] array index
    /// brep_face          Brep.m_F[] array index
    /// brep_trim          Brep.m_T[] array index
    /// brep_loop          Brep.m_L[] array index
    /// mesh_vertex        Mesh.m_V[] array index
    /// meshtop_vertex     MeshTopology.m_topv[] array index
    /// meshtop_edge       MeshTopology.m_tope[] array index
    /// mesh_face          Mesh.m_F[] array index
    /// idef_part          InstanceDefinition.m_object_uuid[] array index
    /// polycurve_segment  PolyCurve::m_segment[] array index
    /// dim_linear_point   LinearDimension2::POINT_INDEX
    /// dim_radial_point   RadialDimension2::POINT_INDEX
    /// dim_angular_point  AngularDimension2::POINT_INDEX
    /// dim_ordinate_point OrdinateDimension2::POINT_INDEX
    /// dim_text_point     TextEntity2 origin point
    /// </summary>
    public ComponentIndexType ComponentIndexType
    {
      get
      {
        if (0xFFFFFFFF == m_type)
          return ComponentIndexType.NoType;
        int t = (int)m_type;
        return (ComponentIndexType)t;
      }
    }
    /// <summary>
    /// The interpretation of m_index depends on the m_type value.
    /// m_type             m_index interpretation (0 based indices)
    /// no_type            used when context makes it clear what array is being index
    /// brep_vertex        Brep.m_V[] array index
    /// brep_edge          Brep.m_E[] array index
    /// brep_face          Brep.m_F[] array index
    /// brep_trim          Brep.m_T[] array index
    /// brep_loop          Brep.m_L[] array index
    /// mesh_vertex        Mesh.m_V[] array index
    /// meshtop_vertex     MeshTopology.m_topv[] array index
    /// meshtop_edge       MeshTopology.m_tope[] array index
    /// mesh_face          Mesh.m_F[] array index
    /// idef_part          InstanceDefinition.m_object_uuid[] array index
    /// polycurve_segment  PolyCurve::m_segment[] array index
    /// dim_linear_point   LinearDimension2::POINT_INDEX
    /// dim_radial_point   RadialDimension2::POINT_INDEX
    /// dim_angular_point  AngularDimension2::POINT_INDEX
    /// dim_ordinate_point OrdinateDimension2::POINT_INDEX
    /// dim_text_point     TextEntity2 origin point
    /// </summary>
    public int Index
    {
      get { return m_index; }
    }

    public static ComponentIndex Unset
    {
      get { return new ComponentIndex(ComponentIndexType.InvalidType, -1); }
    }

  }
}
