using SAC9.Lexer;

namespace SAC9.Parser;

public class Parser : IParser {
  public List<Lexeme> lexemes;

  public Parser(List<Lexeme> lexemes) => this.lexemes = lexemes;

  public Result Parse() { 
        if(lexemes.Count == 0)
            return ParserServices.CreateResult(-1, "جدع",new Node
            { });
            
                return DeclarationList(0, lexemes.Count); }

  public Result DeclarationList(int start, int end) {
    if (start > end)
      return new Result { last = -1 };
    var path1 = Declaration(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return path1;
    }
    if (path1.last + 1 == end)
      return path1;
    var path2 = DeclarationList(path1.last + 1, end);
    // if the second path is invalid return the valid `path`

    // bolierplate code to create a new node
    Node node = ParserServices.CreateNode(
        "DeclarationList", start, path2.last + 1, path1.node, path2.node);
    Result result = ParserServices.CreateResult(path2.last, path2.error, node);
    return result;
  }

  public Result Declaration(int start, int end) {
    var path1 = VarDeclaration(start, end);
    var path2 = FunDeclaration(start, end);
    if (path1.last == -1 && path2.last == -1) {
      return new Result { last = -1, error = "" + path1.error +" or "+ path2.error };
    }
    var result =
        new Result { node = new Node { Type = "Declaration", left = start } };
    if (path1.last != -1) {
      if (path1.node is not null)
        result.node.Children.Add(path1.node);
      result.last = path1.last;
      result.node.right = path1.last + 1;
    }
    if (path2.last != -1) {
      if (path2.node is not null)
        result.node.Children.Add(path2.node);
      result.last = path2.last;
      result.node.right = path2.last + 1;
    }
    return result;
  }

  public Result VarDeclaration(int start, int end) {
    int i = start;
    if (start > end - 3)
      return ParserServices.CreateResult(
          -1, $"incomplete VarDeclaration at line:{lexemes[i].line}");
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
          $"incorrect VarDeclaration at line: {lexemes[i-1].line} col: {lexemes[i-1].column}");
    } else {
      if (ParserServices.TypeSpecifier(lexemes[i++]) &&
          lexemes[i++].type == TokenType.Ident &&
          lexemes[i].type == TokenType.Simecolon) {
        return ParserServices.CreateResult(
            i, "", ParserServices.CreateNode("VarDeclaration", start, i));
      }
      return ParserServices.CreateResult(
          -1,
          $"incorrect VarDeclaration at line: {lexemes[i].line} col: {lexemes[i].column}");
    }
  }

  public Result FunDeclaration(int start, int end) {
    int i = start;
    // TODO: reda red write the example
    if (start > end - 5)
      return ParserServices.CreateResult(
          -1, $"incomplete FunDeclaration {lexemes[start].line}");

    Result path1, path2;
    var getNextPar = (int j) => {
      while (j < end) {
        if (lexemes[j].type == TokenType.ClosePar)
          return j;
        j++;
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
      return ParserServices.CreateResult(
          k, path2.error,
          ParserServices.CreateNode("FunDeclaration", start, k + 1, path1.node,
                                    path2.node));
    }
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
              ParserServices.CreateNode("param", start - 2, start + 1));
        else if (lexemes[start].type == TokenType.OpenBracket &&
                 lexemes[++start].type == TokenType.CloseBracket &&
                 (lexemes[start].type == TokenType.Comma || start == end)) {
          node.Children.Add(
              ParserServices.CreateNode("param", start - 4, start + 1));
        } else
          return ParserServices.CreateError(
              $"function paramers Incorrect at line: {lexemes[end - 1].line}");
      } else
        return ParserServices.CreateError(
            $"function paramers Incorrect at line: {lexemes[end - 1].line}");
    }
    return ParserServices.CreateResult(end, "", node);
  }

  public Result CompoundStmt(int start, int end) {
    var path1 = LocalDeclarations(start, end);
    if (path1.last == -1)
      return ParserServices.CreateResult(
          -1, path1.error,
          ParserServices.CreateNode("CompoundStmt", start, end));
    var path2 = StatementList(path1.last + 1, end);
    if (path2.last == -1)
      return ParserServices.CreateResult(
          -1, path2.error,
          ParserServices.CreateNode("CompoundStmt", start, end));

    return ParserServices.CreateResult(
        end, "",
        ParserServices.CreateNode("CompoundStmt", start, end, path1.node,
                                  path2.node));
  }

  public Result LocalDeclarations(int start, int end) {
    if (start > end)
      return new Result { last = -1 };
    var path1 = Declaration(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return ParserServices.CreateResult(
          start, "", ParserServices.CreateNode("", start, start));
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

  public Result StatementList(int start, int end) {
    if (start > end - 1)
      return new Result { last = -1 };
    var path1 = Statement(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return path1;
    }
    if (path1.last + 1 == end)
      return path1;
    var path2 = StatementList(path1.last + 1, end);
    // if the second path is invalid return the valid `path`
    if (path2.last == -1) {
      return path1;
    }
    // bolierplate code to create a new node
    Node node = ParserServices.CreateNode(
        "StatementList", start, path2.last + 1, path1.node, path2.node);
    Result result = new Result { node = node, last = path2.last };
    return result;
  }

  public Result Statement(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected statement");
    if (lexemes[start].type == TokenType.OpenBrace)
      return CompoundStmt(start, end);
    if (lexemes[start].type == TokenType.if_)
      return SelectionStmt(start, end);
    if (lexemes[start].type == TokenType.while_)
      return IterationStmt(start, end);
    if (lexemes[start].type == TokenType.return_)
      return ReturnStmt(start, end);
    return ExpressionStmt(start, end);
  }

  public Result ExpressionStmt(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected expression statement");
    var path1 = Expression(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected expression statement at line: {lexemes[start].line}");
    if (lexemes[path1.last].type != TokenType.Simecolon)
      return ParserServices.CreateError(
          $"expected {';'} at line: {lexemes[path1.last].line}");
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("ExpressionStmt", start, path1.last + 1,
                                  path1.node));
  }

  public Result SelectionStmt(int start, int end) {
    if (start > end - 6)
      return ParserServices.CreateError("expected selection statement");
    if (lexemes[start++].type != TokenType.if_)
      return ParserServices.CreateError("expected if statement");
    if (lexemes[start++].type != TokenType.OpenPar)
      return ParserServices.CreateError("expected (");
    var path1 = Expression(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected expression statement at line: {lexemes[start].line}");
    if (lexemes[path1.last].type != TokenType.ClosePar)
      return ParserServices.CreateError(
          $"expected ) at line: {lexemes[path1.last].line}");
    var path2 = Statement(path1.last + 1, end);
    if (path2.last == -1)
      return ParserServices.CreateError(
          $"expected statement at line: {lexemes[path1.last + 1].line}");
    if (path2.last + 1 < end &&
        lexemes[path2.last + 1].type == TokenType.else_) {
      var path3 = Statement(path2.last + 2, end);
      if (path3.last == -1)
        return ParserServices.CreateError(
            $"expected statement at line: {lexemes[path2.last + 2].line}");
      return ParserServices.CreateResult(
          path3.last, "",
          ParserServices.CreateNode("SelectionStmt", start, path3.last + 1,
                                    path1.node, path2.node, path3.node));
    }
    return ParserServices.CreateResult(
        path2.last, "",
        ParserServices.CreateNode("SelectionStmt", start, path2.last + 1,
                                  path1.node, path2.node));
  }

  public Result IterationStmt(int start, int end) {
    if (start > end - 5)
      return ParserServices.CreateError("expected iteration statement");
    if (lexemes[start++].type != TokenType.while_)
      return ParserServices.CreateError("expected while statement");
    if (lexemes[start++].type != TokenType.OpenPar)
      return ParserServices.CreateError("expected (");
    var path1 = Expression(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected expression statement at line: {lexemes[start].line}");
    if (lexemes[path1.last].type != TokenType.ClosePar)
      return ParserServices.CreateError(
          $"expected ) at line: {lexemes[path1.last].line}");
    var path2 = Statement(path1.last + 1, end);
    if (path2.last == -1)
      return ParserServices.CreateError(
          $"expected statement at line: {lexemes[path1.last + 1].line}");
    return ParserServices.CreateResult(
        path2.last, "",
        ParserServices.CreateNode("IterationStmt", start, path2.last + 1,
                                  path1.node, path2.node));
  }

  public Result ReturnStmt(int start, int end) {
    if (start > end - 2)
      return ParserServices.CreateError("expected return statement");
    var path1 = Expression(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected expression statement at line: {lexemes[start].line}");
    if (lexemes[path1.last].type != TokenType.Simecolon)
      return ParserServices.CreateError(
          $"expected {';'} at line: {lexemes[path1.last].line}");
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("ReturnStmt", start, path1.last + 1,
                                  path1.node));
  }

  public Result Expression(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected expression");
    var path1 = Var(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected variable at line: {lexemes[start].line}");
    if (path1.last + 1 < end &&
        lexemes[path1.last + 1].type == TokenType.equal_) {
      var path2 = Expression(path1.last + 2, end);
      if (path2.last == -1)
        return ParserServices.CreateError(
            $"expected expression at line: {lexemes[path1.last + 2].line}");
      return ParserServices.CreateResult(
          path2.last, "",
          ParserServices.CreateNode("Expression", start, path2.last + 1,
                                    path1.node, path2.node));
    }
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("Expression", start, path1.last + 1,
                                  path1.node));
  }

  public Result Var(int start, int end) {
    if (start < end && lexemes[start++].type != TokenType.Ident) {
      return ParserServices.CreateError("expected variable");
    }
    Result path1;
    if (start < end && lexemes[start++].type == TokenType.OpenBracket) {
      int j = Enders.BracetClose(start, lexemes, end);
      if (j == -1)
        return ParserServices.CreateError(
            $"expected ] at line: {lexemes[start].line}");

      path1 = Expression(start, j);
      if (path1.last == -1)
        return ParserServices.CreateError(
            $"expected a valid expression in line: {lexemes[start].line}");
      return ParserServices.CreateResult(
          j, "", ParserServices.CreateNode("Var", start, j + 1, path1.node));
    }
    return ParserServices.CreateResult(
        start, "", ParserServices.CreateNode("Var", start, start + 1));
  }

  public Result SimpleExpression(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected simple expression");
    var path1 = AdditiveExpression(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected additive expression at line: {lexemes[start].line}");
    if (path1.last + 1 < end &&
        lexemes[path1.last + 1].type == TokenType.LogOp) {
      var path2 = AdditiveExpression(path1.last + 2, end);
      if (path2.last == -1)
        return ParserServices.CreateError(
            $"expected additive expression at line: {lexemes[path1.last + 2].line}");
      return ParserServices.CreateResult(
          path2.last, "",
          ParserServices.CreateNode("SimpleExpression", start, path2.last + 1,
                                    path1.node, path2.node));
    }
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("SimpleExpression", start, path1.last + 1,
                                  path1.node));
  }

  public Result AdditiveExpression(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected additive expression");
    var path1 = Term(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected term at line: {lexemes[start].line}");
    if (path1.last + 1 < end &&
        lexemes[path1.last + 1].type == TokenType.AddOp) {
      var path2 = AdditiveExpression(path1.last + 2, end);
      if (path2.last == -1)
        return ParserServices.CreateError(
            $"expected additive expression at line: {lexemes[path1.last + 2].line}");
      return ParserServices.CreateResult(
          path2.last, "",
          ParserServices.CreateNode("AdditiveExpression", start, path2.last + 1,
                                    path1.node, path2.node));
    }
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("AdditiveExpression", start, path1.last + 1,
                                  path1.node));
  }

  public Result Term(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected term");
    var path1 = Factor(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected factor at line: {lexemes[start].line}");
    if (path1.last + 1 < end &&
        lexemes[path1.last + 1].type == TokenType.MulOp) {
      var path2 = Term(path1.last + 2, end);
      if (path2.last == -1)
        return ParserServices.CreateError(
            $"expected term at line: {lexemes[path1.last + 2].line}");
      return ParserServices.CreateResult(
          path2.last, "",
          ParserServices.CreateNode("Term", start, path2.last + 1, path1.node,
                                    path2.node));
    }
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("Term", start, path1.last + 1, path1.node));
  }

  public Result Factor(int start, int end) {
    if (start > end - 1)
      return ParserServices.CreateError("expected factor");
    if (lexemes[start].type == TokenType.OpenPar) {
      var path1 = Expression(start + 1, end);
      if (path1.last == -1)
        return ParserServices.CreateError(
            $"expected expression at line: {lexemes[start].line}");
      if (path1.last + 1 < end &&
          lexemes[path1.last + 1].type != TokenType.ClosePar) {
        return ParserServices.CreateError(
            $"expected ) at line: {lexemes[path1.last].line}");
      }
      return ParserServices.CreateResult(
          path1.last + 1, "",
          ParserServices.CreateNode("Factor", start, path1.last + 2,
                                    path1.node));
    }
    var path = Var(start, end);
    if (path.last != -1)
      return path;
    path = Call(start, end);
    path = path.last != -1 ? path : Args(start, end);
    return path;
  }

  public Result Call(int start, int end) {
    if (start > end - 2)
      return ParserServices.CreateError("expected call");
    if (lexemes[start++].type != TokenType.Ident)
      return ParserServices.CreateError("expected ident");
    if (lexemes[start++].type != TokenType.OpenPar)
      return ParserServices.CreateError("expected (");
    var path1 = Args(start, end);
    if (path1.last == -1)
      return ParserServices.CreateError(
          $"expected args at line: {lexemes[start].line}");
    if (lexemes[path1.last].type != TokenType.ClosePar)
      return ParserServices.CreateError(
          $"expected ) at line: {lexemes[path1.last].line}");
    return ParserServices.CreateResult(
        path1.last, "",
        ParserServices.CreateNode("Call", start, path1.last + 1, path1.node));
  }

  public Result Args(int start, int end) {
    if (start > end)
      return new Result { last = -1 };
    var path1 = Expression(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return path1;
    }
    if (path1.last + 1 == end)
      return path1;
    if (lexemes[path1.last].type != TokenType.Comma)
      return path1;
    var path2 = Args(path1.last + 1, end);
    // if the second path is invalid return the valid `path`
    if (path2.last == -1) {
      return path1;
    }
    // bolierplate code to create a new node
    Node node = ParserServices.CreateNode("Args", start, path2.last + 1,
                                          path1.node, path2.node);
    Result result = new Result { node = node, last = path2.last };
    return result;
  }

  public Result ArgList(int start, int end) {
    if (start > end)
      return new Result { last = -1 };
    var path1 = Expression(start, end);
    // if the first path is invalid break the recursive `path`
    if (path1.last == -1) {
      return path1;
    }
    if (path1.last + 1 == end)
      return path1;
    if (lexemes[path1.last].type != TokenType.Comma)
      return path1;
    var path2 = ArgList(path1.last + 1, end);
    // if the second path is invalid return the valid `path`
    if (path2.last == -1) {
      return path1;
    }
    // bolierplate code to create a new node
    Node node = ParserServices.CreateNode("ArgList", start, path2.last + 1,
                                          path1.node, path2.node);
    Result result = new Result { node = node, last = path2.last };
    return result;
  }
}
