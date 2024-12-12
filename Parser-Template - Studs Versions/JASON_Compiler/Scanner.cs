using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

public enum Token_Class
{
    Integer ,Float ,String ,StringValue ,Read ,Write ,Repeat ,Until,
    If ,Else ,ElseIF ,Then ,Return ,EndLine ,Constant ,Identifier,
    PlusOperator ,MinusOperator ,MultiplyOperator ,DivisionOperator ,AssignOperator,
    EqualOpertaor ,GreaterThanOpertaor ,LessThanOpertaor ,Comma ,SimiColon,
    Lparan ,Rparan ,Lbrace ,Rbrace ,OrOperator ,AndOperator
}
namespace JASON_Compiler
{
    

    public class Token
    {
       public string lex;
       public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("elseif", Token_Class.ElseIF);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("int", Token_Class.Integer);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.EndLine);

            Operators.Add("+", Token_Class.PlusOperator);
            Operators.Add("-", Token_Class.MinusOperator);
            Operators.Add("*", Token_Class.MultiplyOperator);
            Operators.Add("/", Token_Class.DivisionOperator);
            Operators.Add(":=", Token_Class.AssignOperator);
            Operators.Add("=", Token_Class.EqualOpertaor);
            Operators.Add(">", Token_Class.GreaterThanOpertaor);
            Operators.Add("<", Token_Class.LessThanOpertaor);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add(";", Token_Class.SimiColon);
            Operators.Add("{", Token_Class.Lparan);
            Operators.Add("}", Token_Class.Rparan);
            Operators.Add("(", Token_Class.Lbrace);
            Operators.Add(")", Token_Class.Rbrace);
            Operators.Add("||", Token_Class.OrOperator);
            Operators.Add("&&", Token_Class.AndOperator);

        }

    public void StartScanning(string SourceCode)
        {
            for(int i=0; i<SourceCode.Length;i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;

                if (CurrentChar >= 'A' && CurrentChar <= 'z') //if you read a character
                {

                    while (CurrentChar >= 'A' && CurrentChar <= 'z' || CurrentChar >= '0' && CurrentChar <= '9' || CurrentChar == '_')
                    {
                        j++;
                        if (j >= SourceCode.Length)
                            break;
                        CurrentChar = SourceCode[j];
                        if (CurrentChar >= 'A' && CurrentChar <= 'z' || CurrentChar >= '0' && CurrentChar <= '9' || CurrentChar == '_')
                        {
                            CurrentLexeme += CurrentChar.ToString();
                        }
                    }

                    i = j - 1;
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar >= '0' && CurrentChar <= '9') //if you read a constant
                {
                    if (j < SourceCode.Length && SourceCode[j + 1] >= 'A' && SourceCode[j + 1] <= 'z' && SourceCode[j + 1] != '.')
                    {
                        j++;
                        while (true)
                        {
                            if(j >= SourceCode.Length || CurrentChar == '\n' || CurrentChar == ' ')
                            {
                                break;
                            }
                            CurrentChar = SourceCode[j];
                            CurrentLexeme += CurrentChar.ToString();
                            j++;
                        }
                    }

                    else
                    {
                        int dot = 0;

                        while (CurrentChar >= '0' && CurrentChar <= '9' || CurrentChar == '.' && dot < 1)
                        {
                            if (CurrentChar == '.')
                                dot++;
                            j++;

                            if (j >= SourceCode.Length) break;

                            CurrentChar = SourceCode[j];
                            if (CurrentChar >= '0' && CurrentChar <= '9' || CurrentChar == '.' && dot < 1)
                            {
                                CurrentLexeme += CurrentChar.ToString();
                            }
                        }
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == '/' && j + 1 < SourceCode.Length && SourceCode[j + 1] == '*') //Comment
                {
                    bool isComplete = false;
                    j++;
                    CurrentChar = SourceCode[j];

                    CurrentLexeme += CurrentChar.ToString();
                    j++;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];
                        CurrentLexeme += CurrentChar.ToString();
                        while (true)
                        {
                            if (j >= SourceCode.Length || j + 1 >= SourceCode.Length || CurrentChar == '\n')
                            {
                                break;
                            }
                            else if (CurrentChar == '*' && SourceCode[j + 1] == '/')
                            {
                                CurrentLexeme += CurrentChar.ToString() + SourceCode[j + 1];
                                isComplete = true;
                                break;
                            }
                            j++;
                            if (j < SourceCode.Length)
                            {
                                CurrentChar = SourceCode[j];
                                CurrentLexeme += CurrentChar.ToString();
                            }
                        }
                    }

                    if (isComplete)
                    {
                        i = j + 1;
                    }
                    else
                    {
                        i = j;
                        FindTokenClass(CurrentLexeme.ToLower());
                    }
                    
                }

                else if (CurrentChar == '+' || CurrentChar == '-' || CurrentChar == '*' || CurrentChar == '/') //Arthmatic operators
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == '=') //Equality
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == ':') //Assignment
                {
                    j++;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];
                    }

                    if (CurrentChar == '=')
                    {
                        CurrentLexeme += CurrentChar.ToString();
                        i = j;
                    }

                    else
                    {
                        i = j - 1;
                    }

                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == '<' || CurrentChar == '>') //Inequality
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == ',' || CurrentChar == ';') //commas
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == '{' || CurrentChar == '}' || CurrentChar == '(' || CurrentChar == ')') //Brackets
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                else if (CurrentChar == '|' && j + 1 < SourceCode.Length && SourceCode[j + 1] == '|') //OR Operator
                {
                    j++;
                    CurrentChar = SourceCode[j];
                    CurrentLexeme += CurrentChar.ToString();

                    i = j;
                    FindTokenClass(CurrentLexeme);
                }

                else if (CurrentChar == '&' && j + 1 < SourceCode.Length && SourceCode[j + 1] == '&') //AND Operator
                {
                    j++;
                    CurrentChar = SourceCode[j];
                    CurrentLexeme += CurrentChar.ToString();

                    i = j;
                    FindTokenClass(CurrentLexeme);
                }

                else if (CurrentChar == '"') //String value
                {
                    bool complete = true;
                    j++;
                    if (j < SourceCode.Length)
                    {
                        CurrentChar = SourceCode[j];

                        while (CurrentChar != '"')
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            j++;
                            if (j >= SourceCode.Length || CurrentChar == '\n')
                            {
                                complete = false;
                                break;
                            }
                            CurrentChar = SourceCode[j];
                        }

                        if (complete)
                        {
                            CurrentLexeme += CurrentChar.ToString();
                            i = j;
                        }
                        else
                        {
                            i = j - 1;
                        }

                    }
                    FindTokenClass(CurrentLexeme.ToLower());

                }

                else
                {
                    FindTokenClass(CurrentLexeme.ToLower());
                }

                if(j >= SourceCode.Length)
                {
                    break;
                }
            }
            
            JASON_Compiler.TokenStream = Tokens;
        }
        void FindTokenClass(string Lex)
        {
            bool unDifined = false;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            if(isReservedWord(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
            }

            //Is it an identifier?
            else if(isIdentifier(Lex))
            {
                Tok.token_type = Token_Class.Identifier;
            }

            //Is it a Constant?
            else if(isConstant(Lex))
            {
                Tok.token_type = Token_Class.Constant;
            }

            //Is it an operator?
            else if(isOperator(Lex))
            {
                Tok.token_type = Operators[Lex];
            }

            //Is it a string?
            else if(isStringValue(Lex))
            {
                Tok.token_type= Token_Class.StringValue;
            }

            //Is it an undefined?
            else
            {
                Errors.Error_List.Add(Lex);
                unDifined = true;
            }

            if(!unDifined)
            {
                Tokens.Add(Tok);
            }

        }

    

        bool isIdentifier(string lex)
        {
            bool isValid=false;
            // Check if the lex is an identifier or not.
            var rx = new Regex(@"^[A-Za-z][A-Za-z0-9_]*$",RegexOptions.Compiled);
            if (rx.IsMatch(lex))
            {
                isValid = true;
            }

            return isValid;
        }
        bool isConstant(string lex)
        {
            bool isValid = false;
            // Check if the lex is a constant (Number) or not.
            var rx = new Regex(@"^[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled);
            if (rx.IsMatch(lex))
            {
                isValid = true;
            }

            return isValid;
        }

        bool isReservedWord(string lex)
        {
            bool isValid = false;
            //Check if the lex is a reserved word like (if,else,..) or not
            for (int i = 0; i < ReservedWords.Count; i++)
            {
                if(ReservedWords.ContainsKey(lex))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        bool isOperator(string lex)
        {
            bool isValid = false;
            //Check if the lex is an operator like (+,-,*,..) or not
            for(int i = 0; i < Operators.Count; i++)
            {
                if(Operators.ContainsKey(lex))
                {
                    isValid = true;
                }
            }

            return isValid;
        }

        bool isStringValue(string lex)
        {
            bool isValid = false;
            //Check if the lex is a string or not
            if (lex[0] == '"' && lex[lex.Length - 1] == '"' && lex.Length > 1)
            {
                isValid = true;
            }

            return isValid;
        }

    }
}
