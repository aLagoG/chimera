/*
Chimera
Date: 21-Oct-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace Chimera
{

    public class Driver
    {

        const string VERSION = "0.3";

        //-----------------------------------------------------------
        static readonly string[] ReleaseIncludes = {
            "Lexical analysis",
            "Syntactic analysis"
        };

        //-----------------------------------------------------------
        void PrintAppHeader()
        {
            Console.WriteLine("Chimera compiler, version " + VERSION);
            Console.WriteLine("Copyright \u00A9 2013 by Andres, Ian, Servio, ITESM CEM."
            );
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

            if (args.Length == 0)
            {
                Console.Error.WriteLine(
                    "Please specify the name of the input file.");
                Environment.Exit(1);
            }

            foreach (string inputPath in args)
            {
                try
                {
                    var input = File.ReadAllText(inputPath);
                    var parser = new Parser(new Scanner(input).Start().GetEnumerator());
                    var program = parser.Program();
                    Console.Write(program.ToStringTreeDot());
                }
                catch (Exception e)
                {
                    Console.Error.WriteLine($"Exception on file: '{inputPath}'");
                    if (e is FileNotFoundException || e is SyntaxError)
                    {
                        Console.Error.WriteLine(e.Message);
                        Environment.Exit(1);
                    }

                    throw;
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
