using System;
using System.Collections.Generic;

namespace DatingApp.API.DTOs
{
    public class FaceDto
    {
        public FaceAttributes FaceAttributes { get; set; }
    }

    public class FaceAttributes
    {
        public FacialHair FacialHair { get; set; }
        public string Glasses { get; set; }
        public MakeUp MakeUp { get; set; }
        public Hair Hair { get; set; }
    }

    public class FacialHair
    {
        public double Moustache { get; set; }
        public double Beard { get; set; }
        public double Sideburns { get; set; }
    }

    public class MakeUp
    {
        public bool EyeMakeup { get; set; }
        public bool LipMakeup { get; set; }
    }

    public class Hair
    {
        public double Bald { get; set; }
        public bool Invisible { get; set; }
        public List<HairType> HairColor { get; set; }
    }

    public class HairType
    {
        public string Color { get; set; }
        public double Confidence { get; set; }
    }
}