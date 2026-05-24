using System.Text;

namespace TTRL
{
    static class Lexer
    {
        public static List<Token> Tokenize(string source)
        {
            var tokens = new List<Token>();
            int pos = 0;

            while (pos < source.Length)
            {
                char c = source[pos];

                if (char.IsWhiteSpace(c))
                {
                    if (c == '\n')
                        tokens.Add(new Token(TokenType.Newline, "\\n"));
                    pos++;
                    continue;
                }

                // Comments: // ...
                if (c == '/' && pos + 1 < source.Length && source[pos + 1] == '/')
                {
                    while (pos < source.Length && source[pos] != '\n')
                        pos++;
                    continue;
                }

                // String literals: "hello world"
                if (c == '"')
                {
                    pos++; // skip opening quote
                    var sb = new StringBuilder();
                    while (pos < source.Length && source[pos] != '"')
                    {
                        if (source[pos] == '\\' && pos + 1 < source.Length)
                        {
                            // Handle escape sequences
                            pos++;
                            sb.Append(source[pos] switch
                            {
                                'n' => '\n',
                                't' => '\t',
                                '"' => '"',
                                '\\' => '\\',
                                _ => source[pos]
                            });
                        }
                        else
                        {
                            sb.Append(source[pos]);
                        }
                        pos++;
                    }
                    pos++; // skip closing quote
                    tokens.Add(new Token(TokenType.StringLiteral, sb.ToString()));
                    continue;
                }

                // Numbers: 42, 3.14
                if (char.IsDigit(c) || (c == '-' && pos + 1 < source.Length && char.IsDigit(source[pos + 1])))
                {
                    var sb = new StringBuilder();
                    bool isFloat = false;
                    if (c == '-') { sb.Append(c); pos++; }
                    while (pos < source.Length && (char.IsDigit(source[pos]) || source[pos] == '.'))
                    {
                        if (source[pos] == '.') isFloat = true;
                        sb.Append(source[pos]);
                        pos++;
                    }
                    tokens.Add(new Token(
                        isFloat ? TokenType.FloatLiteral : TokenType.IntLiteral,
                        sb.ToString()
                    ));
                    continue;
                }

                // Identifiers and keywords
                if (char.IsLetter(c) || c == '_')
                {
                    var sb = new StringBuilder();
                    while (pos < source.Length && (char.IsLetterOrDigit(source[pos]) || source[pos] == '_'))
                    {
                        sb.Append(source[pos]);
                        pos++;
                    }
                    string word = sb.ToString();
                    tokens.Add(new Token(GetKeywordType(word), word));
                    continue;
                }

                // Operators and delimiters
                switch (c)
                {
                    case '+':
                        if (pos + 1 < source.Length && source[pos + 1] == '+')
                        {
                            tokens.Add(new Token(TokenType.Increment, "++"));
                            pos += 2;
                        }
                        else
                        {
                            tokens.Add(new Token(TokenType.Plus, "+"));
                            pos++;
                        }
                        break;
                    case '-':
                        tokens.Add(new Token(TokenType.Minus, "-"));
                        pos++;
                        break;
                    case '*':
                        tokens.Add(new Token(TokenType.Multiply, "*"));
                        pos++;
                        break;
                    case '/':
                        tokens.Add(new Token(TokenType.Divide, "/"));
                        pos++;
                        break;
                    case '=':
                        tokens.Add(new Token(TokenType.Assign, "="));
                        pos++;
                        break;
                    case '(':
                        tokens.Add(new Token(TokenType.LParen, "("));
                        pos++;
                        break;
                    case ')':
                        tokens.Add(new Token(TokenType.RParen, ")"));
                        pos++;
                        break;
                    case '{':
                        tokens.Add(new Token(TokenType.LBrace, "{"));
                        pos++;
                        break;
                    case '}':
                        tokens.Add(new Token(TokenType.RBrace, "}"));
                        pos++;
                        break;
                    case ',':
                        tokens.Add(new Token(TokenType.Comma, ","));
                        pos++;
                        break;
                    default:
                        // Unknown character — skip with optional warning
                        Console.WriteLine($"[Lexer] Unknown character: '{c}' at position {pos}");
                        pos++;
                        break;
                }
            }

            tokens.Add(new Token(TokenType.EOF, ""));
            return tokens;
        }

        private static TokenType GetKeywordType(string word) => word switch
        {
            "print" => TokenType.Print,
            "function" => TokenType.Function,
            "return" => TokenType.Return,
            "exit" => TokenType.Exit,
            "math" => TokenType.Math,
            "readfile" => TokenType.ReadFile,
            "int" => TokenType.IntVar,
            "float" => TokenType.FloatVar,
            "string" => TokenType.StringVar,
            _ => TokenType.Identifier
        };
    }
}