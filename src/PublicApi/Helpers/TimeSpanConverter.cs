using System.Globalization;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace PublicApi.Helpers
{
    public class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        private const string _format = "mm\\:ss\\:fff";

        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.ParseExact(reader.GetString(), _format, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString(_format, CultureInfo.InvariantCulture));
        }
    }

}