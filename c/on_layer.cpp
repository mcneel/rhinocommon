#include "StdAfx.h"

RH_C_FUNCTION ON_Layer* ON_Layer_New()
{
  return new ON_Layer();
}

RH_C_FUNCTION void ON_Layer_Default(ON_Layer* pLayer)
{
  if( pLayer )
    pLayer->Default();
}

RH_C_FUNCTION void ON_Layer_GetLayerName(const ON_Layer* pLayer, CRhCmnStringHolder* pStringHolder)
{
  if( pLayer && pStringHolder)
    pStringHolder->Set( pLayer->LayerName() );
}

RH_C_FUNCTION void ON_Layer_SetLayerName(ON_Layer* pLayer, const RHMONO_STRING* _name)
{
  INPUTSTRINGCOERCE(name, _name);
  if( pLayer )
  {
    pLayer->SetLayerName(name);
  }
}

RH_C_FUNCTION int ON_Layer_GetColor(const ON_Layer* pLayer, bool regularColor)
{
  int rc = 0;
  if( pLayer )
  {
    unsigned int abgr = 0;
    if( regularColor )
      abgr = (unsigned int)(pLayer->Color());
    else
      abgr = (unsigned int)(pLayer->PlotColor());
    rc = (int)abgr;
  }
  return rc;
}

RH_C_FUNCTION void ON_Layer_SetColor(ON_Layer* pLayer, int argb, bool regularColor)
{
  if( pLayer )
  {
    unsigned int abgr = ARGB_to_ABGR(argb);
    if( regularColor )
      pLayer->SetColor( ON_Color(abgr) );
    else
      pLayer->SetPlotColor( ON_Color(abgr) );
  }
}

RH_C_FUNCTION int ON_Layer_GetInt(const ON_Layer* pLayer, int which)
{
  const int idxLinetypeIndex = 0;
  const int idxRenderMaterialIndex = 1;
  const int idxLayerIndex = 2;
  const int idxIgesLevel = 3;
  int rc = -1;
  if( pLayer )
  {
    switch(which)
    {
    case idxLinetypeIndex:
      rc = pLayer->LinetypeIndex();
      break;
    case idxRenderMaterialIndex:
      rc = pLayer->RenderMaterialIndex();
      break;
    case idxLayerIndex:
      rc = pLayer->LayerIndex();
      break;
    case idxIgesLevel:
      rc = pLayer->IgesLevel();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Layer_SetInt(ON_Layer* pLayer, int which, int val)
{
  const int idxLinetypeIndex = 0;
  const int idxRenderMaterialIndex = 1;
  const int idxLayerIndex = 2;
  const int idxIgesLevel = 3;
  if( pLayer )
  {
    switch(which)
    {
    case idxLinetypeIndex:
      pLayer->SetLinetypeIndex(val);
      break;
    case idxRenderMaterialIndex:
      pLayer->SetRenderMaterialIndex(val);
      break;
    case idxLayerIndex:
      pLayer->SetLayerIndex(val);
      break;
    case idxIgesLevel:
      pLayer->SetIgesLevel(val);
      break;
    }
  }
}

RH_C_FUNCTION bool ON_Layer_GetSetBool(ON_Layer* pLayer, int which, bool set, bool val)
{
  const int idxIsVisible = 0;
  const int idxIsLocked = 1;
  const int idxIsExpanded = 2;
  bool rc = val;
  if( pLayer )
  {
    if( set )
    {
      if( idxIsVisible==which )
        pLayer->SetVisible(val);
      else if( idxIsLocked==which )
        pLayer->SetLocked(val);
      else if( idxIsExpanded==which )
        pLayer->m_bExpanded = val;
    }
    else 
    {
      if( idxIsVisible==which )
        rc = pLayer->IsVisible();
      else if( idxIsLocked==which )
        rc = pLayer->IsLocked();
      else if( idxIsExpanded==which )
        rc = pLayer->m_bExpanded;
    }
  }
  return rc;
}

RH_C_FUNCTION double ON_Layer_GetPlotWeight(const ON_Layer* pLayer)
{
  double rc = 0;
  if( pLayer )
    rc = pLayer->PlotWeight();
  return rc;
}

RH_C_FUNCTION void ON_Layer_SetPlotWeight(ON_Layer* pLayer, double value)
{
  if( pLayer )
    pLayer->SetPlotWeight(value);
}

RH_C_FUNCTION ON_UUID ON_Layer_GetGuid(const ON_Layer* pLayer, bool layerId)
{
  if( pLayer )
  {
    if( layerId )
      return pLayer->m_layer_id;
    else
      return pLayer->m_parent_layer_id;
  }
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ON_Layer_SetGuid(ON_Layer* pLayer, bool layerId, ON_UUID value)
{
  if( pLayer )
  {
    if( layerId )
      pLayer->m_layer_id = value;
    else
      pLayer->m_parent_layer_id = value;
  }
}


