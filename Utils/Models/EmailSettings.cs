using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Models
{
    public class EmailSettings
    {
        public string Host { get; set; } = null!;
        public int Port { get; set; }
        public string UserName { get; set; }=null!;
        public string Password { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool Enabled { get; set; } = true;
    }
}