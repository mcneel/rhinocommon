#pragma warning disable 1591
using System;
using System.Diagnostics;
using System.Collections;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  /// <summary>
  /// A collection of all of the available render content types registered with Rhino.
  /// Gets this object from Rhino.Render.Utilities.RenderContentTypes.
  /// </summary>
  public class RenderContentTypes : IEnumerator
  {
    private int _index = -1;

    #region IEnumerator implemenation
    public void Reset()
    {
        _index = -1;
    }
    public object Current
    {
      get
      {
        Guid typeId = UnsafeNativeMethods.Rdk_Factories_GetTypeIdAtIndex(_index);
        if (typeId != Guid.Empty)
        {
          return new RenderContentType(typeId);
        }
        return null;
      }
    }
    public bool MoveNext()
    {
      return Guid.Empty != UnsafeNativeMethods.Rdk_Factories_GetTypeIdAtIndex(++_index);
    }
    #endregion

    public static RenderContentType GetTypeInformation(Guid typeId)
    {
      return new RenderContentType(typeId);
    }
  }

  /// <summary>
  /// Represents one of the render content types registered with Rhino.  Use the RenderContentTypes class
  /// to access these objects.
  /// </summary>
  public class RenderContentType : IDisposable, IEnumerable
  {
    internal RenderContentType(Guid typeId)
    {
      m_typeId = typeId;
    }

    public IEnumerator GetEnumerator()
    {
      return new RenderContentTypes();
    }

    #region properties
    /// <summary>
    /// The kind of content that this type represents.
    /// </summary>
    /// <returns></returns>
	  public RenderContent.Kinds Kind
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_Factory_Kind(ConstPointer(), pString);
          return RenderContent.KindFromString(sh.ToString());
        }
      }
    }

    /// <summary>
    /// Determines if this type is of the specified kind.
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
	  public bool IsKind(RenderContent.Kinds kind) { return Kind == kind; }

    /// <summary>
    /// Returns a new instance of the render content of this type.  This content can be added to a persistant list.
    /// </summary>
    /// <returns></returns>
    public RenderContent NewRenderContent()
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_Factory_NewContent(ConstPointer());
      if (pContent != IntPtr.Zero)
      {
        RenderContent content = RenderContent.FromPointer(pContent);
        content.AutoDelete = true;
        return content;
      }
      return null;
    }

    /// <summary>
    /// Returns the type identifier associated with this type.
    /// </summary>
    public Guid Id
    {
      get
      {
        return UnsafeNativeMethods.Rdk_Factory_TypeId(ConstPointer());
      }
    }

    /// <summary>
    /// Returns the internal name identifier associated with this type.
    /// </summary>
    public String InternalName
    {
      get
      {
        using (Rhino.Runtime.StringHolder sh = new Rhino.Runtime.StringHolder())
        {
          IntPtr pString = sh.NonConstPointer();
          UnsafeNativeMethods.Rdk_Factory_InternalName(ConstPointer(), pString);
          return sh.ToString();
        }
      }
    }

    public Guid RenderEngineId
    {
      get
      {
        return UnsafeNativeMethods.Rdk_Factory_RenderEngineId(ConstPointer());
      }
    }

    public Guid PlugInId
    {
      get
      {
        return UnsafeNativeMethods.Rdk_Factory_PlugInId(ConstPointer());
      }
    }
    #endregion

    #region internals
    private Guid m_typeId;
    internal IntPtr ConstPointer()
    {
      return UnsafeNativeMethods.Rdk_Factories_ConstPointerFromTypeId(m_typeId);
    }

    ~RenderContentType()
    {
      Dispose(false);
    }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool isDisposing)
    {
    }
    #endregion

  }
}


#endif