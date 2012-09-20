namespace Terminal.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Variable
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}