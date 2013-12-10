using Rhino;
using Rhino.Geometry;
using System;
using System.Collections.Generic; 

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("29530E9C-8A5B-47BD-A2F8-0A7BF311D8D3")]
  public class ex_createsurfaceexample : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csCreateSrfExample"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      const bool bIsRational = false;
      const int dim = 3;
      const int u_degree = 2;
      const int v_degree = 3;
      const int u_cv_count = 3;
      const int v_cv_count = 5;
     
      // The knot vectors do NOT have the 2 superfluous knots
      // at the start and end of the knot vector.  If you are
      // coming from a system that has the 2 superfluous knots,
      // just ignore them when creating NURBS surfaces.
      double[] u_knot = new double[u_cv_count + u_degree - 1];
      double[] v_knot = new double[v_cv_count + v_degree - 1];
     
      // make up a quadratic knot vector with no interior knots
      u_knot[0] = u_knot[1] = 0.0;
      u_knot[2] = u_knot[3] = 1.0;
     
      // make up a cubic knot vector with one simple interior knot
      v_knot[0] = v_knot[1] = v_knot[2] = 0.0;
      v_knot[3] = 1.5;
      v_knot[4] = v_knot[5] = v_knot[6] = 2.0;
     
      // Rational control points can be in either homogeneous
      // or euclidean form. Non-rational control points do not
      // need to specify a weight.  
      var CV = new Point3d[u_cv_count, v_cv_count];

      for (int i = 0; i < u_cv_count; i++)
      {
        for (int j = 0; j < v_cv_count; j++)
        {
          CV[i,j] = new Point3d(i, j, i-j);
        }
      }
     
      // creates internal uninitialized arrays for 
      // control points and knots
      var nurbs_surface = NurbsSurface.Create(
        dim,
        bIsRational,
        u_degree + 1,
        v_degree + 1,
        u_cv_count,
        v_cv_count
        );
     
      // add the knots
      for (int i = 0;  i < nurbs_surface.KnotsU.Count; i++)
        nurbs_surface.KnotsU[i] = u_knot[i];
      for (int j = 0; j < nurbs_surface.KnotsV.Count; j++)
        nurbs_surface.KnotsV[j] = v_knot[j];

      // add the control points
      for (int i = 0; i < nurbs_surface.Points.CountU; i++)
      {
        for (int j = 0; j < nurbs_surface.Points.CountV; j++)
        {
          nurbs_surface.Points.SetControlPoint(i, j, new ControlPoint(CV[i, j]));
        }
      }

      if (nurbs_surface.IsValid)
      {
        doc.Objects.AddSurface(nurbs_surface);
        doc.Views.Redraw();
        return Rhino.Commands.Result.Success;
      }
      else
      {
        return Rhino.Commands.Result.Failure;
      }
    }
  }
}