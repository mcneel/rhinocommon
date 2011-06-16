#include "StdAfx.h"

RH_C_FUNCTION ON_Linetype* ON_Linetype_New()
{
  return new ON_Linetype();
}

RH_C_FUNCTION void ON_Linetype_Default(ON_Linetype* pLinetype)
{
  if( pLinetype )
    pLinetype->Default();
}

RH_C_FUNCTION void ON_Linetype_GetLinetypeName(const ON_Linetype* pLinetype, CRhCmnStringHolder* pStringHolder)
{
  if( pLinetype && pStringHolder)
    pStringHolder->Set( pLinetype->LinetypeName() );
}

RH_C_FUNCTION void ON_Linetype_SetLinetypeName(ON_Linetype* pLinetype, const RHMONO_STRING* _name)
{
  INPUTSTRINGCOERCE(name, _name);
  if( pLinetype )
  {
    pLinetype->SetLinetypeName(name);
  }
}

RH_C_FUNCTION int ON_Linetype_GetInt(const ON_Linetype* pLinetype, int which)
{
  const int idxLinetypeIndex = 0;
  const int idxSegmentCount = 1;
  int rc = -1;
  if( pLinetype )
  {
    switch(which)
    {
    case idxLinetypeIndex:
      rc = pLinetype->LinetypeIndex();
      break;
    case idxSegmentCount:
      rc = pLinetype->SegmentCount();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Linetype_SetInt(ON_Linetype* pLinetype, int which, int val)
{
  const int idxLinetypeIndex = 0;
  if( pLinetype )
  {
    switch(which)
    {
    case idxLinetypeIndex:
      pLinetype->SetLinetypeIndex(val);
      break;
    }
  }
}

RH_C_FUNCTION double ON_Linetype_PatternLength(const ON_Linetype* pLinetype)
{
  double rc = 0;
  if( pLinetype )
    rc = pLinetype->PatternLength();
  return rc;
}

RH_C_FUNCTION ON_UUID ON_Linetype_GetGuid(const ON_Linetype* pLinetype)
{
  if( pLinetype )
    return pLinetype->ModelObjectId();
  return ::ON_nil_uuid;
}

RH_C_FUNCTION void ON_Linetype_SetGuid(ON_Linetype* pLinetype, ON_UUID value)
{
  if( pLinetype )
    pLinetype->m_linetype_id = value;
}

RH_C_FUNCTION int ON_Linetype_AppendSegment(ON_Linetype* pLinetype, double length, bool isSolid)
{
  int rc = -1;
  if( pLinetype )
  {
    ON_LinetypeSegment seg;
    seg.m_length = length;
    seg.m_seg_type = isSolid?ON_LinetypeSegment::stLine : ON_LinetypeSegment::stSpace;
    rc = pLinetype->AppendSegment(seg);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Linetype_RemoveSegment(ON_Linetype* pLinetype, int index)
{
  bool rc = false;
  if( pLinetype )
  {
    rc = pLinetype->RemoveSegment(index);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Linetype_SetSegment(ON_Linetype* pLinetype, int index, double length, bool isSolid)
{
  bool rc = false;
  if( pLinetype )
  {
    ON_LinetypeSegment::eSegType lttype = isSolid?ON_LinetypeSegment::stLine : ON_LinetypeSegment::stSpace;
    rc = pLinetype->SetSegment(index, length, lttype);
  }
  return rc;
}

RH_C_FUNCTION void ON_Linetype_GetSegment(const ON_Linetype* pConstLinetype, int index, double* length, bool* isSolid)
{
  if( pConstLinetype && length && isSolid )
  {
    ON_LinetypeSegment seg = pConstLinetype->Segment(index);
    *length = seg.m_length;
    *isSolid = seg.m_seg_type==ON_LinetypeSegment::stLine;
  }
}