using Newtonsoft.Json;
using RestfulAPITest_HTTP_Method;
using RestSharp;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Xml.Linq;
using static System.Net.WebRequestMethods;

[assembly: Parallelize(Workers = 10, Scope = ExecutionScope.MethodLevel)]

namespace RestfulAPITest_RestSharp_Method
{
    [TestClass]
    public class RestSharpTest
    {
        private static RestClient restClient;

        private static readonly string BaseURL = "https://petstore.swagger.io/v2/";

        private static readonly string PetEndpoint = "pet";

        private static string GetURL(string enpoint) => $"{BaseURL}{enpoint}";

        private static Uri GetURI(string endpoint) => new Uri(GetURL(endpoint));

        private readonly List<PetModel> cleanUpList = new List<PetModel>();

        [TestInitialize]
        public async Task TestInitialize()
        {
            restClient = new RestClient();
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            foreach (var data in cleanUpList)
            {
                var restRequest = new RestRequest(GetURI($"{PetEndpoint}/{data.Id}"));
                var restResponse = await restClient.DeleteAsync(restRequest);
            }
        }

        [TestMethod]
        public async Task PostMethod()
        {
            #region CreateUser
            //Create User
            var newPet = new PetModel()
            {
                Id = 0,
                Category = new Category()
                {
                    Id = 9876,
                    Name = "Small Breed"
                },
                Name = "Bruno",
                PhotoUrls = new string[]
                {
                    "https://images.app.goo.gl/J8mwktStNPomuzfp6"
                },
                Tags = new Category[]
                {
                   new Category() { Id = 9876, Name = "Dachschund" }
                },
                Status = "Available"
            };

            // Send Post Request
            var temp = GetURI(PetEndpoint);
            var postRestRequest = new RestRequest(GetURI(PetEndpoint)).AddJsonBody(newPet);
            var postRestResponse = await restClient.ExecutePostAsync<PetModel>(postRestRequest);

            newPet.Id = postRestResponse.Data.Id;

            //Verify POST request status code
            Assert.AreEqual(HttpStatusCode.OK, postRestResponse.StatusCode, "Status code is not equal to 200");
            #endregion

            #region GetPet
            var restRequest = new RestRequest(GetURI($"{PetEndpoint}/{newPet.Id}"), Method.Get);
            var restResponse = await restClient.ExecuteAsync<PetModel>(restRequest);
            #endregion

            #region CleanUp
            cleanUpList.Add(newPet);
            #endregion

            #region Assertions
            Assert.AreEqual(HttpStatusCode.OK, restResponse.StatusCode, "Status code is not equal to 200");
            Assert.AreEqual(newPet.Name, restResponse.Data.Name, "Category name did not match.");
            Assert.AreEqual(newPet.Category.Name, restResponse.Data.Category.Name, "Id and Name did not match.");
            Assert.AreEqual(newPet.PhotoUrls[0], restResponse.Data.PhotoUrls[0], "Could not find URL.");
            Assert.AreEqual(newPet.Tags[0].Name, restResponse.Data.Tags[0].Name, "Tag name did not match.");
            Assert.AreEqual(newPet.Status, restResponse.Data.Status, "Status did not match.");
            #endregion

        }
    }
}
