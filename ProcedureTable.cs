/*
Chimera
Date: 2-Dic-2019
Authors:
	A01371779 Andres De Lago Gomez
	A01377503 Ian Neumann Sanchez
	A01371719 Servio Tulio Reyes Castillo
*/

using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Chimera
{

    public class ProcedureTable : IEnumerable<KeyValuePair<string, ProcedureTable.Row>>
    {

        public class Row
        {
            public Row(Type type, bool isPredefined)
            {
                this.type = type;
                this.isPredefined = isPredefined;
                symbols = new SymbolTable();
            }
            public Type type { get; private set; }
            public bool isPredefined { get; private set; }
            public SymbolTable symbols { get; private set; }

            public override string ToString()
            {
                var symbolsSting = "";
                if (symbols.Count() > 0)
                {
                    symbolsSting = "\t" + symbols.ToString().Replace("\n", "\n\t");
                }
                return $"{isPredefined}, {type} \n{symbolsSting}";
            }
        }

        IDictionary<string, ProcedureTable.Row> data = new SortedDictionary<string, ProcedureTable.Row>();

        //-----------------------------------------------------------
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("Procedure Table\n");
            sb.Append("====================\n");
            foreach (var entry in data)
            {
                if (entry.Value.isPredefined)
                {
                    continue;
                }
                sb.Append(String.Format("{0}: {1}\n",
                                        entry.Key,
                                        entry.Value));
            }
            sb.Append("====================\n");
            return sb.ToString();
        }

        //-----------------------------------------------------------
        public ProcedureTable.Row this[string key]
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
        public IEnumerator<KeyValuePair<string, ProcedureTable.Row>> GetEnumerator()
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
