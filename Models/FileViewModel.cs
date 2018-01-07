using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace gm.Models
{
    public class FileViewModel
    {
        [Required]
        [Display(Name = "备注")]
        public string Remark { get; set; }

        [Required]
        [Display(Name = "mod文件")]
        [FileExtensions(Extensions = ".jar,.zip", ErrorMessage = "上传格式错误")]
        public IFormFile ModFile { get; set; }
    }
}
