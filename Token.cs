namespace TTRL
{
    public struct Token
    {
        public TokenType tokenType;
        public string contents;

        public Token(TokenType type, string tokenContents)
        {
            tokenType = type;
            contents = tokenContents;
        }

        public bool Is(TokenType type) => tokenType == type;
        public bool IsLiteral() =>
            tokenType == TokenType.IntLiteral ||
            tokenType == TokenType.FloatLiteral ||
            tokenType == TokenType.StringLiteral;

        public override string ToString() =>
            $"[{tokenType}] \"{contents}\"";
    }
}