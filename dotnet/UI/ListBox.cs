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
      m_checkedListBox.Visible = false;

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

    public ListBox(string title, string message, System.Collections.IList items, IList<bool> itemState)
    {
      InitializeComponent();
      m_list.Visible = false;
      m_checkedListBox.Visible = true;

      if (!string.IsNullOrEmpty(title))
        this.Text = title;
      if (!string.IsNullOrEmpty(message))
        this.m_lblMessage.Text = message;
      if (items != null)
      {
        object[] list = new object[items.Count];
        items.CopyTo(list, 0);
        this.m_checkedListBox.Items.AddRange(list);
        if (itemState != null && itemState.Count == items.Count)
        {
          for (int i = 0; i < items.Count; i++)
          {
            m_checkedListBox.SetItemChecked(i, itemState[i]);
          }
        }
      }
    }

    public object SelectedItem()
    {
      return m_list.SelectedItem;
    }

    public bool[] GetCheckedItemStates()
    {
      bool[] rc = null;
      int count = m_checkedListBox.Items.Count;
      if (count > 0)
      {
        rc = new bool[count];
        for (int i = 0; i < count; i++)
        {
          rc[i] = m_checkedListBox.GetItemChecked(i);
        }
      }
      return rc;
    }

    private void m_list_DoubleClick(object sender, EventArgs e)
    {
      this.DialogResult = DialogResult.OK;
      Close();
    }
  }
}
