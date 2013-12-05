Imports Rhino

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("4F4C1AE6-725F-44AA-A35D-6D891681BB0B")> _
  Public Class ex_viewportresolution
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbViewportResolution"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim avp = doc.Views.ActiveView.ActiveViewport
      RhinoApp.WriteLine([String].Format("Name = {0}: Width = {1}, Height = {2}", avp.Name, avp.Size.Width, avp.Size.Height))
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace