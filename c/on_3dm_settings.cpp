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