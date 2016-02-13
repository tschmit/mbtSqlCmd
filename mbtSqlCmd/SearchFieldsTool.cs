using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using System.Data.Common;

namespace mbtSqlCmd {
    public class SearchFieldsTool : BaseTool {
        public SearchFieldsTool(ComLineOptions opts) : base(opts) {
        }

        /// <summary>
        /// Search, by regex, which field(s) of which table(s) hold a given value
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2100:ReviewSqlQueriesForSecurityVulnerabilities", 
            Justification = "user must have database credential, table name as parameter")]
        public override void Action() {
            List<string> excludedTypes = new List<string> {
                "34" // image
            };

            Log("-- Searching in Fields...");

            List<string> tables = new List<string>();
			Regex rg = null;
			if (!string.IsNullOrWhiteSpace (_opts.TableName)) {
				rg = new Regex(
					_opts.TableName.Replace("%", ".*").Replace("_", "."),
					RegexOptions.IgnoreCase);
			}
			using (DbConnection con = _opts.GetDbConnection()) {
                con.Open();
				using (DbCommand com = con.CreateCommand(), com2 = con.CreateCommand()) {
                    
					while (true) {
						com.Connection = con;
						com2.Connection = con;
						com.CommandText = GetQuery (SFQueries.EnumTables);                    
						if (string.IsNullOrWhiteSpace (com.CommandText)) {
							Log ($"-- No query for {_opts.DatabaseSystem}:{SFQueries.EnumColumns}");
							break;
						}
						using (DbDataReader dr = com.ExecuteReader ()) {
							while (dr.Read ()) {
								if (rg != null) {
									if (!rg.IsMatch (dr.GetString (0)))
										continue;
								}
								tables.Add (dr.GetString (0));
							}
						}
						Log($"-- {tables.Count} tables...");

						com.CommandText = GetQuery (SFQueries.EnumColumns);
						if (string.IsNullOrWhiteSpace (com.CommandText)) {
							Log ($"-- No query for {_opts.DatabaseSystem}:{SFQueries.EnumColumns}");
							break;
						}

						DbParameter dbp1 = null;
						if (com.CommandText.Contains ("@")) {
							dbp1 = com.CreateParameter ();
							com.Parameters.Add (dbp1);
							dbp1.ParameterName = "@tid";
							dbp1.DbType = DbType.String;
						}

						DbParameter dbp2 = com2.CreateParameter();
						com2.Parameters.Add (dbp2);
						dbp2.ParameterName = "@v";
						dbp2.DbType = DbType.String;
						dbp2.Value = _opts.Where;

						if (_opts.Verbose) {
							Log($"-- @v = {_opts.Where}");
						}				

						rg = new Regex (
							_opts.Where.Replace ("%", ".*").Replace ("_", "."),
							RegexOptions.IgnoreCase);

						string ccv = GetQuery(SFQueries.CheckColumnValue);

						foreach (string kvp in tables) {                        
							String where = String.Empty;
							if (dbp1 != null) {
								dbp1.Value = kvp;
							} else {
								com.CommandText = string.Format(GetQuery(SFQueries.EnumColumns), kvp);
							}
							if (_opts.Verbose) {
								Log($"-- @tid = {kvp}");
								Log($"-- {com.CommandText}");
							}
							using (DbDataReader dr = com.ExecuteReader ()) {
								while (dr.Read ()) {
									if (excludedTypes.Contains (dr.GetString(1))) {
										Log ("-- TABLE {0}: {1} column is excluded due to his type", kvp, dr[0].ToString ());
										continue;
									}
									where += " or " + string.Format(ccv, dr[0].ToString());
								}
							}
							where = where.Remove (0, 4);

							com2.CommandText = "select * from " + kvp + " where " + where;
							if (_opts.Verbose) {
								Console.WriteLine ($"-- {com2.CommandText}");
							}

							try {
								using (DbDataReader dr2 = com2.ExecuteReader ()) {
									if (dr2.HasRows) {
										Log ("-- TABLE {0} ---------------------", kvp);
										int fieldCount = dr2.FieldCount;
										int rowCount = 0;
										while (dr2.Read ()) {
											rowCount++;
											Log($"-- Row {rowCount, 8} -----");
											for (int i = 0; i < fieldCount; i++) {
												if (rg.IsMatch (dr2 [i].ToString ())) {
													Log ("{0,-15}:{1}", dr2.GetName (i), dr2 [i].ToString ());
												}
											}
										}
										Log ("-- TABLE {0}: {1} rows found", kvp, rowCount);
									}
								}
							} catch (Exception ex) {	
								Log (ex.Message);
							}
						}
						break;
					}
                }
            }
        }

		public enum SFQueries {
			EnumTables,
			EnumColumns,
			CheckColumnValue
		}

		public string GetQuery(SFQueries which) {
			switch (which) {
				case SFQueries.CheckColumnValue:
					switch (_opts.DatabaseSystem) {
						case DatabaseSystems.mssql:
							return "cast([{0}] as nvarchar) like @v ";
						case DatabaseSystems.mysql :
							return "{0} like @v";
						default :
							return null;
					}
				case SFQueries.EnumTables:
					switch (_opts.DatabaseSystem) {
						case DatabaseSystems.mssql:
							return "select [name] from sys.tables order by [name]";
						case DatabaseSystems.mysql:
							return "show tables";
					    default :
						    return null;
					}
				case SFQueries.EnumColumns:
					switch (_opts.DatabaseSystem) {
						case DatabaseSystems.mssql:
							return "select c.name, cast(c.system_type_id as varchar) from sys.columns c join sys.tables t on c.object_id = t.object_id where t.name = @tid";
						case DatabaseSystems.mysql:
							return "describe {0}";
						default :
							return null;
					}
				default:
					return null;
			}
		}
    }    
}
