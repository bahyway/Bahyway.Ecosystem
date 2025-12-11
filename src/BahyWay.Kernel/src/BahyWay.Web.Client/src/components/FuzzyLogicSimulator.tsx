import { useState } from 'react';
import { Line } from 'react-chartjs-2';
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler,
} from 'chart.js';
import './FuzzyLogicSimulator.css';

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  Filler
);

export default function FuzzyLogicSimulator() {
  const [temperature, setTemperature] = useState(25);
  const [humidity, setHumidity] = useState(50);
  const [fanSpeed, setFanSpeed] = useState(50);

  // Fuzzy logic calculation
  const calculateFanSpeed = (temp: number, humid: number): number => {
    // Membership functions for temperature
    const cold = Math.max(0, Math.min(1, (20 - temp) / 20));
    const moderate = Math.max(0, Math.min((temp - 15) / 10, (35 - temp) / 10));
    const hot = Math.max(0, Math.min((temp - 30) / 10, 1));

    // Membership functions for humidity
    const lowHumid = Math.max(0, Math.min(1, (40 - humid) / 40));
    const medHumid = Math.max(0, Math.min((humid - 30) / 20, (70 - humid) / 20));
    const highHumid = Math.max(0, Math.min((humid - 60) / 40, 1));

    // Fuzzy rules
    const rules = [
      { condition: Math.min(cold, lowHumid), output: 20 },      // If cold and low humidity -> slow
      { condition: Math.min(cold, medHumid), output: 20 },      // If cold and med humidity -> slow
      { condition: Math.min(cold, highHumid), output: 50 },     // If cold and high humidity -> medium
      { condition: Math.min(moderate, lowHumid), output: 30 },  // If moderate and low -> slow
      { condition: Math.min(moderate, medHumid), output: 50 },  // If moderate and med -> medium
      { condition: Math.min(moderate, highHumid), output: 80 }, // If moderate and high -> fast
      { condition: Math.min(hot, lowHumid), output: 50 },       // If hot and low -> medium
      { condition: Math.min(hot, medHumid), output: 80 },       // If hot and med -> fast
      { condition: Math.min(hot, highHumid), output: 100 },     // If hot and high -> max
    ];

    // Defuzzification (weighted average)
    const numerator = rules.reduce((sum, rule) => sum + rule.condition * rule.output, 0);
    const denominator = rules.reduce((sum, rule) => sum + rule.condition, 0);

    return denominator === 0 ? 50 : Math.round(numerator / denominator);
  };

  const handleTemperatureChange = (value: number) => {
    setTemperature(value);
    setFanSpeed(calculateFanSpeed(value, humidity));
  };

  const handleHumidityChange = (value: number) => {
    setHumidity(value);
    setFanSpeed(calculateFanSpeed(temperature, value));
  };

  // Chart data for membership functions
  const tempRange = Array.from({ length: 41 }, (_, i) => i);
  const chartData = {
    labels: tempRange,
    datasets: [
      {
        label: 'Cold',
        data: tempRange.map(t => Math.max(0, Math.min(1, (20 - t) / 20))),
        borderColor: '#4299e1',
        backgroundColor: 'rgba(66, 153, 225, 0.1)',
        fill: true,
      },
      {
        label: 'Moderate',
        data: tempRange.map(t => Math.max(0, Math.min((t - 15) / 10, (35 - t) / 10))),
        borderColor: '#48bb78',
        backgroundColor: 'rgba(72, 187, 120, 0.1)',
        fill: true,
      },
      {
        label: 'Hot',
        data: tempRange.map(t => Math.max(0, Math.min((t - 30) / 10, 1))),
        borderColor: '#f56565',
        backgroundColor: 'rgba(245, 101, 101, 0.1)',
        fill: true,
      },
    ],
  };

  const chartOptions = {
    responsive: true,
    plugins: {
      legend: {
        position: 'top' as const,
      },
      title: {
        display: true,
        text: 'Temperature Membership Functions',
      },
    },
    scales: {
      y: {
        min: 0,
        max: 1,
        title: {
          display: true,
          text: 'Membership Degree',
        },
      },
      x: {
        title: {
          display: true,
          text: 'Temperature (°C)',
        },
      },
    },
  };

  return (
    <div className="fuzzy-simulator">
      <h3>Temperature Control Fuzzy Logic System</h3>
      <p className="simulator-description">
        Adjust temperature and humidity to see how fuzzy logic determines the optimal fan speed.
      </p>

      <div className="simulator-grid">
        <div className="controls-panel">
          <div className="control-group">
            <label>
              Temperature: <strong>{temperature}°C</strong>
            </label>
            <input
              type="range"
              min="0"
              max="40"
              value={temperature}
              onChange={(e) => handleTemperatureChange(Number(e.target.value))}
              className="slider"
            />
            <div className="range-labels">
              <span>0°C (Cold)</span>
              <span>40°C (Hot)</span>
            </div>
          </div>

          <div className="control-group">
            <label>
              Humidity: <strong>{humidity}%</strong>
            </label>
            <input
              type="range"
              min="0"
              max="100"
              value={humidity}
              onChange={(e) => handleHumidityChange(Number(e.target.value))}
              className="slider"
            />
            <div className="range-labels">
              <span>0% (Dry)</span>
              <span>100% (Wet)</span>
            </div>
          </div>

          <div className="output-display">
            <div className="output-label">Calculated Fan Speed</div>
            <div className="output-value">{fanSpeed}%</div>
            <div className="output-bar">
              <div
                className="output-fill"
                style={{ width: `${fanSpeed}%` }}
              ></div>
            </div>
          </div>
        </div>

        <div className="chart-panel">
          <Line data={chartData} options={chartOptions} />
        </div>
      </div>

      <div className="fuzzy-info">
        <h4>How It Works</h4>
        <p>
          This simulator demonstrates fuzzy logic control - a technique used in AlarmInsight and other
          BahyWay projects for intelligent decision-making.
        </p>
        <ul>
          <li><strong>Fuzzification:</strong> Convert crisp inputs (temp, humidity) to fuzzy sets (cold, moderate, hot)</li>
          <li><strong>Rule Evaluation:</strong> Apply 9 fuzzy rules (e.g., "IF temp is hot AND humidity is high THEN fan is fast")</li>
          <li><strong>Defuzzification:</strong> Convert fuzzy output back to crisp value (fan speed percentage)</li>
        </ul>
      </div>
    </div>
  );
}
