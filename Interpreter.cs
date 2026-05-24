using System.Runtime.InteropServices;
namespace TTRL
{
    static class Interpreter
    {
        public static Dictionary<string, string> string_variables = new();
        public static Dictionary<string, int> int_variables = new();
        public static Dictionary<string, float> float_variables = new();
        public static Dictionary<string, FunctionDeclStatement> functions = new();

        public static bool devmode = true;
        public static string? currentFilePath;

        public static void Start(string filePath)
        {
            currentFilePath = filePath;
            string source = "";
            try { source = File.ReadAllText(filePath); }
            catch { Console.WriteLine("Error reading file"); return; }

            var tokens = Lexer.Tokenize(source);
            var parser = new Parser(tokens);
            ProgramNode program = parser.Parse();

            ExecuteBlock(program.Statements);
        }

        public static void ExecuteBlock(List<Statement> statements)
        {
            foreach (var stmt in statements)
                Execute(stmt);
        }

        private static void Execute(Statement stmt)
        {
            switch (stmt)
            {
                case VarDeclStatement v: ExecuteVarDecl(v); break;
                case AssignStatement a: ExecuteAssign(a); break;
                case PrintStatement p: ExecutePrint(p); break;
                case FunctionDeclStatement f: functions[f.Name] = f; break;
                case CallStatement c: ExecuteCall(c.Call); break;
                case ReadFileStatement r: ExecuteReadFile(r); break;
                case ExitStatement: Environment.Exit(0); break;
            }
        }

        private static void ExecuteVarDecl(VarDeclStatement stmt)
        {
            switch (stmt.VarType)
            {
                case TokenType.IntVar:
                    int_variables[stmt.Name] = (int)EvalNumber(stmt.Value);
                    break;
                case TokenType.FloatVar:
                    float_variables[stmt.Name] = (float)EvalNumber(stmt.Value);
                    break;
                case TokenType.StringVar:
                    string_variables[stmt.Name] = EvalString(stmt.Value);
                    break;
            }
        }

        private static void ExecuteAssign(AssignStatement stmt)
        {
            if (int_variables.ContainsKey(stmt.Name))
            {
                int_variables[stmt.Name] = stmt.IsIncrement
                    ? int_variables[stmt.Name] + 1
                    : (int)EvalNumber(stmt.Value);
            }
            else if (float_variables.ContainsKey(stmt.Name))
            {
                float_variables[stmt.Name] = stmt.IsIncrement
                    ? float_variables[stmt.Name] + 1
                    : (float)EvalNumber(stmt.Value);
            }
            else if (string_variables.ContainsKey(stmt.Name))
            {
                string_variables[stmt.Name] = EvalString(stmt.Value);
            }
            else
            {
                Console.WriteLine($"[Interpreter] Undefined variable: {stmt.Name}");
            }
        }

        private static void ExecutePrint(PrintStatement stmt)
        {
            var result = string.Concat(stmt.Parts.Select(EvalString));
            Console.WriteLine(result);
        }

        public static void ExecuteCall(CallExpr call)
        {
            if (!functions.TryGetValue(call.FunctionName, out var fn))
            {
                Console.WriteLine($"[Interpreter] Unknown function: {call.FunctionName}");
                return;
            }

            var savedFloats = new Dictionary<string, float>(float_variables);

            for (int i = 0; i < fn.Params.Count; i++)
            {
                float val = i < call.Args.Count ? (float)EvalNumber(call.Args[i]) : 0f;
                float_variables[fn.Params[i]] = val;
            }

            ExecuteBlock(fn.Body);

            foreach (var key in fn.Params)
                if (savedFloats.ContainsKey(key))
                    float_variables[key] = savedFloats[key];
                else
                    float_variables.Remove(key);
        }

        private static void ExecuteReadFile(ReadFileStatement stmt)
        {
            string dir = Path.GetDirectoryName(currentFilePath) ?? ".";
            char sep = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? '\\' : '/';
            string path = dir + sep + stmt.FileName;

            if (!File.Exists(path))
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.WriteLine("Error: cannot find file: " + stmt.FileName);
                Console.ResetColor();
                return;
            }
            Console.WriteLine(File.ReadAllText(path));
        }

        public static double EvalNumber(Expression expr)
        {
            switch (expr)
            {
                case LiteralExpr lit when lit.Value.tokenType == TokenType.IntLiteral:
                    return double.Parse(lit.Value.contents);
                case LiteralExpr lit when lit.Value.tokenType == TokenType.FloatLiteral:
                    return double.Parse(lit.Value.contents);
                case VariableExpr v when int_variables.ContainsKey(v.Name):
                    return int_variables[v.Name];
                case VariableExpr v when float_variables.ContainsKey(v.Name):
                    return float_variables[v.Name];
                case BinaryExpr b:
                    double left = EvalNumber(b.Left);
                    double right = EvalNumber(b.Right);
                    return b.Operator.tokenType switch
                    {
                        TokenType.Plus => left + right,
                        TokenType.Minus => left - right,
                        TokenType.Multiply => left * right,
                        TokenType.Divide => left / right,
                        _ => 0
                    };
                case CallExpr c:
                    ExecuteCall(c);
                    return 0;
                default:
                    return 0;
            }
        }

        public static string EvalString(Expression expr)
        {
            switch (expr)
            {
                case LiteralExpr lit:
                    return lit.Value.contents;
                case VariableExpr v:
                    if (string_variables.TryGetValue(v.Name, out var s)) return s;
                    if (int_variables.TryGetValue(v.Name, out var i)) return i.ToString();
                    if (float_variables.TryGetValue(v.Name, out var f)) return f.ToString();
                    return v.Name;
                case BinaryExpr b:
                    return EvalNumber(b).ToString();
                case CallExpr c:
                    ExecuteCall(c);
                    return "";
                default:
                    return "";
            }
        }
    }
}