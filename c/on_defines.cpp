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