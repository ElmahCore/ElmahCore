using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ElmahCore;

public interface IErrorFactory
{
    Task<Error> CreateAsync(Exception exception, HttpContext? context);
}
