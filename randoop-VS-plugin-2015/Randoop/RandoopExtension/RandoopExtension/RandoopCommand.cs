//------------------------------------------------------------------------------
// <copyright file="RandoopCommand.cs" company="SimCorp">
//     Copyright (c) SimCorp.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Randoop;
using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Forms;
using VSLangProj;

namespace RandoopExtension
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class RandoopCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("b0c4bc53-e9e9-480d-8179-9da2bf028d58");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;

        /// <summary>
        /// Initializes a new instance of the <see cref="RandoopCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private RandoopCommand(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static RandoopCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new RandoopCommand(package);

        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {

            randoopExe(RandoopCommandPackage.App);

        }


        private void randoopExe(DTE2 application)
        {
            {
                //////////////////////////////////////////////////////////////////////////////////
                //step 1. when load randoop_net_addin, the path of "randoop" is defined 
                //////////////////////////////////////////////////////////////////////////////////
                string installPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                var randoop_path =
                    installPath + "\\Randoop-NET-release";

                //////////////////////////////////////////////////////////////////////////////////
                // step 2. create win form (if an item is or is not selected in solution explorer)
                //////////////////////////////////////////////////////////////////////////////////

                UIHierarchy solutionExplorer = application.ToolWindows.SolutionExplorer;
                var items = solutionExplorer.SelectedItems as Array;
                var arg = new Arguments(randoop_path);

                //if (items.Length >= 1)
                if (items.Length == 1)
                {
                    /*
                    if (items.Length > 1)
                    {
                        MessageBox.Show("Select only one item.", "ERROR");
                        return;
                    }*/

                    UIHierarchyItem item1 = items.GetValue(0) as UIHierarchyItem;
                    var prJItem = item1.Object as ProjectItem;

                    if (prJItem != null)
                    {
                        string prjPath = prJItem.Properties.Item("FullPath").Value.ToString();
                        if (prjPath.EndsWith(".dll") || prjPath.EndsWith(".exe"))
                            arg.SetDllToTest(prjPath);

                    }
                }

                //////////////////////////////////////////////////////////////////////////////////
                // step 3. show the win form
                //////////////////////////////////////////////////////////////////////////////////

                arg.ShowDialog();

                if (arg.ifContinue() == false)
                {
                    //MessageBox.Show("not going to execute Randoop."); 
                    return;
                }

                //////////////////////////////////////////////////////////////////////////////////
                // step 4. run Randoop.exe while reporting progress
                //////////////////////////////////////////////////////////////////////////////////

                string exepath = randoop_path + "\\bin\\Randoop.exe";

                if (!File.Exists(exepath))
                {
                    MessageBox.Show("Can't find Randoop.exe!", "ERROR");
                    return;
                }

                var prg = new Progress();
                int totalTime = arg.GetTimeLimit();

                prg.getTotalTime(totalTime);
                prg.setRandoopExe(exepath);
                prg.setRandoopArg(arg.GetRandoopArg());

                string out_dir = arg.GetTestFilepath();
                int nTestPfile = arg.GetTestNoPerFile();

                prg.setOutDir(out_dir);
                prg.setTestpFile(nTestPfile);

                string dllTest = arg.GetDllToTest();
                prg.setObjTested(dllTest);

                prg.ShowDialog();

                if (prg.isNormal() == false)
                {
                    return;
                }

                string pathToTestPrj = CreateTestProject(out_dir, dllTest, application.Solution);
                if (!string.IsNullOrEmpty(pathToTestPrj))
                {
                    MessageBox.Show("Test file is created in project: " + pathToTestPrj);
                    //invoke ie to open index.html generated by Randoop
                    System.Diagnostics.Process.Start("IEXPLORE.EXE", pathToTestPrj + "\\index.html");
                }

            }

            return;
        }


        private string CreateTestProject(string outputDirectory, string dllUnderTest, Solution currentSolution)
        {
            try
            {
                var randoopTestProjectExists = false;
                var projects = currentSolution.Projects;
                if (projects == null || projects.Count == 0)
                {
                    MessageBox.Show("No project in current solution.", "ERROR");
                    return null;
                }

                foreach (Project project in projects)
                {
                    if (project.Name.Contains("RandoopTestPrj"))
                        randoopTestProjectExists = true;
                }

                if (randoopTestProjectExists == false)
                {
                    string testProjectPath = currentSolution.FullName;
                    string projectName = "RandoopTestPrj";

                    int index = testProjectPath.LastIndexOf("\\");
                    testProjectPath = testProjectPath.Substring(0, index + 1) + projectName;
                    Solution2 soln = currentSolution as Solution2;
                    string csTemplatePath = soln.GetProjectTemplate("TestProject.zip", "CSharp");
                    if (Directory.Exists(testProjectPath)) { Directory.Delete(testProjectPath); }
                    currentSolution.AddFromTemplate(csTemplatePath, testProjectPath, projectName, false); //IMPORTANT: it always returns NULL
                }

                //locate the Randoop Test Project in current solution
                int indexOfTestProject = 1;
                foreach (Project project in projects)
                {
                    if (project.FullName.Contains("RandoopTestPrj"))
                        break;
                    indexOfTestProject++;
                }

                string testFilePath = outputDirectory + "\\RandoopTest.cs";
                string testHtmlPath = outputDirectory + "\\index.html";
                string testStatPath = outputDirectory + "\\allstats.txt";
                bool testFileWasAdded = false;
                bool testHtmlWasAdded = false;
                bool testStatWasAdded = false;

                Project testProject = currentSolution.Projects.Item(indexOfTestProject);
                if (testProject != null)
                {
                    foreach (ProjectItem it in testProject.ProjectItems)
                    {
                        if (it.Name.Contains("RandoopTest.cs") && (testFileWasAdded == false))
                        {
                            it.Delete();
                            testProject.ProjectItems.AddFromFileCopy(testFilePath);
                            testFileWasAdded = true;
                        }
                        else
                        {
                            if (it.Name.Contains("index.html") && (testHtmlWasAdded == false))
                            {
                                it.Delete();
                                testProject.ProjectItems.AddFromFileCopy(testHtmlPath);
                                testHtmlWasAdded = true;
                            }
                            else
                            {
                                if (it.Name.Contains("allstats.txt") && (testStatWasAdded == false))
                                {
                                    it.Delete();
                                    testProject.ProjectItems.AddFromFileCopy(testStatPath);
                                    testStatWasAdded = true;
                                }
                            }
                        }

                    }

                    if (!testFileWasAdded)
                        testProject.ProjectItems.AddFromFileCopy(testFilePath);

                    if (!testHtmlWasAdded)
                        testProject.ProjectItems.AddFromFileCopy(testHtmlPath);

                    if (!testStatWasAdded)
                        testProject.ProjectItems.AddFromFileCopy(testStatPath);


                    foreach (ProjectItem it in testProject.ProjectItems)
                    {
                        if (it.Name.Contains("UnitTest1.cs"))
                            it.Delete();
                    }


                    //delete original randoop outputs
                    Directory.Delete(outputDirectory, true);

                    //Programmatically add references to project under test 
                    if (randoopTestProjectExists == false)
                    {
                        VSProject selectedVSProject = null;
                        selectedVSProject = (VSProject)testProject.Object;
                        selectedVSProject.References.Add(dllUnderTest);
                    }

                    return (testProject.FullName.Replace("\\RandoopTestPrj.csproj", ""));

                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("ERROR: " + ex.Message);
            }

            return null;
        }

    }
}
