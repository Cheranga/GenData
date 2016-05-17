﻿namespace GenData.Web.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Address Address { get; set; }
        public Result<Address> Result { get; set; }
    }


}
