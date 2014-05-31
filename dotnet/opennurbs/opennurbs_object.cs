using System;
using System.Runtime.Serialization;
//using System.Security.Permissions;
using Rhino.Runtime.InteropWrappers;

namespace Rhino.Runtime
{
  /// <summary>
  /// Represents the error that happen when a class user attempts to execute a modifying operation
  /// on an object that has been added to a document.
  /// </summary>
  //[Serializable]
  public class DocumentCollectedException : Exception
  {
    /// <summary>
    /// Initializes a new instance of the document controlled exception class.
    /// </summary>
    public DocumentCollectedException() :
      base("This object cannot be modified because it is controlled by a document.") { }
  }

  /// <summary>
  /// Base class for .NET classes that wrap C++ unmanaged Rhino classes.
  /// </summary>
  //[Serializable]
  public abstract class CommonObject : IDisposable//, ISerializable
  {
    long m_unmanaged_memory;  // amount of "memory" pressure reported to the .NET runtime

    IntPtr m_ptr = IntPtr.Zero; // C++ pointer. This is only set when the wrapped pointer is of an
                              // object that has been created in .NET and is not part of the document
                              // m_ptr must have been created outside of the document and must be deleted in Dispose

    internal object m__parent;  // May be a Rhino.DocObject.RhinoObject, Rhino.DocObjects.ObjRef,
                                // Rhino.Render.RenderMesh, PolyCurve 
    internal int m_subobject_index = -1;

#if RHINO_SDK
    internal Rhino.DocObjects.RhinoObject ParentRhinoObject()
    {
      return m__parent as Rhino.DocObjects.RhinoObject;
    }
#endif

    internal void SetParent(object parent)
    {
      m__parent = parent;
      m_ptr = IntPtr.Zero;
    }

    internal IntPtr ConstPointer()
    {
      if (IntPtr.Zero != m_ptr)
        return m_ptr;

      IntPtr pConstObject = _InternalGetConstPointer();
      if (IntPtr.Zero == pConstObject)
        throw new DocumentCollectedException();

      return pConstObject;
    }

    // returns null if this is NOT a const object
    // returns "parent" object if this IS a const object
    internal virtual object _GetConstObjectParent()
    {
      if (IntPtr.Zero != m_ptr)
        return null;

      return m__parent;
    }

    internal virtual IntPtr NonConstPointer()
    {
      if (IntPtr.Zero == m_ptr && m_subobject_index >= 0 && m__parent!=null)
      {
        Rhino.Geometry.PolyCurve pc = m__parent as Rhino.Geometry.PolyCurve;
        if (pc != null)
        {
          IntPtr pPolyCurve = pc.NonConstPointer();
          IntPtr pThis = UnsafeNativeMethods.ON_PolyCurve_SegmentCurve(pPolyCurve, m_subobject_index);
          return pThis;
        }

        Rhino.Geometry.BrepLoop loop = this as Rhino.Geometry.BrepLoop;
        if (loop != null)
        {
          IntPtr pBrep = loop.Brep.NonConstPointer();
          return UnsafeNativeMethods.ON_BrepLoop_GetPointer(pBrep, loop.LoopIndex);
        }
      }

      NonConstOperation(); // allows cached data to clean up
      return m_ptr;
    }

    // 27 Aug. 2012 - S. Baer
    // I'm not entirely confident that overriding Equals and GetHashCode
    // is a good thing since internal native pointers can change after
    // function calls suddenly making things that were equal into not equal
    /*
    /// <summary>
    /// Equals is overloaded to check against the underlying native pointer
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public override bool Equals(object obj)
    {
      CommonObject co = obj as CommonObject;
      if (co == null)
        return false;

      IntPtr pThis = ConstPointer();
      IntPtr pObj = co.ConstPointer();
      return pThis == pObj;
    }

    /// <summary>
    /// Uses the native pointer for hash code generation
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode()
    {
      IntPtr pThis = ConstPointer();
      return pThis.ToInt32();
    }
    */

    internal abstract IntPtr _InternalGetConstPointer();
    internal abstract IntPtr _InternalDuplicate(out bool applymempressure);

    /// <summary>
    /// For derived classes implementers.
    /// <para>Defines the necessary implementation to free the instance from being const.</para>
    /// </summary>
    protected virtual void NonConstOperation()
    {
      if ( IntPtr.Zero==m_ptr )
      {
        bool applymempressure;
        IntPtr pNewPointer = _InternalDuplicate(out applymempressure);
        m_ptr = pNewPointer;
        if (applymempressure)
          ApplyMemoryPressure();

#if RHINO_SDK
        Rhino.DocObjects.RhinoObject parent_object = m__parent as Rhino.DocObjects.RhinoObject;
        if (null != parent_object)
        {
          if (parent_object.m_original_geometry == this)
          {
            parent_object.m_original_geometry = null;
            parent_object.m_edited_geometry = this as Geometry.GeometryBase;
          }
          if (parent_object.m_original_attributes == this)
          {
            parent_object.m_original_attributes = null;
            parent_object.m_edited_attributes = this as DocObjects.ObjectAttributes;
          }
        }
#endif
        OnSwitchToNonConst();
      }
    }

    /// <summary>
    /// Is called when a non-const operation first occurs.
    /// </summary>
    protected virtual void OnSwitchToNonConst(){}

    /// <summary>
    /// If you want to keep a copy of this class around by holding onto it in a variable after a command
    /// completes, call EnsurePrivateCopy to make sure that this class is not tied to the document. You can
    /// call this function as many times as you want.
    /// </summary>
    public void EnsurePrivateCopy()
    {
      NonConstOperation();
    }

    /// <summary>
    /// If true this object may not be modified. Any properties or functions that attempt
    /// to modify this object when it is set to "IsReadOnly" will throw a NotSupportedException.
    /// </summary>
    public virtual bool IsDocumentControlled
    {
      get { return (IntPtr.Zero==m_ptr); }
    }

    /// <summary>
    /// Used for "temporary" wrapping of objects that we don't want .NET to destruct
    /// on disposal.
    /// </summary>
    internal void ReleaseNonConstPointer()
    {
      m_ptr = IntPtr.Zero;
    }
    bool m_bDestructOnDispose = true;
    internal void DoNotDestructOnDispose()
    {
      m_bDestructOnDispose = false;
    }

    internal void ConstructNonConstObject(IntPtr nativePointer)
    {
      m_ptr = nativePointer;
    }

    /// <summary>
    /// Assigns a parent object and a subobject index to this.
    /// </summary>
    /// <param name="parentObject">The parent object.</param>
    /// <param name="subobject_index">The subobject index.</param>
    protected void ConstructConstObject(object parentObject, int subobject_index)
    {
      m__parent = parentObject;
      m_subobject_index = subobject_index;
      // We may want to call GC.SuppressFinalize in this situation. This does mean
      // that we would have to tell the GC that the object needs finalization if
      // we in-place copy
    }

    /// <summary>Tests an object to see if it is valid.</summary>
    public virtual bool IsValid
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Object_IsValid(pConstThis, IntPtr.Zero);
      }
    }

    /// <summary>
    /// Determines if an object is valid. Also provides a report on errors if this
    /// object happens not to be valid.
    /// </summary>
    /// <param name="log">A textual log. This out parameter is assigned during this call.</param>
    /// <returns>true if this object is valid; false otherwise.</returns>
    public bool IsValidWithLog(out string log)
    {
      IntPtr pConstThis = ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        bool rc = UnsafeNativeMethods.ON_Object_IsValid(pConstThis, pString);
        log = sh.ToString();
        return rc;
      }
    }

    internal void ApplyMemoryPressure()
    {
      if (IntPtr.Zero != m_ptr)
      {
        uint current_size = UnsafeNativeMethods.ON_Object_SizeOf(m_ptr);
        long difference = current_size - m_unmanaged_memory;
        if (difference > 0)
        {
          GC.AddMemoryPressure(difference);
        }
        else if (difference < 0)
        {
          difference = -difference;
          GC.RemoveMemoryPressure(difference);
        }
        m_unmanaged_memory = current_size;
      }
    }

    #region IDisposable implementation
    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~CommonObject()
    {
      Dispose(false);
    }
    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero == m_ptr || m__parent is ConstCastHolder)
        return;

      Geometry.MeshHolder mh = m__parent as Geometry.MeshHolder;
      if (mh!=null)
      {
        mh.ReleaseMesh();
      }

      if (m_bDestructOnDispose)
      {
        bool inFinalizer = !disposing;
        if (inFinalizer)
        {
          // 11 Feb 2013 (S. Baer) RH-16157
          // When running in the finalizer, the destructor is being called on the GC
          // thread which results in nearly impossible to track down exceptions.
          // Mask the exception in this case and post information to our logging system
          // about the exception so we can better analyze and try to figure out what
          // is going on
          try
          {
            UnsafeNativeMethods.ON_Object_Delete(m_ptr);
          }
          catch (Exception ex)
          {
            HostUtils.ExceptionReport(ex);
          }
        }
        else
        {
          // See above. In this case we are running on the main thread of execution
          // and throwing an exception is a good thing so we can analyze and quickly
          // fix whatever is going wrong
          UnsafeNativeMethods.ON_Object_Delete(m_ptr);
        }
        if (m_unmanaged_memory > 0)
          GC.RemoveMemoryPressure(m_unmanaged_memory);
      }
      m_ptr = IntPtr.Zero;
    }
    #endregion

    internal void ApplyConstCast()
    {
      if (m_ptr == IntPtr.Zero && m__parent!=null)
      {
        IntPtr pConstThis = ConstPointer();
        ConstCastHolder ch = new ConstCastHolder(this, m__parent);
        m__parent = ch;
        m_ptr = pConstThis;
      }
    }
    internal void RemoveConstCast()
    {
      ConstCastHolder ch = m__parent as ConstCastHolder;
      if (m_ptr != IntPtr.Zero && ch!=null)
      {
        m__parent = ch.m_oldparent;
        m_ptr = IntPtr.Zero;
      }
    }
#if RHINO_SDK
    internal void ChangeToConstObject(Rhino.DocObjects.RhinoObject parent)
    {
      m_ptr = IntPtr.Zero;
      m__parent = parent;
    }
#endif

    /// <summary>
    /// Allows construction from inheriting classes.
    /// </summary>
    protected CommonObject() { }

    /// <summary>
    /// Gets true if this class has any custom information attached to it through UserData.
    /// </summary>
    public bool HasUserData
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_Object_FirstUserData(pConstThis) != IntPtr.Zero;
      }
    }

    Rhino.DocObjects.Custom.UserDataList m_userdatalist;
    /// <summary>
    /// List of custom information that is attached to this class.
    /// </summary>
    public Rhino.DocObjects.Custom.UserDataList UserData
    {
      get
      {
        return m_userdatalist ?? (m_userdatalist = new DocObjects.Custom.UserDataList(this));
      }
    }

    /// <summary>
    /// Dictionary of custom information attached to this class. The dictionary is actually user
    /// data provided as an easy to use sharable set of information.
    /// </summary>
    public Rhino.Collections.ArchivableDictionary UserDictionary
    {
      get
      {
        Rhino.DocObjects.Custom.UserDictionary ud = UserData.Find(typeof(Rhino.DocObjects.Custom.SharedUserDictionary)) as Rhino.DocObjects.Custom.SharedUserDictionary;
        if (ud == null)
        {
          ud = new DocObjects.Custom.SharedUserDictionary();
          if (!UserData.Add(ud))
            return null;
        }
        return ud.Dictionary;
      }
    }



    #region user strings
    /// <summary>
    /// Attach a user string (key,value combination) to this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve this string.</param>
    /// <param name="value">string associated with key.</param>
    /// <returns>true on success.</returns>
    internal bool _SetUserString(string key, string value)
    {
      //const lie
      IntPtr pThis = ConstPointer();
      bool rc = UnsafeNativeMethods.ON_Object_SetUserString(pThis, key, value);
      return rc;
    }
    /// <summary>
    /// Gets user string from this geometry.
    /// </summary>
    /// <param name="key">id used to retrieve the string.</param>
    /// <returns>string associated with the key if successful. null if no key was found.</returns>
    internal string _GetUserString(string key)
    {
      IntPtr pThis = ConstPointer();
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        UnsafeNativeMethods.ON_Object_GetUserString(pThis, key, pStringHolder);
        return sh.ToString();
      }
    }

    /// <summary>
    /// Gets the amount of user strings.
    /// </summary>
    internal int _UserStringCount
    {
      get
      {
        IntPtr pThis = ConstPointer();
        int rc = UnsafeNativeMethods.ON_Object_UserStringCount(pThis);
        return rc;
      }
    }

    ///// <summary>
    ///// Gets a copy of all (user key string, user value string) pairs attached to this geometry.
    ///// </summary>
    ///// <returns>A new collection.</returns>
    //internal System.Collections.Specialized.NameValueCollection _GetUserStrings()
    //{
    //  System.Collections.Specialized.NameValueCollection rc = new System.Collections.Specialized.NameValueCollection();
    //  IntPtr pThis = ConstPointer();
    //  int count = 0;
    //  IntPtr pUserStrings = UnsafeNativeMethods.ON_Object_GetUserStrings(pThis, ref count);

    //  using( var keyHolder = new StringHolder() )
    //  using( var valueHolder = new StringHolder() )
    //  {
    //    IntPtr pKeyHolder = keyHolder.NonConstPointer();
    //    IntPtr pValueHolder = valueHolder.NonConstPointer();

    //    for (int i = 0; i < count; i++)
    //    {
    //      UnsafeNativeMethods.ON_UserStringList_KeyValue(pUserStrings, i, true, pKeyHolder);
    //      UnsafeNativeMethods.ON_UserStringList_KeyValue(pUserStrings, i, false, pValueHolder);
    //      string key = keyHolder.ToString();
    //      string value = valueHolder.ToString();
    //      if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(value))
    //        rc.Add(key, value);
    //    }
    //  }

    //  if (IntPtr.Zero != pUserStrings)
    //    UnsafeNativeMethods.ON_UserStringList_Delete(pUserStrings);

    //  return rc;
    //}
    #endregion


//    #region serialization support
//    const string ARCHIVE_3DM_VERSION = "archive3dm";
//    const string ARCHIVE_OPENNURBS_VERSION = "opennurbs";
//    internal static IntPtr SerializeReadON_Object(SerializationInfo info, StreamingContext context)
//    {
//      int version = info.GetInt32("version");
//      int archive_3dm_version = info.GetInt32(ARCHIVE_3DM_VERSION);
//      int archive_opennurbs_version = info.GetInt32(ARCHIVE_OPENNURBS_VERSION);
//      byte[] stream = info.GetValue("data", typeof(byte[])) as byte[];
//      IntPtr rc = UnsafeNativeMethods.ON_ReadBufferArchive(archive_3dm_version, archive_opennurbs_version, stream.Length, stream);
//      return rc;
//    }

//    /// <summary>
//    /// Protected constructor for internal use.
//    /// </summary>
//    /// <param name="info">Serialization data.</param>
//    /// <param name="context">Serialization stream.</param>
//    protected CommonObject( SerializationInfo info, StreamingContext context)
//    {
//      m_ptr = SerializeReadON_Object(info, context);
//    }

//    internal static void SerializeWriteON_Object(IntPtr pConstOnObject, SerializationInfo info, StreamingContext context)
//    {
//      Rhino.FileIO.SerializationOptions options = context.Context as Rhino.FileIO.SerializationOptions;

//      uint length = 0;
//      bool writeuserdata = true;
//      if (options != null)
//        writeuserdata = options.WriteUserData;
//#if RHINO_SDK
//      int rhino_version = (options != null) ? options.RhinoVersion : RhinoApp.ExeVersion;
//#else
//      int rhino_version = (options != null) ? options.RhinoVersion : 5;
//#endif
//      IntPtr pWriteBuffer = UnsafeNativeMethods.ON_WriteBufferArchive_NewWriter(pConstOnObject, rhino_version, writeuserdata, ref length);

//      if (length < int.MaxValue)
//      {
//        int sz = (int)length;
//        IntPtr pByteArray = UnsafeNativeMethods.ON_WriteBufferArchive_Buffer(pWriteBuffer);
//        byte[] bytearray = new byte[sz];
//        System.Runtime.InteropServices.Marshal.Copy(pByteArray, bytearray, 0, sz);

//        info.AddValue("version", 10000);
//        info.AddValue(ARCHIVE_3DM_VERSION, rhino_version);
//        int archive_opennurbs_version = UnsafeNativeMethods.ON_Version();
//        info.AddValue(ARCHIVE_OPENNURBS_VERSION, archive_opennurbs_version);
//        info.AddValue("data", bytearray);
//      }
//      UnsafeNativeMethods.ON_WriteBufferArchive_Delete(pWriteBuffer);
//    }

//    /// <summary>
//    /// Populates a System.Runtime.Serialization.SerializationInfo with the data needed to serialize the target object.
//    /// </summary>
//    /// <param name="info">The System.Runtime.Serialization.SerializationInfo to populate with data.</param>
//    /// <param name="context">The destination (see System.Runtime.Serialization.StreamingContext) for this serialization.</param>
//    [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)]
//    public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
//    {
//      IntPtr pConstThis = ConstPointer();
//      SerializeWriteON_Object(pConstThis, info, context);
//    }
//    #endregion
  }

  class ConstCastHolder
  {
    public object m_oldparent;
    public ConstCastHolder(CommonObject obj, object old_parent)
    {
      m_oldparent = old_parent;
    }
  }
}
