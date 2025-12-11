/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  swcMinify: true,
  output: 'standalone',

  // Environment variables
  env: {
    NEXT_PUBLIC_API_URL: process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8000',
    NEXT_PUBLIC_USERS_SERVICE_URL: process.env.NEXT_PUBLIC_USERS_SERVICE_URL || 'http://localhost:8000',
    NEXT_PUBLIC_PROJECTS_SERVICE_URL: process.env.NEXT_PUBLIC_PROJECTS_SERVICE_URL || 'http://localhost:8001',
    NEXT_PUBLIC_NEWS_SERVICE_URL: process.env.NEXT_PUBLIC_NEWS_SERVICE_URL || 'http://localhost:8002',
  },

  // Image optimization
  images: {
    domains: ['localhost', 'bahyway.com', 'www.bahyway.com'],
  },

  // Webpack configuration
  webpack: (config) => {
    return config;
  },
}

module.exports = nextConfig
