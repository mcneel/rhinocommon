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
    if( ON_LinearDimension2::userpositionedtext_pt_index == which )
      pAnnotation->m_userpositionedtext = true;
  }
}

RH_C_FUNCTION void ON_Annotation2_Text(ON_Annotation2* pAnnotation2, CRhCmnStringHolder* pStringHolder, const RHMONO_STRING* _str, bool formula)
{
  if( pAnnotation2 )
  {
    if( _str )
    {
      INPUTSTRINGCOERCE(str, _str);
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
      if( formula )
      {
        pAnnotation2->SetTextFormula(str);
        pAnnotation2->SetTextValue(NULL);
      }
      else
      {
        pAnnotation2->SetTextValue(str);
        pAnnotation2->SetTextFormula(NULL);
      }
#else
      pAnnotation2->SetUserText(str);
#endif
    }

    if( pStringHolder )
    {
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
      if( formula )
        pStringHolder->Set( pAnnotation2->TextFormula() );
      else
        pStringHolder->Set( pAnnotation2->TextValue() );
#else
      pStringHolder->Set( pAnnotation2->UserText() );
#endif
    }
  }
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

RH_C_FUNCTION int ON_Annotation2_GetJustification(const ON_Annotation2* pConstAnnotation2)
{
  int rc = 0;
  if( pConstAnnotation2 )
  {
    ON_Annotation2* pAnnotation = const_cast<ON_Annotation2*>(pConstAnnotation2);
    rc = (int)(pAnnotation->Justification());
  }
  return rc;
}

RH_C_FUNCTION void ON_Annotation2_SetJustification(ON_Annotation2* pAnnotation2, int justification)
{
  if( pAnnotation2 )
    pAnnotation2->SetJustification((unsigned int)justification);
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

RH_C_FUNCTION ON_RadialDimension2* ON_RadialDimension2_New()
{
  return new ON_RadialDimension2();
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

RH_C_FUNCTION bool ON_RadialDimension2_CreateFromPoints(ON_RadialDimension2* pRadialDimension, ON_3DPOINT_STRUCT center, ON_3DPOINT_STRUCT arrowTip,
                                                        ON_3DVECTOR_STRUCT xaxis, ON_3DVECTOR_STRUCT normal, double offset_distance)
{
  bool rc = false;
  if( pRadialDimension )
  {
    ON_3dPoint _center(center.val);
    ON_3dPoint _arrowtip(arrowTip.val);
    ON_3dVector _xaxis(xaxis.val);
    ON_3dVector _normal(normal.val);
    rc = pRadialDimension->CreateFromPoints(_center, _arrowtip, _xaxis, _normal, offset_distance);
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

RH_C_FUNCTION int ON_TextDot_GetHeight(const ON_TextDot* pConstDot)
{
  if( pConstDot )
    return pConstDot->Height();
  return 0;
}

RH_C_FUNCTION void ON_TextDot_SetHeight(ON_TextDot* pDot, int height)
{
  if( pDot )
    pDot->SetHeight(height);
}

RH_C_FUNCTION void ON_TextDot_GetFontFace(const ON_TextDot* pConstDot, CRhCmnStringHolder* pStringHolder)
{
  if( pConstDot && pStringHolder )
    pStringHolder->Set(pConstDot->FontFace());
}

RH_C_FUNCTION void ON_TextDot_SetFontFace(ON_TextDot* pDot, const RHMONO_STRING* face)
{
  if( pDot && face )
  {
    INPUTSTRINGCOERCE(_face, face);
    pDot->SetFontFace(_face);
  }
}

RH_C_FUNCTION ON_TextEntity2* ON_TextEntity2_New()
{
  return new ON_TextEntity2();
}

RH_C_FUNCTION bool ON_TextEntity2_DrawTextMask(const ON_TextEntity2* pConstTextEntity2)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
    return pConstTextEntity2->DrawTextMask();
#endif
  return false;
}

RH_C_FUNCTION void ON_TextEntity2_SetDrawTextMask(ON_TextEntity2* pTextEntity2, bool val)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pTextEntity2 )
    pTextEntity2->SetDrawTextMask(val);
#endif
}

RH_C_FUNCTION bool ON_TextEntity2_AnnotativeScaling(const ON_TextEntity2* pConstTextEntity2)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
    return pConstTextEntity2->AnnotativeScaling();
#endif
  return false;
}

RH_C_FUNCTION void ON_TextEntity2_SetAnnotativeScaling(ON_TextEntity2* pTextEntity2, bool val)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pTextEntity2 )
    pTextEntity2->SetAnnotativeScaling(val);
#endif
}

RH_C_FUNCTION int ON_TextEntity2_MaskColorSource(const ON_TextEntity2* pConstTextEntity2)
{
  int rc = 0;
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
    rc = pConstTextEntity2->MaskColorSource();
#endif
  return rc;
}

RH_C_FUNCTION int ON_TextEntity2_MaskColor(const ON_TextEntity2* pConstTextEntity2)
{
  int abgr = 0;
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
  {
    ON_Color c = pConstTextEntity2->MaskColor();
    abgr = (int)((unsigned int)c);
  }
#endif
  return abgr;
}

RH_C_FUNCTION void ON_TextEntity2_SetMaskColor(ON_TextEntity2* pTextEntity2, int argb)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pTextEntity2 )
  {
    ON_Color c = ARGB_to_ABGR(argb);
    pTextEntity2->SetMaskColor(c);
  }
#endif
}

RH_C_FUNCTION int ON_TextEntity2_MaskSource(const ON_TextEntity2* pConstTextEntity2)
{
  int rc = 0;
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
    rc = pConstTextEntity2->MaskColorSource();
#endif
  return rc;
}

RH_C_FUNCTION void ON_TextEntity2_SetMaskSource(ON_TextEntity2* pTextEntity2, int source)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pTextEntity2 )
    pTextEntity2->SetMaskColorSource(source);
#endif
}

RH_C_FUNCTION double ON_TextEntity2_MaskOffsetFactor(const ON_TextEntity2* pConstTextEntity2)
{
  double rc = 0;
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pConstTextEntity2 )
    rc = pConstTextEntity2->MaskOffsetFactor();
#endif
  return rc;
}

RH_C_FUNCTION void ON_TextEntity2_SetMaskOffsetFactor(ON_TextEntity2* pTextEntity2, double factor)
{
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pTextEntity2 )
    pTextEntity2->SetMaskOffsetFactor(factor);
#endif
}

#if !defined(OPENNURBS_BUILD) // only available in Rhino
RH_C_FUNCTION int ON_TextEntity_Explode(const ON_TextEntity2* pConstTextEntity2, ON_SimpleArray<ON_Curve*>* pCurveArray)
{
  int rc = 0;
  if( pConstTextEntity2 && pCurveArray )
  {
    CRhinoAnnotationText annotation;
    annotation.SetAnnotation(*pConstTextEntity2);
    const CRhinoText* pText = annotation.Text();
    if( pText )
    {
      const_cast<CRhinoText*>(pText)->Regen();
      annotation.Explode(*pCurveArray);
      rc = pCurveArray->Count();
    }
  }
  return rc;
}
#endif

RH_C_FUNCTION ON_AngularDimension2* ON_AngularDimension2_New(ON_Arc* arc, double offset)
{
  ON_AngularDimension2* rc = NULL;
  if( arc )
  {
    rc = new ON_AngularDimension2();
    ON_3dVector v = arc->StartPoint()-arc->Center();
    v.Unitize();
    ON_3dPoint apex = arc->Center();
    ON_3dPoint p0 = arc->StartPoint();
    ON_3dPoint p1 = arc->EndPoint();
    ON_3dPoint arc_pt = p0 + ( v * offset );
    ON_3dVector normal = arc->Normal();
    rc->CreateFromPoints(apex, p0, p1, arc_pt, normal);
  }
  return rc;
}