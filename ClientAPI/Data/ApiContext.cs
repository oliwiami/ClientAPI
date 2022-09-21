using Microsoft.EntityFrameworkCore;
using ClientAPI.Models;

namespace ClientAPI.Data
{
    public class ApiContext : DbContext
    {
        public DbSet<ClientModel> Clients { get; set; }

        public ApiContext(DbContextOptions<ApiContext> options)
            : base(options)
        {
        }

    }
}
