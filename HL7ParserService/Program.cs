using HL7ParserService.Models;
using HL7ParserService.Utility;

var builder = WebApplication.CreateBuilder();

builder.Services.AddServiceModelServices();
builder.Services.AddServiceModelMetadata();
builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

builder.Services.Configure<MongoDBSettings>(builder.Configuration.GetSection(Constants.MONGO_DB_CONFIGURATION));
builder.Services.Configure<SecuritySettings>(builder.Configuration.GetSection(Constants.SECURITY_CONFIGURATION));
builder.Services.AddSingleton<IS_PortType>();

var app = builder.Build();

app.UseServiceModel(serviceBuilder =>
{
    serviceBuilder.AddService<IS_PortType>();
    serviceBuilder.AddServiceEndpoint<IS_PortType, IIS_PortType>(new BasicHttpBinding(BasicHttpSecurityMode.Transport), Constants.SERVICE_ENDPOINT);
    var serviceMetadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
    serviceMetadataBehavior.HttpsGetEnabled = true;
});

app.Run();
