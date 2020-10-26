using System;
using System.Collections.Generic;

namespace PoC.Reborn.EfCore
{
    public class Entity
    {
        public Guid Id { get; set; }

        public Entity()
        {
            Id = Guid.NewGuid();
        }

        public List<object> domainEvents { get; set; }
    }
}
