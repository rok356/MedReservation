namespace API.Dto
{
    public class ChangeAppointmentDto
    {
        public int OldAppointmentId { get; set; }
        public int NewAppointmentId { get; set; }
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
    }
}
