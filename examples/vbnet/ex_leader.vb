Imports Rhino
Imports Rhino.Geometry
Imports Rhino.Commands
Imports Rhino.Input.Custom
Imports System.Collections.Generic
Imports System.Linq

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("55EDA992-FF8B-4523-BF81-76FEE65E90F6")> _
  Public Class ex_leader
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbLeader"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim points = New List(Of Point3d)()
      points.Add(New Point3d(1, 1, 0))
      points.Add(New Point3d(5, 1, 0))
      points.Add(New Point3d(5, 5, 0))
      points.Add(New Point3d(9, 5, 0))

      Dim xyPlane = Plane.WorldXY

      Dim pts2d = New List(Of Point2d)()
      For Each pt3d As Point3d In points
        Dim x As Double, y As Double
        If xyPlane.ClosestParameter(pt3d, x, y) Then
          Dim pt2d = New Point2d(x, y)
          If pts2d.Count < 1 OrElse pt2d.DistanceTo(pts2d.Last()) > RhinoMath.SqrtEpsilon Then
            pts2d.Add(pt2d)
          End If
        End If
      Next

      doc.Objects.AddLeader(xyPlane, pts2d)
      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace