namespace Bahyway.ShoWay.Domain.Entities
{
    public class KnowledgeNode
    {
        public Guid Id { get; private set; }
        public string OriginalName { get; private set; }
        public string Skeleton { get; private set; }
        public string ColorHex { get; private set; }
        public string LanguageOrigin { get; private set; }

        // Coordinates for the Graph Editor (X, Y)
        public double X { get; set; }
        public double Y { get; set; }

        public KnowledgeNode(string name, string skeleton, string colorHex, string language)
        {
            Id = Guid.NewGuid();
            OriginalName = name;
            Skeleton = skeleton;
            ColorHex = colorHex;
            LanguageOrigin = language;
        }
    }
}