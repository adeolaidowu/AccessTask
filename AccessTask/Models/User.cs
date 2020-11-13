using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccessBankTask.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(20)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(20)]
        public string FirstName { get; set; }

        public LogActivity LogActivity { get; set; }
    }
}
