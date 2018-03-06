using System;

namespace SmokeTest.Models
{
    public class SmokeTestModel
    {
        public int Order { get; set; }
        public string Name { get; set; }
        public Func<ResultModel> Method { get; set; }

        public SmokeTestModel(int order, string name, Func<ResultModel> method)
        {
            Order = order;
            Name = name;
            Method = method;
        }
    }
}
