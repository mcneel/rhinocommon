#include "StdAfx.h"

RH_C_FUNCTION bool ON_DetailView_GetBool(const ON_DetailView* pConstDetail, int which)
{
  const int idxIsParallelProjection = 0;
  const int idxIsPerspectiveProjection = 1;
  const int idxIsProjectionLocked = 2;

  bool rc = false;
  if( pConstDetail )
  {
    if( idxIsParallelProjection == which )
      rc = pConstDetail->m_view.m_vp.Projection()==ON::parallel_view;
    else if( idxIsPerspectiveProjection == which )
      rc = pConstDetail->m_view.m_vp.Projection()==ON::perspective_view;
    else if( idxIsProjectionLocked == which )
      rc = pConstDetail->m_view.m_bLockedProjection;
  }
  return rc;
}

RH_C_FUNCTION void ON_DetailView_SetBool(ON_DetailView* pDetail, int which, bool val)
{
  const int idxIsParallelProjection = 0;
  const int idxIsPerspectiveProjection = 1;
  const int idxIsProjectionLocked = 2;
  if( pDetail )
  {
    if( idxIsParallelProjection == which )
    {
      if( val )
        pDetail->m_view.m_vp.SetProjection(ON::parallel_view);
      else
        pDetail->m_view.m_vp.SetProjection(ON::perspective_view);
    }
    else if( idxIsPerspectiveProjection == which )
    {
      if( val )
        pDetail->m_view.m_vp.SetProjection(ON::perspective_view);
      else
        pDetail->m_view.m_vp.SetProjection(ON::parallel_view);
    }
    else if( idxIsProjectionLocked == which )
      pDetail->m_view.m_bLockedProjection = val;
  }
}

RH_C_FUNCTION double ON_DetailView_GetPageToModelRatio(const ON_DetailView* pConstDetail)
{
  double rc = 0;
  if( pConstDetail )
  {
    rc = pConstDetail->m_page_per_model_ratio;
  }
  return rc;
}

RH_C_FUNCTION bool ON_DetailView_SetScale(ON_DetailView* pDetail, double model_length, int modelUnitSystem, double paper_length, int pageUnitSystem)
{
  bool rc = false;
  ON::unit_system model_units = ON::UnitSystem(modelUnitSystem);
  ON::unit_system paper_units = ON::UnitSystem(pageUnitSystem);
  if( pDetail &&
      pDetail->m_view.m_vp.Projection()==ON::parallel_view &&
      model_units != ON::no_unit_system &&
      paper_units != ON::no_unit_system )
  {
    double model_length_mm = ::fabs( model_length * ON::UnitScale(model_units, ON::millimeters ) );
    double paper_length_mm = ::fabs( paper_length * ON::UnitScale(paper_units, ON::millimeters ) );
    if( model_length_mm <= ON_ZERO_TOLERANCE || paper_length_mm <= ON_ZERO_TOLERANCE )
      return false;

    pDetail->m_page_per_model_ratio = paper_length_mm / model_length_mm;
    rc = true;
/*
    //get width of the detail as it would show up on the paper
    ON_BoundingBox bbox = pDetail->BoundingBox();
    double detail_width_on_paper = bbox.m_max.x - bbox.m_min.x;
    double detail_width_on_paper_mm = detail_width_on_paper * ON::UnitScale(page_units, ON::millimeters);

    if(detail_width_on_paper_mm <= 0.0)
      return false;

    //set up the view frustum
    double frustum_width_mm = detail_width_on_paper_mm / m_detail_view.m_page_per_model_ratio;
    double frustum_width = frustum_width_mm * ON::UnitScale( ON_UnitSystem(ON::millimeters), pDoc->Properties().ModelUnits() );

    // 05 Feb. 2007 S. Baer (RR24026)
    // The screen port aspect ratio should always be calculated off of the detail bounding
    // box because it is stored with double precision values. Using the GetScreenPortAspect
    // wil always cause a slight error in the calculation because it is dividing integers
    //if( !Viewport().VP().GetScreenPortAspect(aspect) )
    //  return;
    double port_width = bbox.m_max.x - bbox.m_min.x;
    double port_height = bbox.m_max.y - bbox.m_min.y;
    if( port_height == 0.0 )
      return false;
    double aspect = fabs(port_width/port_height);

    if(aspect <= 0.0)
      return false;
    double frustum_height = frustum_width / aspect;

    double fr_left, fr_right, fr_top, fr_bottom, fr_near, fr_far;
    Viewport().VP().GetFrustum(&fr_left, &fr_right, &fr_bottom, &fr_top, &fr_near, &fr_far);

    fr_left = (fr_left+fr_right)/2.0 - frustum_width/2.0;
    fr_right = fr_left + frustum_width;
    fr_bottom = (fr_bottom+fr_top)/2.0 - frustum_height/2.0;
    fr_top = fr_bottom + frustum_height;
    return Viewport().m_v.m_vp.SetFrustum(fr_left, fr_right, fr_bottom, fr_top, fr_near, fr_far);
*/
  }
  return rc;
}
