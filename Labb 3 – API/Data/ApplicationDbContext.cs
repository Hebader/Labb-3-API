using Labb_3___API.Models;
using Microsoft.EntityFrameworkCore;

namespace Labb_3___API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        public DbSet<Person> Personer { get; set; }
        public DbSet<Intresse> Intressen { get; set; }
        public DbSet<Länk> Länkar { get; set; }
        public DbSet<IntressePerson> Intresset { get; set; }
       
    }
}
