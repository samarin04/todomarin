﻿using System;
using System.Collections.Generic;
using System.Text;

namespace todomarin.common.Responses
{
    public class Response
    {
        public bool IsSucess { get; set; }
        public string Message { get; set; }
        public object Result { get; set; }
    }
}
