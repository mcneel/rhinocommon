#pragma warning disable 1591
#if RHINO_SDK
namespace Rhino.ApplicationSettings
{
  /// <summary>
  /// Provides static (Shared in Vb.Net) methods to modify Rhino History settings.
  /// </summary>
  public static class HistorySettings
  {
    const int idxRecordingEnabled = 0;
    const int idxUpdateEnabled = 1;
    const int idxObjectLockingEnabled = 2;

    public static bool RecordingEnabled
    {
      get
      {
        return UnsafeNativeMethods.CRhinoHistoryManager_GetBool(idxRecordingEnabled);
      }
      set
      {
        UnsafeNativeMethods.CRhinoHistoryManager_SetBool(idxRecordingEnabled, value);
      }
    }

    public static bool UpdateEnabled
    {
      get
      {
        return UnsafeNativeMethods.CRhinoHistoryManager_GetBool(idxUpdateEnabled);
      }
      set
      {
        UnsafeNativeMethods.CRhinoHistoryManager_SetBool(idxUpdateEnabled, value);
      }
    }
    public static bool ObjectLockingEnabled
    {
      get
      {
        return UnsafeNativeMethods.CRhinoHistoryManager_GetBool(idxObjectLockingEnabled);
      }
      set
      {
        UnsafeNativeMethods.CRhinoHistoryManager_SetBool(idxObjectLockingEnabled, value);
      }
    }
  }
}
// skip
//public class HistoryManager { }
//public class RecordHistoryCommandOptionHelper { }
#endif