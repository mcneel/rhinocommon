// skip for now, these are typically only used for OpenNURBS File I/O

//namespace Rhino.IO
//{
//  public class ON_3dmRevisionHistory { }
//  public class ON_3dmApplication { }
//  public class ON_3dmProperties { }
//}

namespace Rhino.FileIO
{
//  public class File3dmRevisionHistory { }

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

//  public class File3dmApplication { }
//  public class File3dmProperties { }
}