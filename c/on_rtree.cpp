#include "StdAfx.h"

RH_C_FUNCTION ON_RTree* ON_RTree_New()
{
  return new ON_RTree();
}

RH_C_FUNCTION void ON_RTree_Delete(ON_RTree* pTree)
{
  if( pTree )
    delete pTree;
}

RH_C_FUNCTION bool ON_RTree_CreateMeshFaceTree(ON_RTree* pTree, const ON_Mesh* pConstMesh)
{
  bool rc = false;
  if( pTree && pConstMesh )
    rc = pTree->CreateMeshFaceTree(pConstMesh);
  return rc;
}

typedef int (CALLBACK* RTREESEARCHPROC)(int serial_number, void* idA, void* idB);
static RTREESEARCHPROC g_theRTreeSearcher = NULL;

static bool RhCmnTreeSearch(void* context, ON__INT_PTR a_id)
{
  bool rc = false;
  if( g_theRTreeSearcher )
  {
    int cbrc = g_theRTreeSearcher((int)context, (void*)a_id, 0);
    rc = cbrc?true:false;
  }
  return rc;
}

static bool RhCmnTreeSearch2(void* context, ON__INT_PTR a_id, ON__INT_PTR b_id)
{
  bool rc = false;
  if( g_theRTreeSearcher )
  {
    int cbrc = g_theRTreeSearcher((int)context, (void*)a_id, (void*)b_id);
    rc = cbrc?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_RTree_Search(const ON_RTree* pConstTree, ON_3DPOINT_STRUCT pt0, ON_3DPOINT_STRUCT pt1, int serial_number, RTREESEARCHPROC searchCB)
{
  bool rc = false;
  if( pConstTree && searchCB )
  {
    g_theRTreeSearcher = searchCB;
    rc = pConstTree->Search(pt0.val, pt1.val, RhCmnTreeSearch, (void*)serial_number);
  }
  return rc;
}

RH_C_FUNCTION bool ON_RTree_Search2(const ON_RTree* pConstTreeA, const ON_RTree* pConstTreeB, double tolerance, int serial_number, RTREESEARCHPROC searchCB)
{
  bool rc = false;
  if( pConstTreeA && pConstTreeB && searchCB )
  {
#if defined(RHINO_V5SR) // only available in V5
    g_theRTreeSearcher = searchCB;
    ON_RTree::Search(*pConstTreeA, *pConstTreeB, tolerance, RhCmnTreeSearch2, (void*)serial_number);
#endif
  }
  return rc;
}


RH_C_FUNCTION bool ON_RTree_InsertRemove(ON_RTree* pTree, bool insert, ON_3DPOINT_STRUCT pt0, ON_3DPOINT_STRUCT pt1, void* elementId)
{
  bool rc = false;
  if( pTree )
  {
    if( insert )
      rc = pTree->Insert(pt0.val, pt1.val, elementId);
    else
      rc = pTree->Remove(pt0.val, pt1.val, elementId);
  }
  return rc;
}

RH_C_FUNCTION void ON_RTree_RemoveAll(ON_RTree* pTree)
{
  if( pTree )
    pTree->RemoveAll();
}

RH_C_FUNCTION unsigned int ON_RTree_SizeOf(const ON_RTree* pConstTree)
{
  unsigned int rc = 0;
  if( pConstTree )
    rc = (unsigned int)pConstTree->SizeOf();
  return rc;
}

RH_C_FUNCTION int ON_RTree_ElementCount(ON_RTree* pTree)
{
  int rc = 0;
  if( pTree )
    rc = pTree->ElementCount();
  return rc;
}