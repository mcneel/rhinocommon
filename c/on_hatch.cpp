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