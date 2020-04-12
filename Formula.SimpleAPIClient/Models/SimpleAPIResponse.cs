using System;
using System.Collections.Generic;

namespace Formula.SimpleAPIClient
{
    public class SimpleAPIResponse<TData>
    {
        public Boolean IsSuccessful { get; set; }
        public String Message { get; set; }
        public TData Data { get; set; }
        public Dictionary<String, String> Details { get; set; }
    }
}
