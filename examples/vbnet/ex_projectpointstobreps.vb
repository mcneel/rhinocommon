Imports Rhino
Imports Rhino.Commands
Imports System.Collections.Generic
Imports Rhino.Geometry

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("4200CA00-D0C4-406E-A58F-BA185FC2A4A9")> _
  Public Class ex_projectpointstobreps
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbProjPtsToBreps"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gs = New Rhino.Input.Custom.GetObject()
      gs.SetCommandPrompt("select surface")
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface Or Rhino.DocObjects.ObjectType.PolysrfFilter
      gs.DisablePreSelect()
      gs.SubObjectSelect = False
      gs.[Get]()
      If gs.CommandResult() <> Result.Success Then
        Return gs.CommandResult()
      End If
      Dim brep = gs.[Object](0).Brep()
      If brep Is Nothing Then
        Return Result.Failure
      End If

      ' brep on which to project
      ' some random points to project
      ' project on Y axis
      Dim pts = Rhino.Geometry.Intersect.Intersection.ProjectPointsToBreps(New List(Of Brep)() From { _
       brep _
      }, New List(Of Point3d)() From { _
       New Point3d(0, 0, 0), _
       New Point3d(3, 0, 3), _
       New Point3d(-2, 0, -2) _
      }, New Vector3d(0, 1, 0), doc.ModelAbsoluteTolerance)

      If pts IsNot Nothing AndAlso pts.Length > 0 Then
        For Each pt As Point3d In pts
          doc.Objects.AddPoint(pt)
        Next
      End If
      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace