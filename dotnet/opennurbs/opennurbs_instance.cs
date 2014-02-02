using System;
using Rhino.Runtime.InteropWrappers;

//don't make serializable yet.

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the geometry in a block definition.
  /// </summary>
  public class InstanceDefinitionGeometry : GeometryBase
  {
    readonly Guid m_file3dm_id;
    #region internals
    internal InstanceDefinitionGeometry(IntPtr nativePtr, object parent)
      : base(nativePtr, parent, -1)
    { }

    internal InstanceDefinitionGeometry(Guid id, FileIO.File3dm parent)
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

    const int IDX_NAME = 0;
    const int IDX_DESCRIPTION = 1;

    /// <summary>
    /// Gets or sets the name of the definition.
    /// </summary>
    public string Name
    {
      get
      {
        IntPtr ptr = ConstPointer();
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, IDX_NAME, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, IDX_NAME, value);
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
        using (var sh = new StringHolder())
        {
          IntPtr ptr_string = sh.NonConstPointer();
          UnsafeNativeMethods.ON_InstanceDefinition_GetString(ptr, IDX_DESCRIPTION, ptr_string);
          return sh.ToString();
        }
      }
      set
      {
        IntPtr ptr = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetString(ptr, IDX_DESCRIPTION, value);
      }
    }

    /// <summary>
    /// unique id for this instance definition
    /// </summary>
    public Guid Id
    {
      get
      {
        IntPtr ptr_const_this = ConstPointer();
        return UnsafeNativeMethods.ON_InstanceDefinition_GetId(ptr_const_this);
      }
      set
      {
        IntPtr ptr_this = NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_SetId(ptr_this, value);
      }
    }

    internal override IntPtr _InternalGetConstPointer()
    {
#if RHINO_SDK
      DocObjects.Tables.InstanceDefinitionTableEventArgs ide = m__parent as DocObjects.Tables.InstanceDefinitionTableEventArgs;
      if (ide != null)
        return ide.ConstLightPointer();
#endif
      FileIO.File3dm parent_file = m__parent as FileIO.File3dm;
      if (parent_file != null)
      {
        IntPtr ptr_model = parent_file.NonConstPointer();
        return UnsafeNativeMethods.ONX_Model_GetInstanceDefinitionPointer(ptr_model, m_file3dm_id);
      }
      return base._InternalGetConstPointer();
    }

    internal override IntPtr NonConstPointer()
    {
      if (m__parent is FileIO.File3dm)
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
        IntPtr ptr_const_this = ConstPointer();
        IntPtr ptr_id_array = ids.NonConstPointer();
        UnsafeNativeMethods.ON_InstanceDefinition_GetObjectIds(ptr_const_this, ptr_id_array);
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

    internal InstanceReferenceGeometry(IntPtr nativePointer, object parent)
      : base(nativePointer, parent, -1)
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

    /// <summary>Transformation for this reference.</summary>
    public Transform Xform
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
