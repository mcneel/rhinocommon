using System;
using System.Runtime.Serialization;

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents a grid of 3D points
  /// </summary>
  [Serializable]
  public class Point3dGrid : GeometryBase, ISerializable
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
        // serialization constructor

    protected Point3dGrid(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

  }
}
