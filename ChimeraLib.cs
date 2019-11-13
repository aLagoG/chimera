/*
Chimera
Date: 11-Nov-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;

namespace Chimera
{
    public class Lib
    {
        // IO
        public static void WrInt(int i)
        {
            Console.Write(i);
        }
        public static void WrStr(string s)
        {
            Console.Write(s);
        }
        public static void WrStr(bool b)
        {
            Console.Write(b);
        }
        public static void WrLn()
        {
            Console.WriteLine();
        }
        public static int RdInt()
        {
            return Convert.ToInt32(Console.ReadLine());
        }
        public static string RdStr()
        {
            return Console.ReadLine();
        }

        // String
        public static string AtStr(string s, int i)
        {
            return $"{s[i]}";
        }
        public static int LensStrs(string s)
        {
            return s.Length;
        }
        public static int CmpStr(string s1, string s2)
        {
            return s1.CompareTo(s2);
        }
        public static string CatStr(string s1, string s2)
        {
            return s1 + s2;
        }

        // List
        public static int LenLstInt(int[] loi)
        {
            return loi.Length;
        }
        public static int LenLstStr(string[] los)
        {
            return los.Length;
        }
        public static int LenLstBool(bool[] lob)
        {
            return lob.Length;
        }
        public static int[] NewLstInt(int size)
        {
            return new int[size];
        }
        public static string[] NewLstStr(int size)
        {
            return new string[size];
        }
        public static bool[] NewLstBool(int size)
        {
            return new bool[size];
        }

        // Conversion
        public static string IntToStr(int i)
        {
            return $"{i}";
        }
        public static int StrToInt(string s)
        {
            return Convert.ToInt32(s);
        }
    }
}
