namespace HomeController.Controllers
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IHomeController
    {
        Task InitializeController();

        IEnumerable<string> GetCommandPhrases();

        Task ProcessCommandPhrase(string phraseText);
    }
}
