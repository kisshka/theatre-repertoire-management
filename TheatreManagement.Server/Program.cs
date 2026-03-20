
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;

namespace TheatreManagement.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Services

            builder.Services.AddControllers();
 
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();



            // Подключение к БД
            builder.Services.AddDbContext<TheatreManagement.Domain.Data.DataContext>(options =>
            {
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

                var connection = new SqliteConnection(connectionString);
                connection.Open();

                // Кастомная функция Like
                connection.CreateFunction("CustomLike",
                    (string text, string pattern) =>
                        text != null && pattern != null &&
                        text.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0);

                options.UseSqlite(connection);
            });



            //Авторизация
            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<User>()
                    .AddRoles<IdentityRole>()
                    .AddEntityFrameworkStores<DataContext>();


            var app = builder.Build();

        //Создание ролей
            //using (var scope = app.Services.CreateScope())
            //{
            //    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            //    string[] roles = { "MainAdmin", "TravelAdmin", "TourAdmin", "HallAdmin" };

            //    foreach (var role in roles)
            //    {
            //        if (!await roleManager.RoleExistsAsync(role))
            //        {
            //            await roleManager.CreateAsync(new IdentityRole(role));
            //        }
            //    }
            //}

            app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin());
            
            
            app.MapIdentityApi<User>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
