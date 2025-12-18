import { Link } from 'react-router-dom';
import './Navigation.css';

export default function Navigation() {
  return (
    <nav className="navbar">
      <div className="nav-container">
        <Link to="/" className="nav-logo">
          <img src="/Bahyway_Logo.png" alt="BahyWay Logo" className="logo-image" />
          <div className="logo-text-container">
            <span className="logo-text">BahyWay</span>
            <span className="logo-tagline">Ecosystem</span>
          </div>
        </Link>

        <ul className="nav-menu">
          <li className="nav-item">
            <Link to="/" className="nav-link">Home</Link>
          </li>
          <li className="nav-item">
            <Link to="/about" className="nav-link">About</Link>
          </li>
          <li className="nav-item">
            <Link to="/products" className="nav-link">Products</Link>
          </li>
          <li className="nav-item">
            <Link to="/demo" className="nav-link">Demo</Link>
          </li>
          <li className="nav-item">
            <Link to="/portfolio" className="nav-link">Portfolio</Link>
          </li>
          <li className="nav-item">
            <Link to="/research" className="nav-link">Research</Link>
          </li>
          <li className="nav-item">
            <Link to="/blog" className="nav-link">Blog</Link>
          </li>
          <li className="nav-item">
            <Link to="/contact" className="nav-link nav-link-cta">Contact</Link>
          </li>
        </ul>
      </div>
    </nav>
  );
}
