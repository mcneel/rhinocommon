using System;
using System.Collections.Generic;
using System.Text;

namespace Rhino.Collections
{
  ///<summary>
  /// Dictionary Structure
  /// BEGINCHUNK (TCODE_ANONYMOUS_CHUNK)
  /// |- version (int)
  /// |- entry count (int)
  ///    for entry count entries
  ///    |- BEGINCHUNK (TCODE_ANONYMOUS_CHUNK)
  ///    |- type (OnBinaryArchiveDictionary::ItemType)
  ///    |- key (string)
  ///    |- entry contents
  ///    |- ENDCHUNK (TCODE_ANONYMOUS_CHUNK)
  /// ENDCHUNK (TCODE_ANONYMOUS_CHUNK)
  ///</summary>
  public class ArchivableDictionary
  {
    private enum ItemType : int
    {
      // values <= 0 are considered bogus
      // each supported object type has an associated ItemType enum value
      // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      // NEVER EVER Change ItemType values as this will break I/O code
      // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
      itUndefined = 0,
      // some basic types
      itBool = 1, // bool
      itByte = 2, // unsigned char
      itSByte = 3, // char
      itShort = 4, // short
      itUShort = 5, // unsigned short
      itInt32 = 6, // int
      itUInt32 = 7, // unsigned int
      itInt64 = 8, // time_t
      itSingle = 9, // float
      itDouble = 10, // double
      itGuid = 11,
      itString = 12,

      // array of basic .NET data types
      itArrayBool = 13,
      itArrayByte = 14,
      itArraySByte = 15,
      itArrayShort = 16,
      itArrayInt32 = 17,
      itArraySingle = 18,
      itArrayDouble = 19,
      itArrayGuid = 20,
      itArrayString = 21,

      // System::Drawing structs
      itColor = 22,
      itPoint = 23,
      itPointF = 24,
      itRectangle = 25,
      itRectangleF = 26,
      itSize = 27,
      itSizeF = 28,
      itFont = 29,

      // RMA::OpenNURBS::ValueTypes structs
      itInterval = 30,
      itPoint2d = 31,
      itPoint3d = 32,
      itPoint4d = 33,
      itVector2d = 34,
      itVector3d = 35,
      itBoundingBox = 36,
      itRay3d = 37,
      itPlaneEquation = 38,
      itXform = 39,
      itPlane = 40,
      itLine = 41,
      itPoint3f = 42,
      itVector3f = 43,

      // RMA::OpenNURBS classes
      itOnBinaryArchiveDictionary = 44,
      itOnObject = 45, // don't use this anymore
      itOnMeshParameters = 46,
      itOnGeometry = 47,
      itMAXVALUE = 47
    }

    int m_version;
    string m_name;

    public int Version
    {
      get { return m_version; }
      set { m_version = value; }
    }

    public string Name
    {
      get { return m_name; }
      set { m_name = value; }
    }

    // I don't think this needs to be public
    static Guid RhinoDotNetDictionaryId
    {
      get
      {
        // matches id used by old Rhino.NET
        return new Guid("21EE7933-1E2D-4047-869E-6BDBF986EA11");
      }
    }

    /// <summary>Create an instance of a dictionary for writing to a 3dm archive</summary>
    public ArchivableDictionary()
    {
      m_version = 0;
      m_name = String.Empty;
    }

    /// <summary>Create an instance of a dictionary for writing to a 3dm archive</summary>
    /// <param name="version">
    /// Custom version used to help the plug-in developer determine which version of
    /// a dictionary is being written. One good way to write version information is to
    /// use a date style integer (YYYYMMDD)
    /// </param>
    public ArchivableDictionary(int version)
    {
      m_version = version;
      m_name = String.Empty;
    }

    ///<summary>Create an instance of a dictionary for writing to a 3dm archive</summary>
    ///<param name="version">
    /// custom version used to help the plug-in developer determine which version of
    /// a dictionary is being written. One good way to write version information is to
    /// use a date style integer (YYYYMMDD)
    ///</param>
    ///<param name="name">
    /// Optional name to associate with this dictionary.
    /// NOTE: if this dictionary is set as a subdictionary, the name will be changed to
    /// the subdictionary key entry
    ///</param>
    public ArchivableDictionary(int version, string name)
    {
      m_version = version;
      if (String.IsNullOrEmpty(name))
        m_name = String.Empty;
      else
        m_name = name;
    }

    ///<summary>Read a dictionary from an archive.</summary>
    ///<param name='archive'>
    ///The archive to read from. The archive position should be at the beginning of
    ///the dictionary
    ///</param>
    ///<returns>new filled dictionary on success. null on failure</returns>
    internal static ArchivableDictionary Read(Rhino.FileIO.BinaryArchiveReader archive)
    {
      Guid dictionary_id;
      uint version;
      string dictionary_name;
      if( !archive.BeginReadDictionary(out dictionary_id, out version, out dictionary_name) )
        return null;

      // make sure this dictionary is one that was written by Rhino.NET
      if( dictionary_id != RhinoDotNetDictionaryId )
      {
        archive.EndReadDictionary();
        return null;
      }

      ArchivableDictionary dict = new ArchivableDictionary((int)version, dictionary_name);

      const int MAX_ITYPE = (int)ItemType.itMAXVALUE;
      while( true )
      {
        int iType;
        string key;
        int read_rc = archive.BeginReadDictionaryEntry( out iType, out key );
        if( 0 == read_rc )
          return null;
        if( 1 != read_rc )
          break;

        // Make sure this type is readable with the current version of RhinoCommon.
        // ItemTypes wiil be expanded with future supported type
        bool readableType = iType > 0 && iType <= MAX_ITYPE;
        if( readableType )
        {
          ItemType _it = (ItemType)iType;
          dict.ReadAndSetItemType( _it, key, archive );
        }
        if (!archive.EndReadDictionaryEntry())
          return null;
      }
      archive.EndReadDictionary();
      return dict;      
    }

    // private helper function for Read. Reads ItemType specific items from an archive
    // should ONLY be called by Read function
    private bool ReadAndSetItemType(ItemType it, string key, Rhino.FileIO.BinaryArchiveReader archive)
    {
      if( String.IsNullOrEmpty(key) || archive==null )
        return false;
      
      bool rc = false;
      switch(it)
      {
        case ItemType.itBool: //1
          {
            bool val = archive.ReadBool();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itByte: //2
          {
            byte val = archive.ReadByte();
            rc = SetItem(key, val);
          }
          break;

        case ItemType.itSByte: //3
          {
            sbyte val = archive.ReadSByte();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itShort: //4
          {
            short val = archive.ReadShort();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itUShort: //5
          {
            ushort val = archive.ReadUShort();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itInt32: //6
          {
            int val = archive.ReadInt();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itUInt32: //7
          {
            uint val = archive.ReadUInt();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itInt64: //8
          {
            Int64 val=archive.ReadInt64();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itSingle: //9
          {
            float val = archive.ReadSingle();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itDouble: //10
          {
            double val = archive.ReadDouble();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itGuid: //11
          {
            Guid val = archive.ReadGuid();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itString: //12
          {
            string val = archive.ReadString();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itArrayBool: //13
          {
            bool[] arr = archive.ReadBoolArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayByte: //14
          {
            byte[] arr = archive.ReadByteArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArraySByte: //15
          {
            sbyte[] arr = archive.ReadSByteArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayShort: //16
          {
            short[] arr = archive.ReadShortArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayInt32: //17
          {
            int[] arr = archive.ReadIntArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArraySingle: //18
          {
            float[] arr = archive.ReadSingleArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayDouble: //19
          {
            double[] arr = archive.ReadDoubleArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayGuid: //20
          {
            Guid[] arr = archive.ReadGuidArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itArrayString: //21
          {
            string[] arr = archive.ReadStringArray();
            rc = SetItem(key, arr);
          }
          break;
        case ItemType.itColor: //22
          {
            System.Drawing.Color val = archive.ReadColor();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPoint: //23
          {
            System.Drawing.Point val = archive.ReadPoint();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPointF: //24
          {
            System.Drawing.PointF val = archive.ReadPointF();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itRectangle: //25
          {
            System.Drawing.Rectangle val = archive.ReadRectangle();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itRectangleF: //26
          {
            System.Drawing.RectangleF val = archive.ReadRectangleF();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itSize: //27
          {
            System.Drawing.Size val = archive.ReadSize();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itSizeF: //28
          {
            System.Drawing.SizeF val = archive.ReadSizeF();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itFont: //29
          {
            System.Drawing.Font val = archive.ReadFont();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itInterval: //30
          {
            Rhino.Geometry.Interval val = archive.ReadInterval();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPoint2d: //31
          {
            Rhino.Geometry.Point2d val = archive.ReadPoint2d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPoint3d: //32
          {
            Rhino.Geometry.Point3d val = archive.ReadPoint3d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPoint4d: //33
          {
            Rhino.Geometry.Point4d val = archive.ReadPoint4d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itVector2d: //34
          {
            Rhino.Geometry.Vector2d val = archive.ReadVector2d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itVector3d: //35
          {
            Rhino.Geometry.Vector3d val = archive.ReadVector3d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itBoundingBox: //36
          {
            Rhino.Geometry.BoundingBox val = archive.ReadBoundingBox();
              rc = SetItem(key, val);
          }
          break;
        case ItemType.itRay3d: //37
          {
            Rhino.Geometry.Ray3d val = archive.ReadRay3d();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPlaneEquation: //38
          {
            double[] val = archive.ReadPlaneEquation();
            rc = SetPlaneEquation(key, val);
          }
          break;
        case ItemType.itXform: //39
          {
            Rhino.Geometry.Transform val = archive.ReadTransform();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPlane: //40
          {
            Rhino.Geometry.Plane val = archive.ReadPlane();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itLine: //41
          {
            Rhino.Geometry.Line val = archive.ReadLine();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itPoint3f: //42
          {
            Rhino.Geometry.Point3f val = archive.ReadPoint3f();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itVector3f: //43
          {
            Rhino.Geometry.Vector3f val = archive.ReadVector3f();
            rc = SetItem(key, val);
          }
          break;
        case ItemType.itOnBinaryArchiveDictionary: //44
          {
            ArchivableDictionary dict = Read(archive);
            if( dict != null )
              rc = SetItem(key, dict);
          }
          break;
        case ItemType.itOnObject: //45
          {
            int read_rc = 0;
            IntPtr pObject = UnsafeNativeMethods.ON_BinaryArchive_ReadObject(archive.NonConstPointer(), ref read_rc);
            Rhino.Geometry.GeometryBase geom = Rhino.Geometry.GeometryBase.CreateGeometryHelper(pObject, null);
            if( geom!=null )
            {
              rc = SetItem(key, geom);
            }
            else
            {
              // some other ON_Object
              UnsafeNativeMethods.ON_Object_Delete(pObject);
            }
          }
          break;
        case ItemType.itOnMeshParameters: //46
          {
            Rhino.Geometry.MeshingParameters val = archive.ReadMeshingParameters();
            rc = SetItem(key, val);
          }
          break;
      }
      return rc;
    }

    /// <summary>
    /// Write this dictionary to an archive
    /// </summary>
    /// <param name="archive">The archive to write to</param>
    /// <returns>true on success</returns>
    internal bool Write(Rhino.FileIO.BinaryArchiveWriter archive)
    {
      uint version = (uint)m_version;
      if( !archive.BeginWriteDictionary( RhinoDotNetDictionaryId, version, m_name ) )
        return false;

      foreach (KeyValuePair<string, DictionaryItem> kvp in m_items)
    	{
        DictionaryItem item = kvp.Value;
        if( item==null || string.IsNullOrEmpty(kvp.Key) )
          continue;
        if( item.m_type==ItemType.itUndefined || item.m_value==null )
          continue;
        if( !WriteItem(archive, kvp.Key, item.m_type, item.m_value) )
          return false;
    	}
      return archive.EndWriteDictionary();
    }

    // private helper function for Write. Should ONLY be called by Write function
    private bool WriteItem(Rhino.FileIO.BinaryArchiveWriter archive, string entry_name, ItemType it, object val)
    {
      if (archive == null || it == ItemType.itUndefined || string.IsNullOrEmpty(entry_name) || val == null)
        return false;

      if (!archive.BeginWriteDictionaryEntry((int)it, entry_name))
        return false;

      bool rc = true;
      switch (it)
      {
        case ItemType.itBool: // 1
          archive.WriteBool((bool)val);
          break;
        case ItemType.itByte: // 2
          archive.WriteByte((byte)val);
          break;
        case ItemType.itSByte: // 3
          archive.WriteSByte((sbyte)val);
          break;
        case ItemType.itShort: // 4
          archive.WriteShort((short)val);
          break;
        case ItemType.itUShort: // 5
          archive.WriteUShort((ushort)val);
          break;
        case ItemType.itInt32: // 6
          archive.WriteInt((int)val);
          break;
        case ItemType.itUInt32: // 7
          archive.WriteUInt((uint)val);
          break;
        case ItemType.itInt64: // 8
          archive.WriteInt64((Int64)val);
          break;
        case ItemType.itSingle: // 9
          archive.WriteSingle((float)val);
          break;
        case ItemType.itDouble: // 10
          archive.WriteDouble((double)val);
          break;
        case ItemType.itGuid: // 11
          archive.WriteGuid((Guid)val);
          break;
        case ItemType.itString: // 12
          archive.WriteString((String)val);
          break;
        case ItemType.itArrayBool: // 13
          archive.WriteBoolArray((IEnumerable<bool>)val);
          break;
        case ItemType.itArrayByte: // 14
          archive.WriteByteArray((IEnumerable<byte>)val);
          break;
        case ItemType.itArraySByte: // 15
          archive.WriteSByteArray((IEnumerable<sbyte>)val);
          break;
        case ItemType.itArrayShort: // 16
          archive.WriteShortArray((IEnumerable<short>)val);
          break;
        case ItemType.itArrayInt32: // 17
          archive.WriteIntArray((IEnumerable<int>)val);
          break;
        case ItemType.itArraySingle: // 18
          archive.WriteSingleArray((IEnumerable<float>)val);
          break;
        case ItemType.itArrayDouble: // 19
          archive.WriteDoubleArray((IEnumerable<double>)val);
          break;
        case ItemType.itArrayGuid: // 20
          archive.WriteGuidArray((IEnumerable<Guid>)val);
          break;
        case ItemType.itArrayString: // 21
          archive.WriteStringArray((IEnumerable<string>)val);
          break;
        case ItemType.itColor: // 22
          archive.WriteColor((System.Drawing.Color)val);
          break;
        case ItemType.itPoint: // 23
          archive.WritePoint((System.Drawing.Point)val);
          break;
        case ItemType.itPointF: // 24
          archive.WritePointF((System.Drawing.PointF)val);
          break;
        case ItemType.itRectangle: // 25
          archive.WriteRectangle((System.Drawing.Rectangle)val);
          break;
        case ItemType.itRectangleF: // 26
          archive.WriteRectangleF((System.Drawing.RectangleF)val);
          break;
        case ItemType.itSize: // 27
          archive.WriteSize((System.Drawing.Size)val);
          break;
        case ItemType.itSizeF: // 28
          archive.WriteSizeF((System.Drawing.SizeF)val);
          break;
        case ItemType.itFont: // 29
          archive.WriteFont((System.Drawing.Font)val);
          break;
        case ItemType.itInterval: // 30
          archive.WriteInterval((Rhino.Geometry.Interval)val);
          break;
        case ItemType.itPoint2d: // 31
          archive.WritePoint2d((Rhino.Geometry.Point2d)val);
          break;
        case ItemType.itPoint3d: // 32
          archive.WritePoint3d((Rhino.Geometry.Point3d)val);
          break;
        case ItemType.itPoint4d: // 33
          archive.WritePoint4d((Rhino.Geometry.Point4d)val);
          break;
        case ItemType.itVector2d: // 34
          archive.WriteVector2d((Rhino.Geometry.Vector2d)val);
          break;
        case ItemType.itVector3d: // 35
          archive.WriteVector3d((Rhino.Geometry.Vector3d)val);
          break;
        case ItemType.itBoundingBox: // 36
          archive.WriteBoundingBox((Rhino.Geometry.BoundingBox)val);
          break;
        case ItemType.itRay3d: // 37
          archive.WriteRay3d((Rhino.Geometry.Ray3d)val);
          break;
        case ItemType.itPlaneEquation: // 38
          archive.WritePlaneEquation((double[])val);
          break;
        case ItemType.itXform: // 39
          archive.WriteTransform((Rhino.Geometry.Transform)val);
          break;
        case ItemType.itPlane: // 40
          archive.WritePlane((Rhino.Geometry.Plane)val);
          break;
        case ItemType.itLine: // 41
          archive.WriteLine((Rhino.Geometry.Line)val);
          break;
        case ItemType.itPoint3f: // 42
          archive.WritePoint3f((Rhino.Geometry.Point3f)val);
          break;
        case ItemType.itVector3f: // 43
          archive.WriteVector3f((Rhino.Geometry.Vector3f)val);
          break;
        case ItemType.itOnBinaryArchiveDictionary: // 44
          ArchivableDictionary dict = (ArchivableDictionary)val;
          dict.Write(archive);
          break;
        case ItemType.itOnObject: // 45
          break; // skip
        case ItemType.itOnMeshParameters: // 46
          archive.WriteMeshingParameters((Rhino.Geometry.MeshingParameters)val);
          break;
        case ItemType.itOnGeometry: // 47
          archive.WriteGeometry((Rhino.Geometry.GeometryBase)val);
          break;
      }
      rc = rc && archive.EndWriteDictionaryEntry();
      return rc;    
    }

    /// <summary>Entry names</summary>
    public string[] Keys
    {
      get
      {
        string[] rc = new string[m_items.Keys.Count];
        m_items.Keys.CopyTo(rc, 0);
        return rc;
      }
    }

    public bool ContainsKey(string key)
    {
      return m_items.ContainsKey(key);
    }
    public object this[string key]
    {
      get
      {
        object rc = m_items[key];
        if (null == rc)
          return null;
        return ((DictionaryItem)rc).m_value;
      }
    }

    public void Clear()
    {
      m_items.Clear();
    }

    public void Remove(string key)
    {
      m_items.Remove(key);
    }

    public int Count
    {
      get
      {
        return m_items.Count;
      }
    }

    public bool TryGetValue(string key, out object value)
    {
      DictionaryItem di;
      if (m_items.TryGetValue(key, out di))
      {
        value = di.m_value;
        return true;
      }
      value = null;
      return false;
    }

    public bool SetItem(string key, bool val) { return SetItem(key, ItemType.itBool, val); }
    public bool SetItem(string key, byte val) { return SetItem(key, ItemType.itByte, val); }
    [CLSCompliant(false)]
    public bool SetItem(string key, sbyte val) { return SetItem(key, ItemType.itSByte, val); }
    public bool SetItem(string key, short val) { return SetItem(key, ItemType.itShort, val); }
    [CLSCompliant(false)]
    public bool SetItem(string key, ushort val) { return SetItem(key, ItemType.itUShort, val); }
    public bool SetItem(string key, int val) { return SetItem(key, ItemType.itInt32, val); }
    [CLSCompliant(false)]
    public bool SetItem(string key, uint val) { return SetItem(key, ItemType.itUInt32, val); }
    public bool SetItem(string key, Int64 val) { return SetItem(key, ItemType.itInt64, val); }
    public bool SetItem(string key, float val) { return SetItem(key, ItemType.itSingle, val); }
    public bool SetItem(string key, double val) { return SetItem(key, ItemType.itDouble, val); }
    public bool SetItem(string key, Guid val) { return SetItem(key, ItemType.itGuid, val); }
    public bool SetItem(string key, string val) { return SetItem(key, ItemType.itString, val); }
    public bool SetItem(string key, IEnumerable<bool> val) { return SetItem(key, ItemType.itArrayBool, val); }
    public bool SetItem(string key, IEnumerable<byte> val) { return SetItem(key, ItemType.itArrayByte, val); }
    public bool SetItem(string key, IEnumerable<sbyte> val) { return SetItem(key, ItemType.itArraySByte, val); }
    public bool SetItem(string key, IEnumerable<short> val) { return SetItem(key, ItemType.itArrayShort, val); }
    public bool SetItem(string key, IEnumerable<int> val) { return SetItem(key, ItemType.itArrayInt32, val); }
    public bool SetItem(string key, IEnumerable<float> val) { return SetItem(key, ItemType.itArraySingle, val); }
    public bool SetItem(string key, IEnumerable<double> val) { return SetItem(key, ItemType.itArrayDouble, val); }
    public bool SetItem(string key, IEnumerable<Guid> val) { return SetItem(key, ItemType.itArrayGuid, val); }
    public bool SetItem(string key, IEnumerable<string> val) { return SetItem(key, ItemType.itArrayString, val); }
    public bool SetItem(string key, System.Drawing.Color val) { return SetItem(key, ItemType.itColor, val); }
    public bool SetItem(string key, System.Drawing.Point val) { return SetItem(key, ItemType.itPoint, val); }
    public bool SetItem(string key, System.Drawing.PointF val) { return SetItem(key, ItemType.itPointF, val); }
    public bool SetItem(string key, System.Drawing.Rectangle val) { return SetItem(key, ItemType.itRectangle, val); }
    public bool SetItem(string key, System.Drawing.RectangleF val) { return SetItem(key, ItemType.itRectangleF, val); }
    public bool SetItem(string key, System.Drawing.Size val) { return SetItem(key, ItemType.itSize, val); }
    public bool SetItem(string key, System.Drawing.SizeF val) { return SetItem(key, ItemType.itSizeF, val); }
    public bool SetItem(string key, System.Drawing.Font val) { return SetItem(key, ItemType.itFont, val); }
    public bool SetItem(string key, Rhino.Geometry.Interval val) { return SetItem(key, ItemType.itInterval, val); }
    public bool SetItem(string key, Rhino.Geometry.Point2d val) { return SetItem(key, ItemType.itPoint2d, val); }
    public bool SetItem(string key, Rhino.Geometry.Point3d val) { return SetItem(key, ItemType.itPoint3d, val); }
    public bool SetItem(string key, Rhino.Geometry.Point4d val) { return SetItem(key, ItemType.itPoint4d, val); }
    public bool SetItem(string key, Rhino.Geometry.Vector2d val) { return SetItem(key, ItemType.itVector2d, val); }
    public bool SetItem(string key, Rhino.Geometry.Vector3d val) { return SetItem(key, ItemType.itVector3d, val); }
    public bool SetItem(string key, Rhino.Geometry.BoundingBox val) { return SetItem(key, ItemType.itBoundingBox, val); }
    public bool SetItem(string key, Rhino.Geometry.Ray3d val) { return SetItem(key, ItemType.itRay3d, val); }
    bool SetPlaneEquation(string key, double[] eq) { return SetItem(key, ItemType.itPlaneEquation, eq); }
    public bool SetItem(string key, Rhino.Geometry.Transform val) { return SetItem(key, ItemType.itXform, val); }
    public bool SetItem(string key, Rhino.Geometry.Plane val) { return SetItem(key, ItemType.itPlane, val); }
    public bool SetItem(string key, Rhino.Geometry.Line val) { return SetItem(key, ItemType.itLine, val); }
    public bool SetItem(string key, Rhino.Geometry.Point3f val) { return SetItem(key, ItemType.itPoint3f, val); }
    public bool SetItem(string key, Rhino.Geometry.Vector3f val) { return SetItem(key, ItemType.itVector3f, val); }
    public bool SetItem(string key, ArchivableDictionary val) { return SetItem(key, ItemType.itOnBinaryArchiveDictionary, val); }
    public bool SetItem(string key, Rhino.Geometry.MeshingParameters val) { return SetItem(key, ItemType.itOnMeshParameters, val); }
    public bool SetItem(string key, Rhino.Geometry.GeometryBase val) { return SetItem(key, ItemType.itOnGeometry, val); }

    bool SetItem(string key, ItemType it, object val)
    {
      if (string.IsNullOrEmpty(key) || val == null || it == ItemType.itUndefined)
        return false;
      m_items[key] = new DictionaryItem(it, val);
      return true;
    }

    System.Collections.Generic.Dictionary<string, DictionaryItem> m_items = new Dictionary<string, DictionaryItem>();
    private class DictionaryItem
    {
      public DictionaryItem(ItemType t, object val)
      {
        m_type = t;
        m_value = val;
      }
      public ItemType m_type;
      public object m_value;
    }
  }
}

namespace Rhino.FileIO
{
  /// <summary>
  /// Thrown by BinaryArchiveReader and BinaryArchiveWriter classes when
  /// an IO error has occured
  /// </summary>
  public class BinaryArchiveException : System.IO.IOException
  {
    public BinaryArchiveException(string message)
      : base(message)
    { }
  }
  //public class ON_3DM_CHUNK { }
  //public class ON_3dmGoo { }
  //public class ON_BinaryFile { }

  public class BinaryArchiveWriter
  {
    IntPtr m_ptr; // ON_BinaryArchive*
    internal BinaryArchiveWriter(IntPtr pArchive)
    {
      m_ptr = pArchive;
    }

    internal void ClearPointer()
    {
      m_ptr = IntPtr.Zero;
    }

    bool m_bWriteErrorOccured = false;
    public bool WriteErrorOccured
    {
      get { return m_bWriteErrorOccured; }
      set
      {
        // 17 Sept. 2010 S. Baer
        // ?? should we only allow going from false to true??
        m_bWriteErrorOccured = value;
      }
    }

    /// <summary>
    /// If a 3dm archive is being read or written, then this is the
    /// version of the 3dm archive format (1, 2, 3, 4 or 5).
    /// 0     a 3dm archive is not being read/written
    /// 1     a version 1 3dm archive is being read/written
    /// 2     a version 2 3dm archive is being read/written
    /// 3     a version 3 3dm archive is being read/written
    /// 4     a version 4 3dm archive is being read/written
    /// 5     an old version 5 3dm archive is being read
    /// 50    a version 5 3dm archive is being read/written
    /// </summary>
    public int Archive3dmVersion
    {
      get
      {
        return UnsafeNativeMethods.ON_BinaryArchive_Archive3dmVersion(m_ptr);
      }
    }

    /// <summary>
    /// A chunk version is a single byte that encodes a major.minor
    /// version number.  Useful when creating I/O code for 3dm chunks
    /// that may change in the future.  Increment the minor version 
    /// number if new information is added to the end of the chunk. 
    /// Increment the major version if the format of the chunk changes
    /// in some other way.
    /// </summary>
    /// <param name="major">0 to 15</param>
    /// <param name="minor">0 to 16</param>
    /// <returns>true on successful read</returns>
    public void Write3dmChunkVersion(int major, int minor)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_Write3dmChunkVersion(m_ptr, major, minor);
      if (m_bWriteErrorOccured )
        throw new BinaryArchiveException("Write3dmChunkVersion failed");
    }

    public void WriteDictionary(Rhino.Collections.ArchivableDictionary dictionary)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !dictionary.Write(this);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteDictionary failed");
    }

    public void WriteBool(bool value)
    {
      if (!UnsafeNativeMethods.ON_BinaryArchive_WriteBool(m_ptr, value))
        throw new BinaryArchiveException("WriteBool failed");
    }

    public void WriteByte(byte value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteByte(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteByte failed");
    }

    [CLSCompliant(false)]
    public void WriteSByte(sbyte value)
    {
      WriteByte((byte)value);
    }

    public void WriteShort(short value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteShort(m_ptr, value);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteShort failed");
    }

    [CLSCompliant(false)]
    public void WriteUShort(ushort value)
    {
      WriteShort((short)value);
    }

    public void WriteInt(int value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInt failed");
    }

    [CLSCompliant(false)]
    public void WriteUInt(uint value)
    {
      WriteInt((int)value);
    }

    public void WriteInt64(Int64 value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt64(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInt64 failed");
    }

    public void WriteSingle(float value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteSingle failed");
    }

    public void WriteDouble(double value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteDouble failed");
    }

    public void WriteGuid(Guid value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteGuid(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteGuid failed");
    }

    public void WriteString(string value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteString(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteString failed");
    }

    public void WriteBoolArray(IEnumerable<bool> value)
    {
      List<bool> l = new List<bool>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteBool2(m_ptr, count, l.ToArray());
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteBoolArray failed");
      }
    }

    public void WriteByteArray(IEnumerable<byte> value)
    {
      List<byte> l = new List<byte>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteByte2(m_ptr, count, l.ToArray());
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteByteArray failed");
      }
    }

    [CLSCompliant(false)]
    public void WriteSByteArray(IEnumerable<sbyte> value)
    {
      List<byte> l = new List<byte>();
      foreach (sbyte v in value)
        l.Add((byte)v);

      WriteByteArray(l);
    }

    public void WriteShortArray(IEnumerable<short> value)
    {
      List<short> l = new List<short>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteShort2(m_ptr, count, l.ToArray());
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteShortArray failed");
      }
    }

    public void WriteIntArray(IEnumerable<int> value)
    {
      List<int> l = new List<int>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, count, l.ToArray());
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteIntArray failed");
      }
    }

    public void WriteSingleArray(IEnumerable<float> value)
    {
      List<float> l = new List<float>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, count, l.ToArray());
        if (m_bWriteErrorOccured)
          throw new BinaryArchiveException("WriteSingleArray failed");
      }
    }

    public void WriteDoubleArray(IEnumerable<double> value)
    {
      List<double> l = new List<double>(value);
      int count = l.Count;
      WriteInt(count);
      if (count > 0)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, count, l.ToArray());
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteDoubleArray failed");
      }
    }

    public void WriteGuidArray(IEnumerable<Guid> value)
    {
      int count = 0;
      foreach (Guid g in value)
        count++;
      WriteInt(count);
      foreach(Guid g in value)
        WriteGuid(g);
    }

    public void WriteStringArray(IEnumerable<string> value)
    {
      int count = 0;
      foreach (string s in value)
        count++;
      WriteInt(count);
      foreach (string s in value)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteString(m_ptr, s);
        if( m_bWriteErrorOccured )
          throw new BinaryArchiveException("WriteStringArray failed");
      }
    }

    public void WriteColor(System.Drawing.Color value)
    {
      int argb = value.ToArgb();
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteColor(m_ptr, argb);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteColor failed");
    }

    public void WritePoint(System.Drawing.Point value)
    {
      int[] xy = new int[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 2, xy);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint failed");
    }

    public void WritePointF(System.Drawing.PointF value)
    {
      float[] xy = new float[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 2, xy);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePointF failed");
    }

    public void WriteRectangle(System.Drawing.Rectangle value)
    {
      int[] xywh = new int[] { value.X, value.Y, value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 4, xywh);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteRectangle failed");
    }

    public void WriteRectangleF(System.Drawing.RectangleF value)
    {
      float[] f = new float[] { value.X, value.Y, value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 4, f);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteRectangleF failed");
    }

    public void WriteSize(System.Drawing.Size value)
    {
      int[] xy = new int[] { value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 2, xy);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteSize failed");
    }

    public void WriteSizeF(System.Drawing.SizeF value)
    {
      float[] xy = new float[] { value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 2, xy);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteSizeF failed");
    }

    public void WriteFont(System.Drawing.Font value)
    {
      string family_name = value.FontFamily.Name;
      float emSize = value.Size;
      uint font_style = (uint)(value.Style);
      uint graphics_unit = (uint)(value.Unit);
      byte gdiCharSet = value.GdiCharSet;
      bool gdiVerticalFont = value.GdiVerticalFont;
      WriteString(family_name);
      WriteSingle(emSize);
      WriteUInt(font_style);
      WriteUInt(graphics_unit);
      WriteByte(gdiCharSet);
      WriteBool(gdiVerticalFont);
    }

    public void WriteInterval(Rhino.Geometry.Interval value)
    {
      double[] d = new double[] { value.T0, value.T1 };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInterval failed");
    }

    public void WritePoint2d(Rhino.Geometry.Point2d value)
    {
      double[] d = new double[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint2d failed");
    }

    public void WritePoint3d(Rhino.Geometry.Point3d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 3, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint3d failed");
    }

    public void WritePoint4d(Rhino.Geometry.Point4d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z, value.W };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 4, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint4d failed");
    }

    public void WriteVector2d(Rhino.Geometry.Vector2d value)
    {
      double[] d = new double[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector2d failed");
    }

    public void WriteVector3d(Rhino.Geometry.Vector3d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 3, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector3d failed");
    }

    public void WriteBoundingBox(Rhino.Geometry.BoundingBox value)
    {
      WritePoint3d(value.Min);
      WritePoint3d(value.Max);
    }

    public void WriteRay3d(Rhino.Geometry.Ray3d value)
    {
      WritePoint3d(value.Position);
      WriteVector3d(value.Direction);
    }

    internal void WritePlaneEquation(double[] value)
    {
      if (value.Length != 4)
        throw new ArgumentException("Plane equation must have 4 values");
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 4, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePlaneEquation failed");
    }

    public void WriteTransform(Rhino.Geometry.Transform value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteTransform(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteTransform failed");
    }

    public void WritePlane(Rhino.Geometry.Plane value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WritePlane(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePlane failed");
    }

    public void WriteLine(Rhino.Geometry.Line value)
    {
      WritePoint3d(value.From);
      WritePoint3d(value.To);
    }

    public void WritePoint3f(Rhino.Geometry.Point3f value)
    {
      float[] f = new float[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 3, f);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint3f failed");
    }

    public void WriteVector3f(Rhino.Geometry.Vector3f value)
    {
      float[] f = new float[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 3, f);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector3f failed");
    }

    public void WriteMeshingParameters(Rhino.Geometry.MeshingParameters value)
    {
      IntPtr pMeshParameters = value.ConstPointer();
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteMeshParameters(m_ptr, pMeshParameters);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteMeshParameters failed");
    }

    public void WriteGeometry(Rhino.Geometry.GeometryBase value)
    {
      IntPtr pGeometry = value.ConstPointer();
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteGeometry(m_ptr, pGeometry);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteGeometry failed");
    }

    internal bool BeginWriteDictionary( Guid dictionaryId, uint version, string name )
    {
      return UnsafeNativeMethods.ON_BinaryArchive_BeginWriteDictionary(m_ptr, dictionaryId, version, name);
    }
    internal bool EndWriteDictionary()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndWriteDictionary(m_ptr);
    }

    internal bool BeginWriteDictionaryEntry(int de_type, string de_name)
    {
      return UnsafeNativeMethods.ON_BinaryArchive_BeginWriteDictionaryEntry(m_ptr, de_type, de_name);
    }
    internal bool EndWriteDictionaryEntry()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndWriteDictionaryEntry(m_ptr);
    }
  }

  public class BinaryArchiveReader
  {
    IntPtr m_ptr; // ON_BinaryArchive*
    internal BinaryArchiveReader(IntPtr pArchive)
    {
      m_ptr = pArchive;
    }
    internal void ClearPointer()
    {
      m_ptr = IntPtr.Zero;
    }

    bool m_bReadErrorOccured = false;
    public bool ReadErrorOccured
    {
      get { return m_bReadErrorOccured; }
      set
      {
        // 17 Sept. 2010 S. Baer
        // ?? should we only allow going from false to true??
        m_bReadErrorOccured = value;
      }
    }

    /// <summary>
    /// If a 3dm archive is being read or written, then this is the
    /// version of the 3dm archive format (1, 2, 3, 4 or 5).
    /// 0     a 3dm archive is not being read/written
    /// 1     a version 1 3dm archive is being read/written
    /// 2     a version 2 3dm archive is being read/written
    /// 3     a version 3 3dm archive is being read/written
    /// 4     a version 4 3dm archive is being read/written
    /// 5     an old version 5 3dm archive is being read
    /// 50    a version 5 3dm archive is being read/written
    /// </summary>
    public int Archive3dmVersion
    {
      get
      {
        return UnsafeNativeMethods.ON_BinaryArchive_Archive3dmVersion(m_ptr);
      }
    }

    /// <summary>
    /// A chunk version is a single byte that encodes a major.minor
    /// version number.  Useful when creating I/O code for 3dm chunks
    /// that may change in the future.  Increment the minor version 
    /// number if new information is added to the end of the chunk. 
    /// Increment the major version if the format of the chunk changes
    /// in some other way.
    /// </summary>
    /// <param name="major">0 to 15</param>
    /// <param name="minor">0 to 16</param>
    /// <returns>true on successful read</returns>
    public void Read3dmChunkVersion(out int major, out int minor)
    {
      major = 0;
      minor = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_Read3dmChunkVersion(m_ptr, ref major, ref minor);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("Read3dmChunkVersion failed");
    }

    internal IntPtr NonConstPointer()
    {
      return m_ptr;
    }

    public Rhino.Collections.ArchivableDictionary ReadDictionary()
    {
      Rhino.Collections.ArchivableDictionary rc = null;
      if (!m_bReadErrorOccured)
      {
        rc = Rhino.Collections.ArchivableDictionary.Read(this);
        if (null == rc)
          m_bReadErrorOccured = true;
      }
      if (m_bReadErrorOccured)
        throw new BinaryArchiveException("ReadDictionary failed");
      return rc;      
    }

    public bool ReadBool()
    {
      bool rc = false;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadBool(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadBool failed");
      return rc;
    }

    public byte ReadByte()
    {
      byte rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadByte(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadByte failed");
      return rc;
    }

    [CLSCompliant(false)]
    public sbyte ReadSByte()
    {
      return (sbyte)ReadByte();
    }

    public short ReadShort()
    {
      short rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadShort(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadShort failed");
      return rc;
    }

    [CLSCompliant(false)]
    public ushort ReadUShort()
    {
      return (ushort)ReadShort();
    }

    public int ReadInt()
    {
      int rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInt failed");
      return rc;
    }

    [CLSCompliant(false)]
    public uint ReadUInt()
    {
      return (uint)ReadInt();
    }

    public Int64 ReadInt64()
    {
      Int64 rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt64(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInt64 failed");
      return rc;
    }

    public float ReadSingle()
    {
      float rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSingle failed");
      return rc;
    }

    public double ReadDouble()
    {
      double rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadDouble failed");
      return rc;
    }

    public Guid ReadGuid()
    {
      Guid rc = Guid.Empty;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadGuid(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadGuid failed");
      return rc;
    }

    public string ReadString()
    {
      Rhino.Runtime.StringHolder str = new Rhino.Runtime.StringHolder();
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadString(m_ptr, str.NonConstPointer());
      string rc = str.ToString();
      str.Dispose();
      if (m_bReadErrorOccured)
        throw new BinaryArchiveException("ReadString failed");
      return rc;
    }

    public bool[] ReadBoolArray()
    {
      bool[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new bool[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadBool2(m_ptr, count, rc);
          if (m_bReadErrorOccured)
            throw new BinaryArchiveException("ReadBoolArray failed");
        }
      }
      return rc;
    }

    public byte[] ReadByteArray()
    {
      byte[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new byte[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadByte2(m_ptr, count, rc);
          if( m_bReadErrorOccured )
            throw new BinaryArchiveException("ReadByteArray failed");
        }
      }
      return rc;
    }

    [CLSCompliant(false)]
    public sbyte[] ReadSByteArray()
    {
      sbyte[] rc = null;
      byte[] b = ReadByteArray();
      if (b != null)
      {
        // not very efficient, but I doubt many people will ever use this function.
        rc = new sbyte[b.Length];
        for (int i = 0; i < b.Length; i++)
          rc[i] = (sbyte)b[i];
      }
      return rc;
    }

    public short[] ReadShortArray()
    {
      short[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new short[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadShort2(m_ptr, count, rc);
          if( m_bReadErrorOccured )
            throw new BinaryArchiveException("ReadShortArray failed");
        }
      }
      return rc;
    }

    public int[] ReadIntArray()
    {
      int[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new int[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, count, rc);
          if( m_bReadErrorOccured )
            throw new BinaryArchiveException("ReadIntArray failed");
        }
      }
      return rc;
    }

    public float[] ReadSingleArray()
    {
      float[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new float[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, count, rc);
          if( m_bReadErrorOccured )
            throw new BinaryArchiveException("ReadSingleArray failed");
        }
      }
      return rc;
    }

    public double[] ReadDoubleArray()
    {
      double[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new double[count];
        if (count > 0)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, count, rc);
          if( m_bReadErrorOccured )
            throw new BinaryArchiveException("ReadDoubleArray failed");
        }
      }
      return rc;
    }

    public Guid[] ReadGuidArray()
    {
      Guid[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new Guid[count];
        for (int i = 0; i < count; i++)
          rc[i] = ReadGuid();
      }
      return rc;
    }

    public string[] ReadStringArray()
    {
      string[] rc = null;
      int count = ReadInt();
      if (count >= 0)
      {
        rc = new string[count];
        Rhino.Runtime.StringHolder str = new Rhino.Runtime.StringHolder();
        for (int i = 0; i < count; i++)
        {
          m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadString(m_ptr, str.NonConstPointer());
          if (m_bReadErrorOccured)
            break;
          rc[i] = str.ToString();
        }
        str.Dispose();
        if (m_bReadErrorOccured)
          throw new BinaryArchiveException("ReadStringArray failed");
      }
      return rc;
    }

    public System.Drawing.Color ReadColor()
    {
      int argb = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadColor(m_ptr, ref argb);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadColor failed");
      return System.Drawing.Color.FromArgb(argb);
    }

    public System.Drawing.Point ReadPoint()
    {
      int[] xy = new int[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint failed");
      return new System.Drawing.Point(xy[0], xy[1]);
    }

    public System.Drawing.PointF ReadPointF()
    {
      float[] xy = new float[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPointF failed");
      return new System.Drawing.PointF(xy[0], xy[1]);
    }

    public System.Drawing.Rectangle ReadRectangle()
    {
      int[] xywh = new int[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 4, xywh);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadRectangle failed");
      return new System.Drawing.Rectangle(xywh[0], xywh[1], xywh[2], xywh[3]);
    }

    public System.Drawing.RectangleF ReadRectangleF()
    {
      float[] f = new float[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 4, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadRectangleF failed");
      return new System.Drawing.RectangleF(f[0], f[1], f[2], f[3]);
    }

    public System.Drawing.Size ReadSize()
    {
      int[] xy = new int[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSize failed");
      return new System.Drawing.Size(xy[0], xy[1]);
    }

    public System.Drawing.SizeF ReadSizeF()
    {
      float[] xy = new float[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSizeF failed");
      return new System.Drawing.SizeF(xy[0], xy[1]);
    }

    public System.Drawing.Font ReadFont()
    {
      System.Drawing.Font rc = null;

      string family_name = ReadString();
      float emSize = ReadSingle();
      uint font_style = ReadUInt();
      uint graphics_unit= ReadUInt();
      byte gdiCharSet = ReadByte();
      bool gdiVerticalFont = ReadBool();

      try
      {
        if (emSize <= 0.0)
          emSize = 1.0f;
        System.Drawing.FontStyle _font_style = (System.Drawing.FontStyle)font_style;
        System.Drawing.GraphicsUnit _graphics_unit = (System.Drawing.GraphicsUnit)graphics_unit;
        rc = new System.Drawing.Font(family_name, emSize, _font_style, _graphics_unit, gdiCharSet, gdiVerticalFont);
      }
      catch (System.Exception)
      {
        rc = null; 
      }
      return rc;
    }

    public Rhino.Geometry.Interval ReadInterval()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInterval failed");
      return new Rhino.Geometry.Interval(d[0], d[1]);
    }

    public Rhino.Geometry.Point2d ReadPoint2d()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint2d failed");
      return new Rhino.Geometry.Point2d(d[0], d[1]);
    }

    public  Rhino.Geometry.Point3d ReadPoint3d()
    {
      double[] d = new double[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 3, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint3d failed");
      return new Rhino.Geometry.Point3d(d[0], d[1], d[2]);
    }

    public Rhino.Geometry.Point4d ReadPoint4d()
    {
      double[] d = new double[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 4, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint4d failed");
      return new Rhino.Geometry.Point4d(d[0], d[1], d[2], d[3]);
    }

    public Rhino.Geometry.Vector2d ReadVector2d()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector2d failed");
      return new Rhino.Geometry.Vector2d(d[0], d[1]);
    }

    public Rhino.Geometry.Vector3d ReadVector3d()
    {
      double[] d = new double[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 3, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector3d failed");
      return new Rhino.Geometry.Vector3d(d[0], d[1], d[2]);
    }

    public Rhino.Geometry.BoundingBox ReadBoundingBox()
    {
      Rhino.Geometry.Point3d p0 = ReadPoint3d();
      Rhino.Geometry.Point3d p1 = ReadPoint3d();
      return new Rhino.Geometry.BoundingBox(p0, p1);
    }

    public Rhino.Geometry.Ray3d ReadRay3d()
    {
      Rhino.Geometry.Point3d p = ReadPoint3d();
      Rhino.Geometry.Vector3d v = ReadVector3d();
      return new Rhino.Geometry.Ray3d(p, v);
    }

    internal double[] ReadPlaneEquation()
    {
      double[] d = new double[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 4, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPlaneEquation failed");
      return d;
    }

    public Rhino.Geometry.Transform ReadTransform()
    {
      Rhino.Geometry.Transform rc = new Rhino.Geometry.Transform();
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadTransform(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadTransform failed");
      return rc;
    }

    public Rhino.Geometry.Plane ReadPlane()
    {
      Rhino.Geometry.Plane rc = new Rhino.Geometry.Plane();
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadPlane(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPlane failed");
      return rc;
    }

    public Rhino.Geometry.Line ReadLine()
    {
      Rhino.Geometry.Point3d p0 = ReadPoint3d();
      Rhino.Geometry.Point3d p1 = ReadPoint3d();
      return new Rhino.Geometry.Line(p0, p1);
    }

    public Rhino.Geometry.Point3f ReadPoint3f()
    {
      float[] f = new float[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 3, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint3f failed");
      return new Rhino.Geometry.Point3f(f[0], f[1], f[2]);
    }

    public Rhino.Geometry.Vector3f ReadVector3f()
    {
      float[] f = new float[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 3, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector3f failed");
      return new Rhino.Geometry.Vector3f(f[0], f[1], f[2]);
    }

    public Rhino.Geometry.MeshingParameters ReadMeshingParameters()
    {
      IntPtr pMeshParameters = IntPtr.Zero;
      if( !m_bReadErrorOccured )
        pMeshParameters = UnsafeNativeMethods.ON_BinaryArchive_ReadMeshParameters(m_ptr);
      m_bReadErrorOccured = m_bReadErrorOccured || IntPtr.Zero == pMeshParameters;
      if (m_bReadErrorOccured)
        throw new BinaryArchiveException("ReadMeshParameters failed");
      return new Rhino.Geometry.MeshingParameters(pMeshParameters);
    }

    public Rhino.Geometry.GeometryBase ReadGeometry()
    {
      IntPtr pGeometry = IntPtr.Zero;
      if (!m_bReadErrorOccured)
      {
        int read_rc = 0;
        pGeometry = UnsafeNativeMethods.ON_BinaryArchive_ReadGeometry(m_ptr, ref read_rc);
        if (read_rc == 0)
          m_bReadErrorOccured = true;
      }
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadGeometry failed");
      return Rhino.Geometry.GeometryBase.CreateGeometryHelper(pGeometry, null);
    }

    #region dictionary support
    internal bool BeginReadDictionary( out Guid dictionaryId, out uint version, out string name )
    {
      dictionaryId = Guid.Empty;
      version = 0;
      name = String.Empty;
      Runtime.StringHolder str = new Rhino.Runtime.StringHolder();
      bool rc = UnsafeNativeMethods.ON_BinaryArchive_BeginReadDictionary(m_ptr, ref dictionaryId, ref version, str.NonConstPointer());
      name = str.ToString();
      str.Dispose();
      return rc;
    }
    internal bool EndReadDictionary()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndReadDictionary(m_ptr);
    }

    /// <summary>
    /// </summary>
    /// <param name="entryType"></param>
    /// <param name="entryName"></param>
    /// <returns>
    /// 0: serious IO error
    /// 1: success
    /// read information and then call EndReadDictionaryEntry()
    /// 2: at end of dictionary
    /// </returns>
    internal int BeginReadDictionaryEntry(out int entryType, out string entryName)
    {
      entryType = 0;
      entryName = string.Empty;
      Runtime.StringHolder str = new Rhino.Runtime.StringHolder();
      int rc = UnsafeNativeMethods.ON_BinaryArchive_BeginReadDictionaryEntry(m_ptr, ref entryType, str.NonConstPointer());
      entryName = str.ToString();
      str.Dispose();
      return rc;
    }

    internal bool EndReadDictionaryEntry()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndReadDictionaryEntry(m_ptr);
    }

    #endregion
  }
}
