import { useEffect, useState } from 'react';
import './App.css';

interface Forecast {
    id: number;
    date: string;
    temperatureC: number;
    temperatureF: number;
    summary: string;
}

interface DatabaseInfo {
    server: string;
    database: string;
    version: string;
}

function App() {
    const [forecasts, setForecasts] = useState<Forecast[]>();
    const [database, setDatabase] = useState<DatabaseInfo>();

    useEffect(() => {
        populateWeatherData();
        populateDatabaseInfo();
    }, []);

    const contents = forecasts === undefined
        ? <p><em>Loading... Please refresh once the ASP.NET backend has started.</em></p>
        : <table className="table table-striped" aria-labelledby="tableLabel">
            <thead>
                <tr>
                    <th>Date</th>
                    <th>Temp. (C)</th>
                    <th>Temp. (F)</th>
                    <th>Summary</th>
                </tr>
            </thead>
            <tbody>
                {forecasts.map(forecast =>
                    <tr key={forecast.id}>
                        <td>{forecast.date}</td>
                        <td>{forecast.temperatureC}</td>
                        <td>{forecast.temperatureF}</td>
                        <td>{forecast.summary}</td>
                    </tr>
                )}
            </tbody>
        </table>;

    return (
        <div>
            <h1 id="tableLabel">Weather forecast</h1>
            <p>This component demonstrates fetching data from the server, which stores it in SQL Server.</p>
            {contents}
            <p>
                <button onClick={addForecast} disabled={forecasts === undefined}>Add a forecast</button>
            </p>
            {database &&
                <p className="database-info">
                    Database <strong>{database.database}</strong> on server <strong>{database.server}</strong>
                    <br />
                    <small>{database.version}</small>
                </p>}
        </div>
    );

    async function populateWeatherData() {
        const response = await fetch('weatherforecast');
        if (response.ok) {
            const data = await response.json();
            setForecasts(data);
        }
    }

    async function populateDatabaseInfo() {
        const response = await fetch('database');
        if (response.ok) {
            const data = await response.json();
            setDatabase(data);
        }
    }

    async function addForecast() {
        const response = await fetch('weatherforecast', { method: 'POST' });
        if (response.ok) {
            await populateWeatherData();
        }
    }
}

export default App;
