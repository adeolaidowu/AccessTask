using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccessBankTask.Models
{
    public class LogActivity
    {
        [Key]
        public int Id { get; set; }
        public bool IsActive { get; set; } = false;

        public string DeviceIp { get; set; }

        public DateTime LoginTime { get; set; }
        public DateTime LogoutTime { get; set; }

        public string UserId { get; set; }
        public ICollection<User> Users { get; set; }
    }
}
