using SalesSystem.BLL.Services;
using SalesSystem.DAL.Repositories;
using SalesSystem.Entity;
using SalesSystem.Utility;

namespace SalesSystem.Tests;

public class UploadFileServiceTests : IDisposable
{
    private readonly SqliteDatabase _db = new();

    public void Dispose() => _db.Dispose();

    private UploadFileService NewService(out SalesSystem.DAL.DbSalesContext.DBSalesContext ctx)
    {
        ctx = _db.NewContext();
        return new UploadFileService(new GenericRepository<S3DetailsFile>(ctx));
    }

    [Fact]
    public async Task GetYearsWithFiles_ReturnsDistinctYearsPlusCurrent_NewestFirst()
    {
        _db.Seed(ctx => ctx.S3DetailsFile.AddRange(
            new S3DetailsFile { FileName = "a.xlsx", FileType = "x", FileDate = new DateTime(2021, 3, 1) },
            new S3DetailsFile { FileName = "b.xlsx", FileType = "x", FileDate = new DateTime(2021, 9, 1) },
            new S3DetailsFile { FileName = "c.xlsx", FileType = "x", FileDate = new DateTime(2025, 1, 5) }));

        var service = NewService(out var ctx);
        using (ctx)
        {
            var years = await service.getYearsWithFiles();
            var expected = new[] { 2021, 2025, DateTime.Now.Year }.Distinct().OrderByDescending(y => y);
            Assert.Equal(expected, years);
        }
    }

    [Fact]
    public async Task GetYearsWithFiles_WithoutFiles_ReturnsCurrentYear()
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            Assert.Equal(new[] { DateTime.Now.Year }, await service.getYearsWithFiles());
        }
    }

    [Theory]
    [InlineData("../secret.pdf", "Invalid file name")]
    [InlineData("script.exe", "File type not allowed")]
    [InlineData("contract.pdf", "File storage (S3) is not configured")]   // no AWS variables in tests
    public async Task UploadFile_InvalidRequests_ReturnClearMessages(string fileName, string expectedMessage)
    {
        var service = NewService(out var ctx);
        using (ctx)
        {
            var ex = await Assert.ThrowsAsync<BusinessException>(() => service.UploadFile(new byte[] { 1, 2, 3 }, fileName, "application/pdf"));
            Assert.StartsWith(expectedMessage, ex.Message);
        }
    }
}
