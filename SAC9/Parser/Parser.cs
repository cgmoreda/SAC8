using System.Data.Common;
using System.Xml.Linq;
using SAC9.Lexer;

namespace SAC9.Parser {
public static class Parser {
  public static List<Lexeme> lexemes { get; set; } = new List<Lexeme>();

  // Program
  static public NoTermReturn Parse() { return DeclarationList(0); }

  public static NoTermReturn DeclarationList(int leftIndex) {
    if (leftIndex >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = leftIndex, node = null };
    }
    Node root = new Node() {};
    NoTermReturn ntrm = Declaration(leftIndex);
    Node? path1 = ntrm.node;

    if (ntrm.rightIndex != -1) {
      if (path1 != null)
        root.Childs.Add(path1);
      NoTermReturn dls = DeclarationList(ntrm.rightIndex);
      if (dls.node != null) {
        root.Childs.Add(dls.node);
        return new NoTermReturn() { rightIndex = dls.rightIndex, node = root,
                                    error = dls.error };
      } else {
        return ntrm;
      }
    }

    return new NoTermReturn() { rightIndex = -1, node = null,
                                error =
                                    $"parsing error starting at {leftIndex}" };
  }

  public static NoTermReturn Declaration(int left) {
    // Declaration -> VarDeclaration | FunDeclaration
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn varRes = VarDeclaration(left);
    if (varRes != null) {
      NoTermReturn ret = new NoTermReturn();
      ret.node.Childs.Add(varRes.node);
      ret.node.Type = "Declaration";
      ret.rightIndex = varRes.rightIndex;
      return ret;
    }
    NoTermReturn funRes = FunDeclaration(left);
    if (funRes != null) {
      NoTermReturn ret = new NoTermReturn();
      ret.node.Childs.Add(ret.node);
      ret.rightIndex = funRes.rightIndex;
      return ret;
    }

    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn VarDeclaration(int left) {
    // varDeclaration -> typeSpecifier ID ; | typeSpecifier ID [ NUM ] ;
    if (left >= lexemes.Count() || left < 0) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "VarDeclaration",
    };
    if (TypeSpecifier(lexemes[left])) {
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
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
          return new NoTermReturn {
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
          return new NoTermReturn {
            rightIndex = left + 3,
            node = _node,
          };
        } else if (lexemes[2].type == TokenType.OpenBracket) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left + 2].value,
          });
          if (left + 5 >= lexemes.Count()) {
            return new NoTermReturn {
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
                return new NoTermReturn {
                  rightIndex = left + 6,
                  node = _node,
                };
              } else {
                return new NoTermReturn {
                  error =
                      $"Expected ';' at line {lexemes[left + 5].line} at column: {lexemes[left + 5].column}\n",
                  rightIndex = left + 6,
                  node = _node,
                };
              }
            } else {
              return new NoTermReturn {
                error =
                    $"Expected ']' at line {lexemes[left + 4].line} at column: {lexemes[left + 4].column}\n",
                rightIndex = left + 5,
                
                node = _node,
              };
            }
          } else {
            return new NoTermReturn {
              error =
                  $"Expected Number at line {lexemes[left + 3].line} at column: {lexemes[left + 3].column}\n",
              rightIndex = left + 4,
              node = _node,
            };
          }
        } else if (lexemes[2].type == TokenType.OpenPar) {
          return new NoTermReturn {
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
            return new NoTermReturn {
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
          return new NoTermReturn {
            error =
                $"Expected type-specifier at line {line} at column: {column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      }
    }

    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static bool TypeSpecifier(Lexeme identif) {
    return identif.type == TokenType.Void_ || identif.type == TokenType.real_ ||
           identif.type == TokenType.num_;
  }

  public static NoTermReturn FunDeclaration(int left) {
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
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
        return new NoTermReturn {
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
          return new NoTermReturn {
            error = $"unexpected EOF\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
        if (lexemes[left + 2].type == TokenType.OpenPar) {
          _node.Childs.Add(new Node() {
            Type = lexemes[left + 2].value,
          });
          NoTermReturn paramsRes = Params(left + 3);
          if (paramsRes.node != null) {
            _node.Childs.Add(paramsRes.node);
            if (paramsRes.rightIndex >= lexemes.Count()) {
              return new NoTermReturn {
                error = $"unexpected EOF\n",
                rightIndex = paramsRes.rightIndex,
                node = _node,
              };
            }
            if (lexemes[paramsRes.rightIndex].type == TokenType.ClosePar) {
              _node.Childs.Add(new Node() {
                Type = lexemes[paramsRes.rightIndex].value,
              });
              NoTermReturn compoundStmtRes =
                  CompoundStmt(paramsRes.rightIndex + 1);
              if (compoundStmtRes.node != null) {
                _node.Childs.Add(compoundStmtRes.node);
                return new NoTermReturn {
                  rightIndex = compoundStmtRes.rightIndex,
                  node = _node,
                };
              } else {
                return new NoTermReturn {
                  error =
                      $"Expected compound statement at line {lexemes[paramsRes.rightIndex].line} at column: {lexemes[paramsRes.rightIndex].column}\n",
                  rightIndex = paramsRes.rightIndex + 1,
                  node = _node,
                };
              }
            } else {
              return new NoTermReturn {
                error =
                    $"Expected close params at line {lexemes[paramsRes.rightIndex].line} at column: {lexemes[paramsRes.rightIndex].column}\n",
                rightIndex = paramsRes.rightIndex + 1,
                node = _node,
              };
            }
          } else {
            return new NoTermReturn {
              error =
                  $"Expected params at line {lexemes[left + 3].line} at column: {lexemes[left + 3].column}\n",
              rightIndex = left + 4,
              node = _node,
            };
          }
        }
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Params(int left) {
    // Params -> paramList | void
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn paramListRes = ParamList(left);
    if (paramListRes.node != null) {
      return paramListRes;
    } else {
      return new NoTermReturn {
        rightIndex = left + 1,
        node =
            new Node() {
              Type = "Params",
            },
      };
    }
  }

  public static NoTermReturn ParamList(int left) {
    // ParamList -> param | paramList , param

    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    if (lexemes[left].type != TokenType.ClosePar) {
      NoTermReturn paramRes = Param(left);
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
          NoTermReturn paramListRes = ParamList(left + 1);
          if (paramListRes.node != null) {
            _node.Childs.Add(paramListRes.node);
            return new NoTermReturn {
              rightIndex = paramListRes.rightIndex,
              node = _node,
            };
          } else {
            return new NoTermReturn {
              error =
                  $"Expected paramList at line {lexemes[paramRes.rightIndex].line} at column: {lexemes[paramRes.rightIndex].column}\n",
              rightIndex = paramRes.rightIndex + 1,
              node = _node,
            };
          }
        } else if (lexemes[left].type == TokenType.ClosePar) {
          return new NoTermReturn {
            rightIndex = left,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected ',' or ')' at line {lexemes[left].line} at column: {lexemes[left].column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          error =
              $"Expected param at line {lexemes[left].line} at column: {lexemes[left].column}\n",
          rightIndex = left + 1,
          node = null,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Param(int left) {
    // Param -> typeSpecifier ID | typeSpecifier ID [ ]
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Param",
    };
    if (TypeSpecifier(lexemes[left])) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.Ident) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        return new NoTermReturn {
          rightIndex = left + 2,
          node = _node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn CompoundStmt(int left) {
    // CompoundStmt -> { localDeclarations statementList }
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "CompoundStmt",
    };

    NoTermReturn noTermReturn = LocalDeclarations(left + 1);
    if (noTermReturn.node != null) {
      _node.Childs.Add(noTermReturn.node);
      if (noTermReturn.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
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
        return new NoTermReturn {
          error =
              $"Expected paranthaese at line {lexemes[noTermReturn.rightIndex].line} at column: {lexemes[noTermReturn.rightIndex].column}\n",
          rightIndex = noTermReturn.rightIndex + 1,
          node = _node,
        };
      }
    }
    NoTermReturn noTermReturn1 = StatementList(left + 1);
    if (noTermReturn1.node != null) {
      _node.Childs.Add(noTermReturn1.node);
      if (noTermReturn1.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
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
        return new NoTermReturn {
          error =
              $"Expected paranthaese at line {lexemes[noTermReturn1.rightIndex].line} at column: {lexemes[noTermReturn1.rightIndex].column}\n",
          rightIndex = noTermReturn1.rightIndex + 1,
          node = _node,
        };
      }
    }

    return new NoTermReturn() { rightIndex = left, node = _node };
  }

  public static NoTermReturn LocalDeclarations(int left) {
    // LocalDeclarations -> LocalDeclarations varDeclaration | empty
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn varDeclarationRes = VarDeclaration(left);
    if (varDeclarationRes.node != null) {
      Node _node = new Node() {
        Type = "LocalDeclarations",
      };
      _node.Childs.Add(varDeclarationRes.node);
      NoTermReturn localDeclarationsRes =
          LocalDeclarations(varDeclarationRes.rightIndex);
      if (localDeclarationsRes.node != null) {
        _node.Childs.Add(localDeclarationsRes.node);
        return new NoTermReturn {
          rightIndex = localDeclarationsRes.rightIndex,
          node = _node,
        };
      } else {
        return new NoTermReturn {
          rightIndex = varDeclarationRes.rightIndex,
          node = _node,
        };
      }
    }
    return new NoTermReturn {
      rightIndex = left,
      node = null,
    };
  }

  public static NoTermReturn StatementList(int left) {
    // StatementList -> StatementList statement | empty
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn statementRes = Statement(left);
    if (statementRes.node != null) {
      Node _node = new Node() {
        Type = "StatementList",
      };
      _node.Childs.Add(statementRes.node);
      NoTermReturn statementListRes = StatementList(statementRes.rightIndex);
      if (statementListRes.node != null) {
        _node.Childs.Add(statementListRes.node);
        return new NoTermReturn {
          rightIndex = statementListRes.rightIndex,
          node = _node,
        };
      } else {
        return new NoTermReturn {
          rightIndex = statementRes.rightIndex,
          node = _node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = left, node = null };
  }

  public static NoTermReturn Statement(int left) {
    // Statement -> ExpressionStmt | CompoundStmt | SelectionStmt |
    // IterationStmt | ReturnStmt

    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn ExpressionStmt(int left) {
    // ExpressionStmt -> expression ; | ;
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "ExpressionStmt",
    };
    NoTermReturn expressionRes = Expression(left);
    if (expressionRes.node != null) {
      _node.Childs.Add(expressionRes.node);
      if (expressionRes.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = expressionRes.rightIndex,
          node = _node,
        };
      }
      if (lexemes[expressionRes.rightIndex].type == TokenType.Simecolon) {
        _node.Childs.Add(new Node() {
          Type = lexemes[expressionRes.rightIndex].value,
        });
        return new NoTermReturn {
          rightIndex = expressionRes.rightIndex + 1,
          node = _node,
        };
      } else {
        return new NoTermReturn {
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
        return new NoTermReturn {
          rightIndex = left + 1,
          node = _node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn SelectionStmt(int left) {
    // SelectionStmt -> if ( expression ) statement | if ( expression )
    // statement else statement
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "SelectionStmt",
    };
    if (lexemes[left].type == TokenType.if_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        NoTermReturn expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new NoTermReturn {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            NoTermReturn statementRes = Statement(expressionRes.rightIndex + 1);
            if (statementRes.node != null) {
              _node.Childs.Add(statementRes.node);
              if (statementRes.rightIndex >= lexemes.Count()) {
                return new NoTermReturn {
                  error = $"unexpected EOF\n",
                  rightIndex = statementRes.rightIndex,
                  node = _node,
                };
              }
              if (lexemes[statementRes.rightIndex].type == TokenType.else_) {
                _node.Childs.Add(new Node() {
                  Type = lexemes[statementRes.rightIndex].value,
                });
                NoTermReturn statementRes2 =
                    Statement(statementRes.rightIndex + 1);
                if (statementRes2.node != null) {
                  _node.Childs.Add(statementRes2.node);
                  return new NoTermReturn {
                    rightIndex = statementRes2.rightIndex,
                    node = _node,
                  };
                } else {
                  return new NoTermReturn {
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
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn IterationStmt(int left) {
    // IterationStmt -> while ( expression ) statement
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "IterationStmt",
    };
    if (lexemes[left].type == TokenType.while_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        NoTermReturn expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new NoTermReturn {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            NoTermReturn statementRes = Statement(expressionRes.rightIndex + 1);
            if (statementRes.node != null) {
              _node.Childs.Add(statementRes.node);
              return new NoTermReturn {
                rightIndex = statementRes.rightIndex,
                node = _node,
              };
            } else {
              return new NoTermReturn {
                error =
                    $"Expected statement at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
                rightIndex = expressionRes.rightIndex + 1,
                node = _node,
              };
            }
          } else {
            return new NoTermReturn {
              error =
                  $"Expected ')' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new NoTermReturn {
            error =
                $"Expected expression at line {lexemes[left + 2].line} at column: {lexemes}"
          };
        }
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn ReturnStmt(int left) {
    // ReturnStmt -> return ; | return expression ;
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "ReturnStmt",
    };
    if (lexemes[left].type == TokenType.return_) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.Simecolon) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        return new NoTermReturn {
          rightIndex = left + 2,
          node = _node,
        };
      } else {
        NoTermReturn expressionRes = Expression(left + 1);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new NoTermReturn {
              error = $"unexpected EOF\n",
              rightIndex = expressionRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[expressionRes.rightIndex].type == TokenType.Simecolon) {
            _node.Childs.Add(new Node() {
              Type = lexemes[expressionRes.rightIndex].value,
            });
            return new NoTermReturn {
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new NoTermReturn {
              error =
                  $"Expected ';' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new NoTermReturn {
            error =
                $"Expected expression at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
            rightIndex = left + 2,
            node = _node,
          };
        }
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Expression(int left) {
    // Expression -> var = expression | simpleExpression
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn varRes = Var(left);
    if (varRes.node != null) {
      Node _node = new Node() {
        Type = "Expression",
      };
      _node.Childs.Add(varRes.node);
      if (varRes.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      }
      if (lexemes[varRes.rightIndex].type == TokenType.equal_) {
        _node.Childs.Add(new Node() {
          Type = lexemes[varRes.rightIndex].value,
        });
        NoTermReturn expressionRes = Expression(varRes.rightIndex + 1);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          return new NoTermReturn {
            rightIndex = expressionRes.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected expression at line {lexemes[varRes.rightIndex].line} at column: {lexemes[varRes.rightIndex].column}\n",
            rightIndex = varRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      }
    }
    NoTermReturn simpleExpressionRes = SimpleExpression(left);
    if (simpleExpressionRes.node != null) {
      return simpleExpressionRes;
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Var(int left) {
    // Var -> ID | ID [ expression ]
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Var",
    };
    if (lexemes[left].type == TokenType.Ident) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenBracket) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        NoTermReturn expressionRes = Expression(left + 2);
        if (expressionRes.node != null) {
          _node.Childs.Add(expressionRes.node);
          if (expressionRes.rightIndex >= lexemes.Count()) {
            return new NoTermReturn {
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
            return new NoTermReturn {
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new NoTermReturn {
              error =
                  $"Expected ']' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
              rightIndex = expressionRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new NoTermReturn {
            error =
                $"Expected expression at line {lexemes[left + 2].line} at column: {lexemes[left + 2].column}\n",
            rightIndex = left + 3,
            node = _node,
          };
        }
      }
      return new NoTermReturn {
        rightIndex = left + 1,
        node = _node,
      };
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn SimpleExpression(int left) {
    // SimpleExpression -> AdditiveExpression relop AdditiveExpression |
    // AdditiveExpression
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn addRes = AdditiveExpression(left);
    if (addRes.node != null) {
      Node _node = new Node() {
        Type = "SimpleExpression",
      };
      _node.Childs.Add(addRes.node);
      if (addRes.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
      if (relOps(addRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[addRes.rightIndex].value,
        });
        NoTermReturn addRes2 = AdditiveExpression(addRes.rightIndex + 1);
        if (addRes2.node != null) {
          _node.Childs.Add(addRes2.node);
          return new NoTermReturn {
            rightIndex = addRes2.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected AdditiveExpression at line {lexemes[addRes.rightIndex].line} at column: {lexemes[addRes.rightIndex].column}\n",
            rightIndex = addRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn AdditiveExpression(int left) {
    // AdditiveExpression -> AdditiveExpression addOp Term | Term
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn addRes = AdditiveExpression(left);
    if (addRes.node != null) {
      Node _node = new Node() {
        Type = "AdditiveExpression",
      };
      _node.Childs.Add(addRes.node);
      if (addRes.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
      if (addOps(addRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[addRes.rightIndex].value,
        });
        NoTermReturn termRes = Term(addRes.rightIndex + 1);
        if (termRes.node != null) {
          _node.Childs.Add(termRes.node);
          return new NoTermReturn {
            rightIndex = termRes.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected term at line {lexemes[addRes.rightIndex].line} at column: {lexemes[addRes.rightIndex].column}\n",
            rightIndex = addRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          rightIndex = addRes.rightIndex,
          node = _node,
        };
      }
    } else {
      NoTermReturn termRes = Term(left);
      if (termRes.node != null) {
        return new NoTermReturn {
          rightIndex = termRes.rightIndex,
          node = termRes.node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Term(int left) {
    // Term -> Term mulOp Factor | Factor
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn termRes = Term(left);
    if (termRes.node != null) {
      Node _node = new Node() {
        Type = "Term",
      };
      _node.Childs.Add(termRes.node);
      if (termRes.rightIndex >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = termRes.rightIndex,
          node = _node,
        };
      }
      if (multOps(termRes.rightIndex)) {
        _node.Childs.Add(new Node() {
          Type = lexemes[termRes.rightIndex].value,
        });
        NoTermReturn factorRes = Factor(termRes.rightIndex + 1);
        if (factorRes.node != null) {
          _node.Childs.Add(factorRes.node);
          return new NoTermReturn {
            rightIndex = factorRes.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected factor at line {lexemes[termRes.rightIndex].line} at column: {lexemes[termRes.rightIndex].column}\n",
            rightIndex = termRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          rightIndex = termRes.rightIndex,
          node = _node,
        };
      }
    } else {
      NoTermReturn factorRes = Factor(left);
      if (factorRes.node != null) {
        return new NoTermReturn {
          rightIndex = factorRes.rightIndex,
          node = factorRes.node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Factor(int left) {
    // Factor -> ( expression ) | var | call | num
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Factor",
    };
    if (lexemes[left].type == TokenType.OpenPar) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      NoTermReturn expressionRes = Expression(left + 1);
      if (expressionRes.node != null) {
        _node.Childs.Add(expressionRes.node);
        if (expressionRes.rightIndex >= lexemes.Count()) {
          return new NoTermReturn {
            error = $"unexpected EOF\n",
            rightIndex = expressionRes.rightIndex,
            node = _node,
          };
        }
        if (lexemes[expressionRes.rightIndex].type == TokenType.ClosePar) {
          _node.Childs.Add(new Node() {
            Type = lexemes[expressionRes.rightIndex].value,
          });
          return new NoTermReturn {
            rightIndex = expressionRes.rightIndex + 1,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected ')' at line {lexemes[expressionRes.rightIndex].line} at column: {lexemes[expressionRes.rightIndex].column}\n",
            rightIndex = expressionRes.rightIndex + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          error =
              $"Expected expression at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
          rightIndex = left + 2,
          node = _node,
        };
      }
    } else if (lexemes[left].type == TokenType.Ident) {
      NoTermReturn varRes = Var(left);
      if (varRes.node != null) {
        _node.Childs.Add(varRes.node);
        return new NoTermReturn {
          rightIndex = varRes.rightIndex,
          node = _node,
        };
      } else {
        NoTermReturn callRes = Call(left);
        if (callRes.node != null) {
          _node.Childs.Add(callRes.node);
          return new NoTermReturn {
            rightIndex = callRes.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error = $"Expected var or call at line {lexemes[left]}"
          };
        }
      }
    } else if (lexemes[left].type == TokenType.Number) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      return new NoTermReturn {
        rightIndex = left + 1,
        node = _node,
      };
    }
    NoTermReturn call_ = Call(left);
    if (call_.node != null) {
      _node.Childs.Add(call_.node);
      return new NoTermReturn {
        rightIndex = call_.rightIndex,
        node = _node,
      };
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Call(int left) {
    // Call -> ID ( args )
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    Node _node = new Node() {
      Type = "Call",
    };
    if (lexemes[left].type == TokenType.Ident) {
      _node.Childs.Add(new Node() {
        Type = lexemes[left].value,
      });
      if (left + 1 >= lexemes.Count()) {
        return new NoTermReturn {
          error = $"unexpected EOF\n",
          rightIndex = left + 1,
          node = _node,
        };
      }
      if (lexemes[left + 1].type == TokenType.OpenPar) {
        _node.Childs.Add(new Node() {
          Type = lexemes[left + 1].value,
        });
        NoTermReturn argsRes = Args(left + 2);
        if (argsRes.node != null) {
          _node.Childs.Add(argsRes.node);
          if (argsRes.rightIndex >= lexemes.Count()) {
            return new NoTermReturn {
              error = $"unexpected EOF\n",
              rightIndex = argsRes.rightIndex,
              node = _node,
            };
          }
          if (lexemes[argsRes.rightIndex].type == TokenType.ClosePar) {
            _node.Childs.Add(new Node() {
              Type = lexemes[argsRes.rightIndex].value,
            });
            return new NoTermReturn {
              rightIndex = argsRes.rightIndex + 1,
              node = _node,
            };
          } else {
            return new NoTermReturn {
              error =
                  $"Expected ')' at line {lexemes[argsRes.rightIndex].line} at column: {lexemes[argsRes.rightIndex].column}\n",
              rightIndex = argsRes.rightIndex + 1,
              node = _node,
            };
          }
        } else {
          return new NoTermReturn {
            error =
                $"Expected args at line {lexemes[left + 2].line} at column: {lexemes[left + 2].column}\n",
            rightIndex = left + 3,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          error =
              $"Expected '(' at line {lexemes[left + 1].line} at column: {lexemes[left + 1].column}\n",
          rightIndex = left + 2,
          node = _node,
        };
      }
    } else {
      return new NoTermReturn {
        error =
            $"Expected Identifier at line {lexemes[left].line} at column: {lexemes[left].column}\n",
        rightIndex = left + 1,
        node = _node,
      };
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
  }

  public static NoTermReturn Args(int left) {
    // Args -> argList | empty
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn noTermReturn = ArgList(left);
    if (noTermReturn.node != null) {
      return noTermReturn;
    }
    return new NoTermReturn() { rightIndex = left, node = null };
  }

  public static NoTermReturn ArgList(int left) {
    // ArgList -> expression | ArgList , expression
    if (left >= lexemes.Count()) {
      return new NoTermReturn() { rightIndex = left, node = null };
    }
    NoTermReturn expressionRes = Expression(left);
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
        NoTermReturn argListRes = ArgList(left + 1);
        if (argListRes.node != null) {
          _node.Childs.Add(argListRes.node);
          return new NoTermReturn {
            rightIndex = argListRes.rightIndex,
            node = _node,
          };
        } else {
          return new NoTermReturn {
            error =
                $"Expected ArgList at line {lexemes[left].line} at column: {lexemes[left].column}\n",
            rightIndex = left + 1,
            node = _node,
          };
        }
      } else {
        return new NoTermReturn {
          rightIndex = left,
          node = _node,
        };
      }
    }
    return new NoTermReturn() { rightIndex = -1, node = null };
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
