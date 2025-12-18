import { Link } from 'react-router-dom';
import './ComingSoon.css';

interface ComingSoonProps {
  title: string;
  description: string;
}

export default function ComingSoon({ title, description }: ComingSoonProps) {
  return (
    <div className="coming-soon">
      <div className="coming-soon-content">
        <h1>{title}</h1>
        <p>{description}</p>
        <p className="status">Coming Soon! 🚀</p>
        <Link to="/" className="back-button">Back to Home</Link>
      </div>
    </div>
  );
}
