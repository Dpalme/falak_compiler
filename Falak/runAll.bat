cd D:\Escuela\compiladores\falak_compiler\falak
"C:\Program Files\Mono\bin\mcs" -out:falak.exe Driver.cs Parser.cs Scanner.cs Token.cs TokenCategory.cs SyntaxError.cs Node.cs SpecificNodes.cs SemanticVisitor.cs SemanticError.cs WatVisitor.cs
falak.exe Test_Files/001_hello.falak
falak.exe Test_Files/002_binary.falak
falak.exe Test_Files/003_palindrome.falak
falak.exe Test_Files/004_factorial.falak
falak.exe Test_Files/005_arrays.falak
falak.exe Test_Files/006_next_day.falak
falak.exe Test_Files/007_literals.falak
falak.exe Test_Files/008_vars.falak
falak.exe Test_Files/009_operators.falak
falak.exe Test_Files/010_breaks.falak
python Test_Files/runall.py