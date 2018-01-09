using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace gm.Models
{
    public class LoginViewModel
    {
        [Required]
        [Display(Name = "用户名")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "用户名为4-20位")]
        public string Uname { get; set; }

        [Required]
        [Display(Name = "密码")]
        [DataType(DataType.Password)]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "密码最为6-20位")]
        public string Pword { get; set; }

        [Display(Name = "下次自动登录")]
        public bool Remember { get; set; }
    }
}

