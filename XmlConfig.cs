using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlParseInSql
{
    class XmlConfig
    {
        public string InputDirectory { get; set; }

        public string Connection { get; set; }

        public XmlConfig()
        {
            InputDirectory = ConfigurationManager.AppSettings.Get("inputdirectory");
            Connection = ConfigurationManager.ConnectionStrings["myConnectionString"].ConnectionString;
        }
    }
}
