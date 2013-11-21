Imports Rhino
Imports Rhino.Commands

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("A02FA1F8-ACB4-4367-B7D8-DBEE26A96433")> _
  Public Class ex_evnormal
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbDetermineNormDirOfBrepFace"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      ' select a surface
      Dim gs = New Rhino.Input.Custom.GetObject()
      gs.SetCommandPrompt("select surface")
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface
      gs.DisablePreSelect()
      gs.SubObjectSelect = False
      gs.[Get]()
      If gs.CommandResult() <> Result.Success Then
        Return gs.CommandResult()
      End If

      ' get the selected face
      Dim face = gs.[Object](0).Face()
      If face Is Nothing Then
        Return Result.Failure
      End If

      ' pick a point on the surface.  Constain
      ' picking to the face.
      Dim gp = New Rhino.Input.Custom.GetPoint()
      gp.SetCommandPrompt("select point on surface")
      gp.Constrain(face, False)
      gp.[Get]()
      If gp.CommandResult() <> Result.Success Then
        Return gp.CommandResult()
      End If

      ' get the parameters of the point on the
      ' surface that is clesest to gp.Point()
      Dim u As Double, v As Double
      If face.ClosestPoint(gp.Point(), u, v) Then
        Dim dir = face.NormalAt(u, v)
        If face.OrientationIsReversed Then
          dir.Reverse()
        End If
        RhinoApp.WriteLine(String.Format("Surface normal at uv({0:f},{1:f}) = ({2:f},{3:f},{4:f})", u, v, dir.X, dir.Y, dir.Z))
      End If
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace
