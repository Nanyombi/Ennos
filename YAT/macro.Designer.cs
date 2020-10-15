﻿using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace YAT
{
    partial class macro : IXmlSerializable
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private string m_buttonName = "";
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.chkSendCommand = new System.Windows.Forms.CheckBox();
            this.btnSend = new System.Windows.Forms.Button();
            this.txtCommand = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 90F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.Controls.Add(this.chkSendCommand, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.btnSend, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtCommand, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(198, 25);
            this.tableLayoutPanel1.TabIndex = 4;
            this.tableLayoutPanel1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tableLayoutPanel1_MouseDown);
            // 
            // chkSendCommand
            // 
            this.chkSendCommand.AutoSize = true;
            this.chkSendCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chkSendCommand.Location = new System.Drawing.Point(126, 3);
            this.chkSendCommand.MinimumSize = new System.Drawing.Size(50, 0);
            this.chkSendCommand.Name = "chkSendCommand";
            this.chkSendCommand.Size = new System.Drawing.Size(69, 19);
            this.chkSendCommand.TabIndex = 8;
            this.chkSendCommand.Text = "Select";
            this.chkSendCommand.UseVisualStyleBackColor = true;
            this.chkSendCommand.CheckedChanged += new System.EventHandler(this.chkSendCommand_CheckedChanged);
            this.chkSendCommand.MouseDown += new System.Windows.Forms.MouseEventHandler(this.chkSendCommand_MouseDown);
            // 
            // btnSend
            // 
            this.btnSend.AutoSize = true;
            this.btnSend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.btnSend.Location = new System.Drawing.Point(36, 0);
            this.btnSend.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnSend.Name = "btnSend";
            this.btnSend.Padding = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.btnSend.Size = new System.Drawing.Size(84, 25);
            this.btnSend.TabIndex = 4;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            this.btnSend.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnSend_MouseDown);
            // 
            // txtCommand
            // 
            this.txtCommand.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtCommand.Location = new System.Drawing.Point(3, 3);
            this.txtCommand.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.txtCommand.Name = "txtCommand";
            this.txtCommand.Size = new System.Drawing.Size(30, 20);
            this.txtCommand.TabIndex = 3;
            this.txtCommand.TextChanged += new System.EventHandler(this.txtCommand_TextChanged);
            this.txtCommand.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommand_KeyDown);
            this.txtCommand.MouseDown += new System.Windows.Forms.MouseEventHandler(this.txtCommand_MouseDown);
            // 
            // macro
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.MaximumSize = new System.Drawing.Size(0, 25);
            this.MinimumSize = new System.Drawing.Size(50, 25);
            this.Name = "macro";
            this.Size = new System.Drawing.Size(198, 25);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;        


        private System.Windows.Forms.CheckBox chkSendCommand;        
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox txtCommand;

        // Xml Serialization Infrastructure

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Macro");
            writer.WriteAttributeString("command", txtCommand.Text);
            writer.WriteAttributeString("checked", chkSendCommand.Checked.ToString());
            writer.WriteAttributeString("name" , btnSend.Text);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {            
            txtCommand.Text = reader.GetAttribute("command");
            chkSendCommand.Checked = Convert.ToBoolean(reader.GetAttribute("checked"));
            m_buttonName = reader.GetAttribute("name");

            if (m_buttonName != null)
            {
                if (m_buttonName.Length > 0)
                {
                    btnSend.Text = m_buttonName;
                }
            }

        }

        public XmlSchema GetSchema()
        {
            return (null);
        }

        public void CloneSettings(macro other)
        {
            this.chkSendCommand.Checked = other.chkSendCommand.Checked;
            this.txtCommand.Text = other.txtCommand.Text;
        }

    }
}
