using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using LMS.Models.LMSModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860
[assembly: InternalsVisibleTo("LMSControllerTests")]
namespace LMS_CustomIdentity.Controllers
{
    [Authorize(Roles = "Professor")]
    public class ProfessorController : Controller
    {

        private readonly LMSContext db;

        public ProfessorController(LMSContext _db)
        {
            db = _db;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Students(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
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

        public IActionResult Categories(string subject, string num, string season, string year)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            return View();
        }

        public IActionResult CatAssignments(string subject, string num, string season, string year, string cat)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
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

        public IActionResult Submissions(string subject, string num, string season, string year, string cat, string aname)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            return View();
        }

        public IActionResult Grade(string subject, string num, string season, string year, string cat, string aname, string uid)
        {
            ViewData["subject"] = subject;
            ViewData["num"] = num;
            ViewData["season"] = season;
            ViewData["year"] = year;
            ViewData["cat"] = cat;
            ViewData["aname"] = aname;
            ViewData["uid"] = uid;
            return View();
        }

        /*******Begin code to modify********/


        /// <summary>
        /// Returns a JSON array of all the students in a class.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "dob" - date of birth
        /// "grade" - the student's grade in this class
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetStudentsInClass(string subject, int num, string season, int year)
        {
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into allClasses
                        from ac in allClasses
                        where ac.Season == season && ac.Year == year
                        join e in db.Enrolleds on ac.ClassId equals e.Class into allEnrolled
                        from ae in allEnrolled
                        join s in db.Students on ae.Student equals s.UId
                        select new
                        {
                            fname = s.FName,
                            lname = s.LName,
                            uid = s.UId,
                            dob = s.Dob,
                            grade = ae.Grade
                        };

            return Json(query.ToArray());
        }



        /// <summary>
        /// Returns a JSON array with all the assignments in an assignment category for a class.
        /// If the "category" parameter is null, return all assignments in the class.
        /// Each object in the array should have the following fields:
        /// "aname" - The assignment name
        /// "cname" - The assignment category name.
        /// "due" - The due DateTime
        /// "submissions" - The number of submissions to the assignment
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class, 
        /// or null to return assignments from all categories</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentsInCategory(string subject, int num, string season, int year, string category)
        {
            if (category != null)
            {
                var specificCat = from course in db.Courses
                            where course.Department == subject && course.Number == num
                            join c in db.Classes on course.CatalogId equals c.Listing into theClass
                            from tc in theClass
                            where tc.Season == season && tc.Year == year
                            join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass into cat
                            from ca in cat
                            where ca.Name == category
                            join asgn in db.Assignments on ca.CategoryId equals asgn.Category
                            select new
                            {
                                aname = asgn.Name,
                                cname = ca.Name,
                                due = asgn.Due,
                                submissions = asgn.Submissions.Count()
                            };

                return Json(specificCat.ToArray());
            }
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into theClass
                        from tc in theClass
                        where tc.Season == season && tc.Year == year
                        join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass into cat
                        from ca in cat
                        join asgn in db.Assignments on ca.CategoryId equals asgn.Category
                        select new
                        {
                            aname = asgn.Name,
                            cname = ca.Name,
                            due = asgn.Due,
                            submissions = asgn.Submissions.Count()
                        };

            return Json(query.ToArray());
        }


        /// <summary>
        /// Returns a JSON array of the assignment categories for a certain class.
        /// Each object in the array should have the folling fields:
        /// "name" - The category name
        /// "weight" - The category weight
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetAssignmentCategories(string subject, int num, string season, int year)
        {
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into theClass
                        from tc in theClass
                        where tc.Season == season && tc.Year == year
                        join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass
                        select new
                        {
                            name = ac.Name,
                            weight = ac.Weight
                        };

            return Json(query.ToArray());
        }

        /// <summary>
        /// Creates a new assignment category for the specified class.
        /// If a category of the given class with the given name already exists, return success = false.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The new category name</param>
        /// <param name="catweight">The new category weight</param>
        /// <returns>A JSON object containing {success = true/false} </returns>
        public IActionResult CreateAssignmentCategory(string subject, int num, string season, int year, string category, int catweight)
        {
            // Get the ID for the class
            var getID = (from c in db.Classes
                         join course in db.Courses on c.Listing equals course.CatalogId
                         where c.Season == season && c.Year == year &&
                         course.Department == subject && course.Number == num
                         select new
                         {
                             id = c.ClassId
                         }).Single();
            uint classID = getID.id;

            // Create the new category
            AssignmentCategory newCategory = new AssignmentCategory();
            newCategory.Weight = (uint)catweight;
            newCategory.Name = category;
            newCategory.InClass = classID;

            // Try to add the new category to the database
            db.AssignmentCategories.Add(newCategory);
            try
            {
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                return Json(new { success = false });
            }
            return Json(new { success = true });
        }

        /// <summary>
        /// Creates a new assignment for the given class and category.
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The new assignment name</param>
        /// <param name="asgpoints">The max point value for the new assignment</param>
        /// <param name="asgdue">The due DateTime for the new assignment</param>
        /// <param name="asgcontents">The contents of the new assignment</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult CreateAssignment(string subject, int num, string season, int year, string category, string asgname, int asgpoints, DateTime asgdue, string asgcontents)
        {
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into theClass
                        from tc in theClass
                        where tc.Season == season && tc.Year == year
                        join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass into cat
                        from ca in cat
                        where ca.Name == category
                        select new
                        {
                            category = ca.CategoryId,
                            students = tc.Enrolleds,
                            classID = tc.ClassId
                        };

            // Make the new assignment
            Assignment newAssignment = new Assignment();
            newAssignment.Name = asgname;
            newAssignment.MaxPoints = (uint)asgpoints;
            newAssignment.Due = asgdue;
            newAssignment.Contents = asgcontents;
            newAssignment.Category = query.Single().category;

            try
            {
                db.Assignments.Add(newAssignment);
                db.SaveChanges();
            } catch
            {
              return Json(new { success = false });
            }

            // Update the grades of all students in the course
            try
            {
                foreach (Enrolled s in query.Single().students)
                {
                    UpdateStudentGrade(subject, num, season, year, s.Student);
                }
                return Json(new { success = true });
            } catch
            {
                return Json(new { success = false });
            }
        }


        /// <summary>
        /// Gets a JSON array of all the submissions to a certain assignment.
        /// Each object in the array should have the following fields:
        /// "fname" - first name
        /// "lname" - last name
        /// "uid" - user ID
        /// "time" - DateTime of the submission
        /// "score" - The score given to the submission
        /// 
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetSubmissionsToAssignment(string subject, int num, string season, int year, string category, string asgname)
        {
            var query = from course in db.Courses
                        where course.Department == subject && course.Number == num
                        join c in db.Classes on course.CatalogId equals c.Listing into theClass
                        from tc in theClass
                        where tc.Season == season && tc.Year == year
                        join ac in db.AssignmentCategories on tc.ClassId equals ac.InClass into cat
                        from ca in cat
                        where ca.Name == category
                        join asgn in db.Assignments on ca.CategoryId equals asgn.Category into theAssignment
                        from ta in theAssignment
                        where ta.Name == asgname
                        join s in db.Submissions on ta.AssignmentId equals s.Assignment into allSubmissions
                        from aS in allSubmissions
                        join st in db.Students on aS.Student equals st.UId into submittedBy
                        from sb in submittedBy
                        select new
                        {
                            fname = sb.FName,
                            lname = sb.LName,
                            uid = sb.UId,
                            time = aS.Time,
                            score = aS.Score
                        };
            return Json(query.ToArray());
        }


        /// <summary>
        /// Set the score of an assignment submission
        /// </summary>
        /// <param name="subject">The course subject abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The season part of the semester for the class the assignment belongs to</param>
        /// <param name="year">The year part of the semester for the class the assignment belongs to</param>
        /// <param name="category">The name of the assignment category in the class</param>
        /// <param name="asgname">The name of the assignment</param>
        /// <param name="uid">The uid of the student who's submission is being graded</param>
        /// <param name="score">The new score for the submission</param>
        /// <returns>A JSON object containing success = true/false</returns>
        public IActionResult GradeSubmission(string subject, int num, string season, int year, string category, string asgname, string uid, int score)
        {
            var query = from courses in db.Courses
                        where courses.Department == subject && courses.Number == num
                        join c in db.Classes on courses.CatalogId equals c.Listing into allClasses
                        from ac in allClasses
                        where ac.Season == season && ac.Year == year
                        join a in db.AssignmentCategories on ac.ClassId equals a.InClass into categories
                        from cat in categories
                        where cat.Name == category
                        join asgn in db.Assignments on cat.CategoryId equals asgn.Category into allAssignments
                        from allAsgn in allAssignments
                        where allAsgn.Name == asgname
                        join s in db.Submissions on allAsgn.AssignmentId equals s.Assignment into allSubmitted
                        from allSub in allSubmitted
                        where allSub.Student == uid
                        select allSub;

            // Update the student's score on the assignment
            try
            {
                Submission toUpdate = query.Single();
                toUpdate.Score = (uint)score;
                db.SaveChanges();
            }
            catch
            {
                return Json(new { success = false });
            }

            // Update the student's grade for the course
            try
            {
                UpdateStudentGrade(subject, num, season, year, uid);
                return Json(new { success = true });
            } catch
            {
                return Json(new { success = false });
            }

        }

        /// <summary>
        /// Helper method to update a student's grade in a class
        /// </summary>
        /// <param name="subject">The course department abbreviation</param>
        /// <param name="num">The course number</param>
        /// <param name="season">The class season</param>
        /// <param name="year">The class year</param>
        /// <param name="uid">The student's uid</param>
        /// <returns></returns>
        private IActionResult UpdateStudentGrade(string subject, int num, string season, int year, string uid)
        {
            // Get all of the class assignments
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
                            weight = ca.Weight,
                            cname = ca.Name,
                            due = all.Due,
                            classID = tc.ClassId,
                            value = all.MaxPoints,
                            counts = ca.Assignments.Count()
                        };

            // Get all of the scores
            var query2 = from q in query
                         join s in db.Submissions
                         on new { A = q.aId, B = uid } equals new { A = s.Assignment, B = s.Student }
                         into joined
                         from j in joined.DefaultIfEmpty()
                         select new
                         {
                             aname = q.aname,
                             cname = q.cname,
                             weight = q.weight,
                             score = j == null ? 0 : j.Score,
                             maxPoints = q.value,
                             numAssignments = q.counts
                         };

            // Make dictionaries for tracking values
            Dictionary<String, uint> categoryWeights = new Dictionary<String, uint>();
            Dictionary<String, uint> categoryTotalPoints = new Dictionary<string, uint>();
            Dictionary<String, uint> categoryPointsEarned = new Dictionary<string, uint>();

            // Get the categories, assignments, and submissions for the student
            foreach (var x in query2)
            {
                if (x.numAssignments == 0)
                {
                    continue;
                }
                if (!categoryWeights.ContainsKey(x.cname))
                {
                    categoryWeights.Add(x.cname, x.weight);
                    categoryTotalPoints.Add(x.cname, x.maxPoints);
                    categoryPointsEarned.Add(x.cname, x.score);
                }
                else
                {
                    categoryTotalPoints[x.cname]+= x.maxPoints;
                    categoryPointsEarned[x.cname]+= x.score;
                }
            }

            // Calculate the total weight and total points earned
            uint totalWeight = 0;
            double totalScore = 0;
            foreach (string key in  categoryWeights.Keys)
            {
                totalWeight += categoryWeights[key];
                double categoryPerc = (double)categoryPointsEarned[key] / categoryTotalPoints[key];
                totalScore += categoryPerc * categoryWeights[key];
            }

            // Get the final score based on total weight
            double finalScore = (100/ (double)totalWeight) * totalScore;
            string letterGrade = GetLetterGrade(finalScore);

            // Get the class ID for Enrolled table
            uint classID = query.First().classID;

            // Update the student's grade in the enrollment table
            var updateQuery = from e in db.Enrolleds
                              where e.Class == classID && e.Student == uid
                              select e;
            try
            {
                Enrolled enrollment = updateQuery.Single();
                enrollment.Grade = letterGrade;
                db.SaveChanges();
                return Json(new { success = true });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        /// <summary>
        /// Helper method to return a letter grade for
        /// a student in a class
        /// </summary>
        /// <param name="perc">The student's percentage of points earned</param>
        /// <returns>A letter grade</returns>
        private string GetLetterGrade(double perc)
        {
            if (perc >= 93)
            {
                return "A";
            } else if (perc >= 90 && perc < 93)
            {
                return "A-";
            }
            else if (perc >= 87 && perc < 90)
            {
                return "B+";
            }
            else if (perc >= 83 && perc < 87)
            {
                return "B";
            }
            else if (perc >= 80 && perc < 83)
            {
                return "B-";
            }
            else if (perc >= 77 && perc < 80)
            {
                return "C+";
            }
            else if (perc >= 73 && perc < 77)
            {
                return "C";
            }
            else if (perc >= 70 && perc < 73)
            {
                return "C-";
            }
            else if (perc >= 67 && perc < 70)
            {
                return "D+";
            }
            else if (perc >= 63 && perc < 67)
            {
                return "D";
            }
            else if (perc >= 60 && perc < 63)
            {
                return "D-";
            } else
            {
                return "E";
            }
        }

        /// <summary>
        /// Returns a JSON array of the classes taught by the specified professor
        /// Each object in the array should have the following fields:
        /// "subject" - The subject abbreviation of the class (such as "CS")
        /// "number" - The course number (such as 5530)
        /// "name" - The course name
        /// "season" - The season part of the semester in which the class is taught
        /// "year" - The year part of the semester in which the class is taught
        /// </summary>
        /// <param name="uid">The professor's uid</param>
        /// <returns>The JSON array</returns>
        public IActionResult GetMyClasses(string uid)
        {
            var query = from c in db.Classes
                        where c.TaughtBy == uid
                        join course in db.Courses on c.Listing equals course.CatalogId
                        select new
                        {
                            subject = course.Department,
                            number = course.Number,
                            name = course.Name,
                            season = c.Season,
                            year = c.Year
                        };
                        
            return Json(query.ToArray());
        }


        
        /*******End code to modify********/
    }
}

