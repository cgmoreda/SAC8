using System.Text.RegularExpressions;

namespace SAC9.Lexer {
public static class Lexer {
  const string letter = "[\u0600-\u06FF]";

  public static List<Lexeme> scan(string input) {
    int countLine = 0;
    int countColumn = 0;
    List<Lexeme> result = new List<Lexeme>();
    for (int i = 0; i < input.Length; i++) {
      char cur = input[i];
      if (cur == '\n') {
        countLine++;
        countColumn = i + 1;
      }

      int st = i - countColumn;
      if (isNumber("" + cur)) {
        string token = "" + cur;
        while (isNumber(token)) {
          token += input[++i];
        }
        token = token.Substring(0, token.Length - 1);
        i--;
        result.Add(new Lexeme() { type = TokenType.Number, value = token,
                                  line = countLine, column = st });
        continue;
      }
      if (isIdent("" + cur)) {
        string token = "" + cur;
        while (isIdent(token)) {
          token += input[++i];
        }
        token = token.Substring(0, token.Length - 1);
        i--;
        result.Add(new Lexeme() { type = ResWord(token), value = token,
                                  line = countLine, column = st });
        continue;
      }
      if (cur == ';') {
        result.Add(new Lexeme() { type = TokenType.Simecolon, value = ";",
                                  line = countLine, column = st });
      }
      if (cur == ',') {
        result.Add(new Lexeme() { type = TokenType.Comma, value = ",",
                                  line = countLine, column = st });
      }
      if (cur == '=') {
        char next = input[i + 1];
        if (next == '=') {
          result.Add(new Lexeme() { type = TokenType.LogOp, value = "==",
                                    line = countLine, column = st });
          i++;
        } else
          result.Add(new Lexeme() { type = TokenType.equal_, value = "=",
                                    line = countLine, column = st });
        continue;
      }
      if (cur == '{') {
        result.Add(new Lexeme() { type = TokenType.OpenBrace, value = "{",
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '}') {
        result.Add(new Lexeme() { type = TokenType.CloseBrace, value = "}",
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '[') {
        result.Add(new Lexeme() { type = TokenType.OpenBracket, value = "[",
                                  line = countLine, column = st });
        continue;
      }
      if (cur == ']') {
        result.Add(new Lexeme() { type = TokenType.CloseBracket, value = "]",
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '(') {
        result.Add(new Lexeme() { type = TokenType.OpenPar, value = "(",
                                  line = countLine, column = st });
        continue;
      }

      if (cur == ')') {
        result.Add(new Lexeme() { type = TokenType.ClosePar, value = ")",
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '+' || cur == '-') {
        result.Add(new Lexeme() { type = TokenType.AddOp, value = ""+ cur,
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '*' || cur == '/') {
        result.Add(new Lexeme() { type = TokenType.MulOp, value = ""+ cur,
                                  line = countLine, column = st });
        continue;
      }
      if (cur == '<' || cur == '>' || cur == '!') {
        char next = input[i + 1];
        if (next == '=') {
          result.Add(new Lexeme() { type = TokenType.LogOp, value = ""+ cur + "=",
                                    line = countLine, column = st });
          i++;
        } else {
          result.Add(new Lexeme() { type = TokenType.LogOp, value = ""+ cur,
                                    line = countLine, column = st });
        }
        continue;
      }
    }
    return result;
  }

  public static bool isNumber(string input) {
    return isReal(input) || isInt(input);
  }

  public static bool isReal(string input) {
    return Regex.IsMatch(input, @"^(\+|-)?\d+(\.\d+)?$");
  }

  public static bool isInt(string input) {
    return Regex.IsMatch(input, @"^(\+|-)?\d+$");
  }

  public static bool isIdent(string input) {
    return Regex.IsMatch(input, $@"^{letter}({letter}|[0-9])*$");
  }

  public static TokenType ResWord(string input) {
    if (input == "اذا")
      return TokenType.if_;
    else if (input == "اخر")
      return TokenType.else_;
    else if (input == "بينما")
      return TokenType.while_;
    else if (input == "ارجع")
      return TokenType.return_;
    else if (input == "حقيقي")
      return TokenType.real_;
    else if (input == "صحيح")
      return TokenType.num_;
    else if (input == "خالى" || input == "خالي")
      return TokenType.Void_;
    else
      return TokenType.Ident;
  }
}
}
