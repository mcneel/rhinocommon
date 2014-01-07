﻿using System;

partial class Examples
{
  public static Rhino.Commands.Result CreateSpiral(Rhino.RhinoDoc doc)
  {
    var axisStart = new Rhino.Geometry.Point3d(0, 0, 0);
    var axisDir = new Rhino.Geometry.Vector3d(1, 0, 0);
    var radiusPoint = new Rhino.Geometry.Point3d(0, 1, 0);

    Rhino.Geometry.NurbsCurve curve0 = GetSpirial0();
    if (null != curve0)
      doc.Objects.AddCurve(curve0);

    Rhino.Geometry.NurbsCurve curve1 = GetSpirial1();
    if (null != curve1)
      doc.Objects.AddCurve(curve1);

    doc.Views.Redraw();

    return Rhino.Commands.Result.Success;
  }

  private static Rhino.Geometry.NurbsCurve GetSpirial0()
  {
    var axisStart = new Rhino.Geometry.Point3d(0, 0, 0);
    var axisDir = new Rhino.Geometry.Vector3d(1, 0, 0);
    var radiusPoint = new Rhino.Geometry.Point3d(0, 1, 0);

    return Rhino.Geometry.NurbsCurve.CreateSpiral(axisStart, axisDir, radiusPoint, 1, 10, 1.0, 1.0);
  }

  private static Rhino.Geometry.NurbsCurve GetSpirial1()
  {
    var railStart = new Rhino.Geometry.Point3d(0, 0, 0);
    var railEnd = new Rhino.Geometry.Point3d(0, 0, 10);
    var railCurve = new Rhino.Geometry.LineCurve(railStart, railEnd);

    double t0 = railCurve.Domain.Min;
    double t1 = railCurve.Domain.Max;

    var radiusPoint = new Rhino.Geometry.Point3d(1, 0, 0);

    return Rhino.Geometry.NurbsCurve.CreateSpiral(railCurve, t0, t1, radiusPoint, 1, 10, 1.0, 1.0, 12);
  }
}
