namespace DatingApp.API.Models
{
    public class UsersTemplate
    {
        public int Id { get; set; }
        //Photo
        public double FacialHair { get; set; }
        public string Glasses { get; set; }
        public bool MakeUp { get; set; }
        public string Hair { get; set; }
        //Character
        public string Personality { get; set; }
        public string Attitude { get; set; }
        public bool Assertive { get; set; }
        public bool Patriotic { get; set; }
        public bool SelfConfident { get; set; }
        public bool WithSenseOfHumour { get; set; }
        public bool HardWorking { get; set; }
        public bool Tolerant { get; set; }
        public bool Kind { get; set; }

        public int UserId { get; set; }
        public virtual User User { get; set; }
    }
}