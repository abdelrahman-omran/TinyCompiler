using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

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
        public  Node root;
        
        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
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
            functions.Children.Add(Function());
            return functions;
        }
        Node Function()
        {
            Node function = new Node("Function");
            function.Children.Add(FuncDecl());
            function.Children.Add(FunBody());
            return function;
        }
        Node MainFunction()
        {
            Node mainfunc = new Node("Main");
            mainfunc.Children.Add(DataType());
            mainfunc.Children.Add(match(Token_Class.Main));
            mainfunc.Children.Add(match(Token_Class.Lbrace));
            mainfunc.Children.Add(match(Token_Class.Rbrace));
            mainfunc.Children.Add(FunBody());
            return mainfunc;
        }

        Node FuncDecl()
        {
            Node funcdecl = new Node("FuncDecl");
            return funcdecl;
        }

        Node FuncBody()
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
            statements.Children.Add(Statement());
            return statements;
        }
        Node Statement()
        {
            Node statement = new Node("Statement");
            if (1 == 0) ;
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
                statement.Children.Add(IfStatement());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.ElseIF)
            {
                statement.Children.Add(ElseifStatment());
                return statement;
            }
            else if (TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                statement.Children.Add(ElseStatment());
                return statement;
            }
            // if the curr token is repeat
            else if (TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                statement.Children.Add(RepeatStatement());
                return statement;
            }

            return statement;
        }
        Node Read_Statement()
        {
            Node read = new Node("Read_Statement");
            read.Children.Add(match(Token_Class.Read));
            read.Children.Add(match(Token_Class.Identifier));
            read.Children.Add(match(Token_Class.SimiColon));
            return read;
        }
        Node Write_Statement()
        {
            Node write = new Node("Write_Statement");
            write.Children.Add(match(Token_Class.Write));
            write.Children.Add(match(Token_Class.Identifier));
            write.Children.Add(match(Token_Class.SimiColon));
            return write;
        }
        Node Assignment_Statement()
        {
            Node assign = new Node("Assignment_Statement");

            assign.Children.Add(match(Token_Class.Identifier));
            assign.Children.Add(match(Token_Class.AssignOperator));
            assign.Children.Add(Term());
            assign.Children.Add(match(Token_Class.SimiColon));
            return assign;
        }
        // Implement your logic here
        Node IfStatement()
        {
            Node ifStet = new Node("If_Statement");
            ifStet.Children.Add(match(Token_Class.If));
            ifStet.Children.Add(Condition());
            ifStet.Children.Add(match(Token_Class.Then));
            //ifstet.Children.Add(Statements());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIF)
            {
                ifStet.Children.Add(ElseifStatment());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                ifStet.Children.Add(ElseifStatment());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.End)
            {
                ifStet.Children.Add(match(Token_Class.End));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected end\r\n");
                // InputPointer++;

                return null;
            }
            return ifStet;
        }
        Node ElseifStatment()
        {
            Node elsif = new Node("Elsif_Statement");
            elsif.Children.Add(match(Token_Class.ElseIF));
            elsif.Children.Add(Condition());
            elsif.Children.Add(match(Token_Class.Then));
            //elsif.Children.Add(Statements());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.ElseIF)
            {
                elsif.Children.Add(ElseifStatment());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                elsif.Children.Add(ElseStatment());
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.End)
            {
                elsif.Children.Add(match(Token_Class.End));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected end\r\n");
                // InputPointer++;

                return null;
            }
            return elsif;
        }
        Node ElseStatment()
        {
            Node els = new Node("Else_Statement");
            els.Children.Add(match(Token_Class.Else));
            //els.Children.Add(Statements());
            els.Children.Add(match(Token_Class.End));

            return null;
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
            //repeat.Children.Add(Statements());
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
        Node Term()
        {
            Node term = new Node("Term");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Identifier)
            {

                if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.Lbrace)
                {
                    //term.Children.Add(FunctionCall());
                }
                else
                {
                    term.Children.Add(match(Token_Class.Identifier));
                }

            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Constant)
            {
                term.Children.Add(match(Token_Class.Identifier));
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected a Term\r\n");

                return null;
            }

            return term;
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
