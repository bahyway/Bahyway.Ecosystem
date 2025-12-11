-- BahyWay Database Initialization Script

-- Enable Apache AGE extension
CREATE EXTENSION IF NOT EXISTS age;

-- Load AGE into search path
SET search_path = ag_catalog, "$user", public;

-- Create graph for knowledge graph relationships
SELECT create_graph('bahyway_graph');

-- Create additional schemas if needed
CREATE SCHEMA IF NOT EXISTS public;

-- Grant privileges
GRANT ALL PRIVILEGES ON DATABASE bahyway_db TO bahyway;
GRANT ALL PRIVILEGES ON SCHEMA public TO bahyway;
GRANT ALL PRIVILEGES ON SCHEMA ag_catalog TO bahyway;

-- Success message
DO $$
BEGIN
    RAISE NOTICE 'BahyWay database initialized successfully!';
END $$;
