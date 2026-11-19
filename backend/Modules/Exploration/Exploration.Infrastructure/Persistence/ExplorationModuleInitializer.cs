using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Payment.Contracts;

namespace Exploration.Infrastructure.Persistence;

internal sealed class ExplorationModuleInitializer : IHostedService
{
    private static readonly Guid FirstExplorer = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private const string DemoImage = "/api/shared/images/explorer-demo.jpg";
    private const int FortressTourPrice = 100;
    private const int FruskaGoraTourPrice = 50;

    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _environment;

    public ExplorationModuleInitializer(IServiceProvider serviceProvider, IHostEnvironment environment)
    {
        _serviceProvider = serviceProvider;
        _environment = environment;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<ExplorationDbContext>();
        await dbContext.Database.MigrateAsync();

        if (_environment.IsDevelopment() && !await dbContext.Tours.AnyAsync())
        {
            var demoTours = CreateDemoTours();
            dbContext.Tours.AddRange(demoTours.Select(demo => demo.Tour));

            var paymentApi = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
            foreach (var demo in demoTours.Where(demo => demo.Tour.Status == TourStatus.Published))
            {
                var price = demo.Price
                    ?? throw new InvalidOperationException("A published demo tour requires a product price.");
                await paymentApi.PutProductInOfferAsync(
                    demo.Tour.Id,
                    demo.Tour.Name,
                    demo.Tour.AuthorId,
                    price);
            }

            await dbContext.SaveChangesAsync();
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static List<(Tour Tour, int? Price)> CreateDemoTours()
    {
        var fortress = new Tour(FirstExplorer, "Šetnja Petrovaradinskom tvrđavom",
            "Šetnja počinje na Gornjem platou Petrovaradinske tvrđave, vodi pored Sahat kule i podzemnih vojnih galerija, a završava se pogledom na Dunav i stari deo Novog Sada.",
            TourDifficulty.Easy, ["istorija", "priroda"], new RecommendedGroupSize(2, 6));
        fortress.AddTransportTime(TransportMode.Walking, 120);
        fortress.AddKeyPoint("Sahat kula", "Sat čije su kazaljke zamenjene, pa velika pokazuje sate, a mala minute.",
            DemoImage, null, new Location(45.2527, 19.8620), true, new TourLength(0));
        fortress.AddKeyPoint("Vidikovac nad Dunavom", "Pogled na Dunav i stari deo Novog Sada.",
            DemoImage, null, new Location(45.2541, 19.8628), false, new TourLength(0.4));
        fortress.Publish(new TourPrice(false, FortressTourPrice));

        var fruskaGora = new Tour(FirstExplorer, "Biciklom preko Fruške gore",
            "Ruta vodi od Sremske Kamenice preko Popovice do Iriškog venca, pored manastira Beočin i vidikovaca sa kojih se vidi ceo Srem, uz nekoliko dužih uspona.",
            TourDifficulty.Hard, ["biciklizam", "planina"], new RecommendedGroupSize(2, 6));
        fruskaGora.AddTransportTime(TransportMode.Bicycle, 240);
        fruskaGora.AddKeyPoint("Sremska Kamenica", "Polazna tačka ture, u podnožju Fruške gore.",
            DemoImage, null, new Location(45.2205, 19.8410), true, new TourLength(0));
        fruskaGora.AddKeyPoint("Iriški venac", "Vrh ture, sa spomenikom i pogledom na ceo Srem.",
            DemoImage, null, new Location(45.1546, 19.8582), false, new TourLength(9.5));
        fruskaGora.Publish(new TourPrice(false, FruskaGoraTourPrice));

        var cityCenter = new Tour(FirstExplorer, "Obilazak centra grada",
            "Kratak obilazak Trga slobode, Dunavske ulice i Dunavskog parka.",
            TourDifficulty.Moderate, ["grad"], new RecommendedGroupSize(2, 6));

        return [(fortress, FortressTourPrice), (fruskaGora, FruskaGoraTourPrice), (cityCenter, null)];
    }
}
