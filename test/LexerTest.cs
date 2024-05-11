using SAC9.Lexer;

// create a test
public class LexerTest {
  [Test]
  public void Ident() {
    Assert.True(Lexer.isIdent("ا"));
    Assert.True(Lexer.isIdent("ا1"));
    Assert.False(Lexer.isIdent("1ا"));
  }
  [Test]
  public void Number(){
    Assert.True(Lexer.isNumber("1.23"));
    Assert.False(Lexer.isNumber("1..2"));
    Assert.False(Lexer.isNumber("a2.3"));
    Assert.False(Lexer.isNumber("2.3a"));
    Assert.True(Lexer.isNumber("23"));
    Assert.False(Lexer.isNumber(""));
    Console.WriteLine("great number");
  }
}
