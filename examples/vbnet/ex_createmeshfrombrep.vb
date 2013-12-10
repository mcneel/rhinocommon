Imports Rhino

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("E8056E7A-3014-4A06-AA20-EB483AB6E103")> _
  Public Class ex_createmeshfrombrep
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbCreateMeshesFromBreps"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gs = New Rhino.Input.Custom.GetObject()
      gs.SetCommandPrompt("Select surface or polysurface to mesh")
      gs.GeometryFilter = Rhino.DocObjects.ObjectType.Surface Or Rhino.DocObjects.ObjectType.PolysrfFilter
      gs.AcceptNothing(True)
      gs.[Get]()
      If gs.CommandResult() <> Rhino.Commands.Result.Success Then
        Return gs.CommandResult()
      End If
      Dim brep = gs.[Object](0).Brep()
      If brep Is Nothing Then
        Return Rhino.Commands.Result.Failure
      End If

      Dim jaggedAndFaster = Rhino.Geometry.MeshingParameters.Coarse
      Dim smoothAndSlower = Rhino.Geometry.MeshingParameters.Smooth
      Dim defaultMeshParams = Rhino.Geometry.MeshingParameters.[Default]
      Dim minimal = Rhino.Geometry.MeshingParameters.Minimal

      Dim meshes = Rhino.Geometry.Mesh.CreateFromBrep(brep, smoothAndSlower)
      If meshes Is Nothing OrElse meshes.Length = 0 Then
        Return Rhino.Commands.Result.Failure
      End If

      Dim brepMesh = New Rhino.Geometry.Mesh()
      For Each mesh As Rhino.Geometry.Mesh In meshes
        brepMesh.Append(mesh)
      Next
      doc.Objects.AddMesh(brepMesh)
      doc.Views.Redraw()

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace