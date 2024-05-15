using SAC9.Lexer;

namespace SAC9.Parser;

public class ParserServices : IParserServices {
  public static bool TypeSpecifier(Lexeme lexeme) {
    return lexeme.type == TokenType.real_ || lexeme.type == TokenType.num_;
  }

  public static bool addOp(Lexeme lexeme) {
    return lexeme.type == TokenType.AddOp;
  }

  public static bool mulOp(Lexeme lexeme) {
    return lexeme.type == TokenType.MulOp;
  }

  public static bool relOp(Lexeme lexeme) {
    return lexeme.type == TokenType.LogOp;
  }
}
