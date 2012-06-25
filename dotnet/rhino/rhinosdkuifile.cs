#pragma warning disable 1591
using System;
using System.Collections.Generic;

// none of the UI namespace needs to be in the stand-alone opennurbs library
#if RHINO_SDK

namespace Rhino
{
  namespace UI
  {
    public sealed class ToolbarFile
    {
      readonly Guid m_id;
      internal ToolbarFile(Guid id)
      {
        m_id = id;
      }

      public Guid Id { get { return m_id; } }

      /// <summary>Full path to this file on disk</summary>
      public string Path
      {
        get
        {
          using(Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.CRhinoUiFile_FileName(m_id, pString, false);
            return sh.ToString();
          }
        }
      }

      public string Name
      {
        get
        {
          using(Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.CRhinoUiFile_FileName(m_id, pString, true);
            return sh.ToString();
          }
        }
      }

      public bool Close(bool prompt)
      {
        if (prompt)
        {
          System.Windows.Forms.DialogResult rc = Rhino.UI.Dialogs.ShowMessageBox("Close toolbar file?", "Close",
            System.Windows.Forms.MessageBoxButtons.YesNo,
            System.Windows.Forms.MessageBoxIcon.Question);
          if (rc == System.Windows.Forms.DialogResult.No)
            return false;
        }
        return UnsafeNativeMethods.CRhinoUiFile_FileClose(m_id);
      }

      public bool Save()
      {
        return UnsafeNativeMethods.CRhinoUiFile_FileSave(m_id);
      }

      public bool SaveAs(string path)
      {
        return UnsafeNativeMethods.CRhinoUiFile_FileSaveAs(m_id, path);
      }

      public int GroupCount
      {
        get { return UnsafeNativeMethods.CRhinoUiFile_GroupCount(m_id); }
      }

      public int ToolbarCount
      {
        get { return UnsafeNativeMethods.CRhinoUiFile_ToolbarCount(m_id); }
      }

      public Toolbar GetToolbar(int index)
      {
        Guid id = UnsafeNativeMethods.CRhinoUiFile_ToolBarID(m_id, index);
        if (id == Guid.Empty)
          return null;
        return new Toolbar(this, id);
      }

      public ToolbarGroup GetGroup(int index)
      {
        Guid id = UnsafeNativeMethods.CRhinoUiFile_GroupID(m_id, index);
        if (id == Guid.Empty)
          return null;
        return new ToolbarGroup(this, id);
      }

      public ToolbarGroup GetGroup(string name)
      {
        int count = GroupCount;
        for (int i = 0; i < count; i++)
        {
          ToolbarGroup group = GetGroup(i);
          if (string.Compare(group.Name, name) == 0)
            return group;
        }
        return null;
      }
    }

    public sealed class Toolbar
    {
      readonly ToolbarFile m_parent;
      readonly Guid m_id;

      internal Toolbar(ToolbarFile parent, Guid id)
      {
        m_parent = parent;
        m_id = id;
      }

      public Guid Id
      {
        get { return m_id; }
      }

      public string Name
      {
        get
        {
          using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.CRhinoUiFile_ToolBarName(m_parent.Id, m_id, pString);
            return sh.ToString();
          }
        }
      }
    }

    public sealed class ToolbarGroup
    {
      readonly ToolbarFile m_parent;
      readonly Guid m_id;

      internal ToolbarGroup(ToolbarFile parent, Guid id)
      {
        m_parent = parent;
        m_id = id;
      }

      public Guid Id
      {
        get { return m_id; }
      }

      public string Name
      {
        get
        {
          using (Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
          {
            IntPtr pString = sh.NonConstPointer();
            UnsafeNativeMethods.CRhinoUiFile_GroupName(m_parent.Id, m_id, pString);
            return sh.ToString();
          }
        }
      }

      public bool Visible
      {
        get
        {
          return UnsafeNativeMethods.CRhinoUiFile_GroupIsVisible(m_parent.Id, m_id);
        }
        set
        {
          UnsafeNativeMethods.CRhinoUiFile_GroupShow(m_parent.Id, m_id, value);
        }
      }

      public bool IsDocked
      {
        get
        {
          return UnsafeNativeMethods.CRhinoUiFile_GroupIsDocked(m_parent.Id, m_id);
        }
      }
    }

    public sealed class ToolbarFileCollection : IEnumerable<ToolbarFile>
    {
      internal ToolbarFileCollection() { }

      /// <summary>
      /// Number of open toolbar files
      /// </summary>
      public int Count
      {
        get{ return UnsafeNativeMethods.CRhinoUiFile_FileCount(); }
      }

      public ToolbarFile this[int index]
      {
        get
        {
          Guid id = UnsafeNativeMethods.CRhinoUiFile_FileID(index);
          if( Guid.Empty==id )
            throw new IndexOutOfRangeException();
          return new ToolbarFile(id);
        }
      }

      public ToolbarFile FindByName(string name, bool ignoreCase)
      {
        foreach (ToolbarFile tb in this)
        {
          if (string.Compare(tb.Name, name, ignoreCase) == 0)
            return tb;
        }
        return null;
      }

      public ToolbarFile FindByPath(string path)
      {
        foreach (ToolbarFile tb in this)
        {
          if (string.Compare(tb.Path, path, true) == 0)
            return tb;
        }
        return null;
      }

      public ToolbarFile Open(string path)
      {
        if( !System.IO.File.Exists(path) )
          throw new System.IO.FileNotFoundException();
        Guid id = UnsafeNativeMethods.CRhinoUiFile_FileOpen(path);
        if( id==Guid.Empty )
          return null;
        return new ToolbarFile(id);
      }

      public IEnumerator<ToolbarFile> GetEnumerator()
      {
        int count = Count;
        for( int i=0; i<count; i++ ) yield return this[i];
      }

      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
        int count = Count;
        for (int i = 0; i < count; i++) yield return this[i];
      }
    }
  }
}
#endif