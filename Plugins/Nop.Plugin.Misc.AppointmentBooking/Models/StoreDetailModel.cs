namespace Nop.Plugin.Misc.AppointmentBooking.Models
{
    public class StoreDetailModel
    {
        public StoreDetailModel()
        {
            Branch = new Branch();
            AvailableDaysToTakeAppointment = new AvailableDaysToTakeAppointment();
            AvailableTimesToTakeAppointment = new AvailableTimesToTakeAppointment();
        }

        public int OrderId { get; set; }
        public string ServiceId { get; set; }
        public string OpenUntil { get; set; }
        public string SelectAppointmentDate { get; set; }
        public Branch Branch { get; set; }
        public AvailableDaysToTakeAppointment AvailableDaysToTakeAppointment { get; set; }
        public AvailableTimesToTakeAppointment AvailableTimesToTakeAppointment { get; set; }
        
        public string Description { get; set; }
        
        public long Code { get; set; }
    }
}
