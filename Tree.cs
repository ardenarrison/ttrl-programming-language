namespace TTRL
{
    // ── Expressions (things that produce a value) ────────────────────────

    public abstract class Expression { }

    // A literal value: 42, 3.14, "hello"
    public class LiteralExpr : Expression
    {
        public Token Value;
        public LiteralExpr(Token value) { Value = value; }
    }

    // A variable reference: myVar
    public class VariableExpr : Expression
    {
        public string Name;
        public VariableExpr(string name) { Name = name; }
    }

    // A binary math expression: a + b, x * 2
    public class BinaryExpr : Expression
    {
        public Expression Left;
        public Token Operator;   // Plus, Minus, Multiply, Divide
        public Expression Right;
        public BinaryExpr(Expression left, Token op, Expression right)
        { Left = left; Operator = op; Right = right; }
    }

    // A function call used as an expression: getValue()
    public class CallExpr : Expression
    {
        public string FunctionName;
        public List<Expression> Args;
        public CallExpr(string name, List<Expression> args)
        { FunctionName = name; Args = args; }
    }

    // ── Statements (things that do something) ────────────────────────────

    public abstract class Statement { }

    // int x = 5 + 3
    public class VarDeclStatement : Statement
    {
        public TokenType VarType;   // IntVar, FloatVar, StringVar
        public string Name;
        public Expression Value;
        public VarDeclStatement(TokenType type, string name, Expression value)
        { VarType = type; Name = name; Value = value; }
    }

    // x = x + 1   OR   x++
    public class AssignStatement : Statement
    {
        public string Name;
        public Expression Value;     // null if it's an increment
        public bool IsIncrement;
        public AssignStatement(string name, Expression value, bool isIncrement = false)
        { Name = name; Value = value; IsIncrement = isIncrement; }
    }

    // print x "hello" y
    public class PrintStatement : Statement
    {
        public List<Expression> Parts;
        public PrintStatement(List<Expression> parts) { Parts = parts; }
    }

    // function add(a,b) { ... }
    public class FunctionDeclStatement : Statement
    {
        public string Name;
        public List<string> Params;
        public List<Statement> Body;
        public FunctionDeclStatement(string name, List<string> parms, List<Statement> body)
        { Name = name; Params = parms; Body = body; }
    }

    // add(1, 2)  — a call as a standalone statement
    public class CallStatement : Statement
    {
        public CallExpr Call;
        public CallStatement(CallExpr call) { Call = call; }
    }

    // readfile notes.txt
    public class ReadFileStatement : Statement
    {
        public string FileName;
        public ReadFileStatement(string fileName) { FileName = fileName; }
    }

    // exit
    public class ExitStatement : Statement { }

    // The whole program
    public class ProgramNode
    {
        public List<Statement> Statements = new();
    }
}