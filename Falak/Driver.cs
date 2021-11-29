/*
  Falak compiler - Program driver.
  Copyright (C) 2013-2021 Ariel Ortiz, ITESM CEM

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License, or
  (at your option) any later version.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.

  Modified by team 6
  Javier Pascal Flores          A01375925
  Diego Palmerin Bonada         A01747290
  Hector Ivan Aguirre Arteaga   A01169628

*/

using System;
using System.IO;
using System.Text;

namespace Falak
{

    public class Driver
    {

        const string VERSION = "0.5";

        //-----------------------------------------------------------
        static readonly string[] ReleaseIncludes = {
            "Lexical analysis",
            "Syntactic analysis",
            "AST Construction",
            "Semantic Analysis",
            "Wat code generation"
        };

        //-----------------------------------------------------------
        void PrintAppHeader()
        {
            Console.WriteLine("Falak compiler, version " + VERSION);
            Console.WriteLine(
                "Copyright \u00A9 2013-2021 by A. Ortiz, ITESM CEM. modified"
                + " by team 6");
            Console.WriteLine("This program is free software; you may "
                + "redistribute it under the terms of");
            Console.WriteLine("the GNU General Public License version 3 or "
                + "later.");
            Console.WriteLine("This program has absolutely no warranty.");
        }

        //-----------------------------------------------------------
        void PrintReleaseIncludes()
        {
            Console.WriteLine("Included in this release:");
            foreach (var phase in ReleaseIncludes)
            {
                Console.WriteLine("   * " + phase);
            }
        }

        //-----------------------------------------------------------
        void Run(string[] args)
        {

            PrintAppHeader();
            Console.WriteLine();
            PrintReleaseIncludes();
            Console.WriteLine();

            if (args.Length != 1)
            {
                Console.Error.WriteLine(
                    "Please specify the name of the input file.");
                Environment.Exit(1);
            }

            try
            {
                var inputPath = args[0];
                var outputPath = Path.ChangeExtension(inputPath, ".wat");
                var input = File.ReadAllText(inputPath);

                var parser = new Parser(
                    new Scanner(input).Scan().GetEnumerator());
                var program = parser.Program();
                // Console.WriteLine(program.ToStringTree());
                Console.WriteLine("Syntax Ok");

                var semantic = new SemanticVisitor();
                semantic.Visit((dynamic)program);
                var semantic2 = new SecondSemanticVisitor(semantic.TableVariables, semantic.TableFunctions);
                semantic2.Visit((dynamic)program);
                Console.WriteLine("Semantics OK");

                Console.WriteLine();
                Console.WriteLine("Global Variables");
                Console.WriteLine("============");
                foreach (var entry in semantic.TableVariables)
                {
                    Console.WriteLine(entry);
                }
                Console.WriteLine();
                Console.WriteLine("Functions");
                Console.WriteLine("============");
                foreach (var entry in semantic2.TableFunctions)
                {
                    if (!entry.Value.isPrimitive)
                    {
                        Console.WriteLine(entry);
                    }
                }

                var codeGenerator = new WatVisitor(semantic.TableVariables, semantic2.TableFunctions);
                File.WriteAllText(
                    outputPath,
                    codeGenerator.Visit((dynamic)program));
                Console.WriteLine(
                    "Created Wat (WebAssembly text format) file "
                    + $"'{outputPath}'.");
            }
            catch (Exception e)
            {
                if (e is FileNotFoundException || e is SyntaxError || e is SemanticError)
                {
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }
                throw;

            }
        }

        //-----------------------------------------------------------
        public static void Main(string[] args)
        {
            new Driver().Run(args);
        }
    }
}
