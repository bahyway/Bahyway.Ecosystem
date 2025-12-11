grammar Akkadian;

// ==========================================
// PARSER RULES
// ==========================================

program: context+ EOF;

context: 'CONTEXT' IDENTIFIER '{' contextBody* '}';

contextBody
    : identity
    | storage
    | vectorization
    | command
    | ragQuery
    | presentation
    | ruleSet
    ;

// --- IDENTITY BLOCK (The Engine for "Billions of Colors") ---
identity: 'IDENTITY' IDENTIFIER '{' identityBody* '}';

identityBody
    // The "Color" ID generation strategy
    : 'SPATIAL_ID' '{'
        'ALGORITHM' ':' IDENTIFIER          // e.g. EXTENDED_COLOR_64
        'DIMENSIONS' ':' '[' keyList ']'    // e.g. [latitude, longitude, layer, time]
        'PRECISION' ':' identifierOrInt     // e.g. HIGH or 64
      '}'

    // The Business Key (Human readable)
    | 'BUSINESS_KEY' ':' keyList

    // Fuzzy Matching Configuration for Entity Resolution
    | 'FUZZY_RESOLUTION' '{' fuzzyRule* '}'
    ;

fuzzyRule: 'MATCH' '[' keyList ']' 'USING' IDENTIFIER 'THRESHOLD' FLOAT;

// --- RULESET BLOCK (Fuzzy Logic Engine) ---
ruleSet: 'RULESET' IDENTIFIER '{' ruleBody* '}';
ruleBody
    : 'INPUT' ':' IDENTIFIER
    | 'OUTPUT' ':' IDENTIFIER
    | 'LOGIC' ':' STRING  // e.g. "IF distance IS close THEN match IS high"
    | 'ALGORITHM' ':' IDENTIFIER // e.g. MAMDANI, SUGENO
    ;

// --- STORAGE BLOCK ---
storage: 'STORAGE' 'DataVault' '{' storageBody* '}';
storageBody : hub | satellite | link ;

// Hub with Composite Key Support
hub: 'HUB' ':' IDENTIFIER 'WITH' tableOptions? '{' columnList indexBlock? '}';

// Satellite
satellite: 'SATELLITE' ':' IDENTIFIER ('WITH' 'temporal_tracking')? tableOptions? '{' columnList indexBlock? '}';

// Link
link: 'LINK' ':' IDENTIFIER 'WITH' tableOptions? '{' columnList indexBlock? '}';

// Table Options
tableOptions
    : 'PARTITION_BY' ':' partitionStrategy
    | 'SHARD_KEY' ':' IDENTIFIER
    ;

partitionStrategy
    : 'HASH' '(' IDENTIFIER ')'
    | 'RANGE' '(' IDENTIFIER ')'
    | 'LIST' '(' IDENTIFIER ')'
    ;

// Indexes
indexBlock: 'INDEXES' '{' indexDef* '}';
indexDef: indexType ':' '[' keyList ']';
indexType: 'BTREE' | 'HASH' | 'GIN' | 'GIST' | 'BRIN' | 'SPGIST';

// --- VECTORIZATION, COMMAND, RAG (Standard) ---
vectorization: 'VECTORIZATION' '{' vectorBody* '}';
vectorBody: 'MODEL' ':' STRING | 'EMBEDDINGS' '{' embeddingDef* '}';
embeddingDef: IDENTIFIER ':' '[' keyList ']';

command: 'COMMAND' IDENTIFIER '{' commandBody* '}';
commandBody: 'VALIDATION' '{' validationCheck* '}' | 'EXECUTION' '{' statement* '}';
validationCheck: 'CHECK' ':' expression;
statement: 'INSERT_EVENT' ':' IDENTIFIER jsonObject;

ragQuery: 'RAG_QUERY' IDENTIFIER '{' ragBody* '}';
ragBody: 'DESCRIPTION' ':' STRING | 'RETRIEVAL' '{' retrievalBody* '}' | 'GENERATION' '{' generationBody* '}' | 'AUGMENTATION' '{' augmentationBody* '}';
retrievalBody: 'VECTOR_SEARCH' ':' IDENTIFIER 'TOP_K' INT | 'GRAPH_TRAVERSE' ':' INT '_hops' 'FROM' IDENTIFIER 'VIA' ':' '[' keyList ']' | 'TEMPORAL_WINDOW' ':' IDENTIFIER;
generationBody: 'LLM' ':' (IDENTIFIER | STRING) | 'PROMPT_TEMPLATE' ':' STRING | 'REQUIRED_CITATIONS' ':' BOOLEAN;
augmentationBody: 'HYBRID_SEARCH' ':' expression;

presentation: 'PRESENTATION' '{' styleRule* '}';
styleRule: 'STYLE' IDENTIFIER '{' styleProp* '}';
styleProp: 'COLOR' ':' STRING | 'ICON' ':' STRING | 'SHAPE' ':' IDENTIFIER | 'LABEL' ':' IDENTIFIER | 'SIZE_BY' ':' IDENTIFIER;

// --- COMMON ---
columnList: columnDef (',' columnDef)*;
columnDef: IDENTIFIER ':' typeDef constraint*;
typeDef: IDENTIFIER | 'UUID' | 'VARCHAR' '(' INT ')' | 'INT' | 'BIGINT' | 'DECIMAL' '(' INT ',' INT ')' | 'TIMESTAMP' | 'GEOMETRY';
constraint: 'UNIQUE' | 'PRIMARY KEY';
keyList: IDENTIFIER (',' IDENTIFIER)*;

expression: expressionAtom+;
expressionAtom: IDENTIFIER | STRING | INT | FLOAT | BOOLEAN | OPERATOR | '(' | ')' | ',' | '[' | ']' | '=';
jsonObject: '{' jsonContent* '}';
jsonContent: IDENTIFIER | STRING | INT | FLOAT | BOOLEAN | OPERATOR | ',' | ':' | '=' | '(' jsonContent* ')' | '[' jsonContent* ']' | jsonObject;
identifierOrInt: IDENTIFIER | INT;

// ==========================================
// LEXER RULES
// ==========================================
WS: [ \t\r\n]+ -> skip;
COMMENT: '//' ~[\r\n]* -> skip;
BLOCK_COMMENT: '/*' .*? '*/' -> skip;
BOOLEAN: 'true' | 'false';
INT: [0-9]+;
FLOAT: [0-9]+ '.' [0-9]+;
STRING: '"' .*? '"' | '\'' .*? '\'';
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_\-.]*;
OPERATOR: [+\-*/<>!]+;