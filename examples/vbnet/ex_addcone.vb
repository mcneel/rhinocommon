Partial Class Examples
  Public Shared Function AddCone(ByVal doc As Rhino.RhinoDoc) As Rhino.Commands.Result
    Dim plane As Rhino.Geometry.Plane = Rhino.Geometry.Plane.WorldXY
    Dim height As Double = 10
    Dim radius As Double = 5
    Dim cone As New Rhino.Geometry.Cone(plane, height, radius)
    If cone.IsValid Then
      Dim cap_bottom As Boolean = True
      Dim cone_brep As Rhino.Geometry.Brep = cone.ToBrep(cap_bottom)
      If cone_brep IsNot Nothing Then
        doc.Objects.AddBrep(cone_brep)
        doc.Views.Redraw()
      End If
    End If
    Return Rhino.Commands.Result.Success
  End Function
End Class
