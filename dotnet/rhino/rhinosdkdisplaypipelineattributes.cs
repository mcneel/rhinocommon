#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
//using System.Security.Permissions;
using Rhino.Drawing;

#if RHINO_SDK
namespace Rhino.Display
{
  /// <summary>
  /// Represents display pipeline settings, such as "show transparency" and "show grips".
  /// </summary>
  //[Serializable]
  public class DisplayPipelineAttributes : IDisposable, ISerializable
  {
    #region pointer tracking
    private object m_parent;
    internal IntPtr m_pAttrs = IntPtr.Zero;

    internal DisplayPipelineAttributes(IntPtr pAttrs)
    {
      m_pAttrs = pAttrs;
    }

    internal DisplayPipelineAttributes(DisplayModeDescription parent)
    {
      m_parent = parent;
    }

    internal DisplayPipelineAttributes(DisplayPipeline parent)
    {
      m_parent = parent;
    }

    internal IntPtr ConstPointer()
    {
      if (m_pAttrs != IntPtr.Zero)
        return m_pAttrs;

      // Check pipeline_parent first since this is typically time critical
      // code when this is used.
      DisplayPipeline pipeline_parent = m_parent as DisplayPipeline;
      if (pipeline_parent != null)
        return pipeline_parent.DisplayAttributeConstPointer();

      DisplayModeDescription parent = m_parent as DisplayModeDescription;
      if (parent != null)
        return parent.DisplayAttributeConstPointer();
      return IntPtr.Zero;
    }

    internal IntPtr NonConstPointer()
    {
      if (m_pAttrs != IntPtr.Zero)
        return m_pAttrs;

      // Check pipeline_parent first since this is typically time critical
      // code when this is used.
      DisplayPipeline pipeline_parent = m_parent as DisplayPipeline;
      if (pipeline_parent != null)
      {
        // Can't change the attributes in a pipeline, so create a copy
        // under the hood
        IntPtr pConstAttributes = ConstPointer();
        if (pConstAttributes != IntPtr.Zero)
        {
          m_pAttrs = UnsafeNativeMethods.CDisplayPipelineAttributes_New2(pConstAttributes);
          m_parent = null;
          return m_pAttrs;
        }
      }

      DisplayModeDescription parent = m_parent as DisplayModeDescription;
      if (parent != null)
        return parent.DisplayAttributeNonConstPointer();
      return IntPtr.Zero;
    }

    internal void CopyContents(DisplayPipelineAttributes other)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pOther = other.ConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_CopyContents(pThis, pOther);
    }
    #endregion
 
    protected DisplayPipelineAttributes(SerializationInfo info, StreamingContext context)
    {
      m_parent = null;
      m_pAttrs = UnsafeNativeMethods.CDisplayPipelineAttributes_New();
      IntPtr pRhinoProfileContext = Rhino.Runtime.HostUtils.ReadIntoProfileContext(info, "DisplayPipelineAttributes");
      UnsafeNativeMethods.CDisplayPipelineAttributes_LoadProfile(m_pAttrs, pRhinoProfileContext, "DisplayPipelineAttributes");
      UnsafeNativeMethods.CRhinoProfileContext_Delete(pRhinoProfileContext);
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pProfileContext = UnsafeNativeMethods.CRhCmnProfileContext_New();
      UnsafeNativeMethods.CDisplayPipelineAttributes_SaveProfile(pConstThis, pProfileContext, "DisplayPipelineAttributes");
      Rhino.Runtime.HostUtils.WriteIntoSerializationInfo(pProfileContext, info, "DisplayPipelineAttributes");
      UnsafeNativeMethods.CRhinoProfileContext_Delete(pProfileContext);
    }

    ~DisplayPipelineAttributes()
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
      if (m_pAttrs!=IntPtr.Zero)
      {
        UnsafeNativeMethods.CDisplayPipelineAttributes_Delete(m_pAttrs);
      }
      m_pAttrs = IntPtr.Zero;
    }


    #region General display overrides...
    public bool XrayAllObjects
    {
      get { return GetBool(idx_bXrayAllOjbjects); }
      set { SetBool(idx_bXrayAllOjbjects, value); }
    }

    public bool IgnoreHighlights
    {
      get { return GetBool(idx_bIgnoreHighlights); }
      set { SetBool(idx_bIgnoreHighlights, value); }
    }

    public bool DisableConduits
    {
      get { return GetBool(idx_bDisableConduits); }
      set { SetBool(idx_bDisableConduits, value); }
    }

    public bool DisableTransparency
    {
      get { return GetBool(idx_bDisableTransparency); }
      set { SetBool(idx_bDisableTransparency, value); }
    }
    #endregion

    #region General dynamic/runtime object drawing attributes
    public Color ObjectColor
    {
      get { return GetColor(idx_ObjectColor); }
      set { SetColor(idx_ObjectColor, value); }
    }

    public bool ShowGrips
    {
      get { return GetBool(idx_bShowGrips); }
      set { SetBool(idx_bShowGrips, value); }
    }
    #endregion

    #region View specific attributes...
    public sealed class ViewDisplayAttributes
    {
      readonly DisplayPipelineAttributes m_parent;
      internal ViewDisplayAttributes(DisplayPipelineAttributes parent)
      {
        m_parent = parent;
      }

      //bool m_bUseDefaultGrid; <-skipped. Not used in Rhino

      public bool UseDocumentGrid
      {
        get { return m_parent.GetBool(idx_bUseDocumentGrid); }
        set { m_parent.SetBool(idx_bUseDocumentGrid, value); }
      }

      public bool DrawGrid
      {
        get { return m_parent.GetBool(idx_bDrawGrid); }
        set { m_parent.SetBool(idx_bDrawGrid, value); }
      }

      public bool DrawGridAxes
      {
        get { return m_parent.GetBool(idx_bDrawAxes); }
        set { m_parent.SetBool(idx_bDrawAxes, value); }
      }

      public bool DrawZAxis
      {
        get { return m_parent.GetBool(idx_bDrawZAxis); }
        set { m_parent.SetBool(idx_bDrawZAxis, value); }
      }

      public bool DrawWorldAxes
      {
        get { return m_parent.GetBool(idx_bDrawWorldAxes); }
        set { m_parent.SetBool(idx_bDrawWorldAxes, value); }
      }

      public bool ShowGridOnTop
      {
        get { return m_parent.GetBool(idx_bShowGridOnTop); }
        set { m_parent.SetBool(idx_bShowGridOnTop, value); }
      }
      //bool m_bShowTransGrid;

      public bool BlendGrid
      {
        get { return m_parent.GetBool(idx_bBlendGrid); }
        set { m_parent.SetBool(idx_bBlendGrid, value); }
      }

      //int m_nGridTrans;
      public bool DrawTransparentGridPlane
      {
        get { return m_parent.GetBool(idx_bDrawTransGridPlane); }
        set { m_parent.SetBool(idx_bDrawTransGridPlane, value); }
      }

      //int                   m_nGridPlaneTrans;
      //int                   m_nAxesPercentage;
      //bool                  m_bPlaneUsesGridColor;
      //COLORREF              m_GridPlaneColor;
      //int                   m_nPlaneVisibility;
      //int                   m_nWorldAxesColor;
      public Color WorldAxisColorX
      {
        get { return m_parent.GetColor(idx_WxColor); }
        set { m_parent.SetColor(idx_WxColor, value); }
      }
      public Color WorldAxisColorY
      {
        get { return m_parent.GetColor(idx_WyColor); }
        set { m_parent.SetColor(idx_WyColor, value); }
      }
      public Color WorldAxisColorZ
      {
        get { return m_parent.GetColor(idx_WzColor); }
        set { m_parent.SetColor(idx_WzColor, value); }
      }

      //bool                  m_bUseDefaultBg;
      //EFrameBufferFillMode  m_eFillMode;
      //COLORREF              m_SolidColor;
      //ON_wString            m_sBgBitmap;
      //const CRhinoUiDib*    m_pBgBitmap;
      //COLORREF              m_GradTopLeft;
      //COLORREF              m_GradBotLeft;
      //COLORREF              m_GradTopRight;
      //COLORREF              m_GradBotRight;
      //int                   m_nStereoModeEnabled;
      //float                 m_fStereoSeparation;
      //float                 m_fStereoParallax;
      //int                   m_nAGColorMode;
      //int                   m_nAGViewingMode;


      //bool                  m_bUseDefaultScale; <-skipped. Not used in Rhino

      public double HorizontalViewportScale
      {
        get { return m_parent.GetDouble(idx_dHorzScale); }
        set { m_parent.SetDouble(idx_dHorzScale, value); }
      }
      public double VerticalViewportScale
      {
        get { return m_parent.GetDouble(idx_dVertScale); }
        set { m_parent.SetDouble(idx_dVertScale, value); }
      }

      //bool                  m_bUseLineSmoothing;
      //bool                  LoadBackgroundBitmap(const ON_wString&);
      //int                   m_eAAMode;
      //bool                  m_bFlipGlasses;
    }

    ViewDisplayAttributes m_ViewSpecificAttributes;
    public ViewDisplayAttributes ViewSpecificAttributes
    {
      get { return m_ViewSpecificAttributes ?? (m_ViewSpecificAttributes = new ViewDisplayAttributes(this)); }
    }
    #endregion

    #region bool
    const int idx_bXrayAllOjbjects = 0;
    const int idx_bIgnoreHighlights = 1;
    const int idx_bDisableConduits = 2;
    const int idx_bDisableTransparency = 3;
    const int idx_bShowGrips = 4;
    const int idx_bUseDocumentGrid = 5;
    const int idx_bDrawGrid = 6;
    const int idx_bDrawAxes = 7;
    const int idx_bDrawZAxis = 8;
    const int idx_bDrawWorldAxes = 9;
    const int idx_bShowGridOnTop = 10;
    //const int idx_bShowTransGrid = 11;
    const int idx_bBlendGrid = 12;
    const int idx_bDrawTransGridPlane = 13;
    const int idx_bHighlightMeshes = 14;
    const int idx_bSingleMeshWireColor = 15;
    const int idx_bShowMeshWires = 16;
    const int idx_bShowMeshVertices = 17;
    const int idx_bShowCurves = 18;
    const int idx_bShadeSurface = 19;
    const int idx_bLockedObjectsBehind = 20;

    // skipped these because I don't see them actually used in Rhino
    //bool m_bIsSurface;
    //bool m_bUseDefaultGrid;
    bool GetBool(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetBool(pConstThis, which, false, false);
    }
    void SetBool(int which, bool b)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetBool(pThis, which, true, b);
    }
    #endregion

    #region color
    //int                 m_IsHighlighted;
    const int idx_ObjectColor = 0;
    const int idx_WxColor = 1;
    const int idx_WyColor = 2;
    const int idx_WzColor = 3;
    const int idx_MeshWireColor = 4;
    const int idx_CurveColor = 5;
    //int                 m_nLineThickness;
    //UINT                m_nLinePattern;

    Color GetColor(int which)
    {
      IntPtr pConstThis = ConstPointer();
      int argb = UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetColor(pConstThis, which, false, 0);
      return Rhino.Drawing.Color.FromArgb(argb);
    }
    void SetColor(int which, Color c)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetColor(pThis, which, true, c.ToArgb());
    }
    #endregion

    #region double
    const int idx_dHorzScale = 0;
    const int idx_dVertScale = 1;

    double GetDouble(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetDouble(pConstThis, which, false, 0);
    }
    void SetDouble(int which, double d)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetDouble(pThis, which, true, d);
    }
    #endregion

    #region int
    const int idx_nMeshWireThickness = 0;
    const int idx_nCurveThickness = 1;
    int GetInt(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetInt(pConstThis, which, false, 0);
    }
    void SetInt(int which, int i)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetInt(pThis, which, true, i);
    }
    #endregion


    #region Curves specific attributes...
    /// <summary>Draw curves</summary>
    public bool ShowCurves
    {
      get { return GetBool(idx_bShowCurves); }
      set { SetBool(idx_bShowCurves, value); }
    }

    //bool m_bUseDefaultCurve; -- doesn't appear to be used in display pipelane
    /// <summary>Pixel thickness for curves</summary>
    public int CurveThickness
    {
      get { return GetInt(idx_nCurveThickness); }
      set { SetInt(idx_nCurveThickness, (int)value); }
    }

    //UINT m_nCurvePattern;
    //bool m_bCurveKappaHair;
    //bool m_bSingleCurveColor;
    /// <summary>Color used for drawing curves</summary>
    public Rhino.Drawing.Color CurveColor
    {
      get { return GetColor(idx_CurveColor); }
      set { SetColor(idx_CurveColor, value); }
    }
    //ELineEndCapStyle m_eLineEndCapStyle;
    //ELineJoinStyle m_eLineJoinStyle;
    #endregion

    #region Both surface and mesh specific attributes...
    //bool m_bUseDefaultShading;

    /// <summary>Draw shaded meshes and surfaces</summary>
    public bool ShadingEnabled
    {
      get { return GetBool(idx_bShadeSurface); }
      set { SetBool(idx_bShadeSurface, value); }
    }

    //bool m_bUseObjectMaterial;
    //bool m_bSingleWireColor;
    //COLORREF m_WireColor;
    //COLORREF m_ShadedColor;
    //bool m_bUseDefaultBackface;
    //bool m_bUseObjectBFMaterial;
    //bool m_bCullBackfaces;
    #endregion

    #region Surfaces specific attributes...
    //bool m_bUseDefaultSurface;
    //bool m_bSurfaceKappaHair;
    //bool m_bHighlightSurfaces;
    // iso's...
    //bool m_bUseDefaultIso;
    //bool m_bShowIsocurves;
    //bool m_bIsoThicknessUsed;
    //int m_nIsocurveThickness;
    //int m_nIsoUThickness;
    //int m_nIsoVThickness;
    //int m_nIsoWThickness;
    //bool m_bSingleIsoColor;
    //COLORREF m_IsoColor;
    //bool m_bIsoColorsUsed;
    //COLORREF m_IsoUColor;
    //COLORREF m_IsoVColor;
    //COLORREF m_IsoWColor;
    //bool m_bIsoPatternUsed;
    //UINT m_nIsocurvePattern;
    //UINT m_nIsoUPattern;
    //UINT m_nIsoVPattern;
    //UINT m_nIsoWPattern;
    // edges....
    //bool m_bUseDefaultEdges;
    //bool m_bShowEdges;
    //bool m_bShowNakedEdges;
    //bool m_bShowEdgeEndpoints;
    //int m_nEdgeThickness;
    //int m_nEdgeColorUsage;
    //int m_nNakedEdgeOverride;
    //int m_nNakedEdgeColorUsage;
    //int m_nNakedEdgeThickness;
    //int m_nEdgeColorReduction;
    //int m_nNakedEdgeColorReduction;
    //COLORREF m_EdgeColor;
    //COLORREF m_NakedEdgeColor;
    //UINT m_nEdgePattern;
    //UINT m_nNakedEdgePattern;
    //UINT m_nNonmanifoldEdgePattern;
    #endregion

    #region Object locking attributes...
    //bool m_bUseDefaultObjectLocking;
    //int m_nLockedUsage;
    //bool m_bGhostLockedObjects;
    //int m_nLockedTrans;
    //COLORREF m_LockedColor;
    /*
    /// <summary>
    /// If Color.Unset, then a specific lock color is NOT used
    /// </summary>
    public Rhino.Drawing.Color UseSpecificLockColor
    {
      get
      {
      }
      set
      {
      }
    }
    */

    /// <summary>Locked object are drawn behind other objects</summary>
    public bool LockedObjectsDrawBehindOthers
    {
      get { return GetBool(idx_bLockedObjectsBehind); }
      set { SetBool(idx_bLockedObjectsBehind, value); }
    }
    #endregion

    #region Meshes specific attributes...
    public sealed class MeshDisplayAttributes
    {
      readonly DisplayPipelineAttributes m_parent;
      internal MeshDisplayAttributes(DisplayPipelineAttributes parent)
      {
        m_parent = parent;
      }

      //bool m_bUseDefaultMesh; <-skipped. Not used in Rhino
      public bool HighlightMeshes
      {
        get { return m_parent.GetBool(idx_bHighlightMeshes); }
        set { m_parent.SetBool(idx_bHighlightMeshes, value); }
      }

      /// <summary>
      /// Color.Empty means that we are NOT using a single color for all mesh wires.
      /// </summary>
      public Color AllMeshWiresColor
      {
        // This is a combination of
        //bool m_bSingleMeshWireColor;
        //COLORREF m_MeshWireColor;
        get
        {
          if( m_parent.GetBool(idx_bSingleMeshWireColor) )
            return m_parent.GetColor(idx_MeshWireColor);
          return Color.Empty;
        }
        set
        {
          bool single_color = (value!=Color.Empty);
          m_parent.SetBool(idx_bSingleMeshWireColor, single_color);
          if( single_color )
            m_parent.SetColor(idx_MeshWireColor, value);
        }
      }

      public int MeshWireThickness
      {
        get { return m_parent.GetInt(idx_nMeshWireThickness); }
        set { m_parent.SetInt(idx_nMeshWireThickness, value); }
      }

      //UINT m_nMeshWirePattern;

      public bool ShowMeshWires
      {
        get { return m_parent.GetBool(idx_bShowMeshWires); }
        set { m_parent.SetBool(idx_bShowMeshWires, value); }
      }
      public bool ShowMeshVertices
      {
        get { return m_parent.GetBool(idx_bShowMeshVertices); }
        set { m_parent.SetBool(idx_bShowMeshVertices, value); }
      }

      //int m_nVertexCountTolerance;
      //ERhinoPointStyle m_eMeshVertexStyle;

      //int m_nMeshVertexSize;

      //bool m_bUseDefaultMeshEdges; // obsolete - not used in display code
      //bool m_bShowMeshEdges;            // here "Edge" means "break" or "crease" edges
      //bool m_bShowMeshNakedEdges;       // "Naked" means boundary edges
      //bool m_bShowMeshNonmanifoldEdges; // "Nonmanifold means 3 or more faces meet at the edge
      //int m_nMeshEdgeThickness;        // here "Edge" means "break" or "crease" edges
      //bool m_bMeshNakedEdgeOverride; // obsolete - not used in display code
      //int m_nMeshNakedEdgeThickness;
      //int m_nMeshEdgeColorReduction;   // here "Edge" means "break" or "crease" edges
      //int m_nMeshNakedEdgeColorReduction;
      //COLORREF m_MeshEdgeColor;             // here "Edge" means "break" or "crease" edges
      //COLORREF m_MeshNakedEdgeColor;
      //int m_nMeshNonmanifoldEdgeThickness;
      //int m_nMeshNonmanifoldEdgeColorReduction;
      //COLORREF m_MeshNonmanifoldEdgeColor;
    }

    MeshDisplayAttributes m_MeshSpecificAttributes;
    public MeshDisplayAttributes MeshSpecificAttributes
    {
      get { return m_MeshSpecificAttributes ?? (m_MeshSpecificAttributes = new MeshDisplayAttributes(this)); }
    }

    #endregion

    #region Dimensions & Text specific attributes...
    //bool m_bUseDefaultText;
    //bool m_bShowText;
    //bool m_bShowAnnotations;
    //COLORREF m_DotTextColor;
    //COLORREF m_DotBorderColor;
    #endregion

    #region Lights & Lighting specific attributes...
    //bool m_bUseDefaultLights;
    //bool m_bShowLights;
    //bool m_bUseHiddenLights;
    //bool m_bUseLightColor;
    //bool m_bUseDefaultLightingScheme;
    //ELightingScheme m_eLightingScheme;
    //bool m_bUseDefaultEnvironment;
    //int m_nLuminosity;
    //COLORREF m_AmbientColor;
    //ON_ObjectArray<ON_Light> m_Lights;
    //int m_eShadowMapType;
    //int m_nShadowMapSize;
    //int m_nNumSamples;
    //COLORREF m_ShadowColor;
    //ON_3dVector m_ShadowBias;
    //double m_fShadowBlur;
    //bool m_bCastShadows;
    //BYTE m_nShadowBitDepth;
    //BYTE m_nTransparencyTolerance;
    //bool m_bPerPixelLighting;
    #endregion

    #region Points specific attributes...
    //bool m_bUseDefaultPoints;
    //int m_nPointSize;
    //ERhinoPointStyle m_ePointStyle;
    #endregion

    #region Control Polygon specific attributes...
    //bool m_bUseDefaultControlPolygon;
    //bool m_bCPSolidLines;
    //bool m_bCPSingleColor;
    //bool m_bCPHidePoints;
    //bool m_bCPHideSurface;
    //bool m_bCPHighlight;
    //bool m_bCPHidden;
    //int m_nCPWireThickness;
    //int m_nCVSize;
    //ERhinoPointStyle m_eCVStyle;
    //ON_Color m_CPColor;
    #endregion

    #region PointClouds specific attributes...
    //bool m_bUseDefaultPointCloud;
    //int m_nPCSize;
    //ERhinoPointStyle m_ePCStyle;
    //int m_nPCGripSize;
    //ERhinoPointStyle m_ePCGripStyle;
    #endregion

/* 
public:
  ON_MeshParameters*  m_pMeshSettings;
  CDisplayPipelineMaterial*  m_pMaterial;
  // experimental
  bool                m_bDegradeIsoDensity;
  bool                m_bLayersFollowLockUsage;
*/

    public Guid Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CDisplayPipelineAttributes_GetId(pConstThis);
      }
    }

    public string EnglishName
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pName = UnsafeNativeMethods.CDisplayPipelineAttributes_GetName(pConstThis, true);
        return Marshal.PtrToStringUni(pName);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CDisplayPipelineAttributes_SetEnglishName(pThis, value);
      }
    }

    public string LocalName
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pName = UnsafeNativeMethods.CDisplayPipelineAttributes_GetName(pConstThis, false);
        return Marshal.PtrToStringUni(pName);
      }
    }
  }
}
#endif