#include "StdAfx.h"

RH_C_FUNCTION ON_DimStyle* ON_DimStyle_New()
{
  return new ON_DimStyle();
}

RH_C_FUNCTION bool ON_DimStyle_Name(const ON_DimStyle* pConstDimStyle, CRhCmnStringHolder* pStringHolder)
{
  bool rc = false;
  if( pConstDimStyle )
  {
    pStringHolder->Set( pConstDimStyle->Name() );
    rc = true;
  }
  return rc;
}


RH_C_FUNCTION int ON_DimStyle_GetIndex(const ON_DimStyle* pConstDimStyle)
{
  int rc = -1;
  if( pConstDimStyle )
    rc = pConstDimStyle->Index();
  return rc;
}

RH_C_FUNCTION double ON_DimStyle_GetDouble(const ON_DimStyle* pConstDimStyle, int which)
{
  const int idxExtensionLineExtension = 0;
  const int idxExtensionLineOffset = 1;
  const int idxArrowSize = 2;
  const int idxLeaderArrowSize = 3;
  const int idxCenterMark = 4;
  const int idxTextGap = 5;
  const int idxTextHeight = 6;
  const int idxLengthFactor = 7;
  const int idxAlternateLengthFactor = 8;
  double rc = 0.0;

  if( pConstDimStyle )
  {
    switch(which)
    {
    case idxExtensionLineExtension:
      rc = pConstDimStyle->ExtExtension();
      break;
    case idxExtensionLineOffset:
      rc = pConstDimStyle->ExtOffset();
      break;
    case idxArrowSize:
      rc = pConstDimStyle->ArrowSize();
      break;
    case idxLeaderArrowSize:
      rc = pConstDimStyle->LeaderArrowSize();
      break;
    case idxCenterMark:
      rc = pConstDimStyle->CenterMark();
      break;
    case idxTextGap:
      rc = pConstDimStyle->TextGap();
      break;
    case idxTextHeight:
      rc = pConstDimStyle->TextHeight();
      break;
    case idxLengthFactor:
      rc = pConstDimStyle->LengthFactor();
      break;
    case idxAlternateLengthFactor:
      rc = pConstDimStyle->AlternateLengthFactor();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_DimStyle_SetDouble(ON_DimStyle* pDimStyle, int which, double val)
{
  const int idxExtensionLineExtension = 0;
  const int idxExtensionLineOffset = 1;
  const int idxArrowSize = 2;
  const int idxLeaderArrowSize = 3;
  const int idxCenterMark = 4;
  const int idxTextGap = 5;
  const int idxTextHeight = 6;
  const int idxLengthFactor = 7;
  const int idxAlternateLengthFactor = 8;

  if( pDimStyle )
  {
    switch(which)
    {
    case idxExtensionLineExtension:
      pDimStyle->SetExtExtension(val);
      break;
    case idxExtensionLineOffset:
      pDimStyle->SetExtOffset(val);
      break;
    case idxArrowSize:
      pDimStyle->SetArrowSize(val);
      break;
    case idxLeaderArrowSize:
      pDimStyle->SetLeaderArrowSize(val);
      break;
    case idxCenterMark:
      pDimStyle->SetCenterMark(val);
      break;
    case idxTextGap:
      pDimStyle->SetTextGap(val);
      break;
    case idxTextHeight:
      pDimStyle->SetTextHeight(val);
      break;
    case idxLengthFactor:
      pDimStyle->SetLengthFactor(val);
      break;
    case idxAlternateLengthFactor:
      pDimStyle->SetAlternateLengthFactor(val);
      break;
    }
  }
}

RH_C_FUNCTION ON_UUID ON_DimStyle_ModelObjectId(const ON_DimStyle* pDimStyle)
{
  ON_UUID rc;
  if( pDimStyle )
    rc = pDimStyle->ModelObjectId();
  else
    rc = ON_nil_uuid;
  return rc;
}

RH_C_FUNCTION void ON_DimStyle_SetName(ON_DimStyle* pDimStyle, const RHMONO_STRING* _name)
{
  INPUTSTRINGCOERCE(name, _name);
  if( pDimStyle )
  {
    pDimStyle->SetName(name);
  }
}


RH_C_FUNCTION int ON_DimStyle_GetInt(const ON_DimStyle* pConstDimStyle, int which)
{
  const int idxAngleResolution = 0;
  const int idxFontIndex = 1;
  const int idxLengthResolution = 2;
  const int idxLengthFormat = 3;
  const int idxTextAlignment = 4;
  int rc = 0;

  if( pConstDimStyle )
  {
    switch(which)
    {
    case idxAngleResolution:
      rc = pConstDimStyle->AngleResolution();
      break;
    case idxFontIndex:
      rc = pConstDimStyle->FontIndex();
      break;
    case idxLengthResolution:
      rc = pConstDimStyle->LengthResolution();
      break;
    case idxLengthFormat:
      rc = pConstDimStyle->LengthFormat();
      break;
    case idxTextAlignment:
      rc = pConstDimStyle->TextAlignment();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_DimStyle_SetInt(ON_DimStyle* pDimStyle, int which, int val)
{
  const int idxAngleResolution = 0;
  const int idxFontIndex = 1;
  const int idxLengthResolution = 2;
  const int idxLengthFormat = 3;
  const int idxTextAlignment = 4;

  if( pDimStyle )
  {
    switch(which)
    {
    case idxAngleResolution:
      pDimStyle->SetAngleResolution(val);
      break;
    case idxFontIndex:
      pDimStyle->SetFontIndex(val);
      break;
    case idxLengthResolution:
      pDimStyle->SetLengthResolution(val);
      break;
    case idxLengthFormat:
      pDimStyle->SetLengthFormat(val);
      break;
    case idxTextAlignment:
      pDimStyle->SetTextAlignment(ON::TextDisplayMode(val));
      break;
    }
  }
}

RH_C_FUNCTION void ON_DimStyle_GetString(const ON_DimStyle* pConstDimStyle, CRhCmnStringHolder* pString, bool prefix)
{
  if( pConstDimStyle && pString )
  {
    if( prefix )
      pString->Set( pConstDimStyle->Prefix() );
    else
      pString->Set( pConstDimStyle->Suffix() );
  }
}

RH_C_FUNCTION void ON_DimStyle_SetString(ON_DimStyle* pDimStyle, const RHMONO_STRING* str, bool prefix)
{
  if( pDimStyle )
  {
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in Rhino 5
    INPUTSTRINGCOERCE(_str, str);
    if( prefix )
      pDimStyle->SetPrefix(_str);
    else
      pDimStyle->SetSuffix(_str);
#else
    INPUTSTRINGCOERCE(_str, str);
    wchar_t* _hack_str = const_cast<wchar_t*>(_str); //this should have been const. It is not modified
    if( prefix )
      pDimStyle->SetPrefix(_hack_str);
    else
      pDimStyle->SetSuffix(_hack_str);
#endif
  }
}