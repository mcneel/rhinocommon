#include "StdAfx.h"

RH_C_FUNCTION double ON_Annotation2_NumericValue(const ON_Annotation2* pConstAnnotation)
{
  double rc = 0;
  if( pConstAnnotation )
    rc = pConstAnnotation->NumericValue();
  return rc;
}

RH_C_FUNCTION void ON_Annotation2_GetPoint(const ON_Annotation2* pConstAnnotation, int which, ON_2dPoint* point)
{
  if( pConstAnnotation && point )
    *point = pConstAnnotation->Point(which);
}

RH_C_FUNCTION void ON_Annotation2_SetPoint(ON_Annotation2* pAnnotation, int which, ON_2DPOINT_STRUCT point)
{
  if( pAnnotation )
  {
    ON_2dPoint _point(point.val[0], point.val[1]);
    pAnnotation->SetPoint(which, _point);
  }
}

RH_C_FUNCTION const wchar_t* ON_Annotation2_Text(ON_Annotation2* pAnnotation2, const RHMONO_STRING* _str, bool formula)
{
  const wchar_t* rc = NULL;
#if defined(_WIN32)
  if( pAnnotation2 )
  {
    if( _str )
    {
      INPUTSTRINGCOERCE(str, _str);
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
      if( formula )
        pAnnotation2->SetTextFormula(str);
      else
        pAnnotation2->SetTextValue(str);
#else
      pAnnotation2->SetUserText(str);
#endif
    }

#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
    if( formula )
      rc = pAnnotation2->TextFormula();
    else
      rc = pAnnotation2->TextValue();
#else
    rc = pAnnotation2->UserText();
#endif
  }
#endif
  return rc;
}


RH_C_FUNCTION double ON_Annotation2_Height(ON_Annotation2* ptr, bool set, double set_value)
{
  double rc = 0.0;
  if( ptr )
  {
    if( set )
      ptr->m_textheight = set_value;
    rc = ptr->m_textheight;
  }
  return rc;
}

RH_C_FUNCTION int ON_Annotation2_Index(ON_Annotation2* ptr, bool set, int set_value)
{
  int rc = 0;
  if( ptr )
  {
    if( set )
      ptr->m_index = set_value;
    rc = ptr->m_index;
  }
  return rc;
}

RH_C_FUNCTION void ON_Annotation2_Plane(ON_Annotation2* ptr, ON_PLANE_STRUCT* plane, bool set)
{
  if( ptr && plane )
  {
    if( set )
      ptr->m_plane = FromPlaneStruct(*plane);
    else
      CopyToPlaneStruct(*plane, ptr->m_plane);
  }
}

RH_C_FUNCTION ON_LinearDimension2* ON_LinearDimension2_New()
{
  return new ON_LinearDimension2();
}

RH_C_FUNCTION void ON_LinearDimension2_SetLocations(ON_LinearDimension2* pLinearDimension2, ON_2DPOINT_STRUCT ext0, ON_2DPOINT_STRUCT ext1, ON_2DPOINT_STRUCT linePt)
{
  if( pLinearDimension2 )
  {
    const ON_2dPoint* _ext0 = (const ON_2dPoint*)(&ext0);
    const ON_2dPoint* _ext1 = (const ON_2dPoint*)(&ext1);
    const ON_2dPoint* _linePt = (const ON_2dPoint*)(&linePt);
    pLinearDimension2->SetPoint( ON_LinearDimension2::ext0_pt_index, *_ext0);
    pLinearDimension2->SetPoint( ON_LinearDimension2::ext1_pt_index, *_ext1);

    // from CRhinoLinearDimension::UpdateDimPoints
    if ( pLinearDimension2->m_points.Count() >= ON_LinearDimension2::dim_pt_count )
    {
      ON_2dPoint p0 = pLinearDimension2->m_points[ON_LinearDimension2::ext0_pt_index];
      ON_2dPoint p2 = pLinearDimension2->m_points[ON_LinearDimension2::ext1_pt_index];
      
      ON_2dPoint p1(p0.x,_linePt->y);
      pLinearDimension2->m_points[ON_LinearDimension2::arrow0_pt_index] = p1;
      
      ON_2dPoint p3(p2.x,_linePt->y);
      pLinearDimension2->m_points[ON_LinearDimension2::arrow1_pt_index] = p3;
      
      if ( !pLinearDimension2->m_userpositionedtext )
      {
        ON_2dPoint p4( 0.5*(p0.x+p2.x), _linePt->y );
        pLinearDimension2->m_points[ON_LinearDimension2::userpositionedtext_pt_index] = p4;
      }
    }
  }
}

RH_C_FUNCTION bool ON_LinearDimension2_IsAligned(const ON_LinearDimension2* pConstLinearDimension2)
{
  bool rc = false;
  if( pConstLinearDimension2 )
    rc = ( ON::dtDimAligned == pConstLinearDimension2->m_type );
  return rc;
}

RH_C_FUNCTION void ON_LinearDimension2_SetAligned( ON_LinearDimension2* pLinearDimension2, bool val )
{
  if( pLinearDimension2 )
  {
    pLinearDimension2->m_type = val?ON::dtDimAligned : ON::dtDimLinear;
  }
}

RH_C_FUNCTION bool ON_RadialDimension2_IsDiameterDimension( const ON_RadialDimension2* pConstRadialDimension2 )
{
  bool rc = false;
  if( pConstRadialDimension2 )
  {
    rc = (pConstRadialDimension2->Type() == ON::dtDimDiameter);
  }
  return rc;
}

RH_C_FUNCTION ON_TextDot* ON_TextDot_New(const RHMONO_STRING* _str, ON_3DPOINT_STRUCT loc)
{
  INPUTSTRINGCOERCE(str, _str);
  ON_TextDot* ptr = new ON_TextDot();
  ptr->SetTextString(str);
  const ON_3dPoint* _loc = (const ON_3dPoint*)&loc;
  ptr->SetPoint(*_loc);
  return ptr;
}

RH_C_FUNCTION void ON_TextDot_GetSetPoint(ON_TextDot* ptr, bool set, ON_3dPoint* pt)
{
  if( ptr && pt )
  {
    if( set )
      ptr->SetPoint(*pt);
    else
      *pt = ptr->Point();
  }
}

RH_C_FUNCTION void ON_TextDot_GetSetText(ON_TextDot* ptr, bool set, const RHMONO_STRING* _text, CRhCmnStringHolder* pStringHolder)
{
  INPUTSTRINGCOERCE(text, _text);
  if( ptr )
  {
    if( set )
      ptr->SetTextString(text);
    else
    {
      if( pStringHolder )
        pStringHolder->Set(ptr->TextString());
    }
  }
}
