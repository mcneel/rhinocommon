#pragma warning disable 1591
using System;

#if RHINO_SDK
namespace Rhino.Input.Custom
{
  public enum GetLineMode : int
  {
    TwoPoint = 0,
    SurfaceNormal = 1,
    Angled = 2,
    Vertical = 3,
    FourPoint = 4,
    Bisector = 5,
    Perpendicular = 6,
    Tangent = 7,
    CurveEnd = 8,
    CPlaneNormalVector = 9
  };

  /// <summary>
  /// Use GetLine class to interactively get a line.  The Rhino "Line"
  /// command uses GetLine.
  /// </summary>
  public class GetLine : IDisposable
  {
    IntPtr m_pArgsRhinoGetLine;
    public GetLine()
    {
      m_pArgsRhinoGetLine = UnsafeNativeMethods.CArgsRhinoGetLine_New();
    }

    internal IntPtr ConstPointer() { return m_pArgsRhinoGetLine; }
    internal IntPtr NonConstPointer() { return m_pArgsRhinoGetLine; }

    /// <summary>
    /// Passively reclaims unmanaged resources when the class user did not explicitly call Dispose().
    /// </summary>
    ~GetLine()
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

    /// <summary>
    /// For derived class implementers.
    /// <para>This method is called with argument true when class user calls Dispose(), while with argument false when
    /// the Garbage Collector invokes the finalizer, or Finalize() method.</para>
    /// <para>You must reclaim all used unmanaged resources in both cases, and can use this chance to call Dispose on disposable fields if the argument is true.</para>
    /// <para>Also, you must call the base virtual method within your overriding method.</para>
    /// </summary>
    /// <param name="disposing">true if the call comes from the Dispose() method; false if it comes from the Garbage Collector finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
      if (IntPtr.Zero != m_pArgsRhinoGetLine)
      {
        UnsafeNativeMethods.CArgsRhinoGetLine_Delete(m_pArgsRhinoGetLine);
        m_pArgsRhinoGetLine = IntPtr.Zero;
      }
    }

    /// <summary>
    /// Perform the 'get' operation.
    /// </summary>
    /// <param name="line"></param>
    /// <returns></returns>
    public Commands.Result Get(out Rhino.Geometry.Line line)
    {
      IntPtr pThis = NonConstPointer();
      line = Rhino.Geometry.Line.Unset;
      int rc = UnsafeNativeMethods.RHC_RhinoGetLine2(pThis, ref line, IntPtr.Zero);
      return (Commands.Result)rc;
    }

    const int idxFirstPointPrompt = 0;
    const int idxMidPointPrompt = 1;
    const int idxSecondPointPrompt = 2;
    string GetStringHelper(int which)
    {
      IntPtr pConstThis = ConstPointer();
      using(Rhino.Runtime.StringHolder sh = new Runtime.StringHolder())
      {
        IntPtr pString = sh.NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetLine_GetString(pConstThis, which, pString);
        return sh.ToString();
      }
    }
    void SetStringHelper(int which, string s)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CArgsRhinoGetLine_SetString(pThis, which, s);
    }

    /// <summary>Prompt when getting first point</summary>
    public string FirstPointPrompt
    {
      get { return GetStringHelper(idxFirstPointPrompt); }
      set { SetStringHelper(idxFirstPointPrompt, value); }
    }

    /// <summary>Prompt when getting midpoint</summary>
    public string MidPointPrompt
    {
      get { return GetStringHelper(idxMidPointPrompt); }
      set { SetStringHelper(idxMidPointPrompt, value); }
    }

    /// <summary>Prompt when getting second point</summary>
    public string SecondPointPrompt
    {
      get { return GetStringHelper(idxSecondPointPrompt); }
      set { SetStringHelper(idxSecondPointPrompt, value); }
    }

    const int idxAcceptZeroLengthLine = 0;
    const int idxHaveFeedbackColor = 1;
    const int idxEnableFromBothSidesOption = 2;
    const int idxEnableFromMidPointOption = 3;
    const int idxEnableAllVariations = 4;
    bool GetBoolHelper(int which)
    {
      IntPtr pConstThis = ConstPointer();
      return UnsafeNativeMethods.CArgsRhinoGetLine_GetBool(pConstThis, which);
    }

    void SetBoolHelper(int which, bool value)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CArgsRhinoGetLine_SetBool(pThis, which, value);
    }


    /// <summary>
    /// Controls whether or not a zero length line is acceptable.
    /// The default is to require the user to keep picking the end
    /// point until we get a point different than the start point.
    /// </summary>
    public bool AcceptZeroLengthLine
    {
      get { return GetBoolHelper(idxAcceptZeroLengthLine); }
      set { SetBoolHelper(idxAcceptZeroLengthLine, value); }
    }

    /// <summary>
    /// If true, the feedback color is used to draw the dynamic
    /// line when the second point is begin picked.  If false,
    /// the active layer color is used.
    /// </summary>
    public bool HaveFeedbackColor
    {
      get { return GetBoolHelper(idxHaveFeedbackColor); }
    }

    /// <summary>
    /// If set, the feedback color is used to draw the dynamic
    /// line when the second point is begin picked.  If not set,
    /// the active layer color is used.
    /// </summary>
    public System.Drawing.Color FeedbackColor
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int argb = UnsafeNativeMethods.CArgsRhinoGetLine_GetFeedbackColor(pConstThis);
        return System.Drawing.Color.FromArgb(argb);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        int argb = value.ToArgb();
        UnsafeNativeMethods.CArgsRhinoGetLine_SetFeedbackColor(pThis, argb);
      }
    }

    /// <summary>
    /// If FixedLength > 0, the line must have the specified length
    /// </summary>
    public double FixedLength
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        return UnsafeNativeMethods.CArgsRhinoGetLine_GetFixedLength(pConstThis);
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetLine_SetFixedLength(pThis, value);
      }
    }

    /// <summary>
    /// If true, then the "BothSides" option shows up when the
    /// start point is inteactively picked.
    /// </summary>
    /// <param name="on"></param>
    public void EnableFromBothSidesOption(bool on)
    {
      SetBoolHelper(idxEnableFromBothSidesOption, on);
    }

    /// <summary>
    /// If true, the the "MidPoint" options shows up
    /// </summary>
    /// <param name="on"></param>
    public void EnableFromMidPointOption(bool on)
    {
      SetBoolHelper(idxEnableFromMidPointOption, on);
    }

    /// <summary>
    /// If true, then all line variations are shown if the default line mode is used
    /// </summary>
    /// <param name="on"></param>
    public void EnableAllVariations(bool on)
    {
      SetBoolHelper(idxEnableAllVariations, on);
    }

    /// <summary>
    /// Use SetFirstPoint to specify the line's starting point and skip
    /// the start point interactive picking
    /// </summary>
    /// <param name="point"></param>
    public void SetFirstPoint(Rhino.Geometry.Point3d point)
    {
      IntPtr pThis = NonConstPointer();
      UnsafeNativeMethods.CArgsRhinoGetLine_SetFirstPoint(pThis, point);
    }

    /// <summary>
    /// Mode used
    /// </summary>
    public GetLineMode GetLineMode
    {
      get
      {
        IntPtr pConstThis = ConstPointer();
        int rc = UnsafeNativeMethods.CArgsRhinoGetLine_GetLineMode(pConstThis);
        return (GetLineMode)rc;
      }
      set
      {
        IntPtr pThis = NonConstPointer();
        UnsafeNativeMethods.CArgsRhinoGetLine_SetLineMode(pThis, (int)value);
      }
    }

    //public Rhino.DocObjects.ObjRef Point1ObjRef() { }
    //public Rhino.DocObjects.ObjRef Point2ObjRef() { }
  }
}
#endif