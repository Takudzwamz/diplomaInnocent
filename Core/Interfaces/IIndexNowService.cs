using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Interfaces;

public interface IIndexNowService
{
    /// <summary>
    /// Submits a list of URLs to the IndexNow API.
    /// </summary>
    /// <param name="urls">A list of full URLs to submit.</param>
    Task SubmitUrlsAsync(List<string> urls);
}