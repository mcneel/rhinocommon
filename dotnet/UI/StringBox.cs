#pragma warning disable 1591
using System.Windows.Forms;

namespace Rhino.UI
{
  sealed partial class StringBoxForm : Form
  {
    public StringBoxForm(string title, string message, string default_text)
    {
      OnlyNumbers = false;
      InitializeComponent();

      if (!string.IsNullOrEmpty(title))
        Text = title;
      if (!string.IsNullOrEmpty(message))
        m_lblMessage.Text = message;
      if (!string.IsNullOrEmpty(default_text))
      {
        string[] lines = default_text.Split(new char[] { '\n' });
        for (int i = 0; i < lines.Length; i++)
          lines[i] = lines[i].TrimEnd(new char[] { '\r' });
        m_txtbox.Lines = lines;        
      }
      if (Runtime.HostUtils.RunningOnOSX)
      {
        System.Drawing.Point temp = m_btnOk.Location;
        m_btnOk.Location = m_btnCancel.Location;
        m_btnCancel.Location = temp;
      }
    }

    public string GetText()
    {
      return m_txtbox.Text;
    }

    private void m_txtbox_KeyPress(object sender, KeyPressEventArgs e)
    {
      if (OnlyNumbers)
      {
        if (char.IsWhiteSpace(e.KeyChar))
        {
          e.Handled = true;
        }
        else if (!char.IsControl(e.KeyChar))
        {
          double num;
          string text = m_txtbox.Text + e.KeyChar;
          e.Handled = !double.TryParse(text, out num);
        }
      }
    }

    public bool OnlyNumbers { get; set; }
  }
}
