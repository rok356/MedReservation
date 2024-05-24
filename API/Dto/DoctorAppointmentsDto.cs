using System.Collections.Generic;

namespace API.Dto
{
    public class DoctorAppointmentsDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Specialization { get; set; }
        public string WorkingHours { get; set; }
        public List<AppointmentDto> Appointments { get; set; } = new List<AppointmentDto>();
    }

}
