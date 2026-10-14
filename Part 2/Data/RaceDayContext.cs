using Microsoft.EntityFrameworkCore;
using Part_2.Models;
namespace Part_2.Data
{//start of namespace
    public class RaceDayContext : DbContext
    {//start of class
        public RaceDayContext(DbContextOptions<RaceDayContext> options) : base (options) 
        { 
        }
        public DbSet<EventOrganiser> EventOrganiser => Set<EventOrganiser>();

        public DbSet<Participant> Participants => Set<Participant>();

        public DbSet<Category> Categories => Set<Category>();

       public DbSet<Route> Routes => Set<Route>();

       public DbSet<RaceEvent> RaceEvents => Set<RaceEvent>();

        public DbSet<Registration> Registrations => Set<Registration>();

        public DbSet<UserAccount> UserAccounts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EventOrganiser>().ToTable("EventOrganiser").HasKey(x => x.OrganizerID);
            modelBuilder.Entity<EventOrganiser>().HasIndex(x => x.OrganizerEmail).IsUnique();


            modelBuilder.Entity<Participant>().ToTable("Participant").HasKey(x => x.ParticipantID);
            modelBuilder.Entity<Participant>().HasIndex(x => x.ParticipantsEmail).IsUnique();

            modelBuilder.Entity<Category>().ToTable("Category").HasKey(x => x.CategoryID);
            modelBuilder.Entity<Category>().HasIndex(x => x.CategoryName).IsUnique();

            modelBuilder.Entity<RaceRoute>().ToTable("Route").HasKey(x => x.RouteId);
            modelBuilder.Entity<RaceRoute>().HasIndex(x => x.RouteName).IsUnique();

            modelBuilder.Entity<RaceRoute>().Property(x => x.DistanceKm).HasPrecision(6, 2);

            modelBuilder.Entity<RaceEvent>().ToTable("Event").HasKey(x => x.EventID);
            modelBuilder.Entity<RaceEvent>().Property(x => x.EntryFee).HasPrecision(10, 2);



            modelBuilder.Entity<RaceEvent>().HasOne<EventOrganiser>().WithMany().HasForeignKey(x => x.OrganizerID);

            modelBuilder.Entity<RaceEvent>().HasOne<Category>().WithMany().HasForeignKey(x => x.CategoryID);

            modelBuilder.Entity<RaceEvent>().HasOne<RaceRoute>().WithMany().HasForeignKey(x => x.RouteID);

            modelBuilder.Entity<Registration>().ToTable("Registration").HasKey(x => x.RegistrationID);

            modelBuilder.Entity<Registration>().Property(x => x.RegistrationDate).HasDefaultValueSql("Get the Date()");

            modelBuilder.Entity<Registration>().Property(x => x.AveragePace).HasPrecision(5, 2);

            modelBuilder.Entity<Registration>().HasOne<RaceEvent>().WithMany().HasForeignKey(x => x.EventID);

            modelBuilder.Entity<Registration>().HasOne<Participant>().WithMany().HasForeignKey(x => x.ParticipantID);

           modelBuilder.Entity<Registration>().HasIndex(x => new {x.EventID, x.ParticipantID}).IsUnique();

      }
    }//end of class
}//end of names
