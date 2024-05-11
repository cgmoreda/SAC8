using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SAC9.Lexer
{
    static public class Lexer
    {
        static public List<Lexeme> scan(string input)
        {
            int countLine = 0;
            int countColumn = 0;
            List<Lexeme> result = new List<Lexeme>();
            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

            }
            return result;
        }
        public static bool isNumber(string input)
        {
            return isReal(input) || isInt(input);
        }
        public static bool isReal(string input)
        {
            return Regex.IsMatch(input, @"^(\+|-)?\d+(\.\d+)?$");
        }
        public static bool isInt(string input)
        {
            return Regex.IsMatch(input, @"^(\+|-)?\d+$");
        }
        public static bool isIdent(string input)
        {
            string letter = "(ا|ب|ت|ث|ج|ح|خ|د|ذ|ر|ز|س|ش|ص|ض|ط|ظ|ع|غ|ف|ق|ك|ل|م|ن|ه|و|ى)";
            return Regex.IsMatch(input, $@"^{letter}[{letter}0-9]*$");
        }
    }
}
