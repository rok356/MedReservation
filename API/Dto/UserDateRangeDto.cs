using System;

namespace API.Dto
{
    public class UserDateRangeDto
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
    }

}
