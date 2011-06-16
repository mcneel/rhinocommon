#include "StdAfx.h"

RH_C_FUNCTION ON_NurbsSurface* ON_Sphere_GetNurbsForm(ON_Sphere* sphere)
{
  ON_NurbsSurface* rc = NULL;
  if( sphere )
  {
    sphere->plane.UpdateEquation();
    ON_NurbsSurface* ns = new ON_NurbsSurface();
    int success = sphere->GetNurbForm(*ns);
    if( 0==success )
    {
      delete ns;
      ns = NULL;
    }
    else
      rc = ns;
  }
  return rc;
}

RH_C_FUNCTION ON_RevSurface* ON_Sphere_RevSurfaceForm(ON_Sphere* sphere)
{
  ON_RevSurface* rc = NULL;
  if( sphere )
  {
    sphere->plane.UpdateEquation();
    rc = sphere->RevSurfaceForm();
  }
  return rc;
}

/// THESE SHOULD MOVE ONCE I'VE SET UP SEPARATE CPP FILES

RH_C_FUNCTION ON_NurbsSurface* ON_Cone_GetNurbForm(ON_Cone* cone)
{
  ON_NurbsSurface* rc = NULL;
  if( cone )
  {
    cone->plane.UpdateEquation();
    rc = ON_NurbsSurface::New();
    if( !cone->GetNurbForm(*rc) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_RevSurface* ON_Cone_RevSurfaceForm(ON_Cone* cone)
{
  ON_RevSurface* rc = NULL;
  if( cone )
  {
    cone->plane.UpdateEquation();
    rc = cone->RevSurfaceForm();
  }
  return rc;
}

RH_C_FUNCTION ON_NurbsSurface* ON_Cylinder_GetNurbForm(ON_Cylinder* cylinder)
{
  ON_NurbsSurface* rc = NULL;
  if( cylinder )
  {
    cylinder->circle.plane.UpdateEquation();
    rc = ON_NurbsSurface::New();
    if( !cylinder->GetNurbForm(*rc) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_RevSurface* ON_Cylinder_RevSurfaceForm(ON_Cylinder* cylinder)
{
  ON_RevSurface* rc = NULL;
  if( cylinder )
  {
    cylinder->circle.plane.UpdateEquation();
    rc = cylinder->RevSurfaceForm();
  }
  return rc;
}


RH_C_FUNCTION ON_NurbsSurface* ON_Torus_GetNurbForm(ON_Torus* torus)
{
  ON_NurbsSurface* rc = NULL;
  if( torus )
  {
    torus->plane.UpdateEquation();
    rc = ON_NurbsSurface::New();
    if( !torus->GetNurbForm(*rc) )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_RevSurface* ON_Torus_RevSurfaceForm(ON_Torus* torus)
{
  ON_RevSurface* rc = NULL;
  if( torus )
  {
    torus->plane.UpdateEquation();
    rc = torus->RevSurfaceForm();
  }
  return rc;
}
