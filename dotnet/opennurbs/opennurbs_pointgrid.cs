using System;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a grid of 3D points
  /// </summary>
  public class Point3dGrid : GeometryBase
  {
    public Point3dGrid()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PointGrid_New(0,0);
      ConstructNonConstObject(ptr);
    }

    public Point3dGrid(int rows, int columns)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_PointGrid_New(rows, columns);
      ConstructNonConstObject(ptr);
    }


    internal Point3dGrid(IntPtr ptr, Rhino.DocObjects.RhinoObject parent_object, Rhino.DocObjects.ObjRef obj_ref) 
      : base(ptr, parent_object, obj_ref)
    { }
  }
}
