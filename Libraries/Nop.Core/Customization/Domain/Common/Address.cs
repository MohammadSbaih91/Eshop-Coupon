namespace Nop.Core.Domain.Common
{
    /// <summary>
    /// Address
    /// </summary>
    public partial class Address
    {
        public Civility Civility
        {
            get => (Civility) CivilityId;
            set => CivilityId = (int) value;
        }
        
        public Nationality Nationality
        {
            get => (Nationality) NationalityId;
            set => NationalityId = (int) value;
        }

        public NationalityType NationalityType
        {
            get => (NationalityType) NationalityTypeId;
            set => NationalityTypeId = (int) value;
        }

        public int CivilityId { get; set; }
        public int NationalityId { get; set; }
        public int NationalityTypeId { get; set; }
        public string IdentityCardOrPassport { get; set; }
        public string StudentID { get; set; }
        public string UploadStudentID { get; set; }
        public string UploadID { get; set; }
        public string BuildingNo { get; set; }

        /// <summary>
        /// ICloneable.Clone extended 
        /// </summary>
        /// <returns></returns>
        public object ClonePartial(Address addr)
        {
            addr.Nationality = Nationality;
            addr.NationalityType = NationalityType;
            addr.Civility = Civility;
            addr.IdentityCardOrPassport = IdentityCardOrPassport;
            addr.NationalityId = NationalityId;
            addr.NationalityTypeId = NationalityTypeId;
            addr.CivilityId = CivilityId;
            addr.UploadStudentID = UploadStudentID;
            addr.UploadID = UploadID;
            addr.StudentID = StudentID;
            addr.BuildingNo = BuildingNo;
            return addr;
        }
    }
}