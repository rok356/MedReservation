using System.Collections.Generic;

namespace API.Dto
{
    public class DoctorReservedAppointmentsDto
    {
        public int Id { get; set; }
        public List<AppointmentLiteDto> Appointments { get; set; } = new List<AppointmentLiteDto>();
    }
}
