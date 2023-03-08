using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]

namespace RestfulAPITest_HTTP_Method
{
    [TestClass]
    public class HttpClientTest
    {
        private static HttpClient httpClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public void TestInitialize()
        {
            httpClient = new HttpClient();
        }

        [TestCleanup]
        public async Task TestCleanUp()
        {
            foreach (var data in cleanUpList)
            {
                var httpResponse = await httpClient.DeleteAsync(GetURL($"{PetEndpoint}/{data}"));
            }
        }

        [TestMethod]
        public async Task PutMethod()
        {
            #region create data

            // Create Json Object
            PetModel petData = new PetModel()
            {
                Id = 22377,
                Category = new Category()
                {
                    Id=0,
                    Name="testCategory"
                },
                Name = "testPetName",
                PhotoUrls = new string[]
                { 
                    "https://images.app.goo.gl/J8mwktStNPomuzfp6"
                },
                Tags = new Category[]
                {
                   new Category() { Id = 0, Name = "testTags" }
                },
                Status = "testStatus"
            };

            // Serialize Content
            var request = JsonConvert.SerializeObject(petData);
            var postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Post Request
            await httpClient.PostAsync(GetURL(PetEndpoint), postRequest);

            #endregion

            #region get Information of the created data

            // Get Request
            var getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));

            // Deserialize Content
            var listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            var createdPetData = listPetData.Name;

            #endregion

            #region send put request to update data

            // Update value of petData
            petData = new PetModel()
            {
                Id = petData.Id,
                Category = new Category()
                {
                    Id = petData.Category.Id,
                    Name = petData.Category.Name
                },
                Name = "testPetName.update",
                PhotoUrls = new string[]
                {
                    petData.PhotoUrls[0]
                },
                Tags = new Category[]
                {
                   new Category() { Id = petData.Tags[0].Id, Name = petData.Tags[0].Name }
                },
                Status = "pending"
            };

            // Serialize Content
            request = JsonConvert.SerializeObject(petData);
            postRequest = new StringContent(request, Encoding.UTF8, "application/json");

            // Send Put Request
            var httpResponse = await httpClient.PutAsync(GetURL($"{PetEndpoint}"), postRequest);

            // Get Status Code
            var statusCode = httpResponse.StatusCode;

            #endregion

            #region get updated data

            // Get Request
            getResponse = await httpClient.GetAsync(GetURI($"{PetEndpoint}/{petData.Id}"));

            // Deserialize Content
            listPetData = JsonConvert.DeserializeObject<PetModel>(getResponse.Content.ReadAsStringAsync().Result);

            // filter created data
            createdPetData = listPetData.Name;

            #endregion

            #region cleanup data

            // Add data to cleanup list
            cleanUpList.Add(listPetData);

            #endregion

            #region assertion

            // Assertion
            Assert.AreEqual(HttpStatusCode.OK, statusCode, "Status code is not equal to 201");
            Assert.AreEqual(petData.Name, createdPetData, "Name not matching");

            #endregion

        }
    }
}

   