using System.Text.RegularExpressions;

namespace SAC9.Lexer
{
    public static class Lexer
    {
        // Regular expression for matching Arabic letters (unicode range for Arabic characters)
        const string letter = "[\u0621-\u064A]";

        // Main function to scan the input string and return a list of lexemes
        public static List<Lexeme> scan(string input)
        {
            input += ' ';  // Add a space at the end to help handle the last token
            int countLine = 0;  // Track the current line number
            int countColumn = 0;  // Track the current column number
            List<Lexeme> result = new List<Lexeme>();  // List to store the resulting lexemes

            // Loop through each character in the input string
            for (int i = 0; i < input.Length; i++)
            {
                char cur = input[i];  // Current character in the input

                // Handle line breaks and update line and column counters
                if (cur == '\n')
                {
                    countLine++;
                    countColumn = i + 1;
                    continue;
                }

                int st = i - countColumn;  // Calculate column offset for the token

                // Check if the current character is part of a number (integer or real)
                if (isNumber("" + cur))
                {
                    string token = "" + cur;  // Start building the token
                    // Continue to append characters as long as they form part of a number
                    while (isNumber(token) && !token.Contains('\n'))
                    {
                        token += input[++i];  // Append next character
                    }
                    token = token.Substring(0, token.Length - 1);  // Remove the extra character
                    i--;  // Decrement to compensate for the last increment
                    result.Add(new Lexeme()
                    {
                        type = TokenType.Number,  // Token type for numbers
                        value = token,  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }

                // Check if the current character starts an identifier (variable, function, etc.)
                if (isIdent("" + cur))
                {
                    string token = "" + cur;  // Start building the identifier
                    // Continue to append characters as long as they form part of an identifier
                    while (isIdent(token) && !token.Contains('\n'))
                    {
                        token += input[++i];  // Append next character
                    }
                    token = token.Substring(0, token.Length - 1);  // Remove the extra character
                    i--;  // Decrement to compensate for the last increment
                    result.Add(new Lexeme()
                    {
                        type = ResWord(token),  // Check if the identifier is a reserved word
                        value = token,  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }

                // Handle specific characters (e.g., punctuation marks, operators)
                if (cur == ';' || cur == '؛')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.Simecolon,  // Token type for semicolon
                        value = ";",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                }
                if (cur == ',' || cur == '،')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.Comma,  // Token type for comma
                        value = ",",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                }
                if (cur == '=')
                {
                    // Handle the '==' operator (equality check)
                    char next = input[i + 1];
                    if (next == '=')
                    {
                        result.Add(new Lexeme()
                        {
                            type = TokenType.LogOp,  // Logical operator token type
                            value = "==",  // Token value for equality operator
                            line = countLine,  // Line number
                            column = st  // Column number
                        });
                        i++;  // Skip the next character since it's part of '=='
                    }
                    else
                        result.Add(new Lexeme()
                        {
                            type = TokenType.equal_,  // Token type for single equality sign
                            value = "=",  // Token value
                            line = countLine,  // Line number
                            column = st  // Column number
                        });
                    continue;
                }

                // Handle braces, brackets, and parentheses
                if (cur == '{')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.OpenBrace,  // Open brace token type
                        value = "{",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == '}')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.CloseBrace,  // Close brace token type
                        value = "}",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == '[')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.OpenBracket,  // Open bracket token type
                        value = "[",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == ']')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.CloseBracket,  // Close bracket token type
                        value = "]",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == '(')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.OpenPar,  // Open parenthesis token type
                        value = "(",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }

                if (cur == ')')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.ClosePar,  // Close parenthesis token type
                        value = ")",  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }

                // Handle arithmetic and logical operators
                if (cur == '+' || cur == '-')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.AddOp,  // Arithmetic operator token type
                        value = "" + cur,  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == '*' || cur == '/')
                {
                    result.Add(new Lexeme()
                    {
                        type = TokenType.MulOp,  // Arithmetic operator token type
                        value = "" + cur,  // Token value
                        line = countLine,  // Line number
                        column = st  // Column number
                    });
                    continue;
                }
                if (cur == '<' || cur == '>' || cur == '!')
                {
                    // Handle logical operators (e.g., '<', '>', '!=')
                    char next = input[i + 1];
                    if (next == '=')
                    {
                        result.Add(new Lexeme()
                        {
                            type = TokenType.LogOp,  // Logical operator token type
                            value = "" + cur + "=",
                            line = countLine,
                            column = st
                        });
                        i++;  // Skip the next character as part of the '!=' or '<=' operator
                    }
                    else
                    {
                        result.Add(new Lexeme()
                        {
                            type = TokenType.LogOp,  // Logical operator token type
                            value = "" + cur,
                            line = countLine,
                            column = st
                        });
                    }
                    continue;
                }
            }

            return result;  // Return the list of lexemes after scanning the input
        }

        // Helper function to check if the input string is a valid number (integer or real)
        public static bool isNumber(string input)
        {
            return isReal(input) || isInt(input);
        }

        // Check if the input is a valid real number (floating-point number)
        public static bool isReal(string input)
        {
            return Regex.IsMatch(input, @"^(\+|-)?\d+(\.\d+)?$") && !input.Contains('\n');
        }

        // Check if the input is a valid integer
        public static bool isInt(string input)
        {
            return Regex.IsMatch(input, @"^(\+|-)?\d+$") && !input.Contains('\n');
        }

        // Check if the input is a valid identifier (variable or function name)
        public static bool isIdent(string input)
        {
            return Regex.IsMatch(input, ($"^{letter}({letter}|[0-9])*$")) && !input.Contains('\n');
        }

        // Determine the token type based on the input string (for reserved words)
        public static TokenType Re
