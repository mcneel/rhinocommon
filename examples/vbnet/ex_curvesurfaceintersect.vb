Imports Rhino.Geometry.Intersect
Imports Rhino
Imports Rhino.Commands
Imports System.Collections.Generic

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("9376B50C-7F64-43FC-A9EA-D0402D84DF0A")> _
  Public Class ex_curvesurfaceintersect
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbCrvSrfIntersect"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gs = New Rhino.Input.Custom.GetObject()
      gs.SetCommandPrompt("select surface")
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface
      gs.DisablePreSelect()
      gs.SubObjectSelect = False
      gs.[Get]()
      If gs.CommandResult() <> Result.Success Then
        Return gs.CommandResult()
      End If
      Dim srf = gs.[Object](0).Surface()

      Dim gc = New Rhino.Input.Custom.GetObject()
      gc.SetCommandPrompt("select curve")
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      gc.DisablePreSelect()
      gc.SubObjectSelect = False
      gc.[Get]()
      If gc.CommandResult() <> Result.Success Then
        Return gc.CommandResult()
      End If
      Dim crv = gc.[Object](0).Curve()

      If srf Is Nothing OrElse crv Is Nothing Then
        Return Result.Failure
      End If

      Dim tol = doc.ModelAbsoluteTolerance

      Dim cis = Rhino.Geometry.Intersect.Intersection.CurveSurface(crv, srf, tol, tol)
      If cis IsNot Nothing Then
        Dim addedObjs = New List(Of Guid)()
        For Each ie As IntersectionEvent In cis
          If ie.IsOverlap Then
            Dim t0 As Double
            Dim t1 As Double
            crv.ClosestPoint(ie.PointA, t0)
            crv.ClosestPoint(ie.PointA2, t1)
            Dim overlapCrv = crv.DuplicateCurve().Trim(t0, t1)
            addedObjs.Add(doc.Objects.AddCurve(overlapCrv))
          Else
            ' IsPoint
            addedObjs.Add(doc.Objects.AddPoint(ie.PointA))
          End If
        Next
        If addedObjs.Count > 0 Then
          doc.Objects.[Select](addedObjs)
        End If
      End If

      doc.Views.Redraw()

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace