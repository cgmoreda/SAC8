namespace SAC9.Lexer;

public enum TokenType {
  Ident,
  Number,
  AddOp,
  MulOp,
  LogOP,
  OpenPar,
  ClosePar,
  OpenBrace,
  CloseBrace,
  OpenBracket,
  CloseBracket,
  Simecolon,
  Comma,
  Void_,
  if_,
  else_,
  while_,
  return_,
  real_,
  num_,
  equal_,
  invalid
}

public record Lexeme {
  public string value { set; get; } = "";
  public TokenType type { set; get; } = 0;
  public int line { set; get; }
  public int column { set; get; }
}
