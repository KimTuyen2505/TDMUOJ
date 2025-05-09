using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace TDMUOJ
{
    public static class CorsConfig
    {
        public static void Register(HttpConfiguration config)
        {
            var cors = new EnableCorsAttribute(
                origins: "https://localhost:44388",
                headers: "*",
                methods: "GET,POST,PUT,DELETE,OPTIONS"
            );

            config.EnableCors(cors);
        }
    }
}
