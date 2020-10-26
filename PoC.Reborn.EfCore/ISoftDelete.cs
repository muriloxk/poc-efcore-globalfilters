using System;
namespace PoC.Reborn.EfCore
{
    public interface ISoftDelete
    {
        bool IsDeleted { get; set; }
    }
}
