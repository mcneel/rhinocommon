#if RHINO_SDK
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.DocObjects
{
  /// <summary>
  /// Rhino.DocObjects.Tables.BitmapTable entry
  /// </summary>
  //[Serializable]
  public class BitmapEntry : Runtime.CommonObject
  {
    #region members
    // Represents both a CRhinoBitmap and an ON_Bitmap. When m_ptr is
    // null, the object uses m_doc and m_id to look up the const
    // CRhinoBitmap in the bitmap table.
#if RHINO_SDK
    readonly RhinoDoc m_doc;
#endif
    readonly int m_index = -1;
    #endregion

    #region constructors
#if RHINO_SDK
    internal BitmapEntry(int index, RhinoDoc doc)
    {
      m_index = index;
      m_doc = doc;
      m__parent = m_doc;
    }
#endif

    /// <summary>
    /// serialization constructor
    /// </summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected BitmapEntry(SerializationInfo info, StreamingContext context)
      : base (info, context)
    {
    }

    internal BitmapEntry(int index, FileIO.File3dm onxModel)
    {
      m_index = index;
      m__parent = onxModel;
    }

    #endregion

    internal override IntPtr _InternalGetConstPointer()
    {
      throw new NotImplementedException("Tell steve@mcneel.com if you need access to this method");
    }

    internal override IntPtr NonConstPointer()
    {
      throw new NotImplementedException("Tell steve@mcneel.com if you need access to this method");
    }

    internal override IntPtr _InternalDuplicate(out bool applymempressure)
    {
      throw new NotImplementedException("Tell steve@mcneel.com if you need access to this method");
    }

    #region properties
    /// <summary>The name of this bitmap.</summary>
    public string FileName
    {
      get
      {
#if RHINO_SDK
        if (null != m_doc)
          using (var sh = new StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.CRhinoBitmap_GetBitmapName(m_doc.m_docId, m_index, pString);
            return sh.ToString();
          }
#endif
        return string.Empty;
      }
    }

    /// <summary>
    /// Gets a value indicting whether this bitmap is a referenced bitmap. 
    /// Referenced bitmaps are part of referenced documents.
    /// </summary>
    public bool IsReference
    {
      get
      {
#if RHINO_SDK
        if (null == m_doc)
          return false;
        return UnsafeNativeMethods.CRhinoBitmap_IsReference(m_doc.m_docId, m_index);
#else
        return false;
#endif
      }
    }
    #endregion

    #region methods

    /// <summary>
    /// 
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public bool Save(string fileName)
    {
#if RHINO_SDK
      if (null != m_doc)
        return UnsafeNativeMethods.CRhinoBitmap_ExportToFile(m_doc.m_docId, m_index, fileName);
#endif
      return false;
    }
    #endregion
  }
}

namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// Stores the list of bitmaps in a Rhino document.
  /// </summary>
  public sealed class BitmapTable : IEnumerable<BitmapEntry>, Rhino.Collections.IRhinoTable<BitmapEntry>
  {
    readonly RhinoDoc m_doc;
    internal BitmapTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Gets the document that owns this bitmap table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Gets the number of bitmaps in the table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoBitmapTable_BitmapCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// Conceptually, the bitmap table is an array of bitmaps.  The operator[]
    /// can be used to get individual bitmaps.
    /// </summary>
    /// <param name="index">zero based array index.</param>
    /// <returns>
    /// Reference to the bitmap.  If index is out of range, then null is
    /// returned. Note that this reference may become invalid after AddBitmap()
    /// is called.
    /// </returns>
    public BitmapEntry this[int index]
    {
      get
      {
        if (null == Document || index < 0 || index >= Count)
          return null;
        return new BitmapEntry(index, Document);
      }
    }

    /// <summary>
    /// This function first attempts to find the file with "name" on the disk.
    /// If it does find it, "fileName" is set to the full path of the file and
    /// the BitmapEntry returned will be null, even if there was a BitmapEntry
    /// with "name" in the bitmap table.
    /// If the function cannot find the file on the disk, it searches the bitmap
    /// table.  If it finds it, the returned BitmapEntry entry will be the entry
    /// in the table with that name.
    /// Additionally, if "createFile" is true, and an entry is found, the file
    /// will be written to the disk and it's full path will be contained in "fileName".
    /// </summary>
    /// <param name="name">
    /// Name of the file to search for including file extension.
    /// </param>
    /// <param name="createFile">
    /// If this is true, and the file is not found on the disk but is found in
    /// the BitmapTable, then the BitmapEntry will get saved to the Rhino bitmap
    /// file cache and fileName will contain the full path to the cached file.
    /// </param>
    /// <param name="fileName">
    /// The full path to the current location of this file or an empty string
    /// if the file was not found and/or not extracted successfully.
    /// </param>
    /// <returns>
    /// Returns null if "name" was found on the disk.  If name was not found on the disk,
    /// returns the BitmapEntry with the specified name if it is found in the bitmap table
    /// and null if it was not found in the bitmap table.
    /// </returns>
    public BitmapEntry Find(string name, bool createFile, out string fileName)
    {
      fileName = string.Empty;
      int index = -1;
      if (null != Document)
      {
        fileName = Document.FindFile(name);
        if (string.IsNullOrEmpty(fileName))
          index = UnsafeNativeMethods.CRhinoBitmapTable_BitmapFromFileName(Document.m_docId, name);
        if (createFile && string.IsNullOrEmpty(fileName) && index >= 0)
        {
          string tempFileName = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "embedded_files");
          tempFileName = System.IO.Path.Combine(tempFileName, System.IO.Path.GetFileName(name));
          if (System.IO.File.Exists(tempFileName))
            fileName = tempFileName;
          else if (UnsafeNativeMethods.CRhinoBitmap_ExportToFile(m_doc.m_docId, index, tempFileName))
            fileName = tempFileName;
        }
      }
      if (index >= 0)
        return new BitmapEntry(index, m_doc);
      return null;
    }

    /// <summary>Adds a new bitmap with specified name to the bitmap table.</summary>
    /// <param name="bitmapFilename">
    /// If NULL or empty, then a unique name of the form "Bitmap 01" will be automatically created.
    /// </param>
    /// <param name="replaceExisting">
    /// If true and the there is alread a bitmap using the specified name, then that bitmap is replaced.
    /// If false and there is already a bitmap using the specified name, then -1 is returned.
    /// </param>
    /// <returns>
    /// index of new bitmap in table on success. -1 on error.
    /// </returns>
    public int AddBitmap(string bitmapFilename, bool replaceExisting)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_AddBitmap(m_doc.m_docId, bitmapFilename, replaceExisting);
    }

    /// <summary>Deletes a bitmap.</summary>
    /// <param name="bitmapFilename">The bitmap file name.</param>
    /// <returns>
    /// true if successful. false if the bitmap cannot be deleted because it
    /// is the current bitmap or because it bitmap contains active geometry.
    /// </returns>
    public bool DeleteBitmap(string bitmapFilename)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_DeleteBitmap(m_doc.m_docId, bitmapFilename);
    }

    /// <summary>Exports all the bitmaps in the table to files.</summary>
    /// <param name="directoryPath">
    /// full path to the directory where the bitmaps should be saved.
    /// If NULL, a dialog is used to interactively get the directory name.
    /// </param>
    /// <param name="overwrite">0 = no, 1 = yes, 2 = ask.</param>
    /// <returns>Number of bitmaps written.</returns>
    public int ExportToFiles(string directoryPath, int overwrite)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_ExportToFiles(m_doc.m_docId, directoryPath, overwrite);
    }

    /// <summary>Writes a bitmap to a file.</summary>
    /// <param name="index">The index of the bitmap to be written.</param>
    /// <param name="path">
    /// The full path, including file name and extension, name of the file to write.
    /// </param>
    /// <returns>true if successful.</returns>
    public bool ExportToFile(int index, string path)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_ExportToFile(m_doc.m_docId, index, path);
    }

    //[skipping]
    //  void SetRemapIndex( int, // bitmap_index

    #region enumerator
    /// <summary>
    /// BitmapTable enumerator
    /// </summary>
    /// <returns></returns>
    public IEnumerator<BitmapEntry> GetEnumerator()
    {
      return new Collections.TableEnumerator<BitmapTable, BitmapEntry>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Collections.TableEnumerator<BitmapTable, BitmapEntry>(this);
    }
    #endregion

  }
}
#endif