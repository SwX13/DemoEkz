using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoEkz
{
    public class User
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public Role Role { get; set; }
        public bool IsBlocked { get; set; }
        public int FailledAttemp {  get; set; }
        public string FullName { get; set; }
        
        public User() {}
    }
}
