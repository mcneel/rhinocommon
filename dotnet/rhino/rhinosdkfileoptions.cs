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

    /// <summary>
    /// true means we are merging whatever is being read into an existing document.
    ///  This means you need to consider things like:
    /// <para>
    /// If the information being read is in a different unit system, it should be
    /// scaled if UseScaleGeometry is true.
    /// </para>
    /// <para>
    /// There can be existing layers, fonts, materials, dimension styles, hatch
    /// patterns, and so on with the same name as items being read from the file.
    /// </para>
    /// </summary>
    public bool ImportMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportMode, value); }
    }

    /// <summary>
    /// true means we are reading the information into an empty document.  This
    /// means you need to consider things like:
    /// <list type="bullet">
    /// <item><description>Setting the unit system (if the file has a unit system)</description></item>
    /// <item><description>Creating a default layer if one is not there.</description></item>
    /// <item><description>Setting up appropriate views when you're finished reading.</description></item>
    /// </list>
    /// </summary>
    public bool OpenMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.OpenMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.OpenMode, value); }
    }

    /// <summary>
    /// true means we are reading template information in something like
    /// a OnFileNew event.
    /// </summary>
    public bool NewMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.NewMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.NewMode, value); }
    }

    /// <summary>
    /// true means we are reading information that will be used to create an
    /// instance definition or some other type of "inserting" that is supported
    /// by Rhino's "Insert" command.
    /// </summary>
    public bool InsertMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.InsertMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.InsertMode, value); }
    }

    /// <summary>
    /// true means we are reading information for a work session reference model
    /// or a linked instance definition.
    /// </summary>
    public bool ImportReferenceMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportReferenceMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.ImportReferenceMode, value); }
    }

    /// <summary>
    /// true means you cannot ask questions during reading. (no dialogs, no "getters", etc.)
    /// </summary>
    public bool BatchMode
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.BatchMode); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.BatchMode, value); }
    }

    /// <summary>
    /// If this parameter is true, then no questions are asked when unit conversion
    /// scaling is optional and the setting specified by ScaleGeometry is used.
    /// </summary>
    public bool UseScaleGeometry
    {
      get { return GetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.UseScaleGeometry); }
      set { SetBool(UnsafeNativeMethods.FileReadOptionsBoolConsts.UseScaleGeometry, value); }
    }

    /// <summary>
    /// <para>
    /// true: If ImportMode is true and the geometry in the file being read has
    /// a unit system different from the model's unit system, then apply the unit
    /// conversion scale to the file's geometry before adding it to the model.
    /// </para>
    /// <para>
    /// false: Do not scale. Once case where this happens is when an instance
    /// definition is read from a file and the model space instance references
    /// have been scaled. In case the instance definition geometry cannot be
    /// scaled or the net result is that the size of the instance reference
    /// object is scaled by the square of the scale factor.
    /// </para>
    /// </summary>
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