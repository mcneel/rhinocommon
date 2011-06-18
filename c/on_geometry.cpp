#include "StdAfx.h"

RH_C_FUNCTION void ON_Geometry_BoundingBox( const ON_Geometry* ptr, ON_BoundingBox* bbox )
{
  if( ptr && bbox )
    *bbox = ptr->BoundingBox();
}

RH_C_FUNCTION bool ON_Geometry_Rotate( ON_Geometry* ptr, double angle, ON_3DVECTOR_STRUCT axis, ON_3DPOINT_STRUCT center)
{
  bool rc = false;
  if( ptr )
  {
    const ON_3dVector* _axis = (const ON_3dVector*)&axis;
    const ON_3dPoint* _center = (const ON_3dPoint*)&center;
    rc = ptr->Rotate(angle, *_axis, *_center)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Geometry_Translate( ON_Geometry* ptr, ON_3DVECTOR_STRUCT translation_vector)
{
  bool rc = false;
  if( ptr )
  {
    const ON_3dVector* _translation_vector = (const ON_3dVector*)&translation_vector;
    rc = ptr->Translate(*_translation_vector)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Geometry_Scale( ON_Geometry* ptr, double scale)
{
  bool rc = false;
  if( ptr )
  {
    rc = ptr->Scale(scale)?true:false;
  }
  return rc;
}

#if !defined(OPENNURBS_BUILD)
// Add a curve to the partial boundingbox result.
static void ON_Brep_GetTightCurveBoundingBox_Helper( const ON_Curve& crv, ON_BoundingBox& bbox, const ON_Xform* xform, const ON_Xform* xform_inverse )
{
  // Get loose boundingbox of curve.
  ON_BoundingBox tempbox;
  if( !crv.GetBoundingBox(tempbox, false) )
    return;

  // Transform the loose box if necessary. 
  // Note: transforming a box might result in a larger box, 
  //       it's better to transform the curve, 
  //       which might actually result in a smaller box.
  if( xform_inverse )
  {
    tempbox.Transform(*xform_inverse); 
  }

  // If loose boundingbox of curve is inside partial result, return.
  if( bbox.Includes(tempbox, false) )
    return;

  // Get tight boundingbox of curve, grow partial result.
  if( crv.GetTightBoundingBox(tempbox, false, xform) )
    bbox.Union(tempbox);
}

// Add the isocurves of a BrepFace to the partial boundingbox result.
static void ON_Brep_GetTightIsoCurveBoundingBox_Helper( const TL_Brep& tlbrep, const ON_BrepFace& face, ON_BoundingBox& bbox, const ON_Xform* xform, int dir )
{
  ON_Interval domain = face.Domain(1 - dir);
  int degree =         face.Degree(1 - dir);
  int spancount =      face.SpanCount(1 - dir);
  int spansamples =    degree * (degree + 1) - 1;
  if( spansamples < 2 )
    spansamples = 2;

  // pbox delineates the extremes of the face interior.
  // We can use it to trivially reject spans and isocurves.
  ON_BrepLoop* pOuterLoop = face.OuterLoop();
  if( NULL==pOuterLoop )
    return;

  const ON_BoundingBox& pbox = pOuterLoop->m_pbox;
  double t0 = ((dir == 0) ? pbox.Min().y : pbox.Min().x);
  double t1 = ((dir == 0) ? pbox.Max().y : pbox.Max().x);

  // Get the surface span vector.
  ON_SimpleArray<double> spanvector(spancount + 1);
  spanvector.SetCount(spancount + 1);
  face.GetSpanVector(1 - dir, spanvector.Array());

  // Generate a list of all the sampling parameters.
  ON_SimpleArray<double> samples(spancount * spansamples);
  for( int s = 0; s < spancount; s++)
  {
    double s0 = spanvector[s];
    double s1 = spanvector[s+1];

    // Reject span if it does not intersect the pbox.
    if( s1 < t0 ) { continue; }
    if( s0 > t1 ) { continue; }
    
    ON_Interval span(s0, s1);
    for( int i = 1; i < spansamples; i++ )
    {
      double t = span.ParameterAt((double)i / (double)(spansamples - 1));
      // Reject iso if it does not intersect the pbox.
      if( t < t0 )
        continue;
      if( t > t1 )
        break;
      samples.Append(t);
    }
  }

  //Iterate over samples
  int sample_count = samples.Count();
  ON_BoundingBox loose_box;
  ON_SimpleArray<ON_Interval> intervals;
  ON_NurbsCurve isosubcrv;

  for( int i = 0; i<sample_count; i++)
  {
    // Retrieve iso-curve.
    ON_Curve* isocrv = face.IsoCurve(dir, samples[i]);

    while( NULL!=isocrv )
    {
      // Transform isocurve if necessary, this is better than transforming downstream boundingboxes.
      if( xform )
        isocrv->Transform(*xform);

      // Compute loose box.
      if( !isocrv->GetBoundingBox(loose_box, false))
        break;

      // Determine whether the loose box is already contained within the partial result.
      if( bbox.Includes(loose_box, false) ) 
        break;

      // Solve trimming domains for the iso-curve.
      intervals.SetCount(0);
      if( !tlbrep.GetIsoIntervals(face, dir, samples[i], intervals))
        break;

      // Iterate over trimmed iso-curves.
      int interval_count = intervals.Count();
      for( int k=0; k<interval_count; k++ )
      {
        //this to mask a bug in Rhino4. GetNurbForm does not destroy the Curve Tree. It does now.
        isosubcrv.DestroyCurveTree();
        isocrv->GetNurbForm(isosubcrv, 0.0, &intervals[k]);
        ON_Brep_GetTightCurveBoundingBox_Helper(isosubcrv, bbox, NULL, NULL);
      }
      break;
    }

    if( isocrv )
    {
      delete isocrv;
      isocrv = NULL;
    }
  }
}
// Add a face to the partial boundingbox result.
static void ON_Brep_GetTightFaceBoundingBox_Helper( const ON_BrepFace& face, ON_BoundingBox& bbox, const ON_Xform* xform, const ON_Xform* xform_inverse )
{
  ON_BoundingBox loose_box;

  // This should ideally test for planarity inside the OuterLoop() pbox only,
  // but no such function exists in the SDK as far as I can tell.
  if( face.IsPlanar() )
    return;

  // Get loose boundingbox of face.
  if( face.GetBoundingBox(loose_box, false) )
  {
    if( xform_inverse ) 
      loose_box.Transform(*xform_inverse); 

    if( bbox.Includes(loose_box, false) )
      return;
  }

  const TL_Brep* tlbrep = TL_Brep::Promote(face.Brep());
  if( tlbrep )
  {
    ON_Brep_GetTightIsoCurveBoundingBox_Helper( *tlbrep, face, bbox, xform, 0);
    ON_Brep_GetTightIsoCurveBoundingBox_Helper( *tlbrep, face, bbox, xform, 1);
  }
}
static bool ON_Brep_GetTightBoundingBox_Helper( const ON_Brep& brep, ON_BoundingBox& bbox, ON_Xform* xform )
{
  ON_Xform xform_inverse;
  ON_Xform* inverse = NULL;
  if( xform )
  {
    xform_inverse = xform->Inverse();
    inverse = &xform_inverse;
  }
  // make sure we have an empty/invalid bbox
  bbox.Destroy();

  // Compute Vertex bounding box.
  int vertex_count = brep.m_V.Count();
  if( xform )
  {
    ON_3dPointArray vtx(vertex_count);
    for( int i=0; i<vertex_count; i++ )
      vtx.Append(brep.m_V[i].point);
    vtx.GetTightBoundingBox(bbox, false, xform);
  }
  else
  {
    for( int i=0; i<vertex_count; i++ )
      bbox.Set(brep.m_V[i].point,true);
  }

  // Grow partial result with Edge bounding boxes.
  int edge_count = brep.m_E.Count();
  for( int i=0; i<edge_count; i++)
    ON_Brep_GetTightCurveBoundingBox_Helper(brep.m_E[i], bbox, xform, inverse);

  // Grow partial result with Face bounding boxes.
  int face_count = brep.m_F.Count();
  for( int i=0; i<face_count; i++)
    ON_Brep_GetTightFaceBoundingBox_Helper(brep.m_F[i], bbox, xform, inverse);

  return bbox.IsValid();
}
#endif

RH_C_FUNCTION bool ON_Geometry_GetTightBoundingBox(const ON_Geometry* ptr, ON_BoundingBox* bbox, ON_Xform* xform, bool useXform)
{
  bool rc = false;
  if( ptr && bbox )
  {
    if(!useXform || (xform && xform->IsIdentity()))
      xform = NULL;

    // OpenNURBS doesn't have a tight bounding box function for ON_Breps and we
    // want to make this work in V4
#if !defined(OPENNURBS_BUILD)
    const ON_Brep* pBrep = ON_Brep::Cast(ptr);
    if( pBrep )
      rc = ON_Brep_GetTightBoundingBox_Helper(*pBrep, *bbox, xform);
#endif
    // check rc in case the above function fails
    if( !rc )
      rc = ptr->GetTightBoundingBox(*bbox, false, xform);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Geometry_Transform( ON_Geometry* ptr, ON_Xform* xf)
{
  bool rc = false;
  if( ptr && xf )
  {
    rc = ptr->Transform(*xf)?true:false;
  }
  return rc;
}

RH_C_FUNCTION bool ON_Geometry_GetBool(ON_Geometry* pGeometry, int which)
{
  const int idxIsDeformable = 0;
  const int idxMakeDeformable = 1;
  const int idxIsMorphable = 2;
  const int idxHasBrepForm = 3;
  bool rc = false;
  if( pGeometry )
  {
    switch(which)
    {
    case idxIsDeformable:
      rc = pGeometry->IsDeformable();
      break;
    case idxMakeDeformable:
      rc = pGeometry->MakeDeformable();
      break;
    case idxIsMorphable:
      rc = pGeometry->IsMorphable();
      break;
    case idxHasBrepForm:
      rc = pGeometry->HasBrepForm()?true:false;
      break;
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Geometry_ComponentIndex( const ON_Geometry* ptr, ON_COMPONENT_INDEX* ci )
{
  if( ptr && ci )
  {
    *ci = ptr->ComponentIndex();
  }
}


const int idxON_Geometry = 0;
const int idxON_Curve = 1;
const int idxON_NurbsCurve = 2;
const int idxON_PolyCurve = 3;
const int idxON_PolylineCurve = 4;
const int idxON_ArcCurve = 5;
const int idxON_LineCurve = 6;
const int idxON_Mesh = 7;
const int idxON_Point = 8;
const int idxON_TextDot = 9;
const int idxON_Surface = 10;
const int idxON_Brep = 11;
const int idxON_NurbsSurface = 12;
const int idxON_RevSurface = 13;
const int idxON_PlaneSurface = 14;
const int idxON_ClippingPlaneSurface = 15;
const int idxON_Annotation2 = 16;
const int idxON_Hatch = 17;
const int idxON_TextEntity2 = 18;
const int idxON_SumSurface = 19;
const int idxON_BrepFace = 20;
const int idxON_BrepEdge = 21;
const int idxON_InstanceDefinition = 22;
const int idxON_InstanceReference = 23;
const int idxON_Extrusion = 24;
const int idxON_LinearDimension2 = 25;
const int idxON_PointCloud = 26;
const int idxON_DetailView = 27;
const int idxON_AngularDimension2 = 28;
const int idxON_RadialDimension2 = 29;
const int idxON_Leader = 30;
const int idxON_OrdinateDimension2 = 31;
const int idxON_Light = 32;
const int idxON_PointGrid = 33;
const int idxON_MorphControl = 34;

RH_C_FUNCTION int ON_Geometry_GetGeometryType( const ON_Object* pOnObject)
{
  int rc = -1;
  const ON_Geometry* pGeometry = ON_Geometry::Cast(pOnObject);
  if( pGeometry )
  {
    rc = idxON_Geometry;
    // This could probably be optimized by calling ObjectType once and being smart
    // about the cast calls.

    const ON_Geometry* pCastTest = ON_Curve::Cast(pGeometry);
    if( pCastTest )
    {
      rc = idxON_Curve; //1
      pCastTest = ON_NurbsCurve::Cast(pGeometry);
      if( pCastTest )
        return idxON_NurbsCurve; //2

      pCastTest = ON_LineCurve::Cast(pGeometry);
      if( pCastTest )
        return idxON_LineCurve; //6

      pCastTest = ON_PolylineCurve::Cast(pGeometry);
      if( pCastTest )
        return idxON_PolylineCurve; //4

      pCastTest = ON_PolyCurve::Cast(pGeometry);
      if( pCastTest )
        return idxON_PolyCurve; //3

      pCastTest = ON_ArcCurve::Cast(pGeometry);
      if( pCastTest )
        return idxON_ArcCurve; //5
      pCastTest = ON_BrepEdge::Cast(pGeometry);
      if( pCastTest )
        return idxON_BrepEdge; //21
      return rc;
    }

    pCastTest = ON_Mesh::Cast(pGeometry);
    if( pCastTest )
      return idxON_Mesh; //7

    pCastTest = ON_Point::Cast(pGeometry);
    if( pCastTest )
      return idxON_Point; //8

    pCastTest = ON_TextDot::Cast(pGeometry);
    if( pCastTest )
      return idxON_TextDot; //9

    pCastTest = ON_Surface::Cast(pGeometry);
    if( pCastTest ) //10
    {
      pCastTest = ON_NurbsSurface::Cast(pGeometry);
      if( pCastTest )
        return idxON_NurbsSurface; //12
      pCastTest = ON_RevSurface::Cast(pGeometry);
      if( pCastTest )
        return idxON_RevSurface; //13
      pCastTest = ON_ClippingPlaneSurface::Cast(pGeometry);
      if( pCastTest )
        return idxON_ClippingPlaneSurface;  //15
      pCastTest = ON_PlaneSurface::Cast(pGeometry);
      if( pCastTest )
        return idxON_PlaneSurface; //14
      pCastTest = ON_SumSurface::Cast(pGeometry);
      if( pCastTest )
        return idxON_SumSurface; //19
      pCastTest = ON_BrepFace::Cast(pGeometry);
      if( pCastTest )
        return idxON_BrepFace; //20

#if defined(RHINO_V5SR) // only available in Rhino 5
      pCastTest = ON_Extrusion::Cast(pGeometry);
      if( pCastTest )
        return idxON_Extrusion; //24
#endif

      return idxON_Surface;
    }

    pCastTest = ON_Brep::Cast(pGeometry);
    if( pCastTest )
      return idxON_Brep; //11

    pCastTest = ON_Annotation2::Cast(pGeometry);
    if( pCastTest )
    {
      pCastTest = ON_TextEntity2::Cast(pGeometry);
      if( pCastTest )
        return idxON_TextEntity2; //18
      pCastTest = ON_LinearDimension2::Cast(pGeometry);
      if( pCastTest )
        return idxON_LinearDimension2; //25
      pCastTest = ON_AngularDimension2::Cast(pGeometry);
      if( pCastTest )
        return idxON_AngularDimension2; //28
      pCastTest = ON_RadialDimension2::Cast(pGeometry);
      if( pCastTest )
        return idxON_RadialDimension2; // 29
      pCastTest = ON_Leader2::Cast(pGeometry);
      if( pCastTest )
        return idxON_Leader; //30
      pCastTest = ON_OrdinateDimension2::Cast(pGeometry);
      if( pCastTest )
        return idxON_OrdinateDimension2; // 31
      return idxON_Annotation2; //16
    }

    pCastTest = ON_Hatch::Cast(pGeometry);
    if( pCastTest )
      return idxON_Hatch; //17

    pCastTest = ON_InstanceDefinition::Cast(pGeometry);
    if( pCastTest )
      return idxON_InstanceDefinition; //22

    pCastTest = ON_InstanceRef::Cast(pGeometry);
    if( pCastTest )
      return idxON_InstanceReference; //23

    pCastTest = ON_PointCloud::Cast(pGeometry);
    if( pCastTest )
      return idxON_PointCloud; //26

    pCastTest = ON_DetailView::Cast(pGeometry);
    if( pCastTest )
      return idxON_DetailView; // 27

    pCastTest = ON_Light::Cast(pGeometry);
    if( pCastTest )
      return idxON_Light; //32

    pCastTest = ON_PointGrid::Cast(pGeometry);
    if( pCastTest )
      return idxON_PointGrid; //33

    pCastTest = ON_MorphControl::Cast(pGeometry);
    if( pCastTest )
      return idxON_MorphControl; //34
  }
  return rc;
}

RH_C_FUNCTION int ON_Geometry_GetCurveType( const ON_Curve* pCurve)
{
  int rc = -1;
  if( pCurve )
  {
    rc = idxON_Curve;
    const ON_Curve* pCastTest = ON_NurbsCurve::Cast(pCurve);
    if( pCastTest )
      return idxON_NurbsCurve;

    pCastTest = ON_PolylineCurve::Cast(pCurve);
    if( pCastTest )
      return idxON_PolylineCurve;

    pCastTest = ON_LineCurve::Cast(pCurve);
    if( pCastTest )
      return idxON_LineCurve;

    pCastTest = ON_PolyCurve::Cast(pCurve);
    if( pCastTest )
      return idxON_PolyCurve;

    pCastTest = ON_ArcCurve::Cast(pCurve);
    if( pCastTest )
      return idxON_ArcCurve;
  }
  return rc;
}

RH_C_FUNCTION ON_Brep* ON_Geometry_BrepForm(const ON_Geometry* pGeometry)
{
  ON_Brep* rc = NULL;
  if( pGeometry )
    rc = pGeometry->BrepForm();
  return rc;
}

/////////////////////////////////////////////////////////////////////////////
// ON_SimpleArray<ON_Geometry*> 

RH_C_FUNCTION ON_SimpleArray<ON_Geometry*>* ON_GeometryArray_New(int initial_capacity)
{
  return new ON_SimpleArray<ON_Geometry*>(initial_capacity);
}

RH_C_FUNCTION void ON_GeometryArray_Append(ON_SimpleArray<ON_Geometry*>* arrayPtr, ON_Geometry* geomPtr)
{
  if( arrayPtr && geomPtr )
  {
    arrayPtr->Append( geomPtr );
  }
}

//RH_C_FUNCTION int ON_GeometryArray_Count(const ON_SimpleArray<ON_Geometry*>* arrayPtr)
//{
//  int rc = 0;
//  if( arrayPtr )
//    rc = arrayPtr->Count();
//  return rc;
//}

//RH_C_FUNCTION ON_Geometry* ON_GeometryArray_Get(ON_SimpleArray<ON_Geometry*>* arrayPtr, int index)
//{
//  ON_Geometry* rc = NULL;
//  
//  if( arrayPtr && index>=0 )
//  {
//    if( index<arrayPtr->Count() )
//      rc = (*arrayPtr)[index];
//  }
//  return rc;
//}

RH_C_FUNCTION void ON_GeometryArray_Delete(ON_SimpleArray<ON_Geometry*>* arrayPtr)
{
  if( arrayPtr )
    delete arrayPtr;
}

RH_C_FUNCTION int ON_GeometryArray_Count(ON_SimpleArray<ON_Geometry*>* arrayPtr)
{
  int rc = 0;
  if( arrayPtr )
    rc = arrayPtr->Count();
  return rc;
}

RH_C_FUNCTION ON_Geometry* ON_GeometryArray_Get(ON_SimpleArray<ON_Geometry*>* arrayPtr, int index)
{
  ON_Geometry* rc = NULL;
  
  if( arrayPtr && index>=0 )
  {
    if( index<arrayPtr->Count() )
      rc = (*arrayPtr)[index];
  }
  return rc;
}
