using System;
using System.Collections.Generic;

namespace Rhino
{
  namespace UI
  {
    public static class Dialogs
    {
      /// <summary>
      /// Destroy the splash screen if it is being displayed
      /// </summary>
      public static void KillSplash()
      {
        UnsafeNativeMethods.RHC_RhinoKillSplash();
      }

      /// <summary>
      /// Hides a form, calls a provided function, and then shows the form. This works for
      /// modal forms. Useful for selecting objects or getting points while a modal dialog
      /// is running
      /// </summary>
      /// <param name="form"></param>
      /// <param name="pickFunction"></param>
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
        System.Windows.Forms.DialogResult rc = System.Windows.Forms.DialogResult.None;
        System.Windows.Forms.Button accept = form.AcceptButton as System.Windows.Forms.Button;
        System.Windows.Forms.Button cancel = form.CancelButton as System.Windows.Forms.Button;
        if (accept!=null && cancel!=null)
        {
          accept.Click += SemiModalOkClick;
          cancel.Click += SemiModalCancelClick;
          Rhino.Input.Custom.GetString gs = new Rhino.Input.Custom.GetString();
          gs.SetCommandPrompt("Press escape to cancel");
          form.Show(RhinoApp.MainWindow());
          gs.Get();
          rc = System.Windows.Forms.DialogResult.Cancel;
          if (gs.CommandResult() == Rhino.Commands.Result.Success && gs.StringResult() == "ok")
            rc = System.Windows.Forms.DialogResult.OK;
          form.DialogResult = rc;
          form.Close();
        }
        return rc;
      }

      static void SemiModalOkClick(object sender, EventArgs e)
      {
        RhinoApp.SendKeystrokes("ok", true);
      }

      static void SemiModalCancelClick(object sender, EventArgs e)
      {
        RhinoApp.SendKeystrokes("", true);
      }

      /// <summary>
      /// Display a text dialog similar to the dialog used for the "What" command
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
        int rc = UnsafeNativeMethods.RHC_RhinoMessageBox(message, title, flags);
        System.Windows.Forms.DialogResult result = DialogResultFromInt(rc);
        return result;
      }

      /// <summary>
      /// Display Rhino's color selection dialog.
      /// </summary>
      /// <param name="color">
      /// [in/out] Default color for dialog, and will receive new color if function returns true.
      /// </param>
      /// <returns>True if the color changed. False if the color has not changed or the user pressed cancel.</returns>
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
      /// <param name="dialogTitle">The title of the dialog</param>
      /// <returns>True if the color changed. False if the color has not changed or the user pressed cancel</returns>
      public static bool ShowColorDialog(ref System.Drawing.Color color, bool includeButtonColors, string dialogTitle)
      {
        int argb = color.ToArgb();
        bool rc = UnsafeNativeMethods.RHC_RhinoColorDialog(ref argb, includeButtonColors, dialogTitle);
        if (rc)
        {
          color = System.Drawing.Color.FromArgb(argb);
        }
        return rc;
      }

      public static System.Windows.Forms.DialogResult ShowSelectLayerDialog(ref int layerIndex, string dialogTitle, bool showNewLayerButton, bool showSetCurrentButton, ref bool initialSetCurrentState)
      {
        bool rc = UnsafeNativeMethods.RHC_RhinoSelectLayerDialog(dialogTitle, ref layerIndex, showNewLayerButton, showSetCurrentButton, ref initialSetCurrentState);
        if (rc)
          return System.Windows.Forms.DialogResult.OK;
        return System.Windows.Forms.DialogResult.Cancel;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="title"></param>
      /// <param name="message"></param>
      /// <param name="items"></param>
      /// <returns>
      /// selected item
      /// null if the user canceled
      /// </returns>
      public static object ShowComboListBox(string title, string message, System.Collections.IList items)
      {
        object rc = null;
        if (items != null && items.Count > 0)
        {
          ComboListBox dlg = new ComboListBox(title, message, items);
          if (dlg.ShowDialog(RhinoApp.MainWindow()) == System.Windows.Forms.DialogResult.OK)
            rc = dlg.SelectedItem();
        }
        return rc;
      }

      public static object ShowListBox(string title, string message, System.Collections.IList items)
      {
        object rc = null;
        if (items != null && items.Count > 0)
        {
          ListBox dlg = new ListBox(title, message, items);
          if (dlg.ShowDialog(RhinoApp.MainWindow()) == System.Windows.Forms.DialogResult.OK)
            rc = dlg.SelectedItem();
        }
        return rc;
      }

      /// <summary>
      /// 
      /// </summary>
      /// <param name="title"></param>
      /// <param name="message"></param>
      /// <param name="items"></param>
      /// <param name="checkState"></param>
      /// <returns></returns>
      public static bool[] ShowCheckListBox(string title, string message, System.Collections.IList items, System.Collections.Generic.IList<bool> checkState)
      {
        bool[] rc = null;
        if (items != null && items.Count > 0 && checkState != null && checkState.Count == items.Count)
        {
          ListBox dlg = new ListBox(title, message, items, checkState);
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
          EditBox dlg = new EditBox(title, message, defaultText);
          rc = dlg.ShowDialog(RhinoApp.MainWindow());
          if (rc == System.Windows.Forms.DialogResult.OK)
            text = dlg.GetText();
        }
        else
        {
          StringBox dlg = new StringBox(title, message, defaultText);
          rc = dlg.ShowDialog(RhinoApp.MainWindow());
          if (rc == System.Windows.Forms.DialogResult.OK)
            text = dlg.GetText();
        }
        return rc;
      }

      public static System.Windows.Forms.DialogResult ShowNumberBox(string title, string message, ref double number)
      {
        string defaultText = String.Empty;
        if( number != RhinoMath.UnsetValue )
          defaultText = number.ToString();
        StringBox dlg = new StringBox(title, message, defaultText);
        dlg.OnlyNumbers = true;
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
