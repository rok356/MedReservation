using API.Data;
using API.Dto;
using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace API.Services
{
    public class ValidationService
    {
        private readonly ApplicationDbContext _db;
        private readonly UtilityService _utilityService;

        private const string AppointmentNotFoundMessage = "Appointment not found.";
        private const string UnauthorizedToChangeMessage = "You are not authorized to change these appointments.";
        private const string NewAppointmentAlreadyBookedMessage = "New appointment is already booked.";
        private const string DoctorNotFoundMessage = "Doctor not found.";
        private const string DoctorAlreadyExistsMessage = "Doctor is already in the system.";
        private const string PatientAlreadyExistsMessage = "Patient is already in the system.";
        private const string AppointmentNotBookedMessage = "Appointment is not booked.";
        private const string UnauthorizedToCancelMessage = "You are not authorized to cancel this appointment.";
        private const string AppointmentAlreadyBookedMessage = "Appointment is already booked.";

        public ValidationService(ApplicationDbContext db, UtilityService utilityService)
        {
            _db = db;
            _utilityService = utilityService;

        }

        public IActionResult ValidateAppointmentChange(Appointment oldAppointment, Appointment newAppointment, ChangeAppointmentDto changeDto)
        {
            if (oldAppointment == null || newAppointment == null)
            {

                return new NotFoundObjectResult(AppointmentNotFoundMessage);
            }

            if (oldAppointment.DoctorId != changeDto.DoctorId || newAppointment.DoctorId != changeDto.DoctorId)
            {
                return new UnauthorizedObjectResult(UnauthorizedToChangeMessage);
            }

            if (newAppointment.IsBooked)
            {
                return new BadRequestObjectResult(NewAppointmentAlreadyBookedMessage);
            }

            return null;
        }

        public IActionResult ValidateAppointmentCancellation(Appointment appointment, CancelAppointmentDto cancelDto)
        {
            if (appointment == null)
            {
                return new NotFoundObjectResult(AppointmentNotFoundMessage);
            }

            if (appointment.DoctorId != cancelDto.DoctorId)
            {
                return new UnauthorizedObjectResult(UnauthorizedToCancelMessage);
            }

            if (!appointment.IsBooked)
            {
                return new BadRequestObjectResult(AppointmentNotBookedMessage);
            }

            return null;
        }

        public IActionResult ValidateDoctorUpdate(DoctorDto model)
        {
            var fetchedDoctor = _db.Doctors.Find(model.Id);
            if (fetchedDoctor == null)
            {
                return new NotFoundObjectResult(DoctorNotFoundMessage);
            }

            if (_utilityService.DoctorExists(model))
            {
                return new BadRequestObjectResult(DoctorAlreadyExistsMessage);
            }

            return null;
        }

        public IActionResult ValidateDoctorRegistration(DoctorDto model)
        {
            if (_utilityService.DoctorExists(model))
            {
                return new BadRequestObjectResult(DoctorAlreadyExistsMessage);
            }

            return null;
        }

        public IActionResult ValidatePatientRegistration(PatientDto model)
        {
            if (_db.Patients.Any(x => x.FirstName.ToLower() == model.FirstName.ToLower() && x.LastName.ToLower() == model.LastName.ToLower()))
            {
                return new BadRequestObjectResult(PatientAlreadyExistsMessage);
            }

            return null;
        }

        public IActionResult ValidateAppointmentCancellation(PatientAppointmentDto requestData)
        {
            var appointment = _db.Appointments.FirstOrDefault(a => a.Id == requestData.AppointmentId);

            if (appointment == null)
            {
                return new NotFoundObjectResult(AppointmentNotFoundMessage);
            }

            if (!appointment.IsBooked)
            {
                return new BadRequestObjectResult(AppointmentNotBookedMessage);
            }

            if (appointment.PatientId != requestData.PatientId)
            {
                return new UnauthorizedObjectResult(UnauthorizedToCancelMessage);
            }

            return null;
        }

        public IActionResult ValidateAppointmentReservation(PatientAppointmentDto requestData)
        {
            var appointment = _db.Appointments.FirstOrDefault(a => a.Id == requestData.AppointmentId);

            if (appointment == null)
            {
                return new NotFoundObjectResult(AppointmentNotFoundMessage);
            }

            if (appointment.IsBooked)
            {
                return new BadRequestObjectResult(AppointmentAlreadyBookedMessage);
            }

            return null;
        }
    }
}
