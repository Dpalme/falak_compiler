/*

Name: // TODO NAME
Student ID: // TODO Student ID



// TODO grammar lol

Original BNF Grammar:

LL(1) Grammar:

*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

// TODO Types of tokens eg. INT, PLUS, TIMES, POW, OPEN_PAR, CLOSE_PAR, EOF, BAD_TOKEN
public enum TokenCategory {
    
}

public class Token {
    public TokenCategory Category { get; }
    public String Lexeme { get; }

    public Token(TokenCategory category, String lexeme) {
        Category = category;
        Lexeme = lexeme;
    }

    public override String ToString() {
        return $"[{Category}, \"{Lexeme}\"]";
    }
}

public class Scanner {
    readonly String input;
    // TODO regular expression for tokens
    static readonly Regex regex = new Regex(
        @"(\d+)|([+])|([*][*])|([*])|([(])|([)])|(\s)|(.)");

    public Scanner(String input) {
        this.input = input;
    }

    public IEnumerable<Token> Scan() {
        var result = new LinkedList<Token>();

        // TODO match group to token category (Group index starts at 1)
        // eg. if (m.Groups[1].Success) { result.AddLast(new Token(TokenCategory.TOKEN, m.Value))}
        foreach (Match m in regex.Matches(input)) {
            
        }
        result.AddLast(new Token(TokenCategory.EOF, null));

        return result;
    }
}

public class SyntaxError: Exception {}

public class SemanticError: Exception {}

public class Parser {
    IEnumerator<Token> tokenStream;

    public Parser(IEnumerator<Token> tokenStream) {
        this.tokenStream = tokenStream;
        this.tokenStream.MoveNext();
    }

    public TokenCategory Current {
        get {
            return tokenStream.Current.Category;
        }
    }

    public Token Expect(TokenCategory category) {
        if (Current == category) {
            Token current = tokenStream.Current;
            tokenStream.MoveNext();
            return current;
        } else {
            throw new SyntaxError();
        }
    }

    public Node Prog() {
        var result = Expr();
        Expect(TokenCategory.EOF);
        var newNode = new Prog();
        newNode.Add(result);
        return newNode;
    }

    // TODO create Node for each token
}

public class Node: IEnumerable<Node> {

    IList<Node> children = new List<Node>();

    public Node this[int index] {
        get {
            return children[index];
        }
    }

    public Token AnchorToken { get; set; }

    public void Add(Node node) {
        children.Add(node);
    }

    public IEnumerator<Node> GetEnumerator() {
        return children.GetEnumerator();
    }

    System.Collections.IEnumerator
    System.Collections.IEnumerable.GetEnumerator() {
        throw new NotImplementedException();
    }

    public override string ToString() {
        return $"{GetType().Name} {AnchorToken}";
    }

    public string ToStringTree() {
        var sb = new StringBuilder();
        TreeTraversal(this, "", sb);
        return sb.ToString();
    }

    static void TreeTraversal(Node node, string indent, StringBuilder sb) {
        sb.Append(indent);
        sb.Append(node);
        sb.Append('\n');
        foreach (var child in node.children) {
            TreeTraversal(child, indent + "  ", sb);
        }
    }
}

public class Prog: Node {}
// TODO create needed Nodes

public class SemanticVisitor {

    public void Visit(Prog node) {
        Visit((dynamic) node[0]);
    }

    // TODO add needed node visitations

    public void Visit(Int node) {
        int result;
        if (!Int32.TryParse(node.AnchorToken.Lexeme, out result)) {
            throw new SemanticError();
        }
    }
}

public class WATVisitor {

    public String Visit(Prog node) {
        return
            "(module\n"
            // TODO add exam library if any is given
            + "  (import \"math\" \"pow\" (func $pow (param i32 i32) (result i32)))\n"
            + "  (func\n"
            + "    (export \"start\")\n"
            + "    (result i32)\n"
            + Visit((dynamic) node[0])
            + "    return\n"
            + "  )\n"
            + ")\n";
    }

    // TODO add needed WAT strings for each node

    public String Visit(Int node) {
        return $"    i32.const {node.AnchorToken.Lexeme}\n";
    }
}

public class Driver {
    public static void Main() {
        // TODO if it's command line input
        Console.Write("> ");
        var line = Console.ReadLine();
        var parser = new Parser(new Scanner(line).Scan().GetEnumerator());


        try {
            var ast = parser.Prog();
            Console.WriteLine(result.ToStringTree());
            new SemanticVisitor().Visit((dynamic) ast);

            File.WriteAllText(
                "output.wat",
                new WATVisitor().Visit((dynamic) ast));

        } catch (SyntaxError) {
            Console.WriteLine("Bad syntax!");
        } catch (SemanticError) {
            Console.WriteLine("Bad semantics!");
        }
    }
}