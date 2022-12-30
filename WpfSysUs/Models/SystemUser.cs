using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace WpfSysUs.Models
{
    class SystemUser
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Organization { get; set; }
        //public string IP { get; set; }
        public IPAddress IP { get; set; }
        public long longIP { get; set; }
        public string SessionID { get; set; }
        public DateTime DateTimeLog { get; set; }
        public DateTime DateTimeLogOut { get; set; }
        public string TerminationCode { get; set; }

        
    }
}
