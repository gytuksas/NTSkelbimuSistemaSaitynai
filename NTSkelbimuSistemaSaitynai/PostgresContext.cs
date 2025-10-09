using Microsoft.EntityFrameworkCore;
using NTSkelbimuSistemaSaitynai.DbUtils;
using NTSkelbimuSistemaSaitynai.Models;

namespace NTSkelbimuSistemaSaitynai;

public partial class PostgresContext : DbContext
{
    public PostgresContext()
    {
    }

    public PostgresContext(DbContextOptions<PostgresContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Administrator> Administrators { get; set; }

    public virtual DbSet<Apartment> Apartments { get; set; }

    public virtual DbSet<Availability> Availabilities { get; set; }

    public virtual DbSet<Broker> Brokers { get; set; }

    public virtual DbSet<Building> Buildings { get; set; }

    public virtual DbSet<Buyer> Buyers { get; set; }

    public virtual DbSet<Confirmation> Confirmations { get; set; }

    public virtual DbSet<Energyclass> Energyclasses { get; set; }

    public virtual DbSet<Finishtype> Finishtypes { get; set; }

    public virtual DbSet<Heatingtype> Heatingtypes { get; set; }

    public virtual DbSet<Listing> Listings { get; set; }

    public virtual DbSet<Picture> Pictures { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Viewing> Viewings { get; set; }

    public virtual DbSet<Viewingstatus> Viewingstatuses { get; set; }

    string connString = new DbConnection().GetConnectionString();
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql(connString);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Administrator>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("administrator_pkey");

            entity.ToTable("administrator");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("id_user");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Administrator)
                .HasForeignKey<Administrator>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("administrator_id_user_fkey");
        });

        modelBuilder.Entity<Apartment>(entity =>
        {
            entity.HasKey(e => e.IdApartment).HasName("apartment_pkey");

            entity.ToTable("apartment");

            entity.Property(e => e.IdApartment)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_apartment");
            entity.Property(e => e.Apartmentnumber).HasColumnName("apartmentnumber");
            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.Finish).HasColumnName("finish");
            entity.Property(e => e.FkBuildingidBuilding).HasColumnName("fk_buildingid_building");
            entity.Property(e => e.Floor).HasColumnName("floor");
            entity.Property(e => e.Heating).HasColumnName("heating");
            entity.Property(e => e.Iswholebuilding).HasColumnName("iswholebuilding");
            entity.Property(e => e.Notes)
                .HasMaxLength(10000)
                .HasColumnName("notes");
            entity.Property(e => e.Rooms).HasColumnName("rooms");

            entity.HasOne(d => d.FinishNavigation).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.Finish)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("apartment_finish_fkey");

            entity.HasOne(d => d.FkBuildingidBuildingNavigation).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.FkBuildingidBuilding)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("apartment_fk_buildingid_building_fkey");

            entity.HasOne(d => d.HeatingNavigation).WithMany(p => p.Apartments)
                .HasForeignKey(d => d.Heating)
                .HasConstraintName("apartment_heating_fkey");
        });

        modelBuilder.Entity<Availability>(entity =>
        {
            entity.HasKey(e => e.IdAvailability).HasName("availability_pkey");

            entity.ToTable("availability");

            entity.Property(e => e.IdAvailability)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_availability");
            entity.Property(e => e.FkBrokeridUser).HasColumnName("fk_brokerid_user");
            entity.Property(e => e.From).HasColumnName("from");
            entity.Property(e => e.To).HasColumnName("to");

            entity.HasOne(d => d.FkBrokeridUserNavigation).WithMany(p => p.Availabilities)
                .HasForeignKey(d => d.FkBrokeridUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("availability_fk_brokerid_user_fkey");
        });

        modelBuilder.Entity<Broker>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("broker_pkey");

            entity.ToTable("broker");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("id_user");
            entity.Property(e => e.Blocked).HasColumnName("blocked");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Broker)
                .HasForeignKey<Broker>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("broker_id_user_fkey");
        });

        modelBuilder.Entity<Building>(entity =>
        {
            entity.HasKey(e => e.IdBuilding).HasName("building_pkey");

            entity.ToTable("building");

            entity.Property(e => e.IdBuilding)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_building");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Area).HasColumnName("area");
            entity.Property(e => e.City)
                .HasMaxLength(255)
                .HasColumnName("city");
            entity.Property(e => e.Energy).HasColumnName("energy");
            entity.Property(e => e.FkBrokeridUser).HasColumnName("fk_brokerid_user");
            entity.Property(e => e.Floors).HasColumnName("floors");
            entity.Property(e => e.Lastrenovationyear).HasColumnName("lastrenovationyear");
            entity.Property(e => e.Year).HasColumnName("year");

            entity.HasOne(d => d.EnergyNavigation).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.Energy)
                .HasConstraintName("building_energy_fkey");

            entity.HasOne(d => d.FkBrokeridUserNavigation).WithMany(p => p.Buildings)
                .HasForeignKey(d => d.FkBrokeridUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("building_fk_brokerid_user_fkey");
        });

        modelBuilder.Entity<Buyer>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("buyer_pkey");

            entity.ToTable("buyer");

            entity.Property(e => e.IdUser)
                .ValueGeneratedNever()
                .HasColumnName("id_user");
            entity.Property(e => e.Blocked).HasColumnName("blocked");
            entity.Property(e => e.Confirmed).HasColumnName("confirmed");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Buyer)
                .HasForeignKey<Buyer>(d => d.IdUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("buyer_id_user_fkey");
        });

        modelBuilder.Entity<Confirmation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("confirmation_pkey");

            entity.ToTable("confirmation");

            entity.HasIndex(e => e.FkBuyeridUser, "confirmation_fk_buyerid_user_key").IsUnique();

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.Expires).HasColumnName("expires");
            entity.Property(e => e.FkBuyeridUser).HasColumnName("fk_buyerid_user");

            entity.HasOne(d => d.FkBuyeridUserNavigation).WithOne(p => p.Confirmation)
                .HasForeignKey<Confirmation>(d => d.FkBuyeridUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("confirmation_fk_buyerid_user_fkey");
        });

        modelBuilder.Entity<Energyclass>(entity =>
        {
            entity.HasKey(e => e.IdEnergyclass).HasName("energyclass_pkey");

            entity.ToTable("energyclass");

            entity.Property(e => e.IdEnergyclass)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_energyclass");
            entity.Property(e => e.Name)
                .HasMaxLength(4)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Finishtype>(entity =>
        {
            entity.HasKey(e => e.IdFinishtypes).HasName("finishtypes_pkey");

            entity.ToTable("finishtypes");

            entity.Property(e => e.IdFinishtypes)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_finishtypes");
            entity.Property(e => e.Name)
                .HasMaxLength(15)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Heatingtype>(entity =>
        {
            entity.HasKey(e => e.IdHeatingtypes).HasName("heatingtypes_pkey");

            entity.ToTable("heatingtypes");

            entity.Property(e => e.IdHeatingtypes)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_heatingtypes");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        modelBuilder.Entity<Listing>(entity =>
        {
            entity.HasKey(e => e.IdListing).HasName("listing_pkey");

            entity.ToTable("listing");

            entity.HasIndex(e => e.FkPictureid, "listing_fk_pictureid_key").IsUnique();

            entity.Property(e => e.IdListing)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_listing");
            entity.Property(e => e.Askingprice).HasColumnName("askingprice");
            entity.Property(e => e.Description)
                .HasMaxLength(10000)
                .HasColumnName("description");
            entity.Property(e => e.FkPictureid)
                .HasMaxLength(255)
                .HasColumnName("fk_pictureid");
            entity.Property(e => e.Rent).HasColumnName("rent");

            entity.HasOne(d => d.FkPicture).WithOne(p => p.Listing)
                .HasForeignKey<Listing>(d => d.FkPictureid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("listing_fk_pictureid_fkey");
        });

        modelBuilder.Entity<Picture>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("picture_pkey");

            entity.ToTable("picture");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.FkApartmentidApartment).HasColumnName("fk_apartmentid_apartment");
            entity.Property(e => e.Public).HasColumnName("public");

            entity.HasOne(d => d.FkApartmentidApartmentNavigation).WithMany(p => p.Pictures)
                .HasForeignKey(d => d.FkApartmentidApartment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("picture_fk_apartmentid_apartment_fkey");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Session_pkey");

            entity.ToTable("Session");

            entity.Property(e => e.Id)
                .HasMaxLength(255)
                .HasColumnName("id");
            entity.Property(e => e.Created).HasColumnName("created");
            entity.Property(e => e.FkUseridUser).HasColumnName("fk_userid_user");
            entity.Property(e => e.Lastactivity).HasColumnName("lastactivity");
            entity.Property(e => e.Remember).HasColumnName("remember");

            entity.HasOne(d => d.FkUseridUserNavigation).WithMany(p => p.Sessions)
                .HasForeignKey(d => d.FkUseridUser)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Session_fk_userid_user_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("User_pkey");

            entity.ToTable("User");

            entity.Property(e => e.IdUser)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_user");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasColumnName("password");
            entity.Property(e => e.Phone)
                .HasMaxLength(255)
                .HasColumnName("phone");
            entity.Property(e => e.Profilepicture)
                .HasMaxLength(255)
                .HasColumnName("profilepicture");
            entity.Property(e => e.Registrationtime).HasColumnName("registrationtime");
            entity.Property(e => e.Surname)
                .HasMaxLength(255)
                .HasColumnName("surname");
        });

        modelBuilder.Entity<Viewing>(entity =>
        {
            entity.HasKey(e => e.IdViewing).HasName("viewing_pkey");

            entity.ToTable("viewing");

            entity.HasIndex(e => e.FkListingidListing, "viewing_fk_listingid_listing_key").IsUnique();

            entity.Property(e => e.IdViewing)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_viewing");
            entity.Property(e => e.FkAvailabilityidAvailability).HasColumnName("fk_availabilityid_availability");
            entity.Property(e => e.FkListingidListing).HasColumnName("fk_listingid_listing");
            entity.Property(e => e.From).HasColumnName("from");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.To).HasColumnName("to");

            entity.HasOne(d => d.FkAvailabilityidAvailabilityNavigation).WithMany(p => p.Viewings)
                .HasForeignKey(d => d.FkAvailabilityidAvailability)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("viewing_fk_availabilityid_availability_fkey");

            entity.HasOne(d => d.FkListingidListingNavigation).WithOne(p => p.Viewing)
                .HasForeignKey<Viewing>(d => d.FkListingidListing)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("viewing_fk_listingid_listing_fkey");

            entity.HasOne(d => d.StatusNavigation).WithMany(p => p.Viewings)
                .HasForeignKey(d => d.Status)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("viewing_status_fkey");
        });

        modelBuilder.Entity<Viewingstatus>(entity =>
        {
            entity.HasKey(e => e.IdViewingstatus).HasName("viewingstatus_pkey");

            entity.ToTable("viewingstatus");

            entity.Property(e => e.IdViewingstatus)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_viewingstatus");
            entity.Property(e => e.Name)
                .HasMaxLength(20)
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
