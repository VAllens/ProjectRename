using System;

namespace ProjectRename.Core
{
    public static class ConsoleHelper
    {
        public static string ReadLine(string prompt, bool allowEmpty = false, bool lineWrap = false)
        {
            string input;

            do
            {
                if (lineWrap)
                {
                    Console.WriteLine(prompt);
                }
                else
                {
                    Console.Write(prompt);
                }
                input = Console.ReadLine();
            } while (string.IsNullOrEmpty(input) && allowEmpty == false);

            return input;
        }
    }
}