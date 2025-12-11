import './Footer.css';

export default function Footer() {
  const currentYear = new Date().getFullYear();

  return (
    <footer className="footer">
      <div className="footer-container">
        <div className="footer-section">
          <h3>BahyWay Ecosystem</h3>
          <p>Building intelligent solutions with .NET 8.0, React, and AI</p>
        </div>

        <div className="footer-section">
          <h4>Projects</h4>
          <ul>
            <li>AlarmInsight</li>
            <li>ETLway</li>
            <li>SmartForesight</li>
            <li>HireWay</li>
            <li>NajafCemetery</li>
            <li>SteerView</li>
            <li>SSISight</li>
          </ul>
        </div>

        <div className="footer-section">
          <h4>Technologies</h4>
          <ul>
            <li>.NET 8.0</li>
            <li>React + TypeScript</li>
            <li>PostgreSQL + PostGIS</li>
            <li>Redis & RabbitMQ</li>
            <li>Semantic Kernel</li>
          </ul>
        </div>

        <div className="footer-section">
          <h4>Connect</h4>
          <ul>
            <li>GitHub</li>
            <li>LinkedIn</li>
            <li>Email</li>
          </ul>
        </div>
      </div>

      <div className="footer-bottom">
        <p>&copy; {currentYear} BahyWay. All rights reserved.</p>
      </div>
    </footer>
  );
}
