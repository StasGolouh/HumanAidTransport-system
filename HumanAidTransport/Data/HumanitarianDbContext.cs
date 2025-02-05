using HumanAidTransport.Models;
using Microsoft.EntityFrameworkCore;

namespace HumanitarianTransport.Data
{
    public class HumanitarianDbContext : DbContext
    {
        public HumanitarianDbContext(DbContextOptions<HumanitarianDbContext> options) : base(options)
        {
        }
        public DbSet<Volunteer> Volunteer { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<HumanitarianAid> HumanitarianAid { get; set; }
        public DbSet<DeliveryRequest> DeliveryRequests { get; set; }
        public DbSet<TransportOrder> TransportOrders { get; set; }

    }
}
