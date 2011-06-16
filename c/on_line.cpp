#include "StdAfx.h"

RH_C_FUNCTION double ON_Line_DistanceToPoint( const ON_Line* pLine, ON_3DPOINT_STRUCT point, bool minDist)
{
  double rc = -1;
  if( pLine )
  {
    const ON_3dPoint* _point = (const ON_3dPoint*)&point;
    if( minDist )
      rc = pLine->MinimumDistanceTo(*_point);
    else
      rc = pLine->MaximumDistanceTo(*_point);
  }
  return rc;
}

RH_C_FUNCTION double ON_Line_DistanceToLine( const ON_Line* pLine, const ON_Line* pOtherLine, bool minDist)
{
  double rc = -1;
  if( pLine && pOtherLine)
  {
    if( minDist )
      rc = pLine->MinimumDistanceTo(*pOtherLine);
    else
      rc = pLine->MaximumDistanceTo(*pOtherLine);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Line_Transform( ON_Line* pLine, const ON_Xform* xform )
{
  bool rc = false;
  if( pLine && xform )
  {
    rc = pLine->Transform(*xform);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Line_InPlane( const ON_Line* pConstLine, ON_PLANE_STRUCT* plane )
{
  bool rc = false;
  if( pConstLine && plane )
  {
    ON_Plane temp;
    rc = pConstLine->InPlane(temp);
    CopyToPlaneStruct(*plane, temp);
  }
  return rc;
}