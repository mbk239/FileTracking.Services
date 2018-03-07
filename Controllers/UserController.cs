using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileTracking.Services.BO;
using static FileTracking.Client.Common.WSCommonObject;
using FileTracking.Client.Common;

namespace FileTracking.Services.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class UserController : Controller
    {
        [HttpGet("[controller]/[action]/{username}/{password}/{path}")]
        public IActionResult GetAllFilesInformation(string username, string password, string folderPath)
        {
            Authentication user = new Authentication(username, username, username, username);
            FilesBO oFiles = new FilesBO();
            List<FileMessage> result = new List<FileMessage>();           
            return new ObjectResult(result);
        }
    }

}
