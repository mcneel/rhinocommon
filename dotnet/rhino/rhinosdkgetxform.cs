using System;
using Rhino.Geometry;

namespace Rhino.Collections
{
  /// <summary>
  /// Used by the TransformCommand and GetTransform classes
  /// </summary>
  public class TransformObjectList : IDisposable
  {
    public TransformObjectList()
    {
      m_ptr = UnsafeNativeMethods.CRhinoXformObjectList_New();
    }

    #region IDisposable/Pointer handling
    IntPtr m_ptr;
    internal IntPtr ConstPointer() { return m_ptr; }
    internal IntPtr NonConstPointer() { return m_ptr; }

    ~TransformObjectList()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_ptr != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoXformObjectList_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }
    #endregion
  }
}

namespace Rhino.Input.Custom
{
  public abstract class GetTransform : GetPoint
  {
    public GetTransform() : base(true)
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoGetXform_New();
      Construct(ptr);
    }

    internal delegate int CalculateXformCallack(IntPtr pRhinoViewport, Point3d point, ref Transform xform);
    internal static int CustomCalcXform(IntPtr pRhinoViewport, Point3d point, ref Transform xform)
    {
      GetTransform active_gxform = m_active_gp as GetTransform;
      if (null == active_gxform)
        return 0;
      try
      {
        Rhino.Display.RhinoViewport viewport = new Display.RhinoViewport(null, pRhinoViewport);
        xform = active_gxform.CalculateTransform(viewport, point);
        return 1;
      }
      catch (Exception ex)
      {
        Runtime.HostUtils.ExceptionReport(ex);
      }
      return 0;
    }


    /// <summary>
    /// Add any objects you want transformed and grips you want transformed.
    /// Make sure no duplicates are in the list and that no grip ownwers are
    /// passed in as objects.
    /// </summary>
    /// <param name="list"></param>
    public void AddTransformObjects( Rhino.Collections.TransformObjectList list )
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstList = list.ConstPointer();
      UnsafeNativeMethods.CRhinoGetXform_AppendObjects( pThis, pConstList );
    }
    //void AppendObjects( const CRhinoGetObject& get );
    //void AppendObjects( CRhinoObjectIterator& it );
    //void AppendObject( const CRhinoObject* object );

    /// <summary>
    /// Override this virtual function to provide your own custom transformation method.
    /// Call this function to retrieve the final transformation.
    /// </summary>
    /// <param name="viewport"></param>
    /// <param name="point"></param>
    /// <returns></returns>
    public abstract Rhino.Geometry.Transform CalculateTransform( Rhino.Display.RhinoViewport viewport, Rhino.Geometry.Point3d point);

    // I think this can be handled in the Get() function in the base class
    //virtual CRhinoGet::result GetXform( CRhinoHistory* pHistory = NULL );

    /*
  //////////////////////////////////////////////////////////////////
  //
  // Overridden members
  //
  void SetBasePoint( ON_3dPoint base_point, BOOL bShowDistanceInStatusBar = false );
  void OnMouseMove( CRhinoViewport& vp, UINT nFlags, const ON_3dPoint& pt, const CPoint* p );
  void DynamicDraw( HDC, CRhinoViewport& vp, const ON_3dPoint& pt );

  BOOL m_bHaveXform;
  bool m_bMouseDrag; // true if transformation is from a mouse drag
  bool m_bReserved1;
  bool m_bReserved2;
  bool m_bReserved3;
  ON_Xform m_xform;
  ON_3dPoint m_basepoint;
  const CRhinoXformObjectList& ObjectList() const; // returns m_list.

  //////////////////////////////////////////////////////////////////
  //
  // Tools to support custom grip moving relative to the frame returned
  // by CRhinoGripObject::GetGripDirections()
  //

  Description:
    This is a utility function that can be called in CalculateTransform()
    if you want to transform grips relative to the frame returned by
    CRhinoGripObject::GetGripDirections().
  void SetGripFrameTransform( double x_scale, double y_scale, double z_scale );

  Description:
    If GetGripFrameTransform() returns true, then grips should be
    transformed by moving them in the translation retuned by
    GetGripTranslation().  If GetGripFrameTransform() returns false,
    then grips should be transformed by m_xform.
  bool HasGripFrameTransform() const;
  bool GetGripFrameTransform( double* x_scale, double* y_scale, double* z_scale ) const;
     */
  
  }
}