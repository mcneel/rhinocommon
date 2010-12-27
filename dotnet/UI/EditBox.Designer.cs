namespace Rhino.UI
{
  partial class EditBox
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
      System.Windows.Forms.Button m_btnOk;
      System.Windows.Forms.Button m_btnCancel;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditBox));
      this.m_txtbox = new System.Windows.Forms.TextBox();
      this.m_lblMessage = new System.Windows.Forms.Label();
      m_btnOk = new System.Windows.Forms.Button();
      m_btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // m_txtbox
      // 
      this.m_txtbox.Location = new System.Drawing.Point(13, 29);
      this.m_txtbox.Multiline = true;
      this.m_txtbox.Name = "m_txtbox";
      this.m_txtbox.Size = new System.Drawing.Size(259, 192);
      this.m_txtbox.TabIndex = 0;
      // 
      // m_btnOk
      // 
      m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_btnOk.Location = new System.Drawing.Point(116, 227);
      m_btnOk.Name = "m_btnOk";
      m_btnOk.Size = new System.Drawing.Size(75, 23);
      m_btnOk.TabIndex = 1;
      m_btnOk.Text = "OK";
      m_btnOk.UseVisualStyleBackColor = true;
      // 
      // m_btnCancel
      // 
      m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_btnCancel.Location = new System.Drawing.Point(197, 227);
      m_btnCancel.Name = "m_btnCancel";
      m_btnCancel.Size = new System.Drawing.Size(75, 23);
      m_btnCancel.TabIndex = 2;
      m_btnCancel.Text = "Cancel";
      m_btnCancel.UseVisualStyleBackColor = true;
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
      // EditBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(284, 262);
      this.Controls.Add(this.m_lblMessage);
      this.Controls.Add(m_btnCancel);
      this.Controls.Add(m_btnOk);
      this.Controls.Add(this.m_txtbox);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "EditBox";
      this.Text = "EditBox";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TextBox m_txtbox;
    private System.Windows.Forms.Label m_lblMessage;
  }
}