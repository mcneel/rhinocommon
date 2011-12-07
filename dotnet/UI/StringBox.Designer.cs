namespace Rhino.UI
{
  partial class StringBoxForm
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StringBoxForm));
      this.m_btnOk = new System.Windows.Forms.Button();
      this.m_btnCancel = new System.Windows.Forms.Button();
      this.m_txtbox = new System.Windows.Forms.TextBox();
      this.m_lblMessage = new System.Windows.Forms.Label();
      this.SuspendLayout();
      // 
      // m_btnOk
      // 
      this.m_btnOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.m_btnOk.Location = new System.Drawing.Point(116, 55);
      this.m_btnOk.Name = "m_btnOk";
      this.m_btnOk.Size = new System.Drawing.Size(75, 23);
      this.m_btnOk.TabIndex = 1;
      this.m_btnOk.Text = "OK";
      this.m_btnOk.UseVisualStyleBackColor = true;
      // 
      // m_btnCancel
      // 
      this.m_btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.m_btnCancel.Location = new System.Drawing.Point(197, 55);
      this.m_btnCancel.Name = "m_btnCancel";
      this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
      this.m_btnCancel.TabIndex = 2;
      this.m_btnCancel.Text = "Cancel";
      this.m_btnCancel.UseVisualStyleBackColor = true;
      // 
      // m_txtbox
      // 
      this.m_txtbox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.m_txtbox.Location = new System.Drawing.Point(13, 29);
      this.m_txtbox.Name = "m_txtbox";
      this.m_txtbox.Size = new System.Drawing.Size(259, 20);
      this.m_txtbox.TabIndex = 0;
      this.m_txtbox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.m_txtbox_KeyPress);
      // 
      // m_lblMessage
      // 
      this.m_lblMessage.AutoSize = true;
      this.m_lblMessage.Location = new System.Drawing.Point(10, 9);
      this.m_lblMessage.Name = "m_lblMessage";
      this.m_lblMessage.Size = new System.Drawing.Size(80, 13);
      this.m_lblMessage.TabIndex = 3;
      this.m_lblMessage.Text = "Enter some text";
      // 
      // StringBoxForm
      // 
      this.AcceptButton = this.m_btnOk;
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.CancelButton = this.m_btnCancel;
      this.ClientSize = new System.Drawing.Size(284, 88);
      this.Controls.Add(this.m_lblMessage);
      this.Controls.Add(this.m_btnCancel);
      this.Controls.Add(this.m_btnOk);
      this.Controls.Add(this.m_txtbox);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "StringBoxForm";
      this.Text = "StringBox";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox m_txtbox;
    private System.Windows.Forms.Label m_lblMessage;
    private System.Windows.Forms.Button m_btnOk;
    private System.Windows.Forms.Button m_btnCancel;
  }
}