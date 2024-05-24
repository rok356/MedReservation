using System;

namespace API.Dto
{
    public class AppointmentDto
    {
        public int? Id { get; set; }
        public string Doctor { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsBooked { get; set; }

    }
}
