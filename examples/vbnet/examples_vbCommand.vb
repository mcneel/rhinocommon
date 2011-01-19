Imports Rhino

<System.Runtime.InteropServices.Guid("3e574930-06df-4c34-97b5-4d90e43792c6")> _
Public Class examples_vbCommand
  Inherits Rhino.Commands.Command

  Private Shared m_thecommand As examples_vbCommand
  Public Shared ReadOnly Property TheCommand()As examples_vbCommand
    Get
      Return m_thecommand
    End Get
  End Property

  Public Sub New()
    m_thecommand = Me
  End Sub

  Public Overrides ReadOnly Property EnglishName() As String
    Get
      Return "examples_vb"
    End Get
  End Property

  Private Delegate Function TestFunc(ByVal doc As RhinoDoc) As Rhino.Commands.Result
  Private Sub Test(ByVal func As TestFunc, ByVal doc As RhinoDoc)
    RhinoApp.WriteLine("[TEST START] - " + func.Method.ToString())
    Dim rc As Rhino.Commands.Result = func(doc)
    RhinoApp.WriteLine("[TEST DONE] - result = " + rc.ToString())
  End Sub

  Protected Overrides Function RunCommand(ByVal doc As RhinoDoc, ByVal mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
    Test(AddressOf Examples.ActiveViewport, doc)
    Test(AddressOf Examples.AddAnnotationText, doc)
    Test(AddressOf Examples.AddBrepBox, doc)
    Test(AddressOf Examples.AddChildLayer, doc)
    Test(AddressOf Examples.AddCircle, doc)
    Test(AddressOf Examples.AddCone, doc)
    Test(AddressOf Examples.AddCylinder, doc)
    Test(AddressOf Examples.AddLayer, doc)
    Test(AddressOf Examples.AddBackgroundBitmap, doc)
    Test(AddressOf Examples.AddClippingPlane, doc)
    Test(AddressOf Examples.AddLine, doc)
    Test(AddressOf Examples.AddLinearDimension, doc)
    Test(AddressOf Examples.AddLinearDimension2, doc)
    Test(AddressOf Examples.AddMesh, doc)
    Test(AddressOf Examples.AddNamedView, doc)
    Test(AddressOf Examples.AddNurbsCircle, doc)
    Test(AddressOf Examples.AddNurbsCurve, doc)
    Test(AddressOf Examples.AddObjectsToGroup, doc)
    Test(AddressOf Examples.AddSphere, doc)
    Test(AddressOf Examples.AddTexture, doc)
    Test(AddressOf Examples.AddTorus, doc)
    Test(AddressOf Examples.AddTruncatedCone, doc)
    Test(AddressOf Examples.AdvancedDisplay, doc)
    Test(AddressOf Examples.ArcLengthPoint, doc)
    Test(AddressOf Examples.BlockInsertionPoint, doc)
    Test(AddressOf Examples.CommandLineOptions, doc)
    Test(AddressOf Examples.DivideByLengthPoints, doc)
    Test(AddressOf Examples.DetermineObjectLayer, doc)
    Test(AddressOf Examples.OrientOnSrf, doc)
    Test(AddressOf Examples.SelLayer, doc)
    GC.Collect()
  End Function
End Class
