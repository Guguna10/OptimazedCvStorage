namespace OptimazedCvStorage.Models
{
    public class Skill
    {
        public int SkillID { get; set; }
        public int UserID { get; set; }
        public string SkillName { get; set; }
        public string SkillLevel { get; set; }

        public User User { get; set; }
    }
}
