import './Blog.css';

export default function Blog() {
  const articles = [
    {
      title: 'Building a Knowledge Graph for Microservices Architecture',
      excerpt: 'Learn how we visualize dependencies between our 7 projects using React Flow...',
      date: 'Coming Soon',
      category: 'Architecture',
    },
    {
      title: 'Implementing Fuzzy Logic in .NET 8.0',
      excerpt: 'A deep dive into fuzzy logic control systems and their practical applications...',
      date: 'Coming Soon',
      category: 'AI & Machine Learning',
    },
    {
      title: 'Clean Architecture with SharedKernel Pattern',
      excerpt: 'How we maintain consistency across 7 projects with a shared foundation...',
      date: 'Coming Soon',
      category: 'Software Design',
    },
  ];

  return (
    <div className="blog">
      <section className="blog-hero">
        <div className="container">
          <h1>Blog & Community</h1>
          <p>Technical articles, tutorials, and insights from the BahyWay team</p>
        </div>
      </section>

      <section className="blog-content">
        <div className="container">
          <div className="articles-grid">
            {articles.map((article, idx) => (
              <article key={idx} className="article-card">
                <div className="article-category">{article.category}</div>
                <h2>{article.title}</h2>
                <p>{article.excerpt}</p>
                <div className="article-footer">
                  <span className="article-date">{article.date}</span>
                  <a href="#" className="read-more">Read More →</a>
                </div>
              </article>
            ))}
          </div>

          <div className="blog-cta">
            <h3>Stay Updated</h3>
            <p>Subscribe to get notified when we publish new articles</p>
            <div className="subscribe-form">
              <input type="email" placeholder="Enter your email" disabled />
              <button disabled>Coming Soon</button>
            </div>
          </div>
        </div>
      </section>
    </div>
  );
}
