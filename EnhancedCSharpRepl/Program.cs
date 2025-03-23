using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

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
                        Console.WriteLine($"=> {state.ReturnValue}");
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
    }
}