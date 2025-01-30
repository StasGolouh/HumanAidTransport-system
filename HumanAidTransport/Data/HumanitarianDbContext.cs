using HumanAidTransport.Models;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HumanitarianTransport.Data
{
    public class HumanitarianDbContext : DbContext
    {
        public HumanitarianDbContext(DbContextOptions<HumanitarianDbContext> options) : base(options)
        {
        }

        public DbSet<Admin> Admins { get; set; }
        public DbSet<Carrier> Carriers { get; set; }
        public DbSet<HumanitarianAid> HumanitarianAid { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<CarrierOrder> CarrierOrders { get; set; }
    }
}
