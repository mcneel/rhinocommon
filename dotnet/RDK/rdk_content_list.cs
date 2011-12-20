#pragma warning disable 1591
using System;
using System.Diagnostics;
using System.Collections.Generic;

#if RDK_UNCHECKED

namespace Rhino.Render
{
  /// <summary>
  /// Base class that provides access to the document lists of RenderContent instances
  /// ie - the Material, Environment and Texture tables.
  /// </summary>
  public class ContentList : IEnumerator<RenderContent>, IDisposable
  {
    private RenderContent.Kinds m_kind;
    private RhinoDoc m_doc;
    internal ContentList(RenderContent.Kinds kind, RhinoDoc doc)
    {
      m_kind = kind;
      m_doc = doc;
    }
    /// <summary>
    /// The number of top level content objects in this list.
    /// </summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.Rdk_ContentList_Count(ConstPointer());
      }
    }

    /// <summary>
    /// The unique identifier for this list.
    /// </summary>
    public Guid Id
    {
      get
      {
        return UnsafeNativeMethods.Rdk_ContentList_Uuid(ConstPointer());
      }
    }

    /// <summary>
    /// Finds a content by its instance id.
    /// </summary>
    /// <param name="instanceId">Instance id of the content to find.</param>
    /// <param name="includeChildren">Specifies if children should be searched as well as top-level content.</param>
    /// <returns>The found content or null.</returns>
    public RenderContent FindInstance(Guid instanceId, bool includeChildren)
    {
      IntPtr pContent = UnsafeNativeMethods.Rdk_ContentList_FindInstance(ConstPointer(), instanceId, includeChildren);
      return pContent!=IntPtr.Zero ? RenderContent.FromPointer(pContent) : null;
    }

    #region events

    public class ContentListEventArgs : EventArgs
    {
      readonly ContentList m_content_list;
      internal ContentListEventArgs(RenderContent.Kinds kind, RhinoDoc doc) { m_content_list = new Rhino.Render.ContentList(kind, doc); }
      public ContentList ContentList { get { return m_content_list; } }
    }

    internal delegate void ContentListClearingCallback(int kind, int docId);
    internal delegate void ContentListClearedCallback(int kind, int docId);
    internal delegate void ContentListLoadedCallback(int kind, int docId);

    private static ContentListClearingCallback m_OnContentListClearing;
    private static void OnContentListClearing(int kind, int docId)
    {
      if (m_content_list_clearing_event != null)
      {
        try { m_content_list_clearing_event(null, new ContentListEventArgs((RenderContent.Kinds)kind, RhinoDoc.FromId(docId))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler<ContentListEventArgs> m_content_list_clearing_event;

    private static ContentListClearedCallback m_OnContentListCleared;
    private static void OnContentListCleared(int kind, int docId)
    {
      if (m_content_list_cleared_event != null)
      {
        try { m_content_list_cleared_event(null, new ContentListEventArgs((RenderContent.Kinds)kind, RhinoDoc.FromId(docId))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler<ContentListEventArgs> m_content_list_cleared_event;

    private static ContentListLoadedCallback m_OnContentListLoaded;
    private static void OnContentListLoaded(int kind, int docId)
    {
      if (m_content_list_loaded_event != null)
      {
        try { m_content_list_loaded_event(null, new ContentListEventArgs((RenderContent.Kinds)kind, RhinoDoc.FromId(docId))); }
        catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
      }
    }
    internal static EventHandler<ContentListEventArgs> m_content_list_loaded_event;

    /// <summary>
    /// Used to monitor content lists being cleared.
    /// </summary>
    public static event EventHandler<ContentListEventArgs> ContentListClearing
    {
      add
      {
        if (m_content_list_clearing_event == null)
        {
          m_OnContentListClearing = OnContentListClearing;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(m_OnContentListClearing, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_list_clearing_event += value;
      }
      remove
      {
        m_content_list_clearing_event -= value;
        if (m_content_list_clearing_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentListClearing = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor content lists being cleared.
    /// </summary>
    public static event EventHandler<ContentListEventArgs> ContentListCleared
    {
      add
      {
        if (m_content_list_cleared_event == null)
        {
          m_OnContentListCleared = OnContentListCleared;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(m_OnContentListCleared, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_list_cleared_event += value;
      }
      remove
      {
        m_content_list_cleared_event -= value;
        if (m_content_list_cleared_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentListCleared = null;
        }
      }
    }

    /// <summary>
    /// Used to monitor content lists being loading.
    /// </summary>
    public static event EventHandler<ContentListEventArgs> ContentListLoading
    {
      add
      {
        if (m_content_list_loaded_event == null)
        {
          m_OnContentListLoaded = OnContentListLoaded;
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(m_OnContentListLoaded, Rhino.Runtime.HostUtils.m_rdk_ew_report);
        }
        m_content_list_loaded_event += value;
      }
      remove
      {
        m_content_list_loaded_event -= value;
        if (m_content_list_loaded_event == null)
        {
          UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
          m_OnContentListLoaded = null;
        }
      }
    }

    #endregion

    #region IEnumerator implemenation
    private IntPtr m_pIterator = IntPtr.Zero;
    private RenderContent m_content = null;
    public void Reset()
    {
        UnsafeNativeMethods.Rdk_ContentLists_DeleteIterator(m_pIterator);
        m_pIterator = UnsafeNativeMethods.Rdk_ContentLists_NewIterator(ConstPointer());
    }
    public RenderContent Current
    {
      get
      {
        return m_content;
      }
    }
    object System.Collections.IEnumerator.Current { get { return Current; } }
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

