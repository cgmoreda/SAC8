using SAC9.Lexer;
using SAC9.Parser;
using Newtonsoft.Json;

Console.WriteLine("Hello, World!");
string input = @"صحيح وو () {
صحيح   نمتك;
}";
Parser.lexemes = Lexer.scan(input);
var result = Parser.Parse();
Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));