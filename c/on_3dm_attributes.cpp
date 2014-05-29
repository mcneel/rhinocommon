#include "StdAfx.h"

RH_C_FUNCTION ON_3dmObjectAttributes* ON_3dmObjectAttributes_New(const ON_3dmObjectAttributes* pOther)
{
  if( NULL==pOther )
    return new ON_3dmObjectAttributes();
  return new ON_3dmObjectAttributes(*pOther);
}


// I think that sooner or later, these functions should be moved into core opennurbs.dll
RH_C_FUNCTION int ON_3dmObjectAttributes_GetSetInt( ON_3dmObjectAttributes* ptr, int which, bool set, int set_value )
{
  const int idxMode = 0;
  const int idxLineTypeSource = 1;
  const int idxColorSource = 2;
  const int idxPlotColorSource = 3;
  const int idxPlotWeightSource = 4;
  const int idxDisplayMode = 5;
  const int idxLayerIndex = 6;
  const int idxLinetypeIndex = 7;
  const int idxMaterialIndex = 8;
  const int idxMaterialSource = 9;
  const int idxObjectDecoration = 10;
  const int idxWireDensity = 11;
  const int idxSpace = 12;
  const int idxGroupCount = 13;

  int rc = set_value;
  if( ptr )
  {
    if( set )
    {
      switch( which )
      {
      case idxMode:
        ptr->SetMode( ON::ObjectMode(set_value) );
        break;
      case idxLineTypeSource:
        ptr->SetLinetypeSource( ON::ObjectLinetypeSource(set_value) );
        break;
      case idxColorSource:
        ptr->SetColorSource( ON::ObjectColorSource(set_value) );
        break;
      case idxPlotColorSource:
        ptr->SetPlotColorSource( ON::PlotColorSource(set_value) );
        break;
      case idxPlotWeightSource:
        ptr->SetPlotWeightSource( ON::PlotWeightSource(set_value) );
        break;
      case idxDisplayMode:
        ptr->SetDisplayMode( ON::DisplayMode(set_value) );
        break;
      case idxLayerIndex:
        ptr->m_layer_index = set_value;
        break;
      case idxLinetypeIndex:
        ptr->m_linetype_index = set_value;
        break;
      case idxMaterialIndex:
        ptr->m_material_index = set_value;
        break;
      case idxMaterialSource:
        ptr->SetMaterialSource( ON::ObjectMaterialSource(set_value) );
        break;
      case idxObjectDecoration:
        ptr->m_object_decoration = ON::ObjectDecoration(set_value);
        break;
      case idxWireDensity:
        // 28-Feb-2012 Dale Fugier, -1 is acceptable
        // ptr->m_wire_density = set_value<0?0:set_value;
        ptr->m_wire_density = set_value<-1?-1:set_value;
        break;
      case idxSpace:
        ptr->m_space = ON::ActiveSpace(set_value);
        break;
      case idxGroupCount:
        // no set available
        break;
      }
    }
    else
    {
      switch( which )
      {
      case idxMode:
        rc = (int)ptr->Mode();
        break;
      case idxLineTypeSource:
        rc = (int)ptr->LinetypeSource();
        break;
      case idxColorSource:
        rc = (int)ptr->ColorSource();
        break;
      case idxPlotColorSource:
        rc = (int)ptr->PlotColorSource();
        break;
      case idxPlotWeightSource:
        rc = (int)ptr->PlotWeightSource();
        break;
      case idxDisplayMode:
        rc = (int)ptr->DisplayMode();
        break;
      case idxLayerIndex:
        rc = ptr->m_layer_index;
        break;
      case idxLinetypeIndex:
        rc = ptr->m_linetype_index;
        break;
      case idxMaterialIndex:
        rc = ptr->m_material_index;
        break;
      case idxMaterialSource:
        rc = (int)ptr->MaterialSource();
        break;
      case idxObjectDecoration:
        rc = (int)ptr->m_object_decoration;
        break;
      case idxWireDensity:
        rc = ptr->m_wire_density;
        break;
      case idxSpace:
        rc = (int)ptr->m_space;
        break;
      case idxGroupCount:
        rc = ptr->GroupCount();
      }
    }
  }
  return rc;
}


RH_C_FUNCTION bool ON_3dmObjectAttributes_GetSetBool( ON_3dmObjectAttributes* ptr, int which, bool set, bool set_value )
{
  const int idxIsInstanceDefinitionObject = 0;
  const int idxIsVisible = 1;

  bool rc = set_value;
  if( ptr )
  {
    if( set )
    {
      switch(which)
      {
      case idxIsInstanceDefinitionObject:
        // nothing to set
        break;
      case idxIsVisible:
        ptr->SetVisible( set_value );
        break;
      }
    }
    else
    {
      switch(which)
      {
      case idxIsInstanceDefinitionObject:
        rc = ptr->IsInstanceDefinitionObject();
        break;
      case idxIsVisible:
        rc = ptr->IsVisible();
        break;
      }
    }
  }
  return rc;
}

//RH_C_FUNCTION unsigned int ON_3dmObjectAttributes_ApplyParentalControl(ON_3dmObjectAttributes* ptr, const ON_3dmObjectAttributes* parent_attr, unsigned int control_limits)
//{
//  unsigned int rc = 0;
//  if( ptr && parent_attr )
//    rc = ptr->ApplyParentalControl(*parent_attr, control_limits);
//  return rc;
//}

RH_C_FUNCTION ON_UUID ON_3dmObjectAttributes_m_uuid(const ON_3dmObjectAttributes* pConstObjectAttributes)
{
  if( NULL == pConstObjectAttributes )
    return ::ON_nil_uuid;
  return pConstObjectAttributes->m_uuid;
}

RH_C_FUNCTION void ON_3dmObjectAttributes_set_m_uuid(ON_3dmObjectAttributes* pAttributes, ON_UUID id)
{
  if( pAttributes )
    pAttributes->m_uuid = id;
}

RH_C_FUNCTION void ON_3dmObjectAttributes_GetSetString(ON_3dmObjectAttributes* ptr, int which, bool set, const RHMONO_STRING* _str, CRhCmnStringHolder* pStringHolder)
{
  INPUTSTRINGCOERCE(str, _str);
  const int idxName = 0;
  const int idxUrl = 1;
  if( ptr )
  {
    if(set)
    {
      if( idxName == which )
        ptr->m_name = str;
      else if( idxUrl == which )
        ptr->m_url = str;
    }
    else
    {
      if( pStringHolder )
      {
        if( idxName == which )
          pStringHolder->Set( ptr->m_name );
        else if( idxUrl == which )
          pStringHolder->Set( ptr->m_url );
      }
    }
  }
}

RH_C_FUNCTION int ON_3dmObjectAttributes_GetSetColor(ON_3dmObjectAttributes* ptr, int which, bool set, int set_value)
{
  const int idxColor = 0;
  const int idxPlotColor = 1;

  int rc = set_value;
  if( ptr )
  {
    if( set )
    {
      unsigned int abgr = ARGB_to_ABGR((unsigned int)set_value);
      if( idxColor == which )
        ptr->m_color = abgr;
      else if( idxPlotColor == which )
        ptr->m_plot_color = abgr;
    }
    else
    {
      ON_Color c;
      if( idxColor == which )
        c = ptr->m_color;
      else if( idxPlotColor == which )
        c = ptr->m_plot_color;
      unsigned int abgr = (unsigned int)c;
      rc = (int)abgr;
    }
  }
  return rc;
}

RH_C_FUNCTION double ON_3dmObjectAttributes_PlotWeight(ON_3dmObjectAttributes* ptr, bool set, double set_value)
{
  double rc = set_value;
  if( ptr )
  {
    if( set )
      ptr->m_plot_weight_mm = set_value;
    else
      rc = ptr->m_plot_weight_mm;
  }
  return rc;
}

RH_C_FUNCTION ON_UUID ON_3dmObjectAttributes_ViewportId(ON_3dmObjectAttributes* ptr, bool set, ON_UUID set_value)
{
  if( NULL == ptr )
    return ::ON_nil_uuid;
  if( set )
    ptr->m_viewport_id = set_value;
  return ptr->m_viewport_id;
}

RH_C_FUNCTION void ON_3dmObjectAttributes_GroupList(const ON_3dmObjectAttributes* ptr, int* list)
{
  if( ptr && list )
  {
    int count = ptr->GroupCount();
    if( count > 0 )
    {
      const int* src = ptr->GroupList();
      if( src )
        ::memcpy(list, src, count * sizeof(int));
    }
  }
}

RH_C_FUNCTION void ON_3dmObjectAttributes_GroupOp(ON_3dmObjectAttributes* ptr, int whichOp, int index)
{
  if( NULL == ptr )
    return;

  const int idxAddToGroup = 0;
  const int idxRemoveFromGroup = 1;
  const int idxRemoveFromTopGroup = 2;
  const int idxRemoveFromAllGroups = 3;

  switch( whichOp )
  {
  case idxAddToGroup:
    ptr->AddToGroup(index);
    break;
  case idxRemoveFromGroup:
    ptr->RemoveFromGroup(index);
    break;
  case idxRemoveFromTopGroup:
    ptr->RemoveFromTopGroup();
    break;
  case idxRemoveFromAllGroups:
    ptr->RemoveFromAllGroups();
    break;
  }
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_HasDisplayModeOverride(const ON_3dmObjectAttributes* pConstObjectAttributes, ON_UUID viewportId)
{
  bool rc = false;
  if( pConstObjectAttributes )
  {
    ON_UUID dmr_id;
    if( pConstObjectAttributes->FindDisplayMaterialId(viewportId, &dmr_id) )
    {
      //make sure dmr is not the "invisible in detail" id
      if( dmr_id != ON_DisplayMaterialRef::m_invisible_in_detail_id )
        rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_UseDisplayMode(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID rhinoViewportId, ON_UUID modeId)
{
  bool rc = false;
  if( pObjectAttributes )
  {
    ON_DisplayMaterialRef dmr;
    dmr.m_viewport_id = rhinoViewportId;
    dmr.m_display_material_id = modeId;
    rc = pObjectAttributes->AddDisplayMaterialRef(dmr);
  }
  return rc;
}

RH_C_FUNCTION void ON_3dmObjectAttributes_ClearDisplayMode(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID rhinoViewportId)
{
  if( pObjectAttributes )
  {
    pObjectAttributes->RemoveDisplayMaterialRef(rhinoViewportId);
  }
}


RH_C_FUNCTION bool ON_3dmObjectAttributes_HasMapping(ON_3dmObjectAttributes* pObjectAttributes)
{
  for (int i = 0; i < pObjectAttributes->m_rendering_attributes.m_mappings.Count(); i++)
  {
    const ON_MappingRef *pRef = pObjectAttributes->m_rendering_attributes.m_mappings.At(i);
    if (pRef->m_mapping_channels.Count())
      return true;
  }

  return false;
}



