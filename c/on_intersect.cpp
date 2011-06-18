#include "StdAfx.h"

RH_C_FUNCTION bool ON_Intersect_LineLine(ON_Line* lineA, ON_Line* lineB, double* a, double* b)
{
  bool rc = false;
  if( lineA && lineB )
  {
    rc = ::ON_Intersect(*lineA, *lineB, a, b);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Intersect_LinePlane(ON_Line* line, const ON_PLANE_STRUCT* plane, double* parameterOnLine)
{
  bool rc = false;
  if( line && plane )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    rc = ::ON_Intersect(*line, temp, parameterOnLine);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Intersect_PlanePlane(const ON_PLANE_STRUCT* planeA, const ON_PLANE_STRUCT* planeB, ON_Line* line)
{
  bool rc = false;
  if( line && planeA && planeB )
  {
    ON_Plane tempA = FromPlaneStruct(*planeA);
    ON_Plane tempB = FromPlaneStruct(*planeB);
    rc = ::ON_Intersect(tempA, tempB, *line);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Intersect_PlanePlanePlane(const ON_PLANE_STRUCT* planeA, const ON_PLANE_STRUCT* planeB, const ON_PLANE_STRUCT* planeC, ON_3dPoint* intersectionPoint)
{
  bool rc = false;
  if( intersectionPoint && planeA && planeB && planeC )
  {
    ON_Plane tempA = FromPlaneStruct(*planeA);
    ON_Plane tempB = FromPlaneStruct(*planeB);
    ON_Plane tempC = FromPlaneStruct(*planeC);
    rc = ::ON_Intersect(tempA, tempB, tempC, *intersectionPoint);
  }
  return rc;
}

#if !defined(RHINO_V5SR) // fixed in V5
// Copied from opennurbs_intersect.cpp but with a bug fix.
// We can remove it once the bug is fixed in OpenNurbs and once 
// Grasshopper has dropped Rhino4 support.
int PS_Intersect(
        const ON_Plane& plane,
        const ON_Sphere& sphere, 
        ON_Circle& circle
        )
{
  // 16 April 2011 Dale Lear
  //   Prior to this date, this function did not return the correct answer.

  int rc = 0;
  const double sphere_radius = fabs(sphere.radius);
  double tol = sphere_radius*ON_SQRT_EPSILON;
  if ( !(tol >= ON_ZERO_TOLERANCE) )
    tol = ON_ZERO_TOLERANCE;
  const ON_3dPoint sphere_center = sphere.Center();
  ON_3dPoint circle_center = plane.ClosestPointTo(sphere_center);
  double d = circle_center.DistanceTo(sphere_center);

  circle.radius = 0.0;

  if ( ON_IsValid(sphere_radius) && ON_IsValid(d) && d <= sphere_radius + tol )
  {
    if ( sphere_radius > 0.0 )
    {
      d /= sphere_radius;
      d = 1.0 - d*d;
      // The d > 4.0*ON_EPSILON was picked by testing spheres with
      // radius = 1 and center = (0,0,0).  Do not make 4.0*ON_EPSILON 
      // any smaller and please discuss changes with Dale Lear.
      circle.radius = (d > 4.0*ON_EPSILON) ? sphere_radius*sqrt(d) : 0.0;
    }
    else
      circle.radius = 0.0;

    if ( circle.radius <= ON_ZERO_TOLERANCE )
    {
      // return a single point
      rc = 1;
      
      circle.radius = 0.0;

      //  When tolerance is in play, put the point on the sphere.
      //  If the caller prefers the plane, then they can adjust the
      //  returned answer to get the plane.
      ON_3dVector R = circle_center - sphere_center;
      double r0 = R.Length();
      if ( r0 > 0.0 )
      {
        R.Unitize();
        ON_3dPoint C1 = sphere_center + sphere_radius*R;
        double r1 = C1.DistanceTo(sphere_center);
        if ( fabs(sphere.radius-r1) < fabs(sphere.radius-r0) )
          circle_center = C1;
      }
    }
    else 
    {
      // return a circle
      rc = 2;
    }
  }

  // Update circle's plane here in case the input plane 
  // is the circle's plane member.
  circle.plane = plane;
  circle.plane.origin = circle_center;
  circle.plane.UpdateEquation();

  return rc;
}
#endif

RH_C_FUNCTION int ON_Intersect_PlaneSphere(const ON_PLANE_STRUCT* plane, ON_Sphere* sphere, ON_CIRCLE_STRUCT* intersectionCircle)
{
  int rc = 0;
  if( plane && sphere && intersectionCircle )
  {
    sphere->plane.UpdateEquation();
    ON_Plane temp = FromPlaneStruct(*plane);
    ON_Circle circle = FromCircleStruct(*intersectionCircle);
#if defined(RHINO_V5SR) // fixed in V5
    rc = ON_Intersect(temp, *sphere, circle);
#else
    rc = PS_Intersect(temp, *sphere, circle); //Go back to ::ON_Intersect(temp, *sphere, circle); once Grasshopper drops Rhino4 support.
#endif
    CopyToCircleStruct(*intersectionCircle, circle);
  }
  return rc;
}

RH_C_FUNCTION int ON_Intersect_LineSphere(ON_Line* line, ON_Sphere* sphere, ON_3dPoint* point1, ON_3dPoint* point2)
{
  int rc = 0;
  if( line && sphere && point1 && point2 )
  {
    sphere->plane.UpdateEquation();
    rc = ::ON_Intersect(*line, *sphere, *point1, *point2);
  }
  return rc;
}
RH_C_FUNCTION int ON_Intersect_LineCircle(const ON_Line* pLine, const ON_CIRCLE_STRUCT* pCircle, double* t1, ON_3dPoint* point1, double* t2, ON_3dPoint* point2)
{
  int rc = 0;
  if( pLine && pCircle && t1 && point1 && t2 && point2 )
  {
    ON_Circle circle = FromCircleStruct(*pCircle);
    rc = ::ON_Intersect(*pLine, circle, t1, *point1, t2, *point2);
  }
  return rc;
}

RH_C_FUNCTION int ON_Intersect_LineCylinder(ON_Line* line, ON_Cylinder* cylinder, ON_3dPoint* point1, ON_3dPoint* point2)
{
  int rc = 0;
  if( line && cylinder && point1 && point2 )
  {
    cylinder->circle.plane.UpdateEquation();
    rc = ::ON_Intersect(*line, *cylinder, *point1, *point2);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Intersect_BoundingBoxLine(ON_BoundingBox* box, ON_Line* line, double tolerance, ON_Interval* t)
{
  bool rc = false;
  if( box && line )
  {
    rc = ::ON_Intersect(*box, *line, tolerance, t);
  }
  return rc;
}

RH_C_FUNCTION int ON_Intersect_SphereSphere(ON_Sphere* sphereA, ON_Sphere* sphereB, ON_CIRCLE_STRUCT* pCircle)
{
  int rc = 0;
  if( sphereA && sphereB && pCircle )
  {
    sphereA->plane.UpdateEquation();
    sphereB->plane.UpdateEquation();
    ON_Circle circle = FromCircleStruct(*pCircle);
    rc = ::ON_Intersect(*sphereA, *sphereB, circle);
    CopyToCircleStruct(*pCircle, circle);
  }
  return rc;
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION ON_SimpleArray<ON_Polyline*>* ON_Intersect_MeshPlanes1(const ON_Mesh* meshPtr, int plane_count, /*ARRAY*/const ON_PLANE_STRUCT* planes, int* polyline_count)
{
  if( NULL==meshPtr || plane_count<1 || NULL==planes || NULL==polyline_count )
    return NULL;

  ON_SimpleArray<ON_Polyline*>* polylines = new ON_SimpleArray<ON_Polyline*>();

  TL_MeshXPlane mxp(*meshPtr, meshPtr->Topology());
  for( int i=0; i<plane_count; i++ )
  {
    ON_3dPoint point = planes[i].origin;
    ON_3dVector normal = planes[i].zaxis;
    // I looked through TL and the Intersect function just keeps appending to the input polyline list
    mxp.Intersect(point, normal, *polylines);
  }
  *polyline_count = polylines->Count();
  if( *polyline_count < 1 )
  {
    delete polylines;
    return NULL;
  }
  return polylines;
}
#endif

// return number of points in a certain polyline
RH_C_FUNCTION int ON_Intersect_MeshPlanes2(ON_SimpleArray<ON_Polyline*>* pPolylines, int i)
{
  int rc = 0;
  if( pPolylines && i>=0 && i<pPolylines->Count() )
  {
    ON_Polyline* polyline = (*pPolylines)[i];
    if( polyline )
      rc = polyline->Count();
  }
  return rc;
}

RH_C_FUNCTION void ON_Intersect_MeshPlanes3(ON_SimpleArray<ON_Polyline*>* pPolylines, int i, int point_count, /*ARRAY*/ON_3dPoint* points)
{
  if( NULL==pPolylines || i<0 || i>=pPolylines->Count() || point_count<0 || NULL==points || NULL==*points)
    return;
  ON_Polyline* polyline = (*pPolylines)[i];
  if( NULL==polyline || polyline->Count()!=point_count )
    return;

  const ON_3dPoint* source = polyline->Array();
  ::memcpy(points, source, sizeof(ON_3dPoint) * point_count);
}

RH_C_FUNCTION void ON_Intersect_MeshPlanes4(ON_SimpleArray<ON_Polyline*>* pPolylines)
{
  if( NULL==pPolylines )
    return;
  int count = pPolylines->Count();
  for( int i=0; i<count; i++ )
  {
    ON_Polyline* polyline = (*pPolylines)[i];
    if( polyline )
      delete polyline;
  }
  delete pPolylines;
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION double ON_Intersect_MeshRay1(const ON_Mesh* pMesh, ON_3dRay* ray, ON_SimpleArray<int>* face_indices)
{
  double rc = -1.0;
  // it is ok if face_indices is null
  if( pMesh && ray )
  {
    const ON_MeshTree* mt = pMesh->MeshTree();
    ON_3dVector rayVec = ray->m_V;
    if( mt && rayVec.Unitize() )
    {
      double rayRange = mt->m_bbox.MaximumDistanceTo(ray->m_P);
      ON_Line line(ray->m_P, ray->m_P + rayRange * rayVec );
      ON_SimpleArray<ON_CMX_EVENT> hits;
      mt->IntersectLine( line, hits );
      int hitCount = hits.Count();
      if( hitCount > 0 )
      {
        ON_SimpleArray<double> tvals;
        ON_SimpleArray<int> indices;
        // tMin should be between 0 and 1 for the line
        double tMin = 100.0;
        for( int i=0; i<hitCount; i++ )
        {
          const ON_CMX_EVENT& e = hits[i];
          if( e.m_C[0].m_t <= tMin )
          {
            tMin = e.m_C[0].m_t;
            if( face_indices )
            {
              tvals.Append(tMin);
              indices.Append(e.m_M[0].m_face_index);
            }
          }
          if( e.m_type == ON_CMX_EVENT::cmx_overlap && e.m_C[1].m_t <= tMin )
          {
            tMin = e.m_C[1].m_t;
            if( face_indices )
            {
              tvals.Append(tMin);
              indices.Append( e.m_M[1].m_face_index);
            }
          }
        }
        if( tMin >=0 && tMin <= 1.0 )
        {
          if( face_indices )
          {
            for( int i=0; i<tvals.Count(); i++ )
            {
              if( tvals[i]==tMin )
                face_indices->Append(indices[i]);
            }
          }

          double lineLength = line.Length();
          double rayLength = ray->m_V.Length();
          if( rayLength > ON_SQRT_EPSILON )
          {
            rc = tMin * lineLength / rayLength;
          }
        }
      }
    }
  }
  return rc;
}

RH_C_FUNCTION ON_SimpleArray<ON_CMX_EVENT>* ON_Intersect_MeshPolyline1(const ON_Mesh* pMesh, const ON_PolylineCurve* pCurve, int* count)
{
  ON_SimpleArray<ON_CMX_EVENT>* rc = NULL;
  if( pMesh && pCurve && count )
  {
    *count = 0;
    const ON_MeshTree* mesh_tree = pMesh->MeshTree();
    if( mesh_tree )
    {
      rc = new ON_SimpleArray<ON_CMX_EVENT>();
      int pline_count = pCurve->m_pline.Count();
      const ON_3dPoint* points = pCurve->m_pline.Array();
      if( mesh_tree->IntersectPolyline(pline_count, points, *rc) )
      {
        *count = rc->Count();
      }
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Intersect_MeshPolyline_Fill(ON_SimpleArray<ON_CMX_EVENT>* pCMX, int count, /*ARRAY*/ON_3dPoint* points, /*ARRAY*/int* faceIds)
{
  if( pCMX && points && faceIds && pCMX->Count()==count )
  {
    for( int i=0; i<count; i++ )
    {
      points[i] = (*pCMX)[i].m_M[0].m_P;
      faceIds[i] = (*pCMX)[i].m_M[0].m_face_index;
    }
  }

  if( pCMX )
    delete pCMX;
}
#endif

RH_C_FUNCTION ON_SimpleArray<ON_X_EVENT>* ON_Intersect_CurveSelf(const ON_Curve* pCurve, double tolerance)
{
  ON_SimpleArray<ON_X_EVENT>* rc = NULL;
  if(pCurve)
  {
    rc = new ON_SimpleArray<ON_X_EVENT>();
    pCurve->IntersectSelf(*rc, tolerance);
  }
 
  return rc;
}

RH_C_FUNCTION ON_SimpleArray<ON_X_EVENT>* ON_Intersect_CurveCurve(const ON_Curve* pCurveA,
                                                                  const ON_Curve* pCurveB,
                                                                  double tolerance,
                                                                  double overlap_tolerance)
{

  ON_SimpleArray<ON_X_EVENT>* rc = NULL;
  if(pCurveA && pCurveB)
  {
    rc = new ON_SimpleArray<ON_X_EVENT>();
    pCurveA->IntersectCurve(pCurveB, *rc, tolerance, overlap_tolerance);
  }
 
  return rc;
}

RH_C_FUNCTION ON_SimpleArray<ON_X_EVENT>* ON_Intersect_CurveSurface(const ON_Curve* pCurve,
                                                                    const ON_Surface* pSurface,
                                                                    double tolerance,
                                                                    double overlap_tolerance)
{

  ON_SimpleArray<ON_X_EVENT>* rc = NULL;
  if(pCurve && pSurface)
  {
    rc = new ON_SimpleArray<ON_X_EVENT>();
    pCurve->IntersectSurface(pSurface, *rc, tolerance, overlap_tolerance);
  }
 
  return rc;
}
RH_C_FUNCTION ON_SimpleArray<ON_X_EVENT>* ON_Intersect_CurveSurface2(const ON_Curve* pCurve,
                                                                     const ON_Surface* pSurface,
                                                                     double domain0, 
                                                                     double domain1,
                                                                     double tolerance,
                                                                     double overlap_tolerance)
{

  ON_SimpleArray<ON_X_EVENT>* rc = NULL;
  if(pCurve && pSurface)
  {
    ON_Interval domain(domain0, domain1);
    rc = new ON_SimpleArray<ON_X_EVENT>();
    pCurve->IntersectSurface(pSurface, *rc, tolerance, overlap_tolerance, &domain);
  }
 
  return rc;
}

RH_C_FUNCTION void ON_Intersect_IntersectArrayDelete(ON_SimpleArray<ON_X_EVENT>* pArray)
{
  if( pArray )
    delete pArray;
}

RH_C_FUNCTION int ON_Intersect_IntersectArrayCount(const ON_SimpleArray<ON_X_EVENT>* pArray)
{
  int rc = 0;
  if( pArray )
    rc = pArray->Count();
  return rc;
}

RH_C_FUNCTION bool ON_Intersect_CurveIntersectData(const ON_SimpleArray<ON_X_EVENT>* pArray,
                                                   int index, int* type,
                                                   ON_3dPoint* startA, ON_3dPoint* endA,
                                                   ON_3dPoint* startB, ON_3dPoint* endB,
                                                   double* ua0, double* ua1,
                                                   double* ub0, double* ub1,
                                                   double* vb0, double* vb1)
{
  if(pArray && pArray->Count() > index && type && startA && endA 
    && startB && endB && ua0 && ua1 && ub0 && ub1 && vb0 && vb1)
  {
    const ON_X_EVENT* x = &(*pArray)[index];
    if (x)
    {
      *type = (int)(x->m_type);
      *startA = x->m_A[0];
      *endA = x->m_A[1];
      *startB = x->m_B[0];
      *endB = x->m_B[1];
      *ua0 = x->m_a[0];
      *ua1 = x->m_a[1];
      *ub0 = x->m_b[0];
      *ub1 = x->m_b[1];
      *vb0 = x->m_b[2];
      *vb1 = x->m_b[3];
      return true;
    }
  }
  return false;
}

#if !defined(OPENNURBS_BUILD)
RH_C_FUNCTION int ON_RayShooter_OneSurface(ON_3DPOINT_STRUCT _point, ON_3DVECTOR_STRUCT _direction, const ON_Surface* pConstSurface, ON_SimpleArray<ON_3dPoint>* pPoints, int maxReflections)
{
  int rc = 0;
  ON_3dPoint point(_point.val[0], _point.val[1], _point.val[2]);
  ON_3dVector direction(_direction.val[0], _direction.val[1], _direction.val[2]);
  if( pConstSurface && pPoints && maxReflections>0 && point.IsValid() && direction.Unitize() )
  {
    ON_RayShooter shooter;
    ON_X_EVENT hit;
    ON_3dPoint Q = point;
    ON_3dVector R = direction;
    ON_3dVector V[3];
    for( int i=0; i<maxReflections; i++ )
    {
      memset(&hit,0,sizeof(hit));
      ON_3dVector T = R;
      if( !T.Unitize() )
        break;
      if( !shooter.Shoot(Q,T,pConstSurface,hit) )
        break;
      Q = hit.m_A[0];
      pPoints->Append(Q);
      if( !hit.m_snodeB[0] )
        break;
      hit.m_snodeB[0]->Evaluate(hit.m_b[0], hit.m_b[1], 1, 3, &V[0].x);
      ON_3dVector N = ON_CrossProduct(V[1],V[2]);
      if ( !N.Unitize() )
        break;
      double d = N*T;
      R = T + (-2.0*d)*N; // R = reflection direction
    }
    rc = pPoints->Count();
  }
  return rc;
}

RH_C_FUNCTION int ON_RayShooter_ShootRay(ON_3DPOINT_STRUCT _point, ON_3DVECTOR_STRUCT _direction,
                                           const ON_SimpleArray<const ON_Geometry*>* pConstGeometryArray,
                                           ON_SimpleArray<ON_3dPoint>* pPoints, int maxReflections)
{
  int rc = 0;
  ON_3dPoint point(_point.val[0], _point.val[1], _point.val[2]);
  ON_3dVector direction(_direction.val[0], _direction.val[1], _direction.val[2]);

  // Currently only supports surfaces and breps with untrimmed faces.
  // Add support for meshes and trimmed breps

  int count = pConstGeometryArray?pConstGeometryArray->Count():0;
  if( count<1 )
    return 0;
  ON_SimpleArray<const ON_SurfaceTreeNode*> snode_list(count);
  for ( int i=0; i<count; i++ )
  {
    const ON_Geometry* pGeometry = (*pConstGeometryArray)[i];
    const ON_Surface* surface = ON_Surface::Cast(pGeometry);
    if ( surface )
    {
      const ON_SurfaceTree* stree = surface->SurfaceTree();
      if ( stree )
        snode_list.Append(stree);
      continue;
    }
    const ON_Brep* brep = ON_Brep::Cast(pGeometry);
    if( brep )
    {
      for( int fi=0; fi<brep->m_F.Count(); fi++ )
      {
        const ON_SurfaceTree* stree = brep->m_F[fi].SurfaceTree();
        if( stree )
        snode_list.Append(stree);
      }
      continue;
    }
  }
  if( snode_list.Count()<1 )
    return 0;

  if( pPoints && maxReflections>0 && point.IsValid() && direction.Unitize() )
  {
    ON_RayShooter shooter;
    ON_X_EVENT hit;
    ON_3dPoint Q = point;
    ON_3dVector R = direction;
    ON_3dVector V[3];
    for( int i=0; i<maxReflections; i++ )
    {
      memset(&hit,0,sizeof(hit));
      ON_3dVector T = R;
      if( !T.Unitize() )
        break;
      if( !shooter.Shoot(Q,T,snode_list,hit) )
        break;
      Q = hit.m_A[0];
      pPoints->Append(Q);
      if( !hit.m_snodeB[0] )
        break;

      ON_3dVector N = hit.m_B[1]; // surface normal
      double d = -2.0*(N.x*T.x + N.y*T.y + N.z*T.z);
      R.x = T.x + d*N.x;
      R.y = T.y + d*N.y;
      R.z = T.z + d*N.z;

      // Part of the fix for RR 22717.  See opennurbs_plus_xray.cpp
      // for the rest of the fix.
      d = hit.m_A[0].DistanceTo( hit.m_B[0] );
      shooter.m_min_travel_distance = d;
      if( shooter.m_min_travel_distance < 1.0e-8 )
        shooter.m_min_travel_distance = 1.0e-8;
    }
    rc = pPoints->Count();
  }
  return rc;
}

RH_C_FUNCTION ON_SimpleArray<ON_Polyline*>* ON_Intersect_MeshMesh1(const ON_Mesh* pConstMeshA, const ON_Mesh* pConstMeshB, int* polyline_count, double tolerance)
{
  ON_SimpleArray<ON_Polyline*>* rc = NULL;
  if( polyline_count ) *polyline_count = 0;
  if( pConstMeshA && pConstMeshB && polyline_count )
  {
    ON_ClassArray<ON_MMX_Polyline> plines;
    ON_ClassArray<ON_MMX_Polyline> overlapplines;
    if(::ON_MeshMeshIntersect(pConstMeshA, pConstMeshB, plines, overlapplines, tolerance, tolerance))
    {
      rc = new ON_SimpleArray<ON_Polyline*>();
      for( int i=0; i<plines.Count(); i++ )
      {
        ON_Polyline* pl = new ON_Polyline();
        const ON_MMX_Polyline& mmxpoly = plines[i];
        int c = mmxpoly.Count();
        for( int j=0; j<c; j++ )
          pl->Append(mmxpoly[j].m_A.m_P);
        pl->Clean(ON_ZERO_TOLERANCE);
        if( !pl->IsValid() )
        {
          delete pl;
          continue;
        }
        rc->Append(pl);
      }
      for( int i=0; i<overlapplines.Count(); i++ )
      {
        ON_Polyline* pl = new ON_Polyline();
        const ON_MMX_Polyline& mmxpoly = overlapplines[i];
        int c = mmxpoly.Count();
        for( int j=0; i<c; i++ )
          pl->Append(mmxpoly[j].m_A.m_P);
        pl->Clean(ON_ZERO_TOLERANCE);
        if( !pl->IsValid() )
        {
          delete pl;
          continue;
        }
        rc->Append(pl);
      }
      *polyline_count = rc->Count();
    }
  }
  return rc;
}

#endif