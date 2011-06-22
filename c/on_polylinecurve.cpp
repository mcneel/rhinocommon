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

// return number of points in a certain polyline curve
RH_C_FUNCTION int ON_SimpleArray_PolylineCurve_GetCount(ON_SimpleArray<ON_PolylineCurve*>* pPolylineCurves, int i)
{
  int rc = 0;
  if( pPolylineCurves && i>=0 && i<pPolylineCurves->Count() )
  {
    ON_PolylineCurve* polyline = (*pPolylineCurves)[i];
    if( polyline )
      rc = polyline->PointCount();
  }
  return rc;
}

RH_C_FUNCTION void ON_SimpleArray_PolylineCurve_GetPoints(ON_SimpleArray<ON_PolylineCurve*>* pPolylineCurves, int i, int point_count, /*ARRAY*/ON_3dPoint* points)
{
  if( NULL==pPolylineCurves || i<0 || i>=pPolylineCurves->Count() || point_count<0 || NULL==points || NULL==*points)
    return;
  ON_PolylineCurve* polyline = (*pPolylineCurves)[i];
  if( NULL==polyline || polyline->PointCount()!=point_count )
    return;

 
  const ON_3dPoint* source = polyline->m_pline.Array();
  ::memcpy(points, source, sizeof(ON_3dPoint) * point_count);
}

RH_C_FUNCTION void ON_SimpleArray_PolylineCurve_Delete(ON_SimpleArray<ON_PolylineCurve*>* pPolylineCurves, bool delete_individual_curves)
{
  if( pPolylineCurves )
  {
    if( delete_individual_curves )
    {
      for( int i=0; i<pPolylineCurves->Count(); i++ )
      {
        ON_PolylineCurve* pCurve = (*pPolylineCurves)[i];
        if( pCurve )
          delete pCurve;
      }
    }
    delete pPolylineCurves;
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