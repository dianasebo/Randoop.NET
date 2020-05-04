using EnvDTE;
using EnvDTE80;
using System;
using System.IO;
using VSLangProj;

namespace RandoopExtension
{
    public class TestProjectManager
    {
        public string CreateTestProject(string outputDirectory, string dllUnderTest, Solution currentSolution)
        {
            try
            {
                var randoopTestProjectExists = false;
                var projects = currentSolution.Projects;
                if (projects == null || projects.Count == 0)
                {
                    //MessageBox.Show("No project in current solution.", "ERROR");
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

                string testFilePath = outputDirectory + "\\RandoopTest.cs";
                string testHtmlPath = outputDirectory + "\\index.html";
                string testStatPath = outputDirectory + "\\allstats.txt";
                bool testFileWasAdded = false;
                bool testHtmlWasAdded = false;
                bool testStatWasAdded = false;

                //locate the Randoop Test Project in current solution
                Project testProject = null;
                foreach (Project project in projects)
                {
                    if (project.FullName.Contains("RandoopTestPrj"))
                    {
                        testProject = project;
                        break;
                    }
                }

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
                //MessageBox.Show("ERROR: " + ex.Message);
            }

            return null;
        }
    }
}
