using System;
using System.Runtime.InteropServices;
using Rhino.Geometry;

namespace Rhino.DocObjects
{
  /// <summary>
  /// The possible relationships between the instance definition geometry
  /// and the archive containing the original defition.
  /// </summary>
  public enum InstanceDefinitionUpdateType : int
  {
    /// <summary>
    /// This instance definition is never updated. If m_source_archive is set,
    /// it records the origin of the instance definition geometry, but
    /// m_source_archive is never used to update the instance definition
    /// </summary>
    Static = 0,
    /// <summary>
    /// This instance definition geometry was imported from another archive (m_source_archive)
    /// and is embedded. If m_source_archive changes, the user is asked if they want to update
    /// the instance definition.
    /// </summary>
    Embedded = 1,
    /// <summary>
    /// This instance definition geometry was imported from another archive (m_source_archive)
    /// and is embedded. If m_source_archive changes, the instance definition is automatically
    /// updated. If m_source_archive is not available, the instance definition is still valid.
    /// </summary>
    LinkedAndEmbedded = 2,
    /// <summary>
    /// This instance definition geometry was imported from another archive (m_source_archive)
    /// and is not embedded. If m_source_archive changes, the instance definition is automatically
    /// updated. If m_source_archive is not available, the instance definition is not valid.
    /// This does not save runtime memory.  It may save a little disk space, but it is a  foolish
    /// option requested by people who do not understand all the issues.
    /// </summary>
    Linked = 3
  }

  public class InstanceObject : RhinoObject
  {
    internal InstanceObject(uint serialNumber)
      : base(serialNumber) { }

    /// <summary>
    /// transformation applied to an instance definition for this object
    /// </summary>
    public Transform InstanceXform
    {
      get
      {
        Transform xf = new Transform();
        IntPtr ptr = ConstPointer();
        UnsafeNativeMethods.CRhinoInstanceObject_InstanceXform(ptr, ref xf);
        return xf;
      }
    }

    /// <summary>
    /// instance definition that this object uses
    /// </summary>
    public InstanceDefinition InstanceDefinition
    {
      get
      {
        IntPtr ptr = ConstPointer();
        int docId = 0;
        int idef_index = UnsafeNativeMethods.CRhinoInstanceObject_InstanceDefinition(ptr, ref docId);
        if (idef_index < 0)
          return null;
        RhinoDoc doc = RhinoDoc.FromId(docId);
        return new InstanceDefinition(idef_index, doc);
      }
    }
  }

  public sealed class InstanceDefinition // don't derive from ON_InstanceDefinition. We want this class to be read only
  {
    private int m_index;
    private RhinoDoc m_doc;
    private InstanceDefinition() { }
    internal InstanceDefinition(int index, RhinoDoc doc)
    {
      m_index = index;
      m_doc = doc;
    }

    /// <summary>
    /// Number of objects this definition uses. This counts the objects that are used to define the geometry.
    /// This does NOT count the number of references to this instance definition.
    /// </summary>
    public int ObjectCount
    {
      get
      {
        return UnsafeNativeMethods.CRhinoInstanceDefinition_ObjectCount(m_doc.m_docId, m_index);
      }
    }

    public InstanceDefinitionUpdateType UpdateType
    {
      get
      {
        int rc = UnsafeNativeMethods.CRhinoInstanceDefinition_UpdateType(m_doc.m_docId, m_index);
        return (InstanceDefinitionUpdateType)rc;
      }
    }

    /// <summary>
    /// returns an object used as part of this definition.
    /// </summary>
    /// <param name="index">0 &lt;= index &lt; ObjectCount</param>
    /// <returns>
    /// Returns an object that is used to define the geometry.
    /// Does NOT return an object that references this definition.count the number of references to this instance
    /// </returns>
    public DocObjects.RhinoObject Object(int index)
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoInstanceDefinition_Object(m_doc.m_docId, m_index, index);
      return DocObjects.RhinoObject.CreateRhinoObjectHelper(ptr);
    }

    /// <summary>
    /// Get a list of the objects that belong to this instance definition.
    /// </summary>
    /// <returns></returns>
    public DocObjects.RhinoObject[] GetObjects()
    {
      int count = ObjectCount;
      DocObjects.RhinoObject[] rc = new RhinoObject[count];
      for (int i = 0; i < count; i++)
      {
        IntPtr ptr = UnsafeNativeMethods.CRhinoInstanceDefinition_Object(m_doc.m_docId, m_index, i);
        rc[i] = DocObjects.RhinoObject.CreateRhinoObjectHelper(ptr);
      }
      return rc;
    }

    /// <summary>
    /// Get a list of the CRhinoInstanceObjects (inserts) that contains
    /// a reference this instance definition.
    /// </summary>
    /// <param name="wheretoLook">
    /// 0 = get top level references in active document
    /// 1 = get top level and nested references in active document
    /// 2 = check for references from other instance definitions
    /// </param>
    /// <returns></returns>
    public InstanceObject[] GetReferences(int wheretoLook)
    {
      int refCount = UnsafeNativeMethods.CRhinoInstanceDefintition_GetReferences1(m_doc.m_docId, m_index, wheretoLook);
      if (refCount < 1)
        return null;
      InstanceObject[] rc = new InstanceObject[refCount];
      for (int i = 0; i < refCount; i++)
      {
        IntPtr ptr = UnsafeNativeMethods.CRhinoInstanceDefinition_GetReferences2(i);
        if (ptr != IntPtr.Zero)
        {
          uint sn = UnsafeNativeMethods.CRhinoObject_RuntimeSN(ptr);
          rc[i] = new InstanceObject(sn);
        }
      }
      UnsafeNativeMethods.CRhinoInstanceDefinition_GetReferences3();
      return rc;
    }

    /// <summary>
    /// Get a list of all the InstanceDefinitions that contain a reference this InstanceDefinition.
    /// </summary>
    /// <returns></returns>
    public InstanceDefinition[] GetContainers()
    {
      Runtime.INTERNAL_IntArray arr = new Rhino.Runtime.INTERNAL_IntArray();
      IntPtr ptr = arr.m_ptr;
      int count = UnsafeNativeMethods.CRhinoInstanceDefinition_GetContainers(m_doc.m_docId, m_index, ptr);
      InstanceDefinition[] rc = null;
      if( count>0 )
      {
        int[] indices = arr.ToArray();
        if (indices != null)
        {
          count = indices.Length;
          rc = new InstanceDefinition[count];
          for (int i = 0; i < count; i++)
          {
            rc[i] = new InstanceDefinition(indices[i], m_doc);
          }
        }
      }
      arr.Dispose();
      return rc;
    }

    /// <summary>
    /// Determine if this instance definition contains a reference to another instance definition.
    /// </summary>
    /// <param name="otherIdefIndex">index of another instance definition</param>
    /// <returns>
    ///   0      no
    ///   1      other_idef_index is the index of this instance definition
    ///  >1      This InstanceDefinition uses the instance definition
    ///          and the returned value is the nesting depth.
    /// </returns>
    public int UsesDefinition(int otherIdefIndex)
    {
      return UnsafeNativeMethods.CRhinoInstanceDefinition_UsesDefinition(m_doc.m_docId, m_index, otherIdefIndex);
    }

    /// <summary>
    /// Returns true if the instance definition is refereneced.
    /// </summary>
    /// <param name="wheretoLook">
    /// 0 = check for top level references in active document
    /// 1 = check for top level and nested references in active document
    /// 2 = check for references in other instance definitions
    /// </param>
    /// <returns></returns>
    public bool InUse(int wheretoLook)
    {
      return UnsafeNativeMethods.CRhinoInstanceDefinition_InUse(m_doc.m_docId, m_index, wheretoLook);
    }

    /// <summary>
    /// index of this instance definition in the index definition table.
    /// </summary>
    public int Index
    {
      get { return m_index; }
    }

    //[skipping]
    // int m_sort_index;   
    // int m_remap_index;
    public bool IsReference
    {
      get
      {
        return UnsafeNativeMethods.CRhinoInstanceDefinition_IsReference(m_doc.m_docId, m_index);
      }
    }

    public bool IsDeleted
    {
      get
      {
        return UnsafeNativeMethods.CRhinoInstanceDefinition_IsDeleted(m_doc.m_docId, m_index);
      }
    }
    //[skipping]
    //bool IsTenuous() const;
    //BOOL CRhinoInstanceDefinition::GetBBox(
    //bool UsesLayer( int layer_index ) const;
    //bool UsesLinetype( int linetype_index) const;
    //bool CRhinoInstanceDefinition::RemoveLinetypeReference( int linetype_index);

    ////////////////////////////////////////////////////////
    //from ON_InstanceDefinition
    const int idxName = 0;
    const int idxDescription = 1;
    const int idxSourceArchive = 2;
    string GetString(int which)
    {
      IntPtr ptr = UnsafeNativeMethods.CRhinoInstanceDefinition_GetString(m_doc.m_docId, m_index, which);
      if (IntPtr.Zero == ptr)
        return null;
      return Marshal.PtrToStringUni(ptr);
    }

    public string Name
    {
      get{ return GetString(idxName); }
    }

    public string Description
    {
      get{ return GetString(idxDescription); }
    }

    public Guid Id
    {
      get { return UnsafeNativeMethods.CRhinoInstanceDefinition_GetUuid(m_doc.m_docId, m_index); }
    }

    public string SourceArchive
    {
      get { return GetString(idxSourceArchive); }
    }
  }
}


namespace Rhino.DocObjects.Tables
{
  public sealed class InstanceDefinitionTable
  {
    private RhinoDoc m_doc;
    private InstanceDefinitionTable() { }
    internal InstanceDefinitionTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of items in the instance definitions table</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoInstanceDefinitionTable_InstanceDefinitionCount(m_doc.m_docId);
      }
    }
  
    //[skipping]
    //const CRhinoInstanceDefinition* operator[]( int idef_index ) const;

    /// <summary>Finds the instance definition with a given name</summary>
    /// <param name="instanceDefinitionName">name of instance definition to search for (ignores case)</param>
    /// <param name="ignoreDeletedInstanceDefinitions">true means don't search deleted instance definitions</param>
    /// <returns></returns>
    public DocObjects.InstanceDefinition Find(string instanceDefinitionName, bool ignoreDeletedInstanceDefinitions)
    {
      int index = UnsafeNativeMethods.CRhinoInstanceDefinitionTable_FindInstanceDefinition(m_doc.m_docId,
                                                                                      instanceDefinitionName,
                                                                                      ignoreDeletedInstanceDefinitions);
      if (index < 0)
        return null;
      return new Rhino.DocObjects.InstanceDefinition(index, m_doc);
    }

    /// <summary>Finds the instance definition with a given id</summary>
    /// <param name="instanceId">Unique id of the instance definition to search for</param>
    /// <param name="ignoreDeletedInstanceDefinitions">true means don't search deleted instance definitions</param>
    /// <returns></returns>
    public DocObjects.InstanceDefinition Find(Guid instanceId, bool ignoreDeletedInstanceDefinitions)
    {
      int index = UnsafeNativeMethods.CRhinoInstanceDefinitionTable_FindInstanceDefinition2(m_doc.m_docId,
                                                                                       instanceId,
                                                                                       ignoreDeletedInstanceDefinitions);
      if (index < 0)
        return null;
      return new Rhino.DocObjects.InstanceDefinition(index, m_doc);
    }

  //Description:
  //  Add an instance definition to the instance definition table.
  //Parameters:
  //  idef - [in] the UUID in idef is used to find the objects
  //  bReference - [in] true if this is from a reference file
  //  bQuiet - [in] true if errors should be silent
  //Returns:
  //   -1   failure
  //  >=0   index of instance definition in the instance definition table.
  //*/
  //int AddInstanceDefinition( 
  //        const ON_InstanceDefinition& idef, 
  //        bool bReference = false,
  //        bool bQuiet = false
  //        );

  //Description:
  //  Add an instance definition to the instance definition table.
  //Parameters:
  //  idef - [in] the UUID in idef is ignored.
  //  object - [in] will be moved to instance geometry list
  //  bReference - [in] true if this is from a reference file
  //  bQuiet - [in] true if errors should be silent
  //Returns:
  //   -1   failure
  //  >=0   index of instance definition in the instance definition table.
  //*/
  //int AddInstanceDefinition( 
  //        const ON_InstanceDefinition& idef, 
  //        const CRhinoObject* object,
  //        bool bReference = false,
  //        bool bQuiet = false
  //        );

  //Description:
  //  Add an instance definition to the instance definition table.
  //Parameters:
  //  idef - [in] The UUID in idef is ignored.
  //  objects - [in] will be moved to instance geometry list
  //  bReference - [in] true if this is from a reference file
  //  bQuiet - [in] true if errors should be silent
  //Returns:
  //   -1   failure
  //  >=0   index of instance definition in the instance definition table.
  //*/
  //int AddInstanceDefinition( 
  //        const ON_InstanceDefinition& idef, 
  //        const ON_SimpleArray<const CRhinoObject*>& objects,
  //        bool bReference = false,
  //        bool bQuiet = false
  //        );

  //CRhinoInstanceObject* CreateInstanceObject( 
  //  int instance_definition_index,
  //  ON_Xform instance_xform,
  //  const ON_3dmObjectAttributes* instance_attributes = NULL,
  //  CRhinoHistory* pHistory = NULL,
  //  bool bReference = false,
  //  bool bInstanceDefinitionObject = false,
  //  bool bAddToDoc = true
  //  );

    /// <summary>
    /// Modify instance definition name and description.
    /// Does not change instance defintion uuid or geometry.
    /// </summary>
    /// <param name="idef"></param>
    /// <param name="newName"></param>
    /// <param name="newDescription"></param>
    /// <param name="quiet">
    /// if true, information message boxes pop up when illegal changes are attempted.
    /// </param>
    /// <returns>
    /// true if successful
    /// </returns>
    public bool Modify(DocObjects.InstanceDefinition idef, string newName, string newDescription, bool quiet)
    {
      if (null == idef)
        return false;
      return UnsafeNativeMethods.CRhinoInstanceDefinitionTable_ModifyInstanceDefinition(m_doc.m_docId, idef.Index, newName, newDescription, quiet);
    }


  //Description:
  //  If the instance definition has been modified and the modifcation
  //  can be undone, then UndoModifyInstanceDefinition() will restore
  //  the instance definition to its previous state.
  //Returns:
  //  TRUE if this layer had been modified and the modifications were
  //  undone.
  //bool UndoModifyInstanceDefinition(
  //   int idef_index,
  //   unsigned int ur_sn = 0
  //   );

  //Description:
  //  Modify instance definition geometry and replace all references
  //  to the current definition with references to the new definition.
  //Parameters:
  //  idef_index - [in] zero based index of the instance definition
  //       to change.
  //  new_objects - [in] new geometry for the instance definition.
  //  bQuiet - [in] if TRUE, information message boxes pop up
  //      when illegal changes are attempted.
  //Returns:
  //  index of the new definition or -1 if it fails
  //See Also:
  //  CRhinoInstanceDefinitionTable::ModifyInstanceDefinitionGeometry
  //*/
  //bool ModifyInstanceDefinitionGeometry( 
  //   int idef_index,
  //   const ON_SimpleArray<const CRhinoObject*>& new_objects,
  //   bool bQuiet = false
  //   );


    /// <summary>
    /// Deletes instance definition
    /// </summary>
    /// <param name="idefIndex">
    /// zero based index of instance definition to delete.
    /// This must be in the range 0 &lt;= idefIndex &lt; InstanceDefinitionTable.Count
    /// </param>
    /// <param name="deleteReferences">
    /// true to delete all references to this definition.
    /// false to delete definition only if there are no references.
    /// </param>
    /// <param name="quiet">
    /// If true, no warning message box appears if an instance definition cannot be
    /// deleted because it is the current layer or it contains active geometry.
    /// </param>
    /// <returns>
    /// true if successful. false if the instance definition has active references and bDeleteReferences is false.
    /// </returns>
    public bool Delete(int idefIndex, bool deleteReferences, bool quiet)
    {
      return UnsafeNativeMethods.CRhinoInstanceDefinitionTable_DeleteInstanceDefinition(m_doc.m_docId, idefIndex, deleteReferences, quiet);
    }

  //Description:
  //  Purges an instance definition and its definition geometry.
  //Parameters:
  //  idef_index - [in] zero based index of instance definition to delete.
  //      This must be in the range 
  //      0 <= idefIndex < InstanceDefinitionTable.Count
  //Returns:
  //  True if successful. False if the instance definition cannot be purged
  //  because it is in use by reference objects or undo information.
  //bool PurgeInstanceDefinition( int idef_index );

  //Description:
  //  Purge deleted instance definition information that is not
  //  in use.  This function is time consuming and should be used
  //  in a thoughtful manner.    
  //Parameters:
  //  bIgnoreUndoReferences:
  //    If false, then deleted instance definition information
  //    that could possibly be undeleted by the Undo command
  //    will not be deleted.
  //    If true, then all deleted instance definition information
  //    is deleted.
  //void Compact( bool bIgnoreUndoReferences );

  //Description:
  //  Undeletes an instance definition that has been deleted by DeleteLayer().
  //Parameters:
  //  idef_index - [in] zero based index of an instance definition
  //      to undelete. This must be in the range
  //      0 <= idefIndex < InstanceDefinitionTable.Count
  //Returns:
  //  TRUE if successful.
  //bool UndeleteInstanceDefinition( int idef_index );

  //Description:
  //  Read the objects from a file and use them as the instance's
  //  definition geometry.
  //Parameters:
  //  idef_index - [in]
  //    instance definition index
  //  filename - [in]
  //    name of file (can be any type of file that Rhino or a plug-in can read).
  //  bUpdateNestedLinks - [in]
  //    If true and the instance definition referes to a linked instance definition,
  //    that needs to be updated, then the nested defition is also updated.
  //    If false, nested updates are skipped.
  //Returns:
  //  True if successful.
  //bool UpdateLinkedInstanceDefinition(
  //        int idef_index,
  //        const wchar_t* filename,
  //        bool bUpdateNestedLinks
  //        );

  //[skipping]
  //void Sort( int (*compare)(const CRhinoInstanceDefinition*,const CRhinoInstanceDefinition*,void*),

  //Description:
  //  Gets an array of pointers to layers that is sorted by
  //  the values of CRhinoInstanceDefinition::m_sort_index.
  //Parameters:
  //  sorted_list - [out] this array is filled in with
  //      CRhinoInstanceDefinition pointers sorted by
  //      the values of CRhinoInstanceDefinition::m_sort_index.
  //  bIgnoreDeleted - [in] if TRUE then deleted layers are filtered out.
  //Remarks:
  //  Use Sort() to set the values of m_sort_index.
  //void GetSortedList(
  //  ON_SimpleArray<const CRhinoInstanceDefinition*>& sorted_list,
  //  bool bIgnoreDeleted = false
  //  ) const;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ignoreDeleted">if true then deleted layers are filtered out</param>
    /// <returns></returns>
    public DocObjects.InstanceDefinition[] GetList(bool ignoreDeleted)
    {
      Runtime.INTERNAL_IntArray arr = new Runtime.INTERNAL_IntArray();
      IntPtr ptr = arr.m_ptr;
      int count = UnsafeNativeMethods.CRhinoInstanceDefinitionTable_GetList(m_doc.m_docId, ptr, ignoreDeleted);
      DocObjects.InstanceDefinition[] rc = null;
      if( count>0 )
      {
        int[] indices = arr.ToArray();
        if (indices!=null && indices.Length > 0)
        {
          count = indices.Length;
          rc = new Rhino.DocObjects.InstanceDefinition[count];
          for (int i = 0; i < count; i++)
          {
            rc[i] = new Rhino.DocObjects.InstanceDefinition(indices[i], m_doc);
          }
        }
      }
      arr.Dispose();
      return rc;
    }

  //Description:
  //  Gets unsed instance definition name used as default when
  //  creating new instance definitions.
  //Parameters:
  //  result - [out] this is the wString which receives new name
  //void GetUnusedInstanceDefinitionName( ON_wString& result) const;

  //// OBSOLETE - USE ModifyInstanceDefinition
  //bool SetURL( int index, const wchar_t* url );
  //bool SetName( int index, const wchar_t* name );
  //bool SetDescription( int index, const wchar_t* description );

  //[skipping]
  //void SetRemapIndex( int idef_index, int remap_index );
  }
}

namespace Rhino
{
  // this might be best as a nested class
  //public class InstanceObjectPiece { }
}
