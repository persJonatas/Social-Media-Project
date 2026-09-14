using System;

namespace CisApi.Business.Interfaces
{
    public interface ICurrentUserService
    {
        Guid GetUserId();
    }
}