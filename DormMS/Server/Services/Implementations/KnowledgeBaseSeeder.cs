using DormMS.Server.Models;
using DormMS.Server.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace DormMS.Server.Services;

public static class KnowledgeBaseSeeder
{
    private static readonly (string Question, string Answer)[] Entries =
    {
        ("Kako da se registrujem na sistem?",
         "U gornjem desnom uglu ekrana kliknite na dugme prijavi se. Nakon toga, kliknite na link Registrujte se i unesite tražene podatke."),

        ("Kako da se ulogujem?",
         "U gornjem desnom uglu ekrana kliknite na dugme prijavi se i zatim se ulogujete."),

        ("Zašto mi nalog nije verifikovan?",
         "Budite strpljivi, referenti imaju mnogo zahteva koje treba da obrade u toku trajanja konkursa."),

        ("Šta ako unesem pogrešne podatke?",
         "Referent će proveriti unete podatke i ako nisu tačni, nećete moći da se prijavite na konkurs."),

        ("Kako da izmenim svoje podatke (profil)?",
         "U delu profil, ispod forme za pregled podataka treba uključiti mod za promenu tih podataka (klik na dugme izmeni)."),

        ("Zašto mi trebaju finansijski podaci?",
         "Finansijski podaci su neophodni jer se boduju jednim bodom ako student ima prosek prihoda do 50% prosečne zarade u Srbiji. U suprotnom, dobija se 0 bodova."),

        ("Kako da se prijavim na konkurs za smeštaj?",
         "Prvo je potrebno napraviti nalog u sistemu, popuniti profil i finansijske podatke. Zatim treba sačekati da referent verifikuje profil i finansijski status. Na kraju je potrebno prijaviti se na odgovarajući konkurs (brucoši se prijavljuju na konkurs za brucoše) i dostaviti dokument sa fakulteta ili srednje škole. Nakon potvrde referenta, student je uspešno konkurisao za studentski dom."),

        ("Koji dokumenti su potrebni za prijavu?",
         "Ako je student brucoš, potreban je dokument koji dokazuje prosek u srednjoj školi. Za starije studente je potrebna potvrda sa fakulteta."),

        ("Koja je razlika između prijave za brucoše i starije studente?",
         "Brucoši se prijavljuju na konkurs za brucoše i podnose dokument koji dokazuje prosek iz srednje škole. Stariji studenti se prijavljuju na konkurs za starije studente i podnose potvrdu sa fakulteta."),

        ("Do kada traje konkurs i kada saznajem rezultate?",
         "Zavisi od godine, ali generalno do sredine septembra traje prijava za brucoše, za starije do sredine novembra."),

        ("Koji kriterijumi utiču na broj bodova?",
         "Brucoši: prosek iz srednje škole i finansijski status. Stariji studenti: broj godina studiranja, prosek (sa fakulteta), ESPB, finansijski status, dodatni bodovi (više od 120 ESPB)."),

        ("Zašto nisam uspeo da dobijem dom?",
         "Ne postoji prag prolaznosti, sve zavisi od broja prijavljenih i broja mesta u domu."),

        ("Kako mogu da vidim u kojoj sam sobi i ko mi je cimer/ka?",
         "Kada se ulogujete, u odeljku Soba možete videti sve podatke o sobi, uključujući i cimere."),

        ("Da li mogu da zamenim sobu?",
         "Naravno, u odeljku Soba možete zameniti sobu. Potrebno je uneti indeks, ime, prezime drugog studenta i njegovu sobu, a zatim tom studentu stiže zahtev. Potrebno je da student prihvati zahtev i da referent potvrdi zamenu soba."),
    };

    public static async Task SeedKnowledgeBaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var embeddingService = scope.ServiceProvider.GetRequiredService<IEmbeddingService>();

        // Ne ubacuj ponovo ako je baza već popunjena
        if (await context.KnowledgeBaseEntries.AnyAsync())
            return;

        foreach (var (question, answer) in Entries)
        {
            var embeddingArray = await embeddingService.GenerateEmbeddingAsync(question);

            context.KnowledgeBaseEntries.Add(new KnowledgeBaseEntry
            {
                Question = question,
                Answer = answer,
                Embedding = new Vector(embeddingArray)
            });
        }

        await context.SaveChangesAsync();
    }
}