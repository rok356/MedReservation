using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    public class WorkingHour
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Schedule { get; set; }
        public ICollection<Doctor> Doctors { get; set; }

    }
}
