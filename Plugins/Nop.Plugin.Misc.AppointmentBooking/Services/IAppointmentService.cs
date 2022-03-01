using Nop.Plugin.Misc.AppointmentBooking.Domains;
using Nop.Plugin.Misc.AppointmentBooking.Models;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.AppointmentBooking.Services
{
    public interface IAppointmentService
    {
        ServicesMap GetServicesByBranchId(string branchid, string language);

        BranchList GetBranchAroundMe(string currentLatitude, string currentLongitude, string language);

        BranchDetail GetBranchDetail(string language);

        AvailableDaysToTakeAppointment GetAvailableDaysToTakeAppointment(string branchid, string serviceId, string language);

        AvailableTimesToTakeAppointment GetDayAvailableTimesToTakeAppointment(string appointmentDay, string branchId, string serviceId);

        BookAppointmentResponse BookAppointment(BookAppointmentRequest bookAppointmentRequest, string language);

        #region Store Booked Appointment
        void InsertBookedAppointment(BookedAppointment bookedAppointment);

        BookedAppointment GetBookedAppointmentByOrderId(int orderId);

        BookedAppointment UpdateOrderIdByBookedAppointmentId(int bookedAppointmentId, int orderId);
        #endregion

        #region AppointmentBranch
        void DeleteAppointmentBranch(AppointmentBranch appointmentBranch);

        void InsertAppointmentBranch(AppointmentBranch appointmentBranch);

        void UpdateAppointmentBranch(AppointmentBranch appointmentBranch);

        AppointmentBranch GetAppointmentBranchById(int appointmentBranchId);

        IList<AppointmentBranch> GetAppointmentBranchByName(string appointmentBranchName);

        IList<AppointmentBranch> GetAppointmentBranchList();
        #endregion

        IList<int> SendAppointmentBookedCustomerNotification(BookedAppointment bookedAppointment, int languageId);
    }
}
