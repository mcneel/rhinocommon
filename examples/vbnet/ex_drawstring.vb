﻿Imports Rhino
Imports Rhino.DocObjects
Imports Rhino.Geometry
Imports Rhino.Commands
Imports Rhino.Input.Custom

Namespace examples_vb
  Public Class DrawStringCommand
    Inherits Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbDrawString"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As RunMode) As Result
      Dim gp = New GetDrawStringPoint()
      gp.SetCommandPrompt("Point")
      gp.[Get]()
      Return gp.CommandResult()
    End Function
  End Class

  Public Class GetDrawStringPoint
    Inherits GetPoint
    Protected Overrides Sub OnDynamicDraw(e As GetPointDrawEventArgs)
      MyBase.OnDynamicDraw(e)
      Dim xform = e.Viewport.GetTransform(CoordinateSystem.World, CoordinateSystem.Screen)
      Dim current_point = e.CurrentPoint
      current_point.Transform(xform)
      Dim screen_point = New Point2d(current_point.X, current_point.Y)
      Dim msg = String.Format("screen {0:F}, {1:F}", current_point.X, current_point.Y)
      e.Display.Draw2dText(msg, System.Drawing.Color.Blue, screen_point, False)
    End Sub
  End Class
End Namespace