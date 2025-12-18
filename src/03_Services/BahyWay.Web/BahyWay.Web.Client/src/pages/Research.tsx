import './Research.css';

export default function Research() {
  const researchProjects = [
    {
      name: 'WPDD - Workflow Process Design & Development',
      icon: '🧠',
      status: 'Active Research',
      description: 'An enterprise workflow orchestration platform leveraging GraphRAG (Graph Retrieval-Augmented Generation) for intelligent process automation and knowledge management. Current research extends to advanced computer vision applications including YOLOv8-based object detection and hyperspectral imaging for underwater environments.',
      objectives: [
        'Develop GraphRAG-powered knowledge graphs for workflow optimization',
        'Implement YOLOv8 for real-time object detection in complex environments',
        'Explore hyperspectral imaging applications for underwater object detection',
        'Create AI-assisted workflow design tools with computer vision integration',
        'Build advanced semantic search capabilities for enterprise processes',
        'Research multimodal AI systems combining vision and language models'
      ],
      technologies: ['.NET 8.0', 'GraphRAG', 'Microsoft Semantic Kernel', 'YOLOv8', 'PyTorch', 'Neo4j', 'PostgreSQL', 'Computer Vision'],
      publications: 'In preparation',
      collaborators: 'Internal research team',
      github: 'https://github.com/bahyway/WPDDWayLab'
    },
    {
      name: 'Inference-Akkadian',
      icon: '🏛️',
      status: 'Future Research',
      description: 'An ambitious research initiative exploring the intersection of AI, natural language processing, and archaeological linguistics for ancient Akkadian text analysis.',
      objectives: [
        'Develop NLP models for ancient Akkadian language processing',
        'Create computer vision systems for cuneiform recognition',
        'Build linguistic inference engines for historical text analysis',
        'Design knowledge graphs for cross-referencing historical documents',
        'Establish historical context modeling frameworks'
      ],
      technologies: ['Python', 'TensorFlow', 'PyTorch', 'NLP Libraries', 'Computer Vision', 'Knowledge Graphs'],
      publications: 'Planned',
      collaborators: 'Seeking academic partnerships'
    }
  ];

  const researchAreas = [
    {
      area: 'Artificial Intelligence & Machine Learning',
      topics: ['GraphRAG', 'Semantic Kernel', 'Fuzzy Logic Systems', 'Predictive Analytics', 'Deep Learning for NLP']
    },
    {
      area: 'Computer Vision & Image Processing',
      topics: ['YOLOv8 Object Detection', 'Hyperspectral Imaging', 'Underwater Object Recognition', 'Real-time Video Analytics', 'Multimodal AI Systems']
    },
    {
      area: 'Knowledge Graphs & Semantic Technologies',
      topics: ['Graph Databases', 'Ontology Engineering', 'Semantic Search', 'Knowledge Representation']
    },
    {
      area: 'Computational Linguistics',
      topics: ['Ancient Language Processing', 'Historical Text Analysis', 'Linguistic Inference', 'Cross-linguistic Studies']
    },
    {
      area: 'Software Architecture & Design',
      topics: ['Clean Architecture', 'Domain-Driven Design', 'CQRS Patterns', 'Event-Driven Systems']
    }
  ];

  return (
    <div className="research">
      <section className="research-hero">
        <div className="container">
          <h1>Research & Innovation</h1>
          <p>Advancing the frontiers of AI, computer vision, knowledge graphs, and computational linguistics</p>
        </div>
      </section>

      <section className="research-intro">
        <div className="container">
          <div className="intro-content">
            <h2>Our Research Mission</h2>
            <p>
              At BahyWay, we believe in pushing the boundaries of what's possible with technology.
              Our research initiatives combine cutting-edge AI technologies with practical applications,
              spanning from enterprise workflow optimization to the preservation and analysis of ancient
              human knowledge. We are committed to open collaboration, academic rigor, and the responsible
              development of AI technologies.
            </p>
          </div>
        </div>
      </section>

      <section className="research-projects">
        <div className="container">
          <h2>Active Research Projects</h2>
          {researchProjects.map((project, idx) => (
            <div key={idx} className="research-project-card">
              <div className="project-header">
                <div className="project-title-area">
                  <span className="project-icon">{project.icon}</span>
                  <div>
                    <h3>{project.name}</h3>
                    <span className={`research-status status-${project.status.toLowerCase().replace(' ', '-')}`}>
                      {project.status}
                    </span>
                  </div>
                </div>
              </div>

              <p className="project-description">{project.description}</p>

              <div className="project-details">
                <div className="detail-section">
                  <h4>Research Objectives</h4>
                  <ul>
                    {project.objectives.map((obj, i) => (
                      <li key={i}>{obj}</li>
                    ))}
                  </ul>
                </div>

                <div className="detail-section">
                  <h4>Technologies</h4>
                  <div className="tech-tags">
                    {project.technologies.map((tech, i) => (
                      <span key={i} className="tech-tag">{tech}</span>
                    ))}
                  </div>
                </div>

                <div className="project-meta">
                  <div className="meta-item">
                    <strong>Publications:</strong> {project.publications}
                  </div>
                  <div className="meta-item">
                    <strong>Collaborators:</strong> {project.collaborators}
                  </div>
                  {project.github && (
                    <div className="meta-item">
                      <strong>Repository:</strong>{' '}
                      <a href={project.github} target="_blank" rel="noopener noreferrer" className="github-link">
                        GitHub Lab →
                      </a>
                    </div>
                  )}
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>

      <section className="research-areas">
        <div className="container">
          <h2>Research Areas</h2>
          <div className="areas-grid">
            {researchAreas.map((area, idx) => (
              <div key={idx} className="area-card">
                <h3>{area.area}</h3>
                <ul>
                  {area.topics.map((topic, i) => (
                    <li key={i}>{topic}</li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      <section className="collaboration-cta">
        <div className="container">
          <div className="cta-content">
            <h2>Collaborate With Us</h2>
            <p>
              We're always interested in collaborating with academic institutions, research organizations,
              and industry partners. If you're working on related research or interested in our projects,
              we'd love to hear from you.
            </p>
            <div className="cta-buttons">
              <a href="/contact" className="cta-button primary">Get In Touch</a>
              <a href="/blog" className="cta-button secondary">Read Our Publications</a>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
