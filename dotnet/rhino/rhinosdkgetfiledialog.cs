using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
// NOTE: Much of this is a combination of rhinoSdkGetFileDialog.h and RhinoSdkUiFileDialog.h
namespace Rhino.UI
{
  //public enum OpenFileDialogType : int
  //{
  //  Standard = 0,
  //  Template = 1,
  //  Bitmap = 2,
  //  RhinoOnly = 3,
  //  ToolBarCollection = 4,
  //  TextFile = 5,
  //  Worksession = 6,
  //  Import = 7,
  //  Attach = 8,
  //  LoadPlugIn = 9
  //}

  //public enum SaveFileDialogType : int
  //{
  //  Standard = 10,
  //  SaveSmall = 11,
  //  Template = 12,
  //  Bitmap = 13,
  //  Export = 14,
  //  Symbol = 15,
  //  ToolBarCollection = 16,
  //  TextFile = 17,
  //  Worksession = 18,
  //  CurrentToolBarCollectionOnly = 19
  //}

  class FileDialogBase
  {
    //bool m_script_mode;
    string m_default_ext = "";
    string m_filename = "";
    string m_filter = "";
    string m_title = "";
    string m_initial_directory = "";

    //public bool InScriptMode
    //{
    //  get { return m_script_mode; }
    //  set { m_script_mode = value; }
    //}

    static string SetString(string setValue, bool trimPeriod)
    {
      string str = "";
      if (null != setValue)
      {
        str = setValue.Trim();
        if (trimPeriod && str.StartsWith(".", StringComparison.OrdinalIgnoreCase))
          str = str.Substring(1);
      }
      return str;
    }

    public string DefaultExt
    {
      get { return m_default_ext; }
      set { m_default_ext = SetString(value, true); }
    }

    public string FileName
    {
      get { return m_filename; }
      set { m_filename = SetString(value, false); }
    }

    public string Title
    {
      get { return m_title; }
      set { m_title = SetString(value, false); }
    }

    public string Filter
    {
      get { return m_filter; }
      set { m_filter = SetString(value, false); }
    }

    public string InitialDirectory
    {
      get { return m_initial_directory; }
      set { m_initial_directory = SetString(value, false); }
    }
  }

  /// <summary>
  /// Similar to the System.Windows.Forms.OpenFileDialog, but with customized
  /// Rhino user interface
  /// </summary>
  public class OpenFileDialog
  {
    readonly FileDialogBase m_base;
    //OpenFileDialogType m_type = OpenFileDialogType.Standard;
    
    /// <summary>Create a new open file dialog</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public OpenFileDialog()
    {
      m_base = new FileDialogBase();
    }

    //public bool InScriptMode
    //{
    //  get { return m_base.InScriptMode; }
    //  set { m_base.InScriptMode = value; }
    //}

    /// <summary>
    /// The default file name extension. The returned string does not include the period
    /// </summary>
    public string DefaultExt
    {
      get { return m_base.DefaultExt; }
      set { m_base.DefaultExt = value; }
    }

    /// <summary>
    /// Gets or sets a string containing the file name selected in the file dialog box. 
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public string FileName
    {
      get { return m_base.FileName; }
      set { m_base.FileName = value; }
    }

    /// <summary>
    /// Gets or sets the file dialog box title.
    /// </summary>
    public string Title
    {
      get { return m_base.Title; }
      set { m_base.Title = value; }
    }

    /// <summary>
    /// Gets or sets the current file name filter string, which determines
    /// the choices that appear in the "Save as file type" or "Files of type"
    /// box in the dialog box. See System.Windows.Forms.FileDialog for details
    /// </summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public string Filter
    {
      get { return m_base.Filter; }
      set { m_base.Filter = value; }
    }

    /// <summary>
    /// Gets or sets the initial directory displayed by the file dialog box.
    /// </summary>
    public string InitialDirectory
    {
      get { return m_base.InitialDirectory; }
      set { m_base.InitialDirectory = value; }
    }

    /// <summary>Show the actual dialog to allow the user to select a file</summary>
    /// <example>
    /// <code source='examples\vbnet\ex_addbackgroundbitmap.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addbackgroundbitmap.cs' lang='cs'/>
    /// <code source='examples\py\ex_addbackgroundbitmap.py' lang='py'/>
    /// </example>
    public System.Windows.Forms.DialogResult ShowDialog()
    {
      string _defExt = DefaultExt;
      string _filename = FileName;
      string _filter = Filter;
      string _initDir = InitialDirectory;
      string _title = Title;
      IntPtr pDialog = UnsafeNativeMethods.CRhinoUiFileDialog_NewOpen(_defExt, _filename, _filter, _initDir, _title);
      int rc = UnsafeNativeMethods.CRhinoUiFileDialog_Show(pDialog);
      IntPtr pFileName = UnsafeNativeMethods.CRhinoUiFileDialog_Filename(pDialog);
      FileName = Marshal.PtrToStringUni(pFileName);
      UnsafeNativeMethods.CRhinoUiFileDialog_Delete(pDialog);
      System.Windows.Forms.DialogResult result = Dialogs.DialogResultFromInt(rc);
      return result;
    }
  }


  /// <summary>
  /// Similar to the System.Windows.Forms.SaveFileDialog, but with customized
  /// Rhino user interface
  /// </summary>
  public class SaveFileDialog
  {
    readonly FileDialogBase m_base;
    //OpenFileDialogType m_type = OpenFileDialogType.Standard;

    public SaveFileDialog()
    {
      m_base = new FileDialogBase();
    }

    //public bool InScriptMode
    //{
    //  get { return m_base.InScriptMode; }
    //  set { m_base.InScriptMode = value; }
    //}

    /// <summary>
    /// The default file name extension. The returned string does not include the period
    /// </summary>
    public string DefaultExt
    {
      get { return m_base.DefaultExt; }
      set { m_base.DefaultExt = value; }
    }

    /// <summary>
    /// Gets or sets a string containing the file name selected in the file dialog box. 
    /// </summary>
    public string FileName
    {
      get { return m_base.FileName; }
      set { m_base.FileName = value; }
    }

    /// <summary>
    /// Gets or sets the file dialog box title.
    /// </summary>
    public string Title
    {
      get { return m_base.Title; }
      set { m_base.Title = value; }
    }

    /// <summary>
    /// Gets or sets the current file name filter string, which determines
    /// the choices that appear in the "Save as file type" or "Files of type"
    /// box in the dialog box. See System.Windows.Forms.FileDialog for details
    /// </summary>
    public string Filter
    {
      get { return m_base.Filter; }
      set { m_base.Filter = value; }
    }

    /// <summary>
    /// Gets or sets the initial directory displayed by the file dialog box.
    /// </summary>
    public string InitialDirectory
    {
      get { return m_base.InitialDirectory; }
      set { m_base.InitialDirectory = value; }
    }

    public System.Windows.Forms.DialogResult ShowDialog()
    {
      string _defExt = DefaultExt;
      string _filename = FileName;
      string _filter = Filter;
      string _initDir = InitialDirectory;
      string _title = Title;
      IntPtr pDialog = UnsafeNativeMethods.CRhinoUiFileDialog_NewSave(_defExt, _filename, _filter, _initDir, _title);
      int rc = UnsafeNativeMethods.CRhinoUiFileDialog_Show(pDialog);
      IntPtr pFileName = UnsafeNativeMethods.CRhinoUiFileDialog_Filename(pDialog);
      FileName = Marshal.PtrToStringUni(pFileName);
      UnsafeNativeMethods.CRhinoUiFileDialog_Delete(pDialog);
      System.Windows.Forms.DialogResult result = Dialogs.DialogResultFromInt(rc);
      return result;
    }
  }
}
#endif