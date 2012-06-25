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

      public Guid Id
      {
        get { return m_id; }
      }

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

      public string FileAlias
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

      public bool Close()
      {
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

      public ToolbarFile OpenFile(string path)
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