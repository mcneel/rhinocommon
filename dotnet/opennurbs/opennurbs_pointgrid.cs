#pragma warning disable 1591
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


    internal Point3dGrid(IntPtr ptr, object parent) 
      : base(ptr, parent, -1)
    { }
        // serialization constructor

    protected Point3dGrid(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

  }
}
