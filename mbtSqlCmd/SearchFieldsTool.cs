using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace mbtSqlCmd {
    public class SearchFieldsTool : BaseTool {
        public SearchFieldsTool(ComLineOptions opts) : base(opts) {
        }
        [SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", 
            Justification = "user must have database credential, table name as parameter")]
        public override void Action() {
            List<byte> excludedTypes = new List<byte> {
                34 // image
            };

            Log("Searching in Fields...");

            Dictionary<string, int> tables = new Dictionary<string, int>();
            using (SqlConnection con = new SqlConnection(_opts.GetConnectionString())) {
                con.Open();
                using (SqlCommand com = new SqlCommand(), com2 = new SqlCommand()) {
                    com.Connection = con; com2.Connection = con;
                    com.CommandText = "select [object_id], [name] from sys.tables";
                    if ( !String.IsNullOrEmpty(_opts.TableName )) {
                        com.CommandText += " where [name] like '" + _opts.TableName + "'";
                    }
                    com.CommandText += " order by [name]";
                    using (SqlDataReader dr = com.ExecuteReader()) {
                        while (dr.Read())
                            tables[dr.GetString(1)] = dr.GetInt32(0);
                    }

                    com.CommandText = "select name, system_type_id from sys.columns where object_id = @tid";
                    com.Parameters.Add("@tid", SqlDbType.Int);                    

                    com2.Parameters.Add("@v", SqlDbType.NVarChar);
                    com2.Parameters["@v"].Value = _opts.Where;
                    Regex rg = new Regex(
                        _opts.Where.Replace("%", ".*").Replace("_", "."),
                        RegexOptions.IgnoreCase);

                    foreach (KeyValuePair<string, int> kvp in tables) {                        
                        String where = String.Empty;
                        com.Parameters["@tid"].Value = kvp.Value;
                        using (SqlDataReader dr = com.ExecuteReader()) {
                            while (dr.Read()) {
                                if ( excludedTypes.Contains(dr.GetByte(1))) {
                                    Log("-- TABLE {0}: {1} column is excluded due to his type", kvp.Key, dr[0].ToString());
                                    continue;
                                }
                                where += "or cast([" + dr[0].ToString() + "] as nvarchar) like @v ";
                            }
                        }
                        where = where.Remove(0, 3);
                        
                        com2.CommandText = "select * from " + kvp.Key + " where " + where;
                        using (SqlDataReader dr2 = com2.ExecuteReader()) {
                            if (dr2.HasRows) {
                                Log("-- TABLE {0}:", kvp.Key);
                                int fieldCount = dr2.FieldCount;
                                int rowCount = 0;
                                while (dr2.Read()) {
                                    rowCount++;
                                    for(int i = 0; i < fieldCount; i++) {
                                        if (rg.IsMatch(dr2[i].ToString())) {
                                            Log("{0,-15}:{1}", dr2.GetName(i), dr2[i].ToString());
                                        }
                                    }
                                }
                                Log("-- TABLE {0}: {1} rows found", kvp.Key, rowCount);
                            }
                        }
                    }
                }
            }
        }
    }    
}
