using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PersonalCabinet.Models;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Application> Applications { get; set; }

    public virtual DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }

    public virtual DbSet<Document> Documents { get; set; }

    public virtual DbSet<DocumentType> DocumentTypes { get; set; }

    public virtual DbSet<IndividualProfile> IndividualProfiles { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<OrganizationProfile> OrganizationProfiles { get; set; }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<UserPersonalData> UserPersonalData { get; set; }
    public override int SaveChanges()
    {
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Application>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("applications_pkey");
            entity.ToTable("applications");

            entity.HasIndex(e => e.ApplicationNumber, "applications_application_number_key").IsUnique();
            entity.HasIndex(e => e.UserId, "idx_applications_user_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationNumber)
                .HasMaxLength(50)
                .HasColumnName("application_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ExtraData)
                .HasColumnType("jsonb")
                .HasColumnName("extra_data");
            entity.Property(e => e.ObjectAddress).HasColumnName("object_address");
            entity.Property(e => e.RequestedPower)
                .HasColumnName("requested_power");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'NEW'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.DesignDeadline).HasColumnType("date");
            entity.Property(e => e.SubmittedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("submitted_at");
            entity.Property(e => e.Title).HasColumnName("title");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.EnergyDeviceName).HasColumnName("EnergyDeviceName");
            entity.Property(e => e.DeviceAddress).HasColumnName("DeviceAddress");
            entity.Property(e => e.PreviousPowerKw).HasColumnName("PreviousPowerKw"); 
            entity.Property(e => e.TotalPowerKw).HasColumnName("TotalPowerKw");     
            entity.Property(e => e.ReliabilityCategory).HasColumnName("ReliabilityCategory");
            entity.Property(e => e.ApplicationReason).HasColumnName("ApplicationReason");
            entity.Property(e => e.LastName).HasColumnName("LastName");
            entity.Property(e => e.FirstName).HasColumnName("FirstName");
            entity.Property(e => e.MiddleName).HasColumnName("MiddleName");
            entity.Property(e => e.ResidenceAddress).HasColumnName("ResidenceAddress");
            entity.Property(e => e.Phone).HasColumnName("Phone");
            entity.Property(e => e.Inn).HasColumnName("Inn");
            entity.Property(e => e.PassportSeries).HasColumnName("PassportSeries");
            entity.Property(e => e.PassportNumber).HasColumnName("PassportNumber");
            entity.Property(e => e.PassportDate).HasColumnType("date");
            entity.Property(e => e.SNILS).HasColumnName("SNILS");
            entity.Property(e => e.DateSNILS).HasColumnType("date");  
            entity.Property(e => e.PassportWhoIssued).HasColumnName("PassportWhoIssued");
            entity.Property(e => e.AddressRegistr).HasColumnName("AddressRegistr");
            entity.Property(e => e.PaymentPlan).HasColumnName("PaymentPlan");
            entity.Property(e => e.GuarantyingSupplier).HasColumnName("GuarantyingSupplier");
            entity.Property(e => e.OrganizationFullName).HasColumnName("OrganizationFullName");
            entity.Property(e => e.OrganizationShortName).HasColumnName("OrganizationShortName");
            entity.Property(e => e.ContactPerson).HasColumnName("ContactPerson");
            entity.Property(e => e.ApplicantType).HasColumnName("ApplicantType");

            entity.HasOne(d => d.User).WithMany(p => p.Applications)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("applications_user_id_fkey");
        });
        modelBuilder.Entity<UserPersonalData>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId).IsUnique();
            entity.Property(e => e.ResidenceAddress).HasMaxLength(200);
            entity.Property(e => e.Inn).HasMaxLength(12);
            entity.Property(e => e.PassportSeries).HasMaxLength(4);
            entity.Property(e => e.PassportNumber).HasMaxLength(6);
            entity.Property(e => e.PassportDate).HasColumnType("date");

            entity.HasOne(d => d.User)
                .WithOne(p => p.PersonalData)
                .HasForeignKey<UserPersonalData>(d => d.UserId);
        });
        modelBuilder.Entity<ApplicationStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("application_status_history_pkey");

            entity.ToTable("application_status_history");

            entity.HasIndex(e => e.ApplicationId, "idx_status_history_application_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.ChangedBy).HasColumnName("changed_by");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.NewStatus)
                .HasMaxLength(30)
                .HasColumnName("new_status");
            entity.Property(e => e.OldStatus)
                .HasMaxLength(30)
                .HasColumnName("old_status");

            entity.HasOne(d => d.Application).WithMany(p => p.ApplicationStatusHistories)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("application_status_history_application_id_fkey");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.ApplicationStatusHistories)
                .HasForeignKey(d => d.ChangedBy)
                .HasConstraintName("application_status_history_changed_by_fkey");
        });

        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("documents_pkey");

            entity.ToTable("documents");

            entity.HasIndex(e => e.ApplicationId, "idx_documents_application_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(e => e.FilePath).HasColumnName("file_path");
            entity.Property(e => e.MimeType)
                .HasMaxLength(100)
                .HasColumnName("mime_type");
            entity.Property(e => e.OriginalFileName).HasColumnName("original_file_name");
            entity.Property(e => e.StoredFileName).HasColumnName("stored_file_name");
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("uploaded_at");
            entity.Property(e => e.UploadedBy).HasColumnName("uploaded_by");

            entity.HasOne(d => d.Application).WithMany(p => p.Documents)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("documents_application_id_fkey");

            entity.HasOne(d => d.DocumentType).WithMany(p => p.Documents)
                .HasForeignKey(d => d.DocumentTypeId)
                .HasConstraintName("documents_document_type_id_fkey");

            entity.HasOne(d => d.UploadedByNavigation).WithMany(p => p.Documents)
                .HasForeignKey(d => d.UploadedBy)
                .HasConstraintName("documents_uploaded_by_fkey");
        });

        modelBuilder.Entity<DocumentType>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("document_types_pkey");

            entity.ToTable("document_types");

            entity.HasIndex(e => e.Code, "document_types_code_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
        });

        modelBuilder.Entity<IndividualProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("individual_profiles_pkey");

            entity.ToTable("individual_profiles");

            entity.HasIndex(e => e.UserId, "individual_profiles_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .HasColumnName("last_name");
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .HasColumnName("middle_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.IndividualProfile)
                .HasForeignKey<IndividualProfile>(d => d.UserId)
                .HasConstraintName("individual_profiles_user_id_fkey");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("messages_pkey");

            entity.ToTable("messages");

            entity.HasIndex(e => e.ApplicationId, "idx_messages_application_id");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApplicationId).HasColumnName("application_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IsRead)
                .HasDefaultValue(false)
                .HasColumnName("is_read");
            entity.Property(e => e.Message1).HasColumnName("message");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");

            entity.HasOne(d => d.Application).WithMany(p => p.Messages)
                .HasForeignKey(d => d.ApplicationId)
                .HasConstraintName("messages_application_id_fkey");

            entity.HasOne(d => d.Sender).WithMany(p => p.Messages)
                .HasForeignKey(d => d.SenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("messages_sender_id_fkey");
        });

        modelBuilder.Entity<OrganizationProfile>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("organization_profiles_pkey");

            entity.ToTable("organization_profiles");

            entity.HasIndex(e => e.UserId, "organization_profiles_user_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(255)
                .HasColumnName("contact_person");
            entity.Property(e => e.FullName).HasColumnName("full_name");
            entity.Property(e => e.ShortName)
                .HasMaxLength(255)
                .HasColumnName("short_name");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.User).WithOne(p => p.OrganizationProfile)
                .HasForeignKey<OrganizationProfile>(d => d.UserId)
                .HasConstraintName("organization_profiles_user_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.Email, "idx_users_email");


            entity.HasIndex(e => e.Email, "users_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(30)
                .HasColumnName("phone");
            entity.Property(e => e.Role)
                .HasMaxLength(20)
                .HasDefaultValueSql("'USER'::character varying")
                .HasColumnName("role");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp with time zone")
                .HasColumnName("updated_at");
            entity.Property(e => e.UserType)
                .HasMaxLength(30)
                .HasColumnName("user_type");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
