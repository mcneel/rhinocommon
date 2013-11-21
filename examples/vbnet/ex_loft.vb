Imports Rhino.DocObjects
Imports Rhino
Imports Rhino.Commands
Imports System.Collections.Generic
Imports Rhino.Geometry

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("3CEE907B-1325-429E-A2DA-FA1DCA437567")> _
  Public Class ex_loft
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbLoft"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      ' select curves to loft
      Dim gs = New Rhino.Input.Custom.GetObject()
      gs.SetCommandPrompt("select curves to loft")
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      gs.DisablePreSelect()
      gs.SubObjectSelect = False
      gs.GetMultiple(2, 0)
      If gs.CommandResult() <> Result.Success Then
        Return gs.CommandResult()
      End If

      Dim crvs = New List(Of Curve)()
      For Each obj As ObjRef In gs.Objects()
        crvs.Add(obj.Curve())
      Next

      Dim breps = Rhino.Geometry.Brep.CreateFromLoft(crvs, Point3d.Unset, Point3d.Unset, LoftType.Tight, False)
      For Each brep As Brep In breps
        doc.Objects.AddBrep(brep)
      Next

      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace