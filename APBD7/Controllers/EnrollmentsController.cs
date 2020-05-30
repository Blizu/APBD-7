using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD5.Services;
using Microsoft.AspNetCore.Mvc;
using APBD5.DTOs.Requests;
using APBD5.DTOs.Responses;

namespace APBD5.Controllers
{
    [ApiController]
    [Route("api/enrollments")]
    public class EnrollmentsController : ControllerBase
    {
        private readonly IStudentDbService _dbService;

        public EnrollmentsController(IStudentDbService service)
        {
            _dbService = service;
        }

        [HttpPost("promotions")]
        public IActionResult PromoteStudent(PromoteStudentRequest request)
        {
            if (this._dbService.PromoteStudent(request))
            {
                return Ok();
            };

            return BadRequest();
        }

        [HttpPost]
        public IActionResult EnrollStudent(EnrollStudentRequest request)
        {

            EnrollStudent enrollStudent = this._dbService.EnrollStudent(request);

            EnrollStudentResponse enrollStudentResponse = new EnrollStudentResponse()
            {
                Semester = enrollStudent.Semester,
                LastName = enrollStudent.LastName,
                StartDate = enrollStudent.StartDate
            };

            return Ok(enrollStudentResponse);
        }
    }
}