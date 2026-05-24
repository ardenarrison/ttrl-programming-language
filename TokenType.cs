public enum TokenType
{
    None,
    Newline,

    // Keywords
    Print,
    Function,
    Return,
    Exit,
    Math,
    ReadFile,
    DevMode,

    // Variable declarations
    FloatVar,
    IntVar,
    StringVar,

    // Literals
    StringLiteral,   // "hello world"
    IntLiteral,      // 42
    FloatLiteral,    // 3.14

    // Identifiers (variable/function names)
    Identifier,

    // Operators
    Plus,        // +
    Minus,       // -
    Multiply,    // *
    Divide,      // /
    Increment,   // ++
    Assign,      // =

    // Delimiters
    LParen,      // (
    RParen,      // )
    LBrace,      // {
    RBrace,      // }
    Comma,       // ,

    // Other
    Comment,
    EOF
}