using System;
using System.IO;
using System.Text.RegularExpressions;

public class NoComments {
    public static void Main() {
        var regex = new Regex(@"([/][*].*?[*][/])|(.)", RegexOptions.Singleline);
        var text = File.ReadAllText("hello.c");
        foreach (Match match in regex.Matches(text)) {
            if (match.Groups[2].Success) {
                Console.Write(match.Value);
            }
        }
    }
}