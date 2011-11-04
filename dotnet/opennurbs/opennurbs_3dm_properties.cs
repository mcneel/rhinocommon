#pragma warning disable 1591
using System;

namespace Rhino.FileIO
{
  // skip for now, these are typically only used for OpenNURBS File I/O
  //  public class File3dmRevisionHistory { }
  //  public class File3dmApplication { }
  //  public class File3dmProperties { }

  /// <summary>
  /// Notes information stored in a 3dm file
  /// </summary>
  public class File3dmNotes
  {
    public string Notes { get; set; }
    public bool IsVisible { get; set; }
    public bool IsHtml { get; set; }
    public System.Drawing.Rectangle WindowRectangle { get; set; }
  }
}