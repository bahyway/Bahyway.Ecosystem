import KnowledgeGraph from '../components/KnowledgeGraph';
import './Portfolio.css';

export default function Portfolio() {
  return (
    <div className="portfolio">
      <section className="portfolio-hero">
        <div className="container">
          <h1>Portfolio & Knowledge Graph</h1>
          <p>Explore the BahyWay ecosystem architecture and project relationships</p>
        </div>
      </section>

      <section className="knowledge-section">
        <div className="container">
          <h2>Ecosystem Architecture</h2>
          <p className="section-description">
            Interactive visualization showing how SharedKernel connects all 7 projects and their technology dependencies.
            Drag nodes to explore relationships, zoom in/out, and see how everything connects.
          </p>

          <div className="graph-legend">
            <div className="legend-item">
              <span className="legend-color" style={{background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)'}}></span>
              <span>SharedKernel (Foundation)</span>
            </div>
            <div className="legend-item">
              <span className="legend-color" style={{background: '#48bb78'}}></span>
              <span>Production Projects</span>
            </div>
            <div className="legend-item">
              <span className="legend-color" style={{background: '#ed8936'}}></span>
              <span>In Development</span>
            </div>
            <div className="legend-item">
              <span className="legend-color" style={{background: '#4299e1'}}></span>
              <span>Planned Projects</span>
            </div>
          </div>

          <KnowledgeGraph />

          <div className="architecture-details">
            <div className="detail-card">
              <h3>SharedKernel Components</h3>
              <ul>
                <li>Domain Primitives (Entity, ValueObject, Result Pattern)</li>
                <li>Application Abstractions (Logging, Caching, Background Jobs)</li>
                <li>Infrastructure Services (File Storage, Health Checks)</li>
                <li>CQRS with MediatR</li>
                <li>FluentValidation</li>
              </ul>
            </div>

            <div className="detail-card">
              <h3>Shared Technologies</h3>
              <ul>
                <li><strong>PostgreSQL:</strong> Primary database for all projects</li>
                <li><strong>Redis:</strong> Distributed caching</li>
                <li><strong>RabbitMQ:</strong> Event-driven messaging</li>
                <li><strong>Semantic Kernel:</strong> AI integration</li>
                <li><strong>Serilog + Seq:</strong> Centralized logging</li>
              </ul>
            </div>

            <div className="detail-card">
              <h3>Architecture Principles</h3>
              <ul>
                <li>Clean Architecture with layered approach</li>
                <li>Domain-Driven Design (DDD)</li>
                <li>CQRS (Command Query Responsibility Segregation)</li>
                <li>Event-Driven Architecture</li>
                <li>Railway-Oriented Programming (Result Pattern)</li>
              </ul>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
