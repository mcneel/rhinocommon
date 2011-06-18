#include "StdAfx.h"

RH_C_FUNCTION ON_PolylineCurve* ON_PolylineCurve_New( ON_PolylineCurve* pOther )
{
  if( pOther )
    return new ON_PolylineCurve(*pOther);
  return new ON_PolylineCurve();
}

RH_C_FUNCTION ON_PolylineCurve* ON_PolylineCurve_New2(int point_count, /*ARRAY*/const ON_3dPoint* points)
{
  if( point_count<1 || NULL==points )
    return new ON_PolylineCurve();
  
  CHack3dPointArray pts(point_count, (ON_3dPoint*)points);
  return new ON_PolylineCurve(pts);
}

RH_C_FUNCTION int ON_PolylineCurve_PointCount(const ON_PolylineCurve* pCurve)
{
  int rc = 0;
  if( pCurve )
    rc = pCurve->PointCount();
  return rc;
}

RH_C_FUNCTION void ON_PolylineCurve_GetSetPoint(ON_PolylineCurve* pCurve, int index, ON_3dPoint* point, bool set)
{
  if( pCurve && point && index>=0 && index<pCurve->m_pline.Count() )
  {
    if( set )
      pCurve->m_pline[index] = *point;
    else
      *point = pCurve->m_pline[index];
  }
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION void ON_PolylineCurve_Draw(const ON_PolylineCurve* pCrv, CRhinoDisplayPipeline* pDisplayPipeline, int argb, int thickness)
{
  if( pCrv && pDisplayPipeline )
  {
    int abgr = ARGB_to_ABGR(argb);
    pDisplayPipeline->DrawPolyline(pCrv->m_pline, abgr, thickness);
  }
}
#endif