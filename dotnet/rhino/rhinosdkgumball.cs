#pragma warning disable 1591
using System;
using Rhino.Geometry;

namespace Rhino.UI.Gumball
{
#if RHINO_SDK
  /// <summary>
  /// Transformation modes for gumballs.
  /// </summary>
  public enum GumballMode : int
  {
    None = 0, //gb_mode_nothing

    /// <summary>gumball menu button was picked.</summary>
    Menu  = 1, //gb_mode_menu

    /// <summary>Unconstrained translation.</summary>
    TranslateFree = 2, //gb_mode_translatefree

    /// <summary>Translation along a single axis.</summary>
    TranslateX = 3, //gb_mode_translatex
    /// <summary>Translation along a single axis.</summary>
    TranslateY = 4, //gb_mode_translatey
    /// <summary>Translation along a single axis.</summary>
    TranslateZ = 5, //gb_mode_translatez

    /// <summary>Translation in a plane.</summary>
    TranslateXY = 6, //gb_mode_translatexy
    /// <summary>Translation in a plane.</summary>
    TranslateYZ = 7, //gb_mode_translateyz
    /// <summary>Translation in a plane.</summary>
    TranslateZX = 8, //gb_mode_translatezx

    /// <summary>
    /// GumballFrame.ScaleMode is used to enable multi-axis scaling.  This
    /// indicates which control is being dragged.
    /// </summary>
    ScaleX = 9, //gb_mode_scalex
    /// <summary>
    /// GumballFrame.ScaleMode is used to enable multi-axis scaling.  This
    /// indicates which control is being dragged.
    /// </summary>
    ScaleY = 10, //gb_mode_scaley
    /// <summary>
    /// GumballFrame.ScaleMode is used to enable multi-axis scaling.  This
    /// indicates which control is being dragged.
    /// </summary>
    ScaleZ = 11, //gb_mode_scalez

    RotateX = 12, //gb_mode_rotatex
    RotateY = 13, //gb_mode_rotatey
    RotateZ = 14 //gb_mode_rotatez
  }

  public class GumballObject : IDisposable
  {
    public GumballObject()
    {
      m_ptr_gumball = UnsafeNativeMethods.CRhinoGumball_New(IntPtr.Zero);
    }

    internal GumballObject(GumballDisplayConduit parent, bool baseGumball)
    {
      m_parent = parent;
      m_base_gumball = baseGumball;
      m_ptr_gumball = IntPtr.Zero;
    }

    #region IDisposable/Pointer handling
    GumballDisplayConduit m_parent;
    readonly bool m_base_gumball;
    IntPtr m_ptr_gumball;
    internal IntPtr ConstPointer()
    {
      if (m_parent != null)
      {
        IntPtr pConstParent = m_parent.ConstPointer();
        return UnsafeNativeMethods.CRhinoGumballDisplayConduit_GetGumball(pConstParent, m_base_gumball);
      }
      return m_ptr_gumball;
    }
    internal IntPtr NonConstPointer()
    {
      if (m_parent != null)
      {
        IntPtr pConstThis = ConstPointer();
        m_ptr_gumball = UnsafeNativeMethods.CRhinoGumball_New(pConstThis);
        if (m_parent.m_base_gumball == this)
          m_parent.m_base_gumball = null;
        m_parent = null;
      }
      return m_ptr_gumball;
    }

    ~GumballObject()
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
      if (m_ptr_gumball != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoGumball_Delete(m_ptr_gumball);
      }
      m_ptr_gumball = IntPtr.Zero;
    }
    #endregion

    //bool SetCenter(const ON_3dPoint& center);

    public bool SetFromBoundingBox(BoundingBox boundingBox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromBoundingBox(pThis, boundingBox.Min, boundingBox.Max);
    }

    /// <summary>
    /// Sets the gumball bounding box with respect to a frame.
    /// </summary>
    /// <param name="frame">The frame.</param>
    /// <param name="frameBoundingBox">Bounding box with respect to frame.</param>
    /// <returns>
    /// true if input is valid and gumball is set. false if input is not valid.
    /// In this case, gumball is set to the default.
    /// </returns>
    public bool SetFromBoundingBox(Plane frame, BoundingBox frameBoundingBox)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromBoundingBox2(pThis, ref frame, frameBoundingBox.Min, frameBoundingBox.Max);
    }

    public bool SetFromLine(Line line)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromLine(pThis, line.From, line.To);
    }

    public bool SetFromPlane(Plane plane)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromPlane(pThis, ref plane);
    }

    public bool SetFromArc(Arc arc)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromArc(pThis, ref arc);
    }

    public bool SetFromCircle(Circle circle)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromCircle(pThis, ref circle);
    }

    public bool SetFromEllipse(Ellipse ellipse)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromEllipse(pThis, ref ellipse);
    }

    public bool SetFromLight(Light light)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstLight = light.ConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromLight(pThis, pConstLight);
    }

    public bool SetFromHatch(Hatch hatch)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstHatch = hatch.ConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromHatch(pThis, pConstHatch);
    }

    public bool SetFromCurve(Curve curve)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstCurve = curve.ConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromCurve(pThis, pConstCurve);
    }

    public bool SetFromExtrusion(Extrusion extrusion)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstExtrusion = extrusion.ConstPointer();
      return UnsafeNativeMethods.CRhinoGumball_SetFromExtrusion(pThis, pConstExtrusion);
    }

    public GumballFrame Frame
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Plane pl = new Plane();
        Vector3d sgd = new Vector3d();
        int mode = 0;
        UnsafeNativeMethods.CRhinoGumball_GetFrame(pConstThis, ref pl, ref sgd, ref mode);
        return new GumballFrame(pl, sgd, (GumballScaleMode)mode);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        Plane pl = value.Plane;
        UnsafeNativeMethods.CRhinoGumball_SetFrame(pThis, ref pl, value.ScaleGripDistance, (int)value.ScaleMode);
      }
    }
  }

  public class GumballAppearanceSettings
  {
    public GumballAppearanceSettings()
    {
      IntPtr pGumballAppearance = UnsafeNativeMethods.CRhinoGumballAppearance_New();
      RelocateEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableRelocate);
      MenuEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableMenu);
      TranslateXEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXTranslate);
      TranslateYEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYTranslate);
      TranslateZEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZTranslate);
      TranslateXYEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXYTranslate);
      TranslateYZEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYZTranslate);
      TranslateZXEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZXTranslate);
      RotateXEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXRotate);
      RotateYEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYRotate);
      RotateZEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZRotate);
      ScaleXEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXScale);
      ScaleYEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYScale);
      ScaleZEnabled = UnsafeNativeMethods.CRhinoGumballAppearance_GetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZScale);
      FreeTranslate = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.EnableFreeTranslate);
      ColorX = Rhino.Runtime.Interop.ColorFromWin32(UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Xcolor));
      ColorY = Rhino.Runtime.Interop.ColorFromWin32(UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Ycolor));
      ColorZ = Rhino.Runtime.Interop.ColorFromWin32(UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Zcolor));
      ColorMenuButton = Rhino.Runtime.Interop.ColorFromWin32(UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menubuttoncolor));
      Radius = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_radius);
      ArrowHeadLength = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tip_length);
      ArrowHeadWidth = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tip_width);
      ScaleGripSize = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tail_size);
      PlanarTranslationGripCorner = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_ptran_dist);
      PlanarTranslationGripSize = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_ptran_size);
      AxisThickness = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Axis_thickness);
      ArcThickness = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Arc_thickness);
      MenuDistance = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menu_dist);
      MenuSize = UnsafeNativeMethods.CRhinoGumballAppearance_GetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menu_size);
      UnsafeNativeMethods.CRhinoGumballAppearance_Delete(pGumballAppearance);
    }

    internal IntPtr CreatePointer()
    {
      IntPtr pGumballAppearance = UnsafeNativeMethods.CRhinoGumballAppearance_New();
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableRelocate, RelocateEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableMenu, MenuEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXTranslate, TranslateXEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYTranslate, TranslateYEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZTranslate, TranslateZEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXYTranslate, TranslateXYEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYZTranslate, TranslateYZEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZXTranslate, TranslateZXEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXRotate, RotateXEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYRotate, RotateYEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZRotate, RotateZEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableXScale, ScaleXEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableYScale, ScaleYEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetBool(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceBools.EnableZScale, ScaleZEnabled);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.EnableFreeTranslate, FreeTranslate);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Xcolor, ColorX.ToArgb());
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Ycolor, ColorY.ToArgb());
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Zcolor, ColorZ.ToArgb());
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menubuttoncolor, ColorMenuButton.ToArgb());
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_radius, Radius);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tip_length, ArrowHeadLength);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tip_width, ArrowHeadWidth);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_tail_size, ScaleGripSize);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_ptran_dist, PlanarTranslationGripCorner);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Gumball_ptran_size, PlanarTranslationGripSize);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Axis_thickness, AxisThickness);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Arc_thickness, ArcThickness);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menu_dist, MenuDistance);
      UnsafeNativeMethods.CRhinoGumballAppearance_SetInt(pGumballAppearance, UnsafeNativeMethods.GumbalAppearanceInts.Menu_size, MenuSize);
      return pGumballAppearance;
    }

    #region bools
    /// <summary>
    /// When RelocateEnabled is true, the user can reposition the gumball by
    /// tapping the control key while dragging.  Once the repostion drag is
    /// terminated by releasing the/ mouse button, ordinary editing resumes.
    /// The default setting is true.
    /// </summary>
    public bool RelocateEnabled { get; set; }

    /// <summary>
    /// When MenuEnabled is true, the menu "button" is drawn on the gumball.
    /// The default setting is true.
    /// </summary>
    public bool MenuEnabled { get; set; }

    /// <summary>
    /// TranslateXEnabled is true, the X axis translation control is available.
    /// The default setting is true.
    /// </summary>
    public bool TranslateXEnabled { get; set; }
    /// <summary>
    /// TranslateYEnabled is true, the Y axis translation control is available.
    /// The default setting is true.
    /// </summary>
    public bool TranslateYEnabled { get; set; }
    /// <summary>
    /// TranslateZEnabled is true, the Z axis translation control is available.
    /// The default setting is true.
    /// </summary>
    public bool TranslateZEnabled { get; set; }

    /// <summary>
    /// When TranslateXY is true, the XY plane translation control is available
    /// in appropriate views. The default setting is true.
    /// </summary>
    public bool TranslateXYEnabled { get; set; }
    /// <summary>
    /// When TranslateYZ is true, the YZ plane translation control is available
    /// in appropriate views. The default setting is true.
    /// </summary>
    public bool TranslateYZEnabled { get; set; }
    /// <summary>
    /// When TranslateZX is true, the ZX plane translation control is available
    /// in appropriate views. The default setting is true.
    /// </summary>
    public bool TranslateZXEnabled { get; set; }

    /// <summary>
    /// When RotateX is true, the X rotation control is available. The default
    /// setting is true.
    /// </summary>
    public bool RotateXEnabled { get; set; }
    /// <summary>
    /// When RotateY is true, the Y rotation control is available. The default
    /// setting is true.
    /// </summary>
    public bool RotateYEnabled { get; set; }
    /// <summary>
    /// When RotateZ is true, the Z rotation control is available. The default
    /// setting is true.
    /// </summary>
    public bool RotateZEnabled { get; set; }

    /// <summary>
    /// When ScaleXEnabled is true, the X scale control is available. The
    /// default setting is true.
    /// </summary>
    public bool ScaleXEnabled { get; set; }
    /// <summary>
    /// When ScaleYEnabled is true, the Y scale control is available. The
    /// default setting is true.
    /// </summary>
    public bool ScaleYEnabled { get; set; }
    /// <summary>
    /// When ScaleZEnabled is true, the Z scale control is available. The
    /// default setting is true.
    /// </summary>
    public bool ScaleZEnabled { get; set; }
    #endregion
    // At the moment, all gumballs use the same app setting
    // CRhinoGumball::default_bSnappy.

    //// Snappy setting for this gumball. Initialized to
    //// CRhinoGumballAppearance::default_bSnappy;
    //bool m_bSnappy; // true = snappy, false = smooth setting 

  

    /// <summary>
    /// When FreeTranslate is 1, the center translation control can be dragged
    /// in any direction and moves the object the gumball controls. When
    /// FreeTranslate is 2, the center translation control can be dragged in any
    /// direction and moves the object the gumball itself. The default value is 2.
    /// </summary>
    public int FreeTranslate { get; set; }

    /// <summary>Default is Red.</summary>
    public System.Drawing.Color ColorX { get; set; }
    /// <summary>Default is Green.</summary>
    public System.Drawing.Color ColorY { get; set; }
    /// <summary>Default is Blue.</summary>
    public System.Drawing.Color ColorZ { get; set; }

    public System.Drawing.Color ColorMenuButton { get; set; }

    /// <summary>in pixels.</summary>
    public int Radius { get; set; }

    /// <summary>in pixels.</summary>
    public int ArrowHeadLength { get; set; }

    /// <summary>in pixels.</summary>
    public int ArrowHeadWidth { get; set; }

    /// <summary>in pixels.</summary>
    public int ScaleGripSize { get; set; }

    /// <summary>in pixels.</summary>
    public int PlanarTranslationGripCorner { get; set; }
    /// <summary>in pixels.</summary>
    public int PlanarTranslationGripSize { get; set; }
    /// <summary>in pixels.</summary>
    public int AxisThickness { get; set; }
    /// <summary>in pixels.</summary>
    public int ArcThickness { get; set; }
    /// <summary>Distance of menu ball from center.</summary>
    public int MenuDistance { get; set; }
    /// <summary>Radius of menu circle.</summary>
    public int MenuSize { get; set; }
  }

  public class GumballDisplayConduit : IDisposable
  {
    public GumballDisplayConduit()
    {
      m_pGumballDisplayConduit = UnsafeNativeMethods.CRhinoGumballDisplayConduit_New();
    }

    #region IDisposable/Pointer handling
    IntPtr m_pGumballDisplayConduit;
    internal IntPtr ConstPointer() { return m_pGumballDisplayConduit; }
    internal IntPtr NonConstPointer() { return m_pGumballDisplayConduit; }

    ~GumballDisplayConduit()
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
      if (m_pGumballDisplayConduit != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoGumballDisplayConduit_Delete(m_pGumballDisplayConduit);
      }
      m_pGumballDisplayConduit = IntPtr.Zero;
    }
    #endregion

    public bool Enabled
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoGumballDisplayConduit_Enabled(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGumballDisplayConduit_SetEnabled(pThis, value);
      }
    }

    public bool InRelocate
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoGumballDisplayConduit_InRelocate(pConstThis);
      }
    }

    const int idxPreTransform = 0;
    const int idxGumballTransform = 1;
    const int idxTotalTransform = 2;
    Transform GetTransform(int which)
    {
      IntPtr pConstThis = ConstPointer();
      Transform rc = new Transform();
      UnsafeNativeMethods.CRhinoGumballDisplayConduit_GetTransform(pConstThis, which, ref rc);
      return rc;
    }

    /// <summary>
    /// The pre-transform is a transformation that needs to be applied before
    /// the gumball transformation.
    /// </summary>
    public Transform PreTransform
    {
      get { return GetTransform(idxPreTransform); }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGumballDisplayConduit_SetPreTransform(pThis, ref value);
      }
    }

    /// <summary>
    /// The gumball transformation is the transformation calculated by comparing
    /// the current gumball to the starting BaseGumball.
    /// </summary>
    public Transform GumballTransform
    {
      get { return GetTransform(idxGumballTransform); }
    }

    /// <summary>
    /// The total transformation is GumballTransform * PreTransform.
    /// </summary>
    public Transform TotalTransform
    {
      get { return GetTransform(idxTotalTransform); }
    }

    internal GumballObject m_base_gumball;
    internal GumballObject m_gumball;
    /// <summary>Starting location.</summary>
    public GumballObject BaseGumball
    {
      get
      {
        return m_base_gumball ?? (m_base_gumball = new GumballObject(this, true));
      }
    }

    public GumballObject Gumball
    {
      get
      {
        return m_gumball ?? (m_gumball = new GumballObject(this, false));
      }
    }

    /// <summary>
    /// Contents of the gumball are copied to the base gumball of this class.
    /// </summary>
    /// <param name="gumball">The gumball source.</param>
    public void SetBaseGumball(GumballObject gumball)
    {
      SetBaseGumball(gumball, null);
    }

    /// <summary>
    /// Contents of the gumball are copied to the base gumball of this class.
    /// </summary>
    /// <param name="gumball">The gumball source.</param>
    /// <param name="appearanceSettings">The gumball appearance and behavior settings.</param>
    public void SetBaseGumball(GumballObject gumball, GumballAppearanceSettings appearanceSettings)
    {
      IntPtr pConstGumball = gumball.ConstPointer();
      IntPtr pThis = NonConstPointer();
      IntPtr pAppearanceSettings = IntPtr.Zero;
      if (appearanceSettings != null)
        pAppearanceSettings = appearanceSettings.CreatePointer();
      UnsafeNativeMethods.CRhinoGumballDisplayConduit_SetBaseGumball(pThis, pConstGumball, pAppearanceSettings);
    }

    public bool UpdateGumball(Point3d point, Line worldLine)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumballDisplayConduit_UpdateGumball(pThis, point, ref worldLine);
    }


    GumballPickResult m_pick_result;
    /// <summary>The inital mouse down event sets PickResult.</summary>
    public GumballPickResult PickResult
    {
      get
      {
        return m_pick_result ?? (m_pick_result = new GumballPickResult(this));
      }
    }

    public bool PickGumball(Rhino.Input.Custom.PickContext pickContext, Rhino.Input.Custom.GetPoint getPoint)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstPickContext = pickContext.ConstPointer();
      IntPtr pGetPoint = IntPtr.Zero;
      if( getPoint!=null)
        getPoint.NonConstPointer();
      return UnsafeNativeMethods.CRhinoGumballDisplayConduit_PickGumball(pThis, pConstPickContext, pGetPoint);
    }

    public void CheckShiftAndControlKeys()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoGumballDisplayConduit_CheckShiftAndCtrlKeys(pThis);
    }
  }

  public enum GumballScaleMode : int
  {
    Independent = 0,
    XY = 1,
    YZ = 2,
    ZX = 3,
    XYZ = 4
  }

  public struct GumballFrame
  {
    internal GumballFrame(Plane pl, Vector3d sgd, GumballScaleMode mode) : this()
    {
      Plane = pl;
      ScaleGripDistance = sgd;
      ScaleMode = mode;
    }

    public Plane Plane { get; set; }
    public Vector3d ScaleGripDistance { get; set; }
    public GumballScaleMode ScaleMode { get; set; }
  }

  public class GumballPickResult
  {
    readonly GumballDisplayConduit m_parent;
    internal GumballPickResult(GumballDisplayConduit parent)
    {
      m_parent = parent;
    }

    public void SetToDefault()
    {
      IntPtr pConduit = m_parent.NonConstPointer();
      UnsafeNativeMethods.CRhinoGumballPickResult_SetToDefault(pConduit);
    }

    public GumballMode Mode
    {
      get
      {
        IntPtr pConstConduit = m_parent.ConstPointer();
        int rc = UnsafeNativeMethods.CRhinoGumballPickResult_Mode(pConstConduit);
        return (GumballMode)rc;
      }
    }

  }
#endif
}

//public enum GumballPopupMenuItem { }
//public class GumballDrawSettings { }
//public class GumballPickResult { }
//public class GumballPopUpMenu { }
//public class GumballDragger { }

