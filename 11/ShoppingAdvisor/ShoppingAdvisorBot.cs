// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;

namespace ShoppingAdvisor
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ShoppingAdvisorBot : IBot
    {
        private readonly ShoppingAdvisorAccessors _accessors;
        private readonly CatApiClient _catApiClient;
        private readonly PhotoAnalysisService _photoAnalysisService;

        /// <summary>
        /// Initializes a new instance of the class.
        /// </summary>
        public ShoppingAdvisorBot(ShoppingAdvisorAccessors accessors, CatApiClient catApiClient, PhotoAnalysisService photoAnalysisService)
        {
            _accessors = accessors ?? throw new System.ArgumentNullException(nameof(accessors));
            _catApiClient = catApiClient;
            _photoAnalysisService = photoAnalysisService;
        }

        /// <summary>
        /// Every conversation turn for our Echo Bot will call this method.
        /// There are no dialogs used, since it's "single turn" processing, meaning a single
        /// request and response.
        /// </summary>
        /// <param name="turnContext">A <see cref="ITurnContext"/> containing all the data needed
        /// for processing this conversation turn. </param>
        /// <param name="cancellationToken">(Optional) A <see cref="CancellationToken"/> that can be used by other objects
        /// or threads to receive notice of cancellation.</param>
        /// <returns>A <see cref="Task"/> that represents the work queued to execute.</returns>
        /// <seealso cref="BotStateSet"/>
        /// <seealso cref="ConversationState"/>
        /// <seealso cref="IMiddleware"/>
        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    await Greeting(turnContext, cancellationToken);
                }
            }
            else if (turnContext.Activity.Type == ActivityTypes.Message)
            {
                if (turnContext.Activity.Attachments?.Any() ?? false)
                {
                    await AnalyzePhoto(turnContext, cancellationToken);
                }
                else
                {
                    await SendRandomKitty(turnContext, cancellationToken);
                }
            }

            // Save the new turn count into the conversation state.
            await _accessors.ConversationState.SaveChangesAsync(turnContext, cancellationToken: cancellationToken);
        }

        private static async Task Greeting(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync($"👋 Hi there! Welcome to the 'Szkoła Chmury Bot'. Send me an image.", cancellationToken: cancellationToken);
        }

        private async Task AnalyzePhoto(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            var image = turnContext.Activity.Attachments.FirstOrDefault(x => x.ContentType.StartsWith("image"));
            if (image != null)
            {
                await turnContext.SendActivityAsync("🕵️‍ Let me have a look at your image...", cancellationToken: cancellationToken);
                var analysisResult = await _photoAnalysisService.AnalyzePhoto(image.ContentUrl);
                if (!analysisResult.Description.Captions.Any())
                    await turnContext.SendActivityAsync("🙈 I don't know what is it... sorry...", cancellationToken: cancellationToken);
                else
                    await turnContext.SendActivityAsync(
                        $"👩‍🏫 Oh, you have sent me {analysisResult.Description.Captions.OrderByDescending(x => x.Confidence).First().Text}", cancellationToken: cancellationToken);
            }
            else
            {
                await turnContext.SendActivityAsync("👮‍ Sorry, that's not a valid image :(", cancellationToken: cancellationToken);
            }
        }

        private async Task SendRandomKitty(ITurnContext turnContext, CancellationToken cancellationToken)
        {
            await turnContext.SendActivityAsync(
                                    "📸 I'm afraid I can't understand you. You can send me a picture if you want me to check what will I see.", cancellationToken: cancellationToken);

            // get random cat
            var randomCatImage = await _catApiClient.GetRandomCatPhotoAsBase64();

            var kittyReply = turnContext.Activity.CreateReply("🐨 Oh, look at this kitty!");
            kittyReply.Attachments = new List<Attachment>()
                    {
                        new Attachment(randomCatImage.contentType,
                            $"data:{randomCatImage.contentType};base64,{randomCatImage.content}", name: "randomcat")
                    };
            await turnContext.SendActivityAsync(kittyReply, cancellationToken: cancellationToken);
        }
    }
}
