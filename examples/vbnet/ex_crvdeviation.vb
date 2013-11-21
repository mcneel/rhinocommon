Imports System.Runtime.ExceptionServices
Imports Rhino
Imports Rhino.Commands
Imports Rhino.DocObjects
Imports Rhino.Geometry
Imports System.Collections.Generic
Imports Rhino.Input.Custom

Namespace examples_vb
  Class DeviationConduit
    Inherits Rhino.Display.DisplayConduit
    Private _curveA As Curve
    Private _curveB As Curve
    Private _minDistPointA As Point3d
    Private _minDistPointB As Point3d
    Private _maxDistPointA As Point3d
    Private _maxDistPointB As Point3d

    Public Sub New(curveA As Curve, curveB As Curve, minDistPointA As Point3d, minDistPointB As Point3d, maxDistPointA As Point3d, maxDistPointB As Point3d)
      _curveA = curveA
      _curveB = curveB
      _minDistPointA = minDistPointA
      _minDistPointB = minDistPointB
      _maxDistPointA = maxDistPointA
      _maxDistPointB = maxDistPointB
    End Sub

    Protected Overrides Sub DrawForeground(e As Rhino.Display.DrawEventArgs)
      e.Display.DrawCurve(_curveA, System.Drawing.Color.Red)
      e.Display.DrawCurve(_curveB, System.Drawing.Color.Red)

      e.Display.DrawPoint(_minDistPointA, System.Drawing.Color.LawnGreen)
      e.Display.DrawPoint(_minDistPointB, System.Drawing.Color.LawnGreen)
      e.Display.DrawLine(New Line(_minDistPointA, _minDistPointB), System.Drawing.Color.LawnGreen)
      e.Display.DrawPoint(_maxDistPointA, System.Drawing.Color.Red)
      e.Display.DrawPoint(_maxDistPointB, System.Drawing.Color.Red)
      e.Display.DrawLine(New Line(_maxDistPointA, _maxDistPointB), System.Drawing.Color.Red)
    End Sub
  End Class

  <System.Runtime.InteropServices.Guid("83B05F5B-0A15-4239-897F-B291BBE0EDD8")> _
  Public Class ex_crvdeviation
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbCrvDeviation"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gc1 = New Rhino.Input.Custom.GetObject()
      gc1.SetCommandPrompt("first curve")
      gc1.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      gc1.AcceptNothing(False)
      gc1.DisablePreSelect()
      gc1.[Get]()
      If gc1.CommandResult() <> Result.Success Then
        Return gc1.CommandResult()
      End If
      Dim crvA = gc1.[Object](0).Curve()
      If crvA Is Nothing Then
        Return Result.Failure
      End If

      Dim gc2 = New Rhino.Input.Custom.GetObject()
      gc2.SetCommandPrompt("second curve")
      gc2.GeometryFilter = Rhino.DocObjects.ObjectType.Curve
      gc2.AcceptNothing(False)
      gc2.DisablePreSelect()
      gc2.[Get]()
      If gc2.CommandResult() <> Result.Success Then
        Return gc2.CommandResult()
      End If
      Dim crvB = gc2.[Object](0).Curve()
      If crvB Is Nothing Then
        Return Result.Failure
      End If

      Dim tolerance = doc.ModelAbsoluteTolerance

      Dim maxDistance As Double
      Dim maxDistanceParameterA As Double
      Dim maxDistanceParameterB As Double
      Dim minDistance As Double
      Dim minDistanceParameterA As Double
      Dim minDistanceParameterB As Double

      Dim conduit As DeviationConduit
      If Not Curve.GetDistancesBetweenCurves(crvA, crvB, tolerance, maxDistance, maxDistanceParameterA, maxDistanceParameterB, _
       minDistance, minDistanceParameterA, minDistanceParameterB) Then
        Rhino.RhinoApp.WriteLine("Unable to find overlap intervals.")
        Return Rhino.Commands.Result.Success
      Else
        If minDistance <= RhinoMath.ZeroTolerance Then
          minDistance = 0.0
        End If
        Dim maxDistPtA = crvA.PointAt(maxDistanceParameterA)
        Dim maxDistPtB = crvB.PointAt(maxDistanceParameterB)
        Dim minDistPtA = crvA.PointAt(minDistanceParameterA)
        Dim minDistPtB = crvB.PointAt(minDistanceParameterB)

        conduit = New DeviationConduit(crvA, crvB, minDistPtA, minDistPtB, maxDistPtA, maxDistPtB)
        conduit.Enabled = True
        doc.Views.Redraw()

        Rhino.RhinoApp.WriteLine([String].Format("Minimum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", minDistance, minDistPtA.X, minDistPtA.Y, minDistPtA.Z, minDistPtB.X, _
         minDistPtB.Y, minDistPtB.Z))
        Rhino.RhinoApp.WriteLine([String].Format("Maximum deviation = {0}   pointA({1}, {2}, {3}), pointB({4}, {5}, {6})", maxDistance, maxDistPtA.X, maxDistPtA.Y, maxDistPtA.Z, maxDistPtB.X, _
         maxDistPtB.Y, maxDistPtB.Z))
      End If

      Dim str As String = ""
      Dim s = Rhino.Input.RhinoGet.GetString("Press Enter when done", True, str)
      conduit.Enabled = False

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace