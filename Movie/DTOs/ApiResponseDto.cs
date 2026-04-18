
using System.Text.Json.Serialization;

namespace Movie.Tests.DTOs
{
    public class ApiResponseDto
    {
        [JsonPropertyName("msg")]
        public string Msg { get; set; } = string.Empty;

        [JsonPropertyName("movie")]
        public MovieDto Movie { get; set; } = new MovieDto();
    }
}

