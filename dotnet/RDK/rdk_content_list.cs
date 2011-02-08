using System;
using System.Diagnostics;
using System.Collections;

#if USING_RDK

namespace Rhino.Render
{
  /// <summary>
  /// Base class that provides access to the document lists of RenderContent instances
  /// ie - the Material, Environment and Texture tables.
  /// </summary>
  public class ContentList : IEnumerator, IDisposable
  {
    private RenderContent.Kinds m_kind;
    private RhinoDoc m_doc;
    internal ContentList(RenderContent.Kinds kind, RhinoDoc doc)
    {
      m_kind = kind;
      m_doc = doc;
    }
    /// <summary>
    /// The number of top level content objects in this list
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.Rdk_ContentList_Count(ConstPointer());
      }
    }

    /// <summary>
    /// The unique identifier for this list
    /// </summary>
    public Guid Id
    {
      get
      {
        return UnsafeNativeMethods.Rdk_ContentList_Uuid(ConstPointer());
      }
    }

    /// <summary>
    /// Find a content by its instance id.
    /// </summary>
    /// <param name="instanceId">Instance id of the content to find</param>
    /// <param name="includeChildren">Specifies if children should be searched as well as top-level content.</param>
    /// <returns>The found content or null</returns>
    public RenderContent FindInstance(Guid instanceId, bool includeChildren)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_ContentList_FindInstance(ConstPointer(), instanceId, includeChildren);
      return pContent!=IntPtr.Zero ? RenderContent.FromPointer(pContent) : null;
    }

    #region IEnumerator implemenation
    private IntPtr m_pIterator = IntPtr.Zero;
    private RenderContent m_content = null;
    public void Reset()
    {
        UnsafeNativeMethods.Rdk_ContentLists_DeleteIterator(m_pIterator);
        m_pIterator = UnsafeNativeMethods.Rdk_ContentLists_NewIterator(ConstPointer());
    }
    public object Current
    {
      get
      {
        return m_content;
      }
    }
    public bool MoveNext()
    {
      if (m_pIterator == IntPtr.Zero)
      {
        m_pIterator = UnsafeNativeMethods.Rdk_ContentLists_NewIterator(ConstPointer());
      }

      IntPtr pContent = UnsafeNativeMethods.Rdk_ContentLists_Next(m_pIterator);
      if (IntPtr.Zero == pContent)
      {
        m_content = null;
        return false;
      }
      m_content = RenderContent.FromPointer(pContent);
      return true;
    }
    #endregion

    #region internals
    internal IntPtr ConstPointer()
    {
      return UnsafeNativeMethods.Rdk_ContentList_ListFromKind(RenderContent.KindString(m_kind), m_doc.m_docId);
    }
    #endregion

    #region IDisposable
    ~ContentList()
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
      UnsafeNativeMethods.Rdk_ContentLists_DeleteIterator(m_pIterator);
    }
    #endregion

  }
}

#endif

