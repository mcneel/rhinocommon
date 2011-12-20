#pragma warning disable 1591
using System;
using Rhino.Geometry;


namespace Rhino.Display
{
  /// <summary>
  /// Different parallel and perspective projections that are "standard" in Rhino
  /// </summary>
  public enum DefinedViewportProjection : int
  {
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 3,
    Right = 4,
    Front = 5,
    Back = 6,
    Perspective = 7,
    TwoPointPerspective = 8
  }

#if RHINO_SDK
  /// <summary>
  /// Displays geometry with a given projection. In standard modeling views there
  /// is a one to one relationship between RhinoView and RhinoViewports. In a page
  /// layout, there may be multiple RhinoViewports for a single layout.
  /// </summary>
  public class RhinoViewport : IDisposable
  {
    RhinoView m_parent_view;
    IntPtr m_ptr;
    bool m_bDeletePtr; // = false; initialized by runtime

    #region constructors - pointer handling

    internal RhinoViewport(RhinoView parent_view, IntPtr ptr)
    {
      m_ptr = ptr;
      m_parent_view = parent_view;
    }

    readonly Rhino.DocObjects.DetailViewObject m_parent_detail;
    internal RhinoViewport(Rhino.DocObjects.DetailViewObject detail)
    {
      m_parent_detail = detail;
    }

    public RhinoViewport()
    {
      m_ptr = UnsafeNativeMethods.CRhinoViewport_New(IntPtr.Zero);
      m_bDeletePtr = true;
    }

    public RhinoViewport(RhinoViewport other)
    {
      IntPtr pOther = other.ConstPointer();
      m_ptr = UnsafeNativeMethods.CRhinoViewport_New(pOther);
      m_bDeletePtr = true;
    }

    internal void OnDetailCommit()
    {
      if (IntPtr.Zero != m_ptr && m_bDeletePtr)
      {
        UnsafeNativeMethods.CRhinoViewport_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
        m_bDeletePtr = false;
      }
    }

    ~RhinoViewport()
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
      if (IntPtr.Zero != m_ptr && m_bDeletePtr )
      {
        UnsafeNativeMethods.CRhinoViewport_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    internal IntPtr NonConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_parent_detail != null)
      {
        IntPtr pDetail = m_parent_detail.ConstPointer();
        m_ptr = UnsafeNativeMethods.CRhinoDetailViewObject_DuplicateViewport(pDetail);
        m_bDeletePtr = true;
      }
      return m_ptr;
    }

    internal IntPtr ConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_parent_detail != null)
      {
        IntPtr pDetail = m_parent_detail.ConstPointer();
        return UnsafeNativeMethods.CRhinoDetailViewObject_GetViewport(pDetail);
      }
      return m_ptr;
    }

    #endregion

    /// <summary>
    /// Gets the parent view, if there is one
    /// 
    /// Every RhinoView has an associated RhinoViewport that does all the 3d display work.
    /// Those associated viewports return the RhinoView as their parent view. However,
    /// RhinoViewports are used in other image creating contexts that do not have a parent
    /// RhinoView.  If you call ParentView, you MUST check for NULL return values.
    /// </summary>
    public RhinoView ParentView
    {
      get
      {
        if (null == m_parent_view)
        {
          IntPtr ptr = ConstPointer();
          IntPtr pParentView = UnsafeNativeMethods.CRhinoViewport_ParentView(ptr);
          m_parent_view = RhinoView.FromIntPtr(pParentView);
        }
        return m_parent_view;
      }
    }

    /// <summary>Unique id for this viewport</summary>
    public Guid Id
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_ViewportId(ptr);
      }
    }

    /// <summary>
    /// The value of change counter is incremented every time the view projection
    /// or construction plane changes. The user can the mouse and nestable view 
    /// manipulation commands to change a view at any time. The value of change
    /// counter can be used to detect these changes in code that is sensitive to
    /// the view projection.
    /// </summary>
    [CLSCompliant(false)]
    public uint ChangeCounter
    {
      get
      {
        IntPtr ptr = NonConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_ChangeCounter(ptr);
      }
    }

    /// <summary>
    /// returns true if some portion world coordinate bounding box is
    /// potentially visible in the viewing frustum.
    /// </summary>
    /// <param name="bbox"></param>
    /// <returns></returns>
    public bool IsVisible(BoundingBox bbox)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_IsVisible(ptr, bbox.Min, bbox.Max, true);
    }
    /// <summary>
    /// returns true if some portion world coordinate point is
    /// potentially visible in the viewing frustum.
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    public bool IsVisible(Point3d point)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_IsVisible(ptr, point, Point3d.Unset, false);
    }

    /// <summary>
    /// Set the size of the RhinoViewport
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    [Obsolete("Replaced by Size property to be consistent with .NET - will be removed in future WIP")]
    public void SetScreenSize(int width, int height)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetScreenSize(pThis, width, height);
    }

    /// <summary>
    /// Gets or sets the height and width of the viewport (in pixels)
    /// </summary>
    public System.Drawing.Size Size
    {
      get { return Bounds.Size; }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_SetScreenSize(pThis, value.Width, value.Height);
      }
    }

    /// <summary>
    /// Set optimal clipping planes to view objects in a world coordinate 3d bounding box
    /// </summary>
    /// <param name="box"></param>
    public void SetClippingPlanes(BoundingBox box)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetClippingPlanes(pThis, box.Min, box.Max);
    }

    /// <summary>name associated with this viewport</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public string Name
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pThis = NonConstPointer();
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.CRhinoViewport_GetSetName(pThis, null, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_GetSetName(ptr, value, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Viewport target point
    /// </summary>
    public Point3d CameraTarget
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Point3d target = new Point3d();
        UnsafeNativeMethods.CRhinoViewport_Target(pConstThis, ref target);
        return target;
      }
    }

    /// <summary>
    /// Set viewport target point. By default the camera location
    /// is translated so that the camera direction vector is parallel
    /// to the vector from the camera location to the target location.
    /// </summary>
    /// <param name="targetLocation">new target location</param>
    /// <param name="updateCameraLocation">
    /// if true, the camera location is translated so that the camera direction
    /// vector is parallel to the vector from the camera location to the target
    /// location.
    /// If false, the camera location is not changed.
    /// </param>
    /// <remarks>
    /// In general, Rhino users expect to have the camera direction vector and
    /// the vector from the camera location to the target location to be parallel.
    /// If you use the RhinoViewport functions to set the camera location, camera
    /// direction, and target point, then the relationship between these three
    /// points and vectors is automatically maintained.  If you directly manipulate
    /// the camera properties, then you should carefully set the target by calling
    /// SetTarget() with updateCameraLocation=false.
    /// </remarks>
    public void SetCameraTarget(Point3d targetLocation, bool updateCameraLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, targetLocation, updateCameraLocation, idxSetCameraTarget);
    }

    /// <summary>
    /// Set viewport camera location and target location. The camera direction vector is
    /// changed so that it is parallel to the vector from the camera location to
    /// the target location.
    /// </summary>
    /// <param name="targetLocation">new target location</param>
    /// <param name="cameraLocation">new camera location</param>
    public void SetCameraLocations(Point3d targetLocation, Point3d cameraLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraLocations(pThis, targetLocation, cameraLocation);
    }

    /// <summary>
    ///  Set viewport camera location. By default the target location is changed so that
    ///  the vector from the camera location to the target is parallel to the camera direction
    ///  vector.
    /// </summary>
    /// <param name="cameraLocation">new camera location</param>
    /// <param name="updateTargetLocation">
    /// if true, the target location is changed so that the vector from the camera
    /// location to the target is parallel to the camera direction vector.  
    /// If false, the target location is not changed. See the remarks section of
    /// RhinoViewport.SetTarget for important details.
    /// </param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void SetCameraLocation(Point3d cameraLocation, bool updateTargetLocation)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, cameraLocation, updateTargetLocation, idxSetCameraLocation);
    }

    /// <summary>
    /// Set viewport camera direction. By default the target location is changed so that
    /// the vector from the camera location to the target is parallel to the camera direction.
    /// </summary>
    /// <param name="cameraDirection">new camera direction</param>
    /// <param name="updateTargetLocation">
    /// if true, the target location is changed so that the vector from the camera
    /// location to the target is parallel to the camera direction.
    /// If false, the target location is not changed.
    /// See the remarks section of RhinoViewport.SetTarget for important details.
    /// </param>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void SetCameraDirection(Vector3d cameraDirection, bool updateTargetLocation)
    {
      IntPtr pThis = NonConstPointer();
      Point3d dirAsPoint = new Point3d(cameraDirection);
      UnsafeNativeMethods.CRhinoViewport_SetCameraTarget(pThis, dirAsPoint, updateTargetLocation, idxSetCameraDirection);
    }

    public BoundingBox GetCameraExtents(System.Collections.Generic.IEnumerable<Point3d> points)
    {
      Rhino.Collections.Point3dList _points = new Collections.Point3dList(points);
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstViewport = UnsafeNativeMethods.CRhinoViewport_VP(pConstThis);
      BoundingBox bbox = new BoundingBox();
      UnsafeNativeMethods.ON_Viewport_GetCameraExtents(pConstViewport, _points.Count, _points.m_items, ref bbox);
      return bbox;
    }

    /// <summary>
    /// Simple plane information for this viewport's construction plane. If you want
    /// detailed construction lpane information, use GetConstructionPlane
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public Plane ConstructionPlane()
    {
      IntPtr ptr = ConstPointer();
      Plane plane = new Plane();
      UnsafeNativeMethods.CRhinoViewport_ConstructionPlane(ptr, ref plane, false);
      return plane;
    }

    public Rhino.DocObjects.ConstructionPlane GetConstructionPlane()
    {
      IntPtr pConstThis = ConstPointer();
      IntPtr pConstuctionPlane = UnsafeNativeMethods.CRhinoViewport_GetConstructionPlane(pConstThis);
      return Rhino.DocObjects.ConstructionPlane.FromIntPtr(pConstuctionPlane);
    }

    public void SetConstructionPlane(Plane plane)
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_ConstructionPlane(ptr, ref plane, true);
    }

    /// <summary>
    /// Sets the construction plane to cplane.
    /// </summary>
    /// <param name="cplane"></param>
    public void SetConstructionPlane(DocObjects.ConstructionPlane cplane)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pCPlane = cplane.CopyToNative();
      if (pCPlane != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoViewport_SetConstructionPlane(pThis, pCPlane, false);
        UnsafeNativeMethods.ON_3dmConstructionPlane_Delete(pCPlane);
      }
    }

    /// <summary>
    /// Pushes the current construction plane on the viewport's
    /// construction plane stack and sets the construction plane
    /// to cplane.
    /// </summary>
    /// <param name="cplane"></param>
    public void PushConstructionPlane(DocObjects.ConstructionPlane cplane)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pCPlane = cplane.CopyToNative();
      if (pCPlane != IntPtr.Zero)
      {
        UnsafeNativeMethods.CRhinoViewport_SetConstructionPlane(pThis, pCPlane, true);
        UnsafeNativeMethods.ON_3dmConstructionPlane_Delete(pCPlane);
      }
    }

    const int idxPopConstructionPlane = 0;
    const int idxNextConstructionPlane = 1;
    const int idxPrevConstructionPlane = 2;

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to PushConstructionPlane.
    /// </summary>
    /// <returns>true if a construction plane was popped</returns>
    public bool PopConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxPopConstructionPlane);
    }

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to PreviousConstructionPlane.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool NextConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxNextConstructionPlane);
    }

    /// <summary>
    /// Sets the construction plane to the plane that was
    /// active before the last call to NextConstructionPlane
    /// or SetConstructionPlane.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool PreviousConstructionPlane()
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_GetBool(ptr, idxPrevConstructionPlane);
    }

    const int idxConstructionGridVisible = 0;
    const int idxConstructionAxesVisible = 1;
    const int idxWorldAxesVisible = 2;

    public bool ConstructionGridVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxConstructionGridVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxConstructionGridVisible, value);
      }
    }
    public bool ConstructionAxesVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxConstructionAxesVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxConstructionAxesVisible, value);
      }
    }
    public bool WorldAxesVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_View_GetBool(pConstThis, idxWorldAxesVisible);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_View_SetBool(pThis, idxWorldAxesVisible, value);
      }
    }

    public bool SetToPlanView(Point3d planeOrigin, Vector3d planeXaxis, Vector3d planeYaxis, bool setConstructionPlane)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_SetToPlanView(pThis, planeOrigin, planeXaxis, planeYaxis, setConstructionPlane);
    }

    /// <summary>
    /// Set viewport to a defined projection
    /// </summary>
    /// <param name="projection"></param>
    /// <param name="viewName">if not null or empty, the name is set</param>
    /// <param name="updateConstructionPlane">if true, the construction plane is set to the viewport plane</param>
    /// <returns>true if successful</returns>
    public bool SetProjection(DefinedViewportProjection projection, string viewName, bool updateConstructionPlane)
    {
      if (projection == DefinedViewportProjection.None)
        return false;
      IntPtr pThis = NonConstPointer();
      if (string.IsNullOrEmpty(viewName))
        viewName = null;
      return UnsafeNativeMethods.CRhinoViewport_SetProjection(pThis, (int)projection, viewName, updateConstructionPlane);
    }

    /// <summary>
    /// Appends the current view projection and target to the viewport's view stack.
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public void PushViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_PushViewProjection(pThis);
    }

    /// <summary>
    /// Sets the viewport camera projection
    /// </summary>
    /// <param name="projection"></param>
    /// <param name="updateTargetLocation">
    /// if true, the target location is changed so that the vector from the camera location to the target
    /// is parallel to the camera direction vector.  If false, the target location is not changed.
    /// </param>
    /// <returns>true on success</returns>
    public bool SetViewProjection(Rhino.DocObjects.ViewportInfo projection, bool updateTargetLocation)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pConstViewport = projection.ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_SetVP(pThis, pConstViewport, updateTargetLocation);
    }

    /// <summary>
    /// Sets the view projection and target to the settings at the top of
    /// the view stack and removes those settings from the view stack.
    /// </summary>
    /// <returns>true if there were settings that could be popped from the stack</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public bool PopViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_PopViewProjection(pThis);
    }

    public bool PushViewInfo(DocObjects.ViewInfo viewinfo, bool includeTraceImage)
    {      
      IntPtr pThis = NonConstPointer();
      IntPtr pConstView = viewinfo.ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_PushViewInfo(pThis, pConstView, includeTraceImage);
    }

    /// <summary>
    /// Sets the view projection and target to the settings that 
    /// were active before the last call to PrevView.
    /// </summary>
    /// <returns>true if the view stack was popped</returns>
    public bool NextViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_NextPrevViewProjection(pThis, true);
    }

    /// <summary>
    /// Sets the view projection and target to the settings that
    /// were active before the last call to NextViewProjection.
    /// </summary>
    /// <returns>true if the view stack was popped</returns>
    public bool PreviousViewProjection()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_NextPrevViewProjection(pThis, false);
    }

    //[skipping]
    //void ClearUndoInformation( bool bClearProjections = true, bool bClearCPlanes = true );

    /// <summary>
    /// true if construction plane z axis is parallel to camera direction
    /// </summary>
    public bool IsPlanView
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_IsPlanView(pConstThis);
      }
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains all of the
    /// selected document objects that can be seen in view. If the projection is
    /// perspective, the camera angle is not changed.
    /// </summary>
    /// <returns>true if successful</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addlayout.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addlayout.cs' lang='cs'/>
    /// <code source='examples\py\ex_addlayout.py' lang='py'/>
    /// </example>
    public bool ZoomExtents()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoDollyExtents(pThis, false);
    }

    /// <summary>
    /// Dolly the camera location and so that the view frustum contains all of the
    /// selected document objects that can be seen in view. If the projection is
    /// perspective, the camera angle is not changed.
    /// </summary>
    /// <returns>true if successful</returns>
    public bool ZoomExtentsSelected()
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhinoDollyExtents(pThis, true);
    }

    /// <summary>
    /// Zooms the viewport to the given bounding box
    /// </summary>
    /// <param name="box"></param>
    /// <returns>true if successful</returns>
    public bool ZoomBoundingBox(BoundingBox box)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.RHC_RhZoomExtentsHelper(pThis, box.m_min, box.m_max);
    }

    #region mouse
    const int idxMouseRotateView = 0;
    const int idxMouseRotateCamera = 1;
    const int idxMouseInOutDolly = 2;
    const int idxMouseMagnify = 3;
    const int idxMouseTilt = 4;
    const int idxMouseDollyZoom = 5;

    /// <summary>
    /// Rotate viewport around target
    /// </summary>
    /// <param name="mousePreviousPoint"></param>
    /// <param name="mouseCurrentPoint"></param>
    public bool MouseRotateAroundTarget(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseRotateView, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    /// <summary>
    /// Rotate view around camera location
    /// </summary>
    /// <param name="mousePreviousPoint"></param>
    /// <param name="mouseCurrentPoint"></param>
    public bool MouseRotateCamera(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseRotateCamera, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    public bool MouseInOutDolly(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseInOutDolly, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    public bool MouseMagnify(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseMagnify, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    public bool MouseTilt(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseTilt, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    //[skipping]
    //bool MouseAdjustLensLength( const CPoint& mouse0, const CPoint& mouse1, bool bMoveTarget = false);

    public bool MouseDollyZoom(System.Drawing.Point mousePreviousPoint, System.Drawing.Point mouseCurrentPoint)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_MouseAdjust(pThis, idxMouseDollyZoom, mousePreviousPoint.X, mousePreviousPoint.Y, mouseCurrentPoint.X, mouseCurrentPoint.Y);
    }

    //[skipping]
    //ON_3dVector MouseTrackballVector( int mousex, int mousey ) const;
    #endregion

    /// <summary>
    /// Keyboard arrow key interaction tool
    /// </summary>
    /// <param name="leftRight">left/right rotate if true, up/down rotate if false</param>
    /// <param name="angleRadians">
    /// If less than 0, rotation is to left or down.
    /// If greater than 0, rotation is to right or up
    /// </param>
    /// <returns></returns>
    public bool KeyboardRotate(bool leftRight, double angleRadians)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_KeyboardRotate(pThis, leftRight, angleRadians);
    }

    /// <summary>
    /// Keyboard arrow key interaction tool
    /// </summary>
    /// <param name="leftRight">left/right dolly if true, up/down dolly if false</param>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool KeyboardDolly(bool leftRight, double amount)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_KeyboardDolly(pThis, leftRight, amount);
    }

    /// <summary>
    /// Keyboard arrow key interaction tool
    /// </summary>
    /// <param name="amount"></param>
    /// <returns></returns>
    public bool KeyboardDollyInOut(double amount)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_KeyboardDollyInOut(pThis, amount);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="magnificationFactor"></param>
    /// <param name="mode">
    /// false = perform a "dolly" magnification by moving the camera towards/away from
    /// the target so that the amount of the screen subtended by an object changes.
    /// true = perform a "zoom" magnification by adjusting the "lens" angle           
    /// </param>
    /// <returns></returns>
    public bool Magnify(double magnificationFactor, bool mode)
    {
      System.Drawing.Point pt = new System.Drawing.Point(-1, -1);
      return Magnify(magnificationFactor, mode, pt);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="magnificationFactor"></param>
    /// <param name="mode">
    /// false = perform a "dolly" magnification by moving the camera towards/away from
    /// the target so that the amount of the screen subtended by an object changes.
    /// true = perform a "zoom" magnification by adjusting the "lens" angle           
    /// </param>
    /// <param name="fixedScreenPoint"></param>
    /// <returns></returns>
    public bool Magnify(double magnificationFactor, bool mode, System.Drawing.Point fixedScreenPoint)
    {
      int _mode = mode ? 1 : 0;
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_Magnify(pThis, magnificationFactor, _mode, fixedScreenPoint.X, fixedScreenPoint.Y);
    }

    // [skipping]
    //  void SetModelXform( const ON_Xform& model_xform );
    //  void GetModelXform( ON_Xform& model_xform ) const;
    //  void SetDisplayXform( const ON_Xform& display_xform );
    //  void GetDisplayXform( ON_Xform& display_xform ) const;
    //  void SetMarkedObjectXform( int mark_value, const ON_Xform& marked_object_xform );
    //  void GetMarkedObjectXform( int* mark_value, ON_Xform& marked_object_xform  ) const;

    /// <summary>
    /// Takes a rectangle in screen coordinates and returns a transformation
    /// that maps the 3d frustum defined by the rectangle to a -1/+1 clipping
    /// coordinate box. This takes a single point and inflates it by
    /// Rhino.ApplicationSettings.ModelAidSettings.MousePickBoxRadius to define
    /// the screen rectangle
    /// </summary>
    /// <param name="clientX"></param>
    /// <param name="clientY"></param>
    /// <returns></returns>
    public Transform GetPickTransform(int clientX, int clientY)
    {
      IntPtr pConstThis = ConstPointer();
      Transform rc = Transform.Unset;
      UnsafeNativeMethods.CRhinoViewport_GetPickXform(pConstThis, clientX, clientY, ref rc);
      return rc;
    }

    /// <summary>
    /// Takes a rectangle in screen coordinates and returns a transformation
    /// that maps the 3d frustum defined by the rectangle to a -1/+1 clipping
    /// coordinate box. This takes a single point and inflates it by
    /// Rhino.ApplicationSettings.ModelAidSettings.MousePickBoxRadius to define
    /// the screen rectangle
    /// </summary>
    /// <param name="clientPoint"></param>
    /// <returns></returns>
    public Transform GetPickTransform(System.Drawing.Point clientPoint)
    {
      return GetPickTransform(clientPoint.X, clientPoint.Y);
    }
     

    /// <summary>
    /// Takes a rectangle in screen coordinates and returns a transformation
    /// that maps the 3d frustum defined by the rectangle to a -1/+1 clipping
    /// coordinate box.
    /// </summary>
    /// <param name="clientRectangle"></param>
    /// <returns></returns>
    public Transform GetPickTransform(System.Drawing.Rectangle clientRectangle)
    {
      IntPtr pConstThis = ConstPointer();
      Transform rc = Transform.Unset;
      UnsafeNativeMethods.CRhinoViewport_GetPickXform2(pConstThis, clientRectangle.Left, clientRectangle.Top, clientRectangle.Right, clientRectangle.Bottom, ref rc);
      return rc;
    }

    //  CRhinoDisplayPipeline* DisplayPipeline(void) const;

    /// <summary>
    /// Convert a point in parent RhinoView client window coordinates to the Viewport screen port
    /// client coordinates. The screen port of a RhinoViewport may not match the client area of
    /// the parent RhinoView. This occurs in cases when the RhinoViewport is a nested child viewport
    /// </summary>
    /// <param name="point">
    /// point in client coordinates of parent RhinoView window as input. This is
    /// converted to the screen port client coordinates of the viewport as output
    /// </param>
    /// <returns>
    /// true if the point is inside of the RhinoViewport's screen port rectangle
    /// </returns>
    [Obsolete("use ClientToScreen instead - this will be removed in a future WIP")]
    public bool ClientToScreenPort(ref System.Drawing.Point point)
    {
      IntPtr pConstThis = ConstPointer();
      int x = point.X;
      int y = point.Y;
      bool rc = UnsafeNativeMethods.CRhinoViewport_ClientToScreenPort(pConstThis, ref x, ref y);
      point = new System.Drawing.Point(x,y);
      return rc;
    }

    #region Wrappers for ON_Viewport

    // from ON_Geometry
    public BoundingBox GetFrustumBoundingBox()
    {
      Point3d a = new Point3d();
      Point3d b = new Point3d();
      IntPtr pConstThis = ConstPointer();
      if( !UnsafeNativeMethods.CRhinoViewport_GetBBox(pConstThis, ref a, ref b) )
        return BoundingBox.Unset;
      return new BoundingBox(a, b);
    }

    /// <summary>
    /// Rotates about the specified axis. A positive rotation angle results
    /// in a counter-clockwise rotation about the axis (right hand rule).
    /// </summary>
    /// <param name="angleRadians">angle of rotation in radians</param>
    /// <param name="rotationAxis">direction of the axis of rotation</param>
    /// <param name="rotationCenter">point on the axis of rotation</param>
    /// <returns>true if geometry successfully rotated</returns>
    public bool Rotate(double angleRadians, Vector3d rotationAxis, Point3d rotationCenter)
    {
      IntPtr pConstThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_Rotate(pConstThis, angleRadians, rotationAxis, rotationCenter);
    }


    const int idxIsValidCamera = 0;
    const int idxIsValidFrustum = 1;
    const int idxIsPerspectiveProjection = 2;
    const int idxIsTwoPointPerspectiveProjection = 3;
    const int idxIsParallelProjection = 4;
    public bool IsValidCamera
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsValidCamera);
      }
    }
    public bool IsValidFrustum
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsValidFrustum);
      }
    }

    public bool IsPerspectiveProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsPerspectiveProjection);
      }
    }
#if USING_V5_SDK
    public bool IsTwoPointPerspectiveProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsTwoPointPerspectiveProjection);
      }
    }
#endif
    public bool IsParallelProjection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetBool(ptr, idxIsParallelProjection);
      }
    }

#if USING_V5_SDK
    /// <summary>
    /// Use this function to change projections of valid viewports from persective to parallel.
    /// It will make common additional adjustments to the frustum so the resulting views are
    /// similar. The camera location and direction will not be changed.
    /// </summary>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <returns>
    /// If the current projection is parallel and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToParallelProjection(bool symmetricFrustum)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_ChangeToParallelProjection(pThis, symmetricFrustum);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports from parallel to perspective.
    /// It will make common additional adjustments to the frustum and camera location so the
    /// resulting views are similar. The camera direction and target point are not be changed.
    /// </summary>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <param name="lensLength">
    /// (pass 50.0 when in doubt) 35 mm lens length to use when changing from parallel to perspective
    /// projections. If the current projection is perspective or lens_length is &lt;= 0.0, then
    /// this parameter is ignored.
    /// </param>
    /// <returns>
    /// If the current projection is perspective and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToPerspectiveProjection(bool symmetricFrustum, double lensLength)
    {
      return ChangeToPerspectiveProjection(RhinoMath.UnsetValue, symmetricFrustum, lensLength);
    }

    /// <summary>
    /// Use this function to change projections of valid viewports from parallel to perspective.
    /// It will make common additional adjustments to the frustum and camera location so the
    /// resulting views are similar. The camera direction and target point are not be changed.
    /// </summary>
    /// <param name="targetDistance">
    /// If RhinoMath.UnsetValue this parameter is ignored. Otherwise it must be > 0 and indicates
    /// which plane in the current view frustum should be perserved.
    /// </param>
    /// <param name="symmetricFrustum">True if you want the resulting frustum to be symmetric.</param>
    /// <param name="lensLength">
    /// (pass 50.0 when in doubt) 35 mm lens length to use when changing from parallel to perspective
    /// projections. If the current projection is perspective or lens_length is &lt;= 0.0, then
    /// this parameter is ignored.
    /// </param>
    /// <returns>
    /// If the current projection is perspective and bSymmetricFrustum, FrustumIsLeftRightSymmetric()
    /// and FrustumIsTopBottomSymmetric() are all equal, then no changes are made and true is returned.
    /// </returns>
    public bool ChangeToPerspectiveProjection(double targetDistance, bool symmetricFrustum, double lensLength)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_ChangeToPerspectiveProjection(pThis, targetDistance, symmetricFrustum, lensLength);
    }
#endif
    const int idxCameraLocation = 0;
    const int idxCameraDirection = 1;
    const int idxCameraUp = 2;
    const int idxCameraX = 3;
    const int idxCameraY = 4;
    const int idxCameraZ = 5;

    public Point3d CameraLocation
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraLocation, ref v);
        return new Point3d(v);
      }
    }
    public Vector3d CameraDirection
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraDirection, ref v);
        return v;
      }
    }
    /// <example>
    /// <code source='examples\vbnet\ex_addnamedview.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addnamedview.cs' lang='cs'/>
    /// <code source='examples\py\ex_addnamedview.py' lang='py'/>
    /// </example>
    public Vector3d CameraUp
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraUp, ref v);
        return v;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_VP_SetVector(pThis, idxCameraUp, value);
      }
    }

    /// <summary>
    /// Returns true if current camera orientation is valid
    /// </summary>
    /// <param name="frame"></param>
    /// <returns></returns>
    public bool GetCameraFrame(out Plane frame)
    {
      frame = new Plane();
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetCameraFrame(pConstThis, ref frame);
    }

    /// <summary>unit to right vector</summary>
    public Vector3d CameraX
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraX, ref v);
        return v;
      }
    }
    /// <summary>unit up vector</summary>
    public Vector3d CameraY
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraY, ref v);
        return v;
      }
    }
    /// <summary>unit vector in CameraDirection</summary>
    public Vector3d CameraZ
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Vector3d v = new Vector3d();
        UnsafeNativeMethods.CRhinoViewport_VP_GetVector(ptr, idxCameraZ, ref v);
        return v;
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="left">left &lt; right</param>
    /// <param name="right">left &lt; right</param>
    /// <param name="bottom">bottom &lt; top</param>
    /// <param name="top">bottom &lt; top</param>
    /// <param name="nearDistance">0 &lt; nearDistance &lt; farDistance</param>
    /// <param name="farDistance">0 &lt; nearDistance &lt; farDistance</param>
    /// <returns></returns>
    public bool GetFrustum(out double left, out double right, out double bottom, out double top, out double nearDistance, out double farDistance)
    {
      double[] items = new double[6];
      IntPtr ptr = ConstPointer();
      bool rc = UnsafeNativeMethods.CRhinoViewport_VP_GetFrustum(ptr, ref items[0]);
      left = items[0];
      right = items[1];
      bottom = items[2];
      top = items[3];
      nearDistance = items[4];
      farDistance = items[5];
      return rc;
    }

    const int idxFrustumAspect = 0;
    const int idxScreenPortAspect = 1;
    const int idxCamera35mmLensLength = 2;

    /// <summary>frustum's width/height</summary>
    public double FrustumAspect
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxFrustumAspect);
      }
    }

    /// <summary>
    /// Returns world coordinates of frustum's center
    /// </summary>
    /// <param name="center"></param>
    /// <returns>true if the center was successfully computed</returns>
    public bool GetFrustumCenter(out Point3d center)
    {
      center = new Point3d();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetFrustumCenter(ptr, ref center);
    }
    
    /// <summary>Get clipping distance of a point</summary>
    /// <param name="point"></param>
    /// <param name="distance"></param>
    /// <returns>
    /// True if the point is ing the view frustum and near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(Point3d point, out double distance)
    {
      IntPtr ptr = ConstPointer();
      distance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth1(ptr, point, ref distance);
    }
    /// <summary>
    /// Gets near and far clipping distances of a bounding box
    /// </summary>
    /// <param name="bbox"></param>
    /// <param name="nearDistance"></param>
    /// <param name="farDistance"></param>
    /// <returns>
    /// True if the bounding box intersects the view frustum and near_dist/far_dist were set.
    /// False if the bounding box does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(BoundingBox bbox, out double nearDistance, out double farDistance)
    {
      IntPtr ptr = ConstPointer();
      nearDistance = farDistance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth2(ptr, bbox.m_min, bbox.m_max, ref nearDistance, ref farDistance);
    }
    /// <summary>
    /// Gets near and far clipping distances of a sphere
    /// </summary>
    /// <param name="sphere"></param>
    /// <param name="nearDistance"></param>
    /// <param name="farDistance"></param>
    /// <returns>
    /// True if the sphere intersects the view frustum and near_dist/far_dist were set.
    /// False if the sphere does not intesect the view frustum.
    /// </returns>
    public bool GetDepth(Sphere sphere, out double nearDistance, out double farDistance)
    {
      IntPtr ptr = ConstPointer();
      nearDistance = farDistance = 0;
      return UnsafeNativeMethods.CRhinoViewport_VP_GetDepth3(ptr, ref sphere, ref nearDistance, ref farDistance);
    }

    const int idxGetFrustumNearPlane = 0;
    const int idxGetFrustumFarPlane = 1;
    const int idxGetFrustumLeftPlane = 2;
    const int idxGetFrustumRightPlane = 3;
    const int idxGetFrustumBottomPlane = 4;
    const int idxGetFrustumTopPlane = 5;

    /// <summary>Get near clipping plane</summary>
    /// <param name="plane">
    /// near clipping plane if camera and frustum are valid. The plane's
    /// frame is the same as the camera's frame. The origin is located at
    /// the intersection of the camera direction ray and the near clipping
    /// plane.
    /// </param>
    /// <returns>
    /// true if camera and frustum are valid
    /// </returns>
    public bool GetFrustumNearPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumNearPlane, ref plane);
    }
    /// <summary>Get far clipping plane</summary>
    /// <param name="plane">
    /// far clipping plane if camera and frustum are valid. The plane's
    /// frame is the same as the camera's frame. The origin is located at
    /// the intersection of the camera direction ray and the far clipping
    /// plane.
    /// </param>
    /// <returns>
    /// true if camera and frustum are valid
    /// </returns>
    public bool GetFrustumFarPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumFarPlane, ref plane);
    }
    /// <summary>Get left world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum left side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumLeftPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumLeftPlane, ref plane);
    }
    /// <summary>Get right world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum right side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumRightPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumRightPlane, ref plane);
    }
    /// <summary>Get bottom world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum bottom side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumBottomPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumBottomPlane, ref plane);
    }
    /// <summary>Get top world frustum clipping plane</summary>
    /// <param name="plane">
    /// frustum top side clipping plane. The normal points into the visible
    /// region of the frustum. If the projection is perspective, the origin
    /// is at the camera location, otherwise the origin isthe point on the
    /// plane that is closest to the camera location.
    /// </param>
    /// <returns>True if camera and frustum are valid and plane was set</returns>
    public bool GetFrustumTopPlane(out Plane plane)
    {
      IntPtr ptr = ConstPointer();
      plane = new Plane();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetPlane(ptr, idxGetFrustumTopPlane, ref plane);
    }

    /// <summary>Get corners of near clipping plane rectangle</summary>
    /// <returns>
    /// [left_bottom, right_bottom, left_top, right_top] points on success
    /// null on failure
    /// </returns>
    public Point3d[] GetNearRect()
    {
      Point3d[] rc = new Point3d[4];
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetRect(ptr, true, rc))
        rc = null;
      return rc;
    }
    /// <summary>Get corners of far clipping plane rectangle</summary>
    /// <returns>
    /// [left_bottom, right_bottom, left_top, right_top] points on success
    /// null on failure
    /// </returns>
    public Point3d[] GetFarRect()
    {
      Point3d[] rc = new Point3d[4];
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetRect(ptr, false, rc))
        rc = null;
      return rc;
    }

    /// <summary>
    /// Location of viewport in pixels.  These are provided so you can set the port you are using
    /// and get the appropriate transformations to and from screen space.
    /// </summary>
    /// <param name="portLeft">portLeft != portRight</param>
    /// <param name="portRight">portLeft != portRight</param>
    /// <param name="portBottom">portTop != portBottom</param>
    /// <param name="portTop">portTop != portBottom</param>
    /// <param name="portNear"></param>
    /// <param name="portFar"></param>
    /// <returns></returns>
    public bool GetScreenPort(out int portLeft, out int portRight, out int portBottom, out int portTop, out int portNear, out int portFar)
    {
      IntPtr ptr = ConstPointer();
      int[] items = new int[6];
      bool rc = UnsafeNativeMethods.CRhinoViewport_VP_GetScreenPort(ptr, items);
      portLeft = items[0];
      portRight = items[1];
      portBottom = items[2];
      portTop = items[3];
      portNear = items[4];
      portFar = items[5];
      return rc;
    }

    [Obsolete("Replaced by Bounds to be consistent with .NET - will be removed in a future WIP")]
    public System.Drawing.Rectangle ScreenPortBounds
    {
      get
      {
        int l, r, t, b, n, f;
        GetScreenPort(out l, out r, out b, out t, out n, out f);
        return System.Drawing.Rectangle.FromLTRB(l, t, r, b);
      }
    }


    /// <summary>
    /// Gets the size and location of the viewport, in pixels, relative to the parent view
    /// </summary>
    public System.Drawing.Rectangle Bounds
    {
      get
      {
        int l, r, t, b, n, f;
        GetScreenPort(out l, out r, out b, out t, out n, out f);
        return System.Drawing.Rectangle.FromLTRB(l, t, r, b);
      }
    }


    /// <summary>
    /// screen port's width/height
    /// </summary>
    public double ScreenPortAspect
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxScreenPortAspect);
      }
    }

    public bool GetCameraAngle(out double halfDiagonalAngle, out double halfVerticalAngle, out double halfHorizontalAngle)
    {
      halfDiagonalAngle = halfVerticalAngle = halfHorizontalAngle = 0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetCameraAngle(ptr, ref halfDiagonalAngle, ref halfVerticalAngle, ref halfHorizontalAngle);
    }

    public double Camera35mmLensLength
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_VP_GetDouble(ptr, idxCamera35mmLensLength);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoViewport_VP_SetCamera35mmLensLength(pThis, value);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sourceSystem"></param>
    /// <param name="destinationSystem"></param>
    /// <returns>
    /// 4x4 transformation matrix (acts on the left)
    /// Identity matrix is returned if this function fails
    /// </returns>
    public Transform GetTransform(Rhino.DocObjects.CoordinateSystem sourceSystem, Rhino.DocObjects.CoordinateSystem destinationSystem)
    {
      Transform matrix = new Transform();
      IntPtr ptr = ConstPointer();
      if (!UnsafeNativeMethods.CRhinoViewport_VP_GetXform(ptr, (int)sourceSystem, (int)destinationSystem, ref matrix))
        return Transform.Identity;
      return matrix;
    }

    /// <summary>
    /// Gets the world coordinate line in the view frustum that projects to a point on the screen.
    /// </summary>
    /// <param name="screenX">(screenx,screeny) = screen location</param>
    /// <param name="screenY">(screenx,screeny) = screen location</param>
    /// <param name="worldLine">
    /// 3d world coordinate line segment starting on the near clipping
    /// plane and ending on the far clipping plane.
    /// </param>
    /// <returns>
    /// true if successful.
    /// false if view projection or frustum is invalid.
    /// </returns>
    public bool GetFrustumLine(double screenX, double screenY, out Line worldLine)
    {
      worldLine = new Line();
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetFrustumLine(ptr, screenX, screenY, ref worldLine);
    }

    /// <summary>
    /// </summary>
    /// <param name="pointInFrustum"></param>
    /// <param name="pixelsPerUnit">
    /// scale = number of pixels per world unit at the 3d point
    /// </param>
    /// <returns></returns>
    public bool GetWorldToScreenScale(Point3d pointInFrustum, out double pixelsPerUnit)
    {
      pixelsPerUnit = 0;
      IntPtr ptr = ConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_VP_GetWorldToScreenScale(ptr, pointInFrustum, ref pixelsPerUnit);
    }

    /// <summary>
    /// Convert a point from world coordinates in the viewport to a 2d screen
    /// point in the local coordinates of the viewport (X/Y of point is relative
    /// to top left corner of viewport on screen)
    /// </summary>
    /// <param name="worldPoint"></param>
    /// <returns></returns>
    public Point2d WorldToClient(Point3d worldPoint)
    {
      Transform xform = GetTransform(DocObjects.CoordinateSystem.World, DocObjects.CoordinateSystem.Screen);
      Point3d screen_point = xform * worldPoint;
      return new Point2d(screen_point.X, screen_point.Y);
    }

    public System.Drawing.Point ClientToScreen(Point2d clientPoint)
    {
      System.Drawing.Point _point = new System.Drawing.Point();
      _point.X = (int)clientPoint.X;
      _point.Y = (int)clientPoint.Y;
      return ClientToScreen(_point);
    }

    public System.Drawing.Point ClientToScreen(System.Drawing.Point clientPoint)
    {
      var bounds = Bounds;
      System.Drawing.Point rc = new System.Drawing.Point();
      rc.X = clientPoint.X - bounds.Left;
      rc.Y = clientPoint.Y - bounds.Top;
      var parent = this.ParentView;
      if (parent != null)
        rc = parent.ClientToScreen(rc);
      return rc;
    }

    public System.Drawing.Point ScreenToClient(System.Drawing.Point screenPoint)
    {
      System.Drawing.Point rc = screenPoint;
      var parent = this.ParentView;
      if (parent != null)
        rc = parent.ScreenToClient(rc);
      var bounds = Bounds;
      rc.X = rc.X + bounds.Left;
      rc.Y = rc.Y + bounds.Top;
      return rc;
    }

    public Line ClientToWorld(System.Drawing.Point clientPoint)
    {
      Point2d pt = new Point2d(clientPoint.X, clientPoint.Y);
      return ClientToWorld(pt);
    }
    public Line ClientToWorld(Point2d clientPoint)
    {
      Line rc;
      if( GetFrustumLine(clientPoint.X, clientPoint.Y, out rc) )
        return rc;
      return Line.Unset;
    }
    #endregion









    const int idxSetCameraTarget = 0;
    const int idxSetCameraDirection = 1;
    const int idxSetCameraLocation = 2;




    public string WallpaperFilename
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          IntPtr pConstThis = ConstPointer();
          UnsafeNativeMethods.CRhinoViewport_GetWallpaperFilename(pConstThis, pString);
          return sh.ToString();
        }
      }
    }
    public bool WallpaperGrayscale
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_GetWallpaperBool(pConstThis, true);
      }
    }
    public bool WallpaperVisible
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoViewport_GetWallpaperBool(pConstThis, false);
      }
    }

    public bool SetWallpaper(string imageFilename, bool grayscale)
    {
      return SetWallpaper(imageFilename, grayscale, true);
    }
    public bool SetWallpaper(string imageFilename, bool grayscale, bool visible)
    {
      IntPtr pThis = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_SetWallpaper(pThis, imageFilename, grayscale, visible);
    }


    /// <summary>
    /// Remove trace image (background bitmap) for this viewport if one exists
    /// </summary>
    public void ClearTraceImage()
    {
      IntPtr ptr = NonConstPointer();
      UnsafeNativeMethods.CRhinoViewport_ClearTraceImage(ptr);
    }

    /// <summary>
    /// Set trace image (background bitmap) for this viewport
    /// </summary>
    /// <param name="bitmapFileName"></param>
    /// <param name="plane"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="grayscale"></param>
    /// <param name="filtered">true if image should be filtered (bilinear) before displayed</param>
    /// <returns>true if successful</returns>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public bool SetTraceImage(string bitmapFileName, Plane plane, double width, double height, bool grayscale, bool filtered)
    {
      IntPtr ptr = NonConstPointer();
      return UnsafeNativeMethods.CRhinoViewport_SetTraceImage(ptr, bitmapFileName, ref plane, width, height, grayscale, filtered);
    }

    public ViewportType ViewportType
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.CRhinoViewport_ViewportType(pConstThis);
        return (ViewportType)rc;
      }
    }

    public Rhino.Display.DisplayModeDescription DisplayMode
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        Guid id = UnsafeNativeMethods.CRhinoViewport_DisplayModeId(pConstThis);
        return Rhino.Display.DisplayModeDescription.GetDisplayMode(id);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        Guid id = value.Id;
        UnsafeNativeMethods.CRhinoViewport_SetDisplayMode(pThis, id);
      }
    }
  }
#endif

  public enum ViewportType : int
  {
    StandardModelingViewport = 0,
    PageViewMainViewport = 1,
    DetailViewport = 2
  }
}
