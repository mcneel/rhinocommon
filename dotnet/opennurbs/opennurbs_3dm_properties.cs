using System;

namespace Rhino.FileIO
{
  // skip for now, these are typically only used for OpenNURBS File I/O
  //  public class File3dmRevisionHistory { }
  //  public class File3dmApplication { }
  //  public class File3dmProperties { }

  /// <summary>
  /// Represents the notes information stored in a 3dm file.
  /// </summary>
  public class File3dmNotes
  {
    /// <summary>
    /// Gets or sets the text content of the notes.
    /// </summary>
    public string Notes { get; set; }

    /// <summary>
    /// Gets or sets the notes visibility. If the notes are visible, true; false otherwise.
    /// </summary>
    public bool IsVisible { get; set; }

    /// <summary>
    /// Gets or sets the text format. If the format is HTML, true; false otherwise.
    /// </summary>
    public bool IsHtml { get; set; }

    /// <summary>
    /// Gets or sets the position of the Notes when they were saved.
    /// </summary>
    public System.Drawing.Rectangle WindowRectangle { get; set; }
  }
}