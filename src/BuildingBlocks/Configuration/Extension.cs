using Microsoft.Extensions.Configuration;

namespace BuildingBlocks.Configuration
{
    public static class Extension
    {
        public static TModel GetOptions<TModel>(this IConfiguration configuration, string section) where TModel : new()
        {
            var model = new TModel();
            configuration.GetSection(section).Bind(model);

            return model;
        }
        public static string GetOptions(this IConfiguration configuration, string section)
        {
            return configuration.GetSection(section).Value;
        }
    }
}
