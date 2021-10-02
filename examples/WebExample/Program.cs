var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

var modelAssemblyName = typeof(IntegrationUser).Assembly.FullName;

// ���� MySQL��SQLServer��SQLite �������ݿ�� RAHID ���ܣ�Ĭ��ʹ�ü����û���
//builder.Services.AddDbContext<MySqlPortalAccessor<IntegrationUser>>(opts =>
//{
//    opts.UseMySql(MySqlConnectionStringHelper.Validate(builder.Configuration.GetConnectionString("MySqlConnectionString"), out var version), version,
//        a => a.MigrationsAssembly(modelAssemblyName).MigrationsAssembly(modelAssemblyName));

//    opts.UseAccessor(b => b.WithAccess(AccessMode.Write));
//});

builder.Services.AddDbContextPool<SqlServerPortalAccessor<IntegrationUser>>(opts =>
{
    opts.UseSqlServer(builder.Configuration.GetConnectionString("SqlServerConnectionString"),
        a => a.MigrationsAssembly(modelAssemblyName).MigrationsAssembly(modelAssemblyName));

    opts.UseAccessor(b => b.WithAccess(AccessMode.ReadWrite).WithPooling());
});

builder.Services.AddDbContext<SqlitePortalAccessor<IntegrationUser>>(opts =>
{
    opts.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnectionString"),
        a => a.MigrationsAssembly(modelAssemblyName).MigrationsAssembly(modelAssemblyName));

    opts.UseAccessor(b => b.WithAccess(AccessMode.Write).WithPriority(2f));
});

builder.Services.AddLibrame()
    .AddData(opts =>
    {
        // ����ʱÿ���������½����ݿ�
        opts.Access.EnsureDatabaseDeleted = false;

        // ÿ���޸�ѡ��ʱ�Զ�����Ϊ JSON �ļ�
        opts.PropertyChangedAction = (o, e) => o.SaveOptionsAsJson();
    })
    .AddSeeder<InternalPortalAccessorSeeder>()
    //.AddInitializer<InternalPortalAccessorInitializer<MySqlPortalAccessor<IntegrationUser>, InternalPortalAccessorSeeder, IntegrationUser>>()
    .AddInitializer<InternalPortalAccessorInitializer<SqlServerPortalAccessor<IntegrationUser>, InternalPortalAccessorSeeder, IntegrationUser>>()
    .AddInitializer<InternalPortalAccessorInitializer<SqlitePortalAccessor<IntegrationUser>, InternalPortalAccessorSeeder, IntegrationUser>>()
    .AddContent()
    .AddPortal()
    .SaveOptionsAsJson(); // �״α���ѡ��Ϊ JSON �ļ�

var app = builder.Build();

using (var score = app.Services.CreateScope())
{
    score.ServiceProvider.UseAccessorInitializer();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
