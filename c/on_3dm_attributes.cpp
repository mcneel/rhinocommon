#include "StdAfx.h"

RH_C_FUNCTION ON_3dmObjectAttributes* ON_3dmObjectAttributes_New(const ON_3dmObjectAttributes* pOther)
{
  if( NULL==pOther )
    return new ON_3dmObjectAttributes();
  return new ON_3dmObjectAttributes(*pOther);
}

RH_C_FUNCTION void ON_3dmObjectAttributes_Delete(ON_3dmObjectAttributes* pointer)
{
  if (pointer) delete pointer;
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

RH_C_FUNCTION const ON_MaterialRef* ON_3dmObjectAttributes_MaterialRef(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID plugInId)
{
  if (pObjectAttributes == NULL) return NULL;
  const ON_MaterialRef* result = pObjectAttributes->m_rendering_attributes.MaterialRef(plugInId);
  return result;
}

RH_C_FUNCTION void ON_3dmObjectAttributes_EmptyMaterialRefs(ON_3dmObjectAttributes* pObjectAttributes)
{
  if (pObjectAttributes)pObjectAttributes->m_rendering_attributes.m_materials.Empty();
}

RH_C_FUNCTION int ON_3dmObjectAttributes_MaterialRefCount(ON_3dmObjectAttributes* pObjectAttributes)
{
  return (pObjectAttributes ? pObjectAttributes->m_rendering_attributes.m_materials.Count() : 0);
}

RH_C_FUNCTION int ON_3dmObjectAttributes_MaterialRefIndexOf(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID plugInId)
{
  if (pObjectAttributes == NULL) return -1;
  for (int i = 0, count = pObjectAttributes->m_rendering_attributes.m_materials.Count(); i < count; i++)
    if (pObjectAttributes->m_rendering_attributes.m_materials[i].m_plugin_id == plugInId)
      return i;
  return -1;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_RemoveMaterialRefAt(ON_3dmObjectAttributes* pObjectAttributes, int index)
{
  if (pObjectAttributes == NULL) return false;
  if (index < 0 || index >= pObjectAttributes->m_rendering_attributes.m_materials.Count()) return false;
  pObjectAttributes->m_rendering_attributes.m_materials.Remove(index);
  return true;
}

RH_C_FUNCTION const ON_MaterialRef* ON_3dmObjectAttributes_MaterialFromIndex(ON_3dmObjectAttributes* pObjectAttributes, int index)
{
  if (pObjectAttributes == NULL || index < 0 || index >= pObjectAttributes->m_rendering_attributes.m_materials.Count()) return NULL;
  ON_MaterialRef& result = pObjectAttributes->m_rendering_attributes.m_materials[index];
  return &result;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_MaterialRefSource(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID plugInId, int* value)
{
  if (pObjectAttributes == NULL || value == NULL) return false;
  const ON_MaterialRef* mat_ref = pObjectAttributes->m_rendering_attributes.MaterialRef(plugInId);
  if (mat_ref == NULL) return false;
  *value = mat_ref->m_material_source;
  return true;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_MaterialId(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID plugInId, ON_UUID* value, bool backFace)
{
  if (pObjectAttributes == NULL || value == NULL) return false;
  const ON_MaterialRef* mat_ref = pObjectAttributes->m_rendering_attributes.MaterialRef(plugInId);
  if (mat_ref == NULL) return false;
  *value = backFace ? mat_ref->m_material_backface_id : mat_ref->m_material_id;
  return true;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_MaterialIndex(ON_3dmObjectAttributes* pObjectAttributes, ON_UUID plugInId, int* value, bool backFace)
{
  if (pObjectAttributes == NULL || value == NULL) return false;
  const ON_MaterialRef* mat_ref = pObjectAttributes->m_rendering_attributes.MaterialRef(plugInId);
  if (mat_ref == NULL) return false;
  *value = backFace ? mat_ref->m_material_backface_index : mat_ref->m_material_index;
  return true;
}

RH_C_FUNCTION bool ON_3dmObjectAttributes_AddMaterialRef(ON_3dmObjectAttributes* pObjectAttributes, const ON_MaterialRef* pMaterialRef)
{
  if (pObjectAttributes == NULL || pMaterialRef == NULL || pMaterialRef->m_plugin_id == ON_nil_uuid)
    return false;
  ON_MaterialRef* mat_ref = const_cast<ON_MaterialRef*>(pObjectAttributes->m_rendering_attributes.MaterialRef(pMaterialRef->m_plugin_id));
  if (mat_ref == NULL)
    mat_ref = &(pObjectAttributes->m_rendering_attributes.m_materials.AppendNew());
  *mat_ref = *pMaterialRef;
  return true;
}

RH_C_FUNCTION ON_MaterialRef* ON_MaterialRef_New(const ON_MaterialRef* other)
{
  ON_MaterialRef* result = (other ? new ON_MaterialRef(*other) : new ON_MaterialRef());
  return result;
}

RH_C_FUNCTION void ON_MaterialRef_Delete(ON_MaterialRef* pointer)
{
  if (pointer) delete pointer;
}

RH_C_FUNCTION bool ON_MaterialRef_PlugInId(const ON_MaterialRef* pointer, ON_UUID* value)
{
  if (pointer == NULL || value == NULL) return false;
  *value = pointer->m_plugin_id;
  return true;
}

RH_C_FUNCTION bool ON_MaterialRef_SetPlugInId(ON_MaterialRef* pointer, ON_UUID value)
{
  if (pointer == NULL) return false;
  pointer->m_plugin_id = value;
  return true;
}

RH_C_FUNCTION bool ON_MaterialRef_SetMaterialId(ON_MaterialRef* pointer, ON_UUID value, bool backFace)
{
  if (pointer == NULL) return false;
  if (backFace)
    pointer->m_material_backface_id = value;
  else
    pointer->m_material_id = value;
  return true;
}

RH_C_FUNCTION bool ON_MaterialRef_SetMaterialIndex(ON_MaterialRef* pointer, int value, bool backFace)
{
  if (pointer == NULL) return false;
  if (backFace)
    pointer->m_material_backface_index = value;
  else
    pointer->m_material_index = value;
  return true;
}

RH_C_FUNCTION bool ON_MaterialRef_SetMaterialSource(ON_MaterialRef* pointer, int value)
{
  if (pointer == NULL) return false;
  pointer->m_material_source = ON::ObjectMaterialSource(value);
  return true;
}
