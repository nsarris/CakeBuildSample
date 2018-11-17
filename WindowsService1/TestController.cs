using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WindowsService1
{
    public class TestController : ApiController
    {
        [HttpGet]
        public string GetTime() => CommonLibrary.Common.GetTime().ToString();
    }
}