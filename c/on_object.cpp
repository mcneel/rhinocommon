#include "StdAfx.h"

static bool g_InShutDown = false;
bool RhInShutDown()
{
  return g_InShutDown;
}

RH_C_FUNCTION void RhCmn_SetInShutDown()
{
  g_InShutDown = true;
}

RH_C_FUNCTION void ON_Object_Dump( const ON_Object* pConstObject, CRhCmnStringHolder* pStringHolder )
{
  if( pConstObject && pStringHolder )
  {
    ON_wString s;
    ON_TextLog textlog(s);
    pConstObject->Dump(textlog);
    pStringHolder->Set(s);
  }
}

RH_C_FUNCTION void ON_Object_Delete( ON_Object* pObject )
{
  if( pObject && !RhInShutDown() )
    delete pObject;
}

RH_C_FUNCTION ON_Object* ON_Object_Duplicate( ON_Object* pObject )
{
  ON_Object* rc = NULL;
  if( pObject )
    rc = pObject->Duplicate();
  return rc;
}

RH_C_FUNCTION unsigned int ON_Object_ObjectType( ON_Object* pObject )
{
  unsigned int rc = (unsigned int)ON::unknown_object_type;
  if( pObject )
    rc = (unsigned int)pObject->ObjectType();
  return rc;
}


RH_C_FUNCTION bool ON_Object_IsValid(const ON_Object* pConstObject, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;

  if (pConstObject)
  {
    if( pStringHolder )
    {
      ON_wString str;
      ON_TextLog log(str);
      rc = pConstObject->IsValid(&log) ? true: false;
      pStringHolder->Set(str);
    }
    else
    {
      rc = pConstObject->IsValid() ? true : false;
    }
  }
  return rc;
}

RH_C_FUNCTION unsigned int ON_Object_SizeOf(const ON_Object* pObject)
{
  unsigned int rc = 0;
  if( pObject )
    rc = pObject->SizeOf();
  return rc;
}

RH_C_FUNCTION bool ON_Object_SetUserString(const ON_Object* pObject, const RHMONO_STRING* _key, const RHMONO_STRING* _value)
{
  bool rc = false;
  if( pObject && _key )
  {
    ON_Object* ptr = const_cast<ON_Object*>(pObject);
    INPUTSTRINGCOERCE(key, _key);
    INPUTSTRINGCOERCE(value, _value);
    rc = ptr->SetUserString(key, value);
#if defined(RHINO_V5SR) // only available in V5
    RhUpdateTextFieldSerialNumber();
#endif
  }
  return rc;
}

static CRhCmnStringHolder theGetUserString;
RH_C_FUNCTION const RHMONO_STRING* ON_Object_GetUserString(const ON_Object* pObject, const RHMONO_STRING* _key)
{
  const RHMONO_STRING* rc = NULL;
  if( pObject && _key )
  {
    INPUTSTRINGCOERCE(key, _key);
    ON_wString s;
    if( pObject->GetUserString(key, s) )
    {
      theGetUserString.Set(s);
      rc = theGetUserString.Array();
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Object_UserStringCount(const ON_Object* pObject)
{
  int rc = 0;
  if( pObject )
  {
    ON_ClassArray<ON_wString> list;
    rc = pObject->GetUserStringKeys(list);
  }
  return rc;
}

RH_C_FUNCTION ON_ClassArray<ON_UserString>* ON_Object_GetUserStrings(const ON_Object* pObject, int* count)
{
  ON_ClassArray<ON_UserString>* rc = NULL;
  if( pObject && count )
  {
    rc = new ON_ClassArray<ON_UserString>();
    *count = pObject->GetUserStrings(*rc);
    if( rc->Count()<1 )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_UserData* ON_Object_FirstUserData(const ON_Object* pObject)
{
  ON_UserData* rc = NULL;
  if( pObject )
    rc = pObject->FirstUserData();
  return rc;
}

RH_C_FUNCTION void ON_Object_CopyUserData(const ON_Object* pConstSourceObject, ON_Object* pDestinationObject)
{
  if( pConstSourceObject && pDestinationObject )
  {
    pDestinationObject->CopyUserData(*pConstSourceObject);
  }
}

RH_C_FUNCTION int ON_Object_UserDataCount(const ON_Object* pObject)
{
  int rc = 0;
  if( pObject )
  {
    ON_UserData* pUD = pObject->FirstUserData();
    while( pUD )
    {
      rc++;
      pUD = pUD->Next();
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Object_AttachUserData(ON_Object* pOnObject, ON_UserData* pUserData, bool detachIfNeeded)
{
  bool rc = false;
  if( pOnObject && pUserData )
  {
    if( detachIfNeeded )
    {
      ON_Object* pOwner = pUserData->Owner();
      if( pOwner==pOnObject )
        return true; //already attached to this object
      if( pOwner )
        pOwner->DetachUserData(pUserData);
    }
    rc = pOnObject->AttachUserData(pUserData)?true:false;
  }
  return rc;
}

////////////////////////////////////////////////////////////////////////////////////////

static CRhCmnStringHolder theKeyValueHolder;
RH_C_FUNCTION const RHMONO_STRING* ON_UserStringList_KeyValue(const ON_ClassArray<ON_UserString>* pList, int i, bool key)
{
  const RHMONO_STRING* rc = NULL;
  if( pList && i>=0 && i<pList->Count() )
  {
    if( key )
      theKeyValueHolder.Set( (*pList)[i].m_key );
    else
      theKeyValueHolder.Set( (*pList)[i].m_string_value );
    rc = theKeyValueHolder.Array();
  }
  return rc;
}

RH_C_FUNCTION void ON_UserStringList_Delete(ON_ClassArray<ON_UserString>* pList)
{
  if( pList )
    delete pList;
}