using System;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
  public class PointObject : RhinoObject
  {
    internal PointObject(uint serialNumber)
      : base(serialNumber)
    { }

    public Rhino.Geometry.Point PointGeometry
    {
      get
      {
        Rhino.Geometry.Point rc = this.Geometry as Rhino.Geometry.Point;
        return rc;
      }
    }

    public Rhino.Geometry.Point DuplicatePointGeometry()
    {
      Rhino.Geometry.Point rc = this.DuplicateGeometry() as Rhino.Geometry.Point;
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
        Rhino.Geometry.PointCloud rc = this.Geometry as Rhino.Geometry.PointCloud;
        return rc;
      }
    }

    public Rhino.Geometry.PointCloud DuplicatePointCloudGeometry()
    {
      Rhino.Geometry.PointCloud rc = this.DuplicateGeometry() as Rhino.Geometry.PointCloud;
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
    // grip objects should not be able to be constructed by plug-in
    //private GripObject() { }

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
    /// True if the grip has moved from OriginalLocation
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
    /// Move the grip to a new location
    /// </summary>
    /// <param name="xform">
    /// Transformation appliead to the OriginalLocation point
    /// </param>
    public void Move(Transform xform)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip1(ptr, ref xform);
    }
    /// <summary>
    /// Move the grip to a new location
    /// </summary>
    /// <param name="delta">
    /// Translation applied to the OriginalLocation point
    /// </param>
    public void Move(Vector3d delta)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      Point3d point = new Point3d(delta);
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip2(ptr, point, true);
    }
    /// <summary>
    /// Move the grip to a new location
    /// </summary>
    /// <param name="newLocation">
    /// New location for grip
    /// </param>
    public void Move(Point3d newLocation)
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_MoveGrip2(ptr, newLocation, false);
    }

    /// <summary>
    /// Undoes any grip moves made by calling Move
    /// </summary>
    public void UndoMove()
    {
      IntPtr ptr = NonConstPointer_I_KnowWhatImDoing();
      UnsafeNativeMethods.CRhinoGripObject_UndoMode(ptr);      
    }

    /// <summary>
    /// The weight of a NURBS control point grip or RhinoMath.UnsetValue
    /// if the grip is not a NURBS control point grip
    /// </summary>
    public double Weight
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
