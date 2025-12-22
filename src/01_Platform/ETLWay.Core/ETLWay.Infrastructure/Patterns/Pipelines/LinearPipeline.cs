using System.Threading.Tasks;
using ETLWay.Application.Abstractions.Patterns;

namespace ETLWay.Infrastructure.Patterns.Pipelines
{
    public class LinearPipeline<TSource, TTarget> : IPipeline
    {
        private readonly IExtractor<TSource> _extractor;
        private readonly ITransformer<TSource, TTarget> _transformer;
        private readonly ILoader<TTarget> _loader;
        private readonly EtlContext _context;

        // UPDATED CONSTRUCTOR: Now accepts 'context' as a parameter
        public LinearPipeline(
            IExtractor<TSource> extractor,
            ITransformer<TSource, TTarget> transformer,
            ILoader<TTarget> loader,
            EtlContext context) // <--- INJECTED HERE
        {
            _extractor = extractor;
            _transformer = transformer;
            _loader = loader;
            _context = context; // <--- ASSIGNED HERE (Shared Brain)
        }

        public async Task ExecuteAsync()
        {
            // 1. Extract (Stream)
            await foreach (var item in _extractor.ExtractAsync(_context))
            {
                // 2. Transform
                var transformed = await _transformer.TransformAsync(item, _context);

                // 3. Load
                if (transformed != null)
                {
                    await _loader.LoadAsync(transformed, _context);
                }
            }
        }
    }
}