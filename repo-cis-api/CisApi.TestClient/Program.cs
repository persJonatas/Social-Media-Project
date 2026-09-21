using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using CisApi.TestClient;

// parse arg
int userCount = 3;
if (args.Length >= 2 && args[0] == "--users" && int.TryParse(args[1], out int parsedCount))
{
    userCount = parsedCount;
}

Console.WriteLine($"starting simulation with {userCount} users...");

var usersHttpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8080") };
var cisHttpClient = new HttpClient { BaseAddress = new Uri("http://localhost:8081") };

var random = new Random();
var users = new List<SimulatedUser>();

// stats
int successTopics = 0, failedTopics = 0;
int successIdeas = 0, failedIdeas = 0;
int successVotes = 0, failedVotes = 0;
int authErrors = 0, integrationErrors = 0;

var topicIds = new List<string>();
var ideaIds = new List<string>();

// step 1: register and login
for (int i = 0; i < userCount; i++)
{
    var id = Guid.NewGuid().ToString()[..8];
    var su = new SimulatedUser
    {
        Login = $"sim_{id}",
        Password = "password123"
    };

    var req = new UserRequestDto { Name = $"sim user {id}", Login = su.Login, Password = su.Password };
    
    try
    {
        var regRes = await usersHttpClient.PostAsJsonAsync("/api/v1/users", req);
        if (!regRes.IsSuccessStatusCode)
        {
            integrationErrors++;
        }

        var logReq = new LoginRequestDto { Login = su.Login, Password = su.Password };
        var logRes = await usersHttpClient.PostAsJsonAsync("/api/v1/auth/login", logReq);

        if (logRes.IsSuccessStatusCode)
        {
            var tokenData = await logRes.Content.ReadFromJsonAsync<TokenResponseDto>();
            if (tokenData != null && !string.IsNullOrEmpty(tokenData.Token))
            {
                su.Token = tokenData.Token;
                users.Add(su);
            }
            else 
            {
                authErrors++;
            }
        }
        else 
        {
            authErrors++;
        }
    }
    catch 
    {
        integrationErrors++;
    }
}

if (!users.Any())
{
    Console.WriteLine("could not authenticate any simulated users.");
    return;
}

// helper to shoot requests
async Task<string?> PostAsync<T>(string route, T payload, string token)
{
    using var msg = new HttpRequestMessage(HttpMethod.Post, route);
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
    msg.Content = JsonContent.Create(payload);
    
    var res = await cisHttpClient.SendAsync(msg);
    if (!res.IsSuccessStatusCode) return null;

    try
    {
        var node = await res.Content.ReadFromJsonAsync<JsonNode>();
        return node?["id"]?.ToString() ?? node?["topicId"]?.ToString() ?? node?["ideaId"]?.ToString() ?? "ok";
    }
    catch
    {
        return "ok";
    }
}

// step 2: random operations
int operations = userCount * 3; // random iterations

for (int i = 0; i < operations; i++)
{
    var u = users[random.Next(users.Count)];
    int op = random.Next(3); // 0=topic, 1=idea, 2=vote

    switch (op)
    {
        case 0:
            // due to usepathbase + controller route
            var tRes = await PostAsync("/cis-api/v1/cis-api/v1/topics", new CreateTopicRequest { Title = $"topic {Guid.NewGuid().ToString()[..4]}", Description = "generated topic" }, u.Token);
            if (tRes != null && tRes != "ok")
            {
                topicIds.Add(tRes);
                successTopics++;
            }
            else failedTopics++;
            break;

        case 1:
            if (!topicIds.Any()) break;
            var tid = topicIds[random.Next(topicIds.Count)];
            var iRes = await PostAsync($"/cis-api/v1/topics/{tid}/ideas", new CreateIdeaRequestDto { Title = $"idea {Guid.NewGuid().ToString()[..4]}", Description = "generated idea", TopicId = tid }, u.Token);
            if (iRes != null && iRes != "ok")
            {
                ideaIds.Add(iRes);
                successIdeas++;
            }
            else failedIdeas++;
            break;

        case 2:
        {
            if (!ideaIds.Any()) break;
            var iid = ideaIds[random.Next(ideaIds.Count)];
            
            using var msg = new HttpRequestMessage(HttpMethod.Post, $"/cis-api/v1/cis-api/v1/ideas/{iid}/votes");
            msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", u.Token);
            msg.Content = JsonContent.Create(new { IdeaId = iid });
            
            var vRes = await cisHttpClient.SendAsync(msg);
            if (vRes.IsSuccessStatusCode) successVotes++;
            else failedVotes++;
            break;
        }
    }
}

// randomly remove vote
if (ideaIds.Any() && users.Any())
{
    var u = users[random.Next(users.Count)];
    var iid = ideaIds[random.Next(ideaIds.Count)];
    using var msg = new HttpRequestMessage(HttpMethod.Delete, $"/cis-api/v1/cis-api/v1/ideas/{iid}/votes");
    msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", u.Token);
    await cisHttpClient.SendAsync(msg);
}

// final report
Console.WriteLine("\n--- execution report ---");
Console.WriteLine($"simulated users requested: {userCount}");
Console.WriteLine($"authenticated users: {users.Count}");
Console.WriteLine($"topics created   -> success: {successTopics} | failed: {failedTopics}");
Console.WriteLine($"ideas created    -> success: {successIdeas} | failed: {failedIdeas}");
Console.WriteLine($"votes cast       -> success: {successVotes} | failed: {failedVotes}");
Console.WriteLine($"auth errors: {authErrors}");
Console.WriteLine($"integration errs: {integrationErrors}");
Console.WriteLine("------------------------\n");
