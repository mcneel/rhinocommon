#include "StdAfx.h"

RH_C_FUNCTION ON_BezierCurve* ON_BezierCurve_New2d(int count, /*ARRAY*/const ON_2dPoint* points)
{
  ON_BezierCurve* rc = NULL;
  if( count && points )
  {
    ON_2dPointArray pts;
    pts.Append(count, points);
    rc = new ON_BezierCurve(pts);
    pts.KeepArray();
  }
  return rc;
}

RH_C_FUNCTION ON_BezierCurve* ON_BezierCurve_New3d(int count, /*ARRAY*/const ON_3dPoint* points)
{
  ON_BezierCurve* rc = NULL;
  if( count && points )
  {
    ON_3dPointArray pts;
    pts.Append(count, points);
    rc = new ON_BezierCurve(pts);
    pts.KeepArray();
  }
  return rc;
}

RH_C_FUNCTION ON_BezierCurve* ON_BezierCurve_New4d(int count, /*ARRAY*/const ON_4dPoint* points)
{
  ON_BezierCurve* rc = NULL;
  if( count && points )
  {
    ON_4dPointArray pts;
    pts.Append(count, points);
    rc = new ON_BezierCurve(pts);
    pts.KeepArray();
  }
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_IsValid(const ON_BezierCurve* pConstBezierCurve)
{
  bool rc = false;
  if( pConstBezierCurve )
  {
    rc = pConstBezierCurve->IsValid();
  }
  return rc;
}

RH_C_FUNCTION void ON_BezierCurve_Dump(const ON_BezierCurve* pConstBezierCurve, CRhCmnStringHolder* pStringHolder)
{
  if( pConstBezierCurve && pStringHolder )
  {
    ON_wString s;
    ON_TextLog textlog(s);
    pConstBezierCurve->Dump(textlog);
    pStringHolder->Set(s);
  }
}

RH_C_FUNCTION int ON_BezierCurve_Dimension(const ON_BezierCurve* pConstBezierCurve)
{
  int rc = 0;
  if( pConstBezierCurve )
    rc = pConstBezierCurve->Dimension();
  return rc;
}

RH_C_FUNCTION ON_BezierCurve* ON_BezierCurve_Loft(int count, /*ARRAY*/const ON_3dPoint* points)
{
  ON_BezierCurve* rc = NULL;
  if( count && points )
  {
    rc = new ON_BezierCurve();
    ON_3dPointArray _pts(count);
    _pts.Append(count, points);
    if( !rc->Loft(_pts) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_BezierCurve* ON_BezierCurve_Loft2(int count, /*ARRAY*/const ON_2dPoint* points)
{
  ON_BezierCurve* rc = NULL;
  if( count && points )
  {
    rc = new ON_BezierCurve();
    if( rc->Loft(2, count, 2, &(points->x), 0, 0) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_BezierCurve_BoundingBox(const ON_BezierCurve* pConstBezierCurve, bool accurate, ON_BoundingBox* bounding_box)
{
  if( pConstBezierCurve && bounding_box )
  {
    if( accurate )
      pConstBezierCurve->GetTightBoundingBox(*bounding_box);
    else
      *bounding_box = pConstBezierCurve->BoundingBox();
  }
}

RH_C_FUNCTION void ON_BezierCurve_PointAt(const ON_BezierCurve* pConstBezierCurve, double t, ON_3dPoint* point)
{
  if( pConstBezierCurve && point )
    *point = pConstBezierCurve->PointAt(t);
}

RH_C_FUNCTION void ON_BezierCurve_TangentAt(const ON_BezierCurve* pConstBezierCurve, double t, ON_3dVector* tangent)
{
  if( pConstBezierCurve && tangent )
    *tangent = pConstBezierCurve->TangentAt(t);
}

RH_C_FUNCTION void ON_BezierCurve_CurvatureAt(const ON_BezierCurve* pConstBezierCurve, double t, ON_3dVector* tangent)
{
  if( pConstBezierCurve && tangent )
    *tangent = pConstBezierCurve->CurvatureAt(t);
}

RH_C_FUNCTION ON_NurbsCurve* ON_BezierCurve_GetNurbForm(const ON_BezierCurve* pConstBezierCurve)
{
  ON_NurbsCurve* rc = NULL;
  if( pConstBezierCurve )
  {
    rc = new ON_NurbsCurve();
    if( !pConstBezierCurve->GetNurbForm(*rc) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_IsRational(const ON_BezierCurve* pConstBezierCurve)
{
  bool rc = false;
  if( pConstBezierCurve )
    rc = pConstBezierCurve->IsRational();
  return rc;
}

RH_C_FUNCTION int ON_BezierCurve_CVCount(const ON_BezierCurve* pConstBezierCurve)
{
  int rc = 0;
  if( pConstBezierCurve )
    rc = pConstBezierCurve->CVCount();
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_GetCV3d(const ON_BezierCurve* pConstBezierCurve, int index, ON_3dPoint* point)
{
  bool rc = false;
  if( pConstBezierCurve && point )
    rc = pConstBezierCurve->GetCV(index, *point);
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_GetCV4d(const ON_BezierCurve* pConstBezierCurve, int index, ON_4dPoint* point)
{
  bool rc = false;
  if( pConstBezierCurve && point )
    rc = pConstBezierCurve->GetCV(index, *point);
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_MakeRational(ON_BezierCurve* pBezierCurve, bool on)
{
  bool rc = false;
  if( pBezierCurve )
  {
    if( on )
      rc = pBezierCurve->MakeRational();
    else
      rc = pBezierCurve->MakeNonRational();
  }
  return rc;
}

RH_C_FUNCTION bool ON_BezierCurve_ChangeInt(ON_BezierCurve* pBezierCurve, bool degree, int newInt)
{
  bool rc = false;
  if( pBezierCurve )
  {
    if( degree )
      rc = pBezierCurve->IncreaseDegree(newInt);
    else
      rc = pBezierCurve->ChangeDimension(newInt);
  }
  return rc;
}