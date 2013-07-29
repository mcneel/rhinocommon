using System;

//don't make serializable yet.

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the geometry in a block definition.
  /// </summary>
  public class InstanceDefinitionGeometry : GeometryBase
  {
    Guid m_file3dm_id;
    #region internals
    internal InstanceDefinitionGeometry(IntPtr nativePtr, object parent)
      : base(nativePtr, parent, -1)
    { }

    internal InstanceDefinitionGeometry(Guid id, Rhino.FileIO.File3dm parent)
      : base(IntPtr.Zero, parent, -1)
    {
      m_file3dm_id = id;
    }

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceDefinitionGeometry(IntPtr.Zero, null);
    }
    #endregion

    /// <summary>
    /// Initializes a new block definition.
    /// </summary>
    public InstanceDefinitionGeometry()
    {
      IntPtr ptr = UnsafeNativeMethods.ON_InstanceDefinition_New(IntPtr.Zero);
      ConstructNonConstObject(ptr);
    }

    const int idxName = 0;
    const int idxDescription = 1;

    /// <summary>
    /// Gets or sets the name of the definition.
    /// </summary>
    public string Name
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, idxName, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, idxName, value);
      }
    }

    /// <summary>
    /// Gets or sets the description of the definition.
    /// </summary>
    public string Description
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, idxDescription, pString);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, idxDescription, value);
      }
    }

    /// <summary>
    /// unique id for this instance definition
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.ON_InstanceDefinition_GetId(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetId(pThis, value);
      }
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs ide = m__parent as Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs;
      if (ide != null)
        return ide.ConstLightPointer();
#endif
      Rhino.FileIO.File3dm parent_file = m__parent as Rhino.FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr pModel = parent_file.NonConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetInstanceDefinitionPointer(pModel, m_file3dm_id);
      }
      return base._InternalGetConstPointer();
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is Rhino.FileIO.File3dm)
        return _InternalGetConstPointer();

      return base.NonConstPointer();
    }

    /// <summary>
    /// list of object ids in the instance geometry table
    /// </summary>
    /// <returns></returns>
    public Guid[] GetObjectIds()
    {
      using (Runtime.InteropWrappers.SimpleArrayGuid ids = new Runtime.InteropWrappers.SimpleArrayGuid())
      {
        IntPtr pConstThis = ConstPointer();
        IntPtr pIds = ids.NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_GetObjectIds(pConstThis, pIds);
        return ids.ToArray();
      }
    }
  }

  /// <summary>
  /// Represents a reference to the geometry in a block definition.
  /// </summary>
  public class InstanceReferenceGeometry : GeometryBase
  {
    /// <summary>
    /// Constructor used when creating nested instance references.
    /// </summary>
    /// <param name="instanceDefinitionId"></param>
    /// <param name="transform"></param>
    /// <example>
    /// <code source='examples\cs\ex_nestedblock.cs' lang='cs'/>
    /// </example>
    public InstanceReferenceGeometry(Guid instanceDefinitionId, Transform transform)
    {
      IntPtr ptr = UnsafeNativeMethods.ON_InstanceRef_New(instanceDefinitionId, ref transform);
      ConstructNonConstObject(ptr);
    }

    internal InstanceReferenceGeometry(IntPtr native_ptr, object parent)
      : base(native_ptr, parent, -1)
    { }

		/// <summary>
		/// The unique id for the parent instance definition of this instance reference.
		/// </summary>
		public Guid ParentIdefId
		{
			get
			{
				IntPtr ptr_const_this = ConstPointer();
				return UnsafeNativeMethods.ON_InstanceRef_IDefId (ptr_const_this);
			}
		}  

		/// <summary>
		/// Transformation for this reference.
		/// </summary>
		public Rhino.Geometry.Transform Xform
		{
			get
			{
				IntPtr ptr_const_this = ConstPointer();
				Transform rc = new Transform();
				UnsafeNativeMethods.ON_InstanceRef_GetTransform (ptr_const_this, ref rc);
				return rc;
			}
		}

    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceReferenceGeometry(IntPtr.Zero, null);
    }
  }
}
