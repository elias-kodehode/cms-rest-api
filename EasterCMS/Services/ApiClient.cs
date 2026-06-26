using EasterCMS.Entities;

namespace EasterCMS.Services;

public class ApiClient(
    IHttpClientFactory httpClientFactory,
    ILogger<ApiClient> logger)
{
    readonly HttpClient _http = httpClientFactory.CreateClient("api");


    public async Task<List<Participant>> GetParticipants()
    {
        var result = await _http.GetFromJsonAsync<GetParticipantsResponse>("participants");
        return result.Participants;
    }

    public async Task<Participant> GetParticipant(Guid id)
    {
        var result = await _http.GetFromJsonAsync<GetParticipantResponse>("participants");
        return result.Participant;
    }

    public async Task CreateParticipant(CreateParticipantRequest request)
    {
        var response = await _http.PostAsJsonAsync("/participants", request);

        if(!response.IsSuccessStatusCode) {
            logger.LogError("{Code} CreateParticipant operation failed", response.StatusCode);
            return;
        }


    }
}


public record CreateParticipantRequest(
        string FullName,
        int Age,
        string City
    );
    record GetParticipantResponse(Participant Participant);
    record GetParticipantsResponse(List<Participant> Participants);

