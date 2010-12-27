using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Rhino.UI
{
  partial class ListBox : Form
  {
    public ListBox(string title, string message, System.Collections.IList items)
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
        this.m_list.Items.AddRange(list);
      }
    }

    public object SelectedItem()
    {
      return m_list.SelectedItem;
    }

    private void m_list_DoubleClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      Close();
    }
  }
}
