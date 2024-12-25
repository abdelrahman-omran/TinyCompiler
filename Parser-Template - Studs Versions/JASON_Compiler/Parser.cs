using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Remoting.Messaging;
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
        List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
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
                TokenStream[InputPointer + 1].token_type == Token_Class.AssignOperator)
            {
                return true;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                return true;
            }
            // if the curr token is read
            else if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                return true;
            }
            // if the curr token is if
            else if (TokenStream[InputPointer].token_type == Token_Class.If)
            {
                return true;
            }
 
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                return true;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Return)
            {
                return true;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.DivisionOperator && TokenStream[InputPointer + 1].token_type == Token_Class.MultiplyOperator)
            {
                return true;
            }
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
            program.Children.Add(MainFunction());
            //program.Children.Add(match(Token_Class.Dot));
            MessageBox.Show("Success");
            return program;
        }

        Node Functions()
        {
            Node functions = new Node("Functions");
            while (TokenStream[InputPointer + 1].token_type != Token_Class.Main)
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
            mainfunc.Children.Add(Datatype());
            mainfunc.Children.Add(match(Token_Class.Main));
            
            if (TokenStream[InputPointer].token_type != Token_Class.Lbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Lbrace in main function\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    MessageBox.Show("here");

                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                mainfunc.Children.Add(match(Token_Class.Lbrace));
            if (TokenStream[InputPointer].token_type != Token_Class.Rbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Rbrace in main function\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                mainfunc.Children.Add(match(Token_Class.Lbrace));
            mainfunc.Children.Add(FunctionBody());
            return mainfunc;
        }

        Node FunctionDecl()
        {
            Node function_dec = new Node("Function_dec");
            function_dec.Children.Add(Datatype());
            function_dec.Children.Add(Function_name());
            if (TokenStream[InputPointer].token_type != Token_Class.Rbrace)
            {
                Errors.Error_List.Add("Parsing Error: Expected Rbrace in function declaration\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {

                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                function_dec.Children.Add(match(Token_Class.Rbrace));

            if (TokenStream[InputPointer].token_type == Token_Class.Rbrace)
                function_dec.Children.Add(match(Token_Class.Rbrace));
            else
            {
                function_dec.Children.Add(Parameters());
                if (TokenStream[InputPointer].token_type != Token_Class.Lbrace)
                {
                    Errors.Error_List.Add("Parsing Error: Expected Rbrace in function declaration \r\n");
                    bool nextStatement = FoundToken();
                    while (!nextStatement)
                    {
                        InputPointer++;
                        nextStatement = FoundToken();

                    }

                    return null;
                }
                else
                    function_dec.Children.Add(match(Token_Class.Lbrace));

            }
            return function_dec;
        }
        Node Function_name()
        {
            Node function_name = new Node("Function_name");
            if (TokenStream[InputPointer].token_type != Token_Class.Identifier)
            {
                Errors.Error_List.Add("Parsing Error: Expected Identifier in function declaration \r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                function_name.Children.Add(match(Token_Class.Identifier));
            return function_name;
        }

        Node Parameters()
        {
            Node parameters = new Node("Parameters");

            parameters.Children.Add(Parameter());
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters.Children.Add(match(Token_Class.Comma));
                parameters.Children.Add(Parameter());

            }

            return parameters;
        }
        Node Parameter()
        {
            Node parameter = new Node("Parameter");
            parameter.Children.Add(Datatype());

            if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
                parameter.Children.Add(match(Token_Class.Identifier));
            else return null;
            return parameter;
        }
        Node FunctionBody()
        {
            Node funcbody = new Node("Body");
            funcbody.Children.Add(match(Token_Class.Lparan));
            funcbody.Children.Add(Statements());
            funcbody.Children.Add(match(Token_Class.Rparan));
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

            if (TokenStream[InputPointer].token_type == Token_Class.Integer ||
                TokenStream[InputPointer].token_type == Token_Class.Float ||
                TokenStream[InputPointer].token_type == Token_Class.String)
            {

                statement.Children.Add(Declaration_Statement());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Identifier &&
                TokenStream[InputPointer+1].token_type == Token_Class.AssignOperator)
            {
                statement.Children.Add(Assignment_Statement());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                statement.Children.Add(Write_Statement());
                return statement;
            }
            // if the curr token is read
            else if (TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                statement.Children.Add(Read_Statement());
                return statement;
            }
            // if the curr token is if
            else if (TokenStream[InputPointer].token_type == Token_Class.If)
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
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                statement.Children.Add(RepeatStatement());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Return)
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
            call.Children.Add(match(Token_Class.Identifier));
            call.Children.Add(match(Token_Class.Lbrace));
            call.Children.Add(Arguments());
            call.Children.Add(match(Token_Class.Rbrace));

            return call;
        }
        Node Declaration_Statement()
        {
            Node decl = new Node("Declaration_Statement");
            decl.Children.Add(Datatype());
            decl.Children.Add(IdList());
            decl.Children.Add(match(Token_Class.SimiColon));
            return decl;
        }
        Node Read_Statement()
        {
            Node read = new Node("Read_Statement");
            read.Children.Add(match(Token_Class.Read));
            read.Children.Add(match(Token_Class.Identifier));
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
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
            if (TokenStream[InputPointer].token_type == Token_Class.StringValue)
            {
                write.Children.Add(match(Token_Class.StringValue));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.EndLine)
            {
                write.Children.Add(match(Token_Class.EndLine));
            }
            else
            {
                write.Children.Add(Expression());

            }
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in Write Statement\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
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
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                assign.Children.Add(match(Token_Class.Identifier));
            if (TokenStream[InputPointer].token_type != Token_Class.AssignOperator)
            {
                Errors.Error_List.Add("Parsing Error: Missing AssignOperator in Assignment statement \r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            else
                assign.Children.Add(match(Token_Class.AssignOperator));
            assign.Children.Add(Expression());
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon in in Assignment statement\r\n");
                bool nextStatement = FoundToken();
                while (!nextStatement)
                {
                    InputPointer++;
                    nextStatement = FoundToken();

                }

                return null;
            }
            assign.Children.Add(match(Token_Class.SimiColon));
            return assign;
        }
        // Implement your logic here
        Node FullIfStatement()
        {
            Node fullifStet = new Node("FullIfStatement");

            fullifStet.Children.Add(IfStatement());
            fullifStet.Children.Add(ElsePart());
            if(TokenStream[InputPointer].token_type != Token_Class.End)
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
            ifStet.Children.Add(match(Token_Class.Then));
            ifStet.Children.Add(Statements());
            return ifStet;
        }
        Node ElseifStatment()
        {
            Node elsif = new Node("Elsif_Statement");
            elsif.Children.Add(match(Token_Class.ElseIF));
            elsif.Children.Add(Conditions());
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
            rtn.Children.Add(Expression());
            if (TokenStream[InputPointer].token_type != Token_Class.SimiColon)
            {
                Errors.Error_List.Add("Parsing Error: Expected SemiColon\r\n");
                InputPointer++;

                return null;
            }
            else
                rtn.Children.Add(match(Token_Class.SimiColon));
            return rtn;

        }
        Node BooleanOpeartor()
        {
            Node operat = new Node("BooleanOperator");
            if (TokenStream[InputPointer].token_type == Token_Class.AndOperator)
            {
                operat.Children.Add(match(Token_Class.AndOperator));
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.OrOperator)
            {
                operat.Children.Add(match(Token_Class.OrOperator));

            }
            return operat;
        }
        Node Conditions()
        {
            Node conditions = new Node("Conditions");
            conditions.Children.Add(Condition());
            while (TokenStream[InputPointer].token_type == Token_Class.AndOperator || TokenStream[InputPointer].token_type == Token_Class.OrOperator)
            {
                conditions.Children.Add(BooleanOpeartor());
                conditions.Children.Add(Condition());
            }
            return conditions;
        }
        Node Condition()
        {
            Node condition = new Node("Condition");
            condition.Children.Add(match(Token_Class.Identifier));
            condition.Children.Add(ComparisonOperator());
            condition.Children.Add(Term());
            return condition;
        }
        private Node RepeatStatement()
        {
            Node repeat = new Node("Repeat_Statement");
            repeat.Children.Add(match(Token_Class.Repeat));
            repeat.Children.Add(Statements());
            repeat.Children.Add(match(Token_Class.Until));
            repeat.Children.Add(Condition());
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
            if (TokenStream[InputPointer].token_type == Token_Class.DivisionOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.DivisionOperator));
            else if (TokenStream[InputPointer].token_type == Token_Class.PlusOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.PlusOperator));
            else if (TokenStream[InputPointer].token_type == Token_Class.MinusOperator)
                arithmatic_Operator.Children.Add(match(Token_Class.MinusOperator));
            else if (TokenStream[InputPointer].token_type == Token_Class.MultiplyOperator)
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
                equation.Children.Add(Term());

                //E-->T
            }




            while (InputPointer < TokenStream.Count && (TokenStream[InputPointer].token_type == Token_Class.MinusOperator ||
           TokenStream[InputPointer].token_type == Token_Class.PlusOperator ||
           TokenStream[InputPointer].token_type == Token_Class.MultiplyOperator ||
           TokenStream[InputPointer].token_type == Token_Class.DivisionOperator))
            {

                //E-->TOT|TO(E)
                Node next = Arithmatic_Operator();
                if (next != null)
                {

                    equation.Children.Add(next);
                    if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Lbrace)
                        equation.Children.Add(Equation());
                    else if (InputPointer < TokenStream.Count) equation.Children.Add(Term());


                }



            }


            return equation;
        }
        Node Expression()
        {
            Node expression = new Node("Expression");

            if (TokenStream[InputPointer].token_type == Token_Class.StringValue)
                expression.Children.Add(match(Token_Class.StringValue));
            
            else if (TokenStream[InputPointer].token_type == Token_Class.Lbrace)
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
                    expression.Children.Add(Equation());
                }
                else
                {
                    expression.Children.Add(Term());
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
                    term.Children.Add(FunctionCall());
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
            if (TokenStream[InputPointer].token_type == Token_Class.Integer)
                data_type.Children.Add(match(Token_Class.Integer));
            else if (TokenStream[InputPointer].token_type == Token_Class.String)
                data_type.Children.Add(match(Token_Class.String));
            else if (TokenStream[InputPointer].token_type == Token_Class.Float)
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


            parameters.Children.Add(match(Token_Class.Identifier));
            if (TokenStream[InputPointer].token_type == Token_Class.AssignOperator)
            {
                parameters.Children.Add(match(Token_Class.AssignOperator));

                parameters.Children.Add(Expression());
            }
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                parameters.Children.Add(match(Token_Class.Comma));
                parameters.Children.Add(match(Token_Class.Identifier));
                if (TokenStream[InputPointer].token_type == Token_Class.AssignOperator)
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

            arguments.Children.Add(Argument());
            while (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                arguments.Children.Add(match(Token_Class.Comma));
                arguments.Children.Add(Argument());

            }

            return arguments;
        }
        Node Argument()
        {
            Node argument = new Node("Argument");

            //if (TokenStream[InputPointer].token_type == Token_Class.Identifier)
                argument.Children.Add(Expression());
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
