namespace Bahyway.ShoWay.Domain.Interfaces
{
    public interface IPhoneticEngine
    {
        string ExtractSkeleton(string input);
        string GenerateVectorColor(string skeleton);
        string DetectLanguageContext(string input);
    }
}