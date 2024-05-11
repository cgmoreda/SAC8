using SAC9.Lexer;


namespace SAC9.Parser
{
    static public class Parser
    {
        static List<Lexeme> lexemes;
        //Program
        static public NoTermReturn Parse()
        {
            Node root = new Node();
            root.Childs.Add(DeclarationList(0).node);
            root.Type = "Program";
            return new NoTermReturn { 
                node =  root,
                rightIndex = 0
            };
        }
        static public NoTermReturn DeclarationList(int leftIndex)
        {
            Node root = new Node() {};
            NoTermReturn ntrm = Declaration(leftIndex);
            Node path1 = ntrm.node;

            if (path1!=null)
            {
                root.Childs.Add(path1);
                NoTermReturn dls = DeclarationList(ntrm.rightIndex);
                if (dls.node!=null)
                {
                    root.Childs.Add(dls.node);
                    return new NoTermReturn() { rightIndex= dls.rightIndex, node= root };
                }
                else
                {
                    return new NoTermReturn() { rightIndex= -1, node= null };
                }
            }
           
            return new NoTermReturn() { rightIndex= -1, node= null };
           
        }
        static public NoTermReturn Declaration(int left)
        {
            // Declaration -> VarDeclaration | FunDeclaration
            NoTermReturn varRes = VarDeclaration(left);
            if(varRes!=null)
            {
                NoTermReturn ret = new NoTermReturn();
                ret.node.Childs.Add(ret.node);
                ret.rightIndex = varRes.rightIndex;
                return ret;
            }
            NoTermReturn funRes = FunDeclaration(left);
            if(funRes!=null)
            {
                NoTermReturn ret = new NoTermReturn();
                ret.node.Childs.Add(ret.node);
                ret.rightIndex = funRes.rightIndex;
                return ret;
            }

            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn VarDeclaration(int left)
        {
            // varDeclaration -> typeSpecifier ID ; | typeSpecifier ID [ NUM ] ;
            if (TypeSpecifier(lexemes[left])&&left+2<lexemes.Count() && lexemes[left+1].type == TokenType.Ident)
            {
                if (lexemes[left+2].type==TokenType.Simecolon)
                {

                }
            }

            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public bool TypeSpecifier(Lexeme identif)
        {
            return identif.type==TokenType.Void_||identif.type==TokenType.real_||identif.type==TokenType.num_;

        }
        static public NoTermReturn FunDeclaration(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Params(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }   
        static public NoTermReturn ParamList(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Param(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn CompoundStmt(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn LocalDeclarations(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn StatementList(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Statement(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn ExpressionStmt(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn SelectionStmt(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn IterationStmt(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn ReturnStmt(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Expression(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Var(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn SimpleExpression(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn AdditiveExpression(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Term(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Factor(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Call(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn Args(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn ArgList(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn addOps(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }

        static public NoTermReturn multOps(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
        static public NoTermReturn relOps(int left)
        {
            return new NoTermReturn() { rightIndex= -1, node= null };
        }
    }
}
