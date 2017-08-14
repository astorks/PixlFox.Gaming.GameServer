using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PixlFox.Gaming.GameServer
{
    internal class ConsoleAutoComplete
    {
        private readonly StringBuilder _result = new StringBuilder();

        public ConsoleAutoComplete(params string[] values)
        {
            Values = values;
        }

        public IReadOnlyCollection<string> Values { get; set; }

        private IEnumerable<string> Suggestions { get; set; }

        private int CurrentSuggestionIndex { get; set; }

        public string Start()
        {
            var input = default(ConsoleKeyInfo);
            do
            {
                input = Console.ReadKey(intercept: true);
                if (CollectCharacter(input))
                {
                    var suggestions = FindSuggestions();
                    Suggest(suggestions);
                    SuggestAutoComplete();
                }
                else
                {
                    SelectSuggestion(input);
                    Suggest(Suggestions);
                }
            } while (input.Key != ConsoleKey.Enter);
            return _result.ToString();
        }

        private static void ClearCurrentLine()
        {
            var row = Console.CursorTop;
            var col = Console.CursorLeft;
            Console.SetCursorPosition(col, row);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(col, row);
        }

        private void CompleteValue()
        {
            var currentInput = _result.ToString();
            var match = Values.FirstOrDefault(item => item != currentInput && item.StartsWith(currentInput, true, CultureInfo.InvariantCulture));
            if (string.IsNullOrEmpty(match))
            {
                return;
            }

            ClearCurrentLine();
            _result.Clear();

            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(match);
            _result.Append(match);
        }

        private IEnumerable<string> FindSuggestions()
        {
            CurrentSuggestionIndex = -1;
            var lastWord = _result.ToString().Split(new[] { ' ' }).Last();
            if (string.IsNullOrEmpty(lastWord)) { return Enumerable.Empty<string>(); }
            return (Suggestions = Values.Where(x => x != lastWord && x.StartsWith(lastWord, true, CultureInfo.InvariantCulture)));
        }

        private void Suggest(IEnumerable<string> suggestions)
        {
            ClearSuggestions();

            using (new CursorPositionKeeper())
            {
                var topOffset = 1;
                var left = _result.ToString().LastIndexOf(' ') + 1;
                foreach (var suggestion in suggestions)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    if (Console.CursorTop == CurrentSuggestionIndex) { Console.ForegroundColor = ConsoleColor.DarkGreen; }
                    Console.SetCursorPosition(left, Console.CursorTop + topOffset);
                    Console.Write(suggestion);
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        private void SuggestAutoComplete()
        {
            using (new CursorPositionKeeper())
            {
                var last = _result.ToString().Split(' ').LastOrDefault();
                if (string.IsNullOrEmpty(last)) { return; }

                var suggestion = Suggestions.ElementAtOrDefault(CurrentSuggestionIndex + 1);
                if (string.IsNullOrEmpty(suggestion)) { return; }

                Console.ForegroundColor = ConsoleColor.DarkGray;
                var autoComplete = Regex.Replace(suggestion, $"^{last}", string.Empty, RegexOptions.IgnoreCase);
                Console.Write(autoComplete);
            }

            Console.ForegroundColor = ConsoleColor.White;
        }

        private void ClearSuggestions()
        {
            using (new CursorPositionKeeper())
            {
                for (var top = Console.CursorTop + 1; top < Console.WindowHeight; top++)
                {
                    Debug.WriteLine($"top: {top}");
                    Console.SetCursorPosition(0, top);
                    Console.Write(new string(' ', Console.WindowWidth));
                }
            }
        }


        private void SelectSuggestion(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.DownArrow || input.Key == ConsoleKey.Tab)
            {
                CurrentSuggestionIndex++;
            }
            if (input.Key == ConsoleKey.UpArrow)
            {
                CurrentSuggestionIndex--;
            }
        }

        private bool CollectCharacter(ConsoleKeyInfo input)
        {
            if (input.Key == ConsoleKey.Backspace && _result.Length > 0)
            {
                _result.Remove(_result.Length - 1, 1);
                Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);

                using (new CursorPositionKeeper())
                {
                    Console.Write(new string(' ', Console.WindowWidth));
                }
                return true;
            }

            if (char.IsLetterOrDigit(input.KeyChar) || input.KeyChar == ' ')
            {
                _result.Append(input.KeyChar);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(input.KeyChar);
                return true;
            }

            return false;
        }

        private class CursorPositionKeeper : IDisposable
        {
            private int Top { get; } = Console.CursorTop;
            private int Left { get; } = Console.CursorLeft;

            public void Dispose()
            {
                Console.SetCursorPosition(Left, Top);
            }
        }
    }
}
