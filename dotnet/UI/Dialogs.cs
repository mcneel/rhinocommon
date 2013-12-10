#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Rhino.Runtime.InteropWrappers;

// none of the UI namespace needs to be in the stand-alone opennurbs library
#if RHINO_SDK

namespace Rhino
{
  namespace UI
  {
    public class GetColorEventArgs : EventArgs
    {
      readonly System.Drawing.Color m_input_color;
      System.Drawing.Color m_selected_color = System.Drawing.Color.Empty;
      readonly bool m_include_button_colors;
      readonly string m_title;
      internal GetColorEventArgs(System.Drawing.Color inputColor, bool includeButtonColors, string title)
      {
        m_input_color = inputColor;
        m_include_button_colors = includeButtonColors;
        m_title = title;
      }

      public System.Drawing.Color InputColor
      {
        get { return m_input_color; }
      }

      public System.Drawing.Color SelectedColor
      {
        get { return m_selected_color; }
        set { m_selected_color = value; }
      }

      public bool IncludeButtonColors
      {
        get { return m_include_button_colors; }
      }

      public string Title
      {
        get { return m_title; }
      }
    }


    //public delegate System.Windows.Forms.DialogResult ShowColorDialogEventHandler(ref System.Drawing.Color color, bool includeButtonColors, string title, System.Windows.Forms.IWin32Window parent);

    public class WaitCursor : IDisposable
    {
      private readonly int idxDefaultCursor = 0;
      private readonly int idxWaitCursor = 1;

      public WaitCursor()
      {
        Set();
      }

      public void Set() { UnsafeNativeMethods.CRhinoApp_SetCursor(idxWaitCursor); }
      public void Clear() { UnsafeNativeMethods.CRhinoApp_SetCursor(idxDefaultCursor); }

      public void Dispose()
      {
        Clear();
      }
    }

    public static class Dialogs
    {
      public static int ShowContextMenu(IEnumerable<string> items, System.Drawing.Point screenPoint, IEnumerable<int> modes)
      {
        IntPtr ptr_string_array = UnsafeNativeMethods.ON_StringArray_New();
        foreach (string item in items)
        {
          UnsafeNativeMethods.ON_StringArray_Append(ptr_string_array, item);
        }
        int modecount = 0;
        int[] array_modes = null;
        if (modes != null)
        {
          List<int> list_modes = new List<int>(modes);
          modecount = list_modes.Count;
          array_modes = list_modes.ToArray();
        }
        int rc = UnsafeNativeMethods.RHC_ShowContextMenu(ptr_string_array, screenPoint.X, screenPoint.Y, modecount, array_modes);
        UnsafeNativeMethods.ON_StringArray_Delete(ptr_string_array);
        return rc;
      }


      public static void SetCustomColorDialog( EventHandler<GetColorEventArgs> handler)
      {
        m_ShowCustomColorDialog = handler;
        UnsafeNativeMethods.RHC_SetReplaceColorDialogCallback(handler == null ? null : m_callback);
      }

      private static EventHandler<GetColorEventArgs> m_ShowCustomColorDialog;
      private static readonly ColorDialogCallback m_callback = OnCustomColorDialog;
      internal delegate int ColorDialogCallback(ref int argn, int colorButtons, IntPtr titleAsStringHolder, IntPtr hParent);

      private static int OnCustomColorDialog(ref int argb, int colorButtons, IntPtr titleAsStringHolder, IntPtr hParent)
      {
        int rc = 0;
        if (m_ShowCustomColorDialog != null)
        {
          try
          {
            var color = System.Drawing.Color.FromArgb(argb);
            System.Windows.Forms.IWin32Window parent = null;
            string title = StringHolder.GetString(titleAsStringHolder);
            GetColorEventArgs e = new GetColorEventArgs(color, colorButtons==1, title);

            if( hParent != IntPtr.Zero )
              parent = new RhinoWindow(hParent);
            m_ShowCustomColorDialog(parent, e);
            if( e.SelectedColor != System.Drawing.Color.Empty )
            {
              argb = e.SelectedColor.ToArgb();
              rc = 1;
            }
          }
          catch (Exception ex)
          {
            Runtime.HostUtils.ExceptionReport(ex);
          }
        }
        return rc;
      }

      // Functions to add
      //[in rhinosdkutilities.h]
      //  RhinoLineTypeDialog
      //  RhinoPrintWidthDialog
      //  RhinoSelectMultipleLayersDialog
      //  RhinoYesNoMessageBox


      /// <summary>
      /// Destroy the splash screen if it is being displayed.
      /// </summary>
      public static void KillSplash()
      {
        UnsafeNativeMethods.RHC_RhinoKillSplash();
      }

      /// <summary>
      /// Hides a form, calls a provided function, and then shows the form. This works for
      /// modal forms. Useful for selecting objects or getting points while a modal dialog
      /// is running.
      /// </summary>
      /// <param name="form">A form window.</param>
      /// <param name="pickFunction">A picking delegate.</param>
      public static void PushPickButton(System.Windows.Forms.Form form, System.EventHandler<EventArgs> pickFunction)
      {
        if (form == null || pickFunction == null)
          return;

        IntPtr handle = form.Handle;
        if( IntPtr.Zero == handle )
          return;
        if( form.Modal )
        {
          IntPtr pList = UnsafeNativeMethods.RHC_PushPickButtonHide(handle);
          if (IntPtr.Zero!=pList)
          {
            RhinoApp.Wait();
            RhinoApp.SetFocusToMainWindow();
            pickFunction(form, EventArgs.Empty);
            UnsafeNativeMethods.RHC_PushPickButtonShow(pList);
          }
        }
        else
        {
          form.Visible = false;
          RhinoApp.Wait();
          RhinoApp.SetFocusToMainWindow();
          pickFunction(form, EventArgs.Empty);
          form.Visible = true;
        }
      }

      /// <summary>
      /// Show a windows form that is modal in the sense that this function does not return until
      /// the form is closed, but also allows for interaction with other elements of the Rhino
      /// user interface.
      /// </summary>
      /// <param name="form">
      /// The form must have buttons that are assigned to the "AcceptButton" and "CancelButton".
      /// </param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      public static System.Windows.Forms.DialogResult ShowSemiModal(System.Windows.Forms.Form form)
      {
        if (Rhino.Runtime.HostUtils.RunningOnWindows)
          form.Load += SemiModalFormLoad;
        return form.ShowDialog(RhinoApp.MainWindow());
      }

      static void SemiModalFormLoad(object sender, EventArgs e)
      {
        IntPtr hMainWnd = RhinoApp.MainWindowHandle();
        UnsafeNativeMethods.EnableWindow(hMainWnd, true);
      }


      /// <summary>
      /// Display a text dialog similar to the dialog used for the "What" command.
      /// </summary>
      /// <param name="message">Text to display as the message content.</param>
      /// <param name="title">Test to display as the form title.</param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      public static System.Windows.Forms.DialogResult ShowTextDialog(string message, string title)
      {
        int rc = UnsafeNativeMethods.CRhinoTextOut_ShowDialog(message, title);
        return DialogResultFromInt(rc);
      }

      /// <summary>
      /// Same as System.Windows.Froms.MessageBox.Show but using a message box tailored to Rhino.
      /// </summary>
      /// <param name="message">Message box text content.</param>
      /// <param name="title">Message box title text.</param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      public static System.Windows.Forms.DialogResult ShowMessageBox(string message, string title)
      {
        return ShowMessageBox(message, title, System.Windows.Forms.MessageBoxButtons.OK,
          System.Windows.Forms.MessageBoxIcon.None, System.Windows.Forms.MessageBoxDefaultButton.Button1);
      }

      /// <summary>
      /// Same as System.Windows.Froms.MessageBox.Show but using a message box tailored to Rhino.
      /// </summary>
      /// <param name="message">Message box text content.</param>
      /// <param name="title">Message box title text.</param>
      /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
      /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      public static System.Windows.Forms.DialogResult ShowMessageBox(string message,
        string title,
        System.Windows.Forms.MessageBoxButtons buttons,
        System.Windows.Forms.MessageBoxIcon icon)
      {
        return ShowMessageBox(message, title, buttons, icon, System.Windows.Forms.MessageBoxDefaultButton.Button1);
      }
      /// <summary>
      /// Same as System.Windows.Froms.MessageBox.Show but using a message box tailored to Rhino.
      /// </summary>
      /// <param name="message">Message box text content.</param>
      /// <param name="title">Message box title text.</param>
      /// <param name="buttons">One of the System.Windows.Forms.MessageBoxButtons values that specifies which buttons to display in the message box.</param>
      /// <param name="icon">One of the System.Windows.Forms.MessageBoxIcon values that specifies which icon to display in the message box.</param>
      /// <param name="defaultButton">One of the System.Windows.Forms.MessageBoxDefaultButton values that specifies the default button for the message box.</param>
      /// <returns>One of the System.Windows.Forms.DialogResult values.</returns>
      public static System.Windows.Forms.DialogResult ShowMessageBox(string message,
        string title,
        System.Windows.Forms.MessageBoxButtons buttons,
        System.Windows.Forms.MessageBoxIcon icon,
        System.Windows.Forms.MessageBoxDefaultButton defaultButton)
      {
        const uint MB_OK = 0x00000000;
        const uint MB_OKCANCEL = 0x00000001;
        const uint MB_ABORTRETRYIGNORE = 0x00000002;
        const uint MB_YESNOCANCEL = 0x00000003;
        const uint MB_YESNO = 0x00000004;
        const uint MB_RETRYCANCEL = 0x00000005;
        //const uint MB_CANCELTRYCONTINUE = 0x00000006;
        const uint MB_ICONHAND = 0x00000010;
        const uint MB_ICONQUESTION = 0x00000020;
        const uint MB_ICONEXCLAMATION = 0x00000030;
        const uint MB_ICONASTERISK = 0x00000040;
        //const uint MB_USERICON = 0x00000080;
        const uint MB_ICONWARNING = MB_ICONEXCLAMATION;
        const uint MB_ICONERROR = MB_ICONHAND;
        const uint MB_ICONINFORMATION = MB_ICONASTERISK;
        const uint MB_ICONSTOP = MB_ICONHAND;
        const uint MB_DEFBUTTON1 = 0x00000000;
        const uint MB_DEFBUTTON2 = 0x00000100;
        const uint MB_DEFBUTTON3 = 0x00000200;
        //const uint MB_DEFBUTTON4 = 0x00000300;

        uint buttonFlags = 0;
        if (System.Windows.Forms.MessageBoxButtons.AbortRetryIgnore == buttons)
          buttonFlags = MB_ABORTRETRYIGNORE;
        else if (System.Windows.Forms.MessageBoxButtons.OK == buttons)
          buttonFlags = MB_OK;
        else if (System.Windows.Forms.MessageBoxButtons.OKCancel == buttons)
          buttonFlags = MB_OKCANCEL;
        else if (System.Windows.Forms.MessageBoxButtons.RetryCancel == buttons)
          buttonFlags = MB_RETRYCANCEL;
        else if (System.Windows.Forms.MessageBoxButtons.YesNo == buttons)
          buttonFlags = MB_YESNO;
        else if (System.Windows.Forms.MessageBoxButtons.YesNoCancel == buttons)
          buttonFlags = MB_YESNOCANCEL;

        uint iconFlags = 0; //System.Windows.Forms.MessageBoxIcon.None
        if (System.Windows.Forms.MessageBoxIcon.Asterisk == icon)
          iconFlags = MB_ICONASTERISK;
        else if (System.Windows.Forms.MessageBoxIcon.Error == icon)
          iconFlags = MB_ICONERROR;
        else if (System.Windows.Forms.MessageBoxIcon.Exclamation == icon)
          iconFlags = MB_ICONEXCLAMATION;
        else if (System.Windows.Forms.MessageBoxIcon.Hand == icon)
          iconFlags = MB_ICONHAND;
        else if (System.Windows.Forms.MessageBoxIcon.Information == icon)
          iconFlags = MB_ICONINFORMATION;
        else if (System.Windows.Forms.MessageBoxIcon.Question == icon)
          iconFlags = MB_ICONQUESTION;
        else if (System.Windows.Forms.MessageBoxIcon.Stop == icon)
          iconFlags = MB_ICONSTOP;
        else if (System.Windows.Forms.MessageBoxIcon.Warning == icon)
          iconFlags = MB_ICONWARNING;

        uint defaultButtonFlags = 0;
        if (System.Windows.Forms.MessageBoxDefaultButton.Button1 == defaultButton)
          defaultButtonFlags = MB_DEFBUTTON1;
        else if (System.Windows.Forms.MessageBoxDefaultButton.Button2 == defaultButton)
          defaultButtonFlags = MB_DEFBUTTON2;
        else if (System.Windows.Forms.MessageBoxDefaultButton.Button3 == defaultButton)
          defaultButtonFlags = MB_DEFBUTTON3;

        uint flags = buttonFlags | iconFlags | defaultButtonFlags;
        System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;
        try
        {
          int rc = UnsafeNativeMethods.RHC_RhinoMessageBox(message, title, flags);
          result = DialogResultFromInt(rc);
        }
        catch (System.EntryPointNotFoundException)
        {
          if (!Rhino.Runtime.HostUtils.RunningInRhino)
          {
            result = System.Windows.Forms.MessageBox.Show(message, title, buttons, icon, defaultButton);
          }
        }
        return result;
      }

      /// <summary>
      /// Display Rhino's color selection dialog.
      /// </summary>
      /// <param name="color">
      /// [in/out] Default color for dialog, and will receive new color if function returns true.
      /// </param>
      /// <returns>true if the color changed. false if the color has not changed or the user pressed cancel.</returns>
      public static bool ShowColorDialog(ref System.Drawing.Color color)
      {
        return ShowColorDialog(ref color, false, null);
      }

      /// <summary>
      /// Display Rhino's color selection dialog.
      /// </summary>
      /// <param name="color">
      /// [in/out] Default color for dialog, and will receive new color if function returns true.
      /// </param>
      /// <param name="includeButtonColors">
      /// Display button face and text options at top of named color list.
      /// </param>
      /// <param name="dialogTitle">The title of the dialog.</param>
      /// <returns>true if the color changed. false if the color has not changed or the user pressed cancel.</returns>
      public static bool ShowColorDialog(ref System.Drawing.Color color, bool includeButtonColors, string dialogTitle)
      {
        bool rc = false;
        try
        {
          int abgr = System.Drawing.ColorTranslator.ToWin32(color);
          rc = UnsafeNativeMethods.RHC_RhinoColorDialog(ref abgr, includeButtonColors, dialogTitle);
          if (rc)
            color = Rhino.Runtime.Interop.ColorFromWin32(abgr);
        }
        catch (EntryPointNotFoundException)
        {
          if (!Rhino.Runtime.HostUtils.RunningInRhino)
          {
            System.Windows.Forms.ColorDialog cd = new System.Windows.Forms.ColorDialog();
            cd.Color = color;
            if (cd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
              rc = true;
              color = cd.Color;
            }
          }
        }
        return rc;
      }

#if RDK_CHECKED
      /// <summary>
      /// Displays the standard modal color picker dialog for floating point colors.
      /// </summary>
      /// <param name="parent">Parent window for this dialog, should always pass this if calling from a form or user control.</param>
      /// <param name="color">The initial color to set the picker to and also accepts the user's choice.</param>
      /// <param name="allowAlpha">Specifies if the color picker should allow changes to the alpha channel or not.</param>
      /// <returns>true if a color was picked, false if the user canceled the picker dialog.</returns>
      public static bool ShowColorDialog(System.Windows.Forms.IWin32Window parent, ref Rhino.Display.Color4f color, bool allowAlpha)
      {
        if (null == parent)
          parent = RhinoApp.MainWindow();

        IntPtr hWnd = IntPtr.Zero;
        if (null != parent)
          hWnd = parent.Handle;

        Rhino.Display.Color4f c = Rhino.Display.Color4f.Empty;

        bool rc = (1 == UnsafeNativeMethods.Rdk_Globals_ShowColorPicker(hWnd, color, allowAlpha, ref c));
        if (rc)
          color = c;
        return rc;
      }
      /// <summary>
      /// Displays the standard modal color picker dialog for floating point colors.
      /// </summary>
      /// <param name="color">The initial color to set the picker to and also accepts the user's choice.</param>
      /// <param name="allowAlpha">Specifies if the color picker should allow changes to the alpha channel or not.</param>
      /// <returns>true if a color was picked, false if the user canceled the picker dialog.</returns>
      public static bool ShowColorDialog(ref Rhino.Display.Color4f color, bool allowAlpha)
      {
        return ShowColorDialog(null, ref color, allowAlpha);
      }
#endif

      /// <summary>
      /// Displays Rhino's single layer selection dialog.
      /// </summary>
      /// <param name="layerIndex">
      /// Initial layer for the dialog, and will receive selected
      /// layer if function returns DialogResult.OK.
      /// </param>
      /// <param name="dialogTitle">The dialog title.</param>
      /// <param name="showNewLayerButton">true if the new layer button will be visible.</param>
      /// <param name="showSetCurrentButton">true if the set current button will be visible.</param>
      /// <param name="initialSetCurrentState">true if the current state will be initially set.</param>
      /// <returns>A dialog result based on user choice.</returns>
      public static System.Windows.Forms.DialogResult ShowSelectLayerDialog(ref int layerIndex, string dialogTitle, bool showNewLayerButton, bool showSetCurrentButton, ref bool initialSetCurrentState)
      {
        bool rc = UnsafeNativeMethods.RHC_RhinoSelectLayerDialog(dialogTitle, ref layerIndex, showNewLayerButton, showSetCurrentButton, ref initialSetCurrentState);
        if (rc)
          return System.Windows.Forms.DialogResult.OK;
        return System.Windows.Forms.DialogResult.Cancel;
      }

      /// <summary>
      /// Displays Rhino's combo list box.
      /// </summary>
      /// <param name="title">The dialog title.</param>
      /// <param name="message">The dialog message.</param>
      /// <param name="items">A list of items to show.</param>
      /// <returns>
      /// <para>selected item.</para>
      /// <para>null if the user canceled.</para>
      /// </returns>
      public static object ShowComboListBox(string title, string message, System.Collections.IList items)
      {
        object rc = null;
        if (items != null && items.Count > 0)
        {
          ComboListBoxForm dlg = new ComboListBoxForm(title, message, items);
          if (dlg.ShowDialog(RhinoApp.MainWindow()) == System.Windows.Forms.DialogResult.OK)
            rc = dlg.SelectedItem();
        }
        return rc;
      }

      public static object ShowListBox(string title, string message, System.Collections.IList items)
      {
        return ShowListBox(title, message, items, null);
      }

      public static object ShowListBox(string title, string message, System.Collections.IList items, object selectedItem)
      {
        object rc = null;
        if (items != null && items.Count > 0)
        {
          ListBoxForm dlg = new ListBoxForm(title, message, items, selectedItem);
          if (dlg.ShowDialog(RhinoApp.MainWindow()) == System.Windows.Forms.DialogResult.OK)
            rc = dlg.SelectedItem();
        }
        return rc;
      }

      /// <summary>
      /// Displays Rhino's check list box.
      /// </summary>
      /// <param name="title">The dialog title.</param>
      /// <param name="message">The dialog message.</param>
      /// <param name="items">A list of items to show.</param>
      /// <param name="checkState">A list of true/false boolean values.</param>
      /// <returns>An array or boolean values determining if the user checked the corresponding box. On error, null.</returns>
      public static bool[] ShowCheckListBox(string title, string message, System.Collections.IList items, System.Collections.Generic.IList<bool> checkState)
      {
        bool[] rc = null;
        if (items != null && items.Count > 0 && checkState != null && checkState.Count == items.Count)
        {
          ListBoxForm dlg = new ListBoxForm(title, message, items, checkState);
          if (dlg.ShowDialog(RhinoApp.MainWindow()) == System.Windows.Forms.DialogResult.OK)
            rc = dlg.GetCheckedItemStates();
        }
        return rc;
      }

      public static System.Windows.Forms.DialogResult ShowEditBox(string title, string message, string defaultText, bool multiline, out string text)
      {
        text = String.Empty;
        System.Windows.Forms.DialogResult rc;
        if (multiline)
        {
          EditBoxForm dlg = new EditBoxForm(title, message, defaultText);
          rc = dlg.ShowDialog(RhinoApp.MainWindow());
          if (rc == System.Windows.Forms.DialogResult.OK)
            text = dlg.GetText();
        }
        else
        {
          StringBoxForm dlg = new StringBoxForm(title, message, defaultText);
          rc = dlg.ShowDialog(RhinoApp.MainWindow());
          if (rc == System.Windows.Forms.DialogResult.OK)
            text = dlg.GetText();
        }
        return rc;
      }

      /// <summary>
      /// FOR INTERNAL TESTING
      /// Ignore - this is for internal testing and will be removed.
      /// </summary>
      /// <returns>
      /// On Windows (.NET)
      /// {X=0,Y=0,Width=300,Height=126}
      ///   {X=0,Y=0,Width=284,Height=88}
      ///   {X=0,Y=0,Width=284,Height=88}
      /// {X=10,Y=9,Width=49,Height=13}
      ///   {X=0,Y=0,Width=49,Height=13}
      ///   {X=0,Y=0,Width=49,Height=13}
      /// {X=197,Y=55,Width=75,Height=23}
      ///   {X=0,Y=0,Width=75,Height=23}
      ///   {X=0,Y=0,Width=75,Height=23}
      /// {X=116,Y=55,Width=75,Height=23}
      ///   {X=0,Y=0,Width=75,Height=23}
      ///   {X=0,Y=0,Width=75,Height=23}
      /// {X=13,Y=29,Width=259,Height=20}
      ///   {X=0,Y=0,Width=255,Height=16}
      ///   {X=0,Y=0,Width=255,Height=16}
      /// </returns>
      [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
      public static System.Drawing.Rectangle[] StringBoxRects()
      {
        StringBoxForm dlg = new StringBoxForm("title", "message", "default_text");
        dlg.Show();
        dlg.Location = new System.Drawing.Point(0, 0);
        List<System.Drawing.Rectangle> rc = new List<System.Drawing.Rectangle>();
        rc.Add(dlg.Bounds);
        rc.Add(dlg.DisplayRectangle);
        rc.Add(dlg.ClientRectangle);
        for (int i = 0; i < dlg.Controls.Count; i++)
        {
          System.Windows.Forms.Control ctrl = dlg.Controls[i];
          rc.Add(ctrl.Bounds);
          rc.Add(ctrl.DisplayRectangle);
          rc.Add(ctrl.ClientRectangle);
        }
        dlg.Close();
        return rc.ToArray();
      }

      public static System.Windows.Forms.DialogResult ShowNumberBox(string title, string message, ref double number)
      {
        return ShowNumberBox(title, message, ref number, RhinoMath.UnsetValue, RhinoMath.UnsetValue);
      }

      public static System.Windows.Forms.DialogResult ShowNumberBox(string title, string message, ref double number, double minimum, double maximum)
      {
        string defaultText = String.Empty;
        if (number != RhinoMath.UnsetValue)
          defaultText = number.ToString();
        StringBoxForm dlg = new StringBoxForm(title, message, defaultText);
        dlg.SetAsNumberInput(minimum, maximum);
        System.Windows.Forms.DialogResult rc = dlg.ShowDialog(RhinoApp.MainWindow());
        if (rc == System.Windows.Forms.DialogResult.OK)
        {
          string text = dlg.GetText();
          double.TryParse(text, out number);
        }
        return rc;
      }

      public static string[] ShowPropertyListBox(string title, string message, System.Collections.IList items, IList<string> values)
      {
        if (!Runtime.HostUtils.RunningOnWindows)
          throw new NotImplementedException("Not implemented on OSX yet");

        if (null == items || null == values || items.Count < 1 || items.Count != values.Count)
          return null;

        if( Runtime.HostUtils.RunningOnWindows )
        {
          object rs = Runtime.HostUtils.GetRhinoScriptObject();
          if (rs != null)
          {
            string[] _items = new string[items.Count];
            for (int i = 0; i < items.Count; i++)
            {
              object item = items[i];
              if (item == null)
                _items[i] = String.Empty;
              else
                _items[i] = item.ToString();
            }
            string[] _values = new string[values.Count];
            values.CopyTo(_values, 0);
            for (int i = 0; i < values.Count; i++)
            {
              if (string.IsNullOrEmpty(_values[i]))
                _values[i] = " ";
            }
            if (string.IsNullOrEmpty(title))
              title = "Rhino";
            if (string.IsNullOrEmpty(message))
              message = "Items";
            object[] args = new object[] {_items, _values, title, message};
            object invoke_result = rs.GetType().InvokeMember("PropertyListBox", System.Reflection.BindingFlags.InvokeMethod, null, rs, args);
            object[] results = invoke_result as object[];
            if (results != null)
            {
              string[] rc = new string[results.Length];
              for (int i = 0; i < rc.Length; i++)
              {
                object o = results[i];
                if (o != null)
                  rc[i] = o.ToString();
              }
              return rc;
            }
          }
        }
        return null;
      }

      internal static System.Windows.Forms.DialogResult DialogResultFromInt(int val)
      {
        const int IDOK = 1;
        const int IDCANCEL = 2;
        const int IDABORT = 3;
        const int IDRETRY = 4;
        const int IDIGNORE = 5;
        const int IDYES = 6;
        const int IDNO = 7;
        System.Windows.Forms.DialogResult result = System.Windows.Forms.DialogResult.None;
        if (IDOK == val)
          result = System.Windows.Forms.DialogResult.OK;
        else if (IDCANCEL == val)
          result = System.Windows.Forms.DialogResult.Cancel;
        else if (IDABORT == val)
          result = System.Windows.Forms.DialogResult.Abort;
        else if (IDRETRY == val)
          result = System.Windows.Forms.DialogResult.Retry;
        else if (IDIGNORE == val)
          result = System.Windows.Forms.DialogResult.Ignore;
        else if (IDYES == val)
          result = System.Windows.Forms.DialogResult.Yes;
        else if (IDNO == val)
          result = System.Windows.Forms.DialogResult.No;
        return result;
      }
    }
  }
}

#endif