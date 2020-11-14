using DeviceId;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace AccessBankTask.Helpers
{
    public class Utility
    {
        //gets ipaddress
        [Obsolete]
        public static string GetLocalIPAddress()
        {
            //return new DeviceIdBuilder()
            //.AddMachineName()
            //.AddMacAddress()
            //.AddProcessorId()
            //.AddMotherboardSerialNumber()
            //.ToString();
            var hostName = Dns.GetHostName();
            return Dns.GetHostByName(hostName).AddressList[0].ToString();

            //var host = Dns.GetHostEntry(Dns.GetHostName());
            //foreach (var ip in host.AddressList)
            //{
            //    if (ip.AddressFamily == AddressFamily.InterNetwork)
            //    {
            //        return ip.ToString();
            //    }
            //}
            //throw new Exception("No network adapters with an IPv4 address in the system!");
        }


        // get user if from token
        public static string GetUserIdFromToken(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].ToString().Split(" ")[1];
            var handler = new JwtSecurityTokenHandler();

            var claims = handler.ReadJwtToken(token).Claims.Take(1);
            string userId = string.Empty;
            foreach (var claim in claims)
            {
                userId += claim.Value;
            }


            return userId;
        }
    }
}
