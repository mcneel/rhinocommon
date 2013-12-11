Imports Rhino
Imports Rhino.Input
Imports Rhino.Input.Custom
Imports Rhino.DocObjects
Imports Rhino.Commands

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("6BB1D924-10B4-4998-A3FE-0D4FFA78E070")> _
  Public Class CurveClosestPointCommand
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbFindCurveParameterAtPoint"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim objref As Rhino.DocObjects.ObjRef
      Dim rc = RhinoGet.GetOneObject("Select curve", True, ObjectType.Curve, objref)
      If rc <> Result.Success Then
        Return rc
      End If
      Dim curve = objref.Curve()
      If curve Is Nothing Then
        Return Result.Failure
      End If

      Dim gp = New GetPoint()
      gp.SetCommandPrompt("Pick a location on the curve")
      gp.Constrain(curve, False)
      gp.[Get]()
      If gp.CommandResult() <> Result.Success Then
        Return gp.CommandResult()
      End If

      Dim point = gp.Point()
      Dim closestPointParam As Double
      If curve.ClosestPoint(point, closestPointParam) Then
        RhinoApp.WriteLine(String.Format("point: ({0},{1},{2}), parameter: {3}", point.X, point.Y, point.Z, closestPointParam))
        doc.Objects.AddPoint(point)
        doc.Views.Redraw()
      End If
      Return Result.Success
    End Function
  End Class
End Namespace