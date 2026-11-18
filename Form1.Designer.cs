namespace SuperVisionObjectDesktop
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            button1 = new Button();
            DevicesFlowLayout = new FlowLayoutPanel();
            button2 = new Button();
            tabControl1 = new TabControl();
            GraphicsTabs = new TabPage();
            tabPage2 = new TabPage();
            LoadPicture = new PictureBox();
            tabControl1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)LoadPicture).BeginInit();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new Point(1295, 18);
            button1.Name = "button1";
            button1.Size = new Size(119, 67);
            button1.TabIndex = 1;
            button1.Text = "Update devices list";
            button1.UseVisualStyleBackColor = true;
            button1.Click += button1_Click;
            // 
            // DevicesFlowLayout
            // 
            DevicesFlowLayout.BackColor = SystemColors.ButtonHighlight;
            DevicesFlowLayout.Location = new Point(1067, 91);
            DevicesFlowLayout.Name = "DevicesFlowLayout";
            DevicesFlowLayout.Size = new Size(347, 490);
            DevicesFlowLayout.TabIndex = 2;
            // 
            // button2
            // 
            button2.Location = new Point(1067, 18);
            button2.Name = "button2";
            button2.Size = new Size(119, 67);
            button2.TabIndex = 3;
            button2.Text = "Run stream";
            button2.UseVisualStyleBackColor = true;
            button2.Click += button2_Click;
            // 
            // tabControl1
            // 
            tabControl1.Controls.Add(GraphicsTabs);
            tabControl1.Controls.Add(tabPage2);
            tabControl1.Location = new Point(12, 38);
            tabControl1.Name = "tabControl1";
            tabControl1.SelectedIndex = 0;
            tabControl1.Size = new Size(1040, 543);
            tabControl1.TabIndex = 4;
            // 
            // GraphicsTabs
            // 
            GraphicsTabs.Location = new Point(4, 24);
            GraphicsTabs.Name = "GraphicsTabs";
            GraphicsTabs.Padding = new Padding(3);
            GraphicsTabs.Size = new Size(1032, 515);
            GraphicsTabs.TabIndex = 0;
            GraphicsTabs.Text = "tabPage1";
            GraphicsTabs.UseVisualStyleBackColor = true;
            // 
            // tabPage2
            // 
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1032, 515);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "tabPage2";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // LoadPicture
            // 
            LoadPicture.Image = (Image)resources.GetObject("LoadPicture.Image");
            LoadPicture.Location = new Point(1205, 18);
            LoadPicture.Name = "LoadPicture";
            LoadPicture.Size = new Size(72, 67);
            LoadPicture.SizeMode = PictureBoxSizeMode.StretchImage;
            LoadPicture.TabIndex = 5;
            LoadPicture.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.White;
            ClientSize = new Size(1422, 593);
            Controls.Add(LoadPicture);
            Controls.Add(tabControl1);
            Controls.Add(button2);
            Controls.Add(DevicesFlowLayout);
            Controls.Add(button1);
            Name = "Form1";
            Text = "Form1";
            tabControl1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)LoadPicture).EndInit();
            ResumeLayout(false);
        }

        #endregion
        private Button button1;
        private FlowLayoutPanel DevicesFlowLayout;
        private Button button2;
        private TabControl tabControl1;
        private TabPage GraphicsTabs;
        private TabPage tabPage2;
        private PictureBox LoadPicture;
    }
}
