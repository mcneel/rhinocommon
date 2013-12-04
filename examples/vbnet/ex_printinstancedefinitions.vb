Imports Rhino
Imports System.Linq

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("B7391004-8A5D-4B54-88F5-1D2004D70BAC")> _
  Public Class ex_printinstancedefinitions
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbInstanceDefinitionNames"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim idefNames = (From idef In doc.InstanceDefinitions
                       Where idef IsNot Nothing AndAlso Not idef.IsDeleted
                       Select idef.Name)

      For Each n As String In idefNames
        RhinoApp.WriteLine([String].Format("Instance definition = {0}", n))
      Next

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace