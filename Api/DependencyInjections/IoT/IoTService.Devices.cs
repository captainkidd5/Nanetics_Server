﻿using Contracts.Devices.IoT;
using Microsoft.Azure.Devices.Provisioning.Client;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace Api.DependencyInjections.IoT
{
    public partial class IoTService : IIotService
    {
        public async Task<IoTDeviceDTO> AddDevice(string deviceId)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Put, endPoint);
                AddApiAuthorization(msg);
                CreateDeviceRequestDTO createDeviceRequestDTO = new CreateDeviceRequestDTO()
                {
                    displayName = deviceId,
                    template = "dtmi:naneticshub:soil_sensorv2;1",
                    simulated = false,
                    enabled = false,
                    organizations =new string[] {}

                };
                string json = JsonConvert.SerializeObject(new
                {
                    displayName = deviceId,
                    template = "dtmi:Espressif:SensorController;2",
                   // organizations = new string[] {"Tech" },

                });

                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
              //  msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage result = await client.SendAsync(msg);
                if (!result.IsSuccessStatusCode)
                {
                    ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();

                    string errorMsg = response.message;
                    return null;

                }

                dynamic d = await result.Content.ReadFromJsonAsync<dynamic>();
                IoTDeviceDTO iotDevice = await result.Content.ReadFromJsonAsync<IoTDeviceDTO>();

                return iotDevice;
            }
            catch (Exception e)
            {
                return null;
            }


        }

        public async Task<HttpResponseMessage> GetDeviceCredentials(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}/credentials?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);


            string responseContent = await result.Content.ReadAsStringAsync();

            DeviceCredentials credentials = await result.Content.ReadFromJsonAsync<DeviceCredentials>();
            return result;

        }

        public async Task<HttpResponseMessage> GetDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            AddApiAuthorization(msg);

            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }
        public async Task<bool> DeleteDevice(string deviceId)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Delete, endPoint);
                AddApiAuthorization(msg);

                HttpResponseMessage result = await client.SendAsync(msg);
                return result.IsSuccessStatusCode;

            }
            catch (Exception ex)
            {
                return false;
            }


        }
        public async Task<HttpResponseMessage> QueryDevice(string deviceId)
        {
            HttpClient client = _httpClientFactory.CreateClient();
            string endPoint = _iotString + $"query?api-version={apiVersion}";

            HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Get, endPoint);
            HttpResponseMessage result = await client.SendAsync(msg);
            return result;

        }
        public async Task<bool> UpdateDevice(string deviceId, UpdateIoTDeviceRequest updateRequest)
        {
            try
            {
                HttpClient client = _httpClientFactory.CreateClient();
                string endPoint = _iotString + $"devices/{deviceId}?api-version={apiVersion}";

                HttpRequestMessage msg = new HttpRequestMessage(HttpMethod.Patch, endPoint);
                AddApiAuthorization(msg);
     
                //This is the same as UpdateIoTDeviceRequest, just typing it out for the help example
                string json = JsonConvert.SerializeObject(new {
                    displayName = updateRequest.displayName,
                   template = "dtmi:naneticshub:soil_sensorv2;1",
                    simulated = false,
                    enabled = true
                
                });

                msg.Content = new StringContent(json, Encoding.UTF8, "application/json");
              //  msg.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                HttpResponseMessage result = await client.SendAsync(msg);
                if (!result.IsSuccessStatusCode)
                {
                    ErrorDetails? response = await result.Content.ReadFromJsonAsync<ErrorDetails>();
                    string errorMsg = response.message;
                    return false;
                }

                IoTDeviceDTO iotDevice = await result.Content.ReadFromJsonAsync<IoTDeviceDTO>();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
