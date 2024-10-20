using Core.Maybe;
using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.DriverFleet;

public interface IDriverRepository
{
  Task<Driver> Find(long? driverId);
  Task<List<Driver>> FindAll();
  Task<Driver> Save(Driver driver);
  Task<List<Driver>> FindAllById(ICollection<long?> ids);
  Task<Maybe<Driver>> FindById(long? driverId);
}

internal class EfCoreDriverRepository : IDriverRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<Driver> Find(long? driverId)
  {
    return await _context.Drivers.FindAsync(driverId);
  }

  public async Task<Maybe<Driver>> FindById(long? driverId)
  {
    return await Find(driverId).ToMaybeAsync();
  }


  public async Task<List<Driver>> FindAll()
  {
    return await _context.Drivers.ToListAsync();
  }

  public async Task<Driver> Save(Driver driver)
  {
    _context.Drivers.Update(driver);
    await _context.SaveChangesAsync();
    return driver;
  }

  public Task<List<Driver>> FindAllById(ICollection<long?> ids)
  {
    return _context.Drivers.Where(x => ids.Contains(x.Id)).ToListAsync();
  }
}