using LMS.Controllers;
using LMS.Models.LMSModels;
using LMS_CustomIdentity.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;

namespace LMSControllerTests
{
    public class CommonTests
    {
        // Uncomment the methods below after scaffolding
        // (they won't compile until then)

        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());
            var allDepts = ctrl.GetDepartments() as JsonResult;
            dynamic x = allDepts.Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        ///// <summary>
        ///// Make a small in-memory database, containing just one department with some stuff in it
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeOneDeptDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            db.Courses.Add(new Course { Name = "Databases", Number = 5530, Department = "CS" });

            db.SaveChanges();

            return db;
        }



        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }

    }

    public class AdminTests
    {
        //Admin Controller Tests
        [Fact]
        public void CreateThreeDepartment()
        {
            var db = MakeTinyDB();
            CommonController ctrl = new CommonController(db);
            AdministratorController admin = new AdministratorController(db);

            var createdArt = admin.CreateDepartment("ART", "Art") as JsonResult;
            var createdBiol = admin.CreateDepartment("BIOL", "College of Science's Biology") as JsonResult;
            var createdHist = admin.CreateDepartment("HIST", "History as a study") as JsonResult;

            var allDepts = ctrl.GetDepartments() as JsonResult;
            dynamic x = allDepts.Value;
            Assert.Equal(4, x.Length);
            Assert.Equal("ART", x[1].subject);
            Assert.Equal("BIOL", x[2].subject);
            Assert.Equal("HIST", x[3].subject);

            Assert.Equal("Art", x[1].name);
            Assert.Equal("College of Science's Biology", x[2].name);
            Assert.Equal("History as a study", x[3].name);
        }

        [Fact]
        public void CreateDuplicateDepartment()
        {
            var db = MakeTinyDB();
            AdministratorController admin = new AdministratorController(db);

            var created = admin.CreateDepartment("CS", "KSoC") as JsonResult;
            dynamic x = created.Value;
            Assert.Equal(false, x.success);
        }

        [Fact]
        public void GetCoursesOnEmpty()
        {
            var db = MakeTinyDB();
            AdministratorController admin = new AdministratorController(db);

            var courses = admin.GetCourses("CS") as JsonResult;
            dynamic x = courses.Value;
            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void GetOneCourse()
        {
            var db = MakeOneDeptDB();
            AdministratorController admin = new AdministratorController(db);

            var courses = admin.GetCourses("CS") as JsonResult;
            dynamic x = courses.Value;
            Assert.Equal(1, x.Length);
        }

        [Fact]
        public void CreateOneCourse()
        {
            var db = MakeTinyDB();
            AdministratorController admin = new AdministratorController(db);

            admin.CreateCourse("CS", 5530, "Databases");

            var courses = admin.GetCourses("CS") as JsonResult;
            dynamic x = courses.Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("Databases", x[0].name);
            Assert.Equal((uint)5530, x[0].number);
        }

        [Fact]
        public void GetProfessorsEmpty()
        {
            var db = MakeTinyDB();
            AdministratorController admin = new AdministratorController(db);

            admin.CreateCourse("CS", 5530, "Databases");

            var profs = admin.GetProfessors("CS") as JsonResult;
            dynamic x = profs.Value;
            Assert.Equal(0, x.Length);
        }

        [Fact]
        public void Get3Professors()
        {
            var db = MakeOneDeptDB();
            AdministratorController admin = new AdministratorController(db);

            var profs = admin.GetProfessors("CS") as JsonResult;
            dynamic x = profs.Value;
            Assert.Equal(3, x.Length);
        }

        [Fact]
        public void CreateOneClass()
        {
            var db = MakeOneDeptDB();
            AdministratorController admin = new AdministratorController(db);
            CommonController ctrl = new CommonController(db);

            var success = admin.CreateClass("CS", 5530, "Spring", 2023, new DateTime(2023, 1, 1, 12, 0, 0), new DateTime(2023, 1, 1, 13, 0, 0), "WEB", "u0000027") as JsonResult;
            dynamic suc = success.Value;
            Assert.True(suc.success);
            var classes = ctrl.GetClassOfferings("CS", 5530) as JsonResult;
            dynamic x = classes.Value;
            Assert.Equal(1, x.Length);
        }

        [Fact]
        public void CreateDuplicateClass()
        {
            var db = MakeOneDeptDB();
            AdministratorController admin = new AdministratorController(db);
            CommonController ctrl = new CommonController(db);

            var success = admin.CreateClass("CS", 5530, "Spring", 2023, new DateTime(2023, 1, 1, 12, 0, 0), new DateTime(2023, 1, 1, 13, 0, 0), "WEB", "u0000027") as JsonResult;
            dynamic suc = success.Value;
            Assert.True(suc.success);
            var success2 = admin.CreateClass("CS", 5530, "Spring", 2023, new DateTime(2023, 1, 1, 12, 0, 0), new DateTime(2023, 1, 1, 13, 0, 0), "WEB", "u0000027") as JsonResult;
            dynamic suc2 = success2.Value;
            Assert.False(suc2.success);

            var classes = ctrl.GetClassOfferings("CS", 5530) as JsonResult;
            dynamic x = classes.Value;
            Assert.Equal(1, x.Length);
        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        ///// <summary>
        ///// Make a small in-memory database, containing just one department with some stuff in it
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeOneDeptDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            db.Courses.Add(new Course { Name = "Databases", Number = 5530, Department = "CS" });

            db.Professors.Add(new Professor { FName = "Kendra", LName = "Roberts", WorksIn = "CS", UId = "u0000027" });
            db.Professors.Add(new Professor { FName = "Joe", LName = "Normal", WorksIn = "CS", UId = "u0000028" });
            db.Professors.Add(new Professor { FName = "Third", LName = "LNAME", WorksIn = "CS", UId = "u0000029" });



            db.SaveChanges();

            return db;
        }



        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }
    }

    public class ProfTests
    {
        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());
            var allDepts = ctrl.GetDepartments() as JsonResult;
            dynamic x = allDepts.Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        ///// <summary>
        ///// Make a small in-memory database, containing just one department with some stuff in it
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeOneDeptDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            db.Courses.Add(new Course { Name = "Databases", Number = 5530, Department = "CS" });

            db.SaveChanges();

            return db;
        }



        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }
    }

    public class StudTests
    {
        [Fact]
        public void Test1()
        {
            // An example of a simple unit test on the CommonController
            CommonController ctrl = new CommonController(MakeTinyDB());
            var allDepts = ctrl.GetDepartments() as JsonResult;
            dynamic x = allDepts.Value;
            Assert.Equal(1, x.Length);
            Assert.Equal("CS", x[0].subject);
        }

        ///// <summary>
        ///// Make a very tiny in-memory database, containing just one department
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeTinyDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            // TODO: add more objects to the test database

            db.SaveChanges();

            return db;
        }

        ///// <summary>
        ///// Make a small in-memory database, containing just one department with some stuff in it
        ///// and nothing else.
        ///// </summary>
        ///// <returns></returns>
        LMSContext MakeOneDeptDB()
        {
            var contextOptions = new DbContextOptionsBuilder<LMSContext>()
            .UseInMemoryDatabase("LMSControllerTest")
            .ConfigureWarnings(b => b.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .UseApplicationServiceProvider(NewServiceProvider())
            .Options;

            var db = new LMSContext(contextOptions);

            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();

            db.Departments.Add(new Department { Name = "KSoC", Subject = "CS" });

            db.Courses.Add(new Course { Name = "Databases", Number = 5530, Department = "CS" });

            db.SaveChanges();

            return db;
        }



        private static ServiceProvider NewServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
          .AddEntityFrameworkInMemoryDatabase()
          .BuildServiceProvider();

            return serviceProvider;
        }
    }
}