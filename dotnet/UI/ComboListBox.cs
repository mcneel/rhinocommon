#pragma warning disable 1591
using System.Windows.Forms;

namespace Rhino.UI
{
  sealed partial class ComboListBoxForm : Form
  {
    public ComboListBoxForm(string title, string message, System.Collections.IList items)
    {
      InitializeComponent();

      if (!string.IsNullOrEmpty(title))
        Text = title;
      if (!string.IsNullOrEmpty(message))
        m_lblMessage.Text = message;

      if (items != null)
      {
        object[] list = new object[items.Count];
        items.CopyTo(list, 0);
        m_comboBox.Items.AddRange(list);
      }
    }

    public object SelectedItem()
    {
      object rc = m_comboBox.SelectedItem ?? m_comboBox.Text;
      return rc;
    }
  }
}
