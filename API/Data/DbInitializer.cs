using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace API.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext db, AppointmentService appointmentService)
        {
            if (db.Database.GetPendingMigrations().Count() > 0)
            {
                await db.Database.MigrateAsync();
            }


            if (!db.WorkingHours.Any())
            {
                var workingHours = new WorkingHour
                {
                    Name = "Standard",
                    Schedule = @"{
  ""Monday"": ""08:00 - 16:00"",
  ""Tuesday"": ""08:00 - 16:00"",
  ""Wednesday"": ""08:00 - 16:00"",
  ""Thursday"": ""08:00 - 16:00"",
  ""Friday"": ""08:00 - 16:00""
}"
                };
                db.WorkingHours.Add(workingHours);

                var patient = new Patient
                {
                    FirstName = "Mike",
                    LastName = "Doe",
                    Email = "email@email.com",
                    PhoneNumber = "+386 44 555 666",
                };
                db.Patients.Add(patient);

                var doctor = new Doctor
                {
                    FirstName = "Mike",
                    LastName = "Doe",
                    Specialization = "Cardiology",
                    WorkingHour = workingHours,
                };
                db.Doctors.Add(doctor);

                if (db.ChangeTracker.HasChanges())
                {
                    await db.SaveChangesAsync();
                }

                appointmentService.GenerateAppointments(doctor, workingHours);
            }

            if (db.ChangeTracker.HasChanges())
            {
                await db.SaveChangesAsync();
            }

        }
    }
}
