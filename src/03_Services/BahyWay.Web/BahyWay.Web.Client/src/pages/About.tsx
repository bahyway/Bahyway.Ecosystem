import './About.css';

export default function About() {
  return (
    <div className="about">
      <section className="about-hero">
        <div className="container">
          <h1>About BahyWay</h1>
          <p className="about-lead">
            Building the future of intelligent business solutions through innovation and research
          </p>
        </div>
      </section>

      <section className="about-content">
        <div className="container">
          <div className="about-grid">
            <div className="about-section">
              <h2>Our Vision</h2>
              <p>
                BahyWay is a comprehensive ecosystem of intelligent solutions designed to transform
                how businesses operate. We combine production-ready enterprise applications with
                cutting-edge research initiatives in AI, GraphRAG, and computational linguistics.
                Our work spans from practical business solutions to academic research in areas like
                workflow automation, ancient language processing, and knowledge graph technologies.
              </p>
            </div>

            <div className="about-section">
              <h2>Our Approach</h2>
              <p>
                We follow Clean Architecture principles and Domain-Driven Design (DDD) to ensure
                our solutions are maintainable, scalable, and future-proof. Every project in the
                BahyWay ecosystem shares a common foundation through our SharedKernel library.
              </p>
            </div>

            <div className="about-section">
              <h2>Technology Stack</h2>
              <ul className="tech-list">
                <li><strong>Backend:</strong> .NET 8.0, ASP.NET Core, C#</li>
                <li><strong>Frontend:</strong> React 19, TypeScript, Vite</li>
                <li><strong>Database:</strong> PostgreSQL with PostGIS, Neo4j (Graph DB)</li>
                <li><strong>Caching & Messaging:</strong> Redis, RabbitMQ</li>
                <li><strong>Background Jobs:</strong> Hangfire</li>
                <li><strong>AI & Research:</strong> Microsoft Semantic Kernel, GraphRAG, TensorFlow</li>
                <li><strong>NLP & Linguistics:</strong> Advanced NLP libraries, Computer Vision</li>
                <li><strong>Logging:</strong> Serilog with Seq</li>
                <li><strong>Architecture:</strong> Clean Architecture, CQRS, MediatR</li>
              </ul>
            </div>

            <div className="about-section">
              <h2>Our Projects</h2>
              <p>
                The BahyWay ecosystem spans enterprise applications, research initiatives, and future innovations:
              </p>
              <h3 style={{color: '#667eea', fontSize: '1.2rem', marginTop: '1.5rem', marginBottom: '0.75rem'}}>Research & Innovation</h3>
              <ul className="projects-list">
                <li><strong>WPDD:</strong> GraphRAG-powered workflow orchestration platform</li>
                <li><strong>Inference-Akkadian:</strong> AI-driven ancient text analysis (future research)</li>
              </ul>
              <h3 style={{color: '#667eea', fontSize: '1.2rem', marginTop: '1.5rem', marginBottom: '0.75rem'}}>Enterprise Solutions</h3>
              <ul className="projects-list">
                <li><strong>AlarmInsight:</strong> Intelligent alarm management and processing</li>
                <li><strong>ETLway:</strong> File-based data processing with directory monitoring</li>
                <li><strong>SmartForesight:</strong> Predictive analytics and forecasting</li>
                <li><strong>HireWay:</strong> Recruitment and personnel management</li>
                <li><strong>NajafCemetery:</strong> Cemetery records and legal compliance</li>
                <li><strong>SteerView:</strong> Geospatial navigation and mapping</li>
                <li><strong>SSISight:</strong> SSIS integration and monitoring</li>
              </ul>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
