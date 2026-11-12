using InternLog.API.Core.Common;
using InternLog.API.Infrastructure.Data.Models;
using InternLog.API.Modules.Leaves;
using InternLog.API.Modules.WorkLogs;
using InternLog.API.Tests.Infrastructure;

namespace InternLog.API.Tests;

/// <summary>
/// ไฟล์แนบดาวน์โหลดได้เฉพาะ: นักศึกษาเจ้าของ, อาจารย์ที่ดูแลนักศึกษาคนนั้น และ admin
/// </summary>
public class AttachmentAccessTests : IDisposable
{
    private readonly TestDb _db = new();
    private readonly TempFileStorage _files = new();
    private readonly FakeCurrentUser _currentUser = new();

    private readonly User _student, _otherStudent, _advisor, _otherAdvisor, _admin;
    private readonly Leave _leave;
    private readonly Attachment _workLogAttachment;

    public AttachmentAccessTests()
    {
        _student      = _db.AddUser("student@test.com", TestDb.StudentRoleId);
        _otherStudent = _db.AddUser("other-student@test.com", TestDb.StudentRoleId);
        _advisor      = _db.AddUser("advisor@test.com", TestDb.AdvisorRoleId);
        _otherAdvisor = _db.AddUser("other-advisor@test.com", TestDb.AdvisorRoleId);
        _admin        = _db.AddUser("admin@test.com", TestDb.AdminRoleId);

        var internship = _db.AddInternship(_student, _advisor);
        _db.AddInternship(_otherStudent, _otherAdvisor);

        var leavePath = _files.Storage.SaveUploadAsync(TestFiles.Pdf(), "leaves", 1024 * 1024).GetAwaiter().GetResult();
        _leave = new Leave
        {
            StudentID = _student.UserID, InternshipID = internship.InternshipID, LeaveTypeID = 1,
            StartDate = new DateOnly(2026, 11, 20), EndDate = new DateOnly(2026, 11, 20),
            DayCount = 1, Reason = "ป่วย", AttachmentPath = leavePath
        };
        _db.Context.Leaves.Add(_leave);

        var log = new WorkLog { StudentID = _student.UserID, CategoryID = 1, LogDate = new DateOnly(2026, 11, 12), Title = "งาน" };
        _db.Context.WorkLogs.Add(log);
        _db.Context.SaveChanges();

        var logPath = _files.Storage.SaveUploadAsync(TestFiles.Pdf("evidence.pdf"), "worklogs", 1024 * 1024).GetAwaiter().GetResult();
        _workLogAttachment = new Attachment { LogID = log.LogID, FileName = "evidence.pdf", FilePath = logPath, FileSize = 109 };
        _db.Context.Attachments.Add(_workLogAttachment);
        _db.Context.SaveChanges();
    }

    public void Dispose()
    {
        _db.Dispose();
        _files.Dispose();
    }

    private LeaveService Leaves() => new(_db.NewContext(), _currentUser, _files.Storage);
    private WorkLogService WorkLogs() => new(_db.NewContext(), _currentUser, _files.Storage);

    public static TheoryData<string, bool> Viewers => new()
    {
        { "owner", true },
        { "assigned-advisor", true },
        { "admin", true },
        { "other-student", false },
        { "other-advisor", false },
    };

    private void ActAs(string viewer)
    {
        var (user, role) = viewer switch
        {
            "owner"            => (_student, Constants.Roles.Student),
            "assigned-advisor" => (_advisor, Constants.Roles.Advisor),
            "admin"            => (_admin, Constants.Roles.Admin),
            "other-student"    => (_otherStudent, Constants.Roles.Student),
            "other-advisor"    => (_otherAdvisor, Constants.Roles.Advisor),
            _ => throw new ArgumentOutOfRangeException(nameof(viewer))
        };
        _currentUser.As(user.UserID, role);
    }

    [Theory]
    [MemberData(nameof(Viewers))]
    public async Task LeaveAttachment_OnlyAuthorizedViewers(string viewer, bool allowed)
    {
        ActAs(viewer);

        if (allowed)
        {
            var file = await Leaves().GetAttachmentAsync(_leave.LeaveID);
            await using var _ = file.Content;
            Assert.Equal("application/pdf", file.ContentType);
        }
        else
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => Leaves().GetAttachmentAsync(_leave.LeaveID));
            Assert.Equal(403, ex.StatusCode);
        }
    }

    [Theory]
    [MemberData(nameof(Viewers))]
    public async Task WorkLogAttachment_OnlyAuthorizedViewers(string viewer, bool allowed)
    {
        ActAs(viewer);

        if (allowed)
        {
            var file = await WorkLogs().GetAttachmentAsync(_workLogAttachment.AttachmentID);
            await using var _ = file.Content;
            Assert.Equal("evidence.pdf", file.DownloadName);
        }
        else
        {
            var ex = await Assert.ThrowsAsync<AppException>(() => WorkLogs().GetAttachmentAsync(_workLogAttachment.AttachmentID));
            Assert.Equal(403, ex.StatusCode);
        }
    }

    [Fact]
    public async Task LeaveDto_ExposesDownloadEndpoint_NotTheStoragePath()
    {
        _currentUser.As(_student.UserID, Constants.Roles.Student);

        var leaves = await Leaves().GetStudentLeavesAsync();

        Assert.Equal($"/api/leaves/{_leave.LeaveID}/attachment", Assert.Single(leaves).AttachmentUrl);
    }

    [Fact]
    public async Task DeletingWorkLogAttachment_RemovesTheFile()
    {
        _currentUser.As(_student.UserID, Constants.Roles.Student);

        await WorkLogs().DeleteAttachmentAsync(_workLogAttachment.AttachmentID);

        Assert.Null(_files.Storage.Open(_workLogAttachment.FilePath, "x"));
    }
}
