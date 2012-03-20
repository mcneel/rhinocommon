using System;
//public class ON_CheckSum { }
//public class ON_UnitSystem { }

namespace Rhino.Runtime.InteropWrappers
{
  /// <summary>
  /// Represents a wrapper to an unmanaged OpenNurbs string.
  /// <para>Wraps a C++ ON_wString*.</para>
  /// </summary>
  public class StringWrapper : IDisposable
  {
    IntPtr m_ptr; //ON_wString*

    /// <summary>
    /// Initializes a new empty unmanaged string.
    /// </summary>
    public StringWrapper()
    {
      m_ptr = UnsafeNativeMethods.ON_wString_New(null);
    }

    /// <summary>
    /// Initializes a new unmanaged string with an initial value.
    /// </summary>
    /// <param name="s">The initial value.</param>
    public StringWrapper(string s)
    {
      m_ptr = UnsafeNativeMethods.ON_wString_New(s);
    }

    /// <summary>
    /// Gets the const pointer (const ON_wString*).
    /// </summary>
    /// <returns>The const pointer.</returns>
    public IntPtr ConstPointer { get { return m_ptr; } }

    /// <summary>
    /// Gets the non-const pointer (ON_wString*).
    /// </summary>
    /// <returns>The non-const pointer.</returns>
    public IntPtr NonConstPointer { get { return m_ptr; } }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~StringWrapper()
    {
      Dispose(false);
    }

    /// <summary>
    /// Actively reclaims unmanaged resources that this instance uses.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_ptr)
      {
        UnsafeNativeMethods.ON_wString_Delete(m_ptr);
        m_ptr = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Returns the string contents of this wrapper.
    /// </summary>
    /// <returns>A managed string.</returns>
    public override string ToString()
    {
      IntPtr pString = UnsafeNativeMethods.ON_wString_Get(m_ptr);
      string rc = System.Runtime.InteropServices.Marshal.PtrToStringUni(pString);
      return rc ?? String.Empty;
    }

    /// <summary>
    /// Set contents of this string.
    /// </summary>
    /// <param name="s">The new string.</param>
    public void SetString(string s)
    {
      UnsafeNativeMethods.ON_wString_Set(m_ptr, s);
    }

    /// <summary>
    /// Set contents of an ON_wString*
    /// </summary>
    /// <param name="pON_wString"></param>
    /// <param name="s"></param>
    public static void SetStringOnPointer(IntPtr pON_wString, string s)
    {
      UnsafeNativeMethods.ON_wString_Set(pON_wString, s);
    }

    /// <summary>
    /// Get string from an ON_wString*
    /// </summary>
    /// <param name="pConstON_wString"></param>
    /// <returns></returns>
    public static string GetStringFromPointer(IntPtr pConstON_wString)
    {
      string rc = System.Runtime.InteropServices.Marshal.PtrToStringUni(pConstON_wString);
      return rc ?? String.Empty;
    }
  }
}