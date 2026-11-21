using CodeBridge.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

// در صورت عدم دسترسی به Configuration، از این رشته‌ی اتصال پیش‌فرض استفاده می‌شود.
// مقدار باید دقیقاً با Database داخل appsettings.json پروژه‌ی Presentation یکی باشد،
// وگرنه "dotnet ef database update" روی دیتابیسِ متفاوتی اجرا می‌شود و اپ دیتابیس خالی می‌بیند.
// می‌توانید از طریق متغیر محیطی CONNECTION_STRING این مقدار را تغییر دهید.
namespace CodeBridge.Infrastructure;

public class DesignTimeAppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING")
            ?? "Server=.;Database=CodeBridgeDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}