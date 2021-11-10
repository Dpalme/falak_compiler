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

        const string VERSION = "0.3";

        //-----------------------------------------------------------
        static readonly string[] ReleaseIncludes = {
            "Lexical analysis",
            "Syntactic analysis",
            "AST Construction",
            "Semantic analysis"
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
                var input = File.ReadAllText(inputPath);
                var parser = new Parser(
                    new Scanner(input).Scan().GetEnumerator());
                var program = parser.Program();
                Console.WriteLine("Syntax Ok");
                // Console.Write(program.ToStringTree());

                var semantics = new SemanticVisitor();
                semantics.Visit((dynamic) program);
                Console.WriteLine("Semantics OK");
                Console.WriteLine();
                Console.WriteLine("Variables Table");
                Console.WriteLine("============");
                foreach (var entry in semantics.VariablesTable)
                {
                    Console.WriteLine(entry);
                }
                Console.WriteLine();
                Console.WriteLine("Functions Table");
                Console.WriteLine("============");
                foreach (var entry in semantics.FunctionsTable)
                {
                    if (!entry.Value.isPrimitive) {
                        Console.WriteLine(entry);
                    }
                }


            }
            catch (FileNotFoundException e)
            {
                if (e is FileNotFoundException || e is SyntaxError || e is SemanticError)
                {
                    Console.Error.WriteLine(e.Message);
                    Environment.Exit(1);
                }

            }
        }

        //-----------------------------------------------------------
        public static void Main(string[] args)
        {
            new Driver().Run(args);
        }
    }
}
