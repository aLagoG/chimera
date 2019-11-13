/*
Chimera
Date: 11-Nov-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Linq;

namespace Chimera
{
    public enum Type
    {
        BOOL,
        INT,
        VOID,
        STRING,
        LIST,
        INT_LIST,
        BOOL_LIST,
        STRING_LIST,
    }

    static class TypeMethods
    {

        public static Type ToListType(this Type t)
        {
            switch (t)
            {
                case Type.INT:
                    return Type.INT_LIST;
                case Type.STRING:
                    return Type.STRING_LIST;
                case Type.BOOL:
                    return Type.BOOL_LIST;
                default:
                    throw new Exception($"Type {t} has no equivalent list type");
            }
        }

        public static Type FromListType(this Type t)
        {
            switch (t)
            {
                case Type.INT_LIST:
                    return Type.INT;
                case Type.STRING_LIST:
                    return Type.STRING;
                case Type.BOOL_LIST:
                    return Type.BOOL;
                default:
                    throw new Exception($"List type {t} has no equivalent type");
            }
        }

        public static bool CompatibleWith(this Type t, Type other)
        {
            if (t == Type.LIST || other == Type.LIST)
            {
                Type otherType = t == Type.LIST ? other : t;
                var valid = new Type[] { Type.LIST, Type.BOOL_LIST, Type.INT_LIST, Type.STRING_LIST };
                return valid.Contains(otherType);
            }
            return t == other;
        }

        public static string ToCilType(this Type type)
        {
            switch (type)
            {
                case Type.BOOL:
                case Type.INT:
                    return "int32";
                case Type.STRING:
                    return "string";
                case Type.BOOL_LIST:
                case Type.INT_LIST:
                    return "int32[]";
                case Type.STRING_LIST:
                    return "string[]";
                case Type.VOID:
                    return "void";
            }
            throw new Exception($"Could not find CIL type for: {type}");
        }
    }
}
