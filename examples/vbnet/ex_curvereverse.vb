Imports Rhino
Imports Rhino.Geometry
Imports Rhino.Commands
Imports Rhino.Input.Custom

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("26167F88-527C-44BB-BD71-4371144D5992")> _
  Public Class ex_curvereverse
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbReverseCurve"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim go As New Rhino.Input.Custom.GetObject()
      go.SetCommandPrompt("Select curves to reverse")
      go.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      go.GetMultiple(1, 0)
      If go.CommandResult() <> Rhino.Commands.Result.Success Then
        Return go.CommandResult()
      End If

      For i As Integer = 0 To go.ObjectCount - 1
        Dim objRef = go.[Object](i)
        Dim crv = objRef.Curve()
        Dim dup = crv.DuplicateCurve()
        If dup IsNot Nothing Then
          dup.Reverse()
          doc.Objects.Replace(objRef, dup)
        End If
      Next

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace