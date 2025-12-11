import Link from 'next/link'

export default function Home() {
  return (
    <main className="min-h-screen">
      {/* Hero Section */}
      <section className="bg-gradient-to-r from-blue-600 to-blue-800 text-white py-20">
        <div className="container mx-auto px-4">
          <div className="max-w-4xl mx-auto text-center">
            <h1 className="text-5xl md:text-6xl font-bold mb-6">
              Welcome to BahyWay
            </h1>
            <p className="text-xl md:text-2xl mb-8 text-blue-100">
              Modern platform for projects, community, and curated news
            </p>
            <div className="flex gap-4 justify-center">
              <Link
                href="/projects"
                className="bg-white text-blue-600 px-8 py-3 rounded-lg font-semibold hover:bg-blue-50 transition"
              >
                Explore Projects
              </Link>
              <Link
                href="/news"
                className="bg-blue-700 text-white px-8 py-3 rounded-lg font-semibold hover:bg-blue-600 transition"
              >
                Read News
              </Link>
            </div>
          </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-20">
        <div className="container mx-auto px-4">
          <h2 className="text-3xl md:text-4xl font-bold text-center mb-12">
            What We Offer
          </h2>
          <div className="grid md:grid-cols-3 gap-8 max-w-6xl mx-auto">
            <FeatureCard
              title="Project Showcase"
              description="Discover innovative projects from our community members and share your own work"
              icon="📁"
            />
            <FeatureCard
              title="News Aggregation"
              description="Stay updated with curated news from multiple sources and subscriber feeds"
              icon="📰"
            />
            <FeatureCard
              title="Community Hub"
              description="Connect with professionals, collaborate on projects, and engage in discussions"
              icon="👥"
            />
          </div>
        </div>
      </section>

      {/* About Section */}
      <section className="bg-gray-100 py-20">
        <div className="container mx-auto px-4">
          <div className="max-w-4xl mx-auto text-center">
            <h2 className="text-3xl md:text-4xl font-bold mb-6">
              Built the BahyWay
            </h2>
            <p className="text-lg text-gray-700 mb-8">
              A modern, full-stack platform combining corporate presence, community
              collaboration, and intelligent news curation. Built with FastAPI, React,
              PostgreSQL, and Apache AGE for knowledge graphs.
            </p>
            <Link
              href="/about"
              className="inline-block bg-blue-600 text-white px-8 py-3 rounded-lg font-semibold hover:bg-blue-700 transition"
            >
              Learn More
            </Link>
          </div>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-800 text-white py-8">
        <div className="container mx-auto px-4 text-center">
          <p className="text-gray-400">
            © 2025 BahyWay. Built with passion and modern technology.
          </p>
        </div>
      </footer>
    </main>
  )
}

function FeatureCard({ title, description, icon }: { title: string; description: string; icon: string }) {
  return (
    <div className="bg-white p-6 rounded-lg shadow-md hover:shadow-lg transition">
      <div className="text-4xl mb-4">{icon}</div>
      <h3 className="text-xl font-semibold mb-3">{title}</h3>
      <p className="text-gray-600">{description}</p>
    </div>
  )
}
