using System;

//don't make serializable yet.

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the geometry in a block definition.
  /// </summary>
  public class InstanceDefinitionGeometry : GeometryBase
  {
    #region internals
    internal InstanceDefinitionGeometry(IntPtr nativePtr, object parent)
      : base(nativePtr, parent, -1)
    { }

    internal override IntPtr _InternalGetConstPointer()
    {
      Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs ide = m__parent as Rhino.DocObjects.Tables.InstanceDefinitionTableEventArgs;
      if (ide != null)
        return ide.ConstLightPointer();
      return base._InternalGetConstPointer();
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


    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceReferenceGeometry(IntPtr.Zero, null);
    }
  }
}
