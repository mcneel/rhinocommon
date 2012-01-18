
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

  /*
  class File3dmProperties : IDisposable
  {
    IntPtr m_p3dmProperties;

    // revision history
    ON_wString m_sCreatedBy;
    ON_wString m_sLastEditedBy;
    struct tm  m_create_time;
    struct tm  m_last_edit_time;
    int        m_revision_count;

    public File3dmNotes Notes { get; set; }

    ON_WindowsBitmap       m_PreviewImage;     // preview image of model

    // application
    ON_wString m_application_name;    // short name like "Rhino 2.0"
    ON_wString m_application_URL;     // URL
    ON_wString m_application_details; // whatever you want
  }
  */
}