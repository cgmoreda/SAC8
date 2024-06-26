﻿using SAC9.Parser;
using SAC9.Lexer;

namespace SAC9
{
    public interface Itk {
        public Node tree(string source);
    }

    public class Tk 
    {
        public string tree(string source)
        {
           
            Parser.Parser parser = new Parser.Parser(Lexer.Lexer.scan(source));
            var res= parser.Parse();
            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(res));
            return res.error;

        }

        public List<Lexeme> lex(string source)
        {

            Parser.Parser parser = new Parser.Parser(Lexer.Lexer.scan(source));
            var res = Lexer.Lexer.scan(source);
            return res.ToList();

        }
        
        public static void main(string[] args)
        {

        }
    }
}
