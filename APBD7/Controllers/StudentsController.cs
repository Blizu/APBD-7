using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using APBD5.DAL;
using APBD5.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using APBD6.DTOs.Requests;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Konscious.Security.Cryptography;

namespace APBD5.Controllers
{
    [ApiController]
    [Route("api/students")]
    public class StudentsController : ControllerBase
    {

        private readonly IDbService _dbService;

        public IConfiguration Configuration { get; set; }


        public StudentsController(IDbService service)
        {
            _dbService = service;
        }

        [HttpGet]
        [Authorize]
        public IActionResult GetStudents()
        {
            IEnumerable<Student> students = _dbService.GetStudents();

            return Ok(students);
        }

        [HttpGet("enrollment/{studentId}")]
        public IActionResult GetStudentEnrollment(string studentId)
        {
            IEnumerable<Enrollment> enrollments = _dbService.GetStudentEnrollment(studentId);

            return Ok(enrollments);
        }

 
        [HttpPost]
        [AllowAnonymous]
        public IActionResult CreateNewStudent(StudentRequest studentRequest)
        {
            var password = studentRequest.Password;
            var salt = StudentsController.CreateSalt();
            var hashedPassword = CreateHashedPassword(password, salt);

            Student student = new Student()
            {
                FirstName = studentRequest.FirstName,
                LastName = studentRequest.LastName,
                IndexNumber = studentRequest.IndexNumber,
                Password = hashedPassword,
                Salt = salt,
                BirthDate = studentRequest.BirthDate,
            };

            this._dbService.CreateNewStudent(student);

            return Ok(studentRequest);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateStudent(int id)
        {
            return Ok($"Updating finished. Updated id: {id}.");
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteStudent(int id)
        {
            return Ok($"Deleting finished. Deleted id: {id}.");
        }

        [HttpPost("login")]
        public IActionResult Login(LoginRequest request)
        {
            var login = request.Login;
            var password = request.Password;

            var student = this._dbService.GetStudent(login);

            if (student == null)
            {
                return Unauthorized();
            }

            var hashedPassword = CreateHashedPassword(request.Password, student.Salt);

            if (!hashedPassword.Equals(student.Password))
            {
                return Unauthorized();
            }

            var TabOfClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "admin"),
               
            };

            var symetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsecretsecretsecretsecretsecretsecretsecret"));
            var signingCredentials = new SigningCredentials(symetricSecurityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken
            (
                issuer: "s16710",
                claims: TabOfClaims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signingCredentials
            );

            var refreshToken = Guid.NewGuid();

            this._dbService.PutRefreshToken(student, refreshToken.ToString());

            return Ok(new
            {
                accesToken = new JwtSecurityTokenHandler().WriteToken(token),
                refreshToken
            });
        }

        [HttpPost("refresh-token/{refreshToken}")]
        public IActionResult RefreshToken(string refreshToken)
        {
            Student student;
            try
            {
                student = this._dbService.CheckRefreshToken(refreshToken);
            }
            catch (Exception e)
            {
                return Unauthorized();
            }

            var TabOfClaims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "admin"),
                new Claim(ClaimTypes.Role, "admin")
            };

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("secretsecretsecretsecretsecretsecretsecretsecret"));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            JwtSecurityToken token = new JwtSecurityToken
            (
                issuer: "s16710",
                claims: TabOfClaims,
                expires: DateTime.Now.AddMinutes(60),
                signingCredentials: signingCredentials
            );

            var newRefreshToken = Guid.NewGuid();

            this._dbService.PutRefreshToken(student, refreshToken.ToString());

            return Ok(new
            {
                accesToken = new JwtSecurityTokenHandler().WriteToken(token),
                newRefreshToken
            });
        }

        private static string CreateSalt()
        {
            byte[] TabOfRandomBytes = new byte[128 / 8];

            using (var randomNumberGenerator = RandomNumberGenerator.Create())
            {
                randomNumberGenerator.GetBytes(TabOfRandomBytes);
            };

            return Convert.ToBase64String(TabOfRandomBytes);
        }

        private static string CreateHashedPassword(string password, string salt)
        {
            byte[] passwordBytesTable = Encoding.ASCII.GetBytes(password);
            byte[] saltBytesTable = Encoding.ASCII.GetBytes(salt);

            Argon2 hashedPassword = new Argon2d(passwordBytesTable)
            {
                Salt = saltBytesTable,
                Iterations = 6,
                MemorySize = 258,
                DegreeOfParallelism = 8
            };

            byte[] bytesHashedPassword = hashedPassword.GetBytes(128);

            return Encoding.UTF8.GetString(bytesHashedPassword);
        }

    }
}