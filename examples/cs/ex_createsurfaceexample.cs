using Rhino;
using Rhino.Commands;
using Rhino.Geometry;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("29530E9C-8A5B-47BD-A2F8-0A7BF311D8D3")]
  public class CreateSurfaceFromPointsAndKnotsCommand : Command
  {
    public override string EnglishName { get { return "csCreateSurfaceFromPointsAndKnots"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      const bool isRational = false;
      const int numberOfDimensions = 3;
      const int uDegree = 2;
      const int vDegree = 3;
      const int uControlPointCount = 3;
      const int vControlPointCount = 5;
     
      // The knot vectors do NOT have the 2 superfluous knots
      // at the start and end of the knot vector.  If you are
      // coming from a system that has the 2 superfluous knots,
      // just ignore them when creating NURBS surfaces.
      var uKnots = new double[uControlPointCount + uDegree - 1];
      var vKnots = new double[vControlPointCount + vDegree - 1];
     
      // make up a quadratic knot vector with no interior knots
      uKnots[0] = uKnots[1] = 0.0;
      uKnots[2] = uKnots[3] = 1.0;
     
      // make up a cubic knot vector with one simple interior knot
      vKnots[0] = vKnots[1] = vKnots[2] = 0.0;
      vKnots[3] = 1.5;
      vKnots[4] = vKnots[5] = vKnots[6] = 2.0;
     
      // Rational control points can be in either homogeneous
      // or euclidean form. Non-rational control points do not
      // need to specify a weight.  
      var controlPoints = new Point3d[uControlPointCount, vControlPointCount];

      for (int u = 0; u < uControlPointCount; u++)
      {
        for (int v = 0; v < vControlPointCount; v++)
        {
          controlPoints[u,v] = new Point3d(u, v, u-v);
        }
      }
     
      // creates internal uninitialized arrays for 
      // control points and knots
      var nurbsSurface = NurbsSurface.Create(
        numberOfDimensions,
        isRational,
        uDegree + 1,
        vDegree + 1,
        uControlPointCount,
        vControlPointCount
        );
     
      // add the knots
      for (int u = 0;  u < nurbsSurface.KnotsU.Count; u++)
        nurbsSurface.KnotsU[u] = uKnots[u];
      for (int v = 0; v < nurbsSurface.KnotsV.Count; v++)
        nurbsSurface.KnotsV[v] = vKnots[v];

      // add the control points
      for (int u = 0; u < nurbsSurface.Points.CountU; u++)
      {
        for (int v = 0; v < nurbsSurface.Points.CountV; v++)
        {
          nurbsSurface.Points.SetControlPoint(u, v, new ControlPoint(controlPoints[u, v]));
        }
      }

      if (nurbsSurface.IsValid)
      {
        doc.Objects.AddSurface(nurbsSurface);
        doc.Views.Redraw();
        return Result.Success;
      }
      else
      {
        return Result.Failure;
      }
    }
  }
}