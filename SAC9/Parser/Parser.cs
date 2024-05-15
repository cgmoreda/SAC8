using System.Data.Common;
using System.Xml.Linq;
using SAC9.Lexer;

namespace SAC9.Parser {
public static class Parser {
  public static List<Lexeme> lexemes { get; set; } = new List<Lexeme>();

  // Program
  static public Result Parse() { return DeclarationList(0); }

  public static Result DeclarationList(int leftIndex) {
    if (leftIndex >= lexemes.Count()) {
      return new Result() { rightIndex = leftIndex, node = null };
    }
    Node root = new Node() {};
    Result ntrm = Declaration(leftIndex);
    Node? path1 = ntrm.node;

    if (ntrm.rightIndex != -1) {
      if (path1 != null)
        root.Childs.Add(path1);
      Result dls = DeclarationList(ntrm.rightIndex);
      if (dls.node != null) {
        root.Childs.Add(dls.node);
        return new Result() { rightIndex = dls.rightIndex, node = root,
                                    error = dls.error };
      } else {
        return ntrm;
      }
    }

    return new Result() { rightIndex = -1, node = null,
                                error =
                                    $"parsing error starting at {leftIndex}" };
  }

  public static Result Declaration(int left) {
    // Declaration -> VarDeclaration | FunDeclaration
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result varRes = VarDeclaration(left);
    if (varRes.node != null) {
      Result ret = new Result();
      ret.node.Childs.Add(varRes.node);
      ret.node.Type = "Declaration";
      ret.rightIndex = varRes.rightIndex;
      return ret;
    }
    Result funRes = FunDeclaration(left);
    if (funRes.node != null) {
      Result ret = new Result();
      ret.node.Childs.Add(funRes.node);
      ret.node.Type = "Declaration";
      ret.rightIndex = funRes.rightIndex;
      return ret;
    }

    return new Result() { rightIndex = -1, node = null };
  }

  public static Result VarDeclaration(int left) {
    // varDeclaration -> typeSpecifier ID ; | typeSpecifier ID [ NUM ] ;
    if (left >= lexemes.Count() || left < 0) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "VarDeclaration",
    };
    if (TypeSpecifier(lexemes[left])) {
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (lexemes[left + 1].type == TokenType.Ident) {
        if (left + 2 >= lexemes.Count()) {
          return new Result {
            error = $"unexpected EOF\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });

        if (lexemes[left + 2].type == TokenType.Simecolon) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left + 2].value,
          });
          return new Result {
            rightIndex = left + 3,
            node = _node,
          };
        } else if (lexemes[2].type == TokenType.OpenBracket) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left + 2].value,
          });
          if (left + 5 >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = left + 1,
              node = _node,
            };
          }
          if (lexemes[left + 3].type == TokenType.Number) {
            _node.Childs.Add(new Node() {
              Type = lexemes[left + 3].value,
            });
            if (lexemes[left + 4].type == TokenType.CloseBracket) {
              _node.Childs.Add(new Node() {
                Type = lexemes[left + 4].value,
              });
              if (lexemes[left + 5].type == TokenType.Simecolon) {
                _node.Childs.Add(new Node() {
                  Type = lexemes[left + 5].value,
                });
                return new Result {
                  rightIndex = left + 6,
                  node = _node,
                };
              } else {
                return new Result {
                  error =
                      $"Expected ';' at line {lexemes[left + 5].line} at column: {lexemes[left + 5].column}\n",
                  rightIndex = left + 6,
                  node = _node,
                };
              }
            } else {
              return new Result {
                error =
                    $"Expected ']' at line {lexemes[left + 4].line} at column: {lexemes[left + 4].column}\n",
                rightIndex = left + 5,
                
                node = _node,
              };
            }
          } else {
            return new Result {
              error =
                  $"Expected Number at line {lexemes[left + 3].line} at column: {lexemes[left + 3].column}\n",
              rightIndex = left + 4,
              node = _node,
            };
          }
        } else if (lexemes[2].type == TokenType.OpenPar) {
          return new Result {
            error = $"goto declarefunction",
            rightIndex = left + 4,
            node = null,
          };
        }
      } else {
        int line = lexemes[left].line;
        int column = lexemes[left].column + lexemes[left].value.Length + 1;

        for (; left < lexemes.Count(); left++) {
          if (lexemes[left].type == TokenType.Simecolon) {
            return new Result {
              error =
                  $"Expected Identifier at line {line} at column: {column}\n",
              rightIndex = left + 1,
              node = _node,
            };
          }
        }
      }
    } else {
      int line = lexemes[left].line;
      int column = lexemes[left].column;
      for (; left < lexemes.Count(); left++) {
        if (lexemes[left].type == TokenType.Simecolon) {
          return new Result {
            error =
                $"Expected type-specifier at line {line} at column: {column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      }
    }

    return new Result() { rightIndex = -1, node = null };
  }

  public static bool TypeSpecifier(Lexeme identif) {
    return identif.type == TokenType.Void_ || identif.type == TokenType.real_ ||
           identif.type == TokenType.num_;
  }

  public static Result FunDeclaration(int left) {
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    // FunDeclaration -> typeSpecifier ID ( params ) compoundStmt
    Node _node = new Node() {
      Type = "FunDeclaration",
    };
    if (TypeSpecifier(lexemes[left])) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.Ident) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        if (left + 2 >= lexemes.Count()) {
          return new Result {
            error = $"unexpected EOF\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
        if (lexemes[left + 2].type == TokenType.OpenPar) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left + 2].value,
          });
          Result paramsRes = Params(left + 3);
          if (paramsRes.rightIndex != -1 ) {
            if (paramsRes.node != null)
              _node.Childs.Add(paramsRes.node);
            if (paramsRes.rightIndex >= lexemes.Count()) {
              return new Result {
                error = $"unexpected EOF\n",
                rightIndex = paramsRes.rightIndex,
                node = _node,
              };
            }
            if (lexemes[paramsRes.rightIndex].type == TokenType.ClosePar) {
              _node.Childs.Add(new Node() {
                Type = lexemes[paramsRes.rightIndex].value,
              });
              Result compoundStmtRes =
                  CompoundStmt(paramsRes.rightIndex + 1);
              if (compoundStmtRes.node != null) {
                _node.Childs.Add(compoundStmtRes.node);
                return new Result {
                  rightIndex = compoundStmtRes.rightIndex,
                  node = _node,
                };
              } else {
                return new Result {
                  error =
                      $"Expected compound statement at line {lexemes[paramsRes.rightIndex].line} at column: {lexemes[paramsRes.rightIndex].column}\n",
                  rightIndex = paramsRes.rightIndex + 1,
                  node = _node,
                };
              }
            } else {
              return new Result {
                error =
                    $"Expected close params at line {lexemes[paramsRes.rightIndex].line} at column: {lexemes[paramsRes.rightIndex].column}\n",
                rightIndex = paramsRes.rightIndex + 1,
                node = _node,
              };
            }
          } else {
            return new Result {
              error =
                  $"Expected params at line {lexemes[left + 3].line} at column: {lexemes[left + 3].column}\n",
              rightIndex = left + 4,
              node = _node,
            };
          }
        }
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Params(int left) {
    // Params -> paramList | void
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result paramListRes = ParamList(left);
    if (paramListRes.node != null) {
      return paramListRes;
    } else {
      return new Result {
        rightIndex = left + 1,
        node =
            new Node() {
              Type = "Params",
            },
      };
    }
  }

  public static Result ParamList(int left) {
    // ParamList -> param | paramList , param

    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    if (lexemes[left].type != TokenType.ClosePar) {
      Result paramRes = Param(left);
      if (paramRes.node != null) {
        Node _node = new Node() {
          Type = "ParamList",
        };
        _node.Childs.Add(paramRes.node);
        left = paramRes.rightIndex;
        if (lexemes[left].type == TokenType.Comma) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left].value,
          });
          Result paramListRes = ParamList(left + 1);
          if (paramListRes.node != null) {
            _node.Childs.Add(paramListRes.node);
            return new Result {
              rightIndex = paramListRes.rightIndex,
              node = _node,
            };
          } else {
            return new Result {
              error =
                  $"Expected paramList at line {lexemes[paramRes.rightIndex].line} at column: {lexemes[paramRes.rightIndex].column}\n",
              rightIndex = paramRes.rightIndex + 1,
              node = _node,
            };
          }
        } else if (lexemes[left].type == TokenType.ClosePar) {
          return new Result {
            rightIndex = left,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected ',' or ')' at line {lexemes[left].line} at column: {lexemes[left].column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          error =
              $"Expected param at line {lexemes[left].line} at column: {lexemes[left].column}\n",
          rightIndex = left + 1,
          node = null,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Param(int left) {
    // Param -> typeSpecifier ID | typeSpecifier ID [ ]
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Param",
    };
    if (TypeSpecifier(lexemes[left])) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.Ident) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        return new Result {
          rightIndex = left + 2,
          node = _node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result CompoundStmt(int left) {
    // CompoundStmt -> { localDeclarations statementList }
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "CompoundStmt",
    };

    Result noTermReturn = LocalDeclarations(left + 1);
    if (noTermReturn.node != null) {
      _node.Childs.Add(noTermReturn.node);
      if (noTermReturn.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = noTermReturn.rightIndex,
          node = _node,
        };
      }
      if (lexemes[noTermReturn.rightIndex].type == TokenType.CloseBrace) {
        _node.Childs.Add(new Node() {
          Type = lexemes[noTermReturn.rightIndex].value,
        });
        left = noTermReturn.rightIndex + 1;
      } else {
        return new Result {
          error =
              $"Expected paranthaese at line {lexemes[noTermReturn.rightIndex].line} at column: {lexemes[noTermReturn.rightIndex].column}\n",
          rightIndex = noTermReturn.rightIndex + 1,
          node = _node,
        };
      }
    }
    Result noTermReturn1 = StatementList(left + 1);
    if (noTermReturn1.node != null) {
      _node.Childs.Add(noTermReturn1.node);
      if (noTermReturn1.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = noTermReturn1.rightIndex,
          node = _node,
        };
      }
      if (lexemes[noTermReturn1.rightIndex].type == TokenType.CloseBrace) {
        _node.Childs.Add(new Node() {
          Type = lexemes[noTermReturn1.rightIndex].value,
        });

        left = noTermReturn1.rightIndex + 1;
      } else {
        return new Result {
          error =
              $"Expected paranthaese at line {lexemes[noTermReturn1.rightIndex].line} at column: {lexemes[noTermReturn1.rightIndex].column}\n",
          rightIndex = noTermReturn1.rightIndex + 1,
          node = _node,
        };
      }
    }

    return new Result() { rightIndex = left, node = _node };
  }

  public static Result LocalDeclarations(int left) {
    // LocalDeclarations -> LocalDeclarations varDeclaration | empty
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result varDeclarationRes = VarDeclaration(left);
    if (varDeclarationRes.node != null) {
      Node _node = new Node() {
        Type = "LocalDeclarations",
      };
      _node.Childs.Add(varDeclarationRes.node);
      Result localDeclarationsRes =
          LocalDeclarations(varDeclarationRes.rightIndex);
      if (localDeclarationsRes.node != null) {
        _node.Childs.Add(localDeclarationsRes.node);
        return new Result {
          rightIndex = localDeclarationsRes.rightIndex,
          node = _node,
        };
      } else {
        return new Result {
          rightIndex = varDeclarationRes.rightIndex,
          node = _node,
        };
      }
    }
    return new Result {
      rightIndex = left,
      node = null,
    };
  }

  public static Result StatementList(int left) {
    // StatementList -> StatementList statement | empty
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result statementRes = Statement(left);
    if (statementRes.node != null) {
      Node _node = new Node() {
        Type = "StatementList",
      };
      _node.Childs.Add(statementRes.node);
      Result statementListRes = StatementList(statementRes.rightIndex);
      if (statementListRes.node != null) {
        _node.Childs.Add(statementListRes.node);
        return new Result {
          rightIndex = statementListRes.rightIndex,
          node = _node,
        };
      } else {
        return new Result {
          rightIndex = statementRes.rightIndex,
          node = _node,
        };
      }
    }
    return new Result() { rightIndex = left, node = null };
  }

  public static Result Statement(int left) {
    // Statement -> ExpressionStmt | CompoundStmt | SelectionStmt |
    // IterationStmt | ReturnStmt

    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result ExpressionStmt(int left) {
    // ExpressionStmt -> expression ; | ;
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "ExpressionStmt",
    };
    Result expressionRes = Expression(left);
    if (expressionRes.node != null) {
      _node.Childs.Add(expressionRes.node);
      if (expressionRes.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = expressionRes.rightIndex,
          node = _node,
        };
      }
      if (lexemes[expressionRes.rightIndex].type == TokenType.Simecolon) {
        _node.Childs.Add(new Node() {
          Type = lexemes[expressionRes.rightIndex].value,
        });
        return new Result {
          rightIndex = expressionRes.rightIndex + 1,
          node = _node,
        };
      } else {
        return new Result {
          error =
              $"Expected ';' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
          rightIndex = expressionRes.rightIndex + 1,
          node = _node,
        };
      }
    } else {
      if (lexemes[left].type == TokenType.Simecolon) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left].value,
        });
        return new Result {
          rightIndex = left + 1,
          node = _node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result SelectionStmt(int left) {
    // SelectionStmt -> if ( expression ) statement | if ( expression )
    // statement else statement
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "SelectionStmt",
    };
    if (lexemes[left].type == TokenType.if_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        Result expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            Result statementRes = Statement(expressionRes.rightIndex + 1);
            if (statementRes.node != null) {
              _node.Childs.Add(statementRes.node);
              if (statementRes.rightIndex >= lexemes.Count()) {
                return new Result {
                  error = $"unexpected EOF\n",
                  rightIndex = statementRes.rightIndex,
                  node = _node,
                };
              }
              if (lexemes[statementRes.rightIndex].type == TokenType.else_) {
                _node.Childs.Add(new Node() {
                  Type = lexemes[statementRes.rightIndex].value,
                });
                Result statementRes2 =
                    Statement(statementRes.rightIndex + 1);
                if (statementRes2.node != null) {
                  _node.Childs.Add(statementRes2.node);
                  return new Result {
                    rightIndex = statementRes2.rightIndex,
                    node = _node,
                  };
                } else {
                  return new Result {
                    error =
                        $"Expected statement at line {lexemes[statementRes.rightIndex].line} at column: {lexemes[statementRes.rightIndex].column}\n"
                  };
                }
              }
            }
          }
        }
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result IterationStmt(int left) {
    // IterationStmt -> while ( expression ) statement
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "IterationStmt",
    };
    if (lexemes[left].type == TokenType.while_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        Result expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            Result statementRes = Statement(expressionRes.rightIndex + 1);
            if (statementRes.node != null) {
              _node.Childs.Add(statementRes.node);
              return new Result {
                rightIndex = statementRes.rightIndex,
                node = _node,
              };
            } else {
              return new Result {
                error =
                    $"Expected statement at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
                rightIndex = expressionRes.rightIndex + 1,
                node = _node,
              };
            }
          } else {
            return new Result {
              error =
                  $"Expected ')' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new Result {
            error =
                $"Expected expression at line {lexemes[left + 2].line} at column: {lexemes}"
          };
        }
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result ReturnStmt(int left) {
    // ReturnStmt -> return ; | return expression ;
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "ReturnStmt",
    };
    if (lexemes[left].type == TokenType.return_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.Simecolon) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        return new Result {
          rightIndex = left + 2,
          node = _node,
        };
      } else {
        Result expressionRes = Expression(left + 1);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.Simecolon) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            return new Result {
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new Result {
              error =
                  $"Expected ';' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new Result {
            error =
                $"Expected expression at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
            rightIndex = left + 2,
            node = _node,
          };
        }
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Expression(int left) {
    // Expression -> var = expression | simpleExpression
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result varRes = Var(left);
    if (varRes.node != null) {
      Node _node = new Node() {
        Type = "Expression",
      };
      _node.Childs.Add(varRes.node);
      if (varRes.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      }
      if (lexemes[varRes.rightIndex].type == TokenType.equal_) {
        _node.Childs.Add(new Node() {
          Type = lexemes[varRes.rightIndex].value,
        });
        Result expressionRes = Expression(varRes.rightIndex + 1);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          return new Result {
            rightIndex = expressionRes.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected expression at line {lexemes[varRes.rightIndex].line} at column: {lexemes[varRes.rightIndex].column}\n",
            rightIndex = varRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      }
    }
    Result simpleExpressionRes = SimpleExpression(left);
    if (simpleExpressionRes.node != null) {
      return simpleExpressionRes;
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Var(int left) {
    // Var -> ID | ID [ expression ]
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Var",
    };
    if (lexemes[left].type == TokenType.Ident) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenBracket) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        Result expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type ==
              TokenType.CloseBracket) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            return new Result {
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new Result {
              error =
                  $"Expected ']' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new Result {
            error =
                $"Expected expression at line {lexemes[left + 2].line} at column: {lexemes[left + 2].column}\n",
            rightIndex = left + 3,
            node = _node,
          };
        }
      }
      return new Result {
        rightIndex = left + 1,
        node = _node,
      };
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result SimpleExpression(int left) {
    // SimpleExpression -> AdditiveExpression relop AdditiveExpression |
    // AdditiveExpression
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result addRes = AdditiveExpression(left);
    if (addRes.node != null) {
      Node _node = new Node() {
        Type = "SimpleExpression",
      };
      _node.Childs.Add(addRes.node);
      if (addRes.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
      if (relOps(addRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[addRes.rightIndex].value,
        });
        Result addRes2 = AdditiveExpression(addRes.rightIndex + 1);
        if (addRes2.node != null) {
          _node.Childs.Add(addRes2.node);
          return new Result {
            rightIndex = addRes2.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected AdditiveExpression at line {lexemes[addRes.rightIndex].line} at column: {lexemes[addRes.rightIndex].column}\n",
            rightIndex = addRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result AdditiveExpression(int left) {
    // AdditiveExpression -> AdditiveExpression addOp Term | Term
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result addRes = AdditiveExpression(left);
    if (addRes.node != null) {
      Node _node = new Node() {
        Type = "AdditiveExpression",
      };
      _node.Childs.Add(addRes.node);
      if (addRes.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
      if (addOps(addRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[addRes.rightIndex].value,
        });
        Result termRes = Term(addRes.rightIndex + 1);
        if (termRes.node != null) {
          _node.Childs.Add(termRes.node);
          return new Result {
            rightIndex = termRes.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected term at line {lexemes[addRes.rightIndex].line} at column: {lexemes[addRes.rightIndex].column}\n",
            rightIndex = addRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
    } else {
      Result termRes = Term(left);
      if (termRes.node != null) {
        return new Result {
          rightIndex = termRes.rightIndex,
          node = termRes.node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Term(int left) {
    // Term -> Term mulOp Factor | Factor
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result termRes = Term(left);
    if (termRes.node != null) {
      Node _node = new Node() {
        Type = "Term",
      };
      _node.Childs.Add(termRes.node);
      if (termRes.rightIndex >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = termRes.rightIndex,
          node = _node,
        };
      }
      if (multOps(termRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[termRes.rightIndex].value,
        });
        Result factorRes = Factor(termRes.rightIndex + 1);
        if (factorRes.node != null) {
          _node.Childs.Add(factorRes.node);
          return new Result {
            rightIndex = factorRes.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected factor at line {lexemes[termRes.rightIndex].line} at column: {lexemes[termRes.rightIndex].column}\n",
            rightIndex = termRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          rightIndex = termRes.rightIndex,
          node = _node,
        };
      }
    } else {
      Result factorRes = Factor(left);
      if (factorRes.node != null) {
        return new Result {
          rightIndex = factorRes.rightIndex,
          node = factorRes.node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Factor(int left) {
    // Factor -> ( expression ) | var | call | num
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Factor",
    };
    if (lexemes[left].type == TokenType.OpenPar) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      Result expressionRes = Expression(left + 1);
      if (expressionRes.node != null) {
        _node.Childs.Add(expressionRes.node);
        if (expressionRes.rightIndex >= lexemes.Count()) {
          return new Result {
            error = $"unexpected EOF\n",
            rightIndex = expressionRes.rightIndex,
            node = _node,
          };
        }
        if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
          _node.Childs.Add(new Node() {
            Type = lexemes[expressionRes.rightIndex].value,
          });
          return new Result {
            rightIndex = expressionRes.rightIndex + 1,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected ')' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
            rightIndex = expressionRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          error =
              $"Expected expression at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
          rightIndex = left + 2,
          node = _node,
        };
      }
    } else if (lexemes[left].type == TokenType.Ident) {
      Result varRes = Var(left);
      if (varRes.node != null) {
        _node.Childs.Add(varRes.node);
        return new Result {
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      } else {
        Result callRes = Call(left);
        if (callRes.node != null) {
          _node.Childs.Add(callRes.node);
          return new Result {
            rightIndex = callRes.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error = $"Expected var or call at line {lexemes[left]}"
          };
        }
      }
    } else if (lexemes[left].type == TokenType.Number) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      return new Result {
        rightIndex = left + 1,
        node = _node,
      };
    }
    Result call_ = Call(left);
    if (call_.node != null) {
      _node.Childs.Add(call_.node);
      return new Result {
        rightIndex = call_.rightIndex,
        node = _node,
      };
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Call(int left) {
    // Call -> ID ( args )
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Call",
    };
    if (lexemes[left].type == TokenType.Ident) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new Result {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        Result argsRes = Args(left + 2);
        if (argsRes.node != null) {
          _node.Childs.Add(argsRes.node);
          if (argsRes.rightIndex >= lexemes.Count()) {
            return new Result {
              error = $"unexpected EOF\n",
              rightIndex = argsRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[argsRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[argsRes.rightIndex].value,
            });
            return new Result {
              rightIndex = argsRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new Result {
              error =
                  $"Expected ')' at line {lexemes[argsRes.rightIndex].line} at column: {lexemes[argsRes.rightIndex].column}\n",
              rightIndex = argsRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new Result {
            error =
                $"Expected args at line {lexemes[left + 2].line} at column: {lexemes[left + 2].column}\n",
            rightIndex = left + 3,
            node = _node,
          };
        }
      } else {
        return new Result {
          error =
              $"Expected '(' at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
          rightIndex = left + 2,
          node = _node,
        };
      }
    } else {
      return new Result {
        error =
            $"Expected Identifier at line {lexemes[left].line} at column: {lexemes[left].column}\n",
        rightIndex = left + 1,
        node = _node,
      };
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static Result Args(int left) {
    // Args -> argList | empty
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result noTermReturn = ArgList(left);
    if (noTermReturn.node != null) {
      return noTermReturn;
    }
    return new Result() { rightIndex = left, node = null };
  }

  public static Result ArgList(int left) {
    // ArgList -> expression | ArgList , expression
    if (left >= lexemes.Count()) {
      return new Result() { rightIndex = left, node = null };
    }
    Result expressionRes = Expression(left);
    if (expressionRes.node != null) {
      Node _node = new Node() {
        Type = "ArgList",
      };
      _node.Childs.Add(expressionRes.node);
      left = expressionRes.rightIndex;
      if (lexemes[left].type == TokenType.Comma) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left].value,
        });
        Result argListRes = ArgList(left + 1);
        if (argListRes.node != null) {
          _node.Childs.Add(argListRes.node);
          return new Result {
            rightIndex = argListRes.rightIndex,
            node = _node,
          };
        } else {
          return new Result {
            error =
                $"Expected ArgList at line {lexemes[left].line} at column: {lexemes[left].column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      } else {
        return new Result {
          rightIndex = left,
          node = _node,
        };
      }
    }
    return new Result() { rightIndex = -1, node = null };
  }

  public static bool addOps(int left) {
    if (left >= lexemes.Count()) {
      return false;
    }
    return lexemes[left].type == TokenType.AddOp;
  }

  public static bool multOps(int left) {
    if (left >= lexemes.Count()) {
      return false;
    }
    return lexemes[left].type == TokenType.MulOp;
  }

  public static bool relOps(int left) {
    // RelOp -> <= | < | > | >= | == | !=
    if (left >= lexemes.Count()) {
      return false;
    }
    return lexemes[left].type == TokenType.LogOp;
  }
}
}
