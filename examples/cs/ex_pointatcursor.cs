using Rhino;
using Rhino.Commands;
using Rhino.DocObjects;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("1552E348-A3B8-42B9-9948-829F9BA0D9C4")]
  public class PointAtCursorCommand : Command
  {
    public override string EnglishName { get { return "csPointAtCursor"; } }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool GetCursorPos(out System.Drawing.Point point);
 
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point point);

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var result = Result.Failure;
      var view = doc.Views.ActiveView;
      if (view == null) return result;

      System.Drawing.Point windows_drawing_point;
      if (!GetCursorPos(out windows_drawing_point) || !ScreenToClient(view.Handle, ref windows_drawing_point))
        return result;

      var xform = view.ActiveViewport.GetTransform(CoordinateSystem.Screen, CoordinateSystem.World);
      var point = new Rhino.Geometry.Point3d(windows_drawing_point.X, windows_drawing_point.Y, 0.0);
      RhinoApp.WriteLine(String.Format("screen point: ({0}, {1}, {2})", point.X, point.Y, point.Z));
      point.Transform(xform);
      RhinoApp.WriteLine(String.Format("world point: ({0}, {1}, {2})", point.X, point.Y, point.Z));
      result = Result.Success;
      return result;
    }
  }
}