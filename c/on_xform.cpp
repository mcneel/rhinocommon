#include "StdAfx.h"

RH_C_FUNCTION void ON_Xform_Scale( ON_Xform* xf, const ON_PLANE_STRUCT* plane, double xFactor, double yFactor, double zFactor )
{
  if( xf && plane )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    xf->Scale(temp, xFactor, yFactor, zFactor);
  }
}

RH_C_FUNCTION void ON_Xform_Rotation( ON_Xform* xf, double sinAngle, double cosAngle, ON_3DVECTOR_STRUCT rotationAxis, ON_3DPOINT_STRUCT rotationCenter)
{
  if( xf )
  {
    const ON_3dVector* axis = (const ON_3dVector*)&rotationAxis;
    const ON_3dPoint* center = (const ON_3dPoint*)&rotationCenter;
    xf->Rotation(sinAngle, cosAngle, *axis, *center);
  }
}

RH_C_FUNCTION bool ON_Xform_PlaneToPlane( ON_Xform* xf, const ON_PLANE_STRUCT* plane0, const ON_PLANE_STRUCT* plane1, bool rotation)
{
  bool rc = false;
  if( xf && plane0 && plane1 )
  {
    ON_Plane temp0 = FromPlaneStruct(*plane0);
    ON_Plane temp1 = FromPlaneStruct(*plane1);
    if( rotation )
    {
      xf->Rotation(temp0, temp1);
      rc = true;
    }
    else
    {
      rc = xf->ChangeBasis(temp0, temp1);
    }
  }
  return rc;
}

RH_C_FUNCTION void ON_Xform_Mirror( ON_Xform* xf, ON_3DPOINT_STRUCT pointOnMirrorPlane, ON_3DVECTOR_STRUCT normalToMirrorPlane)
{
  if( xf )
  {
    const ON_3dPoint* _pt = (const ON_3dPoint*)&pointOnMirrorPlane;
    const ON_3dVector* _n = (const ON_3dVector*)&normalToMirrorPlane;
    xf->Mirror(*_pt, *_n);
  }
}

RH_C_FUNCTION bool ON_Xform_ChangeBasis2( ON_Xform* xf,
                                          ON_3DVECTOR_STRUCT x0, ON_3DVECTOR_STRUCT y0, ON_3DVECTOR_STRUCT z0,
                                          ON_3DVECTOR_STRUCT x1, ON_3DVECTOR_STRUCT y1, ON_3DVECTOR_STRUCT z1)
{
  bool rc = false;
  if( xf )
  {
    const ON_3dVector* _x0 = (const ON_3dVector*)&x0;
    const ON_3dVector* _y0 = (const ON_3dVector*)&y0;
    const ON_3dVector* _z0 = (const ON_3dVector*)&z0;
    const ON_3dVector* _x1 = (const ON_3dVector*)&x1;
    const ON_3dVector* _y1 = (const ON_3dVector*)&y1;
    const ON_3dVector* _z1 = (const ON_3dVector*)&z1;
    rc = xf->ChangeBasis(*_x0, *_y0, *_z0, *_x1, *_y1, *_z1);
  }
  return rc;
}

RH_C_FUNCTION void ON_Xform_PlanarProjection(ON_Xform* xf, const ON_PLANE_STRUCT* plane)
{
  if( xf && plane )
  {
    ON_Plane temp = FromPlaneStruct(*plane);
    xf->PlanarProjection(temp);
  }
}

RH_C_FUNCTION void ON_Xform_Shear(ON_Xform* xf, 
                                  const ON_PLANE_STRUCT* plane, 
                                  ON_3DVECTOR_STRUCT x, 
                                  ON_3DVECTOR_STRUCT y, 
                                  ON_3DVECTOR_STRUCT z)
{
  if( xf && plane )
  {
    ON_Plane t_plane = FromPlaneStruct(*plane);
    const ON_3dVector* _x = (const ON_3dVector*)&x;
    const ON_3dVector* _y = (const ON_3dVector*)&y;
    const ON_3dVector* _z = (const ON_3dVector*)&z;

    xf->Shear(t_plane, *_x, *_y, *_z);
  }
}

RH_C_FUNCTION int ON_Xform_IsSimilarity(const ON_Xform* xf)
{
  int rc = 0;
  if( xf )
  {
    rc = xf->IsSimilarity();
  }
  return rc;
}

RH_C_FUNCTION double ON_Xform_Determinant(const ON_Xform* xf)
{
  double rc = 0.0;
  if( xf )
    rc = xf->Determinant();
  return rc;
}

RH_C_FUNCTION bool ON_Xform_Invert( ON_Xform* xf )
{
  bool rc = false;
  if( xf )
    rc = xf->Invert();
  return rc;
}

///////////////////////////////////////////////////////////
///////////////////////////////////////////////////////////
typedef void (CALLBACK* MORPHPOINTPROC)(ON_3DPOINT_STRUCT point, ON_3dPoint* out_point);

class CCustomSpaceMorph : public ON_SpaceMorph
{
public:
  CCustomSpaceMorph(double tolerance, bool quickpreview, bool preserveStructure, MORPHPOINTPROC callback)
  {
    SetTolerance(tolerance);
    SetQuickPreview(quickpreview);
    SetPreserveStructure(preserveStructure);
    m_callback = callback;
  }

  virtual ON_3dPoint MorphPoint( ON_3dPoint point ) const
  {
    ON_3dPoint rc = point;
    if( m_callback )
    {
      ON_3DPOINT_STRUCT* _pt = (ON_3DPOINT_STRUCT*)(&point);
      m_callback(*_pt, &rc);
    }
    return rc;
  }

private:
  MORPHPOINTPROC m_callback;
};

RH_C_FUNCTION bool ON_SpaceMorph_MorphGeometry(ON_Geometry* pGeometry, double tolerance, bool quickpreview, bool preserveStructure, MORPHPOINTPROC callback)
{
  bool rc = false;
  if( pGeometry && callback )
  {
    CCustomSpaceMorph sm(tolerance, quickpreview, preserveStructure, callback);
    rc = pGeometry->Morph(sm);
  }
  return rc;
}


////////////////////////////////////////////////////
#if defined(RHINO_V5SR) || defined(OPENNURBS_BUILD)

RH_C_FUNCTION bool ON_Matrix_Transpose(int rows, int cols, /*ARRAY*/double* values)
{
  bool rc = false;
  if(values)
  {
    ON_Matrix m;
    if( m.Create(rows, cols, &values, false) )
      rc = m.Transpose();
  }
  return rc;
}

RH_C_FUNCTION bool ON_Matrix_Swap(int rows, int cols, /*ARRAY*/double* values, bool swaprows, int a, int b)
{
  bool rc = false;
  if( values )
  {
    ON_Matrix m;
    if( m.Create(rows, cols, &values, false) )
    {
      if( swaprows )
        rc = m.SwapRows(a,b);
      else
        rc = m.SwapCols(a,b);
    }
  }
  return rc;
}

RH_C_FUNCTION bool ON_Matrix_Invert(int rows, int cols, /*ARRAY*/double* values, double zeroTolerance)
{
  bool rc = false;
  if(values)
  {
    ON_Matrix m;
    if( m.Create(rows, cols, &values, false) )
      rc = m.Invert(zeroTolerance);
  }
  return rc;
}

RH_C_FUNCTION void ON_Matrix_Multiply(int arows, int acols, /*ARRAY*/double* avalues, int brows, int bcols, /*ARRAY*/double* bvalues, /*ARRAY*/double* rcvalues)
{
  if( avalues && bvalues && rcvalues )
  {
    ON_Matrix a,b,rc;
    if( a.Create(arows, acols, &avalues, false) && b.Create(brows, bcols, &bvalues, false) && rc.Create(arows, bcols, &rcvalues, false) )
    {
      rc.Multiply(a,b);
    }
  }
}

RH_C_FUNCTION int ON_Matrix_RowReduce(int rows, int cols, /*ARRAY*/double* values, double zero_tol, double* determinant, double* pivot)
{
  int rc = 0;
  if( values && determinant && pivot )
  {
    ON_Matrix m;
    if( m.Create(rows, cols, &values, false) )
      rc = m.RowReduce(zero_tol, *determinant, *pivot);
  }
  return rc;
}

RH_C_FUNCTION int ON_Matrix_RowReduce2(int rows, int columns, /*ARRAY*/double* values, double zero_tol, /*ARRAY*/double* b, double* pivot)
{
  int rc = 0;
  if( values && b )
  {
    ON_Matrix m;
    if( m.Create(rows, columns, &values, false) )
      rc = m.RowReduce(zero_tol, b, pivot);
  }
  return rc;
}

RH_C_FUNCTION int ON_Matrix_RowReduce3(int rows, int columns, /*ARRAY*/double* values, double zero_tol, /*ARRAY*/ON_3dPoint* b, double* pivot)
{
  int rc = 0;
  if( values && b )
  {
    ON_Matrix m;
    if( m.Create(rows, columns, &values, false) )
      rc = m.RowReduce(zero_tol, b, pivot);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Matrix_BackSolve(int rows, int columns, /*ARRAY*/double* values, double zero_tol, int bSize, /*ARRAY*/const double* b, /*ARRAY*/double* x)
{
  bool rc = false;
  if( values && b && x )
  {
    ON_Matrix m;
    if( m.Create(rows, columns, &values, false) )
      rc = m.BackSolve(zero_tol, bSize, b, x);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Matrix_BackSolve2(int rows, int columns, /*ARRAY*/const double* values, double zero_tol, int bSize, /*ARRAY*/const ON_3dPoint* b, /*ARRAY*/ON_3dPoint* x)
{
  bool rc = false;
  if( values && b && x )
  {
    double* _values = const_cast<double*>(values);
    ON_Matrix m;
    if( m.Create(rows, columns, &_values, false) )
      rc = m.BackSolve(zero_tol, bSize, b, x);
  }
  return rc;
}

RH_C_FUNCTION bool ON_Matrix_GetBool(int rows, int columns, /*ARRAY*/const double* values, int which)
{
  const int idxIsRowOrthoganal = 0;
  const int idxIsRowOrthoNormal = 1;
  const int idxIsColumnOrthoganal = 2;
  const int idxIsColumnOrthoNormal = 3;
  bool rc = false;

  if( values )
  {
    double* _values = const_cast<double*>(values);
    ON_Matrix m;
    if( m.Create(rows, columns, &_values, false) )
    {
      switch (which)
      {
      case idxIsRowOrthoganal:
        rc = m.IsRowOrthoganal();
        break;
      case idxIsRowOrthoNormal:
        rc = m.IsRowOrthoNormal();
        break;
      case idxIsColumnOrthoganal:
        rc = m.IsColOrthoganal();
        break;
      case idxIsColumnOrthoNormal:
        rc = m.IsColOrthoNormal();
        break;
      default:
        break;
      }
    }
  }
  return rc;
}

#endif