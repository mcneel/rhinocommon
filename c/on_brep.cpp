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
// ON_BrepLoop

RH_C_FUNCTION int ON_BrepLoop_FaceIndex(const ON_Brep* pConstBrep, int loop_index)
{
  int rc = -1;
  if( pConstBrep && loop_index>=0 && loop_index<pConstBrep->m_L.Count() )
    rc = pConstBrep->m_L[loop_index].m_fi;
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
//////////////////////////////////////////////////////////////////////////
// ON_Brep

RH_C_FUNCTION ON_Brep* ON_Brep_New(const ON_Brep* pOther)
{
  if( pOther )
    return ON_Brep::New(*pOther);
  return ON_Brep::New();
}

RH_C_FUNCTION bool ON_Brep_IsDuplicate(const ON_Brep* pConstBrep1, const ON_Brep* pConstBrep2, double tolerance)
{
  bool rc = false;
  if( pConstBrep1 && pConstBrep2 )
    rc = pConstBrep1->IsDuplicate(*pConstBrep2, tolerance);
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
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pBrep && count>0 && parameters )
    rc = pBrep->SplitEdgeAtParameters(edge_index, count, parameters);
#endif
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
  if( pConstBrep && ei )
  {
    if( face_index >= pConstBrep->m_F.Count() ) { return 0; }

    const ON_BrepFace* pFace = pConstBrep->Face(face_index);
    
    int loopCount = pFace->LoopCount();
    for( int i = 0; i < loopCount; i++)
    {
      const ON_BrepLoop* pLoop = pFace->Loop(i);
      int trimCount = pLoop->TrimCount();

      for( int j = 0; j < trimCount; j++)
      {
        const ON_BrepTrim* pTrim = pLoop->Trim(j);
        const ON_BrepEdge* pEdge = pTrim->Edge();

        ei->Append(pEdge->m_edge_index);
      }
    }
    rc = ei->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Brep_FaceFaceIndices( const ON_Brep* pConstBrep, int face_index, ON_SimpleArray<int>* fi )
{
  int rc = 0;
  if( pConstBrep && fi )
  {
    int faceCount = pConstBrep->m_F.Count();
    if( face_index >= faceCount ) { return 0; }

    ON_SimpleArray<bool> map(faceCount);
    for( int i = 0; i < faceCount; i++ )
    { map[i] = false; }
    map[face_index] = true;

    const ON_BrepFace* pFace = pConstBrep->Face(face_index);
    
    int loopCount = pFace->LoopCount();
    for( int i = 0; i < loopCount; i++ )
    {
      const ON_BrepLoop* pLoop = pFace->Loop(i);
      int trimCount = pLoop->TrimCount();

      for( int j = 0; j < trimCount; j++ )
      {
        const ON_BrepTrim* pTrim = pLoop->Trim(j);
        const ON_BrepEdge* pEdge = pTrim->Edge();

        int edgetrimCount = pEdge->TrimCount();
        for( int k = 0; k < edgetrimCount; k++ )
        {
          const ON_BrepTrim* peTrim = pEdge->Trim(k);
          int index = peTrim->Face()->m_face_index;
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
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
  if( pBrep )
    pBrep->StandardizeFaceSurfaces();
#endif
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

RH_C_FUNCTION ON_MassProperties* ON_GeometryMassProperties(bool bArea, ON_SimpleArray<const ON_Geometry*>* pGeometry, double relativeTolerance, double absoluteTolerance)
{
  ON_MassProperties* rc = NULL;
  if( pGeometry && pGeometry->Count() > 0 )
  {
    // Compute base-point of all geometry boundingboxes
    ON_3dPoint basePoint(0,0,0);
    if( !bArea )
    {
      for( int i = 0; i < pGeometry->Count(); i++ )
      {
        const ON_Geometry* geo = pGeometry->Array()[i];
        ON_BoundingBox box = geo->BoundingBox();
        basePoint += box.Center();
      }
      basePoint /= pGeometry->Count();
    }

    // Aggregate all mass properties
    rc = new ON_MassProperties();
    for( int i = 0; i < pGeometry->Count(); i++ )
    {
      const ON_Geometry* geo = pGeometry->Array()[i];

      bool success = false;
      ON_MassProperties mp;

      ON::object_type type = pGeometry->Array()[i]->ObjectType();

      const ON_Brep* pBrep;
      const ON_Surface* pSurface;
      const ON_Mesh* pMesh;
      const ON_Curve* pCurve;

      switch (type)
      {
      case ON::brep_object:
        pBrep = ON_Brep::Cast(geo);
        if( bArea )
          success = pBrep->AreaMassProperties(mp, true, true, true, true, relativeTolerance, absoluteTolerance);
        else
          success = pBrep->VolumeMassProperties(mp, true, true, true, true, basePoint, relativeTolerance, absoluteTolerance);
        break;

      case ON::surface_object:
        pSurface = ON_Surface::Cast(geo);
        if( bArea )
          success = pSurface->AreaMassProperties(mp, true, true, true, true, relativeTolerance, absoluteTolerance);
        else
          success = pSurface->VolumeMassProperties(mp, true, true, true, true, basePoint, relativeTolerance, absoluteTolerance);
        break;

      case ON::mesh_object:
        pMesh = ON_Mesh::Cast(geo);
        if( bArea )
          success = pMesh->AreaMassProperties(mp, true, true, true, true);
        else
          success = pMesh->VolumeMassProperties(mp, true, true, true, true, basePoint);
        break;

      case ON::curve_object:
        if (bArea)
        {
          pCurve = ON_Curve::Cast(geo);
          ON_Plane plane;
          if( pCurve->IsPlanar(&plane, absoluteTolerance) && pCurve->IsClosed() )
          {
            success = pCurve->AreaMassProperties(basePoint, plane.Normal(), mp, true, true, true, true, relativeTolerance, absoluteTolerance);
            if (success )
            {
              mp.m_mass = fabs(mp.m_mass);
            }
          }
        }
        break;
      }

      if( success )
      {
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
