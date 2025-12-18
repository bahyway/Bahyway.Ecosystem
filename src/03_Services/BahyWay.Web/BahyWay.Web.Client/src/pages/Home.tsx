import { Link } from 'react-router-dom';
import './Home.css';

export default function Home() {
  const projects = [
    { name: 'AlarmInsight', desc: 'Intelligent alarm management system', icon: '🚨' },
    { name: 'ETLway', desc: 'File-based data processing pipeline', icon: '📊' },
    { name: 'SmartForesight', desc: 'Predictive analytics & forecasting', icon: '🔮' },
    { name: 'HireWay', desc: 'Recruitment management system', icon: '👥' },
    { name: 'NajafCemetery', desc: 'Cemetery records & legal management', icon: '📜' },
    { name: 'SteerView', desc: 'Geospatial navigation system', icon: '🗺️' },
    { name: 'SSISight', desc: 'SSIS integration monitoring', icon: '⚙️' },
  ];

  return (
    <div className="home">
      {/* Hero Section */}
      <section className="hero">
        <div className="hero-content">
          <h1 className="hero-title">
            Welcome to <span className="gradient-text">BahyWay</span>
          </h1>
          <p className="hero-subtitle">
            A comprehensive ecosystem of 7 intelligent solutions built with .NET 8.0, React, and AI
          </p>
          <div className="hero-buttons">
            <Link to="/products" className="btn btn-primary">Explore Products</Link>
            <Link to="/demo" className="btn btn-secondary">Try Demo</Link>
          </div>
        </div>
      </section>

      {/* Projects Grid */}
      <section className="projects-section">
        <div className="container">
          <h2 className="section-title">Our Ecosystem</h2>
          <p className="section-subtitle">Seven powerful projects, one shared foundation</p>

          <div className="projects-grid">
            {projects.map((project) => (
              <div key={project.name} className="project-card">
                <div className="project-icon">{project.icon}</div>
                <h3>{project.name}</h3>
                <p>{project.desc}</p>
                <Link to="/products" className="project-link">Learn More →</Link>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* Tech Stack Section */}
      <section className="tech-section">
        <div className="container">
          <h2 className="section-title">Built With Modern Technologies</h2>
          <div className="tech-grid">
            <div className="tech-item">
              <h4>.NET 8.0</h4>
              <p>Clean Architecture</p>
            </div>
            <div className="tech-item">
              <h4>React + TypeScript</h4>
              <p>Modern Frontend</p>
            </div>
            <div className="tech-item">
              <h4>PostgreSQL</h4>
              <p>Robust Database</p>
            </div>
            <div className="tech-item">
              <h4>Redis & RabbitMQ</h4>
              <p>Caching & Messaging</p>
            </div>
            <div className="tech-item">
              <h4>Semantic Kernel</h4>
              <p>AI Integration</p>
            </div>
            <div className="tech-item">
              <h4>Fuzzy Logic</h4>
              <p>Smart Decisions</p>
            </div>
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="cta-section">
        <div className="container">
          <h2>Ready to Explore?</h2>
          <p>Discover how BahyWay can transform your business</p>
          <Link to="/contact" className="btn btn-primary btn-large">Get in Touch</Link>
        </div>
      </section>
    </div>
  );
}
