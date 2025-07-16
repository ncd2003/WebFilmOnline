// using necessary namespaces
 // Đảm bảo namespace này khớp với thư mục Data của bạn
using WebFilmOnline.Models; // Đảm bảo namespace này khớp với thư mục Models của bạn
using WebFilmOnline.Services; // Đảm bảo namespace này khớp với thư mục Services của bạn
 // Đảm bảo namespace này khớp với thư mục Interfaces của bạn
using WebFilmOnline.Services.VNPay; // Đảm bảo namespace này khớp với thư mục Services/VNPay của bạn
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Cho ILogger

// THÊM CÁC USING MỚI CHO CHỨC NĂNG BÁO CÁO DOANH THU THỜI GIAN THỰC
using System.Collections.Concurrent;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using FilmServiceApp.Services.Interfaces;
using FilmServiceApp.Services;
using WebFilmOnline.Services.Real_Time;
// using Microsoft.AspNetCore.Mvc; // Không cần thiết ở đây vì đã có trong Controller

var builder = WebApplication.CreateBuilder(args);

// Thêm các dịch vụ vào container.
builder.Services.AddControllersWithViews();

// Cấu hình DbContext
builder.Services.AddDbContext<FilmServiceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // Sử dụng "DefaultConnection"

// Thêm dịch vụ Logging
builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.AddConsole(); // Xuất nhật ký ra console
    loggingBuilder.AddDebug();    // Xuất nhật ký ra cửa sổ debug
});

// Đăng ký các dịch vụ tùy chỉnh
builder.Services.AddScoped<IUserService, UserService>(); // Đăng ký interface và implementation của user service
builder.Services.AddScoped<PointService>(); // Đăng ký PointService
builder.Services.AddScoped<VnPayService>(); // Đăng ký VnPayService

// =====================================================================
// BẮT ĐẦU: THÊM ĐĂNG KÝ DỊCH VỤ CHO BÁO CÁO DOANH THU THỜI GIAN THỰC
// =====================================================================
builder.Services.AddSingleton<ConcurrentQueue<TransactionEvent>>(); // Hàng đợi tin nhắn
builder.Services.AddSingleton<EventProducer>(); // Dịch vụ tạo sự kiện
builder.Services.AddSingleton<StreamProcessor>(); // Dịch vụ xử lý luồng (là Singleton)
builder.Services.AddHostedService(provider => provider.GetRequiredService<StreamProcessor>()); // Đăng ký StreamProcessor là Hosted Service
// =====================================================================
// KẾT THÚC: THÊM ĐĂNG KÝ DỊCH VỤ CHO BÁO CÁO DOANH THU THỜI GIAN THỰC
// =====================================================================


// Thêm dịch vụ Xác thực
builder.Services.AddAuthentication(options =>
{
    // Đặt scheme mặc định là Cookie authentication
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    // Đặt scheme challenge mặc định để xử lý các yêu cầu chưa được xác thực
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Cấu hình các tùy chọn cookie
    options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập của bạn
    options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn cho truy cập bị từ chối
    options.Cookie.Name = "WebFilmOnlineAuth"; // Tên cookie xác thực
    options.Cookie.HttpOnly = true; // Cookie chỉ có thể truy cập qua HTTP, không phải JavaScript
    options.ExpireTimeSpan = TimeSpan.FromDays(7); // Thời gian hết hạn của cookie
    options.SlidingExpiration = true; // Gia hạn thời gian hết hạn của cookie khi người dùng hoạt động
})
.AddGoogle(googleOptions =>
{
    // Lấy ClientId và ClientSecret của Google từ appsettings.json
    googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"] ?? throw new InvalidOperationException("Google ClientId not configured.");
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? throw new InvalidOperationException("Google ClientSecret not configured.");
    // Chỉ định phạm vi thông tin cần yêu cầu từ Google
    googleOptions.Scope.Add("profile"); // Yêu cầu thông tin hồ sơ (tên, ảnh)
    googleOptions.Scope.Add("email");    // Yêu cầu địa chỉ email
})
.AddFacebook(facebookOptions =>
{
    // Lấy AppId và AppSecret của Facebook từ appsettings.json
    facebookOptions.AppId = builder.Configuration["Authentication:Facebook:AppId"] ?? throw new InvalidOperationException("Facebook AppId not configured.");
    facebookOptions.AppSecret = builder.Configuration["Authentication:Facebook:AppSecret"] ?? throw new InvalidOperationException("Facebook AppSecret not configured.");
    // Chỉ định phạm vi thông tin cần yêu cầu từ Facebook
    facebookOptions.Scope.Add("email"); // Yêu cầu địa chỉ email
    facebookOptions.Scope.Add("public_profile"); // Yêu cầu thông tin hồ sơ công khai
});


var app = builder.Build();

// Cấu hình pipeline yêu cầu HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // Giá trị HSTS mặc định là 30 ngày. Bạn có thể muốn thay đổi điều này cho các kịch bản sản xuất, xem [https://aka.ms/aspnetcore-hsts](https://aka.ms/aspnetcore-hsts).
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Kích hoạt middleware xác thực
app.UseAuthentication();
// Kích hoạt middleware ủy quyền
app.UseAuthorization();

// =====================================================================
// BẮT ĐẦU: THÊM ĐỊNH TUYẾN CHO API CONTROLLER VÀ MÔ PHỎNG TẠO GIAO DỊCH
// =====================================================================
app.UseEndpoints(endpoints =>
{
    // Định tuyến cho các API Controller (ví dụ: RevenueController)
    endpoints.MapControllers(); // Đây là dòng quan trọng để RevenueController hoạt động

    // Định tuyến cho các MVC Controller (ví dụ: HomeController)
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});

// Mô phỏng việc tạo giao dịch trong một Task riêng biệt
// Trong một ứng dụng thực tế, EventProducer sẽ được gọi từ các service xử lý nghiệp vụ khác
// (ví dụ: sau khi một thanh toán VNPay thành công, hoặc khi một gói được mua).
var eventProducer = app.Services.GetRequiredService<EventProducer>();
Task.Run(async () =>
{
    var random = new Random();
    var productTypes = new[] { "Gói Cao Cấp", "Gói Cơ Bản", "Phim Lẻ", "Nạp Point" };
    var paymentMethods = new[] { "VNPay", "Point" };

    while (true)
    {
        var userId = $"USER-{random.Next(1000, 9999)}";
        var productType = productTypes[random.Next(productTypes.Length)];
        var productName = $"Sản phẩm {random.Next(1, 10)}";
        var amount = random.Next(50000, 500000); // Từ 50k đến 500k VND
        var paymentMethod = paymentMethods[random.Next(paymentMethods.Length)];
        var status = (random.NextDouble() < 0.8) ? "Thành công" : "Thất bại"; // 80% thành công, 20% thất bại

        eventProducer.GenerateTransaction(userId, productType, productName, amount, paymentMethod, status);

        await Task.Delay(random.Next(500, 1500)); // Tạo giao dịch mỗi 0.5 - 1.5 giây
    }
});

app.Run();
