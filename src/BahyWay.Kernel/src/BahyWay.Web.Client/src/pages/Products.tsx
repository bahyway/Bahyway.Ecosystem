import './Products.css';

export default function Products() {
  const products = [
    {
      name: 'WPDD (Workflow Process Design & Development)',
      icon: '🧠',
      description: 'Enterprise workflow orchestration platform powered by GraphRAG for intelligent process automation and knowledge management. A flagship research project exploring AI-driven workflow optimization, YOLOv8-based object detection, and hyperspectral imaging for underwater environments.',
      features: ['GraphRAG-powered knowledge graphs', 'YOLOv8 object detection', 'Hyperspectral imaging analysis', 'AI-assisted workflow design', 'Underwater object detection', 'Advanced semantic search'],
      status: 'Research',
      tech: ['.NET 8.0', 'GraphRAG', 'Semantic Kernel', 'YOLOv8', 'PyTorch', 'Neo4j', 'PostgreSQL'],
      flagship: true
    },
    {
      name: 'AlarmInsight',
      icon: '🚨',
      description: 'Intelligent alarm management and processing system with advanced filtering and notification capabilities.',
      features: ['Real-time alarm processing', 'Smart filtering', 'Multiple notification channels', 'Historical analysis'],
      status: 'Production',
      tech: ['.NET 8.0', 'PostgreSQL', 'Hangfire', 'Redis']
    },
    {
      name: 'ETLway',
      icon: '📊',
      description: 'File-based data processing pipeline with directory monitoring and automated transformation workflows.',
      features: ['Directory monitoring', 'Automated ETL', 'Data validation', 'Error handling'],
      status: 'Production',
      tech: ['.NET 8.0', 'PostgreSQL', 'File Watchers']
    },
    {
      name: 'SmartForesight',
      icon: '🔮',
      description: 'Predictive analytics and forecasting engine using advanced algorithms and machine learning.',
      features: ['Time-series forecasting', 'Trend analysis', 'Anomaly detection', 'AI-powered predictions'],
      status: 'In Development',
      tech: ['.NET 8.0', 'Semantic Kernel', 'PostgreSQL']
    },
    {
      name: 'HireWay',
      icon: '👥',
      description: 'Complete recruitment management system with compliance tracking and candidate evaluation.',
      features: ['Candidate tracking', 'Interview scheduling', 'Compliance management', 'Audit trails'],
      status: 'Planned',
      tech: ['.NET 8.0', 'PostgreSQL', 'Audit System']
    },
    {
      name: 'NajafCemetery',
      icon: '📜',
      description: 'Cemetery records management with legal compliance and geospatial plot tracking.',
      features: ['Records management', 'Legal compliance', 'Plot mapping', 'Family tracking'],
      status: 'Planned',
      tech: ['.NET 8.0', 'PostgreSQL', 'PostGIS']
    },
    {
      name: 'SteerView',
      icon: '🗺️',
      description: 'Advanced geospatial navigation system with real-time routing and location services.',
      features: ['Interactive maps', 'Route optimization', 'Location tracking', 'POI management'],
      status: 'Planned',
      tech: ['.NET 8.0', 'PostGIS', 'Leaflet']
    },
    {
      name: 'SSISight',
      icon: '⚙️',
      description: 'SSIS package monitoring and integration management with real-time status tracking.',
      features: ['Package monitoring', 'Execution tracking', 'Error alerts', 'Performance metrics'],
      status: 'Planned',
      tech: ['.NET 8.0', 'SQL Server', 'SSIS']
    },
    {
      name: 'Inference-Akkadian',
      icon: '🏛️',
      description: 'Advanced natural language processing and inference system for ancient Akkadian texts. A future research initiative exploring AI-powered archaeological linguistics and historical text analysis.',
      features: ['Ancient text analysis', 'Cuneiform recognition', 'Linguistic inference', 'Historical context modeling', 'Cross-reference research'],
      status: 'Future Research',
      tech: ['Python', 'TensorFlow', 'NLP', 'Computer Vision', 'Knowledge Graphs'],
      flagship: true
    }
  ];

  return (
    <div className="products">
      <section className="products-hero">
        <div className="container">
          <h1>Our Products</h1>
          <p>Seven powerful solutions, one ecosystem</p>
        </div>
      </section>

      <section className="products-list">
        <div className="container">
          {products.map((product) => (
            <div key={product.name} className="product-detail">
              <div className="product-header">
                <div className="product-title-section">
                  <span className="product-large-icon">{product.icon}</span>
                  <div>
                    <h2>{product.name}</h2>
                    <span className={`status-badge status-${product.status.toLowerCase().replace(' ', '-')}`}>
                      {product.status}
                    </span>
                  </div>
                </div>
              </div>

              <p className="product-description">{product.description}</p>

              <div className="product-features">
                <h3>Key Features</h3>
                <ul>
                  {product.features.map((feature, idx) => (
                    <li key={idx}>{feature}</li>
                  ))}
                </ul>
              </div>

              <div className="product-tech">
                <h3>Technology Stack</h3>
                <div className="tech-badges">
                  {product.tech.map((tech, idx) => (
                    <span key={idx} className="tech-badge">{tech}</span>
                  ))}
                </div>
              </div>
            </div>
          ))}
        </div>
      </section>
    </div>
  );
}
