using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtSqlCmd {
    class Program {
        static void Main(string[] args) {
            var options = new ComLineOptions();

            if (!CommandLine.Parser.Default.ParseArguments(args, options)) {
                return;
            }

            options.Validate();
            if ( options.Messages.Count() > 0 ) {
                foreach (var m in options.Messages) {
                    Console.WriteLine(m.Message);
                }
                if ( !options.IsValid )
                    return;
            }

            try {
                BaseTool bt = null;
                switch ( options.Tool) {
                    case Tools.LineComp:
                        bt = new LineCompTool(options);
                        break;
                    case Tools.SearchFields:
                        bt = new SearchFieldsTool(options);
                        break;
                }
                if ( bt != null ) {
                    bt.Action();
                }                
            } catch (Exception ex) {
                while (ex != null) {
                    Console.WriteLine(ex.Message);
                    ex = ex.InnerException;
                }
            }
        }
    }
}

