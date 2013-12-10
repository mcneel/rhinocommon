using Rhino;
using System;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("1552E348-A3B8-42B9-9948-829F9BA0D9C4")]
  public class ex_pointatcursor : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csPointAtCursor"; } }

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool GetCursorPos(out System.Drawing.Point pt);
 
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    public static extern bool ScreenToClient(IntPtr hWnd, ref System.Drawing.Point pt);

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var rslt = Rhino.Commands.Result.Failure;
      var view = doc.Views.ActiveView;
      if (view != null)
      {
        System.Drawing.Point pt;
        if (GetCursorPos(out pt) && ScreenToClient(view.Handle, ref pt))
        {
          var xform = view.ActiveViewport.GetTransform(Rhino.DocObjects.CoordinateSystem.Screen, Rhino.DocObjects.CoordinateSystem.World);
          if (xform != null)
          {
            var point = new Rhino.Geometry.Point3d(pt.X, pt.Y, 0.0);
            RhinoApp.WriteLine(String.Format("screen point: ({0}, {1}, {2})", point.X, point.Y, point.Z));
            point.Transform(xform);
            RhinoApp.WriteLine(String.Format("world point: ({0}, {1}, {2})", point.X, point.Y, point.Z));
            rslt = Rhino.Commands.Result.Success;
          }
        }
      }
      return rslt;
    }
  }
}