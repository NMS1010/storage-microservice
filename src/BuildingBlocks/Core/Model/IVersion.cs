using System.ComponentModel.DataAnnotations;

namespace BuildingBlocks.Core.Model
{
    public interface IVersion
    {
        [ConcurrencyCheck]
        long Version { get; set; }
    }
}
