#include "StdAfx.h"

RH_C_FUNCTION void ON_Begin()
{
#if defined(OPENNURBS_BUILD) // don't call Begin when running in Rhino
  ON::Begin();
#endif
}

RH_C_FUNCTION double ONC_UnitScale(int from, int to)
{
  ON::unit_system usFrom = ON::UnitSystem(from);
  ON::unit_system usTo = ON::UnitSystem(to);
  return ON::UnitScale(usFrom, usTo);
}

RH_C_FUNCTION int ON_Version()
{
  return ON::Version();
}

RH_C_FUNCTION void ON_Revision(CRhCmnStringHolder* pStringHolder)
{
  if( pStringHolder )
  {
    ON_wString s = ON::SourceRevision();
    pStringHolder->Set(s);
  }
}

RH_C_FUNCTION ON_wString* ON_wString_New(const RHMONO_STRING* _text)
{
  INPUTSTRINGCOERCE(text, _text);
  if( text )
    return new ON_wString(text);
  
  return new ON_wString();
}

RH_C_FUNCTION void ON_wString_Delete(ON_wString* pString)
{
  if( pString )
    delete pString;
}

static CRhCmnStringHolder string_get_holder;
RH_C_FUNCTION const RHMONO_STRING* ON_wString_Get(ON_wString* pString)
{
  const RHMONO_STRING* rc = NULL;
  if( pString )
  {
#if defined (__APPLE__)
    string_get_holder.Set(*pString);
    rc = string_get_holder.Array();
#else
    rc = pString->Array();
#endif
  }
  return rc;
}

RH_C_FUNCTION void ON_wString_Set(ON_wString* pString, const RHMONO_STRING* _text)
{
  if( pString )
  {
    INPUTSTRINGCOERCE(text, _text);
    (*pString) = text;
  }
}


RH_C_FUNCTION unsigned int ON_CRC32_Compute(unsigned int current_remainder, int count, /*ARRAY*/ const char* bytes)
{
  return ON_CRC32(current_remainder, count, bytes);
}