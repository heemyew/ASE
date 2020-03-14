namespace WindowsFormsApplication1
{
    partial class TakeAttendace
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
            this.components = new System.ComponentModel.Container();
            this.button2 = new System.Windows.Forms.Button();
            this.imageBox1 = new Emgu.CV.UI.ImageBox();
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(134, 276);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(170, 34);
            this.button2.TabIndex = 6;
            this.button2.Text = "Take Attendance";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // imageBox1
            // 
            this.imageBox1.BackColor = System.Drawing.Color.Transparent;
            this.imageBox1.Location = new System.Drawing.Point(57, 42);
            this.imageBox1.Name = "imageBox1";
            this.imageBox1.Size = new System.Drawing.Size(317, 217);
            this.imageBox1.TabIndex = 5;
            this.imageBox1.TabStop = false;
            // 
            // TakeAttendace
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(461, 322);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.imageBox1);
            this.Name = "TakeAttendace";
            this.Text = "TakeAttendace";
            ((System.ComponentModel.ISupportInitialize)(this.imageBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button2;
        private Emgu.CV.UI.ImageBox imageBox1;
    }
}