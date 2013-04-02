#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Rhino.Collections
{
  /// <summary>
  /// <para>Represents a dictionary class that can be attached to objects and
  /// can be serialized (saved) at necessity.</para>
  /// <para>See remarks for layout.</para>
  /// </summary>
  /// <remarks>
  /// <para>This is the layout of this object:</para>
  /// <para>.</para>
  /// <para>BEGINCHUNK (TCODE_ANONYMOUS_CHUNK)</para>
  /// <para>|- version (int)</para>
  /// <para>|- entry count (int)</para>
  /// <para>   for entry count entries</para>
  /// <para>   |- BEGINCHUNK (TCODE_ANONYMOUS_CHUNK)</para>
  /// <para>   |- key (string)</para>
  /// <para>   |- entry contents</para>
  /// <para>   |- ENDCHUNK (TCODE_ANONYMOUS_CHUNK)</para>
  /// <para>ENDCHUNK (TCODE_ANONYMOUS_CHUNK)</para>
  /// </remarks>
  public class ArchivableDictionary : ICloneable, IDictionary<string, object>
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
    readonly System.Collections.Generic.Dictionary<string, DictionaryItem> m_items = new Dictionary<string, DictionaryItem>();
    Rhino.DocObjects.Custom.UserData m_parent_userdata;

    /// <summary>
    /// Gets or sets the version of this <see cref="ArchivableDictionary"/>.
    /// </summary>
    public int Version
    {
      get { return m_version; }
      set { m_version = value; }
    }

    /// <summary>
    /// Gets or sets the name string of this <see cref="ArchivableDictionary"/>.
    /// </summary>
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

    /// <summary>Initializes an instance of a dictionary for writing to a 3dm archive.</summary>
    public ArchivableDictionary()
    {
      m_version = 0;
      m_name = String.Empty;
    }

    /// <summary>Initializes an instance of a dictionary for writing to a 3dm archive</summary>
    /// <param name="parentUserData">
    /// parent user data if this dictionary is associated with user data
    /// </param>
    public ArchivableDictionary(Rhino.DocObjects.Custom.UserData parentUserData)
    {
      m_parent_userdata = parentUserData;
      m_version = 0;
      m_name = String.Empty;
    }

    /// <summary>Initializes an instance of a dictionary for writing to a 3dm archive.</summary>
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

    ///<summary>Initializes an instance of a dictionary for writing to a 3dm archive.</summary>
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
      m_name = String.IsNullOrEmpty(name) ? String.Empty : name;
    }

    /// <summary>
    /// If this dictionary is part of userdata (or is a UserDictionary), then
    /// this is the parent user data. null if this dictionary is not part of
    /// userdata
    /// </summary>
    public Rhino.DocObjects.Custom.UserData ParentUserData
    {
      get { return m_parent_userdata; }
    }

    /// <summary>
    /// Recursively sets the parent user data for this dictionary
    /// </summary>
    /// <param name="parent"></param>
    internal void SetParentUserData(Rhino.DocObjects.Custom.UserData parent)
    {
      m_parent_userdata = parent;
      object[] values = Values;
      if (values != null)
      {
        for (int i = 0; i < values.Length; i++)
        {
          ArchivableDictionary dict = values[i] as ArchivableDictionary;
          if (dict != null)
            dict.SetParentUserData(parent);
        }
      }
    }

    ///<summary>Reads a dictionary from an archive.</summary>
    ///<param name='archive'>
    ///The archive to read from. The archive position should be at the beginning of
    ///the dictionary
    ///</param>
    ///<returns>new filled dictionary on success. null on failure.</returns>
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
            rc = Set(key, val);
          }
          break;
        case ItemType.itByte: //2
          {
            byte val = archive.ReadByte();
            rc = Set(key, val);
          }
          break;

        case ItemType.itSByte: //3
          {
            sbyte val = archive.ReadSByte();
            rc = Set(key, val);
          }
          break;
        case ItemType.itShort: //4
          {
            short val = archive.ReadShort();
            rc = Set(key, val);
          }
          break;
        case ItemType.itUShort: //5
          {
            ushort val = archive.ReadUShort();
            rc = Set(key, val);
          }
          break;
        case ItemType.itInt32: //6
          {
            int val = archive.ReadInt();
            rc = Set(key, val);
          }
          break;
        case ItemType.itUInt32: //7
          {
            uint val = archive.ReadUInt();
            rc = Set(key, val);
          }
          break;
        case ItemType.itInt64: //8
          {
            Int64 val=archive.ReadInt64();
            rc = Set(key, val);
          }
          break;
        case ItemType.itSingle: //9
          {
            float val = archive.ReadSingle();
            rc = Set(key, val);
          }
          break;
        case ItemType.itDouble: //10
          {
            double val = archive.ReadDouble();
            rc = Set(key, val);
          }
          break;
        case ItemType.itGuid: //11
          {
            Guid val = archive.ReadGuid();
            rc = Set(key, val);
          }
          break;
        case ItemType.itString: //12
          {
            string val = archive.ReadString();
            rc = Set(key, val);
          }
          break;
        case ItemType.itArrayBool: //13
          {
            bool[] arr = archive.ReadBoolArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayByte: //14
          {
            byte[] arr = archive.ReadByteArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArraySByte: //15
          {
            sbyte[] arr = archive.ReadSByteArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayShort: //16
          {
            short[] arr = archive.ReadShortArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayInt32: //17
          {
            int[] arr = archive.ReadIntArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArraySingle: //18
          {
            float[] arr = archive.ReadSingleArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayDouble: //19
          {
            double[] arr = archive.ReadDoubleArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayGuid: //20
          {
            Guid[] arr = archive.ReadGuidArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itArrayString: //21
          {
            string[] arr = archive.ReadStringArray();
            rc = Set(key, arr);
          }
          break;
        case ItemType.itColor: //22
          {
            System.Drawing.Color val = archive.ReadColor();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPoint: //23
          {
            System.Drawing.Point val = archive.ReadPoint();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPointF: //24
          {
            System.Drawing.PointF val = archive.ReadPointF();
            rc = Set(key, val);
          }
          break;
        case ItemType.itRectangle: //25
          {
            System.Drawing.Rectangle val = archive.ReadRectangle();
            rc = Set(key, val);
          }
          break;
        case ItemType.itRectangleF: //26
          {
            System.Drawing.RectangleF val = archive.ReadRectangleF();
            rc = Set(key, val);
          }
          break;
        case ItemType.itSize: //27
          {
            System.Drawing.Size val = archive.ReadSize();
            rc = Set(key, val);
          }
          break;
        case ItemType.itSizeF: //28
          {
            System.Drawing.SizeF val = archive.ReadSizeF();
            rc = Set(key, val);
          }
          break;
        case ItemType.itFont: //29
          {
            System.Drawing.Font val = archive.ReadFont();
            rc = Set(key, val);
          }
          break;
        case ItemType.itInterval: //30
          {
            Rhino.Geometry.Interval val = archive.ReadInterval();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPoint2d: //31
          {
            Rhino.Geometry.Point2d val = archive.ReadPoint2d();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPoint3d: //32
          {
            Rhino.Geometry.Point3d val = archive.ReadPoint3d();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPoint4d: //33
          {
            Rhino.Geometry.Point4d val = archive.ReadPoint4d();
            rc = Set(key, val);
          }
          break;
        case ItemType.itVector2d: //34
          {
            Rhino.Geometry.Vector2d val = archive.ReadVector2d();
            rc = Set(key, val);
          }
          break;
        case ItemType.itVector3d: //35
          {
            Rhino.Geometry.Vector3d val = archive.ReadVector3d();
            rc = Set(key, val);
          }
          break;
        case ItemType.itBoundingBox: //36
          {
            Rhino.Geometry.BoundingBox val = archive.ReadBoundingBox();
              rc = Set(key, val);
          }
          break;
        case ItemType.itRay3d: //37
          {
            Rhino.Geometry.Ray3d val = archive.ReadRay3d();
            rc = Set(key, val);
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
            rc = Set(key, val);
          }
          break;
        case ItemType.itPlane: //40
          {
            Rhino.Geometry.Plane val = archive.ReadPlane();
            rc = Set(key, val);
          }
          break;
        case ItemType.itLine: //41
          {
            Rhino.Geometry.Line val = archive.ReadLine();
            rc = Set(key, val);
          }
          break;
        case ItemType.itPoint3f: //42
          {
            Rhino.Geometry.Point3f val = archive.ReadPoint3f();
            rc = Set(key, val);
          }
          break;
        case ItemType.itVector3f: //43
          {
            Rhino.Geometry.Vector3f val = archive.ReadVector3f();
            rc = Set(key, val);
          }
          break;
        case ItemType.itOnBinaryArchiveDictionary: //44
          {
            ArchivableDictionary dict = Read(archive);
            if( dict != null )
              rc = Set(key, dict);
          }
          break;
        case ItemType.itOnObject: //45
        case ItemType.itOnGeometry: //47
          {
            int read_rc = 0;
            IntPtr pObject = UnsafeNativeMethods.ON_BinaryArchive_ReadObject(archive.NonConstPointer(), ref read_rc);
            Rhino.Geometry.GeometryBase geom = Rhino.Geometry.GeometryBase.CreateGeometryHelper(pObject, null);
            if( geom!=null )
            {
              rc = Set(key, geom);
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
            rc = Set(key, val);
          }
          break;
      }
      return rc;
    }

    /// <summary>
    /// Writes this dictionary to an archive.
    /// </summary>
    /// <param name="archive">The archive to write to.</param>
    /// <returns>true on success.</returns>
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
    private static bool WriteItem(Rhino.FileIO.BinaryArchiveWriter archive, string entry_name, ItemType it, object val)
    {
      if (archive == null || it == ItemType.itUndefined || string.IsNullOrEmpty(entry_name) || val == null)
        return false;

      if (!archive.BeginWriteDictionaryEntry((int)it, entry_name))
        return false;

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
      bool rc = archive.EndWriteDictionaryEntry();
      return rc;    
    }

    /// <summary>Gets all entry names or keys.</summary>
    public string[] Keys
    {
      get
      {
        string[] rc = new string[m_items.Keys.Count];
        m_items.Keys.CopyTo(rc, 0);
        return rc;
      }
    }

    /// <summary>Gets all values in this dictionary.</summary>
    public object[] Values
    {
      get
      {
        object[] rc = new object[m_items.Count];
        int i = 0;
        foreach (var v in m_items.Values)
        {
          rc[i++] = v.m_value;
        }
        return rc;
      }
    }

    /// <summary>
    /// Determines whether the dictionary contains the specified key.
    /// </summary>
    /// <param name="key">The key to locate.</param>
    /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
    /// <exception cref="System.ArgumentNullException">key is null.</exception>
    public bool ContainsKey(string key)
    {
      return m_items.ContainsKey(key);
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get or set.</param>
    /// <returns>
    /// The value associated with the specified key. If the specified key is not
    /// found, a get operation throws a <see cref="System.Collections.Generic.KeyNotFoundException"/>.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">If the key is null.</exception>
    /// <exception cref="System.Collections.Generic.KeyNotFoundException">If the key is not found.</exception>
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

    /// <summary>
    /// Removes all keys and values from the dictionary.
    /// </summary>
    public void Clear()
    {
      m_items.Clear();
    }

    /// <summary>
    /// Removes the value with the specified key from the dictionary.
    /// </summary>
    /// <param name="key">The key of the element to remove.</param>
    /// <exception cref="System.ArgumentNullException">Key is null.</exception>
    /// <returns>true if the element is successfully found and removed; otherwise, false.
    /// This method returns false if key is not found.
    /// </returns>
    public bool Remove(string key)
    {
      return m_items.Remove(key);
    }

    /// <summary>
    /// Gets the number of key/value pairs contained in the dictionary.
    /// </summary>
    public int Count
    {
      get
      {
        return m_items.Count;
      }
    }

    /// <summary>
    /// Gets the value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns and if the key is found,
    /// contains the value associated with the specified key;
    /// otherwise, null. This parameter is passed uninitialized.</param>
    /// <returns>true if the dictionary contains an element with the specified key; otherwise, false.</returns>
    /// <exception cref="System.ArgumentNullException">Key is null.</exception>
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

    // Mimic what PersistentSettings data access methods
    
    bool TryGetHelper<T>(string key, out T value, T defaultValue)
    {
      object obj;
      if (TryGetValue(key, out obj) && obj is T)
      {
        value = (T)obj;
        return true;
      }
      value = defaultValue;
      return false;
    }

    T GetHelper<T>(string key, T defaultValue)
    {
      if (!m_items.ContainsKey(key))
        throw new KeyNotFoundException(key);
      T rc;
      if (TryGetHelper(key, out rc, defaultValue))
        return rc;
      throw new Exception("key '" + key + "' value type is not a " + defaultValue.GetType());
    }

    T GetWithDefaultHelper<T>(string key, T defaultValue)
    {
      T rc;
      TryGetHelper(key, out rc, defaultValue);
      return rc;
    }

    /// <summary>
    /// Get value as string, will only succeed if value was created using Set(string key, string value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetString(string key, out string value)
    {
      return TryGetHelper(key, out value, string.Empty);
    }
    /// <summary>
    /// Get value as string, will only succeed if value was created using Set(string key, string value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetString(string key)
    {
      return GetHelper(key, string.Empty);
    }
    /// <summary>
    /// Get value as string, will return defaultValue unless value was created using Set(string key, string value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public string GetString(string key, string defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as bool, will only succeed if value was created using Set(string key, bool value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetBool(string key, out bool value)
    {
      return TryGetHelper(key, out value, false);
    }
    /// <summary>
    /// Get value as bool, will only succeed if value was created using Set(string key, bool value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public bool GetBool(string key)
    {
      return GetHelper(key, false);
    }
    /// <summary>
    /// Get value as bool, will return defaultValue unless value was created using Set(string key, bool value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public bool GetBool(string key, bool defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as float, will only succeed if value was created using Set(string key, float value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetFloat(string key, out float value)
    {
      return TryGetHelper(key, out value, 0f);
    }
    /// <summary>
    /// Get value as float, will only succeed if value was created using Set(string key, float value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public float GetFloat(string key)
    {
      return GetHelper(key, 0f);
    }
    /// <summary>
    /// Get value as float, will return defaultValue unless value was created using Set(string key, float value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public float GetFloat(string key, float defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as double, will only succeed if value was created using Set(string key, double value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetDouble(string key, out double value)
    {
      return TryGetHelper(key, out value, 0.0);
    }
    /// <summary>
    /// Get value as double, will only succeed if value was created using Set(string key, double value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public double GetDouble(string key)
    {
      return GetHelper(key, 0.0);
    }
    /// <summary>
    /// Get value as int, will return defaultValue unless value was created using Set(string key, int value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public int GetInteger(string key, int defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as int, will only succeed if value was created using Set(string key, int value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetInteger(string key, out int value)
    {
      return TryGetHelper(key, out value, 0);
    }
    /// <summary>
    /// Get value as int, will only succeed if value was created using Set(string key, int value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public int GetInteger(string key)
    {
      return GetHelper(key, 0);
    }
    /// <summary>
    /// Get value as int, will return defaultValue unless value was created using Set(string key, int value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public int Getint(string key, int defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as Point3f, will only succeed if value was created using Set(string key, Point3f value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetPoint3f(string key, out Rhino.Geometry.Point3f value)
    {
      return TryGetHelper(key, out value, Rhino.Geometry.Point3f.Unset);
    }
    /// <summary>
    /// Get value as Point3f, will only succeed if value was created using Set(string key, Point3f value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Rhino.Geometry.Point3f GetPoint3f(string key)
    {
      return GetHelper(key, Rhino.Geometry.Point3f.Unset);
    }
    /// <summary>
    /// Get value as Point3f, will return defaultValue unless value was created using Set(string key, Point3f value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Rhino.Geometry.Point3f GetPoint3f(string key, Rhino.Geometry.Point3f defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as Point3d, will only succeed if value was created using Set(string key, Point3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetPoint3d(string key, out Rhino.Geometry.Point3d value)
    {
      return TryGetHelper(key, out value, Rhino.Geometry.Point3d.Unset);
    }
    /// <summary>
    /// Get value as Point3d, will only succeed if value was created using Set(string key, Point3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Rhino.Geometry.Point3d GetPoint3d(string key)
    {
      return GetHelper(key, Rhino.Geometry.Point3d.Unset);
    }
    /// <summary>
    /// Get value as Point3d, will return defaultValue unless value was created using Set(string key, Point3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Rhino.Geometry.Point3d GetPoint3d(string key, Rhino.Geometry.Point3d defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as Vector3d, will only succeed if value was created using Set(string key, Vector3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetVector3d(string key, out Rhino.Geometry.Vector3d value)
    {
      return TryGetHelper(key, out value, Rhino.Geometry.Vector3d.Unset);
    }
    /// <summary>
    /// Get value as Vector3d, will only succeed if value was created using Set(string key, Vector3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Rhino.Geometry.Vector3d GetVector3d(string key)
    {
      return GetHelper(key, Rhino.Geometry.Vector3d.Unset);
    }
    /// <summary>
    /// Get value as Vector3d, will return defaultValue unless value was created using Set(string key, Vector3d value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Rhino.Geometry.Vector3d GetVector3d(string key, Rhino.Geometry.Vector3d defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }
    /// <summary>
    /// Get value as Guid, will only succeed if value was created using Set(string key, Guid value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool TryGetGuid(string key, out Guid value)
    {
      return TryGetHelper(key, out value, Guid.Empty);
    }
    /// <summary>
    /// Get value as Guid, will only succeed if value was created using Set(string key, Guid value)
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public Guid GetGuid(string key)
    {
      return GetHelper(key, Guid.Empty);
    }
    /// <summary>
    /// Get value as Guid, will return defaultValue unless value was created using Set(string key, Guid value)
    /// </summary>
    /// <param name="key"></param>
    /// <param name="defaultValue"></param>
    /// <returns></returns>
    public Guid GetGuid(string key, Guid defaultValue)
    {
      return GetWithDefaultHelper(key, defaultValue);
    }

    /// <summary>
    /// Sets a <see cref="bool"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="bool"/> value.
    /// <para>Because <see cref="bool"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para>
    /// </param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, bool val) { return SetItem(key, ItemType.itBool, val); }

    /// <summary>
    /// Sets a <see cref="byte"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="byte"/>.
    /// <para>Because <see cref="byte"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, byte val) { return SetItem(key, ItemType.itByte, val); }

    /// <summary>
    /// Sets a <see cref="sbyte"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="sbyte"/>.
    /// <para>Because <see cref="sbyte"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    [CLSCompliant(false)]
    public bool Set(string key, sbyte val) { return SetItem(key, ItemType.itSByte, val); }

    /// <summary>
    /// Sets a <see cref="short"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="short"/>.
    /// <para>Because <see cref="short"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, short val) { return SetItem(key, ItemType.itShort, val); }

    /// <summary>
    /// Sets a <see cref="ushort"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="ushort"/>.
    /// <para>Because <see cref="ushort"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    [CLSCompliant(false)]
    public bool Set(string key, ushort val) { return SetItem(key, ItemType.itUShort, val); }

    /// <summary>
    /// Sets a <see cref="int"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="int"/>.
    /// <para>Because <see cref="int"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, int val) { return SetItem(key, ItemType.itInt32, val); }

    /// <summary>
    /// Sets a <see cref="uint"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="uint"/>.
    /// <para>Because <see cref="uint"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    [CLSCompliant(false)]
    public bool Set(string key, uint val) { return SetItem(key, ItemType.itUInt32, val); }

    /// <summary>
    /// Sets a <see cref="long"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="long"/>.
    /// <para>Because <see cref="long"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, Int64 val) { return SetItem(key, ItemType.itInt64, val); }

    /// <summary>
    /// Sets a <see cref="float"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="float"/>.
    /// <para>Because <see cref="float"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, float val) { return SetItem(key, ItemType.itSingle, val); }

    /// <summary>
    /// Sets a <see cref="double"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="double"/>.
    /// <para>Because <see cref="double"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, double val) { return SetItem(key, ItemType.itDouble, val); }

    /// <summary>
    /// Sets a <see cref="Guid"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="Guid"/>.
    /// <para>Because <see cref="Guid"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, Guid val) { return SetItem(key, ItemType.itGuid, val); }

    /// <summary>
    /// Sets a <see cref="string"/>.
    /// </summary>
    /// <param name="key">The text key.</param>
    /// <param name="val">A <see cref="string"/>.
    /// <para>Because <see cref="string"/> is immutable, it is not possible to modify the object while it is in this dictionary.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, string val) { return SetItem(key, ItemType.itString, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="bool"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<bool> val) { return SetItem(key, ItemType.itArrayBool, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="byte"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<byte> val) { return SetItem(key, ItemType.itArrayByte, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="sbyte"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    [CLSCompliant(false)]
    public bool Set(string key, IEnumerable<sbyte> val) { return SetItem(key, ItemType.itArraySByte, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="short"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<short> val) { return SetItem(key, ItemType.itArrayShort, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="int"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<int> val) { return SetItem(key, ItemType.itArrayInt32, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="float"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<float> val) { return SetItem(key, ItemType.itArraySingle, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="double"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<double> val) { return SetItem(key, ItemType.itArrayDouble, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="Guid"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<Guid> val) { return SetItem(key, ItemType.itArrayGuid, val); }

    /// <summary>
    /// Sets a list, an array or any enumerable of <see cref="string"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because this interface is a reference type, changes to the assigned object <b>will modify</b> this entry inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, IEnumerable<string> val) { return SetItem(key, ItemType.itArrayString, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.Color"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.Color"/> has value semantics, changes to the
    /// assigning value will leave this entry unchanged.</para></param>
    /// <returns>true if set operation succeeded, otherwise false.</returns>
    public bool Set(string key, System.Drawing.Color val) { return SetItem(key, ItemType.itColor, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.Point"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.Point"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.Point val) { return SetItem(key, ItemType.itPoint, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.PointF"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.PointF"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.PointF val) { return SetItem(key, ItemType.itPointF, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.Rectangle"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.Rectangle"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.Rectangle val) { return SetItem(key, ItemType.itRectangle, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.RectangleF"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.RectangleF"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.RectangleF val) { return SetItem(key, ItemType.itRectangleF, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.Size"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.Size"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.Size val) { return SetItem(key, ItemType.itSize, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.SizeF"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.SizeF"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, System.Drawing.SizeF val) { return SetItem(key, ItemType.itSizeF, val); }

    /// <summary>
    /// Sets a <see cref="System.Drawing.Font"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="System.Drawing.Font"/> is immutable, it is not possible to modify the object while it is in this dictionary.</para></param>
    public bool Set(string key, System.Drawing.Font val) { return SetItem(key, ItemType.itFont, val); }

    /// <summary>
    /// Sets an <see cref="Rhino.Geometry.Interval"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Interval"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Interval val) { return SetItem(key, ItemType.itInterval, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Point2d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A point for that key.
    /// <para>Because <see cref="Rhino.Geometry.Point2d"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Point2d val) { return SetItem(key, ItemType.itPoint2d, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Point3d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A point for that key.
    /// <para>Because <see cref="Rhino.Geometry.Point3d"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Point3d val) { return SetItem(key, ItemType.itPoint3d, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Point4d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Point4d"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Point4d val) { return SetItem(key, ItemType.itPoint4d, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Vector2d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Vector2d"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Vector2d val) { return SetItem(key, ItemType.itVector2d, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Vector3d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Vector3d"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Vector3d val) { return SetItem(key, ItemType.itVector3d, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.BoundingBox"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.BoundingBox"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.BoundingBox val) { return SetItem(key, ItemType.itBoundingBox, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Ray3d"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Ray3d"/> has value semantics and is immutable, no changes to this object are possible.</para></param>
    public bool Set(string key, Rhino.Geometry.Ray3d val) { return SetItem(key, ItemType.itRay3d, val); }

    bool SetPlaneEquation(string key, double[] eq) { return SetItem(key, ItemType.itPlaneEquation, eq); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Transform"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A transform for that key.
    /// <para>Because <see cref="Rhino.Geometry.Transform"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Transform val) { return SetItem(key, ItemType.itXform, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Plane"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A plane for that key.
    /// <para>Because <see cref="Rhino.Geometry.Plane"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Plane val) { return SetItem(key, ItemType.itPlane, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Line"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Line"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Line val) { return SetItem(key, ItemType.itLine, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Point3f"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Point3f"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Point3f val) { return SetItem(key, ItemType.itPoint3f, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.Vector3f"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A value for that key.
    /// <para>Because <see cref="Rhino.Geometry.Vector3f"/> has value semantics, changes to the assigning value will leave this entry unchanged.</para></param>
    public bool Set(string key, Rhino.Geometry.Vector3f val) { return SetItem(key, ItemType.itVector3f, val); }

    /// <summary>
    /// Sets another <see cref="ArchivableDictionary"/> as entry in this dictionary.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">An object for that key.
    /// <para>Because this class is a reference type and is mutable, changes to this object <b>will propagate</b> to the object inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    public bool Set(string key, ArchivableDictionary val) { return SetItem(key, ItemType.itOnBinaryArchiveDictionary, val); }

    /// <summary>
    /// Sets a <see cref="Rhino.Geometry.MeshingParameters"/>.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">An object for that key.
    /// <para>Because this class is a reference type and is mutable, changes to this object <b>will propagate</b> to the object inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate.</para></param>
    public bool Set(string key, Rhino.Geometry.MeshingParameters val) { return SetItem(key, ItemType.itOnMeshParameters, val); }

    /// <summary>
    /// Sets any class deriving from the <see cref="Rhino.Geometry.GeometryBase"/> base class.
    /// </summary>
    /// <param name="key">A text key.</param>
    /// <param name="val">A geometry object for that key.
    /// <para>Because this class is a reference type and is mutable, changes to this object <b>will propagate</b> to the object inside the dictionary.</para>
    /// <para>It is up to the user to clone this entry when appropriate. You can use <see cref="Rhino.Geometry.GeometryBase.Duplicate"/> for this.</para></param>
    public bool Set(string key, Rhino.Geometry.GeometryBase val) { return SetItem(key, ItemType.itOnGeometry, val); }

    bool SetItem(string key, ItemType it, object val)
    {
      if (string.IsNullOrEmpty(key) || val == null || it == ItemType.itUndefined)
        return false;
      m_items[key] = new DictionaryItem(it, val);
      return true;
    }

    /// <summary>
    /// Set an enum value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public bool SetEnumValue<T>(T enumValue) 
        where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum) throw new ArgumentException("!typeof(T).IsEnum");

        Type enumType = typeof(T);

        return SetEnumValue(enumType.Name, enumValue);
    }

    /// <summary>
    /// Set an enum value in the dictionary with a custom key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public bool SetEnumValue<T>(String key, T enumValue) 
        where T : struct, IConvertible
    {
        if (null == key) throw new ArgumentNullException("key");

        if (!typeof(T).IsEnum) throw new ArgumentException("!typeof(T).IsEnum");
        return Set(key, enumValue.ToString(CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Get an enum value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the dictionary.</exception>
    /// <exception cref="FormatException">Thrown when the string retrieved from the dictionary is not convertible to the enum type.</exception>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetEnumValue<T>()
        where T : struct, IConvertible
    {
        if (!typeof(T).IsEnum) throw new ArgumentException("!typeof(T).IsEnum");
        Type enumType = typeof(T);

        return GetEnumValue<T>(enumType.Name);
    }

    /// <summary>
    /// Get an enum value from the dictionary using a custom key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found in the dictionary.</exception>
    /// <exception cref="FormatException">Thrown when the string retrieved from the dictionary is not convertible to the enum type.</exception>
    /// <returns></returns>
    [CLSCompliant(false)]
    public T GetEnumValue<T>(String key) 
        where T : struct, IConvertible
    {
        if (null == key) throw new ArgumentNullException("key");

        if (!typeof(T).IsEnum) throw new ArgumentException("!typeof(T).IsEnum");
        if (ContainsKey(key))
        {
            T enumValue;
            if (TryGetEnumValue(key, out enumValue))
                return enumValue;
            throw new FormatException("Could not recognize the value in the ArchivableDictionary as enum value.");
        }
        throw new KeyNotFoundException();
    }


    /// <summary>
    /// Attempt to get an enum value from the dictionary using a custom key.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="key"></param>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    [CLSCompliant(false)]
    public bool TryGetEnumValue<T>(String key, out T enumValue) 
        where T : struct, IConvertible
    {
        if (null == key) throw new ArgumentNullException("key");
        
        Type enumType = typeof (T);
        if (!enumType.IsEnum) throw new ArgumentException("!typeof(T).IsEnum");
        enumValue = default(T);
        String enumString;
        if (TryGetString(key, out enumString))
        {
            foreach (T e in Enum.GetValues(enumType))
            {
                if (enumString.Equals(e.ToString(CultureInfo.InvariantCulture), StringComparison.OrdinalIgnoreCase))
                {
                    enumValue = e;
                    return true;
                }
            }
        }
        return false;
    }


    /// <summary>
    /// Remmove an enum value from the dictionary.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    [CLSCompliant(false)]
    public bool RemoveEnumValue<T>()
        where T : struct, IConvertible
    {
        Type enumType = typeof(T);
        if (!enumType.IsEnum)
            throw new ArgumentException("!typeof(T).IsEnum");

        if (ContainsKey(enumType.Name))
        {
            return Remove(enumType.Name);
        }
        return false;
    }

    /// <summary>
    /// Add the contents from the source dictionary.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public bool AddContentsFrom(ArchivableDictionary source)
    {
        if (null == source) throw new ArgumentNullException("source");

        Type archDictType = GetType();
        foreach (String key in source.Keys)
        {
            object o = source[key];
            MethodInfo setter = archDictType.GetMethod("Set", new[] { typeof(String), o.GetType() });
            if (setter != null)
            {
                setter.Invoke(this, new[] { key, o });
            }
            else
            {
                String err = "Could not find setter for type " + o.GetType();
                throw new ArgumentException(err);
            }
        }
        return true;
    }

    /// <summary>
    /// Replace the contents of the dictionary with that of the given source dictionary.
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public bool ReplaceContentsWith(ArchivableDictionary source)
    {
        if (null == source) throw new ArgumentNullException("source");

        Clear();
        return AddContentsFrom(source);
    }

    private class DictionaryItem
    {
      public DictionaryItem(ItemType t, object val)
      {
        m_type = t;
        m_value = val;
      }
      public readonly ItemType m_type;
      public readonly object m_value;

      public DictionaryItem CreateCopy()
      {
        object val = m_value;
        ICloneable clonable = m_value as ICloneable;
        if (clonable != null)
        {
          val = clonable.Clone();
        }
        return new DictionaryItem(m_type, val);
      }
    }

    /// <summary>
    /// Constructs a deep copy of this object.
    /// </summary>
    /// <returns>The copy of this object.</returns>
    public ArchivableDictionary Clone()
    {
      ArchivableDictionary clone = new ArchivableDictionary(m_version, m_name);
      foreach (System.Collections.Generic.KeyValuePair<string, DictionaryItem> item in m_items)
      {
        clone.m_items.Add(item.Key, item.Value.CreateCopy());
      }
      return clone;
    }

    object ICloneable.Clone()
    {
      return Clone();
    }

    /// <summary>
    /// This is not supported and always throws <see cref="NotSupportedException"/> at the moment.
    /// </summary>
    /// <param name="key">Unused.</param>
    /// <param name="value">Unused.</param>
    void IDictionary<string, object>.Add(string key, object value)
    {
      throw new NotSupportedException("You must use the SetXXX() methods to set the content of this archive.");
    }

    ICollection<string> IDictionary<string, object>.Keys
    {
      get { return Array.AsReadOnly(Keys); }
    }

    ICollection<object> IDictionary<string, object>.Values
    {
      get { return Values; }
    }

    object IDictionary<string, object>.this[string key]
    {
      get
      {
        return this[key];
      }
      set
      {
        throw new NotSupportedException("You must use the SetXXX() methods to set the content of this archive.");
      }
    }

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
      throw new NotSupportedException("You must use the SetXXX() methods to set the content of this archive.");
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
      return m_items.ContainsKey(item.Key);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
      if (array == null)
      {
        throw new ArgumentNullException("array");
      }
      if (arrayIndex < 0 || arrayIndex > array.Length)
      {
        throw new ArgumentOutOfRangeException("arrayIndex");
      }
      if (array.Length - arrayIndex < Count)
      {
        throw new ArgumentException("This dictionary does not fit into the array.");
      }
      foreach(var content in this)
      {
        array[arrayIndex++] = content;
      }
    }

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly
    {
      get { return true; /* because we do not support the Add() methods, we return true here */ }
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
      return m_items.Remove(item.Key);
    }

    /// <summary>
    /// Gets the enumerator of this dictionary.
    /// </summary>
    /// <returns>A <see cref="IEnumerator{T}"/>, where T is an instance of <see cref="KeyValuePair{T0,T1}"/>, with T0 set as string, and T1 as Syste.Object.</returns>
    public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
    {
      foreach (var element in m_items)
      {
        DictionaryItem item = element.Value;
        if(item != null)
          yield return new KeyValuePair<string, object>(element.Key, item.m_value);
      }
    }

    /// <summary>
    /// Gets the enumerator of this dictionary.
    /// </summary>
    /// <returns>A <see cref="IEnumerator{T}"/>, where T is an instance of <see cref="KeyValuePair{T0,T1}"/>, with T0 set as string, and T1 as Syste.Object.</returns>
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }
  }
}

namespace Rhino.FileIO
{
  /// <summary>
  /// Thrown by BinaryArchiveReader and BinaryArchiveWriter classes when
  /// an IO error has occured.
  /// </summary>
  public class BinaryArchiveException : System.IO.IOException
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="BinaryArchiveException"/> class.
    /// </summary>
    /// <param name="message">The inner message to show to users.</param>
    public BinaryArchiveException(string message)
      : base(message)
    { }
  }
  //public class ON_3DM_CHUNK { }
  //public class ON_3dmGoo { }
  //public class ON_BinaryFile { }

  /// <summary>
  /// Represents an entity that is able to write data to an archive.
  /// </summary>
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

    bool m_bWriteErrorOccured;

    /// <summary>
    /// Gets or sets whether an error occurred.
    /// </summary>
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
    /// 50    a version 5 3dm archive is being read/written.
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
    /// <param name="major">0 to 15.</param>
    /// <param name="minor">0 to 16.</param>
    /// <returns>true on successful read.</returns>
    public void Write3dmChunkVersion(int major, int minor)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_Write3dmChunkVersion(m_ptr, major, minor);
      if (m_bWriteErrorOccured )
        throw new BinaryArchiveException("Write3dmChunkVersion failed");
    }

    /// <summary>
    /// Delivers the complete content of a dictionary to the archive.
    /// </summary>
    /// <param name="dictionary">A dictionary to archive.</param>
    public void WriteDictionary(Rhino.Collections.ArchivableDictionary dictionary)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !dictionary.Write(this);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteDictionary failed");
    }

    /// <summary>
    /// Writes a <see cref="bool"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteBool(bool value)
    {
      if (!UnsafeNativeMethods.ON_BinaryArchive_WriteBool(m_ptr, value))
        throw new BinaryArchiveException("WriteBool failed");
    }

    /// <summary>
    /// Writes a <see cref="byte"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteByte(byte value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteByte(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteByte failed");
    }

    /// <summary>
    /// Writes a <see cref="sbyte"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    [CLSCompliant(false)]
    public void WriteSByte(sbyte value)
    {
      WriteByte((byte)value);
    }

    /// <summary>
    /// Writes a <see cref="short"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteShort(short value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteShort(m_ptr, value);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteShort failed");
    }

    /// <summary>
    /// Writes a <see cref="ushort"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    [CLSCompliant(false)]
    public void WriteUShort(ushort value)
    {
      WriteShort((short)value);
    }

    /// <summary>
    /// Writes a <see cref="int"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteInt(int value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInt failed");
    }

    /// <summary>
    /// Writes a <see cref="uint"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    [CLSCompliant(false)]
    public void WriteUInt(uint value)
    {
      WriteInt((int)value);
    }

    /// <summary>
    /// Writes a <see cref="Int64"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteInt64(Int64 value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt64(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInt64 failed");
    }

    /// <summary>
    /// Writes a <see cref="float"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteSingle(float value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteSingle failed");
    }

    /// <summary>
    /// Writes a <see cref="double"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteDouble(double value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteDouble failed");
    }

    /// <summary>
    /// Writes a <see cref="Guid"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteGuid(Guid value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteGuid(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteGuid failed");
    }

    /// <summary>
    /// Writes a <see cref="string"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteString(string value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteString(m_ptr, value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteString failed");
    }

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="bool"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="byte"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="sbyte"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
    [CLSCompliant(false)]
    public void WriteSByteArray(IEnumerable<sbyte> value)
    {
      List<byte> l = new List<byte>();
      foreach (sbyte v in value)
        l.Add((byte)v);

      WriteByteArray(l);
    }

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="short"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="int"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="float"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="double"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="Guid"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteGuidArray(IEnumerable<Guid> value)
    {
      int count = 0;
      foreach (Guid g in value)
        count++;

      WriteInt(count);

      foreach (Guid g in value)
        WriteGuid(g);
    }

    /// <summary>
    /// Writes a list, an array, or any enumerable of <see cref="string"/> to the archive.
    /// <para>The return will always be an array.</para>
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteStringArray(IEnumerable<string> value)
    {
      int count = 0;
      foreach (string s in value)
        count++;

      WriteInt(count);

      foreach (string s in value)
      {
        m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteString(m_ptr, s);
        if (m_bWriteErrorOccured)
          throw new BinaryArchiveException("WriteStringArray failed");
      }
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.Color"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteColor(System.Drawing.Color value)
    {
      int argb = value.ToArgb();
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteColor(m_ptr, argb);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteColor failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.Point"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePoint(System.Drawing.Point value)
    {
      int[] xy = new int[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 2, xy);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.PointF"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePointF(System.Drawing.PointF value)
    {
      float[] xy = new float[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 2, xy);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePointF failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.Rectangle"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteRectangle(System.Drawing.Rectangle value)
    {
      int[] xywh = new int[] { value.X, value.Y, value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 4, xywh);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteRectangle failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.RectangleF"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteRectangleF(System.Drawing.RectangleF value)
    {
      float[] f = new float[] { value.X, value.Y, value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 4, f);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteRectangleF failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.Size"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteSize(System.Drawing.Size value)
    {
      int[] xy = new int[] { value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteInt2(m_ptr, 2, xy);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteSize failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.SizeF"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteSizeF(System.Drawing.SizeF value)
    {
      float[] xy = new float[] { value.Width, value.Height };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 2, xy);
      if (m_bWriteErrorOccured)
        throw new BinaryArchiveException("WriteSizeF failed");
    }

    /// <summary>
    /// Writes a <see cref="System.Drawing.Font"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Interval"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteInterval(Rhino.Geometry.Interval value)
    {
      double[] d = new double[] { value.T0, value.T1 };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteInterval failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Point2d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePoint2d(Rhino.Geometry.Point2d value)
    {
      double[] d = new double[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint2d failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Point3d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePoint3d(Rhino.Geometry.Point3d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 3, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint3d failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Point4d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePoint4d(Rhino.Geometry.Point4d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z, value.W };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 4, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint4d failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Vector2d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteVector2d(Rhino.Geometry.Vector2d value)
    {
      double[] d = new double[] { value.X, value.Y };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 2, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector2d failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Vector3d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteVector3d(Rhino.Geometry.Vector3d value)
    {
      double[] d = new double[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteDouble2(m_ptr, 3, d);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector3d failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.BoundingBox"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteBoundingBox(Rhino.Geometry.BoundingBox value)
    {
      WritePoint3d(value.Min);
      WritePoint3d(value.Max);
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Ray3d"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
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

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Transform"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteTransform(Rhino.Geometry.Transform value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteTransform(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteTransform failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Plane"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePlane(Rhino.Geometry.Plane value)
    {
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WritePlane(m_ptr, ref value);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePlane failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Line"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteLine(Rhino.Geometry.Line value)
    {
      WritePoint3d(value.From);
      WritePoint3d(value.To);
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Point3f"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WritePoint3f(Rhino.Geometry.Point3f value)
    {
      float[] f = new float[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 3, f);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WritePoint3f failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.Vector3f"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteVector3f(Rhino.Geometry.Vector3f value)
    {
      float[] f = new float[] { value.X, value.Y, value.Z };
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteSingle2(m_ptr, 3, f);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteVector3f failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.MeshingParameters"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
    public void WriteMeshingParameters(Rhino.Geometry.MeshingParameters value)
    {
      IntPtr pMeshParameters = value.ConstPointer();
      m_bWriteErrorOccured = m_bWriteErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_WriteMeshParameters(m_ptr, pMeshParameters);
      if( m_bWriteErrorOccured )
        throw new BinaryArchiveException("WriteMeshParameters failed");
    }

    /// <summary>
    /// Writes a <see cref="Rhino.Geometry.GeometryBase"/> value to the archive.
    /// </summary>
    /// <param name="value">A value to write.</param>
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

  /// <summary>
  /// Represents an entity that is capable of reading a binary archive and
  /// instantiating strongly-typed objects.
  /// </summary>
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

    bool m_bReadErrorOccured;

    /// <summary>
    /// Gets or sets whether en error occurred during reading.
    /// </summary>
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
    /// 50    a version 5 3dm archive is being read/written.
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
    /// <param name="major">0 to 15.</param>
    /// <param name="minor">0 to 16.</param>
    /// <returns>true on successful read.</returns>
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

    /// <summary>
    /// Reads a complete <see cref="Rhino.Collections.ArchivableDictionary"/> from the archive.
    /// </summary>
    /// <returns>The newly instantiated object.</returns>
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

    /// <summary>
    /// Reads a <see cref="bool"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public bool ReadBool()
    {
      bool rc = false;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadBool(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadBool failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="byte"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public byte ReadByte()
    {
      byte rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadByte(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadByte failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="sbyte"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    [CLSCompliant(false)]
    public sbyte ReadSByte()
    {
      return (sbyte)ReadByte();
    }

    /// <summary>
    /// Reads a <see cref="short"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public short ReadShort()
    {
      short rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadShort(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadShort failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="ushort"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    [CLSCompliant(false)]
    public ushort ReadUShort()
    {
      return (ushort)ReadShort();
    }

    /// <summary>
    /// Reads a <see cref="int"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public int ReadInt()
    {
      int rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInt failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="uint"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    [CLSCompliant(false)]
    public uint ReadUInt()
    {
      return (uint)ReadInt();
    }

    /// <summary>
    /// Reads a <see cref="long"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public Int64 ReadInt64()
    {
      Int64 rc = 0; 
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt64(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInt64 failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="float"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public float ReadSingle()
    {
      float rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSingle failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="double"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public double ReadDouble()
    {
      double rc = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadDouble failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="Guid"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
    public Guid ReadGuid()
    {
      Guid rc = Guid.Empty;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadGuid(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadGuid failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="string"/> from the archive.
    /// </summary>
    /// <returns>The value that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="bool"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="byte"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="sbyte"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="short"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="int"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="float"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="double"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="Guid"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads an array of <see cref="string"/> from the archive.
    /// <para>An array is returned even if the input was another enumerable type.</para>
    /// </summary>
    /// <returns>The array that was read.</returns>
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

    /// <summary>
    /// Reads a <see cref="System.Drawing.Color"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.Color ReadColor()
    {
      int argb = 0;
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadColor(m_ptr, ref argb);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadColor failed");
      return System.Drawing.Color.FromArgb(argb);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.Point"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.Point ReadPoint()
    {
      int[] xy = new int[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint failed");
      return new System.Drawing.Point(xy[0], xy[1]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.PointF"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.PointF ReadPointF()
    {
      float[] xy = new float[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPointF failed");
      return new System.Drawing.PointF(xy[0], xy[1]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.Rectangle"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.Rectangle ReadRectangle()
    {
      int[] xywh = new int[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 4, xywh);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadRectangle failed");
      return new System.Drawing.Rectangle(xywh[0], xywh[1], xywh[2], xywh[3]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.RectangleF"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.RectangleF ReadRectangleF()
    {
      float[] f = new float[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 4, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadRectangleF failed");
      return new System.Drawing.RectangleF(f[0], f[1], f[2], f[3]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.Size"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.Size ReadSize()
    {
      int[] xy = new int[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadInt2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSize failed");
      return new System.Drawing.Size(xy[0], xy[1]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.SizeF"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.SizeF ReadSizeF()
    {
      float[] xy = new float[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 2, xy);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadSizeF failed");
      return new System.Drawing.SizeF(xy[0], xy[1]);
    }

    /// <summary>
    /// Reads a <see cref="System.Drawing.Font"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public System.Drawing.Font ReadFont()
    {
      System.Drawing.Font rc;

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

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Interval"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Interval ReadInterval()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadInterval failed");
      return new Rhino.Geometry.Interval(d[0], d[1]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Point2d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Point2d ReadPoint2d()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint2d failed");
      return new Rhino.Geometry.Point2d(d[0], d[1]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Point3d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public  Rhino.Geometry.Point3d ReadPoint3d()
    {
      double[] d = new double[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 3, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint3d failed");
      return new Rhino.Geometry.Point3d(d[0], d[1], d[2]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Point4d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Point4d ReadPoint4d()
    {
      double[] d = new double[4];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 4, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint4d failed");
      return new Rhino.Geometry.Point4d(d[0], d[1], d[2], d[3]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Vector2d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Vector2d ReadVector2d()
    {
      double[] d = new double[2];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 2, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector2d failed");
      return new Rhino.Geometry.Vector2d(d[0], d[1]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Vector3d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Vector3d ReadVector3d()
    {
      double[] d = new double[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadDouble2(m_ptr, 3, d);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector3d failed");
      return new Rhino.Geometry.Vector3d(d[0], d[1], d[2]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.BoundingBox"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.BoundingBox ReadBoundingBox()
    {
      Rhino.Geometry.Point3d p0 = ReadPoint3d();
      Rhino.Geometry.Point3d p1 = ReadPoint3d();
      return new Rhino.Geometry.BoundingBox(p0, p1);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Ray3d"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
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

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Transform"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Transform ReadTransform()
    {
      Rhino.Geometry.Transform rc = new Rhino.Geometry.Transform();
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadTransform(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadTransform failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Plane"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Plane ReadPlane()
    {
      Rhino.Geometry.Plane rc = new Rhino.Geometry.Plane();
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadPlane(m_ptr, ref rc);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPlane failed");
      return rc;
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Line"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Line ReadLine()
    {
      Rhino.Geometry.Point3d p0 = ReadPoint3d();
      Rhino.Geometry.Point3d p1 = ReadPoint3d();
      return new Rhino.Geometry.Line(p0, p1);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Point3f"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Point3f ReadPoint3f()
    {
      float[] f = new float[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 3, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadPoint3f failed");
      return new Rhino.Geometry.Point3f(f[0], f[1], f[2]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.Vector3f"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
    public Rhino.Geometry.Vector3f ReadVector3f()
    {
      float[] f = new float[3];
      m_bReadErrorOccured = m_bReadErrorOccured || !UnsafeNativeMethods.ON_BinaryArchive_ReadSingle2(m_ptr, 3, f);
      if( m_bReadErrorOccured )
        throw new BinaryArchiveException("ReadVector3f failed");
      return new Rhino.Geometry.Vector3f(f[0], f[1], f[2]);
    }

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.MeshingParameters"/> from the archive.
    /// </summary>
    /// <returns>The element that was read.</returns>
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

    /// <summary>
    /// Reads a <see cref="Rhino.Geometry.GeometryBase"/>-derived object from the archive.
    /// <para>The <see cref="Rhino.Geometry.GeometryBase"/> class is abstract.</para>
    /// </summary>
    /// <returns>The element that was read.</returns>
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

    /// <summary>
    /// 
    /// </summary>
    /// <param name="version">.3dm file version (2, 3, 4, 5 or 50)</param>
    /// <param name="comment">
    /// String with application name, et cetera.  This information is primarily
    /// used when debugging files that contain problems.  McNeel and Associates
    /// stores application name, application version, compile date, and the OS
    /// in use when file was written.
    /// </param>
    /// <returns>true on success</returns>
    public bool Read3dmStartSection(out int version, out string comment)
    {
      using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pThis = NonConstPointer();
        IntPtr pString = sh.NonConstPointer();
        version = 0;
        bool rc = UnsafeNativeMethods.ON_BinaryArchive_Read3dmStartSection(pThis, ref version, pString);
        comment = sh.ToString();
        return rc;
      }
    }

    /// <summary>
    /// Fnction for studying contents of a file.  The primary use is as an aid
    /// to help dig through files that have been damaged (bad disks, transmission
    /// errors, etc.) If an error is found, a line that begins with the word
    /// "ERROR" is printed.
    /// </summary>
    /// <param name="log">log where information is printed to</param>
    /// <returns>
    /// 0 if something went wrong, otherwise the typecode of the chunk that
    /// was just studied.
    /// </returns>
    [CLSCompliant(false)]
    public uint Dump3dmChunk(TextLog log)
    {
      IntPtr pThis = NonConstPointer();
      IntPtr pTextLog = log.NonConstPointer();
      return UnsafeNativeMethods.ON_BinaryArchive_Dump3dmChunk(pThis, pTextLog, 0);
    }

    /// <summary>
    /// true if at end of a file
    /// </summary>
    /// <returns></returns>
    public bool AtEnd()
    {
      IntPtr pConstThis = NonConstPointer();
      return UnsafeNativeMethods.ON_BinaryArchive_AtEnd(pConstThis);
    }


    #region dictionary support
    internal bool BeginReadDictionary( out Guid dictionaryId, out uint version, out string name )
    {
      dictionaryId = Guid.Empty;
      version = 0;
      using(Runtime.StringHolder str = new Rhino.Runtime.StringHolder())
      {
        bool rc = UnsafeNativeMethods.ON_BinaryArchive_BeginReadDictionary(m_ptr, ref dictionaryId, ref version,
                                                                           str.NonConstPointer());
        name = str.ToString();
        return rc;
      }
    }
    internal bool EndReadDictionary()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndReadDictionary(m_ptr);
    }

    /// <summary>See return.</summary>
    /// <returns>
    /// 0: serious IO error
    /// 1: success
    /// read information and then call EndReadDictionaryEntry()
    /// 2: at end of dictionary.
    /// </returns>
    internal int BeginReadDictionaryEntry(out int entryType, out string entryName)
    {
      entryType = 0;
      using (Runtime.StringHolder str = new Rhino.Runtime.StringHolder())
      {
        int rc = UnsafeNativeMethods.ON_BinaryArchive_BeginReadDictionaryEntry(m_ptr, ref entryType, str.NonConstPointer());
        entryName = str.ToString();
        return rc;
      }
    }

    internal bool EndReadDictionaryEntry()
    {
      return UnsafeNativeMethods.ON_BinaryArchive_EndReadDictionaryEntry(m_ptr);
    }

    #endregion
  }

  public enum BinaryArchiveMode : int
  {
    Unknown = 0,
    Read = 1,
    Write = 2,
    ReadWrite = 3,
    Read3dm = 5,
    Write3dm = 6
  }

  public class BinaryArchiveFile : IDisposable
  {
    string m_filename;
    BinaryArchiveMode m_mode;
    IntPtr m_pBinaryFile = IntPtr.Zero;

    BinaryArchiveReader m_reader;
    BinaryArchiveWriter m_writer;

    public BinaryArchiveFile(string filename, BinaryArchiveMode mode)
    {
      m_filename = filename;
      m_mode = mode;
    }

    public bool Open()
    {
      if( m_pBinaryFile == IntPtr.Zero )
        m_pBinaryFile = UnsafeNativeMethods.ON_BinaryFile_Open(m_filename, (int)m_mode);
      return m_pBinaryFile != IntPtr.Zero;
    }

    public void Close()
    {
      UnsafeNativeMethods.ON_BinaryFile_Close(m_pBinaryFile);
      m_pBinaryFile = IntPtr.Zero;
      if (m_reader != null)
        m_reader.ClearPointer();
      m_reader = null;
      if (m_writer != null)
        m_writer.ClearPointer();
      m_writer = null;
    }

    IntPtr NonConstPointer()
    {
      if (m_pBinaryFile == IntPtr.Zero)
        throw new BinaryArchiveException("File has not been opened");
      return m_pBinaryFile;
    }

    public BinaryArchiveReader Reader
    {
      get
      {
        if (m_reader == null)
        {
          IntPtr pFile = NonConstPointer();
          if (m_mode != BinaryArchiveMode.Read && m_mode != BinaryArchiveMode.Read3dm && m_mode != BinaryArchiveMode.ReadWrite)
            throw new BinaryArchiveException("File not created with a read mode");
          m_reader = new BinaryArchiveReader(pFile);
        }
        return m_reader;
      }
    }

    public BinaryArchiveWriter Writer
    {
      get
      {
        if (m_writer == null)
        {
          IntPtr pFile = NonConstPointer();
          if (m_mode != BinaryArchiveMode.Write && m_mode != BinaryArchiveMode.Write3dm && m_mode != BinaryArchiveMode.ReadWrite)
            throw new BinaryArchiveException("File not created with a write mode");
          m_writer = new BinaryArchiveWriter(pFile);
        }
        return m_writer;
      }
    }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~BinaryArchiveFile() { Close(); }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Close();
      GC.SuppressFinalize(this);
    }
  }

  /// <summary>
  /// Contains options for serializing -or storing- data,
  /// such as Rhino version and user data.
  /// </summary>
  public class SerializationOptions
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="SerializationOptions"/> class.
    /// </summary>
    public SerializationOptions()
    {
#if RHINO_SDK
      RhinoVersion = RhinoApp.ExeVersion;
#else
      RhinoVersion = 5;
#endif
      WriteUserData = true;
    }

    /// <summary>
    /// Gets or sets a value indicating the Rhino version.
    /// </summary>
    public int RhinoVersion { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to write user data.
    /// </summary>
    public bool WriteUserData { get; set; }
  }
}
