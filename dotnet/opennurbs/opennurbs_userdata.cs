using System;
using System.Linq;
using System.Reflection;

namespace Rhino.DocObjects.Custom
{
  /// <summary>
  /// Provides a base class for custom classes of information which may be attached to
  /// geometry or attribute classes.
  /// </summary>
  public abstract class UserData : IDisposable
  {
    static int g_next_serial_number = 1;
    int m_serial_number=-1;
    IntPtr m_native_pointer = IntPtr.Zero;

    #region IDisposable implementation
    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~UserData()
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
      if (IntPtr.Zero == m_native_pointer)
        return;

      // Make sure that the class is not attached to an object before deleting.
      if (UnsafeNativeMethods.CRhCmnUserData_Delete(m_native_pointer, true))
      {
        g_attached_custom_user_datas.Remove(m_serial_number);
        m_native_pointer = IntPtr.Zero;
      }
    }
    #endregion

    internal virtual IntPtr NonConstPointer(bool createIfMissing)
    {
#if RHINO_SDK
      if (createIfMissing && IntPtr.Zero == m_native_pointer)
      {
        m_serial_number = g_next_serial_number++;
        Type t = GetType();
        Guid managed_type_id = t.GUID;
        string description = Description;

        if (this is UserDictionary)
        {
          Guid id = RhinoApp.Rhino5Id;
          m_native_pointer = UnsafeNativeMethods.CRhCmnUserData_New(m_serial_number, managed_type_id, id, description);
        }
        else
        {
          PlugIns.PlugIn plugin = PlugIns.PlugIn.Find(t.Assembly);
          Guid plugin_id = plugin.Id;
          m_native_pointer = UnsafeNativeMethods.CRhCmnUserData_New(m_serial_number, managed_type_id, plugin_id, description);
        }
      }
#endif
      return m_native_pointer;
    }

    /// <summary>Descriptive name of the user data.</summary>
    public virtual string Description { get { return "RhinoCommon UserData"; } }

    /// <summary>
    /// If you want to save this user data in a 3dm file, override
    /// ShouldWrite and return true.  If you do support serialization,
    /// you must also override the Read and Write functions.
    /// </summary>
    public virtual bool ShouldWrite { get { return false; } }

    /// <summary>Writes the content of this data to a stream archive.</summary>
    /// <param name="archive">An archive.</param>
    /// <returns>true if the data was successfully written. The default implementation always returns false.</returns>
    protected virtual bool Write(FileIO.BinaryArchiveWriter archive) { return false; }

    /// <summary>Reads the content of this data from a stream archive.</summary>
    /// <param name="archive">An archive.</param>
    /// <returns>true if the data was successfully written. The default implementation always returns false.</returns>
    protected virtual bool Read(FileIO.BinaryArchiveReader archive) { return false; }

    /// <summary>
    /// Is called when the object associated with this data is transformed. If you override this
    /// function, make sure to call the base class if you want the stored Transform to be updated.
    /// </summary>
    /// <param name="transform">The transform being applied.</param>
    protected virtual void OnTransform(Geometry.Transform transform)
    {
      UnsafeNativeMethods.ON_UserData_OnTransform(m_native_pointer, ref transform);
    }

    /// <summary>
    /// Is called when the object is being duplicated.
    /// </summary>
    /// <param name="source">The source data.</param>
    protected virtual void OnDuplicate(UserData source) { }

    internal delegate void TransformUserDataCallback(int serialNumber, ref Geometry.Transform xform);
    internal delegate int ArchiveUserDataCallback(int serialNumber);
    internal delegate int ReadWriteUserDataCallback(int serialNumber, int writing, IntPtr pBinaryArchive);
    internal delegate int DuplicateUserDataCallback(int serialNumber, IntPtr pNativeUserData);
    internal delegate IntPtr CreateUserDataCallback(Guid managedTypeId);
    internal delegate void DeleteUserDataCallback(int serialNumber);

    private static TransformUserDataCallback g_on_transform_user_data;
    private static ArchiveUserDataCallback g_on_archive;
    private static ReadWriteUserDataCallback g_on_read_write;
    private static DuplicateUserDataCallback g_on_duplicate;
    private static CreateUserDataCallback g_on_create;
    private static DeleteUserDataCallback g_on_delete;

    private static void OnTransformUserData(int serialNumber, ref Geometry.Transform xform)
    {
      UserData ud = FromSerialNumber(serialNumber);
      if (ud!=null)
      {
        try
        {
          ud.OnTransform(xform);
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
    }
    private static int OnArchiveUserData(int serialNumber)
    {
      int rc = 0; //FALSE
      UserData ud = FromSerialNumber(serialNumber);
      if (ud != null)
      {
        try
        {
          // the user data class MUST have a GuidAttribute in order to write
          var attr = ud.GetType().GetTypeInfo().GetCustomAttributes(typeof(System.Runtime.InteropServices.GuidAttribute), false);
          if( attr.Count()==1 )
            rc = ud.ShouldWrite ? 1 : 0;
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }
    private static int OnReadWriteUserData(int serialNumber, int writing, IntPtr pBinaryArchive)
    {
      int rc = 0; //FALSE
      UserData ud = FromSerialNumber(serialNumber);
      if (ud != null)
      {
        try
        {
          if (0 == writing)
          {
            FileIO.BinaryArchiveReader reader = new FileIO.BinaryArchiveReader(pBinaryArchive);
            rc = ud.Read(reader) ? 1 : 0;
          }
          else
          {
            FileIO.BinaryArchiveWriter writer = new FileIO.BinaryArchiveWriter(pBinaryArchive);
            rc = ud.Write(writer) ? 1 : 0;
          }
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }
    private static int OnDuplcateUserData(int serialNumber, IntPtr pNativeUserData)
    {
      int rc = 0;
      UserData ud = FromSerialNumber(serialNumber);
      if (ud != null)
      {
        try
        {
          Type t = ud.GetType();
          UserData new_ud = Activator.CreateInstance(t) as UserData;
          if (new_ud != null)
          {
            new_ud.m_serial_number = g_next_serial_number++;
            new_ud.m_native_pointer = pNativeUserData;
            StoreInRuntimeList(new_ud);
            new_ud.OnDuplicate(ud);
            rc = new_ud.m_serial_number;
          }
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }
    private static IntPtr OnCreateInstance(Guid managedTypeId)
    {
      IntPtr rc = IntPtr.Zero;
      Type t = null;
      for (int i = 0; i < g_types.Count; i++)
      {
        if (g_types[i].GetTypeInfo().GUID == managedTypeId)
        {
          t = g_types[i];
          break;
        }
      }
      if (t != null)
      {
        try
        {
          UserData ud = Activator.CreateInstance(t) as UserData;
          if (ud != null)
          {
            rc = ud.NonConstPointer(true);
            if (ud.m_serial_number > 0)
              g_attached_custom_user_datas.Add(ud.m_serial_number, ud);
          }
        }
        catch (Exception ex)
        {
          Runtime.HostUtils.ExceptionReport(ex);
        }
      }
      return rc;
    }
    private static void OnDelete(int serialNumber)
    {
      UserData ud = FromSerialNumber(serialNumber);
      if( ud!=null )
      {
        ud.m_native_pointer = IntPtr.Zero;
        GC.SuppressFinalize(ud);
        g_attached_custom_user_datas.Remove(serialNumber);
      }
    }

    static readonly System.Collections.Generic.List<Type> g_types = new System.Collections.Generic.List<Type>();
    internal static void RegisterType(Type t)
    {
      g_types.Add(t);

      g_on_transform_user_data = OnTransformUserData;
      g_on_archive = OnArchiveUserData;
      g_on_read_write = OnReadWriteUserData;
      g_on_duplicate = OnDuplcateUserData;
      g_on_create = OnCreateInstance;
      g_on_delete = OnDelete;
      UnsafeNativeMethods.CRhCmnUserData_SetCallbacks(g_on_transform_user_data, g_on_archive, g_on_read_write, g_on_duplicate, g_on_create, g_on_delete);
    }
    #region statics

    static readonly System.Collections.Generic.Dictionary<int, UserData> g_attached_custom_user_datas = new System.Collections.Generic.Dictionary<int, UserData>();
    internal static UserData FromSerialNumber(int serialNumber)
    {
      if (serialNumber < 1)
        return null;
      UserData rc;
      if (g_attached_custom_user_datas.TryGetValue(serialNumber, out rc))
        return rc;

      return null;
    }
    internal static void StoreInRuntimeList(UserData ud)
    {
      g_attached_custom_user_datas[ud.m_serial_number] = ud;
    }
    internal static void RemoveFromRuntimeList(UserData ud)
    {
      g_attached_custom_user_datas.Remove(ud.m_serial_number);
    }


    /// <summary>
    /// Expert user tool that copies user data that has a positive 
    /// CopyCount from the source object to a destination object.
    /// Generally speaking you don't need to use Copy().
    /// Simply rely on things like the copy constructors to do the right thing.
    /// </summary>
    /// <param name="source">A source object for the data.</param>
    /// <param name="destination">A destination object for the data.</param>
    public static void Copy(Runtime.CommonObject source, Runtime.CommonObject destination)
    {
      IntPtr const_source = source.ConstPointer();
      IntPtr ptr_destination = destination.NonConstPointer();
      UnsafeNativeMethods.ON_Object_CopyUserData(const_source, ptr_destination);
    }

    /// <summary>
    /// Moves the user data from objectWithUserData to a temporary data storage
    /// identifierd by the return Guid.  When MoveUserDataFrom returns, the
    /// objectWithUserData will not have any user data.
    /// </summary>
    /// <param name="objectWithUserData">Object with user data attached.</param>
    /// <returns>
    /// Guid identifier for storage of UserData that is held in a temporary list
    /// by this class. This function should be used in conjunction with MoveUserDataTo
    /// to transfer the user data to a different object.
    /// Returns Guid.Empty if there was no user data to transfer.
    /// </returns>
    public static Guid MoveUserDataFrom(Runtime.CommonObject objectWithUserData)
    {
      Guid id = Guid.NewGuid();
      IntPtr const_ptr_onobject = objectWithUserData.ConstPointer();
      if (UnsafeNativeMethods.ON_UserDataHolder_MoveUserDataFrom(id, const_ptr_onobject))
        return id;
      return Guid.Empty;
    }
    
    /// <summary>
    /// Moves the user data.
    /// <para>See <see cref="MoveUserDataFrom"/> for more information.</para>
    /// </summary>
    /// <param name="objectToGetUserData">Object data source.</param>
    /// <param name="id">Target.</param>
    /// <param name="append">If the data should be appended or replaced.</param>
    public static void MoveUserDataTo(Runtime.CommonObject objectToGetUserData, Guid id, bool append)
    {
      if (id != Guid.Empty)
      {
        IntPtr const_ptr_onobject = objectToGetUserData.ConstPointer();
        UnsafeNativeMethods.ON_UserDataHolder_MoveUserDataTo(id, const_ptr_onobject, append);
      }
    }

    Geometry.Transform m_cached_transform = Geometry.Transform.Identity;
    /// <summary>
    /// Updated if user data is attached to a piece of geometry that is
    /// transformed and the virtual OnTransform() is not overridden.  If you
    /// override OnTransform() and want Transform to be updated, then call the 
    /// base class OnTransform() in your override.
    /// The default constructor sets Transform to the identity.
    /// </summary>
    public Geometry.Transform Transform
    {
      get
      {
        if (IntPtr.Zero != m_native_pointer)
        {
          UnsafeNativeMethods.ON_UserData_GetTransform(m_native_pointer, ref m_cached_transform);
        }
        return m_cached_transform;
      }
      protected set
      {
        m_cached_transform = value;
        if (IntPtr.Zero != m_native_pointer)
        {
          UnsafeNativeMethods.ON_UserData_SetTransform(m_native_pointer, ref m_cached_transform);
        }
      }
    }
    #endregion
  }

  /// <summary>
  /// Represents user data with unknown origin.
  /// </summary>
  public class UnknownUserData : UserData
  {
    /// <summary>
    /// Constructs a new unknown data entity.
    /// </summary>
    /// <param name="pointerNativeUserData">A pointer to the entity.</param>
    public UnknownUserData(IntPtr pointerNativeUserData)
    {
    }
  }

  /// <summary>Represents a collection of user data.</summary>
  public class UserDataList
  {
    readonly Runtime.CommonObject m_parent;
    internal UserDataList(Runtime.CommonObject parent)
    {
      m_parent = parent;
    }

    /// <summary>Number of UserData objects in this list.</summary>
    public int Count
    {
      get
      {
        IntPtr const_ptr_onobject = m_parent.ConstPointer();
        return UnsafeNativeMethods.ON_Object_UserDataCount(const_ptr_onobject);
      }
    }

    /// <summary>
    /// If the userdata is already in a different UserDataList, it
    /// will be removed from that list and added to this list.
    /// </summary>
    /// <param name="userdata">Data element.</param>
    /// <returns>Whether this operation succeeded.</returns>
    public bool Add(UserData userdata)
    {
      if (!(userdata is SharedUserDictionary))
      {
        Type t = userdata.GetType();
        if (!t.GetRuntimeMethods().Any(m => m.IsConstructor && m.GetParameters().Length == 0 && m.IsPublic))
          throw new ArgumentException("userdata must be a public class and have a parameterless constructor");
      }
      IntPtr const_ptr_onobject = m_parent.ConstPointer();
      IntPtr ptr_userdata = userdata.NonConstPointer(true);
      const bool detach_if_needed = true;
      bool rc = UnsafeNativeMethods.ON_Object_AttachUserData(const_ptr_onobject, ptr_userdata, detach_if_needed);
      if (rc)
        UserData.StoreInRuntimeList(userdata);
      return rc;
    }
    
    /// <summary>
    /// Remove the userdata from this list
    /// </summary>
    /// <param name="userdata"></param>
    /// <returns>true if the user data was successfully removed</returns>
    public bool Remove(UserData userdata)
    {
      IntPtr const_ptr_onobject = m_parent.ConstPointer();
      IntPtr ptr_userdata = userdata.NonConstPointer(false);
      bool rc = UnsafeNativeMethods.ON_Object_DetachUserData(const_ptr_onobject, ptr_userdata);
      if( rc )
        UserData.RemoveFromRuntimeList(userdata);
      return rc;
    }


    /// <summary>
    /// Finds a specific data type in this regulated collection.
    /// </summary>
    /// <param name="userdataType">A data type.</param>
    /// <returns>The found data, or null of nothing was found.</returns>
    public UserData Find(Type userdataType)
    {
      if (!userdataType.GetTypeInfo().IsSubclassOf(typeof(UserData)))
        return null;
      Guid id = userdataType.GetTypeInfo().GUID;
      IntPtr const_ptr_onobject = m_parent.ConstPointer();
      int serial_number = UnsafeNativeMethods.CRhCmnUserData_Find(const_ptr_onobject, id);
      return UserData.FromSerialNumber(serial_number);
    }
  }

  /// <summary>
  /// Defines the storage data class for a <see cref="Rhino.Collections.ArchivableDictionary">user dictionary</see>.
  /// </summary>
  [System.Runtime.InteropServices.Guid("171E831F-7FEF-40E2-9857-E5CCD39446F0")]
  public class UserDictionary : UserData
  {
    Collections.ArchivableDictionary m_dictionary;
    /// <summary>
    /// Gets the dictionary that is associated with this class.
    /// <para>This dictionary is unique.</para>
    /// </summary>
    public Collections.ArchivableDictionary Dictionary
    {
      get { return m_dictionary??(m_dictionary=new Collections.ArchivableDictionary(this)); }
    }

    /// <summary>
    /// Gets the text "RhinoCommon UserDictionary".
    /// </summary>
    public override string Description
    {
      get
      {
        return "RhinoCommon UserDictionary";
      }
    }

    ///// <summary>
    ///// Clones the user data.
    ///// </summary>
    ///// <param name="source">The source data.</param>
    //protected override void OnDuplicate(UserData source)
    //{
    //  UserDictionary dict = source as UserDictionary;
    //  if (dict != null)
    //  {
    //    m_dictionary = dict.m_dictionary.Clone();
    //    m_dictionary.SetParentUserData(this);
    //  }
    //}

    /// <summary>
    /// Writes this entity if the count is larger than 0.
    /// </summary>
    public override bool ShouldWrite
    {
      get { return m_dictionary.Count > 0; }
    }

    /// <summary>
    /// Is called to read this entity.
    /// </summary>
    /// <param name="archive">An archive.</param>
    /// <returns>Always returns true.</returns>
    protected override bool Read(FileIO.BinaryArchiveReader archive)
    {
      m_dictionary = archive.ReadDictionary();
      return true;
    }

    /// <summary>
    /// Is called to write this entity.
    /// </summary>
    /// <param name="archive">An archive.</param>
    /// <returns>Always returns true.</returns>
    protected override bool Write(FileIO.BinaryArchiveWriter archive)
    {
      archive.WriteDictionary(Dictionary);
      return true;
    }
  }

  [System.Runtime.InteropServices.Guid("2544A64E-220D-4D65-B8D4-611BB57B46C7")]
  class SharedUserDictionary : UserDictionary
  {
  }
}