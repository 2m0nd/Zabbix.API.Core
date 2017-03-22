﻿using System;
using System.Dynamic;
using System.Threading.Tasks;

namespace ZabbixAPICore
{
    public class Response
    {
        public int id { get; set; }
        public string jsonrpc { get; set; }
        public dynamic result = new ExpandoObject();
        public Error error { get; set; }
    }
}