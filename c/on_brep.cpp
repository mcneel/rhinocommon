#include "StdAfx.h"

//////////////////////////////////////////////////////////////////////////
// ON_BrepEdge

RH_C_FUNCTION double ON_BrepEdge_GetTolerance(const ON_BrepEdge* pConstBrepEdge)
{
  double rc = 0;
  if( pConstBrepEdge )
    rc = pConstBrepEdge->m_tolerance;
  return rc;
}

RH_C_FUNCTION void ON_BrepEdge_SetTolerance(ON_BrepEdge* pBrepEdge, double tol)
{
  if( pBrepEdge )
    pBrepEdge->m_tolerance = tol;
}

// IsSmoothManifoldEdge is not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_BrepEdge_IsSmoothManifoldEdge(const ON_BrepEdge* pConstBrepEdge, double angle_tol)
{
  bool rc = false;
  if( pConstBrepEdge )
    rc = pConstBrepEdge->IsSmoothManifoldEdge(angle_tol);
  return rc;
}

#endif

//////////////////////////////////////////////////////////////////////////
// ON_BrepTrim
RH_C_FUNCTION int ON_BrepTrim_Type(const ON_Brep* pConstBrep, int trim_index)
{
  int rc = 0;
  if( pConstBrep && trim_index>=0 && trim_index<pConstBrep->m_T.Count() )
    rc = (int)(pConstBrep->m_T[trim_index].m_type);
  return rc;
}

RH_C_FUNCTION void ON_BrepTrim_SetType(ON_Brep* pBrep, int trim_index, int trimtype)
{
  if( pBrep && trim_index>=0 && trim_index<pBrep->m_T.Count() )
    pBrep->m_T[trim_index].m_type = (ON_BrepTrim::TYPE)trimtype;
}

RH_C_FUNCTION int ON_BrepTrim_Iso(const ON_Brep* pConstBrep, int trim_index)
{
  int rc = 0;
  if( pConstBrep && trim_index>=0 && trim_index<pConstBrep->m_T.Count() )
    rc = (int)(pConstBrep->m_T[trim_index].m_iso);
  return rc;
}

RH_C_FUNCTION void ON_BrepTrim_SetIso(ON_Brep* pBrep, int trim_index, int iso)
{
  if( pBrep && trim_index>=0 && trim_index<pBrep->m_T.Count() )
    pBrep->m_T[trim_index].m_iso = (ON_Surface::ISO)iso;
}

RH_C_FUNCTION double ON_BrepTrim_Tolerance(const ON_Brep* pConstBrep, int trim_index, int which)
{
  double rc = 0;
  if( pConstBrep && trim_index>=0 && trim_index<pConstBrep->m_T.Count() )
    rc = pConstBrep->m_T[trim_index].m_tolerance[which];
  return rc;
}

RH_C_FUNCTION void ON_BrepTrim_SetTolerance(ON_Brep* pBrep, int trim_index, int which, double tolerance)
{
  if( pBrep && trim_index>=0 && trim_index<pBrep->m_T.Count() )
    pBrep->m_T[trim_index].m_tolerance[which] = tolerance;
}

RH_C_FUNCTION int ON_BrepTrim_ItemIndex(const ON_Brep* pConstBrep, int trim_index, int which)
{
  const int idxLoopIndex=0;
  const int idxFaceIndex=1;
  const int idxEdgeIndex=2;
  int rc = -1;
  if( pConstBrep && trim_index>=0 && trim_index<pConstBrep->m_T.Count() )
  {
    switch(which)
    {
    case idxLoopIndex:
      rc = pConstBrep->m_T[trim_index].m_li;
      break;
    case idxFaceIndex:
      rc = pConstBrep->m_T[trim_index].FaceIndexOf();
      break;
    case idxEdgeIndex:
      rc = pConstBrep->m_T[trim_index].m_ei;
      break;
    }
  }
  return rc;
}


//////////////////////////////////////////////////////////////////////////
// ON_BrepLoop

RH_C_FUNCTION int ON_BrepLoop_FaceIndex(const ON_Brep* pConstBrep, int loop_index)
{
  int rc = -1;
  if( pConstBrep && loop_index>=0 && loop_index<pConstBrep->m_L.Count() )
    rc = pConstBrep->m_L[loop_index].m_fi;
  return rc;
}

RH_C_FUNCTION int ON_BrepLoop_TrimIndex(const ON_BrepLoop* pConstLoop, int trim_index)
{
  int rc = -1;
  if( pConstLoop && trim_index>=0 && trim_index<pConstLoop->TrimCount())
  {
    const ON_BrepTrim* pConstTrim = pConstLoop->Trim(trim_index);
    if( pConstTrim )
      rc = pConstTrim->m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_BrepLoop_TrimCount(const ON_BrepLoop* pConstLoop)
{
  int rc = 0;
  if( pConstLoop )
    rc = pConstLoop->TrimCount();
  return rc;
}

RH_C_FUNCTION int ON_BrepLoop_Type(const ON_Brep* pConstBrep, int loop_index)
{
  int rc = 0;
  if( pConstBrep && loop_index>=0 && loop_index<pConstBrep->m_L.Count() )
    rc = (int)(pConstBrep->m_L[loop_index].m_type);
  return rc;
}

RH_C_FUNCTION ON_BrepLoop* ON_BrepLoop_GetPointer(const ON_Brep* pConstBrep, int loop_index)
{
  ON_BrepLoop* rc = NULL;
  if( pConstBrep )
    rc = pConstBrep->Loop(loop_index);
  return rc;
}

RH_C_FUNCTION ON_Curve* ON_BrepLoop_GetCurve3d(const ON_Brep* pConstBrep, int loop_index)
{
  ON_Curve* rc = NULL;
  if( pConstBrep )
  {
    ON_BrepLoop* pLoop = pConstBrep->Loop(loop_index);
    if( pLoop )
      rc = pConstBrep->Loop3dCurve(*pLoop, true);
  }
  return rc;
}

RH_C_FUNCTION ON_Curve* ON_BrepLoop_GetCurve2d(const ON_Brep* pConstBrep, int loop_index)
{
  ON_Curve* rc = NULL;
  if( pConstBrep )
  {
    ON_BrepLoop* pLoop = pConstBrep->Loop(loop_index);
    if( pLoop )
      rc = pConstBrep->Loop2dCurve(*pLoop);
  }
  return rc;
}

//////////////////////////////////////////////////////////////////////////
// ON_BrepFace

RH_C_FUNCTION int ON_BrepFace_LoopCount(const ON_BrepFace* pConstBrepFace)
{
  int rc = 0;
  if( pConstBrepFace )
    rc = pConstBrepFace->LoopCount();
  return rc;
}

RH_C_FUNCTION int ON_BrepFace_LoopIndex(const ON_BrepFace* pConstBrepFace, int index_in_face)
{
  int rc = -1;
  if( pConstBrepFace && index_in_face>=0 && index_in_face<pConstBrepFace->LoopCount() )
    rc = pConstBrepFace->m_li[index_in_face];
  return rc;
}

RH_C_FUNCTION int ON_BrepFace_OuterLoopIndex(const ON_BrepFace* pConstBrepFace)
{
  int rc = -1;
  if( pConstBrepFace )
  {
    ON_BrepLoop* pLoop = pConstBrepFace->OuterLoop();
    if( pLoop )
      rc = pLoop->m_loop_index;
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_BrepFace_BrepExtrudeFace(const ON_Brep* pConstBrep, int face_index, const ON_Curve* pConstCurve, bool bCap)
{
  ON_Brep* rc = NULL;
  if( pConstBrep && pConstCurve )
  {
    if( face_index >= 0 && face_index < pConstBrep->m_F.Count() )
    {
      ON_Brep* pNewBrep = ON_Brep::New( *pConstBrep );
      if( pNewBrep )
      {
        pNewBrep->DestroyMesh( ON::any_mesh );
        int result = ON_BrepExtrudeFace( *pNewBrep, face_index, *pConstCurve, bCap );
        // 0 == failure, 1 or 2 == success
        if( 0 == result )
          delete pNewBrep;
        else
          rc = pNewBrep;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_BrepFace_SurfaceIndex(const ON_BrepFace* pConstBrepFace)
{
  int rc = -1;
  if( pConstBrepFace )
    rc = pConstBrepFace->SurfaceIndexOf();
  return rc;
}

//////////////////////////////////////////////////////////////////////////
// ON_Brep

RH_C_FUNCTION ON_Brep* ON_Brep_New(const ON_Brep* pOther)
{
  if( pOther )
    return ON_Brep::New(*pOther);
  return ON_Brep::New();
}

#if !defined(OPENNURBS_BUILD)
class CRhHackBrep : public CRhinoBrepObject
{
public:
  void ClearBrep(){m_geometry=0;}
};
#endif

RH_C_FUNCTION bool ON_Brep_IsDuplicate(const ON_Brep* pConstBrep1, const ON_Brep* pConstBrep2, double tolerance)
{
  bool rc = false;
  if( pConstBrep1 && pConstBrep2 )
  {
#if !defined(OPENNURBS_BUILD)
    if( pConstBrep1==pConstBrep2 )
      return true;
    // Really lame that the Rhino SDK requires CRhinoObjects for
    // comparison, but it works for now.  Create temporary CRhinoBrep
    // objects that hold the ON_Breps and call RhinoCompareGeometry
    CRhHackBrep brepa;
    CRhHackBrep brepb;
    ON_Brep* pBrepA = const_cast<ON_Brep*>(pConstBrep1);
    ON_Brep* pBrepB = const_cast<ON_Brep*>(pConstBrep2);

    brepa.SetBrep(pBrepA);
    brepb.SetBrep(pBrepB);
    rc = ::RhinoCompareGeometry(&brepa, &brepb);
    brepa.ClearBrep();
    brepb.ClearBrep();
    //rc = pConstBrep1->IsDuplicate(*pConstBrep2, tolerance);
#endif
  }
  return rc;
}

RH_C_FUNCTION bool ON_Brep_IsValidTest(const ON_Brep* pConstBrep, int which_test, CRhCmnStringHolder* pStringHolder)
{
  const int idxIsValidTopology = 0;
  const int idxIsValidGeometry = 1;
  const int idxIsValidTolerancesAndFlags = 2;
  bool rc = false;
  if( pConstBrep )
  {
    ON_wString str;
    ON_TextLog log(str);
    ON_TextLog* _log = pStringHolder ? &log : NULL;

    switch(which_test)
    {
    case idxIsValidTopology:
      rc = pConstBrep->IsValidTopology(_log);
      break;
    case idxIsValidGeometry:
      rc = pConstBrep->IsValidGeometry(_log);
      break;
    case idxIsValidTolerancesAndFlags:
      rc = pConstBrep->IsValidTolerancesAndFlags(_log);
      break;
    default:
      break;
    }

    if( pStringHolder )
      pStringHolder->Set(str);
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ONC_BrepFromMesh( const ON_Mesh* pConstMesh, bool bTrimmedTriangles)
{
  ON_Brep* rc = NULL;
  if( pConstMesh )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    rc = ON_BrepFromMesh(top, bTrimmedTriangles);
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_FromBox( ON_3DPOINT_STRUCT boxmin, ON_3DPOINT_STRUCT boxmax)
{
  ON_Brep* rc = NULL;
  const ON_3dPoint* _boxmin = (const ON_3dPoint*)&boxmin;
  const ON_3dPoint* _boxmax = (const ON_3dPoint*)&boxmax;

  ON_3dPoint corners[8];
  corners[0] = *_boxmin;
  corners[1].x = _boxmax->x;
  corners[1].y = _boxmin->y;
  corners[1].z = _boxmin->z;

  corners[2].x = _boxmax->x;
  corners[2].y = _boxmax->y;
  corners[2].z = _boxmin->z;

  corners[3].x = _boxmin->x;
  corners[3].y = _boxmax->y;
  corners[3].z = _boxmin->z;

  corners[4].x = _boxmin->x;
  corners[4].y = _boxmin->y;
  corners[4].z = _boxmax->z;

  corners[5].x = _boxmax->x;
  corners[5].y = _boxmin->y;
  corners[5].z = _boxmax->z;

  corners[6].x = _boxmax->x;
  corners[6].y = _boxmax->y;
  corners[6].z = _boxmax->z;

  corners[7].x = _boxmin->x;
  corners[7].y = _boxmax->y;
  corners[7].z = _boxmax->z;
  rc = ::ON_BrepBox(corners);
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_FromBox2( /*ARRAY*/const ON_3dPoint* corners )
{
  ON_Brep* rc = NULL;
  if( corners )
    rc = ::ON_BrepBox(corners);
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_FromCylinder(ON_Cylinder* cylinder, bool capBottom, bool capTop)
{
  ON_Brep* rc = NULL;
  if( cylinder )
  {
    cylinder->circle.plane.UpdateEquation();
    rc = ON_BrepCylinder(*cylinder, capBottom, capTop);
  }
  return rc;
}

RH_C_FUNCTION void ON_Brep_DuplicateEdgeCurves(const ON_Brep* pBrep, ON_SimpleArray<ON_Curve*>* pOutCurves, bool nakedOnly)
{
  if (pBrep && pOutCurves)
  {
    for(int i = 0; i < pBrep->m_E.Count(); i++)
    {
      const ON_BrepEdge& edge = pBrep->m_E[i];
      if( nakedOnly && edge.m_ti.Count()!=1 )
        continue;

      ON_Curve* curve = edge.DuplicateCurve();
      if(curve)
      {
        // From RhinoScript:
        // make the curve direction go in the natural boundary loop direction
        // so that the curve directions come out consistantly
        if( pBrep->m_T[edge.m_ti[0]].m_bRev3d )
          curve->Reverse();
        if( pBrep->m_T[edge.m_ti[0]].Face()->m_bRev )
          curve->Reverse();

        pOutCurves->Append(curve);
      }
    }
  }
}

RH_C_FUNCTION void ON_Brep_DuplicateVertices( const ON_Brep* pBrep, ON_3dPointArray* outPoints)
{
  if( pBrep && outPoints )
  {
    for( int i = 0; i < pBrep->m_V.Count(); i++)
    {
      outPoints->Append(pBrep->m_V[i].point);
    }
  }
}

RH_C_FUNCTION void ON_Brep_Flip(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->Flip();
}

// SplitKinkyFaces is not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_Brep_SplitKinkyFaces(ON_Brep* pBrep, double tolerance, bool compact)
{
  bool rc = false;

  if( pBrep )
  {
    if (tolerance <= 0.0)
    {
       rc = pBrep->SplitKinkyFaces(ON_PI / 180.0, compact);
    }
    else
    {
      rc = pBrep->SplitKinkyFaces(tolerance, compact);
    }
  }
  
  return rc;
}

RH_C_FUNCTION bool ON_Brep_SplitKinkyFace(ON_Brep* pBrep, int face_index, double kink_tol)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->SplitKinkyFace(face_index, kink_tol);
  return rc;
}

RH_C_FUNCTION bool ON_Brep_SplitKinkyEdge(ON_Brep* pBrep, int edge_index, double kink_tol)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->SplitKinkyEdge(edge_index, kink_tol);
  return rc;
}

RH_C_FUNCTION int ON_Brep_SplitEdgeAtParameters(ON_Brep* pBrep, int edge_index, int count, /*ARRAY*/const double* parameters)
{
  int rc = 0;
  if( pBrep && count>0 && parameters )
    rc = pBrep->SplitEdgeAtParameters(edge_index, count, parameters);
  return rc;
}

#endif

RH_C_FUNCTION bool ON_Brep_ShrinkFaces( ON_Brep* pBrep )
{
  bool rc = false;

  if( pBrep )
  {
    rc = pBrep->ShrinkSurfaces(); 
    if( rc ) { pBrep->CullUnusedFaces(); }
  }

  return rc;
}

RH_C_FUNCTION int ON_Brep_GetInt(const ON_Brep* pConstBrep, int which)
{
  const int idxSolidOrientation = 0;
  const int idxFaceCount = 1;
  const int idxIsManifold = 2;
  const int idxEdgeCount = 3;
  const int idxLoopCount = 4;
  const int idxTrimCount = 5;
  const int idxSurfaceCount = 6;
  const int idxVertexCount = 7;
  const int idxC2Count = 8;
  const int idxC3Count = 9;

  int rc = 0;
  if( pConstBrep )
  {
    switch(which)
    {
    case idxSolidOrientation:
      rc = pConstBrep->SolidOrientation();
      break;
    case idxFaceCount:
      rc = pConstBrep->m_F.Count();
      break;
    case idxIsManifold:
      rc = pConstBrep->IsManifold()?1:0;
      break;
    case idxEdgeCount:
      rc = pConstBrep->m_E.Count();
      break;
    case idxLoopCount:
      rc = pConstBrep->m_L.Count();
      break;
    case idxTrimCount:
      rc = pConstBrep->m_T.Count();
      break;
    case idxSurfaceCount:
      rc = pConstBrep->m_S.Count();
      break;
    case idxVertexCount:
      rc = pConstBrep->m_V.Count();
      break;
    case idxC2Count:
      rc = pConstBrep->m_C2.Count();
      break;
    case idxC3Count:
      rc = pConstBrep->m_C3.Count();
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Brep_FaceIsSurface(const ON_Brep* pConstBrep, int faceIndex)
{
  bool rc = false;
  if( pConstBrep )
  {
    if( faceIndex<0 )
      rc = pConstBrep->IsSurface();
    else
      rc = pConstBrep->FaceIsSurface(faceIndex);
  }
  return rc;
}

RH_C_FUNCTION const ON_BrepFace* ON_Brep_BrepFacePointer( const ON_Brep* pConstBrep, int faceIndex )
{
  const ON_BrepFace* rc = NULL;
  if( pConstBrep )
  {
    rc = pConstBrep->Face(faceIndex);
  }
  return rc;
}

// not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION void ON_Brep_RebuildTrimsForV2(ON_Brep* pBrep, ON_BrepFace* pBrepFace, const ON_NurbsSurface* pConstNurbsSurface)
{
  if( pBrep && pBrepFace && pConstNurbsSurface )
    pBrep->RebuildTrimsForV2(*pBrepFace, *pConstNurbsSurface);
}

RH_C_FUNCTION bool ON_Brep_RebuildEdges(ON_Brep* pBrep, int face_index, double tolerance, bool rebuildSharedEdges, bool rebuildVertices)
{
  bool rc = false;
  if( pBrep && face_index>=0 && face_index<pBrep->m_F.Count() )
  {
    ON_BrepFace& face = pBrep->m_F[face_index];
    rc = pBrep->RebuildEdges(face, tolerance, rebuildSharedEdges?1:0, rebuildVertices?1:0);
  }
  return rc;
}

#endif

RH_C_FUNCTION void ON_Brep_Compact(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->Compact();
}

RH_C_FUNCTION bool ON_BrepFace_IsReversed( const ON_BrepFace* pConstFace )
{
  bool rc = false;
  if( pConstFace )
    rc = pConstFace->m_bRev;
  return rc;
}

RH_C_FUNCTION void ON_BrepFace_SetIsReversed( ON_BrepFace* pBrepFace, bool reversed )
{
  if( pBrepFace )
    pBrepFace->m_bRev = reversed;
}

// not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_BrepFace_ChangeSurface( ON_Brep* pBrep, int face_index, int surface_index )
{
  bool rc = false;
  if( pBrep && face_index>=0 && face_index<pBrep->m_F.Count() )
  {
    rc = pBrep->m_F[face_index].ChangeSurface(surface_index);
  }
  return rc;
}

#endif

RH_C_FUNCTION const ON_BrepEdge* ON_Brep_BrepEdgePointer( const ON_Brep* pConstBrep, int edgeIndex )
{
  const ON_BrepEdge* rc = NULL;
  if( pConstBrep )
  {
    rc = pConstBrep->Edge(edgeIndex);
  }
  return rc;
}

RH_C_FUNCTION const ON_BrepTrim* ON_Brep_BrepTrimPointer( const ON_Brep* pConstBrep, int trimIndex )
{
  const ON_BrepTrim* rc = NULL;
  if( pConstBrep )
  {
    rc = pConstBrep->Trim(trimIndex);
  }
  return rc;
}

RH_C_FUNCTION const ON_Surface* ON_Brep_BrepSurfacePointer( const ON_Brep* pConstBrep, int surfaceIndex )
{
  const ON_Surface* rc = NULL;
  if( pConstBrep && surfaceIndex>=0 && surfaceIndex<pConstBrep->m_S.Count() )
  {
    rc = pConstBrep->m_S[surfaceIndex];
  }
  return rc;
}

RH_C_FUNCTION const ON_Curve* ON_Brep_BrepCurvePointer( const ON_Brep* pConstBrep, int curveIndex, bool c2 )
{
  const ON_Curve* rc = NULL;
  if( pConstBrep && curveIndex>=0 )
  {
    if( c2 && curveIndex<pConstBrep->m_C2.Count() )
      rc = pConstBrep->m_C2[curveIndex];
    else if( !c2 && curveIndex<pConstBrep->m_C3.Count() )
      rc = pConstBrep->m_C3[curveIndex];
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_AddCurve( ON_Brep* pBrep, const ON_Curve* pConstCurve, bool c2 )
{
  int rc = -1;
  if( pBrep && pConstCurve )
  {
    ON_Curve* pCurve = pConstCurve->Duplicate();
    if( pCurve )
    {
      if( c2 )
      {
        pBrep->m_C2.Append(pCurve);
        rc = pBrep->m_C2.Count()-1;
      }
      else
      {
        pBrep->m_C3.Append(pCurve);
        rc = pBrep->m_C3.Count()-1;
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_FromSurface( const ON_Surface* pConstSurface )
{
  ON_Brep* rc = NULL;
  if( pConstSurface )
  {
    ON_Brep* pNewBrep = ON_Brep::New();
    if( pNewBrep )
    {
      ON_Surface* pNewSurface = pConstSurface->DuplicateSurface();
      if( pNewSurface )
      {
        if( pNewBrep->Create(pNewSurface) )
          rc = pNewBrep;

        if( NULL==rc )
          delete pNewSurface;
      }
      if( NULL==rc )
        delete pNewBrep;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_DuplicateFace( const ON_Brep* pConstBrep, int faceIndex, bool duplicateMeshes )
{
  ON_Brep* rc = NULL;

  if( pConstBrep )
    rc = pConstBrep->DuplicateFace(faceIndex, duplicateMeshes?TRUE:FALSE);
  return rc;
}

RH_C_FUNCTION ON_Surface* ON_Brep_DuplicateFaceSurface( const ON_Brep* pConstBrep, int faceIndex )
{
  ON_Surface* rc = NULL;
  if( pConstBrep )
  {
    ON_BrepFace* pFace = pConstBrep->Face(faceIndex);
    if( pFace )
    {
      const ON_Surface* pSurf = pFace->SurfaceOf();
      if( pSurf )
        rc = pSurf->DuplicateSurface();
    }
  }
  return rc;
}

RH_C_FUNCTION const ON_Surface* ON_BrepFace_SurfaceOf( const ON_Brep* pConstBrep, int faceIndex )
{
  const ON_Surface* rc = NULL;
  if( pConstBrep )
  {
    ON_BrepFace* pFace = pConstBrep->Face(faceIndex);
    if( pFace )
      rc = pFace->SurfaceOf();
  }
  return rc;
}

RH_C_FUNCTION const ON_Mesh* ON_BrepFace_Mesh( const ON_Brep* pConstBrep, int faceIndex, int meshtype )
{
  const ON_Mesh* rc = NULL;
  if( pConstBrep )
  {
    ON_BrepFace* pFace = pConstBrep->Face(faceIndex);
    if( pFace )
    {
      rc = pFace->Mesh( ON::MeshType(meshtype) );
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_BrepFace_SetMesh( ON_BrepFace* pBrepFace, ON_Mesh* pMesh, int meshtype )
{
  bool rc = false;
  if( pBrepFace && pMesh )
  {
    rc = pBrepFace->SetMesh( ON::MeshType(meshtype), pMesh );
  }
  return rc;
}

RH_C_FUNCTION const ON_Brep* ON_BrepSubItem_Brep( const ON_Geometry* pConstGeometry, int* index )
{
  const ON_Brep* rc = NULL;
  if( index )
  {
    const ON_BrepFace* pBrepFace = ON_BrepFace::Cast(pConstGeometry);
    const ON_BrepEdge* pBrepEdge = ON_BrepEdge::Cast(pConstGeometry);
    const ON_BrepTrim* pBrepTrim = ON_BrepTrim::Cast(pConstGeometry);
    if( pBrepFace )
    {
      rc = pBrepFace->Brep();
      *index = pBrepFace->m_face_index;
    }
    else if( pBrepEdge )
    {
      rc = pBrepEdge->Brep();
      *index = pBrepEdge->m_edge_index;
    }
    else if( pBrepTrim )
    {
      rc = pBrepTrim->Brep();
      *index = pBrepTrim->m_trim_index;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_EdgeTrimCount( const ON_Brep* pConstBrep, int edge_index )
{
  int rc = 0;
  if( pConstBrep )
  {
    const ON_BrepEdge* pEdge = pConstBrep->Edge(edge_index);
    if( pEdge )
      rc = pEdge->m_ti.Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_EdgeFaceIndices( const ON_Brep* pConstBrep, int edge_index, ON_SimpleArray<int>* fi )
{
  int rc = 0;
  if( pConstBrep && fi )
  {
    const ON_BrepEdge* pEdge = pConstBrep->Edge(edge_index);
    
    int trimCount = pEdge->TrimCount();
    for( int i = 0; i < trimCount; i++)
    {
      const ON_BrepTrim* pTrim = pEdge->Trim(i);
      const ON_BrepFace* pFace = pTrim->Face();
      fi->Append(pFace->m_face_index);
    }

    rc = fi->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_FaceEdgeIndices( const ON_Brep* pConstBrep, int face_index, ON_SimpleArray<int>* ei )
{
  int rc = 0;
  if( pConstBrep && ei  )
  {
    const ON_BrepFace* pFace = pConstBrep->Face(face_index);
    if( pFace )
    {
      int loopCount = pFace->LoopCount();
      for( int i = 0; i < loopCount; i++)
      {
        const ON_BrepLoop* pLoop = pFace->Loop(i);
        if( NULL==pLoop )
          continue;

        int trimCount = pLoop->TrimCount();
        for( int j = 0; j < trimCount; j++)
        {
          const ON_BrepTrim* pTrim = pLoop->Trim(j);
          if( NULL==pTrim )
            continue;
          const ON_BrepEdge* pEdge = pTrim->Edge();
          if( pEdge )
            ei->Append(pEdge->m_edge_index);
        }
      }
      rc = ei->Count();
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_FaceFaceIndices( const ON_Brep* pConstBrep, int face_index, ON_SimpleArray<int>* fi )
{
  int rc = 0;
  if( pConstBrep && fi )
  {
    int faceCount = pConstBrep->m_F.Count();
    if( face_index >= faceCount )
      return 0;

    ON_SimpleArray<bool> map(faceCount);
    for( int i = 0; i < faceCount; i++ )
      map[i] = false;
    map[face_index] = true;

    const ON_BrepFace* pFace = pConstBrep->Face(face_index);
    
    int loopCount = pFace->LoopCount();
    for( int i = 0; i < loopCount; i++ )
    {
      const ON_BrepLoop* pLoop = pFace->Loop(i);
      if( NULL==pLoop )
        continue;
      int trimCount = pLoop->TrimCount();

      for( int j = 0; j < trimCount; j++ )
      {
        const ON_BrepTrim* pTrim = pLoop->Trim(j);
        if( NULL==pTrim )
          continue;
        const ON_BrepEdge* pEdge = pTrim->Edge();
        if( NULL==pEdge )
          continue;

        int edgetrimCount = pEdge->TrimCount();
        for( int k = 0; k < edgetrimCount; k++ )
        {
          const ON_BrepTrim* peTrim = pEdge->Trim(k);
          if( NULL==peTrim )
            continue;
          ON_BrepFace* peTrimFace = peTrim->Face();
          if( NULL==peTrimFace )
            continue;
          int index = peTrimFace->m_face_index;
          if (!map[index])
          {
            fi->Append(index);
            map[index] = true;
          }
        }
      }
    }
    rc = fi->Count();
  }
  return rc;
}

// not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION ON_Brep* ON_Brep_CopyTrims( const ON_BrepFace* pConstBrepFace, const ON_Surface* pConstSurface, double tolerance)
{
  ON_Brep* rc = NULL;

  if( pConstBrepFace && pConstSurface )
  {
    ON_Brep* brep = pConstBrepFace->Brep();
    int fi = pConstBrepFace->m_face_index;

    ON_Brep* brp = brep->DuplicateFace(fi, FALSE);
    ON_Surface* srf = pConstSurface->DuplicateSurface();
    
    int si = brp->AddSurface(srf);
    brp->m_F[0].ChangeSurface(si);

    if (brp->RebuildEdges(brp->m_F[0], tolerance, TRUE, TRUE))
    { brp->Compact(); }
    else
    { delete brp; }

    rc = brp;
  }

  return rc;
}

#endif

RH_C_FUNCTION int ON_Brep_AddTrimCurve( ON_Brep* pBrep, const ON_Curve* pConstCurve )
{
  int rc = -1;
  if( pBrep && pConstCurve )
  {
    ON_Curve* pCurve = pConstCurve->DuplicateCurve();
    if( pCurve )
      rc = pBrep->AddTrimCurve(pCurve);
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_AddEdgeCurve( ON_Brep* pBrep, const ON_Curve* pConstCurve )
{
  int rc = -1;
  if( pBrep && pConstCurve )
  {
    ON_Curve* pCurve = pConstCurve->DuplicateCurve();
    if( pCurve )
      rc = pBrep->AddEdgeCurve(pCurve);
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_AddSurface( ON_Brep* pBrep, const ON_Surface* pConstSurface )
{
  int rc = -1;
  if( pBrep && pConstSurface )
  {
    ON_Surface* pNewSurface = pConstSurface->DuplicateSurface();
    if( pNewSurface )
    {
      rc = pBrep->AddSurface(pNewSurface);
      if( -1==rc )
        delete pNewSurface;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Brep_SetEdgeCurve( ON_Brep* pBrep, int edgecurveIndex, int c3Index, ON_INTERVAL_STRUCT subdomain )
{
  bool rc = false;
  if( pBrep && edgecurveIndex>=0 && edgecurveIndex<pBrep->m_E.Count() )
  {
    ON_BrepEdge& edge = pBrep->m_E[edgecurveIndex];
    ON_Interval _subdomain(subdomain.val[0], subdomain.val[1]);
    if( _subdomain.IsValid() )
      rc = pBrep->SetEdgeCurve(edge, c3Index, &_subdomain);
    else
      rc = pBrep->SetEdgeCurve(edge, c3Index);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Brep_SetTrimCurve( ON_Brep* pBrep, int trimcurveIndex, int c3Index, ON_INTERVAL_STRUCT subdomain )
{
  bool rc = false;
  if( pBrep && trimcurveIndex>=0 && trimcurveIndex<pBrep->m_T.Count() )
  {
    ON_BrepTrim& trim = pBrep->m_T[trimcurveIndex];
    ON_Interval _subdomain(subdomain.val[0], subdomain.val[1]);
    if( _subdomain.IsValid() )
      rc = pBrep->SetTrimCurve(trim, c3Index, &_subdomain);
    else
      rc = pBrep->SetTrimCurve(trim, c3Index);
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewVertex( ON_Brep* pBrep )
{
  int rc = -1;
  if( pBrep )
  {
    ON_BrepVertex& vertex = pBrep->NewVertex();
    rc = vertex.m_vertex_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewVertex2( ON_Brep* pBrep, ON_3DPOINT_STRUCT point, double tolerance )
{
  int rc = -1;
  if( pBrep )
  {
    ON_3dPoint _point(point.val);
    ON_BrepVertex& vertex = pBrep->NewVertex(_point, tolerance);
    rc = vertex.m_vertex_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewEdge( ON_Brep* pBrep, int curveIndex )
{
  int rc = -1;
  if( pBrep )
  {
    ON_BrepEdge& edge = pBrep->NewEdge(curveIndex);
    rc = edge.m_edge_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewEdge2( ON_Brep* pBrep, int vertex1, int vertex2, int curveIndex, ON_INTERVAL_STRUCT subdomain, double tolerance )
{
  int rc = -1;
  if( pBrep && vertex1>=0 && vertex1<pBrep->m_V.Count() && vertex2>=0 && vertex2<pBrep->m_V.Count() )
  {
    ON_BrepVertex& start = pBrep->m_V[vertex1];
    ON_BrepVertex& end = pBrep->m_V[vertex2];
    ON_Interval _subdomain(subdomain.val[0], subdomain.val[1]);
    const ON_Interval* interval = _subdomain.IsValid() ? &_subdomain: NULL;
    ON_BrepEdge& edge = pBrep->NewEdge(start, end, curveIndex, interval, tolerance);
    rc = edge.m_edge_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewFace(ON_Brep* pBrep, int si)
{
  int rc = -1;
  if( pBrep )
  {
    ON_BrepFace& face = pBrep->NewFace(si);
    rc = face.m_face_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewFace2(ON_Brep* pBrep, const ON_Surface* pConstSurface)
{
  int rc = -1;
  if( pBrep && pConstSurface )
  {
    ON_BrepFace* pFace = pBrep->NewFace(*pConstSurface);
    if( pFace )
      rc = pFace->m_face_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewRuledFace(ON_Brep* pBrep, int edgeA, bool revEdgeA, int edgeB, bool revEdgeB)
{
  int rc = -1;
  if( pBrep && edgeA>=0 && edgeA<pBrep->m_E.Count() && edgeB>=0 && edgeB<pBrep->m_E.Count() )
  {
    ON_BrepEdge& _edgeA = pBrep->m_E[edgeA];
    ON_BrepEdge& _edgeB = pBrep->m_E[edgeB];
    ON_BrepFace* pFace = pBrep->NewRuledFace(_edgeA, revEdgeA, _edgeB, revEdgeB);
    if( pFace )
      rc = pFace->m_face_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewConeFace(ON_Brep* pBrep, int vertexIndex, int edgeIndex, bool revEdge)
{
  int rc = -1;
  if( pBrep && vertexIndex>=0 && vertexIndex<pBrep->m_V.Count() && edgeIndex>=0 && edgeIndex<pBrep->m_E.Count() )
  {
    ON_BrepVertex& vertex = pBrep->m_V[vertexIndex];
    ON_BrepEdge& edge = pBrep->m_E[edgeIndex];
    ON_BrepFace* pFace = pBrep->NewConeFace(vertex, edge, revEdge);
    if( pFace )
      rc = pFace->m_face_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewLoop(ON_Brep* pBrep, int loopType, int face_index)
{
  ON_BrepLoop::TYPE _looptype = (ON_BrepLoop::TYPE)loopType;
  int rc = -1;
  if( pBrep )
  {
    if( face_index>=0 )
    {
      ON_BrepFace* pBrepFace = pBrep->Face(face_index);
      if( pBrepFace )
      {
        ON_BrepLoop& loop = pBrep->NewLoop(_looptype, *pBrepFace);
        rc = loop.m_loop_index;
      }
    }
    else
    {
      ON_BrepLoop& loop = pBrep->NewLoop(_looptype);
      rc = loop.m_loop_index;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewOuterLoop(ON_Brep* pBrep, int faceIndex)
{
  int rc = -1;
  if( pBrep )
  {
    ON_BrepLoop* pLoop = pBrep->NewOuterLoop(faceIndex);
    if( pLoop )
      rc = pLoop->m_loop_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewPlanarFaceLoop(ON_Brep* pBrep, int faceIndex, int loopType, ON_SimpleArray<ON_Curve*>* pCurveArray)
{
  int rc = -1;
  if( pBrep && pCurveArray )
  {
    ON_BrepLoop::TYPE _looptype = (ON_BrepLoop::TYPE)loopType;
    if( pBrep->NewPlanarFaceLoop(faceIndex, _looptype, *pCurveArray, true) )
    {
      ON_BrepLoop* pLoop = pBrep->m_L.Last();
      if( pLoop )
        rc = pLoop->m_loop_index;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_BrepVertex* ON_BrepVertex_GetPointer(ON_Brep* pBrep, int index)
{
  if( pBrep )
    return pBrep->m_V.At(index);
  return NULL;
}

RH_C_FUNCTION int ON_Brep_NewTrim( ON_Brep* pBrep, int curveIndex )
{
  int rc = -1;
  if( pBrep )
  {
    ON_BrepTrim& trim = pBrep->NewTrim(curveIndex);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewTrim2( ON_Brep* pBrep, bool bRev3d, int loopIndex, int c2i )
{
  int rc = -1;
  if( pBrep && loopIndex>=0 && loopIndex<pBrep->m_L.Count() )
  {
    ON_BrepLoop& loop = pBrep->m_L[loopIndex];
    ON_BrepTrim& trim = pBrep->NewTrim(bRev3d, loop, c2i);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewTrim3( ON_Brep* pBrep, bool bRev3d, int edgeIndex, int c2i )
{
  int rc = -1;
  if( pBrep && edgeIndex>=0 && edgeIndex<pBrep->m_E.Count() )
  {
    ON_BrepEdge& edge = pBrep->m_E[edgeIndex];
    ON_BrepTrim& trim = pBrep->NewTrim(edge, bRev3d, c2i);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewTrim4( ON_Brep* pBrep, int edgeIndex, bool bRev3d, int loopIndex, int c2i )
{
  int rc = -1;
  if( pBrep && edgeIndex>=0 && edgeIndex<pBrep->m_E.Count() && loopIndex>=0 && loopIndex<pBrep->m_L.Count() )
  {
    ON_BrepEdge& edge = pBrep->m_E[edgeIndex];
    ON_BrepLoop& loop = pBrep->m_L[loopIndex];
    ON_BrepTrim& trim = pBrep->NewTrim(edge, bRev3d, loop, c2i);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewSingularTrim(ON_Brep* pBrep, int vertexIndex, int loopIndex, int iso, int c2i)
{
  int rc = -1;
  if( pBrep && vertexIndex>=0 && vertexIndex<pBrep->m_V.Count() && loopIndex>=0 && loopIndex<pBrep->m_L.Count() )
  {
    ON_BrepVertex& vertex = pBrep->m_V[vertexIndex];
    ON_BrepLoop& loop = pBrep->m_L[loopIndex];
    ON_Surface::ISO _iso = (ON_Surface::ISO)iso;
    ON_BrepTrim& trim = pBrep->NewSingularTrim(vertex, loop, _iso, c2i);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewPointOnFace(ON_Brep* pBrep, int faceIndex, double s, double t)
{
  int rc = -1;
  if( pBrep && faceIndex>=0 && faceIndex<pBrep->m_F.Count() )
  {
    ON_BrepFace& face = pBrep->m_F[faceIndex];
    ON_BrepVertex& vertex = pBrep->NewPointOnFace(face, s, t);
    rc = vertex.m_vertex_index;
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_NewCurveOnFace(ON_Brep* pBrep, int faceIndex, int edgeIndex, bool bRev3d, int c2i)
{
  int rc = -1;
  if( pBrep && faceIndex>=0 && faceIndex<pBrep->m_F.Count() && edgeIndex>=0 && edgeIndex<pBrep->m_E.Count() )
  {
    ON_BrepFace& face = pBrep->m_F[faceIndex];
    ON_BrepEdge& edge = pBrep->m_E[edgeIndex];
    ON_BrepTrim& trim = pBrep->NewCurveOnFace(face, edge, bRev3d, c2i);
    rc = trim.m_trim_index;
  }
  return rc;
}

RH_C_FUNCTION void ON_Brep_Append(ON_Brep* pBrep, const ON_Brep* pConstOtherBrep)
{
  if(pBrep && pConstOtherBrep)
    pBrep->Append(*pConstOtherBrep);
}

RH_C_FUNCTION void ON_Brep_SetVertices(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->SetVertices();
}

RH_C_FUNCTION void ON_Brep_SetTrimIsoFlags(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->SetTrimIsoFlags();
}

RH_C_FUNCTION ON_Brep* ONC_ON_BrepCone( const ON_Cone* cone, bool cap )
{
  ON_Brep* rc = NULL;
  if( cone )
  {
    ON_Cone* pCone = const_cast<ON_Cone*>(cone);
    pCone->plane.UpdateEquation();
    rc = ON_BrepCone(*cone, cap?1:0);
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ONC_ON_BrepRevSurface( const ON_RevSurface* pConstRevSurface, bool capStart, bool capEnd )
{
  ON_Brep* rc = NULL;
  if( pConstRevSurface )
  {
    ON_RevSurface* pRevSurface = pConstRevSurface->Duplicate();
    rc = ON_BrepRevSurface(pRevSurface, capStart?1:0, capEnd?1:0);
  }
  return rc;
}

RH_C_FUNCTION void ON_Brep_DeleteFace( ON_Brep* pBrep, int faceIndex )
{
  if( pBrep )
  {
    ON_BrepFace* pFace = pBrep->Face(faceIndex);
    if( pFace )
    {
      pBrep->DeleteFace(*pFace, TRUE);
      pBrep->Compact();
    }
  }
}

RH_C_FUNCTION bool ON_Brep_FlipReversedSurfaces(ON_Brep* pBrep)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->FlipReversedSurfaces();
  return rc;
}

// not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_Brep_SplitClosedFaces(ON_Brep* pBrep, int min_degree)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->SplitClosedFaces(min_degree);
  return rc;
}

RH_C_FUNCTION bool ON_Brep_SplitBipolarFaces(ON_Brep* pBrep)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->SplitBipolarFaces();
  return rc;
}

#endif

RH_C_FUNCTION ON_Brep* ON_Brep_SubBrep(const ON_Brep* pConstBrep, int count, /*ARRAY*/int* face_indices)
{
  ON_Brep* rc = NULL;
  if( pConstBrep && count>0 && face_indices)
    rc = pConstBrep->SubBrep(count, face_indices);
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Brep_ExtractFace(ON_Brep* pBrep, int face_index)
{
  ON_Brep* rc = NULL;
  if( pBrep )
    rc = pBrep->ExtractFace(face_index);
  return rc;
}

RH_C_FUNCTION bool ON_Brep_StandardizeFaceSurface(ON_Brep* pBrep, int face_index)
{
  bool rc = false;
  if( pBrep )
    rc = pBrep->StandardizeFaceSurface(face_index);
  return rc;
}

RH_C_FUNCTION void ON_Brep_StandardizeFaceSurfaces(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->StandardizeFaceSurfaces();
}

RH_C_FUNCTION void ON_Brep_Standardize(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->Standardize();
}

RH_C_FUNCTION bool ON_Brep_CullUnused(ON_Brep* pBrep, int which)
{
  const int idxCullUnusedFaces = 0;
  const int idxCullUnusedLoops = 1;
  const int idxCullUnusedTrims = 2;
  const int idxCullUnusedEdges = 3;
  const int idxCullUnusedVertices = 4;
  const int idxCullUnused3dCurves = 5;
  const int idxCullUnused2dCurves = 6;
  const int idxCullUnusedSurfaces = 7;
  bool rc = false;
  if( pBrep )
  {
    switch(which)
    {
    case idxCullUnusedFaces:
      rc = pBrep->CullUnusedFaces();
      break;
    case idxCullUnusedLoops:
      rc = pBrep->CullUnusedLoops();
      break;
    case idxCullUnusedTrims:
      rc = pBrep->CullUnusedTrims();
      break;
    case idxCullUnusedEdges:
      rc = pBrep->CullUnusedEdges();
      break;
    case idxCullUnusedVertices:
      rc = pBrep->CullUnusedVertices();
      break;
    case idxCullUnused3dCurves:
      rc = pBrep->CullUnused3dCurves();
      break;
    case idxCullUnused2dCurves:
      rc = pBrep->CullUnused2dCurves();
      break;
    case idxCullUnusedSurfaces:
      rc = pBrep->CullUnusedSurfaces();
      break;
    default:
      break;
    }
  }
  return rc;
}

// Declared but not implemented in opennurbs
//RH_C_FUNCTION int ON_Brep_MergeFaces(ON_Brep* pBrep, int face0, int face1)
//{
//  int rc = -1;
//  if( pBrep )
//    rc = pBrep->MergeFaces(face0, face1);
//  return rc;
//}
//
//RH_C_FUNCTION bool ON_Brep_MergeFaces2(ON_Brep* pBrep)
//{
//  bool rc = false;
//  if( pBrep )
//    rc = pBrep->MergeFaces();
//  return rc;
//}

// Region topology information is not currently available in stand alone OpenNURBS build
#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION int ON_Brep_RegionTopologyCount(const ON_Brep* pConstBrep, bool region)
{
  int rc = 0;
  if( pConstBrep )
  {
    if( region )
      rc = pConstBrep->RegionTopology().m_R.Count();
    else
      rc = pConstBrep->RegionTopology().m_FS.Count();
  }
  return rc;
}

RH_C_FUNCTION bool ON_BrepRegion_IsFinite(const ON_Brep* pConstBrep, int index)
{
  bool rc = false;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( index>=0 && index<top.m_R.Count() )
      rc = top.m_R[index].IsFinite();
  }
  return rc;
}

RH_C_FUNCTION void ON_BrepRegion_BoundingBox(const ON_Brep* pConstBrep, int index, ON_BoundingBox* bbox)
{
  if( pConstBrep && bbox )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( index>=0 && index<top.m_R.Count() )
      *bbox = top.m_R[index].BoundingBox();
  }
}

RH_C_FUNCTION ON_Brep* ON_BrepRegion_RegionBoundaryBrep(const ON_Brep* pConstBrep, int index)
{
  ON_Brep* rc = NULL;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( index>=0 && index<top.m_R.Count() )
      rc = top.m_R[index].RegionBoundaryBrep();
  }
  return rc;
}

RH_C_FUNCTION bool ON_BrepRegion_IsPointInside(const ON_Brep* pConstBrep, int index, ON_3DPOINT_STRUCT point, double tolerance, bool strictly_inside)
{
  bool rc = false;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( index>=0 && index<top.m_R.Count() )
    {
      ON_3dPoint _point(point.val);
      rc = top.m_R[index].IsPointInside(_point, tolerance, strictly_inside);
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_BrepRegion_FaceSideCount(const ON_Brep* pConstBrep, int index)
{
  int rc = 0;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( index>=0 && index<top.m_R.Count() )
      rc = top.m_R[index].m_fsi.Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_BrepFaceSide_SurfaceNormalDirection(const ON_Brep* pConstBrep, int region_index, int face_index)
{
  int rc = 1;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( region_index>=0 && region_index<top.m_R.Count() )
    {
      ON_BrepFaceSide* face_side = top.m_R[region_index].FaceSide(face_index);
      if( face_side )
        rc = face_side->SurfaceNormalDirection();
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_BrepFaceSide_Face(const ON_Brep* pConstBrep, int region_index, int face_index)
{
  int rc = -1;
  if( pConstBrep )
  {
    const ON_BrepRegionTopology& top = pConstBrep->RegionTopology();
    if( region_index>=0 && region_index<top.m_R.Count() )
    {
      ON_BrepFaceSide* face_side = top.m_R[region_index].FaceSide(face_index);
      if( face_side )
        rc = face_side->m_fi;
    }
  }
  return rc;
}

#endif

////////////////////////////////////////////////////////////////////////////////////
// Meshing and mass property calculations are not available in stand alone opennurbs

#if !defined(OPENNURBS_BUILD)

RH_C_FUNCTION ON_MassProperties* ON_Brep_MassProperties(bool bArea, const ON_Brep* pBrep, double relativeTolerance, double absoluteTolerance)
{
  ON_MassProperties* rc = NULL;
  if( pBrep )
  {
    rc = new ON_MassProperties();
    bool success = false;
    if( bArea )
      success = pBrep->AreaMassProperties(*rc, true, true, true, true, relativeTolerance, absoluteTolerance);
    else
      success = pBrep->VolumeMassProperties(*rc, true, true, true, true, ON_UNSET_POINT, relativeTolerance, absoluteTolerance);
    if( !success )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}

RH_C_FUNCTION double ON_Brep_Area(const ON_Brep* pBrep, double relativeTolerance, double absoluteTolerance)
{
  double area = 0.0;
  if( pBrep )
  {
    ON_MassProperties rc;
    bool success = false;
    success = pBrep->AreaMassProperties(rc, true, false, false, false, relativeTolerance, absoluteTolerance);
    area = rc.Area();
  }
  return area;
}
RH_C_FUNCTION double ON_Brep_Volume(const ON_Brep* pBrep, double relativeTolerance, double absoluteTolerance)
{
  double volume = 0.0;
  if( pBrep )
  {
    ON_MassProperties rc;
    bool success = false;
    success = pBrep->VolumeMassProperties(rc, true, false, false, false, ON_UNSET_POINT, relativeTolerance, absoluteTolerance);
    volume = rc.Volume();
  }
  return volume;
}

RH_C_FUNCTION ON_MassProperties* ON_Geometry_AreaMassProperties(const ON_SimpleArray<const ON_Geometry*>* pConstGeometryArray, double relativeTolerance, double absoluteTolerance)
{
  ON_MassProperties* rc = NULL;
  if( pConstGeometryArray && pConstGeometryArray->Count() > 0 )
  {
    ON_BoundingBox bbox;
    for( int i = 0; i < pConstGeometryArray->Count(); i++ )
    {
      const ON_Geometry* geo = (*pConstGeometryArray)[i];
      if( NULL==geo )
        continue;
      geo->GetBoundingBox(bbox,TRUE);
    }
    ON_3dPoint basepoint = bbox.Center();


    // Aggregate all mass properties
    for( int i = 0; i < pConstGeometryArray->Count(); i++ )
    {
      const ON_Geometry* geo = (*pConstGeometryArray)[i];
      if( NULL==geo )
        continue;

      bool success = false;
      ON_MassProperties mp;

      const ON_Brep* pBrep = ON_Brep::Cast(geo);
      if( pBrep )
        success = pBrep->AreaMassProperties(mp, true, true, true, true, relativeTolerance, absoluteTolerance);

      const ON_Surface* pSurface = success?0:ON_Surface::Cast(geo);
      if( pSurface )
        success = pSurface->AreaMassProperties(mp, true, true, true, true, relativeTolerance, absoluteTolerance);

      const ON_Mesh* pMesh = success?0:ON_Mesh::Cast(geo);
      if( pMesh )
        success = pMesh->AreaMassProperties(mp, true, true, true, true);

      const ON_Curve* pCurve = success?0:ON_Curve::Cast(geo);
      ON_Plane plane;
      if( pCurve && pCurve->IsPlanar(&plane, absoluteTolerance) && pCurve->IsClosed() )
        success = pCurve->AreaMassProperties(basepoint, plane.Normal(), mp, true, true, true, true, relativeTolerance, absoluteTolerance);

      if( success )
      {
        if( NULL==rc )
          rc = new ON_MassProperties(mp);
        else
          rc->Sum(1, &mp, true);
      }
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_CreateMesh( const ON_Brep* pConstBrep, ON_SimpleArray<ON_Mesh*>* meshes )
{
  int rc = 0;
  if( pConstBrep && meshes )
  {
    ON_MeshParameters mp;
    pConstBrep->CreateMesh(mp, *meshes);
    int count = meshes->Count();
    for( int i=count-1; i>=0; i-- )
    {
      ON_Mesh* pMesh = (*meshes)[i];
      if( NULL==pMesh )
        meshes->Remove(i);
    }
    rc = meshes->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_CreateMesh2( const ON_Brep* pConstBrep, ON_SimpleArray<ON_Mesh*>* meshes, 
                                      bool bSimplePlanes, 
                                      bool bRefine, 
                                      bool bJaggedSeams, 
                                      bool bComputeCurvature, 
                                      int grid_min_count, 
                                      int grid_max_count, 
                                      int face_type, 
                                      double tolerance, 
                                      double min_tolerance, 
                                      double relative_tolerance, 
                                      double grid_amplification, 
                                      double grid_angle, 
                                      double grid_aspect_ratio, 
                                      double refine_angle, 
                                      double min_edge_length, 
                                      double max_edge_length )
{
  int rc = 0;
  if( pConstBrep && meshes )
  {
    ON_MeshParameters mp;
    mp.m_bComputeCurvature = bComputeCurvature;
    mp.m_bJaggedSeams = bJaggedSeams;
    mp.m_bRefine = bRefine;
    mp.m_bSimplePlanes = bSimplePlanes;
    mp.m_face_type = face_type;
    mp.m_grid_amplification = grid_amplification;
    mp.m_grid_angle = grid_angle;
    mp.m_grid_aspect_ratio = grid_aspect_ratio;
    mp.m_grid_max_count = grid_max_count;
    mp.m_grid_min_count = grid_min_count;
    mp.m_max_edge_length = max_edge_length;
    mp.m_min_edge_length = min_edge_length;
    mp.m_min_tolerance = min_tolerance;
    mp.m_refine_angle = refine_angle;
    mp.m_relative_tolerance = relative_tolerance;
    mp.m_texture_range = 2;
    mp.m_tolerance = tolerance;

    pConstBrep->CreateMesh(mp, *meshes);
    int count = meshes->Count();
    for( int i=count-1; i>=0; i-- )
    {
      ON_Mesh* pMesh = (*meshes)[i];
      if( NULL==pMesh )
        meshes->Remove(i);
    }
    rc = meshes->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_CreateMesh3( const ON_Brep* pConstBrep, ON_SimpleArray<ON_Mesh*>* meshes, const ON_MeshParameters* pConstMeshParameters )
{
  int rc = 0;
  if( pConstBrep && meshes && pConstMeshParameters )
  {
    pConstBrep->CreateMesh(*pConstMeshParameters, *meshes);
    int count = meshes->Count();
    for( int i=count-1; i>=0; i-- )
    {
      ON_Mesh* pMesh = (*meshes)[i];
      if( NULL==pMesh )
        meshes->Remove(i);
    }
    rc = meshes->Count();
  }
  return rc;
}

#endif

RH_C_FUNCTION ON_Brep* ON_Brep_FromSphere( const ON_Sphere* pConstSphere )
{
  ON_Brep* rc = NULL;
  if( pConstSphere )
  {
    ON_Sphere* pSphere = const_cast<ON_Sphere*>(pConstSphere);
    pSphere->plane.UpdateEquation();
    rc = ON_BrepSphere(*pConstSphere);
  }
  return rc;
}