using System;

namespace PoC.Reborn.EfCore
{
    public class Cliente : Entity,
                           IMultiTenant,
                           ISoftDelete
    {
        public Cliente() 
        {
           
        }

        public string Nome { get; set; }
        public DateTime DataNascimento { get; set; }
        public Guid? TenantId { get; set; }
        public bool IsDeleted { get; set; }
    }
}