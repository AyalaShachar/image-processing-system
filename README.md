# Image Processing & Routing System

A full-stack asynchronous image processing and rule-based routing engine built with **.NET 8 Core** and **Angular 19**. The system simulates a real-time image processing pipeline, analyzing uploaded image properties (dimensions, file size, format) and dynamically routing them through parallel background workflows with simulated processing delays.

---

## 🚀 Key Features

### Backend (.NET Core)
* **Asynchronous Background Processing:** Utilizes `BackgroundService` paired with thread-safe `System.Threading.Channels` for non-blocking, parallel execution.
* **Rule-Based Routing Engine:** Sequentially evaluates image properties against business rules to determine execution pathways.
* **Thread-Safe State Management:** Leverages `ConcurrentDictionary` and immutable collection patterns to ensure accurate real-time monitoring across concurrent operations.
* **Development Optimization:** Includes a configurable `SpeedFactor` in configuration to shorten long pipeline delays during local testing.

### Frontend (Angular)
* **Real-Time Live Dashboard:** Continuously polls active pipeline stats every few seconds to provide visual feedback.
* **Analytical Visualization:** Displays dynamic **Pie Charts** (with on-slice counts) reflecting active workflows and status distribution.
* **Smart Grid Gallery:** Displays processed images scaled dynamically according to their modified runtime references without mutating source disk files.
* **Interactive Modals:** Clicking an image opens a detailed, scrollable log revealing its full pipeline execution path.
* **Modern Angular:** Standalone components, Signals for state, and `inject()` for dependency injection (no NgModules).

---

## 🛠️ Architecture & Routing Workflow

When an image is uploaded, the API stores it, returns immediately, and hands the work off to a background worker. The engine evaluates the rules **in order — the first match wins**:

1. **Square dimensions & daytime** (08:00–19:00) ➔ `SquarePipeline` (30s delay → finish).
2. **Width > Height** ➔ `CirclePipeline` (random 15–30s delay, reduces width by 10px, re-evaluates).
3. **File > 3MB & Width < Height** ➔ `SlowPipeline` (15s delay, doubles width, re-evaluates).
4. **File format is JPG** ➔ `StarPipeline` (10s delay, doubles width, then chains to `CirclePipeline`).
5. **Width < 20px** ➔ immediately terminates with a `ProcessError` status.

*If no rule matches, processing concludes successfully with a `Finished` status.*

> Image dimensions are changed **only as references** — the source file on disk is never resized.
> Infinite-loop cases are intentionally not guarded against, per the spec.

### Project structure

```
image-processing/
├── imageProcessing.Api/            # .NET 8 Web API (Controllers)
│   └── imageProcessing.Api/
│       ├── Models/                 # ImageItem, ImageStatus, PipelineNames
│       ├── Services/               # repository, storage, pipeline engine, queue, monitor
│       ├── Dtos/                   # API response shapes
│       └── Controllers/            # ImagesController, PipelinesController
└── image-processing-ui/            # Angular 19 (standalone, Signals, inject())
    └── src/app/
        ├── core/                   # models + ImageService / PipelineService
        └── features/               # upload, status (charts), gallery
```

### API endpoints

| Method | Route | Description |
| --- | --- | --- |
| POST | `/images` | Upload an image (multipart `file`); processing starts in the background |
| GET  | `/images` | List images that finished or errored |
| GET  | `/images/{id}` | Image details incl. pipeline history |
| GET  | `/images/{id}/download` | Download the stored file |
| GET  | `/pipelines` | Active pipelines and the image count in each |

---

## ⚙️ How to Run Locally

### Prerequisites
* .NET 8 SDK
* Node.js 18+ & npm
* Angular CLI (`npm install -g @angular/cli`) — optional, `npm start` works without it

The client's API base URL is set in
[`src/app/core/api.config.ts`](image-processing-ui/src/app/core/api.config.ts)
and must match the URL the API is served on.

### 1. Run the Backend API

**Option A — Visual Studio / IIS Express (HTTPS, `https://localhost:44320`):**
The client is configured for this URL out of the box.

```bash
# trust the ASP.NET Core dev certificate once, so the browser accepts HTTPS
dotnet dev-certs https --trust
```
Then run the project from Visual Studio using the **IIS Express** profile.
Swagger UI: `https://localhost:44320/swagger`.

**Option B — Kestrel over HTTP (no certificate, `http://localhost:5182`):**

```bash
cd imageProcessing.Api
dotnet restore
dotnet run --project imageProcessing.Api --launch-profile http
```
Then set `API_BASE_URL` in `api.config.ts` to `http://localhost:5182`.
Swagger UI: `http://localhost:5182/swagger`.

> Pipeline delays are scaled by `SpeedFactor` in `appsettings.Development.json`
> (default `0.1` → a 30s delay takes 3s). Set it to `1.0` for the real spec timings.
> HTTPS redirection is disabled in Development so the client can call over plain HTTP.

### 2. Run the Frontend

```bash
cd image-processing-ui
npm install
npm start
```

Open `http://localhost:4200`. CORS is configured on the API to allow this origin.

---

## 📝 Notes
* Reads image dimensions with **SixLabors.ImageSharp 3.1.x** (free license; v4 requires a paid key).
* All state is **in-memory** (no database); restarting the API clears uploaded images.
* Charts are rendered with **Chart.js** + `chartjs-plugin-datalabels`.
