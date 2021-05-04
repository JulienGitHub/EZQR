using System;

namespace QRReader
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.listView = new System.Windows.Forms.ListView();
            this.buttonCopy = new System.Windows.Forms.Button();
            this.videoSourcePlayer1 = new AForge.Controls.VideoSourcePlayer();
            this.webcamButton = new System.Windows.Forms.Button();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.findCam = new System.Windows.Forms.Button();
            this.clearList = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.infoButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // listView
            // 
            this.listView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView.FullRowSelect = true;
            this.listView.GridLines = true;
            this.listView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView.HideSelection = false;
            this.listView.Location = new System.Drawing.Point(475, 52);
            this.listView.Name = "listView";
            this.listView.Size = new System.Drawing.Size(251, 237);
            this.listView.TabIndex = 4;
            this.listView.UseCompatibleStateImageBehavior = false;
            this.listView.View = System.Windows.Forms.View.Details;
            this.listView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ItemSelectionChanged);
            // 
            // buttonCopy
            // 
            this.buttonCopy.Location = new System.Drawing.Point(475, 295);
            this.buttonCopy.Name = "buttonCopy";
            this.buttonCopy.Size = new System.Drawing.Size(251, 37);
            this.buttonCopy.TabIndex = 16;
            this.buttonCopy.Text = "Copy To Clipboard";
            this.buttonCopy.Click += new System.EventHandler(this.buttonCopy_Click);
            // 
            // videoSourcePlayer1
            // 
            this.videoSourcePlayer1.Location = new System.Drawing.Point(12, 3);
            this.videoSourcePlayer1.Name = "videoSourcePlayer1";
            this.videoSourcePlayer1.Size = new System.Drawing.Size(457, 329);
            this.videoSourcePlayer1.TabIndex = 9;
            this.videoSourcePlayer1.Text = "videoSourcePlayer1";
            this.videoSourcePlayer1.VideoSource = null;
            this.videoSourcePlayer1.NewFrame += new AForge.Controls.VideoSourcePlayer.NewFrameHandler(this.videoSourcePlayer1_NewFrame);
            // 
            // webcamButton
            // 
            this.webcamButton.Location = new System.Drawing.Point(220, 349);
            this.webcamButton.Name = "webcamButton";
            this.webcamButton.Size = new System.Drawing.Size(75, 23);
            this.webcamButton.TabIndex = 10;
            this.webcamButton.Text = "Start/Stop";
            this.webcamButton.UseVisualStyleBackColor = true;
            this.webcamButton.Click += new System.EventHandler(this.webcamButton_Click);
            // 
            // comboBox1
            // 
            this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(93, 349);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(121, 21);
            this.comboBox1.TabIndex = 11;
            // 
            // findCam
            // 
            this.findCam.Location = new System.Drawing.Point(12, 347);
            this.findCam.Name = "findCam";
            this.findCam.Size = new System.Drawing.Size(75, 23);
            this.findCam.TabIndex = 12;
            this.findCam.Text = "Find Cam";
            this.findCam.UseVisualStyleBackColor = true;
            this.findCam.Click += new System.EventHandler(this.findCam_Click);
            // 
            // clearList
            // 
            this.clearList.Location = new System.Drawing.Point(475, 3);
            this.clearList.Name = "clearList";
            this.clearList.Size = new System.Drawing.Size(251, 43);
            this.clearList.TabIndex = 13;
            this.clearList.Text = "Clear list";
            this.clearList.UseVisualStyleBackColor = true;
            this.clearList.Click += new System.EventHandler(this.clearList_Click);
            // 
            // statusStrip
            // 
            this.statusStrip.Location = new System.Drawing.Point(0, 380);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(738, 22);
            this.statusStrip.SizingGrip = false;
            this.statusStrip.TabIndex = 14;
            this.statusStrip.Text = "statusStrip";
            // 
            // infoButton
            // 
            this.infoButton.Location = new System.Drawing.Point(618, 349);
            this.infoButton.Name = "infoButton";
            this.infoButton.Size = new System.Drawing.Size(108, 23);
            this.infoButton.TabIndex = 15;
            this.infoButton.Text = "Info";
            this.infoButton.UseVisualStyleBackColor = true;
            this.infoButton.Click += new System.EventHandler(this.infoButton_Click);
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(738, 402);
            this.Controls.Add(this.infoButton);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.clearList);
            this.Controls.Add(this.findCam);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.webcamButton);
            this.Controls.Add(this.videoSourcePlayer1);
            this.Controls.Add(this.buttonCopy);
            this.Controls.Add(this.listView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Main";
            this.Text = "QR Reader";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ListView listView;
        private System.Windows.Forms.Button buttonCopy;
        private AForge.Controls.VideoSourcePlayer videoSourcePlayer1;
        private System.Windows.Forms.Button webcamButton;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.Button findCam;
        private System.Windows.Forms.Button clearList;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.Button infoButton;
    }
}

