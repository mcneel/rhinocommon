#pragma warning disable 1591
using System;

namespace Rhino.Display
{
  public class DisplayBitmap : IDisposable
  {
    IntPtr m_pDisplayBmp;
    internal IntPtr NonConstPointer() { return m_pDisplayBmp; }

    public DisplayBitmap(System.Drawing.Bitmap bitmap)
    {
      IntPtr hBitmap = bitmap.GetHbitmap();
      m_pDisplayBmp = UnsafeNativeMethods.CRhCmnDisplayBitmap_New(hBitmap);
    }

    private DisplayBitmap(IntPtr pBmp)
    {
      m_pDisplayBmp = pBmp;
    }

    public static DisplayBitmap Load(string path)
    {
      IntPtr pBmp = UnsafeNativeMethods.CRhCmnDisplayBitmap_New2(path);
      if (IntPtr.Zero == pBmp)
        return null;
      return new DisplayBitmap(pBmp);
    }


    ~DisplayBitmap() { Dispose(false); }

    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pDisplayBmp)
      {
        UnsafeNativeMethods.CRhCmnDisplayBitmap_Delete(m_pDisplayBmp);
      }
      m_pDisplayBmp = IntPtr.Zero;
    }

  }

  class DisplayBitmapDrawList
  {
  }
}
