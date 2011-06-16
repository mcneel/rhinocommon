#include "StdAfx.h"

RH_C_FUNCTION ON_PlaneSurface* ON_PlaneSurface_New(const ON_PLANE_STRUCT* plane, ON_INTERVAL_STRUCT xExtents, ON_INTERVAL_STRUCT yExtents)
{
  ON_PlaneSurface* rc = NULL;
  if( plane )
  {
    const ON_Interval* _x = (const ON_Interval*)&xExtents;
    const ON_Interval* _y = (const ON_Interval*)&yExtents;

    ON_Plane temp = FromPlaneStruct(*plane);
    rc = new ON_PlaneSurface(temp);

    if (rc)
    {
      rc->SetExtents(0, *_x, true);
      rc->SetExtents(1, *_y, true);
    }
  }
  return rc;
}

