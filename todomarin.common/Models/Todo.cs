﻿using System;

namespace todomarin.common.Models
{
    public class Todo
    {
        public DateTime CreatedTime { get; set; }
        public string TaskDescription { get; set; }
        public bool IsCompleted { get; set; }
    }
}
