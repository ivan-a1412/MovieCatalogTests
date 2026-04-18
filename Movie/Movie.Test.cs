using Movie.Tests.DTOs;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace Movie.Tests
{
    [TestFixture]
    public class MovieTests
    {
        private RestClient client;
        private static string? createdMovieId;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            string jwtToken = GetJwtToken("testivan18@gmail.com", "123456");

            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            RestClient authClient = new RestClient("http://144.91.123.158:5000");
            RestRequest request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            RestResponse response = authClient.Execute(request);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new InvalidOperationException(
                    $"Authentication failed. Status: {response.StatusCode}, Body: {response.Content}");
            }

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content!);
            string? token = json.GetProperty("accessToken").GetString();

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new InvalidOperationException("Token is missing from authentication response.");
            }

            return token;
        }

        [Test, Order(1)]
        public void CreateMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "Test Movie Title",
                description = "Test Movie Description"
            });

            RestResponse response = client.Execute(request);
            ApiResponseDto? result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Movie, Is.Not.Null);
            Assert.That(result.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(result.Msg, Is.EqualTo("Movie created successfully!"));

            createdMovieId = result.Movie.Id;
        }

        [Test, Order(2)]
        public void EditMovie_WithValidId_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", createdMovieId!);
            request.AddJsonBody(new
            {
                title = "Edited Movie Title",
                description = "Edited Movie Description"
            });

            RestResponse response = client.Execute(request);
            ApiResponseDto? result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Test, Order(3)]
        public void GetAllMovies_ShouldReturnNonEmptyArray()
        {
            RestRequest request = new RestRequest("/api/Catalog/All", Method.Get);

            RestResponse response = client.Execute(request);
            var movies = JsonSerializer.Deserialize<JsonElement>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(movies.ValueKind, Is.EqualTo(JsonValueKind.Array));
            Assert.That(movies.GetArrayLength(), Is.GreaterThan(0));
        }

        [Test, Order(4)]
        public void DeleteMovie_WithValidId_ShouldReturnSuccess()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", createdMovieId!);

            RestResponse response = client.Execute(request);
            ApiResponseDto? result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(result!.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Test, Order(5)]
        public void CreateMovie_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Create", Method.Post);
            request.AddJsonBody(new
            {
                title = "",
                description = ""
            });

            RestResponse response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test, Order(6)]
        public void EditMovie_WithInvalidId_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "nonexistentid123");
            request.AddJsonBody(new
            {
                title = "Some Title",
                description = "Some Description"
            });

            RestResponse response = client.Execute(request);
            ApiResponseDto? result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result!.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Test, Order(7)]
        public void DeleteMovie_WithInvalidId_ShouldReturnBadRequest()
        {
            RestRequest request = new RestRequest("/api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "nonexistentid123");

            RestResponse response = client.Execute(request);
            ApiResponseDto? result = JsonSerializer.Deserialize<ApiResponseDto>(response.Content!);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(result!.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            client?.Dispose();
        }
    }
}