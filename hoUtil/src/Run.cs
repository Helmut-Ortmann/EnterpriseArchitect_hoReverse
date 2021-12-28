using System;
using EA;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.RUN
{

    public class Run
    {
        // ReSharper disable once NotAccessedField.Local
        private Repository _rep;
        // ReSharper disable once NotAccessedField.Local
        private readonly DateTime _start;

        public Run(Repository rep)
        {
            _rep = rep;
            _start = new DateTime();

        }
        
    }
}
