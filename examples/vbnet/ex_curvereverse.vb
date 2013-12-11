Imports Rhino
Imports Rhino.Commands
Imports Rhino.Input
Imports Rhino.DocObjects

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("732BA8B1-6436-4556-8303-F3D9C7F41A8F")> _
  Public Class ReverseCurveCommand
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbReverseCurve"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim objRefs As ObjRef()
      Dim rc = RhinoGet.GetMultipleObjects("Select curves to reverse", True, ObjectType.Curve, objRefs)
      If rc <> Result.Success Then
        Return rc
      End If

      For Each objRef As ObjRef In objRefs
        Dim curveCopy = objRef.Curve().DuplicateCurve()
        If curveCopy IsNot Nothing Then
          curveCopy.Reverse()
          doc.Objects.Replace(objRef, curveCopy)
        End If
      Next
      Return Result.Success
    End Function
  End Class
End Namespace