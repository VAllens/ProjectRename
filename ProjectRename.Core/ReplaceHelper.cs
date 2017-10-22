using System;
using System.IO;
using System.Linq;
using System.Text;

namespace ProjectRename.Core
{
    public static class ReplaceHelper
    {
        private static readonly string[] Helps = { "-?", "/?", "-h", "/h", "help", "--help", "/help" };

        public static void Run(string[] args)
        {
            //string currentDirectory = Environment.CurrentDirectory;
            //string path = Directory.GetCurrentDirectory();

            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Clear();

            Console.WriteLine("Welcome to use Project Rename tools");
            Console.WriteLine("by Allen");
            Console.WriteLine("blog: http://vallen.cnblogs.com");
            Console.WriteLine();

            string workPath = GetWorkPath(args);
            DirectoryInfo dir = new DirectoryInfo(workPath);
            Console.WriteLine("Is about to be replace in this work directory: " + workPath);

            try
            {
                string go;
                do
                {
                    ReplaceWorkDir(dir);
                    Console.WriteLine();
                    Console.Write("Do you want to continue using other text instead? (Y/N) ");
                    go = Console.ReadLine().Trim().ToLower();
                } while (go == "y");
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("---\nThe following error occurred while executing the snippet:\n{0}\n---", ex.ToString()));
            }
            finally
            {
                Console.Write("Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void ReplaceWorkDir(DirectoryInfo dir)
        {
            string findText = ConsoleHelper.ReadLine("Please enter the old name you want to find: ");
            Console.WriteLine("You entered the: " + findText);
            string replaceText = ConsoleHelper.ReadLine("Please enter the new name you want to replace: ");
            Console.WriteLine("You entered the: " + replaceText);
            Console.Write(string.Format("Replace {0} with {1}, do you want to continue? (Y/N) ", findText, replaceText));
            if (Console.ReadLine().Trim().ToLower() != "y")
            {
                return;
            }

            ReplaceFolderName(dir, findText, replaceText);
            ReplaceDirectories(dir, findText, replaceText);
        }

        /// <summary>
        /// 替换指定目录下的所有子文件夹名称
        /// </summary>
        /// <param name="parent">指定目录</param>
        /// <param name="findText">要查找的字符文本</param>
        /// <param name="replaceText">要替换的字符文本</param>
        private static void ReplaceFolderName(DirectoryInfo parent, string findText, string replaceText)
        {
            var dirs = parent.GetDirectories();
            foreach (var dir in dirs)
            {
                ReplaceFolderName(dir, findText, replaceText);

                if (dir.Name.Contains(findText))
                {
                    string newFolderName = dir.Name.Replace(findText, replaceText);
                    string parentPath = dir.Parent.FullName;
                    string newPath = Path.Combine(parentPath, newFolderName);
                    Console.WriteLine("old folder name before replacement: " + dir.FullName);
                    dir.MoveTo(newPath);
                    Console.WriteLine("New folder name after replacement: " + newPath);
                }
            }
        }

        /// <summary>
        /// 替换指定目录下的所有文件夹(包括子文件夹)
        /// </summary>
        /// <param name="parent">指定目录</param>
        /// <param name="findText">要查找的字符文本</param>
        /// <param name="replaceText">要替换的字符文本</param>
        private static void ReplaceDirectories(DirectoryInfo parent, string findText, string replaceText)
        {
            ReplaceFiles(parent, findText, replaceText);
            var dirs = parent.GetDirectories();
            foreach (var dir in dirs)
            {
                ReplaceDirectories(dir, findText, replaceText);
            }
        }

        /// <summary>
        /// 替换指定目录下的所有文件的名称和内容
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="findText"></param>
        /// <param name="replaceText"></param>
        private static void ReplaceFiles(DirectoryInfo parent, string findText, string replaceText)
        {
            var files = parent.GetFiles();
            foreach (var file in files)
            {
                Console.WriteLine("File: " + file.FullName + "\t Size: " + file.Length);
                ReplaceText(file.FullName, findText, replaceText, null);
            }
        }

        /// <summary>
        /// 替换指定文件的名称和内容
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="findText"></param>
        /// <param name="replaceText"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        private static bool ReplaceText(string fileName, string findText, string replaceText, string options)
        {
            string extensionName = Path.GetExtension(fileName);
            
            if (fileName.Contains(findText))
            {
                string path = Path.GetDirectoryName(fileName);
                string oldFileName = Path.GetFileNameWithoutExtension(fileName);
                string newFileName = oldFileName.Replace(findText, replaceText);
                string newFilePath = Path.Combine(path, newFileName + extensionName);

                File.Move(fileName, newFilePath);
                fileName = newFilePath;
                Console.WriteLine("New file name after replacement: " + newFilePath);
            }

            if (extensionName.Equals(".dll", StringComparison.Ordinal) ||
                extensionName.Equals(".exe", StringComparison.Ordinal) ||
                extensionName.Equals(".png", StringComparison.Ordinal) ||
                extensionName.Equals(".gif", StringComparison.Ordinal) ||
                extensionName.Equals(".jpg", StringComparison.Ordinal))
            {
                return false;
            }

            Encoding encoding = Encoding.UTF8;
            using (FileStream fileStream = File.OpenRead(fileName))
            {
                for (long index = 0; index < fileStream.Length; ++index)
                {
                    if (fileStream.ReadByte() == 0)
                    {
                        return false;
                    }
                }

                byte[] buffer = new byte[2];
                if (fileStream.Read(buffer, 0, 2) > 2)
                {
                    if (buffer == new byte[] { byte.MaxValue, 254 })
                    {
                        encoding = Encoding.Unicode;
                    }
                    if (buffer == new byte[] { 254, byte.MaxValue })
                    {
                        encoding = Encoding.BigEndianUnicode;
                    }
                    if (buffer == new byte[] { 239, 187 })
                    {
                        encoding = Encoding.UTF8;
                    }
                }
                fileStream.Close();
            }

            string contents = TextHelper.ReplaceText(File.ReadAllText(fileName, encoding), findText, replaceText, options);
            File.WriteAllText(fileName, contents, encoding);

            return true;
        }

        /// <summary>
        /// 获取工作目录
        /// </summary>
        /// <param name="args"></param>
        /// <param name="showHelp"></param>
        /// <returns></returns>
        private static string GetWorkPath(string[] args, bool showHelp = true)
        {
            string workPath;
            if (!args.Any() || args.Any(t => Helps.Contains(t)))
            {
                if (showHelp)
                {
                    ShowHelp();
                }

                do
                {
                    Console.WriteLine();
                    Console.Write("Please input a work path: ");
                    workPath = Console.ReadLine();
                } while (string.IsNullOrEmpty(workPath));
            }
            else
            {
                workPath = args.First();
            }

            if (!Directory.Exists(workPath))
            {
                Console.WriteLine("This is an invalid working directory.");
                workPath = GetWorkPath(args, false);
            }

            return workPath;
        }

        /// <summary>
        /// 显示帮助文档
        /// </summary>
        private static void ShowHelp()
        {
            Console.WriteLine("usage: ProjectRename [--help] or <path>");
            Console.WriteLine("Note: only one input parameter is accepted.");
            Console.WriteLine();
            Console.WriteLine("options:");
            Console.WriteLine("       [--help]: Output command help documentation, accept the following commands:");
            Console.WriteLine("              " + string.Join(" | ", Helps));
            Console.WriteLine("         <path>: Work path. Sample:");
            Console.WriteLine("              \"" + Environment.CurrentDirectory + "\"");
            Console.WriteLine("Sample:");
            Console.WriteLine("       ProjectRename \"" + Environment.CurrentDirectory + "\"");
            Console.WriteLine("       ProjectRename --help");
        }
    }
}