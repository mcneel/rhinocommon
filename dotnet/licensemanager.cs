using System;
using System.Runtime.InteropServices;

#if RHINO_SDK
using Rhino.PlugIns;

namespace Rhino.Runtime
{
  static class LicenseManager
  {
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <returns>
    /// Returns LicenseUtils.Initialize()
    /// </returns>
    internal delegate bool InitializeCallback();
    private static readonly InitializeCallback InitializeProc = InitializeHelper;
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <param name="message"></param>
    /// <param name="resultString"></param>
    internal delegate void EchoCallback([MarshalAs(UnmanagedType.LPWStr)]string message, IntPtr resultString);
    private static readonly EchoCallback EchoProc = EchoHelper;
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <param name="cdKey"></param>
    /// <returns></returns>
    internal delegate bool ShowValidationUiCallback([MarshalAs(UnmanagedType.LPWStr)]string cdKey);
    private static readonly ShowValidationUiCallback ShowValidationUiProc = ShowValidationUiHelper;
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <param name="mode">
    /// Constants defined in rh_license.cpp which define which LicenseUtils
    /// method to call.
    /// </param>
    /// <param name="id">
    /// License type id
    /// </param>
    /// <returns></returns>
    internal delegate int UuidCallback(int mode, Guid id);
    static readonly UuidCallback UuidProc = UuidHelper;
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <param name="title">
    /// License description string
    /// </param>
    /// <param name="id">
    /// License type id
    /// </param>
    /// <param name="productBuildType"></param>
    /// <param name="path">
    /// Full path to module which we are getting a license for, will check
    /// module for the required digital signatures.
    /// </param>
    /// <param name="validator">
    /// A pointer to the CRhinoLicenseValidator associated with this call, this
    /// object contains the ValidateProductKey method used by the Zoo Client to
    /// validate key values.
    /// </param>
    /// <returns>
    /// Returns a value of 1 on success or 0 on failure.
    /// </returns>
    internal delegate int GetLicenseCallback([MarshalAs(UnmanagedType.LPWStr)]string title, Guid id, int productBuildType, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr validator);
    private static readonly GetLicenseCallback GetLicenseProc = GetLicenseHelper;
    /// <summary>
    /// Delegate passed to rhcommon_c as a call back function pointer
    /// </summary>
    /// <param name="id">
    /// License type id
    /// </param>
    /// <param name="title">
    /// License description string
    /// </param>
    /// <param name="capabilities">
    /// Bitwise flag containing a list of buttons to be displayed on the custom
    /// get license dialog.
    /// </param>
    /// <param name="textMask">
    /// Optional text mask to be applied to the license key input control
    /// </param>
    /// <param name="path">
    /// Full path to module which we are getting a license for, will check
    /// module for the required digital signatures.
    /// </param>
    /// <param name="validator">
    /// A pointer to the CRhinoLicenseValidator associated with this call, this
    /// object contains the ValidateProductKey method used by the Zoo Client to
    /// validate key values.
    /// </param>
    /// <returns>
    /// Returns a value of 1 on success or 0 on failure.
    /// </returns>
    internal delegate int GetCustomLicenseCallback(Guid id, [MarshalAs(UnmanagedType.LPWStr)]string title, uint capabilities, [MarshalAs(UnmanagedType.LPWStr)]string textMask, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr validator);
    private static readonly GetCustomLicenseCallback GetCustomLicenseProc = CustomGetLicenseHelper;

    internal delegate bool AskUserForLicenseCallback([MarshalAs(UnmanagedType.LPWStr)]string productTitle, bool standAlone, IntPtr parent, Guid productId, int productBuildType, [MarshalAs(UnmanagedType.LPWStr)]string texMask, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr validator);
    private static readonly AskUserForLicenseCallback AskUserForLicenseProc = AskUserForLicenseHelper;
    /// <summary>
    /// Gets set to true after initial call to
    /// UnsafeNativeMethods.RHC_SetLicenseManagerCallbacks and is checked to
    /// make sure it only gets called one time.
    /// </summary>
    private static bool _setCallbacksWasRun;
    /// <summary>
    /// Only needs to be called once, will call into rhcommon_c and set call
    /// back function pointers
    /// </summary>
    static public void SetCallbacks()
    {
      if (_setCallbacksWasRun) return;
      _setCallbacksWasRun = true;
      UnsafeNativeMethods.RHC_SetLicenseManagerCallbacks(InitializeProc,
                                                         EchoProc,
                                                         ShowValidationUiProc,
                                                         UuidProc,
                                                         GetLicenseProc,
                                                         GetCustomLicenseProc,
                                                         AskUserForLicenseProc);
    }

    #region rhcommon_c call back methods
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    static bool InitializeHelper()
    {
      return LicenseUtils.Initialize();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <param name="resultString"></param>
    static void EchoHelper([MarshalAs(UnmanagedType.LPWStr)]string message, IntPtr resultString)
    {
      var echoResult = LicenseUtils.Echo(message);
      UnsafeNativeMethods.ON_wString_Set(resultString, echoResult);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="cdKey"></param>
    /// <returns></returns>
    static bool ShowValidationUiHelper([MarshalAs(UnmanagedType.LPWStr)]string cdKey)
    {
      return LicenseUtils.ShowLicenseValidationUi(cdKey);
    }
    /// <summary>
    /// Helper class used by GetLicenseHelper and GetCustomLicenseHelper
    /// methods when calling back into UnsafeNativeMethods.RHC_GetLicense
    /// </summary>
    class ValidatorHelper
    {
      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="validator">CRhinoLicenseValidator unmanaged pointer</param>
      /// <param name="id">License type Id</param>
      /// <param name="title">License title</param>
      /// <param name="path">Full path to the executable requesting the license</param>
      public ValidatorHelper(IntPtr validator, Guid id, string title, string path)
      {
        _validator = validator;
        _productId = id;
        _productTitle = title;
        _path = path;
      }
      /// <summary>
      /// Passed as a delegate to LicenseUtils.GetLicense(), will call the C++
      /// ValidateProductKey() method then and copy CRhinoLicenseValidator C++
      /// data to the LicenseData output object.
      /// </summary>
      /// <param name="productKey"></param>
      /// <param name="licenseData">
      /// Key information is coppied to this object
      /// </param>
      /// <returns></returns>
      public ValidateResult ValidateProductKey(string productKey, out LicenseData licenseData)
      {
        // Create an empty LicenseData
        licenseData = new LicenseData();
        var rc = 0;
        // Call back into the C++ unmanaged function pointer
        if (IntPtr.Zero != _validator)
          rc = UnsafeNativeMethods.RHC_ValidateProductKey(productKey, _validator);
        if (rc != 1)
          return (-1 == rc ? ValidateResult.ErrorHideMessage : ValidateResult.ErrorShowMessage);
        // Copy unmanaged C++ validate data to the managed LicenseData
        var year = 0;
        var month = 0;
        var day = 0;
        var hour = 0;
        var minute = 0;
        var second = 0;
        var licenseCount = 0;
        var buildType = 0;
        IntPtr hIcon;
        // String placeholders
        using (var shSerailNumber = new StringHolder())
        using (var shLicenseTitle = new StringHolder())
        using (var shProductLicense = new StringHolder())
        {
          // Get ON_wString pointers
          var pSerialNumber = shSerailNumber.NonConstPointer();
          var pLicenseTitle = shLicenseTitle.NonConstPointer();
          var pProductLicense = shProductLicense.NonConstPointer();
          hIcon = UnsafeNativeMethods.RHC_ExtractLicenseData(_validator,
                                                             ref year,
                                                             ref month,
                                                             ref day,
                                                             ref hour,
                                                             ref minute,
                                                             ref second,
                                                             pSerialNumber,
                                                             ref licenseCount,
                                                             pLicenseTitle,
                                                             pProductLicense,
                                                             ref buildType);
          // Copy ON_wString values to C# strings
          licenseData.SerialNumber = shSerailNumber.ToString();
          licenseData.LicenseTitle = shLicenseTitle.ToString();
          licenseData.ProductLicense = shProductLicense.ToString();
        }
        // Set the expiration date using the C++ date data
        if (year >= 2010)
          licenseData.DateToExpire = new DateTime(year, month, day, hour, minute, second);
        licenseData.LicenseCount = licenseCount;
        licenseData.BuildType = (LicenseBuildType)buildType;
        // Icon associated with the specified license type
        if (hIcon != IntPtr.Zero)
        {
          // Create a new icon from the handle.
          var newIcon = System.Drawing.Icon.FromHandle(hIcon);

          // Set the LicenseData icon. Note, LicenseData creates it's own copy of the icon.
          licenseData.ProductIcon = newIcon;

          // When using Icon::FromHandle, you must dispose of the original icon by using the
          // DestroyIcon method in the Win32 API to ensure that the resources are released.
          DestroyIcon(newIcon.Handle);
        }

        return ValidateResult.Success;
      }

      #region Methods required by Zoo Client
      /// <summary>
      /// This method is called by the Zoo Client using reflection so there are
      /// no direct calls to it here.  The Zoo Client will fail if this method
      /// is not defined.
      /// </summary>
      /// <returns></returns>
      public string Path() { return _path; }
      /// <summary>
      /// This method is called by the Zoo Client using reflection so there are
      /// no direct calls to it here.  The Zoo Client will fail if this method
      /// is not defined.
      /// </summary>
      /// <returns></returns>
      public Guid ProductId() { return _productId; }
      /// <summary>
      /// This method is called by the Zoo Client using reflection so there are
      /// no direct calls to it here.  The Zoo Client will fail if this method
      /// is not defined.
      /// </summary>
      /// <returns></returns>
      public string ProductTitle() { return _productTitle; }
      #endregion Methods required by Zoo Client

      #region Interop Windows imports
      [DllImport("user32.dll", CharSet = CharSet.Auto)]
      extern static bool DestroyIcon(IntPtr handle);
      #endregion Interop Windows imports

      #region Members
      /// <summary>
      /// CRhinoLicenseValidator pointer passed to the constructor
      /// </summary>
      readonly IntPtr _validator;
      readonly Guid _productId;
      readonly string _productTitle;
      /// <summary>
      /// Full path to the executable requesting the license.
      /// </summary>
      readonly string _path;
      #endregion Members
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="id"></param>
    /// <param name="productBuildType"></param>
    /// <param name="path"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    private static int GetLicenseHelper([MarshalAs(UnmanagedType.LPWStr)]string title, Guid id, int productBuildType, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr validator)
    {
      var helper = new ValidatorHelper(validator, id, title, path);
      var result = LicenseUtils.GetLicense(productBuildType, helper.ValidateProductKey);
      return (result ? 1 : 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="title"></param>
    /// <param name="capabilities"></param>
    /// <param name="textMask"></param>
    /// <param name="path"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    private static int CustomGetLicenseHelper(Guid id, [MarshalAs(UnmanagedType.LPWStr)]string title, uint capabilities, [MarshalAs(UnmanagedType.LPWStr)]string textMask, [MarshalAs(UnmanagedType.LPWStr)]string path, IntPtr validator)
    {
      var helper = new ValidatorHelper(validator, id, title, path);
      var result = LicenseUtils.GetLicense(helper.ValidateProductKey, (int)capabilities, textMask);
      return (result ? 1 : 0);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="productTitle"></param>
    /// <param name="standAlone"></param>
    /// <param name="parent"></param>
    /// <param name="productId"></param>
    /// <param name="productBuildType"></param>
    /// <param name="path"></param>
    /// <param name="validator"></param>
    /// <returns></returns>
    private static bool AskUserForLicenseHelper([MarshalAs(UnmanagedType.LPWStr)] string productTitle,
                                                bool standAlone,
                                                IntPtr parent,
                                                Guid productId,
                                                int productBuildType,
                                                [MarshalAs(UnmanagedType.LPWStr)] string textMask,
                                                [MarshalAs(UnmanagedType.LPWStr)] string path,
                                                IntPtr validator)
    {
      var helper = new ValidatorHelper(validator, productId, productTitle, path);
      var parentControl = System.Windows.Forms.ContainerControl.FromHandle(parent);
      var rc = LicenseUtils.AskUserForLicense(productBuildType, standAlone, parentControl, textMask, helper.ValidateProductKey);
      return rc;
    }
    /// <summary>
    /// The CRhCmn_ZooClient; ReturnLicense(), CheckOutLicense(),
    /// CheckInLicense(), ConvertLicense(), and GetLicenseType() methods call
    /// this method with the appropriate mode.
    /// </summary>
    /// <param name="mode">Calling function Id</param>
    /// <param name="id">License type Id</param>
    /// <returns></returns>
    static int UuidHelper(int mode, Guid id)
    {
      // Keep these values in synch with the values located at the top
      // of the rh_licensemanager.cpp file in the rhcommon_c project.
      const int modeReturnLicense = 1;
      const int modeCheckOutLicense = 2;
      const int modeCheckInLicense = 3;
      const int modeConvertLicense = 4;
      const int modeGetLicenseType = 5;
      switch (mode)
      {
        case modeReturnLicense:
          return LicenseUtils.ReturnLicense(id) ? 1 : 0;
        case modeCheckOutLicense:
          return LicenseUtils.CheckOutLicense(id) ? 1 : 0;
        case modeCheckInLicense:
          return LicenseUtils.CheckInLicense(id) ? 1 : 0;
        case modeConvertLicense:
          return LicenseUtils.ConvertLicense(id) ? 1 : 0;
        case modeGetLicenseType:
          return LicenseUtils.GetLicenseType(id);
      }
      return 0;
    }
    #endregion rhcommon_c call back methods
  }
}
#endif
