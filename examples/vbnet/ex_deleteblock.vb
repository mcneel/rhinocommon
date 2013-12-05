Imports Rhino

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("D232D930-7C77-4FE4-9D9D-077748A474A6")> _
  Public Class ex_deleteblock
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbDeleteInstanceDefinition"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      ' Get the name of the instance definition to rename
      Dim idefName As String = ""
      Dim rc = Rhino.Input.RhinoGet.GetString("Name of block to delete", True, idefName)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      If [String].IsNullOrWhiteSpace(idefName) Then
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' Verify instance definition exists
      Dim idef = doc.InstanceDefinitions.Find(idefName, True)
      If idef Is Nothing Then
        RhinoApp.WriteLine([String].Format("Block ""{0}"" not found.", idefName))
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' Verify instance definition can be deleted
      If idef.IsReference Then
        RhinoApp.WriteLine([String].Format("Unable to delete block ""{0}"".", idefName))
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' delete block and all references
      If Not doc.InstanceDefinitions.Delete(idef.Index, True, True) Then
        RhinoApp.WriteLine([String].Format("Could not delete {0} block", idef.Name))
        Return Rhino.Commands.Result.Failure
      End If

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace