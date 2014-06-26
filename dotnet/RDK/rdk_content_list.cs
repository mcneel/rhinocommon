#pragma warning disable 1591
using System;
using System.Collections;
using System.Collections.Generic;

#if RDK_CHECKED

namespace Rhino.Render
{
  public sealed class RenderMaterialTable : IEnumerable<RenderMaterial>, Collections.IRhinoTable<RenderMaterial>
  {
    internal RenderMaterialTable(RhinoDoc rhinoDoc)
    {
      m_rhino_doc = rhinoDoc;
    }

    private readonly RhinoDoc m_rhino_doc;
    public RhinoDoc Document { get { return m_rhino_doc; } }

    private ContentList<RenderMaterial> m_internal_content_list;
    private ContentList<RenderMaterial> InternalContentList
    {
      get { return (m_internal_content_list ?? (m_internal_content_list = new ContentList<RenderMaterial>(RenderContentKind.Material, Document))); }
    }

    #region IEnumerable required
    public IEnumerator<RenderMaterial> GetEnumerator()
    {
      var document = Document;
      return new ContentList<RenderMaterial>(RenderContentKind.Material, document);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion IEnumerable required

    #region IRhinoTable required

    public int Count { get { return InternalContentList.Count; } }

    public RenderMaterial this[int index] { get { return InternalContentList[index]; } }

    #endregion IRhinoTable required
  }

  public sealed class RenderEnvironmentTable : IEnumerable<RenderEnvironment>, Collections.IRhinoTable<RenderEnvironment>
  {
    internal RenderEnvironmentTable(RhinoDoc rhinoDoc)
    {
      //RhinoDoc.LayerTableEvent 
      m_rhino_doc = rhinoDoc;
    }

    private readonly RhinoDoc m_rhino_doc;
    public RhinoDoc Document { get { return m_rhino_doc; } }

    private ContentList<RenderEnvironment> m_internal_content_list;
    private ContentList<RenderEnvironment> InternalContentList
    {
      get { return (m_internal_content_list ?? (m_internal_content_list = new ContentList<RenderEnvironment>(RenderContentKind.Environment, Document))); }
    }

    #region IEnumerable required
    public IEnumerator<RenderEnvironment> GetEnumerator()
    {
      var document = Document;
      return new ContentList<RenderEnvironment>(RenderContentKind.Environment, document);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion IEnumerable required

    #region IRhinoTable required

    public int Count { get { return InternalContentList.Count; } }

    public RenderEnvironment this[int index] { get { return InternalContentList[index]; } }

    #endregion IRhinoTable required
  }

  public sealed class RenderTextureTable : IEnumerable<RenderTexture>, Collections.IRhinoTable<RenderTexture>
  {
    internal RenderTextureTable(RhinoDoc rhinoDoc)
    {
      //RhinoDoc.LayerTableEvent 
      m_rhino_doc = rhinoDoc;
    }

    private readonly RhinoDoc m_rhino_doc;
    public RhinoDoc Document { get { return m_rhino_doc; } }

    private ContentList<RenderTexture> m_internal_content_list;
    private ContentList<RenderTexture> InternalContentList
    {
      get { return (m_internal_content_list ?? (m_internal_content_list = new ContentList<RenderTexture>(RenderContentKind.Texture, Document))); }
    }

    #region IEnumerable required
    public IEnumerator<RenderTexture> GetEnumerator()
    {
      var document = Document;
      return new ContentList<RenderTexture>(RenderContentKind.Texture, document);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion IEnumerable required

    #region IRhinoTable required

    public int Count { get { return InternalContentList.Count; } }

    public RenderTexture this[int index] { get { return InternalContentList[index]; } }

    #endregion IRhinoTable required
  }

  internal class RenderContentTable
  {
    internal RenderContentTable(RhinoDoc rhinoDoc, RenderContentKind kind)
    {
      //RhinoDoc.LayerTableEvent 
      m_rhino_doc = rhinoDoc;
      m_render_content_kind = kind;
    }
    
    public RhinoDoc Document { get { return m_rhino_doc; } }
    public RenderContentKind ContentKind{ get { return m_render_content_kind; } }

    private readonly RhinoDoc m_rhino_doc;    
    private readonly RenderContentKind m_render_content_kind;

    #region events

    internal delegate void ContentListClearingCallback(int kind, int docId);
    internal delegate void ContentListClearedCallback(int kind, int docId);
    internal delegate void ContentListLoadedCallback(int kind, int docId);
    internal delegate void ContentCustomEventCallback(Guid eventId, IntPtr contextPtr);

    public class RenderContentTableEventArgs : EventArgs
    {
      internal RenderContentTableEventArgs(RhinoDoc document, RenderContentKind kind)
      {
        m_rhino_doc = document;
        m_content_kind = kind;
      }

      public RhinoDoc Document { get { return m_rhino_doc; } }
      public RenderContentKind ContentKind { get { return m_content_kind; } }

      private readonly RhinoDoc m_rhino_doc;
      private readonly RenderContentKind m_content_kind;
    }

    //private static ContentListClearingCallback g_on_content_list_clearing;
    //private static void OnContentListClearing(int kind, int docId)
    //{
    //  if (ContentListClearingEvent != null)
    //  {
    //    try { ContentListClearingEvent(null, new ContentListEventArgs((RenderContentKind)kind)); }
    //    catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
    //  }
    //}
    //internal static EventHandler<ContentListEventArgs> ContentListClearingEvent;
    ///// <summary>
    ///// Used to monitor content lists being cleared.
    ///// </summary>
    //public static event EventHandler<ContentListEventArgs> ContentListClearing
    //{
    //  add
    //  {
    //    if (ContentListClearingEvent == null)
    //    {
    //      g_on_content_list_clearing = OnContentListClearing;
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(g_on_content_list_clearing, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //    }
    //    ContentListClearingEvent += value;
    //  }
    //  remove
    //  {
    //    ContentListClearingEvent -= value;
    //    if (ContentListClearingEvent == null)
    //    {
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearingEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //      g_on_content_list_clearing = null;
    //    }
    //  }
    //}

    //private static ContentListClearedCallback m_OnContentListCleared;
    //private static void OnContentListCleared(int kind, int docId)
    //{
    //  if (m_content_list_cleared_event != null)
    //  {
    //    try { m_content_list_cleared_event(null, new ContentListEventArgs((RenderContentKind)kind, RhinoDoc.FromId(docId))); }
    //    catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
    //  }
    //}
    //internal static EventHandler<ContentListEventArgs> m_content_list_cleared_event;

    //private static ContentListLoadedCallback m_OnContentListLoaded;
    //private static void OnContentListLoaded(int kind, int docId)
    //{
    //  if (m_content_list_loaded_event != null)
    //  {
    //    try { m_content_list_loaded_event(null, new ContentListEventArgs((RenderContentKind)kind, RhinoDoc.FromId(docId))); }
    //    catch (Exception ex) { Runtime.HostUtils.ExceptionReport(ex); }
    //  }
    //}
    //internal static EventHandler<ContentListEventArgs> m_content_list_loaded_event;

    ///// <summary>
    ///// Used to monitor content lists being cleared.
    ///// </summary>
    //public static event EventHandler<ContentListEventArgs> ContentListCleared
    //{
    //  add
    //  {
    //    if (m_content_list_cleared_event == null)
    //    {
    //      m_OnContentListCleared = OnContentListCleared;
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(m_OnContentListCleared, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //    }
    //    m_content_list_cleared_event += value;
    //  }
    //  remove
    //  {
    //    m_content_list_cleared_event -= value;
    //    if (m_content_list_cleared_event == null)
    //    {
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListClearedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //      m_OnContentListCleared = null;
    //    }
    //  }
    //}

    ///// <summary>
    ///// Used to monitor content lists being loading.
    ///// </summary>
    //public static event EventHandler<ContentListEventArgs> ContentListLoading
    //{
    //  add
    //  {
    //    if (m_content_list_loaded_event == null)
    //    {
    //      m_OnContentListLoaded = OnContentListLoaded;
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(m_OnContentListLoaded, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //    }
    //    m_content_list_loaded_event += value;
    //  }
    //  remove
    //  {
    //    m_content_list_loaded_event -= value;
    //    if (m_content_list_loaded_event == null)
    //    {
    //      UnsafeNativeMethods.CRdkCmnEventWatcher_SetContentListLoadedEventCallback(null, Rhino.Runtime.HostUtils.m_rdk_ew_report);
    //      m_OnContentListLoaded = null;
    //    }
    //  }
    //}

    #endregion
  }

  /// <summary>
  /// Base class that provides access to the document lists of RenderContent instances
  /// ie - the Material, Environment and Texture tables.
  /// </summary>
  class ContentList<TContentType> : IEnumerator<TContentType>, IEnumerable<TContentType> where TContentType : RenderContent
  {
    private readonly RenderContentKind m_kind;
    private readonly RhinoDoc m_doc;
    internal ContentList(RenderContentKind kind, RhinoDoc doc)
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

    public TContentType this[int index]
    {
      get
      {
        var i = 0;
        foreach (var content in this)
        {
          if (i == index) return content;
          i++;
        }
        return null;
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
    public TContentType FindInstance(Guid instanceId, bool includeChildren)
    {
      var content_pointer = UnsafeNativeMethods.Rdk_ContentList_FindInstance(ConstPointer(), instanceId, includeChildren);
      if (IntPtr.Zero == content_pointer)
        return null;
      var content = RenderContent.FromPointer(content_pointer);
      return content as TContentType;
    }

    #region IEnumerator implemenation
    private IntPtr m_pIterator = IntPtr.Zero;
    private RenderContent m_content;
    public void Reset()
    {
        UnsafeNativeMethods.Rdk_ContentLists_DeleteIterator(m_pIterator);
        m_pIterator = UnsafeNativeMethods.Rdk_ContentLists_NewIterator(ConstPointer());
    }
    public TContentType Current
    {
      get
      {
        return m_content as TContentType;
      }
    }
    object IEnumerator.Current { get { return Current; } }
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

    public IEnumerator<TContentType> GetEnumerator()
    {
      return this;
    }

    ~ContentList()
    {
      Dispose(false);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
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

