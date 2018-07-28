using System;
using System.Linq;
using System.Threading.Tasks;
using Messenger.Client.Objects;
using Messenger.Client.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FacebookDemoBot.Controllers
{
    [Route("api/webhook")]
    public class FacebookController : Controller
    {
        const string accessToken = "EAAJNBQJOADABAOyaRaqURp6pQceFwhznjS6Sv0QERdfOgGrmW3U4LwcXaQqdBgqpTaVM6ZApxcZAamcwVLMthP5ZBjYHokDWyqGEuOOVeTaJRoy1ZB6IZAjFcqetuQxRki5rcpiJtoKmcWw8Idqfq6BJzrV1HP6qESHZCjpZCMhHAZDZD";

        private readonly IMessengerMessageSender messageSender;
        private readonly ILogger<FacebookController> logger;

        public FacebookController(IMessengerMessageSender messageSender, ILogger<FacebookController> logger)
        {
            this.messageSender = messageSender;
            this.logger = logger;
        }

        [HttpGet]
        [Route("ping")]
        public IActionResult Ping()
        {
            var date = DateTime.UtcNow;
            var message = $"Hello! Today is {date.DayOfWeek}, it's {date:HH:mm} now.";
            this.logger.LogInformation($"Ping requested. Sending message :'{message}'");
            return Ok(message);
        }


        // GET api/values
        [HttpGet]
        public IActionResult Get(
            [FromQuery(Name = "hub.mode")] string hubMode, 
            [FromQuery(Name = "hub.verify_token")] string hubVerifytoken, 
            [FromQuery(Name = "hub.challenge")] string hubChallenge)
        {
            string VERIFY_TOKEN = "vkmsmvls245awsdkfj42-3331";

            if(VERIFY_TOKEN == hubVerifytoken && hubMode == "subscribe")
            {
                return Ok(hubChallenge);
            }

            return Forbid("Please specify hub.verify_token doesn't match or ");
        }        

        [HttpPost]
        public async Task HandleMessage([FromBody] MessengerObject obj)
        {
            System.Console.WriteLine("Handling message");
            foreach (var entry in obj.Entries)
            {
                MessengerResponse response = await HandleEntry(entry);
                
                if(response.Failed)
                {
                    this.logger.LogError($"Failed to send message to facebook. Error message: '{response.RawResponse}'");
                }
            }
        }

        private Task<MessengerResponse> HandleEntry(MessengerEntry entry)
        {
            var messaging = entry.Messaging.First();
            var text = messaging.Message.Text;
            var response = new MessengerMessage {Text = text};
            return messageSender.SendAsync(response, messaging.Sender);
        }  
    }
}
