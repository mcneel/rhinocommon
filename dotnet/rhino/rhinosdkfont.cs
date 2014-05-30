#pragma warning disable 1591
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Rhino.Runtime.InteropWrappers;

#if RHINO_SDK
namespace Rhino.DocObjects
{
  public class Font
  {
    public static string[] AvailableFontFaceNames()
    {
      IntPtr pStringArray = UnsafeNativeMethods.ON_StringArray_New();
      int count = UnsafeNativeMethods.CRhinoFontTable_GetFontNames(pStringArray);
      string[] rc = new string[count];
      using (var sh = new StringHolder())
      {
        IntPtr pStringHolder = sh.NonConstPointer();
        for( int i=0; i<count; i++ )
        {
          UnsafeNativeMethods.ON_StringArray_Get(pStringArray, i, pStringHolder);
          rc[i] = sh.ToString();
        }
      }
      UnsafeNativeMethods.ON_StringArray_Delete(pStringArray);
      if (count < 1)
      {
        Rhino.Drawing.Text.InstalledFontCollection fonts = new Rhino.Drawing.Text.InstalledFontCollection();
        rc = new string[fonts.Families.Length];
        for (int i = 0; i < fonts.Families.Length; i++)
          rc[i] = fonts.Families[i].Name;
      }
      Array.Sort(rc);
      return rc;
    }

    private readonly int m_index;
    private readonly RhinoDoc m_doc;

    internal Font(int index, RhinoDoc doc)
    {
      m_index = index;
      m_doc = doc;
    }

    public string FaceName
    {
      get
      {
        IntPtr pName = UnsafeNativeMethods.CRhinoFont_FaceName(m_doc.m_docId, m_index);
        if (IntPtr.Zero == pName)
          return null;
        return Marshal.PtrToStringUni(pName);
      }
    }

    const int idxBold = 0;
    const int idxItalic = 1;

    public bool Bold
    {
      get
      {
        return UnsafeNativeMethods.CRhinoFont_GetBool(m_doc.m_docId, m_index, idxBold);
      }
    }

    public bool Italic
    {
      get
      {
        return UnsafeNativeMethods.CRhinoFont_GetBool(m_doc.m_docId, m_index, idxItalic);
      }
    }
  }
}

namespace Rhino.DocObjects.Tables
{
  /// <summary>
  /// Font tables store the list of fonts in a Rhino document.
  /// </summary>
  public sealed class FontTable : IEnumerable<Font>, Rhino.Collections.IRhinoTable<Font>
  {
    private readonly RhinoDoc m_doc;
    internal FontTable(RhinoDoc doc)
    {
      m_doc = doc;
    }

    /// <summary>Document that owns this table.</summary>
    public RhinoDoc Document
    {
      get { return m_doc; }
    }

    /// <summary>Number of fonts in the table.</summary>
    public int Count
    {
      get
      {
        return UnsafeNativeMethods.CRhinoFontTable_FontCount(m_doc.m_docId);
      }
    }

    /// <summary>
    /// At all times, there is a "current" font.  Unless otherwise specified,
    /// new dimension objects are assigned to the current font. The current
    /// font is never deleted.
    /// Returns: Zero based font index of the current font.
    /// </summary>
    public int CurrentIndex
    {
      get
      {
        return UnsafeNativeMethods.CRhinoFontTable_CurrentFontIndex(m_doc.m_docId);
      }
    }

    public Rhino.DocObjects.Font this[int index]
    {
      get
      {
        if (index < 0 || index >= Count)
          return null;
        return new Rhino.DocObjects.Font(index, m_doc);
      }
    }

    /// <example>
    /// <code source='examples\vbnet\ex_textjustify.vb' lang='vbnet'/>
    /// <code source='examples\cs\ex_textjustify.cs' lang='cs'/>
    /// <code source='examples\py\ex_textjustify.py' lang='py'/>
    /// </example>
    public int FindOrCreate(string face, bool bold, bool italic)
    {
      return UnsafeNativeMethods.CRhinoFontTable_FindOrCreate(m_doc.m_docId, face, bold, italic);
    }

    #region enumerator

    // for IEnumerable<Layer>
    public IEnumerator<Font> GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<FontTable, Font>(this);
    }

    // for IEnumerable
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
      return new Rhino.Collections.TableEnumerator<FontTable, Font>(this);
    }

    #endregion

  }
}
#endif