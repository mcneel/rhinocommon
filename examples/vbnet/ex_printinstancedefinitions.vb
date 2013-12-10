Imports Rhino
Imports Rhino.Commands
Imports System.Linq

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("B7391004-8A5D-4B54-88F5-1D2004D70BAC")> _
  Public Class ex_printinstancedefinitions
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbInstanceDefinitionNames"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim instanceDefinitionNames = (From instanceDefinition In doc.InstanceDefinitions
                                     Where instanceDefinition IsNot Nothing AndAlso Not instanceDefinition.IsDeleted
                                     Select instanceDefinition.Name)

      For Each n As String In instanceDefinitionNames
        RhinoApp.WriteLine([String].Format("Instance definition = {0}", n))
      Next

      Return Result.Success
    End Function
  End Class
End Namespace