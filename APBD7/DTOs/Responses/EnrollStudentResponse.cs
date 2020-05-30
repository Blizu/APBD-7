using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace APBD5.DTOs.Responses
{
    public class EnrollStudentResponse
    {
        public string LastName { get; set; }
     
        public DateTime StartDate { get; set; }

        public int Semester { get; set; }

    }  
}
