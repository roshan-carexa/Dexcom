using Microsoft.EntityFrameworkCore;
using Dexcom.Data;
using Dexcom.Services;
using Dexcom.Services.Jobs;
using Dexcom.Settings;
using Quartz;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DexcomSettings>(
    builder.Configuration.GetSection(DexcomSettings.SectionName));

builder.Services.AddDbContext<DexcomDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient("DexcomApi", client =>
{
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddScoped<IDexcomAuthService, DexcomAuthService>();
builder.Services.AddScoped<IDexcomDataService, DexcomDataService>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .SetIsOriginAllowed(_ => true)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Quartz Configuration
builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("EgvsJob");
    q.AddJob<EgvsJobs>(opts => opts.WithIdentity(jobKey));
    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("Egvs-trigger")
        .StartNow()
        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(5))
            .RepeatForever()
        )
    );
});

// Quartz Hosted Services
builder.Services.AddQuartzHostedService(q =>
{
    q.WaitForJobsToComplete = true;
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DexcomDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Dexcom Integration API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.MapControllers();

app.MapGet("/", () => "OK");

app.Run();
