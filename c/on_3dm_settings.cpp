#include "StdAfx.h"

RH_C_FUNCTION void ON_3dmConstructionPlane_Copy(const ON_3dmConstructionPlane* pCP, ON_PLANE_STRUCT* plane,
                                                double* grid_spacing, double* snap_spacing,
                                                int* grid_line_count, int* grid_thick_freq,
                                                bool* depthbuffered, CRhCmnStringHolder* pString)
{
  if( pCP )
  {
    if( plane )
      CopyToPlaneStruct(*plane, pCP->m_plane);
    if( grid_spacing )
      *grid_spacing = pCP->m_grid_spacing;
    if( snap_spacing )
      *snap_spacing = pCP->m_snap_spacing;
    if( grid_line_count )
      *grid_line_count = pCP->m_grid_line_count;
    if( grid_thick_freq )
      *grid_thick_freq = pCP->m_grid_thick_frequency;
    if( depthbuffered )
      *depthbuffered = pCP->m_bDepthBuffer;
    if( pString )
      pString->Set(pCP->m_name);
  }
}

RH_C_FUNCTION ON_3dmConstructionPlane* ON_3dmConstructionPlane_New(const ON_PLANE_STRUCT* plane,
                                                                   double grid_spacing,
                                                                   double snap_spacing,
                                                                   int grid_line_count,
                                                                   int grid_thick_frequency,
                                                                   bool depthBuffered,
                                                                   const RHMONO_STRING* _name)
{
  ON_3dmConstructionPlane* rc = NULL;
  if( _name && plane )
  {
    INPUTSTRINGCOERCE(name, _name);
    rc = new ON_3dmConstructionPlane();
    rc->m_plane = FromPlaneStruct(*plane);
    rc->m_grid_spacing = grid_spacing;
    rc->m_snap_spacing = snap_spacing;
    rc->m_grid_line_count = grid_line_count;
    rc->m_grid_thick_frequency = grid_thick_frequency;
    rc->m_bDepthBuffer = depthBuffered;
    rc->m_name = name;
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmConstructionPlane_Delete(ON_3dmConstructionPlane* pCPlane)
{
  if( pCPlane )
    delete pCPlane;
}


RH_C_FUNCTION ON_3dmView* ON_3dmView_New(const ON_3dmView* pConstOther3dmView)
{
  if( pConstOther3dmView )
    return new ON_3dmView(*pConstOther3dmView);
  return new ON_3dmView();
}

RH_C_FUNCTION void ON_3dmView_Delete(ON_3dmView* ptr)
{
  if( ptr )
    delete ptr;
}

RH_C_FUNCTION void ON_3dmView_NameGet(const ON_3dmView* pView, CRhCmnStringHolder* pString)
{
  if( pView && pString)
    pString->Set(pView->m_name);
}

RH_C_FUNCTION void ON_3dmView_NameSet(ON_3dmView* pView, const RHMONO_STRING* _name)
{
  INPUTSTRINGCOERCE(name, _name);
  if( pView)
  {
    pView->m_name = name;
  }
}

RH_C_FUNCTION const ON_Viewport* ON_3dmView_ViewportPointer(const ON_3dmView* pView)
{
  const ON_Viewport* rc = NULL;
  if( pView )
  {
    rc = &(pView->m_vp);
  }
  return rc;
}

RH_C_FUNCTION ON_EarthAnchorPoint* ON_EarthAnchorPoint_New()
{
  return new ON_EarthAnchorPoint();
}

RH_C_FUNCTION void ON_EarthAnchorPoint_Delete(ON_EarthAnchorPoint* pEarthAnchor)
{
  if( pEarthAnchor )
    delete pEarthAnchor;
}

RH_C_FUNCTION double ON_EarthAnchorPoint_GetDouble(const ON_EarthAnchorPoint* pConstEarthAnchor, int which)
{
  const int idxEarthBasepointLatitude = 0;
  const int idxEarthBasepointLongitude = 1;
  const int idxEarthBasepointElevation = 2;
  double rc = 0;
  if( pConstEarthAnchor )
  {
    if( idxEarthBasepointLatitude==which )
      rc = pConstEarthAnchor->m_earth_basepoint_latitude;
    else if( idxEarthBasepointLongitude==which )
      rc = pConstEarthAnchor->m_earth_basepoint_longitude;
    else if( idxEarthBasepointElevation==which )
      rc = pConstEarthAnchor->m_earth_basepoint_elevation;
  }
  return rc;
}

RH_C_FUNCTION void ON_EarthAnchorPoint_SetDouble(ON_EarthAnchorPoint* pEarthAnchor, int which, double val)
{
  const int idxEarthBasepointLatitude = 0;
  const int idxEarthBasepointLongitude = 1;
  const int idxEarthBasepointElevation = 2;
  if( pEarthAnchor )
  {
    if( idxEarthBasepointLatitude==which )
      pEarthAnchor->m_earth_basepoint_latitude = val;
    else if( idxEarthBasepointLongitude==which )
      pEarthAnchor->m_earth_basepoint_longitude = val;
    else if( idxEarthBasepointElevation==which )
      pEarthAnchor->m_earth_basepoint_elevation = val;
  }
}

RH_C_FUNCTION int ON_EarthAnchorPoint_GetEarthBasepointElevationZero(const ON_EarthAnchorPoint* pConstEarthAnchor)
{
  int rc = 0;
  if( pConstEarthAnchor )
    rc = pConstEarthAnchor->m_earth_basepoint_elevation_zero;
  return rc;
}

RH_C_FUNCTION void ON_EarthAnchorPoint_SetEarthBasepointElevationZero(ON_EarthAnchorPoint* pEarthAnchor, int val)
{
  if( pEarthAnchor )
    pEarthAnchor->m_earth_basepoint_elevation_zero = val;
}

RH_C_FUNCTION void ON_EarthAnchorPoint_ModelBasePoint(ON_EarthAnchorPoint* pEarthAnchor, bool set, ON_3dPoint* point)
{
  if( pEarthAnchor && point )
  {
    if( set )
      pEarthAnchor->m_model_basepoint = *point;
    else
      *point = pEarthAnchor->m_model_basepoint;
  }
}

RH_C_FUNCTION void ON_EarthAnchorPoint_ModelDirection(ON_EarthAnchorPoint* pEarthAnchor, bool north, bool set, ON_3dVector* vector)
{
  if( pEarthAnchor && vector )
  {
    if( set )
    {
      if( north )
        pEarthAnchor->m_model_north = *vector;
      else
        pEarthAnchor->m_model_east = *vector;
    }
    else
    {
      *vector = north? pEarthAnchor->m_model_north : pEarthAnchor->m_model_east;
    }
  }
}

RH_C_FUNCTION void ON_EarthAnchorPoint_GetString(const ON_EarthAnchorPoint* pConstEarthAnchor, bool name, CRhCmnStringHolder* pString)
{
  if( pConstEarthAnchor && pString )
  {
    if( name )
      pString->Set( pConstEarthAnchor->m_name );
    else
      pString->Set( pConstEarthAnchor->m_description );
  }
}

RH_C_FUNCTION void ON_EarthAnchorPoint_SetString(ON_EarthAnchorPoint* pEarthAnchor, bool name, const RHMONO_STRING* str)
{
  if( pEarthAnchor && str )
  {
    INPUTSTRINGCOERCE(_str, str);
    if( name )
      pEarthAnchor->m_name = _str;
    else
      pEarthAnchor->m_description = _str;
  }
}

RH_C_FUNCTION void ON_EarthAnchorPoint_GetModelCompass(const ON_EarthAnchorPoint* pConstEarthAnchor, ON_PLANE_STRUCT* plane)
{
  if( pConstEarthAnchor && plane )
  {
    ON_Plane _plane;
    if( pConstEarthAnchor->GetModelCompass(_plane) )
      CopyToPlaneStruct(*plane, _plane);
  }
}

RH_C_FUNCTION void ON_EarthAnchorPoint_GetModelToEarthTransform(const ON_EarthAnchorPoint* pConstEarthAnchor, int units, ON_Xform* xform)
{
  if( pConstEarthAnchor && xform )
  {
    ON_UnitSystem us(ON::UnitSystem(units));
    pConstEarthAnchor->GetModelToEarthXform(us, *xform);
  }
}

RH_C_FUNCTION void ON_3dmSettings_GetModelUrl(const ON_3dmSettings* pConstSettings, CRhCmnStringHolder* pString)
{
  if( pConstSettings && pString )
    pString->Set(pConstSettings->m_model_URL);
}

RH_C_FUNCTION void ON_3dmSettings_SetModelUrl(ON_3dmSettings* pSettings, const RHMONO_STRING* str)
{
  if( pSettings )
  {
    INPUTSTRINGCOERCE(_str, str);
    pSettings->m_model_URL = _str;
  }
}

RH_C_FUNCTION void ON_3dmSettings_GetModelBasepoint(const ON_3dmSettings* pConstSettings, ON_3dPoint* point)
{
  if( pConstSettings && point )
    *point = pConstSettings->m_model_basepoint;
}

RH_C_FUNCTION void ON_3dmSettings_SetModelBasepoint(ON_3dmSettings* pSettings, ON_3DPOINT_STRUCT point )
{
  if( pSettings )
  {
    ON_3dPoint pt(point.val);
    pSettings->m_model_basepoint = pt;
  }
}

RH_C_FUNCTION double ON_3dmSettings_GetDouble(const ON_3dmSettings* pConstSettings, int which)
{
  const int idxModelAbsTol = 0;
  const int idxModelAngleTol = 1;
  const int idxModelRelTol = 2;
  const int idxPageAbsTol = 3;
  const int idxPageAngleTol = 4;
  const int idxPageRelTol = 5;
  double rc = 0;
  if( pConstSettings )
  {
    switch( which )
    {
    case idxModelAbsTol:
      rc = pConstSettings->m_ModelUnitsAndTolerances.m_absolute_tolerance;
      break;
    case idxModelAngleTol:
      rc = pConstSettings->m_ModelUnitsAndTolerances.m_angle_tolerance;
      break;
    case idxModelRelTol:
      rc = pConstSettings->m_ModelUnitsAndTolerances.m_relative_tolerance;
      break;
    case idxPageAbsTol:
      rc = pConstSettings->m_PageUnitsAndTolerances.m_absolute_tolerance;
      break;
    case idxPageAngleTol:
      rc = pConstSettings->m_PageUnitsAndTolerances.m_angle_tolerance;
      break;
    case idxPageRelTol:
      rc = pConstSettings->m_PageUnitsAndTolerances.m_relative_tolerance;
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmSettings_SetDouble(ON_3dmSettings* pSettings, int which, double val)
{
  const int idxModelAbsTol = 0;
  const int idxModelAngleTol = 1;
  const int idxModelRelTol = 2;
  const int idxPageAbsTol = 3;
  const int idxPageAngleTol = 4;
  const int idxPageRelTol = 5;

  if( pSettings )
  {
    switch( which )
    {
    case idxModelAbsTol:
      pSettings->m_ModelUnitsAndTolerances.m_absolute_tolerance = val;
      break;
    case idxModelAngleTol:
      pSettings->m_ModelUnitsAndTolerances.m_angle_tolerance = val;
      break;
    case idxModelRelTol:
      pSettings->m_ModelUnitsAndTolerances.m_relative_tolerance = val;
      break;
    case idxPageAbsTol:
      pSettings->m_PageUnitsAndTolerances.m_absolute_tolerance = val;
      break;
    case idxPageAngleTol:
      pSettings->m_PageUnitsAndTolerances.m_angle_tolerance = val;
      break;
    case idxPageRelTol:
      pSettings->m_PageUnitsAndTolerances.m_relative_tolerance = val;
      break;
    }
  }
}

RH_C_FUNCTION int ON_3dmSettings_GetSetUnitSystem(ON_3dmSettings* pSettings, bool model, bool set, int set_val)
{
  int rc = set_val;
  if( pSettings )
  {
    if( set )
    {
      if( model )
        pSettings->m_ModelUnitsAndTolerances.m_unit_system = ON::UnitSystem(set_val);
      else
        pSettings->m_PageUnitsAndTolerances.m_unit_system = ON::UnitSystem(set_val);
    }
    else
    {
      if( model )
        rc = (int)pSettings->m_ModelUnitsAndTolerances.m_unit_system.m_unit_system;
      else
        rc = (int)pSettings->m_PageUnitsAndTolerances.m_unit_system.m_unit_system;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_3dmRenderSettings* ON_3dmRenderSettings_New(const ON_3dmRenderSettings* other)
{
  if( other )
    return new ON_3dmRenderSettings(*other);
  return new ON_3dmRenderSettings();
}

RH_C_FUNCTION const ON_3dmRenderSettings* ON_3dmRenderSettings_ConstPointer(int docId)
{
  const ON_3dmRenderSettings* rc = NULL;
#if !defined(OPENNURBS_BUILD)
  CRhinoDoc* pDoc = RhDocFromId(docId);
  if( pDoc )
    rc = &(pDoc->Properties().RenderSettings());
#endif
  return rc;
}

RH_C_FUNCTION void ON_3dmRenderSettings_Delete(ON_3dmRenderSettings* pRenderSettings)
{
  if( pRenderSettings )
    delete pRenderSettings;
}

RH_C_FUNCTION int ON_3dmRenderSettings_GetColor(const ON_3dmRenderSettings* pConstRenderSettings, int which)
{
  const int idxAmbientLight = 0;
  const int idxBackgroundColorTop = 1;
  const int idxBackgroundColorBottom = 2;
  int rc = 0;
  if( pConstRenderSettings )
  {
    unsigned int abgr=0;
    switch(which)
    {
    case idxAmbientLight:
      abgr = (unsigned int)(pConstRenderSettings->m_ambient_light);
      break;
    case idxBackgroundColorTop:
      abgr = (unsigned int)(pConstRenderSettings->m_background_color);
      break;
    case idxBackgroundColorBottom:
      abgr = (unsigned int)(pConstRenderSettings->m_background_bottom_color);
      break;
    }
    rc = (int)abgr;
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmRenderSettings_SetColor(ON_3dmRenderSettings* pRenderSettings, int which, int argb)
{
  const int idxAmbientLight = 0;
  const int idxBackgroundColorTop = 1;
  const int idxBackgroundColorBottom = 2;
  if( pRenderSettings )
  {
    unsigned int abgr = ARGB_to_ABGR(argb);
    switch(which)
    {
    case idxAmbientLight:
      pRenderSettings->m_ambient_light = abgr;
      break;
    case idxBackgroundColorTop:
      pRenderSettings->m_background_color = abgr;
      break;
    case idxBackgroundColorBottom:
      pRenderSettings->m_background_bottom_color = abgr;
      break;
    }
  }
}

RH_C_FUNCTION bool ON_3dmRenderSettings_GetBool(const ON_3dmRenderSettings* pConstRenderSettings, int which)
{
  const int idxUseHiddenLights = 0;
  const int idxDepthCue = 1;
  const int idxFlatShade = 2;
  const int idxRenderBackFaces = 3;
  const int idxRenderPoints = 4;
  const int idxRenderCurves = 5;
  const int idxRenderIsoparams = 6;
  const int idxRenderMeshEdges = 7;
  const int idxRenderAnnotation = 8;
  bool rc = false;
  if( pConstRenderSettings )
  {
    switch(which)
    {
    case idxUseHiddenLights:
      rc = pConstRenderSettings->m_bUseHiddenLights?true:false;
      break;
    case idxDepthCue:
      rc = pConstRenderSettings->m_bDepthCue?true:false;
      break;
    case idxFlatShade:
      rc = pConstRenderSettings->m_bFlatShade?true:false;
      break;
    case idxRenderBackFaces:
      rc = pConstRenderSettings->m_bRenderBackfaces?true:false;
      break;
    case idxRenderPoints:
      rc = pConstRenderSettings->m_bRenderPoints?true:false;
      break;
    case idxRenderCurves:
      rc = pConstRenderSettings->m_bRenderCurves?true:false;
      break;
    case idxRenderIsoparams:
      rc= pConstRenderSettings->m_bRenderIsoparams?true:false;
      break;
    case idxRenderMeshEdges:
      rc = pConstRenderSettings->m_bRenderMeshEdges?true:false;
      break;
    case idxRenderAnnotation:
      rc = pConstRenderSettings->m_bRenderAnnotation?true:false;
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmRenderSettings_SetBool(ON_3dmRenderSettings* pRenderSettings, int which, bool b)
{
  const int idxUseHiddenLights = 0;
  const int idxDepthCue = 1;
  const int idxFlatShade = 2;
  const int idxRenderBackFaces = 3;
  const int idxRenderPoints = 4;
  const int idxRenderCurves = 5;
  const int idxRenderIsoparams = 6;
  const int idxRenderMeshEdges = 7;
  const int idxRenderAnnotation = 8;
  if( pRenderSettings )
  {
    switch(which)
    {
    case idxUseHiddenLights:
      pRenderSettings->m_bUseHiddenLights = b;
      break;
    case idxDepthCue:
      pRenderSettings->m_bDepthCue = b;
      break;
    case idxFlatShade:
      pRenderSettings->m_bFlatShade = b;
      break;
    case idxRenderBackFaces:
      pRenderSettings->m_bRenderBackfaces = b;
      break;
    case idxRenderPoints:
      pRenderSettings->m_bRenderPoints = b;
      break;
    case idxRenderCurves:
      pRenderSettings->m_bRenderCurves = b;
      break;
    case idxRenderIsoparams:
      pRenderSettings->m_bRenderIsoparams = b;
      break;
    case idxRenderMeshEdges:
      pRenderSettings->m_bRenderMeshEdges = b;
      break;
    case idxRenderAnnotation:
      pRenderSettings->m_bRenderAnnotation = b;
      break;
    default:
      break;
    }
  }
}

RH_C_FUNCTION int ON_3dmRenderSettings_GetInt(const ON_3dmRenderSettings* pConstRenderSettings, int which)
{
  const int idxBackgroundStyle = 0;
  const int idxAntialiasStyle = 1;
  const int idxShadowmapStyle = 2;
  const int idxShadowmapWidth = 3;
  const int idxShadowmapHeight = 4;
  const int idxImageWidth = 5;
  const int idxImageHeight = 6;
  int rc = 0;
  if( pConstRenderSettings )
  {
    switch(which)
    {
    case idxBackgroundStyle:
      rc = pConstRenderSettings->m_background_style;
      break;
    case idxAntialiasStyle:
      rc = pConstRenderSettings->m_antialias_style;
      break;
    case idxShadowmapStyle:
      rc = pConstRenderSettings->m_shadowmap_style;
      break;
    case idxShadowmapWidth:
      rc = pConstRenderSettings->m_shadowmap_width;
      break;
    case idxShadowmapHeight:
      rc = pConstRenderSettings->m_shadowmap_height;
      break;
    case idxImageWidth:
      rc = pConstRenderSettings->m_image_width;
      break;
    case idxImageHeight:
      rc = pConstRenderSettings->m_image_height;
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmRenderSettings_SetInt(ON_3dmRenderSettings* pRenderSettings, int which, int i)
{
  const int idxBackgroundStyle = 0;
  const int idxAntialiasStyle = 1;
  const int idxShadowmapStyle = 2;
  const int idxShadowmapWidth = 3;
  const int idxShadowmapHeight = 4;
  const int idxImageWidth = 5;
  const int idxImageHeight = 6;
  int rc = 0;
  if( pRenderSettings )
  {
    switch(which)
    {
    case idxBackgroundStyle:
      pRenderSettings->m_background_style = i;
      break;
    case idxAntialiasStyle:
      pRenderSettings->m_antialias_style = i;
      break;
    case idxShadowmapStyle:
      pRenderSettings->m_shadowmap_style = i;
      break;
    case idxShadowmapWidth:
      pRenderSettings->m_shadowmap_width = i;
      break;
    case idxShadowmapHeight:
      pRenderSettings->m_shadowmap_height = i;
      break;
    case idxImageWidth:
      pRenderSettings->m_image_width = i;
      break;
    case idxImageHeight:
      pRenderSettings->m_image_height = i;
      break;
    default:
      break;
    }
  }
}
