using AutoMapper;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Contracts.Media;
using DatabaseServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.Authentication;
using Models.Phones;
using Newtonsoft.Json.Linq;
using Api.DependencyInjections.Authentication;
using Api.DependencyInjections.S3;
using Contracts.GroupingStuff;
using Models.GroupingStuff;
using Contracts.Authentication.Identity.Create;

namespace Api.Controllers.groupings
{
    [ApiController]
    [Authorize(Roles = "Admin, User")]
    [Route("[controller]")]
    public class GroupingController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly IAuthManager _authManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<GroupingController> _logger;
        private readonly IMapper _iMapper;
        private readonly IConfiguration _configuration;
        private readonly IS3Helper _s3Helper;

        public GroupingController(AppDbContext appDbContext, IAuthManager authManager,
            UserManager<ApplicationUser> userManager, ILogger<GroupingController> logger, IMapper iMapper, IConfiguration configuration, IS3Helper s3Helper)
        {
            _appDbContext = appDbContext;
            _authManager = authManager;
            _userManager = userManager;
            _logger = logger;
            _iMapper = iMapper;
            _configuration = configuration;
            _s3Helper = s3Helper;
        }


        [HttpGet]
        [Route("doesgroupingExist")]

        public async Task<IActionResult> DoesgroupingExist([FromQuery] string groupingName)
        {
            bool exists = await _appDbContext.Groupings.FirstOrDefaultAsync(x => x.Name == groupingName) != null;

            return Ok(exists);

        }

        [HttpPost]
        [Route("creategrouping")]

        public async Task<IActionResult> Creategrouping([FromBody] GroupingRegistrationRequest groupingRegistrationRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Create grouping on database ATTEMPT for " +
                "grouping name {GroupingName} BY {User}",
            HttpContext.GetEndpoint(),
                     HttpContext.Request.Method,
                     groupingRegistrationRequest.Name,
                     user.Email);

            user = await _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);


            //if (user.Groupings != null && user.Groupings.Count > 0)
            //    return BadRequest("User has max grouping count");

            if (user.Groupings == null)
                user.Groupings = new List<Grouping>();
            bool exists = await _appDbContext.Groupings.FirstOrDefaultAsync(x => x.Name.ToLower() == groupingRegistrationRequest.Name.ToLower()) != null;

            if (exists)
                return BadRequest("Name taken");

            Grouping grouping = _iMapper.Map<Grouping>(groupingRegistrationRequest);

            grouping.Id = Guid.NewGuid().ToString();
            grouping.User = user;
          //  grouping.UserId = user.Id;
            grouping.BannerImagePath = string.Empty;
            user.Groupings.Add(grouping);
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Create grouping on database SUCCESS for " +
              "grouping name {GroupingName} BY {User}",
          HttpContext.GetEndpoint(),
                   HttpContext.Request.Method,
                   groupingRegistrationRequest.Name,
                   user.Email);
            return Ok();


        }

        [HttpPut]
        [Route("updategrouping")]

        public async Task<IActionResult> Updategrouping([FromBody] GroupingUpdateRequest groupingUpdateRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Update grouping on database ATTEMPT for " +
          "grouping name {GroupingName} BY {User}",
      HttpContext.GetEndpoint(),
               HttpContext.Request.Method,
               groupingUpdateRequest.Name,
               user.Email);
            user = await _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id);

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (user.Groupings == null)
                return BadRequest($"User has no groupings");

            Grouping grouping = user.Groupings.FirstOrDefault(x => x.Id == groupingUpdateRequest.Id);
            if (grouping == null)
                return BadRequest($"grouping with id {groupingUpdateRequest.Id} not found");

            if (grouping.User != user)
                return BadRequest("User must own grouping to update it");
            grouping = _iMapper.Map<Grouping>(groupingUpdateRequest);


            _appDbContext.Update(user);
            var result = await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Update grouping on database SUCCESS for " +
         "grouping name {GroupingName} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              groupingUpdateRequest.Name,
              user.Email);
            return Ok(result);

        }

        [HttpGet]
        [Route("getgroupingsForUser")]

        public async Task<IActionResult> GetgroupingesForUser([FromQuery] string userId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Groupings for user on database ATTEMPT for " +
         " userId {UserId} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              userId,
              user.Email);


            List<Grouping> groupings = _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id.ToString() == userId).Result.Groupings;

            return Ok(groupings);


        }
        [HttpGet]
        [Route("getgrouping")]

        public async Task<IActionResult> Getgrouping()
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Groupings for user on database ATTEMPT for " +
         " userId {UserId} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
              user.Email,
              user.Email);
            var usr = await _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id);
            var groupings = usr.Groupings;
            List<GroupingDTO> groupingDTOs = _iMapper.Map<List<Grouping>, List<GroupingDTO>>(groupings);

            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Get Groupings for user on database SUCCESS for " +
        " userId {UserId} BY {User}",
    HttpContext.GetEndpoint(),
             HttpContext.Request.Method,
             user.Email,
             user.Email);
            return Ok(groupingDTOs);


        }
        /// <summary>
        /// User must be owner of grouping to delete it
        /// </summary>
        /// <param name="groupingId"></param>
        /// <returns></returns>

        [HttpDelete]
        [Route("deletegrouping")]
        public async Task<IActionResult> Deletegrouping([FromQuery] string groupingId)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete grouping on database ATTEMPT for " +
         "grouping id {GroupingId} BY {User}",
     HttpContext.GetEndpoint(),
              HttpContext.Request.Method,
             groupingId,
              user.Email);
            List<Grouping> groupings = _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Groupings;

            Grouping grouping = groupings.FirstOrDefault(x => x.Id == groupingId);

            if (grouping == null)
                return NotFound($"User does not have grouping with id{groupingId}");
            if (grouping.User != user)
                return BadRequest("Must be owner of grouping to delete it.");
            _appDbContext.Groupings.Remove(grouping);
            var result = await _appDbContext.SaveChangesAsync();
            _logger.LogInformation("Controller: {Controller_Action}, HTTP Method: {Http_Method}, Message: Delete grouping on database SUCCESS for " +
      "grouping id {GroupingId} BY {User}",
  HttpContext.GetEndpoint(),
           HttpContext.Request.Method,
          groupingId,
           user.Email);
            return Ok(result);
        }

        [HttpPost]
        [Route("getUploadSignature")]

        public async Task<IActionResult> GetUploadSignature([FromBody] ImageUploadSignatureRequest imageUploadRequest)
        {
            ApplicationUser user = await _authManager.VerifyRefreshTokenAndReturnUser(Request);
            if (user == null)
                return Unauthorized("Invalid refresh token");

            List<Grouping> groupings = _appDbContext.Users.Include("Groupings").FirstOrDefaultAsync(x => x.Id == user.Id).Result.Groupings;

            Grouping grouping = groupings.FirstOrDefault(x => x.Id == imageUploadRequest.GroupingId);

            if (grouping == null)
                return NotFound($"User does not have grouping with id{imageUploadRequest.GroupingId}");



            var filename = GenerateFileName(imageUploadRequest.FileName);


            string presignedUrl = _s3Helper.GetPreSignedUrl(user.Id.ToString(),imageUploadRequest.FileName,grouping.Id);


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

        //    List<grouping> groupings = _appDbContext.Users.Include("groupings").FirstOrDefaultAsync(x => x.Id == user.Id).Result.groupings;

        //    grouping grouping = groupings.FirstOrDefault(x => x.Id == bannerUploadRequest.groupingId);

        //    if (grouping == null)
        //        return NotFound($"User does not have grouping with id{bannerUploadRequest.groupingId}");


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
