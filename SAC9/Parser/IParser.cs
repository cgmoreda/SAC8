using SAC9.Lexer;

namespace SAC9.Parser;

public interface IParser {
  public Result Parse();

  public Result DeclarationList(int start, int end);

  public Result Declaration(int start, int end);

  public Result VarDeclaration(int start, int end);

  public Result FunDeclaration(int start, int end);

  public Result Params(int start, int end);

  public Result ParamList(int start, int end);

  public Result Param(int start, int end);

  public Result CompoundStmt(int start, int end);

  public Result LocalDeclarations(int start, int end);

  public Result StatementList(int start, int end);

  public Result Statement(int start, int end);

  public Result ExpressionStmt(int start, int end);

  public Result SelectionStmt(int start, int end);

  public Result IterationStmt(int start, int end);

  public Result ReturnStmt(int start, int end);

  public Result Expression(int start, int end);

  public Result Var(int start, int end);

  public Result SimpleExpression(int start, int end);

  public Result AdditiveExpression(int start, int end);

  public Result Term(int start, int end);

  public Result Factor(int start, int end);

  public Result Call(int start, int end);

  public Result Args(int start, int end);

  public Result ArgList(int start, int end);
}

public interface IParserServices {
  public static abstract bool TypeSpecifier(Lexeme lexeme);
  public static abstract bool addOp(Lexeme lexeme);
  public static abstract bool mulOp(Lexeme lexeme);
  public static abstract bool relOp(Lexeme lexeme);
}
