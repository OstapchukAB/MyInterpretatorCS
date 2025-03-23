using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections;
using System.Text;

namespace CSharpRepl
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("C# Interactive Interpreter");
            Console.WriteLine("Enter 'exit' to quit.");

            ScriptState<object>? state = null;
            var scriptOptions = ScriptOptions.Default
               .WithImports(
                    "System",
                    "System.Linq",
                    "System.Collections.Generic",
                    "System.Text",
                    "System.Text.RegularExpressions",
                    "System.IO")
               .WithReferences(
                   typeof(System.Linq.Enumerable).Assembly,
                   typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly,
                   typeof(System.Text.RegularExpressions.Regex).Assembly);

            while (true)
            {
                Console.Write("> ");
                string input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
                    break;

                try
                {
                    state = await Execute(input, state, scriptOptions);
                    if (state?.ReturnValue != null)
                    {
                        Console.WriteLine($"=> {FormatResult(state.ReturnValue)}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        static async Task<ScriptState<object>> Execute(string code,
                                                     ScriptState<object> previousState,
                                                     ScriptOptions options)
        {
            return previousState == null
                ? await CSharpScript.RunAsync(code, options)
                : await previousState.ContinueWithAsync(code, options);
        }
        static string FormatResult(object result)
        {
            if (result == null) return "null";

            var sb = new StringBuilder();

            // Для строк возвращаем как есть
            if (result is string str) return $"\"{str}\"";

            // Для IEnumerable (кроме строк) форматируем как коллекцию
            if (result is IEnumerable enumerable && !(result is string))
            {
                sb.Append("{ ");
                foreach (var item in enumerable)
                {
                    sb.Append(FormatSingleItem(item));
                    sb.Append(", ");
                }
                if (sb.Length > 2) sb.Remove(sb.Length - 2, 2);
                sb.Append(" }");
                return sb.ToString();
            }

            return FormatSingleItem(result);
        }

        static string FormatSingleItem(object item)
        {
            if (item == null) return "null";

            // Специальная обработка для byte[]
            if (item is byte[] byteArray)
                return $"byte[{byteArray.Length}] {{ {BitConverter.ToString(byteArray).Replace("-", ", ")} }}";

            // Для строк оборачиваем в кавычки
            if (item is string s) return $"\"{s}\"";

            // Для символов добавляем одинарные кавычки
            if (item is char c) return $"'{c}'";

            return item.ToString();
        }
    }
}