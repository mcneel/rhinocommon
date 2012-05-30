#include "StdAfx.h"

void CopyToCircleStruct(ON_CIRCLE_STRUCT& cs, const ON_Circle& circle)
{
  memcpy(&cs, &circle, sizeof(ON_CIRCLE_STRUCT));
}

ON_Circle FromCircleStruct(const ON_CIRCLE_STRUCT& cs)
{
  ON_Circle circle;
  memcpy(&circle, &cs, sizeof(ON_CIRCLE_STRUCT));
  circle.plane.UpdateEquation();
  return circle;
}


RH_C_FUNCTION void ON_Circle_Create3Pt(ON_CIRCLE_STRUCT* c, ON_3DPOINT_STRUCT p, ON_3DPOINT_STRUCT q, ON_3DPOINT_STRUCT r)
{
  if( c )
  {
    const ON_3dPoint* _p = (const ON_3dPoint*)&p;
    const ON_3dPoint* _q = (const ON_3dPoint*)&q;
    const ON_3dPoint* _r = (const ON_3dPoint*)&r;
    ON_Circle _c(*_p, *_q, *_r);
    _c.plane.UpdateEquation();
    CopyToCircleStruct(*c, _c);
  }
}

RH_C_FUNCTION bool ON_Circle_CreatePtVecPt(ON_CIRCLE_STRUCT* c, ON_3DPOINT_STRUCT p, ON_3DVECTOR_STRUCT tan_at_p, ON_3DPOINT_STRUCT q)
{
  bool rc = false;
  if( c )
  {
    const ON_3dPoint* _p = (const ON_3dPoint*)&p;
    const ON_3dVector* _tan_at_p = (const ON_3dVector*)&tan_at_p;
    const ON_3dPoint* _q = (const ON_3dPoint*)&q;
    ON_Circle _c;
    rc = _c.Create( *_p, *_tan_at_p, *_q );
    _c.plane.UpdateEquation();
    CopyToCircleStruct(*c, _c);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Circle_IsInPlane(const ON_CIRCLE_STRUCT* c, const ON_PLANE_STRUCT* plane, double tolerance)
{
  bool rc = false;
  if ( c && plane )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    ON_Circle circle = FromCircleStruct(*c);
    rc = circle.IsInPlane(temp, tolerance);
  }
  return rc;
}


RH_C_FUNCTION void ON_Circle_BoundingBox(const ON_CIRCLE_STRUCT* c, ON_BoundingBox* bbox)
{
  if( c && bbox )
  {
    ON_Circle circle = FromCircleStruct(*c);
    *bbox = circle.BoundingBox();
  }
}

RH_C_FUNCTION bool ON_Circle_Transform( ON_CIRCLE_STRUCT* c, ON_Xform* xf)
{
  bool rc = false;
  if( c && xf )
  {
    ON_Circle circle = FromCircleStruct(*c);
    rc = circle.Transform(*xf);
    CopyToCircleStruct(*c, circle);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Circle_ClosestPointTo( const ON_CIRCLE_STRUCT* c,
                                             ON_3DPOINT_STRUCT testPoint,
                                             double* t)
{
  bool rc = false;
  const ON_3dPoint* _testPoint = (const ON_3dPoint*)&testPoint;

  if (c)
  {
    ON_Circle circle = FromCircleStruct(*c);
    rc = circle.ClosestPointTo(*_testPoint, t);
  }

  return rc;
}

RH_C_FUNCTION int ON_Circle_GetNurbForm(const ON_CIRCLE_STRUCT* pCircle, ON_NurbsCurve* nurbs_curve)
{
  int rc = 0;
  if( pCircle && nurbs_curve )
  {
    ON_Circle circle = FromCircleStruct(*pCircle);
    rc = circle.GetNurbForm(*nurbs_curve);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Circle_TryFitTTT(const ON_Curve* c1, const ON_Curve* c2, const ON_Curve* c3, 
                                       double seed1, double seed2, double seed3, 
                                       ON_CIRCLE_STRUCT* circleFit)
{
#if !defined(OPENNURBS_BUILD)
  if( !c1 || !c2 || !c3 ) { return false; }
  if( !circleFit ) { return false; }

  //copied this (with modifications) from CRhGetCircleTTT::CalculateCircleTanTanTan
  double d1=0.0, d2=0.0, d3=0.0;
  double t1 = seed1;
  double t2 = seed2;
  double t3 = seed3;

  double tol = RhinoCurveTangencyTolerance();
  bool found = false;

  // initialize points and tangents
  ON_3dPoint pt1 = c1->PointAt(t1);
  ON_3dPoint pt2 = c2->PointAt(t2);
  ON_3dPoint pt3 = c3->PointAt(t3);
  ON_3dVector tan1 = c1->TangentAt(t1);
  ON_3dVector tan2 = c2->TangentAt(t2);
  ON_3dVector tan3 = c3->TangentAt(t3);

  ON_Circle circle;
  for ( int i=0; i<20; i++)
  {
    // make a guess
    if ( !circle.Create(pt1, pt2, pt3))
      break;

    // check how good the fit is
    { //Check against first curve
      double t;
      circle.ClosestPointTo(pt1, &t);
      ON_3dVector tan = circle.TangentAt(t);
      ON_3dVector n = circle.Center()-pt1;
      n.Unitize();
      d1 = fabs( ON_DotProduct( tan1, n));
    }

    { //Check against second curve
      double t;
      circle.ClosestPointTo( pt2, &t);
      ON_3dVector tan = circle.TangentAt( t);
      ON_3dVector n = circle.Center()-pt2;
      n.Unitize();
      d2 = fabs( ON_DotProduct( tan2, n));
    }

    { //Check against third curve
      double t;
      circle.ClosestPointTo( pt3, &t);
      ON_3dVector tan = circle.TangentAt( t);
      ON_3dVector n = circle.Center()-pt3;
      n.Unitize();
      d3 = fabs( ON_DotProduct( tan3, n));
    }

    // If the fit is close enough, abort.
    if ( d1 < tol && d2 < tol && d3 < tol)
    {
      found = true;
      break;
    }

    // improve the guess by dropping the circle center perpendicular to the input curves close to the pick points
    ON_3dPoint center = circle.Center();

    {
      if ( !TL_GetLocalPerpPoint(*c1, center, seed1, &t1))
        break;
      c1->EvTangent( t1, pt1, tan1);
    }

    {
      if ( !TL_GetLocalPerpPoint( *c2, center, seed2, &t2))
        break;
      c2->EvTangent( t2, pt2, tan2);
    }

    {
      if ( !TL_GetLocalPerpPoint( *c3, center, seed3, &t3))
        break;
      c3->EvTangent( t3, pt3, tan3);
    }
  }

  if (found)
  {
    CopyToCircleStruct(*circleFit, circle);
    return true;
  }
#endif
  return false;
}

RH_C_FUNCTION bool ON_Circle_TryFitTT(const ON_Curve* c1, const ON_Curve* c2, 
                                      double seed1, double seed2,
                                      ON_CIRCLE_STRUCT* circleFit)
{
#if !defined(OPENNURBS_BUILD)
  if( !c1 || !c2 ) { return false; }
  if( !circleFit ) { return false; }

  //copied this (with modifications) from CRhGetCircleTTT::CalculateCircleTanTan
  double t1 = seed1;
  double t2 = seed2;

  double tol = RhinoCurveTangencyTolerance();

  // initialize points and tangents
  ON_3dPoint pt1 = c1->PointAt(t1);
  ON_3dPoint pt2 = c2->PointAt(t2);
  ON_3dVector tan1 = c1->TangentAt(t1);
  ON_3dVector tan2 = c2->TangentAt(t2);

  ON_Circle circle;

  for ( int i=0; i<20; i++)
  {
    // make a guess
    if ( !circle.Create(pt2, tan2, pt1))
      break;

    // check how good the fit is
    ON_3dVector n = circle.Center() - pt1;
    if ( n.Unitize())
    {
      ON_3dVector tan1 = c1->TangentAt( t1);
      double d = fabs( ON_DotProduct( n, tan1));

      if ( d < tol)
      {
        CopyToCircleStruct(*circleFit, circle);
        return true;
      }
    }

    // get perp to first curve from the second point
    if ( !TL_GetLocalPerpPoint( *c1, circle.Center(), t1, &t1))
      break;
    pt1 = c1->PointAt( t1);

  }
#endif
  return false;
 }