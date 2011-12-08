using System;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

//don't make serializable yet.

namespace Rhino.Geometry
{
  /// <summary>
  /// Represents the geometry in a block definition.
  /// </summary>
  public class InstanceDefinitionGeometry : GeometryBase
  {
    #region internals
    internal InstanceDefinitionGeometry(IntPtr native_ptr, object parent)
      : base(native_ptr, parent, -1)
    { }

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
    internal InstanceReferenceGeometry(IntPtr native_ptr, object parent)
      : base(native_ptr, parent, -1)
    { }


    internal override GeometryBase DuplicateShallowHelper()
    {
      return new InstanceReferenceGeometry(IntPtr.Zero, null);
    }
  }
}
