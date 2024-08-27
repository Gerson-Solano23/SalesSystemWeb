using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SalesSystem.Entity
{
    public class S3DetailsFile
    {
        [Key]
        public int Id { get; set; }
        public DateTime? FileDate { get; set; }
        public string FileName { get; set; }
        public string FileType { get; set; }
    }
}
