namespace RandoopExtension
{
    partial class Arguments
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
            this.unitUnderTestTextBox = new System.Windows.Forms.TextBox();
            this.unitUnderTestLabel = new System.Windows.Forms.Label();
            this.testFrameworkLabel = new System.Windows.Forms.Label();
            this.testFrameworkComboBox = new System.Windows.Forms.ComboBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.outputDirectoryLabel = new System.Windows.Forms.Label();
            this.outputDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.browseUnitUnderTestButton = new System.Windows.Forms.Button();
            this.browseOutputDirectoryButton = new System.Windows.Forms.Button();
            this.browseForUnitUnderTest = new System.Windows.Forms.OpenFileDialog();
            this.browseForOutputDirectory = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // unitUnderTestTextBox
            // 
            this.unitUnderTestTextBox.Location = new System.Drawing.Point(218, 36);
            this.unitUnderTestTextBox.Name = "unitUnderTestTextBox";
            this.unitUnderTestTextBox.Size = new System.Drawing.Size(309, 22);
            this.unitUnderTestTextBox.TabIndex = 0;
            // 
            // unitUnderTestLabel
            // 
            this.unitUnderTestLabel.AutoSize = true;
            this.unitUnderTestLabel.Location = new System.Drawing.Point(29, 39);
            this.unitUnderTestLabel.Name = "unitUnderTestLabel";
            this.unitUnderTestLabel.Size = new System.Drawing.Size(108, 17);
            this.unitUnderTestLabel.TabIndex = 1;
            this.unitUnderTestLabel.Text = "Unit Under Test";
            // 
            // testFrameworkLabel
            // 
            this.testFrameworkLabel.AutoSize = true;
            this.testFrameworkLabel.Location = new System.Drawing.Point(29, 114);
            this.testFrameworkLabel.Name = "testFrameworkLabel";
            this.testFrameworkLabel.Size = new System.Drawing.Size(153, 17);
            this.testFrameworkLabel.TabIndex = 2;
            this.testFrameworkLabel.Text = "Desired test framework";
            // 
            // testFrameworkComboBox
            // 
            this.testFrameworkComboBox.FormattingEnabled = true;
            this.testFrameworkComboBox.Items.AddRange(new object[] {
            "MSTest",
            "NUnit"});
            this.testFrameworkComboBox.Location = new System.Drawing.Point(218, 111);
            this.testFrameworkComboBox.Name = "testFrameworkComboBox";
            this.testFrameworkComboBox.Size = new System.Drawing.Size(102, 24);
            this.testFrameworkComboBox.TabIndex = 3;
            this.testFrameworkComboBox.SelectedIndexChanged += new System.EventHandler(this.testFrameworkComboBox_SelectedIndexChanged);
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(539, 230);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(103, 23);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            this.generateButton.Click += new System.EventHandler(this.generateButton_Click);
            // 
            // outputDirectoryLabel
            // 
            this.outputDirectoryLabel.AutoSize = true;
            this.outputDirectoryLabel.Location = new System.Drawing.Point(27, 76);
            this.outputDirectoryLabel.Name = "outputDirectoryLabel";
            this.outputDirectoryLabel.Size = new System.Drawing.Size(110, 17);
            this.outputDirectoryLabel.TabIndex = 6;
            this.outputDirectoryLabel.Text = "Output directory";
            // 
            // outputDirectoryTextBox
            // 
            this.outputDirectoryTextBox.Location = new System.Drawing.Point(218, 74);
            this.outputDirectoryTextBox.Name = "outputDirectoryTextBox";
            this.outputDirectoryTextBox.Size = new System.Drawing.Size(309, 22);
            this.outputDirectoryTextBox.TabIndex = 5;
            // 
            // browseUnitUnderTestButton
            // 
            this.browseUnitUnderTestButton.Location = new System.Drawing.Point(567, 35);
            this.browseUnitUnderTestButton.Name = "browseUnitUnderTestButton";
            this.browseUnitUnderTestButton.Size = new System.Drawing.Size(75, 23);
            this.browseUnitUnderTestButton.TabIndex = 7;
            this.browseUnitUnderTestButton.Text = "Browse";
            this.browseUnitUnderTestButton.UseVisualStyleBackColor = true;
            this.browseUnitUnderTestButton.Click += new System.EventHandler(this.BrowseUnitUnderTestButton_Click);
            // 
            // browseOutputDirectoryButton
            // 
            this.browseOutputDirectoryButton.Location = new System.Drawing.Point(567, 73);
            this.browseOutputDirectoryButton.Name = "browseOutputDirectoryButton";
            this.browseOutputDirectoryButton.Size = new System.Drawing.Size(75, 23);
            this.browseOutputDirectoryButton.TabIndex = 8;
            this.browseOutputDirectoryButton.Text = "Browse";
            this.browseOutputDirectoryButton.UseVisualStyleBackColor = true;
            this.browseOutputDirectoryButton.Click += new System.EventHandler(this.BrowseOutputDirectoryButton_Click);
            // 
            // Arguments
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(680, 283);
            this.Controls.Add(this.browseOutputDirectoryButton);
            this.Controls.Add(this.browseUnitUnderTestButton);
            this.Controls.Add(this.outputDirectoryLabel);
            this.Controls.Add(this.outputDirectoryTextBox);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.testFrameworkComboBox);
            this.Controls.Add(this.testFrameworkLabel);
            this.Controls.Add(this.unitUnderTestLabel);
            this.Controls.Add(this.unitUnderTestTextBox);
            this.Name = "Arguments";
            this.Text = "Randoop Arguments";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox unitUnderTestTextBox;
        private System.Windows.Forms.Label unitUnderTestLabel;
        private System.Windows.Forms.Label testFrameworkLabel;
        private System.Windows.Forms.ComboBox testFrameworkComboBox;
        private System.Windows.Forms.Button generateButton;
        private System.Windows.Forms.Label outputDirectoryLabel;
        private System.Windows.Forms.TextBox outputDirectoryTextBox;
        private System.Windows.Forms.Button browseUnitUnderTestButton;
        private System.Windows.Forms.Button browseOutputDirectoryButton;
        private System.Windows.Forms.OpenFileDialog browseForUnitUnderTest;
        private System.Windows.Forms.FolderBrowserDialog browseForOutputDirectory;
    }
}