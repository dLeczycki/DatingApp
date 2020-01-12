using System;
using DatingApp.API.DTOs;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddApplicationError(this HttpResponse response, string message)
        {
            response.Headers.Add("Application-Error", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Application-Error");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }

        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages)
        {
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
            var camelCaseFormatter = new JsonSerializerSettings();
            camelCaseFormatter.ContractResolver = new CamelCasePropertyNamesContractResolver();
            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormatter));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }

        public static int CalculateAge(this DateTime theDateTime)
        {
            var age = DateTime.Today.Year - theDateTime.Year;
            if (theDateTime.AddYears(age) > DateTime.Today)
                age--;

            return age;
        }

        public static double CalculateFacialHair(this FacialHair facialHair)
        {
            double hair = (facialHair.Beard + facialHair.Moustache + facialHair.Sideburns) / 3;
            if (hair == 0) return 0;
            else if (hair < 0.5) return 0.5;
            else return 1;
        }

        public static string SetHair(this Hair hair)
        {
            string recognizedHair = "bald";
            double confidence = hair.Bald;
            foreach (var type in hair.HairColor)
            {
                if (type.Confidence > confidence)
                {
                    recognizedHair = type.Color;
                    confidence = type.Confidence;
                }
            }

            return recognizedHair;
        }

        public static string SetGlasses(this FaceAttributes attributes)
        {
            if (attributes.Glasses != "NoGlasses") return "HasGlasses";
            else return "NoGlasses";
        }

        public static bool SetMakeUp(this MakeUp makeUp)
        {
            if (makeUp.EyeMakeup || makeUp.LipMakeup) return true;
            else return false;
        }
    }
}