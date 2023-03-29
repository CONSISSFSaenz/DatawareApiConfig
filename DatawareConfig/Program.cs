var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


//services cors
//builder.Services.AddCors(p => p.AddPolicy("corsapp", builder =>
//{
//    //builder.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
//    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
//}));
builder.Services.AddCors(options =>
    options.AddPolicy("MyPolicy",
        builder => {
            builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
        }
    )
);

var app = builder.Build();


// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseAuthorization();
app.UseCors("MyPolicy");
app.MapControllers();

app.Run();
