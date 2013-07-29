#include "StdAfx.h"

RH_C_FUNCTION ON_InstanceDefinition* ON_InstanceDefinition_New(const ON_InstanceDefinition* pOther)
{
  if( pOther )
    return new ON_InstanceDefinition(*pOther);
  return new ON_InstanceDefinition();
}

RH_C_FUNCTION void ON_InstanceDefinition_GetString(const ON_InstanceDefinition* pConstInstanceDefinition, int which, CRhCmnStringHolder* pStringHolder)
{
  const int idxName = 0;
  const int idxDescription = 1;

  if( pConstInstanceDefinition && pStringHolder )
  {
    if( idxName == which )
    {
      pStringHolder->Set( pConstInstanceDefinition->m_name );
    }
    else if( idxDescription == which )
    {
      pStringHolder->Set( pConstInstanceDefinition->m_description );
    }
  }
}

RH_C_FUNCTION void ON_InstanceDefinition_SetString( ON_InstanceDefinition* pInstanceDefinition, int which, const RHMONO_STRING* _str)
{
  const int idxName = 0;
  const int idxDescription = 1;

  if( pInstanceDefinition )
  {
    INPUTSTRINGCOERCE(str, _str);
    if( idxName == which )
    {
      pInstanceDefinition->SetName(str);
    }
    else if( idxDescription == which )
    {
      pInstanceDefinition->SetDescription(str);
    }
  }
}

RH_C_FUNCTION ON_UUID ON_InstanceDefinition_GetId( const ON_InstanceDefinition* pConstIdef )
{
  if( pConstIdef )
    return pConstIdef->m_uuid;
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ON_InstanceDefinition_SetId( ON_InstanceDefinition* pIdef, ON_UUID id )
{
  if( pIdef )
    pIdef->m_uuid = id;
}

RH_C_FUNCTION void ON_InstanceDefinition_GetObjectIds( const ON_InstanceDefinition* pConstIdef, ON_SimpleArray<ON_UUID>* pIds )
{
  if( pConstIdef && pIds )
  {
    *pIds = pConstIdef->m_object_uuid;
  }
}

RH_C_FUNCTION ON_InstanceRef* ON_InstanceRef_New( ON_UUID instanceDefinitionId, ON_Xform* instanceXform)
{
  ON_InstanceRef* ptr = new ON_InstanceRef();
  if (instanceXform)
    ptr->m_xform = *instanceXform;
  ptr->m_instance_definition_uuid = instanceDefinitionId;
  return ptr;
}

RH_C_FUNCTION ON_UUID ON_InstanceRef_IDefId( const ON_InstanceRef* pConstInstanceRef )
{
  if ( pConstInstanceRef )
    return pConstInstanceRef->m_instance_definition_uuid;
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ON_InstanceRef_GetTransform( const ON_InstanceRef* pConstInstanceRef, ON_Xform* transform )
{
  if ( pConstInstanceRef && transform )
    *transform = pConstInstanceRef->m_xform;
}