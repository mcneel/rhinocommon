Imports Rhino
Imports Rhino.Collections
Imports Rhino.Commands
Imports System.Collections.Generic
Imports Rhino.Display
Imports Rhino.Geometry
Imports Rhino.Input.Custom
Imports System.Windows
Imports System.Windows.Controls

Namespace examples_vb
  <System.Runtime.InteropServices.Guid("53C19637-9A3A-465A-AA59-4C2828EB976D")> _
  Public Class ex_extractthumbnail
    Inherits Rhino.Commands.Command
    Public Overrides ReadOnly Property EnglishName() As String
      Get
        Return "vbExtractThumbnail"
      End Get
    End Property

    Protected Overrides Function RunCommand(doc As RhinoDoc, mode As Rhino.Commands.RunMode) As Rhino.Commands.Result
      Dim gf = Rhino.Input.RhinoGet.GetFileName(GetFileNameMode.OpenImage, "*.3dm", "select file", Nothing)
      If gf = String.Empty OrElse Not System.IO.File.Exists(gf) Then
        Return Result.Cancel
      End If

      Dim bitmap = Rhino.FileIO.File3dm.ReadPreviewImage(gf)
      ' convert System.Drawing.Bitmap to BitmapSource
      Dim imgSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero, Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions())

      ' show in WPF window
      Dim w = New Window()
      Dim img = New Image()
      img.Source = imgSrc
      w.Content = img
      w.Show()

      Return Rhino.Commands.Result.Success
    End Function
  End Class
End Namespace