#pragma warning disable 1591
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Xml;
#if RHINO_SDK
using System.Windows.Forms;
#endif
// RMA_DONT_LOCALIZE (Tells the build process string parser to ignore this file)

// ReSharper disable CheckNamespace
namespace Rhino.UI
// ReSharper restore CheckNamespace
{
  sealed class LocalizationStringTable
  {
    // NOTE: we may want to use System.Collections.Specialized.StringDictionary instead
    readonly Dictionary<string, string> m_command_list = new Dictionary<string, string>();
    readonly Dictionary<string, string> m_string_list = new Dictionary<string, string>();
    readonly Dictionary<string, string> m_dialog_list = new Dictionary<string, string>();

    public Dictionary<string, string> StringList { get { return m_string_list; } }

    public Dictionary<string, string> CommandList { get { return m_command_list; } }

    public Dictionary<string, string> DialogList { get { return m_dialog_list; } }

    /// <summary>
    /// Look for XML file decorating the name with both the Locale ID as a number and a System.Globalization.CultureInfo.Name.
    /// </summary>
    private string XmlFileExists(string dir, string filename, int languageId)
    {
      string[] s_seps = { "_", "-", " ", "" };

      foreach (string t in s_seps)
      {
        string xml_path = Path.Combine(dir, String.Format("{0}{1}{2}.xml", filename, t, languageId));
        if (File.Exists(xml_path))
          return xml_path;
      }
      var ci = new CultureInfo(languageId);
// ReSharper disable LoopCanBeConvertedToQuery
      foreach (string t in s_seps)
// ReSharper restore LoopCanBeConvertedToQuery
      {
        string xml_path = Path.Combine(dir, String.Format("{0}{1}{2}.xml", filename, t, ci.Name));
        if (File.Exists(xml_path))
          return xml_path;
      }
      return null;
    }

    /// <summary>
    /// Strip trailing "[[some number]]" from end of string.
    /// </summary>
    private static string StripTrailingSquareBrackets(string s)
    {
      int i = s.LastIndexOf("[[", StringComparison.Ordinal);
      if (i < 0)
        return s;
      return s.Substring(0, i);
    }
    /// <summary>
    /// Takes an embedded resource name and checks to see if it contains ".Localization." in the name and starts
    /// or ends with the locale ID or locale culture string.
    /// </summary>
    private bool ResourceNameContainsLocaleID(string s, int languageId, string cultureName)
    {
      if (string.IsNullOrEmpty(s))
        return false;
      // Convert string to upper case so our checks can be case insensitive
      string s_upper = s.ToUpper();
      const string key = ".LOCALIZATION.";
      int index = s_upper.IndexOf(key, StringComparison.Ordinal) + key.Length;
      if (index <= s_upper.Length)
      {
        string substring = s_upper.Substring(index);
        if (s_upper.Contains(key))
        {
          // Contains the localization key
          string s_language_id = languageId.ToString(CultureInfo.InvariantCulture);
          // Check to see if it starts or ends with language ID or ends with langage id + ".xml"
          if (substring.StartsWith(s_language_id) || substring.EndsWith(s_language_id) ||
              substring.EndsWith(s_language_id + ".XML"))
            return true;
          // Check to see if it starts or ends with culture string or ends with culture string + ".xml"
          if (substring.StartsWith(cultureName) || substring.EndsWith(cultureName) ||
              substring.EndsWith(cultureName + ".XML"))
            return true;
        }
      }
      return false;
    }
    /// <summary>
    /// Looks in the specified assembly for an embedded resource that contains ".Localization." in the name and starts
    /// or ends with the locale ID or locale culture string.
    /// </summary>
    private XmlTextReader LoadFromAssemblyEmbeddedResource(Assembly assembly, int languageId)
    {
      if (null != assembly)
      {
        // Extract resource embedded in the specified assembly names
        string[] names = assembly.GetManifestResourceNames();
        {
          // Convert locale ID to culture prefix, ie: 1034 = "es-es"
          var culture_info = new CultureInfo(languageId);
          string culture_name = culture_info.Name.ToUpper();
          string xml_file = null;
          // Scan named resource list for the first item that appears to match our search
          for (int i = 0; null == xml_file && i < names.Length; i++)
            if (ResourceNameContainsLocaleID(names[i], languageId, culture_name))
              xml_file = names[i];
          if (!string.IsNullOrEmpty(xml_file))
          {
            // Resource found so extract into a stream
            var resource_stream = assembly.GetManifestResourceStream(xml_file);
            if (null != resource_stream)
            {
              var stream = new StreamReader(resource_stream);
              return new XmlTextReader(stream);
            }
          }
        }
      }
      return null;
    }
    /// <summary>
    /// Look in the assembly folder or sub folders for a XML file that starts with the locale ID or locale ID
    /// converted to culture string (something like "es-es") and if it is found attach a XmlTextReader to it.
    /// </summary>
    private XmlTextReader TextReaderFromFile(Assembly a, int languageId)
    {
      //Attempt to load the XML file. First place to look is in the same directory as the assembly
      string dir = Path.GetDirectoryName(a.Location);
      string xml_path = null;
      //
      // There was a problem with the Toolbars plug-in, it gets compiled as RhinoXmlUiDotNet.rhp then renamed
      // to Toolbars.rhp by the build process which means the assembly name is RhinoXmlUiDotNet and the file 
      // name is Toolbars.  The build process also creates and installs "Toolbars <locale>.xml" localization
      // files which were never getting found because we were only looking for files that begin with the 
      // Assembly name.  The following loop looks for files that start with the assembly file name first and
      // if not found will use the internal assembly name.
      //
      for (int x = 0; string.IsNullOrEmpty(xml_path) && x < 2; x++)
      {
        string filename;
        if (x < 1)
        {
          // First check using the actual assembly file name
          filename = Path.GetFileNameWithoutExtension(a.Location);
        }
        else
        {
          // Second, look using the assembly name
          AssemblyName assname = a.GetName();
          filename = assname.Name;
        }
        if (!string.IsNullOrEmpty(filename) && filename.EndsWith("_d", StringComparison.InvariantCultureIgnoreCase))
        {
          int start_index = filename.Length - 2;
          if (start_index >= 0)
            filename = filename.Remove(start_index);
        }
        xml_path = XmlFileExists(dir, filename, languageId);
        if (string.IsNullOrEmpty(xml_path) && !string.IsNullOrEmpty(dir))
        {
          bool found = false;
          var di = new DirectoryInfo(dir);
          var sub_directories = di.GetDirectories();
          foreach (DirectoryInfo t in sub_directories)
          {
            xml_path = XmlFileExists(t.FullName, filename, languageId);
            if (!string.IsNullOrEmpty(xml_path))
            {
              found = true;
              break;
            }
          }
          if (found)
            break;
          if (x > 0)
            return null;
        }
      }
      if (string.IsNullOrEmpty(xml_path))
        return null;
      return new XmlTextReader(xml_path);
    }
    /// <summary>
    /// Look for a XML file with the current language ID and if one is not found then look in the specified assembly
    /// for an embedded resource with the name "*.Localization.[locale]*.xml", if one is found then parse the XML
    /// and extract the strings from it.
    /// </summary>
    /// <param name="a">Check this assembly folder and its embedded resources for the specified locale XML file.</param>
    /// <param name="languageId">Locale ID to check for.</param>
    /// <returns>true if the operation was successful.</returns>
    public bool LoadFromFile(Assembly a, int languageId)
    {
      bool rc = true;
      try
      {
        XmlTextReader reader = TextReaderFromFile(a, languageId) ?? LoadFromAssemblyEmbeddedResource(a, languageId);
        if (null == reader)
          return false; // External or embedded XML file not found
        var doc = new XmlDocument();
        doc.Load(reader);
        reader.Close();

        XmlNode root = doc.DocumentElement;
        XmlNodeList command_list = null == root ? null : root.SelectNodes("RMACOMMANDNAME");
        XmlNodeList string_list = null == root ? null : root.SelectNodes("RMASTRING");
        XmlNodeList dialog_list = null == root ? null : root.SelectNodes("DIALOG");

        if (null != command_list)
        {
          const string prefix = "Command::";
          int prefix_length = prefix.Length;
          int count = command_list.Count;
          for (int i = 0; i < count; i++)
          {
            if (command_list[i].Attributes == null)
              continue;

            var en = command_list[i].Attributes["English"];
            if (en == null)
              continue;

            string key = en.Value;
            key = key.Substring(prefix_length);
            var loc = command_list[i].Attributes["Localized"];
            if (loc == null)
              continue;

            string value = command_list[i].Attributes["Localized"].Value;
            int last_index = value.LastIndexOf("::", StringComparison.Ordinal);
            if (last_index > 0)
              value = value.Substring(last_index + 2);

            // Only add to dictionary if the value has been translated
            if (0 != String.CompareOrdinal(key, value) && !m_command_list.ContainsKey(key))
              m_command_list.Add(key, StripTrailingSquareBrackets(value));
          }
        }

        if (null != string_list)
        {
          int count = string_list.Count;
          for (int i = 0; i < count; i++)
          {
            if (string_list[i].Attributes == null)
              continue;

            string key = string_list[i].Attributes["English"].Value;
            string value = string_list[i].Attributes["Localized"].Value;
            // Only add to dictionary if the value has been translated
            if (0 != String.CompareOrdinal(key, value)&& !m_string_list.ContainsKey(key))
              m_string_list.Add(key, StripTrailingSquareBrackets(value));
          }
        }
        
        if (null != dialog_list)
        {
          int dialog_count = dialog_list.Count;
          for (int current_dialog = 0; current_dialog < dialog_count; current_dialog++)
          {
            XmlNode dialog_node = dialog_list[current_dialog];
            if (dialog_node.Attributes == null)
              continue;

            XmlAttribute attrib = dialog_node.Attributes["English"];
            if (null != attrib)
            {
              string dialog_name = dialog_node.Attributes["English"].Value;
              string english_text_value = dialog_name;
              int last_index = dialog_name.LastIndexOf("::", StringComparison.Ordinal);
              if (last_index > 0)
              {
                english_text_value = dialog_name.Substring(last_index + 2);
                dialog_name = dialog_name.Substring(0, last_index);
              }
              string dialog_text_value = dialog_node.Attributes["Localized"].Value;
              last_index = dialog_text_value.LastIndexOf("::", StringComparison.Ordinal);
              if (last_index > 0)
                dialog_text_value = dialog_text_value.Substring(last_index + 2);
              // Only add to dictionary if the dialog text item has been translated
              if (!string.IsNullOrEmpty(dialog_text_value) && 0 != String.CompareOrdinal(english_text_value, dialog_text_value) && !m_dialog_list.ContainsKey(dialog_text_value))
              {
                if (m_dialog_list.ContainsKey(dialog_name))
                  System.Diagnostics.Debug.WriteLine("Dialog key exists in localization dictionary:: " + dialog_name);
                else
                  m_dialog_list.Add(dialog_name, dialog_text_value);
              }
            }
            
            XmlNodeList control_list = dialog_node.SelectNodes("CONTROL");
            if (null != control_list)
            {
              int control_count = control_list.Count;
              for (int current_control = 0; current_control < control_count; current_control++)
              {
                XmlNode control_node = control_list[current_control];
                if (control_node.Attributes == null)
                  continue;

                String key = control_node.Attributes["English"].Value;
                String value = control_node.Attributes["Localized"].Value;
                String english_value = key;
                int key_index = key.LastIndexOf("::", StringComparison.Ordinal);
                if (key_index > 0)
                {
                  english_value = key.Substring(key_index + 2);
                  key = key.Substring(0,key_index);
                }
                int value_index = value.LastIndexOf("::", StringComparison.Ordinal);
                if (value_index > 0)
                  value = value.Substring(value_index + 2);
                // Only add to dictionary if the value has been translated
                if (0 != String.CompareOrdinal(value, english_value) && !m_dialog_list.ContainsKey(key))
                  m_dialog_list.Add(key, value);
              }
            }
          }
        }
      }
#if RHINO_SDK
      catch(Exception exception)
      {
        Rhino.Runtime.HostUtils.ExceptionReport(exception);
        rc = false;
      }
#else
      catch(Exception)
      {
        rc = false;
      }
#endif

      return rc;
    }

#if RHINO_SDK
    /// <summary>
    /// Recursive helper function for LocalizeUtils.LocalizeForm.
    /// </summary>
    internal void LocalizeControlTree(string formName, string formClassName, Control ctrl, ToolTip[] tooltips)
    {
      if (formName == null || ctrl == null)
        return;

      if (!string.IsNullOrEmpty(ctrl.Text))
      {
        string key= formName + "::" + ctrl.Name + "::Text";
        string value;
        if (m_dialog_list.TryGetValue(key, out value))
          ctrl.Text = value;
        else if (!string.IsNullOrEmpty(formClassName))
        {
          key = formClassName + "::" + ctrl.Name + "::Text";
          if (m_dialog_list.TryGetValue(key, out value))
            ctrl.Text = value;
        }
      }

      // Localize child controls stored in this controls component list such as ContextMenuStrip items.
      Control[] component_controls = LocalizationUtils.GetComponentControls(ctrl);
      if (null != component_controls)
      {
        Type ctrl_type = ctrl.GetType();
        string ctrl_class_name = ctrl_type.Name;
        if (string.IsNullOrEmpty(ctrl_class_name))
          ctrl_class_name = formClassName;
        foreach (Control t in component_controls)
          LocalizeControlTree(formName, ctrl_class_name, t, tooltips);
      }

      if( null != (ctrl as ToolStrip))
        LocalizeToolStripCollection(formName, formClassName, (ctrl as ToolStrip).Items);
      else if (null != (ctrl as ListBox))
        LocalizeListBoxItems(formName, formClassName, ctrl as ListBox);
      else if (null != (ctrl as ListView))
        LocalizeListView(formName, formClassName, ctrl as ListView);
      else if (null != (ctrl as ComboBox))
        LocalizeComboBoxItems(formName, formClassName, ctrl as ComboBox);
      else
      {
        int count = ctrl.Controls.Count;
        for (int i = 0; i < count; i++)
        {
          Control c = ctrl.Controls[i];
          if (null != tooltips)
          {
            foreach (ToolTip tooltip in tooltips)
            {
              // If there is a tool top associated with the Form or UserControl being localized then
              // check to see if any of the child controls are associated with the ToolTip Component
              if (!string.IsNullOrEmpty(tooltip.GetToolTip(c)))
              {
                string[] tool_tip_text = { "::ToolTipText", "::ToolTip" };
                foreach (string t in tool_tip_text)
                {
// This child control has a tooltip so attempt to find the controls ToolTipText entry
                  // in the string table and localize it if found
                  string key = formName + "::" + c.Name + t;
                  string value;
                  if (m_dialog_list.TryGetValue(key, out value))
                  {
                    // This will clear the tool-tip prior to setting the new string, not doing this causes strange
                    // things to happen in certain Asian language systems
                    tooltip.SetToolTip(c, string.Empty);
                    tooltip.SetToolTip(c, value);
                    break;
                  }
                  if (!string.IsNullOrEmpty(formClassName))
                  {
                    key = formClassName + "::" + c.Name + t;
                    if (m_dialog_list.TryGetValue(key, out value))
                    {
                      // This will clear the tool-tip prior to setting the new string, not doing this causes strange
                      // things to happen in certain Asian language systems
                      tooltip.SetToolTip(c, string.Empty);
                      tooltip.SetToolTip(c, value);
                      break;
                    }
                  }
                }
              }
            }
          }
          LocalizeControlTree(formName, formClassName, ctrl.Controls[i], tooltips);
        }
      }
    }

    /// <summary>
    /// Recursive helper function for LocalizeUtils.LocalizeForm.
    /// </summary>
    internal void LocalizeToolStripCollection(string formName, string formClassName, ToolStripItemCollection collection)
    {
      if (null == formName || null == collection)
        return;
      int count = collection.Count;
      for (int i = 0; i < count; i++)
      {
        ToolStripItem tsi = collection[i];
        if (!string.IsNullOrEmpty(tsi.Text))
        {
          string key= formName + "::" + tsi.Name + "::Text";
          string value;
          if (m_dialog_list.TryGetValue(key, out value))
            tsi.Text = value;
          else if (!string.IsNullOrEmpty(formClassName))
          {
            key = formClassName + "::" + tsi.Name + "::Text";
            if (m_dialog_list.TryGetValue(key, out value))
              tsi.Text = value;
          }
        }
        if (!string.IsNullOrEmpty(tsi.ToolTipText))
        {
          string[] tool_tip_text = { "::ToolTipText", "::ToolTip" };
          foreach (string t in tool_tip_text)
          {
            string key = formName + "::" + tsi.Name + t;
            string value;
            if (m_dialog_list.TryGetValue(key, out value))
            {
              tsi.ToolTipText = value;
              break;
            }
            if (!string.IsNullOrEmpty(formClassName))
            {
              key = formClassName + "::" + tsi.Name + t;
              if (m_dialog_list.TryGetValue(key, out value))
              {
                tsi.ToolTipText = value;
                break;
              }
            }
          }
        }
        if (null != (tsi as ToolStripDropDownItem))
          LocalizeToolStripCollection( formName, formClassName, (tsi as ToolStripDropDownItem).DropDownItems);
      }
    }

    void LocalizeListBoxItems(string formName, string formClassName, ListBox lb)
    {
      if (null == formName || null == lb || string.IsNullOrEmpty(lb.Name))
        return;

      for (int i = 0, count = lb.Items.Count; i < count; i++)
      {
        object obj = lb.Items[i];
        if (null != obj && (null != obj as string))
        {
          var s = obj as string;
          if (!string.IsNullOrEmpty(s))
          {
            string key= formName + "::" + lb.Name + "::Items";
            if (i > 0)
              key += i.ToString(CultureInfo.InvariantCulture);
            string value;
            if (m_dialog_list.TryGetValue(key, out value))
              lb.Items[i] = value;
            else if (!string.IsNullOrEmpty(formClassName))
            {
              key = formName + "::" + formClassName + "::Items";
              if (i > 0)
                key += i.ToString(CultureInfo.InvariantCulture);
              if (m_dialog_list.TryGetValue(key, out value))
                lb.Items[i] = value;
            }
          }
        }
      }
    }

    void LocalizeListView(string formName, string formClassName, ListView lv)
    {
      if (null == formName || null == lv || string.IsNullOrEmpty(lv.Name))
        return;
      if (lv.Columns.Count < 1)
        return;
      for (int i = 0; i < 2; i++)
      {
        // First look at this control, in case this is a derived class, then get list view parent so we have a place to look for ControlHeader variables
        Control ctrl = i < 1 ? lv : lv.Parent;
        if (null != ctrl)
        {
          // Get list of fields (variables) that the ctrl class includes
          FieldInfo[] fields = ctrl.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
          {
            // Find all the ColumnHeader fields in the ctrl class and build a dictionary of the field value and its field name
            var header_fields = new Dictionary<ColumnHeader,string>();
            for (int j = 0, length = fields.Length; j < length; j++)
            {
              FieldInfo fi = fields[j];
              if (null != fi)
              {
                var ch = fi.GetValue(ctrl) as ColumnHeader;
                if (null != ch)
                  header_fields.Add(ch, fi.Name);
              }
            }
            if (header_fields.Count > 0)
            {
              // Iterate column header list
              for (int j = 0, count = lv.Columns.Count; j < count; j++)
              {
                ColumnHeader header = lv.Columns[j];
                // If this header item is not null and the text is not empty and the header object is in the control dictionary
                string control_field_name;
                if (null != header && !string.IsNullOrEmpty(header.Text) && header_fields.TryGetValue(header, out control_field_name) && !string.IsNullOrEmpty(control_field_name))
                {
                  // Look up the localized string for this column header item
                  string key = formName + "::" + control_field_name + "::Text";
                  string value;
                  if (m_dialog_list.TryGetValue(key, out value))
                    header.Text = value;
                  else if (!string.IsNullOrEmpty(formClassName))
                  {
                    key= formName + "::" + formClassName + "::" + control_field_name + "::Text";
                    if (m_dialog_list.TryGetValue(key, out value))
                    {
                      header.Text = value;
                    }
                    else
                    {
                      key = formClassName + "::" + control_field_name + "::Text";
                      if (m_dialog_list.TryGetValue(key, out value))
                        header.Text = value;
                    }
                  }
                }
              }
            }
          }
        }
      }
    }

    /// <summary>
    /// Localizes combo list items.
    /// </summary>
    /// <param name="formName">The form name.</param>
    /// <param name="formClassName">The form class name.</param>
    /// <param name="cb">A Windows Forms combo box.</param>
    public void LocalizeComboBoxItems(string formName, string formClassName, ComboBox cb)
    {
      if (null == formName || null == cb || string.IsNullOrEmpty(cb.Name))
        return;

      for (int i=0, count=cb.Items.Count; i < count; i++)
      {
        var s = cb.Items[i] as string;
        if (!string.IsNullOrEmpty(s))
        {
          string key = formName + "::" + cb.Name + "::Items";
          if (i > 0)
            key += i.ToString(CultureInfo.InvariantCulture);
          string value;
          if (m_dialog_list.TryGetValue(key, out value))
            cb.Items[i] = value;
          else if (!string.IsNullOrEmpty(formClassName))
          {
            key = formName + "::" + formClassName + "::Items";
            if (i > 0)
              key += i.ToString(CultureInfo.InvariantCulture);
            if (m_dialog_list.TryGetValue(key, out value))
              cb.Items[i] = value;
            else
            {
              key = formClassName + "::" + cb.Name + "::Items";
              if (i > 0)
                key += i.ToString(CultureInfo.InvariantCulture);
              if (m_dialog_list.TryGetValue(key, out value))
                cb.Items[i] = value;
            }
          }
        }
      }
    }
#endif
  }
}
