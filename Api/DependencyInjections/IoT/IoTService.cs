﻿using Api.DependencyInjections.Azure;
using Contracts.Devices.IoT;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Devices;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Shared;
using Models.Authentication;
using Models.Devices;
using Newtonsoft.Json;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        private const string _iotString = "https://naneticshub.azureiotcentral.com/api/";
        private const string apiVersion = "2022-10-31-preview";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IKeyVaultRetriever _keyVaultRetriever;

        private readonly string _sasSignature;
        public IoTService(IHttpClientFactory httpClientFactory, IKeyVaultRetriever keyVaultRetriever)
        {
            _httpClientFactory = httpClientFactory;
            _keyVaultRetriever = keyVaultRetriever;
            string value = _keyVaultRetriever.RetrieveKey("IoTCentralApiToken").Value;
            _sasSignature = value.Split("SharedAccessSignature ")[1];
        }
        public async Task<HttpResponseMessage> CreateIoTApiToken()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string tokenId = Guid.NewGuid().ToString();
            string endPoint = _iotString + $"api/apiTokens/{tokenId}?api-version=2022-10-31-preview";
            // Create the role assignment array
            RoleAssignmentDTO[] roleAssignments = new RoleAssignmentDTO[]
            {
        new RoleAssignmentDTO { Role = "Admin" }
            };

            string json = JsonConvert.SerializeObject(roleAssignments);

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
            msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
            msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(_keyVaultRetriever.RetrieveKey("IoTCentralApiToken").Value);
            HttpResponseMessage result = await client.SendAsync(msg);
            if (result.IsSuccessStatusCode)
            {
                IoTAPITokenResponseDTO? response = await result.Content.ReadFromJsonAsync<IoTAPITokenResponseDTO>();
            }
            if (!result.IsSuccessStatusCode)
            {
                ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                string errorMsg = response.message;
            }
            return result;

        }
        //public async Task<HttpResponseMessage> CreateOperatorToken()
        //{
        //    HttpClient client = _httpClientFactory.CreateClient();
        //    string endPoint = _iotString + $"apiTokens/operator-token?api-version={apiVersion}";

        //    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
        //    HttpResponseMessage result = await client.SendAsync(msg);
        //    return result;

        //}

        //public async Task<HttpResponseMessage> CreateAdminToken()
        //{
        //    HttpClient client = _httpClientFactory.CreateClient();
        //    string endPoint = _iotString + $"apiTokens/admin-token?api-version={apiVersion}";

        //    HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
        //    HttpResponseMessage result = await client.SendAsync(msg);
        //    return result;

        //}
        private void AddApiAuthorization(HttpRequestMessage msg)
        {
            try
            {
                msg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedAccessSignature", _sasSignature);

            }
            catch (Exception ex)
            {
                Console.WriteLine("test");
            }


        }


        /// <summary>
        /// Creates a new device group with the user's id as the group id (max 1000, probably just do this on the portal)
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<bool> CreateGroup(ApplicationUser user)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"deviceGroups/{user.Id}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
                AddApiAuthorization(msg);

                CreateGroupRequestDTO requestDTO = new CreateGroupRequestDTO()
                {
                    DisplayName = user.Email.ToLower() + "_device_group",

                };
                string json = JsonConvert.SerializeObject(requestDTO);

                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
                HttpResponseMessage result = await client.SendAsync(msg);
                return result.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                return false;
            }

        }


        public async Task<HttpResponseMessage> GetIoTTemplates()
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"deviceTemplates?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);
            if (!result.IsSuccessStatusCode)
            {
                IoTResponseError? response = await result.Content.ReadFromJsonAsync<IoTResponseError>();

                string errorMsg = response.error.message;
                return null;

            }

            DeviceTemplateCollection iotDevice = await result.Content.ReadFromJsonAsync<DeviceTemplateCollection>();
            //dynamic iotDevice = await result.Content.ReadFromJsonAsync<dynamic>();

            return result;

        }

  
        public class DeviceTemplateCollection
        {

            public string nextLink { get; set; }
            public DeviceTemplate[] value { get; set; }
        }

        public class DeviceTemplate
        {
            public string id { get; set; }
            public string[] type { get; set; }
            public object capabilityModel { get; set; }
            public string description { get; set; }
            public string displayName { get; set; }
            public string etag { get; set; }
        }

    }
}
