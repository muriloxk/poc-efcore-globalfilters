using System;
namespace PoC.Reborn.EfCore
{
    public interface IMultiTenant
    {
        Guid? TenantId { get; set; }
    }
}
