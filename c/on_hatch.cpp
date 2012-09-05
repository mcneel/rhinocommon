#include "StdAfx.h"

RH_C_FUNCTION ON_HatchPattern* ON_HatchPattern_New()
{
  return new ON_HatchPattern();
}

RH_C_FUNCTION void ON_HatchPattern_GetString(const ON_HatchPattern* pConstHatchPattern, CRhCmnStringHolder* pString, bool name)
{
  if( pConstHatchPattern && pString )
  {
    if( name )
      pString->Set(pConstHatchPattern->Name());
    else
      pString->Set(pConstHatchPattern->Description());
  }
}

RH_C_FUNCTION void ON_HatchPattern_SetString(ON_HatchPattern* pHatchPattern, const RHMONO_STRING* str, bool name)
{
  if( pHatchPattern )
  {
    INPUTSTRINGCOERCE(_str, str);
    if( name )
      pHatchPattern->SetName(_str);
    else
      pHatchPattern->SetDescription(_str);
  }
}

RH_C_FUNCTION int ON_HatchPattern_GetFillType(const ON_HatchPattern* pConstHatchPattern)
{
  int rc = 0;
  if( pConstHatchPattern )
    rc = (int)(pConstHatchPattern->FillType());
  return rc;
}

RH_C_FUNCTION void ON_HatchPattern_SetFillType(ON_HatchPattern* pHatchPattern, int filltype)
{
  if( pHatchPattern )
  {
    pHatchPattern->SetFillType( (ON_HatchPattern::eFillType)filltype );
  }
}

RH_C_FUNCTION int ON_Hatch_PatternIndex(const ON_Hatch* pConstHatch)
{
  int rc = -1;
  if( pConstHatch )
  {
    rc = pConstHatch->PatternIndex();
  }
  return rc;
}

RH_C_FUNCTION void ON_Hatch_SetPatternIndex(ON_Hatch* pHatch, int val)
{
  if( pHatch )
    pHatch->SetPatternIndex(val);
}

RH_C_FUNCTION double ON_Hatch_GetRotation(const ON_Hatch* pConstHatch)
{
  double rc = 0;
  if( pConstHatch )
    rc = pConstHatch->PatternRotation();
  return rc;
}

RH_C_FUNCTION void ON_Hatch_SetRotation(ON_Hatch* pHatch, double rotation)
{
  if( pHatch )
    pHatch->SetPatternRotation(rotation);
}

RH_C_FUNCTION double ON_Hatch_GetScale(const ON_Hatch* pConstHatch)
{
  double rc = 0;
  if( pConstHatch )
    rc = pConstHatch->PatternScale();
  return rc;
}

RH_C_FUNCTION void ON_Hatch_SetScale(ON_Hatch* pHatch, double rotation)
{
  if( pHatch )
    pHatch->SetPatternScale(rotation);
}


////////////////////////////////////////////////////////////////////////////////////
// Meshing and mass property calculations are not available in stand alone opennurbs

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION ON_MassProperties* ON_Hatch_AreaMassProperties(const ON_Hatch* pConstHatch, double rel_tol, double abs_tol)
{
  ON_MassProperties* rc = NULL;
  if( pConstHatch )
  {
    ON_BoundingBox bbox = pConstHatch->BoundingBox();
    ON_3dPoint basepoint = bbox.Center();
    basepoint = pConstHatch->Plane().ClosestPointTo(basepoint);

    ON_ClassArray<ON_MassProperties> list;

    for( int i=0; i<pConstHatch->LoopCount(); i++ )
    {
      const ON_HatchLoop* pLoop = pConstHatch->Loop(i);
      if( NULL==pLoop )
        continue;
      ON_Curve* pCurve = pConstHatch->LoopCurve3d(i);
      if( NULL==pCurve )
        continue;
      
      ON_MassProperties mp;
      if( pCurve->AreaMassProperties(basepoint, pConstHatch->Plane().Normal(), mp, true, true, true, true, rel_tol, abs_tol) )
      {
        mp.m_mass = fabs(mp.m_mass);
        if( pLoop->Type() == ON_HatchLoop::ltInner )
          mp.m_mass = -mp.m_mass;

        list.Append(mp);
      }
      delete pCurve;
    }

    if( list.Count()==1 )
    {
      rc = new ON_MassProperties();
      *rc = list[0];
    }
    else if( list.Count()>1 )
    {
      int count = list.Count();
      const ON_MassProperties* pieces = list.Array();
      rc = new ON_MassProperties();
      if( !rc->Sum(count, pieces) )
      {
        delete rc;
        rc = NULL;
      }
    }
  }
  return rc;
}
#endif

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION void ON_Hatch_Explode(const ON_Hatch* pConstHatch,
                                    const CRhinoObject* pConstParentRhinoObject,
                                    ON_SimpleArray<ON_Geometry*>* pOutputGeometry)
{
  if( pConstHatch && pOutputGeometry )
  {
    ON_SimpleArray<CRhinoObject*> subobjects;
    CRhinoHatch hatchobject;
    if( NULL==pConstParentRhinoObject )
    {
      hatchobject.SetHatch(*pConstHatch);

      // 5 September 2012 S. Baer (Super-Mega-Hack)
      // The hatch object needs to create a cached hatch display in order
      // for GetSubObjects to properly work.  We need to eventually fix
      // the problem in the core, buit for now I'm just calling Pick since
      // it will create a HatchDisplay when one doesn't exist
      CRhinoPickContext pc;
      CRhinoObjRefArray ar;
      hatchobject.Pick(pc, ar);

      pConstParentRhinoObject = &hatchobject;
    }

    pConstParentRhinoObject->GetSubObjects(subobjects);
    for( int i=0; i<subobjects.Count(); i++ )
    {
      CRhinoObject* pRhinoObject = subobjects[i];
      if( pRhinoObject )
      {
        const ON_Geometry* pGeometry = pRhinoObject->Geometry();
        if( pGeometry )
          pOutputGeometry->Append( pGeometry->Duplicate() );
        delete pRhinoObject;
      }
    }
  }
}
#endif

RH_C_FUNCTION void ON_Hatch_LoopCurve3d(const ON_Hatch* pConstHatch, ON_SimpleArray<ON_Curve*>* pCurveArray, bool outer)
{
  if( pConstHatch && pCurveArray )
  {
    ON_HatchLoop::eLoopType looptype = outer ? ON_HatchLoop::ltOuter : ON_HatchLoop::ltInner;
    int count = pConstHatch->LoopCount();
    for( int i=0; i<count; i++ )
    {
      const ON_HatchLoop* pLoop = pConstHatch->Loop(i);
      if( pLoop && pLoop->Type()==looptype )
      {
        ON_Curve* crv = pConstHatch->LoopCurve3d(i);
        if( crv )
          pCurveArray->Append(crv);
      }
    }
  }
}