using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;

namespace PoC.Reborn.EfCore
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<Context>();
            optionsBuilder.UseSqlServer("Server=localhost; Database=Clientes;User Id=sa; Password=yourStrong(!)Password;");

            LimparBanco(optionsBuilder);
            PopularBanco(optionsBuilder);

            using (var context = new Context(optionsBuilder.Options))
            {
                var clientes = await context.Clientes.ToListAsync();
                Console.ReadKey();
            }
        }

        private static void PopularBanco(DbContextOptionsBuilder<Context> optionsBuilder)
        { 
            using (var context = new Context(optionsBuilder.Options))
            {

                var contador = 0;
                for (var i = 1; i <= 10; i++)
                {
                    var cliente = new Cliente() {
                                                    Nome = $"Nome{i}",
                                                    DataNascimento = DateTime.Now,
                                                    TenantId = Guid.NewGuid(),
                                                    IsDeleted = false
                                                };

                    if (i % 2 == 0)
                    {
                        cliente.TenantId = Guider.GetTesterGuid();

                        if (contador < 3)
                        {
                            cliente.IsDeleted = false;
                            contador++;
                        }
                        else
                        {
                            cliente.IsDeleted = true;
                        }
                    }

                    cliente.domainEvents = new List<object>() { new DomainEvent(), new DomainEvent() };
                    context.Clientes.Add(cliente);
                }

                context.SaveChanges();
            }
        }

        private static void LimparBanco(DbContextOptionsBuilder<Context> optionsBuilder)
        {
            using (var context = new Context(optionsBuilder.Options))
            {
                if (context.Clientes.Count() > 0)
                {
                    var clientes = context.Clientes.ToList();
                    context.Clientes.RemoveRange(clientes);
                    context.SaveChanges();
                }
            }
        }
    }
}
