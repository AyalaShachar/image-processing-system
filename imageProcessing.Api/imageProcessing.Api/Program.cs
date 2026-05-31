using imageProcessing.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// CORS policy allowing the Angular client to call this API.
const string AngularCorsPolicy = "AngularClient";
builder.Services.AddCors(options =>
{
    options.AddPolicy(AngularCorsPolicy, policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add services to the container.
builder.Services.AddControllers();

// In-memory image store, shared across the whole application (thread-safe).
builder.Services.AddSingleton<IImageRepository, ImageRepository>();

// Handles saving uploaded files to disk and reading their dimensions.
builder.Services.AddSingleton<IImageStorageService, ImageStorageService>();

// Pipeline processing: configurable delays, live monitor, async queue, engine
// and the background worker that runs queued images through the pipelines.
builder.Services.Configure<PipelineOptions>(builder.Configuration.GetSection("Pipeline"));
builder.Services.AddSingleton<IPipelineMonitor, PipelineMonitor>();
builder.Services.AddSingleton<IPipelineQueue, PipelineQueue>();
builder.Services.AddSingleton<IPipelineEngine, PipelineEngine>();
builder.Services.AddHostedService<PipelineBackgroundService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors(AngularCorsPolicy);

app.UseAuthorization();

app.MapControllers();

app.Run();
