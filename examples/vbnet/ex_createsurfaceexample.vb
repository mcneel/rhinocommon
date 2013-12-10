Imports Rhino
Imports Rhino.Geometry
Imports System.Collections.Generic

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("F96CEFCF-C5E0-4013-A773-995794899506")> _
  Public Class ex_createsurfaceexample
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbCreateSrfExample"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Const bIsRational As Boolean = False
      Const [dim] As Integer = 3
      Const u_degree As Integer = 2
      Const v_degree As Integer = 3
      Const u_cv_count As Integer = 3
      Const v_cv_count As Integer = 5

      ' The knot vectors do NOT have the 2 superfluous knots
      ' at the start and end of the knot vector.  If you are
      ' coming from a system that has the 2 superfluous knots,
      ' just ignore them when creating NURBS surfaces.
      Dim u_knot As Double() = New Double(u_cv_count + u_degree - 2) {}
      Dim v_knot As Double() = New Double(v_cv_count + v_degree - 2) {}

      ' make up a quadratic knot vector with no interior knots
      u_knot(0) = InlineAssignHelper(u_knot(1), 0.0)
      u_knot(2) = InlineAssignHelper(u_knot(3), 1.0)

      ' make up a cubic knot vector with one simple interior knot
      v_knot(0) = InlineAssignHelper(v_knot(1), InlineAssignHelper(v_knot(2), 0.0))
      v_knot(3) = 1.5
      v_knot(4) = InlineAssignHelper(v_knot(5), InlineAssignHelper(v_knot(6), 2.0))

      ' Rational control points can be in either homogeneous
      ' or euclidean form. Non-rational control points do not
      ' need to specify a weight.  
      Dim CV = New Point3d(u_cv_count - 1, v_cv_count - 1) {}

      For i As Integer = 0 To u_cv_count - 1
        For j As Integer = 0 To v_cv_count - 1
          CV(i, j) = New Point3d(i, j, i - j)
        Next
      Next

      ' creates internal uninitialized arrays for 
      ' control points and knots
      Dim nurbs_surface = NurbsSurface.Create([dim], bIsRational, u_degree + 1, v_degree + 1, u_cv_count, v_cv_count)

      ' add the knots
      For i As Integer = 0 To nurbs_surface.KnotsU.Count - 1
        nurbs_surface.KnotsU(i) = u_knot(i)
      Next
      For j As Integer = 0 To nurbs_surface.KnotsV.Count - 1
        nurbs_surface.KnotsV(j) = v_knot(j)
      Next

      ' add the control points
      For i As Integer = 0 To nurbs_surface.Points.CountU - 1
        For j As Integer = 0 To nurbs_surface.Points.CountV - 1
          nurbs_surface.Points.SetControlPoint(i, j, New ControlPoint(CV(i, j)))
        Next
      Next

      If nurbs_surface.IsValid Then
        doc.Objects.AddSurface(nurbs_surface)
        doc.Views.Redraw()
        Return Rhino.Commands.Result.Success
      Else
        Return Rhino.Commands.Result.Failure
      End If
    End Function
    Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
      target = value
      Return value
    End Function
  End Class
End Namespace