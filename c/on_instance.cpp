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