using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtSqlCmd {
    public abstract class BaseTool {
        protected ComLineOptions _opts;


        protected void Log() {
            Console.WriteLine();
        }

        protected void Log(String m, params object[] os) {
            Console.WriteLine(m, os);
        }

        public BaseTool(ComLineOptions opts) {
            _opts = opts;
        }

        public abstract void Action();
    }
}
