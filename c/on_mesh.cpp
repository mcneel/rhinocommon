#include "StdAfx.h"

RH_C_FUNCTION ON_Mesh* ON_Mesh_New(const ON_Mesh* pOther)
{
  if( pOther )
    return new ON_Mesh(*pOther);
  return new ON_Mesh();
}

RH_C_FUNCTION void ON_Mesh_CopyFrom(const ON_Mesh* srcConstMesh, ON_Mesh* destMesh)
{
  if( srcConstMesh && destMesh )
  {
    *destMesh = *srcConstMesh;
  }
}

RH_C_FUNCTION bool ON_Mesh_HasSurfaceParameters(const ON_Mesh* pConstMesh)
{
  bool rc = false;
  if( pConstMesh )
    rc = pConstMesh->HasSurfaceParameters();
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_EvaluateMeshGeometry(ON_Mesh* pMesh, const ON_Surface* pConstSurface)
{
  bool rc = false;
  if( pMesh && pConstSurface )
    rc = pMesh->EvaluateMeshGeometry(*pConstSurface);
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetVertex(ON_Mesh* pMesh, int vertexIndex, float x, float y, float z)
{
  bool rc = false;
  if( pMesh )
  {
    rc = pMesh->SetVertex(vertexIndex, ON_3fPoint(x,y,z));
    pMesh->DestroyRuntimeCache();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetFace(ON_Mesh* pMesh, int faceIndex, int vertex1, int vertex2, int vertex3, int vertex4)
{
  bool rc = false;
  if( pMesh )
  {
    rc = pMesh->SetQuad(faceIndex, vertex1, vertex2, vertex3, vertex4);
    pMesh->DestroyRuntimeCache();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetTextureCoordinate(ON_Mesh* pMesh, int index, float s, float t)
{
  bool rc = false;
  if( pMesh )
  {
    //David: Really? Casting to doubles first then back to floats in SetTextureCoord? Seems roundabout...
    rc = pMesh->SetTextureCoord(index, (double)s, (double)t);
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_AddFace(ON_Mesh* pMesh, int vertex1, int vertex2, int vertex3, int vertex4)
{
  int rc = -1;
  if( pMesh )
  {
    int faceIndex = pMesh->m_F.Count();
    if( pMesh->SetQuad(faceIndex, vertex1, vertex2, vertex3, vertex4) )
      rc = faceIndex;
    pMesh->DestroyRuntimeCache();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_InsertFace(ON_Mesh* pMesh, int index, int vertex1, int vertex2, int vertex3, int vertex4)
{
  bool rc = false;
  if( pMesh && index>=0 && index<pMesh->m_F.Count() )
  {
    ON_MeshFace face;
    face.vi[0] = vertex1;
    face.vi[1] = vertex2;
    face.vi[2] = vertex3;
    face.vi[3] = vertex4;
    pMesh->m_F.Insert(index, face);
    rc = true;
    pMesh->DestroyRuntimeCache();
  }
  return rc;
}

//RH_C_FUNCTION bool ON_Mesh_SetVertices(ON_Mesh* ptr, int count, ON_3fPoint* locations, bool append)
//{
//  bool rc = false;
//  if( ptr && count>0 && locations )
//  {
//    int startIndex = 0;
//    if( append )
//      int startIndex = ptr->m_V.Count();
//    
//    ptr->m_V.SetCapacity(startIndex + count);
//    ON_3fPoint* dest = ptr->m_V.Array() + startIndex;
//    ::memcpy(dest, locations, count*sizeof(ON_3fPoint));
//    ptr->m_V.SetCount(startIndex+count);
//
//    rc = true;
//    ptr->InvalidateBoundingBoxes();
//    ptr->DestroyTopology();
//  }
//  return rc;
//}

RH_C_FUNCTION bool ON_Mesh_SetNormal(ON_Mesh* pMesh, int index, ON_3FVECTOR_STRUCT vector, bool faceNormal)
{
  // if index == Count, then we are appending
  bool rc = false;
  if( pMesh && index>=0 )
  {
    const ON_3fVector* _vector = (const ON_3fVector*)&vector;
    ON_3fVectorArray* list = faceNormal ? &(pMesh->m_FN) : &(pMesh->m_N);

    if( index < list->Count() )
    {
      (*list)[index] = *_vector;
    }
    else if( index == list->Count() )
    {
      list->Append(*_vector);
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetColor(ON_Mesh* pMesh, int index, int argb)
{
  // if index == Count, then we are appending
  bool rc = false;
  if( pMesh && index>=0 )
  {
    unsigned int abgr = ARGB_to_ABGR(argb);
    ON_Color color = abgr;
    if( index < pMesh->m_C.Count() )
    {
      pMesh->m_C[index] = color;
    }
    else if( index == pMesh->m_C.Count() )
    {
      pMesh->m_C.Append(color);
    }
    memset(&(pMesh->m_Ctag),0,sizeof(pMesh->m_Ctag));
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetNormals(ON_Mesh* ptr, int count, /*ARRAY*/const ON_3fVector* normals, bool append)
{
  bool rc = false;
  if( ptr && count>0 && normals )
  {
    int startIndex = 0;
    if( append )
      int startIndex = ptr->m_N.Count();
    
    ptr->m_N.SetCapacity(startIndex + count);
    ON_3fVector* dest = ptr->m_N.Array() + startIndex;
    ::memcpy(dest, normals, count*sizeof(ON_3fVector));
    ptr->m_N.SetCount(startIndex+count);

    rc = true;
    ptr->InvalidateBoundingBoxes();
    ptr->DestroyTopology();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_SetTextureCoordinates(ON_Mesh* ptr, int count, /*ARRAY*/const ON_2fPoint* tcs, bool append)
{
  bool rc = false;
  if( ptr && count>0 && tcs )
  {
    int startIndex = 0;
    if( append )
      int startIndex = ptr->m_T.Count();
    
    ptr->m_T.SetCapacity(startIndex + count);
    ON_2fPoint* dest = ptr->m_T.Array() + startIndex;
    ::memcpy(dest, tcs, count*sizeof(ON_2fPoint));
    ptr->m_T.SetCount(startIndex+count);

    rc = true;
    ptr->InvalidateBoundingBoxes();
    ptr->DestroyTopology();
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_GetMappingTag(const ON_Mesh* pConstMesh, int which_tag, ON_UUID* id, int* mapping_type, unsigned int* crc, ON_Xform* xf)
{
  if( pConstMesh && id && mapping_type && crc && xf )
  {
    if( 0==which_tag )
    {
      *id = pConstMesh->m_Ctag.m_mapping_id;
      *mapping_type = (int)pConstMesh->m_Ctag.m_mapping_type;
      *crc = pConstMesh->m_Ctag.m_mapping_crc;
      *xf = pConstMesh->m_Ctag.m_mesh_xform;
    }
  }
}

RH_C_FUNCTION void ON_Mesh_SetMappingTag(ON_Mesh* pMesh, int which_tag, ON_UUID id, int mapping_type, unsigned int crc, const ON_Xform* xf)
{
  if( pMesh && xf )
  {
    if( 0==which_tag )
    {
      pMesh->m_Ctag.m_mapping_id = id;
      pMesh->m_Ctag.m_mapping_type = (ON_TextureMapping::TYPE)mapping_type;
      pMesh->m_Ctag.m_mapping_crc = crc;
      pMesh->m_Ctag.m_mesh_xform = *xf;
    }
  }
}


RH_C_FUNCTION bool ON_Mesh_SetVertexColors(ON_Mesh* pMesh, int count, /*ARRAY*/const int* argb, bool append)
{
  bool rc = false;
  if( pMesh && count>0 && argb )
  {
    unsigned int* list = (unsigned int*)argb;
    for( int i=0; i<count; i++ )
      list[i] = ARGB_to_ABGR(list[i]);

    int startIndex = 0;
    if( append )
      int startIndex = pMesh->m_C.Count();
    
    pMesh->m_C.SetCapacity(startIndex + count);
    ON_Color* dest = pMesh->m_C.Array() + startIndex;
    ::memcpy(dest, list, count*sizeof(unsigned int));
    pMesh->m_C.SetCount(startIndex+count);
    memset(&(pMesh->m_Ctag),0,sizeof(pMesh->m_Ctag));
    rc = true;
  }
  return rc;
}

//RH_C_FUNCTION bool ON_Mesh_SetFaces(ON_Mesh* ptr, int count, int* vertices, bool quads, bool append)
//{
//  bool rc = false;
//  if( ptr && count>2 && vertices )
//  {
//    if( !append )
//      ptr->m_F.SetCount(0);
//    int faceid = ptr->m_F.Count();
//    int start = faceid;
//    if( quads )
//    {
//      int end = count/4 + start;
//      for( int i=start; i<end; i++ )
//      {
//        int index0 = *vertices++;
//        int index1 = *vertices++;
//        int index2 = *vertices++;
//        int index3 = *vertices++;
//        ptr->SetQuad(i, index0, index1, index2, index3);
//      }
//    }
//    else
//    {
//      int end = count/3 + start;
//      for( int i=start; i<end; i++ )
//      {
//        int index0 = *vertices++;
//        int index1 = *vertices++;
//        int index2 = *vertices++;
//        ptr->SetTriangle(i, index0, index1, index2);
//      }
//    }
//
//    rc = true;
//    ptr->InvalidateBoundingBoxes();
//    ptr->DestroyTopology();
//  }
//  return rc;
//}
RH_C_FUNCTION void ON_Mesh_SetInt( ON_Mesh* pMesh, int which, int value )
{
  const int idxVertexCount = 0;
  const int idxFaceCount = 1;
  //const int idxQuadCount = 2;
  //const int idxTriangleCount = 3;
  const int idxHiddenVertexCount = 4;
  //const int idxDisjointMeshCount = 5;
  const int idxFaceNormalCount = 6;
  const int idxNormalCount = 7;
  const int idxColorCount = 8;
  const int idxTextureCoordinateCount = 9;
  if( pMesh )
  {
    switch(which)
    {
    case idxVertexCount:
      pMesh->m_V.SetCount(value);
      break;
    case idxFaceCount:
      pMesh->m_F.SetCount(value);
      break;
    case idxHiddenVertexCount:
      pMesh->m_H.SetCount(value);
      break;
    case idxFaceNormalCount:
      pMesh->m_FN.SetCount(value);
      break;
    case idxNormalCount:
      pMesh->m_N.SetCount(value);
      break;
    case idxColorCount:
      pMesh->m_C.SetCount(value);
      break;
    case idxTextureCoordinateCount:
      pMesh->m_T.SetCount(value);
      break;
    }
  }
}

RH_C_FUNCTION int ON_Mesh_GetInt( const ON_Mesh* pConstMesh, int which )
{
  const int idxVertexCount = 0;
  const int idxFaceCount = 1;
  const int idxQuadCount = 2;
  const int idxTriangleCount = 3;
  const int idxHiddenVertexCount = 4;
  const int idxDisjointMeshCount = 5;
  const int idxFaceNormalCount = 6;
  const int idxNormalCount = 7;
  const int idxColorCount = 8;
  const int idxTextureCoordinateCount = 9;
  const int idxMeshTopologyVertexCount = 10;
  const int idxSolidOrientation = 11;
  const int idxMeshTopologyEdgeCount = 12;

  int rc = -1;
  if( pConstMesh )
  {
    switch( which )
    {
    case idxVertexCount:
      rc = pConstMesh->VertexCount();
      break;
    case idxFaceCount:
      rc = pConstMesh->FaceCount();
      break;
    case idxQuadCount:
      rc = pConstMesh->QuadCount();
      break;
    case idxTriangleCount:
      rc = pConstMesh->TriangleCount();
      break;
    case idxHiddenVertexCount:
      rc = pConstMesh->HiddenVertexCount();
      break;
    case idxDisjointMeshCount:
#if !defined(OPENNURBS_BUILD)
      {
        ON_SimpleArray<ON_Mesh*> meshes;
        rc = RhinoSplitDisjointMesh( pConstMesh, meshes, true );
      }
#endif
      break;
    case idxFaceNormalCount:
      rc = pConstMesh->m_FN.Count();
      break;
    case  idxNormalCount:
      rc = pConstMesh->m_N.Count();
      break;
    case idxColorCount:
      rc = pConstMesh->m_C.Count();
      break;
    case idxTextureCoordinateCount:
      rc = pConstMesh->m_T.Count();
      break;
    case idxMeshTopologyVertexCount:
      {
        const ON_MeshTopology& top = pConstMesh->Topology();
        rc = top.m_topv.Count();
        break;
      }
    case idxSolidOrientation:
      {
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)// only available in V5
        rc = pConstMesh->SolidOrientation();
#endif
      }
      break;
    case idxMeshTopologyEdgeCount:
      {
        const ON_MeshTopology& top = pConstMesh->Topology();
        rc = top.m_tope.Count();
        break;
      }
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetBool( const ON_Mesh* pMesh, int which )
{
  const int idxHasVertexNormals = 0;
  const int idxHasFaceNormals = 1;
  const int idxHasTextureCoordinates = 2;
  const int idxHasSurfaceParameters = 3;
  const int idxHasPrincipalCurvatures = 4;
  const int idxHasVertexColors = 5;
  const int idxIsClosed = 6;

  bool rc = false;
  if( pMesh )
  {
    switch( which )
    {
    case idxHasVertexNormals:
      rc = pMesh->HasVertexNormals();
      break;
    case idxHasFaceNormals:
      rc = pMesh->HasFaceNormals();
      break;
    case idxHasTextureCoordinates:
      rc = pMesh->HasTextureCoordinates();
      break;
    case idxHasSurfaceParameters:
      rc = pMesh->HasSurfaceParameters();
      break;
    case idxHasPrincipalCurvatures:
      rc = pMesh->HasPrincipalCurvatures();
      break;
    case idxHasVertexColors:
      rc = pMesh->HasVertexColors();
      break;
    case idxIsClosed:
      rc = pMesh->IsClosed();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_Flip(ON_Mesh* ptr, bool vertNorm, bool faceNorm, bool faceOrientation)
{
  if( ptr )
  {
    if( faceOrientation )
      ptr->FlipFaceOrientation();
    if( faceNorm )
      ptr->FlipFaceNormals();
    if( vertNorm )
      ptr->FlipVertexNormals();
  }
}

RH_C_FUNCTION bool ON_Mesh_NonConstBoolOp(ON_Mesh* ptr, int which)
{
  const int idxUnitizeVertexNormals = 0;
  const int idxUnitizeFaceNormals = 1;
  const int idxConvertQuadsToTriangles = 2;
  const int idxComputeFaceNormals = 3;
  const int idxCompact = 4;
  const int idxComputeVertexNormals = 5;
  const int idxNormalizeTextureCoordinates = 6;
  const int idxTransposeTextureCoordinates = 7;
  const int idxTransposeSurfaceParameters = 8;

  bool rc = false;
  if( ptr )
  {
    switch(which)
    {
    case idxUnitizeVertexNormals:
      rc = ptr->UnitizeVertexNormals();
      break;
    case idxUnitizeFaceNormals:
      rc = ptr->UnitizeFaceNormals();
      break;
    case idxConvertQuadsToTriangles:
      rc = ptr->ConvertQuadsToTriangles();
      break;
    case idxComputeFaceNormals:
      rc = ptr->ComputeFaceNormals();
      break;
    case idxCompact:
      rc = ptr->Compact();
      break;
    case idxComputeVertexNormals:
      rc = ptr->ComputeVertexNormals();
      break;
    case idxNormalizeTextureCoordinates:
      rc = ptr->NormalizeTextureCoordinates();
      break;
    case idxTransposeTextureCoordinates:
      rc = ptr->TransposeTextureCoordinates();
      break;
    case idxTransposeSurfaceParameters:
      rc = ptr->TransposeSurfaceParameters();
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_ConvertTrianglesToQuads(ON_Mesh* ptr, double angle_tol, double min_diag_ratio)
{
  bool rc = false;
  if( ptr )
  {
    rc = ptr->ConvertTrianglesToQuads(angle_tol, min_diag_ratio);
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_CullOp(ON_Mesh* ptr, bool faces)
{
  int rc = -1;
  if( ptr )
  {
    if( faces )
      rc = ptr->CullDegenerateFaces();
    else
      rc = ptr->CullUnusedVertices();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_Reverse(ON_Mesh* ptr, bool texturecoords, int direction)
{
  bool rc = false;
  if( ptr )
  {
    if( texturecoords )
      rc = ptr->ReverseTextureCoordinates(direction);
    else
      rc = ptr->ReverseSurfaceParameters(direction);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_CombineIdenticalVertices(ON_Mesh* ptr, bool ignore_normals, bool ignore_tcs)
{
  bool rc = false;
  if( ptr )
  {
    rc = ptr->CombineIdenticalVertices(ignore_normals, ignore_tcs);
    if( rc && ptr->VertexCount() != ptr->m_S.Count() )
      ptr->m_S.SetCount(0);
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_Append(ON_Mesh* ptr, const ON_Mesh* other)
{
  if( ptr && other )
    ptr->Append(*other);
}

RH_C_FUNCTION bool ON_Mesh_IsManifold(const ON_Mesh* ptr, bool topotest, bool* isOriented, bool* hasBoundary)
{
  bool rc = false;
  if( ptr )
  {
    rc = ptr->IsManifold(topotest, isOriented, hasBoundary);
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_DeleteFace(ON_Mesh* pMesh, int count, /*ARRAY*/const int* indices)
{
  int rc = 0;
  if( pMesh && count>0 && indices )
  {
    // 6 March 2010 S. Baer
    // Faces need to be deleted in reverse index order so the lower ids are still
    // valid after deleting the higher ids
    ON_SimpleArray<int> sorted_indices(count);
    sorted_indices.Append(count, indices);
    sorted_indices.QuickSort(ON_CompareDecreasing<int>);

    // in case duplicates are in the list
    int prev_index = -1;

    for( int i=0; i<count; i++ )
    {
      int index = sorted_indices[i];
      if( index == prev_index )
        continue;
      prev_index = index;
      if( pMesh->DeleteFace( index ) )
        rc++;
    }

    // 6 March 2010 - S. Baer
    // Invalidate the cached IsClosed state. This forces the closed state
    // to be recomputed the next time IsClosed is called.
    pMesh->SetClosed(-1);

    // Compact is a pretty heavy handed function and should be called sparingly
    // Maybe it is okay to use this call in this function since the RhinoCommon
    // SDK is forcing the user to pass an array of faces to delete instead of a
    // single face.
    pMesh->Compact();
    pMesh->DestroyTopology();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_Vertex(const ON_Mesh* ptr, int index, ON_3fPoint* pt)
{
  bool rc = false;
  if( ptr && pt && index>=0 )
  {
    const ON_3fPoint* vert = ptr->m_V.At(index);
    if( vert )
    {
      *pt = *vert;
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetNormal(const ON_Mesh* pConstMesh, int index, ON_3fVector* vector, bool faceNormal)
{
  bool rc = false;
  if( pConstMesh && vector && index>=0 )
  {
    const ON_3fVector* vec = NULL;
    if( faceNormal )
      vec = pConstMesh->m_FN.At(index);
    else
      vec = pConstMesh->m_N.At(index);

    if( vec )
    {
      *vector = *vec;
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetColor(const ON_Mesh* pConstMesh, int index, int* abgr)
{
  bool rc = false;
  if( pConstMesh && abgr && index>=0 && index<pConstMesh->m_C.Count() )
  {
    unsigned int c = (unsigned int)(pConstMesh->m_C[index]);
    *abgr = (int)c;
    rc = true;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetFace(const ON_Mesh* pConstMesh, int face_index, ON_MeshFace* face)
{
  bool rc = false;
  if( pConstMesh && face && face_index>=0 && face_index<pConstMesh->m_F.Count() )
  {
    *face = pConstMesh->m_F[face_index];
    rc = true;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetFaceVertices(const ON_Mesh* pConstMesh,
                                           int face_index,
                                           ON_3fPoint* p0,
                                           ON_3fPoint* p1,
                                           ON_3fPoint* p2,
                                           ON_3fPoint* p3)
{
  bool rc = false;
  if( pConstMesh && face_index>=0 && face_index<pConstMesh->m_F.Count() && p0 && p1 && p2 && p3 )
  {
    const ON_MeshFace& face = pConstMesh->m_F[face_index];
    *p0 = pConstMesh->m_V[face.vi[0]];
    *p1 = pConstMesh->m_V[face.vi[1]];
    *p2 = pConstMesh->m_V[face.vi[2]];
    *p3 = pConstMesh->m_V[face.vi[3]];
    rc = true;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_GetTextureCoordinate(const ON_Mesh* pConstMesh, int index, float* s, float* t)
{
  bool rc = false;
  if( pConstMesh && index>=0 && index<pConstMesh->m_T.Count() && s && t )
  {
    ON_2fPoint tc = pConstMesh->m_T[index];
    *s = tc.x;
    *t = tc.y;
    rc = true;
  }
  return rc;
}

// !!!!IMPORTANT!!!! Use an array of ints instead of bools. Bools have to be marshaled
// in different ways through .NET which can cause all sorts of problems.
RH_C_FUNCTION bool ON_Mesh_NakedEdgePoints( const ON_Mesh* pMesh, /*ARRAY*/int* naked_status, int count )
{
  bool rc = false;
  // taken from RhinoScript implementation of the same function
  if( pMesh && naked_status && count==pMesh->VertexCount() )
  {
    const ON_MeshTopology& top = pMesh->Topology();
    if( top.TopEdgeCount() > 0 )
    {
      for( int b=0; b<top.m_tope.Count(); b++ )
      {
        const ON_MeshTopologyEdge& tope = top.m_tope[b];
        for( int c=0; c<2; c++ )
        {
          const ON_MeshTopologyVertex& topv = top.m_topv[ tope.m_topvi[c] ];
          if( tope.m_topf_count == 1 || topv.m_v_count > 1 )
          {
            for( int d=0; d<topv.m_v_count; d++ )
            {
              int vi = topv.m_vi[d];
              naked_status[vi] = 1;
            }
          }
        }
      }
      rc = true;
    }        
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_IsPointInside(const ON_Mesh* pConstMesh, ON_3DPOINT_STRUCT point, double tolerance, bool strictlyin)
{
  bool rc = false;
#if defined(RHINO_V5SR) // only available in V5
  if( pConstMesh )
  {
    ON_3dPoint _point(point.val);
    rc = pConstMesh->IsPointInside(_point, tolerance, strictlyin);
  }
#endif
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_IndexOpBool(ON_Mesh* pMesh, int which, int index)
{
  const int idxCollapseEdge=0;
  const int idxIsSwappableEdge=1;
  const int idxSwapEdge=2;
  bool rc = false;
  if( pMesh )
  {
    switch(which)
    {
    case idxCollapseEdge:
      rc = pMesh->CollapseEdge(index);
      break;
    case idxIsSwappableEdge:
      rc = pMesh->IsSwappableEdge(index);
      break;
    case idxSwapEdge:
      rc = pMesh->SwapEdge(index);
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_FaceIsHidden(const ON_Mesh* pConstMesh, int index)
{
  bool rc = false;
  if( pConstMesh )
    rc = pConstMesh->FaceIsHidden(index);
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_FaceHasNakedEdges(const ON_Mesh* pConstMesh, int index)
{
  bool rc = false;
  if( pConstMesh )
  {
    const ON_MeshTopology& topology = pConstMesh->Topology();
    const ON_MeshTopologyFace* face_top = topology.m_topf.At(index);
    if( face_top )
    {
      for( int i=0; i<4; i++ )
      {
        int edge = face_top->m_topei[i];
        if( topology.m_tope[edge].m_topf_count == 1 )
        {
          rc = true;
          break;
        }
      }
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_FaceTopologicalVertices(const ON_Mesh* pConstMesh, int index, /*ARRAY*/int* verts)
{
  bool rc = false;
  if( pConstMesh && verts )
  {
    const ON_MeshTopology& topology = pConstMesh->Topology();
    rc = topology.GetTopFaceVertices(index, verts);
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_ClearList( ON_Mesh* pMesh, int which )
{
  const int idxClearVertices = 0;
  const int idxClearFaces = 1;
  const int idxClearNormals = 2;
  const int idxClearFaceNormals = 3;
  const int idxClearColors = 4;
  const int idxClearTextureCoordinates = 5;
  const int idxClearHiddenVertices = 6;

  if( pMesh )
  {
    if( idxClearVertices == which )
      pMesh->m_V.SetCount(0);
    else if( idxClearFaces == which )
      pMesh->m_F.SetCount(0);
    else if( idxClearNormals == which )
      pMesh->m_N.SetCount(0);
    else if( idxClearFaceNormals == which )
      pMesh->m_FN.SetCount(0);
    else if( idxClearColors == which )
      pMesh->m_C.SetCount(0);
    else if( idxClearTextureCoordinates == which )
      pMesh->m_T.SetCount(0);
    else if( idxClearHiddenVertices == which )
      pMesh->m_H.SetCount(0);
  }
}
RH_C_FUNCTION bool ON_Mesh_GetHiddenValue(const ON_Mesh* pConstMesh, int index)
{
  bool rc = false;
  if( pConstMesh && index>=0 && index<pConstMesh->m_H.Count())
  {
    rc = pConstMesh->m_H[index];
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_HiddenVertexOp( ON_Mesh* pMesh, int index, int op)
{
  if( !pMesh )
    return;

  const int idxHideVertex = 0;
  const int idxShowVertex = 1;
  const int idxHideAll = 2;
  const int idxShowAll = 3;
  const int idxEnsureHiddenList = 4;
  const int idxCleanHiddenList = 5;

  if( idxHideVertex == op || idxShowVertex == op )
  {
    bool show = (idxShowVertex==op);
    // Show or Hide the vertex at [index].
    if( index>=0 && index<pMesh->m_H.Count() )
      pMesh->m_H[index] = show;
  }
  else if( idxHideAll == op || idxShowAll == op )
  {
    // Set all the values in m_H to true or false.
    bool hide = (idxHideAll==op);
    int count = pMesh->m_H.Count();
    for( int i = 0; i < count; i++)
    {
      pMesh->m_H[i] = hide;
    }
  }
  else if( idxEnsureHiddenList == op)
  {
    // Make sure the m_H array contains the same amount of entries as the m_V array. 
    // This function leaves the contents of m_H untouched, so they will be garbage when 
    // the function grew the m_H array.
    int count = pMesh->m_V.Count();
    if( pMesh->m_H.Count() != count )
    { 
      pMesh->m_H.SetCapacity( count );
      pMesh->m_H.SetCount( count );
    }
  }
  else if( idxCleanHiddenList == op)
  {
    // If the m_H array contains only false values, erase it.
    int count = pMesh->m_H.Count();
    if( count > 0 )
    {
      bool clean = true;
      for( int i = 0; i < count; i++ )
      {
        if( pMesh->m_H[i] )
        {
          clean = false;
          break;
        }
      }

      if( clean )
        pMesh->m_H.SetCount(0);
    }
  }
}

RH_C_FUNCTION void ON_Mesh_RepairHiddenArray( ON_Mesh* pMesh )
{
  if( !pMesh )
    return;

  int v_count = pMesh->m_V.Count();
  int h_count = pMesh->m_H.Count();

  // No hidden flags equals a valid mesh.
  // An equal amount of vertices and hidden flags equal a valid mesh.
  if( 0==h_count || v_count==h_count )
    return;

  if( h_count > v_count )
  {
    // Remove the trailing hidden flags.
    pMesh->m_H.SetCount(v_count);
  }
  else
  {
    // Add new hidden flags to account for unhandled vertices.
    int count_to_add = v_count - h_count;
    pMesh->m_H.SetCapacity(v_count);
    for( int i=0; i<count_to_add; i++)
    {
      pMesh->m_H.Append(false);
    }
  }
}

RH_C_FUNCTION int ON_Mesh_GetVertexFaces( const ON_Mesh* pMesh, ON_SimpleArray<int>* face_indices, int vertex_index )
{
  int rc = 0;
  if( pMesh && face_indices && vertex_index>=0 && vertex_index<pMesh->m_V.Count())
  {
    const ON_SimpleArray<ON_MeshFace>& faces = pMesh->m_F;
    int count = faces.Count();
    for( int i=0; i<count; i++ )
    {
      const ON_MeshFace& face = faces[i];
      if( face.vi[0] == vertex_index || face.vi[1] == vertex_index ||
          face.vi[2] == vertex_index || face.vi[3] == vertex_index )
      {
        face_indices->Append(i);
      }
    }
    rc = face_indices->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_GetTopologicalVertices( const ON_Mesh* pMesh, ON_SimpleArray<int>* vertex_indices, int vertex_index )
{
  int rc = 0;
  if( pMesh && vertex_indices && vertex_index>=0 && vertex_index<pMesh->m_V.Count())
  {
    const ON_MeshTopology& top = pMesh->Topology();
    if( top.m_topv_map.Count()>=vertex_index )
    {
      int top_vertex_index = top.m_topv_map[vertex_index];
      if( top_vertex_index>=0 && top_vertex_index<=top.m_topv.Count() )
      {
        ON_MeshTopologyVertex v = top.m_topv[top_vertex_index];
        if( v.m_v_count > 1 )
        {
          vertex_indices->Append(v.m_v_count, v.m_vi);
        }
      }
    }
    rc = vertex_indices->Count();
  }
  return rc;
}

static void TestAndAppend( ON_SimpleArray<int>& indices, int index )
{
  int count = indices.Count();
  for( int i=0; i<count; i++ )
  {
    if( indices[i]==index )
      return;
  }
  indices.Append(index);
}

RH_C_FUNCTION int ON_Mesh_GetConnectedVertices( const ON_Mesh* pMesh, ON_SimpleArray<int>* vertex_indices, int vertex_index )
{
  // I think we can use
  // int ON_Mesh::GetVertexEdges
  // to speed this up


  int rc = 0;
  if( pMesh && vertex_indices && vertex_index>=0 && vertex_index<pMesh->m_V.Count())
  {
    // typically the number of edges connected to this vertex is very small < 10
    int count = pMesh->m_F.Count();
    for( int i=0; i<count; i++ )
    {
      const ON_MeshFace& face = pMesh->m_F[i];
      if( face.vi[0] == vertex_index )
      {
        TestAndAppend(*vertex_indices, face.vi[1]);
        if( face.vi[0]==face.vi[3] )
          TestAndAppend(*vertex_indices, face.vi[2]);
        else
          TestAndAppend(*vertex_indices, face.vi[3]);
      }
      if( face.vi[1] == vertex_index )
      {
        TestAndAppend(*vertex_indices, face.vi[0]);
        TestAndAppend(*vertex_indices, face.vi[2]);
      }
      if( face.vi[2] == vertex_index )
      {
        TestAndAppend(*vertex_indices, face.vi[1]);
        TestAndAppend(*vertex_indices, face.vi[3]);
      }
      if( face.vi[3]!=face.vi[0] && face.vi[3]==vertex_index )
      {
        TestAndAppend(*vertex_indices, face.vi[2]);
        TestAndAppend(*vertex_indices, face.vi[0]);
      }
    }
    rc = vertex_indices->Count();
  }
  return rc;
}

/////////////////////////////////////////////////////////////////////
/////////////////////////////////////////////////////////////////////

RH_C_FUNCTION bool ON_MeshTopologyEdge_TopVi(const ON_Mesh* pConstMesh, int edgeindex, int* v0, int* v1)
{
  bool rc = false;
  if( pConstMesh && v0 && v1 && edgeindex>=0)
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( edgeindex < top.m_tope.Count() )
    {
      const ON_MeshTopologyEdge& edge = top.m_tope[edgeindex];
      *v0 = edge.m_topvi[0];
      *v1 = edge.m_topvi[1];
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_MeshTopologyEdge_TopfCount(const ON_Mesh* pConstMesh, int edgeindex)
{
  int rc = 0;
  if( pConstMesh && edgeindex>=0 )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( edgeindex < top.m_tope.Count() )
      rc = top.m_tope[edgeindex].m_topf_count;
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshTopologyEdge_TopfList2(const ON_Mesh* pConstMesh, int edgeindex, int count, /*ARRAY*/int* faces, /*ARRAY*/bool* directionsMatch)
{
  if( pConstMesh && edgeindex>=0 && faces )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( edgeindex < top.m_tope.Count() )
    {
      const ON_MeshTopologyEdge& edge = top.m_tope[edgeindex];
      if( count==edge.m_topf_count )
      {
        memcpy(faces, edge.m_topfi, count*sizeof(int));
        if( directionsMatch )
        {
          for( int i=0; i<count; i++ )
          {
            const ON_MeshTopologyFace& face = top.m_topf[faces[i]];
            directionsMatch[i] = false;
            for( int j=0; j<4; j++)
            {
              if( face.m_topei[j]==edgeindex )
                directionsMatch[i] = (face.m_reve[j]==0);
            }
          }
        }
      }
    }
  }
}

RH_C_FUNCTION void ON_MeshTopologyEdge_TopfList(const ON_Mesh* pConstMesh, int edgeindex, int count, /*ARRAY*/int* faces)
{
  return ON_MeshTopologyEdge_TopfList2(pConstMesh, edgeindex , count , faces, NULL);
}

RH_C_FUNCTION void ON_MeshTopology_TopEdgeLine(const ON_Mesh* pConstMesh, int edge_index, ON_Line* line)
{
  if( pConstMesh && line )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    *line = top.TopEdgeLine(edge_index);
  }
}

RH_C_FUNCTION int ON_MeshTopology_TopEdge(const ON_Mesh* pConstMesh, int vert1, int vert2)
{
  int rc = -1;
  if( pConstMesh )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    rc = top.TopEdge(vert1, vert2);
  }
  return rc;
}

RH_C_FUNCTION bool ON_MeshTopology_GetTopFaceVertices(const ON_Mesh* pConstMesh, int index, int* a, int* b, int* c, int* d)
{
  bool rc = false;
  if( pConstMesh )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    int v[4];
    rc = top.GetTopFaceVertices(index, v);
    if( rc )
    {
      *a = v[0];
      *b = v[1];
      *c = v[2];
      *d = v[3];
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_MeshTopology_TopItemIsHidden(const ON_Mesh* pConstMesh, int which, int index)
{
  const int idxTopVertexIsHidden = 0;
  const int idxTopEdgeIsHidden = 1;
  const int idxTopFaceIsHidden = 2;
  bool rc = false;
  if( pConstMesh )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    switch(which)
    {
    case idxTopVertexIsHidden:
      rc = top.TopVertexIsHidden(index);
      break;
    case idxTopEdgeIsHidden:
      rc = top.TopEdgeIsHidden(index);
      break;
    case idxTopFaceIsHidden:
      rc = top.TopFaceIsHidden(index);
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION ON_MeshParameters* ON_MeshParameters_New()
{
  return new ON_MeshParameters();
}

RH_C_FUNCTION void ON_MeshParameters_Delete(ON_MeshParameters* pMeshParameters)
{
  if( pMeshParameters )
    delete pMeshParameters;
}

RH_C_FUNCTION bool ON_MeshParameters_GetBool(const ON_MeshParameters* pConstMeshParameters, int which)
{
  const int idxJaggedSeams = 0;
  const int idxRefineGrid = 1;
  const int idxSimplePlanes = 2;
  const int idxComputeCurvature = 3;
  bool rc = false;
  if( pConstMeshParameters )
  {
    switch(which)
    {
    case idxJaggedSeams:
      rc = pConstMeshParameters->m_bJaggedSeams;
      break;
    case idxRefineGrid:
      rc = pConstMeshParameters->m_bRefine;
      break;
    case idxSimplePlanes:
      rc = pConstMeshParameters->m_bSimplePlanes;
      break;
    case idxComputeCurvature:
      rc = pConstMeshParameters->m_bComputeCurvature;
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshParameters_SetBool(ON_MeshParameters* pMeshParameters, int which, bool val)
{
  const int idxJaggedSeams = 0;
  const int idxRefineGrid = 1;
  const int idxSimplePlanes = 2;
  const int idxComputeCurvature = 3;
  if( pMeshParameters )
  {
    switch(which)
    {
    case idxJaggedSeams:
      pMeshParameters->m_bJaggedSeams = val;
      break;
    case idxRefineGrid:
      pMeshParameters->m_bRefine = val;
      break;
    case idxSimplePlanes:
      pMeshParameters->m_bSimplePlanes = val;
      break;
    case idxComputeCurvature:
      pMeshParameters->m_bComputeCurvature = val;
      break;
    default:
      break;
    }
  }
}

RH_C_FUNCTION double ON_MeshParameters_GetDouble(const ON_MeshParameters* pConstMeshParameters, int which)
{
  const int idxGridAngle = 0;
  const int idxGridAspectRatio = 1;
  const int idxGridAmplification = 2;
  const int idxTolerance = 3;
  const int idxMinimumTolerance = 4;
  const int idxRelativeTolerance = 5;
  const int idxMinimumEdgeLength = 6;
  const int idxMaximumEdgeLength = 7;
  const int idxRefineAngle = 8;
  double rc = 0;
  if( pConstMeshParameters )
  {
    switch(which)
    {
    case idxGridAngle: //0
      rc = pConstMeshParameters->m_grid_angle;
      break;
    case idxGridAspectRatio: //1
      rc = pConstMeshParameters->m_grid_aspect_ratio;
      break;
    case idxGridAmplification: //2
      rc = pConstMeshParameters->m_grid_amplification;
      break;
    case idxTolerance: //3
      rc = pConstMeshParameters->m_tolerance;
      break;
    case idxMinimumTolerance: //4
      rc = pConstMeshParameters->m_min_tolerance;
      break;
    case idxRelativeTolerance: //5
      rc = pConstMeshParameters->m_relative_tolerance;
      break;
    case idxMinimumEdgeLength: //6
      rc = pConstMeshParameters->m_min_edge_length;
      break;
    case idxMaximumEdgeLength: //7
      rc = pConstMeshParameters->m_max_edge_length;
      break;
    case idxRefineAngle: //8
      rc = pConstMeshParameters->m_refine_angle;
      break;
    default:
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshParameters_SetDouble(ON_MeshParameters* pMeshParameters, int which, double val)
{
  const int idxGridAngle = 0;
  const int idxGridAspectRatio = 1;
  const int idxGridAmplification = 2;
  const int idxTolerance = 3;
  const int idxMinimumTolerance = 4;
  const int idxRelativeTolerance = 5;
  const int idxMinimumEdgeLength = 6;
  const int idxMaximumEdgeLength = 7;
  const int idxRefineAngle = 8;
  if( pMeshParameters )
  {
    switch(which)
    {
    case idxGridAngle: //0
      pMeshParameters->m_grid_angle = val;
      break;
    case idxGridAspectRatio: //1
      pMeshParameters->m_grid_aspect_ratio = val;
      break;
    case idxGridAmplification: //2
      pMeshParameters->m_grid_amplification = val;
      break;
    case idxTolerance: //3
      pMeshParameters->m_tolerance = val;
      break;
    case idxMinimumTolerance: //4
      pMeshParameters->m_min_tolerance = val;
      break;
    case idxRelativeTolerance: //5
      pMeshParameters->m_relative_tolerance = val;
      break;
    case idxMinimumEdgeLength: //6
      pMeshParameters->m_min_edge_length = val;
      break;
    case idxMaximumEdgeLength: //7
      pMeshParameters->m_max_edge_length = val;
      break;
    case idxRefineAngle: //8
      pMeshParameters->m_refine_angle = val;
      break;
    default:
      break;
    }
  }
}

RH_C_FUNCTION int ON_MeshParameters_GetGridCount(const ON_MeshParameters* pConstMeshParameters, bool mincount)
{
  int rc = 0;
  if( pConstMeshParameters )
  {
    if(mincount)
      rc = pConstMeshParameters->m_grid_min_count;
    else
      rc = pConstMeshParameters->m_grid_max_count;
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshParameters_SetGridCount(ON_MeshParameters* pMeshParameters, bool mincount, int count)
{
  if( pMeshParameters )
  {
    if(mincount)
      pMeshParameters->m_grid_min_count = count;
    else
      pMeshParameters->m_grid_max_count = count;
  }
}


RH_C_FUNCTION bool ON_MeshParameters_Copy(const ON_MeshParameters* pConstMP, /*ARRAY*/bool* bvals, /*ARRAY*/int* ivals, /*ARRAY*/double* dvals)
{
  bool rc = false;
  if( pConstMP && bvals && ivals && dvals )
  {
    bvals[0] = pConstMP->m_bJaggedSeams;
    bvals[1] = pConstMP->m_bSimplePlanes;
    bvals[2] = pConstMP->m_bRefine;
    bvals[3] = pConstMP->m_bComputeCurvature;

    ivals[0] = pConstMP->m_grid_min_count;
    ivals[1] = pConstMP->m_grid_max_count;
    ivals[2] = pConstMP->m_face_type;

    dvals[0] = pConstMP->m_grid_amplification;
    dvals[1] = pConstMP->m_tolerance;
    dvals[2] = pConstMP->m_grid_angle;
    dvals[3] = pConstMP->m_grid_aspect_ratio;
    dvals[4] = pConstMP->m_refine_angle;
    dvals[5] = pConstMP->m_min_tolerance;
    dvals[6] = pConstMP->m_max_edge_length;
    dvals[7] = pConstMP->m_min_edge_length;
    dvals[8] = pConstMP->m_relative_tolerance;

    rc = true;
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_TopologyVertex(const ON_Mesh* pConstMesh, int index, ON_3fPoint* point)
{
  if( pConstMesh && point )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( index>=0 && index < top.m_topv.Count() )
    {
      const int* vi = top.m_topv[index].m_vi;
      if( vi )
      {
        index = *vi;
        *point = pConstMesh->m_V[index];
      }
    }
  }
}

RH_C_FUNCTION void ON_Mesh_SetTopologyVertex(ON_Mesh* pMesh, int index, ON_3FPOINT_STRUCT point)
{
  if( pMesh && index>=0 )
  {
    const ON_MeshTopology& top = pMesh->Topology();
    if( index<=top.m_topv.Count() )
    {
      const int* vi = top.m_topv[index].m_vi;
      int count = top.m_topv[index].m_v_count;
      ON_3fPoint _pt(point.val[0], point.val[1], point.val[2]);
      for( int i=0; i<count; i++ )
      {
        int vertex=vi[i];
        pMesh->m_V[vertex] = _pt;
      }
    }
  }
}

RH_C_FUNCTION bool ON_Mesh_GetFaceCenter(const ON_Mesh* pConstMesh, int faceIndex, ON_3dPoint* center)
{
  bool rc = false;
  if( pConstMesh && center && faceIndex>=0 && faceIndex<pConstMesh->FaceCount())
  {
    const ON_MeshFace& face = pConstMesh->m_F[faceIndex];
    if( face.IsQuad() )
      *center = 0.25 * (pConstMesh->m_V[face.vi[0]]+pConstMesh->m_V[face.vi[1]]+pConstMesh->m_V[face.vi[2]]+pConstMesh->m_V[face.vi[3]]);
    else
      *center = (1.0/3.0) * (pConstMesh->m_V[face.vi[0]]+pConstMesh->m_V[face.vi[1]]+pConstMesh->m_V[face.vi[2]]);
    rc = true;
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_TopologyVertexIndex(const ON_Mesh* pConstMesh, int index)
{
  int rc = -1;
  if( pConstMesh && index>=0 && index<pConstMesh->VertexCount() )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    rc = top.m_topv_map[index];
  }
  return rc;
}

RH_C_FUNCTION void ON_Mesh_DestroyTextureData(ON_Mesh* pMesh)
{
  if( pMesh )
  {
    pMesh->m_Ttag.Default();
    pMesh->m_T.Destroy();
    pMesh->m_TC.Destroy();
    pMesh->m_packed_tex_domain[0].Set(0,1);
    pMesh->m_packed_tex_domain[1].Set(0,1);
    pMesh->InvalidateTextureCoordinateBoundingBox();
  }
}

/////////////////////////////////////////////////////////

RH_C_FUNCTION int ON_MeshTopologyVertex_Count(const ON_Mesh* pConstMesh, int topologyVertexIndex, bool vertices)
{
  int rc = -1;
  if( pConstMesh && topologyVertexIndex>=0 )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( topologyVertexIndex<top.TopVertexCount() )
    {
      if( vertices )
        rc = top.m_topv[topologyVertexIndex].m_v_count;
      else
        rc = top.m_topv[topologyVertexIndex].m_tope_count;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshTopologyVertex_GetIndices(const ON_Mesh* pConstMesh, int topologyVertexIndex, int count, /*ARRAY*/int* rc)
{
  if( pConstMesh && topologyVertexIndex>=0 && count>0 && rc )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( topologyVertexIndex<top.TopVertexCount() )
    {
      if( top.m_topv[topologyVertexIndex].m_v_count == count )
      {
        const int* source = top.m_topv[topologyVertexIndex].m_vi;
        memcpy(rc, source, count*sizeof(int));
      }
    }
  }
}

RH_C_FUNCTION void ON_MeshTopologyVertex_ConnectedVertices(const ON_Mesh* pConstMesh, int topologyVertexIndex, int count, /*ARRAY*/int* rc)
{
  if( pConstMesh && topologyVertexIndex>=0 && count>0 && rc )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( topologyVertexIndex<top.TopVertexCount() )
    {
      const ON_MeshTopologyVertex& tv = top.m_topv[topologyVertexIndex];
      if( tv.m_tope_count == count )
      {
        for( int i=0; i<count; i++ )
        {
          int edge_index = tv.m_topei[i];
          const ON_MeshTopologyEdge& edge = top.m_tope[edge_index];
          if( edge.m_topvi[0]==topologyVertexIndex )
            rc[i] = edge.m_topvi[1];
          else
            rc[i] = edge.m_topvi[0];
        }
      }
    }
  }
}

RH_C_FUNCTION bool ON_MeshTopologyVertex_SortEdges(const ON_Mesh* pConstMesh, int topologyVertexIndex)
{
  bool rc = false;
  if( pConstMesh )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( topologyVertexIndex>=0 )
      rc = top.SortVertexEdges(topologyVertexIndex);
    else
      rc = top.SortVertexEdges();
  }
  return rc;
}

RH_C_FUNCTION void ON_MeshTopologyVertex_ConnectedFaces(const ON_Mesh* pConstMesh, int topologyVertexIndex, ON_SimpleArray<int>* face_indices)
{
  if( pConstMesh && topologyVertexIndex>=0 && face_indices )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( topologyVertexIndex<top.TopVertexCount() )
    {
      const ON_MeshTopologyVertex& tv = top.m_topv[topologyVertexIndex];
      ON_SimpleArray<int> faces;
      for( int i=0; i<tv.m_tope_count; i++ )
      {
        int edge_index = tv.m_topei[i];
        const ON_MeshTopologyEdge& edge = top.m_tope[edge_index];
        for( int j=0; j<edge.m_topf_count; j++ )
        {
          faces.Append(edge.m_topfi[j]);
        }
      }
      faces.QuickSort(ON_CompareIncreasing<int>);
      int prev = -1;
      for( int i=0; i<faces.Count(); i++ )
      {
        if( prev!=faces[i] )
        {
          face_indices->Append(faces[i]);
          prev = faces[i];
        }
      }
    }
  }
}

RH_C_FUNCTION bool ON_MeshTopologyFace_Edges(const ON_Mesh* pConstMesh, int faceIndex, int* a, int* b, int* c, int* d)
{
  bool rc = false;
  if( pConstMesh && faceIndex>=0 && a && b && c && d )
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( faceIndex < top.m_topf.Count() )
    {
      const ON_MeshTopologyFace& face = top.m_topf[faceIndex];
      *a = face.m_topei[0];
      *b = face.m_topei[1];
      *c = face.m_topei[2];
      *d = face.m_topei[3];
      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_MeshTopologyFace_Edges2(const ON_Mesh* pConstMesh, int faceIndex, int* a, int* b, int* c, int* d, /*ARRAY*/int* orientationSame)
{
  bool rc = false;
  if( pConstMesh && faceIndex>=0 && a && b && c && d && orientationSame)
  {
    const ON_MeshTopology& top = pConstMesh->Topology();
    if( faceIndex < top.m_topf.Count() )
    {
      const ON_MeshTopologyFace& face = top.m_topf[faceIndex];
      *a = face.m_topei[0];
      *b = face.m_topei[1];
      *c = face.m_topei[2];
      *d = face.m_topei[3];
      for( int i=0; i<4; i++ )
        orientationSame[i] = (face.m_reve[i]==0)?1:0;
      rc = true;
    }
  }
  return rc;
}

/////////////////////////////////////////////////////////////////////////////
// ClosestPoint, Intersection, and mass property calculations are not
// provided in stand alone OpenNURBS

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION int ON_Mesh_GetClosestPoint(const ON_Mesh* ptr, ON_3DPOINT_STRUCT p, ON_3dPoint* q, double max_dist)
{
  int rc = -1;
  if( ptr && q )
  {
    const ON_3dPoint* _p = (const ON_3dPoint*)&p;
    ON_MESH_POINT mp;
    if( ptr->GetClosestPoint(*_p, &mp, max_dist) )
    {
      rc = mp.m_face_index;
      *q = mp.m_P;
    }
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_GetClosestPoint2(const ON_Mesh* pMesh, ON_3DPOINT_STRUCT testPoint, ON_3dPoint* closestPt, ON_3dVector* closestNormal, double max_dist)
{
  int rc = -1;
  if( pMesh && closestPt && closestNormal )
  {
    const ON_3dPoint* _testPoint = (const ON_3dPoint*)&testPoint;
    ON_MESH_POINT mp;
    if( pMesh->GetClosestPoint(*_testPoint, &mp, max_dist) )
    {
      if( mp.m_face_index>=0 && mp.m_face_index<pMesh->m_F.Count() )
      {
        *closestPt = mp.m_P;
        if( pMesh->m_N.Count()>0 )
        {
          const ON_MeshFace& face = pMesh->m_F[mp.m_face_index];
          ON_3dVector n0 = pMesh->m_N[face.vi[0]];
          ON_3dVector n1 = pMesh->m_N[face.vi[1]];
          ON_3dVector n2 = pMesh->m_N[face.vi[2]];
          ON_3dVector n3 = pMesh->m_N[face.vi[3]];
          *closestNormal = (n0 * mp.m_t[0]) +
                           (n1 * mp.m_t[1]) +
                           (n2 * mp.m_t[2]) +
                           (n3 * mp.m_t[3]);
          closestNormal->Unitize();
        }
        else if( pMesh->m_FN.Count()>0 )
        {
          *closestNormal = pMesh->m_FN[mp.m_face_index];
        }
        else
        {
          ON_3dPoint pA, pB, pC;
          if( mp.GetTriangle(pA, pB, pC ) )
          {
            *closestNormal = ON_TriangleNormal(pA, pB, pC);
          }
        }
        rc = mp.m_face_index;
      }
    }
  }
  return rc;
}

struct ON_MESHPOINT_STRUCT
{
    double m_et; 
    
    //ON_COMPONENT_INDEX m_ci;
    unsigned int m_ci_type;
    int m_ci_index;

    int m_edge_index;
    int m_face_index;
    char m_Triangle;
    double m_t0;
    double m_t1;
    double m_t2;
    double m_t3;

    //ON_3dPoint m_P;
    double m_Px;
    double m_Py;
    double m_Pz;
};

RH_C_FUNCTION bool ON_Mesh_GetClosestPoint3(const ON_Mesh* pConstMesh, ON_3DPOINT_STRUCT p, ON_MESHPOINT_STRUCT* meshpoint, double max_dist)
{
  bool rc = false;
  if( pConstMesh && meshpoint )
  {
    const ON_3dPoint* _p = (const ON_3dPoint*)&p;
    ON_MESH_POINT mp;
    rc = pConstMesh->GetClosestPoint(*_p, &mp, max_dist);
    if(rc)
    {
      meshpoint->m_et = mp.m_et;
      meshpoint->m_ci_type = mp.m_ci.m_type;
      meshpoint->m_ci_index = mp.m_ci.m_index;
      meshpoint->m_edge_index = mp.m_edge_index;
      meshpoint->m_face_index = mp.m_face_index;
      meshpoint->m_Triangle = mp.m_Triangle;
      meshpoint->m_t0 = mp.m_t[0];
      meshpoint->m_t1 = mp.m_t[1];
      meshpoint->m_t2 = mp.m_t[2];
      meshpoint->m_t3 = mp.m_t[3];
      meshpoint->m_Px = mp.m_P.x;
      meshpoint->m_Py = mp.m_P.y;
      meshpoint->m_Pz = mp.m_P.z;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Mesh_MeshPointAt(const ON_Mesh* pConstMesh, int faceIndex, double t0, double t1, double t2, double t3, ON_3dPoint* p)
{
  bool rc = false;
  if( pConstMesh )
  {
    // test to see if face exists
    if( faceIndex >= 0 && faceIndex < pConstMesh->m_F.Count() )
    {
      /// Barycentric quad coordinates for the point on the mesh
      /// face mesh.Faces[FaceIndex].  
      
      /// If the face is a triangle
      /// disregard T[3] (it should be set to 0.0). 
      
      /// If the face is
      /// a quad and is split between vertexes 0 and 2, then T[3]
      /// will be 0.0 when point is on the triangle defined by vi[0],
      /// vi[1], vi[2] 

      /// T[1] will be 0.0 when point is on the
      /// triangle defined by vi[0], vi[2], vi[3]. 

      /// If the face is a
      /// quad and is split between vertexes 1 and 3, then T[2] will
      /// be -1 when point is on the triangle defined by vi[0],
      /// vi[1], vi[3] 

      /// and m_t[0] will be -1 when point is on the
      /// triangle defined by vi[1], vi[2], vi[3].

      ON_MeshFace face = pConstMesh->m_F[faceIndex];

      // Collect data for barycentric evaluation.
      ON_3dPoint p0, p1, p2;

      if( face.IsTriangle() )
      {
        p0 = pConstMesh->m_V[face.vi[0]];
        p1 = pConstMesh->m_V[face.vi[1]];
        p2 = pConstMesh->m_V[face.vi[2]];
      }
      else
      {
        if( t3 == 0 )
        { // point is on subtriangle {0,1,2}
          p0 = pConstMesh->m_V[face.vi[0]];
          p1 = pConstMesh->m_V[face.vi[1]];
          p2 = pConstMesh->m_V[face.vi[2]];
        }
        else if( t1 == 0 )
        { // point is on subtriangle {0,2,3}
          p0 = pConstMesh->m_V[face.vi[0]];
          p1 = pConstMesh->m_V[face.vi[2]];
          p2 = pConstMesh->m_V[face.vi[3]];
          t0 = t0;
          t1 = t2;
          t2 = t3;
        }
        else if( t2 == -1 )
        { // point is on subtriangle {0,1,3}
          p0 = pConstMesh->m_V[face.vi[0]];
          p1 = pConstMesh->m_V[face.vi[1]];
          p2 = pConstMesh->m_V[face.vi[3]];
          t0 = t0;
          t1 = t1;
          t2 = t3;
        }
        else
        { // point must be on remaining subtriangle {1,2,3}
          p0 = pConstMesh->m_V[face.vi[1]];
          p1 = pConstMesh->m_V[face.vi[2]];
          p2 = pConstMesh->m_V[face.vi[3]];
          t0 = t1;
          t1 = t2;
          t2 = t3;
        }
      }

      p->x = t0 * p0.x + t1 * p1.x + t2 * p2.x;
      p->y = t0 * p0.y + t1 * p1.y + t2 * p2.y;
      p->z = t0 * p0.z + t1 * p1.z + t2 * p2.z;

      rc = true;
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_MESHPOINT_GetTriangle(const ON_Mesh* pConstMesh, const ON_MESHPOINT_STRUCT* meshpoint, int* a, int* b, int* c)
{
  bool rc = false;
  if( pConstMesh && meshpoint && a && b && c )
  {
    ON_MESH_POINT mp;
    mp.m_et = meshpoint->m_et;
    mp.m_ci.m_type = ON_COMPONENT_INDEX::Type(meshpoint->m_ci_type);
    mp.m_ci.m_index = meshpoint->m_ci_index;
    mp.m_edge_index = meshpoint->m_edge_index;
    mp.m_face_index = meshpoint->m_face_index;
    mp.m_mesh = pConstMesh;
    mp.m_mnode = NULL;
    mp.m_P.Set( meshpoint->m_Px, meshpoint->m_Py, meshpoint->m_Pz );
    mp.m_t[0] = meshpoint->m_t0;
    mp.m_t[1] = meshpoint->m_t1;
    mp.m_t[2] = meshpoint->m_t2;
    mp.m_t[3] = meshpoint->m_t3;
    mp.m_Triangle = meshpoint->m_Triangle;
    rc = mp.GetTriangle(*a, *b, *c);
  }
  return rc;
}

RH_C_FUNCTION int ON_Mesh_IntersectMesh(const ON_Mesh* ptr, const ON_Mesh* meshB, ON_SimpleArray<ON_Line>* lineArray)
{
  int rc = 0;
  if( ptr && meshB && lineArray )
  {
    rc = ptr->IntersectMesh(*meshB, *lineArray);
  }
  return rc;
}

RH_C_FUNCTION ON_MassProperties* ON_Mesh_MassProperties(bool bArea, const ON_Mesh* pMesh)
{
  ON_MassProperties* rc = NULL;
  if( pMesh )
  {
    rc = new ON_MassProperties();
    bool success = false;
    if( bArea )
      success = pMesh->AreaMassProperties( *rc, true, true, false, false );
    else
      success = pMesh->VolumeMassProperties( *rc, true, true, false, false, ON_UNSET_POINT );

    if( !success )
    {
      delete rc;
      rc = NULL;
    }
  }
  return rc;
}
#endif
