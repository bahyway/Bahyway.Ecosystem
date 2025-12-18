-- Node Definition for DeceasedPerson
SELECT * FROM cypher('wadi_al_salaam_north_graph', $$
    CREATE (:DeceasedPerson { _type: 'Concept' })
$$) as (n agtype);
-- Node Definition for FamilyPlot
SELECT * FROM cypher('wadi_al_salaam_north_graph', $$
    CREATE (:FamilyPlot { _type: 'Concept' })
$$) as (n agtype);
