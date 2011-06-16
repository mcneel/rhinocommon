using System;
using System.IO;
using System.Collections.Generic;

namespace Rhino.FileIO
{
  public class File3dm : IDisposable
  {
    IntPtr m_ptr = IntPtr.Zero; //ONX_Model*
    File3dmObjectTable m_object_table;
    File3dmLayerTable m_layer_table;

    internal IntPtr ConstPointer()
    {
      return NonConstPointer(); // all ONX_Models are non-const
    }
    internal IntPtr NonConstPointer()
    {
      if (m_ptr == IntPtr.Zero)
        throw new ObjectDisposedException("File3dm");
      return m_ptr;
    }

    #region statics
    /// <summary>
    /// Read a 3dm file from a specified location
    /// </summary>
    /// <param name="path"></param>
    /// <returns>new File3dm on success, null on error</returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static File3dm Read(string path)
    {
      if (!File.Exists(path))
        throw new FileNotFoundException();
      IntPtr pONX_Model = UnsafeNativeMethods.ONX_Model_ReadFile(path);
      if (pONX_Model == IntPtr.Zero)
        return null;
      return new File3dm(pONX_Model);
    }


    /// <summary>Read only the notes from an existing 3dm file</summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static string ReadNotes(string path)
    {
      if (!File.Exists(path))
        throw new FileNotFoundException();

      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ONX_Model_ReadNotes(path, pString);
        return sh.ToString();
      }
    }
    #endregion

    /// <summary>
    /// Check a model to make sure it is valid.
    /// </summary>
    /// <param name="errors">
    /// if errors are found, a description of the problem is put in this variable.
    /// </param>
    /// <returns>true if the model is valid</returns>
    public bool IsValid(out string errors)
    {
      IntPtr pConstThis = ConstPointer();
      errors = "";
      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        bool rc = UnsafeNativeMethods.ONX_Model_IsValid(pConstThis, pString);
        errors = sh.ToString();
        return rc;
      }
    }

    /// <summary>
    /// Quickly fills in the little details, like making sure there is at least
    /// one layer and table indices make sense.  For a full blown check and repair,
    /// call Audit(true).
    /// </summary>
    public void Polish()
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.ONX_Model_Polish(pThis);
    }

    /// <summary>
    /// Check a model to make sure it is valid and, if possible
    /// and requested, attempt to repair.
    /// </summary>
    /// <param name="attemptRepair">
    /// if true and a problem is found, the problem is repaired
    /// </param>
    /// <param name="repairCount">number of successful repairs</param>
    /// <param name="errors">
    /// if errors are found, a description of the problem is put in this
    /// </param>
    /// <param name="warnings">
    /// If problems were found, warning ids are appended to this list.
    /// 1 (MaterialTable flaws), 2 (LayerTable is not perfect),
    /// 3 (some ObjectTable.Attributes.Id was nil or not unique),
    /// 4 (ObjectTable.IsValid() is false),
    /// 5 (some IDefTable entry has an invalid or duplicate name),
    /// 6 (some IDefTable.ObjectId is not valid),
    /// 7 (some ObjectTable.Geometry is null),
    /// 8 (some ObjectTable.Geometry.IsValid is false),
    /// 9 (some ObjectTable.Attributes is not valid),
    /// 10 (LinetypeTable is not perfect), 11 (LinetypeTable is not perfect),
    /// 12 (some IDefTable.Id was Empty or not unique),
    /// 13 (some TextureMappingTable.MappingId was Empty or not unique),
    /// 14 (some MaterialTable.Id was Empty or not unique),
    /// 15 (some LightTable.LightId was Empty or not unique)
    /// </param>
    /// <returns>
    /// &lt;0 (model has serious errors),
    /// 0 (model is ok),
    /// &gt;0 (number of problems that were found)
    /// </returns>
    public int Audit(bool attemptRepair, out int repairCount, out string errors, out int[] warnings)
    {
      IntPtr pThis = NonConstPointer();
      errors = "";
      repairCount = 0;
      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        Rhino.Runtime.InteropWrappers.SimpleArrayInt w = new Runtime.InteropWrappers.SimpleArrayInt();
        IntPtr pWarnings = w.NonConstPointer();
        int rc = UnsafeNativeMethods.ONX_Model_Audit(pThis, attemptRepair, ref repairCount, pString, pWarnings);
        warnings = w.ToArray();
        w.Dispose();
        errors = sh.ToString();
        return rc;
      }
    }

    //int m_3dm_file_version;
    //int m_3dm_opennurbs_version;

    public string StartSectionComments
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ONX_Model_GetStartSectionComments(pConstThis, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ONX_Model_SetStartSectionComments(pThis, value);
      }
    }

    public File3dmNotes Notes
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          bool visible = false;
          bool html = false;
          int top=0, bottom=0, left=0, right=0;
          UnsafeNativeMethods.ONX_Model_GetNotes(pConstThis, pString, ref visible, ref html, ref left, ref top, ref right, ref bottom);
          File3dmNotes n = new File3dmNotes();
          n.IsHtml = html;
          n.Notes = sh.ToString();
          n.IsVisible = visible;
          n.WindowRectangle = System.Drawing.Rectangle.FromLTRB(left, top, right, bottom);
          return n;
        }
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        File3dmNotes n = (value!=null)? value : new File3dmNotes();
        UnsafeNativeMethods.ONX_Model_SetNotes(pThis, n.Notes, n.IsVisible, n.IsHtml, n.WindowRectangle.Left, n.WindowRectangle.Top, n.WindowRectangle.Right, n.WindowRectangle.Bottom);
      }
    }
    //public File3dmProperties GetProperties();
    //public void SetProperties(File3dnProperties);

    [Obsolete("Use Objects instead. This will be removed from a future WIP")]
    public File3dmObjectTable ObjectTable
    {
      get { return m_object_table ?? (m_object_table = new File3dmObjectTable(this)); }
    }

    public File3dmObjectTable Objects
    {
      get { return m_object_table ?? (m_object_table = new File3dmObjectTable(this)); }
    }

    public IList<Rhino.DocObjects.Layer> Layers
    {
      get { return m_layer_table ?? (m_layer_table = new File3dmLayerTable(this)); }
    }

    #region diagnostic dumps
    const int idxDumpAll = 0;
    const int idxDumpSummary = 1;
    const int idxBitmapTable = 2;
    const int idxTextureMappingTable = 3;
    const int idxMaterialTable = 4;
    const int idxLinetypeTable = 5;
    internal const int idxLayerTable = 6;
    const int idxLightTable = 7;
    const int idxGroupTable = 8;
    const int idxFontTable = 9;
    const int idxDimStyleTable = 10;
    const int idxHatchPatternTable = 11;
    const int idxIDefTable = 12;
    internal const int idxObjectTable = 13;
    const int idxHistoryRecordTable = 14;
    const int idxUserDataTable = 15;
    internal string Dump(int which)
    {
      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.ONX_Model_Dump(pConstThis, which, pString);
        return sh.ToString();
      }
    }

    /// <summary>Text dump of entire model</summary>
    /// <returns></returns>
    public string Dump()
    {
      return Dump(idxDumpAll);
    }

    /// <summary>text dump of model properties and settings</summary>
    /// <returns></returns>
    public string DumpSummary()
    {
      return Dump(idxDumpSummary);
    }
    /*
    /// <summary>text dump of bitmap table</summary>
    /// <returns></returns>
    public string DumpBitmapTable()
    {
      return Dump(idxDumpBitmapTable);
    }

    /// <summary>text dump of texture mapping table</summary>
    /// <returns></returns>
    public string DumpTextureMappingTable()
    {
      return Dump(idxDumpTextureMappingTable);
    }

    /// <summary>text dump of render material table</summary>
    /// <returns></returns>
    public string DumpMaterialTable()
    {
      return Dump(idxDumpMaterialTable);
    }

    /// <summary>text dump of line type table</summary>
    /// <returns></returns>
    public string DumpLinetypeTable()
    {
      return Dump(idxDumpLinetypeTable);
    }

    /// <summary>text dump of layer table</summary>
    /// <returns></returns>
    public string DumpLayerTable()
    {
      return Dump(idxDumpLayerTable);
    }

    /// <summary>text dump of light table</summary>
    /// <returns></returns>
    public string DumpLightTable()
    {
      return Dump(idxDumpLightTable);
    }

    /// <summary>text dump of group table</summary>
    /// <returns></returns>
    public string DumpGroupTable()
    {
      return Dump(idxDumpGroupTable);
    }

    /// <summary>text dump of font table</summary>
    /// <returns></returns>
    public string DumpFontTable()
    {
      return Dump(idxDumpFontTable);
    }

    /// <summary>text dump of dimstyle table</summary>
    /// <returns></returns>
    public string DumpDimStyleTable()
    {
      return Dump(idxDumpDimStyleTable);
    }

    /// <summary>text dump of hatch pattern table</summary>
    /// <returns></returns>
    public string DumpHatchPatternTable()
    {
      return Dump(idxDumpHatchPatternTable);
    }

    /// <summary>text dump of instance definition table</summary>
    /// <returns></returns>
    public string DumpIDefTable()
    {
      return Dump(idxDumpIDefTable);
    }

    /// <summary>text dump of history record table</summary>
    /// <returns></returns>
    public string DumpHistoryRecordTable()
    {
      return Dump(idxDumpHistoryRecordTable);
    }

    /// <summary>text dump of user data table</summary>
    /// <returns></returns>
    public string DumpUserDataTable()
    {
      return Dump(idxDumpUserDataTable);
    }
    */
    #endregion

    #region constructor-dispose logic
    private File3dm() { } //for now... we will need to make public when we have write support in this class
    private File3dm(IntPtr pONX_Model)
    {
      m_ptr = pONX_Model;
    }

    ~File3dm() { Dispose(false); }
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ONX_Model_Delete(m_ptr);
      }
      m_ptr = IntPtr.Zero;
    }
    #endregion
  }


  /// <summary>
  /// Used to store geometry table object definition and attributes in a File3dm
  /// </summary>
  public class File3dmObject
  {
    int m_index;
    File3dm m_parent;
    Rhino.Geometry.GeometryBase m_geometry;

    internal File3dmObject(int index, File3dm parent)
    {
      m_index = index;
      m_parent = parent;
    }

    internal IntPtr GetGeometryConstPointer()
    {
      IntPtr pModel = m_parent.ConstPointer();
      return UnsafeNativeMethods.ONX_Model_ModelObjectGeometry(pModel, m_index);
    }

    public Rhino.Geometry.GeometryBase Geometry
    {
      get
      {
        IntPtr pGeometry = GetGeometryConstPointer();
        if( m_geometry==null || m_geometry.ConstPointer()!=pGeometry )
          m_geometry = Rhino.Geometry.GeometryBase.CreateGeometryHelper(pGeometry, this);
        return m_geometry;
      }
    }
  }

  public class File3dmObjectTable : IEnumerable<File3dmObject>, Rhino.Collections.IRhinoTable<File3dmObject>
  {
    File3dm m_parent;
    internal File3dmObjectTable(File3dm parent)
    {
      m_parent = parent;
    }

    /// <summary>text dump of object table</summary>
    /// <returns></returns>
    public string Dump()
    {
      return m_parent.Dump(File3dm.idxObjectTable);
    }

    #region properties
    /// <summary>
    /// Gets the number of File3dmObjects in this table
    /// </summary>
    public int Count
    {
      get
      {
        IntPtr pConstParent = m_parent.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_TableCount(pConstParent, File3dm.idxObjectTable);
      }
    }

    /// <summary>
    /// Gets the File3dmObject at the given index. 
    /// The index must be valid or an IndexOutOfRangeException will be thrown.
    /// </summary>
    /// <param name="index">Index of File3dmObject to access.</param>
    /// <exception cref="IndexOutOfRangeException">Thrown when the index is invalid.</exception>
    /// <returns>The File3dmObject at [index].</returns>
    public File3dmObject this[int index]
    {
      get
      {
        int count = Count;
        if (index < 0 || index >= count)
          throw new IndexOutOfRangeException();

        if (m_objects == null)
          m_objects = new List<File3dmObject>(count);
        int existing_list_count = m_objects.Count;
        for (int i = existing_list_count; i < count; i++)
        {
          m_objects.Add(new File3dmObject(i, m_parent));
        }

        return m_objects[index];
      }
    }
    List<File3dmObject> m_objects; // = null; initialized to null by runtime
    #endregion

    #region methods

    public File3dmObject[] FindByLayer(string layerName)
    {
      File3dmLayerTable layers = m_parent.Layers as File3dmLayerTable;
      int layer_index = layers.Find(layerName);
      if (layer_index < 0)
        return new File3dmObject[0];

      List<File3dmObject> rc = new List<File3dmObject>();
      int cnt = Count;
      IntPtr pConstModel = m_parent.ConstPointer();
      for (int i = 0; i < cnt; i++)
      {
        if( UnsafeNativeMethods.ONX_Model_ObjectTable_LayerIndexTest(pConstModel, i, layer_index) )
          rc.Add( this[i] );
      }
      return rc.ToArray();
    }


    /// <summary>BoundingBox of every object in this table</summary>
    /// <returns></returns>
    public Rhino.Geometry.BoundingBox GetBoundingBox()
    {
      Rhino.Geometry.BoundingBox bbox = new Geometry.BoundingBox();
      IntPtr pConstModel = m_parent.ConstPointer();
      UnsafeNativeMethods.ONX_Model_BoundingBox(pConstModel, ref bbox);
      return bbox;
    }
    #endregion

    #region IEnumerable Implementation
    public IEnumerator<File3dmObject> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<File3dmObjectTable, File3dmObject>(this);
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<File3dmObjectTable, File3dmObject>(this);
    }
    #endregion
  }

  class File3dmLayerTable : IList<Rhino.DocObjects.Layer>, Rhino.Collections.IRhinoTable<Rhino.DocObjects.Layer>
  {
    File3dm m_parent;
    internal File3dmLayerTable(File3dm parent)
    {
      m_parent = parent;
    }

    public int Find(string name)
    {
      int cnt = Count;
      for (int i = 0; i < cnt; i++)
      {
        if (this[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
          return i;
      }
      return -1;
    }

    public int IndexOf(DocObjects.Layer item)
    {
      File3dm file = item.m__parent as File3dm;
      if (file == m_parent)
        return item.LayerIndex;
      return -1;
    }

    public void Insert(int index, DocObjects.Layer item)
    {
      if (index < 0 || index > Count)
        throw new IndexOutOfRangeException();
      IntPtr pParent = m_parent.NonConstPointer();
      IntPtr pConstLayer = item.ConstPointer();
      UnsafeNativeMethods.ONX_Model_LayerTable_Insert(pParent, pConstLayer, index);
    }

    public void RemoveAt(int index)
    {
      if (index < 0 || index >= Count)
        throw new IndexOutOfRangeException();
      IntPtr pParent = m_parent.NonConstPointer();
      UnsafeNativeMethods.ONX_Model_LayerTable_RemoveAt(pParent, index);
    }

    public DocObjects.Layer this[int index]
    {
      get
      {
        IntPtr pConstParent = m_parent.ConstPointer();
        Guid id = UnsafeNativeMethods.ONX_Model_LayerTable_Id(pConstParent, index);
        if (id==Guid.Empty )
          throw new IndexOutOfRangeException();
        return new DocObjects.Layer(id, m_parent);
      }
      set
      {
        Insert(index, value);
      }
    }

    public void Add(DocObjects.Layer item)
    {
      int index = Count;
      Insert(index, item);
    }

    public void Clear()
    {
      IntPtr pParent = m_parent.NonConstPointer();
      UnsafeNativeMethods.ONX_Model_LayerTable_Clear(pParent);
    }

    public bool Contains(DocObjects.Layer item)
    {
      return IndexOf(item) != -1;
    }

    public void CopyTo(DocObjects.Layer[] array, int arrayIndex)
    {
      int available = array.Length - arrayIndex;
      int cnt = Count;
      if (available < cnt)
        throw new ArgumentException("The number of elements in the source ICollection<T> is greater than the available space from arrayIndex to the end of the destination array.");
      for (int i = 0; i < cnt; i++)
      {
        array[arrayIndex++] = this[i];
      }
    }

    public int Count
    {
      get
      {
        IntPtr pConstParent = m_parent.ConstPointer();
        return UnsafeNativeMethods.ONX_Model_TableCount(pConstParent, File3dm.idxLayerTable);
      }
    }

    public bool IsReadOnly
    {
      get { return false; }
    }

    public bool Remove(DocObjects.Layer item)
    {
      int index = IndexOf(item);
      if (index >= 0)
        RemoveAt(index);
      return (index >= 0);
    }

    public IEnumerator<DocObjects.Layer> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<File3dmLayerTable, Rhino.DocObjects.Layer>(this);
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<File3dmLayerTable, Rhino.DocObjects.Layer>(this);
    }
  }
}