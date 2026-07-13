
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;
using TheatreManagement.Server.Services;

namespace TheatreManagement.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            Func<string, string, bool> customLike = DataContext.CustomLike;
            var builder = WebApplication.CreateBuilder(args);

            //Services

            builder.Services.AddControllers();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //Мой сервис
            builder.Services.AddScoped<AvailabilityService>();


            // Подключение к БД
            builder.Services.AddDbContext<TheatreManagement.Domain.Data.DataContext>(options =>
            {

                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                var connection = new SqliteConnection(connectionString);
                connection.Open();

                // Кастомная функция Like
                connection.CreateFunction<string, string, bool>("CustomLike",
                    customLike);

                options.UseSqlite(connection);
            });


            //Авторизация
            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<User>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<DataContext>();

            //Почтааааа
            builder.Services.AddTransient<EmailService>();
            builder.Services.AddTransient<IEmailSender<User>, EmailSender>();
            builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

            var app = builder.Build();

            app.UseCors(policy => policy
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin());

            //Создание ролей
            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

                string[] roles = { "MainAdmin", "VisitAdmin", "TourAdmin", "StationarAdmin", "SystemAdmin" };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }

                var systemAdminEmail = "System1@admin.com";
                var systemAdminUser = await userManager.FindByEmailAsync(systemAdminEmail);

                if (systemAdminUser == null)
                {
                    var systemAdmin = new User
                    {
                        UserName = systemAdminEmail,
                        Email = systemAdminEmail,
                        Surname = "Системный",
                        Name = "Администратор",
                        FatherName = ""
                    };

                    var result = await userManager.CreateAsync(systemAdmin, "System1@admin.com");

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(systemAdmin, "SystemAdmin");
                    }
                    else
                    {
                        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                        Console.WriteLine($"Ошибка создания пользователя: {errors}");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            

            //app.UseDefaultFiles();

            app.MapControllers();
            app.MapIdentityApi<User>();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}