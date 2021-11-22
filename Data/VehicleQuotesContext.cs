using Microsoft.EntityFrameworkCore;
using VehicleQuotes.Models;

namespace VehicleQuotes 
{
    public class VehicleQuotesContext : DbContext
    {
        public VehicleQuotesContext (DbContextOptions<VehicleQuotesContext> options) : base(options)
        {
            
        }
        public DbSet<Make> Makes {get;set;}
        public DbSet<Size> Sizes {get;set;}
        public DbSet<BodyType> BodyTypes {get;set;}
        public DbSet<Model> Models {get;set;}
        public DbSet<ModelStyle> ModelStyles {get;set;}
        public DbSet<ModelStyleYear> ModelStyleYears {get;set;}
    }
}