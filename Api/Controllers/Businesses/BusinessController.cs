using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Contracts.BusinessStuff;
using Contracts.Media;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.BusinessStuff;
using Models.Phones;
using Newtonsoft.Json.Linq;
using Api.DependencyInjections.Authentication;
using Api.DependencyInjections.S3;

namespace Api.Controllers.Businesses
{
    [ApiController]
    [Authorize(Roles = "Admin, User")]
    [Route("[controller]")]
    public class BusinessController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _iMapper;
        private readonly IConfiguration _configuration;
        private readonly IS3Helper _s3Helper;

        public BusinessController(AppDbContext appDbContext, IAuthManager authManager,
            UserManager<ApplicationUser> userManager, IMapper iMapper, IConfiguration configuration, IS3Helper s3Helper)
        {
            _appDbContext = appDbContext;
            _authManager = authManager;
            _userManager = userManager;
            _iMapper = iMapper;
            _configuration = configuration;
            _s3Helper = s3Helper;
        }


        [HttpGet]
        [Route("doesBusinessExist")]

        public async Task<IActionResult> DoesBusinessExist([FromQuery] string businessName)
        {
            bool exists = await _appDbContext.Businesses.FirstOrDefaultAsync(x => x.Name == businessName) != null;

            return Ok(exists);

        }

        [HttpPost]
        [Route("createBusiness")]

        public async Task<IActionResult> CreateBusiness([FromBody] BusinessRegistrationRequest businessRegistrationRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            user = await _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            if (user.Businesses != null && user.Businesses.Count > 0)
                return BadRequest("User has max business count");

            if (user.Businesses == null)
                user.Businesses = new List<Business>();
            bool exists = await _appDbContext.Businesses.FirstOrDefaultAsync(x => x.Name.ToLower() == businessRegistrationRequest.Name.ToLower()) != null;

            if (exists)
                return BadRequest("Name taken");

            Business business = _iMapper.Map<Business>(businessRegistrationRequest);

            business.Id = Guid.NewGuid().ToString();
            business.User = user;
            business.UserId = user.Id;
            business.BannerImagePath = string.Empty;
            user.Businesses.Add(business);
            await _userManager.UpdateAsync(user);

            //var result = await _appDbContext.Businesses.AddAsync(business);
            // await _appDbContext.SaveChangesAsync();

            return Ok();


        }

        [HttpPut]
        [Route("updateBusiness")]

        public async Task<IActionResult> UpdateBusiness([FromBody] BusinessUpdateRequest businessUpdateRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            user = await _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (user.Businesses == null)
                return BadRequest($"User has no businesses");

            Business business = user.Businesses.FirstOrDefault(x => x.Id == businessUpdateRequest.Id);
            if (business == null)
                return BadRequest($"Business with id {businessUpdateRequest.Id} not found");

            if (business.User != user)
                return BadRequest("User must own business to update it");
            business = _iMapper.Map<Business>(businessUpdateRequest);


            _appDbContext.Update(user);
            var result = await _appDbContext.SaveChangesAsync();

            return Ok(result);

        }

        [HttpGet]
        [Route("getBusinessesForUser")]

        public async Task<IActionResult> GetBusinessesForUser([FromQuery] string userId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            List<Business> businesses = _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id.ToString() == userId).Result.Businesses;

            return Ok(businesses);


        }
        [HttpGet]
        [Route("getBusiness")]

        public async Task<IActionResult> GetBusiness()
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            var usr = await _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id);
            var busineses = usr.Businesses;
            List<BusinessDTO> businessDTOs = _iMapper.Map<List<Business>, List<BusinessDTO>>(busineses);
            return Ok(businessDTOs);


        }
        /// <summary>
        /// User must be owner of business to delete it
        /// </summary>
        /// <param name="businessId"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("deleteBusiness")]
        public async Task<IActionResult> DeleteBusiness([FromQuery] string businessId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            List<Business> businesses = _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Businesses;

            Business business = businesses.FirstOrDefault(x => x.Id == businessId);

            if (business == null)
                return NotFound($"User does not have business with id{businessId}");
            if (business.User != user)
                return BadRequest("Must be owner of business to delete it.");
            _appDbContext.Businesses.Remove(business);
            var result = await _appDbContext.SaveChangesAsync();

            return Ok(result);
        }

        [HttpPost]
        [Route("getUploadSignature")]

        public async Task<IActionResult> GetUploadSignature([FromBody] ImageUploadSignatureRequest imageUploadRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            List<Business> businesses = _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Businesses;

            Business business = businesses.FirstOrDefault(x => x.Id == imageUploadRequest.BusinessId);

            if (business == null)
                return NotFound($"User does not have business with id{imageUploadRequest.BusinessId}");



            var filename = GenerateFileName(imageUploadRequest.FileName);


            string presignedUrl = _s3Helper.GetPreSignedUrl(user.Id.ToString(),imageUploadRequest.FileName,business.Id);


            return Ok(new SignatureResponse() { Url = presignedUrl});

        }

        private class SignatureResponse
        {
            public string Url { get; set; }
        }

        //[HttpPost]
        //[Route("uploadBanner")]

        //public async Task<IActionResult> uploadBanner([FromBody] BannerUploadRequest bannerUploadRequest)
        //{
        //    ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
        //    if (user == null)
        //        return Unauthorized("Invalid refresh token");

        //    List<Business> businesses = _appDbContext.Users.Include("Businesses").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Businesses;

        //    Business business = businesses.FirstOrDefault(x => x.Id == bannerUploadRequest.BusinessId);

        //    if (business == null)
        //        return NotFound($"User does not have business with id{bannerUploadRequest.BusinessId}");


        //    string keyVaultName = _configuration.GetSection("Azure").GetSection("KeyVaultName").Value;
        //    string kvUri = "https://" + keyVaultName + ".vault.azure.net";
        //    SecretClient secretClient = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());

        //    var filename = GenerateFileName(bannerUploadRequest.File.Name, user.Email);
        //    var fileUrl = "";

        //    string blobContainerSecret = secretClient.GetSecret("switchcount-blob-production").Value.Value;

        //    BlobContainerClient container = new BlobContainerClient(blobContainerSecret, "production");



        //    BlobClient blob = container.GetBlobClient(filename);
        //    using (Stream stream = bannerUploadRequest.File.OpenReadStream())
        //    {
        //        blob.Upload(stream);
        //    }
        //    fileUrl = blob.Uri.AbsoluteUri;

        //    var result = fileUrl;
        //    return Ok(result);

        //}
        private string GenerateFileName(string fileName)
        {
            try
            {
                string strFileName = string.Empty;
                string[] strName = fileName.Split('.');
                strFileName = strName[0] + "-" + Guid.NewGuid() + "-" + DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd") + "/"
                   + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." +
                   strName[strName.Length - 1];
                return strFileName;
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }
    }
}
