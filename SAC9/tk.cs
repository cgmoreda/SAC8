using SAC9.Parser;
using SAC9.Lexer;

namespace SAC9
{
    public interface Itk {
        public Node tree(string source);
    }

    public class tk 
    {
        public string tree(string source)
        {
           
            Parser.Parser parser = new Parser.Parser(Lexer.Lexer.scan(source));
            var res= parser.Parse();
            Console.WriteLine(res.error);
            return res.error;

        }

        public List<Lexeme> lex(string source)
        {

            Parser.Parser parser = new Parser.Parser(Lexer.Lexer.scan(source));
            var res = Lexer.Lexer.scan(source);
            return res.ToList();

        }

    }
}
