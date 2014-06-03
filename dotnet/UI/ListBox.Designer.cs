#if RHINO_SDK
namespace Rhino.UI
{
  partial class ListBoxForm
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      System.Windows.Forms.Button m_btnCancel;
      System.Windows.Forms.Button m_btnOk;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ListBoxForm));
      this.m_lblMessage = new System.Windows.Forms.Label();
      this.m_list = new System.Windows.Forms.ListBox();
      this.m_checkedListBox = new System.Windows.Forms.CheckedListBox();
      m_btnCancel = new System.Windows.Forms.Button();
      m_btnOk = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // m_btnCancel
      // 
      m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_btnCancel.Location = new System.Drawing.Point(143, 234);
      m_btnCancel.Name = "m_btnCancel";
      m_btnCancel.Size = new System.Drawing.Size(75, 23);
      m_btnCancel.TabIndex = 7;
      m_btnCancel.Text = "Cancel";
      m_btnCancel.UseVisualStyleBackColor = true;
      // 
      // m_btnOk
      // 
      m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_btnOk.Location = new System.Drawing.Point(62, 234);
      m_btnOk.Name = "m_btnOk";
      m_btnOk.Size = new System.Drawing.Size(75, 23);
      m_btnOk.TabIndex = 6;
      m_btnOk.Text = "OK";
      m_btnOk.UseVisualStyleBackColor = true;
      // 
      // m_lblMessage
      // 
      this.m_lblMessage.AutoSize = true;
      this.m_lblMessage.Location = new System.Drawing.Point(10, 6);
      this.m_lblMessage.Name = "m_lblMessage";
      this.m_lblMessage.Size = new System.Drawing.Size(60, 13);
      this.m_lblMessage.TabIndex = 5;
      this.m_lblMessage.Text = "Select Item";
      // 
      // m_list
      // 
      this.m_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_list.FormattingEnabled = true;
      this.m_list.Location = new System.Drawing.Point(13, 26);
      this.m_list.Name = "m_list";
      this.m_list.Size = new System.Drawing.Size(205, 199);
      this.m_list.TabIndex = 4;
      // 
      // m_checkedListBox
      // 
      this.m_checkedListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_checkedListBox.CheckOnClick = true;
      this.m_checkedListBox.FormattingEnabled = true;
      this.m_checkedListBox.Location = new System.Drawing.Point(13, 26);
      this.m_checkedListBox.Name = "m_checkedListBox";
      this.m_checkedListBox.Size = new System.Drawing.Size(205, 199);
      this.m_checkedListBox.TabIndex = 8;
      this.m_checkedListBox.Visible = false;
      // 
      // ListBoxForm
      // 
      this.AcceptButton = m_btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = m_btnCancel;
      this.ClientSize = new System.Drawing.Size(228, 262);
      this.Controls.Add(this.m_checkedListBox);
      this.Controls.Add(m_btnCancel);
      this.Controls.Add(m_btnOk);
      this.Controls.Add(this.m_lblMessage);
      this.Controls.Add(this.m_list);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ListBoxForm";
      this.Text = "ListBox";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label m_lblMessage;
    private System.Windows.Forms.ListBox m_list;
    private System.Windows.Forms.CheckedListBox m_checkedListBox;
  }
}
#endif