using SAC9.Lexer;

namespace SAC9.Parser;

public class ParserServices : IParserServices {
  public static bool TypeSpecifier(Lexeme lexeme) {
    return lexeme.type == TokenType.real_ || lexeme.type == TokenType.num_ || lexeme.type == TokenType.Void_;
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

  public static Node CreateNode(string type, int left, int right,
                                params Node?[] children) {
    Node node = new Node { Type = type, left = left, right = right };
    for (int i = 0; i < children.Length; i++) {
      if (children[i] is not null)
        node.Children.Add(children[i]);
    }
    return node;
  }

  public static Result CreateResult(int last, string error, Node? node = null) {
    return new Result { last = last, error = error, node = node };
  }
  // TODO the interface 
  public static Result CreateError(string error){
    return new Result { last = -1, error = error};
  }
}

public static class Enders {
  public static int CompoundStatementClose(int i, List<Lexeme> lex, int end) {
    int cnt = 1;
    for (; i < end; i++) {
      if (lex[i].type == TokenType.OpenBrace)
        cnt++;
      else if (lex[i].type == TokenType.CloseBrace)
        cnt--;
      if (cnt == 0)
        return i;
    }
    return -1;
  }
  public static int BracetClose(int i, List<Lexeme> lex, int end) {
    int cnt = 1;
    for (; i < end; i++) {
      if (lex[i].type == TokenType.OpenBracket)
        cnt++;
      else if (lex[i].type == TokenType.OpenBracket)
        cnt--;
      if (cnt == 0)
        return i;
    }
    return -1;
  }
}
