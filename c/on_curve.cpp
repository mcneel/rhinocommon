#include "StdAfx.h"

RH_C_FUNCTION bool ON_Curve_Domain( ON_Curve* pCurve, bool set, ON_Interval* ival )
{
  bool rc = false;
  if( pCurve && ival )
  {
    if( set )
    {
      rc = pCurve->SetDomain(*ival);
    }
    else
    {
      *ival = pCurve->Domain();
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_Curve* ON_Curve_DuplicateCurve(ON_Curve* pCurve)
{
  ON_Curve* rc = NULL;
  if( pCurve )
    rc = pCurve->DuplicateCurve();
  return rc;
}

RH_C_FUNCTION bool ON_Curve_ChangeDimension(ON_Curve* pCurve, int desired_dimension)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->ChangeDimension(desired_dimension);
  return rc;
}

RH_C_FUNCTION bool ON_Curve_ChangeClosedCurveSeam(ON_Curve* pCurve, double t)
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->ChangeClosedCurveSeam(t)?true:false;
  return rc;
}

RH_C_FUNCTION int ON_Curve_SpanCount(const ON_Curve* pConstCurve)
{
  int rc = 0;
  if( pConstCurve )
    rc = pConstCurve->SpanCount();
  return rc;
}

RH_C_FUNCTION bool ON_Curve_SpanInterval(const ON_Curve* pConstCurve, int spanIndex, ON_Interval* spanDomain)
{
  bool rc = false;
  if( pConstCurve && spanDomain )
  {
    int count = pConstCurve->SpanCount();
    if( spanIndex >= 0 && spanIndex < count )
    {
      double* knots = new double[count + 1];
      pConstCurve->GetSpanVector(knots); 
      spanDomain->Set(knots[spanIndex], knots[spanIndex+1]);

      delete [] knots;
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Curve_Dimension(const ON_Curve* pConstCurve)
{
  int rc = 0;
  if( pConstCurve )
    rc = pConstCurve->Dimension();
  return rc;
}

RH_C_FUNCTION int ON_Curve_Degree(const ON_Curve* pConstCurve)
{
  int rc = 0;
  if( pConstCurve )
    rc = pConstCurve->Degree();
  return rc;
}

RH_C_FUNCTION int ON_Curve_HasNurbForm(const ON_Curve* pConstCurve)
{
  int rc = 0;
  if( pConstCurve )
    rc = pConstCurve->HasNurbForm();
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsLinear(const ON_Curve* pConstCurve, double tolerance)
{
  bool rc = false;
  if( pConstCurve )
    rc = pConstCurve->IsLinear(tolerance)?true:false;
  return rc;
}

RH_C_FUNCTION int ON_Curve_IsPolyline1( const ON_Curve* pConstCurve, ON_3dPointArray* points )
{
  int pointCount = 0;
  if( pConstCurve )
    pointCount = pConstCurve->IsPolyline(points);
  return pointCount;
}

RH_C_FUNCTION void ON_Curve_IsPolyline2( const ON_Curve* pCurve, ON_3dPointArray* points, int* pointCount, ON_SimpleArray<double>* t )
{
  if( NULL == pointCount || NULL == pCurve )
    return;

  *pointCount = pCurve->IsPolyline( points, t );
  if( 0 == pointCount )
    return;
}

RH_C_FUNCTION bool ON_Curve_IsArc( const ON_Curve* pCurve, int ignore, ON_PLANE_STRUCT* plane, ON_Arc* arc, double tolerance )
{
  bool rc = false;
  if( pCurve )
  {
    // ignore = 0 (none)
    // ignore = 1 (ignore plane)
    // ignore = 2 (ignore plane and arc)
    if( ignore>0 )
      plane = NULL;
    if( ignore>1 )
      arc = NULL;
    ON_Plane temp;
    ON_Plane* pPlane = NULL;
    if( plane )
    {
      temp = FromPlaneStruct(*plane);
      pPlane = &temp;
    }
    rc = pCurve->IsArc(pPlane,arc,tolerance)?true:false;
    if( plane )
      CopyToPlaneStruct(*plane, temp);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsEllipse( const ON_Curve* pCurve, int ignore, ON_PLANE_STRUCT* plane, ON_Ellipse* ellipse, double tolerance )
{
  bool rc = false;
  if( pCurve )
  {
    // ignore = 0 (none)
    // ignore = 1 (ignore plane)
    // ignore = 2 (ignore plane and ellipse)
    if( ignore>0 )
      plane = NULL;
    if( ignore>1 )
      ellipse = NULL;
    ON_Plane temp;
    ON_Plane* pPlane = NULL;
    if( plane )
    {
      temp = FromPlaneStruct(*plane);
      pPlane = &temp;
    }
    rc = pCurve->IsEllipse(pPlane,ellipse,tolerance);
    if( plane )
      CopyToPlaneStruct(*plane, temp);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsPlanar( const ON_Curve* pCurve, bool ignorePlane, ON_PLANE_STRUCT* plane, double tolerance )
{
  bool rc = false;
  if(ignorePlane)
    plane = NULL;
  if( pCurve )
  {
    ON_Plane temp;
    ON_Plane* pPlane = NULL;
    if( plane )
    {
      temp = FromPlaneStruct(*plane);
      pPlane = &temp;
    }
    rc = pCurve->IsPlanar(pPlane, tolerance)?true:false;
    if( plane )
      CopyToPlaneStruct(*plane, temp);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsInPlane(const ON_Curve* pCurve, const ON_PLANE_STRUCT* plane, double tolerance)
{
  bool rc = false;
  if( pCurve && plane )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    rc = pCurve->IsInPlane(temp,tolerance)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetBool( const ON_Curve* pCurve, int which )
{
  const int idxIsClosed = 0;
  const int idxIsPeriodic = 1;
  bool rc = false;
  if( pCurve )
  {
    if( idxIsClosed == which )
      rc = pCurve->IsClosed()?true:false;
    else if(idxIsPeriodic == which )
      rc = pCurve->IsPeriodic()?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_Reverse( ON_Curve* pCurve )
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->Reverse()?true:false;
  return rc;
}

RH_C_FUNCTION bool ON_Curve_SetPoint( ON_Curve* pCurve, ON_3DPOINT_STRUCT pt, bool startpoint )
{
  bool rc = false;
  if( pCurve )
  {
    const ON_3dPoint* _pt = (const ON_3dPoint*)&pt;
    if( startpoint )
      rc = pCurve->SetStartPoint(*_pt)?true:false;
    else
      rc = pCurve->SetEndPoint(*_pt)?true:false;
  }
  return rc;
}

RH_C_FUNCTION void ON_Curve_PointAt( const ON_Curve* pCurve, double t, ON_3dPoint* pt, int which )
{
  const int idxPointAtT = 0;
  const int idxPointAtStart = 1;
  const int idxPointAtEnd = 2;
  if( pCurve && pt )
  {
    if( idxPointAtT == which )
      *pt = pCurve->PointAt(t);
    else if( idxPointAtStart == which )
      *pt = pCurve->PointAtStart();
    else if( idxPointAtEnd == which )
      *pt = pCurve->PointAtEnd();
  }
}

RH_C_FUNCTION void ON_Curve_GetVector( const ON_Curve* pCurve, int which, double t, ON_3dVector* vec )
{
  const int idxDerivateAt = 0;
  const int idxTangentAt = 1;
  const int idxCurvatureAt = 2;
  if( pCurve && vec )
  {
    if( idxDerivateAt == which )
      *vec = pCurve->DerivativeAt(t);
    else if( idxTangentAt == which )
      *vec = pCurve->TangentAt(t);
    else if( idxCurvatureAt == which )
      *vec = pCurve->CurvatureAt(t);
  }
}

RH_C_FUNCTION bool ON_Curve_Evaluate( const ON_Curve* pCurve, int derivatives, int side, double t, ON_3dPointArray* outVectors )
{
  bool rc = false;
  
  if( pCurve && outVectors )
  {
    if( derivatives >= 0 )
    {
      outVectors->Reserve(derivatives+1);
      if (pCurve->Evaluate(t, derivatives, 3, &outVectors->Array()->x, side, 0))
      {
        outVectors->SetCount(derivatives+1);
        rc = true;
      }
    }
  }

  return rc;
}

RH_C_FUNCTION bool ON_Curve_FrameAt( const ON_Curve* pConstCurve, double t, ON_PLANE_STRUCT* plane, bool zero_twisting)
{
  bool rc = false;
  if( pConstCurve && plane )
  {
    ON_Plane temp;
#if defined(RHINO_V5SR) // only available in V5
    if( zero_twisting )
      rc = RhinoGetPerpendicularCurvePlane(pConstCurve, t, temp);
    else
      rc = pConstCurve->FrameAt(t, temp)?true:false;
#else
    rc = pConstCurve->FrameAt(t, temp)?true:false;
#endif
    CopyToPlaneStruct(*plane, temp);
  }
  return rc;
}

// not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_Curve_GetClosestPoint( const ON_Curve* pCurve, ON_3DPOINT_STRUCT test_point, double* t, double maximum_distance)
{
  bool rc = false;
  if( pCurve )
  {
    const ON_3dPoint* pt = (const ON_3dPoint*)&test_point;
    rc = pCurve->GetClosestPoint(*pt, t, maximum_distance);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetLength( const ON_Curve* pCurve, double* length, double fractional_tol, ON_INTERVAL_STRUCT sub_domain, bool ignoreSubDomain)
{
  const ON_Interval* _sub_domain = NULL;
  if( !ignoreSubDomain )
    _sub_domain = (const ON_Interval*)&sub_domain;
  bool rc = false;
  if( pCurve && length )
  {
    rc = pCurve->GetLength(length, fractional_tol, _sub_domain)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsShort( const ON_Curve* pCurve, double tolerance, ON_INTERVAL_STRUCT sub_domain, bool ignoreSubDomain)
{
  const ON_Interval* _sub_domain = NULL;
  if( !ignoreSubDomain )
    _sub_domain = (const ON_Interval*)&sub_domain;
  bool rc = false;
  if( pCurve )
  {
    rc = pCurve->IsShort(tolerance, _sub_domain);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_RemoveShortSegments( ON_Curve* pCurve, double tolerance )
{
  bool rc = false;
  if( pCurve )
    rc = pCurve->RemoveShortSegments(tolerance);
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetNormalizedArcLengthPoint( const ON_Curve* pCurve, double s, double* t, double fractional_tol, ON_INTERVAL_STRUCT sub_domain, bool ignoreSubDomain)
{
  bool rc = false;
  const ON_Interval* _sub_domain = NULL;
  if( !ignoreSubDomain )
    _sub_domain = (const ON_Interval*)&sub_domain;
  if( pCurve && t )
  {
    rc = pCurve->GetNormalizedArcLengthPoint(s, t, fractional_tol, _sub_domain)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetNormalizedArcLengthPoints( const ON_Curve* pCurve, int count, /*ARRAY*/double* s, /*ARRAY*/double* t, double abs_tol, double frac_tol, ON_INTERVAL_STRUCT sub_domain, bool ignoreSubDomain)
{
  bool rc = false;
  const ON_Interval* _sub_domain = NULL;
  if( !ignoreSubDomain )
    _sub_domain = (const ON_Interval*)&sub_domain;
  if( pCurve && count>0 && s && t )
  {
    rc = pCurve->GetNormalizedArcLengthPoints(count, s, t, abs_tol, frac_tol, _sub_domain)?true:false;
  }
  return rc;
}

#endif

RH_C_FUNCTION ON_Curve* ON_Curve_TrimExtend( const ON_Curve* pCurve, double t0, double t1, bool trimming)
{
  ON_Curve* rc = NULL;
  if( pCurve )
  {
    if( trimming )
    {
      rc = ::ON_TrimCurve(*pCurve, ON_Interval(t0,t1));
    }
    else
    {
      ON_Curve* pNewCurve = pCurve->DuplicateCurve();
      if( pNewCurve )
      {
        if( pNewCurve->Extend(ON_Interval(t0,t1)) )
          rc = pNewCurve;
        else
          delete pNewCurve;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_Split( const ON_Curve* pCurve, double t, ON_Curve** left, ON_Curve** right )
{
  bool rc = false;
  if( pCurve && left && right )
  {
    rc = pCurve->Split(t, *left, *right)?true:false;
  }
  return rc;
}

RH_C_FUNCTION ON_NurbsCurve* ON_Curve_NurbsCurve(const ON_Curve* pCurve, double tolerance, ON_INTERVAL_STRUCT sub_domain, bool ignoreSubDomain)
{
  ON_NurbsCurve* rc = NULL;
  if( pCurve )
  {
    const ON_Interval* _sub_domain = NULL;
    if( !ignoreSubDomain )
      _sub_domain = (const ON_Interval*)&sub_domain;
    rc = pCurve->NurbsCurve(NULL,tolerance,_sub_domain);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetNurbParameter(const ON_Curve* pCurve, double t_in, double* t_out, bool nurbToCurve)
{
  bool rc = false;
  if( pCurve && t_out )
  {
    if( nurbToCurve )
      rc = pCurve->GetCurveParameterFromNurbFormParameter(t_in,t_out)?true:false;
    else
      rc = pCurve->GetNurbFormParameterFromCurveParameter(t_in,t_out)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsClosable( const ON_Curve* curvePtr, double tolerance, double min_abs_size, double min_rel_size )
{
  bool rc = false;
  if( curvePtr )
  {
    rc = curvePtr->IsClosable(tolerance, min_abs_size, min_rel_size);
  }
  return rc;
}

RH_C_FUNCTION int ON_Curve_ClosedCurveOrientation( const ON_Curve* curvePtr, ON_Xform* xform)
{
  int rc = 0;
  if( curvePtr )
  {
    rc = ::ON_ClosedCurveOrientation(*curvePtr, xform);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_GetNextDiscontinuity(const ON_Curve* curvePtr, int continuityType, double t0, double t1, double* t)
{
  bool rc = false;
  if( curvePtr )
  {
    ON::continuity c = ON::Continuity(continuityType);
    rc = curvePtr->GetNextDiscontinuity(c, t0, t1, t);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Curve_IsContinuous(const ON_Curve* curvePtr, int continuityType, double t)
{
  bool rc = false;
  if( curvePtr )
  {
    ON::continuity c = ON::Continuity(continuityType);
    rc = curvePtr->IsContinuous(c, t);
  }
  return rc;
}

RH_C_FUNCTION ON_SimpleArray<ON_X_EVENT>* ON_Curve_IntersectPlane(const ON_Curve* pConstCurve, ON_PLANE_STRUCT* plane, double tolerance)
{
  ON_SimpleArray<ON_X_EVENT>* rc = NULL;
  if(pConstCurve && plane)
  {
#if defined(RHINO_V5SR) // only available in V5
    rc = new ON_SimpleArray<ON_X_EVENT>();
    ON_Plane _plane = ::FromPlaneStruct(*plane);
    if( pConstCurve->IntersectPlane( _plane.plane_equation, *rc, tolerance, tolerance) < 1 )
    {
      // no intersections found. No need to create a list of intersections
      delete rc;
      rc = NULL;
    }
#endif
  }
 
  return rc;
}


////////////////////////////////////////////////////////////////////////////////////
// Meshing and mass property calculations are not available in stand alone opennurbs

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION ON_MassProperties* ON_Curve_AreaMassProperties(const ON_Curve* pCurve, double rel_tol, double abs_tol, double curve_planar_tol)
{
  ON_MassProperties* rc = NULL;
  if( pCurve )
  {
    ON_Plane plane;
    if( pCurve->IsPlanar(&plane, curve_planar_tol) && pCurve->IsClosed() )
    {
      ON_BoundingBox bbox = pCurve->BoundingBox();
      ON_3dPoint basepoint = bbox.Center();
      basepoint = plane.ClosestPointTo(basepoint);
      rc = new ON_MassProperties();
      bool getresult = pCurve->AreaMassProperties(basepoint, plane.Normal(), *rc, true, true, true, true, rel_tol, abs_tol);
      if( getresult )
      {
        rc->m_mass = fabs(rc->m_mass);
      }
      else
      {
        delete rc;
        rc = NULL;
      }
    }
  }
  return rc;
}

#endif
