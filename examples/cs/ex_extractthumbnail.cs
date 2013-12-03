using Rhino;
using Rhino.Collections;
using Rhino.Commands;
using System.Collections.Generic;
using Rhino.Display;
using Rhino.Geometry;
using Rhino.Input.Custom;
using System;
using System.Windows;
using System.Windows.Controls;

namespace examples_cs
{
  [System.Runtime.InteropServices.Guid("9E940874-883B-4537-81C2-F001654DC497")]
  public class ex_extractthumbnail : Rhino.Commands.Command
  {
    public override string EnglishName { get { return "csExtractThumbnail"; } }

    protected override Rhino.Commands.Result RunCommand(RhinoDoc doc, Rhino.Commands.RunMode mode)
    {
      var gf = Rhino.Input.RhinoGet.GetFileName(GetFileNameMode.OpenImage, "*.3dm", "select file", null);
      if (gf == string.Empty || !System.IO.File.Exists(gf))
        return Result.Cancel;

      var bitmap = Rhino.FileIO.File3dm.ReadPreviewImage(gf);
      // convert System.Drawing.Bitmap to BitmapSource
      var imgSrc = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(bitmap.GetHbitmap(), IntPtr.Zero,
        Int32Rect.Empty, System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());

      // show in WPF window
      var w = new Window();
      var img = new Image();
      img.Source = imgSrc;
      w.Content = img;
      w.Show();

      return Rhino.Commands.Result.Success;
    }
  }
}