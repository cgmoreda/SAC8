using SAC9.Lexer;

// create a test
public class LexerTest {
  void check(List<Lexeme> expect, List<Lexeme> found) {
    Assert.True(
        expect.Count == found.Count,
        $"expected size: {expect.Count} || found size{found.Count}:: {found[0]}");
    for (int i = 0; i < found.Count; i++) {
      Assert.True(
          expect[i] == found[i],
          $"error in{i}'th token: expected {expect[i]} but found {found[i]}");
    }
  }

  [Test]
  public void Ident() {
    Assert.True(Lexer.isIdent("ا"));
    Assert.True(Lexer.isIdent("ا1"));
    Assert.False(Lexer.isIdent("1ا"));
  }

  [Test]
  public void Number() {
    Assert.True(Lexer.isNumber("1.23"));
    Assert.False(Lexer.isNumber("1..2"));
    Assert.False(Lexer.isNumber("a2.3"));
    Assert.False(Lexer.isNumber("2.3a"));
    Assert.True(Lexer.isNumber("23"));
    Assert.False(Lexer.isNumber(""));
    Console.WriteLine("great number");
  }

  [Test]
  public void ReservedWords() {
    Assert.True(Lexer.ResWord("خالى") == TokenType.Void_, "error in void");
    Assert.True(Lexer.ResWord("خالي") == TokenType.Void_, "error in void");
    Assert.True(Lexer.ResWord("اذا") == TokenType.if_, "error in if");
    Assert.True(Lexer.ResWord("اخر") == TokenType.else_, "error in else");
    Assert.True(Lexer.ResWord("بينما") == TokenType.while_, "error in while");
    Assert.True(Lexer.ResWord("ارجع") == TokenType.return_, "error in return ");
    Assert.True(Lexer.ResWord("حقيقي") == TokenType.real_, "error in real");
    Assert.True(Lexer.ResWord("صحيح") == TokenType.num_, "error in int");
    Assert.True(Lexer.ResWord("احا") == TokenType.Ident, "error in a7a");
  }

  [Test]
  public void scan1() {
    string input = "صحيح رقم1 = 5;";
    List<Lexeme> expect = new List<Lexeme>() {
      new Lexeme() { type = TokenType.num_, value = "صحيح", line = 0,
                     column = 0 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 0,
                     column = 5 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 0,
                     column = 10 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 0,
                     column = 12 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 0,
                     column = 13 }
    };
    List<Lexeme> found = Lexer.scan(input);
    check(expect, found);
  }

  [Test]
  public void scan2() {
    string input = String.Join("\n", new string[] {
      "صحيح رقم1 = 5;",
      "صحيح احمد[5] = {0,1,2,3,4};",
    });
    List<Lexeme> expect = new List<Lexeme>() {
      new Lexeme() { type = TokenType.num_, value = "صحيح", line = 0,
                     column = 0 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 0,
                     column = 5 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 0,
                     column = 10 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 0,
                     column = 12 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 0,
                     column = 13 },
      new Lexeme() { type = TokenType.num_, value = "صحيح", line = 1,
                     column = 0 },
      new Lexeme() { type = TokenType.Ident, value = "احمد", line = 1,
                     column = 5 },
      new Lexeme() { type = TokenType.OpenBracket, value = "[", line = 1,
                     column = 9 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 1,
                     column = 10 },
      new Lexeme() { type = TokenType.CloseBracket, value = "]", line = 1,
                     column = 11 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 1,
                     column = 13 },
      new Lexeme() { type = TokenType.OpenBrace, value = "{", line = 1,
                     column = 15 },
      new Lexeme() { type = TokenType.Number, value = "0", line = 1,
                     column = 16 },
      new Lexeme() { type = TokenType.Comma, value = ",", line = 1,
                     column = 17 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 1,
                     column = 18 },
      new Lexeme() { type = TokenType.Comma, value = ",", line = 1,
                     column = 19 },
      new Lexeme() { type = TokenType.Number, value = "2", line = 1,
                     column = 20 },
      new Lexeme() { type = TokenType.Comma, value = ",", line = 1,
                     column = 21 },
      new Lexeme() { type = TokenType.Number, value = "3", line = 1,
                     column = 22 },
      new Lexeme() { type = TokenType.Comma, value = ",", line = 1,
                     column = 23 },
      new Lexeme() { type = TokenType.Number, value = "4", line = 1,
                     column = 24 },
      new Lexeme() { type = TokenType.CloseBrace, value = "}", line = 1,
                     column = 25 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 1,
                     column = 26 }
    };
    List<Lexeme> found = Lexer.scan(input);
    check(expect, found);
  }

  [Test]
  public void scan3() {
    string input =String.Join("\n", new string[] {
      "صحيح رقم1 = 5;",
      "بينما (رقم1 <= 5 )",
      "رقم 1 = رقم1 + 1+2;",
    });
    List<Lexeme> expect = new List<Lexeme>() {
      // Lexemes for line1
      new Lexeme() { type = TokenType.num_, value = "صحيح", line = 0,
                     column = 0 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 0,
                     column = 5 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 0,
                     column = 10 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 0,
                     column = 12 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 0,
                     column = 13 },
      // Lexemes for line2
      new Lexeme() { type = TokenType.while_, value = "بينما", line = 1,
                     column = 0 },
      new Lexeme() { type = TokenType.OpenPar, value = "(", line = 1,
                     column = 6 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 1,
                     column = 7 },
      new Lexeme() { type = TokenType.LogOp, value = "<=", line = 1,
                     column = 13 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 1,
                     column = 16 },
      new Lexeme() { type = TokenType.ClosePar, value = ")", line = 1,
                     column = 18 },
      // Lexemes for line3
      new Lexeme() { type = TokenType.Ident, value = "رقم", line = 2,
                     column = 0 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 2,
                     column = 4 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 2,
                     column = 6 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 2,
                     column = 8 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 2,
                     column = 13 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 2,
                     column = 14 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 2,
                     column = 15 },
      new Lexeme() { type = TokenType.Number, value = "2", line = 2,
                     column = 16 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 2,
                     column = 17 }
    };

    List<Lexeme> found = Lexer.scan(input);
    check(expect, found);
  }

  [Test]
  public void scan4() {
    string input = String.Join("\n", new string[] {
      "صحيح رقم1 = 5;",
      "اذا (رقم1 <= 5 )",
      "رقم 1 = رقم1 + 1+2;",
      "اخر",
      "رقم 1 = رقم1 + 1+2;",
    });
    List<Lexeme> expect = new List<Lexeme>() {
      // Lexemes for line1
      new Lexeme() { type = TokenType.num_, value = "صحيح", line = 0,
                     column = 0 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 0,
                     column = 5 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 0,
                     column = 10 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 0,
                     column = 12 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 0,
                     column = 13 },
      // Lexemes for line2
      new Lexeme() { type = TokenType.if_, value = "اذا", line = 1,
                     column = 0 },
      new Lexeme() { type = TokenType.OpenPar, value = "(", line = 1,
                     column = 4 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 1,
                     column = 7 },
      new Lexeme() { type = TokenType.LogOp, value = "<=", line = 1,
                     column = 13 },
      new Lexeme() { type = TokenType.Number, value = "5", line = 1,
                     column = 16 },
      new Lexeme() { type = TokenType.ClosePar, value = ")", line = 1,
                     column = 18 },
      // Lexemes for line3
      new Lexeme() { type = TokenType.Ident, value = "رقم", line = 2,
                     column = 0 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 2,
                     column = 4 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 2,
                     column = 6 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 2,
                     column = 8 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 2,
                     column = 13 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 2,
                     column = 14 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 2,
                     column = 15 },
      new Lexeme() { type = TokenType.Number, value = "2", line = 2,
                     column = 16 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 2,
                     column = 17 },
      // lexems for line 4

      new Lexeme() { type = TokenType.else_, value = "اخر", line = 3,
                     column = 0 },
      // lexems for line 5
      new Lexeme() { type = TokenType.Ident, value = "رقم", line = 4,
                     column = 0 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 4,
                     column = 4 },
      new Lexeme() { type = TokenType.equal_, value = "=", line = 4,
                     column = 6 },
      new Lexeme() { type = TokenType.Ident, value = "رقم1", line = 4,
                     column = 8 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 4,
                     column = 13 },
      new Lexeme() { type = TokenType.Number, value = "1", line = 4,
                     column = 14 },
      new Lexeme() { type = TokenType.AddOp, value = "+", line = 4,
                     column = 15 },
      new Lexeme() { type = TokenType.Number, value = "2", line = 4,
                     column = 16 },
      new Lexeme() { type = TokenType.Simecolon, value = ";", line = 4,
                     column = 17 },
    };

    List<Lexeme> found = Lexer.scan(input);
    check(expect, found);
  }
}
