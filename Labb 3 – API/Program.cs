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
                // Inkludera intresse-listan f�r varje person
                var personer = await context.Personer
                    .Include(p => p.Intresset) // Inkludera intressen f�r varje person
                    .ThenInclude(ip => ip.Intresse) // Inkludera intresse-data f�r varje intresse-person relation
                    .Select(p => new
                    {
                        p.PersonId,
                        p.PersonNamn,
                        p.Telefonnummer,
                        // Lista med titlar fr�n personens intresse-lista
                        IntresseTitlar = p.Intresset.Select(ip => ip.Intresse.Titel)
                    })
                    .ToListAsync();

                // Returnera personer med intresse-titlar
                return Results.Ok(personer);
            });


            // 2. H�mta alla intressen som �r kopplade till en specifik person
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

            // H�mta alla l�nkar som �r kopplade till en specifik person
            app.MapGet("/api/personer/{personId:int}/", async (int personId, ApplicationDbContext context) =>
            {
                // H�mta personens intressen
                var person = await context.Personer
                    .Include(p => p.Intresset)
                        .ThenInclude(ip => ip.Intresse)
                    .FirstOrDefaultAsync(p => p.PersonId == personId);

                // Kontrollera om personen inte hittades
                if (person == null)
                {
                    return Results.NotFound("Personen hittades inte.");
                }

                // H�mta alla l�nkar kopplade till personens intressen
                var l�nkar = new List<L�nk>();
                foreach (var intressePerson in person.Intresset)
                {
                    var intresseL�nkar = await context.L�nkar
                        .Where(l => l.FkIntresseId == intressePerson.FkIntresseId)
                        .ToListAsync();
                    l�nkar.AddRange(intresseL�nkar);
                }

                return Results.Ok(l�nkar);
            });

            //Koppla intresse till en person

            app.MapPost("/api/personer/{personId:int}/kopplaintresse/{intresseId:int}", async (int personId, int intresseId, ApplicationDbContext context) =>
            {
                // H�mta personen och intresset fr�n databasen
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
                    return Results.BadRequest("Personen �r redan kopplad till detta intresse.");
                }

                // Skapa en ny instans av IntressePerson f�r att koppla personen till intresset
                var nyIntressePerson = new IntressePerson
                {
                    FkPersonId = personId,
                    FkIntresseId = intresseId,
                    Person = person,
                    Intresse = intresse
                };

                // L�gg till nyIntressePerson till person.Intresset-samlingen
                person.Intresset.Add(nyIntressePerson);

                // L�gg till nyIntressePerson till intresse.Intresset-samlingen
                intresse.Intresset.Add(nyIntressePerson);

                // Spara �ndringarna i databasen
                await context.SaveChangesAsync();

                // Returnera NoContent vid lyckad operation
                return Results.NoContent();
            });


            //// L�gg till en ny l�nk f�r en specifik person och ett specifikt intresse
            //app.MapPost("/api/intressen/{intresseId:int}/lankar", async (int intresseId, [FromBody] string url, ApplicationDbContext context) =>
            //{
            //    // H�mta IntressePerson-objektet baserat p� personId och intresseId
            //    var intressePerson = await context.Intresset

            //        .FirstOrDefaultAsync(ip => ip.FkIntresseId == intresseId);

            //    // Kontrollera om IntressePerson hittades
            //    if (intressePerson == null)
            //    {
            //        return Results.NotFound("Intresset hittades inte eller �r inte kopplat till den h�r personen.");
            //    }

            //    // Skapa en ny instans av L�nk och s�tt URL samt FkIntresseId
            //    var nyL�nk = new L�nk
            //    {
            //        URL = url,
            //        FkIntresseId = intresseId
            //    };

            //    // L�gg till den nya l�nken i kontextet
            //    context.L�nkar.Add(nyL�nk);

            //    // Spara �ndringarna i databasen
            //    try
            //    {
            //        await context.SaveChangesAsync();

            //        // Returnera Created-svar med den nya l�nken
            //        return Results.Created($"/api/personer/{personId}/intressen/{intresseId}/lankar/{nyL�nk.L�nkId}", nyL�nk);
            //    }
            //    catch (Exception ex)
            //    {
            //        // Hantera fel vid sparning av �ndringar
            //        return Results.Problem("Ett fel intr�ffade vid sparning av l�nken i databasen: " + ex.Message);
            //    }
            ///
            app.MapPost("/Links/", async (L�nk link, ApplicationDbContext context) =>
            {
                context.L�nkar.Add(link);
                await context.SaveChangesAsync();
                return Results.Created($"/Links/{link.L�nkId}",link);   
         
            });


            // K�r applikationen
            app.Run();
        }
    }
}