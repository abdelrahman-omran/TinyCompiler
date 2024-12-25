using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.AxHost;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        
        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        int InputPointer = 0;
        int TokenStreamCount;
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            this.TokenStreamCount = TokenStream.Count;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }
        bool FoundToken()
        {

            if (TokenStream[InputPointer].token_type == Token_Class.Integer ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String)
            {

                return true;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                TokenStream[InputPointer+1].token_type == Token_Class.AssignOperator)
            {
                return true;
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                return true;
            }
            // if the curr token is read
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                return true;
            }
            // if the curr token is if
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.If)
            {
                return true;
            }
 
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                return true;
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Return)
            {
                return true;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.DivisionOperator && TokenStream[InputPointer + 1].token_type == Token_Class.MultiplyOperator)
            {
                return true;
            }
            //else if(TokenStream)
            else
            {
                //Errors.Error_List.Add("Parsing Error: Expected Statement\r\n");
                return false;
            }
        }

        Node Program()
        {
            Node program = new Node("Program");
            program.Children.Add(Functions());
            if(InputPointer < TokenStreamCount)
                program.Children.Add(MainFunction());
            else
                Errors.Error_List.Add("Parsing Error: Missing Main function\r\n");

                //program.Children.Add(match(Token_Class.Dot));
                MessageBox.Show("Success");
            return program;
        }

        Node Functions()
        {
            Node functions = new Node("Functions");
            while (InputPointer < TokenStreamCount && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
                functions.Children.Add(Function());
            return functions;
        }
        Node Function()
        {
            Node function = new Node("Function");
            function.Children.Add(FunctionDecl());
            function.Children.Add(FunctionBody());
            return function;
        }
        Node MainFunction()
        {
            Node mainfunc = new Node("Main");
            Node dataty = Datatype();
            if (dataty != null)
                mainfunc.Children.Add(dataty);
            else
                Errors.Error_List.Add("Parsing Error: Missing Datatype in Main function\r\n");

            if(InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Main)
            mainfunc.Children.Add(match(Token_Class.Main));
            else
                Errors.Error_List.Add("Parsing Error: Missing 'main' in Main function\r\n");

            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Lbrace)
                mainfunc.Children.Add(match(Token_Class.Lbrace));
            else
                Errors.Error_List.Add("Parsing Error: Missing 'Lbrace' in Main function\r\n");

            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Rbrace)
                mainfunc.Children.Add(match(Token_Class.Rbrace));
            else
                Errors.Error_List.Add("Parsing Error: Missing 'Rbrace' in Main function\r\n");

            int CurrentInputPointer = InputPointer;
            Node funbody = FunctionBody();
            if (funbody != null)
                mainfunc.Children.Add(funbody);
            else
            {
                InputPointer = CurrentInputPointer;
                Errors.Error_List.Add("Parsing Error: Missing Function Body in Main function\r\n");
            }
            return mainfunc;
        }

        Node FunctionDecl()
        {
            Node function_dec = new Node("Function_dec");
            Node dataty = Datatype();
            if (dataty != null)
                function_dec.Children.Add(dataty);
            else
                Errors.Error_List.Add("Parsing Error: Missing Datatype in function declaration\r\n");

            int CurrentInputPointer = InputPointer;
            Node funName = Function_name();
            if(funName != null)
            function_dec.Children.Add(funName);
            else
            {
                InputPointer = CurrentInputPointer;
                Errors.Error_List.Add("Parsing Error: Missing function name in function Decalaration\r\n");

            }

            if (TokenStream[InputPointer].token_type != Token_Class.Lbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Lbrace in function declaration\r\n");
            }
            else
                function_dec.Children.Add(match(Token_Class.Lbrace));

            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Rbrace)
                function_dec.Children.Add(match(Token_Class.Rbrace));
            else
            {
                CurrentInputPointer = InputPointer;

                Node param = Parameters();
                if(param != null)
                function_dec.Children.Add(param);
                else
                {
                    InputPointer = CurrentInputPointer;
                    Errors.Error_List.Add("Parsing Error: Missing parameters function declaration\r\n");

                }
                if (TokenStream[InputPointer].token_type != Token_Class.Rbrace)
                {
                    Errors.Error_List.Add("Parsing Error: Expected Rbrace in function declaration \r\n");
                }
                else
                {
                    function_dec.Children.Add(match(Token_Class.Rbrace));
                }

            }
            return function_dec;
        }
        Node Function_name()
        {
            Node function_name = new Node("Function_name");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in function Name \r\n");
            }
            else
                function_name.Children.Add(match(Token_Class.Identifier));
            return function_name;
        }

        Node Parameters()
        {
            Node parameters = new Node("Parameters");
            int CurrentInputPointer = InputPointer;
            Node param = Parameter();
            if (param != null)
                parameters.Children.Add(param);
            else
            {
                InputPointer = CurrentInputPointer;
                Errors.Error_List.Add("Parsing Error: Expected a Parameter \r\n");
            }
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters.Children.Add(match(Token_Class.Comma));

                param = Parameter();
                if (param != null)
                    parameters.Children.Add(param);
                else
                {
                    InputPointer = CurrentInputPointer;
                    Errors.Error_List.Add("Parsing Error: Expected a Parameter \r\n");
                }
            }

            return parameters;
        }
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            Node dataty = Datatype();
            if (dataty != null)
                parameter.Children.Add(dataty);
            else
                Errors.Error_List.Add("Parsing Error: Missing Datatype before parameter\r\n");

            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in Parameter \r\n");
            }
            else
                parameter.Children.Add(match(Token_Class.Identifier));
            return parameter;
        }
        Node FunctionBody()
        {
            Node funcbody = new Node("Body");
            if (TokenStream[InputPointer].token_type != Token_Class.Lparan)
            {
                Errors.Error_List.Add("Parsing Error: Expected Left Paranthese in function body\r\n");
            }
            else
                funcbody.Children.Add(match(Token_Class.Lparan));

            funcbody.Children.Add(Statements());

            if (TokenStream[InputPointer].token_type != Token_Class.Rparan)
            {
                Errors.Error_List.Add("Parsing Error: Expected Right Paranthese in function body\r\n");
            }
            else
            {
                funcbody.Children.Add(match(Token_Class.Rparan));
            }
            return funcbody;
        }

        Node Statements()
        {
            Node statements = new Node("Statements");
            Node state = Statement();
            while (state != null)
            {
                statements.Children.Add(state);
                state = Statement();
            }
            return statements;
        }
        Node Statement()
        {
            Node statement = new Node("Statement");
            if (InputPointer + 1 < TokenStreamCount && TokenStream[InputPointer + 1].token_type == Token_Class.Main)
            {
                return null;
            }
            else if (InputPointer < TokenStreamCount && (TokenStream[InputPointer].token_type == Token_Class.Integer ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String))
            {

                statement.Children.Add(Declaration_Statement());
                return statement;
            }
            else if (InputPointer < TokenStreamCount && InputPointer+1 < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {
                statement.Children.Add(Assignment_Statement());
                return statement;
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                statement.Children.Add(Write_Statement());
                return statement;
            }
            // if the curr token is read
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                statement.Children.Add(Read_Statement());
                return statement;
            }
            // if the curr token is if
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.If)
            {
                statement.Children.Add(FullIfStatement());
                return statement;
            }
            /*            else if (TokenStream[InputPointer].token_type == Token_Class.ElseIF)
                        {
                            statement.Children.Add(ElseifStatment());
                            return statement;
                        }
                        else if (TokenStream[InputPointer].token_type == Token_Class.Else)
                        {
                            statement.Children.Add(ElseStatment());
                            return statement;
                        }*/
            // if the curr token is repeat
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                statement.Children.Add(RepeatStatement());
                return statement;
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Return)
            {
                statement.Children.Add(Return_Statement());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.DivisionOperator && TokenStream[InputPointer + 1].token_type == Token_Class.MultiplyOperator)
            {
                statement.Children.Add(Comment_Statement());
                return statement;
            }

            else
            {
                //Errors.Error_List.Add("Parsing Error: Expected Statement\r\n");


                //bool nextStatement = FoundToken();
/*                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }*/

                return null;

            
            }

        }
        Node Comment_Statement()
        {
            Node comment = new Node("Comment_Statement");
                InputPointer += 2;
                while (!(TokenStream[InputPointer].token_type == Token_Class.MultiplyOperator && TokenStream[InputPointer + 1].token_type == Token_Class.DivisionOperator))
                {

                    InputPointer++;
                    if (InputPointer == TokenStream.Count)
                    {
                        InputPointer -= 2;
                        break;
                    }
                }
            MessageBox.Show("comment");
                InputPointer += 2;
            return comment;
        }

        Node FunctionCall()
        {
            Node call = new Node("FunctionCall");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in FunctionCall \r\n");
            }
            else
                call.Children.Add(match(Token_Class.Identifier));

            if (TokenStream[InputPointer].token_type != Token_Class.Lbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Lbrace in FunctionCall \r\n");
            }
            else
                call.Children.Add(match(Token_Class.Lbrace));
            Node args = Arguments();
            int Current = InputPointer;
            if(args != null)
                call.Children.Add(args);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected Arguments in FunctionCall \r\n");

            }

            if (TokenStream[InputPointer].token_type != Token_Class.Rbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Rbrace in FunctionCall \r\n");
            }
            else
                call.Children.Add(match(Token_Class.Rbrace));

            return call;
        }
        Node Declaration_Statement()
        {
            Node decl = new Node("Declaration_Statement");
            Node dataty = Datatype();
            if (dataty != null)
                decl.Children.Add(dataty);
            else
                Errors.Error_List.Add("Parsing Error: Missing Datatype in Declaration statement\r\n");
            Node args = IdList();
            int Current = InputPointer;
            if (args != null)
                decl.Children.Add(args);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected Arguments in Declaration statement \r\n");

            }
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in Declaration statement\r\n");
            else
             decl.Children.Add(match(Token_Class.SimiColon));
            return decl;
        }
        Node Read_Statement()
        {
            Node read = new Node("Read_Statement");
            read.Children.Add(match(Token_Class.Read));
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in Read Statement\r\n");
            }
            else
                read.Children.Add(match(Token_Class.Identifier));
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon\r\n");
            }
            else
            {
                read.Children.Add(match(Token_Class.SimiColon));
            }
            return read;
        }
        Node Write_Statement()
        {
            Node write = new Node("Write_Statement");
            write.Children.Add(match(Token_Class.Write));
            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.StringValue)
            {
                write.Children.Add(match(Token_Class.StringValue));
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.EndLine)
            {
                write.Children.Add(match(Token_Class.EndLine));
            }
            else
            {
                int Current = InputPointer;
                Node exp = Expression();
                if (exp != null)
                    write.Children.Add(exp);
                else
                {
                    InputPointer = Current;
                    Errors.Error_List.Add("Parsing Error: Expected an Experssion in write statement\r\n");
                }


            }
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in Write Statement\r\n");
            }
            else
                write.Children.Add(match(Token_Class.SimiColon));
            return write;
        }
        Node Assignment_Statement()
        {
            Node assign = new Node("Assignment_Statement");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Missing Identifier in Assignment statement\r\n");
            }
            else
                assign.Children.Add(match(Token_Class.Identifier));
            if (TokenStream[InputPointer].token_type != Token_Class.AssignOperator)
            {
                Errors.Error_List.Add("Parsing Error: Missing AssignOperator in Assignment statement \r\n");
            }
            else
                assign.Children.Add(match(Token_Class.AssignOperator));
            int Current = InputPointer;
            Node exp = Expression();
            if (exp != null)
                assign.Children.Add(exp);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected an Experssion in assign statement\r\n");
            }
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in in Assignment statement\r\n");
            }
            else
                assign.Children.Add(match(Token_Class.SimiColon));
            return assign;
        }
        // Implement your logic here
        Node FullIfStatement()
        {
            Node fullifStet = new Node("FullIfStatement");
            Node if_state = IfStatement();
            if (if_state != null)
            {
                MessageBox.Show("here2");
                fullifStet.Children.Add(if_state);
                fullifStet.Children.Add(ElsePart());
            }
            else
            {
                while (TokenStream[InputPointer].token_type != Token_Class.End)
                {
                    InputPointer++;
                }
                return null;
            }
            if (TokenStream[InputPointer].token_type != Token_Class.End)
            {
                Errors.Error_List.Add("Parsing Error: Expected end\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                fullifStet.Children.Add(match(Token_Class.End));

            return fullifStet;
        }
        Node ElsePart()
        {
            Node elsepart = new Node("Else Part");
            while (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIF)
            {
                elsepart.Children.Add(ElseifStatment());
            }
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                elsepart.Children.Add(ElseStatment());
            }
            return elsepart;
        }
        Node IfStatement()
        {
            Node ifStet = new Node("If_Statement");
            ifStet.Children.Add(match(Token_Class.If));
            ifStet.Children.Add(Conditions());
            //ifStet.Children.Add(match(Token_Class.Then));
            if (TokenStream[InputPointer].token_type != Token_Class.Then)
            {
                Errors.Error_List.Add("Parsing Error: Expected 'then' in If statement\r\n");
                //bool nextStatement = FoundToken();

                return null;
            }
            else
                ifStet.Children.Add(match(Token_Class.Then));
            ifStet.Children.Add(Statements());
            return ifStet;
        }
        Node ElseifStatment()
        {
            Node elsif = new Node("Elsif_Statement");
            elsif.Children.Add(match(Token_Class.ElseIF));
            elsif.Children.Add(Conditions());
            //elsif.Children.Add(match(Token_Class.Then));
            if (TokenStream[InputPointer].token_type != Token_Class.Then)
            {
                Errors.Error_List.Add("Parsing Error: Expected 'then' in Else If statement\r\n");

                return null;
            }
            else
                elsif.Children.Add(match(Token_Class.Then));
             elsif.Children.Add(Statements());
            return elsif;
        }
        Node ElseStatment()
        {
            Node els = new Node("Else_Statement");
            els.Children.Add(match(Token_Class.Else));
            els.Children.Add(Statements());
            //els.Children.Add(match(Token_Class.End));

            return els;
        }
        Node Return_Statement()
        {
            Node rtn = new Node("Return_Statement");
            rtn.Children.Add(match(Token_Class.Return));
            int Current = InputPointer;
            Node exp = Expression();
            if (exp != null)
                rtn.Children.Add(exp);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected an Experssion in return statement\r\n");
            }
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in return statement\r\n");

            }
            else
                rtn.Children.Add(match(Token_Class.SimiColon));
            return rtn;

        }
        Node BooleanOpeartor()
        {
            Node operat = new Node("BooleanOperator");
            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.AndOperator)
            {
                operat.Children.Add(match(Token_Class.AndOperator));
            }
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.OrOperator)
            {
                operat.Children.Add(match(Token_Class.OrOperator));

            }
            return operat;
        }
        Node Conditions()
        {
            Node conditions = new Node("Conditions");
            int current  = InputPointer;
            Node condition = Condition();
            if (condition != null)
            conditions.Children.Add(condition);
            else
            {
                InputPointer = current;
                Errors.Error_List.Add("Parsing Error: Expected condition\r\n");

            }
            while (TokenStream[InputPointer].token_type == Token_Class.AndOperator || TokenStream[InputPointer].token_type == Token_Class.OrOperator)
            {
                conditions.Children.Add(BooleanOpeartor());

                current = InputPointer;
                condition = Condition();
                if (condition != null)
                    conditions.Children.Add(condition);
                else
                {
                    InputPointer = current;
                    Errors.Error_List.Add("Parsing Error: Expected condition\r\n");

                }
            }
            return conditions;
        }
        Node Condition()
        {
            Node condition = new Node("Condition");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in condition\r\n");
            }
            else
                condition.Children.Add(match(Token_Class.Identifier));
            Node comp = ComparisonOperator();
            int current = InputPointer;
            if(comp != null)
            condition.Children.Add(comp);
            else
            {
                InputPointer = current;
                Errors.Error_List.Add("Parsing Error: Expected Comparison Operator in condition\r\n");

            }
            Node term = Term();
            if (term != null)
            condition.Children.Add(term);
            else
            {
                InputPointer = current;
                Errors.Error_List.Add("Parsing Error: Expected Term in condition\r\n");


            }
            return condition;
        }
        private Node RepeatStatement()
        {
            Node repeat = new Node("Repeat_Statement");
            repeat.Children.Add(match(Token_Class.Repeat));
            repeat.Children.Add(Statements());
            repeat.Children.Add(match(Token_Class.Until));
            int current = InputPointer;
            Node condition = Condition();
            if (condition != null)
                repeat.Children.Add(condition);
            else
            {
                InputPointer = current;
                Errors.Error_List.Add("Parsing Error: Expected condition\r\n");

            }
            return repeat;
        }
        Node ComparisonOperator()
        {
            Node comp = new Node("ComparisonOperator");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.EqualOpertaor)
            {
                comp.Children.Add(match(Token_Class.EqualOpertaor));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.GreaterThanOpertaor)
            {
                comp.Children.Add(match(Token_Class.GreaterThanOpertaor));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LessThanOpertaor)
            {
                comp.Children.Add(match(Token_Class.LessThanOpertaor));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected Comparison Operator\r\n");
                return null;
            }

            return comp;
        }

        Node Arithmatic_Operator()
        {
            Node arithmatic_Operator = new Node("Arithmatic_Operator");
            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.DivisionOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.DivisionOperator));
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.PlusOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.PlusOperator));
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.MinusOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.MinusOperator));
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.MultiplyOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.MultiplyOperator));
            else return null;

            return arithmatic_Operator;

        }

        Node Equation()
        {
            Node equation = new Node("Equation");
            if (TokenStream[InputPointer].token_type == Token_Class.Lbrace)
            {
                MessageBox.Show("lbrace");
                equation.Children.Add(match(Token_Class.Lbrace));
                equation.Children.Add(Equation());
                if (TokenStream[InputPointer].token_type == Token_Class.Rbrace)
                    equation.Children.Add(match(Token_Class.Rbrace));
                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected a Right brace\r\n");
                    return null;
                }

            }
            else
            {
                int current = InputPointer;
                Node term = Term();
                if(term!=null)
                equation.Children.Add(term);
                else
                {
                    InputPointer = current;
                    Errors.Error_List.Add("Parsing Error: Expected a term in the equation\r\n");

                }

                //E-->T
            }



            equation.Children.Add(Equation_tail());






            return equation;
        }
        Node Equation_tail()
        {
            Node equation_tail = new Node("Equation_tail");
            while (InputPointer < TokenStream.Count && (TokenStream[InputPointer].token_type == Token_Class.MinusOperator ||
         TokenStream[InputPointer].token_type == Token_Class.PlusOperator ||
         TokenStream[InputPointer].token_type == Token_Class.MultiplyOperator ||
         TokenStream[InputPointer].token_type == Token_Class.DivisionOperator))
            {


                Node next = Arithmatic_Operator();
                if (next != null)
                {

                    equation_tail.Children.Add(next);
                    if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Lbrace)
                    {
                        equation_tail.Children.Add(match(Token_Class.Lbrace));
                        equation_tail.Children.Add(Equation());
                        if (TokenStream[InputPointer].token_type == Token_Class.Rbrace)
                            equation_tail.Children.Add(match(Token_Class.Rbrace));
                        else
                        {
                            Errors.Error_List.Add("Parsing Error: Expected a Right brace\r\n");
                            return null;
                        }
                    }
                    else if (InputPointer < TokenStream.Count)
                    {
                        int current = InputPointer;
                        Node term = Term();
                        if (term != null)
                            equation_tail.Children.Add(term);
                        else
                        {
                            InputPointer = current;
                            Errors.Error_List.Add("Parsing Error: Expected a term in the equation\r\n");

                        }
                    }


                }



            }
            return equation_tail;
        }
        Node Expression()
        {
            Node expression = new Node("Expression");

            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.StringValue)
                expression.Children.Add(match(Token_Class.StringValue));
            
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Lbrace)
                expression.Children.Add(Equation());
            else
            {
                Node nextOp = null;

                if (InputPointer + 1 < TokenStream.Count)
                {
                    InputPointer++;
                    nextOp = Arithmatic_Operator();
                    InputPointer--;
                }

                if (nextOp != null)
                {
                    InputPointer--;
                    int current = InputPointer;
                    Node eq = Equation();
                    if (eq != null)
                        expression.Children.Add(eq);
                    else
                    {
                        InputPointer = current;
                        Errors.Error_List.Add("Parsing Error: Expected Equation in the equation\r\n");

                    }
                }
                else
                {
                    int current = InputPointer;
                    Node term = Term();
                    if (term != null)
                        expression.Children.Add(term);
                    else
                    {
                        InputPointer = current;
                        Errors.Error_List.Add("Parsing Error: Expected a term in the equation\r\n");

                    }
                }
            }

            return expression;
        }

        Node Term()
        {
            Node term = new Node("Term");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {

                if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.Lbrace)
                {
                    int current = InputPointer;
                    Node funcall = FunctionCall();
                    if(funcall != null) 
                    term.Children.Add(funcall);
                    else
                    {
                        InputPointer = current;
                        Errors.Error_List.Add("Parsing Error: Expected a Function call\r\n");

                    }
                }
                else
                {
                    term.Children.Add(match(Token_Class.Identifier));
                }

            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Constant)
            {
                term.Children.Add(match(Token_Class.Constant));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected a Term\r\n");

                return null;
            }

            return term;
        }
        Node Datatype()
        {
            Node data_type = new Node("Datatype");
            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Integer)
                data_type.Children.Add(match(Token_Class.Integer));
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.String)
                data_type.Children.Add(match(Token_Class.String));
            else if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.Float)
                data_type.Children.Add(match(Token_Class.Float));
            else
            {
                //MessageBox.Show("null72");

                return null;
            }
            return data_type;

        }
        Node IdList()
        {
            Node parameters = new Node("IdList");

            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in Idlist\r\n");
            }
            else
                parameters.Children.Add(match(Token_Class.Identifier));
            if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.AssignOperator)
            {
                parameters.Children.Add(match(Token_Class.AssignOperator));

                int Current = InputPointer;

                //if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
                Node exp = Expression();
                if (exp != null)
                    parameters.Children.Add(exp);
                else
                {
                    InputPointer = Current;
                    Errors.Error_List.Add("Parsing Error: Expected an Experssion in Idlist\r\n");
                }
            }
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters.Children.Add(match(Token_Class.Comma));
                parameters.Children.Add(match(Token_Class.Identifier));
                if (InputPointer < TokenStreamCount && TokenStream[InputPointer].token_type == Token_Class.AssignOperator)
                {
                                    parameters.Children.Add(match(Token_Class.AssignOperator));

                    parameters.Children.Add(Expression());
                }

            }

            return parameters;
        }
        Node Arguments()
        {
            Node arguments = new Node("Arguments");
            Node arg = Argument();
            int Current = InputPointer;
            if(arg!=null)
                arguments.Children.Add(arg);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected an Argument\r\n");

            }
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                arguments.Children.Add(match(Token_Class.Comma));
                Current = InputPointer;
                arg = Argument();
                if (arg != null)
                    arguments.Children.Add(arg);
                else
                {
                    InputPointer = Current;
                    Errors.Error_List.Add("Parsing Error: Expected an Argument\r\n");

                }

            }

            return arguments;
        }
        Node Argument()
        {
            Node argument = new Node("Argument");
            int Current = InputPointer;

            //if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
            Node exp  = Expression();
            if(exp!=null)
                argument.Children.Add(exp);
            else
            {
                InputPointer = Current;
                Errors.Error_List.Add("Parsing Error: Expected an Experssion\r\n");
            }
           // else return null;
            return argument;
        }
        public Node match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString()  + "\r\n");
                InputPointer++;
                return null;
            }
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}
