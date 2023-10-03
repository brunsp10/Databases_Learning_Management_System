using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private LMSContext db;
        public StudentController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Catalog()
        {
            return View();
        }

        public IActionResult Class(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult Assignment(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }


        public IActionResult ClassListings(string subject, string num)
        {
            System.Diagnostics.Debug.WriteLine(subject + num);
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            return View();
        }


        /*******Begin code to modify********/

        /// <summary>
        /// Returns a JSON array of the classes the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester
        /// "year" - The year part of the semester
        /// "grade" - The grade earned in the class, or "--" if one hasn't been assigned
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from e in db.Enrolleds
                        where e.Student == uid
                        join c in db.Classes on e.Class equals c.ClassId into allClasses
                        from ac in allClasses
                        join courses in db.Courses on ac.Listing equals courses.CatalogId
                        select new
                        {
                            subject = courses.Department,
                            number = courses.Number,
                            name = courses.Name,
                            season = ac.Season,
                            year = ac.Year,
                            grade = e.Grade
                        };
            return Json(query.ToArray());
        }

        /// <summary>
        /// Returns a JSON array of all the assignments in the given class that the given student is enrolled in.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The category name that the assignment belongs to
        /// "due" - The due Date/Time
        /// "score" - The score earned by the student, or null if the student has not submitted to this assignment.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="uid"></param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInClass(string subject, int num, string season, int year, string uid)
        {
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into theClass
                        from tc in theClass
                        where tc.Season == season && tc.Year == year
                        join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass into cat
                        from ca in cat
                        join asgn in db.Assignments on ca.CategoryId equals asgn.Category into allAssignments
                        from all in allAssignments
                        select new
                        {
                            aname = all.Name,
                            aId = all.AssignmentId,
                            cname = ca.Name,
                            due = all.Due,
                        };

            var query2 = from q in query
                         join s in db.Submissions
                         on new {A = q.aId, B = uid} equals new {A=s.Assignment, B=s.Student}
                         into joined
                         from j in joined.DefaultIfEmpty()
                         select new
                         {
                             aname = q.aname,
                             cname = q.cname,
                             due = q.due,
                             score = j == null ? null : (uint?)j.Score
                         };

            return Json(query2.ToArray());
        }



        /// <summary>
        /// Adds a submission to the given assignment for the given student
        /// The submission should use the current time as its DateTime
        /// You can get the current time with DateTime.Now
        /// The score of the submission should start as 0 until a Professor grades it
        /// If a Student submits to an assignment again, it should replace the submission contents
        /// and the submission time (the score should remain the same).
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="uid">The student submitting the assignment</param>
        /// <param name="contents">The text contents of the student's submission</param>
        /// <returns>A JSON object containing {success = true/false}</returns>
        public IActionResult SubmitAssignmentText(string subject, int num, string season, int year,
          string category, string asgname, string uid, string contents)
        {
            // Check if already submitted
            var submissionCheck = from course in db.Courses
                                  where course.Department == subject && course.Number == num
                                  join c in db.Classes on course.CatalogId equals c.Listing into allClasses
                                  from ac in allClasses
                                  where ac.Season == season && ac.Year == year
                                  join cat in db.AssignmentCategories on ac.ClassId equals cat.InClass into allCategories
                                  from allCats in allCategories
                                  where allCats.Name == category
                                  join asgn in db.Assignments on allCats.CategoryId equals asgn.Category into allAssignments
                                  from allAsgn in allAssignments
                                  where allAsgn.Name == asgname
                                  join s in db.Submissions on allAsgn.AssignmentId equals s.Assignment into allSubmissions
                                  from allSub in allSubmissions
                                  where allSub.Student == uid
                                  select allSub;

            if (submissionCheck.Any())
            {
                try
                {
                    Submission submission = submissionCheck.Single();
                    submission.SubmissionContents = contents;
                    submission.Time = DateTime.Now;
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch
                {
                    return Json(new { success = false });
                }

            } else
            {
                // Get the assignment id
                var query = from course in db.Courses
                            where course.Department == subject && course.Number == num
                            join c in db.Classes on course.CatalogId equals c.Listing into allClasses
                            from ac in allClasses
                            where ac.Season == season && ac.Year == year
                            join cat in db.AssignmentCategories on ac.ClassId equals cat.InClass into allCategories
                            from allCats in allCategories
                            where allCats.Name == category
                            join asgn in db.Assignments on allCats.CategoryId equals asgn.Category into allAssignments
                            from allAsgn in allAssignments
                            where allAsgn.Name == asgname
                            select allAsgn;

                Submission newSubmission = new Submission();
                newSubmission.SubmissionContents = contents;
                newSubmission.Time = DateTime.Now;
                newSubmission.Student = uid;
                newSubmission.Score = 0;
                newSubmission.Assignment = query.Single().AssignmentId;
                db.Add(newSubmission);
                try
                {
                    db.SaveChanges();
                    return Json(new { success = true });
                }
                catch
                {
                    return Json(new { success = false });
                }
            }
        }

        /// <summary>
        /// Enrolls a student in a class.
        /// </summary>
        /// <param name="subject">The department subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester</param>
        /// <param name="year">The year part of the semester</param>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing {success = {true/false}. 
        /// false if the student is already enrolled in the class, true otherwise.</returns>
        public IActionResult Enroll(string subject, int num, string season, int year, string uid)
        {
            // Get the class to enroll in
            var query = (from course in db.Courses
                        join c in db.Classes on course.CatalogId equals c.Listing
                        where course.Department == subject && course.Number == num &&
                        c.Season == season && c.Year == (uint)year
                        select new
                        {
                            id = c.ClassId
                        }).Single();

            // Make a new enrollment
            Enrolled newEnrollment = new Enrolled();
            newEnrollment.Student = uid;
            newEnrollment.Class = query.id;
            newEnrollment.Grade = "--";

            // Try to add the new enrollment
            db.Enrolleds.Add(newEnrollment);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }

            return Json(new { success = true});
        }



        /// <summary>
        /// Calculates a student's GPA
        /// A student's GPA is determined by the grade-point representation of the average grade in all their classes.
        /// Assume all classes are 4 credit hours.
        /// If a student does not have a grade in a class ("--"), that class is not counted in the average.
        /// If a student is not enrolled in any classes, they have a GPA of 0.0.
        /// Otherwise, the point-value of a letter grade is determined by the table on this page:
        /// https://advising.utah.edu/academic-standards/gpa-calculator-new.php
        /// </summary>
        /// <param name="uid">The uid of the student</param>
        /// <returns>A JSON object containing a single field called "gpa" with the number value</returns>
        public IActionResult GetGPA(string uid)
        {
            int count = 0;
            double totalPoints = 0;
            Dictionary<String, double> points = new Dictionary<String, double>();
            points.Add("A", 4.0);
            points.Add("A-", 3.7);
            points.Add("B+", 3.3);
            points.Add("B", 3.0);
            points.Add("B-", 2.7);
            points.Add("C+", 2.3);
            points.Add("C", 2.0);
            points.Add("C-", 1.7);
            points.Add("D+", 1.3);
            points.Add("D", 1.0);
            points.Add("D-", 0.7);
            points.Add("E", 0.0);

            var query = (from s in db.Students
                        where s.UId == uid
                        select new
                        {
                            grades = s.Enrolleds.ToArray()
                        }).Single();

            foreach (Enrolled g in query.grades)
            {
                if (g.Grade == "--")
                {
                    continue;
                }
                count++;
                totalPoints += points[g.Grade];
            }
            if (count == 0)
            {
                JsonObject gpa = new JsonObject();
                gpa.Add("gpa", 0.0);
                return Json(gpa);
            } else {
                double result = totalPoints / count;
                JsonObject gpa = new JsonObject();
                gpa.Add("gpa", result);
                return Json(gpa);
            }
        }
                
        /*******End code to modify********/

    }
}

