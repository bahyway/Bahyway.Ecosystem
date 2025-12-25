using Bahyway.ShoWay.Domain.Entities;
using Bahyway.ShoWay.Domain.Interfaces;

namespace Bahyway.ShoWay.Application.UseCases
{
    public class CreateNodeCommand
    {
        private readonly IPhoneticEngine _engine;

        public CreateNodeCommand(IPhoneticEngine engine)
        {
            _engine = engine;
        }

        public KnowledgeNode Execute(string rawInput)
        {
            // 1. Extract Skeleton (The "Root")
            string skeleton = _engine.ExtractSkeleton(rawInput);

            // 2. Generate Color (The "Vector")
            string color = _engine.GenerateVectorColor(skeleton);

            // 3. Context
            string lang = _engine.DetectLanguageContext(rawInput);

            // 4. Return the Entity
            return new KnowledgeNode(rawInput, skeleton, color, lang);
        }
    }
}