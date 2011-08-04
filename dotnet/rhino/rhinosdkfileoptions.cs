using System;

#if RHINO_SDK
namespace Rhino.FileIO
{
  public class FileWriteOptions : IDisposable
  {
    bool m_bDoDelete; // = false; initialized to false by runtime
    IntPtr m_ptr;

    public FileWriteOptions()
    {
      m_ptr = UnsafeNativeMethods.CRhinoFileWriteOptions_New();
      m_bDoDelete = true;
    }

    internal FileWriteOptions(IntPtr ptr)
    {
      m_ptr = ptr;
    }

    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    #region properties
    const int idxSelectedMode = 0;      // Write selected objects only
    const int idxTransformMode = 1;     // Apply GetGransform()
    const int idxRenderMeshesMode = 2;  // Include render meshes
    const int idxPreviewMode = 3;       // Include preview image
    const int idxBitmapsMode = 4;       // Include bitmap table
    const int idxHistoryMode = 5;       // Include history
    //const int idxAsVersion2 = 6;        // Write as version 2
    //const int idxAsVersion3 = 7;        // Write as version 3
    const int idxAsTemplate = 8;        // Write as template
    const int idxBatchMode = 9;         // Suppress dialog boxes
    const int idxGeometryOnly = 10;     // Write geometry only
    //const int idxInstallPlugin = 11;    // Set when the plugin is being installed
    const int idxSaveUserData = 12;     // Set when user data should be saved.
    //const int idxAsVersion4 = 13;       // Write as version 4

    bool GetBool(int which)
    {
      return UnsafeNativeMethods.CRhinoFileWriteOptions_GetBool(m_ptr, which);
    }
    void SetBool(int which, bool value)
    {
      if (m_bDoDelete) // means this is not "const"
        UnsafeNativeMethods.CRhinoFileWriteOptions_SetBool(m_ptr, which, value);
    }

    public bool WriteSelectedObjectsOnly
    {
      get { return GetBool(idxSelectedMode); }
      set { SetBool(idxSelectedMode, value); }
    }

    public bool ApplyTransform
    {
      get { return GetBool(idxTransformMode); }
      set { SetBool(idxTransformMode, value); }
    }

    public bool IncludeRenderMeshes
    {
      get { return GetBool(idxRenderMeshesMode); }
      set { SetBool(idxRenderMeshesMode, value); }
    }

    public bool IncludePreviewImage
    {
      get { return GetBool(idxPreviewMode); }
      set { SetBool(idxPreviewMode, value); }
    }

    public bool IncludeBitmapTable
    {
      get { return GetBool(idxBitmapsMode); }
      set { SetBool(idxBitmapsMode, value); }
    }

    public bool IncludeHistory
    {
      get { return GetBool(idxHistoryMode); }
      set { SetBool(idxHistoryMode, value); }
    }

    public bool WriteAsTemplate
    {
      get { return GetBool(idxAsTemplate); }
      set { SetBool(idxAsTemplate, value); }
    }

    public bool SuppressDialogBoxes
    {
      get { return GetBool(idxBatchMode); }
      set { SetBool(idxBatchMode, value); }
    }

    public bool WriteGeometryOnly
    {
      get { return GetBool(idxGeometryOnly); }
      set { SetBool(idxGeometryOnly, value); }
    }

    public bool WriteUserData
    {
      get { return GetBool(idxSaveUserData); }
      set { SetBool(idxSaveUserData, value); }
    }

    public int FileVersion
    {
      get
      {
        return UnsafeNativeMethods.CRhinoFileWriteOptions_GetFileVersion(m_ptr);
      }
      set
      {
        if (m_bDoDelete)
          UnsafeNativeMethods.CRhinoFileWriteOptions_SetFileVersion(m_ptr, value);
      }
    }

    public Rhino.Geometry.Transform Xform
    {
      get
      {
        Rhino.Geometry.Transform xf = new Rhino.Geometry.Transform();
        UnsafeNativeMethods.CRhinoFileWriteOptions_Transform(m_ptr, true, ref xf);
        return xf;
      }
      set
      {
        if (m_bDoDelete)
          UnsafeNativeMethods.CRhinoFileWriteOptions_Transform(m_ptr, false, ref value);
      }
    }

    #endregion

    #region disposable
    ~FileWriteOptions()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr && m_bDoDelete)
      {
        UnsafeNativeMethods.CRhinoFileWriteOptions_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
      m_bDoDelete = false;
    }
    #endregion
  }

  public class FileReadOptions : IDisposable
  {
    bool m_bDoDelete; // = false; initialized to false by runtime
    IntPtr m_ptr;

    public FileReadOptions()
    {
      m_ptr = UnsafeNativeMethods.CRhinoFileReadOptions_New();
      m_bDoDelete = true;
    }

    //public FileReadOptions(bool import)
    //{
    //  m_ptr = UnsafeNativeMethods.CRhinoFileReadOptions_New2(import);
    //  m_bDoDelete = true;
    //}

    internal FileReadOptions(IntPtr ptr)
    {
      m_ptr = ptr;
    }

    internal IntPtr ConstPointer()
    {
      return m_ptr;
    }

    #region properties
    const int idxImportMode          = 0;
    const int idxOpenMode            = 1;
    const int idxNewMode             = 2;
    const int idxInsertMode          = 3; // Will be set when running the Rhino Insert command
    const int idxImportReferenceMode = 4; // document being imported as a reference 
    const int idxBatchMode           = 5; // no dialogs
    const int idxUseScaleGeometry    = 6;
    const int idxScaleGeometry       = 7;
    // This is to allow a plugin to support only Open or only Import, etc.
    // CRhinoPlugInManager::RegisterPlugIn() requires the plugin to add a filetype
    // but a selective mode plugin doesn't want to add a plugin each time
    // it gets called.  The plugin will add a filetype when InstallPlugin is set.
    //const int idxInstallPlugin = 8;    // Set when the plugin is being installed
    // This is only used when BatchMode is true.  If this is true, and in batch
    // mode then instance definitions with a IDEF_UPDATE_TYPE equal to
    // will be updated when the file open is complete otherwise they will not.
    //const int idxScriptUpdateEmbededInstanceDefinitions = 9;

    bool GetBool(int which)
    {
      return UnsafeNativeMethods.CRhinoFileReadOptions_GetBool(m_ptr, which);
    }
    void SetBool(int which, bool value)
    {
      if (m_bDoDelete) // means this is not "const"
        UnsafeNativeMethods.CRhinoFileReadOptions_SetBool(m_ptr, which, value);
    }

    public bool ImportMode
    {
      get { return GetBool(idxImportMode); }
      set { SetBool(idxImportMode, value); }
    }
    public bool OpenMode
    {
      get { return GetBool(idxOpenMode); }
      set { SetBool(idxOpenMode, value); }
    }
    public bool NewMode
    {
      get { return GetBool(idxNewMode); }
      set { SetBool(idxNewMode, value); }
    }
    public bool InsertMode
    {
      get { return GetBool(idxInsertMode); }
      set { SetBool(idxInsertMode, value); }
    }
    public bool ImportReferenceMode
    {
      get { return GetBool(idxImportReferenceMode); }
      set { SetBool(idxImportReferenceMode, value); }
    }
    public bool BatchMode
    {
      get { return GetBool(idxBatchMode); }
      set { SetBool(idxBatchMode, value); }
    }
    public bool UseScaleGeometry
    {
      get { return GetBool(idxUseScaleGeometry); }
      set { SetBool(idxUseScaleGeometry, value); }
    }
    public bool ScaleGeometry
    {
      get { return GetBool(idxScaleGeometry); }
      set { SetBool(idxScaleGeometry, value); }
    }

    #endregion

    #region disposable
    ~FileReadOptions()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr && m_bDoDelete)
      {
        UnsafeNativeMethods.CRhinoFileReadOptions_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
      m_bDoDelete = false;
    }
    #endregion
  }
}
#endif