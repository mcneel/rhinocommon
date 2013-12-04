Imports Rhino
Imports Rhino.DocObjects
Imports Rhino.FileIO
Imports System.Linq

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("03528527-2752-4CE0-B7DA-7872AD3F0151")> _
  Public Class ex_printinstancedefinitiontree
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbInstanceDefinitionTree"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim idefs = doc.InstanceDefinitions
      Dim idefCount = idefs.Count

      If idefCount = 0 Then
        RhinoApp.WriteLine("Document contains no instance definitions.")
        Return Rhino.Commands.Result.[Nothing]
      End If

      Dim dump = New TextLog()
      dump.IndentSize = 4

      For i As Integer = 0 To idefCount - 1
        DumpInstanceDefinition(idefs(i), dump, True)
      Next

      RhinoApp.WriteLine(dump.ToString())

      Return Rhino.Commands.Result.Success
    End Function

    Private Sub DumpInstanceDefinition(idef As InstanceDefinition, ByRef dump As TextLog, bRoot As Boolean)
      If idef IsNot Nothing AndAlso Not idef.IsDeleted Then
        Dim node As String
        If bRoot Then
          node = "─"
        Else
          node = "└"
        End If
        dump.Print([String].Format("{0} Instance definition {1} = {2}" & vbLf, node, idef.Index, idef.Name))

        Dim idefObjectCount As Integer = idef.ObjectCount
        If idefObjectCount > 0 Then
          dump.PushIndent()
          For i As Integer = 0 To idefObjectCount - 1
            Dim obj = idef.[Object](i)
            If obj IsNot Nothing AndAlso TypeOf obj Is InstanceObject Then
              DumpInstanceDefinition(TryCast(obj, InstanceObject).InstanceDefinition, dump, False)
            Else
              ' Recursive...
              dump.Print([String].Format("└ Object {0} = {1}" & vbLf, i, obj.ShortDescription(False)))
            End If
          Next
          dump.PopIndent()
        End If
      End If
    End Sub
  End Class
End Namespace