using Microsoft.EntityFrameworkCore;
using VAA.EAPI.Microservices.FlightOperations.Catering.Infrastructure.Models.Entity;

namespace VAA.EAPI.Microservices.FlightOperations.Catering.Services
{
    public class PopsDataContext : DbContext
    {
        public PopsDataContext(DbContextOptions<PopsDataContext> options)
            : base(options)
        {
        }

        public virtual DbSet<MealOrders> MealOrders { get; set; }

        public virtual DbSet<Menu> Menus { get; set; }

        public virtual DbSet<PreselectOffer> PreSelectOffers { get; set; }

        public DbSet<Flight> Flights { get; set; }

        public DbSet<FlightLeg> FlightLegs { get; set; }

        public DbSet<PreselectAudit> PreselectAudits { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure MealOrder entity
            modelBuilder.Entity<MealOrders>(entity =>
            {
                entity.ToTable("MealOrders", "POPS");

                entity.HasKey(e => e.MealOrderID);

                entity.Property(x => x.MealOrderCode)
                 .HasMaxLength(50)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.MealOrderID)
                 .UseIdentityColumn(1, 1)
                 .IsRequired();

                entity.Property(x => x.OrderID)
                 .HasMaxLength(20)
                 .IsUnicode(false);

                entity.Property(x => x.FlightNumber)
                 .HasMaxLength(10)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.RecordLocator)
                 .HasMaxLength(10)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.PaxId)
                 .HasMaxLength(10)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.MealCode)
                 .HasMaxLength(30)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.OrderStatus)
                 .HasMaxLength(10)
                 .IsUnicode(false)
                 .IsRequired();

                entity.Property(x => x.FlightDate)
                 .HasColumnType("datetime");

                entity.Property(x => x.ModifiedOn)
                 .HasColumnType("datetimeoffset(0)")
                 .IsRequired();

                entity.HasIndex(x => new { x.RecordLocator, x.FlightNumber, x.PaxId })
                .HasDatabaseName("IX_MealOrders_Locator_Flight_Pax");

            });


            // Configure Menus entity
            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("Menus", "POPS");

                entity.HasKey(e => e.Id);

                entity.Property(e => e.Name).HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(300);
                entity.Property(e => e.Type).HasMaxLength(20);
                entity.Property(e => e.User).HasMaxLength(30);
                entity.Property(e => e.Nutrition).HasMaxLength(4000);
                entity.Property(e => e.Allergies).HasMaxLength(4000);
                entity.Property(e => e.Meal_Short_Name).HasMaxLength(10);
                entity.Property(e => e.Meal_Type).HasMaxLength(30);
                entity.Property(e => e.Colour).HasMaxLength(30);
            });

            // Configure Preselect_Offer entity
            modelBuilder.Entity<PreselectOffer>(entity =>
            {
                entity.ToTable("Preselect_Offer", "POPS");

                // Composite Key
                entity.HasKey(e => new
                {
                    e.Menus_Id,
                    e.Meal_Code,
                    e.Flight_Codes_Airline,
                    e.Flight_Codes_Number,
                    e.Flight_Codes_Suffix,
                    e.Cabin_Code,
                    e.Display_Sequence,
                    e.Offer_Effective_From,
                    e.Offer_Effective_To
                });

                // Relationships
                entity.HasOne(e => e.Menu)
                      .WithMany()
                      .HasForeignKey(e => e.Menus_Id);

                // Property configurations
                entity.Property(e => e.Meal_Code).HasMaxLength(5);
                entity.Property(e => e.Flight_Codes_Airline).HasMaxLength(2);
                entity.Property(e => e.Flight_Codes_Number).HasMaxLength(4);
                entity.Property(e => e.Flight_Codes_Suffix).HasMaxLength(2);
                entity.Property(e => e.Cabin_Code).HasMaxLength(20);
                entity.Property(e => e.User).HasMaxLength(30);
                entity.Property(e => e.Action_By).HasMaxLength(30);
            });

            //Configure Flight
            modelBuilder.Entity<Flight>(builder =>
            {

                builder.ToTable("Flights", "EODS");

                builder.HasKey(f => f.FlightId);

                builder.Property(f => f.FlightId)
                    .HasColumnName("Flight_Id")
                    .IsRequired();

                builder.Property(f => f.Aircraft)
                    .HasColumnName("Aircraft");

                builder.Property(f => f.AircraftRegistration)
                    .HasColumnName("Aircraft_Registration")
                    .HasMaxLength(6);

                builder.Property(f => f.FlightDate)
                    .HasColumnName("Flight_Date")
                    .HasColumnType("date");

                builder.Property(f => f.ScheDepTimestampLoc)
                    .HasColumnName("Sche_Dep_Timestamp_Loc")
                    .IsRequired();

                builder.Property(f => f.OperatingAirline)
                    .HasColumnName("Operating_Airline")
                    .HasMaxLength(2)
                    .IsRequired();

                builder.Property(f => f.OperatingFlightNum)
                    .HasColumnName("Operating_Flight_Num")
                    .HasMaxLength(4)
                    .IsRequired();

                builder.Property(f => f.OperatingFlightCodeSuffix)
                    .HasColumnName("Operating_Flight_Code_Suffix")
                    .HasMaxLength(2)
                    .IsRequired();

                builder.Property(f => f.ScheDepTimestampUtc)
                    .HasColumnName("Sche_Dep_Timestamp_UTC")
                    .IsRequired();

                builder.Property(f => f.DepTimestampLoc)
                    .HasColumnName("Dep_Timestamp_Loc");

                builder.Property(f => f.Origin)
                    .HasColumnName("Origin")
                    .HasMaxLength(3)
                    .IsRequired();

                builder.Property(f => f.Destination)
                    .HasColumnName("Destination")
                    .HasMaxLength(3)
                    .IsRequired();

                builder.Property(f => f.OriginTerminal)
                    .HasColumnName("Origin_Terminal")
                    .HasMaxLength(100);

                builder.Property(f => f.DepTimestampUtc)
                    .HasColumnName("Dep_Timestamp_UTC");

                builder.Property(f => f.DestinationTerminal)
                    .HasColumnName("Destination_Terminal")
                    .HasMaxLength(100);

                builder.Property(f => f.ScheArrTimestampLoc)
                    .HasColumnName("Sche_Arr_Timestamp_Loc")
                    .IsRequired();

                builder.Property(f => f.ScheArrTimestampUtc)
                    .HasColumnName("Sche_Arr_Timestamp_UTC")
                    .IsRequired();

                builder.Property(f => f.ArrTimestampLoc)
                    .HasColumnName("Arr_Timestamp_Loc");

                builder.Property(f => f.ArrTimestampUtc)
                    .HasColumnName("Arr_Timestamp_UTC");

                builder.Property(f => f.Status)
                    .HasColumnName("Status")
                    .HasMaxLength(20);

                builder.Property(f => f.Priority)
                    .HasColumnName("Priority")
                    .HasMaxLength(20);

                builder.Property(f => f.TotalPaxCapacity)
                    .HasColumnName("Total_Pax_Capacity");

                builder.Property(f => f.NoPaxBooked)
                    .HasColumnName("No_Pax_Booked");

                builder.Property(f => f.NoStaffPax)
                    .HasColumnName("No_Staff_Pax");

                builder.Property(f => f.NoStaffCompanionPax)
                    .HasColumnName("No_Staff_Companion_Pax");

                builder.Property(f => f.NoPaxWithConnectFlt)
                    .HasColumnName("No_Pax_with_Connect_Flt");

                builder.Property(f => f.NoDeportee)
                    .HasColumnName("No_Deportee");

                builder.Property(f => f.NoUnaccompaniedMinor)
                    .HasColumnName("No_Unaccompanied_Minor");

                builder.Property(f => f.NoPreferentialPax)
                    .HasColumnName("No_Preferential_Pax");

                builder.Property(f => f.NoInfantPax)
                    .HasColumnName("No_Infant_Pax");
            });

            //Configure FlightLegs            
            modelBuilder.Entity<FlightLeg>(builder =>
            {

                builder.ToTable("Flight_Legs", "EODS");

                // PK (IDENTITY)
                builder.HasKey(x => x.Leg);
                builder.Property(x => x.Leg)
                    .HasColumnName("Leg")
                    .UseIdentityColumn();

                // FK to Flights
                builder.Property(x => x.FlightId)
                    .HasColumnName("Flight")
                    .IsRequired();

                // Columns
                builder.Property(x => x.FlightDate)
                    .HasColumnName("Flight_Date")
                    .HasColumnType("date");

                builder.Property(x => x.SeqNo)
                    .HasColumnName("Seq_No");

                builder.Property(x => x.Origin)
                    .HasColumnName("Origin")
                    .HasMaxLength(3)
                    .IsRequired();

                builder.Property(x => x.Destination)
                    .HasColumnName("Destination")
                    .HasMaxLength(3)
                    .IsRequired();

                builder.Property(x => x.Aircraft)
                    .HasColumnName("Aircraft");

                builder.Property(x => x.AircraftRegistration)
                    .HasColumnName("Aircraft_Registration")
                    .HasMaxLength(6);

                builder.Property(x => x.ScheDepTimestampLoc)
                    .HasColumnName("Sche_Dep_Timestamp_Loc")
                    .IsRequired();

                builder.Property(x => x.ScheDepTimestampUtc)
                    .HasColumnName("Sche_Dep_Timestamp_UTC")
                    .IsRequired();

                builder.Property(x => x.DepTimestampLoc)
                    .HasColumnName("Dep_Timestamp_Loc");

                builder.Property(x => x.DepTimestampUtc)
                    .HasColumnName("Dep_Timestamp_UTC");

                builder.Property(x => x.ScheArrTimestampLoc)
                    .HasColumnName("Sche_Arr_Timestamp_Loc")
                    .IsRequired();

                builder.Property(x => x.ScheArrTimestampUtc)
                    .HasColumnName("Sche_Arr_Timestamp_UTC")
                    .IsRequired();

                builder.Property(x => x.ArrTimestampLoc)
                    .HasColumnName("Arr_Timestamp_Loc");

                builder.Property(x => x.ArrTimestampUtc)
                    .HasColumnName("Arr_Timestamp_UTC");

                builder.Property(x => x.Status)
                    .HasColumnName("Status")
                    .HasMaxLength(20);

                builder.Property(x => x.OriginTerminal)
                    .HasColumnName("Origin_Terminal")
                    .HasMaxLength(100);

                builder.Property(x => x.DestinationTerminal)
                    .HasColumnName("Destination_Terminal")
                    .HasMaxLength(100);

                builder.Property(x => x.TotalPaxCapacity)
                    .HasColumnName("Total_Pax_Capacity");

                builder.Property(x => x.NoPaxBooked)
                    .HasColumnName("No_Pax_Booked");

                builder.Property(x => x.NoStaffPax)
                    .HasColumnName("No_Staff_Pax");

                builder.Property(x => x.NoStaffCompanionPax)
                    .HasColumnName("No_Staff_Companion_Pax");

                builder.Property(x => x.NoPaxWithConnectFlight)
                    .HasColumnName("No_Pax_with_Connect_Flight");

                builder.Property(x => x.NoDeportee)
                    .HasColumnName("No_Deportee");

                builder.Property(x => x.NoUnaccompaniedMinor)
                    .HasColumnName("No_Unaccompanied_Minor");

                builder.Property(x => x.NoPreferentialPax)
                    .HasColumnName("No_Preferential_Pax");

                builder.Property(x => x.NoInfantPax)
                    .HasColumnName("No_Infant_Pax");

                builder.Property(x => x.Priority)
                    .HasColumnName("Priority")
                    .HasMaxLength(20);

                // Relationship: FlightLeg (many) → Flight (one)
                builder.HasOne(x => x.Flight)
                    .WithMany(f => f.FlightLegs)
                    .HasForeignKey(x => x.FlightId)
                    .OnDelete(DeleteBehavior.Restrict)               // match typical DB NO ACTION
                    .HasConstraintName("Flt_Legs_Flights_FK");      // exact FK name

            });

            //Configure Preselect_Audit
            modelBuilder.Entity<PreselectAudit>(builder =>
            {

                builder.ToTable("Preselect_Audit", "PSODS");

                // Composite PK (order matches your PK definition)
                builder.HasKey(x => new
                {
                    x.FlightCodesAirline,
                    x.FlightCodesSuffix,
                    x.FlightCodesNumber,
                    x.BookingReference
                });

                // Key columns
                builder.Property(x => x.FlightCodesAirline)
                    .HasColumnName("Flight_Codes_Airline")
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(x => x.FlightCodesSuffix)
                    .HasColumnName("Flight_Codes_Suffix")
                    .HasMaxLength(2)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(x => x.FlightCodesNumber)
                    .HasColumnName("Flight_Codes_Number")
                    .HasMaxLength(4)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(x => x.BookingReference)
                    .HasColumnName("Booking_Reference")
                    .HasMaxLength(6)
                    .IsUnicode(false)
                    .IsRequired();

                // Other columns
                builder.Property(x => x.TransactionId)
                    .HasColumnName("Transaction_Id")
                    .HasMaxLength(40)
                    .IsUnicode(false);

                builder.Property(x => x.Status)
                    .HasColumnName("Status")
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .IsRequired();

                builder.Property(x => x.OfferDateTime)
                    .HasColumnName("Offer_DateTime");

                builder.Property(x => x.ReminderDateTime)
                    .HasColumnName("Reminder_DateTime");

                builder.Property(x => x.OrderDateTime)
                    .HasColumnName("Order_DateTime");

                builder.Property(x => x.Created)
                    .HasColumnName("Created");

                builder.Property(x => x.Modified)
                    .HasColumnName("Modified");

                builder.Property(x => x.User)
                    .HasColumnName("User")
                    .HasMaxLength(30)
                    .IsUnicode(false);

                builder.Property(x => x.Referrer)
                    .HasColumnName("Referrer")
                    .HasMaxLength(50)     
                    .IsUnicode(true);

                builder.Property(x => x.FlightDate)
                    .HasColumnName("Flight_Date");

                builder.Property(x => x.PromptedDateTime)
                    .HasColumnName("Prompted_DateTime");

            });
        }
    }
}

