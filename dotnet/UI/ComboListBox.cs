using System;
using System.Windows.Forms;

namespace Rhino.UI
{
  partial class ComboListBox : Form
  {
    public ComboListBox(string title, string message, System.Collections.IList items)
    {
      InitializeComponent();

      if (!string.IsNullOrEmpty(title))
        this.Text = title;
      if (!string.IsNullOrEmpty(message))
        this.m_lblMessage.Text = message;

      if (items != null)
      {
        object[] list = new object[items.Count];
        items.CopyTo(list, 0);
        this.m_comboBox.Items.AddRange(list);
      }
    }

    public object SelectedItem()
    {
      object rc = m_comboBox.SelectedItem;
      if (rc == null)
        rc = m_comboBox.Text;
      return rc;
    }

    private void m_list_DoubleClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      Close();
    }
  }
}
