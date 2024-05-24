using API.Data;
using API.Models;
using System.Collections.Generic;
using System;

namespace API.Services
{
    public class AppointmentService
    {
        private readonly ApplicationDbContext _db;

        public AppointmentService(ApplicationDbContext db)
        {
            _db = db;
        }

        public void GenerateAppointments(Doctor doctor, WorkingHour workingHour)
        {
            DateTime startDate = DateTime.Today.Date;
            DateTime endDate = startDate.AddDays(30);

            var timeSlots = GenerateTimeSlots(workingHour.Schedule, startDate, endDate);

            foreach (var kvp in timeSlots)
            {
                var day = kvp.Key;
                var slots = kvp.Value;

                foreach (var slot in slots)
                {
                    var currentDate = slot.Item1;

                    while (currentDate <= slot.Item2)
                    {
                        if (currentDate.TimeOfDay >= slot.Item1.TimeOfDay && currentDate.TimeOfDay < slot.Item2.TimeOfDay)
                        {
                            var appointmentToAdd = new Appointment
                            {
                                DoctorId = doctor.Id,
                                DateTime = currentDate,
                                IsBooked = false
                            };

                            _db.Appointments.Add(appointmentToAdd);
                        }

                        currentDate = currentDate.AddMinutes(30);
                    }
                }
            }

            _db.SaveChanges();
        }

        public static Dictionary<string, List<Tuple<DateTime, DateTime>>> GenerateTimeSlots(string workingHoursJson, DateTime from, DateTime to)
        {
            var timeSlots = new Dictionary<string, List<Tuple<DateTime, DateTime>>>();

            var json = Newtonsoft.Json.Linq.JObject.Parse(workingHoursJson);

            var currentDate = from.Date;

            while (currentDate <= to.Date)
            {
                var currentDayOfWeek = currentDate.DayOfWeek.ToString();

                if (json.ContainsKey(currentDayOfWeek))
                {
                    var hours = json[currentDayOfWeek].ToString().Split(" - ");
                    var startTime = TimeSpan.Parse(hours[0]);
                    var endTime = TimeSpan.Parse(hours[1]);

                    var startDate = currentDate.Add(startTime);
                    var endDate = currentDate.Add(endTime);

                    if (!timeSlots.ContainsKey(currentDayOfWeek))
                    {
                        timeSlots[currentDayOfWeek] = new List<Tuple<DateTime, DateTime>>();
                    }

                    timeSlots[currentDayOfWeek].Add(Tuple.Create(startDate, endDate));
                }

                currentDate = currentDate.AddDays(1);
            }

            return timeSlots;
        }
    }
}
