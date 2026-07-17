using DormMS.Server.Services.Interfaces;
using DormMS.Server.Repositories.Interfaces;
using DormMS.Server.Models;
using DormMS.Server.DTOs;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace DormMS.Server.Services.Implementations;

public class DocumentGenerationService : IDocumentGenerationService
{
    private readonly ICompetitionApplicationRepository _applicationRepository;
    private readonly IWebHostEnvironment _env;
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public DocumentGenerationService(
           ICompetitionApplicationRepository applicationRepository,
           IWebHostEnvironment env,
           AppDbContext context,
           IConfiguration configuration)
    {
        _applicationRepository = applicationRepository;
        _env = env;
        _context = context;
        _configuration = configuration;
    }

    public async Task<GenerateDocumentsResultDto> GenerateDocumentsForCompetitionAsync(int competitionId)
    {
        var result = new GenerateDocumentsResultDto();

        var applicationsWithoutDocument = await _context.CompetitionApplications
            .Include(a => a.Student)
            .ThenInclude(s => s.FinancialStatus)
            .Where(a => a.CompetitionId == competitionId && string.IsNullOrWhiteSpace(a.DocumentUrl))
            .ToListAsync();

        result.TotalProcessed = applicationsWithoutDocument.Count;

        //var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "documents");

        var configuredPath = _configuration["FileStorage:UploadsPath"] ?? "../DormMS-Uploads";
        var uploadsFolder = Path.GetFullPath(Path.Combine(_env.ContentRootPath, configuredPath, "documents"));
        Directory.CreateDirectory(uploadsFolder);

        foreach (var application in applicationsWithoutDocument)
        {
            try
            {
                var student = application.Student;
                var safeFileName = SanitizeFileName($"{student.StudentIDNumber}_{student.Name}_{student.Surname}");
                var fileName = $"{safeFileName}.pdf";
                var filePath = Path.Combine(uploadsFolder, fileName);

                if (student.IsFreshman)
                {
                    GenerateFreshmanCertificate(filePath, student);
                }
                else
                {
                    GenerateUniversityCertificate(filePath, student);
                }

                application.DocumentUrl = $"/uploads/documents/{fileName}";

                result.SuccessfullyGenerated++;
            }
            catch (Exception ex)
            {
                result.Errors.Add($"Error generating document for application {application.Id}: {ex.Message}");
                result.Failed++;
            }
        }

        await _context.SaveChangesAsync();

        return result;
    }
    private void GenerateFreshmanCertificate(string filePath, Student student)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Republika Srbija").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().Text("Srednja škola — Niš").FontSize(15).Bold();
                    col.Item().AlignCenter().PaddingBottom(15).Text("Uverenje o završenom srednjem obrazovanju")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().PaddingBottom(15).Text("SVEDOČANSTVO").FontSize(13).Bold();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    void Row(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(180).Text(label).Bold();
                            row.RelativeItem().Text(value);
                        });
                    }

                    Row("Ime i prezime:", $"{student.Name} {student.Surname}");
                    Row("JMBG:", student.JMBG);
                    Row("Smer:", "Opšti / Prirodno-matematički smer");
                    Row("Godina završetka:", "2026");
                    Row("Opšti uspeh (prosečna ocena):", $"{student.HighSchoolAverageGrade:0.00}");
                    Row("Namena dokumenta:", $"Prijava za smeštaj u dom — {student.FacultyName}");

                    col.Item().PaddingTop(20).Text(
                        "Ovim se potvrđuje da je gore navedeni učenik/učenica završio/la srednju školu " +
                        "sa naznačenim opštim uspehom. Dokument se izdaje u svrhu konkurisanja za smeštaj " +
                        "u studentskom domu.");

                    col.Item().PaddingTop(30).Text("Mesto i datum izdavanja: Niš, 20.06.2026.");
                    col.Item().PaddingTop(30).Text("M.P. ____________________________");
                    col.Item().Text("(potpis ovlašćenog lica)").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Republika Srbija").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().Text($"Opština {student.MunicipalityOfResidence}").FontSize(15).Bold();
                    col.Item().AlignCenter().PaddingBottom(15).Text("Odeljenje za socijalna i finansijska pitanja")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().PaddingBottom(15).Text("POTVRDA O FINANSIJSKOM STATUSU").FontSize(13).Bold();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    void Row(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(200).Text(label).Bold();
                            row.RelativeItem().Text(value);
                        });
                    }

                    Row("Ime i prezime:", $"{student.Name} {student.Surname}");
                    Row("JMBG:", student.JMBG);
                    Row("Adresa prebivališta:", student.HomeAddress);
                    Row("Opština prebivališta:", student.MunicipalityOfResidence);
                    Row("Mesto prebivališta:", student.MunicipalityOfResidence ?? "Nije dostupno");

                    if (student.FinancialStatus != null)
                    {
                        Row("Ukupan prihod porodice:", $"{student.FinancialStatus.TotalFamilyIncome:0.00} RSD");
                        Row("Broj članova porodice:", student.FinancialStatus.FamilyMemberCount.ToString());
                        Row("Prihod po članu porodice:", $"{student.FinancialStatus.IncomePerMember:0.00} RSD");
                    }
                    else
                    {
                        Row("Ukupan prihod porodice:", "Nije dostupno");
                        Row("Broj članova porodice:", "Nije dostupno");
                        Row("Prihod po članu porodice:", "Nije dostupno");
                    }

                    col.Item().PaddingTop(20).Text(
                       "Ovim se potvrđuje da su gore navedeni podaci o mestu prebivališta i finansijskom statusu " +
    "porodice tačni prema evidenciji opštine. Dokument se izdaje u svrhu konkurisanja " +
    "za smeštaj u studentskom domu.");

                    col.Item().PaddingTop(30).Text($"Mesto i datum izdavanja: {student.MunicipalityOfResidence}, {DateTime.Now:dd.MM.yyyy}.");
                    col.Item().PaddingTop(30).Text("M.P. ____________________________");
                    col.Item().Text("(potpis ovlašćenog lica opštine)").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

        });

        document.GeneratePdf(filePath);
    }

    private void GenerateUniversityCertificate(string filePath, Student student)
    {
        var document = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Univerzitet u Nišu").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().Text(student.FacultyName ?? "Fakultet").FontSize(15).Bold();
                    col.Item().AlignCenter().PaddingBottom(15).Text("Studentska služba")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().PaddingBottom(15).Text("POTVRDA O STATUSU STUDENTA").FontSize(13).Bold();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    void Row(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(180).Text(label).Bold();
                            row.RelativeItem().Text(value);
                        });
                    }

                    Row("Ime i prezime:", $"{student.Name} {student.Surname}");
                    Row("JMBG:", student.JMBG);
                    Row("Broj indeksa:", student.StudentIDNumber);
                    Row("Fakultet:", student.FacultyName ?? "");
                    Row("Ukupno ostvareno ESPB bodova:", $"{student.ESPB}");
                    Row("Prosečna ocena tokom studija:", $"{student.AverageGrade:0.00}");
                    Row("Namena dokumenta:", "Prijava za smeštaj u studentski dom");

                    col.Item().PaddingTop(20).Text(
                        "Ovim se potvrđuje da je gore navedeni student redovno upisan na osnovne studije " +
                        "i da su podaci o ostvarenim ESPB bodovima i prosečnoj oceni tačni prema evidenciji " +
                        "studentske službe. Potvrda se izdaje u svrhu konkurisanja za smeštaj u studentskom domu.");

                    col.Item().PaddingTop(30).Text("Mesto i datum izdavanja: Niš, 20.06.2026.");
                    col.Item().PaddingTop(30).Text("M.P. ____________________________");
                    col.Item().Text("(potpis ovlašćenog lica studentske službe)").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });

            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2.5f, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontFamily("DejaVu Sans").FontSize(11));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("Republika Srbija").FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().Text($"Opština {student.MunicipalityOfResidence}").FontSize(15).Bold();
                    col.Item().AlignCenter().PaddingBottom(15).Text("Odeljenje za socijalna i finansijska pitanja")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().AlignCenter().PaddingBottom(15).Text("POTVRDA O FINANSIJSKOM STATUSU").FontSize(13).Bold();
                });

                page.Content().Column(col =>
                {
                    col.Spacing(8);

                    void Row(string label, string value)
                    {
                        col.Item().Row(row =>
                        {
                            row.ConstantItem(200).Text(label).Bold();
                            row.RelativeItem().Text(value);
                        });
                    }

                    Row("Ime i prezime:", $"{student.Name} {student.Surname}");
                    Row("JMBG:", student.JMBG);
                    Row("Adresa prebivališta:", student.HomeAddress);
                    Row("Opština prebivališta:", student.MunicipalityOfResidence);
                    Row("Mesto prebivališta:", student.MunicipalityOfResidence ?? "Nije dostupno");

                    if (student.FinancialStatus != null)
                    {
                        Row("Ukupan prihod porodice:", $"{student.FinancialStatus.TotalFamilyIncome:0.00} RSD");
                        Row("Broj članova porodice:", student.FinancialStatus.FamilyMemberCount.ToString());
                        Row("Prihod po članu porodice:", $"{student.FinancialStatus.IncomePerMember:0.00} RSD");
                    }
                    else
                    {
                        Row("Ukupan prihod porodice:", "Nije dostupno");
                        Row("Broj članova porodice:", "Nije dostupno");
                        Row("Prihod po članu porodice:", "Nije dostupno");
                    }

                    col.Item().PaddingTop(20).Text(
                        "Ovim se potvrđuje da su gore navedeni podaci o mestu prebivališta i finansijskom statusu " +
                        "porodice tačni prema evidenciji opštine. Dokument se izdaje u svrhu konkurisanja " +
                        "za smeštaj u studentskom domu.");

                    col.Item().PaddingTop(30).Text($"Mesto i datum izdavanja: {student.MunicipalityOfResidence}, {DateTime.Now:dd.MM.yyyy}.");
                    col.Item().PaddingTop(30).Text("M.P. ____________________________");
                    col.Item().Text("(potpis ovlašćenog lica opštine)").FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        document.GeneratePdf(filePath);
    }

    private static string SanitizeFileName(string input)
    {
        var map = new Dictionary<char, string>
        {
            ['š'] = "s",
            ['Š'] = "S",
            ['č'] = "c",
            ['Č'] = "C",
            ['ć'] = "c",
            ['Ć'] = "C",
            ['ž'] = "z",
            ['Ž'] = "Z",
            ['đ'] = "dj",
            ['Đ'] = "Dj"
        };

        var sb = new System.Text.StringBuilder();
        foreach (var ch in input)
            sb.Append(map.TryGetValue(ch, out var replacement) ? replacement : ch.ToString());

        return sb.ToString().Replace(" ", "_");
    }
}