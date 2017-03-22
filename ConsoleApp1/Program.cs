﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Reflection;
using System.Threading.Tasks;

namespace ZabbixAPICore
{
    class Program
    {
        static void Main(string[] args)
        {
            Zabbix zabbix = new Zabbix("admin", "zabbix", "http://192.168.1.15/api_jsonrpc.php");
            zabbix.Login().Wait();

            Task<Response> responseObj = zabbix.objectResponse("user.create", new
            {
                alias = "joe.kowalsky",
                passwd = "password",
                name = "Joe",
                surname = "Kowalsky",
                autologin = 1,
                autologout = 0,
                type = 2,
                usrgrps = new[]
                {
                    new
                    {
                        usrgrpid = 7
                    }
                },
                user_medias = new[]
                {
                    new
                    {
                        mediatypeid = 1,
                        sendto = "joe.kowalsky@example.com",
                        active = 0,
                        severity = 63,
                        period = "1-7,00:00-24:00"
                    }
                }
            });

            if(responseObj.Result.error != null)
            {
                var error = responseObj.Result.error;
                Console.WriteLine(error.GetErrorMsg());
            }
            else
            {
                string userid = responseObj.Result.result.userids[0];
                Console.WriteLine(userid);
            }

            zabbix.Logout().Wait();

            Console.ReadKey();
        }
    }
}