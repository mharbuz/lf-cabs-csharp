﻿using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace LegacyFighter.CabsTests.Integration;

public class CalculateDriverTravelledDistanceIntegrationTest
{
  private static readonly Instant Noon = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant NoonFive = Noon.Plus(Duration.FromMinutes(5));
  private static readonly Instant NoonTen = NoonFive.Plus(Duration.FromMinutes(5));

  private CabsApp _app = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;
  private IClock Clock { get; set; } = default!;

  [SetUp]
  public void InitializeApp()
  {
    Clock = Substitute.For<IClock>();
    _app = CabsApp.CreateInstance(ctx => ctx.AddSingleton(Clock));
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task DistanceIsZeroWhenZeroPositions()
  {
    //given
    var driver = await Fixtures.ADriver();

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonFive);

    //then
    Assert.AreEqual("0km", distance.PrintIn("km"));
  }

  [Test]
  public async Task TravelledDistanceWithoutMultiplePositionsIzZero()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    ItIsNoon();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonFive);

    //then
    Assert.AreEqual("0km", distance.PrintIn("km"));
  }

  [Test]
  public async Task CanCalculateTravelledDistanceFromShortTransit()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    ItIsNoon();
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonFive);

    //then
    Assert.AreEqual("4.009km", distance.PrintIn("km"));
  }

  [Test]
  public async Task CanCalculateTravelledDistanceWithBreakWithin()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    ItIsNoon();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    //and
    ItIsNoonFive();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonFive);

    //then
    Assert.AreEqual("8.017km", distance.PrintIn("km"));
  }

  [Test]
  public async Task CanCalculateTravelledDistanceWithMultipleBreaks()
  {
    //given
    var driver = await Fixtures.ADriver();
    //and
    ItIsNoon();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    //and
    ItIsNoonFive();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    //and
    ItIsNoonTen();
    //and
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.31861111111111, -1.6997222222222223);
    await DriverTrackingService.RegisterPosition(driver.Id, 53.32055555555556, -1.7297222222222221);

    //when
    var distance = await DriverTrackingService.CalculateTravelledDistance(driver.Id, Noon, NoonTen);

    //then
    Assert.AreEqual("12.026km", distance.PrintIn("km"));
  }

  private void ItIsNoon()
  {
    Clock.GetCurrentInstant().Returns(Noon);
  }

  private void ItIsNoonFive()
  {
    Clock.GetCurrentInstant().Returns(NoonFive);
  }

  private void ItIsNoonTen()
  {
    Clock.GetCurrentInstant().Returns(NoonTen);
  }
}