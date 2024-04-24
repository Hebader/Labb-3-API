using Labb_3___API.Data;
using Labb_3___API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Labb_3___API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            _ = app.MapGet("/api/personer", async (ApplicationDbContext context) =>
            {
                // Inkludera intresse-listan för varje person
                var personer = await context.Personer
                    .Include(p => p.Intresset) // Inkludera intressen för varje person
                    .ThenInclude(ip => ip.Intresse) // Inkludera intresse-data för varje intresse-person relation
                    .Select(p => new
                    {
                        p.PersonId,
                        p.PersonNamn,
                        p.Telefonnummer,
                        // Lista med titlar från personens intresse-lista
                        IntresseTitlar = p.Intresset.Select(ip => ip.Intresse.Titel)
                    })
                    .ToListAsync();

                // Returnera personer med intresse-titlar
                return Results.Ok(personer);
            });


            // 2. Hämta alla intressen som är kopplade till en specifik person
            app.MapGet("/api/personer/{personId:int}/intressen", async (int personId, ApplicationDbContext context) =>
            {
                var person = await context.Personer
                    .Include(p => p.Intresset)
                    .ThenInclude(ip => ip.Intresse)
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                if (person == null)
                {
                    return Results.NotFound();
                }

                var intressen = person.Intresset.Select(ip => new
                {
                    ip.Intresse.IntresseId,
                    ip.Intresse.Titel,
                    ip.Intresse.Beskrivning
                });

                return Results.Ok(intressen);
            });

            // Hämta alla länkar som är kopplade till en specifik person
            app.MapGet("/api/personer/{personId:int}/", async (int personId, ApplicationDbContext context) =>
            {
                // Hämta personens intressen
                var person = await context.Personer
                    .Include(p => p.Intresset)
                        .ThenInclude(ip => ip.Intresse)
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                // Kontrollera om personen inte hittades
                if (person == null)
                {
                    return Results.NotFound("Personen hittades inte.");
                }

                // Hämta alla länkar kopplade till personens intressen
                var länkar = new List<Länk>();
                foreach (var intressePerson in person.Intresset)
                {
                    var intresseLänkar = await context.Länkar
                        .Where(l => l.FkIntresseId == intressePerson.FkIntresseId)
                        .ToListAsync();
                    länkar.AddRange(intresseLänkar);
                }

                return Results.Ok(länkar);
            });

            //Koppla intresse till en person

            app.MapPost("/api/personer/{personId:int}/kopplaintresse/{intresseId:int}", async (int personId, int intresseId, ApplicationDbContext context) =>
            {
                // Hämta personen och intresset från databasen
                var person = await context.Personer
                    .Include(p => p.Intresset) // Inkludera personens intresse-relationslista
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                var intresse = await context.Intressen
                    .Include(i => i.Intresset) // Inkludera intresset relationslista
                    .FirstOrDefaultAsync(i => i.IntresseId == intresseId);

                // Kontrollera om personen eller intresset inte hittades
                if (person == null || intresse == null)
                {
                    return Results.NotFound("Personen eller intresset hittades inte.");
                }

                // Kontrollera om personen redan har detta intresse kopplat
                bool alreadyLinked = person.Intresset.Any(ip => ip.FkIntresseId == intresseId);
                if (alreadyLinked)
                {
                    return Results.BadRequest("Personen är redan kopplad till detta intresse.");
                }

                // Skapa en ny instans av IntressePerson för att koppla personen till intresset
                var nyIntressePerson = new IntressePerson
                {
                    FkPersonId = personId,
                    FkIntresseId = intresseId,
                    Person = person,
                    Intresse = intresse
                };

                // Lägg till nyIntressePerson till person.Intresset-samlingen
                person.Intresset.Add(nyIntressePerson);

                // Lägg till nyIntressePerson till intresse.Intresset-samlingen
                intresse.Intresset.Add(nyIntressePerson);

                // Spara ändringarna i databasen
                await context.SaveChangesAsync();

                // Returnera NoContent vid lyckad operation
                return Results.NoContent();
            });


            //// Lägg till en ny länk för en specifik person och ett specifikt intresse
            //app.MapPost("/api/intressen/{intresseId:int}/lankar", async (int intresseId, [FromBody] string url, ApplicationDbContext context) =>
            //{
            //    // Hämta IntressePerson-objektet baserat på personId och intresseId
            //    var intressePerson = await context.Intresset

            //        .FirstOrDefaultAsync(ip => ip.FkIntresseId == intresseId);

            //    // Kontrollera om IntressePerson hittades
            //    if (intressePerson == null)
            //    {
            //        return Results.NotFound("Intresset hittades inte eller är inte kopplat till den här personen.");
            //    }

            //    // Skapa en ny instans av Länk och sätt URL samt FkIntresseId
            //    var nyLänk = new Länk
            //    {
            //        URL = url,
            //        FkIntresseId = intresseId
            //    };

            //    // Lägg till den nya länken i kontextet
            //    context.Länkar.Add(nyLänk);

            //    // Spara ändringarna i databasen
            //    try
            //    {
            //        await context.SaveChangesAsync();

            //        // Returnera Created-svar med den nya länken
            //        return Results.Created($"/api/personer/{personId}/intressen/{intresseId}/lankar/{nyLänk.LänkId}", nyLänk);
            //    }
            //    catch (Exception ex)
            //    {
            //        // Hantera fel vid sparning av ändringar
            //        return Results.Problem("Ett fel inträffade vid sparning av länken i databasen: " + ex.Message);
            //    }
            ///
            app.MapPost("/Links/", async (Länk link, ApplicationDbContext context) =>
            {
                context.Länkar.Add(link);
                await context.SaveChangesAsync();
                return Results.Created($"/Links/{link.LänkId}",link);   
         
            });


            // Kör applikationen
            app.Run();
        }
    }
}