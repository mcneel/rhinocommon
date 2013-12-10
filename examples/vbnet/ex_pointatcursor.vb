Imports Rhino

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("B1999883-CE95-4727-A047-4CD3881AD866")> _
  Public Class ex_pointatcursor
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbPointAtCursor"
      End Get
    End Property

    <System.Runtime.InteropServices.DllImport("user32.dll")> _
    Public Shared Function GetCursorPos(ByRef pt As System.Drawing.Point) As Boolean
    End Function

    <System.Runtime.InteropServices.DllImport("user32.dll")> _
    Public Shared Function ScreenToClient(hWnd As IntPtr, ByRef pt As System.Drawing.Point) As Boolean
    End Function

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim rslt = Rhino.Commands.Result.Failure
      Dim view = doc.Views.ActiveView
      If view IsNot Nothing Then
        Dim pt As System.Drawing.Point
        If GetCursorPos(pt) AndAlso ScreenToClient(view.Handle, pt) Then
          Dim xform = view.ActiveViewport.GetTransform(Rhino.DocObjects.CoordinateSystem.Screen, Rhino.DocObjects.CoordinateSystem.World)
          If xform <> Nothing Then
            Dim point = New Rhino.Geometry.Point3d(pt.X, pt.Y, 0.0)
            RhinoApp.WriteLine([String].Format("screen point: ({0}, {1}, {2})", point.X, point.Y, point.Z))
            point.Transform(xform)
            RhinoApp.WriteLine([String].Format("world point: ({0}, {1}, {2})", point.X, point.Y, point.Z))
            rslt = Rhino.Commands.Result.Success
          End If
        End If
      End If
      Return rslt
    End Function
  End Class
End Namespace