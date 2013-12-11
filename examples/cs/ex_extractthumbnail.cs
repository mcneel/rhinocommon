using Rhino;
using Rhino.Commands;
using Rhino.Input;
using Rhino.Input.Custom;
using System;
using System.Windows;
using System.Windows.Controls;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("9E940874-883B-4537-81C2-F001654DC497")]
  public class ExtractThumbnailCommand : Command
  {
    public override string EnglishName { get { return "csExtractThumbnail"; } }

    protected override Result RunCommand(RhinoDoc doc, RunMode mode)
    {
      var gf = RhinoGet.GetFileName(GetFileNameMode.OpenImage, "*.3dm", "select file", null);
      if (gf == string.Empty || !System.IO.File.Exists(gf))
        return Result.Cancel;

      var bitmap = Rhino.FileIO.File3dm.ReadPreviewImage(gf);
      // convert System.Drawing.Bitmap to BitmapSource
      var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero,
        Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

      // show in WPF window
      var window = new Window();
      var image = new Image {Source = imageSource};
      window.Content = image;
      window.Show();

      return Result.Success;
    }
  }
}