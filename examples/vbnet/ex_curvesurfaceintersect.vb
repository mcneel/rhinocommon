Imports Rhino
Imports Rhino.Geometry.Intersect
Imports Rhino.Input.Custom
Imports Rhino.DocObjects
Imports Rhino.Commands
Imports System.Collections.Generic

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("336908EB-273C-4A81-BE53-8EEFC0470B6C")> _
  Public Class CurveSurfaceIntersectCommand
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbCurveSurfaceIntersect"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim gs = New GetObject()
      gs.SetCommandPrompt("select surface")
      gs.GeometryFilter = ObjectType.Surface
      gs.DisablePreSelect()
      gs.SubObjectSelect = False
      gs.[Get]()
      If gs.CommandResult() <> Result.Success Then
        Return gs.CommandResult()
      End If
      Dim surface = gs.[Object](0).Surface()

      Dim gc = New GetObject()
      gc.SetCommandPrompt("select curve")
      gc.GeometryFilter = ObjectType.Curve
      gc.DisablePreSelect()
      gc.SubObjectSelect = False
      gc.[Get]()
      If gc.CommandResult() <> Result.Success Then
        Return gc.CommandResult()
      End If
      Dim curve = gc.[Object](0).Curve()

      If surface Is Nothing OrElse curve Is Nothing Then
        Return Result.Failure
      End If

      Dim tolerance = doc.ModelAbsoluteTolerance

      Dim curveIntersections = Intersection.CurveSurface(curve, surface, tolerance, tolerance)
      If curveIntersections IsNot Nothing Then
        Dim addedObjects = New List(Of Guid)()
        For Each curveIntersection As IntersectionEvent In curveIntersections
          If curveIntersection.IsOverlap Then
            Dim t0 As Double
            Dim t1 As Double
            curve.ClosestPoint(curveIntersection.PointA, t0)
            curve.ClosestPoint(curveIntersection.PointA2, t1)
            Dim overlapCurve = curve.DuplicateCurve().Trim(t0, t1)
            addedObjects.Add(doc.Objects.AddCurve(overlapCurve))
          Else
            ' IsPoint
            addedObjects.Add(doc.Objects.AddPoint(curveIntersection.PointA))
          End If
        Next
        If addedObjects.Count > 0 Then
          doc.Objects.[Select](addedObjects)
        End If
      End If

      doc.Views.Redraw()

      Return Result.Success
    End Function
  End Class
End Namespace