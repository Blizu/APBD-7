using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using APBD5.Models;
using Microsoft.AspNetCore.Authorization;


namespace APBD5.DAL
{
    public class DbService : IDbService
    {

      
        public IEnumerable<Student> GetStudents()
        {
            List<Student> students = new List<Student>();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = "SELECT * FROM STUDENT";

                con.Open();

                var reader = com.ExecuteReader();

                while (reader.Read())
                {
                    Student student = new Student();

                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.BirthDate = (DateTime)reader["BirthDate"];
                    student.Password = reader["Password"].ToString();
                    student.RefreshToken = reader["RefreshToken"].ToString();
                    student.Salt = reader["Salt"].ToString();
                   

                    students.Add(student);
                }

                con.Close();
            }

            return students;
        }

        public IEnumerable<Enrollment> GetStudentEnrollment(string IdOfStudent)
        {
            List<Enrollment> enrollments = new List<Enrollment>();

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = $"SELECT * FROM dbo.ENROLLMENT WHERE IdEnrollment = (SELECT IDENROLLMENT FROM dbo.STUDENT WHERE INDEXNUMBER = @studentId)";
               
                com.Parameters.AddWithValue("studentId", IdOfStudent);

                con.Open();

                var reader = com.ExecuteReader();

                while (reader.Read())
                {
                    
                    Console.WriteLine(reader);

                    Enrollment enrollment = new Enrollment();

                    enrollment.Semester = (int)reader["Semester"];
                    enrollment.IdEnrollment = (int)reader["IdEnrollment"];
                    enrollment.IdStudy = (int)reader["IdStudy"];
                    enrollment.StartDate = System.Convert.ToDateTime(reader["StartDate"].ToString());

                    enrollments.Add(enrollment);
                }

                con.Close();
            }

            return enrollments;
        }

        public Study GetStudy(string nameOfStudy)
        {
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.CommandText = "SELECT * FROM STUDIES WHERE NAME = @studyName";
                com.Parameters.AddWithValue("studyName", nameOfStudy);

                con.Open();

                var reader = com.ExecuteReader();

                Study study = new Study();

                if (!reader.Read())
                {
                    return null;
                }

                study.IdStudy = (int)reader["IdStudy"];
                study.Name = reader["Name"].ToString();

                con.Close();

                return study;
            }
        }

        public Student GetStudent(string indexNumber)
        {
            Student student = new Student();

            try
            {
                using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
                using (var com = new SqlCommand())
                {
                    com.Connection = con;

                    com.CommandText = "SELECT * FROM STUDENT WHERE IndexNumber = @indexNumber";
                    com.Parameters.AddWithValue("indexNumber", indexNumber);

                    con.Open();

                    var reader = com.ExecuteReader();

                    if (!reader.Read())
                    {
                        return null;
                    }

                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.BirthDate = (DateTime)reader["BirthDate"];
                    student.Password = reader["Password"].ToString();
                    student.RefreshToken = reader["RefreshToken"].ToString();
                    student.Salt = reader["Salt"].ToString();
                    

                    con.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                return null;
            }

            return student;
        }


        public void PutRefreshToken(Student student, string refreshToken)
        {
            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;

                com.CommandText = "UPDATE Student SET RefreshToken=@refreshToken WHERE IndexNumber=@indexNumber";
                com.Parameters.AddWithValue("refreshToken", refreshToken);
                com.Parameters.AddWithValue("indexNumber", student.IndexNumber);

                con.Open();

                try
                {
                    com.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
                finally
                {
                    con.Close();
                }
            }
        }

        [AllowAnonymous]
        public Student CheckRefreshToken(string refreshToken)
        {
            Student student = new Student();

            try
            {
                using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
                using (var com = new SqlCommand())
                {
                    com.Connection = con;

                    com.CommandText = "SELECT * FROM STUDENT WHERE RefreshToken = @refreshToken";
                    com.Parameters.AddWithValue("refreshToken", refreshToken);

                    con.Open();

                    var reader = com.ExecuteReader();

                    if (!reader.Read())
                    {
                        throw new Exception("Invalid");
                    }

                    student.FirstName = reader["FirstName"].ToString();
                    student.LastName = reader["LastName"].ToString();
                    student.IndexNumber = reader["IndexNumber"].ToString();
                    student.BirthDate = (DateTime)reader["BirthDate"];
                    student.Password = reader["Password"].ToString();
                    student.RefreshToken = reader["RefreshToken"].ToString();

                    con.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw new Exception("CheckRefreshTokenError");
              
            }

            return student;
        }

        public async void CreateNewStudent(Student student)
        {
            try
            {
                using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
                using (var com = new SqlCommand())
                {
                    com.Connection = con;

                    com.CommandText = "INSERT INTO STUDENT (IndexNumber, FirstName, LastName, BirthDate, Password, Salt) VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @Password, @Salt)";
                    com.Parameters.AddWithValue("IndexNumber", student.IndexNumber);
                    com.Parameters.AddWithValue("FirstName", student.FirstName);
                    com.Parameters.AddWithValue("LastName", student.LastName);
                    com.Parameters.AddWithValue("BirthDate", student.BirthDate);
                    com.Parameters.AddWithValue("Password", student.Password);
                    com.Parameters.AddWithValue("Salt", student.Salt);

                    con.Open();

                    await com.ExecuteNonQueryAsync();

                    con.Close();
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                throw new Exception("CreateNewStudentError");
            }
        }


    }
}

