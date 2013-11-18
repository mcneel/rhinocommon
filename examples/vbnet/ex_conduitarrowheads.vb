Imports Rhino
Imports Rhino.Geometry
Imports System.Collections.Generic
Imports Rhino.Input.Custom

Namespace examples_cs
  Class DrawArrowHeadsConduit
    Inherits Rhino.Display.DisplayConduit
    Private _line As Line
    Private _screenSize As Integer
    Private _worldSize As Double

    Public Sub New(line As Line, screenSize As Integer, worldSize As Double)
      _line = line
      _screenSize = screenSize
      _worldSize = worldSize
    End Sub

    Protected Overrides Sub DrawForeground(e As Rhino.Display.DrawEventArgs)
      e.Display.DrawArrow(_line, System.Drawing.Color.Black, _screenSize, _worldSize)
    End Sub
  End Class

  <System.Runtime.InteropServices.Guid("A7236E94-85BD-4D63-9950-19C268E63661")> _
  Public Class ex_conduitarrowheads
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbDrawArrowHeads"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      ' get arrow head size
      Dim go = New Rhino.Input.Custom.GetOption()
      go.SetCommandPrompt("ArrowHead length in screen size (pixles) or world size (percentage of arrow lenght)?")
      go.AddOption("screen")
      go.AddOption("world")
      go.[Get]()
      If go.CommandResult() <> Rhino.Commands.Result.Success Then
        Return go.CommandResult()
      End If

      Dim screenSize As Integer = 0
      Dim worldSize As Double = 0.0
      If go.[Option]().EnglishName = "screen" Then
        Dim gi = New Rhino.Input.Custom.GetInteger()
        gi.SetLowerLimit(0, True)
        gi.SetCommandPrompt("Length of arrow head in pixels")
        gi.[Get]()
        If gi.CommandResult() <> Rhino.Commands.Result.Success Then
          Return gi.CommandResult()
        End If
        screenSize = gi.Number()
      Else
        Dim gi = New Rhino.Input.Custom.GetInteger()
        gi.SetLowerLimit(0, True)
        gi.SetUpperLimit(100, False)
        gi.SetCommandPrompt("Lenght of arrow head in percentage of total arrow lenght")
        gi.[Get]()
        If gi.CommandResult() <> Rhino.Commands.Result.Success Then
          Return gi.CommandResult()
        End If
        worldSize = gi.Number() / 100.0
      End If


      ' get arrow start and end points
      Dim gp = New Rhino.Input.Custom.GetPoint()
      gp.SetCommandPrompt("Start of line")
      gp.[Get]()
      If gp.CommandResult() <> Rhino.Commands.Result.Success Then
        Return gp.CommandResult()
      End If
      Dim ptStart = gp.Point()

      gp.SetCommandPrompt("End of line")
      gp.SetBasePoint(ptStart, False)
      gp.DrawLineFromPoint(ptStart, True)
      gp.[Get]()
      If gp.CommandResult() <> Rhino.Commands.Result.Success Then
        Return gp.CommandResult()
      End If
      Dim ptEnd = gp.Point()

      Dim v = ptEnd - ptStart
      If v.IsTiny(Rhino.RhinoMath.ZeroTolerance) Then
        Return Rhino.Commands.Result.[Nothing]
      End If

      Dim line = New Line(ptStart, ptEnd)

      Dim conduit = New DrawArrowHeadsConduit(line, screenSize, worldSize)
      ' toggle conduit on/off
      conduit.Enabled = Not conduit.Enabled
      RhinoApp.WriteLine("draw arrowheads conduit enabled = {0}", conduit.Enabled)

      doc.Views.Redraw()
      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace
