using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using DormMS.Server.DTOs;
using DormMS.Server.Models;

namespace DormMS.Server.Seed;

/// <summary>
/// Popunjava bazu realnim demo podacima: jedan studentski dom, sobe, referent,
/// studenti, finansijski statusi, konkursi sa prijavama, veseraj i notifikacije.
///
/// Pokrece se SAMO ako je "Seed:DemoData" = true (Seed__DemoData=true u Docker-u
/// ili appsettings.Development.json za lokalni razvoj) i ako baza nije vec popunjena.
/// Idempotentno: ako dom vec postoji, ne radi nista.
///
/// Demo nalozi (lozinke su u bazi hesovane, ovde su samo radi dokumentacije):
///   Referent: manager@dormms.local / Manager123!
///   Studenti: student1@dormms.local ... student40@dormms.local / Student123!
/// </summary>
public static class DemoDataSeeder
{
    private const string ManagerPassword = "Manager123!";
    private const string StudentPassword = "Student123!";

    private static readonly string[] Faculties =
    {
        "Građevinsko-arhitektonski fakultet (GAF)",
        "Elektronski fakultet",
        "Mašinski fakultet",
        "Pravni fakultet",
        "Filozofski fakultet",
        "Fakultet sporta i fizičkog vaspitanja (DIF)",
        "Fakultet zaštite na radu",
        "Ekonomski fakultet",
        "Medicinski fakultet",
        "Prirodno-matematički fakultet (PMF)",
        "Fakultet umetnosti"
    };

    private static readonly string[] Cities =
    {
        "Leskovac", "Vranje", "Pirot", "Aleksinac", "Prokuplje", "Knjaževac",
        "Zaječar", "Kruševac", "Jagodina", "Paraćin", "Ćuprija", "Bor",
        "Negotin", "Surdulica", "Vladičin Han", "Bela Palanka", "Svrljig",
        "Doljevac", "Merošina", "Gadžin Han", "Sokobanja"
    };

    private static readonly string[] Streets =
    {
        "Njegoševa", "Knjaževačka", "Dušanova", "Nikole Pašića",
        "Skopska", "Sremska", "Vojvode Mišića", "Jadranska",
        "Prvomajska", "Kosovska"
    };

    private static readonly string[] MaleFirstNames =
    {
        "Nikola", "Marko", "Luka", "Stefan", "Miloš", "Đorđe", "Nemanja",
        "Aleksandar", "Vukašin", "Filip", "Jovan", "Petar", "Uroš", "Bogdan",
        "Lazar", "Dušan", "Mihajlo", "Vuk", "Strahinja", "Andrej"
    };

    private static readonly string[] FemaleFirstNames =
    {
        "Milica", "Jovana", "Ana", "Teodora", "Anđela", "Sara", "Katarina",
        "Mina", "Nevena", "Tamara", "Marija", "Sofija", "Ivana", "Aleksandra",
        "Jelena", "Dragana", "Nikolina", "Kristina", "Emilija", "Marina"
    };

    private static readonly string[] Surnames =
    {
        "Jovanović", "Petrović", "Nikolić", "Marković", "Đorđević",
        "Stojanović", "Ilić", "Stanković", "Pavlović", "Milošević",
        "Todorović", "Popović", "Simić", "Ristić", "Kostić",
        "Vasić", "Radovanović", "Đukić", "Mitrović", "Živković"
    };

    public static async Task SeedDemoDataAsync(this IServiceProvider services, IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("Seed:DemoData"))
            return;

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Idempotentnost - demo podaci se ubacuju samo jednom.
        if (await context.Dorms.AnyAsync())
            return;

        // Determinisan random -> svaki developer dobija identicne demo podatke.
        var random = new Random(20251102);
        var hasher = new PasswordHasher<User>();

        var dorm = new Dorm
        {
            Name = "Studentski dom Kralj Aleksandar I",
            Location = "Niš",
            Municipality = "Medijana"
        };
        context.Dorms.Add(dorm);
        await context.SaveChangesAsync();

        // --- Sobe: 2 sprata x 12 soba, sprat 1 zenski, sprat 2 muski (naizmenicno 2 i 3 kreveta) ---
        var rooms = new List<DormitoryRoom>();
        for (var floor = 1; floor <= 2; floor++)
        {
            var gender = floor == 1 ? GenderType.Female : GenderType.Male;

            for (var number = 1; number <= 12; number++)
            {
                rooms.Add(new DormitoryRoom
                {
                    RoomNumber = $"{floor}{number:D2}",
                    Capacity = number % 2 == 0 ? 3 : 2,
                    CurrentOccupancy = 0,
                    Gender = gender,
                    Status = RoomStatus.Available,
                    DormId = dorm.Id
                });
            }
        }
        context.DormitoryRooms.AddRange(rooms);
        await context.SaveChangesAsync();

        // --- Referent (dorm manager) za dom ---
        var managerEmail = "manager@dormms.local";
        var manager = new DormManager
        {
            Name = "Miloš",
            Surname = "Stanković",
            Email = managerEmail,
            PhoneNumber = "0601234567",
            DormId = dorm.Id
        };
        context.DormManagers.Add(manager);
        await context.SaveChangesAsync();

        var managerUser = new User
        {
            Email = managerEmail,
            Role = UserRole.DormManager,
            IsActive = true,
            DormManagerId = manager.Id
        };
        managerUser.PasswordHash = hasher.HashPassword(managerUser, ManagerPassword);
        context.Users.Add(managerUser);
        await context.SaveChangesAsync();

        // --- Studenti: 30 starijih + 10 brucosa ---
        var currentYear = DateTime.Now.Year;
        var students = new List<Student>();
        var financialData = new List<(Student Student, decimal Income, int Members)>();

        for (var i = 1; i <= 40; i++)
        {
            var isFreshman = i > 30;
            var gender = random.Next(2) == 0 ? Gender.Male : Gender.Female;
            var firstName = gender == Gender.Male
                ? MaleFirstNames[random.Next(MaleFirstNames.Length)]
                : FemaleFirstNames[random.Next(FemaleFirstNames.Length)];
            var surname = Surnames[random.Next(Surnames.Length)];

            var facultyIndex = random.Next(Faculties.Length);
            var age = isFreshman ? 19 : random.Next(20, 26);

            var student = new Student
            {
                Name = firstName,
                Surname = surname,
                Email = $"student{i}@dormms.local",
                StudentIDNumber = GenerateIndexNumber(random, Faculties[facultyIndex]),
                Age = age,
                Gender = gender,
                YearOfEnrollment = isFreshman ? currentYear : currentYear - random.Next(1, 5),
                StudyLevel = random.Next(100) < 90 ? StudyLevel.Undergraduate : StudyLevel.Postgraduate,
                Status = StudentStatus.Active,
                ProfileStatus = StudentProfileStatus.Verified,
                FacultyId = facultyIndex + 1,
                FacultyName = Faculties[facultyIndex],
                MunicipalityOfResidence = random.Next(100) < 90 ? Cities[random.Next(Cities.Length)] : "Niš",
                HomeAddress = $"{Streets[random.Next(Streets.Length)]} {random.Next(1, 150)}",
                IsBudgetStudent = true,
                ESPB = isFreshman ? 0 : random.Next(60, 241),
                AverageGrade = isFreshman ? null : Math.Round((decimal)(random.NextDouble() * 3.5 + 6.5), 2),
                HighSchoolAverageGrade = isFreshman ? Math.Round((decimal)(random.NextDouble() * 1.5 + 3.5), 2) : null
            };
            student.JMBG = GenerateJmbg(random, age, gender);

            students.Add(student);
            financialData.Add((student, random.Next(40000, 180001), random.Next(2, 7)));
        }

        context.Students.AddRange(students);
        await context.SaveChangesAsync();

        // User nalozi za studente (potrebno nakon SaveChanges da bi StudentId bio dodeljen)
        foreach (var student in students)
        {
            var user = new User
            {
                Email = student.Email,
                Role = UserRole.Student,
                IsActive = true,
                StudentId = student.Id
            };
            user.PasswordHash = hasher.HashPassword(user, StudentPassword);
            context.Users.Add(user);
        }

        // Finansijski statusi (neophodno za bodovanje/rangiranje)
        foreach (var (student, income, members) in financialData)
        {
            context.StudentFinancialStatuses.Add(new StudentFinancialStatus
            {
                StudentId = student.Id,
                TotalFamilyIncome = income,
                FamilyMemberCount = members
            });
        }
        await context.SaveChangesAsync();

        // --- Raspodela soba: samo stariji studenti; svaki treci ostaje bez sobe (kandidat za konkurs) ---
        var withoutRoom = new List<Student>();
        var olderStudents = students.Where(s => !s.IsFreshman).ToList();

        for (var i = 0; i < olderStudents.Count; i++)
        {
            var student = olderStudents[i];

            if (i % 3 == 2)
            {
                withoutRoom.Add(student);
                continue;
            }

            var studentRoomGender = student.Gender == Gender.Male ? GenderType.Male : GenderType.Female;
            var room = rooms.FirstOrDefault(r => r.Gender == studentRoomGender && r.CurrentOccupancy < r.Capacity);
            if (room is null)
            {
                withoutRoom.Add(student);
                continue;
            }

            student.DormitoryRoomId = room.Id;
            room.CurrentOccupancy++;
        }

        foreach (var room in rooms)
        {
            room.Status = room.CurrentOccupancy >= room.Capacity
                ? RoomStatus.FullyOccupied
                : RoomStatus.Available;
        }
        await context.SaveChangesAsync();

        // --- Konkursi ---
        var adminId = await context.Admins.Select(a => (int?)a.Id).FirstOrDefaultAsync();

        var openCompetition = new Competition
        {
            Name = "Konkurs za smeštaj u studentskom domu 2025/2026",
            Description = "Redovni konkurs za smeštaj starijih studenata u studentskom domu Kralj Aleksandar I.",
            StartDate = DateTime.Now.Date.AddDays(-7),
            EndDate = DateTime.Now.Date.AddDays(21),
            Status = CompetitionStatus.Open,
            Type = CompetitionType.General,
            MaxStudents = 60,
            CreatedByAdminId = adminId
        };

        var completedCompetition = new Competition
        {
            Name = "Konkurs za brucoše 2024/2025",
            Description = "Konkurs za upis brucoša u studentski dom za školsku 2024/2025. godinu.",
            StartDate = DateTime.Now.Date.AddMonths(-10),
            EndDate = DateTime.Now.Date.AddMonths(-9),
            Status = CompetitionStatus.Completed,
            Type = CompetitionType.Freshmen,
            MaxStudents = 6,
            CreatedByAdminId = adminId
        };

        context.Competitions.AddRange(openCompetition, completedCompetition);
        await context.SaveChangesAsync();

        // Prijave starijih studenata bez sobe na otvoreni konkurs
        foreach (var student in withoutRoom)
        {
            context.CompetitionApplications.Add(new CompetitionApplication
            {
                StudentId = student.Id,
                CompetitionId = openCompetition.Id,
                Status = ApplicationStatus.Pending,
                AppliedAt = DateTime.Now.Date.AddDays(-random.Next(1, 6))
            });
        }
        await context.SaveChangesAsync();

        // --- Zavrsen konkurs za brucose: prijave + rang lista ---
        var freshmen = students.Where(s => s.IsFreshman).ToList();
        var approvedFreshmen = freshmen.Take(8).ToList();
        var rejectedFreshmen = freshmen.Skip(8).ToList();

        foreach (var student in approvedFreshmen)
        {
            context.CompetitionApplications.Add(new CompetitionApplication
            {
                StudentId = student.Id,
                CompetitionId = completedCompetition.Id,
                Status = ApplicationStatus.Approved,
                AppliedAt = completedCompetition.StartDate.AddDays(3)
            });
        }

        foreach (var student in rejectedFreshmen)
        {
            context.CompetitionApplications.Add(new CompetitionApplication
            {
                StudentId = student.Id,
                CompetitionId = completedCompetition.Id,
                Status = ApplicationStatus.Rejected,
                Notes = "Nedostaje odgovarajuća dokumentacija.",
                AppliedAt = completedCompetition.StartDate.AddDays(4)
            });
        }
        await context.SaveChangesAsync();

        const decimal averageSalary = 121650m;
        var rankedFreshmen = approvedFreshmen
            .Select(s =>
            {
                var financial = financialData.First(f => f.Student.Id == s.Id);
                var incomePerMember = Math.Round(financial.Income / financial.Members, 2);
                var highSchoolPoints = Math.Round((s.HighSchoolAverageGrade ?? 0m) * 8m, 2);
                var socialBonus = incomePerMember > 0 && incomePerMember < averageSalary * 0.5m ? 1m : 0m;
                return new
                {
                    Student = s,
                    IncomePerMember = incomePerMember,
                    HighSchoolPoints = highSchoolPoints,
                    SocialBonus = socialBonus,
                    TotalPoints = Math.Round(highSchoolPoints + socialBonus, 2)
                };
            })
            .OrderByDescending(r => r.TotalPoints)
            .ThenByDescending(r => r.HighSchoolPoints)
            .ToList();

        for (var i = 0; i < rankedFreshmen.Count; i++)
        {
            var entry = rankedFreshmen[i];
            var financial = financialData.First(f => f.Student.Id == entry.Student.Id);

            context.RankingResults.Add(new RankingResult
            {
                CompetitionId = completedCompetition.Id,
                StudentId = entry.Student.Id,
                FacultyName = entry.Student.FacultyName ?? "Unknown",
                IsFreshman = true,
                CalculatedPoints = entry.TotalPoints,
                Status = i < completedCompetition.MaxStudents
                    ? AssignmentStatus.Assigned
                    : AssignmentStatus.Waitlisted,
                HighSchoolAverageGrade = entry.Student.HighSchoolAverageGrade,
                HighSchoolPoints = entry.HighSchoolPoints,
                SocialBonus = entry.SocialBonus,
                TotalFamilyIncome = financial.Income,
                FamilyMemberCount = financial.Members,
                IncomePerMember = entry.IncomePerMember
            });
        }
        await context.SaveChangesAsync();

        // --- Veseraj: masine + par rezervacija ---
        var machines = Enumerable.Range(1, 4)
            .Select(n => new LaundryMachine
            {
                Name = $"Veš mašina {n}",
                Description = n % 2 == 0 ? "Mašina za pranje veša (6 kg)" : "Mašina za pranje veša (8 kg)",
                IsActive = true,
                DormId = dorm.Id
            })
            .ToList();
        context.LaundryMachines.AddRange(machines);
        await context.SaveChangesAsync();

        var studentsWithRooms = students.Where(s => s.DormitoryRoomId != null).Take(5).ToList();
        for (var i = 0; i < studentsWithRooms.Count; i++)
        {
            var start = DateTime.Now.Date.AddDays(i % 3 + 1).AddHours(18);
            context.LaundryReservations.Add(new LaundryReservation
            {
                LaundryMachineId = machines[i % machines.Count].Id,
                StudentId = studentsWithRooms[i].Id,
                StartTime = start,
                EndTime = start.AddHours(2),
                Status = LaundryReservationStatus.Upcoming
            });
        }
        await context.SaveChangesAsync();

        // --- Notifikacije ---
        foreach (var student in studentsWithRooms.Take(3))
        {
            var room = rooms.First(r => r.Id == student.DormitoryRoomId);
            context.Notifications.Add(new Notification
            {
                StudentId = student.Id,
                Message = $"Dodeljena vam je soba {room.RoomNumber} u domu {dorm.Name}.",
                IsRead = false,
                CreatedAt = DateTime.Now.Date.AddDays(-2)
            });
        }

        context.Notifications.Add(new Notification
        {
            DormId = dorm.Id,
            Message = "Konkurs za smeštaj za 2025/2026. godinu je otvoren. Prijavite se na vreme.",
            IsRead = false,
            CreatedAt = DateTime.Now.Date.AddDays(-7)
        });

        await context.SaveChangesAsync();
    }

    private static string GenerateIndexNumber(Random random, string facultyName)
    {
        if (facultyName.Contains("Elektronski"))
            return random.Next(10000, 19999).ToString();
        if (facultyName.Contains("GAF"))
            return (random.Next(2) == 0 ? "pa-" : "rg-") + random.Next(1000, 9999);
        if (facultyName.Contains("Mašinski"))
            return random.Next(1000, 9999).ToString();

        return random.Next(1000, 99999).ToString();
    }

    private static string GenerateJmbg(Random random, int age, Gender gender)
    {
        var day = random.Next(1, 28).ToString("D2");
        var month = random.Next(1, 13).ToString("D2");
        var yearPart = ((DateTime.Now.Year - age) % 1000).ToString("D3");
        var region = random.Next(70, 100).ToString("D2");
        var serial = random.Next(gender == Gender.Male ? 0 : 500, gender == Gender.Male ? 500 : 1000).ToString("D3");

        var first12 = day + month + yearPart + region + serial;
        var digits = first12.Select(c => c - '0').ToArray();

        var sum = 7 * (digits[0] + digits[6])
                + 6 * (digits[1] + digits[7])
                + 5 * (digits[2] + digits[8])
                + 4 * (digits[3] + digits[9])
                + 3 * (digits[4] + digits[10])
                + 2 * (digits[5] + digits[11]);

        var remainder = sum % 11;
        var control = remainder < 2 ? remainder : 11 - remainder;

        return first12 + control;
    }
}
