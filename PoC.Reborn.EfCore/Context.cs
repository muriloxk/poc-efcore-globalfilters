using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PoC.Reborn.EfCore
{
    public class Context : DbContext
    {
        public Context(DbContextOptions<Context> context) : base(context) { }

        public DbSet<Cliente> Clientes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Cliente>().ToTable("Cliente");

            modelBuilder.Entity<Cliente>().Property(x => x.Id)
                                     .IsRequired()
                                     .HasColumnType("CHAR(36)");

            modelBuilder.Entity<Cliente>().Property(x => x.Nome)
                                          .IsRequired()
                                          .HasMaxLength(150)
                                          .HasColumnType("VARCHAR(150)");

            modelBuilder.Entity<Cliente>().Property(x => x.TenantId)
                              .HasColumnType("CHAR(36)");


            modelBuilder.Entity<Cliente>().Property(x => x.IsDeleted)
                              .HasColumnType("BIT");

            modelBuilder.Entity<Cliente>().Property(x => x.DataNascimento)
                                          .IsRequired()
                                          .HasColumnType("DATETIME");

            modelBuilder.Entity<Cliente>().Ignore(x => x.domainEvents);


            //Filtros compostos

            //foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            //{
            //    ConfigureBasePropertiesMethodInfo
            //     .MakeGenericMethod(entityType.ClrType)
            //     .Invoke(this, new object[] { modelBuilder, entityType });
            //}


            //Sem combinação, quando usado, sobrescreve os outros filtros
            modelBuilder.SetQueryFilterOnAllEntities<ISoftDelete>(p => !p.IsDeleted);

        }

        protected virtual Expression<Func<TEntity, bool>> CreateFilterExpression<TEntity>()
                                                       where TEntity : class
        {
            Expression<Func<TEntity, bool>> expression = null;

            if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
                expression = e => !EF.Property<bool>(e, "IsDeleted");

            if (typeof(IMultiTenant).IsAssignableFrom(typeof(TEntity)))
            {
                Expression<Func<TEntity, bool>> multiTenantFilterExpression = e => EF.Property<Guid>(e, "TenantId") == Guider.GetTesterGuid();
                expression = expression == null ? multiTenantFilterExpression : expression.CombineExpressions(multiTenantFilterExpression);
            }

            return expression;
        }

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            var entries = ChangeTracker.Entries().ToList();
            var results = base.SaveChanges(acceptAllChangesOnSuccess);

            return results;
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess,
                                                   CancellationToken cancellationToken = new CancellationToken())
        {
            var entries = ChangeTracker.Entries().ToList();
            var results = base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);

            return results;
        }
    }
}
