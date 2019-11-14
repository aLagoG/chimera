/*
Chimera
Date: 11-Nov-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Text;
using System.Collections.Generic;

namespace Chimera
{

    public class SymbolTable : IEnumerable<KeyValuePair<string, SymbolTable.Row>>
    {

        public class Row
        {
            public Row(Type type, Kind kind, int pos = -1, dynamic value = null)
            {
                this.type = type;
                this.kind = kind;
                this.pos = pos;
                this.value = value;
            }
            public Type type { get; private set; }
            public Kind kind { get; private set; }
            public int pos { get; private set; }
            public dynamic value { get; private set; }

            public override string ToString()
            {
                var posString = pos == -1 ? "-" : $"{pos}";
                return $"{type}, {kind}, {posString}";
            }
        }

        IDictionary<string, SymbolTable.Row> data = new SortedDictionary<string, SymbolTable.Row>();

        //-----------------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Symbol Table\n");
            sb.Append("====================\n");
            foreach (var entry in data)
            {
                sb.Append(String.Format("{0}: {1}\n",
                                        entry.Key,
                                        entry.Value));
            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        public SymbolTable.Row this[string key]
        {
            get
            {
                return data[key];
            }
            set
            {
                data[key] = value;
            }
        }

        //-----------------------------------------------------------
        public bool Contains(string key)
        {
            return data.ContainsKey(key);
        }

        //-----------------------------------------------------------
        public IEnumerator<KeyValuePair<string, SymbolTable.Row>> GetEnumerator()
        {
            return data.GetEnumerator();
        }

        //-----------------------------------------------------------
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
