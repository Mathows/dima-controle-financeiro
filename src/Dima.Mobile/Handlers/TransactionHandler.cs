using System.Net.Http.Json;
using Dima.Core.Handlers;
using Dima.Core.Models;
using Dima.Core.Requests.Transactions;
using Dima.Core.Responses;

namespace Dima.Mobile.Handlers;

public class TransactionHandler(IHttpClientFactory httpClientFactory) : ITransactionHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<Transaction?>> CreateAsync(CreateTransactionRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/transactions", request);
        return await result.Content.ReadFromJsonAsync<Response<Transaction?>>()
               ?? new Response<Transaction?>(null, 400, "Falha ao criar lançamento");
    }

    public async Task<Response<Transaction?>> UpdateAsync(UpdateTransactionRequest request)
    {
        var result = await _client.PutAsJsonAsync($"v1/transactions/{request.Id}", request);
        return await result.Content.ReadFromJsonAsync<Response<Transaction?>>()
               ?? new Response<Transaction?>(null, 400, "Falha ao atualizar lançamento");
    }

    public async Task<Response<Transaction?>> DeleteAsync(DeleteTransactionRequest request)
    {
        var result = await _client.DeleteAsync($"v1/transactions/{request.Id}");
        return await result.Content.ReadFromJsonAsync<Response<Transaction?>>()
               ?? new Response<Transaction?>(null, 400, "Falha ao excluir lançamento");
    }

    public async Task<Response<Transaction?>> GetByIdAsync(GetTransactionByIdRequest request)
        => await _client.GetFromJsonAsync<Response<Transaction?>>($"v1/transactions/{request.Id}")
           ?? new Response<Transaction?>(null, 400, "Não foi possível obter o lançamento");

    public async Task<PagedResponse<List<Transaction>?>> GetByPeriodAsync(GetTransactionsByPeriodRequest request)
    {
        var startDate = request.StartDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var endDate = request.EndDate?.ToString("yyyy-MM-dd") ?? string.Empty;
        var url = $"v1/transactions?startDate={startDate}&endDate={endDate}&pageNumber={request.PageNumber}&pageSize={request.PageSize}";
        return await _client.GetFromJsonAsync<PagedResponse<List<Transaction>?>>(url)
               ?? new PagedResponse<List<Transaction>?>(null, 400, "Não foi possível obter os lançamentos");
    }
}
