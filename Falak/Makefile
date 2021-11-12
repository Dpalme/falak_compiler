#   Authors:
#   Javier Pascal Flores          A01375925
#   Diego Palmerin Bonada         A01747290
#   Hector Ivan Aguirre Arteaga   A01169628

falak.exe: Driver.cs Scanner.cs Token.cs TokenCategory.cs Parser.cs \
	SyntaxError.cs Node.cs SpecificNodes.cs SemanticVisitor.cs \
	SemanticError.cs

	mcs -out:falak.exe Driver.cs Parser.cs Scanner.cs Token.cs TokenCategory.cs \
	 SyntaxError.cs Node.cs SpecificNodes.cs SemanticVisitor.cs SemanticError.cs

clean:
	rm -f falak.exe