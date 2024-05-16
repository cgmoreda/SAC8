Class TokenType

Enumeration TokenType
An enumeration is a distinct type consisting of a set of named constants called the enumerator list. In this code, TokenType is an enumeration representing the different types of tokens that can be identified during lexical analysis. Each constant represents a specific token type:
•	Ident: Identifier token
•	Number: Numeric constant token
•	AddOp: Addition operator token
•	MulOp: Multiplication operator token
•	LogOp: Logical operator token
•	OpenPar: Opening parenthesis token
•	ClosePar: Closing parenthesis token
•	OpenBrace: Opening brace token
•	CloseBrace: Closing brace token
•	OpenBracket: Opening bracket token
•	CloseBracket: Closing bracket token
•	Simecolon: Semicolon token
•	Comma: Comma token
•	Void_: void keyword token
•	if_: if keyword token
•	else_: else keyword token
•	while_: while keyword token
•	return_: return keyword token
•	real_: real keyword token
•	num_: num keyword token
•	equal_: Equality operator token
•	invalid: Invalid token
Record Lexeme
A record is a reference type that provides automatic implementations of standard functionality for data storage and retrieval. In this case, the Lexeme record represents a lexical element identified during lexical analysis. It contains the following properties:
•	value: A string representing the value of the lexeme.
•	type: A TokenType representing the type of the lexeme.
•	line: An integer representing the line number where the lexeme was found.
•	column: An integer representing the column number where the lexeme starts. Constants and Fields:
•	The constant letter represents a regular expression pattern for matching Arabic letters.
•	Methods:
•	scan(string input): This method takes an input string and scans it character by character to identify lexemes. It iterates through the input string, recognizing tokens such as numbers, identifiers, punctuation symbols, operators, and reserved words. Each recognized lexeme is instantiated as a Lexeme object and added to a list, which is then returned.
•	isNumber(string input): This method checks if the input string represents a number, either integer or real.
•	isReal(string input): This method checks if the input string represents a real number.
•	isInt(string input): This method checks if the input string represents an integer.
•	isIdent(string input): This method checks if the input string represents a valid identifier according to a specified pattern.
•	ResWord(string input): This method identifies reserved words in the input string and returns the corresponding TokenType.
•	Token Recognition:
•	The method recognizes various types of tokens, including numbers, identifiers, punctuation symbols (;, ,, [, ], (, )), arithmetic operators (+, -, *, /), relational operators (<, >, <=, >=, !=, ==), and reserved words (e.g., اذا, اخر, بينما, ارجع, حقيقي, صحيح, خالى).
•	Lexeme Generation:
•	For each recognized token, a Lexeme object is created with the appropriate TokenType, value, line number, and column number. This Lexeme object is then added to the list of results.
•	

Interfaces
IParser Interface:
Description: Defines methods for parsing syntactic constructs of source code.
Methods:
1.	Parse(): Initiates parsing process.
2.	DeclarationList(int start, int end): Parses list of declarations.
3.	Declaration(int start, int end): Parses single declaration.
4.	VarDeclaration(int start, int end): Parses variable declaration.
5.	FunDeclaration(int start, int end): Parses function declaration.
6.	Params(int start, int end): Parses function parameters.
7.	ParamList(int start, int end): Parses list of parameters.
8.	Param(int start, int end): Parses single parameter.
9.	CompoundStmt(int start, int end): Parses compound statement.
10.	LocalDeclarations(int start, int end): Parses local variable declarations.
11.	StatementList(int start, int end): Parses list of statements.
12.	Statement(int start, int end): Parses single statement.
13.	ExpressionStmt(int start, int end): Parses expression statement.
14.	SelectionStmt(int start, int end): Parses selection statement.
15.	IterationStmt(int start, int end): Parses iteration statement.
16.	ReturnStmt(int start, int end): Parses return statement.
17.	Expression(int start, int end): Parses expression.
18.	Var(int start, int end): Parses variable.
19.	SimpleExpression(int start, int end): Parses simple expression.
20.	AdditiveExpression(int start, int end): Parses additive expression.
21.	Term(int start, int end): Parses term.
22.	Factor(int start, int end): Parses factor.
23.	Call(int start, int end): Parses function call.
24.	Args(int start, int end): Parses function call arguments.
25.	ArgList(int start, int end): Parses list of arguments.
IParserServices Interface:
Description: Provides auxiliary services for parsing.
Methods:
1.	TypeSpecifier(Lexeme lexeme): Checks if lexeme represents a type specifier.
2.	addOp(Lexeme lexeme): Checks if lexeme represents an addition operator.
3.	mulOp(Lexeme lexeme): Checks if lexeme represents a multiplication operator.
4.	relOp(Lexeme lexeme): Checks if lexeme represents a relational operator.
5.	CreateNode(string type, int left, int right, params Node[] childrens): Creates AST node.

IParserServices Interface:
1.	Description: Defines auxiliary services required by the parser during parsing.
2.	Methods:
3.	TypeSpecifier(Lexeme lexeme): Determines if a given lexeme represents a type specifier.
4.	addOp(Lexeme lexeme): Determines if a given lexeme represents an addition operator.
5.	mulOp(Lexeme lexeme): Determines if a given lexeme represents a multiplication operator.
6.	relOp(Lexeme lexeme): Determines if a given lexeme represents a relational operator.
7.	CreateNode(string type, int left, int right, params Node[] childrens): Creates a node for the AST with specified type, left and right positions, and optional children nodes.
8.	Classes:
9.	Node Class:
10.	Description: Represents a node in the abstract syntax tree (AST) corresponding to a syntactic construct of the source code.
11.	Properties:
12.	left (int): Position of the leftmost character associated with the node.
13.	right (int): Position of the rightmost character associated with the node.
14.	Type (string): Type of the node, indicating the syntactic construct it represents.
15.	Children (List<Node>): List of child nodes of the current node.
16.	Result Class:
17.	Description: Represents the result of a parsing operation, encapsulating information such as the position of the last character parsed, encountered error messages, and the resulting node.
18.	Properties:
19.	last (int): Position of the last character parsed successfully.
20.	error (string): Error message if parsing encountered an error.
21.	node (Node?): Resulting node of the parsing operation, which may be null if parsing failed.
22.	
