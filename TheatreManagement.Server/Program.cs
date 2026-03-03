
using Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheatreManagement.Domain.Data;

namespace TheatreManagement.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Services

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Db
            builder.Services.AddDbContext<TheatreManagement.Domain.Data.DataContext>(options =>
                    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            //Authorization
            builder.Services.AddAuthorization();
            builder.Services.AddIdentityApiEndpoints<User>()
                            .AddEntityFrameworkStores<DataContext>();

            var app = builder.Build();

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
