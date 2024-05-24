using API.Data;
using API.Dto;
using API.Models;
using System.Linq;

namespace API.Services
{
    public class UtilityService
    {
        private readonly ApplicationDbContext _db;

        public UtilityService(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool DoctorExists(DoctorDto model)
        {
            return _db.Doctors.Any(x => x.FirstName.ToLower() == model.FirstName.ToLower() && x.LastName.ToLower() == model.LastName.ToLower());
        }

        public WorkingHour GetWorkingHoursByName(string name)
        {
            return _db.WorkingHours.SingleOrDefault(x => x.Name == name);
        }

        public bool PatientExists(PatientDto model)
        {
            var fetchedPatient = _db.Patients.FirstOrDefault(x => x.FirstName == model.FirstName && x.LastName == model.LastName && x.Email == model.Email);

            return fetchedPatient != null;
        }

    }
}
