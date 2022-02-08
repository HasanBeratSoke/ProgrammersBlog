using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using ProgrammersBlog.Data.Concrete.EntityFramework.Contexts;
using ProgrammersBlog.Mvc.AutoMapper.Profiles;
using ProgrammersBlog.Mvc.Helpers;
using ProgrammersBlog.Mvc.Helpers.Abstract;
using ProgrammersBlog.Mvc.Helpers.Concrete;
using ProgrammersBlog.Services.AutoMapper.Profiles;
using ProgrammersBlog.Services.Extensions;

namespace ProgrammersBlog.Mvc
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //mvc uygulamas� oldu�unu bu kod ile belirtiyoruz.
            //Add json options ekleme nedenimiz controllerdan  viewa model d�nerken javascriptin bu modeli tan�mas� i�in json formata �evirmemiz gerekmesi.
            services.AddControllersWithViews().AddRazorRuntimeCompilation().AddJsonOptions(opt=>
                opt.JsonSerializerOptions.Converters.Add( new JsonStringEnumConverter())
            );
            services.AddSession();
            services.AddAutoMapper(typeof(CategoryProfile), typeof(ArticleProfile),typeof(UserProfile), typeof(ViewModelsProfile));

            //Bi<im service/Extensions k�sm�nda olu�turdu�umuz dosya. �nterface leri vs vermek i�in.
            services.LoadMyServices(Configuration.GetConnectionString(name:"LocalDB"));
            services.AddScoped<IImageHelper, ImageHelper>();
            //Cookie bilgilerini veriyoruz.
            services.ConfigureApplicationCookie(options =>
            {
                //Kullan�c� giri� yapmadan bir yere ula�mak istedi�inde bu adrese logine y�nlendiriyor kullan�c�y�.
                options.LoginPath = new PathString("/Admin/User/Login");
                options.LogoutPath = new PathString("/Admin/User/Logout");

                options.Cookie = new CookieBuilder
                {
                    Name =  "ProgrammersBlog",
                    //Gelen isteklerin sadece http �zerinden olmas�n� sa�l�yor. Yaz�lan js kodu ile cookie bilgilerine ula��lmas� engelleniyor bu sayede.
                    HttpOnly=true,
                    //Gelen isteklerin sadece kendi ssitemiz �zerinden gelenlerini kabul ediyor. Cookileri ele ge�irilen birinin bilgileriyle ba�ka bir adresden istek gelmesi engelleniyor.
                    SameSite=SameSiteMode.Strict,
                    //Normalde .Always olmal� gelen b�t�n istekler https �zerinden olmal� ama geli�tirme a�amas� oldu�u i�in SameAsRequest yapt�k http isteklerinide kabul edecek.
                    SecurePolicy=CookieSecurePolicy.SameAsRequest,
                };
                //Kullan�c� giri� yapt�ktan sonra belirle s�re hesab� a��k kalacak m� ne kadar a��k kalacak. 7 g�n yapt�k.
                options.SlidingExpiration = true;
                options.ExpireTimeSpan = System.TimeSpan.FromDays(7);
                //Giri� yapm�� kullan�c� yetkisi olmayan bir yere ula�maya �al��t���nda bu sayfaya y�nlenirilecek burda hata verece�iz.
                options.AccessDeniedPath = new PathString("/Admin/User/AccessDenied");
            });

            services.AddRazorPages();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                //Bulunmayan bir viewe gitti�imizde bize bunu belirterek yard�mc� oluyor.
                app.UseStatusCodePages();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                //Admin area ile article sayfas�na girdi�imizde articels �zerinde de�i�iklik yapabilece�iz. Fakat normal kullan�c� girdi�inde sadece okuyabilecek.
                endpoints.MapAreaControllerRoute(
                    name:"Admin",
                    areaName:"Admin",
                    pattern:"Admin/{controller=Home}/{action=Index}/{id?}"
                    );
                //Ba�lang��ta home indexinden a��l��� sa�l�yor.
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
