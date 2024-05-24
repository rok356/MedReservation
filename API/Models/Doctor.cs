using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        [ForeignKey("WorkingHour")]
        [Required]
        public int WorkingHourId { get; set; }
        public WorkingHour WorkingHour { get; set; }

        [InverseProperty("Doctor")]
        public virtual ICollection<Appointment> Appointments { get; set; }

    }
}
