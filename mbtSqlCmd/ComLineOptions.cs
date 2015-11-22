using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mbtSqlCmd {
    public enum Tools {
        LineComp,
        SearchFields
    }


    public class ComLineOptions {
        [Option('d', "database", Required = true, DefaultValue = null, HelpText = "database name")]
        public String DatabaseName { get; set; }

        [Option('E', "usetrusted", Required = false, DefaultValue = false, HelpText = "use trusted connexion")]
        public bool UseTrustedConnexion { get; set; }

        [Option('i', "input", Required = false, DefaultValue = null, HelpText = "input file")]
        public String InputFile { get; set; }

        [Option('P', "pwd", Required = false, DefaultValue = null, HelpText = "password")]
        public String Password { get; set; }

        [Option('S', "instance", Required = true, DefaultValue = null, HelpText = "sql instance")]
        public String ServerName { get; set; }

        [Option('t', "table", Required = false, DefaultValue = null, HelpText = "table name or table name mask for SearchFields")]
        public String TableName { get; set; }

        [Option('U', "user", Required = false, DefaultValue = null, HelpText = "user name")]
        public String UserName { get; set; }

        [Option("tool", Required = false, DefaultValue = Tools.LineComp, HelpText = "tool name: LineComp, SearchFields")]
        public Tools Tool { get; set; }

        [Option('w', "where", Required = false, DefaultValue = null, HelpText = "where clause without the where, or field value name mask for SearchFields")]
        public String Where { get; set; }


        [HelpOption]
        public String GetUsage() {
            var help = new HelpText {
                Heading = new HeadingInfo("mbtSqlTools", "1.0.0.0"),
                Copyright = new CopyrightInfo("MBT", 2015),
                AddDashesToOption = true,
                AdditionalNewLineAfterOption = true
            };

            help.AddOptions(this);

            return help;
        }

        private List<OptionValidationMessage> _mes = new List<OptionValidationMessage>();

        public Boolean Validate() {
            _mes.Clear();

            if ( !String.IsNullOrEmpty(InputFile)) {
                if ( !System.IO.File.Exists(InputFile)) {
                    AddError("Can't find file {0}", InputFile);
                }
            }

            return IsValid;
        }

        public IEnumerable<OptionValidationMessage> Messages { get { return _mes; } }

        public void AddError(String m, params object[] a) {
            _mes.Add(new OptionValidationMessage {
                Status = OptionValidationMessageStatus.error,
                Message = String.Format(m, a)
            });
        }

        public Boolean IsValid { get { return !_mes.Any(x => x.Status == OptionValidationMessageStatus.error); } }

        public String GetConnectionString() {
            SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
            csb.DataSource = ServerName;
            if (UseTrustedConnexion) {
                csb.IntegratedSecurity = true;
            } else {
                csb.IntegratedSecurity = false;
                csb.UserID = UserName;
                csb.Password = Password;
            }
            csb.InitialCatalog = DatabaseName;
            csb.MultipleActiveResultSets = true;

            return csb.ConnectionString;
        }
    }

    public enum OptionValidationMessageStatus {
        info,
        warning,
        error
    }

    public class OptionValidationMessage {
        public OptionValidationMessageStatus Status { get; set; }
        public String Message { get; set; }

        public static OptionValidationMessage Error(String mes, params Object[] l) {
            return new OptionValidationMessage { Status = OptionValidationMessageStatus.error, Message = String.Format(mes, l) };
        }
    }
}
