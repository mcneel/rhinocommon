using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace examples_cs
{
  partial class OptionsControl : UserControl
  {
    public OptionsControl()
    {
      InitializeComponent();
    }
  }

  class CustomOptionPage : Rhino.UI.OptionsDialogPage
  {
    OptionsControl m_control = new OptionsControl();
    public CustomOptionPage()
      : base("CS_OptionPage")
    { }

    public override Control PageControl
    {
      get { return m_control; }
    }
  }
}
