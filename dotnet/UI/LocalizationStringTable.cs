using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace Rhino.UI
{
  class LocalizationStringTable
  {
    // NOTE: we may want to use System.Collections.Specialized.StringDictionary instead
    Dictionary<string, string> m_command_list = new Dictionary<string,string>();
    Dictionary<string, string> m_string_list = new Dictionary<string, string>();
    Dictionary<string, string> m_dialog_list = new Dictionary<string, string>();

    public LocalizationStringTable()
    {
    }

    public Dictionary<string, string> StringList
    {
      get { return this.m_string_list; }
    }

    /// <summary>
    /// Look for XML file decorating the name with both the Locale ID as a number and a System.Globalization.CultureInfo.Name
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="filename"></param>
    /// <param name="language_id"></param>
    /// <returns></returns>
    private string XmlFileExists(string dir, string filename, int language_id)
    {
      string[] sSeps = { "_", "-", " ", "" };
      string xmlPath = null;
      for (int i = 0; i < sSeps.Length; i++)
      {
        xmlPath = System.IO.Path.Combine(dir, String.Format("{0}{1}{2}.xml", filename, sSeps[i], language_id));
        if (System.IO.File.Exists(xmlPath))
          return xmlPath;
      }
      System.Globalization.CultureInfo ci = new System.Globalization.CultureInfo((int)language_id);
      for (int i = 0; i < sSeps.Length; i++)
      {
        xmlPath = System.IO.Path.Combine(dir, String.Format("{0}{1}{2}.xml", filename, sSeps[i], ci.Name));
        if (System.IO.File.Exists(xmlPath))
          return xmlPath;
      }
      return null;
    }

    /// <summary>
    /// Strip trailing "[[some number]]" from end of string
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private string StripTrailingSquareBrackets(string s)
    {
      int i = s.LastIndexOf("[[");
      if (i < 0)
        return s;
      return s.Substring(0, i);
    }

    public bool LoadFromFile(Assembly a, int language_id)
    {
      //Attempt to load the XML file. First place to look is in the same directory as the assembly
      string dir = System.IO.Path.GetDirectoryName(a.Location);
      string xmlPath = null;
      //
      // There was a problem with the Toolbars plug-in, it gets compiled as RhinoXmlUiDotNet.rhp then renamed
      // to Toolbars.rhp by the build process which means the assembly name is RhinoXmlUiDotNet and the file 
      // name is Toolbars.  The build process also creates and installs "Toolbars <locale>.xml" localization
      // files which were never getting found because we were only looking for files that begin with the 
      // Assembly name.  The following loop looks for files that start with the assembly file name first and
      // if not found will use the internal assembly name.
      //
      for (int x = 0; string.IsNullOrEmpty(xmlPath) && x < 2; x++)
      {
        string filename = null;
        if (x < 1)
        {
          // First check using the actual assembly file name
          filename = System.IO.Path.GetFileNameWithoutExtension(a.Location);
        }
        else
        {
          // Second, look using the assembly name
          AssemblyName assname = a.GetName();
          filename = null == assname ? null : assname.Name;
        }
        if (!string.IsNullOrEmpty(filename) && filename.EndsWith("_d", System.StringComparison.InvariantCultureIgnoreCase))
        {
          int start_index = filename.Length - 2;
          if (start_index >= 0)
            filename = filename.Remove(start_index);
        }
        xmlPath = this.XmlFileExists(dir, filename, language_id);
        if (string.IsNullOrEmpty(xmlPath))
        {
          bool found = false;
          System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(dir);
          System.IO.DirectoryInfo[] sub_directories = di.GetDirectories();
          for (int i = 0; i < sub_directories.Length; i++)
          {
            xmlPath = this.XmlFileExists(sub_directories[i].FullName, filename, language_id);
            if (!string.IsNullOrEmpty(xmlPath))
            {
              found = true;
              break;
            }
          }
          if (found)
            break;
          else if (x > 0 && !found)
            return false;
        }
      }
      bool rc = true;
      try
      {
        XmlTextReader reader = new XmlTextReader(xmlPath);
        XmlDocument doc = new XmlDocument();
        doc.Load(reader);
        reader.Close();

        XmlNode root = doc.DocumentElement;
        XmlNodeList command_list = null == root ? null : root.SelectNodes("RMACOMMANDNAME");
        XmlNodeList string_list = null == root ? null : root.SelectNodes("RMASTRING");
        XmlNodeList dialog_list = null == root ? null : root.SelectNodes("DIALOG");

        if (null != command_list)
        {
          string prefix = "Command::";
          int prefix_length = prefix.Length;
          int count = command_list.Count;
          for (int i = 0; i < count; i++)
          {
            string key = command_list[i].Attributes["English"].Value;
            key = key.Substring(prefix_length);
            string value = command_list[i].Attributes["Localized"].Value;
            int last_index = value.LastIndexOf("::");
            if (last_index > 0)
              value = value.Substring(last_index + 2);
            // Only add to dictionary if the value has been translated
            if (0 != string.Compare(key, value) && !m_command_list.ContainsKey(key))
              m_command_list.Add(key, this.StripTrailingSquareBrackets(value));
          }
        }

        if (null != string_list)
        {
          int count = string_list.Count;
          for (int i = 0; i < count; i++)
          {
            string key = string_list[i].Attributes["English"].Value;
            string value = string_list[i].Attributes["Localized"].Value;
            // Only add to dictionary if the value has been translated
            if (0 != string.Compare(key, value)&& !m_string_list.ContainsKey(key))
              m_string_list.Add(key, this.StripTrailingSquareBrackets(value));
          }
        }
        
        if (null != dialog_list)
        {
          int dialog_count = dialog_list.Count;
          for (int current_dialog = 0; current_dialog < dialog_count; current_dialog++)
          {
            XmlNode dialog_node = dialog_list[current_dialog];
            XmlAttribute attrib = dialog_node.Attributes["English"];
            if (null != attrib)
            {
              string dialog_name = dialog_node.Attributes["English"].Value;
              string english_text_value = dialog_name;
              int last_index = dialog_name.LastIndexOf("::");
              if (last_index > 0)
              {
                english_text_value = dialog_name.Substring(last_index + 2);
                dialog_name = dialog_name.Substring(0, last_index);
              }
              string dialog_text_value = dialog_node.Attributes["Localized"].Value;
              last_index = dialog_text_value.LastIndexOf("::");
              if (last_index > 0)
                dialog_text_value = dialog_text_value.Substring(last_index + 2);
              // Only add to dictionary if the dialog text item has been translated
              if (!string.IsNullOrEmpty(dialog_text_value) && 0 != string.Compare(english_text_value, dialog_text_value) && !m_dialog_list.ContainsKey(dialog_text_value))
                m_dialog_list.Add(dialog_name, dialog_text_value);
            }
            
            XmlNodeList control_list = dialog_node.SelectNodes("CONTROL");
            if (null != control_list)
            {
              int control_count = control_list.Count;
              for (int current_control = 0; current_control < control_count; current_control++)
              {
                XmlNode control_node = control_list[current_control];
                String key = control_node.Attributes["English"].Value;
                String value = control_node.Attributes["Localized"].Value;
                String english_value = key;
                int key_index = key.LastIndexOf("::");
                if (key_index > 0)
                {
                  english_value = key.Substring(key_index + 2);
                  key = key.Substring(0,key_index);
                }
                int value_index = value.LastIndexOf("::");
                if (value_index > 0)
                  value = value.Substring(value_index + 2);
                // Only add to dictionary if the value has been translated
                if (0 != string.Compare(value, english_value) && !m_dialog_list.ContainsKey(key))
                  m_dialog_list.Add(key, value);
              }
            }
          }
        }
      }
      catch(Exception exception)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(exception);
        rc = false;
      }

      return rc;
    }

    /// <summary>
    /// Recursive helper function for LocalizeUtils.LocalizeForm.
    /// </summary>
    /// <param name="form_name"></param>
    /// <param name="form_class_name"></param>
    /// <param name="ctrl"></param>
    internal void LocalizeControlTree(string form_name, string form_class_name, Control ctrl)
    {
      if (form_name == null || ctrl == null)
        return;

      if (!string.IsNullOrEmpty(ctrl.Text))
      {
        string key= form_name + "::" + ctrl.Name + "::Text";
        string value = null;
        if (this.m_dialog_list.TryGetValue(key, out value))
          ctrl.Text = value;
        else if (!string.IsNullOrEmpty(form_class_name))
        {
          key = form_class_name + "::" + ctrl.Name + "::Text";
          if (m_dialog_list.TryGetValue(key, out value))
            ctrl.Text = value;
        }
      }

      Control context_menu = ctrl.ContextMenuStrip;
      if (null != context_menu)
      {
        string ctrl_class_name = string.Empty;
        Type ctrl_type = ctrl.GetType();
        if (null != ctrl_type)
          ctrl_class_name = ctrl_type.Name;
        if (string.IsNullOrEmpty(ctrl_class_name))
          ctrl_class_name = form_class_name;
        this.LocalizeControlTree(form_name, ctrl_class_name, context_menu);
      }

      if( null != (ctrl as ToolStrip))
        this.LocalizeToolStripCollection(form_name, form_class_name, (ctrl as ToolStrip).Items);
      else if (null != (ctrl as ListBoxForm))
        this.LocalizeListBoxItems(form_name, form_class_name, ctrl as ListBox);
      else if (null != (ctrl as ListView))
        this.LocalizeListView(form_name, form_class_name, ctrl as ListView);
      else if (null != (ctrl as ComboBox))
        this.LocalizeComboBoxItems(form_name, form_class_name, ctrl as ComboBox);
      else
      {
        int count = ctrl.Controls.Count;
        for (int i = 0; i < count; i++)
          this.LocalizeControlTree(form_name, form_class_name, ctrl.Controls[i]);
      }
    }

    /// <summary>
    /// Recursive helper function for LocalizeUtils.LocalizeForm.
    /// </summary>
    /// <param name="form_name"></param>
    /// <param name="form_class_name"></param>
    /// <param name="collection"></param>
    internal void LocalizeToolStripCollection(string form_name, string form_class_name, ToolStripItemCollection collection)
    {
      if (null == form_name || null == collection)
        return;
      int count = collection.Count;
      for (int i = 0; i < count; i++)
      {
        ToolStripItem tsi = collection[i];
        if (!string.IsNullOrEmpty(tsi.Text))
        {
          string key= form_name + "::" + tsi.Name + "::Text";
          string value=null;
          if (this.m_dialog_list.TryGetValue(key, out value))
            tsi.Text = value;
          else if (!string.IsNullOrEmpty(form_class_name))
          {
            key = form_class_name + "::" + tsi.Name + "::Text";
            if (this.m_dialog_list.TryGetValue(key, out value))
              tsi.Text = value;
          }
        }
        if (!string.IsNullOrEmpty(tsi.ToolTipText))
        {
          string key = form_name + "::" + tsi.Name + "::ToolTipText";
          string value = null;
          if (this.m_dialog_list.TryGetValue(key, out value))
            tsi.ToolTipText = value;
          else if (!string.IsNullOrEmpty(form_class_name))
          {
            key = form_class_name + "::" + tsi.Name + "::ToolTipText";
            if (this.m_dialog_list.TryGetValue(key, out value))
              tsi.ToolTipText = value;
          }
        }
        if (null != (tsi as ToolStripDropDownItem))
          LocalizeToolStripCollection( form_name, form_class_name, (tsi as ToolStripDropDownItem).DropDownItems);
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="form_name"></param>
    /// <param name="form_class_name"></param>
    /// <param name="lb"></param>
    void LocalizeListBoxItems(string form_name, string form_class_name, ListBox lb)
    {
      if (null == form_name || null == lb || string.IsNullOrEmpty(lb.Name))
        return;

      for (int i = 0, count = lb.Items.Count; i < count; i++)
      {
        object obj = lb.Items[i];
        if (null != obj && (null != obj as string))
        {
          string s = obj as string;
          if (!string.IsNullOrEmpty(s))
          {
            string key= form_name + "::" + lb.Name + "::Items";
            if (i > 0)
              key += i.ToString();
            string value = null;
            if (this.m_dialog_list.TryGetValue(key, out value))
              lb.Items[i] = value;
            else if (!string.IsNullOrEmpty(form_class_name))
            {
              key = form_name + "::" + form_class_name + "::Items";
              if (i > 0)
                key += i.ToString();
              if (this.m_dialog_list.TryGetValue(key, out value))
                lb.Items[i] = value;
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="form_name"></param>
    /// <param name="form_class_name"></param>
    /// <param name="lv"></param>
    void LocalizeListView(string form_name, string form_class_name, ListView lv)
    {
      if (null == form_name || null == lv || string.IsNullOrEmpty(lv.Name))
        return;
      if (lv.Columns.Count < 1)
        return;
      for (int i = 0; i < 2; i++)
      {
        // First look at this control, in case this is a derived class, then get list view parent so we have a place to look for ControlHeader variables
        Control ctrl = i < 1 ? lv : lv.Parent;
        if (null != ctrl && null != ctrl.GetType())
        {
          // Get list of fields (variables) that the ctrl class includes
          FieldInfo[] fields = ctrl.GetType().GetFields((BindingFlags)(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
          if (null != fields)
          {
            // Find all the ColumnHeader fields in the ctrl class and build a dictionary of the field value and its field name
            Dictionary<ColumnHeader,string> header_fields = new Dictionary<ColumnHeader,string>();
            for (int j = 0, length = fields.Length; j < length; j++)
            {
              FieldInfo fi = fields[j];
              if (null != fi)
              {
                ColumnHeader ch = fi.GetValue(ctrl) as ColumnHeader;
                if (null != ch)
                  header_fields.Add(ch, fi.Name);
              }
            }
            if (header_fields.Count > 0)
            {
              // Iterate column header list
              string control_field_name = string.Empty;
              for (int j = 0, count = lv.Columns.Count; j < count; j++)
              {
                ColumnHeader header = lv.Columns[j];
                // If this header item is not null and the text is not empty and the header object is in the control dictionary
                if (null != header && !string.IsNullOrEmpty(header.Text) && header_fields.TryGetValue(header, out control_field_name) && !string.IsNullOrEmpty(control_field_name))
                {
                  // Look up the localized string for this column header item
                  string key = form_name + "::" + control_field_name + "::Text";
                  string value = string.Empty;
                  if (this.m_dialog_list.TryGetValue(key, out value))
                    header.Text = value;
                  else if (!string.IsNullOrEmpty(form_class_name))
                  {
                    key= form_name + "::" + form_class_name + "::" + control_field_name + "::Text";
                    if (this.m_dialog_list.TryGetValue(key, out value))
                      header.Text = value;
                  }
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="form_name"></param>
    /// <param name="form_class_name"></param>
    /// <param name="cb"></param>
    void LocalizeComboBoxItems(string form_name, string form_class_name, ComboBox cb)
    {
      if (null == form_name || null == cb || string.IsNullOrEmpty(cb.Name))
        return;

      for (int i=0, count=cb.Items.Count; i < count; i++)
      {
        string s = cb.Items[i] as string;
        if (!string.IsNullOrEmpty(s))
        {
          string key = form_name + "::" + cb.Name + "::Items";
          if (i > 0)
            key += i.ToString();
          string value = null;
          if (this.m_dialog_list.TryGetValue(key, out value))
            cb.Items[i] = value;
          else if (!string.IsNullOrEmpty(form_class_name))
          {
            key = form_name + "::" + form_class_name + "::Items";
            if (i > 0)
              key += i.ToString();
            if (this.m_dialog_list.TryGetValue(key, out value))
              cb.Items[i] = value;
            else
            {
              key = form_class_name + "::" + cb.Name + "::Items";
              if (i > 0)
                key += i.ToString();
              if (this.m_dialog_list.TryGetValue(key, out value))
                cb.Items[i] = value;
            }
          }
        }
      }
    }
  }
}
