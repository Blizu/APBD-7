using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using APBD5.Models;

namespace APBD5.DAL
{
    public interface IDbService
    {
        public IEnumerable<Student> GetStudents();

        public IEnumerable<Enrollment> GetStudentEnrollment(string studentId);

        public Student CheckRefreshToken(string refreshToken);

        public void CreateNewStudent(Student student);
        public void PutRefreshToken(Student student, string refreshToken);

        public Study GetStudy(string studyName);

        public Student GetStudent(string indexNumber);
    }
}

