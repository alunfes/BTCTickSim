namespace BTCTickSim
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonStart = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonTest = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.label3 = new System.Windows.Forms.Label();
            this.buttonMultiGASIM = new System.Windows.Forms.Button();
            this.buttonContiGASim = new System.Windows.Forms.Button();
            this.listBox2 = new System.Windows.Forms.ListBox();
            this.buttonGA = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(48, 184);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(194, 83);
            this.buttonStart.TabIndex = 0;
            this.buttonStart.Text = "Best Chrom SIM";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(44, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 24);
            this.label1.TabIndex = 1;
            this.label1.Text = "label1";
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(303, 897);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(194, 83);
            this.buttonTest.TabIndex = 2;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(44, 73);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "label2";
            // 
            // listBox1
            // 
            this.listBox1.FormattingEnabled = true;
            this.listBox1.HorizontalScrollbar = true;
            this.listBox1.ItemHeight = 24;
            this.listBox1.Location = new System.Drawing.Point(303, 182);
            this.listBox1.Name = "listBox1";
            this.listBox1.ScrollAlwaysVisible = true;
            this.listBox1.Size = new System.Drawing.Size(1180, 340);
            this.listBox1.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(45, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "label3";
            // 
            // buttonMultiGASIM
            // 
            this.buttonMultiGASIM.Location = new System.Drawing.Point(49, 439);
            this.buttonMultiGASIM.Name = "buttonMultiGASIM";
            this.buttonMultiGASIM.Size = new System.Drawing.Size(194, 83);
            this.buttonMultiGASIM.TabIndex = 6;
            this.buttonMultiGASIM.Text = "Multi GA SIM";
            this.buttonMultiGASIM.UseVisualStyleBackColor = true;
            this.buttonMultiGASIM.Click += new System.EventHandler(this.buttonMultiGASIM_Click);
            // 
            // buttonContiGASim
            // 
            this.buttonContiGASim.Location = new System.Drawing.Point(48, 568);
            this.buttonContiGASim.Name = "buttonContiGASim";
            this.buttonContiGASim.Size = new System.Drawing.Size(194, 83);
            this.buttonContiGASim.TabIndex = 7;
            this.buttonContiGASim.Text = "Conti GA SIM";
            this.buttonContiGASim.UseVisualStyleBackColor = true;
            this.buttonContiGASim.Click += new System.EventHandler(this.buttonContiGASim_Click);
            // 
            // listBox2
            // 
            this.listBox2.FormattingEnabled = true;
            this.listBox2.HorizontalScrollbar = true;
            this.listBox2.ItemHeight = 24;
            this.listBox2.Location = new System.Drawing.Point(303, 528);
            this.listBox2.Name = "listBox2";
            this.listBox2.ScrollAlwaysVisible = true;
            this.listBox2.Size = new System.Drawing.Size(1180, 340);
            this.listBox2.TabIndex = 8;
            // 
            // buttonGA
            // 
            this.buttonGA.Location = new System.Drawing.Point(48, 312);
            this.buttonGA.Name = "buttonGA";
            this.buttonGA.Size = new System.Drawing.Size(194, 83);
            this.buttonGA.TabIndex = 9;
            this.buttonGA.Text = "GA";
            this.buttonGA.UseVisualStyleBackColor = true;
            this.buttonGA.Click += new System.EventHandler(this.buttonGA_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1974, 1344);
            this.Controls.Add(this.buttonGA);
            this.Controls.Add(this.listBox2);
            this.Controls.Add(this.buttonContiGASim);
            this.Controls.Add(this.buttonMultiGASIM);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.buttonTest);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.buttonStart);
            this.Name = "Form1";
            this.Text = "Form1";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button buttonStart;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonMultiGASIM;
        private System.Windows.Forms.Button buttonContiGASim;
        private System.Windows.Forms.ListBox listBox2;
        private System.Windows.Forms.Button buttonGA;
    }
}

