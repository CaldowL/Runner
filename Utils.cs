using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Runner
{
    public class Utils
    {
        public dynamic getConfig(string path,string key)
        {
            return "";
        }

        public string getUUID()
        {
            Guid guid = Guid.NewGuid();
            return guid.ToString().ToUpper().Replace("-", "");
        }
    }
}
