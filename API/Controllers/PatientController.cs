using API.Data;
using API.Dto;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : Controller
    {

        private readonly ApplicationDbContext _db;
        private readonly ValidationService _validationService;
        private readonly ILogger<DoctorController> _logger;

        private const string OperationSuccessfulMessage = "Operation Successful.";
        private const string DoctorNotFoundMessage = "Doctor not found";
        private const string AppointmentReservedSuccessMessage = "Appointment reserved successfully.";
        private const string AppointmentCanceledSuccessMessage = "Appointment canceled successfully.";


        public PatientController(ApplicationDbContext db, ValidationService validationService, ILogger<DoctorController> logger)
        {
            _db = db;
            _validationService = validationService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all free appointments for a specific doctor.
        /// </summary>
        /// <param name="doctorId">The ID of the doctor.</param>
        /// <returns>A list of all free appointments for the specified doctor.</returns>
        [HttpGet("doctor-free-appointments/{doctorId}")]
        public IActionResult GetAllFreeAppointments(int doctorId)
        {
            var currentTime = DateTime.Now;

            var appointments = _db.Appointments
                                  .Where(a => !a.IsBooked && a.DoctorId == doctorId && a.DateTime > currentTime)
                                  .Select(a => new AppointmentDto
                                  {
                                      Id = a.Id,
                                      Doctor = $"{a.Doctor.FirstName} {a.Doctor.LastName}",
                                      DateTime = a.DateTime,
                                      IsBooked = a.IsBooked
                                  })
                                  .ToList();

            _logger.LogError(OperationSuccessfulMessage);
            return Ok(appointments);
        }

        /// <summary>
        /// Reserves an appointment.
        /// </summary>
        /// <param name="requestData">The appointment details.</param>
        /// <returns>Success message if reservation is successful.</returns>
        [HttpPut("reserve-appointment")]
        public IActionResult ReserveAppointment([FromBody] PatientAppointmentDto requestData)
        {
            var appointment = _db.Appointments.FirstOrDefault(a => a.Id == requestData.AppointmentId);

            var validationResult = _validationService.ValidateAppointmentReservation(requestData);
            if (validationResult != null)
            {
                return validationResult;
            }

            appointment.IsBooked = true;
            appointment.PatientId = requestData.PatientId;
            appointment.Status = "Booked";
            _db.SaveChanges();

            _logger.LogError(AppointmentReservedSuccessMessage);
            return Ok(AppointmentReservedSuccessMessage);

    }


        /// <summary>
        /// Cancels an appointment for a patient.
        /// </summary>
        /// <param name="requestData">The appointment details.</param>
        /// <returns>Success message if cancellation is successful.</returns>
        [HttpPut("cancel-appointment")]
        public IActionResult CancelAppointment([FromBody] PatientAppointmentDto requestData)
        {
            var appointment = _db.Appointments.FirstOrDefault(a => a.Id == requestData.AppointmentId);

            var validationResult = _validationService.ValidateAppointmentCancellation(requestData);
            if (validationResult != null)
            {
                return validationResult;
            }

            appointment.IsBooked = false;
            appointment.PatientId = null;
            appointment.Status = "Canceled";
            _db.SaveChanges();

            _logger.LogError(AppointmentCanceledSuccessMessage);
            return Ok(AppointmentCanceledSuccessMessage);
    }


        /// <summary>
        /// Retrieves all doctors with working hours.
        /// </summary>
        /// <returns>A list of all doctors.</returns>
        [HttpGet("all-doctors")]
        public IActionResult GetAllDoctors()
        {
            var doctors = _db.Doctors.Select(x => new DoctorDto

            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Specialization = x.Specialization,
                WorkingHours = x.WorkingHour.Schedule
            }).ToList();

            _logger.LogError(OperationSuccessfulMessage);
            return Ok(doctors);
        }

        /// <summary>
        /// Retrieves appointments for a doctor within a specified date range.
        /// </summary>
        /// <param name="parameters">The date range and doctor ID.</param>
        /// <returns>Appointments for the specified doctor within the date range.</returns>
        [HttpGet("doctor-reserved-appointments")]
        public IActionResult GetOne([FromBody] UserDateRangeDto parameters)
        {
            var doctor = _db.Doctors
                            .Include(d => d.Appointments)
                            .Include(d => d.WorkingHour)
                            .FirstOrDefault(d => d.Id == parameters.DoctorId);

            if (doctor == null)
            {
                _logger.LogError(DoctorNotFoundMessage);
                return NotFound(DoctorNotFoundMessage);

    }

            var filteredAppointments = doctor.Appointments
                                             .Where(a => a.DateTime >= parameters.StartDate && a.DateTime <= parameters.EndDate && a.IsBooked == true)
                                             .Select(a => new AppointmentDto
                                             {
                                                 Id = a.PatientId == parameters.PatientId ? a.Id : (int?)null, // Set Id only if PatientId matches
                                                 Doctor = $"{doctor.FirstName} {doctor.LastName}",
                                                 DateTime = a.DateTime,
                                                 IsBooked = a.IsBooked
                                             }).ToList();

            var toReturn = new DoctorAppointmentsDto
            {
                Id = doctor.Id,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Specialization = doctor.Specialization,
                WorkingHours = doctor.WorkingHour.Schedule,
                Appointments = filteredAppointments
            };

            _logger.LogError(OperationSuccessfulMessage);
            return Ok(toReturn);
        }


        /// <summary>
        /// Registers a new patient.
        /// </summary>
        /// <param name="model">The patient details.</param>
        /// <returns>Success message if registration is successful.</returns>
        [HttpPost("register")]
        public IActionResult Create(PatientDto model)
        {
            var validationResult = _validationService.ValidatePatientRegistration(model);
            if (validationResult != null)
            {
                return validationResult;
            }

            var patientToAdd = new Patient
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
            };

            _db.Patients.Add(patientToAdd);
            _db.SaveChanges();

            _logger.LogTrace(OperationSuccessfulMessage);
            return Ok(OperationSuccessfulMessage);
        }
    }
}
