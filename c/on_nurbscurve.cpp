#include "StdAfx.h"


RH_C_FUNCTION ON_NurbsCurve* ON_NurbsCurve_New( ON_NurbsCurve* pOther )
{
  if( pOther )
    return ON_NurbsCurve::New(*pOther);
  return ON_NurbsCurve::New();
}

RH_C_FUNCTION bool ON_NurbsCurve_IsDuplicate(ON_NurbsCurve* crv1, ON_NurbsCurve* crv2, bool ignore, double tol )
{
  if( NULL==crv1 || NULL ==crv2 )
    return false;
  return crv1->IsDuplicate(*crv2, ignore, tol);
}

RH_C_FUNCTION bool ON_NurbsCurve_Create(ON_NurbsCurve* crv, int dim, bool rat, int order, int cv_count)
{
  if( NULL == crv )
    return false;
  return crv->Create(dim, rat?TRUE:FALSE, order, cv_count);
}

RH_C_FUNCTION bool ON_NurbsCurve_CreateClampedUniformNurbs(ON_NurbsCurve* crv, int dim, int order, int count, /*ARRAY*/const ON_3dPoint* pts, double knot_delta)
{
  if( NULL==crv || NULL == pts )
    return false;
  const ON_3dPoint* _pts = (const ON_3dPoint*)pts;
  return crv->CreateClampedUniformNurbs(dim, order, count, _pts, knot_delta);
}

RH_C_FUNCTION bool ON_NurbsCurve_CreatePeriodicUniformNurbs(ON_NurbsCurve* crv, int dim, int order, int count, /*ARRAY*/const ON_3dPoint* pts, double knot_delta)
{
  if( NULL==crv || NULL == pts )
    return false;
  return crv->CreatePeriodicUniformNurbs(dim, order, count, pts, knot_delta);
}

RH_C_FUNCTION ON_NurbsCurve* ON_NurbsCurve_CreateControlPointCurve(int count, /*ARRAY*/const ON_3dPoint* points, int degree)
{
  if( count < 2 || NULL == points )
    return NULL;

  int order = ( count <= degree ) ? count : degree + 1;
  ON_NurbsCurve* pNC = ON_NurbsCurve::New();
  if( points[0].DistanceTo(points[count-1]) < ON_SQRT_EPSILON )
    pNC->CreatePeriodicUniformNurbs( 3, order, count-1, points );
  else
    pNC->CreateClampedUniformNurbs( 3, order, count, points );

  if( !pNC->IsValid() )
  {
    delete pNC;
    return NULL;
  }
  return pNC;
}

RH_C_FUNCTION bool ON_NurbsCurve_GetBool(ON_NurbsCurve* pCurve, int which)
{
  const int idxIsRational = 0;
  const int idxIsClampedStart = 1;
  const int idxIsClampedEnd = 2;
  const int idxZeroCVs = 3;
  const int idxClampStart = 4;
  const int idxClampEnd = 5;
  const int idxMakeRational = 6;
  const int idxMakeNonRational = 7;
  const int idxHasBezierSpans = 8;
  bool rc = false;
  if( pCurve )
  {
    switch(which)
    {
    case idxIsRational:
      rc = pCurve->IsRational();
      break;
    case idxIsClampedStart:
      rc = pCurve->IsClamped(0);
      break;
    case idxIsClampedEnd:
      rc = pCurve->IsClamped(1);
      break;
    case idxZeroCVs:
      rc = pCurve->ZeroCVs();
      break;
    case idxClampStart:
      rc = pCurve->ClampEnd(0);
      break;
    case idxClampEnd:
      rc = pCurve->ClampEnd(1);
      break;
    case idxMakeRational:
      rc = pCurve->MakeRational();
      break;
    case idxMakeNonRational:
      rc = pCurve->MakeNonRational();
      break;
    case idxHasBezierSpans:
      rc = pCurve->HasBezierSpans();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_NurbsCurve_GetInt( const ON_NurbsCurve* pCurve, int which )
{
  const int idxCVSize = 0;
  const int idxOrder = 1;
  const int idxCVCount = 2;
  const int idxKnotCount = 3;
  const int idxCVStyle = 4;
  int rc = 0;
  if( pCurve )
  {
    switch(which)
    {
    case idxCVSize:
      rc = pCurve->CVSize();
      break;
    case idxOrder:
      rc = pCurve->Order();
      break;
    case idxCVCount:
      rc = pCurve->CVCount();
      break;
    case idxKnotCount:
      rc = pCurve->KnotCount();
      break;
    case idxCVStyle:
      rc = pCurve->CVStyle();
      break;
    }
  }
  return rc;
}

//RH_C_FUNCTION bool ON_NurbsCurve_SetCV(ON_NurbsCurve* pCurve, int cvIndex, ON_3DPOINT_STRUCT point)
//{
//  bool rc = false;
//  if( pCurve )
//  {
//    const ON_3dPoint* _point = (const ON_3dPoint*)&point;
//    rc = pCurve->SetCV(cvIndex, *_point)?true:false;
//  }
//  return rc;
//}

RH_C_FUNCTION double ON_NurbsCurve_SuperfluousKnot(const ON_NurbsCurve* pConstNurbsCurve, int end)
{
  double rc = 0;
  if( pConstNurbsCurve )
    rc = pConstNurbsCurve->SuperfluousKnot(end);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_SetCV2(ON_NurbsCurve* pCurve, int cvIndex, ON_4dPoint* point)
{
  bool rc = false;
  if( pCurve )
  {
    if( point->w != 1.0 )
    {
      point->x *= point->w;
      point->y *= point->w;
      point->z *= point->w;

      if( !pCurve->IsRational() )
      {
        pCurve->MakeRational();
      }
    }

    rc = pCurve->SetCV(cvIndex, *point)?true:false;
  }
  return rc;
}

//RH_C_FUNCTION bool ON_NurbsCurve_GetCV(const ON_NurbsCurve* pCurve, int cvIndex, ON_3dPoint* point)
//{
//  bool rc = false;
//  if( pCurve && point )
//  {
//    rc = pCurve->GetCV(cvIndex, *point)?true:false;
//  }
//  return rc;
//}

RH_C_FUNCTION bool ON_NurbsCurve_GetCV2(const ON_NurbsCurve* pCurve, int cvIndex, ON_4dPoint* point)
{
  bool rc = false;
  if( pCurve && point )
  {
    rc = pCurve->GetCV(cvIndex, *point)?true:false;

    if( point->w != 1.0)
    {
      double f = 1.0 / point->w;
      point->x *= f;
      point->y *= f;
      point->z *= f;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_SetKnot(ON_NurbsCurve* pCurve, int knotIndex, double knotValue)
{
  bool rc = false;
  if( pCurve )
  {
    rc = pCurve->SetKnot(knotIndex, knotValue);
  }
  return rc;
}

RH_C_FUNCTION double ON_NurbsCurve_Knot(const ON_NurbsCurve* crv, int knotIndex)
{
  double rc = 0;
  if( crv )
    rc = crv->Knot(knotIndex);
  return rc;
}

RH_C_FUNCTION int ON_NurbsCurve_KnotMultiplicity(const ON_NurbsCurve* crv, int knotIndex)
{
  int rc = 0;
  if( crv )
    rc = crv->KnotMultiplicity(knotIndex);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_MakeUniformKnotVector(ON_NurbsCurve* pCurve, double delta, bool clamped)
{
  bool rc = false;
  if( pCurve )
  {
    if( clamped )
      rc = pCurve->MakeClampedUniformKnotVector(delta);
    else
      rc = pCurve->MakePeriodicUniformKnotVector(delta);
  }
  return rc;
}

RH_C_FUNCTION double ON_NurbsCurve_GrevilleAbcissa(const ON_NurbsCurve* pCurve, int index)
{
  double rc = 0;
  if( pCurve )
    rc = pCurve->GrevilleAbcissa(index);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_GetGrevilleAbcissae(const ON_NurbsCurve* pCurve, int count, /*ARRAY*/double* ga)
{
  bool rc = false;
  if( pCurve && ga )
    rc = pCurve->GetGrevilleAbcissae(ga);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_InsertKnot(ON_NurbsCurve* pCurve, double knotValue, int knotMultiplicity)
{
  bool rc = false;
  if( pCurve )
  {
    rc = pCurve->InsertKnot(knotValue, knotMultiplicity);
  }
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_IncreaseDegree(ON_NurbsCurve* pCurve, int desiredDegree)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->IncreaseDegree(desiredDegree);
  return rc;
}

RH_C_FUNCTION double ON_NurbsCurve_ControlPolygonLength(const ON_NurbsCurve* pCurve)
{
  double rc = 0.0;
  if( pCurve )
    rc = pCurve->ControlPolygonLength();
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_MakePiecewiseBezier(ON_NurbsCurve* pCurve, bool setEndWeightsToOne)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->MakePiecewiseBezier(setEndWeightsToOne);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_ChangeEndWeights(ON_NurbsCurve* pCurve, double w0, double w1)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->ChangeEndWeights(w0,w1);
  return rc;
}

RH_C_FUNCTION bool ON_NurbsCurve_Reparameterize(ON_NurbsCurve* pCurve, double c)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->Reparameterize(c);
  return rc;
}

#if !defined(OPENNURBS_BUILD)  //not available in opennurbs build

// 8-Feb-2013 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-15661
RH_C_FUNCTION bool RHC_RhinoCreateSpiral0( ON_3DPOINT_STRUCT axis_start, ON_3DVECTOR_STRUCT axis_dir, ON_3DPOINT_STRUCT radius_point, double pitch, double turn_count, double radius0, double radius1, ON_NurbsCurve* pCurve )
{
  bool rc = false;
  if( pCurve )
  {
    ON_3dPoint axisStart(axis_start.val);
    ON_3dVector axisDir(axis_dir.val);
    ON_3dPoint radiusPoint(radius_point.val);
    rc = RhinoCreateSpiral( axisStart, axisDir, radiusPoint, pitch, turn_count, radius0, radius1, *pCurve );
    if( rc && pCurve )
      rc = pCurve->IsValid() ? true : false;
  }
  return rc;
}

// 8-Feb-2013 Dale Fugier, http://mcneel.myjetbrains.com/youtrack/issue/RH-15661
RH_C_FUNCTION bool RHC_RhinoCreateSpiral1( const ON_Curve* pRail, double rail_t0, double rail_t1, ON_3DPOINT_STRUCT radius_point, double pitch, double turn_count, double radius0, double radius1, int points_per_turn, ON_NurbsCurve* pCurve )
{
  bool rc = false;
  if( pRail && pCurve )
  {
    ON_3dPoint radiusPoint(radius_point.val);
    rc = RhinoCreateSpiral( *pRail, rail_t0, rail_t1, radiusPoint, pitch, turn_count, radius0, radius1, points_per_turn, *pCurve );
    if( rc && pCurve )
      rc = pCurve->IsValid() ? true : false;
  }
  return rc;
}

#endif

/// This should eventually move to an on_ellipse.cpp file
RH_C_FUNCTION int ON_Ellipse_GetNurbForm(ON_Ellipse* ellipse, ON_NurbsCurve* pNurbsCurve)
{
  int rc = 0;
  if( ellipse && pNurbsCurve )
    rc = ellipse->GetNurbForm(*pNurbsCurve);
  return rc;
}
//David: I think the above fails because Ellipse has a Plane whose memory layout is not synonymous with ON_Plane
RH_C_FUNCTION int ON_Ellipse_GetNurbForm2( const ON_PLANE_STRUCT* plane, double r0, double r1, ON_NurbsCurve* pNurbsCurve )
{
  int rc = 0;
  if( plane && pNurbsCurve )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    ON_Ellipse ell(temp, r0, r1);
    rc = ell.GetNurbForm(*pNurbsCurve);
  }
  return rc;
}