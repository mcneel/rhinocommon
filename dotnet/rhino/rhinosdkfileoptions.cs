#pragma warning disable 1591
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

    bool GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts which)
    {
      return UnsafeNativeMethods.CRhinoFileWriteOptions_GetBool(m_ptr, which);
    }
    void SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts which, bool value)
    {
      if (m_bDoDelete) // means this is not "const"
        UnsafeNativeMethods.CRhinoFileWriteOptions_SetBool(m_ptr, which, value);
    }

    public bool WriteSelectedObjectsOnly
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.SelectedMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.SelectedMode, value); }
    }

    public bool ApplyTransform
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.TransformMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.TransformMode, value); }
    }

    public bool IncludeRenderMeshes
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.RenderMeshesMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.RenderMeshesMode, value); }
    }

    public bool IncludePreviewImage
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.PreviewMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.PreviewMode, value); }
    }

    public bool IncludeBitmapTable
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.BitmapsMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.BitmapsMode, value); }
    }

    public bool IncludeHistory
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.HistoryMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.HistoryMode, value); }
    }

    public bool WriteAsTemplate
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.AsTemplate); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.AsTemplate, value); }
    }

    public bool SuppressDialogBoxes
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.BatchMode); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.BatchMode, value); }
    }

    public bool WriteGeometryOnly
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.GeometryOnly); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.GeometryOnly, value); }
    }

    public bool WriteUserData
    {
      get { return GetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.SaveUserData); }
      set { SetBool(UnsafeNativeMethods.FileWriteOptionsBoolConsts.SaveUserData, value); }
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

    public Geometry.Transform Xform
    {
      get
      {
        Geometry.Transform xf = new Geometry.Transform();
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

    bool GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts which)
    {
      return UnsafeNativeMethods.CRhinoFileReadOptions_GetBool(m_ptr, which);
    }
    void SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts which, bool value)
    {
      if (m_bDoDelete) // means this is not "const"
        UnsafeNativeMethods.CRhinoFileReadOptions_SetBool(m_ptr, which, value);
    }

    public bool ImportMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportMode, value); }
    }
    public bool OpenMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.OpenMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.OpenMode, value); }
    }
    public bool NewMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.NewMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.NewMode, value); }
    }
    public bool InsertMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.InsertMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.InsertMode, value); }
    }
    public bool ImportReferenceMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportReferenceMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportReferenceMode, value); }
    }
    public bool BatchMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.BatchMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.BatchMode, value); }
    }
    public bool UseScaleGeometry
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.UseScaleGeometry); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.UseScaleGeometry, value); }
    }
    public bool ScaleGeometry
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ScaleGeometry); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ScaleGeometry, value); }
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