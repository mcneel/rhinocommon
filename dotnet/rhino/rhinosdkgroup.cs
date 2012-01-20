#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
//[skipping] CRhinoGroup

namespace Rhino.DocObjects.Tables
{
  public enum GroupTableEventType : int
  {
    Added = 0,
    Deleted = 1,
    Undeleted = 2,
    Modified = 3,
    Sorted = 4
  }

  public class GroupTableEventArgs : EventArgs
  {
    readonly int m_docId;
    readonly GroupTableEventType m_event_type;

    internal GroupTableEventArgs(int docId, int event_type)
    {
      m_docId = docId;
      m_event_type = (GroupTableEventType)event_type;
    }

    RhinoDoc m_doc = null;
    public RhinoDoc Document
    {
      get { return m_doc ?? (m_doc = RhinoDoc.FromId(m_docId)); }
    }

    public GroupTableEventType EventType
    {
      get{ return m_event_type; }
    }
  }

  /// <summary>
  /// Group tables store the list of groups in a Rhino document.
  /// </summary>
  public sealed class GroupTable
  {
    private readonly RhinoDoc m_doc;
    private GroupTable() { }
    internal GroupTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this group table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of groups in the group table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoGroupTable_GroupCount(m_doc.m_docId);
      }
    }

    /// <summary>Finds a group with a given name.</summary>
    /// <param name="groupName">
    /// Name of group to search for. Ignores case.
    /// </param>
    /// <param name="ignoreDeletedGroups">
    /// true means don't search deleted groups.
    /// </param>
    /// <returns>
    /// &gt;=0 index of the group with the given name.
    /// -1 no group found with the given name.
    /// </returns>
    public int Find(string groupName, bool ignoreDeletedGroups)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_FindGroup(m_doc.m_docId, groupName, ignoreDeletedGroups);
    }

    /// <summary>Adds a new empty group to the group table.</summary>
    /// <param name="groupName">name of new group.</param>
    /// <returns>
    /// &gt;=0 index of new group. 
    /// -1 group not added because a group with that name already exists.
    /// </returns>
    /// <remarks>
    /// In some cases, calling Add() can cause the group indices to become invalid.
    /// </remarks>
    public int Add(string groupName)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.m_docId, groupName, 0, null);
    }

    /// <summary>Adds a new empty group to the group table.</summary>
    /// <returns>
    /// &gt;=0 index of new group. 
    /// -1 group not added because a group with that name already exists.
    /// </returns>
    /// <remarks>
    /// In some cases, calling Add() can cause the group indices to become invalid.
    /// </remarks>
    public int Add()
    {
      return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.m_docId, null, 0, null);
    }

    /// <summary>
    /// Adds a new group to the group table with a set of objects.
    /// </summary>
    /// <param name="groupName">Name of new group.</param>
    /// <param name="objectIds">An array, a list or any enumerable set of object IDs.</param>
    /// <returns>
    /// &gt;=0 index of new group. 
    /// <para>-1 group not added because a group with that name already exists.</para>
    /// </returns>
    /// <remarks>
    /// In some cases, calling Add() can cause the group indices to become invalid.
    /// </remarks>
    public int Add(string groupName, System.Collections.Generic.IEnumerable<Guid> objectIds)
    {
      if( null==objectIds )
        return Add(groupName);
      Rhino.Collections.RhinoList<Guid> ids = new Rhino.Collections.RhinoList<Guid>();
      foreach( Guid id in objectIds)
      {
        ids.Add(id);
      }
      if( ids.Count<1 )
        return Add(groupName);
      return UnsafeNativeMethods.CRhinoGroupTable_Add(m_doc.m_docId, groupName, ids.Count, ids.m_items);
    }

    /// <summary>
    /// Adds a new group to the group table with a set of objects.
    /// </summary>
    /// <param name="objectIds">An array, a list or any enumerable set of object IDs.</param>
    /// <returns>
    /// &gt;=0 index of new group.
    /// <para>-1 group not added because a group with that name already exists.</para>
    /// </returns>
    /// <remarks>
    /// In some cases, calling Add() can cause the group indices to become invalid.
    /// </remarks>
    /// <example>
    /// <code source='examples\vbnet\ex_addobjectstogroup.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_addobjectstogroup.cs' lang='cs'/>
    /// <code source='examples\py\ex_addobjectstogroup.py' lang='py'/>
    /// </example>
    public int Add(System.Collections.Generic.IEnumerable<Guid> objectIds)
    {
      return Add(null, objectIds);
    }

    /// <summary>
    /// Adds an object to an existing group.
    /// </summary>
    /// <param name="groupIndex">The group index value.</param>
    /// <param name="objectId">An ID of an object.</param>
    /// <returns>true if the operation was successful.</returns>
    public bool AddToGroup(int groupIndex, Guid objectId)
    {
      Guid[] ids = new Guid[] { objectId };
      return UnsafeNativeMethods.CRhinoGroupTable_AddToGroup(m_doc.m_docId, groupIndex, 1, ids);
    }

    /// <summary>
    /// Adds a list of objects to an existing group.
    /// </summary>
    /// <param name="groupIndex">The group index value.</param>
    /// <param name="objectIds">An array, a list or any enumerable set of IDs to objects.</param>
    /// <returns>true if at least an operation was successful.</returns>
    public bool AddToGroup(int groupIndex, System.Collections.Generic.IEnumerable<Guid> objectIds)
    {
      if (null == objectIds)
        return false;
      Rhino.Collections.RhinoList<Guid> ids = new Rhino.Collections.RhinoList<Guid>();
      foreach (Guid id in objectIds)
      {
        ids.Add(id);
      }
      if (ids.Count < 1)
        return false;
      return UnsafeNativeMethods.CRhinoGroupTable_AddToGroup(m_doc.m_docId, groupIndex, ids.Count, ids.m_items);
    }

    /// <summary>
    /// Deletes a group from this table.
    /// <para>Deleted groups are kept in the runtime group table so that undo
    /// will work with groups.  Call IsDeleted() to determine if a group is deleted.</para>
    /// </summary>
    /// <param name="groupIndex">An group index to be deleted.</param>
    /// <returns>true if the operation was successful.</returns>
    public bool Delete(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_DeleteGroup(m_doc.m_docId, groupIndex, true);
    }

    public bool Undelete(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_DeleteGroup(m_doc.m_docId, groupIndex, false);
    }

    public bool IsDeleted(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_IsDeleted(m_doc.m_docId, groupIndex);
    }

    public string GroupName(int groupIndex)
    {
      IntPtr pName = UnsafeNativeMethods.CRhinoGroupTable_GroupName(m_doc.m_docId, groupIndex);
      if( IntPtr.Zero==pName )
        return null;
      return Marshal.PtrToStringUni(pName);
    }

    public bool ChangeGroupName(int groupIndex, string newName)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_ChangeGroupName(m_doc.m_docId, groupIndex, newName);
    }

    public string[] GroupNames( bool ignoreDeletedGroups )
    {
      int count = Count;
      if (count < 1)
        return null;
      Rhino.Collections.RhinoList<string> names = new Rhino.Collections.RhinoList<string>(count);
      for (int i = 0; i < count; i++)
      {
        if (ignoreDeletedGroups && IsDeleted(i))
          continue;
        string name = GroupName(i);
        if (!string.IsNullOrEmpty(name))
          names.Add(name);
      }
      if (names.Count < 1)
        return null;
      return names.ToArray();
    }

    const int idxHideGroup = 0;
    const int idxShowGroup = 1;
    const int idxLockGroup = 2;
    const int idxUnlockGroup = 3;
    const int idxGroupObjectCount = 4;

    public int Hide(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.m_docId, groupIndex, idxHideGroup);
    }
    public int Show(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.m_docId, groupIndex, idxShowGroup);
    }
    public int Lock(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.m_docId, groupIndex, idxLockGroup);
    }
    public int Unlock(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.m_docId, groupIndex, idxUnlockGroup);
    }
    public int GroupObjectCount(int groupIndex)
    {
      return UnsafeNativeMethods.CRhinoGroupTable_GroupOp(m_doc.m_docId, groupIndex, idxGroupObjectCount);
    }

    /// <summary>
    /// Gets an array of all of the objects in a group.
    /// </summary>
    /// <param name="groupIndex">The index of the group in this table.</param>
    /// <returns>An array with all the objects in the specified group.</returns>
    public RhinoObject[] GroupMembers(int groupIndex)
    {
      using (Rhino.Runtime.INTERNAL_RhinoObjectArray rhobjs = new Runtime.INTERNAL_RhinoObjectArray())
      {
        IntPtr pRhinoObjects = rhobjs.NonConstPointer();
        UnsafeNativeMethods.CRhinoGroupTable_GroupMembers(m_doc.m_docId, groupIndex, pRhinoObjects);
        return rhobjs.ToArray();
      }
    }
  }
}
#endif