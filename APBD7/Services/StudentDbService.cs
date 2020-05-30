using APBD5.DTOs.Requests;
using APBD5.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace APBD5.Services
{
    public class StudentDbService : IStudentDbService
    {
        public bool PromoteStudent(PromoteStudentRequest promoteStudentRequest)
        {
            using (var con =
                new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.Connection.Open();
                com.Transaction = com.Connection.BeginTransaction();

                try
                {
                    com.CommandText =
                        "SELECT * FROM ENROLLMENT WHERE IdStudy = (SELECT IdStudy FROM STUDIES WHERE NAME = @StudyName) AND Semester = @Semester";
                    com.Parameters.AddWithValue("StudyName", promoteStudentRequest.Studies);
                    com.Parameters.AddWithValue("Semester", promoteStudentRequest.Semester);

                    var reader = com.ExecuteReader();

                    if (!reader.Read())
                    {
                        reader.Close();
                        return false;
                    }
                    reader.Close();
                    com.Parameters.Clear();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                    com.Transaction.Rollback();
                    return false;
                }

                try
                {
                    
                    com.CommandType = CommandType.StoredProcedure;
                    com.CommandText = "PromoteStudents";
                    com.Parameters.AddWithValue("@OldSemester", promoteStudentRequest.Semester);
                    com.Parameters.AddWithValue("@StudiesName", promoteStudentRequest.Studies);
                    com.ExecuteNonQuery();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error);
                    com.Transaction.Rollback();
                    return false;
                }

                com.Transaction.Commit();
            }

            return true;
        }

        public Study GetStudy(string nameOfStudy)
        {
            using (var connection = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var command = new SqlCommand())
            {
                command.Connection = connection;

                command.CommandText = "SELECT * FROM STUDIES WHERE NAME = @studyName";
                command.Parameters.AddWithValue("studyName", nameOfStudy);

                connection.Open();

                var reader = command.ExecuteReader();

                Study study = new Study();

                if (!reader.Read())
                {
                    throw new Exception("No studies");
                }

                study.IdStudy = (int)reader["IdStudy"];
                study.Name = reader["Name"].ToString();

                connection.Close();

                return study;
            }
        }

        public EnrollStudent EnrollStudent(EnrollStudentRequest enrollStudentRequest)
        {
            Study study;
            Student student = new Student
            {
                FirstName = enrollStudentRequest.FirstName,
                LastName = enrollStudentRequest.LastName,
                IndexNumber = enrollStudentRequest.IndexNumber,
                BirthDate = enrollStudentRequest.BirthDate,
            };

            try
            {
                study = this.GetStudy(enrollStudentRequest.Studies);
            }
            catch (Exception exception)
            {
                throw exception;
            }

           
            DateTime enrollmentDate = DateTime.Now;

            using (var con = new SqlConnection("Data Source=db-mssql;Initial Catalog=s16710;Integrated Security=True"))
            using (var com = new SqlCommand())
            {
                com.Connection = con;
                com.Connection.Open();
                var transaction = com.Connection.BeginTransaction();
                com.Transaction = transaction;

                try
                {
                    com.CommandText = "SELECT IdEnrollment FROM ENROLLMENT WHERE IdStudy = @IdStudy AND Semester = 1";
                    com.Parameters.AddWithValue("IdStudy", study.IdStudy);

                    var reader = com.ExecuteReader();

                    if (!reader.Read())
                    {
                        com.CommandText =
                            "INSERT INTO ENROLLMENT (Semester, IdStudy, StartDate) VALUES (1, @IdStudy, @StartDate)";
                        com.Parameters.AddWithValue("StartDate", enrollmentDate);
                        com.Parameters.AddWithValue("IdStudy", study.IdStudy);
                        com.ExecuteNonQuery();
                    }
                    reader.Close();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    Console.WriteLine(error);
                    throw new Exception("Błąd podczas zapisu");
                }

                try
                {
                    com.CommandText = "SELECT * FROM STUDENT WHERE IndexNumber = @studentIndexNumber";
                    com.Parameters.AddWithValue("studentIndexNumber", student.IndexNumber);

                    var reader = com.ExecuteReader();

                    if (reader.Read())
                    {
                        reader.Close();
                        transaction.Rollback();
                        throw new Exception("Student istnieje!");
                    }

                    reader.Close();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    Console.WriteLine(error);
                    throw new Exception("Nie można uzyskać informacji o studencie");
                }

                try
                {
                    com.CommandText =
                        "SELECT IdEnrollment FROM ENROLLMENT WHERE IdStudy = @IdStudy AND Semester = 1";

                    var reader = com.ExecuteReader();

                    if (!reader.Read())
                    {
                        reader.Close();
                        transaction.Rollback();
                        throw new Exception("Brak wpisu");
                    }

                    var enrollmentId = (int)reader["IdEnrollment"];
                    reader.Close();

                    com.CommandText = "INSERT INTO STUDENT (IndexNumber, FirstName, LastName, BirthDate, IdEnrollment) VALUES (@IndexNumber, @FirstName, @LastName, @BirthDate, @IdEnrollment)";
                    com.Parameters.AddWithValue("IndexNumber", student.IndexNumber); 
                    com.Parameters.AddWithValue("FirstName", student.FirstName);
                    com.Parameters.AddWithValue("LastName", student.LastName);
                    com.Parameters.AddWithValue("BirthDate", student.BirthDate);
                    com.Parameters.AddWithValue("IdEnrollment", enrollmentId);
                    com.ExecuteNonQuery();
                }
                catch (Exception error)
                {
                    transaction.Rollback();
                    Console.WriteLine(error);
                    throw new Exception("Problem podczas zapisywania studenta");
                }

                transaction.Commit();
            }

            return new EnrollStudent()
            {
                Semester = 1,
                LastName = student.LastName,
                StartDate = enrollmentDate
            };
        }

        bool IStudentDbService.PromoteStudent(PromoteStudentRequest promoteStudentRequest)
        {
            throw new NotImplementedException();
        }

        EnrollStudent IStudentDbService.EnrollStudent(EnrollStudentRequest enrollStudentRequest)
        {
            throw new NotImplementedException();
        }

        Study IStudentDbService.GetStudy(string nameOfStudy)
        {
            throw new NotImplementedException();
        }
    }
}
