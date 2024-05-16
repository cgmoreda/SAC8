using SAC9.Lexer;

namespace SAC9.Parser;

public class Parser : IParser {
  public List<Lexeme> lexemes;

  public Parser(List<Lexeme> lexemes) => this.lexemes = lexemes;

  public Result Parse() { return DeclarationList(0, lexemes.Count - 1); }

  public Result DeclarationList(int start, int end) {
    if (start > end)
      return new Result { last = -1 };
    var path1 = Declaration(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return path1;
    }
    var path2 = DeclarationList(path1.last + 1, end);
    // if the second path is invalid return the valid `path`
    if (path2.last == -1) {
      return path1;
    }
    // bolierplate code to create a new node
    Node node = ParserServices.CreateNode("DeclarationList", start, path2.last,
                                          path1.node, path2.node);
    Result result = new Result { node = node, last = path2.last,
                                 error = path1.error + path2.error };
    return result;
  }

  public Result Declaration(int start, int end) {
    var path1 = VarDeclaration(start, end);
    var path2 = FunDeclaration(start, end);
    if (path1.last == -1 && path2.last == -1) {
      return new Result { last = -1, error = "" + path1.error + path2.error };
    }
    var result =
        new Result { node = new Node { Type = "Declaration", left = start } };
    if (path1.last != -1) {
      if (path1.node is not null)
        result.node.Children.Add(path1.node);
      result.last = result.node.right = path1.last;
    }
    if (path2.last != -1) {
      if (path2.node is not null)
        result.node.Children.Add(path2.node);
      result.last = result.node.right = path2.last;
    }
    return result;
  }

  public Result VarDeclaration(int start, int end) {
    int i = start;
    if (start < end - 3)
      return ParserServices.CreateResult(
          -1, $"incomplete Declaration at line:{lexemes[i].line}");
    if (start < end - 5 && lexemes[start + 2].type == TokenType.OpenBracket) {
      if (ParserServices.TypeSpecifier(lexemes[i++]) &&
          lexemes[i++].type == TokenType.Ident &&
          lexemes[i++].type == TokenType.OpenBracket &&
          lexemes[i++].type == TokenType.Number &&
          lexemes[i++].type == TokenType.CloseBracket &&
          lexemes[i].type == TokenType.Simecolon) {
        return ParserServices.CreateResult(
            i, "", ParserServices.CreateNode("VarDeclaration", start, i));
      }
      return ParserServices.CreateResult(
          -1,
          $"incorrect Declaration at line: ${lexemes[i].line} col: ${lexemes[i].column}");
    } else {
      if (ParserServices.TypeSpecifier(lexemes[i++]) &&
          lexemes[i++].type == TokenType.Ident &&
          lexemes[i].type == TokenType.Simecolon) {
        return ParserServices.CreateResult(
            i, "", ParserServices.CreateNode("VarDeclaration", start, i));
      }
      return ParserServices.CreateResult(
          -1,
          $"incorrect Declaration at line: ${lexemes[i].line} col: ${lexemes[i].column}");
    }
  }

  public Result FunDeclaration(int start, int end) {
    int i = start;
    // TODO: reda red write the example
    if (start < end - 5)
      return ParserServices.CreateResult(
          -1, $"incomplete FunDeclaration {lexemes[start].line}");

    Result path1, path2;
    var getNextPar = (int j) => {
      while (j < end) {
        j++;
        if (lexemes[j].type == TokenType.ClosePar)
          return j;
      }
      return -1;
    };
    int j, k;
    if (ParserServices.TypeSpecifier(lexemes[i++]) &&
        lexemes[i++].type == TokenType.Ident &&
        lexemes[i++].type == TokenType.OpenPar && (j = getNextPar(i)) != -1) {
      path1 = Params(i, j++);
      if ((k = Enders.CompoundStatementClose(j + 1, lexemes, end)) == -1)
        return ParserServices.CreateResult(
            -1, $"expected {'}'} at line : {lexemes[end].line} ");
      path2 = CompoundStmt(j, k);
    } else
      return ParserServices.CreateResult(
          -1,
          $"incomplete function Declaration at line: {lexemes[i].line} column: {lexemes[i].column}");
  }

  public Result Params(int start, int end) {
    // TODO: Sherif
    var node = ParserServices.CreateNode("paramList", start, end - 1);
    for (; start < end; start++) {
      if (ParserServices.TypeSpecifier(lexemes[start++]) &&
          lexemes[start++].type == TokenType.Ident) {
        if (lexemes[start].type == TokenType.Comma || start == end)
          node.Children.Add(
              ParserServices.CreateNode("param", start - 2, start));
        else if (lexemes[start].type == TokenType.OpenBracket &&
                 lexemes[++start].type == TokenType.CloseBracket &&
                 (lexemes[start].type == TokenType.Comma || start == end)) {
          node.Children.Add(
              ParserServices.CreateNode("param", start - 4, start));
        } else
          return ParserServices.CreateError(
              $"function paramers Incorrect at line: {lexemes[end].line}");
      } else
        return ParserServices.CreateError(
            $"function paramers Incorrect at line: {lexemes[end].line}");
    }
    return ParserServices.CreateResult(end, "", node);
  }

  public Result CompoundStmt(int start, int end) {}

  public Result LocalDeclarations(int start, int end) {}

  public Result StatementList(int start, int end) {}

  public Result Statement(int start, int end) {}

  public Result ExpressionStmt(int start, int end) {}

  public Result SelectionStmt(int start, int end) {}

  public Result IterationStmt(int start, int end) {}

  public Result ReturnStmt(int start, int end) {}

  public Result Expression(int start, int end) {}

  public Result Var(int start, int end) {}

  public Result SimpleExpression(int start, int end) {}

  public Result AdditiveExpression(int start, int end) {}

  public Result Term(int start, int end) {}

  public Result Factor(int start, int end) {}

  public Result Call(int start, int end) {}

  public Result Args(int start, int end) {}

  public Result ArgList(int start, int end) {}
}
