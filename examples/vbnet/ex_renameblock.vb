Imports Rhino

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("FF8C0F4C-36A4-4CF8-BF17-199E98C97383")> _
  Public Class ex_renameblock
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbRenameInstanceDefinition"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      ' Get the name of the instance definition to rename
      Dim idefName As String = ""
      Dim rc = Rhino.Input.RhinoGet.GetString("Name of block to rename", True, idefName)
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

      ' Verify instance definition is rename-able
      If idef.IsDeleted OrElse idef.IsReference Then
        RhinoApp.WriteLine([String].Format("Unable to rename block ""{0}"".", idefName))
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' Get the new instance definition name
      Dim idefNewName As String = ""
      rc = Rhino.Input.RhinoGet.GetString("Name of block to rename", True, idefNewName)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      If [String].IsNullOrWhiteSpace(idefNewName) Then
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' Verify the new instance definition name is not already in use
      Dim existingIdef = doc.InstanceDefinitions.Find(idefNewName, True)
      If existingIdef IsNot Nothing AndAlso Not existingIdef.IsDeleted Then
        RhinoApp.WriteLine([String].Format("Block ""{0}"" already exists.", existingIdef))
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' change the block name
      If Not doc.InstanceDefinitions.Modify(idef.Index, idefNewName, idef.Description, True) Then
        RhinoApp.WriteLine([String].Format("Could not rename {0} to {1}", idef.Name, idefNewName))
        Return Rhino.Commands.Result.Failure
      End If

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace