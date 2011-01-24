
//  public class RhinoBitmap { }
  //class RHINO_SDK_CLASS CRhinoBitmap
  //{
  //public:
  //  /*
  //  Returns:
  //    Bitmap name.  When the bitmap is an embedded bitmap file, the
  //    full path to the file is returned.
  //  */
  //  const wchar_t* BitmapName() const;

  //  /*
  //  Description:
  //    Gets a CRhinoDib of the bitmap
  //  Parameters:
  //    dib - [in] if not NULL, the dib is put into this bitmap.
  //  Returns:
  //    If successful, a pointer to a CRhinoDib.  The caller
  //    is responsible for deleting this CRhinoDib.
  //  */
  //  class CRhinoDib* Dib( 
  //    class CRhinoDib* dib = NULL
  //    ) const;

  //  /*
  //  Description:
  //    Get array index of the bitmap in the bitmap table.
  //  Returns:
  //     -1:  The bitmap is not in the bitmap table
  //    >=0:  Index of the bitmap in the bitmap table.
  //  */
  //  int BitmapTableIndex() const;

  //  // Runtime index used to sort bitmaps in bitmap dialog
  //  int m_sort_index;   

  //  // Runtime index used when remapping bitmaps for import/export
  //  int m_remap_index;

  //  const ON_Bitmap* Bitmap() const;

  //[moved to function on Bitmap Table]
  //  bool ExportToFile( const wchar_t* path ) const;
  //};

namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// Store the list of bitmaps in a Rhino document
  /// </summary>
  public sealed class BitmapTable
  {
    readonly RhinoDoc m_doc;
    private BitmapTable() { }
    internal BitmapTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this bitmap table</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of bitmaps in the table</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoBitmapTable_BitmapCount(m_doc.m_docId);
      }
    }

    //  Parameters:
    //    i - [in] 0 <= i < BitmapTable.Count
    //  Returns:
    //    Pointer to bitmap or NULL.
    //  const CRhinoBitmap* operator[](int i) const;

    //  const CRhinoBitmap* FindBitmap( 
    //        ON_UUID bitmap_id,
    //        bool bIgnoreDeleted = true
    //        ) const;

    //  Description:
    //    Get a bitmap.
    //  Parameters:
    //    bitmap_name - [in] name of bitmap to search for.  The
    //       search ignores case.  
    //    bitmap - [out] if not NULL, then this bitmap
    //       is filled in.
    //    bLoadFromFile - [in] if true, and the bitmap is on disk
    //       but not in the current table, then the bitmap is
    //       added to the table.     
    //  Returns:
    //    NULL if the bitmap could not be found, otherwise a pointer
    //    to the bitmap is returned.
    //  const CRhinoBitmap* Bitmap( 
    //        const wchar_t* bitmap_filename,
    //        bool bLoadFromFile=false
    //        ) const;

    //  Description:
    //    Adds a new bitmap with specified name to the bitmap table.
    //  Parameters:
    //    bmi - [in] definition of new bitmap. (Copied into the table.)  
    //    bits - [in] pointer to bitmap bits.  If NULL, then it will
    //           be assumed the bitmap bits are after the palette
    //           in bmi.
    //    bitmap_filename - [in] If NULL or empty, then a unique name
    //        of the form "Bitmap 01" will be automatically created.
    //    bReplaceExisting - [in] If true and the there is alread a bitmap
    //        using the specified name, then that bitmap is replace.
    //        If false and there is already a bitmap using the specified
    //        name, then NULL is returned.
    //  Returns:
    //    A pointer to the added bitmap or NULL if the bitmap name
    //    is in use and bReplaceExisting = false.
    //  const CRhinoBitmap* AddBitmap( 
    //     const BITMAPINFO* bmi,
    //     const void* bits,
    //     const wchar_t* bitmap_filename = NULL,
    //     bool bReplaceExisting = false
    //     );


    /// <summary>Adds a new bitmap with specified name to the bitmap table</summary>
    /// <param name="bitmapFilename">
    /// If NULL or empty, then a unique name of the form "Bitmap 01" will be automatically created.
    /// </param>
    /// <param name="replaceExisting">
    /// If true and the there is alread a bitmap using the specified name, then that bitmap is replaced.
    /// If false and there is already a bitmap using the specified name, then -1 is returned.
    /// </param>
    /// <returns>
    /// index of new bitmap in table on success. -1 on error
    /// </returns>
    public int AddBitmap(string bitmapFilename, bool replaceExisting)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_AddBitmap(m_doc.m_docId, bitmapFilename, replaceExisting);
    }

    //  Description:
    //    Adds a new bitmap with specified name to the bitmap table.
    //  Parameters:
    //    pBitmap - [in] an bitmap created with either 
    //       new ON_WindowsBitmapEx() or new ON_EmbeddedBitmap().
    //       If pBitmap->m_name is not set, a name will be provided.
    //    bReplaceExisting - [in] If true and the there is alread a bitmap
    //        using the specified name, then that bitmap is replace.
    //        If false and there is already a bitmap using the specified
    //        name, then NULL is returned.
    //  Returns:
    //    NULL if the bitmap cannot be added or the name is in use.
    //  const CRhinoBitmap* AddBitmap( 
    //      ON_Bitmap* pBitmap,
    //      bool bReplaceExisting = false
    //      );

    /// <summary>deletes bitmap</summary>
    /// <param name="bitmapFilename"></param>
    /// <returns>
    /// true if successful. false if the bitmap cannot be deleted because it
    /// is the current bitmap or because it bitmap contains active geometry.
    /// </returns>
    public bool DeleteBitmap(string bitmapFilename)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_DeleteBitmap(m_doc.m_docId, bitmapFilename);
    }

    //[skipping]
    //  void Sort( int (*compare)(const CRhinoBitmap * const *,const CRhinoBitmap * const *,void*),

    //  // Description:
    //  //   Gets an array of pointers to bitmaps that is sorted by
    //  //   the values of CRhinoBitmap::m_sort_index.
    //  // Parameters:
    //  //   sorted_list - [out] this array is returned with length
    //  //       BitmapTable.Count and is sorted by the values of
    //  //       CRhinoBitmap::m_sort_index.
    //  //   bIgnoreDeleted - [in] TRUE means don't include
    //  //       deleted bitmaps.
    //  // Remarks:
    //  //   Use Sort() to set the values of m_sort_index.
    //  void GetSortedList(
    //    ON_SimpleArray<const CRhinoBitmap*>& sorted_list
    //    ) const;

    /// <summary>Export all the bitmaps in the table to files</summary>
    /// <param name="directoryPath">
    /// full path to the directory where the bitmaps should be saved.
    /// If NULL, a dialog is used to interactively get the directory name.
    /// </param>
    /// <param name="overwrite">0 = no, 1 = yes, 2 = ask</param>
    /// <returns>Number of bitmaps written</returns>
    public int ExportToFiles(string directoryPath, int overwrite)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_ExportToFiles(m_doc.m_docId, directoryPath, overwrite);
    }

    /// <summary>Write a bitmap to a file</summary>
    /// <param name="index"></param>
    /// <param name="path">
    /// full path, including file name and extension, name of the file to write
    /// </param>
    /// <returns>true if successful</returns>
    public bool ExportToFile(int index, string path)
    {
      return UnsafeNativeMethods.CRhinoBitmapTable_ExportToFile(m_doc.m_docId, index, path);
    }

    //[skipping]
    //  void SetRemapIndex( int, // bitmap_index
  }
}
