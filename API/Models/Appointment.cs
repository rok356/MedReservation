using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsBooked { get; set; }
        [ForeignKey("Patient")]
        public int? PatientId { get; set; }
        public string Status { get; set; }
        public virtual Patient Patient { get; set; }

        public virtual Doctor Doctor { get; set; }

    }
}
