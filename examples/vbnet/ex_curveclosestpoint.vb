Imports Rhino
Imports Rhino.Geometry
Imports Rhino.Input.Custom

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("2C395C2C-DF9E-4758-A1D9-9800C0E93E5D")> _
  Public Class ex_curveclosestpoint
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbFindCurveParameterAtPoint"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim objref As Rhino.DocObjects.ObjRef
      Dim rc = Rhino.Input.RhinoGet.GetOneObject("Select curve", True, Rhino.DocObjects.ObjectType.Curve, objref)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      Dim crv = objref.Curve()
      If crv Is Nothing Then
        Return Rhino.Commands.Result.Failure
      End If

      Dim gp = New Rhino.Input.Custom.GetPoint()
      gp.SetCommandPrompt("Pick a location on the curve")
      gp.Constrain(crv, False)
      gp.[Get]()
      If gp.CommandResult() <> Rhino.Commands.Result.Success Then
        Return gp.CommandResult()
      End If

      Dim p = gp.Point()
      Dim cp As Double
      If crv.ClosestPoint(p, cp) Then
        Rhino.RhinoApp.WriteLine([String].Format("point: ({0},{1},{2}), parameter: {3}", p.X, p.Y, p.Z, cp))
        doc.Objects.AddPoint(p)
        doc.Views.Redraw()
      End If
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace