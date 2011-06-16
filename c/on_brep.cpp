#include "StdAfx.h"

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

RH_C_FUNCTION bool ON_BrepEdge_IsSmoothManifoldEdge(const ON_BrepEdge* pConstBrepEdge, double angle_tol)
{
  bool rc = false;
  if( pConstBrepEdge )
    rc = pConstBrepEdge->IsSmoothManifoldEdge(angle_tol);
  return rc;
}
////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION ON_Brep* ON_Brep_New(const ON_Brep* pOther)
{
  if( pOther )
    return ON_Brep::New(*pOther);
  return ON_Brep::New();
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

RH_C_FUNCTION void ON_Brep_GetWireframe( const ON_Brep* pBrep, int density, ON_SimpleArray<ON_Curve*>* pWireframe )
{
  if (pBrep && pWireframe)
  {
    if( density < -1 ) { density = -1; }
    if( density > 99 ) { density = 99; }

    CRhinoBrepObject obj;
    obj.SetBrep(pBrep->Duplicate());

    CRhinoObjectAttributes att;
    att.m_wire_density = density;

    obj.ModifyAttributes(att, false, true);
    obj.GetWireframeCurves(*pWireframe);
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
  int rc = 0;
  if( pConstBrep )
  {
    if( idxSolidOrientation==which )
    {
      rc = pConstBrep->SolidOrientation();
    }
    else if( idxFaceCount==which )
    {
      rc = pConstBrep->m_F.Count();
    }
    else if( idxIsManifold==which )
    {
      rc = pConstBrep->IsManifold()?1:0;
    }
    else if( idxEdgeCount==which )
    {
      rc = pConstBrep->m_E.Count();
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

RH_C_FUNCTION void ON_Brep_Compact(ON_Brep* pBrep)
{
  if( pBrep )
    pBrep->Compact();
}
///////////////////////////////////////////////////////////////////////////////////////////

RH_C_FUNCTION bool ON_BrepFace_IsReversed( const ON_BrepFace* pConstFace )
{
  bool rc = false;
  if( pConstFace )
    rc = pConstFace->m_bRev;
  return rc;
}

RH_C_FUNCTION bool ON_BrepFace_ChangeSurface( ON_Brep* pBrep, int face_index, int surface_index )
{
  bool rc = false;
  if( pBrep && face_index>=0 && face_index<pBrep->m_F.Count() )
  {
    rc = pBrep->m_F[face_index].ChangeSurface(surface_index);
  }
  return rc;
}

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

RH_C_FUNCTION int ON_Brep_PointIsOnFace( const ON_Brep* pConstBrep, int faceIndex, double u, double v )
{
  int rc = 0;
  if( pConstBrep )
  {
    const TL_Brep* tl = TL_Brep::Promote(pConstBrep);
    if( tl )
    {
      ON_2dPoint pt(u,v);
      rc = tl->PointIsOnFace(faceIndex, pt);
    }
  }
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

ON_Brep* ON_Brep_ExtrudeCurve( const ON_Curve* pConstCurve, ON_3DVECTOR_STRUCT direction )
{
  ON_Brep* rc = NULL;
  
  if( pConstCurve )
  {
    const ON_3dVector* _dir = (const ON_3dVector*)&direction;
    ON_Surface* srf = RhinoExtrudeCurveStraight(pConstCurve, *_dir);
    
    if( srf )
    {
      rc = srf->BrepForm();
      if( rc )
      {
        rc->SplitKinkyFaces(); 
      }
      delete srf;
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


ON_Brep* ON_Brep_ExtrudeCurveToPoint( const ON_Curve* pConstCurve, ON_3DPOINT_STRUCT tip )
{
  ON_Brep* rc = NULL;
  
  if( pConstCurve )
  {
    const ON_3dPoint* _tip = (const ON_3dPoint*)&tip;
    ON_Surface* srf = RhinoExtrudeCurveToPoint(pConstCurve, *_tip);
    
    if( srf )
    {
      rc = srf->BrepForm();
      if( rc )
      {
        rc->SplitKinkyFaces(); 
      }
      //David: Is this correct?
      delete srf;
    }
  }
  return rc;
}

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

//////////////////////////////////////////////////////////////////////////////

#if defined(RHINO_V5SR) // only available in V5

class CRhCmnUnroll : public CRhinoUnroll
{
public:
  CRhCmnUnroll( const ON_Surface* surface, double abstol, double reltol );
  CRhCmnUnroll( const ON_Brep* brep, double abstol, double reltol );
  virtual ~CRhCmnUnroll();
  ON_Brep* m_pTempBrep;
};

CRhCmnUnroll::CRhCmnUnroll(const ON_Surface* surface, double abstol, double reltol )
: CRhinoUnroll(surface, abstol, reltol )
, m_pTempBrep(NULL)
{
}

CRhCmnUnroll::CRhCmnUnroll(const ON_Brep* brep, double abstol, double reltol)
: CRhinoUnroll(brep, abstol, reltol )
, m_pTempBrep(NULL)
{
}

CRhCmnUnroll::~CRhCmnUnroll()
{
  if( m_pTempBrep )
    delete m_pTempBrep;
}

RH_C_FUNCTION CRhCmnUnroll* CRhinoUnroll_NewSrf( const ON_Surface* pConstSurface, double absTol, double relTol )
{
  CRhCmnUnroll* rc = NULL;
  if( pConstSurface )
  {
    rc = new CRhCmnUnroll(pConstSurface, absTol, relTol);
  }
  return rc;
}

RH_C_FUNCTION CRhCmnUnroll* CRhinoUnroll_NewBrp( const ON_Brep* pConstBrep, double absTol, double relTol )
{
  CRhCmnUnroll* rc = NULL;
  if( pConstBrep )
  {
    bool create_temp = false;
    for( int i=0; i<pConstBrep->m_F.Count(); i++ )
    {
      const ON_BrepFace& face = pConstBrep->m_F[i];
      if( face.m_bRev )
      {
        create_temp = true;
        break;
      }
    }

    ON_Brep* pBrep = NULL;
    if( create_temp )
    {
      pBrep = pConstBrep->Duplicate();
      for (int i = 0; i<pBrep->m_F.Count(); i++)
      {
        ON_BrepFace& face = pBrep->m_F[i];
        if(face.m_bRev)
          face.Reverse(1);
      }
      pConstBrep = pBrep;
    }

    rc = new CRhCmnUnroll(pConstBrep, absTol, relTol);
    if( pBrep )
      rc->m_pTempBrep = pBrep;
  }
  return rc;
}

RH_C_FUNCTION void CRhinoUnroll_Delete( CRhCmnUnroll* pUnroll )
{
  if( pUnroll )
    delete pUnroll;
}

RH_C_FUNCTION int CRhinoUnroll_PrepareFaces( CRhCmnUnroll* pUnroll )
{
  int rc = -1;
  if( pUnroll )
    rc = pUnroll->PrepareFaces();
  return rc;
}

RH_C_FUNCTION void CRhinoUnroll_PrepareCurves( CRhCmnUnroll* pUnroll, ON_SimpleArray<const ON_Curve*>* pConstCurves )
{
  if( pUnroll && pConstCurves )
  {
    pUnroll->PrepareCurves(*pConstCurves);
  }
}

RH_C_FUNCTION void CRhinoUnroll_PreparePoints( CRhCmnUnroll* pUnroll, int count, /*ARRAY*/const ON_3dPoint* points)
{
  if( pUnroll && count>0 && points )
  {
    ON_3dPoint* _points = const_cast<ON_3dPoint*>(points);
    CHack3dPointArray pts(count, _points);
    pUnroll->PreparePoints(pts);
  }
}

RH_C_FUNCTION void CRhinoUnroll_PrepareDots( CRhCmnUnroll* pUnroll, ON_SimpleArray<const ON_TextDot*>* pConstDots )
{
  if( pUnroll && pConstDots )
  {
    pUnroll->PrepareDots(*pConstDots);
  }
}

class CRhCmnUnrollResults
{
public:
  ON_SimpleArray<ON_Brep*> breps;
  ON_ClassArray<ON_SimpleArray<ON_Curve*> > curves;
  ON_ClassArray<ON_SimpleArray<ON_3dPoint> > points;
  ON_ClassArray<ON_SimpleArray<ON_TextDot*> > dots;

  int CurveCount()
  {
    int rc = 0;
    for( int i=0; i<curves.Count(); i++ )
      rc += curves[i].Count();
    return rc;
  }
  int PointCount()
  {
    int rc = 0;
    for( int i=0; i<points.Count(); i++ )
      rc += points[i].Count();
    return rc;
  }
  int DotCount()
  {
    int rc = 0;
    for( int i=0; i<dots.Count(); i++ )
      rc += dots[i].Count();
    return rc;
  }
};

RH_C_FUNCTION CRhCmnUnrollResults* CRhinoUnroll_CreateFlatBreps( CRhCmnUnroll* pUnroll, double explode_dist, int* brepCount, int* curveCount, int* pointCount, int* dotCount)
{
  CRhCmnUnrollResults* rc = NULL;
  if( pUnroll && brepCount && curveCount && pointCount && dotCount )
  {
    *brepCount = 0;
    *curveCount = 0;
    *pointCount = 0;
    *dotCount = 0;
    bool explode = explode_dist>=0;
    if( !explode )
      explode_dist = 2.0;
    if( pUnroll->FlattenFaces() && pUnroll->CreateFlatBreps(explode, explode_dist)>0 )
    {
      rc = new CRhCmnUnrollResults();
      pUnroll->CollectResults( rc->breps, rc->curves, rc->points, rc->dots );
      *brepCount = rc->breps.Count();
      *curveCount = rc->CurveCount();
      *pointCount = rc->PointCount();
      *dotCount = rc->DotCount();
    }
  }
  return rc;
}

RH_C_FUNCTION void CRhinoUnrollResults_Delete(CRhCmnUnrollResults* pResults)
{
  if( pResults )
    delete pResults;
}

RH_C_FUNCTION ON_Brep* CRhinoUnrollResults_GetBrep(CRhCmnUnrollResults* pResults, int index)
{
  ON_Brep* rc = NULL;
  if( pResults && index>=0 && index<pResults->breps.Count())
  {
    rc = pResults->breps[index];
  }
  return rc;
}

RH_C_FUNCTION ON_Curve* CRhinoUnrollResults_GetCurve(CRhCmnUnrollResults* pResults, int index)
{
  ON_Curve* rc = NULL;
  if( pResults )
  {
    int count = 0;
    for( int i=0; i<pResults->curves.Count(); i++ )
    {
      if( (pResults->curves[i].Count()+count)>index )
      {
        int local_index = index-count;
        rc = pResults->curves[i][local_index];
        break;
      }
      count += pResults->curves[i].Count();
    }
  }
  return rc;
}

RH_C_FUNCTION ON_TextDot* CRhinoUnrollResults_GetDot(CRhCmnUnrollResults* pResults, int index)
{
  ON_TextDot* rc = NULL;
  if( pResults )
  {
    int count = 0;
    for( int i=0; i<pResults->dots.Count(); i++ )
    {
      if( (pResults->dots[i].Count()+count)>index )
      {
        int local_index = index-count;
        rc = pResults->dots[i][local_index];
        break;
      }
      count += pResults->dots[i].Count();
    }
  }
  return rc;
}

RH_C_FUNCTION void CRhinoUnrollResults_GetPoints( CRhCmnUnrollResults* pResults, int count, /*ARRAY*/ON_3dPoint* points)
{
  if( pResults && count>0 && count==pResults->PointCount() && points )
  {
    int offset = 0;
    for( int i=0; i<pResults->points.Count(); i++ )
    {
      int copy_count = pResults->points[i].Count();
      if( copy_count>0 )
      {
        const ON_3dPoint* source = pResults->points[i].Array();
        ON_3dPoint* dest = points+offset;
        ::memcpy(dest, source, copy_count*sizeof(ON_3dPoint));
      }
      offset+=copy_count;
    }
  }
}


RH_C_FUNCTION ON_Brep* CRhinoFitPatch_Fit1(ON_SimpleArray<const ON_Geometry*>* pGeometryArray, const ON_Surface* pConstSurface, double tolerance)
{
  ON_Brep* rc = NULL;
  if( pGeometryArray && pGeometryArray->Count()>0 && pConstSurface )
    rc = CRhinoFitPatch::Fit(*pGeometryArray, pConstSurface, tolerance);

  return rc;
}

RH_C_FUNCTION ON_Brep* CRhinoFitPatch_Fit2(ON_SimpleArray<const ON_Geometry*>* pGeometryArray, int uSpans, int vSpans, double tolerance)
{
  ON_Brep* rc = NULL;
  if( pGeometryArray )
    rc = CRhinoFitPatch::Fit(*pGeometryArray,uSpans, vSpans, tolerance);
  return rc;
}

#endif