#pragma warning disable 1591
using System.Windows.Forms;

namespace Rhino.UI
{
  partial class EditBoxForm : Form
  {
    public EditBoxForm(string title, string message, string default_text)
    {
      InitializeComponent();

      if (!string.IsNullOrEmpty(title))
        this.Text = title;
      if (!string.IsNullOrEmpty(message))
        m_lblMessage.Text = message;
      if (!string.IsNullOrEmpty(default_text))
      {
        string[] lines = default_text.Split(new char[] { '\n' });
        for (int i = 0; i < lines.Length; i++)
          lines[i] = lines[i].TrimEnd(new char[] { '\r' });
        m_txtbox.Lines = lines;        
      }
    }

    public string GetText()
    {
      return m_txtbox.Text;
    }
  }
}
