#include "StdAfx.h"

///////////////////////////////////////////////////////////////////////////////////////
RH_C_FUNCTION ON_SimpleArray<ON_Line>* ON_LineArray_New()
{
  return new ON_SimpleArray<ON_Line>();
}

RH_C_FUNCTION void ON_LineArray_Delete( ON_SimpleArray<ON_Line>* pArray )
{
  if( pArray )
    delete pArray;
}

RH_C_FUNCTION int ON_LineArray_Count( const ON_SimpleArray<ON_Line>* pArray )
{
  int rc = 0;
  if( pArray )
    rc = pArray->Count();
  return rc;
}

RH_C_FUNCTION void ON_LineArray_CopyValues( const ON_SimpleArray<ON_Line>* pArray, ON_Line* lines )
{
  if( pArray && lines )
  {
    int count = pArray->Count();
    if( count > 0 )
    {
      const ON_Line* source = pArray->Array();
      ::memcpy(lines, source, count * sizeof(ON_Line));
    }
  }
}

///////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<ON_COMPONENT_INDEX>* ON_ComponentIndexArray_New()
{
  return new ON_SimpleArray<ON_COMPONENT_INDEX>();
}

RH_C_FUNCTION void ON_ComponentIndexArray_Delete( ON_SimpleArray<ON_COMPONENT_INDEX>* pArray )
{
  if( pArray )
    delete pArray;
}

RH_C_FUNCTION int ON_ComponentIndexArray_Count( const ON_SimpleArray<ON_COMPONENT_INDEX>* pArray )
{
  int rc = 0;
  if( pArray )
    rc = pArray->Count();
  return rc;
}

RH_C_FUNCTION void ON_ComponentIndexArray_CopyValues( const ON_SimpleArray<ON_COMPONENT_INDEX>* pArray, ON_COMPONENT_INDEX* ci )
{
  if( pArray && ci )
  {
    int count = pArray->Count();
    if( count > 0 )
    {
      const ON_COMPONENT_INDEX* source = pArray->Array();
      ::memcpy(ci, source, count * sizeof(ON_COMPONENT_INDEX));
    }
  }
}

///////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_2dPointArray* ON_2dPointArray_New(int capacity)
{
  if( capacity < 1 )
    return new ON_2dPointArray();
  return new ON_2dPointArray(capacity);
}

RH_C_FUNCTION void ON_2dPointArray_Delete( ON_2dPointArray* pArray )
{
  if( pArray )
    delete pArray;
}


RH_C_FUNCTION int ON_2dPointArray_Count( const ON_2dPointArray* pArray )
{
  int rc = 0;
  if( pArray )
    rc = pArray->Count();
  return rc;
}

RH_C_FUNCTION void ON_2dPointArray_CopyValues( const ON_2dPointArray* pArray, /*ARRAY*/ON_2dPoint* pts )
{
  if( pArray && pts )
  {
    int count = pArray->Count();
    if( count > 0 )
    {
      const ON_2dPoint* source = pArray->Array();
      ::memcpy(pts, source, count * sizeof(ON_2dPoint));
    }
  }
}



///////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_3dPointArray* ON_3dPointArray_New(int capacity)
{
  if( capacity < 1 )
    return new ON_3dPointArray();
  return new ON_3dPointArray(capacity);
}

RH_C_FUNCTION void ON_3dPointArray_Delete( ON_3dPointArray* pArray )
{
  if( pArray )
    delete pArray;
}


RH_C_FUNCTION int ON_3dPointArray_Count( const ON_3dPointArray* pArray )
{
  int rc = 0;
  if( pArray )
    rc = pArray->Count();
  return rc;
}

RH_C_FUNCTION void ON_3dPointArray_CopyValues( const ON_3dPointArray* pArray, /*ARRAY*/ON_3dPoint* pts )
{
  if( pArray && pts )
  {
    int count = pArray->Count();
    if( count > 0 )
    {
      const ON_3dPoint* source = pArray->Array();
      ::memcpy(pts, source, count * sizeof(ON_3dPoint));
    }
  }
}

/////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<int>* ON_IntArray_New()
{
  return new ON_SimpleArray<int>();
}

RH_C_FUNCTION void ON_IntArray_CopyValues(const ON_SimpleArray<int>* ptr, /*ARRAY*/int* vals)
{
  if( ptr && vals )
  {
    int count = ptr->Count();
    if( count > 0 )
    {
      const int* source = ptr->Array();
      ::memcpy(vals, source, count * sizeof(int));
    }
  }
}

RH_C_FUNCTION int ON_IntArray_Count(const ON_SimpleArray<int>* ptr)
{
  int rc = 0;
  if( ptr )
    rc = ptr->Count();
  return rc;
}

RH_C_FUNCTION void ON_IntArray_Delete(ON_SimpleArray<int>* p)
{
  if( p )
    delete p;
}

/////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<ON_UUID>* ON_UUIDArray_New()
{
  return new ON_SimpleArray<ON_UUID>();
}

RH_C_FUNCTION void ON_UUIDArray_CopyValues(const ON_SimpleArray<ON_UUID>* ptr, /*ARRAY*/ON_UUID* vals)
{
  if( ptr && vals )
  {
    int count = ptr->Count();
    if( count > 0 )
    {
      const ON_UUID* source = ptr->Array();
      ::memcpy(vals, source, count * sizeof(ON_UUID));
    }
  }
}

RH_C_FUNCTION int ON_UUIDArray_Count(const ON_SimpleArray<ON_UUID>* ptr)
{
  int rc = 0;
  if( ptr )
    rc = ptr->Count();
  return rc;
}

RH_C_FUNCTION void ON_UUIDArray_Delete(ON_SimpleArray<ON_UUID>* p)
{
  if( p )
    delete p;
}


////////////////////////////////////
RH_C_FUNCTION ON_SimpleArray<double>* ON_DoubleArray_New()
{
  return new ON_SimpleArray<double>();
}

RH_C_FUNCTION int ON_DoubleArray_Count(const ON_SimpleArray<double>* ptr)
{
  int rc = 0;
  if( ptr )
    rc = ptr->Count();
  return rc;
}

RH_C_FUNCTION void ON_DoubleArray_Append(ON_SimpleArray<double>* pArray, int count, /*ARRAY*/const double* vals)
{
  if( pArray && count>0 && vals )
    pArray->Append(count, vals);
}

RH_C_FUNCTION void ON_DoubleArray_CopyValues(const ON_SimpleArray<double>* ptr, double* vals)
{
  if( ptr && vals )
  {
    int count = ptr->Count();
    if( count > 0 )
    {
      const double* source = ptr->Array();
      ::memcpy(vals, source, count * sizeof(double));
    }
  }
}

RH_C_FUNCTION void ON_DoubleArray_Delete(ON_SimpleArray<double>* p)
{
  if( p )
    delete p;
}

//////////////////


RH_C_FUNCTION ON_SimpleArray<ON_Brep*>* ON_BrepArray_New()
{
  return new ON_SimpleArray<ON_Brep*>();
}

RH_C_FUNCTION void ON_BrepArray_Delete(ON_SimpleArray<ON_Brep*>* pBrepArray)
{
  if( pBrepArray )
    delete pBrepArray;
}

RH_C_FUNCTION int ON_BrepArray_Count(const ON_SimpleArray<ON_Brep*>* pBrepArray)
{
  int rc = 0;
  if( pBrepArray )
  {
    rc = pBrepArray->Count();
  }
  return rc;
}

RH_C_FUNCTION void ON_BrepArray_Append(ON_SimpleArray<ON_Brep*>* pBrepArray, ON_Brep* pBrep)
{
  if( pBrepArray && pBrep )
  {
    pBrepArray->Append(pBrep);
  }
}

RH_C_FUNCTION ON_Brep* ON_BrepArray_Get(ON_SimpleArray<ON_Brep*>* pBrepArray, int index)
{
  ON_Brep* rc = NULL;
  if( pBrepArray && index>=0 && index<pBrepArray->Count() )
  {
    rc = (*pBrepArray)[index];
  }
  return rc;
}

//////////////////


RH_C_FUNCTION ON_SimpleArray<ON_Mesh*>* ON_MeshArray_New()
{
  return new ON_SimpleArray<ON_Mesh*>();
}

RH_C_FUNCTION void ON_MeshArray_Delete(ON_SimpleArray<ON_Mesh*>* pMeshArray)
{
  if( pMeshArray )
    delete pMeshArray;
}

RH_C_FUNCTION int ON_MeshArray_Count(const ON_SimpleArray<ON_Mesh*>* pMeshArray)
{
  int rc = 0;
  if( pMeshArray )
  {
    rc = pMeshArray->Count();
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshArray_Append(ON_SimpleArray<ON_Mesh*>* pMeshArray, ON_Mesh* pMesh)
{
  if( pMeshArray && pMesh )
  {
    pMeshArray->Append(pMesh);
  }
}

RH_C_FUNCTION ON_Mesh* ON_MeshArray_Get(ON_SimpleArray<ON_Mesh*>* pMeshArray, int index)
{
  ON_Mesh* rc = NULL;
  if( pMeshArray && index>=0 && index<pMeshArray->Count() )
  {
    rc = (*pMeshArray)[index];
  }
  return rc;
}

RH_C_FUNCTION ON_ClassArray<ON_wString>* ON_StringArray_New()
{
  return new ON_ClassArray<ON_wString>();
}

RH_C_FUNCTION void ON_StringArray_Append(ON_ClassArray<ON_wString>* pStrings, const RHMONO_STRING* str)
{
  if( pStrings && str )
  {
    INPUTSTRINGCOERCE(_str, str);
    pStrings->Append(_str);
  }
}

RH_C_FUNCTION void ON_StringArray_Delete(ON_ClassArray<ON_wString>* pStrings)
{
  if(pStrings)
    delete pStrings;
}

RH_C_FUNCTION void ON_StringArray_Get(const ON_ClassArray<ON_wString>* pStrings, int index, CRhCmnStringHolder* pStringHolder)
{
  if( pStrings && index>=0 && index<pStrings->Count() && pStringHolder )
  {
    pStringHolder->Set((*pStrings)[index]);
  }
}

//////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<ON_Surface*>* ON_SurfaceArray_New()
{
  return new ON_SimpleArray<ON_Surface*>();
}

RH_C_FUNCTION void ON_SurfaceArray_Delete(ON_SimpleArray<ON_Surface*>* pSurfaceArray)
{
  if( pSurfaceArray )
    delete pSurfaceArray;
}

RH_C_FUNCTION int ON_SurfaceArray_Count(const ON_SimpleArray<ON_Surface*>* pSurfaceArray)
{
  int rc = 0;
  if( pSurfaceArray )
  {
    rc = pSurfaceArray->Count();
  }
  return rc;
}

RH_C_FUNCTION ON_Surface* ON_SurfaceArray_Get(ON_SimpleArray<ON_Surface*>* pSurfaceArray, int index)
{
  ON_Surface* rc = NULL;
  if( pSurfaceArray && index>=0 && index<pSurfaceArray->Count() )
  {
    rc = (*pSurfaceArray)[index];
  }
  return rc;
}

///////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<ON_Interval>* ON_IntervalArray_New()
{
  return new ON_SimpleArray<ON_Interval>();
}

RH_C_FUNCTION void ON_IntervalArray_Delete(ON_SimpleArray<ON_Interval>* pIntervalArray)
{
  if( pIntervalArray )
    delete pIntervalArray;
}

RH_C_FUNCTION int ON_IntervalArray_Count(const ON_SimpleArray<ON_Interval>* pConstIntervalArray)
{
  if( pConstIntervalArray )
    return pConstIntervalArray->Count();
  return 0;
}

RH_C_FUNCTION void ON_IntervalArray_CopyValues(const ON_SimpleArray<ON_Interval>* pSrcIntervalArray, /*ARRAY*/ON_Interval* dest)
{
  if( pSrcIntervalArray && dest )
  {
    const ON_Interval* pSrc = pSrcIntervalArray->Array();
    int count = pSrcIntervalArray->Count();
    ::memcpy(dest, pSrc, count*sizeof(ON_Interval));
  }
}

///////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_SimpleArray<ON_BezierCurve*>* ON_SimpleArray_BezierCurveNew()
{
  return new ON_SimpleArray<ON_BezierCurve*>();
}

RH_C_FUNCTION void ON_SimpleArray_BezierCurveDelete(ON_SimpleArray<ON_BezierCurve*>* pBezArray)
{
  if( pBezArray )
    delete pBezArray;
}

RH_C_FUNCTION ON_BezierCurve* ON_SimpleArray_BezierCurvePtr(ON_SimpleArray<ON_BezierCurve*>* pBezArray, int index)
{
  ON_BezierCurve* rc = NULL;
  if( pBezArray && index>=0 && index<pBezArray->Count() )
    rc = (*pBezArray)[index];
  return rc;
}

RH_C_FUNCTION void ON_BezierCurve_Delete(ON_BezierCurve* pBez)
{
  if( pBez )
    delete pBez;
}


RH_C_FUNCTION ON_SimpleArray<const ON_3dmObjectAttributes*>* ON_SimpleArray_3dmObjectAttributes_New()
{
  return new ON_SimpleArray<const ON_3dmObjectAttributes*>();
}

RH_C_FUNCTION void ON_SimpleArray_3dmObjectAttributes_Delete( ON_SimpleArray<const ON_3dmObjectAttributes*>* pArray )
{
  if( pArray )
    delete pArray;
}

RH_C_FUNCTION void ON_SimpleArray_3dmObjectAttributes_Add( ON_SimpleArray<const ON_3dmObjectAttributes*>* pArray, const ON_3dmObjectAttributes* pAttributes )
{
  if( pArray && pAttributes )
    pArray->Append(pAttributes);
}

/////////////////////////////////////////////////////////////////////////////
// ON_SimpleArray<ON_Curve*> 

RH_C_FUNCTION ON_SimpleArray<ON_Curve*>* ON_CurveArray_New(int initial_capacity)
{
  return new ON_SimpleArray<ON_Curve*>(initial_capacity);
}

RH_C_FUNCTION void ON_CurveArray_Append(ON_SimpleArray<ON_Curve*>* arrayPtr, ON_Curve* curvePtr)
{
  if( arrayPtr && curvePtr )
  {
    arrayPtr->Append( curvePtr );
  }
}

RH_C_FUNCTION int ON_CurveArray_Count(const ON_SimpleArray<ON_Curve*>* arrayPtr)
{
  int rc = 0;
  if( arrayPtr )
    rc = arrayPtr->Count();
  return rc;
}

RH_C_FUNCTION ON_Curve* ON_CurveArray_Get(ON_SimpleArray<ON_Curve*>* arrayPtr, int index)
{
  ON_Curve* rc = NULL;
  
  if( arrayPtr && index>=0 )
  {
    if( index<arrayPtr->Count() )
      rc = (*arrayPtr)[index];
  }
  return rc;
}

RH_C_FUNCTION void ON_CurveArray_Delete(ON_SimpleArray<ON_Curve*>* arrayPtr)
{
  if( arrayPtr )
    delete arrayPtr;
}

