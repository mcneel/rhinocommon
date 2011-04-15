using System;
using System.Collections.Generic;
//public struct GripDirections { }
//public struct GripStatus { }

namespace Rhino.Display
{
  public class GripsDrawEventArgs : DrawEventArgs
  {
    internal IntPtr m_pGripsDrawSettings;

    internal GripsDrawEventArgs(IntPtr pGripsDrawSettings)
    {
      m_pGripsDrawSettings = pGripsDrawSettings;
      IntPtr pDisplayPipeline = UnsafeNativeMethods.CRhinoDrawGripsSettings_DisplayPipelinePtr(pGripsDrawSettings);
      m_dp = new DisplayPipeline(pDisplayPipeline);
    }

    /// <summary>
    /// If true, then draw stuff that moves when grips are dragged,
    /// like the curve being bent by a dragged control point.
    /// </summary>
    public bool DrawStaticStuff
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDrawGripSettings_GetBool(m_pGripsDrawSettings, true);
      }
    }

    /// <summary>
    /// If true, then draw stuff that does not move when grips are
    /// dragged, like the control polygon of the "original" curve.
    /// </summary>
    public bool DrawDynamicStuff
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDrawGripSettings_GetBool(m_pGripsDrawSettings, false);
      }
    }

    //skipping m_cp_grip_style

    const int idxCpGripLineStyle = 0;
    const int idxGripStatusCount = 1;

    /// <summary>
    /// What kind of line is used to display things like control polygons.
    /// 0 = no control polygon,  1 = solid control polygon,  2 = dotted control polygon
    /// </summary>
    public int ControlPolygonStyle
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDrawGripSettings_GetInt(m_pGripsDrawSettings, idxCpGripLineStyle);
      }
    }

    public int GripStatusCount
    {
      get
      {
        return UnsafeNativeMethods.CRhinoDrawGripSettings_GetInt(m_pGripsDrawSettings, idxGripStatusCount);
      }
    }
  }
}
//public class ObjectGrips { }
//public class CopyGripsHelper{}
//public class KeepKinkySurfaces{}

namespace Rhino.DocObjects.Custom
{
  public delegate void TurnOnGripsEventHandler(Rhino.DocObjects.RhinoObject rhObj);

  [Obsolete("CustomObjectGrips are under development and should not be used in a shipping plug-in")]
  public abstract class CustomObjectGrips : IDisposable
  {
    #region statics
    static int m_serial_number_counter = 1;
    static List<CustomObjectGrips> m_all_custom_grips = new List<CustomObjectGrips>();
    static Dictionary<Guid, TurnOnGripsEventHandler> m_registered_enablers = new Dictionary<Guid, TurnOnGripsEventHandler>();
    static CustomObjectGrips FromSerialNumber(int serial_number)
    {
      for (int i = 0; i < m_all_custom_grips.Count; i++)
      {
        CustomObjectGrips grips = m_all_custom_grips[i];
        if (grips.m_runtime_serial_number == serial_number)
          return grips;
      }
      return null;
    }

    public static void RegisterGripsEnabler(TurnOnGripsEventHandler enabler, Type customGripsType)
    {
      if (!customGripsType.IsSubclassOf(typeof(CustomObjectGrips)))
        throw new ArgumentException("customGripsType must be derived from CustomObjectGrips");
      if (enabler == null)
        throw new ArgumentNullException("enabler");

      Guid key = customGripsType.GUID;
      if (m_registered_enablers.ContainsKey(key))
        m_registered_enablers[key] = enabler;
      else
      {
        // This is a new enabler that needs to be registerer with RhinoApp()
        m_registered_enablers.Add(key, enabler);
        UnsafeNativeMethods.CRhinoApp_RegisterGripsEnabler(key, m_TurnOnGrips);
      }
    }

    #endregion

    #region pointer management
    int m_runtime_serial_number;
    IntPtr m_ptr;
    bool m_bDeleteOnDispose = true;

    internal IntPtr NonConstPointer() { return m_ptr; }
    IntPtr ConstPointer() { return m_ptr; }

    internal void OnAttachedToRhinoObject(Rhino.DocObjects.RhinoObject rhObj)
    {
      m_bDeleteOnDispose = false;
      // make sure all of the callback functions are hooked up
      UnsafeNativeMethods.CRhinoObjectGrips_SetCallbacks(m_OnResetCallback,
        m_OnResetMeshesCallback, m_OnUpdateMeshCallback, m_OnNewGeometryCallback,
        m_OnDrawCallback);
    }
    #endregion

    protected CustomObjectGrips()
    {
      m_runtime_serial_number = m_serial_number_counter++;
      Guid id = GetType().GUID;
      m_ptr = UnsafeNativeMethods.CRhCmnObjectGrips_New(m_runtime_serial_number, id);
      m_all_custom_grips.Add(this);
    }

    List<CustomGripObject> m_grip_list = new List<CustomGripObject>();
    protected void AddGrip(Rhino.DocObjects.Custom.CustomGripObject grip)
    {
      m_grip_list.Add(grip);

      IntPtr pGrip = grip.NonConstPointer();
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoObjectGrips_AddGrip(pThis, pGrip);      
    }

    const int idxNewLocation = 0;
    const int idxGripsMoved = 1;
    const int idxDragging = 2;

    /// <summary>
    /// true if grips are currently being dragged
    /// </summary>
    /// <returns></returns>
    public static bool Dragging()
    {
      return UnsafeNativeMethods.CRhinoObjectGrips_GetBool(IntPtr.Zero, idxDragging);
    }

    /// <summary>
    /// true if some of the grips have been moved. GripObject.NewLocation() sets
    /// NewLocation=true.  Derived classes can set NewLocation to false after 
    /// updating temporary display information.
    /// </summary>
    public bool NewLocation
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoObjectGrips_GetBool(pConstThis, idxNewLocation);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoObjectGrips_SetBool(pThis, idxNewLocation, value);
      }
    }

    /// <summary>
    /// If GripsMoved is true if some of the grips have ever been moved
    /// GripObject.NewLocation() sets GripsMoved=true.
    /// </summary>
    public bool GripsMoved
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CRhinoObjectGrips_GetBool(pConstThis, idxGripsMoved);
      }
    }




    /// <summary>
    /// Resets location of all grips to original spots and cleans up stuff that
    /// was created by dynamic dragging.  This is required when dragging is
    /// canceled or in the Copy command when grips are "copied". The override
    /// should clean up dynamic workspace stuff.
    /// </summary>
    protected virtual void OnReset()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhCmnObjectGrips_ResetBase(pThis, true);
    }

    /// <summary>
    /// Just before Rhino turns off object grips, it calls this function.
    /// If grips have modified any display meshes, they must override
    /// this function and restore the meshes to their original states.
    /// </summary>
    protected virtual void OnResetMeshes()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhCmnObjectGrips_ResetBase(pThis, false);
    }

    /// <summary>
    /// Just before Rhino shades an object with grips on, it calls OnUpdateMesh()
    /// to update the display meshes.  Grips that modify surface or mesh objects
    /// must override this function and modify the display meshes here.
    /// </summary>
    /// <param name="meshType"></param>
    protected virtual void OnUpdateMesh(Rhino.Geometry.MeshType meshType)
    {
      //base class does nothing
    }

    // Use NewGeometry for now instead of NewObject(). We can always add a NewObject()
    // virtual function that just calls NewGeometry by default
    //protected virtual RhinoObject NewObject()
    //{
    //  // The default calls NewGeometry
    //  Rhino.Geometry.GeometryBase geometry = NewGeometry();
    //  if (geometry != null)
    //  {
    //    IntPtr pConstThis = ConstPointer();
    //    IntPtr pGeometry = geometry.NonConstPointer();
    //    IntPtr pNewRhinoObject = UnsafeNativeMethods.CRhCmnObjectGrips_CreateRhinoObject(pConstThis, pGeometry);
    //    return NewRhinoObject???
    //  }
    //  return null;
    //}

    /// <summary>
    /// If the grips control just one object, then override NewGeometry(). When
    /// NewGeometry() is called, return new geometry calculated from the current
    /// grip locations. This happens once at the end of a grip drag.
    /// </summary>
    /// <returns></returns>
    protected virtual Rhino.Geometry.GeometryBase NewGeometry()
    {
      return null;
    }

    /// <summary>
    /// The default draws the grips.  Override if you need to draw dynamic stuff
    /// and then call CustomObjectGrips.OnDraw() to draw the grips themselves.
    /// </summary>
    /// <param name="args"></param>
    protected virtual void OnDraw(Rhino.Display.GripsDrawEventArgs args)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhinoObjectGrips_DrawBase(pThis, args.m_pGripsDrawSettings);
    }

    ~CustomObjectGrips() { Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (m_bDeleteOnDispose && IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.CRhCmnObjectGrips_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }

    internal delegate void CRhinoGripsEnablerCallback(IntPtr pRhinoObject, Guid enabler_id);
    private static CRhinoGripsEnablerCallback m_TurnOnGrips = CRhinoGripsEnabler_TurnOnGrips;
    private static void CRhinoGripsEnabler_TurnOnGrips(IntPtr pRhinoObject, Guid enabler_id)
    {
      TurnOnGripsEventHandler handler = null;
      if (m_registered_enablers.TryGetValue(enabler_id, out handler))
      {
        RhinoObject rhobj = RhinoObject.CreateRhinoObjectHelper(pRhinoObject);
        if( rhobj!=null )
          handler(rhobj);
      }
    }


    internal delegate void CRhinoObjectGripsResetCallback(int serial_number);
    internal delegate void CRhinoObjectGripsUpdateMeshCallback(int serial_number, int meshType);
    internal delegate IntPtr CRhinoObjectGripsNewGeometryCallback(int serial_number);
    internal delegate void CRhinoObjectGripsDrawCallback(int serial_numer, IntPtr pDrawSettings);
    
    private static CRhinoObjectGripsResetCallback m_OnResetCallback = CRhinoObjectGrips_Reset;
    private static CRhinoObjectGripsResetCallback m_OnResetMeshesCallback = CRhinoObjectGrips_ResetMeshes;
    private static CRhinoObjectGripsUpdateMeshCallback m_OnUpdateMeshCallback = CRhinoObjectGrips_UpdateMesh;
    private static CRhinoObjectGripsNewGeometryCallback m_OnNewGeometryCallback = CRhinoObjectGrips_NewGeometry;
    private static CRhinoObjectGripsDrawCallback m_OnDrawCallback = CRhinoObjectGrips_Draw;

    private static void CRhinoObjectGrips_Reset(int serial_number)
    {
      CustomObjectGrips grips = FromSerialNumber(serial_number);
      if (grips != null)
      {
        try
        {
          grips.OnReset();
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void CRhinoObjectGrips_ResetMeshes(int serial_number)
    {
      CustomObjectGrips grips = FromSerialNumber(serial_number);
      if (grips != null)
      {
        try
        {
          grips.OnResetMeshes();
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static void CRhinoObjectGrips_UpdateMesh(int serial_number, int meshType)
    {
      CustomObjectGrips grips = FromSerialNumber(serial_number);
      if (grips != null)
      {
        try
        {
          grips.OnUpdateMesh((Geometry.MeshType)meshType);
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static IntPtr CRhinoObjectGrips_NewGeometry(int serial_number)
    {
      IntPtr rc = IntPtr.Zero;
      CustomObjectGrips grips = FromSerialNumber(serial_number);
      if (grips != null)
      {
        try
        {
          Rhino.Geometry.GeometryBase geom = grips.NewGeometry();
          if (geom != null)
          {
            rc = geom.NonConstPointer();
            geom.ReleaseNonConstPointer();
          }
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }
    private static void CRhinoObjectGrips_Draw(int serial_number, IntPtr pDrawSettings)
    {
      CustomObjectGrips grips = FromSerialNumber(serial_number);
      if (grips != null)
      {
        try
        {
          Rhino.Display.GripsDrawEventArgs args = new Display.GripsDrawEventArgs(pDrawSettings);
          grips.OnDraw(args);
        }
        catch (Exception ex)
        {
          Rhino.Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }

    /// <summary>Get neighbors</summary>
    /// <param name="gripIndex">index of grip where the search begins</param>
    /// <param name="dr">
    /// 1 = next grip in the first parameter direction
    /// -1 = prev grip in the first parameter direction
    /// </param>
    /// <param name="ds">
    /// 1 = next grip in the second parameter direction
    /// -1 = prev grip in the second parameter direction
    /// </param>
    /// <param name="dt">
    /// 1 = next grip in the third parameter direction
    /// -1 = prev grip in the third parameter direction
    /// </param>
    /// <param name="wrap">If true and object is "closed", the search will wrap.</param>
    /// <returns>Pointer to the desired neighbor or NULL if there is no neighbor</returns>
    protected virtual GripObject NeighborGrip(int gripIndex, int dr, int ds, int dt, bool wrap){ return null; }

    /// <summary>
    /// If the grips are control points of a NURBS surface, then this gets the
    /// index of the grip that controls the (i,j)-th cv.
    /// </summary>
    /// <param name="cvI"></param>
    /// <param name="cvJ"></param>
    /// <returns>A grip controling a NURBS surface CV or NULL.</returns>
    protected virtual GripObject NurbsSurfaceGrip( int cvI, int cvJ ){ return null; }

    /// <summary>
    /// If the grips control a NURBS surface, this returns a pointer to that
    /// surface.  You can look at but you must NEVER change this surface.
    /// </summary>
    /// <returns>A pointer to a NURBS surface or NULL</returns>
    protected virtual Rhino.Geometry.NurbsSurface NurbsSurface(){ return null; }
  }
}