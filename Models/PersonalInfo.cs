namespace OptimazedCvStorage.Models
{
    public class PersonalInfo
    {
        public int PersonalInfoID { get; set; }
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime DateOfBirth { get; set; }

        public User User { get; set; }
    }
}
