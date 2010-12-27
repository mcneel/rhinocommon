namespace Rhino.UI
{
  partial class ComboListBox
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComboListBox));
      this.m_lblMessage = new System.Windows.Forms.Label();
      this.m_comboBox = new System.Windows.Forms.ComboBox();
      m_btnOk = new System.Windows.Forms.Button();
      m_btnCancel = new System.Windows.Forms.Button();
      this.SuspendLayout();
      // 
      // m_lblMessage
      // 
      this.m_lblMessage.AutoSize = true;
      this.m_lblMessage.Location = new System.Drawing.Point(12, 9);
      this.m_lblMessage.Name = "m_lblMessage";
      this.m_lblMessage.Size = new System.Drawing.Size(60, 13);
      this.m_lblMessage.TabIndex = 1;
      this.m_lblMessage.Text = "Select Item";
      // 
      // m_btnOk
      // 
      m_btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
      m_btnOk.Location = new System.Drawing.Point(64, 237);
      m_btnOk.Name = "m_btnOk";
      m_btnOk.Size = new System.Drawing.Size(75, 23);
      m_btnOk.TabIndex = 2;
      m_btnOk.Text = "OK";
      m_btnOk.UseVisualStyleBackColor = true;
      // 
      // m_btnCancel
      // 
      m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      m_btnCancel.Location = new System.Drawing.Point(145, 237);
      m_btnCancel.Name = "m_btnCancel";
      m_btnCancel.Size = new System.Drawing.Size(75, 23);
      m_btnCancel.TabIndex = 3;
      m_btnCancel.Text = "Cancel";
      m_btnCancel.UseVisualStyleBackColor = true;
      // 
      // m_comboBox
      // 
      this.m_comboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.Simple;
      this.m_comboBox.FormattingEnabled = true;
      this.m_comboBox.Location = new System.Drawing.Point(13, 26);
      this.m_comboBox.Name = "m_comboBox";
      this.m_comboBox.Size = new System.Drawing.Size(205, 207);
      this.m_comboBox.TabIndex = 4;
      // 
      // ComboListBox
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.ClientSize = new System.Drawing.Size(230, 272);
      this.Controls.Add(this.m_comboBox);
      this.Controls.Add(m_btnCancel);
      this.Controls.Add(m_btnOk);
      this.Controls.Add(this.m_lblMessage);
      this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ComboListBox";
      this.Text = "ComboListBox";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.Label m_lblMessage;
    private System.Windows.Forms.ComboBox m_comboBox;
  }
}