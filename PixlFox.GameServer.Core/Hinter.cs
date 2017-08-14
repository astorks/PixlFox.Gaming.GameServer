using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HinterLib
{
    public class Hinter
    {
        public static List<string> PreviousCommands { get; } = new List<string>();
        public static int PreviousCommandIndex { get; set; } = 0;

        public static string ReadHintedLine<T, TResult>(IEnumerable<T> hintSource, Func<T, TResult> hintField, string inputRegex = ".*", ConsoleColor hintColor = ConsoleColor.DarkGray, string linePrefix = "")
        {
            ConsoleKeyInfo input;

            var suggestion = string.Empty;
            var userInput = string.Empty;
            var readLine = string.Empty;

            var characterIndex = 0;

            if (!string.IsNullOrWhiteSpace(linePrefix))
                Console.Write(linePrefix);

            while (ConsoleKey.Enter != (input = Console.ReadKey()).Key)
            {
                if (input.Key == ConsoleKey.UpArrow)
                {
                    if (PreviousCommandIndex > 0)
                    {
                        PreviousCommandIndex--;
                        userInput = PreviousCommands[PreviousCommandIndex];
                        characterIndex = userInput.Length;
                    }
                }
                else if (input.Key == ConsoleKey.DownArrow)
                {
                    if (PreviousCommandIndex < PreviousCommands.Count - 1)
                    {
                        PreviousCommandIndex++;
                        userInput = PreviousCommands[PreviousCommandIndex];
                        characterIndex = userInput.Length;
                    }
                    else
                    {
                        PreviousCommandIndex = PreviousCommands.Count;
                        userInput = "";
                        characterIndex = userInput.Length;
                    }
                }
                else if (input.Key == ConsoleKey.LeftArrow)
                {
                    if (characterIndex > 0)
                        characterIndex--;
                }
                else if (input.Key == ConsoleKey.RightArrow)
                {
                    if (characterIndex < userInput.Length)
                        characterIndex++;
                }
                else if (input.Key == ConsoleKey.Home)
                    characterIndex = 0;
                else if (input.Key == ConsoleKey.End)
                    characterIndex = userInput.Length;
                else if (input.Key == ConsoleKey.L && input.Modifiers == ConsoleModifiers.Control)
                    return "clear();";
                else if (input.Key == ConsoleKey.Escape)
                {
                    userInput = "";
                    characterIndex = userInput.Length;
                }
                else if (input.Key == ConsoleKey.Backspace)
                {
                    if (characterIndex > 0)
                    {
                        userInput = userInput.Any() ? userInput.Remove(characterIndex - 1, 1) : string.Empty;
                        characterIndex--;
                    }
                }
                else if (input.Key == ConsoleKey.Tab)
                {
                    userInput = suggestion ?? userInput;
                    characterIndex = userInput.Length;
                }
                else if (input != null && Regex.IsMatch(input.KeyChar.ToString(), inputRegex))
                {
                    if (!string.IsNullOrWhiteSpace(input.KeyChar.ToString()) || input.Key == ConsoleKey.Spacebar)
                    {
                        if (userInput.Length == characterIndex)
                            userInput += input.KeyChar.ToString();
                        else
                            userInput = userInput.Insert(characterIndex, input.KeyChar.ToString());

                        characterIndex++;
                    }
                }

                suggestion = hintSource.Select(item => hintField(item).ToString())
                    .FirstOrDefault(item => item.Length > userInput.Length && item.Substring(0, userInput.Length) == userInput);

                readLine = userInput;

                ClearCurrentConsoleLine(linePrefix);

                Console.Write(userInput);

                var originalColor = Console.ForegroundColor;

                Console.ForegroundColor = hintColor;

                if (userInput.Any() && !string.IsNullOrWhiteSpace(suggestion)) Console.Write(suggestion.Substring(userInput.Length, suggestion.Length - userInput.Length));

                Console.ForegroundColor = originalColor;

                Console.SetCursorPosition(linePrefix.Length + characterIndex, Console.CursorTop);
            }

            ClearCurrentConsoleLine(linePrefix);
            Console.WriteLine(readLine);

            if(!string.IsNullOrWhiteSpace(readLine))
                PreviousCommands.Add(readLine);

            PreviousCommandIndex = PreviousCommands.Count;

            return readLine;
        }

        private static void ClearCurrentConsoleLine(string linePrefix)
        {
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
            if(!string.IsNullOrWhiteSpace(linePrefix))
                Console.Write(linePrefix);
        }
    }
}