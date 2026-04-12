using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CertificationTrack",
                columns: table => new
                {
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TrackName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certific__E922109781937ECA", x => x.CertificationTrackId);
                });

            migrationBuilder.CreateTable(
                name: "Equipment",
                columns: table => new
                {
                    EquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EquipmentName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Equipmen__34474479DE5D2144", x => x.EquipmentId);
                });

            migrationBuilder.CreateTable(
                name: "ExpertiseArea",
                columns: table => new
                {
                    ExpertiseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpertiseName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Expertis__909D2EA1BF9483F3", x => x.ExpertiseId);
                });

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Role__8AFACE1A2C127234", x => x.RoleId);
                });

            migrationBuilder.CreateTable(
                name: "Room",
                columns: table => new
                {
                    RoomId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false),
                    Location = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Room__32863939F494A3BF", x => x.RoomId);
                });

            migrationBuilder.CreateTable(
                name: "SubjectCategory",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CategoryName = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__SubjectC__19093A0B1BF38FBC", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "User",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "varchar(20)", unicode: false, maxLength: 20, nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__User__1788CC4C3D214947", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__User__RoleId__276EDEB3",
                        column: x => x.RoleId,
                        principalTable: "Role",
                        principalColumn: "RoleId");
                });

            migrationBuilder.CreateTable(
                name: "RoomEquipment",
                columns: table => new
                {
                    RoomEquipmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    EquipmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__RoomEqui__809194CAB7CD1722", x => x.RoomEquipmentId);
                    table.ForeignKey(
                        name: "FK__RoomEquip__Equip__3A81B327",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId");
                    table.ForeignKey(
                        name: "FK__RoomEquip__RoomI__398D8EEE",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                });

            migrationBuilder.CreateTable(
                name: "Course",
                columns: table => new
                {
                    CourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    DurationHours = table.Column<int>(type: "int", nullable: false),
                    DefaultCapacity = table.Column<int>(type: "int", nullable: false),
                    EnrollmentFee = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    PrerequisiteCourseId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Course__C92D71A7EAAF5542", x => x.CourseId);
                    table.ForeignKey(
                        name: "FK__Course__Category__3F466844",
                        column: x => x.CategoryId,
                        principalTable: "SubjectCategory",
                        principalColumn: "CategoryId");
                    table.ForeignKey(
                        name: "FK__Course__Prerequi__403A8C7D",
                        column: x => x.PrerequisiteCourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId");
                });

            migrationBuilder.CreateTable(
                name: "Certificate",
                columns: table => new
                {
                    CertificateId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IssuedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CertificateReferenceNumber = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    CertificateStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certific__BBF8A7C16D3CB5E4", x => x.CertificateId);
                    table.ForeignKey(
                        name: "FK__Certifica__Certi__5CD6CB2B",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTrack",
                        principalColumn: "CertificationTrackId");
                    table.ForeignKey(
                        name: "FK__Certifica__UserI__5BE2A6F2",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "InstructorAvailability",
                columns: table => new
                {
                    AvailabilityId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DayOfWeek = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Instruct__DA3979B14C90008D", x => x.AvailabilityId);
                    table.ForeignKey(
                        name: "FK__Instructo__UserI__2D27B809",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "InstructorExpertise",
                columns: table => new
                {
                    InstructorExpertiseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpertiseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Instruct__838966F77FB79AB8", x => x.InstructorExpertiseId);
                    table.ForeignKey(
                        name: "FK__Instructo__Exper__31EC6D26",
                        column: x => x.ExpertiseId,
                        principalTable: "ExpertiseArea",
                        principalColumn: "ExpertiseId");
                    table.ForeignKey(
                        name: "FK__Instructo__UserI__32E0915F",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "InstructorProfile",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Bio = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false),
                    Notes = table.Column<string>(type: "varchar(1000)", unicode: false, maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Instruct__1788CC4CEA6D0CC1", x => x.UserId);
                    table.ForeignKey(
                        name: "FK__Instructo__UserI__2A4B4B5E",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Notification",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Message = table.Column<string>(type: "varchar(255)", unicode: false, maxLength: 255, nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: false),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Notifica__20CF2E1287BDC95F", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK__Notificat__UserI__6A30C649",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "TraineeCertificationProgress",
                columns: table => new
                {
                    ProgressId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    ProgressPercent = table.Column<int>(type: "int", nullable: false),
                    EligibleDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__TraineeC__BAE29CA5BB312F9D", x => x.ProgressId);
                    table.ForeignKey(
                        name: "FK__TraineeCe__Certi__59063A47",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTrack",
                        principalColumn: "CertificationTrackId");
                    table.ForeignKey(
                        name: "FK__TraineeCe__UserI__5812160E",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "CertificationTrackCourse",
                columns: table => new
                {
                    TrackCourseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    CertificationTrackId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Certific__C105F53ABACA6688", x => x.TrackCourseId);
                    table.ForeignKey(
                        name: "FK__Certifica__Certi__5441852A",
                        column: x => x.CertificationTrackId,
                        principalTable: "CertificationTrack",
                        principalColumn: "CertificationTrackId");
                    table.ForeignKey(
                        name: "FK__Certifica__Cours__5535A963",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId");
                });

            migrationBuilder.CreateTable(
                name: "CourseEquipmentRequirement",
                columns: table => new
                {
                    CourseEquipmentRequirementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuantityRequired = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    EquipmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CourseEq__DC2F3B191FBC31FF", x => x.CourseEquipmentRequirementId);
                    table.ForeignKey(
                        name: "FK__CourseEqu__Cours__4316F928",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK__CourseEqu__Equip__440B1D61",
                        column: x => x.EquipmentId,
                        principalTable: "Equipment",
                        principalColumn: "EquipmentId");
                });

            migrationBuilder.CreateTable(
                name: "CourseSession",
                columns: table => new
                {
                    SessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    SessionCapacity = table.Column<int>(type: "int", nullable: false),
                    SessionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AvailableSeats = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoomId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__CourseSe__C9F492904CDB8DCE", x => x.SessionId);
                    table.ForeignKey(
                        name: "FK__CourseSes__Cours__46E78A0C",
                        column: x => x.CourseId,
                        principalTable: "Course",
                        principalColumn: "CourseId");
                    table.ForeignKey(
                        name: "FK__CourseSes__RoomI__48CFD27E",
                        column: x => x.RoomId,
                        principalTable: "Room",
                        principalColumn: "RoomId");
                    table.ForeignKey(
                        name: "FK__CourseSes__UserI__47DBAE45",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "Enrollment",
                columns: table => new
                {
                    EnrollmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Status = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    EnrollmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OutstandingBalance = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    IsOverdue = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SessionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Enrollme__7F68771BF6E24110", x => x.EnrollmentId);
                    table.ForeignKey(
                        name: "FK__Enrollmen__Sessi__4CA06362",
                        column: x => x.SessionId,
                        principalTable: "CourseSession",
                        principalColumn: "SessionId");
                    table.ForeignKey(
                        name: "FK__Enrollmen__UserI__4BAC3F29",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "UserId");
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResult",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Remarks = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    RecordTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    RecordDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsPassed = table.Column<bool>(type: "bit", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Assessme__9769020826C1D63F", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK__Assessmen__Enrol__4F7CD00D",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId");
                });

            migrationBuilder.CreateTable(
                name: "Payment",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentStatus = table.Column<string>(type: "varchar(50)", unicode: false, maxLength: 50, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    PaidDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EnrollmentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Payment__9B556A38A4C3837F", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK__Payment__Enrollm__6D0D32F4",
                        column: x => x.EnrollmentId,
                        principalTable: "Enrollment",
                        principalColumn: "EnrollmentId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResult_EnrollmentId",
                table: "AssessmentResult",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_CertificationTrackId",
                table: "Certificate",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificate_UserId",
                table: "Certificate",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTrackCourse_CertificationTrackId",
                table: "CertificationTrackCourse",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificationTrackCourse_CourseId",
                table: "CertificationTrackCourse",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_CategoryId",
                table: "Course",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Course_PrerequisiteCourseId",
                table: "Course",
                column: "PrerequisiteCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEquipmentRequirement_CourseId",
                table: "CourseEquipmentRequirement",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseEquipmentRequirement_EquipmentId",
                table: "CourseEquipmentRequirement",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSession_CourseId",
                table: "CourseSession",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSession_RoomId",
                table: "CourseSession",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseSession_UserId",
                table: "CourseSession",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_SessionId",
                table: "Enrollment",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Enrollment_UserId",
                table: "Enrollment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorAvailability_UserId",
                table: "InstructorAvailability",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorExpertise_ExpertiseId",
                table: "InstructorExpertise",
                column: "ExpertiseId");

            migrationBuilder.CreateIndex(
                name: "IX_InstructorExpertise_UserId",
                table: "InstructorExpertise",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notification_UserId",
                table: "Notification",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_EnrollmentId",
                table: "Payment",
                column: "EnrollmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipment_EquipmentId",
                table: "RoomEquipment",
                column: "EquipmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RoomEquipment_RoomId",
                table: "RoomEquipment",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCertificationProgress_CertificationTrackId",
                table: "TraineeCertificationProgress",
                column: "CertificationTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_TraineeCertificationProgress_UserId",
                table: "TraineeCertificationProgress",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_User_RoleId",
                table: "User",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "UQ__User__A9D10534EE24796E",
                table: "User",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentResult");

            migrationBuilder.DropTable(
                name: "Certificate");

            migrationBuilder.DropTable(
                name: "CertificationTrackCourse");

            migrationBuilder.DropTable(
                name: "CourseEquipmentRequirement");

            migrationBuilder.DropTable(
                name: "InstructorAvailability");

            migrationBuilder.DropTable(
                name: "InstructorExpertise");

            migrationBuilder.DropTable(
                name: "InstructorProfile");

            migrationBuilder.DropTable(
                name: "Notification");

            migrationBuilder.DropTable(
                name: "Payment");

            migrationBuilder.DropTable(
                name: "RoomEquipment");

            migrationBuilder.DropTable(
                name: "TraineeCertificationProgress");

            migrationBuilder.DropTable(
                name: "ExpertiseArea");

            migrationBuilder.DropTable(
                name: "Enrollment");

            migrationBuilder.DropTable(
                name: "Equipment");

            migrationBuilder.DropTable(
                name: "CertificationTrack");

            migrationBuilder.DropTable(
                name: "CourseSession");

            migrationBuilder.DropTable(
                name: "Course");

            migrationBuilder.DropTable(
                name: "Room");

            migrationBuilder.DropTable(
                name: "User");

            migrationBuilder.DropTable(
                name: "SubjectCategory");

            migrationBuilder.DropTable(
                name: "Role");
        }
    }
}
