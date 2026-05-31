# Image Processing

A rule-based image routing/processing system: a .NET Core Web API that runs
uploaded images through a chain of asynchronous pipelines, and an Angular 19
client that uploads images and visualizes processing in real time.

## Architecture

```
image-processing/
├── imageProcessing.Api/        # .NET 8 Web API (Controllers)
│   └── imageProcessing.Api/
│       ├── Models/             # ImageItem, ImageStatus, PipelineNames
│       ├── Services/           # repository, storage, pipeline engine, queue, monitor
│       ├── Dtos/               # API response shapes
│       └── Controllers/        # ImagesController, PipelinesController
└── image-processing-ui/        # Angular 19 (standalone, Signals, inject())
    └── src/app/
        ├── core/               # models + ImageService / PipelineService
        └── features/           # upload, status (charts), gallery
```

- **In-memory** storage via a thread-safe `ConcurrentDictionary` singleton (no DB).
- **Asynchronous processing**: uploads are queued (`System.Threading.Channels`)
  and run by a `BackgroundService`, so the API never blocks.
- A thread-safe **monitor** tracks how many images each pipeline is processing.

## Routing rules

Rules are evaluated in order; the first match wins. If none match, the image is
**Finished**.

1. width == height **and** processed during daytime (08–19) → `SquarePipeline`
2. width > height → `CirclePipeline`
3. file > 3 MB **and** width < height → `SlowPipeline`
4. file type is JPG → `StarPipeline`
5. width < 20 → finish with **ProcessError**

## Pipelines

| Pipeline | Action |
| --- | --- |
| `SquarePipeline` | delay 30s → finish |
| `SlowPipeline`   | delay 15s → double width → re-run ImagePipeline |
| `CirclePipeline` | random 15–30s delay → reduce width by 10px → re-run ImagePipeline |
| `StarPipeline`   | delay 10s → double width → run CirclePipeline |

> Image dimensions are changed only as references — the file on disk is never resized.
> Delays can be shortened for development via `SpeedFactor` (see below).

## API

| Method | Route | Description |
| --- | --- | --- |
| POST | `/images` | Upload an image (multipart `file`); processing starts in the background |
| GET  | `/images` | List images that finished or errored |
| GET  | `/images/{id}` | Image details incl. pipeline history |
| GET  | `/images/{id}/download` | Download the stored file |
| GET  | `/pipelines` | Active pipelines and the image count in each |

## Running locally

### API (`http://localhost:5182`)

```bash
cd imageProcessing.Api
dotnet run --project imageProcessing.Api --launch-profile http
```

Swagger UI: `http://localhost:5182/swagger`.

In development, delays are scaled by `SpeedFactor` (`appsettings.Development.json`,
default `0.1` → a 30s delay takes 3s). Set it to `1.0` for the real spec timings.

### Client (`http://localhost:4200`)

```bash
cd image-processing-ui
npm install
npm start
```

CORS is configured on the API to allow `http://localhost:4200`.

## Notes

- Uses **SixLabors.ImageSharp 3.1.x** (free license) to read image dimensions.
- Infinite-loop cases are intentionally not guarded against, per the spec.
