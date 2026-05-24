namespace TTRL
{
    class Parser
    {
        private readonly List<Token> _tokens;
        private int _pos = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        private Token Current => _tokens[_pos];
        private Token Peek(int offset = 1) => _tokens[Math.Min(_pos + offset, _tokens.Count - 1)];

        private Token Advance()
        {
            var t = _tokens[_pos];
            if (_pos < _tokens.Count - 1) _pos++;
            return t;
        }

        private Token Expect(TokenType type)
        {
            if (Current.tokenType != type)
                throw new Exception($"[Parser] Expected {type} but got {Current.tokenType} (\"{Current.contents}\") at token {_pos}");
            return Advance();
        }

        private bool Check(TokenType type) => Current.tokenType == type;

        private bool Match(TokenType type)
        {
            if (Check(type)) { Advance(); return true; }
            return false;
        }

        public ProgramNode Parse()
        {
            var program = new ProgramNode();
            while (!Check(TokenType.EOF))
            {
                var stmt = ParseStatement();
                if (stmt != null)
                    program.Statements.Add(stmt);
            }
            return program;
        }

        private Statement? ParseStatement()
        {
            SkipNewlines();
            if (Check(TokenType.EOF)) return null;

            return Current.tokenType switch
            {
                TokenType.IntVar => ParseVarDecl(TokenType.IntVar),
                TokenType.FloatVar => ParseVarDecl(TokenType.FloatVar),
                TokenType.StringVar => ParseVarDecl(TokenType.StringVar),
                TokenType.Print => ParsePrint(),
                TokenType.Function => ParseFunctionDecl(),
                TokenType.ReadFile => ParseReadFile(),
                TokenType.Exit => ParseExit(),
                TokenType.Comment => SkipComment(),
                TokenType.Identifier => ParseIdentifierStatement(),
                TokenType.Newline => SkipComment(),
                _ => UnknownStatement()
            };
        }

        private Statement? SkipComment()
        {
            Advance();
            return null;
        }

        private VarDeclStatement ParseVarDecl(TokenType varType)
        {
            Advance();
            string name = Expect(TokenType.Identifier).contents;
            Match(TokenType.Assign);
            Expression value = varType == TokenType.StringVar
                ? ParseStringExpr()
                : ParseExpr();
            return new VarDeclStatement(varType, name, value);
        }

        private Expression ParseStringExpr()
        {
            if (Check(TokenType.StringLiteral))
                return new LiteralExpr(Advance());

            // unquoted fallback: collect until end of line
            var parts = new List<string>();
            while (!AtEnd())
                parts.Add(Advance().contents);
            return new LiteralExpr(new Token(TokenType.StringLiteral, string.Join(" ", parts)));
        }

        private PrintStatement ParsePrint()
        {
            Advance(); // consume 'print'
            var parts = new List<Expression>();
            while (!AtEnd())
            {
                if (Check(TokenType.StringLiteral))
                    parts.Add(new LiteralExpr(Advance()));
                else if (Check(TokenType.Identifier))
                    parts.Add(new VariableExpr(Advance().contents));
                else if (Check(TokenType.IntLiteral) || Check(TokenType.FloatLiteral))
                    parts.Add(new LiteralExpr(Advance()));
                else
                    break;
            }
            return new PrintStatement(parts);
        }

        private FunctionDeclStatement ParseFunctionDecl()
        {
            Advance(); // consume 'function'
            string name = Expect(TokenType.Identifier).contents;

            // handle optional newline between name and (
            SkipNewlines();
            Expect(TokenType.LParen);

            var parms = new List<string>();
            while (!Check(TokenType.RParen) && !Check(TokenType.EOF))
            {
                parms.Add(Expect(TokenType.Identifier).contents);
                Match(TokenType.Comma);
            }
            Expect(TokenType.RParen);

            // { may be on same line or next line
            SkipNewlines();
            Expect(TokenType.LBrace);

            var body = new List<Statement>();
            while (!Check(TokenType.RBrace) && !Check(TokenType.EOF))
            {
                // skip blank lines inside the body
                if (Check(TokenType.Newline))
                {
                    Advance();
                    continue;
                }
                var stmt = ParseStatement();
                if (stmt != null) body.Add(stmt);
            }

            if (!Check(TokenType.RBrace))
                throw new Exception($"[Parser] Unclosed function body for '{name}' — missing }}");

            Expect(TokenType.RBrace);
            return new FunctionDeclStatement(name, parms, body);
        }

        private Statement ParseIdentifierStatement()
        {
            string name = Advance().contents;

            if (Check(TokenType.LParen))
                return new CallStatement(ParseCallArgs(name));

            if (Check(TokenType.Increment))
            {
                Advance();
                return new AssignStatement(name, null!, isIncrement: true);
            }

            Match(TokenType.Assign);
            Expression value = ParseExpr();
            return new AssignStatement(name, value);
        }

        private CallExpr ParseCallArgs(string name)
        {
            Expect(TokenType.LParen);
            var args = new List<Expression>();
            while (!Check(TokenType.RParen) && !Check(TokenType.EOF))
            {
                args.Add(ParseExpr());
                Match(TokenType.Comma);
            }
            Expect(TokenType.RParen);
            return new CallExpr(name, args);
        }

        private ReadFileStatement ParseReadFile()
        {
            Advance();
            string fileName = Advance().contents;
            return new ReadFileStatement(fileName);
        }

        private ExitStatement ParseExit()
        {
            Advance();
            return new ExitStatement();
        }

        private Expression ParseExpr() => ParseAddSub();

        private Expression ParseAddSub()
        {
            var left = ParseMulDiv();
            while (!AtEnd() && (Check(TokenType.Plus) || Check(TokenType.Minus)))
            {
                Token op = Advance();
                var right = ParseMulDiv();
                left = new BinaryExpr(left, op, right);
            }
            return left;
        }

        private Expression ParseMulDiv()
        {
            var left = ParsePrimary();
            while (!AtEnd() && (Check(TokenType.Multiply) || Check(TokenType.Divide)))
            {
                Token op = Advance();
                var right = ParsePrimary();
                left = new BinaryExpr(left, op, right);
            }
            return left;
        }

        private Expression ParsePrimary()
        {
            if (Check(TokenType.LParen))
            {
                Advance();
                var inner = ParseExpr();
                Expect(TokenType.RParen);
                return inner;
            }

            if (Check(TokenType.IntLiteral) || Check(TokenType.FloatLiteral) || Check(TokenType.StringLiteral))
                return new LiteralExpr(Advance());

            if (Check(TokenType.Identifier))
            {
                string name = Advance().contents;
                if (Check(TokenType.LParen))
                    return ParseCallArgs(name);
                return new VariableExpr(name);
            }

            throw new Exception($"[Parser] Unexpected token in expression: {Current}");
        }

        private Statement? UnknownStatement()
        {
            Console.WriteLine($"[Parser] Skipping unknown token: {Current}");
            Advance();
            return null;
        }

        private void SkipNewlines()
        {
            while (Check(TokenType.Newline)) Advance();
        }

        private bool AtEnd() => Check(TokenType.EOF) || Check(TokenType.Newline) || Check(TokenType.RBrace);
    }
}