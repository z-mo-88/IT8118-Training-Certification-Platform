using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrainingSystem.API.Models;

namespace TrainingSystem.API.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AssessmentResult> AssessmentResults { get; set; }

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<CertificationTrack> CertificationTracks { get; set; }

    public virtual DbSet<CertificationTrackCourse> CertificationTrackCourses { get; set; }

    public virtual DbSet<Course> Courses { get; set; }

    public virtual DbSet<CourseEquipmentRequirement> CourseEquipmentRequirements { get; set; }

    public virtual DbSet<CourseSession> CourseSessions { get; set; }

    public virtual DbSet<Enrollment> Enrollments { get; set; }

    public virtual DbSet<Equipment> Equipment { get; set; }

    public virtual DbSet<ExpertiseArea> ExpertiseAreas { get; set; }

    public virtual DbSet<InstructorAvailability> InstructorAvailabilities { get; set; }

    public virtual DbSet<InstructorExpertise> InstructorExpertises { get; set; }

    public virtual DbSet<InstructorProfile> InstructorProfiles { get; set; }

    public virtual DbSet<Notification> Notifications { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<RoomEquipment> RoomEquipments { get; set; }

    public virtual DbSet<SubjectCategory> SubjectCategories { get; set; }

    public virtual DbSet<TraineeCertificationProgress> TraineeCertificationProgresses { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TrainingCertificationDB;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AssessmentResult>(entity =>
        {
            entity.HasKey(e => e.ResultId).HasName("PK__Assessme__9769020826C1D63F");

            entity.ToTable("AssessmentResult");

            entity.Property(e => e.Remarks)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Enrollment).WithMany(p => p.AssessmentResults)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Assessmen__Enrol__4F7CD00D");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7C16D3CB5E4");

            entity.ToTable("Certificate");

            entity.Property(e => e.CertificateReferenceNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CertificateStatus)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CertificationTrack).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.CertificationTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Certi__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.Certificates)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__UserI__5BE2A6F2");
        });

        modelBuilder.Entity<CertificationTrack>(entity =>
        {
            entity.HasKey(e => e.CertificationTrackId).HasName("PK__Certific__E922109781937ECA");

            entity.ToTable("CertificationTrack");

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.TrackName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<CertificationTrackCourse>(entity =>
        {
            entity.HasKey(e => e.TrackCourseId).HasName("PK__Certific__C105F53ABACA6688");

            entity.ToTable("CertificationTrackCourse");

            entity.HasOne(d => d.CertificationTrack).WithMany(p => p.CertificationTrackCourses)
                .HasForeignKey(d => d.CertificationTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Certi__5441852A");

            entity.HasOne(d => d.Course).WithMany(p => p.CertificationTrackCourses)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Cours__5535A963");
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.CourseId).HasName("PK__Course__C92D71A7EAAF5542");

            entity.ToTable("Course");

            entity.Property(e => e.Description)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.EnrollmentFee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Title)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Courses)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Course__Category__3F466844");

            entity.HasOne(d => d.PrerequisiteCourse).WithMany(p => p.InversePrerequisiteCourse)
                .HasForeignKey(d => d.PrerequisiteCourseId)
                .HasConstraintName("FK__Course__Prerequi__403A8C7D");
        });

        modelBuilder.Entity<CourseEquipmentRequirement>(entity =>
        {
            entity.HasKey(e => e.CourseEquipmentRequirementId).HasName("PK__CourseEq__DC2F3B191FBC31FF");

            entity.ToTable("CourseEquipmentRequirement");

            entity.HasOne(d => d.Course).WithMany(p => p.CourseEquipmentRequirements)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseEqu__Cours__4316F928");

            entity.HasOne(d => d.Equipment).WithMany(p => p.CourseEquipmentRequirements)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseEqu__Equip__440B1D61");
        });

        modelBuilder.Entity<CourseSession>(entity =>
        {
            entity.HasKey(e => e.SessionId).HasName("PK__CourseSe__C9F492904CDB8DCE");

            entity.ToTable("CourseSession");

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Course).WithMany(p => p.CourseSessions)
                .HasForeignKey(d => d.CourseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSes__Cours__46E78A0C");

            entity.HasOne(d => d.Room).WithMany(p => p.CourseSessions)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSes__RoomI__48CFD27E");

            entity.HasOne(d => d.User).WithMany(p => p.CourseSessions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CourseSes__UserI__47DBAE45");
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.EnrollmentId).HasName("PK__Enrollme__7F68771BF6E24110");

            entity.ToTable("Enrollment");

            entity.Property(e => e.OutstandingBalance).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Session).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.SessionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__Sessi__4CA06362");

            entity.HasOne(d => d.User).WithMany(p => p.Enrollments)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Enrollmen__UserI__4BAC3F29");
        });

        modelBuilder.Entity<Equipment>(entity =>
        {
            entity.HasKey(e => e.EquipmentId).HasName("PK__Equipmen__34474479DE5D2144");

            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.EquipmentName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<ExpertiseArea>(entity =>
        {
            entity.HasKey(e => e.ExpertiseId).HasName("PK__Expertis__909D2EA1BF9483F3");

            entity.ToTable("ExpertiseArea");

            entity.Property(e => e.ExpertiseName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<InstructorAvailability>(entity =>
        {
            entity.HasKey(e => e.AvailabilityId).HasName("PK__Instruct__DA3979B14C90008D");

            entity.ToTable("InstructorAvailability");

            entity.Property(e => e.DayOfWeek)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.InstructorAvailabilities)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__UserI__2D27B809");
        });

        modelBuilder.Entity<InstructorExpertise>(entity =>
        {
            entity.HasKey(e => e.InstructorExpertiseId).HasName("PK__Instruct__838966F77FB79AB8");

            entity.ToTable("InstructorExpertise");

            entity.HasOne(d => d.Expertise).WithMany(p => p.InstructorExpertises)
                .HasForeignKey(d => d.ExpertiseId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__Exper__31EC6D26");

            entity.HasOne(d => d.User).WithMany(p => p.InstructorExpertises)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__UserI__32E0915F");
        });

        modelBuilder.Entity<InstructorProfile>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Instruct__1788CC4CEA6D0CC1");

            entity.ToTable("InstructorProfile");

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.Bio)
                .HasMaxLength(1000)
                .IsUnicode(false);
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithOne(p => p.InstructorProfile)
                .HasForeignKey<InstructorProfile>(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Instructo__UserI__2A4B4B5E");
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId).HasName("PK__Notifica__20CF2E1287BDC95F");

            entity.ToTable("Notification");

            entity.Property(e => e.Message)
                .HasMaxLength(255)
                .IsUnicode(false);

            entity.HasOne(d => d.User).WithMany(p => p.Notifications)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Notificat__UserI__6A30C649");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A38A4C3837F");

            entity.ToTable("Payment");

            entity.Property(e => e.AmountPaid).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentStatus)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.Enrollment).WithMany(p => p.Payments)
                .HasForeignKey(d => d.EnrollmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Enrollm__6D0D32F4");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A2C127234");

            entity.ToTable("Role");

            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.RoomId).HasName("PK__Room__32863939F494A3BF");

            entity.ToTable("Room");

            entity.Property(e => e.Location)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.RoomName)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<RoomEquipment>(entity =>
        {
            entity.HasKey(e => e.RoomEquipmentId).HasName("PK__RoomEqui__809194CAB7CD1722");

            entity.ToTable("RoomEquipment");

            entity.HasOne(d => d.Equipment).WithMany(p => p.RoomEquipments)
                .HasForeignKey(d => d.EquipmentId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomEquip__Equip__3A81B327");

            entity.HasOne(d => d.Room).WithMany(p => p.RoomEquipments)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__RoomEquip__RoomI__398D8EEE");
        });

        modelBuilder.Entity<SubjectCategory>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__SubjectC__19093A0B1BF38FBC");

            entity.ToTable("SubjectCategory");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TraineeCertificationProgress>(entity =>
        {
            entity.HasKey(e => e.ProgressId).HasName("PK__TraineeC__BAE29CA5BB312F9D");

            entity.ToTable("TraineeCertificationProgress");

            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.HasOne(d => d.CertificationTrack).WithMany(p => p.TraineeCertificationProgresses)
                .HasForeignKey(d => d.CertificationTrackId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TraineeCe__Certi__59063A47");

            entity.HasOne(d => d.User).WithMany(p => p.TraineeCertificationProgresses)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TraineeCe__UserI__5812160E");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C3D214947");

            entity.ToTable("User");

            entity.HasIndex(e => e.Email, "UQ__User__A9D10534EE24796E").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false);
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__User__RoleId__276EDEB3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
