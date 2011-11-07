using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Specifies enumerated constants used to indicate the internal alignment and justification of text.
  /// </summary>
  public enum TextJustification : int
  {
    /// <summary>
    /// The default justification.
    /// </summary>
    None = 0,

    /// <summary>
    /// Left justification.
    /// </summary>
    Left = 1 << 0,

    /// <summary>
    /// Center justification.
    /// </summary>
    Center = 1 << 1,

    /// <summary>
    /// Right justification.
    /// </summary>
    Right = 1 << 2,

    /// <summary>
    /// Bottom inner alignment.
    /// </summary>
    Bottom = 1 << 16,

    /// <summary>
    /// Middle inner alignment.
    /// </summary>
    Middle = 1 << 17,

    /// <summary>
    /// Top inner alignment.
    /// </summary>
    Top = 1 << 18,

    /// <summary>
    /// Combination of left justification and bottom alignment.
    /// </summary>
    BottomLeft = Bottom | Left,

    /// <summary>
    /// Combination of center justification and bottom alignment.
    /// </summary>
    BottomCenter = Bottom | Center,

    /// <summary>
    /// Combination of right justification and bottom alignment.
    /// </summary>
    BottomRight = Bottom | Right,

    /// <summary>
    /// Combination of left justification and middle alignment.
    /// </summary>
    MiddleLeft = Middle | Left,

    /// <summary>
    /// Combination of center justification and middle alignment.
    /// </summary>
    MiddleCenter = Middle | Center,

    /// <summary>
    /// Combination of right justification and middle alignment.
    /// </summary>
    MiddleRight = Middle | Right,

    /// <summary>
    /// Combination of left justification and top alignment.
    /// </summary>
    TopLeft = Top | Left,

    /// <summary>
    /// Combination of center justification and top alignment.
    /// </summary>
    TopCenter = Top | Center,

    /// <summary>
    /// Combination of right justification and top alignment.
    /// </summary>
    TopRight = Top | Right
  }

  /// <summary>
  /// Provides a common base class to all annotation geometry.
  /// <para>This class refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class AnnotationBase : GeometryBase, ISerializable
  {
    internal AnnotationBase(IntPtr native_pointer, object parent)
      : base(native_pointer, parent, -1)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected AnnotationBase() { }

    /// <summary>
    /// Protected serialization constructor for internal use.
    /// </summary>
    protected AnnotationBase(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    #region internal helper functions
    internal Point2d GetPoint(int which)
    {
      IntPtr pConstThis = ConstPointer();
      Point2d rc = new Point2d();
      UnsafeNativeMethods.ON_Annotation2_GetPoint(pConstThis, which, ref rc);
      return rc;
    }
    internal void SetPoint(int which, Point2d pt)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_Annotation2_SetPoint(pThis, which, pt);
    }

    #endregion

    /// <summary>
    /// Gets the numeric value, depending on geometry type.
    /// <para>LinearDimension: distance between arrow tips</para>
    /// <para>RadialDimension: radius or diamater depending on type</para>
    /// <para>AngularDimension: angle in degrees</para>
    /// <para>Leader or Text: UnsetValue</para>
    /// </summary>
    public double NumericValue
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_NumericValue(pConstThis);
      }
    }

    /// <summary>
    /// Gets or sets the text for this annotation.
    /// </summary>
    public string Text
    {
      get
      {
        IntPtr pThis = ConstPointer();
        IntPtr pText = UnsafeNativeMethods.ON_Annotation2_Text(pThis, null, false);
        return pText == IntPtr.Zero ? String.Empty : Marshal.PtrToStringUni(pText);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Text(pThis, value, false);
      }
    }

    /// <summary>
    /// Gets or sets a formula for this annotation.
    /// </summary>
    public string TextFormula
    {
      get
      {
        IntPtr pThis = ConstPointer();
        IntPtr pText = UnsafeNativeMethods.ON_Annotation2_Text(pThis, null,true);
        return pText == IntPtr.Zero ? String.Empty : Marshal.PtrToStringUni(pText);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Text(pThis, value,true);
      }
    }

    /// <summary>
    /// Gets or sets the text height in model units.
    /// </summary>
    public double TextHeight
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Height(pConstThis, false, 0.0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Height(pThis, true, value);
      }
    }

    /// <summary>
    /// Gets or sets the plane containing the annotation.
    /// </summary>
    public Plane Plane
    {
      get
      {
        Plane rc = new Plane();
        IntPtr pThis = ConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Plane(pThis, ref rc, false);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Plane(pThis, ref value, true);
      }
    }


  }

  /// <summary>
  /// Represents a linear dimension.
  /// <para>This entity refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class LinearDimension : AnnotationBase, ISerializable
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearDimension"/> class.
    /// </summary>
    public LinearDimension()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LinearDimension2_New();
      ConstructNonConstObject(ptr);
    }

    /// <example>
    /// <code source='examples\vbnet\ex_addlineardimension2.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlineardimension2.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlineardimension2.py' lang='py'/>
    /// </example>
    public LinearDimension(Plane dimensionPlane, Point2d extensionLine1End, Point2d extensionLine2End, Point2d pointOnDimensionLine)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_LinearDimension2_New();
      ConstructNonConstObject(ptr);
      Plane = dimensionPlane;
      SetLocations(extensionLine1End, extensionLine2End, pointOnDimensionLine);
    }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected LinearDimension(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

#if RHINO_SDK
    /// <summary>
    /// Initializes a new instance of the <see cref="LinearDimension"/> class, based on three points.
    /// </summary>
    public static LinearDimension FromPoints(Point3d extensionLine1End, Point3d extensionLine2End, Point3d pointOnDimensionLine)
    {
      Point3d[] points = new Point3d[] { extensionLine1End, extensionLine2End, pointOnDimensionLine };
      // Plane dimPlane = new Plane(extensionLine1End, extensionLine2End, pointOnDimensionLine);
      Plane dimPlane;
      if (Plane.FitPlaneToPoints(points, out dimPlane) != PlaneFitResult.Success)
        return null;
      double s, t;
      if (!dimPlane.ClosestParameter(extensionLine1End, out s, out t))
        return null;
      Point2d ext1 = new Point2d(s, t);
      if (!dimPlane.ClosestParameter(extensionLine2End, out s, out t))
        return null;
      Point2d ext2 = new Point2d(s, t);
      if (!dimPlane.ClosestParameter(pointOnDimensionLine, out s, out t))
        return null;
      Point2d linePt = new Point2d(s, t);
      return new LinearDimension(dimPlane, ext1, ext2, linePt);
    }
#endif

    internal LinearDimension(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new LinearDimension(IntPtr.Zero, null);
    }

    /// <summary>
    /// Gets the distance between arrow tips.
    /// </summary>
    public double DistanceBetweenArrowTips
    {
      get{ return NumericValue; }
    }

    /// <summary>
    /// Index of DimensionStyle in document DimStyle table used by the dimension
    /// </summary>
    public int DimensionStyleIndex
    {
      get
      {
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Index(pThis, false, 0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Index(pThis, true, value);
      }
    }

    const int ext0_pt_index = 0;   // end of first extension line
    const int arrow0_pt_index = 1; // arrowhead tip on first extension line
    const int ext1_pt_index = 2;   // end of second extension line
    const int arrow1_pt_index = 3; // arrowhead tip on second extension line
    const int userpositionedtext_pt_index = 4;

    /// <summary>
    /// Gets the end of the first extension line.
    /// </summary>
    public Point2d ExtensionLine1End
    {
      get { return GetPoint(ext0_pt_index); }
      //set { SetPoint(ext0_pt_index, value); }
    }

    /// <summary>
    /// Gets the end of the second extension line.
    /// </summary>
    public Point2d ExtensionLine2End
    {
      get { return GetPoint(ext1_pt_index); }
      //set { SetPoint(ext1_pt_index, value); }
    }

    /// <summary>
    /// Gets the arrow head end of the first extension line.
    /// </summary>
    public Point2d Arrowhead1End
    {
      get { return GetPoint(arrow0_pt_index); }
      //set { SetPoint(arrow0_pt_index, value); }
    }

    /// <summary>
    /// Gets the arrow head end of the second extension line.
    /// </summary>
    public Point2d Arrowhead2End
    {
      get { return GetPoint(arrow1_pt_index); }
      //set { SetPoint(arrow1_pt_index, value); }
    }

    /// <summary>
    /// Gets the position of text on the plane.
    /// </summary>
    public Point2d TextPosition
    {
      get { return GetPoint(userpositionedtext_pt_index); }
      //set { SetPoint(userpositionedtext_pt_index, value); }
    }

    /// <summary>
    /// Sets the three locations of the point, unsing two-dimensional points that refer to the plane of the annotation.
    /// </summary>
    public void SetLocations(Point2d extensionLine1End, Point2d extensionLine2End, Point2d pointOnDimensionLine)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ON_LinearDimension2_SetLocations(pThis, extensionLine1End, extensionLine2End, pointOnDimensionLine);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this annotation is aligned.
    /// </summary>
    public bool Aligned
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_LinearDimension2_IsAligned(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_LinearDimension2_SetAligned(pThis, value);
      }
    }
  }

  /// <summary>
  /// Represents a dimension of a circular entity that can be measured with radius or diameter.
  /// <para>This entity refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class RadialDimension : AnnotationBase, ISerializable
  {
    internal RadialDimension(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected RadialDimension(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    /// <summary>
    /// Gets a value indicating whether the value refers to the diameter, rather than the radius.
    /// </summary>
    public bool IsDiameterDimension
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_RadialDimension2_IsDiameterDimension(pConstThis);
      }
    }
  }

  /// <summary>
  /// Represents a dimension of an entity that can be measured with an angle.
  /// <para>This entity refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class AngularDimension : AnnotationBase, ISerializable
  {
    internal AngularDimension(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected AngularDimension(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
  }

  /// <summary>
  /// Represents the geometry of a dimension that displays a coordinate of a point.
  /// <para>This class refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class OrdinateDimension : AnnotationBase, ISerializable
  {
    internal OrdinateDimension(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected OrdinateDimension(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
  }

  /// <summary>
  /// Represents text geometry.
  /// <para>This class refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class TextEntity : AnnotationBase, ISerializable
  {
    internal TextEntity(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected TextEntity(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new TextEntity(IntPtr.Zero, null);
    }

    /// <summary>
    /// Gets or sets the index of font in document font table used by the text.
    /// </summary>
    public int FontIndex
    {
      get
      {
        IntPtr pThis = ConstPointer();
        return UnsafeNativeMethods.ON_Annotation2_Index(pThis, false, 0);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_Annotation2_Index(pThis, true, value);
      }
    }

#if RHINO_SDK
    public Curve[] Explode()
    {
      IntPtr pConstThis = ConstPointer();
      Runtime.InteropWrappers.SimpleArrayCurvePointer curves = new Runtime.InteropWrappers.SimpleArrayCurvePointer();
      IntPtr pCurves = curves.NonConstPointer();
      UnsafeNativeMethods.ON_TextEntity_Explode(pConstThis, pCurves);
      return curves.ToNonConstArray();
    }
#endif
  }

  /// <summary>
  /// Represents a leader, or an annotation entity with text that resembles an arrow pointing to something.
  /// <para>This class refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class Leader : AnnotationBase, ISerializable
  {
    internal Leader(IntPtr native_pointer, object parent)
      : base(native_pointer, parent)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected Leader(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }
  }

  /// <summary>
  /// Represents a text dot, or an annotation entity with text that always faces the camera and always has the same size.
  /// <para>This class refers to the geometric element that is independent from the document.</para>
  /// </summary>
  [Serializable]
  public class TextDot : GeometryBase, ISerializable
  {
    internal TextDot(IntPtr native_pointer, object parent)
      :base(native_pointer, parent, -1)
    { }

    /// <summary>
    /// Protected constructor used in serialization.
    /// </summary>
    protected TextDot(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new TextDot(IntPtr.Zero, null);
    }

    /// <summary>
    /// Initializes a new textdot based on the text and the location.
    /// </summary>
    /// <param name="text">Text</param>
    /// <param name="location">A position</param>
    public TextDot(string text, Point3d location)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_TextDot_New(text, location);
      ConstructNonConstObject(ptr);
    }
    
    /// <summary>
    /// Gets or sets the position of the textdot.
    /// </summary>
    public Point3d Point
    {
      get
      {
        Point3d rc = new Point3d();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetPoint(ptr, false, ref rc);
        return rc;
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetPoint(ptr, true, ref value);
      }
    }

    /// <summary>
    /// Gets or sets the text of the textdot.
    /// </summary>
    public string Text
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_TextDot_GetSetText(ptr, false, null, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_TextDot_GetSetText(ptr, true, value, IntPtr.Zero);
      }
    }

    // Do not wrap height, fontface and display. These are not currently used in Rhino
  }
}
