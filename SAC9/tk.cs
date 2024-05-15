

using SAC9.Parser;
using SAC9.Lexer;

namespace SAC9
{
    public interface Itk {
        public Node tree(string source);
    }

    public class tk 
    {
        public Node tree(string source)
        {
            Parser.Parser.lexemes =  Lexer.Lexer.scan(source);
            return Parser.Parser.Parse().node;
        }
        
    }
}
