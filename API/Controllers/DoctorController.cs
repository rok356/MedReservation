using API.Data;
using API.Dto;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        private readonly AppointmentService _appointmentService;
        private readonly UtilityService _utilityService;
        private readonly ValidationService _validationService;
        private readonly ILogger<DoctorController> _logger;


        private const string OperationSuccessfulMessage = "OperationSuccessful.";
        private const string InvalidWorkingHoursMessage = "Invalid working hours name.";
        public const string AppointmentChangedSuccessfullyMessage = "Appointment changed successfully.";
        public const string AppointmentCancelledSuccessfullyMessage = "Appointment Cancelled successfully.";
        public const string DoctorRegisteredSuccessfullyMessage = "Doctor registered successfully.";
        public const string DoctorUpdatedSuccessfullyMessage = "Doctor updated successfully.";
        public const string DoctorDeletedSuccessfullyMessage = "Doctor successfully deleted.";
        private const string NoAppointmentsFoundMessage = "Doctor's appointments not found.";

        public DoctorController(ApplicationDbContext db, AppointmentService appointmentService, UtilityService utilityService,
                                ValidationService validationService, ILogger<DoctorController> logger)
        {
            _db = db;
            _appointmentService = appointmentService;
            _utilityService = utilityService;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Registers a new doctor.
        /// </summary>
        /// <param name="model">Doctor information.</param>
        /// <returns>Success message if registration is successful.</returns>
        [HttpPost("register")]
        public IActionResult Create(DoctorDto model)
        {
            var validationResult = _validationService.ValidateDoctorRegistration(model);
            if (validationResult != null)
            {
                return validationResult;
            }

            var fetchedWorkingHours = _utilityService.GetWorkingHoursByName(model.WorkingHours);
            if (fetchedWorkingHours == null)
            {
                _logger.LogError(InvalidWorkingHoursMessage);
                return BadRequest(InvalidWorkingHoursMessage);
            }

            var doctorToAdd = new Doctor
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Specialization = model.Specialization,
                WorkingHourId = fetchedWorkingHours.Id,
            };

            _db.Doctors.Add(doctorToAdd);
            _db.SaveChanges();

            _appointmentService.GenerateAppointments(doctorToAdd, fetchedWorkingHours);

            _logger.LogTrace(DoctorRegisteredSuccessfullyMessage);
            return Ok(DoctorRegisteredSuccessfullyMessage);
        }

        /// <summary>
        /// Updates an existing doctor's information.
        /// </summary>
        /// <param name="model">Updated doctor information.</param>
        /// <returns>Success message if update is successful.</returns>
        [HttpPut("update")]
        public IActionResult Update(DoctorDto model)
        {
            var fetchedDoctor = _db.Doctors.Find(model.Id);
            var validationResult = _validationService.ValidateDoctorUpdate(model);
            if (validationResult != null)
            {
                return validationResult;
            }

            fetchedDoctor.FirstName = model.FirstName;
            fetchedDoctor.LastName = model.LastName;
            fetchedDoctor.Specialization = model.Specialization;

            _db.SaveChanges();

            _logger.LogTrace(DoctorUpdatedSuccessfullyMessage);
            return Ok(DoctorUpdatedSuccessfullyMessage);

        }

        /// <summary>
        /// Deletes a doctor by ID.
        /// </summary>
        /// <param name="id">The ID of the doctor to delete.</param>
        /// <returns>Success message if deletion is successful.</returns>
        [HttpDelete("delete/{id}")]
        public IActionResult Delete(int id)
        {
            var fetchedObj = _db.Doctors.Find(id);
            if (fetchedObj == null)
            {
                return NotFound();
            }

            _db.Doctors.Remove(fetchedObj);
            _db.SaveChanges();

            _logger.LogTrace(DoctorDeletedSuccessfullyMessage);
            return Ok(DoctorDeletedSuccessfullyMessage);


        }
        /// <summary>
        /// Retrieves the reserved appointments for a doctor by ID.
        /// </summary>
        /// <param name="id">The ID of the doctor.</param>
        /// <returns>Reserved appointments for the doctor.</returns>
        [HttpGet("reserved-appointments/{id}")]
        public IActionResult GetReservedAppointments(int id)
        {
            var doctor = _db.Doctors
                            .Include(d => d.Appointments)
                            .Include(d => d.WorkingHour)
                            .FirstOrDefault(d => d.Id == id);

            if (doctor == null)
            {
                _logger.LogError(NoAppointmentsFoundMessage);
                return NotFound(NoAppointmentsFoundMessage);
            }

            var filteredAppointments = doctor.Appointments
                                             .Where(a => a.IsBooked)
                                             .Select(a => new AppointmentLiteDto
                                             {
                                                 Id = a.Id,
                                                 DateTime = a.DateTime,
                                             }).ToList();

            var toReturn = new DoctorReservedAppointmentsDto
            {
                Id = doctor.Id,
                Appointments = filteredAppointments
            };

            _logger.LogTrace(OperationSuccessfulMessage);
            return Ok(toReturn);
        }

        /// <summary>
        /// Doctor changes an appointment from one slot to another.
        /// </summary>
        /// <param name="changeDto">Change appointment details.</param>
        /// <returns>Success message if appointment change is successful.</returns>

        [HttpPut("change-appointment")]
        public IActionResult ChangeAppointment([FromBody] ChangeAppointmentDto changeDto)
        {
            var oldAppointment = _db.Appointments.FirstOrDefault(a => a.Id == changeDto.OldAppointmentId);
            var newAppointment = _db.Appointments.FirstOrDefault(a => a.Id == changeDto.NewAppointmentId);

            var validationResult = _validationService.ValidateAppointmentChange(oldAppointment, newAppointment, changeDto);
            if (validationResult != null)
            {
                return validationResult;
            }

            newAppointment.IsBooked = true;
            newAppointment.PatientId = changeDto.PatientId;
            newAppointment.Status = "Booked";

            oldAppointment.IsBooked = false;
            oldAppointment.PatientId = null;
            oldAppointment.Status = "Changed";

            _db.SaveChanges();

            _logger.LogTrace(AppointmentChangedSuccessfullyMessage);
            return Ok(AppointmentChangedSuccessfullyMessage);
        }

        /// <summary>
        /// Doctor cancels a scheduled appointment.
        /// </summary>
        /// <param name="cancelDto">Cancellation details.</param>
        /// <returns>Success message if appointment cancellation is successful.</returns>

        [HttpPut("cancel-appointment")]
        public IActionResult CancelAppointment([FromBody] CancelAppointmentDto cancelDto)
        {
            var appointment = _db.Appointments.FirstOrDefault(a => a.Id == cancelDto.AppointmentId);

            var validationResult = _validationService.ValidateAppointmentCancellation(appointment, cancelDto);
            if (validationResult != null)
            {
                return validationResult;
            }

            appointment.IsBooked = false;
            appointment.PatientId = null;
            appointment.Status = "Canceled";
            _db.SaveChanges();

            _logger.LogTrace(AppointmentCancelledSuccessfullyMessage);
            return Ok(AppointmentCancelledSuccessfullyMessage);
        }
    }
}
