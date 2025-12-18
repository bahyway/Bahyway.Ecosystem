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
    | metaAlgorithmics
    ;

// --- IDENTITY BLOCK ---
identity: 'IDENTITY' IDENTIFIER '{' identityBody* '}';

identityBody
    : 'SPATIAL_ID' '{'
        'ALGORITHM' ':' IDENTIFIER
        'DIMENSIONS' ':' '[' keyList ']'
        'PRECISION' ':' identifierOrInt
      '}'
    | 'BUSINESS_KEY' ':' keyList
    | 'FUZZY_RESOLUTION' '{' fuzzyRule* '}'
    ;

fuzzyRule: 'MATCH' '[' keyList ']' 'USING' IDENTIFIER 'THRESHOLD' FLOAT;

// --- STORAGE BLOCK ---
storage: 'STORAGE' 'DataVault' '{' storageBody* '}';
storageBody : hub | satellite | link ;

// Hub: Allows multiple WITH clauses (e.g. WITH PARTITION... WITH CLUSTER...)
hub: 'HUB' ':' IDENTIFIER ('WITH' tableOptions)* '{' columnList indexBlock? '}';

// Satellite: Allows WITH temporal_tracking AND tableOptions
satellite: 'SATELLITE' ':' IDENTIFIER
           ('WITH' ('temporal_tracking' | tableOptions))*
           '{' columnList indexBlock? '}';

// Link
link: 'LINK' ':' IDENTIFIER ('WITH' tableOptions)* '{' columnList indexBlock? '}';

// Table Options
tableOptions
    : 'PARTITION_BY' ':' partitionStrategy
    | 'CLUSTERED_BY' ':' IDENTIFIER
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

// --- VECTORIZATION ---
vectorization: 'VECTORIZATION' '{' vectorBody* '}';
vectorBody: 'MODEL' ':' STRING | 'EMBEDDINGS' '{' embeddingDef* '}';
embeddingDef: IDENTIFIER ':' '[' keyList ']';

// --- COMMANDS ---
command: 'COMMAND' IDENTIFIER '{' commandBody* '}';
commandBody: 'VALIDATION' '{' validationCheck* '}' | 'EXECUTION' '{' statement* '}';
validationCheck: 'CHECK' ':' expression;
statement: 'INSERT_EVENT' ':' IDENTIFIER jsonObject;

// --- RAG QUERY ---
ragQuery: 'RAG_QUERY' IDENTIFIER '{' ragBody* '}';
ragBody
    : 'DESCRIPTION' ':' STRING
    | 'RETRIEVAL' '{' retrievalBody* '}'
    | 'GENERATION' '{' generationBody* '}'
    | 'AUGMENTATION' '{' augmentationBody* '}'
    ;

retrievalBody
    : 'VECTOR_SEARCH' ':' IDENTIFIER 'TOP_K' INT
    | 'GRAPH_TRAVERSE' ':' INT '_hops' 'FROM' IDENTIFIER 'VIA' ':'? '[' keyList ']' // ':' is now optional
    | 'TEMPORAL_WINDOW' ':' IDENTIFIER
    ;

generationBody
    : 'LLM' ':' (IDENTIFIER | STRING)
    | 'PROMPT_TEMPLATE' ':' STRING
    | 'REQUIRED_CITATIONS' ':' BOOLEAN
    ;

augmentationBody
    : 'HYBRID_SEARCH' ':' expression
    ;

// --- PRESENTATION ---
presentation: 'PRESENTATION' '{' styleRule* '}';
styleRule: 'STYLE' IDENTIFIER '{' styleProp* '}';
styleProp
    : 'COLOR' ':' STRING
    | 'ICON' ':' STRING
    | 'SHAPE' ':' IDENTIFIER
    | 'LABEL' ':' IDENTIFIER
    | 'SIZE_BY' ':' IDENTIFIER
    ;

// --- FUZZY RULES ---
ruleSet: 'RULESET' IDENTIFIER '{' ruleBody* '}';
ruleBody
    : 'INPUT' ':' IDENTIFIER
    | 'OUTPUT' ':' IDENTIFIER
    | 'LOGIC' ':' STRING
    | 'ALGORITHM' ':' IDENTIFIER
    ;

// --- META ALGORITHMICS ---
metaAlgorithmics: 'META_ALGORITHMICS' '{' metaBody* '}';
metaBody
    : 'EXECUTION_MODEL' ':' IDENTIFIER
    | 'ACTORS' '{' actorDef* '}'
    | 'PARALLEL_PATTERNS' '{' patternDef* '}'
    ;
actorDef: IDENTIFIER ':' '{' configProp* '}';
patternDef: IDENTIFIER ':' '{' configProp* '}';
configProp: IDENTIFIER ':' (IDENTIFIER | INT | expression);

// --- COMMON STRUCTURES ---
columnList: columnDef (',' columnDef)*; // Enforces commas between columns
columnDef: IDENTIFIER ':' typeDef constraint*;
typeDef: IDENTIFIER | 'UUID' | 'VARCHAR' '(' INT ')' | 'INT' | 'BIGINT' | 'DECIMAL' '(' INT ',' INT ')' | 'TIMESTAMP' | 'GEOMETRY';
constraint: 'UNIQUE' | 'PRIMARY KEY';
keyList: IDENTIFIER (',' IDENTIFIER)*;

// Safe Expressions
expression: expressionAtom+;
expressionAtom: IDENTIFIER | STRING | INT | FLOAT | BOOLEAN | OPERATOR | '(' | ')' | ',' | '[' | ']' | '=';

// JSON Object
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