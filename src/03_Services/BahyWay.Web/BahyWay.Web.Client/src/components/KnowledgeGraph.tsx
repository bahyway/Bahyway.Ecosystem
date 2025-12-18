import { useCallback } from 'react';
import {
  ReactFlow,
  type Node,
  type Edge,
  Background,
  Controls,
  MiniMap,
  useNodesState,
  useEdgesState,
  addEdge,
  type Connection,
} from 'reactflow';
import 'reactflow/dist/style.css';
import './KnowledgeGraph.css';

const initialNodes: Node[] = [
  // Central SharedKernel node
  {
    id: 'sharedkernel',
    type: 'default',
    data: { label: '🔧 SharedKernel\n(Foundation)' },
    position: { x: 400, y: 50 },
    style: {
      background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)',
      color: 'white',
      border: '3px solid #764ba2',
      borderRadius: '12px',
      padding: '20px',
      fontSize: '16px',
      fontWeight: 'bold',
      width: 200,
    },
  },
  // 7 Project nodes in a circular arrangement
  {
    id: 'alarminsight',
    data: { label: '🚨 AlarmInsight' },
    position: { x: 100, y: 200 },
    style: { background: '#48bb78', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'etlway',
    data: { label: '📊 ETLway' },
    position: { x: 300, y: 250 },
    style: { background: '#48bb78', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'smartforesight',
    data: { label: '🔮 SmartForesight' },
    position: { x: 500, y: 250 },
    style: { background: '#ed8936', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'hireway',
    data: { label: '👥 HireWay' },
    position: { x: 700, y: 200 },
    style: { background: '#4299e1', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'najafcemetery',
    data: { label: '📜 NajafCemetery' },
    position: { x: 700, y: 350 },
    style: { background: '#4299e1', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'steerview',
    data: { label: '🗺️ SteerView' },
    position: { x: 500, y: 400 },
    style: { background: '#4299e1', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  {
    id: 'ssisight',
    data: { label: '⚙️ SSISight' },
    position: { x: 300, y: 400 },
    style: { background: '#4299e1', color: 'white', borderRadius: '8px', padding: '15px', fontWeight: '600' },
  },
  // Technology nodes
  {
    id: 'postgres',
    data: { label: '🐘 PostgreSQL' },
    position: { x: 50, y: 500 },
    style: { background: '#336791', color: 'white', borderRadius: '8px', padding: '10px', fontSize: '12px' },
  },
  {
    id: 'redis',
    data: { label: '🔴 Redis' },
    position: { x: 200, y: 550 },
    style: { background: '#dc382d', color: 'white', borderRadius: '8px', padding: '10px', fontSize: '12px' },
  },
  {
    id: 'rabbitmq',
    data: { label: '🐰 RabbitMQ' },
    position: { x: 350, y: 550 },
    style: { background: '#ff6600', color: 'white', borderRadius: '8px', padding: '10px', fontSize: '12px' },
  },
  {
    id: 'semantickernel',
    data: { label: '🤖 Semantic Kernel' },
    position: { x: 550, y: 550 },
    style: { background: '#512bd4', color: 'white', borderRadius: '8px', padding: '10px', fontSize: '12px' },
  },
];

const initialEdges: Edge[] = [
  // SharedKernel to all projects
  { id: 'sk-ai', source: 'sharedkernel', target: 'alarminsight', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-etl', source: 'sharedkernel', target: 'etlway', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-sf', source: 'sharedkernel', target: 'smartforesight', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-hw', source: 'sharedkernel', target: 'hireway', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-nc', source: 'sharedkernel', target: 'najafcemetery', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-sv', source: 'sharedkernel', target: 'steerview', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  { id: 'sk-ss', source: 'sharedkernel', target: 'ssisight', animated: true, style: { stroke: '#667eea', strokeWidth: 3 } },
  // Projects to technologies
  { id: 'ai-pg', source: 'alarminsight', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'ai-redis', source: 'alarminsight', target: 'redis', style: { stroke: '#a0aec0' } },
  { id: 'etl-pg', source: 'etlway', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'sf-sk', source: 'smartforesight', target: 'semantickernel', style: { stroke: '#a0aec0' } },
  { id: 'sf-pg', source: 'smartforesight', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'hw-pg', source: 'hireway', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'nc-pg', source: 'najafcemetery', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'sv-pg', source: 'steerview', target: 'postgres', style: { stroke: '#a0aec0' } },
  { id: 'ss-pg', source: 'ssisight', target: 'postgres', style: { stroke: '#a0aec0' } },
];

export default function KnowledgeGraph() {
  const [nodes, , onNodesChange] = useNodesState(initialNodes);
  const [edges, setEdges, onEdgesChange] = useEdgesState(initialEdges);

  const onConnect = useCallback(
    (params: Connection) => setEdges((eds) => addEdge(params, eds)),
    [setEdges]
  );

  return (
    <div className="knowledge-graph-container">
      <ReactFlow
        nodes={nodes}
        edges={edges}
        onNodesChange={onNodesChange}
        onEdgesChange={onEdgesChange}
        onConnect={onConnect}
        fitView
      >
        <Background />
        <Controls />
        <MiniMap zoomable pannable />
      </ReactFlow>
    </div>
  );
}
