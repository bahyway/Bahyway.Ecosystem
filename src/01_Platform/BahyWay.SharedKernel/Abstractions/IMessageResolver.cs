namespace BahyWay.SharedKernel.Application.Abstractions
{
    public interface IMessageResolver
    {
        // Get a simple message
        string GetError(string code);
        string GetScoreMessage(string code);

        // Get a message and fill in placeholders like {0}
        string GetError(string code, params object[] args);
        string GetScoreMessage(string code, params object[] args);
    }
}