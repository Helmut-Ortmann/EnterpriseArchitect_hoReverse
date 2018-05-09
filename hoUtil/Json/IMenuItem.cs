﻿namespace hoUtils.Json
{
    /// <summary>
    /// To access the information needed to create a MenuItem
    /// </summary>
    public interface IMenuItem
    {
        string Name { get;  }
        string Description { get;  }
        string ListNo { get; }
    }
}
