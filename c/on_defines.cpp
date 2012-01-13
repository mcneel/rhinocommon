#include "StdAfx.h"

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
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pStringHolder )
  {
    ON_wString s = ON::Revision();
    pStringHolder->Set(s);
  }
#endif
}

RH_C_FUNCTION ON_wString* ON_wString_New()
{
  return new ON_wString();
}

RH_C_FUNCTION void ON_wString_Delete(ON_wString* pString)
{
  if( pString )
    delete pString;
}

RH_C_FUNCTION const wchar_t* ON_wString_Get(ON_wString* pString)
{
  const wchar_t* rc = NULL;
  if( pString )
    rc = pString->Array();
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