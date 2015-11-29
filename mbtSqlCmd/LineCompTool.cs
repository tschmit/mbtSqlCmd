using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtSqlCmd {
    public class LineCompTool : BaseTool {

        public LineCompTool(ComLineOptions opts) : base(opts) {
        }

        public override void Action() {
            Log("Line comparator");            
            List<String> stSqls = new List<String>();

            if (!String.IsNullOrEmpty(_opts.Query)) {
                stSqls.Add(_opts.Query);
            }

            if (!String.IsNullOrEmpty(_opts.TableName)) {
                stSqls.Add(
                    String.Format("select * from {0} {1}",
                        _opts.TableName,
                        String.IsNullOrEmpty(_opts.Where) ? "" : String.Format("where {0}", _opts.Where)));
            }

            if ( !String.IsNullOrEmpty(_opts.InputFile)) {
                StringBuilder sb = new StringBuilder(1000);
                foreach (String line in System.IO.File.ReadAllLines(_opts.InputFile) ) {
                    if (line.Trim().Length == 0)
                        continue;
                    if ( line.ToLower().StartsWith("go")) {
                        if (sb.Length > 0) {
                            stSqls.Add(sb.ToString());
                            sb.Clear();
                        }
                        continue;
                    }
                    sb.AppendLine(line);
                }
                if ( sb.Length > 0 ) {
                    stSqls.Add(sb.ToString());
                }
            }

            Log();
            Log(_opts.GetConnectionString(true));            
            Log();

            Int32 i = 0;
            List<List<object[]>> rows = new List<List<object[]>>();
            Int32 fieldCount = 0;
            Int32 maxFieldNameLength = 15;
            using (SqlConnection con = new SqlConnection(_opts.GetConnectionString())) {
                con.Open();
                using(SqlCommand com = new SqlCommand()) {
                    com.Connection = con;
                    foreach (String stSql in stSqls) {
                        Log(stSql.Trim());
                        com.CommandText = stSql;
                        using (SqlDataReader dr = com.ExecuteReader()) {
                            do {
                                fieldCount = dr.FieldCount;
                                List<object[]> l = new List<object[]>();
                                object[] os = new object[fieldCount];
                                l.Add(os);
                                rows.Add(l);
                                for (i = 0; i < dr.FieldCount; i++) {
                                    os[i] = dr.GetName(i);
                                    if (os[i].ToString().Length > maxFieldNameLength)
                                        maxFieldNameLength = os[i].ToString().Length;
                                }
                                i = 0;
                                while (dr.Read()) {
                                    os = new object[fieldCount];
                                    dr.GetValues(os);
                                    l.Add(os);
                                    i++;
                                }
                            } while (dr.NextResult());
                        }
                    }
                }
            }

            foreach (List<object[]> l in rows) {
                if (l.Count < 3) {
                    Log("-- not enough rows");
                    return;
                }
                if ( l.Count % 2 == 0) {
                    Log("-- wrong number of rows");
                    return;
                }
                Log("--");
                Int32 delta = (l.Count - 1) / 2;
                for (Int32 j = 1; j <= delta; j++) {
                    Log("-- Comparing {0} and {1}", j, j + delta);
                    for (i = 0; i < l[1].Length; i++) {
                        if (l[j][i] is DateTime || l[j][i] is SqlDateTime) {
                            DateTime dt1 = (DateTime)l[j][i], dt2 = (DateTime)l[j + delta][i];
                            if (dt1 != dt2) {
                                Log("{0,-" + maxFieldNameLength.ToString() + "} : {1} => {2}", l[0][i].ToString(), dt1.ToString("O"), dt2.ToString("O"));
                            }
                            continue;
                        }
                        if (l[j][i].ToString() != l[j + delta][i].ToString()) {
                            Log("{0,-" + maxFieldNameLength.ToString() + "} : {1} => {2}", l[0][i].ToString(), l[j][i].ToString(), l[j + delta][i].ToString());
                        }
                    }
                }
            }
        }
    }
}
