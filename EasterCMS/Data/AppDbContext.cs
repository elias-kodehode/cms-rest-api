using EasterCMS.Models;
using Microsoft.EntityFrameworkCore;

namespace EasterCMS.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options): DbContext(options)
{
    public DbSet<Participant> Participants { get; set; }
}
