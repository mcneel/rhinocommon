#pragma warning disable 1591
using System;
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.Collections
{
  /// <summary>
  /// Used by the TransformCommand and GetTransform classes.
  /// </summary>
  public class TransformObjectList : IDisposable
  {
    public TransformObjectList()
    {
      m_ptr = UnsafeNativeMethods.CRhinoXformObjectList_New();
    }

    internal TransformObjectList(Rhino.Input.Custom.GetTransform parent)
    {
      m_ptr = IntPtr.Zero;
      m_parent = parent;
    }

    #region IDisposable/Pointer handling
    Rhino.Input.Custom.GetTransform m_parent;
    IntPtr m_ptr;
    internal IntPtr ConstPointer()
    {
      if (m_parent != null)
      {
        IntPtr pConstParent = m_parent.ConstPointer();
        return UnsafeNativeMethods.CRhinoXformObjectList_PtrFromGetXform(pConstParent);
      }
      return m_ptr;
    }
    internal IntPtr NonConstPointer()
    {
      if (m_parent != null)
      {
        IntPtr pParent = m_parent.NonConstPointer();
        return UnsafeNativeMethods.CRhinoXformObjectList_PtrFromGetXform(pParent);
      }
      return m_ptr;
    }

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

    /// <summary>
    /// Gets the bounding box of all of the objects that this list contains.
    /// </summary>
    /// <param name="regularObjects"></param>
    /// <param name="grips"></param>
    /// <returns>
    /// Unset BoundingBox if this list is empty.
    /// </returns>
    public BoundingBox GetBoundingBox(bool regularObjects, bool grips)
    {
      BoundingBox rc = BoundingBox.Unset;
      IntPtr pConstThis = ConstPointer();
      UnsafeNativeMethods.CRhinoXformObjectList_BoundingBox(pConstThis, regularObjects, grips, ref rc);
      return rc;
    }

    public bool DisplayFeedbackEnabled
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoXformObjectList_DisplayFeedbackEnabled(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoXformObjectList_SetDisplayFeedback(pThis, value);
      }
    }

    public bool UpdateDisplayFeedbackTransform(Transform xform)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoXformObjectList_UpdateDisplayFeedbackTransform(pThis, ref xform);
    }
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
    /// Adds any objects you want transformed and grips you want transformed.
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

    //////////////////////////////////////////////////////////////////
    // Overridden members
    //void SetBasePoint( ON_3dPoint base_point, BOOL bShowDistanceInStatusBar = false );
    //void OnMouseMove( CRhinoViewport& vp, UINT nFlags, const ON_3dPoint& pt, const CPoint* p );
    //void DynamicDraw( HDC, CRhinoViewport& vp, const ON_3dPoint& pt );

    public bool HaveTransform
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoGetXform_HaveTransform(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGetXform_SetHaveTransform(pThis, value);
      }
    }
    public Transform Transform
    {
      get
      {
        Transform rc = Transform.Unset;
        IntPtr pConstThis = ConstPointer();
        UnsafeNativeMethods.CRhinoGetXform_Transform(pConstThis, ref rc);
        return rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGetXform_SetTransform(pThis, ref value);
      }
    }
    /*
  bool m_bMouseDrag; // true if transformation is from a mouse drag
  ON_3dPoint m_basepoint;
    */
    Rhino.Collections.TransformObjectList m_object_list;
    public Rhino.Collections.TransformObjectList ObjectList
    {
      get
      {
        return m_object_list ?? (m_object_list = new Collections.TransformObjectList(this));
      }
    }
    /*
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




    /// <summary>
    /// After setting up options and so on, call GetPoint::GetXform to get the Transformation.
    /// </summary>
    /// <returns></returns>
    [CLSCompliant(false)]
    public GetResult GetXform()
    {
      return GetXformHelper();
    }
  } 
}
#endif