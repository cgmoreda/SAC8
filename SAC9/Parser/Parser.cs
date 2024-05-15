using System.Data.Common;
using System.Xml.Linq;
using SAC9.Lexer;

namespace SAC9.Parser;
public class Parser{
  public List<Lexeme> lexemes;
  public Parser(List<Lexeme> lexemes){
    this.lexemes = lexemes;
  }
}
