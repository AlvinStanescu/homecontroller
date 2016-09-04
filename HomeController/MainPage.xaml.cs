namespace HomeController
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Windows.Media.SpeechRecognition;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    using HomeController.Controllers;

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const double ConfidenceThreshold = 0.6;

        private SpeechRecognizer recognizer;
        private readonly IDictionary<string, IHomeController> availableCommands = new Dictionary<string, IHomeController>();

        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void InitializeDependencies(object sender, RoutedEventArgs e)
        {
            await this.InitializeControllers();
            await this.RegisterVoiceActivation();
        }

        private async Task InitializeControllers()
        {
            var lightingController = new HueLightingController();
            await lightingController.InitializeController();

            foreach (var command in lightingController.GetCommandPhrases())
            {
                availableCommands[command] = lightingController;
            }
        }

        private async Task RegisterVoiceActivation()
        {
            recognizer = new SpeechRecognizer
            {
                Constraints = { new SpeechRecognitionListConstraint(availableCommands.Keys) }
            };

            recognizer.ContinuousRecognitionSession.ResultGenerated += HandleVoiceCommand;
            recognizer.ContinuousRecognitionSession.AutoStopSilenceTimeout = TimeSpan.FromDays(1000);
            recognizer.StateChanged += HandleStateChange;

            var compilationResult = await recognizer.CompileConstraintsAsync();
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                await Dispatcher.RunIdleAsync(_ => lastState.Text = $"Compilation failed: {compilationResult.Status}");
            }
        }
        
        private async void HandleStateChange(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            await Dispatcher.RunIdleAsync(_ => lastState.Text = args.State.ToString());
        }

        private async void HandleVoiceCommand(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            string text = args.Result.Text;

            if (args.Result.RawConfidence >= ConfidenceThreshold)
            {
                IHomeController controller;
                if (availableCommands.TryGetValue(text, out controller))
                {
                    // Asynchronously call the handler; we don't care about the result.
                    // Suppress CS4014, else we will get a compiler warning about not awaiting the Task.
#pragma warning disable CS4014
                    Task.Run(async () =>
                    {
                        await controller.ProcessCommandPhrase(text);
                        await UpdateUserInterface(args, text);
                    });
#pragma warning restore
                }
            }
            else
            {
                await UpdateUserInterface(args, text);
            }
        }

        private async Task UpdateUserInterface(SpeechContinuousRecognitionResultGeneratedEventArgs args, string text)
        {
            await Dispatcher.RunIdleAsync(_ =>
            {
                lastText.Text = text;
                lastConfidence.Text = $"{args.Result.Confidence} ({args.Result.RawConfidence})";
                lastTimestamp.Text = DateTime.Now.TimeOfDay.ToString();
            });
        }
    }
}
