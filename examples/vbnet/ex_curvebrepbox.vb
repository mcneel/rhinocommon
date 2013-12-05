Imports Rhino
Imports Rhino.Geometry
Imports Rhino.Commands

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("7B4E4AAF-8842-4629-AB96-8654A711FA00")> _
  Public Class ex_curvebrepbox
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbBoxFromCrvsBBox"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gc = New Rhino.Input.Custom.GetObject()
      gc.SetCommandPrompt("select curve")
      gc.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      gc.DisablePreSelect()
      gc.SubObjectSelect = False
      gc.[Get]()
      If gc.CommandResult() <> Result.Success Then
        Return gc.CommandResult()
      End If
      If gc.[Object](0).Curve() Is Nothing Then
        Return Result.Failure
      End If
      Dim crv = gc.[Object](0).Curve()


      Dim view = doc.Views.ActiveView
      Dim plane = view.ActiveViewport.ConstructionPlane()
      ' Create a construction plane aligned bounding box
      Dim bbox = crv.GetBoundingBox(plane)

      If bbox.IsDegenerate(doc.ModelAbsoluteTolerance) > 0 Then
        RhinoApp.WriteLine("the curve's bounding box is degenerate (flat) in at least one direction so a box cannot be created.")
        Return Rhino.Commands.Result.Failure
      End If
      Dim box = New Box(bbox)
      Dim brep__1 = Brep.CreateFromBox(box)
      doc.Objects.AddBrep(brep__1)
      doc.Views.Redraw()

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace