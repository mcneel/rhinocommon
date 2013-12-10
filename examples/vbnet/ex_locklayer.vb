Imports Rhino
Imports System.Linq

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("72203654-7B13-444B-BBC8-5A9EB5C8F8A3")> _
  Public Class ex_locklayer
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbLockLayer"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim layerName As String = ""
      Dim rc = Rhino.Input.RhinoGet.GetString("Name of layer to lock", True, layerName)
      If rc <> Rhino.Commands.Result.Success Then
        Return rc
      End If
      If [String].IsNullOrWhiteSpace(layerName) Then
        Return Rhino.Commands.Result.[Nothing]
      End If

      ' because of sublayers it's possible that mone than one layer has the same name
      ' so simply calling doc.Layers.Find(layerName) isn't good enough.  If "layerName" returns
      ' more than one layer then present them to the user and let him decide.
      Dim matchingLayers = (From layer In doc.Layers Where layer.Name = layerName Select layer).ToList()

      Dim layerToRename As Rhino.DocObjects.Layer = Nothing
      If matchingLayers.Count = 0 Then
        RhinoApp.WriteLine([String].Format("Layer ""{0}"" does not exist.", layerName))
        Return Rhino.Commands.Result.[Nothing]
      ElseIf matchingLayers.Count = 1 Then
        layerToRename = matchingLayers(0)
      ElseIf matchingLayers.Count > 1 Then
        For i As Integer = 0 To matchingLayers.Count - 1
          RhinoApp.WriteLine([String].Format("({0}) {1}", i + 1, matchingLayers(i).FullPath.Replace("::", "->")))
        Next
        Dim selectedLayer As Integer = -1
        rc = Rhino.Input.RhinoGet.GetInteger("which layer?", True, selectedLayer)
        If rc <> Rhino.Commands.Result.Success Then
          Return rc
        End If
        If selectedLayer > 0 AndAlso selectedLayer <= matchingLayers.Count Then
          layerToRename = matchingLayers(selectedLayer - 1)
        Else
          Return Rhino.Commands.Result.[Nothing]
        End If
      End If

      If Not layerToRename.IsLocked Then
        layerToRename.IsLocked = True
        layerToRename.CommitChanges()
        Return Rhino.Commands.Result.Success
      Else
        RhinoApp.WriteLine([String].Format("layer {0} is already locked.", layerToRename.FullPath))
        Return Rhino.Commands.Result.[Nothing]
      End If
    End Function
  End Class
End Namespace