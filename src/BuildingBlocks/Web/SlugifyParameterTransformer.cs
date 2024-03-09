using Microsoft.AspNetCore.Routing;
using System.Text.RegularExpressions;

namespace BuildingBlocks.Web
{
    public class SlugifyParameterTransformer : IOutboundParameterTransformer
    {
        public string TransformOutbound(object value)
        {
            return value is null
                ? null
                : Regex.Replace(value.ToString() ?? string.Empty, "([a-z])([A-Z])", "$1-$2", RegexOptions.CultureInvariant)
                    .ToLower();
        }
    }
}
