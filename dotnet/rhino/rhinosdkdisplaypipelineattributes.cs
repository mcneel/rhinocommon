using System;
using System.Runtime.InteropServices;
using System.Drawing;

namespace Rhino.Display
{
  class DisplayPipelineAttributes
  {
    object m_parent;
    internal DisplayPipelineAttributes(DisplayModeDescription parent)
    {
      m_parent = parent;
    }

    IntPtr ConstPointer()
    {
      DisplayModeDescription parent = m_parent as DisplayModeDescription;
      if (parent != null)
        return parent.DisplayAttributeConstPointer();
      return IntPtr.Zero;
    }

    IntPtr NonConstPointer()
    {
      DisplayModeDescription parent = m_parent as DisplayModeDescription;
      if (parent != null)
        return parent.DisplayAttributeNonConstPointer();
      return IntPtr.Zero;
    }

    public string EnglishName
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pName = UnsafeNativeMethods.CDisplayPipelineAttributes_GetName(pConstThis,true);
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

    #region bool indices
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
    const int idx_bShowTransGrid = 11;
    const int idx_bBlendGrid = 12;
    const int idx_bDrawTransGridPlane = 13;

    // skipped these because I don't see them actually used in Rhino
    //bool m_bIsSurface;
    //bool m_bUseDefaultGrid;
    #endregion

    //const CRhinoObjectIterator* m_pIteratorOverride;
    //ON_UUID                     m_CurrentObjectId;
    //BYTE                m_BBoxMode;

    // General dynamic/runtime object drawing attributes...
    //int                 m_IsHighlighted;
    const int idx_ObjectColor = 0;
    const int idx_WxColor = 1;
    const int idx_WyColor = 2;
    const int idx_WzColor = 3;

    //int                 m_nLineThickness;
    //UINT                m_nLinePattern;

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

    Color GetColor(int which)
    {
      IntPtr pConstThis = ConstPointer();
      int argb = UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetColor(pConstThis, which, false, 0);
      return Color.FromArgb(argb);
    }
    void SetColor(int which, Color c)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CDisplayPipelineAttributes_GetSetColor(pThis, which, true, c.ToArgb());
    }

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

    public bool UseDocumentGrid
    {
      get { return GetBool(idx_bUseDocumentGrid); }
      set { SetBool(idx_bUseDocumentGrid, value); }
    }

    public bool DrawGrid
    {
      get { return GetBool(idx_bDrawGrid); }
      set { SetBool(idx_bDrawGrid, value); }
    }

    public bool DrawGridAxes
    {
      get { return GetBool(idx_bDrawAxes); }
      set { SetBool(idx_bDrawAxes, value); }
    }

    public bool DrawZAxis
    {
      get { return GetBool(idx_bDrawZAxis); }
      set { SetBool(idx_bDrawZAxis, value); }
    }

    public bool DrawWorldAxes
    {
      get { return GetBool(idx_bDrawWorldAxes); }
      set { SetBool(idx_bDrawWorldAxes, value); }
    }

    public bool ShowGridOnTop
    {
      get { return GetBool(idx_bShowGridOnTop); }
      set { SetBool(idx_bShowGridOnTop, value); }
    }

    public bool BlendGrid
    {
      get { return GetBool(idx_bBlendGrid); }
      set { SetBool(idx_bBlendGrid, value); }
    }

    public bool DrawTransparentGridPlane
    {
      get { return GetBool(idx_bDrawTransGridPlane); }
      set { SetBool(idx_bDrawTransGridPlane, value); }
    }

    public Color WorldAxisColorX
    {
      get { return GetColor(idx_WxColor); }
      set { SetColor(idx_WxColor, value); }
    }
    public Color WorldAxisColorY
    {
      get { return GetColor(idx_WyColor); }
      set { SetColor(idx_WyColor, value); }
    }
    public Color WorldAxisColorZ
    {
      get { return GetColor(idx_WzColor); }
      set { SetColor(idx_WzColor, value); }
    }
    /* 
 
/////////////////////////////////////
// View specific attributes...
public:  
  int                   m_nGridTrans;
  int                   m_nGridPlaneTrans;
  int                   m_nAxesPercentage;
  bool                  m_bPlaneUsesGridColor; // just expose the GridPlaneColor
  COLORREF              m_GridPlaneColor;
  int                   m_nPlaneVisibility;
  int                   m_nWorldAxesColor; // just expose colors
  
  bool                  m_bUseDefaultBg;
  EFrameBufferFillMode  m_eFillMode;
  COLORREF              m_SolidColor;

  ON_wString            m_sBgBitmap;
  const CRhinoUiDib*    m_pBgBitmap;
  
  COLORREF              m_GradTopLeft;
  COLORREF              m_GradBotLeft;
  COLORREF              m_GradTopRight;
  COLORREF              m_GradBotRight;

  int                   m_nStereoModeEnabled;
  float                 m_fStereoSeparation;
  float                 m_fStereoParallax;
  int                   m_nAGColorMode;
  int                   m_nAGViewingMode;
  
  bool                  m_bUseDefaultScale;
  double                m_dHorzScale;
  double                m_dVertScale;
 
  bool                  m_bUseLineSmoothing;
  
  bool                  LoadBackgroundBitmap(const ON_wString&);
  
  int                   m_eAAMode;
  
  bool                  m_bFlipGlasses;
  
  BYTE                m__pad01Views[127];
  
/////////////////////////////////////
// Curves specific attributes...
public:
  bool                m_bShowCurves;
  bool                m_bUseDefaultCurve;
  int                 m_nCurveThickness;
  int                 m_nCurveTrans;
  UINT                m_nCurvePattern;
  bool                m_bCurveKappaHair;
  bool                m_bSingleCurveColor;
  COLORREF            m_CurveColor;
  ELineEndCapStyle    m_eLineEndCapStyle;
  ELineJoinStyle      m_eLineJoinStyle;
 
  BYTE                m__pad01Curves[128];
    
/////////////////////////////////////
// Both surface and mesh specific attributes...
  bool                m_bUseDefaultShading;
  bool                m_bShadeSurface;
  
  bool                m_bUseObjectMaterial;
  bool                m_bSingleWireColor;
  COLORREF            m_WireColor;
  COLORREF            m_ShadedColor;

  bool                m_bUseDefaultBackface;  
  bool                m_bUseObjectBFMaterial;
  bool                m_bCullBackfaces;

  //UINT                m_nTechMask;
  
  BYTE                m__pad01SrfMsh[128];

/////////////////////////////////////
// Surfaces specific attributes...
public:
  bool                m_bUseDefaultSurface;
  
  bool                m_bSurfaceKappaHair;
  bool                m_bHighlightSurfaces;
  
  // iso's...
  bool                m_bUseDefaultIso;
  bool                m_bShowIsocurves;
  
  bool                m_bIsoThicknessUsed;
  int                 m_nIsocurveThickness;
  int                 m_nIsoUThickness;
  int                 m_nIsoVThickness;
  int                 m_nIsoWThickness;

  bool                m_bSingleIsoColor;
  COLORREF            m_IsoColor;
  bool                m_bIsoColorsUsed;
  COLORREF            m_IsoUColor;
  COLORREF            m_IsoVColor;
  COLORREF            m_IsoWColor;
  
  bool                m_bIsoPatternUsed;
  UINT                m_nIsocurvePattern;
  UINT                m_nIsoUPattern;
  UINT                m_nIsoVPattern;
  UINT                m_nIsoWPattern;
  
  BYTE                m__pad01Surfaces[128];

  // edges....
  bool                m_bUseDefaultEdges;
  bool                m_bShowEdges;
  bool                m_bShowNakedEdges;
  bool                m_bShowEdgeEndpoints;
  
  int                 m_nEdgeThickness;
  int                 m_nEdgeColorUsage;
  int                 m_nNakedEdgeOverride;
  int                 m_nNakedEdgeColorUsage;
  int                 m_nNakedEdgeThickness;
  int                 m_nEdgeColorReduction;
  int                 m_nNakedEdgeColorReduction;
  
  COLORREF            m_EdgeColor;
  COLORREF            m_NakedEdgeColor;

  UINT                m_nEdgePattern;
  UINT                m_nNakedEdgePattern;
  UINT                m_nNonmanifoldEdgePattern;
  
  BYTE                m__pad01Edges[124];

  
/////////////////////////////////////
// Object locking attributes....
  bool                m_bUseDefaultObjectLocking;
  int                 m_nLockedUsage;
  bool                m_bGhostLockedObjects;
  int                 m_nLockedTrans;
  COLORREF            m_LockedColor;
  bool                m_bLockedObjectsBehind;
  
/////////////////////////////////////
// Misc....
  ON_MeshParameters*  m_pMeshSettings;
  
  CDisplayPipelineMaterial*  m_pMaterial;
  
  // experimental
  bool                m_bDegradeIsoDensity;



  bool                m_bLayersFollowLockUsage;
  
  BYTE                m__pad01Misc[127];
  
/////////////////////////////////////
// Meshes specific attributes...
public:
  bool                m_bUseDefaultMesh;
  bool                m_bHighlightMeshes;
  bool                m_bSingleMeshWireColor;
  COLORREF            m_MeshWireColor;
  int                 m_nMeshWireThickness;
  UINT                m_nMeshWirePattern;
  bool                m_bShowMeshWires;
  bool                m_bShowMeshVertices;
  int                 m_nVertexCountTolerance;
  ERhinoPointStyle    m_eMeshVertexStyle;
  int                 m_nMeshVertexSize;

  // edges....
  bool                m_bUseDefaultMeshEdges; // obsolete - not used in display code

  bool                m_bShowMeshEdges;            // here "Edge" means "break" or "crease" edges
  bool                m_bShowMeshNakedEdges;       // "Naked" means boundary edges
  bool                m_bShowMeshNonmanifoldEdges; // "Nonmanifold means 3 or more faces meet at the edge
  
  int                 m_nMeshEdgeThickness;        // here "Edge" means "break" or "crease" edges

  bool                m_bMeshNakedEdgeOverride; // obsolete - not used in display code
private:
  BYTE                m__pad_bReserved_001;
  BYTE                m__pad_bReserved_002;
  BYTE                m__pad_bReserved_003;

public:
  int                 m_nMeshNakedEdgeThickness;
  //int                 m_nMeshNonmanifoldEdgeThickness; // this is a few lines down
  int                 m_nMeshEdgeColorReduction;   // here "Edge" means "break" or "crease" edges
  int                 m_nMeshNakedEdgeColorReduction; 
  //int                 m_nMeshNonmanifoldEdgeColorReduction; // this is a few lines down
  
  COLORREF            m_MeshEdgeColor;             // here "Edge" means "break" or "crease" edges
  COLORREF            m_MeshNakedEdgeColor;
  //COLORREF            m_MeshNonmanifoldEdgeColor; // this is a few lines down

/////////////////////////////////////
// Dimensions & Text specific attributes...
public:
  bool                m_bUseDefaultText;
  bool                m_bShowText;
  bool                m_bShowAnnotations;
  COLORREF            m_DotTextColor;
  COLORREF            m_DotBorderColor;

  BYTE                m__pad01TextAnn[64];

/////////////////////////////////////
// More Mesh non-manifold edge attributes
public:
  // look up a few lines for the break/crease edge and naked/boundary edge attributes.
  int                 m_nMeshNonmanifoldEdgeThickness;
  int                 m_nMeshNonmanifoldEdgeColorReduction;
  COLORREF            m_MeshNonmanifoldEdgeColor;

private:
  BYTE                m__pad_bReserved004[52];
  
/////////////////////////////////////
// Lights & Lighting specific attributes...
public:
  bool                m_bUseDefaultLights;
  bool                m_bShowLights;
  bool                m_bUseHiddenLights;
  bool                m_bUseLightColor;

  bool                m_bUseDefaultLightingScheme;
  ELightingScheme     m_eLightingScheme;
  
  bool                m_bUseDefaultEnvironment;
  int                 m_nLuminosity;
  COLORREF            m_AmbientColor;

  ON_ObjectArray<ON_Light>  m_Lights;
  
  int                 m_eShadowMapType;
  int                 m_nShadowMapSize;
  int                 m_nNumSamples;
  COLORREF            m_ShadowColor;
  ON_3dVector         m_ShadowBias;
  double              m_fShadowBlur;
  bool                m_bCastShadows;
  BYTE                m_nShadowBitDepth;
  BYTE                m_nTransparencyTolerance;
  bool                m_bPerPixelLighting;
  
  BYTE                m__pad01Lighting[76];
  
/////////////////////////////////////
// Points specific attributes...
public:
  bool                m_bUseDefaultPoints;
  int                 m_nPointSize;
  ERhinoPointStyle    m_ePointStyle;
  
  BYTE                m__pad01Points[128];
  
/////////////////////////////////////
// Control Polygon specific attributes...
public:
  bool                m_bUseDefaultControlPolygon;
  bool                m_bCPSolidLines;
  bool                m_bCPSingleColor;
  bool                m_bCPHidePoints;
  bool                m_bCPHideSurface;
  bool                m_bCPHighlight;
  bool                m_bCPHidden;
  int                 m_nCPWireThickness;

  int                 m_nCVSize;
  ERhinoPointStyle    m_eCVStyle;

  ON_Color            m_CPColor;

  BYTE                m__pad01CtrlPoly[128];
  
/////////////////////////////////////
// PointClouds specific attributes...
public:
  bool                m_bUseDefaultPointCloud;
  int                 m_nPCSize;
  ERhinoPointStyle    m_ePCStyle;
  int                 m_nPCGripSize;
  ERhinoPointStyle    m_ePCGripStyle;

  BYTE                m__pad01PointClouds[128];

/////////////////////////////////////
// Caching specific attributes...
public:
  bool                m_bUseDefaultCaching;
  bool                m_bAutoUpdateCaching;
  bool                m_bCachingEnabled;
  bool                m_bCacheEverything;
  UINT                m_nCacheLists;
  
/////////////////////////////////// 
// general class object attributes...
public:
  ON_UUID               Id(void) const;
  
  ON_wString            Name(void) const;
  
  virtual
  const wchar_t*        EnglishName(void) const;
  
  virtual
  const wchar_t*        LocalName(void) const;
  
  const CRuntimeClass*  Pipeline(void) const;
  ON_UUID               PipelineId(void) const;
  
  void                  SetUuid(ON_UUID);
  void                  SetName(const ON_wString&);
  void                  SetPipeline(const CRuntimeClass*);

  bool SaveProfile( const wchar_t* lpsSection, CRhinoProfileContext& ) const; // save in registry
  void LoadProfile( const wchar_t* lpsSection, CRhinoProfileContext& ); // load from registry

///////////////////////////////////
//
protected:
  virtual
  void                  Defaults(void);
  
///////////////////////////////////
//
private:
  ON_UUID               m_uuid;
  ON_wString            m_sName;
  ON_UUID               m_PipelineId;
  const CRuntimeClass*  m_pPipeline;
     */
  }
}
