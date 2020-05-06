using System.Text;
using System.Windows.Forms;

namespace RandoopExtension
{
    public partial class Arguments : Form
    {
        private readonly string randoopPath;
        private bool outputDirectorySet;

        public string OutputDirectory { get => outputDirectory; set => outputDirectory = value; }
        private string outputDirectory;
        public string UnitUnderTest { get => unitUnderTest; set => unitUnderTest = value; }
        private string unitUnderTest;

        public string TestFrameworkName { get => testFrameworkName; set => testFrameworkName = value; }
        private string testFrameworkName;

        public string ArgumentsString { get => argumentsString; set => argumentsString = value; }
        private string argumentsString;

        public Arguments()
        {
            InitializeComponent();
        }

        public Arguments(string randoopPath)
        {
            this.randoopPath = randoopPath;
            InitializeComponent();
        }

        private void BrowseUnitUnderTestButton_Click(object sender, System.EventArgs e)
        {
            browseForUnitUnderTest.Filter = ".NET assembilies (*.dll; *.exe)|*.dll;*.exe|All files (*.*)|*.*";
            if (browseForUnitUnderTest.ShowDialog() == DialogResult.OK)
            {
                unitUnderTestTextBox.Text = browseForUnitUnderTest.FileName;
                unitUnderTest = browseForUnitUnderTest.FileName;

                int nameindex = unitUnderTest.LastIndexOf("\\");
                var dllDirectory = unitUnderTest.Substring(0, nameindex);
                if (outputDirectorySet == false)
                {
                    outputDirectoryTextBox.Text = dllDirectory + "\\randoop_output";
                    outputDirectory = dllDirectory + "\\randoop_output";
                }

            }
        }

        private void BrowseOutputDirectoryButton_Click(object sender, System.EventArgs e)
        {
            if (browseForOutputDirectory.ShowDialog() == DialogResult.OK)
            {
                outputDirectoryTextBox.Text = browseForOutputDirectory.SelectedPath;
                outputDirectory = outputDirectoryTextBox.Text;
                outputDirectorySet = true;
            }
        }

        private void testFrameworkComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            testFrameworkName = testFrameworkComboBox.Text;
        }

        private void generateButton_Click(object sender, System.EventArgs e)
        {
            testFrameworkName = testFrameworkComboBox.Text;

            StringBuilder randoopArg = new StringBuilder();

            var if_internal = false;
            var if_static = true;

            if (if_internal)
                randoopArg.Append(" /internal"); // MessageBox.Show("TRUE INTERNAL");
            if (!if_static)
                randoopArg.Append(" /nostatic"); // MessageBox.Show("FALSE STATIC");      

            var dir_config = randoopPath + "\\config_files";
            randoopArg.Append(" /configfiles:\"");
            randoopArg.Append(dir_config);
            randoopArg.Append("\"");

            var time_limit = "100";
            var restart_time = "100";
            var if_allow_null = false;
            var if_true_rand = false;
            var if_fair_explore = false;


            randoopArg.Append(" /noexplorer /timelimit:");
            randoopArg.Append(time_limit);


            randoopArg.Append(" /restart:");
            randoopArg.Append(restart_time); // MessageBox.Show(restart_time);
            if (if_allow_null)
                randoopArg.Append(" /allownull"); // MessageBox.Show("Allow Null");
            if (if_true_rand)
                randoopArg.Append(" /truerandom"); // MessageBox.Show("TRUE RAND");
            if (if_fair_explore)
                randoopArg.Append(" /fairexploration"); // MessageBox.Show("Fair Explore");


            var if_out_normal = true;
            var if_out_single = false;

            if (if_out_normal)
                randoopArg.Append(" /outputnormal");
            if (if_out_single)
                randoopArg.Append(" /singledir");

            randoopArg.Append(" /outputdir:\"");
            randoopArg.Append(outputDirectory);
            randoopArg.Append("\"");


            randoopArg.Append(" \"");
            randoopArg.Append(unitUnderTest);
            randoopArg.Append("\"");

            argumentsString = randoopArg.ToString();

            Close();
        }
    }
}
