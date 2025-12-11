import FuzzyLogicSimulator from '../components/FuzzyLogicSimulator';
import './Demo.css';

export default function Demo() {
  return (
    <div className="demo">
      <section className="demo-hero">
        <div className="container">
          <h1>Interactive Demos</h1>
          <p>Experience BahyWay technologies with live, interactive demonstrations</p>
        </div>
      </section>

      <section className="demo-content">
        <div className="container">
          <FuzzyLogicSimulator />

          <div className="demo-grid">
            <div className="demo-card coming-soon-demo">
              <div className="demo-icon">🚨</div>
              <h3>AlarmInsight Demo</h3>
              <p>Real-time alarm processing and filtering simulation</p>
              <span className="badge">Coming Soon</span>
            </div>

            <div className="demo-card coming-soon-demo">
              <div className="demo-icon">📊</div>
              <h3>ETLway Pipeline</h3>
              <p>Watch data transformation workflows in action</p>
              <span className="badge">Coming Soon</span>
            </div>

            <div className="demo-card coming-soon-demo">
              <div className="demo-icon">🗺️</div>
              <h3>SteerView Maps</h3>
              <p>Interactive geospatial navigation with PostGIS</p>
              <span className="badge">Coming Soon</span>
            </div>

            <div className="demo-card coming-soon-demo">
              <div className="demo-icon">🤖</div>
              <h3>Semantic Kernel AI</h3>
              <p>AI-powered natural language processing</p>
              <span className="badge">Coming Soon</span>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
