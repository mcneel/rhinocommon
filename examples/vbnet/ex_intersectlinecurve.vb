Imports Rhino
Imports Rhino.Geometry
Imports Rhino.Geometry.Intersect

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("DFE91F17-5F1C-401E-A9F1-79A336B959A4")> _
  Public Class ex_intersectlinecurve
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbIntersectLineCurve"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim circle As Circle
      Dim rc = Rhino.Input.RhinoGet.GetCircle(circle)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      doc.Objects.AddCircle(circle)
      doc.Views.Redraw()

      Dim line As Line
      rc = Rhino.Input.RhinoGet.GetLine(line)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      doc.Objects.AddLine(line)
      doc.Views.Redraw()

      Dim t1 As Double, t2 As Double
      Dim point1 As Point3d, point2 As Point3d
      Dim lineCircleIntersect = Intersection.LineCircle(line, circle, t1, point1, t2, point2)
      Dim msg As String = ""
      Select Case lineCircleIntersect
        Case LineCircleIntersection.None
          msg = "line does not intersect circle"
          Exit Select
        Case LineCircleIntersection.[Single]
          msg = [String].Format("line intersects circle at point ({0},{1},{2})", point1.X, point1.Y, point1.Z)
          doc.Objects.AddPoint(point1)
          Exit Select
        Case LineCircleIntersection.Multiple
          msg = [String].Format("line intersects circle at points ({0},{1},{2}) and ({3},{4},{5})", point1.X, point1.Y, point1.Z, point2.X, point2.Y, _
            point2.Z)
          doc.Objects.AddPoint(point1)
          doc.Objects.AddPoint(point2)
          Exit Select
      End Select
      RhinoApp.WriteLine(msg)
      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace