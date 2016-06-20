using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ReplaceScript
{
    public enum State
    {
        HTML,
        PHP,
        Script
    }

    class Program
    {
        static AutoResetEvent endOfThreadEvent;
        static AutoResetEvent endOfProgramEvent;
        static int activeThreads = 0;
        static List<string> fileErrors;
        static List<string> ignoredFiles;

        static void Main(string[] args)
        {
            if (args.Length < 5)
            {
                Console.WriteLine("Try use ./ProgramName 'ReplaceFolderPath' 'FileToReplacePHPFunc' 'WordToFind' 'WordWillReplace' 'NumOfThreads' 'FolderNamesToIgnore'");
                Environment.Exit(1);
            }

            string folderPath = args[0];
            if (!Directory.Exists(folderPath))
            {
                Console.WriteLine("Folder do not exists!");
                Environment.Exit(1);
            }

            fileErrors = new List<string>();
            ignoredFiles = new List<string>();
            endOfThreadEvent = new AutoResetEvent(false);
            endOfProgramEvent = new AutoResetEvent(false);

            string filePath = args[1];
            string before = args[2];
            string after = args[3];


            int nthreads = int.Parse(args[4]);

            string[] files = Directory.GetFiles(folderPath, filePath, SearchOption.AllDirectories);

            bool valid;
            foreach (var file in files)
            {
                valid = true;
                for (int i = 5; i < args.Length; i++)
                {
                    if (file.Contains(args[i]))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    Parser p = new Parser(file, before, after);
                    new Thread(Replace).Start(p);
                    activeThreads++;
                    if (activeThreads >= nthreads)
                    {
                        endOfThreadEvent.WaitOne();
                    }
                }
                else
                {
                    ignoredFiles.Add(file);
                }
            }

            endOfProgramEvent.WaitOne();

            Console.ForegroundColor = ConsoleColor.Red;
            foreach (var item in fileErrors)
            {
                Console.WriteLine(item);
            }
            Console.ResetColor();

            Console.BackgroundColor = ConsoleColor.Yellow;
            foreach (var item in ignoredFiles)
            {
                Console.WriteLine(item);
            }
            Console.ResetColor();

            Console.WriteLine("Fim Script!");
            Console.Read();
        }

        private static void Replace(object o)
        {
            Parser p = o as Parser;
            try
            {
                p.Replace();
                Console.WriteLine(p.FilePath + " finished!");
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                fileErrors.Add(p.FilePath);
            }

            activeThreads--;
            endOfThreadEvent.Set();
            if (activeThreads == 0)
            {
                endOfProgramEvent.Set();
            }
        }
    }
}
