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
    const [error, setError] = useState<string>();
    const [adding, setAdding] = useState(false);

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
                {forecasts.length === 0 &&
                    <tr>
                        <td colSpan={4}><em>No forecasts available</em></td>
                    </tr>}
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
            {error && <p className="error" role="alert">{error}</p>}
            {contents}
            <p>
                <button onClick={addForecast} disabled={forecasts === undefined || adding}>
                    {adding ? 'Adding...' : 'Add a forecast'}
                </button>
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
            setForecasts(await response.json());
            setError(undefined);
        } else if (response.status === 503) {
            // The backend is running but can't reach the database: show an empty table and the reason.
            setForecasts([]);
            setError(await describeError(response));
        }
    }

    async function populateDatabaseInfo() {
        const response = await fetch('database');
        if (response.ok) {
            setDatabase(await response.json());
        }
    }

    async function addForecast() {
        setAdding(true);
        setError(undefined);
        try {
            const response = await fetch('weatherforecast', { method: 'POST' });
            if (response.ok) {
                await populateWeatherData();
                await populateDatabaseInfo();
            } else {
                setError(`Could not add a forecast. ${await describeError(response)}`);
            }
        } finally {
            setAdding(false);
        }
    }
}

// Turns an error response (ProblemDetails from ASP.NET Core) into a readable message.
async function describeError(response: Response) {
    try {
        const problem = await response.json();
        return `${problem.title ?? response.statusText}: ${problem.detail ?? ''}`;
    } catch {
        return `${response.status} ${response.statusText}`;
    }
}

export default App;
