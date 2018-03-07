using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FileTracking.Services.BO;
using static FileTracking.Client.Common.WSCommonObject;
using System.Web;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FileTracking.Services.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class FileController : Controller
    {
        [HttpGet("[controller]/[action]/{username}/{password}/{id}")]
        public ActionResult DownloadFile(string username, string password, string id)
        {
            Authentication user = new Authentication(username, username, username, username);
            //Need check permission every time
            string path = @"D:\CV_E_NTDINH.docx";

            // work with apples to build your file in memory
            byte[] file = System.IO.File.ReadAllBytes(path);

            Response.Headers.Add("Content -Disposition", "attachment; filename=downlaod.docx");
            return File(file, "pplication/ms-word");
        }
    }
}
