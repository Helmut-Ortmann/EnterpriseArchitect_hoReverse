using System;
using EA;

// ReSharper disable once CheckNamespace
namespace hoReverse.hoUtils.RUN
{

    public class Run
    {
        private Repository _rep;
        private DateTime _start;

        public Run(Repository rep)
        {
            _rep = rep;
            _start = new DateTime();

        }
        
    }
}
