#pragma warning disable 1591
using System;
using Rhino.Geometry;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class PointObject : RhinoObject
  {
    internal PointObject(uint serialNumber)
      : base(serialNumber)
    { }

    internal PointObject(bool custom) { }

    public Rhino.Geometry.Point PointGeometry
    {
      get
      {
        Rhino.Geometry.Point rc = Geometry as Rhino.Geometry.Point;
        return rc;
      }
    }

    public Rhino.Geometry.Point DuplicatePointGeometry()
    {
      Rhino.Geometry.Point rc = DuplicateGeometry() as Rhino.Geometry.Point;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoPointObject_InternalCommitChanges;
    }
  }

  public class PointCloudObject : RhinoObject
  {
    internal PointCloudObject(uint serialNumber)
      : base(serialNumber)
    { }

    public Rhino.Geometry.PointCloud PointCloudGeometry
    {
      get
      {
        Rhino.Geometry.PointCloud rc = Geometry as Rhino.Geometry.PointCloud;
        return rc;
      }
    }

    public Rhino.Geometry.PointCloud DuplicatePointCloudGeometry()
    {
      Rhino.Geometry.PointCloud rc = DuplicateGeometry() as Rhino.Geometry.PointCloud;
      return rc;
    }

    internal override CommitGeometryChangesFunc GetCommitFunc()
    {
      return UnsafeNativeMethods.CRhinoPointCloudObject_InternalCommitChanges;
    }
  }


  // 20 Jan 2010 - S. Baer
  // I think CRhinoGripObjectEx can probably be merged with GripObject
  public class GripObject : RhinoObject
  {
    internal GripObject() {}

    internal GripObject(uint serialNumber)
      : base(serialNumber)
    {
    }


    public Point3d CurrentLocation
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.CRhinoGripObject_GripLocation(ptr, ref rc, true);
        return rc;
      }
      set
      {
        Move(value);
      }
    }

    public Point3d OriginalLocation
    {
      get
      {
        IntPtr ptr = ConstPointer();
        Point3d rc = new Point3d();
        UnsafeNativeMethods.CRhinoGripObject_GripLocation(ptr, ref rc, false);
        return rc;
      }
    }

    /// <summary>
    /// true if the grip has moved from OriginalLocation.
    /// </summary>
    public bool Moved
    {
      get 
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoGripObject_Moved(ptr);
      }
    }

    /// <summary>
    /// Moves the grip to a new location.
    /// </summary>
    /// <param name="xform">
    /// Transformation appliead to the OriginalLocation point.
    /// </param>
    public void Move(Transform xform)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip1(ptr, ref xform);
    }
    /// <summary>
    /// Moves the grip to a new location.
    /// </summary>
    /// <param name="delta">
    /// Translation applied to the OriginalLocation point.
    /// </param>
    public void Move(Vector3d delta)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      Point3d point = new Point3d(delta);
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip2(ptr, point, true);
    }
    /// <summary>
    /// Moves the grip to a new location.
    /// </summary>
    /// <param name="newLocation">
    /// New location for grip.
    /// </param>
    public void Move(Point3d newLocation)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip2(ptr, newLocation, false);
    }

    /// <summary>
    /// Undoes any grip moves made by calling Move.
    /// </summary>
    public void UndoMove()
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_UndoMode(ptr);      
    }

    /// <summary>
    /// The weight of a NURBS control point grip or RhinoMath.UnsetValue
    /// if the grip is not a NURBS control point grip.
    /// </summary>
    public virtual double Weight
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoGripObject_GetSetWeight(ptr, false, 0);
      }
      set 
      {
        IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
        UnsafeNativeMethods.CRhinoGripObject_GetSetWeight(ptr, true, value);
      }
    }

    public Guid OwnerId
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoGripObject_GetOwnerId(ptr);
      }
    }

    /// <summary>
    /// Used to get a grip's logical neighbors, like NURBS curve, suface,
    /// and cage control point grips.
    /// </summary>
    /// <param name="directionR">
    /// -1 to go back one grip, +1 to move forward one grip.  For curves, surfaces
    /// and cages, this is the first parameter direction.
    /// </param>
    /// <param name="directionS">
    /// -1 to go back one grip, +1 to move forward one grip.  For surfaces and
    /// cages this is the second parameter direction.
    /// </param>
    /// <param name="directionT">
    /// For cages this is the third parameter direction
    /// </param>
    /// <param name="wrap"></param>
    /// <returns>logical neighbor or null if the is no logical neighbor</returns>
    public GripObject NeighborGrip(int directionR, int directionS, int directionT, bool wrap)
    {
      IntPtr pConstThis = ConstPointer();
      uint sn = UnsafeNativeMethods.CRhinoGripObject_NeighborGrip(pConstThis, directionR, directionS, directionT, wrap);
      if( sn!=0 )
        return new GripObject(sn);
      return null;
    }

    public int Index
    {
      get
      {
        IntPtr ptr = ConstPointer();
        return UnsafeNativeMethods.CRhinoGripObject_Index(ptr);
      }
    }
  }

}

namespace Rhino.DocObjects.Custom
{
  public abstract class CustomPointObject : PointObject, IDisposable
  {
    protected CustomPointObject()
      : base(true)
    {
      Guid type_id = GetType().GUID;
      if (SubclassCreateNativePointer)
        m_pRhinoObject = UnsafeNativeMethods.CRhinoCustomPointObject_New(type_id);
    }
    protected CustomPointObject(Point point)
      : base(true)
    {
      Guid type_id = GetType().GUID;
      IntPtr pConstPoint = point.ConstPointer();
      m_pRhinoObject = UnsafeNativeMethods.CRhinoCustomObject_New2(type_id, pConstPoint);
    }

    ~CustomPointObject() { Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pRhinoObject)
      {
        // This delete is safe in that it makes sure the object is NOT
        // under control of the Rhino Document
        UnsafeNativeMethods.CRhinoObject_Delete(m_pRhinoObject);
      }
      m_pRhinoObject = IntPtr.Zero;
    }
  }


  public class CustomGripObject : GripObject, IDisposable
  {
    #region statics
    // this will probably end up in RhinoObject
    static readonly System.Collections.Generic.List<CustomGripObject> m_all_custom_grips = new System.Collections.Generic.List<CustomGripObject>();
    static CustomGripObject m_prev_found;
    static RhinoObject GetCustomObject(uint serial_number)
    {
      if (m_prev_found != null && m_prev_found.m_rhinoobject_serial_number == serial_number)
        return m_prev_found;

      for (int i = 0; i < m_all_custom_grips.Count; i++)
      {
        if (m_all_custom_grips[i].m_rhinoobject_serial_number == serial_number)
        {
          m_prev_found = m_all_custom_grips[i];
          return m_prev_found;
        }
      }
      return null;
    }
    #endregion

    public CustomGripObject()
    {
      m_pRhinoObject = UnsafeNativeMethods.CRhCmnGripObject_New();
      m_rhinoobject_serial_number = UnsafeNativeMethods.CRhinoObject_RuntimeSN(m_pRhinoObject);
      m_all_custom_grips.Add(this);

      UnsafeNativeMethods.CRhCmnGripObject_SetCallbacks(m_Destructor, m_GetWeight, m_SetWeight);
    }

    ~CustomGripObject(){ Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      m_all_custom_grips.Remove(this);
      if ( IntPtr.Zero != m_pRhinoObject )
      {
        // This delete is safe in that it makes sure the object is NOT
        // under control of the Rhino Document
        UnsafeNativeMethods.CRhinoObject_Delete(m_pRhinoObject);
      }
      m_pRhinoObject = IntPtr.Zero;
    }

    public new int Index
    {
      get{ return base.Index; }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGripObject_SetIndex(pThis, value);
      }
    }

    public new Point3d OriginalLocation
    {
      get{ return base.OriginalLocation; }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CRhinoGripObject_SetGripLocation(pThis, value);
      }
    }

    // define a weight override so we don't end up in a circular call
    public override double Weight
    {
      get { return RhinoMath.UnsetValue; }
      set { //do nothing
      }
    }

    public virtual void NewLocation()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CRhCmnGripObject_NewLocationBase(pThis);
    }


    internal delegate void CRhinoObjectDestructorCallback(uint serial_number);
    internal delegate double CRhinoGripObjectWeightCallback(uint serial_number);
    internal delegate void CRhinoGripObjectSetWeightCallback(uint serial_number, double weight);

    private static readonly CRhinoObjectDestructorCallback m_Destructor = CRhinoObject_Destructor;
    private static readonly CRhinoGripObjectWeightCallback m_GetWeight = CRhinoGripObject_GetWeight;
    private static readonly CRhinoGripObjectSetWeightCallback m_SetWeight = CRhinoGripObject_SetWeight;

    private static void CRhinoObject_Destructor(uint serial_number)
    {
      CustomGripObject grip = GetCustomObject(serial_number) as CustomGripObject;
      if (grip != null)
      {
        grip.m_pRhinoObject = IntPtr.Zero;
        GC.SuppressFinalize(grip);
      }
    }

    private static double CRhinoGripObject_GetWeight(uint serial_number)
    {
      CustomGripObject grip = GetCustomObject(serial_number) as CustomGripObject;
      if (grip != null)
      {
        return grip.Weight;
      }
      return RhinoMath.UnsetValue;
    }
    private static void CRhinoGripObject_SetWeight(uint serial_number, double weight)
    {
      CustomGripObject grip = GetCustomObject(serial_number) as CustomGripObject;
      if (grip != null)
        grip.Weight = weight;
    }


  }
}

#endif