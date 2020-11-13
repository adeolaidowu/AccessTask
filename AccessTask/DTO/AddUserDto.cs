using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace AccessBankTask.DTOs
{
    public class AddUserDto
    {
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(20)]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(20)]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Password can not be more than 10")]
        [MaxLength(10)]
        [MinLength(4)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
